using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using ServicioXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Utilidades;
using VeriFactu.Common.Exceptions;

namespace GestoresDeNegocio.Venta.Factura
{
    public class GeneradorSii
    {
        public ContextoSe Contexto { get; }
        public FacturaEmtDtm Factura { get; }
        public SociedadDtm Sociedad { get; }
        public CertificadoDtm Certificado { get; }
        private string _password => ApiDeCertificados.LeerPasswordDeCertificado(Contexto, Certificado.Id);
        private Certificado _certificado => ApiDeCertificados.ObtenerCertificado(Contexto, Certificado.Id, _password);

        private string _numeroImplantacion => Sociedad.ObtenerSSII_Implantacion(Contexto);

        public GeneradorSii(ContextoSe contexto, FacturaEmtDtm factura)
        : this(contexto, factura.Cg(contexto).Sociedad(contexto))
        {
            Factura = factura;
        }

        public GeneradorSii(ContextoSe contexto, SociedadDtm sociedad)
        {
            Contexto = contexto;
            Sociedad = sociedad;
            Certificado = ExtensorDeSociedades.ObtenerCertificado(sociedad, contexto);
            CrearDirectorios(sociedad);
            ValidarConexionConLaAeat();
        }

        internal static void CrearDirectorios(SociedadDtm sociedad)
        {

            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Ficheros_De_Sii) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Ficheros_De_Sii);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains);

            if (!Path.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log))
                Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log);

            var rutaLog = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, sociedad.NIFSinIsoEs);
            if (!Path.Exists(rutaLog))
                Directory.CreateDirectory(rutaLog);

            var rutaBc = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, sociedad.NIFSinIsoEs);
            if (!Path.Exists(rutaBc))
                Directory.CreateDirectory(rutaBc);
        }

        public static void BlanquearDirectorios(SociedadDtm sociedad)
        {
            // 1. Definimos la lista de rutas base a limpiar
            List<string> rutasALimpiar = new List<string>
            {
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, sociedad.NIFSinIsoEs),
            };

            foreach (var ruta in rutasALimpiar)
            {
                if (Directory.Exists(ruta))
                {
                    LimpiarArchivosDeCarpeta(ruta);
                }
            }
        }

        private static void LimpiarArchivosDeCarpeta(string ruta)
        {
            try
            {
                string[] ficheros = Directory.GetFiles(ruta);
                foreach (string fichero in ficheros)
                {
                    File.Delete(fichero);
                }
            }
            catch (Exception ex)
            {
                throw Excepciones.Emitir($"No se pudo limpiar la carpeta {ruta}: {ex.Message}");
            }
        }

        public void AltaDeFacturaSif(bool esUnLote)
        {
            Contexto.IniciarTraza($"{nameof(AltaDeFacturaSif)}_{Factura.Referencia}".NormalizarFichero(), debugar: true);
            try
            {
                if (!esUnLote)
                {
                    var log = Contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), Factura.Id, errorSiNoHay: false);
                    if (log is null)
                        GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' no está indicada que se ha de enviar a la AEAT");

                    if (log.EnviadaEl is not null)
                        GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' se envió a la AEAT el {log.EnviadaEl.Fecha().ToString("dd-MM-yyyy HH:mm:ss")}");
                }

                var nif = Factura.Sociedad(Contexto).NIFSinIsoEs;
                var nombreRazon = Factura.Sociedad(Contexto).RazonSocial;
                AuditoriaSii auditoriaSii = null;
                var ficheroCsv = FicheroDondeEstaLaFactura();
                if (!ficheroCsv.IsNullOrEmpty())
                {
                    RecomponerAuditoria(ficheroCsv);
                    GestorDeErrores.Emitir(ltrSii.FacturaEnviada.Replace(nameof(FacturaEmtDtm.Referencia), Factura.Referencia).Replace(ltrSii.ficheroCsv, ficheroCsv));
                }

                var facturaAeat = BuscarFactura();
                if (facturaAeat != null)
                    GestorDeErrores.Emitir(ltrSii.FacturaEnLaAEAT.Replace(nameof(FacturaEmtDtm.Referencia), Factura.Referencia));

                var verifactu = Factura.Verifactu(Contexto, errorSiNoHay: false);
                if (verifactu != null)
                    GestorDeErrores.Emitir(ltrSii.FacturaEnVerifactu.Replace(nameof(FacturaEmtDtm.Referencia), Factura.Referencia));

                try
                {
                    if (Factura.EsRectificativa)
                        auditoriaSii = AltaDeFacturaRectificativa();
                    else
                        auditoriaSii = Factura.AltaDeFacturaSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza);

                    // auditoriaSii != null → AEAT aceptó la factura
                    try
                    {
                        Factura.AsociarAuditoriaSii(Contexto, auditoriaSii);
                    }
                    catch (Exception exDb)
                    {
                        // El VerifactuDtm pudo crearse (RegistrarVerifactu OK) pero fallaron
                        // las subidas de ficheros de auditoría al servidor documental.
                        // En cualquier caso AEAT ya tiene la factura: no relanzar para que
                        // el caller pueda commitear y continuar con la impresión.
                        // Se encola un trabajo para recuperar la auditoría cuando sea posible.
                        Contexto.AnotarTraza(
                            $"AEAT aceptó '{Factura.Referencia}' pero AsociarAuditoriaSii falló — se encola reconstrucción",
                            GestorDeErrores.Detalle(exDb));
                        GestoresDeNegocio.Ventas.TrabajosDeFacturasEmt.SometerReconstruccionVerifactu(Contexto, Factura.Id);
                    }

                    // Informativa: si falla no debe interrumpir, la AEAT ya tiene la factura
                    try
                    {
                        Factura.CrearTraza(Contexto, ltrSii.TrazaRegistroEnLaAeat, auditoriaSii.Detalle);
                    }
                    catch (Exception exTraza)
                    {
                        Contexto.AnotarTraza($"No se ha podido crear la traza '{ltrSii.TrazaRegistroEnLaAeat}' de '{Factura.Referencia}'", GestorDeErrores.Detalle(exTraza));
                    }
                }
                finally
                {
                    _certificado.Dispose();
                }
            }
            catch (Exception e)
            {
                Contexto.AnotarExcepcion(e);
                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }

        private AuditoriaSii AltaDeFacturaRectificativa()
        {
            if (Factura.MotivoDeRectificacion == enumMotivoDeRectificacion.DatosErroneos)
            {
                var rectificada = Factura.RectificaA(Contexto);
                if (rectificada.Verifactu(Contexto).Cancelada == false)
                    throw new System.Exception($"No se puede crear la factura '{Factura.Referencia}' ya que previamente se ha de cancelar la rectificada '{rectificada.Referencia}'");

                return Factura.AltaDeFacturaSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza);
            }

            if (Factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImportes)
            {
                var rectificada = Factura.RectificaA(Contexto);
                if (rectificada.Verifactu(Contexto).Cancelada == true)
                    throw new System.Exception($"No se puede crear la factura '{Factura.Referencia}' ya que la rectificada '{rectificada.Referencia}' está cancelada en la AEAT");

                return Factura.ClaseRectificativa == enumClaseDeRectificativa.OR
                ? Factura.RectificarFacturaPorSustitucionSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza)
                : Factura.RectificarFacturaPorDiferenciaSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza);
            }

            throw new System.Exception($"El motivo de rectificación '{Factura.MotivoDeRectificacion.Descripcion()}' de la factura '{Factura.Referencia}' no está implementado para enviar al sii");
        }

        public void CancelarLaRectificada()
        {
            var rectificada = Factura.RectificaA(Contexto);
            var verifactu = rectificada.Verifactu(Contexto);
            if (verifactu.Cancelada == true)
                GestorDeErrores.Emitir($"No se puede emitir la factura '{Factura.Referencia}' ya que la factura que rectifica '{rectificada.Referencia}' se ha cancelado en la AEAT, cree una nueva factura y cancele esta en su sistema");

            if (Factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImpago)
                GestorDeErrores.Emitir($"El motivo de rectificación '{Factura.MotivoDeRectificacion.Descripcion()}' de la factura '{Factura.Referencia}' no está implementado para enviar al sii");

            try
            {
                rectificada.CancelarFacturaSIF(Contexto, _certificado, _numeroImplantacion);
            }
            catch (Exception e)
            {
                TraducirErrorDeLaAeat(e);
                throw;
            }
            verifactu.Cancelada = true;
            verifactu.ModificarComoAdministrador(Contexto);
        }


        private void ValidarConexionConLaAeat()
        {
            try
            {
                Sociedad.ValidarConexionConLaAeat(Contexto, _certificado, _numeroImplantacion, DateTime.Now.Year, DateTime.Now.Month);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FaultException))
                    throw Excepciones.Emitir($"{ltrSii.VerifactuNoActivo}: {((FaultException)e).Fault.faultstring}");

                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        // Solo relanza si reconoce el error; si no, el llamador hace 'throw;' y conserva la excepción original
        // (p. ej. una FaultException, de la que depende la cancelación de la emisión)
        private void TraducirErrorDeLaAeat(Exception e)
        {
            for (var ex = e; ex != null; ex = ex.InnerException)
            {
                if (ex is WebException { Response: HttpWebResponse { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden } })
                    throw Excepciones.Emitir(msjCertificados.CertificadoNoAutorizado.Replace("[Empresa]", Sociedad.RazonSocial), e);

                if (ex.Message.Contains("Certificate is out of date", StringComparison.OrdinalIgnoreCase))
                    throw Excepciones.Emitir(msjCertificados.CertificadoCaducado.Replace("[Empresa]", Sociedad.Nombre), e);
            }
        }

        private List<FacturaAeatDto> ConsultaDeFacturasInterna(int ano, int mes)
        {
            try
            {
                return Sociedad.ConsultaDeFacturasEmitidas(Contexto, _certificado, _numeroImplantacion, ano, mes);
            }
            catch (Exception e)
            {
                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        public List<FacturaAeatDto> ConsultaDeFacturas(int ano, int mes)
        {
            return ConsultaDeFacturasInterna(ano, mes);
        }

        public void ValidarNif()
        {

            try
            {
                Factura.ValidarNif(Contexto, _certificado, _numeroImplantacion);
            }
            catch (Exception e)
            {
                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                _certificado.Dispose();
            }
        }


        public void ValidarNif(string nif, string razonSocial)
        {
            try
            {
                Verifactu.ValidarNif(Contexto, nif, razonSocial, _certificado, _numeroImplantacion);
            }
            catch (Exception e)
            {
                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        public void ValidarVat()
        {

            try
            {
                Factura.ValidarVat(Contexto, _certificado, _numeroImplantacion);
            }
            catch (Exception e)
            {
                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        public string FicheroDondeEstaLaFactura()
        {
            var mesBuscado = Factura.FacturadaEl.Fecha().Month;
            var anoDeEmision = Factura.FacturadaEl.Fecha().Year;
            var mesActual = DateTime.Now.Month;

            var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Factura.Sociedad(Contexto).NIFSinIsoEs);

            while (mesBuscado <= mesActual)
            {
                var fichero = $"{anoDeEmision}{mesBuscado.ToString().PadLeft(2, '0')}.csv";
                var rutaFicheroCsv = Path.Combine(rutaBlockChain, fichero);
                if (File.Exists(rutaFicheroCsv))
                {
                    bool esta = EstaEnElFichero(rutaFicheroCsv);
                    if (esta) return fichero;
                }
                mesBuscado++;
            }
            return null;
        }

        private bool EstaEnElFichero(string rutaFichero)
        {
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            foreach (var fila in fichero)
            {
                linea++;
                if (fila.EnBlanco)
                    continue;

                if (fila.Columnas != 7)
                    GestorDeErrores.Emitir($" El fichero '{rutaFichero}' está corrupto, ya que ha de tener '7' columnas, y tiene '{fila.Columnas}' para la fila '{linea}'");
                var numeroDeFacturaEnviada = fila["F"].Trim();
                if (numeroDeFacturaEnviada == Factura.NumeroDeFactura)
                {
                    return true;
                }
            }
            return false;
        }

        private void RecomponerAuditoria(string ficheroCsv)
        {
            if (Factura.Verifactu(Contexto, errorSiNoHay: false) != null)
                return;

            var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Factura.Sociedad(Contexto).NIFSinIsoEs);
            var rutaFicheroCsv = Path.Combine(rutaBlockChain, ficheroCsv);
            var fila = BuscarFactura(rutaFicheroCsv);

            Factura.RecomponerVerifactu(Contexto, fila["C"]);
        }

        private CsvFila BuscarFactura(string rutaFichero)
        {
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            foreach (var fila in fichero)
            {
                linea++;
                if (fila.EnBlanco)
                    continue;

                if (fila.Columnas != 7)
                    GestorDeErrores.Emitir($" El fichero '{rutaFichero}' está corrupto, ya que ha de tener '7' columnas, y tiene '{fila.Columnas}' para la fila '{linea}'");
                var numeroDeFacturaEnviada = fila["F"].Trim();
                if (numeroDeFacturaEnviada == Factura.NumeroDeFactura)
                {
                    return fila;
                }
            }
            return null;
        }

        // Reconstruye los ficheros de la cadena de registros de facturación del año en curso que usa la librería VeriFactu, con los registros que
        // tiene la AEAT. La cadena es única para la sociedad en este sistema informático y se ordena por fecha y hora de generación, sin separar
        // series (Orden HAC/1177/2024, art. 7). Formato de la librería (Blockchain.GetControlFileLine y GetVarFileLine):
        //   - aaaamm.csv (mes de generación), una línea por registro: id;fecha y hora de generación;huella;fecha de expedición (dd-MM-yyyy);nif;número[;encadenamiento]
        //   - _nif.csv, solo el último registro, sin salto de línea: de él toma la librería el registro anterior de la siguiente alta
        public void RecomponerBlockChain()
        {
            var ano = DateTime.Now.Year;
            var registros = RegistrosDeLaCadenaEnLaAeat(ano, DateTime.Now.Month);
            if (registros.Count == 0)
                GestorDeErrores.Emitir($"La AEAT no tiene registros de facturación de la sociedad '{Sociedad.Referencia}' generados por este sistema en el año '{ano}'");

            var ultimo = registros.Last();
            var anuladoDespues = registros.FirstOrDefault(r => EsAnulado(r) && UltimaModificacion(r) > ultimo.Generado);
            if (anuladoDespues is not null)
                GestorDeErrores.Emitir($"La factura '{anuladoDespues.NumSerie}' se anuló después del último alta '{ultimo.NumSerie}'; el registro de anulación es el último de la cadena " +
                                       $"y la consulta de la AEAT no devuelve sus datos, así que no se puede recomponer la cadena de forma fiable");

            var rutaCadena = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Sociedad.NIFSinIsoEs);
            Directory.CreateDirectory(rutaCadena);
            var blockChain = CancelarArchivosDeBlockChain(ano);
            foreach (var fichero in Directory.GetFiles(rutaCadena, $"{ano}??*.csv"))
                File.Delete(fichero);

            var id = UltimoIdDeAnosAnteriores(rutaCadena, ano);
            var avisos = new List<string>();
            RegistroDeLaCadenaVerifactu anterior = null;
            string lineaDelUltimo = null;
            foreach (var registrosDelMes in registros.GroupBy(r => r.Generado.LocalDateTime.ToString("yyyyMM")))
            {
                var lineas = new StringBuilder();
                foreach (var registro in registrosDelMes)
                {
                    id++;
                    if (anterior is not null && registro.HuellaAnterior != anterior.Huella)
                        avisos.Add($"'{registro.NumSerie}' no encadena con '{anterior.NumSerie}', entre ambos hay registros que la consulta no devuelve (subsanaciones o anulaciones)");

                    lineaDelUltimo = $"{id};{registro.FechaHoraHusoGenRegistro};{registro.Huella};{registro.FechaExpedicion};{registro.IDEmisor};{registro.NumSerie}";
                    lineas.Append($"{lineaDelUltimo};[{Encadenamiento(registro)}]\n");
                    AsociarALaCadena(registro, blockChain);
                    anterior = registro;
                }

                var rutaDelMes = Path.Combine(rutaCadena, $"{registrosDelMes.Key}.csv");
                File.WriteAllText(rutaDelMes, lineas.ToString());
                blockChain.AnexarArchivo(Contexto, rutaDelMes, copiar: true);
            }

            File.WriteAllText(Path.Combine(rutaCadena, $"_{Sociedad.NIFSinIsoEs}.csv"), lineaDelUltimo);

            Sociedad.CrearTraza(Contexto, "Cadena de registros de Verifactu recompuesta",
                $"Se ha recompuesto la cadena del año '{ano}' con {registros.Count} registros de la AEAT; el último es '{ultimo.NumSerie}' generado el '{ultimo.FechaHoraHusoGenRegistro}'" +
                (avisos.Count == 0 ? "" : $"{Environment.NewLine}Avisos:{Environment.NewLine}{string.Join(Environment.NewLine, avisos)}"));
        }

        // Registros de la AEAT de los meses del año hasta el indicado, ordenados por fecha y hora de generación
        private List<RegistroDeLaCadenaVerifactu> RegistrosDeLaCadenaEnLaAeat(int ano, int hastaMes)
        {
            var certificado = _certificado;
            try
            {
                var registros = new List<RegistroDeLaCadenaVerifactu>();
                for (var mes = 1; mes <= hastaMes; mes++)
                    registros.AddRange(Sociedad.RegistrosDeLaCadena(certificado, _numeroImplantacion, ano, mes));

                return registros
                    .Where(r => !r.FechaHoraHusoGenRegistro.IsNullOrEmpty())
                    .GroupBy(r => r.Huella).Select(g => g.First())
                    .OrderBy(r => r.Generado)
                    .ToList();
            }
            catch (Exception e)
            {
                TraducirErrorDeLaAeat(e);
                throw;
            }
            finally
            {
                certificado.Dispose();
            }
        }

        // La cadena es continua entre años: los ids siguen al último de los ficheros de años anteriores
        private static long UltimoIdDeAnosAnteriores(string rutaCadena, int ano)
        {
            long ultimoId = 0;
            foreach (var fichero in Directory.GetFiles(rutaCadena, "??????.csv"))
            {
                var nombre = Path.GetFileNameWithoutExtension(fichero);
                if (!int.TryParse(nombre.Left(4), out var anoDelFichero) || anoDelFichero >= ano)
                    continue;

                var ultimaLinea = File.ReadLines(fichero).LastOrDefault(linea => !linea.IsNullOrEmpty());
                if (ultimaLinea is not null && long.TryParse(ultimaLinea.Split(';')[0], out var idDelFichero))
                    ultimoId = Math.Max(ultimoId, idDelFichero);
            }
            return ultimoId;
        }

        private static string Encadenamiento(RegistroDeLaCadenaVerifactu registro) =>
            registro.PrimerRegistro
            ? "PrimerRegistro=S"
            : $"NumSerieFactura={registro.NumSerieAnterior}&FechaExpedicionFactura={registro.FechaExpedicionAnterior}&Huella={registro.HuellaAnterior}";

        private static bool EsAnulado(RegistroDeLaCadenaVerifactu registro) => registro.Estado?.StartsWith("Anulad", StringComparison.OrdinalIgnoreCase) == true;

        private static DateTimeOffset UltimaModificacion(RegistroDeLaCadenaVerifactu registro) =>
            registro.TimestampUltimaModificacion.IsNullOrEmpty()
            ? DateTimeOffset.MinValue
            : DateTimeOffset.Parse(registro.TimestampUltimaModificacion, System.Globalization.CultureInfo.InvariantCulture);

        // El registro de Verifactu de la factura queda asociado al archivador de la cadena
        private void AsociarALaCadena(RegistroDeLaCadenaVerifactu registro, ArchivadorDtm blockChain)
        {
            var factura = Sociedad.LeerFacturaEmt(Contexto, registro.NumSerie, errorSiNoHay: false);
            var verifactu = factura?.Verifactu(Contexto, errorSiNoHay: false);
            if (verifactu is not null && verifactu.IdBlockChain != blockChain.Id)
            {
                verifactu.IdBlockChain = blockChain.Id;
                verifactu.ModificarComoAdministrador(Contexto);
            }
        }

        public void ReconstruirVerifactu(EntornoDeTrabajo entorno)
        {
            // 1ª opción: reconstruir desde el fichero CSV del blockchain local
            var ficheroCsv = FicheroDondeEstaLaFactura();
            if (!ficheroCsv.IsNullOrEmpty())
            {
                entorno.CrearTraza($"Reconstruyendo Verifactu de '{Factura.Referencia}' desde CSV local '{ficheroCsv}'");
                RecomponerAuditoria(ficheroCsv);
                return;
            }

            // 2ª opción: consultar directamente a la AEAT
            entorno.CrearTraza($"CSV local no encontrado para '{Factura.Referencia}', consultando la AEAT");
            SincronizarConDatosDeLaAeat();
        }

        public void SincronizarConDatosDeLaAeat()
        {
            // Precondición: si el log ya tiene fecha de envío el proceso ya completó
            // correctamente en su momento — no hay nada que sincronizar
            var log = Contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), Factura.Id);
            if (log.EnviadaEl is not null)
                GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' se envió el '{log.EnviadaEl.Fecha().ToString("dd-MM-yyyy HH:mm:ss")}', según el log de envío");

            var facturaAeat = BuscarFactura();
            if (facturaAeat is null)
                GestorDeErrores.Emitir($"No se ha encontrado la factura '{Factura.Referencia}' en la AEAT");

            if (Factura.Verifactu(Contexto, errorSiNoHay: false) == null)
            {
                var ficheroCsv = FicheroDondeEstaLaFactura();
                if (ficheroCsv.IsNullOrEmpty())
                {
                    var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Factura.Sociedad(Contexto).NIFSinIsoEs);
                    GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' no se localiza en el directorio '{rutaBlockChain}', considere reconstruir el fichero de BlockChain en la edición de la sociedad '{Sociedad.NIF}'");
                }
                Factura.RecomponerVerifactu(Contexto, facturaAeat.Huella);
            }

            log.EnviadaEl = DateTime.Now;
            log.Modificar(Contexto);
        }

        private ArchivadorDtm CancelarArchivosDeBlockChain(int ano)
        {
            var ultimaEnAEAT = Sociedad.UltimaFactura(Contexto, ano, errorSiNoHay: false);
            if (ultimaEnAEAT is null)
                GestorDeErrores.Emitir($"No hay facturas emitidas en la sociedad '{Sociedad.Referencia}' en el año '{ano}'");

            while (true)
            {
                if (ultimaEnAEAT.Verifactu(Contexto, errorSiNoHay: false) != null)
                    break;

                var aux = ultimaEnAEAT.Anterior(Contexto);
                if (aux is null || aux.Ano < ano)
                    break;

                ultimaEnAEAT = aux;
            }

            var verifactu = ultimaEnAEAT.Verifactu(Contexto, errorSiNoHay: false);
            if (verifactu is null)
                GestorDeErrores.Emitir($"De las facturas emitidas en la sociedad '{Sociedad.Referencia}' en el año '{ano}' no hay ninguna con auditoría de Verifactu");

            var archivador = verifactu.ObtenerBlockChain(Contexto);
            archivador.MarcarArchivosComoCancelados(Contexto);
            return archivador;
        }

        private FacturaAeatDto BuscarFactura()
        {
            if (Factura.FacturadaEl is null)
                GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' no tiene fecha de emisión");
            var facturas = ConsultaDeFacturasInterna(((DateTime)Factura.FacturadaEl).Year, ((DateTime)Factura.FacturadaEl).Month);
            return facturas.FirstOrDefault(f => f.NumeroFactura == Factura.NumeroDeFactura);
        }

        public static bool VerifactuActivo(ContextoSe contexto, FacturaEmtDtm facturaDmt)
        {
            try
            {
                var _ = new GeneradorSii(contexto, facturaDmt.Sociedad(contexto));
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(ltrSii.VerifactuNoActivo))
                    return false;
                throw;
            }
        }
    }
}