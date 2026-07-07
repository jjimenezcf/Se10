using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;

namespace GestoresDeNegocio.Gastos
{
    internal static class AgrupadosDeFacturasRec
    {
        public static decimal? ResolverCampoCalculado(ElementoDeProcesoDtm registro, MetricaDeTotales metrica, ContextoSe contexto)
        {
            if (!(registro is FacturaRecDtm factura)) return null;
            var campo = metrica.Campo.Substring("calculado:".Length).Trim().ToLowerInvariant();
            return campo switch
            {
                "baseimponible" or "bi" or "base"      => factura.BaseImponible != 0 ? factura.BaseImponible : (decimal?)null,
                "total" or "importe" or "totaldelpago" => factura.TotalDelPago  != 0 ? factura.TotalDelPago  : (decimal?)null,
                _ => null
            };
        }

        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
            => AgrupadosDeUnProcesoHelper.ResolverEtiquetaComun(prop, id, contexto, tipos, estados, cgs);
    }
}
