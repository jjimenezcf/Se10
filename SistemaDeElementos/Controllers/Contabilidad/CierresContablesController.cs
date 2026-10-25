using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CierresContablesController : EntidadController<ContextoSe, CierreContableDtm, CierreContableDto>
    {
        public CierresContablesController(GestorDeCierresContables gestorDeCierresContables, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCierresContables,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudCierresContables()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeCierresContables(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        // Los cierres solo se consultan: los registran los trabajos de crear y anular lote contable
        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            indicadores[IndCrud.SiempreEnConsulta] = true;
            return indicadores;
        }

        // Somete el trabajo de crear cierre contable de la sociedad, que crea su lote contable y al terminar registra el cierre
        public JsonResult epCrearCierre(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idSociedad = (int)parametros.LeerValor<long>(nameof(CrearCierreContableDto.IdSociedad));
                var cerrarHasta = parametros.LeerValor<DateTime?>(nameof(CrearCierreContableDto.CerrarHasta), null);
                if (cerrarHasta is null)
                    GestorDeErrores.Emitir($"Ha de indicar la fecha hasta la que se cierra la sociedad");
                var contabilidad = parametros.LeerValor<string>(nameof(CrearCierreContableDto.Contabilidad), "");
                // "-1" es la opción "Seleccionar..." de la lista, que TryParse aceptaría como valor numérico
                if (!Enum.TryParse<enumSistemaContable>(contabilidad, out var sistema) || !Enum.IsDefined(sistema))
                    GestorDeErrores.Emitir($"Ha de indicar en qué contabilidad se contabiliza la sociedad");
                var digitos = (int)parametros.LeerValor<long>(nameof(CrearCierreContableDto.Digitos), 0);

                TrabajosContables.SometerCrearCierreContable(Contexto, idSociedad, (DateTime)cerrarHasta, sistema, digitos);

                r.Mensaje = $"Trabajo de creación del cierre contable sometido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No ha podido someter la creación del cierre contable.");
            }
            return new JsonResult(r);
        }

        // Somete el trabajo de eliminar el cierre contable, que anula su lote contable y elimina el cierre
        public JsonResult epEliminarCierre(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idCierre = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var cierre = Contexto.SeleccionarPorId<CierreContableDtm>(idCierre);
                TrabajosContables.SometerEliminarCierreContable(Contexto, idCierre);

                r.Mensaje = $"Trabajo de eliminación del cierre contable '{cierre.Nombre}' sometido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No ha podido someter la eliminación del cierre contable.");
            }
            return new JsonResult(r);
        }
    }
}
