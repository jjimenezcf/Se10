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
using Inicializador.Logistica;
using ServicioDeDatos.Seguridad;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class DevolucionesDeVentaController : EntidadController<ContextoSe, DevolucionDeVentaDtm, DevolucionDeVentaDto>
    {
        public DevolucionesDeVentaController(GestorDeDevolucionesDeVenta gestorDeDevolucionesDeVenta, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeDevolucionesDeVenta,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudDevolucionesDeVenta()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeDevolucionesDeVenta).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Logistica}/{nameof(CrudDevolucionesDeVenta)}";
                    return base.View(destino, new DescriptorDeDevolucionesDeVenta(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<DevolucionDeVentaDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeDevolucionesDeVenta(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        // Acción Devolver de las líneas de un documento de venta facturado: añade la línea a la devolución en tramitación elegida o, si no se
        // elige ninguna, crea una nueva (ver ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta)
        public JsonResult epDevolverLineaDeVenta(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epDevolverLineaDeVenta));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idLineaDeVenta = (int)parametros.LeerValor<long>(nameof(DevolverVentaDto.IdLineaDeVenta), 0);
                if (idLineaDeVenta == 0)
                    GestorDeErrores.Emitir("Ha de indicar la línea vendida que se devuelve");
                var cantidad = parametros.LeerValor<decimal?>(nameof(DevolverVentaDto.Cantidad), null);
                if (cantidad == null || cantidad <= 0)
                    GestorDeErrores.Emitir("Ha de indicar la cantidad a devolver");
                var idDevolucion = (int)parametros.LeerValor<long>(nameof(DevolverVentaDto.IdDevolucion), 0);

                var devolucion = ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta(Contexto, idLineaDeVenta, cantidad.Value, idDevolucion);
                r.Datos = devolucion.Id;
                r.Consola = idDevolucion > 0
                    ? $"Se ha añadido la línea a la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}'"
                    : $"Se ha creado la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' con la línea devuelta";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al devolver la línea vendida.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Check de aceptar del grid de devoluciones de la edición del documento de venta: transita la devolución a aceptada, como la
        // transición 'DDV: Aceptar' (entradas en el almacén con su preasiento y el barrido, regularización de las facturas y su documento,
        // ver GestorDeDevolucionesDeVenta.AntesDeTransitar y DespuesDeTransitar)
        public JsonResult epAceptarDevolucion(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epAceptarDevolucion));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idDevolucion = (int)parametros.LeerValor<long>(nameof(DevolverVentaDto.IdDevolucion), 0);
                if (idDevolucion == 0)
                    GestorDeErrores.Emitir($"Ha de indicar la {enumNegocio.DevolucionVenta.Singular(true)} que se acepta");

                var devolucion = Contexto.SeleccionarPorId<DevolucionDeVentaDtm>(idDevolucion, usarLaCache: false);
                if (devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada))
                    GestorDeErrores.Emitir($"La {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' ya está aceptada");
                if (devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada))
                    GestorDeErrores.Emitir($"La {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' está cancelada, no se puede aceptar");

                devolucion = devolucion.TransitarALaEtapa(Contexto, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.EstadosDeLaEtapa(), delSistema: false);
                devolucion = Contexto.SeleccionarPorId<DevolucionDeVentaDtm>(idDevolucion, usarLaCache: false);

                r.Datos = devolucion.Id;
                r.Consola = $"Se ha aceptado la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' el {devolucion.DevueltaEl:dd/MM/yyyy}";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al aceptar la devolución.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Acción Cancelar del grid de devoluciones de la edición del documento de venta: transita la devolución en tramitación a cancelada, como
        // la transición 'DDV: Cancelar'. No crea movimientos ni toca facturas; lo cancelado deja de contar como devuelto
        public JsonResult epCancelarDevolucion(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCancelarDevolucion));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idDevolucion = (int)parametros.LeerValor<long>(nameof(DevolverVentaDto.IdDevolucion), 0);
                if (idDevolucion == 0)
                    GestorDeErrores.Emitir($"Ha de indicar la {enumNegocio.DevolucionVenta.Singular(true)} que se cancela");

                var devolucion = Contexto.SeleccionarPorId<DevolucionDeVentaDtm>(idDevolucion, usarLaCache: false);
                if (!devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                    GestorDeErrores.Emitir($"La {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' no está {enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Nombre()}, no se puede cancelar");

                devolucion = devolucion.TransitarALaEtapa(Contexto, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada.EstadosDeLaEtapa(), delSistema: false);

                r.Datos = devolucion.Id;
                r.Consola = $"Se ha cancelado la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}'";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al cancelar la devolución.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public IActionResult MaestrosDeDevolucionesDeVenta()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opciÃ³n sÃ³lo se permite a parametrizadores");

                InzDevolucionesDeVenta.ModeloDeDevolucionesDeVenta(Contexto);
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
