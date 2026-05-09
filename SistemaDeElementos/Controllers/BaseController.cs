using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Descriptores;
using SistemaDeElementos.UtilidadesIu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;
using static SistemaDeElementos.Middleware.EliminarFicherosMiddelware;

namespace MVCSistemaDeElementos.Controllers
{

    public class BaseController<TElemento> : Controller
    where TElemento : ElementoDto
    {
        protected GestorDeErrores GestorDeErrores { get; }
        public ILogger Logger { get; set; }
        protected DatosDeConexion DatosDeConexion => Contexto.DatosDeConexion;
        public IMapper Mapeador => Contexto.Mapeador;

        protected ContextoSe Contexto { get; private set; }

        public BaseController(GestorDeErrores gestorDeErrores, ContextoSe contexto, IMapper mapeador)
        {
            GestorDeErrores = gestorDeErrores;
            Contexto = contexto;
            Contexto.Mapeador = mapeador;
            Contexto.IniciarTraza(GetType().Name);
        }

        protected override void Dispose(bool disposing)
        {
            Contexto.CerrarTraza();
            base.Dispose(disposing);
        }

        protected IActionResult RenderizarErrorDe(string indice, Exception e)
        {
            var partes = indice.Split(Simbolos.Guion);
            var clase = partes.Length != 3 ? indice : partes[2].Split(Simbolos.Punto)[0];
            Contexto.RegistrarConEnvio($"Error al renderizar el crud de '{clase}'", e);
            ServicioDeCaches.EliminarElemento(CacheDe.RenderCrud, indice);
            return RenderMensaje(e.Message);
        }

        protected ViewResult RenderMensaje(string mensaje)
        {
            Contexto.RegistrarConEnvio($"Error al renderizar el elemento de la clase '{typeof(TElemento).Name}'", mensaje);
            ViewBag.Mensaje = mensaje;
            ViewBag.DatosDeConexion = DatosDeConexion;
            return View(nameof(RenderMensaje));
        }


        public JsonResult epCrearRelacionPost(int idNegocio)
        {
            var body = ApiController.LeerBody(HttpContext);
            return epCrearRelacion(idNegocio, body.parametros[ltrParametrosEp.ElementoJson].ToString());
        }


        public virtual JsonResult epCrearRelacion(int idNegocio, string elementoJson)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// END-POIN: desde el ApiDeArchivos. Sube un fichero al gestor documental o a la ruta indicada
        /// </summary>
        /// <param name="fichero">fichero a subir</param>
        /// <param name="rutaDestino">si no se sube al gestor documenta, nombre de la ruta donde se almacenará</param>
        /// <param name="extensionesValidas">extensiones que ha de tener el archivo a subir</param>
        /// <returns>0 si no ha subido al gestor documental, o id del archivo subido al gestor documental</returns>
        [HttpPost]
        public JsonResult epSubirArchivo(IFormFile fichero, string rutaDestino, string extensionesValidas)
        {

            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epSubirArchivo));
            try
            {
                Contexto.AnotarParametros(new List<object> { rutaDestino, extensionesValidas });
                return SubirArchivo(Contexto, Mapeador, HttpContext, fichero, rutaDestino, extensionesValidas);
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }

        public static JsonResult SubirArchivo(ContextoSe contexto, IMapper mapeador, HttpContext httpContext, IFormFile fichero, string rutaDestino, string extensionesValidas)
        {
            var r = new Resultado();

            try
            {
                if (fichero == null)
                {
                    GestorDeErrores.Emitir("No se ha identificado el fichero");
                }

                ApiController.CumplimentarDatosDeUsuarioDeConexion(contexto, mapeador, httpContext);
                ValidarExtension(fichero, extensionesValidas.ToLower());
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, fichero.FileName);
                using (var stream = new FileStream(rutaConFichero, FileMode.Create))
                {
                    fichero.CopyTo(stream);
                }

                if (rutaDestino.IsNullOrEmpty())
                {
                    r.Datos = contexto.SubirArchivo(rutaConFichero, sanitizar: true);
                }
                else
                {
                    rutaDestino = Path.Combine(GestorDeVariables.RutaBaseConDestino, rutaDestino); // $@"{GestorDeVariables.RutaBaseConDestino}\{rutaDestino.Replace("/", @"\")}".Replace(@"\\", @"\");

                    if (!Directory.Exists(rutaDestino))
                    {
                        Directory.CreateDirectory(rutaDestino);
                    }

                    int numero = 1;
                    var ficheroSinExtension = Path.GetFileNameWithoutExtension(fichero.FileName).Replace(" ", "_");
                    var extension = Path.GetExtension(fichero.FileName);
                    while (System.IO.File.Exists(Path.Combine(rutaDestino, $@"{ficheroSinExtension}{extension}")))
                    {
                        if (numero == 1)
                        {
                            ficheroSinExtension = $"{ficheroSinExtension}_{numero}";
                        }
                        else
                        {
                            ficheroSinExtension = ficheroSinExtension.Replace($"_{numero - 1}", $"_{numero}");
                        }

                        numero++;
                    }

                    System.IO.File.Move(rutaConFichero, Path.Combine(rutaDestino, $@"{ficheroSinExtension}{extension}"));

                    r.Datos = $@"{ficheroSinExtension}{extension}";
                }

                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "fichero subido";
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                {
                    r.Mensaje = e.Message;
                }
                else
                {
                    r.Mensaje = $"No se ha podido subir el fichero. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
                }
            }


