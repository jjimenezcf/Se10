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
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto.SistemaDocumental;

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

        // "Imprimir seleccionado": si no hay más plantillas que la estándar se imprime directamente la oferta de cada documento de venta
        // seleccionado; si las hay, se abre la modal para elegir y al aceptar se llama a Imprimir (como en PedidosController)
        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Dsa_ModalDeImprimir:
                    var ids = (List<int>)parametros[ltrParametrosEp.ids];
                    var plantillas = new ServicioDePlantillas(Contexto, enumNegocio.Venta, ids).Plantillas();
                    if (!plantillas.Abrir)
                        ImprimirOfertas(ids);
                    return plantillas;
                case eventosDeMf.Dsa_CambiarDatos:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        protected override bool Imprimir(int idNegocio, Dictionary<string, object> parametros)
        {
            if (base.Imprimir(idNegocio, parametros))
                return true;

            var plantilla = parametros.LeerValor<string>(ltrParametrosEp.Plantilla);
            if (plantilla != EstandarPlt.Estandard)
                return false;

            ImprimirOfertas(new List<int> { (int)parametros.LeerValor<long>(ltrParametrosEp.idElemento) });
            return true;
        }

        private void ImprimirOfertas(List<int> idsDeVentas)
        {
            foreach (var id in idsDeVentas)
                GestorDeVentas.ImprimirOferta(Contexto, Contexto.SeleccionarPorId<VentaDtm>(id));
        }

        // Acción Devolver de las líneas de un documento de venta servido (aún sin prefacturar): corrige lo servido de la línea (cantidad y
        // precio de venta) y su salida del almacén, y si la oferta estaba impresa la reimprime (ver ExtensorDeVentas.ModificarLineaServida y
        // GestorDeVentas.ReimprimirOferta)
        public JsonResult epModificarLineaServida(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epModificarLineaServida));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idLineaDeVenta = (int)parametros.LeerValor<long>(nameof(ModificarLineaVentaDto.IdLineaDeVenta), 0);
                if (idLineaDeVenta == 0)
                    GestorDeErrores.Emitir("Ha de indicar la línea servida que se modifica");
                var cantidad = parametros.LeerValor<decimal?>(nameof(ModificarLineaVentaDto.Cantidad), null);
                if (cantidad is null)
                    GestorDeErrores.Emitir("Ha de indicar la cantidad servida");
                var precio = parametros.LeerValor<decimal?>(nameof(ModificarLineaVentaDto.Precio), null);
                if (precio is null)
                    GestorDeErrores.Emitir("Ha de indicar el precio de venta");

                var resultado = ExtensorDeVentas.ModificarLineaServida(Contexto, idLineaDeVenta, cantidad.Value, precio.Value);
                var ofertaAnterior = resultado.LineaModificada ? GestorDeVentas.ReimprimirOferta(Contexto, resultado.Venta) : null;

                r.Datos = idLineaDeVenta;
                r.Consola = $"Se ha modificado lo servido de la línea"
                    + (resultado.Devolucion is null ? "" : $"; la salida está en un periodo cerrado, lo no servido se devuelve en la {enumNegocio.DevolucionVenta.Singular(true)} '{resultado.Devolucion.Referencia}', acéptela en el grid de devoluciones")
                    + (ofertaAnterior is null ? "" : $"; la oferta anterior se ha renombrado a '{ofertaAnterior}' y se ha vuelto a imprimir");
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al modificar lo servido de la línea.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // "Cambiar datos" del menú de edición (modal CambiarDatosDeVentaDto): cambia el nombre, el cliente y la fecha de servicio; si está
        // servido rehace las salidas del almacén (ver ExtensorDeVentas.CambiarDatos). Si la oferta estaba impresa, la anterior se renombra
        // con un postfijo "(n)" y se imprime de nuevo (ver GestorDeVentas.ReimprimirOferta)
        public JsonResult epCambiarDatos(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCambiarDatos));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idVenta = (int)parametros.LeerValor<long>(nameof(CambiarDatosDeVentaDto.IdElemento), 0);
                if (idVenta == 0)
                    GestorDeErrores.Emitir($"Ha de indicar el {enumNegocio.Venta.Singular(true)}");
                if (!ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Venta, idVenta).HayPermisosDe(enumModoDeAccesoDeDatos.Gestor))
                    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene permiso de gestor sobre el {enumNegocio.Venta.Singular(true)}, no puede cambiar sus datos");
                var nombre = parametros.LeerValor<string>(nameof(CambiarDatosDeVentaDto.Nombre), null);
                var idCliente = (int)parametros.LeerValor<long>(nameof(CambiarDatosDeVentaDto.IdCliente), 0);
                var servidoEl = parametros.LeerValor<DateTime?>(nameof(CambiarDatosDeVentaDto.ServidoEl), null);

                var venta = ExtensorDeVentas.CambiarDatos(Contexto, idVenta, nombre, idCliente, servidoEl);
                var ofertaAnterior = GestorDeVentas.ReimprimirOferta(Contexto, venta);

                r.Datos = venta.Id;
                r.Consola = $"Se han cambiado los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}'"
                    + (venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Servicio) ? $"; las salidas del almacén se han rehecho a la fecha {venta.ServidoEl:dd/MM/yyyy}" : "")
                    + (ofertaAnterior is null ? "" : $"; la oferta anterior se ha renombrado a '{ofertaAnterior}' y se ha vuelto a imprimir");
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al cambiar los datos del documento de venta.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
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
