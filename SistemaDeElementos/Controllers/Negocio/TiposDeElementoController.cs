using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class TiposDeElementoController : JerarquiaController<ContextoSe>
    {

        public TiposDeElementoController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
         : base(contexto
               , mapeador
               , gestorDeErrores)
        {
        }

        public IActionResult TiposDeArchivador() => TiposDeElemento(enumNegocio.Archivador, nameof(TiposDeArchivador));
        public IActionResult TiposDeCircuitoDoc() => TiposDeElemento(enumNegocio.CircuitoDoc, nameof(TiposDeCircuitoDoc));
        public IActionResult TiposDePreasiento() => TiposDeElemento(enumNegocio.Preasiento, nameof(TiposDePreasiento));
        public IActionResult TiposDeTarea() => TiposDeElemento(enumNegocio.Tarea, nameof(TiposDeTarea));
        public IActionResult TiposDeRegistroEs() => TiposDeElemento(enumNegocio.Registro, nameof(TiposDeRegistroEs));
        public IActionResult TiposDeExpediente() => TiposDeElemento(enumNegocio.Expediente, nameof(TiposDeExpediente));
        public IActionResult TiposDePleito() => TiposDeElemento(enumNegocio.Pleito, nameof(TiposDePleito));
        public IActionResult TiposDeContrato() => TiposDeElemento(enumNegocio.Contrato, nameof(TiposDeContrato));
        public IActionResult TiposDePresupuesto() => TiposDeElemento(enumNegocio.Presupuesto, nameof(TiposDePresupuesto));
        public IActionResult TiposDeFacturaEmt() => TiposDeElemento(enumNegocio.FacturaEmitida, nameof(TiposDeFacturaEmt));
        public IActionResult TiposDeParteTr() => TiposDeElemento(enumNegocio.ParteDeTrabajo, nameof(TiposDeParteTr));
        public IActionResult TiposDePlanificaciondeVenta() => TiposDeElemento(enumNegocio.PlanificacionDeVenta, nameof(TiposDePlanificaciondeVenta));
        public IActionResult TiposDeRemesaFae() => TiposDeElemento(enumNegocio.RemesaFae, nameof(TiposDeRemesaFae));
        public IActionResult TiposDePago() => TiposDeElemento(enumNegocio.Pago, nameof(TiposDePago));
        public IActionResult TiposDeRemesaPag() => TiposDeElemento(enumNegocio.RemesaPag, nameof(TiposDeRemesaPag));
        public IActionResult TiposDeFacturaRec() => TiposDeElemento(enumNegocio.FacturaRecibida, nameof(TiposDeFacturaRec));
        public IActionResult TiposDePedido() => TiposDeElemento(enumNegocio.Pedido, nameof(TiposDePedido));

        public IActionResult TiposDeElemento(enumNegocio negocio, string accion)
        {
            try
            {
                var gestorDeNegocio = NegociosDeSe.CrearGestor(Contexto, negocio);
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                //return ViewFormulario(new DescriptorDeTiposDeElemento(gestorDeNegocio, $"tipo-{negocio}", $"Tipos de {negocio.ToNombre()}", nameof(TiposDeElementoController), nameof(TiposDeElemento), accion));
                return ViewFormulario(DescriptorDeTiposDeElemento.CrearDescriptor(gestorDeNegocio, $"tipo-{negocio}", $"Tipos de {negocio.ToNombre()}", nameof(TiposDeElementoController), nameof(TiposDeElemento), accion));
            }
            catch (Exception e)
            {
                return RenderMensaje(GestorDeErrores.Mensaje(e));
            }
        }


        [AllowAnonymous]
        public JsonResult epLeerTipoPorGuid(string enumNegocio, int idTipo, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(nameof(epLeerTipoPorGuid));
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var parametros = extJson.ToDiccionarioDeParametros(parametrosJson);
                var guid = parametros.LeerValor<string>(ltrParametrosEp.guid);
                var id = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                ValidarConsultaPorGuid(NegociosDeSe.ToEnumerado(enumNegocio), id, guid);
                return epLeerTipo(enumNegocio, idTipo, parametrosJson);
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



        public JsonResult epLeerTipo(string enumNegocio, int idTipo, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza("Leer tipo por id");
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext, extJson.ToDiccionarioDeParametros(parametrosJson));
                var negocio = NegociosDeSe.ToEnumerado(enumNegocio);
                r.Datos = negocio.CrearGestorDeTipo(Contexto).LeerElementoPorId(idTipo, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"Tipo '{r.Datos.Nombre}' leido";
            }
            catch (Exception e)
            {
                Contexto.CerrarTraza();
                ApiController.PrepararError(e, r, $"No se ha podido leer el tipo {idTipo} del negocio '{enumNegocio}'.");
            }
            return new JsonResult(r);

        }


        public JsonResult epCrearNodo(string negocio, string json)
        {
            var r = new Resultado();

            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var tipoDto = ApiDeTipos.PersistirTipoDto(Contexto, NegociosDeSe.ToEnumerado(negocio), json, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = "Tipo creado";
                r.Datos = tipoDto;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido crear.");
            }
            return new JsonResult(r);
        }

        public JsonResult epPersistirNodo(string negocio, string json, string operacion)
        {
            var r = new Resultado();

            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var tipoDto = ApiDeTipos.PersistirTipoDto(Contexto, NegociosDeSe.ToEnumerado(negocio), json, new ParametrosDeNegocio(operacion.ToTipoOperacion(), aplicarJoin: true) { Peticion = enumPeticion.PersistirElemento });
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"información del tipo {operacion}";
                r.Datos = tipoDto;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, $"No se ha podido {operacion}.");
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerNodoSeleccionado(string negocio, int id, string filtrosJson)
        {
            return LeerNodoSeleccionado(negocio, id, () => ApiDeTipos.LeerTipoDtoPorId(Contexto, NegociosDeSe.ToEnumerado(negocio), id, filtrosJson));
        }

        public override JsonResult epLeerJerarquia(string negocio, int idPadre, string filtrosJson)
        {
            return LeerJerarquia(negocio, idPadre, () => ApiDeTipos.LeerJerarquia(Contexto, NegociosDeSe.ToEnumerado(negocio), idPadre, filtrosJson));
        }

        public JsonResult epLeerPlantillas(string filtrosJson)
        {
            var r = new Resultado();
            var filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
            Contexto.IniciarTraza("Leer plantillas por tipo");
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var filtroNegocio = filtros.Where(x => x.Clausula.ToLower() == ltrParametrosEp.idNegocio.ToLower()).First();
                var filtroTipo = filtros.Where(x => x.Clausula.ToLower() == ltrParametrosEp.idTipo.ToLower()).First();
                var idNegocio = (int)long.Parse(filtroNegocio.Valor);
                var plantillas = NegociosDeSe.ToEnumerado(idNegocio).LeerPlantillasPorTipoDto(Contexto, (int)long.Parse(filtroTipo.Valor));
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"Plantillas por tipo leidas";
                r.Datos = plantillas;
            }
            catch (Exception e)
            {
                Contexto.CerrarTraza();
                ApiController.PrepararError(e, r, $"No se ha podido leer las plantillas por tipo.");
            }
            return new JsonResult(r);

        }

        public JsonResult epLeerPlantilla(int id, string parametrosJson)
        {
            var r = new Resultado();
            var parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza("Leer plantilla por tipo");
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio);
                var negocio = NegociosDeSe.ToEnumerado(idNegocio);
                var gestor = NegociosDeSe.CrearGestor(Contexto, negocio.ObtenerMetadatos().PlantillasPorTipoDtm, typeof(PlantillaPorTipoDto));
                var parametrosNeg = new ParametrosDeNegocio(enumTipoOperacion.Eliminar);
                parametrosNeg.EsUnaPeticion = true;
                parametrosNeg.Peticion = enumPeticion.epLeerPorId;
                r.Datos = gestor.LeerElementoPorId(id, parametrosNeg.Parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"Plantilla por tipo leida";
            }
            catch (Exception e)
            {
                Contexto.CerrarTraza();
                ApiController.PrepararError(e, r, $"No se ha podido leer la plantilla por tipo.");
            }
            return new JsonResult(r);

        }

        public JsonResult epCrearPlantilla(int idNegocio, string elementoJson)
        {
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                var tipoDtm = NegociosDeSe.ToEnumerado(idNegocio).ObtenerMetadatos().PlantillasPorTipoDtm;
                var gestor = NegociosDeSe.CrearGestor(Contexto, tipoDtm, typeof(PlantillaPorTipoDto));
                var elemento = JsonConvert.DeserializeObject<PlantillaPorTipoDto>(elementoJson);
                var parametros = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
                parametros.EsUnaPeticion = true;
                parametros.Peticion = enumPeticion.PersistirElemento;
                r.Datos = gestor.PersistirElementoDto(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        public JsonResult epBorrarPlantilla(int id, string parametrosJson)
        {
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio);
                var negocio = NegociosDeSe.ToEnumerado(idNegocio);
                var gestor = NegociosDeSe.CrearGestor(Contexto, negocio.ObtenerMetadatos().PlantillasPorTipoDtm, typeof(PlantillaPorTipoDto));
                var parametrosNeg = new ParametrosDeNegocio(enumTipoOperacion.Eliminar);
                parametrosNeg.EsUnaPeticion = true;
                parametrosNeg.Peticion = enumPeticion.epBorrarVinculo;
                r.Datos = gestor.PersistirRegistro(gestor.LeerRegistroPorId(id, aplicarJoin: false), parametrosNeg);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        public JsonResult epModificarPlantilla(int idNegocio, string elementoJson)
        {
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                var tipoDtm = NegociosDeSe.ToEnumerado(idNegocio).ObtenerMetadatos().PlantillasPorTipoDtm;
                var gestor = NegociosDeSe.CrearGestor(Contexto, tipoDtm, typeof(PlantillaPorTipoDto));
                var elemento = JsonConvert.DeserializeObject<PlantillaPorTipoDto>(elementoJson);
                var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
                parametros.EsUnaPeticion = true;
                parametros.Peticion = enumPeticion.PersistirElemento;
                r.Datos = gestor.PersistirElementoDto(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        public FileStreamResult epDescargarPlantilla(string negocio, int idPlantilla, int idArchivo)
        {
            return ApiController.DescargarArchivo(Contexto, idArchivo, enumRutas.RutaDePlantillas, usarCacheado: false);
        }


        public JsonResult epLeerClases(string filtrosJson)
        {
            var r = new Resultado();
            var filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
            Contexto.IniciarTraza("Leer clases por tipo");
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var filtroNegocio = filtros.Where(x => x.Clausula.ToLower() == ltrParametrosEp.idNegocio.ToLower()).First();
                var filtroTipo = filtros.Where(x => x.Clausula.ToLower() == ltrParametrosEp.idTipo.ToLower()).First();
                var idNegocio = (int)long.Parse(filtroNegocio.Valor);
                var clases = NegociosDeSe.ToEnumerado(idNegocio).LeerClasesDelTipoDto(Contexto, (int)long.Parse(filtroTipo.Valor));
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"Clases por tipo leidas";
                r.Datos = clases;
            }
            catch (Exception e)
            {
                Contexto.CerrarTraza();
                ApiController.PrepararError(e, r, $"No se ha podido leer las clases por tipo.");
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerClase(int id, string parametrosJson)
        {
            var r = new Resultado();
            var parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza("Leer clase por tipo");
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio);
                var negocio = NegociosDeSe.ToEnumerado(idNegocio);
                var gestor = NegociosDeSe.CrearGestor(Contexto, negocio.ObtenerMetadatos().ClasesDelTipoDtm, typeof(ClaseDelTipoDto));
                var parametrosNeg = new ParametrosDeNegocio(enumTipoOperacion.Eliminar);
                parametrosNeg.EsUnaPeticion = true;
                parametrosNeg.Peticion = enumPeticion.epLeerPorId;
                r.Datos = gestor.LeerElementoPorId(id, parametrosNeg.Parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"Clase por tipo leida";
            }
            catch (Exception e)
            {
                Contexto.CerrarTraza();
                ApiController.PrepararError(e, r, $"No se ha podido leer la clase por tipo.");
            }
            return new JsonResult(r);

        }

        public JsonResult epCrearClase(int idNegocio, string elementoJson)
        =>
        ApiController.CrearElemento<ClaseDelTipoDto>(Contexto, HttpContext, NegociosDeSe.ToEnumerado(idNegocio).ObtenerMetadatos().ClasesDelTipoDtm, elementoJson);

        public JsonResult epBorrarClase(int id, string parametrosJson)
        {
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
            var negocio = NegociosDeSe.ToEnumerado((int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio));
            return ApiController.BorrarElemento<ClaseDelTipoDto>(Contexto, HttpContext, id, negocio.ObtenerMetadatos().ClasesDelTipoDtm);
        }

        public JsonResult epModificarClase(int idNegocio, string elementoJson)
        =>
        ApiController.ModificarElemento<ClaseDelTipoDto>(Contexto, HttpContext, NegociosDeSe.ToEnumerado(idNegocio).ObtenerMetadatos().ClasesDelTipoDtm, elementoJson);


        protected override IEnumerable<TipoDeElementoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            return ApiDeTipos.LeerElementos<TipoDeElementoDto>(Contexto, filtros, orden, opcionesDeMapeo);
        }

    }
}
