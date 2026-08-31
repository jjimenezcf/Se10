using Gestor.Errores;
using GestoresDeNegocio.Logistica;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Logistica;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
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
    }
}
