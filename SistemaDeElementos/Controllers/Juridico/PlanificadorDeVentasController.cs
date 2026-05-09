using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Juridico;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos.Extensores;
using iText.Commons.Actions.Contexts;
using iText.Layout.Element;
using ServicioDeDatos.Ventas;
using GestorDeElementos;
using GestoresDeNegocio.Guarderias;
using ModeloDeDto.Guarderias;
using System;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlanificadorDeVentasController : EntidadController<ContextoSe, PlanificadorDeVentaDtm, PlanificadorDeVentaDto>
    {
        public PlanificadorDeVentasController(GestorDelPlanificadorDeVentas gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPlanificadorDeVentas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDePlanificadorDeVentas(Contexto, ModoDescriptor.Mantenimiento));
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDelPlanificadorDeVentas(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Plf_GenerarPlanificadorDeVenta:
                    GestorDelPlanificadorDeVentas.Generar(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                    return true;
                case eventosDeMf.Plf_IrAPlvDeVenta:
                    foreach (var id in (List<int>)parametros[ltrParametrosEp.ids])
                    {
                        var parte = Contexto.SeleccionarPorId<PlanificadorDeVentaDtm>(id);
                        parte.ValidarQueReferenciaA<PlanificacionDeVentaDtm>(Contexto, nameof(PlanificacionDeVentaDtm.IdPlanificador));
                    }
                    return true;

            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

    }
}
