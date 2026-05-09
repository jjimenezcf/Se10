
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using System;
using System.Linq;
using System.Collections.Generic;
using Utilidades;
using Gestor.Errores;
using iText.Kernel.Pdf;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using iText.Layout;
using iText.Layout.Element;
using System.Collections.Concurrent;
using GestorDeElementos.Extensores;
using Newtonsoft.Json;
using static Gestor.Errores.GestorDeErrores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using GestorDeElementos;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Entorno;

namespace GestoresDeNegocio.TrabajosSometidos
{


    public class LectorDeCoreoImap
    {
        public ImapClient ClienteImap { get; }
       
        private ContextoSe _Contexto { get; }

        private string _Cuenta { get; }

        private List<MiCorreoDto> MisCorreos
        {
            get
            {
                var cacheDeCorreos = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Todos);
                if (!cacheDeCorreos.ContainsKey(_Cuenta))
                {
                    cacheDeCorreos[_Cuenta] = new List<MiCorreoDto>();
                }
                return (List<MiCorreoDto>)cacheDeCorreos[_Cuenta];
            }
        } 

        public LectorDeCoreoImap(ContextoSe contexto, string cuenta, string password)
        {
            string host = "imap.gmx.com";
            int port = 993;
            var client = new ImapClient();
            _Cuenta = cuenta;
            _Contexto = contexto;
            try
            {
                client.Connect(host, port, SecureSocketOptions.SslOnConnect);
                client.Authenticate(cuenta, password);
                ClienteImap = client;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar o autenticar con el servidor IMAP", ex);
            }
        }

        public MiCorreoDto LeerCorreo(int idMiCorreoDto)
        {
            var miCorreoDto = MisCorreos.FirstOrDefault(x => x.Id == idMiCorreoDto);
            if (miCorreoDto == null)
            {
                var datos = BuscarMensajePorMessageIdEnTodosBuzones(idMiCorreoDto);
                if (datos.mail is null)
                    Emitir($"No se ha localizado el email '{idMiCorreoDto}' en ningún buzón de la cuenta '{_Cuenta}'");
                miCorreoDto = CrearCorreo(datos.mail, datos.folder);
            }
            return miCorreoDto;
        }

        public void Desconectar()
        {
            if (ClienteImap.IsConnected)
            {
                ClienteImap.Disconnect(true);
            }
        }

