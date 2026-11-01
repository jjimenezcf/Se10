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
    public class InventarioController : EntidadController<ContextoSe, MovimientoDeAlmacenDtm, InventarioDto>
    {
        public InventarioController(GestorDeInventario gestorDeInventario, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeInventario,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudInventario()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Consulta;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeInventario).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Logistica}/{nameof(CrudInventario)}";
                    return base.View(destino, new DescriptorDeInventario(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<InventarioDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeInventario(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }

        }

        // Url de la edición del unitario de la fila del inventario (el id de la fila es el de su último movimiento), para navegar desde la columna del material
        public JsonResult epObtenerUrlAlUnitario(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idMovimiento = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var movimiento = Contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(idMovimiento);
                r.Datos = movimiento.UrlDelUnitario();
                r.Consola = $"Material del inventario obtenido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener acceso al material del inventario.");
            }
            return new JsonResult(r);
        }
    }
}
