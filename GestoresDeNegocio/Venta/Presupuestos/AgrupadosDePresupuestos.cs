using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;

namespace GestoresDeNegocio.Presupuesto
{
    internal static class AgrupadosDePresupuestos
    {
        public static decimal? ResolverCampoCalculado(ElementoDeProcesoDtm registro, MetricaDeTotales metrica, ContextoSe contexto)
        {
            if (!(registro is PresupuestoDtm presupuesto)) return null;
            var campo = metrica.Campo.Substring("calculado:".Length).Trim().ToLowerInvariant();
            return campo switch
            {
                "total" or "importe" or "importetotal" => presupuesto.Total > 0 ? presupuesto.Total : (decimal?)null,
                _ => null
            };
        }

        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
            => AgrupadosDeUnProcesoHelper.ResolverEtiquetaComun(prop, id, contexto, tipos, estados, cgs);
    }
}
