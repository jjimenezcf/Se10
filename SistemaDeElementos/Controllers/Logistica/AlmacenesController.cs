using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using GestoresDeNegocio.Logistica;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Inicializador.Logistica;

namespace MVCSistemaDeElementos.Controllers
{
    public class AlmacenesController : EntidadController<ContextoSe, AlmacenDtm, AlmacenDto>
    {
        public AlmacenesController(GestorDeAlmacenes gestorDeAlmacenes, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAlmacenes,
           gestorDeErrores
         )
        {
        }

        // las opciones de navegación del menú no hacen nada en el servidor: abren el otro mantenimiento en el cliente
        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Alm_IrACompras:
                case eventosDeMf.Alm_IrAVentas:
                case eventosDeMf.Alm_IrARecuentos:
                case eventosDeMf.Alm_IrAInventario:
                case eventosDeMf.Alm_IrAMovimientos:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public IActionResult CrudAlmacenes()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeAlmacenes(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        public IActionResult MaestrosDeAlmacenes()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzAlmacenes.ModeloDeAlmacenes(Contexto);
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
