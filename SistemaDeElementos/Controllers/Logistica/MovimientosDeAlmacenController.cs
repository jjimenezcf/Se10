using Gestor.Errores;
using GestoresDeNegocio.Logistica;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Logistica;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class MovimientosDeAlmacenController : EntidadController<ContextoSe, MovimientoDeAlmacenDtm, MovimientoDeAlmacenDto>
    {
        public MovimientosDeAlmacenController(GestorDeMovimientosDeAlmacen gestorDeMovimientosDeAlmacen, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeMovimientosDeAlmacen,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudMovimientosDeAlmacen()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Consulta;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeMovimientosDeAlmacen).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Logistica}/{nameof(CrudMovimientosDeAlmacen)}";
                    return base.View(destino, new DescriptorDeMovimientosDeAlmacen(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<MovimientoDeAlmacenDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeMovimientosDeAlmacen(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }

        }

        // Url de la edición del proveedor del pedido recepcionado, para navegar desde la columna del proveedor del grid. No se usa el
        // genérico epObtenerUrlAlProveedor porque el movimiento no es IUsaProveedor: su proveedor es el del pedido de la recepción
        public JsonResult epObtenerUrlAlProveedorDelMovimiento(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idMovimiento = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var movimiento = Contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(idMovimiento);
                r.Datos = movimiento.UrlDelProveedor(Contexto);
                r.Consola = $"Proveedor del movimiento obtenido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener acceso al proveedor del movimiento.");
            }
            return new JsonResult(r);
        }

        // Url de la edición del preasiento contable del movimiento, para navegar desde la columna del preasiento del grid
        public JsonResult epObtenerUrlAlPreasiento(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idMovimiento = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var movimiento = Contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(idMovimiento);
                r.Datos = movimiento.UrlDelPreasiento(Contexto);
                r.Consola = $"Preasiento del movimiento obtenido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener acceso al preasiento del movimiento.");
            }
            return new JsonResult(r);
        }

        // Url de la edición del documento (pedido, regularización...) que generó el movimiento, para navegar desde el origen o el destino del grid
        public JsonResult epObtenerUrlAlDocumento(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idMovimiento = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var movimiento = Contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(idMovimiento);
                r.Datos = movimiento.UrlDelDocumento(Contexto);
                r.Consola = $"Documento del movimiento obtenido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener acceso al documento del movimiento.");
            }
            return new JsonResult(r);
        }
    }
}
