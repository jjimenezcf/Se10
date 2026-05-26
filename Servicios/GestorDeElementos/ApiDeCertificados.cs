extern alias BouncyCastleNuevo;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Signature;
using FirmaXadesNetCore.Signature.Parameters;
using Gestor.Errores;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Kernel.Crypto;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Utilidades;
using static iText.Signatures.PdfSigner;
// CORRECCIÓN DE MAPEO: Ajustamos los tipos para que iText 9 sea feliz
using AsymmetricKeyParameter = BouncyCastleNuevo::Org.BouncyCastle.Crypto.AsymmetricKeyParameter;
// Añadimos el alias nativo para la interfaz de certificados que iText exige en las cadenas (Chain)
using IX509Certificate = iText.Commons.Bouncycastle.Cert.IX509Certificate;
using Pkcs12Store = BouncyCastleNuevo::Org.BouncyCastle.Pkcs.Pkcs12Store;
using Pkcs12StoreBuilder = BouncyCastleNuevo::Org.BouncyCastle.Pkcs.Pkcs12StoreBuilder;



//https://github.com/itext/i7ns-samples/blob/develop/itext/itext.publications/itext.publications.signing-examples.simple-test/iText/SigningExamples/Simple/TestSignBCSimple.cs

namespace GestorDeElementos
{
    public class Certificado : IDisposable
    {
        public AsymmetricKeyParameter Key { get; private set; }

        public IX509Certificate[] Chain { get; private set; }

        public int Id { get; }

        private string _pfxFile { get; }


        public string Password { get; }

        public string RutaDelCertificado => _pfxFile;
        public X509Certificate2 X509Certificado
                 =>
                 X509CertificateLoader.LoadPkcs12(
                     File.ReadAllBytes(_pfxFile),
                     Password,
                     X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                 );


        public RSA RSAPrivateKey
        {
            get
            {
                if (!X509Certificado.HasPrivateKey)
                    GestorDeErrores.Emitir($"El certificado '{_pfxFile}' no tiene clave privada");

                return X509Certificado.GetRSAPrivateKey();
            }
        }

        public Certificado(int id, string rutaCompletaDelPfx, string claveDelPfx = null)
        {

            _pfxFile = rutaCompletaDelPfx;
            Password = claveDelPfx;
            Id = id;
            //using (var file = File.OpenRead(rutaCompletaDelPfx))
            //{
            var password = claveDelPfx?.ToCharArray() ?? new char[] { /* password en blanco */ };

            Pkcs12Store pkcs12 = new Pkcs12StoreBuilder().Build();
            var file = new FileStream(rutaCompletaDelPfx, FileMode.Open, FileAccess.Read);
            try
            {
                pkcs12.Load(file, claveDelPfx.ToCharArray());
            }
            finally
            {
                file.Close();
                file.Dispose();
                file = null;
            }
            string alias = null;
            foreach (string al in pkcs12.Aliases)
            {
                if (pkcs12.IsKeyEntry(al))
                {
                    alias = al;
                    break;
                }
            }

            Key = pkcs12.GetKey(alias).Key;
            var chainEntries = pkcs12.GetCertificateChain(alias);
            Chain = new IX509Certificate[chainEntries.Length];

            for (int i = 0; i < chainEntries.Length; i++)
                Chain[i] = new X509CertificateBC(chainEntries[i].Certificate);

            // }
        }

