using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using Utilidades;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using GestoresDeNegocio.Logistica;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Inicializador.Logistica;

namespace MVCSistemaDeElementos.Controllers
{
    public class RegularizacionesController : EntidadController<ContextoSe, RegularizacionDtm, RegularizacionDto>
    {
        public RegularizacionesController(GestorDeRegularizaciones gestorDeRegularizaciones, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeRegularizaciones,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudRegularizaciones()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeRegularizaciones(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        public IActionResult MaestrosDeRegularizaciones()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzRegularizaciones.ModeloDeRegularizaciones(Contexto);
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
