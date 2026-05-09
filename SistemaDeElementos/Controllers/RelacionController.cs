using Microsoft.AspNetCore.Mvc;
using System;
using Gestor.Errores;
using GestorDeElementos;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Controllers
{
    public class RelacionController<TContexto, TRelacion, TElemento> : EntidadController<TContexto, TRelacion, TElemento>
        where TContexto : ContextoSe
        where TRelacion : RelacionDtm
        where TElemento : ElementoDto
    {

        protected GestorDeRelaciones<TContexto, TRelacion, TElemento> GestorDeRelaciones => (GestorDeRelaciones<TContexto,TRelacion,TElemento>)_GestorDeElementos;

        public RelacionController(GestorDeRelaciones<TContexto, TRelacion, TElemento> gestorDeRelaciones, GestorDeErrores gestorErrores)
        : base(gestorDeRelaciones, gestorErrores)
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson)
        {
            var r = new Resultado();
            var tran = _GestorDeElementos.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                var relacion = JsonConvert.DeserializeObject<TElemento>(elementoJson);
                var parametros = AntesDeEjecutar_CrearElemento(relacion);
                parametros.Parametros[ltrParametrosNeg.Peticion] = enumPeticion.epCrearRelacion;
                GestorDeRelaciones.CrearRelacion(relacion, parametros, true);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Registro creado";
                _GestorDeElementos.Commit(tran);
            }
            catch (Exception e)
            {
                _GestorDeElementos.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido crear.");
            }

            return new JsonResult(r);
        }

        public JsonResult epCrearRelaciones(string propiedadId, int id, string idsJson)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                List<int> listaIds = JsonConvert.DeserializeObject<List<int>>(idsJson);
                var relacionados = 0;
                var mensajeInformativo = "";
                var parametros = new Dictionary<string,object>();            
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epCrearRelaciones;
                foreach (var idParaRelacionar in listaIds)
                {
                    if (!GestorDeRelaciones.CrearRelacion(propiedadId, id, idParaRelacionar, false, parametros).existe)
                        relacionados++;
                    else
                        mensajeInformativo = mensajeInformativo + Environment.NewLine + $"Existe la relación {id} con {idParaRelacionar}";
                }
                r.Total = relacionados;
                r.Consola = $"Se han relacionado {relacionados} de los {listaIds.Count} marcados" +
                              Environment.NewLine + mensajeInformativo;
                //r.Mensaje = $"Se han relacionado {relacionados} de los {listaIds.Count} marcados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error en el proceso de relación.");
            }

            return new JsonResult(r);
        }

        public JsonResult epModificarRelacion(string elementoJson)
        {
            var r = new Resultado();
            var tran = _GestorDeElementos.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                var elemento = JsonConvert.DeserializeObject<TElemento>(elementoJson);
                var parametros = AntesDeEjecutar_ModificarElemento(elemento);
                parametros.Parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros.Parametros[ltrParametrosNeg.Peticion] = enumPeticion.epModificarRelacion;
                GestorDeRelaciones.ModificarRelacion(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Registro modificado";
                _GestorDeElementos.Commit(tran);
            }
            catch (Exception e)
            {
                _GestorDeElementos.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido modificar.");
            }

            return new JsonResult(r);
        }


        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson)
        {
            return ApiController.BorrarRelacionPorId(GestorDeRelaciones, id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);
        }

    }
}
