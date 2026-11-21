using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using GestoresDeNegocio.Logistica;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Inicializador.Logistica;

namespace MVCSistemaDeElementos.Controllers
{
    public class VentasController : EntidadController<ContextoSe, VentaDtm, VentaDto>
    {
        public VentasController(GestorDeVentas gestorDeVentas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeVentas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudVentas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeVentas).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Logistica}/{nameof(CrudVentas)}";
                    return base.View(destino, new DescriptorDeVentas(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<VentaDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeVentas(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        // Venta de material desde la consulta de inventario (modal CrearVentaDto, ver Inventario.ts): crea el documento de venta con su línea,
        // la salida del almacén (y su preasiento si procede) y barre los movimientos posteriores. Devuelve la url de edición de la venta
        public JsonResult epCrearVenta(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idAlmacen = (int)parametros.LeerValor<long>(nameof(CrearVentaDto.IdAlmacen), 0);
                var idUnitario = (int)parametros.LeerValor<long>(nameof(CrearVentaDto.IdUnitario), 0);
                var idCliente = (int)parametros.LeerValor<long>(nameof(CrearVentaDto.IdCliente), 0);
                var fecha = parametros.LeerValor<DateTime?>(nameof(CrearVentaDto.Fecha), null);
                var cantidad = parametros.LeerValor<decimal?>(nameof(CrearVentaDto.Cantidad), null);
                var precio = parametros.LeerValor<decimal?>(nameof(CrearVentaDto.Precio), null);
                var facturar = parametros.LeerValor(nameof(CrearVentaDto.Facturar), false);
                var servido = parametros.LeerValor(nameof(CrearVentaDto.Servido), false) || facturar;

                if (fecha is null) GestorDeErrores.Emitir("Ha de indicar la fecha de la venta");
                if (cantidad is null) GestorDeErrores.Emitir("Ha de indicar la cantidad a vender");
                if (precio is null) GestorDeErrores.Emitir("Ha de indicar el precio de venta");

                var venta = ExtensorDeVentas.CrearVentaDeMaterial(Contexto, idAlmacen, idUnitario, idCliente, (DateTime)fecha, (decimal)cantidad, (decimal)precio, servido, facturar);

                // si se ha facturado se navega a la prefactura en lugar de al documento de venta
                var url = facturar && venta.IdFacturaEmt is not null
                    ? enumNegocio.FacturaEmitida.UrlDeEdicion(Contexto, (int)venta.IdFacturaEmt)
                    : enumNegocio.Venta.UrlDeEdicion(Contexto, venta.Id);
                r.Datos = new { venta.Id, Url = url };
                r.Consola = $"Se ha creado el {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' y la salida del almacén" +
                            (facturar ? ", y su prefactura" : servido ? ", y se ha dado por servido" : "");
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Gestor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al crear la venta de material.");
            }
            return new JsonResult(r);
        }

        public IActionResult MaestrosDeVentas()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzVentas.ModeloDeVentas(Contexto);
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
