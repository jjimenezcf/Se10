using Microsoft.AspNetCore.Mvc;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Flows;
using Utilidades;
using ModeloDeDto.Entorno;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using System.Threading;
using ServicioDeDatos;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.TrabajosSometidos;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using System;
using ModeloDeDto.SistemaDocumental;
using GestorDeElementos;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;
using Microsoft.Extensions.Configuration;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Controllers
{
    public class MiCorreoController : BaseController<MiCorreoDto>
    {
        private enumMiCorreoModoAcceso? _Protocolo;
        private string _ClienteSecreto;
        private string _MiCorreo;

        public MiCorreoController(GestorDeErrores gestorDeErrores, ContextoSe contexto, IMapper mapeador, IConfiguration configuration)
        : base(gestorDeErrores, contexto, mapeador)
        {
            var seccion = configuration.GetSection(typeof(ltrAppSetting.DatosIniciales).Name);
            if (seccion is null) throw new Exception($"Debe definir la sección de '{typeof(ltrAppSetting.DatosIniciales).Name}' en el AppSetting");

            _MiCorreo = seccion[ltrAppSetting.DatosIniciales.MiCorreo];
            if (_MiCorreo is null)
                throw new Exception($"Debe definir en la sección de 'DatosIniciales' del AppSetting la clave '{ltrAppSetting.DatosIniciales.MiCorreo}'");

            _ClienteSecreto = seccion[ltrAppSetting.DatosIniciales.ClienteSecreto];
            if (_ClienteSecreto is null)
                throw new Exception($"Debe definir en la sección de 'DatosIniciales' del AppSetting la clave '{ltrAppSetting.DatosIniciales.ClienteSecreto}'");

            try
            {
                _Protocolo = ApiDeEnsamblados.ToEnumerado<enumMiCorreoModoAcceso>(seccion[ltrAppSetting.DatosIniciales.Protocolo]);
            }
            catch
            {
                throw new Exception($"Ha de definir el protocolo de acceso al correo, valores válidos: {enumMiCorreoModoAcceso.Auth2}, {enumMiCorreoModoAcceso.IMAP}, {enumMiCorreoModoAcceso.ApiKey}");
            }
        }

        public JsonResult epPeticionDeAcceso()
        {
            var r = new Resultado();
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            if (_Protocolo == enumMiCorreoModoAcceso.IMAP)
            {
                r.Datos = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{nameof(MiCorreoController).Replace("Controller", "")}/{nameof(CrudDeMiCorreoImap)}";
            }
            else if (_Protocolo == enumMiCorreoModoAcceso.ApiKey)
            {
                r.Datos = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{nameof(MiCorreoController).Replace("Controller", "")}/{nameof(CrudDeMiCorreoApiKey)}";
            }
            else
            {
                var cache = ServicioDeCaches.Obtener(CacheDe.Ent_FlujodeAutorizacion);
                Contexto.AnotarTraza("Apertura de fichero", Path.Combine(ltrLectorDeCorreo.RutaDeTokens, _ClienteSecreto));
                using (var ficheroAuthJson = new FileStream(Path.Combine(ltrLectorDeCorreo.RutaDeTokens, _ClienteSecreto), FileMode.Open, FileAccess.Read))
                {
                    var secrets = GoogleClientSecrets.FromStream(ficheroAuthJson).Secrets;
                    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = secrets,
                        Scopes = new[] { GmailService.Scope.GmailModify },
                        DataStore = new FileDataStore(ltrLectorDeCorreo.RutaDeTokens, true)
                    });

                    cache[_ClienteSecreto] = flow;
                }

                var f = (GoogleAuthorizationCodeFlow)cache[_ClienteSecreto];

                // Redirigir al usuario a la URL de autorización de Google
                r.Datos = f.CreateAuthorizationCodeRequest(@$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{nameof(MiCorreoController).Replace("Controller", "")}/{nameof(CrudDeMiCorreo)}").Build();
            }

            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = $"Petición de permisos realizada";
            return new JsonResult(r);
        }

        public IActionResult CrudDeMiCorreoImap()
        {
            return CrudDeMiCorreo("", "");
        }

        public IActionResult CrudDeMiCorreoApiKey()
        {
            return CrudDeMiCorreo("", "");
        }

        public IActionResult CrudDeMiCorreo(string code, string state)
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var cacheCredenciales = ServicioDeCaches.Obtener(CacheDe.Ent_MisCorreos_Credenciales);
            if (!cacheCredenciales.ContainsKey(_MiCorreo) && !code.IsNullOrEmpty())
            {
                cacheCredenciales[_MiCorreo] = ObtenerCredencialesUsandoAuth2(code);
            }


            var titulo = $"Correo de ({_MiCorreo})";
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{titulo}-{typeof(DescriptorDeMiCorreo).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Entorno}/{nameof(CrudDeMiCorreo)}";
                    return base.View(destino, new DescriptorDeMiCorreo(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = new DescriptorDeMiCorreo(Contexto, ModoDescriptor.Mantenimiento, titulo);
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }

        }

        [HttpPost]
        public JsonResult epLeerDatosPost(string modo, string accion, string posicion, string cantidad)
        {
            var body = ApiController.LeerBody(HttpContext);
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            ResultadoDeLectura<MiCorreoDto> infoObtenida = null;
            var r = new Resultado();
            try
            {
                var pos = posicion.Entero();
                var can = cantidad.Entero();

                var filtro = body.parametros.LeerValor<string>(ltrParametrosEp.Filtro);
                var orden = body.parametros.LeerValor<string>(ltrParametrosEp.Orden);
                List<ClausulaDeFiltrado> filtros = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
                List<ClausulaDeOrdenacion> ordenes = orden == null ? new List<ClausulaDeOrdenacion>() : JsonConvert.DeserializeObject<List<ClausulaDeOrdenacion>>(orden);
                var guid = filtros.First(x => x.Clausula == ltrParametrosEp.guid).Valor;
                var idBuzon = filtros.First(x => x.Clausula.ToLower() == nameof(MiCorreoDto.Buzon).ToLower()).Valor;
                var buzon = Contexto.SeleccionarPorId<BuzonDeMiSociedadDtm>(idBuzon.Entero(), aplicarPermisos: true).Buzon;
                if (accion == epAcciones.buscar.ToString())
                {
                    var filtroAsunto = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(MiCorreoDto.Asunto).ToLower())?.Valor;
                    ServicioDeCaches.EliminarElemento(CacheDe.Ent_MisCorreos_Cantidad, $"{guid}-{buzon}-{filtroAsunto}");
                    ServicioDeCaches.EliminarElemento(CacheDe.Ent_MisCorreos_Del_Buzon, $"{guid}-{buzon}");
                }

                switch (_Protocolo)
                {
                    case enumMiCorreoModoAcceso.ApiKey:
                        infoObtenida = LeerDatosParaElGridApiKey(modo, accion, pos, can, guid, buzon, filtros, ordenes, body.parametrosJson);
                        break;
                    case enumMiCorreoModoAcceso.Auth2:
                        infoObtenida = LeerDatosParaElGridAuth2(modo, accion, pos, can, guid, buzon, filtros, ordenes, body.parametrosJson);
                        break;
                    case enumMiCorreoModoAcceso.IMAP:
                        infoObtenida = LeerDatosParaElGridImap(modo, accion, pos, can, guid, buzon, filtros, ordenes, body.parametrosJson);
                        break;
                    default: throw new Exception($"no se ha implementado cómo leer los datos del modo {_Protocolo}");
                }
                r.Estado = enumEstadoPeticion.Ok;
                r.Datos = infoObtenida;
                r.Mensaje = pos > 0 && !infoObtenida.registros.Any() ? "No hay más elementos" : "";
            }
            catch (Exception ex)
            {
                ApiController.PrepararError(ex, r, "Error al acceder a leer los correo");
            }
            var a = new JsonResult(r);
            return a;
        }

        public ResultadoDeLectura<MiCorreoDto> LeerDatosParaElGridImap(string modo, string accion, int pos, int can, string guid, string buzon, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, string parametrosJson)
        {
            var filtroAsunto = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(MiCorreoDto.Asunto).ToLower())?.Valor;
            var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
            try
            {
                var total = clienteImap.Total(guid, buzon, filtroAsunto);

                if (accion == epAcciones.ultima.ToString())
                    pos = total - can;

                var correos = clienteImap.LeerMensajesDelBuzon(guid, buzon, pos, can, filtroAsunto);
                var datos = (elementos: correos, totalLeidos: correos.Count);

                return new ResultadoDeLectura<MiCorreoDto>(datos.elementos, pos, can, total);
            }
            finally
            {
                clienteImap.Desconectar();
            }
        }

        public ResultadoDeLectura<MiCorreoDto> LeerDatosParaElGridApiKey(string modo, string accion, int pos, int can, string guid, string buzon, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, string parametrosJson)
        {
            var filtroAsunto = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(MiCorreoDto.Asunto).ToLower())?.Valor;
            var total = LectorDeGmailApiKey.CachearMisCorreos(Contexto, guid, buzon, "apiKey", filtroAsunto);

            if (accion == epAcciones.ultima.ToString())
                pos = total - can;

            var correos = LectorDeGmailApiKey.LeerMisCorreos(guid, buzon, pos, can, filtroAsunto);
            var datos = (elementos: correos, totalLeidos: correos.Count);

            return new ResultadoDeLectura<MiCorreoDto>(datos.elementos, pos, can, total);
        }

        public ResultadoDeLectura<MiCorreoDto> LeerDatosParaElGridAuth2(string modo, string accion, int pos, int can, string guid, string buzon, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, string parametrosJson)
        {
            var filtroAsunto = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(MiCorreoDto.Asunto).ToLower())?.Valor;

            GoogleCredential credenciales = LectorDeGmailAuth2.Credenciales(_MiCorreo);
            var total = LectorDeGmailAuth2.CachearMisCorreos(Contexto, guid, buzon, credenciales, filtroAsunto);

            if (accion == epAcciones.ultima.ToString())
                pos = total - can;

            var correos = LectorDeGmailAuth2.LeerMisCorreos(guid, buzon, pos, can, filtroAsunto);
            var datos = (elementos: correos, totalLeidos: correos.Count);

            return new ResultadoDeLectura<MiCorreoDto>(datos.elementos, pos, can, total);
        }

        public JsonResult epArchivarCorreo(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.id))) throw new Exception("Debe indicar el id interno del mensaje a archivar");
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.idTipo))) throw new Exception("Debe indicar el id de tipo de archivador al que archivar el correo");
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.IdCg))) throw new Exception("Debe indicar el id del cg al que archivar el correo");
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.enumNegocio))) throw new Exception("Debe indicar el negocio al que asociar el correo");

                var idCorreo = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.id));
                var idTipo = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.idTipo));
                var idCg = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.IdCg));
                var nombre = parametros.LeerValor<string>(nameof(ArchivoDto.Nombre));
                var negocio = parametros.LeerValor<enumNegocio>(nameof(ltrParametrosEp.enumNegocio));
                IElementoDto elementoDto;
                switch (_Protocolo)
                {
                    case enumMiCorreoModoAcceso.Auth2:
                        elementoDto = LectorDeGmailAuth2.ArchivarCorreo(Contexto, negocio, idCorreo, idTipo, idCg, nombre, _MiCorreo, parametros);
                        break;
                    case enumMiCorreoModoAcceso.IMAP:
                        var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
                        try
                        {
                            elementoDto = clienteImap.ArchivarCorreo(negocio, idCorreo, idTipo, idCg, nombre, parametros);
                            clienteImap.DarPorProcesado(idCorreo);
                        }
                        finally
                        {
                            clienteImap.Desconectar();
                        }
                        break;
                    default: throw new Exception($"no se ha implementado cómo leer los datos del modo {_Protocolo}");
                }

                r.Consola = $"Correo archivado correctamente";
                r.Datos = elementoDto;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ExtensorDeMiCorreo.VaciarCaches();
                ApiController.PrepararError(e, r, "Error al archivar el correo.");
            }
            return new JsonResult(r);
        }

        public JsonResult epAsociarAlElemento(string parametrosJson)
        {
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(MiCorreoDto.Id)))
                    throw new Exception("Debe indicar el id del correo para poder eliminar");
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.enumNegocio))) throw new Exception("Debe indicar el negocio al que asociar el correo");
                var negocio = parametros.LeerValor<enumNegocio>(nameof(ltrParametrosEp.enumNegocio));
                var idElemento = 0;
                var idCarpetaDestino = 0;

                var idArchivadorDestino = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdArchivadorDeDestino), 0);
                idCarpetaDestino = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdCarpetaDeDestino), 0);
                var idTareaDestino = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdTareaDeDestino), 0);
                if (negocio == enumNegocio.Archivador)
                {
                    if (!parametros.ContieneClave(nameof(ltrDeUnArchivador.IdArchivador))) GestorDeErrores.Emitir("Debe indicar el archivador donde asociar");
                    if (!parametros.ContieneClave(nameof(ltrDeUnArchivador.IdCarpeta))) GestorDeErrores.Emitir("Debe indicar el la carpeta donde asociar");
                    idElemento = (int)parametros.LeerValor<long>(nameof(ltrDeUnArchivador.IdArchivador));
                    idCarpetaDestino = (int)parametros.LeerValor<long>(nameof(ltrDeUnArchivador.IdCarpeta));
                }

                if (negocio == enumNegocio.Tarea)
                {
                    if (!parametros.ContieneClave(nameof(SeleccionarComoVincularDto.IdTarea))) GestorDeErrores.Emitir("Debe indicar el la tarea donde asociar");
                    idElemento = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdTarea));
                }
                if (negocio == enumNegocio.Registro)
                {
                    if (!parametros.ContieneClave(nameof(SeleccionarComoVincularDto.IdRegistroEs))) GestorDeErrores.Emitir("Debe indicar el registro de E/S donde asociar");
                    idElemento = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdRegistroEs));
                }
                if (negocio == enumNegocio.FacturaRecibida)
                {
                    if (!parametros.ContieneClave(nameof(SeleccionarComoVincularDto.IdFacturaRec))) GestorDeErrores.Emitir("Debe indicar el la factura a la que asociar el correo");
                    idElemento = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdFacturaRec));
                }
                if (negocio == enumNegocio.Expediente)
                {
                    if (!parametros.ContieneClave(nameof(SeleccionarComoVincularDto.IdExpediente))) GestorDeErrores.Emitir("Debe indicar el expediente al que asociar el correo");
                    idElemento = (int)parametros.LeerValor<long>(nameof(SeleccionarComoVincularDto.IdExpediente));
                }
                var idCorreo = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.id));
                if (idElemento == 0)
                    GestorDeErrores.Emitir($"Debe indicar '{negocio.Singular()}' en el que asociar el correo");

                switch (negocio)
                {
                    case enumNegocio.Archivador:
                        AsociarCorreoEnArchivador(negocio, idElemento, idCarpetaDestino, idCorreo);
                        break;
                    case enumNegocio.Tarea:
                        AsociarCorreoEnTarea(negocio, idElemento, idCarpetaDestino, idArchivadorDestino, idCorreo);
                        break;
                    case enumNegocio.Registro:
                        AsociarCorreoEnRegistroEs(negocio, idElemento, idCarpetaDestino, idArchivadorDestino, idCorreo, idTareaDestino);
                        break;
                    case enumNegocio.Expediente:
                        AsociarCorreoEnExpediente(negocio, idElemento, idCarpetaDestino, idArchivadorDestino, idCorreo, idTareaDestino);
                        break;
                    case enumNegocio.FacturaRecibida:
                        AsociarCorreoEnFacturaRec(negocio, idElemento, idCarpetaDestino, idArchivadorDestino, idCorreo);
                        break;
                }

                r.Consola = $"Correo asociado correctamente";
                r.Datos = null;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ExtensorDeMiCorreo.VaciarCaches();
                ApiController.PrepararError(e, r, "Error al asociar el correo.");
            }
            return new JsonResult(r);
        }

        public FileStreamResult epDescargarAdjunto(string idMail, string idAdjunto, string idParte)
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var ruta = "";
            try
            {
                switch (_Protocolo)
                {
                    case enumMiCorreoModoAcceso.Auth2:
                        GoogleCredential credenciales = LectorDeGmailAuth2.Credenciales(_MiCorreo);
                        ruta = LectorDeGmailAuth2.DescargarAdjunto(idMail, idAdjunto, idParte, credenciales);
                        break;
                    case enumMiCorreoModoAcceso.IMAP:
                        ruta = DescargarAdjuntos(idMail, idAdjunto);
                        break;
                    default: throw new Exception($"no se ha implementado cómo imprimir los datos del modo {_Protocolo}");
                }
                return DevolverStream(ruta);
            }
            catch
            {
                return DevolverStream(ApiDeArchivos.FicheroNoEncontrado);
            }
        }

        public FileStreamResult epImprimirCorreo(string buzon, string idMail)
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var ruta = "";

            switch (_Protocolo)
            {
                case enumMiCorreoModoAcceso.Auth2:
                    GoogleCredential credenciales = LectorDeGmailAuth2.Credenciales(_MiCorreo);
                    ruta = LectorDeGmailAuth2.ImprimirCorreo(idMail, credenciales);
                    break;
                case enumMiCorreoModoAcceso.IMAP:
                    ruta = ImprimirCorreo(buzon, idMail);
                    break;
                default: throw new Exception($"no se ha implementado cómo imprimir los datos del modo {_Protocolo}");
            }

            return DevolverStream(ruta);
        }

        public JsonResult epEliminarCorreo(string parametrosJson)
        {
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(MiCorreoDto.Id))) throw new Exception("Debe indicar el id del correo para poder eliminar");

                switch (_Protocolo)
                {
                    case enumMiCorreoModoAcceso.Auth2:
                        GoogleCredential credenciales = LectorDeGmailAuth2.Credenciales(_MiCorreo);
                        LectorDeGmailAuth2.EliminarCorreo(Contexto, parametros.LeerValor<int>(nameof(MiCorreoDto.Id)), credenciales);
                        break;
                    case enumMiCorreoModoAcceso.IMAP:
                        EliminarCorreo(parametros.LeerValor<int>(nameof(MiCorreoDto.Id)));
                        break;
                    default: throw new Exception($"no se ha implementado cómo imprimir los datos del modo {_Protocolo}");
                }
                r.Consola = $"Correo eliminado correctamente";
                r.Datos = null;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ExtensorDeMiCorreo.VaciarCaches();
                ApiController.PrepararError(e, r, "Error al eliminar el correo.");
            }
            return new JsonResult(r);
        }

        protected override MiCorreoDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            switch (_Protocolo)
            {
                case enumMiCorreoModoAcceso.Auth2:
                    return LectorDeGmailAuth2.LeerPorId(id);
                case enumMiCorreoModoAcceso.IMAP:
                    return LeerPorId(id);
                default: throw new Exception($"no se ha implementado cómo leer los datos del modo {_Protocolo}");
            }
        }

        private GoogleCredential ObtenerCredencialesUsandoAuth2(string code)
        {
            var cacheFlujoAuth2 = ServicioDeCaches.Obtener(CacheDe.Ent_FlujodeAutorizacion);

            var f = (GoogleAuthorizationCodeFlow)cacheFlujoAuth2[_ClienteSecreto];
            var token = f.ExchangeCodeForTokenAsync(_MiCorreo, code, $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{nameof(MiCorreoController).Replace("Controller", "")}/{nameof(CrudDeMiCorreo)}", CancellationToken.None).Result;

            string ficheroDeCredenciales = Path.Combine(ltrLectorDeCorreo.RutaDeTokens, $"{_MiCorreo}.tkrp");
            var contenidoToken = JsonConvert.SerializeObject(token);
            System.IO.File.WriteAllText(ficheroDeCredenciales, contenidoToken);

            TokenResponse tokenLeido = JsonConvert.DeserializeObject<TokenResponse>(contenidoToken);
            GoogleCredential credenciales = GoogleCredential.FromAccessToken(tokenLeido.AccessToken);
            return credenciales;
        }

        public FileStreamResult ObtenerArchivo(string ruta)
        {
            return DevolverStream(ruta);
        }

        private void AsociarCorreoEnArchivador(enumNegocio negocio, int idElemento, int idCarpetaDestino, int idCorreo)
        {
            if (_Protocolo == enumMiCorreoModoAcceso.Auth2) LectorDeGmailAuth2.AsociarCorreoEnArchivador(Contexto, idCorreo, negocio, idElemento, idCarpetaDestino, _MiCorreo);
            else
            {
                var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
                try
                {
                    clienteImap.AsociarCorreoEnArchivador(idCorreo, negocio, idElemento, idCarpetaDestino);
                    clienteImap.DarPorProcesado(idCorreo);
                }
                finally
                {
                    clienteImap.Desconectar();
                }
            }
        }

        private void AsociarCorreoEnFacturaRec(enumNegocio negocio, int idElemento, int idCarpetaDestino, int idArchivadorDestino, int idCorreo)
        {
            if (_Protocolo == enumMiCorreoModoAcceso.Auth2) LectorDeGmailAuth2.AsociarCorreoEnFacturaRec(Contexto, idCorreo, negocio, idElemento, idArchivadorDestino, idCarpetaDestino, _MiCorreo);
            else
            {
                var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
                try
                {
                    clienteImap.AsociarCorreoEnFacturaRec(idCorreo, negocio, idElemento, idArchivadorDestino, idCarpetaDestino);
                    clienteImap.DarPorProcesado(idCorreo);
                }
                finally
                {
                    clienteImap.Desconectar();
                }
            }
        }

        private void AsociarCorreoEnTarea(enumNegocio negocio, int idElemento, int idCarpetaDestino, int idArchivadorDestino, int idCorreo)
        {
            if (_Protocolo == enumMiCorreoModoAcceso.Auth2) LectorDeGmailAuth2.AsociarCorreoEnTarea(Contexto, idCorreo, negocio, idElemento, idArchivadorDestino, idCarpetaDestino, _MiCorreo);
            else
            {
                var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
                try
                {
                    clienteImap.AsociarCorreoEnTarea(idCorreo, negocio, idElemento, idArchivadorDestino, idCarpetaDestino);
                    clienteImap.DarPorProcesado(idCorreo);
                }
                finally
                {
                    clienteImap.Desconectar();
                }
            }
        }

        private void AsociarCorreoEnRegistroEs(enumNegocio negocio, int idElemento, int idCarpetaDestino, int idArchivadorDestino, int idCorreo, int idTareaDestino)
        {
            if (_Protocolo == enumMiCorreoModoAcceso.Auth2) LectorDeGmailAuth2.AsociarCorreoEnRegistroEs(Contexto, idCorreo, negocio, idElemento, idTareaDestino, idArchivadorDestino, idCarpetaDestino, _MiCorreo);
            else
            {
                var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
                try
                {
                    clienteImap.AsociarCorreoEnRegistroEs(idCorreo, negocio, idElemento, idTareaDestino, idArchivadorDestino, idCarpetaDestino);
                    clienteImap.DarPorProcesado(idCorreo);
                }
                finally
                {
                    clienteImap.Desconectar();
                }
            }
        }

        private void AsociarCorreoEnExpediente(enumNegocio negocio, int idElemento, int idCarpetaDestino, int idArchivadorDestino, int idCorreo, int idTareaDestino)
        {
            if (_Protocolo == enumMiCorreoModoAcceso.Auth2) LectorDeGmailAuth2.AsociarCorreoEnExpediente(Contexto, idCorreo, negocio, idElemento, idTareaDestino, idArchivadorDestino, idCarpetaDestino, _MiCorreo);
            else
            {
                var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
                try
                {
                    clienteImap.AsociarCorreoEnExpediente(idCorreo, negocio, idElemento, idTareaDestino, idArchivadorDestino, idCarpetaDestino);
                    clienteImap.DarPorProcesado(idCorreo);
                }
                finally
                {
                    clienteImap.Desconectar();
                }
            }
        }

        private string DescargarAdjuntos(string idMail, string idAdjunto)
        {
            var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
            try
            {
                return clienteImap.DescargarAdjunto(idMail, idAdjunto);
            }
            finally
            {
                clienteImap.Desconectar();
            }
        }

        private string ImprimirCorreo(string buzon, string idMail)
        {
            var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
            try
            {
                return clienteImap.ImprimirCorreo(buzon, idMail);
            }
            finally
            {
                clienteImap.Desconectar();
            }
        }

        private MiCorreoDto LeerPorId(int idCorreo)
        {
            var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
            try
            {
                return clienteImap.LeerCorreo(idCorreo);
            }
            finally
            {
                clienteImap.Desconectar();
            }

        }

        private void EliminarCorreo(int idCorreo)
        {
            var clienteImap = new LectorDeCoreoImap(Contexto, _MiCorreo, _ClienteSecreto);
            try
            {
                clienteImap.EliminarCorreo(idCorreo);
                clienteImap.DarPorProcesado(idCorreo);
            }
            finally
            {
                clienteImap.Desconectar();
            }
        }
    }
}