            return new JsonResult(r);

        }



        public JsonResult EpNegociosParaAdjuntarDocumentacion(bool incluirSinPermisos)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epVincular));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var negociosPermitidos = NegociosDeSe.NegociosQuePermitenSubirDocumentacionDesdeElMovilAlUsuarioConectado(Contexto, incluirSinPermisos);
                r.Datos = negociosPermitidos;
                r.Total = negociosPermitidos.Count;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Consola = $"Lista de negocios leidos";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al los negocios a los que incluirle documentación");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerDatosParaInicializarVista(string negocio, string parametrosJson)
        {
            var r = new Resultado();
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epVincular));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var modoDeAcceso = ApiDePermisos.LeerModoDeAccesoAlNegocio(Contexto, NegociosDeSe.ToEnumerado(negocio));

                if (parametros.LeerValor<enumModoDeTrabajo>(ltrParametrosEp.Modo) == enumModoDeTrabajo.Nuevo)
                {
                    var enumNegocio = NegociosDeSe.ToEnumerado(negocio);

                    r.Datos = DatosParaInicializarLaCreacion(enumNegocio, parametros, new DatosDeCreacion(Contexto, enumNegocio));
                }
                else
                    if (parametros.LeerValor<enumModoDeTrabajo>(ltrParametrosEp.Modo) == enumModoDeTrabajo.Mantenimiento)
                    {
                        var enumNegocio = NegociosDeSe.ToEnumerado(negocio);
                        parametros[nameof(enumNegocio)] = enumNegocio;
                        r.Datos = new DatosParaElMantenimiento(
                            espanes: new EstadosDeEspanes(Contexto, NegociosDeSe.ToEnumerado(negocio), parametros).Estados,
                            filtros: enumNegocio == enumNegocio.No_Definido
                                     ? null
                                     : Contexto.SeleccionarDtos<PlantillaDeFiltradoDto, PlantillaDeFiltradoDtm>(new Dictionary<string, object>
                                          {
                                          { nameof(PlantillaDeFiltradoDtm.IdNegocio), enumNegocio.IdNegocio()},
                                          { nameof(PlantillaDeFiltradoDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario},
                                          { nameof(PlantillaDeFiltradoDtm.Vista), parametros.LeerValor<string>(nameof(PlantillaDeFiltradoDtm.Vista)) }
                                          }),
                            indicadores: IndicadoresParaInicializarLaVistaMnt(Contexto, parametros)
                            );
                    }

                r.ModoDeAcceso = modoDeAcceso.Render();
                r.Consola = $"El usuario {DatosDeConexion.Login} tiene permisos de {modoDeAcceso}";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al obtener los datos de inicialización de la vista asociada al negocio {negocio} para el usuario {DatosDeConexion.Login}.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        protected virtual Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = new Dictionary<string, object>
            {
                { IndCrud.SiempreEnConsulta, false },
                { IndCrud.CapaConVinculados, enumNegocio.VistaMvc.Parametro(enumParametrosDeVistaMvc.Con_Capa_Los_Vinculados,crearParametro: true, valorPorDefecto: "S").Valor.EsTrue() },
            };
            var negocio = parametros.LeerValor(nameof(enumNegocio), enumNegocio.No_Definido);
            if (negocio != enumNegocio.No_Definido)
            {
                var tamanoDelVisorJobject = (JObject)negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Tamano_Del_Visor);
                if (tamanoDelVisorJobject.HasValues)
                    indicadores.Add(eventosDeMf.Comun_Tamano_Del_Visor, tamanoDelVisorJobject[ltrParametrosDeUsuarios.tamanoDelVisor].ToObject<int>());

                indicadores.Add(Variable.IA_Usada, CacheDeVariable.IA_Usada);

                var mostrarVisor = (JObject)negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Mostrar_El_Visor_Al_Iniciar);
                indicadores.Add(IndCrud.MostrarVisorAlIniciar, mostrarVisor.HasValues ? mostrarVisor[IndCrud.MostrarVisorAlIniciar].ToObject<bool>() : true);
            }
            else
            {
                indicadores.Add(eventosDeMf.Comun_Tamano_Del_Visor, 0);
                indicadores.Add(IndCrud.MostrarVisorAlIniciar, true);
            }
            return indicadores;
        }

        public JsonResult epTransitar(string parametrosJson)
        {
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var resultado = Transitar(parametros);
            return new JsonResult(resultado);
        }

        protected Resultado Transitar(Dictionary<string, object> parametros)
        {
            var r = new Resultado();
            try
            {
                var idNegocio = ApiController.BuscarNegocio(new List<ClausulaDeFiltrado>(), parametros);
                if (idNegocio == 0)
                {
                    GestorDeErrores.Emitir("Debe indicar el negocio del elemento a transitar");
                }
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epTransitar;
                var negocio = NegociosDeSe.ToEnumerado(idNegocio);
                var elemento = ObtenerElementoDto(parametros, negocio);
                Contexto.IniciarTraza(GetType().Name + "_" + nameof(epTransitar));
                var transicion = AntesDeTransitar(negocio, elemento, parametros);
                var transaccion = Contexto.IniciarTransaccion();
                try
                {
                    elemento = ApiParaElFlujo.Transitar(Contexto, NegociosDeSe.ToEnumerado(idNegocio), parametros);
                    elemento = DespuesDeTransitar(transicion, negocio, elemento, parametros);
                    r.Mensaje = $"transición ejecutada";
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                    r.Estado = enumEstadoPeticion.Ok;

                    Contexto.Commit(transaccion);
                }
                catch (Exception e)
                {
                    Contexto.AnotarExcepcion(e);
                    ErrorAlTransitar(Contexto, e, r, elemento);
                    ServicioDeCaches.EliminarTodas();
                    Contexto.Rollback(transaccion);
                }
                finally
                {
                    Contexto.CerrarTraza();
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al transitar.");
            }
            return r;
        }

        private ElementoDeUnProcesoDto ObtenerElementoDto(Dictionary<string, object> parametros, enumNegocio negocio)
        {
            var idElemento = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idElemento));
            var gestor = NegociosDeSe.CrearGestor(Contexto, negocio);
            var elemento = (ElementoDeUnProcesoDto)gestor.LeerElementoPorId(idElemento, new Dictionary<string, object>());
            return elemento;
        }

        protected virtual TransicionDtm AntesDeTransitar(enumNegocio negocio, ElementoDeUnProcesoDto elemento, Dictionary<string, object> parametros)
        {

            var idTransicion = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idTransicion, 0));
            var idEstadoDestino = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idEstadoDestino, 0));

            if (idTransicion == 0 && idEstadoDestino == 0)
            {
                GestorDeErrores.Emitir("Debe indicar la transición a realizar sobre el elemento o el estado destino al que devolver el elemento");
            }

            if (idEstadoDestino > 0)
            {
                parametros[ltrParametrosEp.asunto] = $"El usuario '{Contexto.Usuario.Login}' ha devuelto el elemento sin indicar el motivo";
                parametros[ltrParametrosEp.detalleAsunto] = $"El usuario '{Contexto.Usuario.Login}' ha devuelto el elemento sin indicar el motivo";
            }
            TransicionDtm transicion;
            if (idTransicion == 0)
            {
                transicion = GestorDeTransiciones.TransicionHasta(Contexto, negocio, elemento.IdEstado, idEstadoDestino, delSistema: false);
                parametros[ltrParametrosEp.idTransicion] = (long)transicion.Id;
            }
            else transicion = negocio.Transicion(Contexto, idTransicion);
            return transicion;
        }

        protected virtual void ObtenerIdDeTransicionDelIdEstadoDestino(enumNegocio negocio, IElementoDeUnProcesoDto elemento, Dictionary<string, object> parametros)
        {
            var idTransicion = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idTransicion, 0));
            if (idTransicion != 0)
                return;

            var idEstadoDestino = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idEstadoDestino, 0));

            if (idEstadoDestino == 0)
            {
                GestorDeErrores.Emitir("Debe indicar la transición a realizar sobre el elemento o el estado destino al que devolver el elemento");
            }
            if (idEstadoDestino > 0)
            {
                parametros[ltrParametrosEp.asunto] = $"El usuario '{Contexto.Usuario.Login}' ha devuelto el elemento sin indicar el motivo";
                parametros[ltrParametrosEp.detalleAsunto] = $"El usuario '{Contexto.Usuario.Login}' ha devuelto el elemento sin indicar el motivo";
            }
            TransicionDtm transicion;
            transicion = GestorDeTransiciones.TransicionHasta(Contexto, negocio, elemento.IdEstado, idEstadoDestino, delSistema: false);
            parametros[ltrParametrosEp.idTransicion] = (long)transicion.Id;
        }

        protected virtual ElementoDeUnProcesoDto DespuesDeTransitar(TransicionDtm transicion, enumNegocio negocio, ElementoDeUnProcesoDto elemento, Dictionary<string, object> parametros)
        {
            return (ElementoDeUnProcesoDto)negocio.LeerElemento(Contexto, elemento.Id, parametros: new Dictionary<string, object> { { ltrParametrosNeg.UsarLaCache, false } });
        }

        protected virtual void ErrorAlTransitar(ContextoSe contexto, Exception excepcion, Resultado resultado, ElementoDeUnProcesoDto elemento)
        {
            ApiController.PrepararError(excepcion, resultado, "Error al transitar.");
        }

        public JsonResult epImprimir(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            parametros[ltrParametrosNeg.Peticion] = enumPeticion.epImprimir;
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var idNegocio = ApiController.BuscarNegocio(new List<ClausulaDeFiltrado>(), parametros);
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epImprimir));
            try
            {
                var negocio = idNegocio > 0 ? NegociosDeSe.ToEnumerado(idNegocio) : enumNegocio.No_Definido;
                var impreso = Imprimir(idNegocio, parametros);

                if (!impreso)
                    GestorDeErrores.Emitir($"No se ha encontrado la plantilla '{parametros.LeerValor<string>(ltrParametrosEp.Plantilla)}' para procesar la solicitud");

                r.Mensaje = $"Impresión realizada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ErrorAlImprimir(Contexto, e, r, parametros);
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        protected virtual bool Imprimir(int idNegocio, Dictionary<string, object> parametros)
        {
            var negocio = NegociosDeSe.ToEnumerado(idNegocio);
            var idPlantilla = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.IdPlantilla));
            var idElemento = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idElemento));
            enumClaseDePlantilla clase = parametros.LeerValor<enumClaseDePlantilla>(nameof(IPlantillaPlt.Clase));

            if (negocio == enumNegocio.No_Definido || clase == enumClaseDePlantilla.programada)
                return false;

            var fichero = new ServicioDeImpresion(Contexto, negocio, idElemento, idPlantilla).Imprimir(clase);
            if (fichero is null) return false;

            ServidorDocumental.AnexarArchivo(Contexto, negocio, idElemento, fichero, sanitizar: false);
            return true;
        }

        protected virtual void ErrorAlImprimir(ContextoSe contexto, Exception excepcion, Resultado resultado, Dictionary<string, object> parametros)
        {
            ApiController.PrepararError(excepcion, resultado, "Error al imprimir.");
        }

        [HttpPost]
        public JsonResult epEnviarPorCorreo(string peticion)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epEnviarPorCorreo));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var body = ApiController.LeerBody(HttpContext);
                body.parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                GestorDeCorreos.CrearCorreoDe(Contexto, body.parametros);

                r.Mensaje = $"Correo enviado";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al enviar el correo.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epDarDeAlta(int id, string parametrosJson) => AplicarBaja(id, parametrosJson, baja: false);

        public JsonResult epDarDeBaja(int id, string parametrosJson) => AplicarBaja(id, parametrosJson, baja: true);

        protected virtual JsonResult AplicarBaja(int id, string parametrosJson, bool baja)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
                parametros[ltrParametrosNeg.Peticion] = baja ? enumPeticion.epDarDeBaja : enumPeticion.epDarDeAlta;
                var negocio = ObtenerNegocio(parametros);

                var gestor = NegociosDeSe.CrearGestor(Contexto, negocio);
                var elemento = (ElementoDto)gestor.LeerElementoPorId(id, null);

                if (((IUsaBajaDto)elemento).Baja == baja)
                {
                    GestorDeErrores.Emitir("El elemento ya ha sido modificado por otro usuario");
                }
                ((IUsaBajaDto)elemento).Baja = baja;
                gestor.PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros: parametros));

                r.Datos = elemento;
                r.ModoDeAcceso = elemento.ModoDeAcceso.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer.");
            }
            return new JsonResult(r);
        }

        protected enumNegocio ObtenerNegocio(Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(NegocioPor.idNegocio))
            {
                GestorDeErrores.Emitir("Debe indicar el negocio para realizar la operación solicitada");
            }

            var id32 = Convert.ToInt32(parametros.LeerValor(NegocioPor.idNegocio, 0));
            return NegociosDeSe.ToEnumerado(id32);
        }



        [AllowAnonymous]
        public JsonResult epLeerPorIdPorGuid(int id, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(nameof(epLeerPorIdPorGuid));
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                ValidarConsultaPorGuid(id, parametrosJson);
                return epLeerPorId(id, parametrosJson);
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al validar el Guid para consulta.");
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerPorId(int id, string parametrosJson = null)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerPorId));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext, extJson.ToDiccionarioDeParametros(parametrosJson));
                var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
                parametros[ltrParametrosDto.DescargarGestionDocumental] = true;
                parametros[ltrParametrosNeg.UsarLaCache] = CacheDeVariable.CFG_Usar_Cache_En_EpLeerPorId.EsTrue();
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epLeerPorId;

                var elemento = LeerPorId(id, parametros);

                if (elemento.ModoDeAcceso.Equals(enumModoDeAccesoDeDatos.SinPermiso))
                {
                    GestorDeErrores.Emitir("El usuario conectado no tiene acceso al elemento solicitado");
                }

                r.Datos = elemento;
                r.ModoDeAcceso = elemento.ModoDeAcceso.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [AllowAnonymous]
        public JsonResult epLeerElementosPorGuid(string filtrosJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(nameof(epLeerElementosPorGuid));
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var parametros = extJson.ToDiccionarioDeParametros(parametrosJson);
                var guid = parametros.LeerValor<string>(ltrParametrosEp.guid);
                var id = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
                var filtroPorNegocio = filtros.FirstOrDefault(c => c.Clausula.ToLower() == ltrParametrosEp.idNegocio.ToLower());
                var negocio = filtroPorNegocio != null ? NegociosDeSe.ToEnumerado(filtroPorNegocio.Valor.Entero()) : NegociosDeSe.ToEnumerado(NegociosDeSe.ToDtm(typeof(TElemento)));

                ValidarConsultaPorGuid(negocio, id, guid);
                return epLeerElementos(filtrosJson, parametrosJson);
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al validar el Guid para consulta.");
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerElementos(string filtrosJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerElementos));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext, extJson.ToDiccionarioDeParametros(parametrosJson));
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epLeerElementos;
                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
                if (!parametros.ContainsKey(ltrParametrosEp.Cantidad))
                {
                    GestorDeErrores.Emitir("Debe indicar la cantidad de elementos a leer");
                }

                if (!parametros.ContainsKey(ltrParametrosEp.ObtenerSeguridad))
                {
                    GestorDeErrores.Emitir("Debe indicar la si se obtiene la seguridad por elemento devuelto");
                }

                var orden = DefinirOrdenacion(parametros);
                //cache[parametros[ltrParametrosEp.guid].ToString()] 

                var posicion = parametros.LeerValor<int>(ltrParametrosEp.Posicion, 0);
                var elementos = LeerElementos(posicion, Convert.ToInt32(parametros[ltrParametrosEp.cantidad]), filtros, orden, parametros);
                //}
                r.Datos = elementos; // cache[parametros[ltrParametrosEp.guid].ToString()];
                r.Total = elementos.Count(); // ( (List<TElemento>) cache[parametros[ltrParametrosEp.guid].ToString()]).Count();
                r.Consola = $"se han leido {r.Datos.Count} elementos";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith(ApiDeDetalles.MensajeDeNoUsaDetalle))
                {
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
                    r.Mensaje = e.Message;
                    r.Datos = null;
                }
                else
                {
                    ApiController.PrepararError(e, r, "Error al leer.");
                }
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        [HttpPost]
        public JsonResult epProcesarPeticion(int idNegocio, int idVista, string peticion)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epProcesarPeticion));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var body = ApiController.LeerBody(HttpContext);
                var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
                var vista = idVista == 0 ? null : Contexto.SeleccionarPorId<VistaMvcDtm>(idVista);
                r.Datos = ProcesarPeticion(negocio, vista, peticion, body.parametros);
                r.Consola = $"Petición '{peticion}' procesada correctamente";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al procesar la petición '{peticion}'.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epProcesarOpcionMf(int idNegocio, string opcionMf, bool esContextual, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epProcesarOpcionMf));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epProcesarOpcionMf;
                var negocio = ValidarPararametrosMf(idNegocio, opcionMf, esContextual, parametros);
                var resultado = ProcesarOpcionMf(negocio, opcionMf, parametros);
                if (resultado is not null && resultado.GetType() == typeof(Resultado))
                {
                    r = resultado;
                }
                else
                {
                    r.Datos = resultado;
                    r.Consola = $"Opción '{opcionMf}' procesada correctamente";
                    r.Estado = enumEstadoPeticion.Ok;
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al procesar la opción {opcionMf}.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epVincular(int idNegocio, int idVinculado, int idElemento1, string elementoJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epVincular));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                var elemento = JsonConvert.DeserializeObject<SelectorDto>(elementoJson);
                AntesDeEjecutar_ValidarNegocios(idNegocio, idVinculado);
                int vinculado = Vincular(idNegocio, idVinculado, idElemento1, elemento, parametros);

                r.Datos = vinculado;
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"vínculo entre {NegociosDeSe.ToEnumerado(idNegocio)} y {NegociosDeSe.ToEnumerado(idVinculado)} creado";
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al crear el vínculo.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        protected virtual int Vincular(int idNegocio, int idVinculado, int idElemento1, SelectorDto elemento2, Dictionary<string, object> parametros)
        {
            return GestorDeVinculos.Vincular(Contexto
                , NegociosDeSe.ToEnumerado(idNegocio)
                , NegociosDeSe.ToEnumerado(idVinculado)
                , idElemento1
                , elemento2.IdElemento
                , parametros);
        }


        public JsonResult epLeerVinculosConElNegocio(int idNegocio, string enumerado, int idElemento, string parametrosJson)
        {
            var idEnumerado = ApiDeEnsamblados.ToEnumerado<enumNegocio>(enumerado).IdNegocio();
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerVinculosCon));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                var vinculado = NegociosDeSe.ToEnumerado(idEnumerado);
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                if (vinculado == enumNegocio.Archivador)
                {
                    parametros.Add(ltrParametrosNeg.IncluirDetalles, true);
                }
                var negocio = NegociosDeSe.ToEnumerado(idNegocio);
                var registro = negocio.RegistroPorId(Contexto, idElemento);
                var vinculados = registro.Elementos(Contexto, vinculado.TipoDto(), parametros);
                r.Datos = vinculados;
                r.Total = vinculados.Count();
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"vínculos entre {negocio} y {vinculado} para el id {idElemento} leidos";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [AllowAnonymous]
        public JsonResult epLeerVinculosConPorGuid(int idNegocio,int idVinculado, int idElemento1, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(nameof(epLeerVinculosConPorGuid));
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var parametros = extJson.ToDiccionarioDeParametros(parametrosJson);
                var guid = parametros.LeerValor<string>(ltrParametrosEp.guid);
                var id = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var negocio = NegociosDeSe.ToEnumerado(NegociosDeSe.ToDtm(typeof(TElemento)));

                ValidarConsultaPorGuid(negocio, id, guid);
                return epLeerVinculosCon(idNegocio,idVinculado ,idElemento1, parametrosJson);
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al validar el Guid para consulta.");
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerVinculosCon(int idNegocio, int idVinculado, int idElemento1, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerVinculosCon));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                var orden = new List<ClausulaDeOrdenacion>();
                AntesDeEjecutar_ValidarNegocios(idNegocio, idVinculado);
                var vinculado = NegociosDeSe.ToEnumerado(idVinculado);
                if (vinculado == enumNegocio.Archivador)
                {
                    parametros.Add(ltrParametrosNeg.IncluirDetalles, true);
                }

                var vinculados = GestorDeVinculos.ElementosVinculados(Contexto, NegociosDeSe.ToEnumerado(idNegocio), vinculado, idElemento1, parametros);
                r.Datos = vinculados;
                r.Total = vinculados.Count;
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"vínculos entre {NegociosDeSe.ToEnumerado(idNegocio)} y {NegociosDeSe.ToEnumerado(idVinculado)} para el id {idElemento1} leidos";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epBorrarVinculo(int idNegocio, int idVinculado, int idElemento1, int idElemento2, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epBorrarVinculo));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epBorrarVinculo;
                AntesDeEjecutar_ValidarNegocios(idNegocio, idVinculado);
                parametros[ltrParametrosNeg.ValidarPermisosDePersistencia] = true;
                GestorDeVinculos.BorrarVinculo(Contexto
                , NegociosDeSe.ToEnumerado(idNegocio)
                , NegociosDeSe.ToEnumerado(idVinculado)
                , idElemento1
                , idElemento2
                , parametros);

                r.Datos = null;
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"vínculo entre {NegociosDeSe.ToEnumerado(idNegocio)} y {NegociosDeSe.ToEnumerado(idVinculado)} borrado";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al borrar el vínculo.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epCrearVinculo(int idNegocio, int idVinculado, int idElemento1, string elementoJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCrearVinculo));
            var t = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                var elemento = JsonConvert.DeserializeObject<TElemento>(elementoJson);
                AntesDeEjecutar_ValidarNegocios(idNegocio, idVinculado);
                parametros.Add(nameof(ltrParametrosNeg.IdNegocio), idNegocio);
                parametros.Add(nameof(ltrParametrosNeg.IdElemento), idElemento1);
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epCrearVinculo;
                var vinculado = GestorDeVinculos.CrearVinculo(Contexto
                    , NegociosDeSe.ToEnumerado(idNegocio)
                    , NegociosDeSe.ToEnumerado(idVinculado)
                    , idElemento1
                    , elemento
                    , parametros);

                r.Datos = vinculado;
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"vínculo entre {NegociosDeSe.ToEnumerado(idNegocio)} y {NegociosDeSe.ToEnumerado(idVinculado)} creado";
                Contexto.Commit(t);
            }
            catch (Exception e)
            {
                Contexto.Rollback(t);
                ApiController.PrepararError(e, r, "Error al crear el vínculo.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epCrearDetalle(int idNegocio, string elementoJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCrearDetalle));
            var t = Contexto.IniciarTransaccion();
            try
            {
                if (idNegocio < 1) GestorDeErrores.Emitir("Debe indicar el negocio");
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros.Add(nameof(ltrParametrosNeg.IdNegocio), idNegocio);
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epCrearDetalle;
                var detalle = CrearDetalle(Contexto, NegociosDeSe.ToEnumerado(idNegocio), elementoJson, parametros);
                r.Datos = detalle;
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"Detalle creado";
                Contexto.Commit(t);
            }
            catch (Exception e)
            {
                Contexto.Rollback(t);
                ApiController.PrepararError(e, r, "Error al crear el detalle.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        protected virtual IDetalleDto CrearDetalle(ContextoSe contexto, enumNegocio enumNegocio, string elemento, Dictionary<string, object> parametros)
        {
            throw new Exception("Debe indicar cómo crear el detalle");
        }

        private void AntesDeEjecutar_ValidarNegocios(int idNegocio, int idVinculado)
        {
            if (idNegocio < 1)
            {
                GestorDeErrores.Emitir("Debe indicar el negocio del que se quieren los vínculos a leer");
            }

            if (idVinculado < 1)
            {
                GestorDeErrores.Emitir("Debe indicar el negocio del los elementos vinculados a leer");
            }
        }

        protected virtual enumNegocio ValidarPararametrosMf(int idNegocio, string opcion, bool esContextual, Dictionary<string, object> parametros)
        {

            if (opcion == eventosDeMf.Comun_GuardarDatosCreacion ||
                opcion == eventosDeMf.Comun_GuardarPlantillaCreacion ||
                opcion == eventosDeMf.Comun_EliminarPlantillaCreacion ||
                opcion == eventosDeMf.Comun_GuardarPlantillaFiltrado ||
                opcion == eventosDeMf.Comun_EliminarPlantillaFiltrado ||
                opcion == eventosDeMf.Comun_OcultarColumnas)
            {
                return idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            }

            var listaIds = new List<int>();
            if (!esContextual)
            {
                if (!parametros.ContainsKey(ltrParametrosEp.ids))
                {
                    GestorDeErrores.Emitir("Debe indicar los ids a procesar");
                }

                listaIds.AddRange(((JArray)parametros[ltrParametrosEp.ids]).Select(jObject => (int)jObject));

                if (listaIds.Count == 0)
                {
                    GestorDeErrores.Emitir($"Debe indicar algún elemento que procesar para la opción {opcion}");
                }
            }

            if (esContextual && parametros.ContainsKey(ltrParametrosEp.ids))
            {
                listaIds.AddRange(((JArray)parametros[ltrParametrosEp.ids]).Select(jObject => (int)jObject));
            }

            parametros[ltrParametrosEp.ids] = listaIds;

            return idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
        }

        protected virtual ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(TElemento elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Modificar);
        }

        private static List<ClausulaDeOrdenacion> DefinirOrdenacion(Dictionary<string, object> parametros)
        {
            var orden = new List<ClausulaDeOrdenacion>();
            if (!((string)parametros.LeerValor(ltrParametrosEp.ordenarPor, "")).IsNullOrEmpty())
            {
                var ordenes = parametros[ltrParametrosEp.ordenarPor].ToString().Split(Simbolos.PuntoComa);
                foreach (var o in ordenes)
                {
                    IncluirOrden(o.Split(":"), orden);
                }
            }
            else
            {
                var negocio = parametros.LeerValor(ltrParametrosEp.negocio, "");
                var enumNegocio = NegociosDeSe.ToEnumerado(negocio);
                if (enumNegocio != enumNegocio.No_Definido)
                {
                    orden.Add(new ClausulaDeOrdenacion { Modo = ModoDeOrdenancion.ascendente, OrdenarPor = nameof(IElementoDtm.Nombre) });
                }
                else
                    if (ApiDeInterfaceDto.ImplementaNombreDto(typeof(TElemento)))
                    {
                        orden.Add(new ClausulaDeOrdenacion { Modo = ModoDeOrdenancion.ascendente, OrdenarPor = nameof(IElementoDtm.Nombre) });
                    }
            }

            return orden;

            static void IncluirOrden(string[] partes, List<ClausulaDeOrdenacion> orden)
            {
                if (partes.Length == 3)
                {
                    orden.Add(new ClausulaDeOrdenacion(partes[1], (ModoDeOrdenancion)Enum.Parse(typeof(ModoDeOrdenancion), partes[2])));
                }
                else
                    if (partes.Length == 2)
                    {
                        orden.Add(new ClausulaDeOrdenacion(partes[0], (ModoDeOrdenancion)Enum.Parse(typeof(ModoDeOrdenancion), partes[1])));
                    }
                    else
                    {
                        orden.Add(new ClausulaDeOrdenacion { Modo = ModoDeOrdenancion.ascendente, OrdenarPor = partes[0] });
                    }
            }
        }

        protected virtual dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Comun_PermisosDeElemento:
                    return GestorDePemisosDelElemento.CrearPermisos(Contexto, negocio, (List<int>)parametros[ltrParametrosEp.ids]);
                case eventosDeMf.Comun_Imprimir:
                    return new ServicioDePlantillas(Contexto, negocio, (List<int>)parametros[ltrParametrosEp.ids]).Plantillas();
                case eventosDeMf.Comun_OcultarColumnas:
                    return null;

            }
            throw new Exception($"No se ha definido como procesar la opcion {opcion} para el negocio de {NegociosDeSe.ToNombre(negocio)}");
        }

        protected virtual dynamic ProcesarPeticion(enumNegocio negocio, VistaMvcDtm vista, string peticion, Dictionary<string, object> parametros)
        {
            switch (peticion)
            {
                case eventosDeMf.Comun_GuardarEstadosDeExpansores:
                    var espansores = parametros.LeerValor<List<EstadoDeEspan>>(ltrParametrosEp.datosPeticion);
                    JObject estados = JObject.FromObject(new { estados = espansores });
                    if (negocio == enumNegocio.No_Definido)
                        vista.ResetearParametroDeVistaPorUsuario(Contexto, enumParametrosDeUsuario.USU_Vista_De_Edicion, estados.ToString());
                    else
                        negocio.ResetearParametroDeUsuario(Contexto, enumParametrosDeUsuario.USU_Vista_De_Edicion, estados.ToString());
                    return null;
                case eventosDeMf.Comun_Tamano_Del_Encolumnado:
                    if (negocio == enumNegocio.No_Definido) vista.GuardarElTamanoDeColumnasDeVista(Contexto, parametros);
                    else negocio.Guardar_USU_Tamano_Del_Encolumnado(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_Cantidad_A_Leer:
                    if (negocio == enumNegocio.No_Definido)
                        vista.Guardar_USU_Cantidad_A_Leer(Contexto, parametros);
                    else
                        negocio.Guardar_USU_Cantidad_A_Leer(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_Tamano_Del_Visor:
                    negocio.Guardar_USU_Tamano_Del_Visor(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_Mostrar_Visor_Al_iniciar:
                    negocio.Guardar_USU_Mostrar_Visor_Al_Iniciar(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_Disposicion_Del_Encolumnado:
                    negocio.Guardar_USU_Disposicion_Del_Encolumnado(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_Ordenacion_Del_Resultado:
                    negocio.Guardar_USU_Ordenacion_Del_Resultado(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_GuardarColumnasDelGrid:
                    negocio.Guardar_USU_Colunas_Del_Grid(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_EliminarColumnasDelGrid:
                    negocio.EliminarParametroDeUsuario(Contexto, enumParametrosDeUsuario.USU_Colunas_Del_Grid);
                    negocio.EliminarParametroDeUsuario(Contexto, enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado);
                    ServicioDeCaches.EliminarElementos(CacheDe.RenderCrud, $"{Contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}");
                    return null;
                case eventosDeMf.Comun_GuardarDisposicionDeArchivos:
                    vista.GuardarDisposicionDeArchivos(Contexto, parametros);
                    return null;
                case eventosDeMf.Comun_LeerDisposicionDeArchivos:
                    return vista.LeerDisposicionDeArchivos(Contexto, parametros);
                case ltrParametrosNeg.ModificoParaTransitar:
                    TransitarTrasModificar(negocio, parametros);
                    return null;
            }
            throw new Exception($"No se ha definido como procesar la peticion {peticion} para el negocio de {NegociosDeSe.ToNombre(negocio)}");
        }

        private void TransitarTrasModificar(enumNegocio negocio, Dictionary<string, object> parametros)
        {
            var tran = Contexto.IniciarTransaccion();
            try
            {
                Type tipoDto = negocio.TipoDto();
                JObject datos = parametros.LeerValor<JObject>(ltrParametrosEp.DatosPrincipales);
                object objetoDto = datos.ToObject(tipoDto);
                var gestor = negocio.CrearGestor(Contexto);
                var elemento = gestor.PersistirElementoDto(objetoDto, new ParametrosDeNegocio(enumTipoOperacion.Modificar, new Dictionary<string, object>
                {
                    {ltrParametrosNeg.Peticion,enumPeticion.epModificarPorId },
                    {ltrParametrosNeg.EsUnaTransicion, true }
                }));

                ObtenerIdDeTransicionDelIdEstadoDestino(negocio, (IElementoDeUnProcesoDto)elemento, parametros);
                var transicionPendiente = negocio.Transicion(Contexto, Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idTransicion)));
                var origen = negocio.Estado(Contexto, transicionPendiente.IdOrigen);
                if (!origen.Cancelado && !origen.Terminado)
                {
                    foreach (var key in parametros.Keys.Where(x => x.Split(".").Length == 3))
                    {
                        var tipoAmpliacionDto = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelModeloDeDto, key);
                        datos = parametros.LeerValor<JObject>(key);
                        var ampliacionDto = datos.ToObject(tipoAmpliacionDto);
                        gestor = NegociosDeSe.CrearGestor(Contexto, tipoAmpliacionDto.ToDtm(), tipoAmpliacionDto);
                        var ampliacionDtm = gestor.LeerRegistro(nameof(IAmpliacionDto.IdElemento), ((IAmpliacionDto)ampliacionDto).IdElemento.ToString(), errorSiNoHay: false);
                        if (ampliacionDtm is not null)
                        {
                            ((IElementoDto)ampliacionDto).Id = ((IRegistro)ampliacionDtm).Id;
                        }
                        gestor.PersistirElementoDto(ampliacionDto, new ParametrosDeNegocio(((IElementoDto)ampliacionDto).Id == 0 ?
                            enumTipoOperacion.Insertar :
                            enumTipoOperacion.Modificar, new Dictionary<string, object>
                            {
                                {ltrParametrosNeg.Peticion,enumPeticion.epPersistirAmpliacion },
                                {ltrParametrosNeg.TransionPendienteDeEjecucion,transicionPendiente },
                                {ltrParametrosNeg.ValidarPermisosDePersistencia, false }
                            }));
                    }
                }

                var resultado = Transitar(parametros);
                if (resultado is not null && resultado.Estado == enumEstadoPeticion.Error)
                {
                    ServicioDeCaches.EliminarTodas();
                    GestorDeErrores.Emitir(resultado.Mensaje, resultado.Consola);
                }
                Contexto.Commit(tran);
            }
            catch
            {
                Contexto.Rollback(tran);
                throw;
            }
        }

        protected virtual IEnumerable<ElementoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            throw new Exception($"Debe implementar el método de {nameof(LeerElementos)}");
        }

        protected virtual TElemento LeerPorId(int id, Dictionary<string, object> parametros)
        {
            return null;
        }

        private static void ValidarExtension(IFormFile fichero, string extensiones)
        {
            if (extensiones.IsNullOrEmpty() || extensiones.EndsWith("*"))
            {
                return;
            }

            if (fichero.EsImagen() && (extensiones.Contains(enumExtensiones.png.ToString())
                                   || extensiones.Contains(enumExtensiones.jpg.ToString())
                                   || extensiones.Contains(enumExtensiones.svg.ToString())
                                   ))
            {
                return;
            }

            if (fichero.EsCertificado() && (extensiones.Contains(enumExtensiones.cer.ToString())
                                   || extensiones.Contains(enumExtensiones.pfx.ToString())
                                   || extensiones.Contains(enumExtensiones.p12.ToString())
                                   ))
            {
                return;
            }

            if (fichero.EsCsv() && extensiones.Contains(enumExtensiones.csv.ToString()))
            {
                return;
            }

            if (fichero.EsPdf() && extensiones.Contains(enumExtensiones.pdf.ToString()))
                return;

            if (fichero.EsDocx() && extensiones.Contains(enumExtensiones.docx.ToString()))
                return;

            if (fichero.EsXml() && extensiones.Contains(enumExtensiones.xml.ToString()))
                return;

            if (fichero.EsZip() && extensiones.Contains(enumExtensiones.zip.ToString()))
                return;

            throw new Exception($"Para el tipo de fichero {fichero.ContentType} sólo se aceptan '{extensiones}'");

        }

        public IActionResult VistaDelPanelDeControl(ContextoSe contexto)
        {
            ViewBag.DatosDeConexion = Contexto.DatosDeConexion;
            var descriptor = new PanelDeControl(contexto, "layout-Se");

            return View("../Acceso/PanelDeControl", descriptor);
        }

        public IActionResult VistaDelPanelDeControlConCuerpoHtml(ContextoSe contexto, string cuerpoHtml)
        {
            ViewBag.DatosDeConexion = Contexto.DatosDeConexion;
            var descriptor = new PanelDeControl(contexto, "layout-Se", cuerpoHtml);

            return View("../Acceso/PanelDeControl", descriptor);
        }

        public IActionResult VistaGraficoDeUnProceso(ContextoSe contexto)
        {
            ViewBag.DatosDeConexion = Contexto.DatosDeConexion;
            var descriptor = new GraficoDeUnProceso(contexto, "layout-Se");

            return View("../Negocio/GraficoDeUnProceso", descriptor);
        }


        protected dynamic DatosParaInicializarLaCreacion(enumNegocio negocio, Dictionary<string, object> parametros, DatosDeCreacion datosDeCreacion)
        {
            if (negocio == enumNegocio.No_Definido)
                return null;

            var datosPropuestos = new DatosPropuestos();
            if (negocio.UsaCg())
            {
                datosPropuestos.CGsAccesibles = CGParaCrear(negocio);
                datosPropuestos.CGPropuesto = datosPropuestos.CGsAccesibles.Count == 1
                    ? datosPropuestos.CGsAccesibles[0]
                    : datosPropuestos.CGsAccesibles.Count == 0
                    ? null
                    : datosDeCreacion is not null && datosDeCreacion.IdCg is not null && datosPropuestos.CGsAccesibles.Exists(x => x.Id == datosDeCreacion.IdCg.Entero())
                    ? datosPropuestos.CGsAccesibles.First(x => x.Id == datosDeCreacion.IdCg.Entero())
                    : null;
            }

            if (negocio.UsaTipo())
            {
                datosPropuestos.TiposAccesibles = ApiDeTipos.TiposConPermisoDeGestor(Contexto, negocio, parametros);
                datosPropuestos.TipoPropuesto = datosPropuestos.TiposAccesibles.Count == 1
                    ? datosPropuestos.TiposAccesibles[0]
                    : datosPropuestos.TiposAccesibles.Count == 0
                    ? null
                    : datosDeCreacion is not null && datosDeCreacion.IdTipo is not null && datosPropuestos.TiposAccesibles.Exists(x => x.Id == datosDeCreacion.IdTipo.Entero())
                    ? datosPropuestos.TiposAccesibles.First(x => x.Id == datosDeCreacion.IdTipo.Entero())
                    : null;
            }

            datosPropuestos.Nombre = datosDeCreacion.Nombre;
            datosPropuestos.Descripcion = datosDeCreacion.Descripcion;
            datosPropuestos.Otros = datosDeCreacion.Otros;

            datosPropuestos.Plantillas = Contexto.SeleccionarDtos<PlantillaDeCreacionDto, PlantillaDeCreacionDtm>(new Dictionary<string, object>
            {
                { nameof(PlantillaDeCreacionDtm.IdNegocio), negocio.IdNegocio()},
                { nameof(PlantillaDeCreacionDtm.IdUsuario),Contexto.DatosDeConexion.IdUsuario},
                { nameof(PlantillaDeCreacionDtm.Vista),parametros.LeerValor<string>(nameof(PlantillaDeCreacionDtm.Vista))}
            });

            return datosPropuestos;
        }

        private List<CentroGestorDtm> CGParaCrear(enumNegocio negocio)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_CgsConGestion);
            var indice = $"{negocio.IdNegocio()}-{Contexto.DatosDeConexion.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = Contexto.SeleccionarTodos<CentroGestorDtm>(new Dictionary<string, object> { { nameof(NegociosDeUnCgDtm.Negocio), $"{negocio}{Simbolos.PuntoComa}{enumModoDeAccesoDeDatos.Gestor}" } });
            }
            return (List<CentroGestorDtm>)cache[indice];
        }

        protected FileStreamResult DescargarArchivo(int idArchivo, bool auditarDescarga, bool errorSiNoEsta = false)
        {
            var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            if (auditarDescarga) archivo.AuditarDescarga(Contexto);
            var pendienteDeBorrar = new Ficheros(HttpContext);
            var ruta = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: true);

            if (ApiDeArchivos.FicheroNoEncontrado != ruta && ApiDeArchivos.FicheroBloqueado != ruta)
                pendienteDeBorrar.Add(ruta);
            else
            {
                if (errorSiNoEsta)
                    GestorDeErrores.Emitir($"El archivo solicitado {(ApiDeArchivos.FicheroNoEncontrado == ruta ? "ya no se encuentra en el sistema" : "está bloqueado")}");
                return DevolverStream(ruta, MimeTypeMap.GetMimeType(Path.GetExtension(ruta)), Path.GetFileName(ruta));
            }

            return DevolverStream(ruta, MimeTypeMap.GetMimeType(Path.GetExtension(archivo.Nombre)), archivo.Nombre);
        }

        protected FileStreamResult DevolverStream(string ruta) => DevolverStream(ruta, MimeTypeMap.GetMimeType(Path.GetExtension(ruta)), Path.GetFileName
            (ruta));

        protected FileStreamResult DevolverStream(string ruta, string mimeType, string nombreFichero)
        {
            try
            {
                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    Inline = true,
                    FileName = nombreFichero.Replace("\r", "")
                };

                try
                {
                    Response.Headers.Append("Content-Disposition", contentDisposition.ToString());
                }
                catch (Exception exc)
                {
                    Contexto.AnotarExcepcion(exc);
                    Response.Headers.Append("Content-Disposition", "");
                }


                // You don't need to dispose the stream. It will be disposed by the FileStreamResult.WriteFile method
                // Fuente: "https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net"
                return File(new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), mimeType);
            }
            catch (Exception ex)
            {
                Contexto.IniciarTraza(nameof(DevolverStream), debugar: true);
                Contexto.AnotarExcepcion(ex);
                throw;
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }

        private const string HtmlErrorTemplate = @"<div class='mensaje-en-panel-de-control'><h2>{0}</h2>{1}</div>";

        protected ContentResult DevolverPaginaWebConMensaje(string mensaje, string masInformacionEnHtml = null)
        {
            var info = string.IsNullOrEmpty(masInformacionEnHtml) ? "" : $"<div class='additional-info'>{masInformacionEnHtml}</div>";

            var htmlErrorTemplate = @"
<html>
  <head>
    <meta charset='UTF-8'>
    <title>Error</title>
    <style>
      .mensaje-en-panel-de-control {{
        display: flex;
        justify-content: flex-start;
        align-items: flex-start;
        height: 100vh;
        padding: 60px 20px 20px 20px;
        box-sizing: border-box;
        font-family: Arial, sans-serif;
        background-color: #ffffff;
        color: #721c24;
        text-align: center;
        flex-direction: column;
      }}
      .mensaje-en-panel-de-control h2 {{
        margin-bottom: 1rem;
      }}
      .mensaje-en-panel-de-control .additional-info {{
        margin-top: 1rem;
        color: #495057;
        font-size: 1rem;
        text-align: left;
      }}
    </style>
  </head>
  <body>
    <div class='mensaje-en-panel-de-control'>
      <h2>{0}</h2>
      {1}
    </div>
  </body>
</html>";
            var htmlError = string.Format(htmlErrorTemplate, mensaje, info);
            return Content(htmlError, "text/html", System.Text.Encoding.UTF8);
        }

        protected IActionResult DevolverPanelDeControlConMensaje(string mensaje, string masInformacionEnHtml = null)
        {

            var info = string.IsNullOrEmpty(masInformacionEnHtml) ? "" : $"<div class='additional-info'>{masInformacionEnHtml}</div>";
            var htmlError = string.Format(HtmlErrorTemplate, mensaje, info);

            return VistaDelPanelDeControlConCuerpoHtml(Contexto, htmlError);
        }

        protected void ValidarConsultaPorGuid(int id, string parametrosJson)
        {
            ValidarConsultaPorGuid(id, extJson.ToDiccionarioDeParametros(parametrosJson));
        }

        private void ValidarConsultaPorGuid(int id, Dictionary<string, object> parametros)
        {
            var guid = parametros.LeerValor<string>(ltrParametrosEp.guid);
            var idElemento = (int)parametros.LeerValor<long>(ltrParametrosEp.id, (long)0);
            var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio, (long)0);
            var enumNegocio = idNegocio == 0 ? NegociosDeSe.NegocioDeUnDto(typeof(TElemento)) : NegociosDeSe.ToEnumerado(idNegocio);
            ValidarConsultaPorGuid(enumNegocio, idElemento == 0 ? id : idElemento, guid);
        }

        protected void ValidarConsultaPorGuid(enumNegocio negocio, int id, string guid)
        {
            GestorDeConsultasConGuid.ValidarGuid(contexto: Contexto, negocio, id, guid);
        }

        public ViewResult ViewCrud<T>(DescriptorDeCrud<T> descriptor)
        where T : ElementoDto
        {
            try
            {
                var destino = ApiController.PrepararDescriptor(this, ControllerContext, descriptor, Contexto, HttpContext);
                ViewBag.DatosDeConexion = DatosDeConexion;
                return base.View(destino, descriptor);
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        public ViewResult ViewPagina(DescriptorDePaginaDeConsulta pagina)
        {
            try
            {
                var destino = ApiController.PrepararPagina(this, ControllerContext, pagina);
                ViewBag.DatosDeConexion = DatosDeConexion;
                return base.View(destino, pagina);
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
