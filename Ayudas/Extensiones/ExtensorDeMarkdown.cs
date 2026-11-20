using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Utilidades
{
    // Convierte un markdown (.md) en un documento html para mostrarlo en el visor de archivos
    public static class ExtensorDeMarkdown
    {
        // - el html que lleve el markdown se escapa (DisableHtml): lo que se muestra es solo lo que genera Markdig
        // - los identificadores de los títulos siguen la regla de GitHub, para que funcionen los índices con enlaces "#..."
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseAdvancedExtensions()
            .DisableHtml()
            .Build();

        // nombreDeDescarga: si se indica, la página lleva arriba un enlace para descargar el .md original. El fichero va
        // incrustado en el propio enlace (data:), así descargarlo no hace una segunda petición al servidor (los enlaces
        // de descarga con guid pueden ser de un solo uso)
        public static string ToHtml(string rutaDelMd, string nombreDeDescarga = null)
        {
            var markdown = File.ReadAllText(rutaDelMd);
            var documento = Markdown.Parse(markdown, Pipeline);
            QuitarEnlacesConCodigo(documento);
            var cuerpo = documento.ToHtml(Pipeline);
            var titulo = WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(nombreDeDescarga ?? rutaDelMd));
            if (!string.IsNullOrEmpty(nombreDeDescarga))
            {
                var nombre = WebUtility.HtmlEncode(nombreDeDescarga);
                var contenido = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(markdown));
                cuerpo = $"""<div class="descarga"><a download="{nombre}" href="data:text/markdown;charset=utf-8;base64,{contenido}">Descargar {nombre}</a></div>""" + cuerpo;
            }

            return $$"""
                <!DOCTYPE html>
                <html lang="es">
                <head>
                <meta charset="utf-8">
                <title>{{titulo}}</title>
                <style>
                {{Estilos}}
                </style>
                </head>
                <body class="markdown">
                {{cuerpo}}
                </body>
                </html>
                """;
        }

        // un enlace javascript:, vbscript: o data: se ejecutaría al pulsarlo en el visor
        private static void QuitarEnlacesConCodigo(MarkdownDocument documento)
        {
            foreach (var enlace in documento.Descendants<LinkInline>())
            {
                var url = new string((enlace.Url ?? "").Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c)).ToArray());
                if (url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && !(enlace.IsImage && url.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("data:image/svg", StringComparison.OrdinalIgnoreCase)))
                    enlace.Url = "#";
            }
        }

        // el visor añade al body "white-space: pre-wrap; font-family: monospace" para los textos: aquí se anula con !important
        private const string Estilos = """
            body.markdown { white-space: normal !important; font-family: -apple-system, "Segoe UI", Roboto, Helvetica, Arial, sans-serif !important;
                            font-size: 14px; line-height: 1.6; color: #1f2328; background: #fff; max-width: 980px; margin: 0 auto; padding: 16px 24px; }
            .markdown h1, .markdown h2 { border-bottom: 1px solid #d1d9e0; padding-bottom: .3em; }
            .markdown h1, .markdown h2, .markdown h3, .markdown h4 { margin: 24px 0 12px; line-height: 1.25; font-weight: 600; }
            .markdown h1 { font-size: 2em; } .markdown h2 { font-size: 1.5em; } .markdown h3 { font-size: 1.25em; }
            .markdown a { color: #0969da; text-decoration: none; } .markdown a:hover { text-decoration: underline; }
            .markdown code { font-family: Consolas, "Courier New", monospace; font-size: 85%; background: #eff1f3; padding: .2em .4em; border-radius: 6px; }
            .markdown pre { background: #f6f8fa; padding: 12px 16px; border-radius: 6px; overflow: auto; line-height: 1.45; white-space: pre; }
            .markdown pre code { background: none; padding: 0; font-size: 90%; }
            .markdown table { border-collapse: collapse; margin: 12px 0; display: block; overflow: auto; }
            .markdown th, .markdown td { border: 1px solid #d1d9e0; padding: 6px 13px; vertical-align: top; }
            .markdown th { background: #f6f8fa; font-weight: 600; } .markdown tr:nth-child(2n) td { background: #f6f8fa; }
            .markdown blockquote { margin: 0; padding: 0 1em; color: #59636e; border-left: .25em solid #d1d9e0; }
            .markdown hr { border: 0; border-top: 1px solid #d1d9e0; margin: 24px 0; }
            .markdown img { max-width: 100%; }
            .markdown .descarga { text-align: right; margin-bottom: 8px; }
            .markdown .descarga a { display: inline-block; padding: 5px 14px; border: 1px solid #d1d9e0; border-radius: 6px; background: #f6f8fa; color: #1f2328; font-weight: 600; }
            .markdown .descarga a:hover { background: #eef1f4; text-decoration: none; }
            @media print { .markdown .descarga { display: none; } }
            """;
    }
}