        public int Total(string guid, string buzon, string filtroAsunto)
        {
            ValidarBuzon(buzon);
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Cantidad);
            var indice = $"{guid}-{buzon}-{filtroAsunto}";
            if (!cache.ContainsKey(indice))
            {
                var correosDto = LeerTodoElBuzon(guid, buzon,0,-1, filtroAsunto);
                if (correosDto.Count == 0)
                    return 0;
                cache[indice] = correosDto.Count;
            }
            return (int)cache[indice];
        }

        public List<MiCorreoDto> LeerTodoElBuzon(string guid, string buzon, int pos, int cant, string filtroAsunto)
        {
            var cacheDelBuzon = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Del_Buzon);
            var indBuzon = $"{guid}-{buzon}";
            if (!cacheDelBuzon.ContainsKey(indBuzon))
            {
                var correos = LeerCorreosDelBuzon(buzon);
                if (correos.Count == 0)
                    return correos;
                cacheDelBuzon[indBuzon] = correos;
            }
            return (List<MiCorreoDto>)cacheDelBuzon[indBuzon];
        }

        public List<MiCorreoDto> LeerMensajesDelBuzon(string guid, string buzon, int pos, int cant, string filtroAsunto)
        {
            var cacheDelBuzon = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Del_Buzon);
            var indBuzon = $"{guid}-{buzon}";
            if (!cacheDelBuzon.ContainsKey(indBuzon))
            {
                var correosdto = LeerCorreosDelBuzon(buzon);
                if (correosdto.Count == 0)
                    return correosdto;

                cacheDelBuzon[indBuzon] = correosdto;
            }

            var cacheSolicitados = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_filtrados);
            var indice = $"{guid}-{buzon}-{filtroAsunto}"; 
            if (!cacheSolicitados.ContainsKey(indice))
            {
                cacheSolicitados[indice] = (cant == -1 ? (List<MiCorreoDto>)cacheDelBuzon[indBuzon] : ((List<MiCorreoDto>)cacheDelBuzon[indBuzon]).Skip(pos).Take(cant))
                                      .Where(correo => correo.Asunto.Contains(filtroAsunto.IsNullOrEmpty() ? "" : filtroAsunto))
                                      .ToList();

            }

            return (List<MiCorreoDto>)cacheSolicitados[indice];
        }

        private List<MiCorreoDto> LeerCorreosDelBuzon(string buzon)
        {
            var correosBuzon = new List<MiCorreoDto>();
            IMailFolder folder = AbrirCarpeta(buzon);
            try
            {
                var messages = folder.Fetch(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
                foreach (var eMail in messages)
                {
                    if (EstaProcesado(eMail, _Contexto))
                        continue;
                    var correo = CrearCorreo(eMail, folder);
                    correosBuzon.Add(correo);
                    if (!MisCorreos.Any(x => x.IdMensaje == correo.IdMensaje))
                        MisCorreos.Add(correo);
                }
                return correosBuzon;
            }
            finally
            {
                if (folder.IsOpen) folder.Close();
            }
        }

        public string ImprimirCorreo(string buzon, string idMail)
        {
            return ImprimirCorreoPDF(buzon, idMail);
        }

        public string DescargarAdjunto(string idMail, string idAdjunto)
        {
            var miCorreoDto = MisCorreos.First(x => x.IdMensaje == idMail);
            if (!miCorreoDto.ConAdjuntos)
                Emitir($"El correo indicado '{idMail}' no contiene el adjuntos");

            var adjuntos = JsonConvert.DeserializeObject<List<Adjunto>>(miCorreoDto.Adjuntos);
            var adjunto = adjuntos.First(x => x.IdAdjunto == idAdjunto);
            var mail = ObtenerMail(idMail);
            var basicPart = BuscarAdjunto(mail, adjunto);
            if (basicPart == null)
                //Emitir($"El correo indicado '{idMail}' no contiene el adjunto '{idAdjunto}'");
                return null;

            IMailFolder folder = ObtenerCarpetaDelMensaje(idMail);
            return SalvarFichero(mail, basicPart, folder, adjunto);
        }

        public IElementoDto ArchivarCorreo(enumNegocio negocio, int idMiCorreoDto, int idTipo, int idCg, string nombre, Dictionary<string, object> parametros)
        {
            var correo = LeerCorreo(idMiCorreoDto);
            var datos = PrepararDatosParaIncorporar(correo);
            IElementoDtm elemento = negocio.NuevoDtm(_Contexto, idCg, idTipo, nombre, datos.correo.Cuerpo, parametros);
            negocio.Insertar(_Contexto, elemento, parametros: parametros);
            AdjuntarAdjuntos(_Contexto, negocio, elemento, (datos.correo, JsonConvert.DeserializeObject<List<Adjunto>>(correo.Adjuntos), datos.archivoImpreso));
            var accion = $"Creado: {negocio.Singular()}. Elemento: {elemento.Referencia(_Contexto)}. El: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {_Contexto.DatosDeConexion.Login}";
            datos.correo.Auditoria(_Contexto, negocio, elemento, accion);
            return negocio.LeerElemento(_Contexto, elemento.Id);
        }

        public void AsociarCorreoEnArchivador(int idMiCorreoDto, enumNegocio negocio, int idArchivador, int idCarpeta)
        {
            var datos = PrepararDatosParaIncorporar(LeerCorreo(idMiCorreoDto));
            var archivador = _Contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            var carpeta = idCarpeta > 0 ? _Contexto.SeleccionarPorId<CarpetaDtm>(idCarpeta) : null;
            var negocioAdj = idCarpeta > 0 ? enumNegocio.Carpeta : negocio;
            INombre elemento = idCarpeta > 0 ? carpeta : archivador;
            AdjuntarAdjuntos(_Contexto, negocioAdj, elemento,  datos);
            var accion = $"Asociar correo. Negocio: {(idCarpeta > 0 ? enumNegocio.Carpeta : negocio).Singular()}. Elemento: {(idCarpeta > 0 ? carpeta.Referencia(_Contexto) : archivador.Referencia(_Contexto))}. El: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {_Contexto.DatosDeConexion.Login}";
            datos.correo.Auditoria(_Contexto, negocio, archivador, accion);
        }
        
        public void AsociarCorreoEnTarea(int idCorreo, enumNegocio negocio, int idTarea, int idArchivador, int idCarpeta)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta);
                return;
            }
            AsociarCorreo<TareaDtm>(negocio, idCorreo, idTarea);
        }
        
        public void AsociarCorreoEnRegistroEs(int idCorreo, enumNegocio negocio, int idRegistro, int idTarea, int idArchivador, int idCarpeta)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta);
                return;
            }
            if (idTarea > 0)
            {
                AsociarCorreoEnTarea(idCorreo, enumNegocio.Tarea, idTarea, idArchivador, idCarpeta);
                return;
            }
            AsociarCorreo<RegistroEsDtm>(negocio, idCorreo, idRegistro);
        }

        public void AsociarCorreoEnExpediente(int idCorreo, enumNegocio negocio, int idExpediente, int idTarea, int idArchivador, int idCarpeta)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta);
                return;
            }
            if (idTarea > 0)
            {
                AsociarCorreoEnTarea(idCorreo, enumNegocio.Tarea, idTarea, idArchivador, idCarpeta);
                return;
            }
            AsociarCorreo<ExpedienteDtm>(negocio, idCorreo, idExpediente);
        }
        
        public void AsociarCorreoEnFacturaRec(int idCorreo, enumNegocio negocio, int idFacturaRec, int idArchivador, int idCarpeta)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta);
                return;
            }
            AsociarCorreo<FacturaRecDtm>(negocio, idCorreo, idFacturaRec);
        }

        public void EliminarCorreo(int idMiCorreoDto)
        {
            var miCorreoDto = LeerCorreo(idMiCorreoDto);
            int? idArchivo = null;
            if (miCorreoDto.Cuerpo.Length > 1999)
            {
                var archivoImpreso = ImprimirCorreo(miCorreoDto.Buzon, miCorreoDto.IdMensaje);
                idArchivo = ServidorDocumental.SubirArchivo(_Contexto, archivoImpreso, sanitizar: false);
                miCorreoDto.Cuerpo = $"Correo impreso, Id: {idArchivo}, Fichero: {Path.GetFileName(archivoImpreso)}";
            }

            new MiCorreoDtm
            {
                IdMensaje = miCorreoDto.IdMensaje,
                Buzon = miCorreoDto.Buzon,
                Fecha = miCorreoDto.Fecha,
                Emisor = miCorreoDto.Emisor,
                To = miCorreoDto.To,
                Asunto = miCorreoDto.Asunto,
                Cuerpo = miCorreoDto.Cuerpo,
                Adjuntos = miCorreoDto.Adjuntos,
                Accion = $"Eliminado el: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {_Contexto.DatosDeConexion.Login}",
                IdElemento = idArchivo
            }.Insertar(_Contexto);
        }

        private void AsociarCorreo<T>(enumNegocio negocio, int idMiCorreoDto, int idElemento)
        where T : ElementoDtm
        {
            var datos = PrepararDatosParaIncorporar(LeerCorreo(idMiCorreoDto));
            var elemento = _Contexto.SeleccionarPorId<T>(idElemento);
            AdjuntarAdjuntos(_Contexto, negocio, elemento, datos);
            var accion = $"Asociar correo. Negocio: {negocio.Singular()}: {elemento.Referencia(_Contexto)}. El: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {_Contexto.DatosDeConexion.Login}";
            datos.correo.Auditoria(_Contexto, negocio, elemento, accion);
        }

        private string ImprimirCorreoPDF(string buzon, string idMail)
        {
            var folder = AbrirCarpeta(buzon);
            var mensaje = LeerMensajePorIdMensaje(folder, idMail);
            var correo = CrearCorreo(mensaje, folder);
            var sb = new StringBuilder();
            sb.AppendLine($"De: {correo.Emisor}");
            sb.AppendLine($"Para: {correo.To}");
            sb.AppendLine($"Fecha: {correo.Fecha}");
            sb.AppendLine($"Asunto: {correo.Asunto}");
            sb.AppendLine($"");
            sb.AppendLine($"Cuerpo");
            sb.AppendLine($"{correo.Cuerpo}");
            sb.AppendLine($"Adjuntos: {correo.Adjuntos}");
            var rawMessage = sb.ToString();
            var decodedMessage = extCadenas.Base64UrlDecode(rawMessage);
            // Crear un nuevo documento PDF
            var nombre = correo.Emisor;
            Match match = Regex.Match(correo.Emisor, @"^(.*?)\s<(.*)>$");
            if (match.Success) nombre = match.Groups[1].Value;
            var nombrefichero = $"{nombre.Replace("\"", "")}_{idMail}.pdf".NormalizarFichero();
            var fichero = Path.Combine(enumRutas.RutaDeDescarga, nombrefichero);
            var pdfDocumento = new PdfDocument(new PdfWriter(fichero));
            var documento = new Document(pdfDocumento);

            // Agregar el contenido del correo electrónico al documento PDF
            documento.Add(new Paragraph(decodedMessage));

            // Cerrar el documento PDF
            documento.Close();
            return fichero;
        }

        private IMessageSummary LeerMensajePorIdMensaje(IMailFolder folder, string idMensaje)
        {
            AbrirCarpeta(folder.Name, FolderAccess.ReadWrite);
            var messages = folder.Fetch(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
            foreach (var eMail in messages)
            {
                var id = IdMensaje(eMail).ToString();
                if (id == idMensaje) return eMail;
            }

            throw new Exception($"El mensaje con ID '{idMensaje}' no se ha localizado en el buzón '{folder.Name}'");
        }

        private void ValidarBuzon(string buzon)
        {
            try
            {
                var folders = ClienteImap.GetFolders(ClienteImap.PersonalNamespaces[0]);
                if (!folders.Any(x => x.Name.Equals(buzon, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"No existe el buzón '{buzon}' en la cuenta '{_Cuenta}'");
                }
            }
            catch (Exception e)
            {
                GestorDeErrores.Emitir($"No hay acceso al buzón '{buzon}'", e);
                throw;
            }
        }

        private IMailFolder AbrirCarpeta(string buzon, FolderAccess modo = FolderAccess.ReadOnly, bool crearSiNoExiste = true)
        {
            if (!BuzonExiste(buzon))
            {
                if (crearSiNoExiste)
                    CrearBuzon(buzon);
                else
                    Emitir($"La carpeta {buzon} no existe");
            }

            var folder = ClienteImap.GetFolder(buzon);
            if (folder.IsOpen)
            {
                if (folder.Access != modo)
                {
                    folder.Close();
                    folder.Open(modo);
                }
            }
            else
            {                
                folder.Open(modo);
            }

            return folder;
        }

        private bool BuzonExiste(string buzon)
        {
            try
            {
                var personalNamespace = ClienteImap.PersonalNamespaces[0];
                var folders = ClienteImap.GetFolders(personalNamespace);
                return folders.Any(f => f.FullName == buzon);
            }
            catch (Exception ex)
            {
                Emitir($"Error al comprobar si existe el buzón '{buzon}': {ex.Message}");
                return false;
            }
        }
        private IMailFolder CrearBuzon(string buzon)
        {
            try
            {
                var personalNamespace = ClienteImap.PersonalNamespaces[0];
                return ClienteImap.GetFolder(personalNamespace).Create(buzon, true);
            }
            catch (Exception ex)
            {
                Emitir($"Error al crear el buzón '{buzon}': {ex.Message}");
                return null;
            }
        }

        private MiCorreoDto CrearCorreo(IMessageSummary eMail, IMailFolder folder)
        {
            AbrirCarpeta(folder.Name);
            try
            {
                MiCorreoDto correo = new();
                var message = folder.GetMessage(eMail.UniqueId);
                correo.Id = IdMensaje(eMail); // Encriptacion.GenerarEntero(eMail.EmailId ?? eMail.Envelope.MessageId); //
                correo.Buzon = folder.Name;
                correo.Asunto = eMail.Envelope.Subject;
                correo.Emisor = eMail.Envelope.From.ToString();
                correo.To = string.Join(",", eMail.Envelope.To);
                correo.Cuerpo = message.TextBody ?? "";
                correo.CuerpoHtml = !string.IsNullOrEmpty(message.HtmlBody) ? extHtml.SanitizeHtml(message.HtmlBody) : extHtml.TextoToHtml(correo.Cuerpo);
                correo.Fecha = eMail.Date.ToLocalTime().DateTime;
                correo.IdMensaje = IdMensaje(eMail).ToString();
                correo.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
                correo.SerializarAdjuntos(Adjuntos(eMail));
                return correo;
            }
            finally
            {
               if (folder.IsOpen) folder.Close();
            }
        }


        private List<Adjunto> Adjuntos(IMessageSummary eMail)
        {
            var adjuntos = new List<Adjunto>();

            // Procesar adjuntos tradicionales
            if (eMail.Attachments != null && eMail.Attachments.Any())
            {
                foreach (var attachment in eMail.Attachments)
                {
                    var adjunto = new Adjunto
                    {
                        Fichero = attachment.FileName ?? "archivo adjunto sin nombre",
                        TipoMime = attachment.ContentType?.MimeType ?? "desconocido",
                        IdMail = IdMensaje(eMail).ToString(),
                        IdAdjunto = attachment.ContentId ?? Guid.NewGuid().ToString(),
                        IdParte = attachment.PartSpecifier
                    };

                    adjuntos.Add(adjunto);
                }
            }

            // Procesar adjuntos incrustados en el cuerpo
            if (eMail.Body != null)
            {
                ProcesarPartesCuerpo(eMail.Body, adjuntos, IdMensaje(eMail).ToString());
            }

            return adjuntos;
        }

        private void ProcesarPartesCuerpo(BodyPart bodyPart, List<Adjunto> adjuntos, string idMail)
        {
            if (bodyPart is BodyPartMultipart multipart)
            {
                foreach (var subPart in multipart.BodyParts)
                {
                    ProcesarPartesCuerpo(subPart, adjuntos, idMail);
                }
            }
            else if (bodyPart is BodyPartBasic basic)
            {
                if (!string.IsNullOrEmpty(basic.FileName))
                {
                    var adjunto = new Adjunto
                    {
                        Fichero = basic.FileName,
                        TipoMime = basic.ContentType?.MimeType ?? "desconocido",
                        IdMail = idMail,
                        IdAdjunto = basic.ContentId ?? Guid.NewGuid().ToString(),
                        IdParte = basic.PartSpecifier
                    };

                    adjuntos.Add(adjunto);
                }
            }
        }
        private BodyPartBasic BuscarAdjunto(IMessageSummary eMail, Adjunto adjunto)
        {
            // Primero, buscar en los adjuntos directos
            if (eMail.Attachments != null)
            {
                foreach (var attachment in eMail.Attachments)
                {
                    if (attachment.ContentId == adjunto.IdAdjunto)
                    {
                        return attachment;
                    }
                }
            }

            // Si no se encuentra en los adjuntos directos, buscar en el cuerpo
            if (eMail.Body != null)
            {
                return BuscarAdjuntoEnBodyPart(eMail.Body, adjunto);
            }

            return null;
        }

        private BodyPartBasic BuscarAdjuntoEnBodyPart(BodyPart bodyPart, Adjunto adjunto)
        {
            if (bodyPart is BodyPartMultipart multipart)
            {
                foreach (var subPart in multipart.BodyParts)
                {
                    var result = BuscarAdjuntoEnBodyPart(subPart, adjunto);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            else if (bodyPart is BodyPartBasic basic)
            {
                if (!string.IsNullOrEmpty(basic.FileName) && adjunto.IdAdjunto == basic.ContentId)
                {
                    return basic;
                }
            }

            return null;
        }

        private IMessageSummary ObtenerMail(string idMail)
        {
            var personal = ClienteImap.GetFolder(ClienteImap.PersonalNamespaces[0]);
            foreach (var folder in personal.GetSubfolders(false))
            {
                AbrirCarpeta(folder.Name);
                var messages = folder.Fetch(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
                foreach (var eMail in messages)
                {
                    var id = IdMensaje(eMail);
                    if (id == idMail.Entero())
                        return eMail;
                }
            }
            return null;
        }

        private IMailFolder ObtenerCarpetaDelMensaje(string idMail)
        {
            var personal = ClienteImap.GetFolder(ClienteImap.PersonalNamespaces[0]);
            foreach (var folder in personal.GetSubfolders(false))
            {
                AbrirCarpeta(folder.Name);
                var messages = folder.Fetch(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
                foreach (var eMail in messages)
                {
                    var id = IdMensaje(eMail);
                    if (id == idMail.Entero())
                        return folder;
                }
            }
            return null;
        }

        private int IdMensaje(IMessageSummary eMail)
        {
            return extEncriptacion.GenerarEntero(eMail.Envelope.MessageId);
            //eMail.EmailId ?? eMail.Envelope.MessageId
        }

        public (IMessageSummary mail, IMailFolder folder) BuscarMensajePorMessageIdEnTodosBuzones(int idMicorreo)
        {
            // Obtener la lista de todos los buzones
            var folders = ClienteImap.GetFolders(ClienteImap.PersonalNamespaces[0]);

            foreach (var folder in folders)
            {
                try
                {
                    if (folder.Name == ExtensorDeMiCorreo.BuzonProcesados) continue;
                    AbrirCarpeta(folder.Name);

                    // Obtener todos los mensajes del buzón
                    var mails = folder.Fetch(0, -1, MessageSummaryItems.Envelope | MessageSummaryItems.UniqueId);

                    // Buscar el mensaje con el MessageId específico
                    var mail = mails.FirstOrDefault(m => IdMensaje(m) == idMicorreo);

                    if (mail != null)
                    {
                        return (mail, folder);
                    }
                }
                catch (Exception ex)
                {
                    // Loguear la excepción o manejarla según sea necesario'
                    Console.WriteLine($"Error al buscar en el buzón '{folder.FullName}': {ex.Message}");
                }
                finally
                {
                    // Cerrar el buzón después de buscar
                    if (folder.IsOpen) folder.Close();
                }
            }

            return (null,null); // Si no se encuentra el mensaje en ningún buzón
        }

        private void AdjuntarAdjuntos(ContextoSe contexto, enumNegocio negocio, INombre elemento, (MiCorreoDto correo, List<Adjunto> adjuntos, string archivoImpreso) datos)
        {
            if (!datos.archivoImpreso.IsNullOrEmpty())
            {
                var archivo = ServidorDocumental.AnexarArchivo(contexto, negocio, elemento.Id, datos.archivoImpreso, sanitizar: true);
                ServidorDocumental.BloquearArchivo(contexto, negocio.IdNegocio(), elemento.Id, archivo.Id, ltrDeUnArchivo.CorreoImportado);
            }

            var rutas = new List<string>();
            if (datos.correo.ConAdjuntos)
            {
                foreach (var adjunto in datos.adjuntos)
                {
                    var fichero = DescargarAdjunto(datos.correo.IdMensaje, adjunto.IdAdjunto);
                    if (fichero is null)
                        continue;
                    rutas.Add(fichero);
                    var archivo = ServidorDocumental.AnexarArchivo(contexto, negocio, elemento.Id, fichero, copiar:true, quitarExtensionHtml: true, sanitizar: true);
                    if (negocio == enumNegocio.FacturaRecibida && datos.adjuntos.Count == 1)
                    {
                        ((FacturaRecDtm)elemento).IdArchivo = archivo.Id;
                        ((FacturaRecDtm)elemento).Modificar(contexto);
                    }
                    ServidorDocumental.BloquearArchivo(contexto, negocio.IdNegocio(), elemento.Id, archivo.Id, $"Adjunto del correo {datos.correo.Asunto}", validarSiEstaTerminado: false);
                }
            }
            GestorDeMiCorreo.AdjuntarCuerpoHtml(contexto, negocio, elemento, datos.correo, rutas);
            foreach(var ruta in rutas)
            {
                if (File.Exists(ruta))
                    File.Delete(ruta);
            }
        }

        private string SalvarFichero(IMessageSummary message, BodyPartBasic basicPart, IMailFolder folder, Adjunto adjunto)
        {
            string rutaCompleta = Path.Combine(enumRutas.RutaDeDescarga, ("Z_" + adjunto.IdAdjunto + adjunto.Fichero).NormalizarFichero());
            rutaCompleta = ApiDeArchivos.ObtenerNombreUnico(rutaCompleta);

            // Obtener la entidad (parte del cuerpo)
            var entity = folder.GetBodyPart(message.UniqueId, basicPart);

            using (var stream = File.Create(rutaCompleta))
            {
                if (entity is MimePart mimePart)
                {
                    // Caso estándar: PDF, JPG, DOCX, etc.
                    mimePart.Content.DecodeTo(stream);
                }
                else if (entity is MessagePart messagePart)
                {
                    // Caso específico: Archivos .eml (message/rfc822)
                    // Aquí escribimos el mensaje completo en el stream
                    messagePart.Message.WriteTo(stream);
                }
                else
                {
                    stream.Dispose(); // Cerramos el stream antes de salir
                    File.Delete(rutaCompleta); // Limpiamos el archivo vacío creado
                    Emitir($"No se pudo obtener el contenido del adjunto. Tipo detectado: {entity?.GetType().Name}");
                    return null;
                }
            }

            return rutaCompleta;
        }

        private (MiCorreoDto correo, List<Adjunto> adjuntos, string archivoImpreso)
        PrepararDatosParaIncorporar(MiCorreoDto correo)
        {
            var adjuntos = correo.ConAdjuntos ? JsonConvert.DeserializeObject<List<Adjunto>>(correo.Adjuntos) : new List<Adjunto>();
            string fileNames = string.Join(",", adjuntos.Select(e => e.Fichero));
            var archivoImpreso = ImprimirCorreoPDF(correo.Buzon, correo.IdMensaje);
            var cuerpo = $"Enviado por: {correo.Emisor}{Environment.NewLine}Asunto: ${correo.Asunto}{Environment.NewLine}{Environment.NewLine}Cuerpo{Environment.NewLine}${(correo.Cuerpo.IsNullOrEmpty() ? "(sin cuerpo)" : correo.Cuerpo)}{Environment.NewLine}{Environment.NewLine}Adjuntos: ${fileNames} {Environment.NewLine}{Environment.NewLine}Fecha de recepción: ${correo.Fecha} {Environment.NewLine}Identificador: ${correo.IdMensaje}";
            correo.Cuerpo = cuerpo.Length > 1999
            ? $"Correo impreso, Fichero: {Path.GetFileName(archivoImpreso)}"
            : cuerpo;

            return (correo, adjuntos, archivoImpreso);
        }

        private bool EstaProcesado(IMessageSummary eMail, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Procesados);
            var idMensaje = IdMensaje(eMail).ToString();
            if (!cache.ContainsKey(idMensaje))
            {
                var a = contexto.SeleccionarPorPropiedad<MiCorreoDtm>(nameof(MiCorreoDtm.IdMensaje),idMensaje, errorSiNoHay: false);
                cache[idMensaje] = a is not null;
            }
            return (bool)cache[idMensaje];
        }

        public void DarPorProcesado(int idCorreoDto)
        {
            var miCorreo = LeerCorreo(idCorreoDto);
            IMailFolder buzonOrigen = AbrirCarpeta(miCorreo.Buzon, FolderAccess.ReadWrite);
            IMailFolder buzonDestino = AbrirCarpeta(ExtensorDeMiCorreo.BuzonProcesados, FolderAccess.ReadWrite, crearSiNoExiste: true);
            IMessageSummary mensaje = LeerMensajePorIdMensaje(buzonOrigen, miCorreo.IdMensaje);
            try
            {
                buzonOrigen.MoveTo(mensaje.UniqueId, buzonDestino);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al mover el mensaje: {ex.Message}");
            }
            finally
            {
                if (buzonOrigen.IsOpen) buzonOrigen.Close();
                if (buzonDestino.IsOpen) buzonDestino.Close();
                EliminarCaches();
            }
        }

        private static void EliminarCaches()
        {
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Cantidad);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Del_Buzon);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_filtrados);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Procesados);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Todos);
        }
    }
}
