using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class EstadosController : EntidadController<ContextoSe, EstadoDtm, EstadoDto>
    {
        public EstadosController(GestorDeEstados gestorDeEstados, GestorDeErrores gestorDeErrores, IMapper mapper)
        : base(gestorDeEstados, gestorDeErrores)
        {
            Contexto.Mapeador = mapper;
        }

        public IActionResult CrudDeEstados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            try
            {
                enumNegocio negocio = ApiController.AsignarNegocioDeLaRequest(_GestorDeElementos, HttpContext);
                return ViewCrud(new DescriptorDeEstados(Contexto, ModoDescriptor.Mantenimiento, negocio));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Transiciones:
                    return null;
                case eventosDeMf.Flujo:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        [HttpGet]
        public JsonResult epObtenerFlujo(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.enumNegocio))) throw new Exception("Debe indicar el negocio para el flujo que se quiere mostrar");
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.idEstado))) throw new Exception("Debe indicar el id del estado inicial");

                var negocio = parametros.LeerValor<string>(nameof(ltrParametrosEp.enumNegocio));
                var idEstado = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.idEstado));
                Contexto.IniciarTraza(GetType().Name + "_" + nameof(epObtenerFlujo));
                try
                {
                    ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                    var negocioEnum = GestorDeElementos.NegociosDeSe.ToEnumerado(negocio);
                    r.Datos = NegociosDeSe.ObtenerFlujoDeNegocio(Contexto, negocioEnum, idEstado);
                    r.Estado = enumEstadoPeticion.Ok;
                    r.Mensaje = "flujo obtenido";
                }
                catch (Exception e)
                {
                    ApiController.PrepararError(e, r, $"Error al obtener el flujo del negocio {negocio} para el estado {idEstado}");
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, e.Message);
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [HttpGet]
        public JsonResult epLeerPosicionesDeEstados(string negocio)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerPosicionesDeEstados));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = NegociosDeSe.ToEnumerado(negocio).PosicionesDeEstados(Contexto);
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al obtener la disposición del dashboard para el usuario {DatosDeConexion.Login}.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [HttpPost]
        public JsonResult epGrabarDisposicionDeEstados(string negocio)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGrabarDisposicionDeEstados));
            var peticion = eventosDeMf.Comun_GuardarDisposicionEstados.ToString();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var body = ApiController.LeerBody(HttpContext);
                r.Datos = ProcesarPeticion(NegociosDeSe.ToEnumerado(negocio), vista: null, peticion, body.parametros);
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



    }

}
