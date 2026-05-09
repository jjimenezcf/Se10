using Gestor.Errores;
using GestorDeElementos;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using iText.Kernel.Pdf;
using ModeloDeDto.Entorno;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilidades;
using iText.Layout;
using iText.Layout.Element;
using static Gestor.Errores.GestorDeErrores;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using System.Text.RegularExpressions;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using System.Web;
using GestoresDeNegocio.Entorno;

namespace GestoresDeNegocio.TrabajosSometidos
{

    public static class ltrLectorDeCorreo
    {
        internal const string Aplicacion = "Sistema de elementos";
        public static string RutaDeTokens = enumRutas.RutaDeToken;
    }

    public static class LectorDeGmailApiKey
    {
        public static int CachearMisCorreos(ContextoSe contexto, string guid, string buzon, string apiKey, string filtroAsunto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Cantidad);
            var indice = $"{guid}-{buzon}-{filtroAsunto}";
            if (!cache.ContainsKey(indice))
            {
                var servicioGmail = new GmailService(new BaseClientService.Initializer()
                {
                    ApiKey = apiKey
                });

                cache[indice] = servicioGmail.CachearMensajes(contexto, guid, buzon, idLabel: buzon, filtroAsunto);
            }
            return (int)cache[indice];
        }

        public static List<MiCorreoDto> LeerMisCorreos(string guid, string buzon, int posicion, int cantidad, string filtroAsunto)
        {
            return LectorGmail.LeerMisCorreos(guid, buzon, posicion, cantidad, filtroAsunto);
        }
    }

    public static class LectorDeGmailAuth2
    {

        public static GoogleCredential Credenciales(string email)
        {
            var cacheCredenciales = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Credenciales);
            if (!cacheCredenciales.ContainsKey(email))
                cacheCredenciales[email] = LeerCredenciales(email);
            GoogleCredential credenciales = (GoogleCredential)cacheCredenciales[email];
            return credenciales;
        }

        private static GoogleCredential LeerCredenciales(string email)
        {
            string ruta = ltrLectorDeCorreo.RutaDeTokens;
            string ficheroDeCredenciales = Path.Combine(ruta, $"{email}.tkrp");
            if (!Path.Exists(ficheroDeCredenciales))
            {
                GestorDeErrores.Emitir($"No se encuentra el fichero de credenciales, '{ficheroDeCredenciales}', vuelva a conectarse");
            }
            string contenido = File.ReadAllText(ficheroDeCredenciales);
            TokenResponse tokenLeido = JsonConvert.DeserializeObject<TokenResponse>(contenido);
            return GoogleCredential.FromAccessToken(tokenLeido.AccessToken);
        }

        public static int CachearMisCorreos(ContextoSe contexto, string guid, string buzon, object credenciales, string filtroAsunto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Cantidad);
            var indice = $"{guid}-{buzon}";
            if (!cache.ContainsKey(indice))
            {
                var servicioGmail = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = (IConfigurableHttpClientInitializer)credenciales,
                    ApplicationName = ltrLectorDeCorreo.Aplicacion
                });

                UsersResource.LabelsResource.ListRequest request = servicioGmail.Users.Labels.List("me");
                IList<Label> buzones = request.Execute().Labels;
                if (buzones == null || buzones.Count == 0 || !buzones.Any(x => x.Name == buzon))
                {
                    Emitir($"En la cuenta de gmail no existe la etiqueta: '{buzon}'");
                }

                cache[indice] = servicioGmail.CachearMensajes(contexto, guid, buzon, idLabel: buzones.First(x => x.Name.ToLower() == buzon.ToLower()).Id, filtroAsunto);
            }
            return (int)cache[indice];
        }

        public static List<MiCorreoDto> LeerMisCorreos(string guid, string buzon, int posicion, int cantidad, string filtroAsunto)
        {
            return LectorGmail.LeerMisCorreos(guid, buzon, posicion, cantidad, filtroAsunto);
        }

        public static void AsociarCorreoEnExpediente(ContextoSe contexto, int idCorreo, enumNegocio negocio, int idExpediente, int idTarea, int idArchivador, int idCarpeta, string email)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(contexto, idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta, email);
                return;
            }
            if (idTarea > 0)
            {
                AsociarCorreoEnTarea(contexto, idCorreo, enumNegocio.Tarea, idTarea, idArchivador, idCarpeta, email);
                return;
            }
            negocio.AsociarCorreo<ExpedienteDtm>(contexto, idCorreo, idExpediente, email);
        }

        public static void AsociarCorreoEnRegistroEs(ContextoSe contexto, int idCorreo, enumNegocio negocio, int idRegistro, int idTarea, int idArchivador, int idCarpeta, string email)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(contexto, idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta, email);
                return;
            }
            if (idTarea > 0)
            {
                AsociarCorreoEnTarea(contexto, idCorreo, enumNegocio.Tarea,idTarea, idArchivador, idCarpeta, email);
                return;
            }
            negocio.AsociarCorreo<RegistroEsDtm>(contexto, idCorreo, idRegistro, email);
        }

        public static void AsociarCorreoEnFacturaRec(ContextoSe contexto, int idCorreo, enumNegocio negocio, int idFacturaRec, int idArchivador, int idCarpeta, string email)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(contexto, idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta, email);
                return;
            }
            negocio.AsociarCorreo<FacturaRecDtm>(contexto, idCorreo, idFacturaRec, email);
        }

        public static void AsociarCorreoEnTarea(ContextoSe contexto, int idCorreo, enumNegocio negocio, int idTarea, int idArchivador, int idCarpeta, string email)
        {
            if (idArchivador > 0)
            {
                AsociarCorreoEnArchivador(contexto, idCorreo, enumNegocio.Archivador, idArchivador, idCarpeta, email);
                return;
            }
            negocio.AsociarCorreo<TareaDtm>(contexto, idCorreo, idTarea, email);
        }


        public static void AsociarCorreoEnArchivador(ContextoSe contexto, int idCorreo, enumNegocio negocio, int idArchivador, int idCarpeta, string email)
        {
            GoogleCredential credenciales = Credenciales(email);
            var datos = PrepararDatosParaIncorporar(idCorreo, credenciales);
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            var carpeta = idCarpeta > 0 ? contexto.SeleccionarPorId<CarpetaDtm>(idCarpeta) : null;
            AdjuntarAdjuntos(contexto, idCarpeta > 0 ? enumNegocio.Carpeta : negocio, idCarpeta > 0 ? carpeta : archivador, credenciales, datos);
            var accion = $"Asociar correo. Negocio: {(idCarpeta > 0 ? enumNegocio.Carpeta : negocio).Singular()}. Elemento: {(idCarpeta > 0 ? carpeta.Referencia(contexto) : archivador.Referencia(contexto))}. El: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {contexto.DatosDeConexion.Login}";
            datos.correo.Auditoria(contexto, negocio, archivador, accion);
        }

        private static void AsociarCorreo<T>(this enumNegocio negocio, ContextoSe contexto, int idCorreo, int idElemento, string email)
        where T : ElementoDtm
        {
            GoogleCredential credenciales = Credenciales(email);
            var datos = PrepararDatosParaIncorporar(idCorreo, credenciales);
            var elemento = contexto.SeleccionarPorId<T>(idElemento);
            AdjuntarAdjuntos(contexto, negocio, elemento, credenciales, datos);
            var accion = $"Asociar correo. Negocio: {negocio.Singular()}: {elemento.Referencia(contexto)}. El: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {contexto.DatosDeConexion.Login}";
            datos.correo.Auditoria(contexto, negocio, elemento, accion);
        }

        public static IElementoDto ArchivarCorreo(ContextoSe contexto, enumNegocio negocio, int id, int idTipo, int idCg, string nombre, string email, Dictionary<string, object> parametros)
        {
            GoogleCredential credenciales = Credenciales(email);
            var datos = PrepararDatosParaIncorporar(id, credenciales);
            IElementoDtm elemento = negocio.NuevoDtm(contexto, idCg, idTipo, nombre, datos.correo.Cuerpo, parametros);
            negocio.Insertar(contexto, elemento, parametros: parametros);
            AdjuntarAdjuntos(contexto, negocio, elemento, credenciales, datos);
            var accion = $"Creado: {negocio.Singular()}. Elemento: {elemento.Referencia(contexto)}. El: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {contexto.DatosDeConexion.Login}";
            datos.correo.Auditoria(contexto, negocio, elemento, accion);
            return negocio.LeerElemento(contexto, elemento.Id);
        }

        public static string ImprimirCorreo(string idMail, GoogleCredential credenciales)
        {
            var servicioGmail = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credenciales,
                ApplicationName = ltrLectorDeCorreo.Aplicacion
            });
            return servicioGmail.ImprimirCorreoPDF(idMail);
        }

        public static void EliminarCorreo(ContextoSe contexto, int id, GoogleCredential credenciales)
        {
            var servicioGmail = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credenciales,
                ApplicationName = ltrLectorDeCorreo.Aplicacion
            });

            var correo = LeerPorId(id);
            int? idArchivo = null;
            if (correo.Cuerpo.Length > 1999)
            {
                var archivoImpreso = ImprimirCorreo(correo.IdMensaje, credenciales);
                idArchivo = ServidorDocumental.SubirArchivo(contexto, archivoImpreso, sanitizar: false);
                correo.Cuerpo = $"Correo impreso, Id: {idArchivo}, Fichero: {Path.GetFileName(archivoImpreso)}";
            }

            new MiCorreoDtm
            {
                IdMensaje = correo.IdMensaje,
                Buzon = correo.Buzon,
                Fecha = correo.Fecha,
                Emisor = correo.Emisor,
                To = correo.To,
                Asunto = correo.Asunto,
                Cuerpo = correo.Cuerpo,
                Adjuntos = correo.Adjuntos,
                Accion = $"Eliminado el: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. Por: {contexto.DatosDeConexion.Login}",
                IdElemento = idArchivo
            }.Insertar(contexto);


            //servicioGmail.Users.Messages.Delete("me", correo.IdMensaje).Execute();
        }

        public static MiCorreoDto LeerPorId(int id)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_filtrados);

            KeyValuePair<string, object> entrada = cache.FirstOrDefault(x => ((MiCorreoDto)x.Value).Id == id);
            return (MiCorreoDto)entrada.Value;
        }

        public static string DescargarAdjunto(string idMail, string idAdjunto, string idParte, GoogleCredential credenciales)
        {
            var servicioGmail = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credenciales,
                ApplicationName = ltrLectorDeCorreo.Aplicacion
            });


            var lectorDeAdjuntos = servicioGmail.Users.Messages.Attachments.Get("me", idMail, idAdjunto);
            var adjunto = lectorDeAdjuntos.Execute();
            var stream = adjunto.Data.ToCharArray();

            if (stream != null)
            {
                var datos = servicioGmail.BuscarAdjunto(idMail, idParte);
                string base64String = adjunto.Data.Replace('-', '+').Replace('_', '/').PadRight(adjunto.Data.Length + (4 - adjunto.Data.Length % 4) % 4, '=');
                byte[] decodedData = Convert.FromBase64String(base64String);
                var fichero = Path.Combine(enumRutas.RutaDeDescarga, datos?.Fichero ?? "sin_nombre.pdf");
                BinaryWriter writer = new BinaryWriter(new FileStream(fichero, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
                writer.Write(decodedData);
                writer.Close();
                return fichero;
            }
            throw new Exception("No se ha localizado el adjunto");
        }

        private static void AdjuntarAdjuntos(ContextoSe contexto, enumNegocio negocio, INombre elemento, GoogleCredential credenciales, (MiCorreoDto correo, List<Adjunto> adjuntos, string archivoImpreso) datos)
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
                    var fichero = DescargarAdjunto(datos.correo.IdMensaje, adjunto.IdAdjunto, adjunto.IdParte, credenciales);
                    if (fichero == null)
                        continue;
                    rutas.Add(fichero);
                    var archivo = ServidorDocumental.AnexarArchivo(contexto, negocio, elemento.Id, fichero, copiar: false, quitarExtensionHtml: false, sanitizar: true);                  
                    if (negocio == enumNegocio.FacturaRecibida && datos.adjuntos.Count == 1)
                    {
                        ((FacturaRecDtm)elemento).IdArchivo = archivo.Id;
                        ((FacturaRecDtm)elemento).Modificar(contexto);
                    }
                    ServidorDocumental.BloquearArchivo(contexto, negocio.IdNegocio(), elemento.Id, archivo.Id, $"Adjunto del correo {datos.correo.Asunto}", validarSiEstaTerminado: false);
                }
            }
            GestorDeMiCorreo.AdjuntarCuerpoHtml(contexto, negocio, elemento, datos.correo, rutas);
            foreach (var ruta in rutas)
            {
                if (File.Exists(ruta))
                    File.Delete(ruta);
            }
        }

        private static (MiCorreoDto correo, List<Adjunto> adjuntos, string archivoImpreso)
        PrepararDatosParaIncorporar(int id, GoogleCredential credenciales)
        {
            var correo = LeerPorId(id);
            var adjuntos = JsonConvert.DeserializeObject<List<Adjunto>>(correo.Adjuntos);
            string fileNames = string.Join(",", adjuntos.Select(e => e.Fichero));
            var archivoImpreso = ImprimirCorreo(correo.IdMensaje, credenciales);
            var cuerpo = $"Enviado por: {correo.Emisor}{Environment.NewLine}Asunto: ${correo.Asunto}{Environment.NewLine}{Environment.NewLine}Cuerpo{Environment.NewLine}${(correo.Cuerpo.IsNullOrEmpty() ? "(sin cuerpo)" : correo.Cuerpo)}{Environment.NewLine}{Environment.NewLine}Adjuntos: ${fileNames} {Environment.NewLine}{Environment.NewLine}Fecha de recepción: ${correo.Fecha} {Environment.NewLine}Identificador: ${correo.IdMensaje}";
            correo.Cuerpo = cuerpo.Length > 1999
            ? $"Correo impreso, Fichero: {Path.GetFileName(archivoImpreso)}"
            : cuerpo;

            return (correo, adjuntos, archivoImpreso);
        }



    }

    internal static class LectorGmail
    {
        internal static List<MiCorreoDto> LeerMisCorreos(string guid, string buzon, int posicion, int cantidad, string filtroAsunto)
        {
            List<MiCorreoDto> correos = new();
            List<MiCorreoDto> correosOrdenados;

            if (cantidad + posicion == 0)
                return correos;

            var cantidadPorLeer = posicion + cantidad;

            var cacheDeMisCorreoAMostrar = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Del_Buzon);
            var indiceLeidos = $"{guid}-{buzon}";
            var correosAMostrar = (List<MiCorreoDto>)cacheDeMisCorreoAMostrar[indiceLeidos];

            if (correosAMostrar == null || correosAMostrar.Count == 0)
                return correos;

            var cacheDeMisCorreosAlmacenados = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_filtrados);
            var posicionLectora = -1;
            foreach (var correoDto in correosAMostrar)
            {
                var indiceDeDetalle = correoDto.IdMensaje;

                posicionLectora += 1;
                if (!(posicionLectora >= posicion && posicionLectora < cantidadPorLeer))
                    continue;

                correos.Add((MiCorreoDto)cacheDeMisCorreosAlmacenados[indiceDeDetalle]);
            }

            correosOrdenados = correos.OrderByDescending(c => c.Fecha).ToList();
            return correosOrdenados;
        }

        internal static int CachearMensajes(this GmailService service, ContextoSe contexto, string guid, string buzon, string idLabel, string filtroAsunto)
        {
            var cacheDelBuzon = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Del_Buzon);
            var indBuzon = $"{guid}-{buzon}";
            if (!cacheDelBuzon.ContainsKey(indBuzon))
            {
                cacheDelBuzon[indBuzon] = LeerMensajesDelBuzon(service,contexto,buzon, idLabel, filtroAsunto);
            }
            var correosAMostrar = (List<MiCorreoDto>)cacheDelBuzon[indBuzon];
            var total = correosAMostrar.Count;
            //foreach (var mensaje in mensajes)
            //{
            //    if (mensaje.EstaProcesado(contexto)) total--;
            //}
            return total;
        }

        internal static List<MiCorreoDto> LeerMensajesDelBuzon(this GmailService servicioGmail, ContextoSe contexto, string buzon, string idLabel, string filtroAsunto)
        {
            UsersResource.MessagesResource.ListRequest lectorDelBuzon = servicioGmail.Users.Messages.List("me");
            lectorDelBuzon.LabelIds = idLabel;
            lectorDelBuzon.MaxResults = 500;
            //lectorDelBuzon.Q = "from:ejemplo@ejemplo.com to:sistemadeelemento+tarea";

            var todosLosCorreosAMostrar = new List<MiCorreoDto>();
            string nextPageToken = null;
            do
            {
                lectorDelBuzon.PageToken = nextPageToken;
                var lectorDeGmail = lectorDelBuzon.Execute();
                var mensajesDeGmail = lectorDeGmail.Messages;
                var correosParaMostrar = new List<MiCorreoDto>();

                var cacheDeFiltrados = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_filtrados);
                if (todosLosCorreosAMostrar != null && mensajesDeGmail is not null && mensajesDeGmail.Count > 0)
                {
                    foreach (var mensajeDeGmail in mensajesDeGmail)
                    {
                        if (mensajeDeGmail.EstaProcesado(contexto))
                            continue;
                        var indiceDeDetalle = mensajeDeGmail.Id;
                        if (!cacheDeFiltrados.ContainsKey(indiceDeDetalle))
                        {
                            UsersResource.MessagesResource.GetRequest lectorDeMail = servicioGmail.Users.Messages.Get("me", mensajeDeGmail.Id);
                            Message mail = lectorDeMail.Execute();

                            var correo = CrearCorreoConAdjuntos(mail, buzon, servicioGmail);

                            cacheDeFiltrados[indiceDeDetalle] = correo;
                        }

                        if (!filtroAsunto.IsNullOrEmpty() && !((MiCorreoDto)cacheDeFiltrados[indiceDeDetalle]).Asunto.Contains(filtroAsunto, StringComparison.CurrentCultureIgnoreCase))
                            continue;

                        correosParaMostrar.Add((MiCorreoDto)cacheDeFiltrados[indiceDeDetalle]);
                    }

                    todosLosCorreosAMostrar.AddRange(correosParaMostrar);
                    nextPageToken = lectorDeGmail.NextPageToken;
                }
                else
                {
                    break;
                }

            } while (!string.IsNullOrEmpty(nextPageToken));
            return todosLosCorreosAMostrar;
        }

        private static DateTime Fecha(this Message mail)
        {
            var dateHeader = mail.Payload.Headers.FirstOrDefault(h => h.Name == "Date");

            if (dateHeader != null)
            {
                DateTime date;
                if (DateTime.TryParse(dateHeader.Value, out date))
                {
                    return date;
                }
                else
                {
                    extFechas.MilisegundosToFecha(mail.InternalDate);
                }
            }
            return extFechas.MilisegundosToFecha(mail.InternalDate);
        }

        internal static bool EstaProcesado(this Message mensaje, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Procesados);
            if (!cache.ContainsKey(mensaje.Id))
            {
                var a = contexto.SeleccionarPorPropiedad<MiCorreoDtm>(nameof(MiCorreoDtm.IdMensaje), mensaje.Id, errorSiNoHay: false);
                cache[mensaje.Id] = a is not null;
            }
            return (bool)cache[mensaje.Id];
        }

        private static List<Adjunto> Adjuntos(this GmailService servicioGmail, Message mail)
        {
            var adjuntos = new List<Adjunto>();

            if (mail.Payload.Parts != null && mail.Payload.Parts.Any(x => !x.Filename.IsNullOrEmpty()))
            {
                foreach (var part in mail.Payload.Parts.Where(x => !x.Filename.IsNullOrEmpty()))
                {
                    var adjunto = new Adjunto();
                    adjunto.Fichero = part.Filename;
                    adjunto.TipoMime = part.MimeType;
                    adjunto.IdMail = mail.Id;
                    adjunto.IdAdjunto = part.Body.AttachmentId;
                    adjunto.IdParte = part.PartId + '-' + part.Filename;
                    adjuntos.Add(adjunto);
                }
            }
            return adjuntos;
        }

        internal static Adjunto BuscarAdjunto(this GmailService servicioGmail, string idMail, string idParte)
        {
            UsersResource.MessagesResource.GetRequest lectorDeMail = servicioGmail.Users.Messages.Get("me", idMail);
            Message mail = lectorDeMail.Execute();
            var adjuntos = servicioGmail.Adjuntos(mail);
            //return adjuntos.First();
            return adjuntos.FirstOrDefault(x => x.IdParte == idParte);

        }

        public static void CambiarEtiqueta(this GmailService servicioGmail, string idMensaje, string etiquetaActual, string nuevaEtiqueta)
        {
            // Crear el objeto de modificación del mensaje
            var modificacion = new ModifyMessageRequest
            {
                AddLabelIds = new List<string> { nuevaEtiqueta },
                RemoveLabelIds = new List<string> { etiquetaActual }
            };

            // Realizar la modificación del mensaje
            servicioGmail.Users.Messages.Modify(modificacion, "me", idMensaje).Execute();
        }


        public static string ImprimirCorreoPDF(this GmailService servicioGmail, string idMail)
        {

            var mensaje = servicioGmail.Users.Messages.Get("me", idMail).Execute();
            if (mensaje == null)
                Emitir($"El correo '{idMail}' no se ha localizado en Gmail");

            var correo = CrearCorreo(mensaje, "");
            var sb = new StringBuilder();
            sb.AppendLine($"De: {correo.Emisor}");
            sb.AppendLine($"Para: {correo.To}");
            sb.AppendLine($"Fecha: {correo.Fecha}");
            sb.AppendLine($"Asunto: {correo.Asunto}");
            sb.AppendLine($"");
            sb.AppendLine($"Cuerpo");
            sb.AppendLine($"{correo.Cuerpo}");
            sb.AppendLine($"Adjuntos: {servicioGmail.Adjuntos(mensaje).ToString(x => x.Fichero, ",")}");
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

        private static MiCorreoDto CrearCorreoConAdjuntos(Message mail, string buzon, GmailService servicioGmail)
        {
            MiCorreoDto correo = CrearCorreo(mail, buzon);
            var adjuntos = servicioGmail.Adjuntos(mail);
            correo.ConAdjuntos = adjuntos.Count > 0;
            correo.Adjuntos = JsonConvert.SerializeObject(adjuntos);
            return correo;
        }

        private static MiCorreoDto CrearCorreo(Message mail, string buzon)
        {
            MiCorreoDto correo = new();
            var subjectHeader = mail.Payload.Headers.FirstOrDefault(h => h.Name == "Subject");
            var sender = mail.Payload.Headers.FirstOrDefault(h => h.Name == "From");
            var to = mail.Payload.Headers.FirstOrDefault(h => h.Name == "Delivered-To");
            if (to is null) to = mail.Payload.Headers.FirstOrDefault(h => h.Name == "To");
            correo.Id = extEncriptacion.GenerarEntero(mail.Id);
            correo.Buzon = buzon;
            correo.Asunto = subjectHeader?.Value ?? "(Sin Asunto}";
            correo.Emisor = sender.Value;
            correo.To = to.Value;
            correo.Cuerpo = mail.Cuerpo_3();
            correo.CuerpoHtml = extHtml.SanitizeHtml(mail.CuerpoHtml());
            correo.Fecha = mail.Fecha();
            correo.IdMensaje = mail.Id;
            correo.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
            return correo;
        }


        private static string Cuerpo(this Message mail)
        {
            if (mail.Payload.Body.Data != null)
            {
                string bodyBase64Url = mail.Payload.Body.Data;
                string base64String = bodyBase64Url.Replace('-', '+').Replace('_', '/').PadRight(bodyBase64Url.Length + (4 - bodyBase64Url.Length % 4) % 4, '=');
                string bodyText = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
                return bodyText;
            }

            if (mail.Payload.Parts != null)
            {
                foreach (var part in mail.Payload.Parts.Where(x => x.Body.Data != null))
                {
                    string bodyBase64Url = part.Body.Data;
                    string base64String = bodyBase64Url.Replace('-', '+').Replace('_', '/').PadRight(bodyBase64Url.Length + (4 - bodyBase64Url.Length % 4) % 4, '=');
                    string bodyText = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
                    return bodyText;
                }
            }

            return "(Sin cuerpo)";
        }

        private static string Cuerpo_2(this Message mail)
        {
            if (mail.Payload.Body.Data != null)
            {
                return extCadenas.DecodeBase64String(mail.Payload.Body.Data);
            }

            if (mail.Payload.Parts != null)
            {
                foreach (var part in mail.Payload.Parts)
                {
                    if (part.MimeType == "text/html" || part.MimeType == "text/plain")
                    {
                        return extCadenas.DecodeBase64String(part.Body.Data);
                    }
                }
            }

            return "(Sin cuerpo)";
        }
        private static string CuerpoHtml(this Message mail)
        {
            if (mail.Payload.Body.Data != null)
            {
                var cuerpo = extCadenas.DecodeBase64String(mail.Payload.Body.Data);
                return cuerpo.IsNullOrEmpty() || cuerpo == Environment.NewLine ? "<p>(Sin cuerpo)</p>" : cuerpo;
            }

            if (mail.Payload.Parts != null)
            {
                StringBuilder cuerpoHtml = new StringBuilder();

                foreach (var part in mail.Payload.Parts)
                {
                    if (part.MimeType == "text/html")
                    {
                        cuerpoHtml.Append(extCadenas.DecodeBase64String(part.Body.Data));
                    }
                    else if (part.MimeType == "multipart/alternative")
                    {
                        var htmlPart = part.Parts.FirstOrDefault(p => p.MimeType == "text/html");
                        if (htmlPart != null)
                        {
                            cuerpoHtml.Append(extCadenas.DecodeBase64String(htmlPart.Body.Data));
                        }
                    }
                }

                if (cuerpoHtml.Length > 0)
                {
                    return cuerpoHtml.ToString();
                }
            }

            // Si no se encuentra contenido HTML, convertir el texto plano a HTML
            var textoPlano = mail.Cuerpo_3();
            return textoPlano == "(Sin cuerpo)" ? "<p>(Sin cuerpo)</p>" : $"<pre>{HttpUtility.HtmlEncode(textoPlano)}</pre>";
        }

        private static string Cuerpo_3(this Message mail)
        {
            if (mail.Payload.Body.Data != null)
            {
                var cuerpo = extCadenas.DecodeBase64String(mail.Payload.Body.Data);
                return cuerpo.IsNullOrEmpty() || cuerpo == Environment.NewLine ? "(Sin cuerpo)" : cuerpo;
            }

            if (mail.Payload.Parts != null)
            {
                StringBuilder cuerpo = new StringBuilder();

                foreach (var part in mail.Payload.Parts)
                {
                    if (part.MimeType == "text/html")
                    {
                        cuerpo.AppendLine(extCadenas.ConvertHtmlToPlainText(extCadenas.DecodeBase64String(part.Body.Data)));
                    }
                    else if (part.MimeType == "text/plain")
                    {
                        cuerpo.AppendLine(extCadenas.DecodeBase64String(part.Body.Data));
                    }
                    else if (part.MimeType == "multipart/alternative")
                    {
                        var htmlPart = part.Parts.FirstOrDefault(p => p.MimeType == "text/html");
                        var plainPart = part.Parts.FirstOrDefault(p => p.MimeType == "text/plain");

                        if (htmlPart != null)
                        {
                            cuerpo.AppendLine(extCadenas.ConvertHtmlToPlainText(extCadenas.DecodeBase64String(htmlPart.Body.Data)));
                        }

                        if (plainPart != null)
                        {
                            cuerpo.AppendLine(extCadenas.DecodeBase64String(plainPart.Body.Data));
                        }

                    }
                }

                if (cuerpo.Length > 0)
                {
                    return cuerpo.ToString();
                }
            }

            return "(Sin cuerpo)";
        }


        //private static string DecodeBase64String(string base64String)
        //{
        //    string bodyBase64Url = base64String;
        //    base64String = bodyBase64Url.Replace('-', '+').Replace('_', '/').PadRight(bodyBase64Url.Length + (4 - bodyBase64Url.Length % 4) % 4, '=');
        //    return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        //}


    }
}