        private static string GetCertificateAlias(Pkcs12Store store)
        {
            foreach (string currentAlias in store.Aliases)
            {
                if (store.IsKeyEntry(currentAlias))
                {
                    return currentAlias;
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (File.Exists(_pfxFile))
            {
                File.Delete(_pfxFile);
            }
        }
    }

    public class Firmante
    {
        private readonly Certificado _Certificado;

        private ContextoSe _Contexto { get; }

        public Firmante(ContextoSe contexto, Certificado certificado)
        {
            _Certificado = certificado;
            _Contexto = contexto;
        }

        public Firmante(ContextoSe contexto, CertificadoDtm certificadoDtm)
        {
            var password = ApiDeCertificados.LeerPasswordDeCertificado(contexto, certificadoDtm.Id);
            Certificado certificado = ApiDeCertificados.ObtenerCertificado(contexto, certificadoDtm.Id, password);
            _Certificado = certificado;
            _Contexto = contexto;
        }

        public void FirmarPdf(string rutaDocumentoSinFirma, string rutaDocumentoFirmado, string usuario, bool visible)
        {
            using (FileStream os = new FileStream(rutaDocumentoFirmado, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfReader reader = new PdfReader(rutaDocumentoSinFirma);

                StampingProperties stampingProps = new StampingProperties();
                stampingProps.UseAppendMode();

                PdfSigner signer = new PdfSigner(reader, os, stampingProps);

                // 3. Certificación + apariencia todo en SignerProperties
                SignerProperties signerProperties = new SignerProperties();
                signerProperties
                    .SetCertificationLevel(AccessPermissions.NO_CHANGES_PERMITTED)
                    .SetReason("Firma de Factura - " + ParametrosDeArchivadores.SistemaInformaticoQueFirma)
                    .SetLocation(ParametrosDeArchivadores.LugarDeFirma)
                    .SetFieldName("Firma_" + usuario);

                if (visible)
                {
                    signerProperties
                        .SetPageRect(new Rectangle(36, 36, 200, 100))
                        .SetPageNumber(1);
                }

                signer.SetSignerProperties(signerProperties);

                // 5. Algoritmo y Firma
                IExternalSignature pks = new PrivateKeySignature(
                    new PrivateKeyBC(_Certificado.Key),
                    DigestAlgorithms.SHA256
                );

                // 6. Ejecución
                signer.SignDetached(pks, _Certificado.Chain, null, null, null, 0, CryptoStandard.CMS);
            }
        }


        public byte[] FirmarRegistro(byte[] datos)
        {

            using (RSA rsa = _Certificado.RSAPrivateKey)
            {
                return rsa.SignData(datos, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

        }

        public void FirmarXmlOld(string rutaDocumentoSinFirma, string rutaDocumentoFirmado)
        {
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                _Contexto.AnotarTraza("Fichero a firmar", rutaDocumentoSinFirma);
                doc.Load(rutaDocumentoSinFirma);

                _Contexto.AnotarTraza("Cargando certificado", _Certificado.Key.ToString());
                if (!File.Exists(_Certificado.RutaDelCertificado))
                    GestorDeErrores.Emitir($"El  certificado '{_Certificado.RutaDelCertificado}' no existe en la ruta indicada");

                if (ApiDeArchivos.EstaElFicheroBloqueado(_Certificado.RutaDelCertificado))
                    GestorDeErrores.Emitir($"El  certificado '{_Certificado.RutaDelCertificado}' esta bloqueado");

                SignedXml signedXml = new SignedXml(doc);
                try
                {
                    signedXml.SigningKey = _Certificado.RSAPrivateKey;
                }
                catch (CryptographicException cryptoEx)
                {
                    _Contexto.AnotarExcepcion(cryptoEx);
                    throw;
                }
                catch (Exception ex)
                {
                    _Contexto.AnotarExcepcion(ex);
                    throw;
                }
                Reference reference = new Reference("");
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                signedXml.AddReference(reference);

                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new KeyInfoX509Data(_Certificado.X509Certificado));
                signedXml.KeyInfo = keyInfo;
                signedXml.ComputeSignature();

                XmlElement xmlDigitalSignature = signedXml.GetXml();
                doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

                _Contexto.AnotarTraza("Salvando xml firmado en:", rutaDocumentoFirmado);
                doc.Save(rutaDocumentoFirmado);
            }
        }

        public void FirmarXml(string rutaDocumentoSinFirma, string rutaDocumentoFirmado)
        {
            try
            {
                if (this._Certificado == null)
                {
                    throw new InvalidOperationException("El objeto _Certificado no está inicializado.");
                }

                // 1. Carga segura del certificado (.NET 10)
                X509Certificate2 cert = null;
                string rutaCert = this._Certificado.RutaDelCertificado;
                string passCert = this._Certificado.Password;

                if (File.Exists(rutaCert))
                {
                    cert = X509CertificateLoader.LoadPkcs12FromFile(rutaCert, passCert, X509KeyStorageFlags.Exportable);
                }
                else
                {
                    throw new FileNotFoundException($"No se encontró el certificado en: {rutaCert}");
                }

                // 2. Instanciar el servicio de la librería
                FirmaXadesNetCore.XadesService xadesService = new FirmaXadesNetCore.XadesService();

                // 3. Configurar parámetros base de la firma
                FirmaXadesNetCore.Signature.Parameters.SignatureParameters parametros = new FirmaXadesNetCore.Signature.Parameters.SignatureParameters
                {
                    SignaturePackaging = FirmaXadesNetCore.Signature.Parameters.SignaturePackaging.ENVELOPED,
                    DataFormat = new FirmaXadesNetCore.Signature.Parameters.DataFormat
                    {
                        MimeType = "text/xml"
                    },
                    Signer = new FirmaXadesNetCore.Crypto.Signer(cert)
                };

                // 4. Cargar el XML en memoria para comprobar formato e inyectar cuna si es UBL
                XmlDocument docManipulable = new XmlDocument();
                docManipulable.PreserveWhitespace = true;
                docManipulable.Load(rutaDocumentoSinFirma);

                XmlElement rootXml = docManipulable.DocumentElement;

                if (rootXml != null && rootXml.LocalName == "Invoice" && rootXml.NamespaceURI == Extensores.NamespacesUbl.NsInvoice)
                {
                    string nsExt = Extensores.NamespacesUbl.NsExt;

                    // Inyectamos el atributo de namespace si no estuviera ya en la raíz
                    if (!rootXml.HasAttribute("xmlns:ext"))
                    {
                        rootXml.SetAttribute("xmlns:ext", nsExt);
                    }

                    // Creamos los elementos de la cuna UBL
                    XmlElement ublExtensions = docManipulable.CreateElement("ext", "UBLExtensions", nsExt);
                    XmlElement ublExtension = docManipulable.CreateElement("ext", "UBLExtension", nsExt);
                    XmlElement extensionContent = docManipulable.CreateElement("ext", "ExtensionContent", nsExt);

                    ublExtension.AppendChild(extensionContent);
                    ublExtensions.AppendChild(ublExtension);

                    // CRÍTICO UBL XSD: Las extensiones DEBEN ser el primer hijo de la raíz
                    rootXml.PrependChild(ublExtensions);

                    // Configuramos el destino exacto de la firma usando el objeto de tu librería
                    parametros.SignatureDestination = new FirmaXadesNetCore.Signature.Parameters.SignatureXPathExpression
                    {
                        XPathExpression = "/*[local-name()='Invoice']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']"
                    };
                }

                // 5. Firmar el documento utilizando flujos de memoria intermedios
                using (MemoryStream msEntrada = new MemoryStream())
                {
                    // Guardamos el XML (con o sin cuna inyectada) en el Stream temporal
                    var settings = new XmlWriterSettings { Indent = true, Encoding = System.Text.Encoding.UTF8 };
                    using (XmlWriter writer = XmlWriter.Create(msEntrada, settings))
                    {
                        docManipulable.Save(writer);
                    }

                    // Reset del puntero del stream antes de firmar
                    msEntrada.Position = 0;

                    // Ejecutamos la firma criptográfica XAdES
                    var documentoFirmado = xadesService.Sign(msEntrada, parametros);

                    // 6. Guardar el archivo firmado definitivo en disco
                    using (FileStream fsSalida = new FileStream(rutaDocumentoFirmado, FileMode.Create, FileAccess.Write))
                    {
                        documentoFirmado.Save(fsSalida);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en el firmado digital del XML: {ex.Message}", ex);
            }
        }
        public string FirmarImagenComoXml(ContextoSe contexto, string fichero, string destino)
        {
            // 1. Crear el XML contenedor
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null)); // Buenas prácticas: Declaramos codificación

            var root = xmlDoc.CreateElement("ImagenFirmada");
            xmlDoc.AppendChild(root);

            // Añadir el nombre del fichero de origen
            var nombreElement = xmlDoc.CreateElement("NombreFichero");
            nombreElement.InnerText = System.IO.Path.GetFileName(fichero);
            root.ChildNodes.Item(0)?.AppendChild(nombreElement); // O root.AppendChild(nombreElement);

            root.AppendChild(nombreElement);

            // Convertir la imagen física a base64
            var imagenBase64 = Convert.ToBase64String(File.ReadAllBytes(fichero));
            var imagenElement = xmlDoc.CreateElement("ImagenBase64");
            imagenElement.InnerText = imagenBase64;
            root.AppendChild(imagenElement);

            // 2. Asegurar existencia del directorio de intercambio transitorio
            if (!Directory.Exists(CacheDeVariable.CFG_Ruta_Ficheros_A_Firmar))
            {
                Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Ficheros_A_Firmar);
            }

            // Usamos Path.ChangeExtension para forzar que el destino siempre sea XML de manera robusta
            destino = System.IO.Path.ChangeExtension(destino, "xml");

            var xmlTempPath = System.IO.Path.Combine(CacheDeVariable.CFG_Ruta_Ficheros_A_Firmar, $"{Guid.NewGuid()}.xml");

            try
            {
                // Guardar el XML en UTF-8 nativo para que coincida con la declaración
                var settings = new XmlWriterSettings { Indent = true, Encoding = System.Text.Encoding.UTF8 };
                using (var writer = XmlWriter.Create(xmlTempPath, settings))
                {
                    xmlDoc.Save(writer);
                }

                // 3. Delegar en nuestro método unificado de firma
                FirmarXml(xmlTempPath, destino);

                return destino;
            }
            finally
            {
                // 4. Garantía absoluta de limpieza de la caché
                if (File.Exists(xmlTempPath))
                {
                    File.Delete(xmlTempPath);
                }
            }
        }
    }

    public static class ApiDeCertificados
    {
        public static string ObtenerSHA1(string str)
        {
            SHA1 sha1 = SHA1.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder sb = new StringBuilder();
            byte[] stream = sha1.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        public static Certificado ObtenerCertificado(ContextoSe contexto, int idCertificado, string password)
        {
            CertificadoDtm certificado = ValidarUsoDelCertificado(contexto, idCertificado);

            var ficheroDeFirma = certificado.DescargarCertificado(contexto);
            try
            {
                return new Certificado(idCertificado, ficheroDeFirma, password);
            }
            catch
            {
                if (File.Exists(ficheroDeFirma)) File.Delete(ficheroDeFirma);
                throw;
            }
        }

        public static CertificadoDtm ValidarUsoDelCertificado(ContextoSe contexto, int idCertificado)
        {
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.Firmar, true } };
            var certificado = contexto.SeleccionarPorId<CertificadoDtm>(idCertificado, errorSiNoHay: false, parametros: parametros);
            if (certificado == null)
                GestorDeErrores.Emitir($"El usuario '{contexto.DatosDeConexion.Login}' no tiene permisos para firmar con el certificado '{certificado.Nombre}'");
            return certificado;
        }
        public static void ValidarCertificado(ContextoSe contexto, CertificadoDtm certificado)
        {
            var descargado = certificado.DescargarCertificado(contexto);
            try
            {
                contexto.AnotarTraza("fichero descargado", descargado);
                var certificadoXml = X509CertificateLoader.LoadPkcs12FromFile(
                    descargado,
                    certificado.Password,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                );
                certificado.ExpiraEl = DateTime.Parse(certificadoXml.GetExpirationDateString());
                certificado.Archivo = null;
            }
            catch (Exception ex)
            {
                contexto.AnotarExcepcion(ex);
                GestorDeErrores.Emitir($"Error al validar el certificado '{certificado.Nombre}'. {ex.Message}");
            }
            finally
            {
                if (File.Exists(descargado)) File.Delete(descargado);
            }
        }

        public static string Firmar(ContextoSe contexto, string fichero, int idCertificado, string password, bool visible)
        {
            var certificado = ObtenerCertificado(contexto, idCertificado, password);
            return Firmar(contexto, fichero, certificado, visible);
        }

        public static string LeerPasswordDeCertificado(ContextoSe contexto, int idCertificado)
        {
            ValidarUsoDelCertificado(contexto, idCertificado);

            var consulta = new ConsultaSql<CredencialesDeCertificado>(
                $@"SELECT 
                     {ICampos.ID} as {nameof(CredencialesDeCertificado.Id)}
                   , CONVERT(VARCHAR , DECRYPTBYPASSPHRASE('sistemaSe',  {ICampos.PASSWORD})) as {nameof(CredencialesDeCertificado.Password)}
                   FROM {Esquemas.ENTORNO}.{Tablas.CERTIFICADO}
                   where {ICampos.ID} = {idCertificado}");

            var credenciales = consulta.LanzarConsulta(contexto);
            if (credenciales.Count == 0)
                throw new Exception($"Certificado no localizado");

            return credenciales[0].Password;
        }

        public static string FirmarContenidoXml(ContextoSe contexto, Certificado certificado, string contenidoXml, string ruta, string nombreFichero)
        {
            var temporal = System.IO.Path.Combine($@"{ruta}", $"{nombreFichero}.{enumExtensiones.xml}");
            try
            {
                var firmante = new Firmante(contexto, certificado);
                var destino = System.IO.Path.Combine($@"{ruta}", $"{nombreFichero}_firmado.{enumExtensiones.xml}");


                // 1. Guardar el contenido XML en un archivo temporal
                File.WriteAllText(temporal, contenidoXml);

                // 2. Firmar el archivo XML
                firmante.FirmarXml(temporal, destino);

                // 3. Devolver la ruta del archivo firmado
                return destino;
            }
            finally
            {
                if (File.Exists(temporal)) File.Delete(temporal);
                certificado.Dispose();
                certificado = null;
            }
        }

        private static string Firmar(ContextoSe contexto, string fichero, Certificado certificado, bool visible)
        {
            try
            {
                var firmante = new Firmante(contexto, certificado);
                var destino = System.IO.Path.Combine($@"{System.IO.Path.GetDirectoryName(fichero)}", $"{System.IO.Path.GetFileNameWithoutExtension(fichero)}_firmado{System.IO.Path.GetExtension(fichero)}");

                var extension = System.IO.Path.GetExtension(fichero).ToLower().Replace(".", "");

                if (ExtensorDeTipoDeArchivos.EsImagen(extension, errorSiNoEstaCatalogada: false))
                    destino = firmante.FirmarImagenComoXml(contexto, fichero, destino);
                else
                {
                    if (extension == enumExtensiones.xml.ToString())
                    {
                        if (FirmadorXadesService.PuedeFirmarConJar())
                        {
                            FirmadorXadesService.Firmar(fichero, destino, certificado.RutaDelCertificado, certificado.Password);
                        }
                        else
                        {
                            firmante.FirmarXml(fichero, destino);
                        }
                    }
                    else
                    {
                        if (extension == enumExtensiones.pdf.ToString())
                            firmante.FirmarPdf(fichero, destino, contexto.DatosDeConexion.Login, visible);
                        else
                            if (extension == enumExtensiones.docx.ToString())
                            {
                                var ficheroPdf = extDocx.ToPdf(fichero, ParametrosDeArchivadores.ClaveConvertApi); // "1CPToQIEOjozKPfq";
                                firmante.FirmarPdf(ficheroPdf, destino, contexto.DatosDeConexion.Login, visible);
                                destino = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destino), $"{System.IO.Path.GetFileNameWithoutExtension(destino)}.{enumExtensiones.pdf}");
                            }
                            else
                                GestorDeErrores.Emitir($"No se ha implementado como firmar ficheros con la extención {System.IO.Path.GetExtension(fichero)}");
                    }
                }
                return destino;
            }
            finally
            {
                certificado.Dispose();
                certificado = null;
            }
        }

