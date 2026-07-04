using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

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

            return campoNorm switch
            {
                "enjornadas" or "jornadas" or "jornada"  => plf.EnJornadas(),
                "enhoras"    or "horas"    or "hora"     => plf.EnHoras(),
                "endias"     or "dias"     or "dia"      => plf.EnDias(),
                "enminutos"  or "minutos"  or "minuto"   => plf.EnMinutos(),
                _                                         => null
            };
        }

        // Firma compatible con ResolverEtiquetaDelegate
        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
        {
            if (id == null) return "—";
            if (!int.TryParse(id.ToString(), out var idInt)) return id.ToString();

            return prop.ToLowerInvariant() switch
            {
                "idresponsable" =>
                    contexto.Set<UsuarioDtm>()
                        .Where(u => u.Id == idInt)
                        .Select(u => u.Nombre + " " + u.Apellido)
                        .FirstOrDefault() ?? id.ToString(),

                "idsolicitante" =>
                    contexto.Set<InterlocutorDtm>()
                        .Where(i => i.Id == idInt)
                        .Select(i => i.Nombre)
                        .FirstOrDefault() ?? id.ToString(),

                "idtipo"   => tipos  .FirstOrDefault(t => t.Id == idInt)?.Nombre ?? id.ToString(),
                "idestado" => estados.FirstOrDefault(e => e.Id == idInt)?.Nombre ?? id.ToString(),
                "idcg"     => cgs    .FirstOrDefault(c => c.Id == idInt)?.Nombre ?? id.ToString(),

                _ => id.ToString()
            };
        }
    }
}
