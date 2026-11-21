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

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Ral_PrepararRecuento:
                    // la modal se abre con el almacén filtrado y al aceptar se llama a epPrepararRecuento
                    return null;
                case eventosDeMf.Ral_VenderMaterial:
                    // la modal se abre con los datos de la fila (ver epDatosParaVender) y al aceptar se llama a VentasController.epCrearVenta
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        // Somete el trabajo que genera el Excel del recuento del almacén
        public JsonResult epPrepararRecuento(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(PrepararRecuentoDto.IdAlmacen))) throw new Exception("Debe indicar el almacén del que se prepara el recuento");

                var idAlmacen = (int)parametros.LeerValor<long>(nameof(PrepararRecuentoDto.IdAlmacen));
                var referencia = parametros.LeerValor(nameof(PrepararRecuentoDto.Referencia), "");
                var nombre = parametros.LeerValor(nameof(PrepararRecuentoDto.Nombre), "");

                var clase = parametros.LeerValor(nameof(PrepararRecuentoDto.Clase), "");
                // ha de llegar uno de los valores del enumerado (Inicial, Recuento o AjusteDePrecio), no un número que lo parezca
                if (!Enum.GetNames<enumRegularizacionAlm>().Contains(clase) || !Enum.TryParse<enumRegularizacionAlm>(clase, out var claseDeRegularizacion))
                    throw new Exception($"Debe indicar la clase de regularización para la que se prepara el Excel: {string.Join(", ", Enum.GetNames<enumRegularizacionAlm>())}");

                var trabajo = TrabajosParaRegularizaciones.SometerPrepararRecuento(Contexto, idAlmacen, referencia, nombre, claseDeRegularizacion);

                r.Consola = "Se ha sometido la preparación del recuento, se le enviará el Excel por correo cuando finalice";
                r.Datos = trabajo;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al someter la preparación del recuento.");
            }
            return new JsonResult(r);
        }

        // Datos con los que se abre la modal de venta de material (CrearVentaDto) para la fila del inventario: su id es el de su último
        // movimiento, del que se toman el almacén y el material; el precio que se propone es el de venta del material
        public JsonResult epDatosParaVender(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idMovimiento = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var movimiento = Contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(idMovimiento);
                var almacen = movimiento.Almacen(Contexto);
                var unitario = movimiento.Unitario(Contexto);
                r.Datos = new CrearVentaDto
                {
                    IdAlmacen = almacen.Id,
                    Almacen = almacen.Expresion,
                    IdUnitario = unitario.Id,
                    Unitario = unitario.Expresion,
                    Fecha = DateTime.Today,
                    Precio = unitario.Venta
                };
                r.Consola = "Datos para la venta obtenidos correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se pueden obtener los datos para vender el material.");
            }
            return new JsonResult(r);
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
