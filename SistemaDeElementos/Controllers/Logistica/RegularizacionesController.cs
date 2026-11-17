using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;
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

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Ral_ImportarRecuento:
                    // la modal se propone con los datos de la edición y al aceptar se llama a epImportarRecuento
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        // Somete el trabajo que importa el recuento del Excel a la regularización que se está editando
        public JsonResult epImportarRecuento(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(RecuentoDto.IdArchivo))) throw new Exception("Debe indicar el fichero con el recuento a importar");
                if (!parametros.ContieneClave(ltrDeUnaRegularizacion.IdRegularizacion)) throw new Exception("Debe indicar la regularización a la que se importa el recuento");

                var idArchivo = (int)parametros.LeerValor<long>(nameof(RecuentoDto.IdArchivo));
                var idRegularizacion = (int)parametros.LeerValor<long>(ltrDeUnaRegularizacion.IdRegularizacion);

                var trabajo = TrabajosParaRegularizaciones.SometerImportarRecuentos(Contexto, idArchivo, idRegularizacion);

                r.Consola = "Se ha sometido la importación del recuento, se le notificará cuando finalice";
                r.Datos = trabajo;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al someter la importación del recuento.");
            }
            return new JsonResult(r);
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