        public static void ValidarPassword(ContextoSe contexto, int idCertificado, string password)
        {
            ObtenerCertificado(contexto, idCertificado, password);
        }

        public static string DescargarCertificado(this CertificadoDtm certificado, ContextoSe contexto)
        {

            if (certificado.IdArchivo.Entero() == 0)
                GestorDeErrores.Emitir($"No ha indicado el archivo del certificado {certificado.Nombre}");
            try
            {
                return certificado.Archivo(contexto).DescargarCertificado(contexto);
            }
            catch (Exception e)
            {
                throw new Exception(Excepciones.MensajeCompleto(e));
            }

        }

        public static string DescargarCertificado(this ArchivoDtm archivo, ContextoSe contexto)
        {
            try
            {
                return archivo.DescargarArchivo(rutaDeDescarga: CacheDeVariable.CFG_Ruta_Ficheros_De_Certificados, usarCacheado: true, ponerTickAlNombre: false, contexto.Traza);
            }
            catch (Exception ex)
            {
                contexto.AnotarExcepcion(ex);
                throw;
            }
            finally
            {
                if (File.Exists(archivo.SeDecargado(CacheDeVariable.CFG_Ruta_Ficheros_De_Certificados)))
                {
                    File.Delete(archivo.SeDecargado(CacheDeVariable.CFG_Ruta_Ficheros_De_Certificados));
                }
            }
        }


    }

}