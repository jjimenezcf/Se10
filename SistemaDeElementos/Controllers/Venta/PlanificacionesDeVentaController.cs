using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using System;
using Utilidades;
using System.Collections.Generic;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using Inicializador.Ventas;
using MVCSistemaDeElementos.Descriptores;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlanificacionesDeVentaController : EntidadController<ContextoSe, PlanificacionDeVentaDtm, PlanificacionDeVentaDto>
    {
        public PlanificacionesDeVentaController(GestorDePlanificacionesDeVenta gestorDePlanificacionesDeVenta, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePlanificacionesDeVenta,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPlanificacionesDeVenta()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDePlanificacionesDeVenta(Contexto, ModoDescriptor.Mantenimiento));
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
                case eventosDeMf.Plv_IrAPartesTr:
                    enumNegocio.PlanificacionDeVenta.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(PlanificacionDeVentaDtm.IdParteTr));
                    return null;
                case eventosDeMf.Plv_IrAFacturasEmt:
                    enumNegocio.PlanificacionDeVenta.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(PlanificacionDeVentaDtm.IdFacturaEmt));
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public IActionResult MaestrosDePlanificacionesDeVenta()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzPlanificacionesDeVenta.ModeloDePlanificacionesDeVenta(Contexto);
                ViewBag.Mensaje = "Maestros inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar los maestros.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
        }

    }
}
