using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MVCSistemaDeElementos.UtilidadesIu
{
    /// <summary>
    /// Manuales de usuario de las vistas (wwwroot/ayuda/manuales). Ver wwwroot/ayuda/manuales/LEEME.md
    ///
    ///   - Específico de una vista: wwwroot/ayuda/manuales/{módulo funcional}/{controlador}/{vista}.md
    ///     (p. ej. Venta/FacturasEmt/CrudFacturasEmt.md); también se fusionan los {vista}_*.md de esa carpeta, ordenados por nombre.
    ///     El módulo funcional no hace falta conocerlo: se busca la carpeta del controlador en cualquier módulo.
    ///   - Común a todos los CRUD: los wwwroot/ayuda/manuales/Crud*.md, ordenados por nombre (mantenimiento, creación, edición...).
    ///
    /// El manual de una vista es el específico seguido de los comunes. Sirve para mostrarlo con el ? del pie de página
    /// y para dárselo a la IA como contexto para que responda a ¿cómo hago ...?
    /// </summary>
    public static class ApiDeManuales
    {
        public const string CarpetaDeAyuda = "ayuda";
        public const string CarpetaDeManuales = "manuales";
        public const string PrefijoDeLosComunes = "Crud";

        /// <summary>
        /// Vista con la que se pide el manual de las páginas de consulta por guid (acción Consultar de EntidadController), que se abren
        /// sin usuario. Todas comparten el manual Consulta_PorGuid.md (no empieza por Crud, así no se mezcla con los de los crud).
        /// </summary>
        public const string VistaDeConsultaPorGuid = "Consultar";
        public const string ManualDeConsultaPorGuid = "Consulta_PorGuid.md";

        public static bool EsLaVistaDeConsultaPorGuid(string vista) => string.Equals(vista, VistaDeConsultaPorGuid, StringComparison.OrdinalIgnoreCase);

        public static string RutaDeManuales(string webRootPath) => Path.Combine(webRootPath, CarpetaDeAyuda, CarpetaDeManuales);

        /// <summary>Ficheros del manual específico de la vista (vacío si aún no tiene)</summary>
        public static List<string> FicherosEspecificos(string webRootPath, string controlador, string vista)
        {
            var raiz = RutaDeManuales(webRootPath);
            if (!Directory.Exists(raiz) || string.IsNullOrWhiteSpace(controlador))
                return new List<string>();

            // sólo en el segundo nivel ({módulo}/{controlador}): un controlador puede llamarse como un módulo (p. ej. Negocio/Negocio)
            var carpeta = Directory.GetDirectories(raiz)
                .Select(modulo => Path.Combine(modulo, controlador))
                .FirstOrDefault(Directory.Exists);
            if (carpeta == null)
                return new List<string>();

            return Directory.GetFiles(carpeta, "*.md")
                .Where(f =>
                {
                    var nombre = Path.GetFileNameWithoutExtension(f);
                    return string.IsNullOrWhiteSpace(vista)
                        || nombre.Equals(vista, StringComparison.OrdinalIgnoreCase)
                        || nombre.StartsWith($"{vista}_", StringComparison.OrdinalIgnoreCase);
                })
                // primero el {vista}.md y luego sus anexos {vista}_*.md
                .OrderBy(f => Path.GetFileNameWithoutExtension(f).Length == (vista ?? "").Length ? 0 : 1)
                .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Ficheros del manual común a todos los CRUD (Crud*.md en la raíz de manuales)</summary>
        public static List<string> FicherosComunes(string webRootPath)
        {
            var raiz = RutaDeManuales(webRootPath);
            if (!Directory.Exists(raiz))
                return new List<string>();

            return Directory.GetFiles(raiz, $"{PrefijoDeLosComunes}*.md", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Manual de la vista en markdown: el específico seguido de los comunes. Si la vista aún no tiene manual propio,
        /// se avisa al principio y se devuelven sólo los comunes. Devuelve null si no hay ningún documento.
        /// Los comunes describen los crud (mantenimiento, creación, edición): sólo se añaden a las vistas Crud...; una página que no sigue
        /// el estándar (p. ej. el panel de control, vista PanelDeControl de Home) sólo muestra su propio manual.
        /// </summary>
        public static string ManualDeLaVista(string webRootPath, string controlador, string vista, string titulo)
        {
            var especificos = FicherosEspecificos(webRootPath, controlador, vista);
            var esUnCrud = (vista ?? "").StartsWith(PrefijoDeLosComunes, StringComparison.OrdinalIgnoreCase);
            var esConsultaPorGuid = EsLaVistaDeConsultaPorGuid(vista);
            var comunes = esUnCrud ? FicherosComunes(webRootPath) : new List<string>();
            if (esConsultaPorGuid)
            {
                var deConsulta = Path.Combine(RutaDeManuales(webRootPath), ManualDeConsultaPorGuid);
                if (File.Exists(deConsulta))
                    comunes.Add(deConsulta);
            }
            if (especificos.Count == 0 && comunes.Count == 0)
                return null;

            var partes = new List<string>();
            if (especificos.Count > 0)
                partes.AddRange(especificos.Select(f => File.ReadAllText(f).Trim()));
            else if (!esConsultaPorGuid)
                partes.Add($"# {(string.IsNullOrWhiteSpace(titulo) ? vista : titulo)}\n\n> Esta vista aún no tiene manual propio " +
                           $"(ayuda/manuales/[módulo]/{controlador}/{vista}.md). A continuación se describe el funcionamiento común a todas las vistas.");

            if (comunes.Count > 0)
            {
                if (!esConsultaPorGuid)
                    partes.Add("# Funcionamiento común de las vistas");
                partes.AddRange(comunes.Select(f => File.ReadAllText(f).Trim()));
            }

            return string.Join("\n\n---\n\n", partes);
        }
    }
}
