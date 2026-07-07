using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using static ServicioDeDatos.Tarea.enumEtiquetasDeTareas;

namespace GestoresDeNegocio.Tarea
{
    // Resolvers de agregación específicos de Tareas.
    internal static class AgrupadosDeTareas
    {
        // Firma compatible con ResolverCalculadoDelegate
        public static decimal? ResolverCampoCalculado(ElementoDeProcesoDtm registro, MetricaDeTotales metrica, ContextoSe contexto)
        {
            if (!(registro is TareaDtm tarea)) return null;

            var campo = metrica.Campo
                .Substring("calculado:".Length)
                .Trim()
                .ToLowerInvariant();

            // TiempoEnEstado lo resuelve el resolver genérico de PreguntaParaLaIa, no la planificación
            if (campo.StartsWith("tiempoenestado")) return null;

            // Ampliacion<T> ya cachea internamente: no lanza SQL si ya se cargó para esta tarea
            var plf = tarea.Ampliacion<PlfDeTareaDtm>(contexto, errorSiNoHay: false);
            if (plf == null) return null;

            // Normalizar: quitar prefijos "duracion", espacios, guiones
            var campoNorm = campo.Replace("duracion", "").Replace("_", "").Replace("-", "").Replace(" ", "");

            // Etiquetas primarias desde enumEtiquetasDeTareas; aliases adicionales por conveniencia
            if (campoNorm.Equals(nameof(EnJornadas), StringComparison.OrdinalIgnoreCase) || campoNorm is "jornadas" or "jornada") return plf.EnJornadas();
            if (campoNorm.Equals(nameof(EnHoras),    StringComparison.OrdinalIgnoreCase) || campoNorm is "horas"    or "hora")    return plf.EnHoras();
            if (campoNorm.Equals(nameof(EnDias),     StringComparison.OrdinalIgnoreCase) || campoNorm is "dias"     or "dia")     return plf.EnDias();
            if (campoNorm.Equals(nameof(EnMinutos),  StringComparison.OrdinalIgnoreCase) || campoNorm is "minutos"  or "minuto")  return plf.EnMinutos();
            return null;
        }

        // Firma compatible con ResolverEtiquetaDelegate
        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
        {
            // Claves virtuales (string): el valor ya ES la etiqueta
            if (prop.Equals(nameof(ReferenciaExpediente), StringComparison.OrdinalIgnoreCase) ||
                prop.Equals(nameof(NombreExpediente),     StringComparison.OrdinalIgnoreCase) ||
                prop.Equals(nameof(ReferenciaPpt),        StringComparison.OrdinalIgnoreCase) ||
                prop.Equals(nameof(NombrePpt),            StringComparison.OrdinalIgnoreCase))
                return id?.ToString() ?? "—";

            // FK específica de Tareas: factura emitida
            if (prop.Equals(nameof(IdFacturaEmt), StringComparison.OrdinalIgnoreCase) && id != null && int.TryParse(id.ToString(), out var idFae))
            {
                var fae = contexto.Set<FacturaEmtDtm>().FirstOrDefault(f => f.Id == idFae);
                if (fae != null)
                    return fae.NumeroDeFactura.Length > 0 ? fae.NumeroDeFactura : fae.Nombre;
                return id.ToString();
            }

            return AgrupadosDeUnProcesoHelper.ResolverEtiquetaComun(prop, id, contexto, tipos, estados, cgs);
        }

        // Carga en batch solo los vínculos N:M que el usuario ha pedido en agruparPor,
        // y devuelve un closure que resuelve las claves virtuales correspondientes.
        // Firma compatible con el patrón descubierto por reflexión en PreguntaParaLaIa.
        public static Func<string, ElementoDeProcesoDtm, string> ObtenerResolverDeClaves(
            IEnumerable<int> idElementos, ContextoSe contexto, IEnumerable<string> agruparPor)
        {
            var ids   = idElementos.ToList();
            var props = new HashSet<string>(agruparPor, StringComparer.OrdinalIgnoreCase);

            var necesitaExp = props.Contains(nameof(ReferenciaExpediente)) || props.Contains(nameof(NombreExpediente));
            var necesitaPpt = props.Contains(nameof(ReferenciaPpt))        || props.Contains(nameof(NombrePpt));

            var cacheExp = necesitaExp
                ? contexto.Set<TareasDeUnExpedienteDtm>()
                    .Where(v => ids.Contains(v.idElemento2))
                    .Join(contexto.Set<ExpedienteDtm>(), v => v.idElemento1, e => e.Id,
                          (v, e) => new { TareaId = v.idElemento2, e.Referencia, e.Nombre })
                    .ToList()
                    .GroupBy(x => x.TareaId)
                    .ToDictionary(g => g.Key, g => g.First())
                : null;

            var cachePpt = necesitaPpt
                ? contexto.Set<TareasDeUnPresupuestoDtm>()
                    .Where(v => ids.Contains(v.idElemento2))
                    .Join(contexto.Set<PresupuestoDtm>(), v => v.idElemento1, p => p.Id,
                          (v, p) => new { TareaId = v.idElemento2, p.Referencia, p.Nombre })
                    .ToList()
                    .GroupBy(x => x.TareaId)
                    .ToDictionary(g => g.Key, g => g.First())
                : null;

            return (prop, registro) =>
            {
                var id = registro.Id;
                if (prop.Equals(nameof(ReferenciaExpediente), StringComparison.OrdinalIgnoreCase) && cacheExp != null)
                    return cacheExp.TryGetValue(id, out var e1) ? e1.Referencia ?? "—" : "—";
                if (prop.Equals(nameof(NombreExpediente), StringComparison.OrdinalIgnoreCase) && cacheExp != null)
                    return cacheExp.TryGetValue(id, out var e2) ? e2.Nombre ?? "—" : "—";
                if (prop.Equals(nameof(ReferenciaPpt), StringComparison.OrdinalIgnoreCase) && cachePpt != null)
                    return cachePpt.TryGetValue(id, out var p1) ? p1.Referencia ?? "—" : "—";
                if (prop.Equals(nameof(NombrePpt), StringComparison.OrdinalIgnoreCase) && cachePpt != null)
                    return cachePpt.TryGetValue(id, out var p2) ? p2.Nombre ?? "—" : "—";
                return null;
            };
        }
    }
}