/*
 * 
 * 
 
string[] Scopes = { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify, GmailService.Scope.MailGoogleCom };
string ApplicationName = "Gmail";

UserCredential credential;
// Load client secrets.
string credPath = "token.json";
using (var stream = new FileStream(@"c:\Users\jjimenezc\desarrollo\Google\Gmail\cliente_escritorio.json", FileMode.Open, FileAccess.Read))
{
    * The file token.json stores the user's access and refresh tokens, and is created  automatically when the authorization flow completes for the first time. *
    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        Scopes,
        "jjimenezcf",
        CancellationToken.None,
        new FileDataStore(credPath, true)).Result;
    Console.WriteLine("Credential file saved to: " + credPath);
}

// GoogleCredential credenciales = GoogleCredential.FromFile(credPath).CreateScoped(new[] { "https://mail.google.com/" });

// Create Gmail API service.
var service = new GmailService(new BaseClientService.Initializer
{
    HttpClientInitializer = credential,
    ApplicationName = ApplicationName
});

// Define parameters of request.
UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

// List labels.
IList<Google.Apis.Gmail.v1.Data.Label> labels = request.Execute().Labels;
Console.WriteLine("Labels:");
if (labels == null || labels.Count == 0)
{
    Console.WriteLine("No labels found.");
    return;
}


// Crear el objeto SaslMechanism a partir de las credenciales de Google OAuth2
string accessToken = await credential.GetAccessTokenForRequestAsync();
var networkCredential = new NetworkCredential(accessToken, string.Empty);
var saslMechanism = new SaslMechanismOAuth2(networkCredential);


// Define parameters of request to get messages from the inbox
UsersResource.MessagesResource.ListRequest messageRequest = service.Users.Messages.List("me");
messageRequest.LabelIds = "INBOX";
messageRequest.MaxResults = 40; // Adjust this value to get more or fewer messages

// Execute request and get messages
IList<Message> messages = messageRequest.Execute().Messages;

// Print the subject of each message
Console.WriteLine("\nMessages in Inbox:");
if (messages == null || messages.Count == 0)
    return;

foreach (var message in messages)
{
    // Get the full message
    UsersResource.MessagesResource.GetRequest messageGetRequest = service.Users.Messages.Get("me", message.Id);
    Message fullMessage = messageGetRequest.Execute();

    // Print the subject of the message
    var subjectHeader = fullMessage.Payload.Headers.FirstOrDefault(h => h.Name == "Subject");
    Console.WriteLine($"Subject:{Environment.NewLine}{subjectHeader?.Value ?? "(No subject)"}{Environment.NewLine}");

    // Print the body of the message
    Console.WriteLine($"Body:{fullMessage.Raw}{Environment.NewLine}");



    // Print the attachments of the message
    if (fullMessage.Payload.Parts != null && fullMessage.Payload.Parts.Any(x => x.Filename != null && x.Filename.Trim() != ""))
    {
        foreach (var part in fullMessage.Payload.Parts.Where(x => x.Filename != null && x.Filename.Trim() != ""))
        {

            Console.WriteLine($"Attachment: {part.Filename}, Mime: {part.MimeType}");

            var attachmentRequest = service.Users.Messages.Attachments.Get("me", fullMessage.Id, part.Body.AttachmentId);
            var attachment = attachmentRequest.Execute();
            string downloadPath = Path.Combine(@"c:\Users\jjimenezc\desarrollo\Google\Gmail", part.Filename);
            //if (part.MimeType.Contains("text"))
            //{
            //    byte[] bytes = Convert.FromBase64CharArray(attachment.Data.ToCharArray(), 0, attachment.Data.ToCharArray().Length);
            //    File.WriteAllBytes(downloadPath, bytes);
            //}
            //else
            //{

            //var a = WebEncoders.Base64UrlDecode(attachment.Data);
            string base64String = attachment.Data.Replace('-', '+').Replace('_', '/').PadRight(attachment.Data.Length + (4 - attachment.Data.Length % 4) % 4, '=');


            // Convert the URL-safe base64 string to a byte array
            byte[] decodedData = Convert.FromBase64String(base64String);

            //byte[] bytes = Encoding.Default.GetBytes(a);
            BinaryWriter writer = new BinaryWriter(new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            writer.Write(decodedData);
            writer.Close();
            //}

            Console.WriteLine($"Archivo '{part.Filename}' descargado en '{downloadPath}'.");
        }
    }
    else
    {
        Console.WriteLine($"No attachments found.{Environment.NewLine}");
    }

}






*/