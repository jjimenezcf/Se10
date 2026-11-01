using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeVentas
    {
        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremento = enumNegocio.Venta.LeerCrearParametro(contexto, enumParametrosDeVentas.DSA_IncrementarOrdenEn, "10");
            if (incremento is null || incremento.Valor.Entero() == 0)
                GestorDeErrores.Emitir($"Ha de definir el parámetro '{enumParametrosDeVentas.DSA_IncrementarOrdenEn}' del negocio de '{enumNegocio.Venta.Singular()}' con un valor mayor de 0");
            return incremento.Valor.Entero();
        }

        public static VentaDtm Venta(this LineaDeUnaVentaDtm linea, ContextoSe contexto)
        {
            if (linea.Elemento is not null && linea.IdElemento == linea.Elemento.Id)
            {
                return linea.Elemento;
            }

            return linea.Elemento = contexto.SeleccionarPorId<VentaDtm>(linea.IdElemento);
        }

        /// <summary>
        /// Valor del stock (precio medio ponderado) del unitario de la línea en el almacén del documento de venta a fecha de solicitud:
        /// el Valor del último movimiento de almacén realizado hasta ese día. Si no se ha indicado la fecha de solicitud se toma la de hoy.
        /// Devuelve null si no hay movimientos anteriores.
        /// </summary>
        public static decimal? PrecioAlmacen(this LineaDeUnaVentaDtm linea, ContextoSe contexto)
        {
            var venta = linea.Venta(contexto);
            var hasta = (venta.SolicitadoEl ?? DateTime.Now).Date.AddDays(1);

            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == venta.IdAlmacen
                         && x.IdUnitario == linea.IdUnitario
                         && x.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                         && x.RealizadoEl < hasta)
                .OrderByDescending(x => x.RealizadoEl).ThenByDescending(x => x.Id)
                .Select(x => (decimal?)x.Valor)
                .FirstOrDefault();
        }

        /// <summary>
        /// Margen (%) del precio de venta neto de descuento sobre el precio del almacén: (precio neto - precio almacén) / precio neto.
        /// Si es negativo se está vendiendo por debajo del coste.
        /// </summary>
        public static decimal? Margen(this LineaDeUnaVentaDtm linea, decimal? precioAlmacen)
        {
            var precioNeto = linea.Precio * (1 - (linea.Descuento ?? 0m) / 100);
            if (precioAlmacen is null || precioNeto == 0)
                return null;

            return Math.Round((precioNeto - (decimal)precioAlmacen) / precioNeto * 100, 2);
        }

        public static AlmacenDtm Almacen(this VentaDtm venta, ContextoSe contexto)
        {
            if (venta.Almacen is not null && venta.IdAlmacen == venta.Almacen.Id)
            {
                return venta.Almacen;
            }

            return venta.Almacen = contexto.SeleccionarPorId<AlmacenDtm>(venta.IdAlmacen);
        }

        /// <summary>
        /// DSA: Solicitado --> DSA: Servir --> DSA: Sirviendo (del sistema)
        /// Se ha de llamar al realizar la primera salida del almacén del documento de venta
        /// </summary>
        public static void Servir(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: transitar del sistema a la etapa DSA_Etapa_Servicio y fijar ServidoEl
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Devolver a solicitado --> DSA: Solicitado (del sistema)
        /// Se ha de llamar tras anular el último movimiento de salida del almacén del documento de venta
        /// </summary>
        public static void DevolverASolicitado(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: transitar del sistema a la etapa DSA_Etapa_Solicitud y limpiar ServidoEl
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado
        /// </summary>
        public static void AntesDeFacturar(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: validaciones previas a la facturación
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado: crea la prefactura
        /// </summary>
        public static void CrearPrefactura(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: crear la prefactura (factura emitida) a partir de las líneas y asignar IdFactura
        }

        /// <summary>
        /// DSA: Facturado --> DSA: Cancelar prefactura --> DSA: Sirviendo (del sistema)
        /// Se ha de llamar desde el ExtensorDeFacturasEmt al cancelar la prefactura
        /// </summary>
        public static void CancelarPrefactura(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: desvincular la factura (IdFactura) y transitar del sistema a la etapa DSA_Etapa_Servicio
        }

        /// <summary>
        /// DSA: Solicitado --> DSA: Cancelar --> DSA: Cancelado
        /// </summary>
        public static void AntesDeCancelar(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: validaciones previas a la cancelación
        }
    }
}
