using DocumentFormat.OpenXml.Wordprocessing;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using DocumentFormat.OpenXml;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Utilidades;
using DocumentFormat.OpenXml.Packaging;
using ServicioDeDatos.Elemento;
using System.Reflection;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Negocio;

namespace ServicioDeReportes.Base
{
    public class TablaPlantillaDto
    {
        public string Nombre { get; set; } = "";
        public List<string> Encolumnado { get; set; } = new List<string>();

        public TablaPlantillaDto(Type tipoDto, enumEncabezadosDeTablas? nombre = null)
        {
            Nombre = nombre is null ? tipoDto.Name.Replace("Dto", "") : ((enumEncabezadosDeTablas)nombre).ToString();
            foreach (var propiedad in tipoDto.GetProperties())
            {
                if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true))
                    continue;
                if (propiedad.Name.StartsWith(nameof(IRegistro.Id)))
                    continue;
                if (propiedad.Name.Equals(nameof(ElementoDto.EstaCancelada)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.EstaTerminada)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.NombreModificable)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.informacion)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.ModoDeAcceso))
                    )
                    continue;
                if (tipoDto.HeredaDe(typeof(EsUnDetalleDto), incluirTipo: false))
                {
                    if (propiedad.Name.Equals(nameof(EsUnDetalleDto.Elemento)))
                        continue;
                }

                if (tipoDto.ImplementaAuditoriaDto())
                {
                    if (propiedad.Name.Equals(nameof(IAuditadoDto.Creador)) ||
                        propiedad.Name.Equals(nameof(IAuditadoDto.Modificador)) ||
                        propiedad.Name.Equals(nameof(IAuditadoDto.CreadoEl)) ||
                        propiedad.Name.Equals(nameof(IAuditadoDto.ModificadoEl))
                       )
                       continue;
                }

                if (tipoDto == typeof(HitoDto))
                {
                    if (propiedad.Name.Equals(nameof(HitoDto.Elemento)))
                        continue;
                    if (propiedad.Name.Equals(nameof(HitoDto.Negocio)))
                        continue;
                }
                if (tipoDto == typeof(ObservacionDto))
                {
                    if (propiedad.Name != nameof(ObservacionDto.Nombre) && propiedad.Name != nameof(ObservacionDto.Descripcion))
                        continue;
                }
                if (tipoDto == typeof(DireccionDto))
                {
                    if (propiedad.Name != nameof(DireccionDto.NombreDireccion) && propiedad.Name != nameof(DireccionDto.Calificador))
                        continue;
                }
                Encolumnado.Add(propiedad.Name);
            }
        }
    }

    public static class ApiDeEtiquetas
    {

        public static string CrearFicheroDeEtiquetas(Type tipoDtm)
        {
            if (!Path.Exists(enumRutas.RutaDePlantillas))
                Directory.CreateDirectory(enumRutas.RutaDePlantillas);

            string fichero = Path.Combine(enumRutas.RutaDePlantillas, $"Etiquetas de {tipoDtm.Name}.{enumExtensiones.docx}".NormalizarFichero());
            var etiquetas = ObtenerEtiquetas(tipoDtm);
            var tablasDto = DefinirTablasDto(tipoDtm);
            var etiquetasDeTablas = EtiquetasDeTablas(tipoDtm);
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(fichero, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                mainPart.Document.Body = new Body();

                NumberingDefinitionsPart numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>("nd");
                var nivel = new Level(new NumberingFormat() { Val = NumberFormatValues.Bullet }, new LevelText() { Val = "·" });
                nivel.LevelIndex = 0;
                var numeracionAbstracta = new AbstractNum(nivel);
                var numeracion = new NumberingInstance(new AbstractNumId() { Val = 0 }) { NumberID = 1 };
                Numbering element = new Numbering(numeracionAbstracta, numeracion);
                numberingPart.Numbering = element;

                foreach (var etiqueta in etiquetas) mainPart.Document.Body.AppendLabel(etiqueta);

                foreach (var tablaDto in tablasDto)
                {
                    mainPart.Document.Body.AppendLine();
                    var tabla = mainPart.Document.Body.AppendTable();
                    tabla.AppendKeyRow(tablaDto.Nombre, tablaDto.Encolumnado.Count);

                    TableRow filaDeEncabezado = new TableRow();
                    for (int i = 0; i < tablaDto.Encolumnado.Count; i++)
                        filaDeEncabezado.AppendCell(new Paragraph(new Run(new Text(tablaDto.Encolumnado[i]))));
                    tabla.Append(filaDeEncabezado);

                    TableRow filaDeDatos = new TableRow();
                    for (int i = 0; i < tablaDto.Encolumnado.Count; i++) filaDeDatos.Append(new TableCell(new Paragraph(new Run(new Text($"{Simbolos.PltInicio}{tablaDto.Encolumnado[i]}{Simbolos.PltCierre}")))));
                    tabla.Append(filaDeDatos);
                }

                foreach (var etiqueta in etiquetasDeTablas) mainPart.Document.Body.AppendLabel(etiqueta);
            }
            return fichero;
        }

        private static void AppendLabel(this Body cuerpo, string etiqueta)
        {
            var bloque = DefinirParrafo();
            bloque.parrafo.Append(bloque.propiedades);
            Run run = new Run();
            run.AppendChild(etiqueta.Contains("Etiquetas de") || etiqueta.Contains("--") || etiqueta.Contains(" ") || etiqueta.Equals(Environment.NewLine)
            ? new Text(etiqueta)
            : new Text($"{Simbolos.PltInicio}{etiqueta}{Simbolos.PltCierre}"));
            bloque.parrafo.Append(run);
            cuerpo.Append(bloque.parrafo);
        }

        private static (Paragraph parrafo, ParagraphProperties propiedades) DefinirParrafo()
        {
            var parrafo = new Paragraph();
            var propiedades = new ParagraphProperties(
                new NumberingProperties(
                    new NumberingLevelReference() { Val = 0 },
                    new NumberingId() { Val = 1 }
                )
            );
            return (parrafo, propiedades);
        }

        private static void AppendLine(this Body cuerpo)
        {
            Paragraph parrafo = new Paragraph();
            Run run = new Run();
            run.AppendChild(new Text(Environment.NewLine));
            parrafo.Append(run);
            cuerpo.Append(parrafo);
        }

        private static Table AppendTable(this Body cuerpo)
        {
            Table tabla = new Table();
            //TableProperties tableProperties = new TableProperties(new TableBorders(new TopBorder(), new BottomBorder(),
            //    new LeftBorder(), new RightBorder(), new InsideHorizontalBorder(), new InsideVerticalBorder()));
            //tabla.AppendChild(tableProperties);
            cuerpo.Append(tabla);
            return tabla;
        }

        private static TableRow AppendKeyRow(this Table tabla, string clave, int celadas)
        {
            TableRow fila = new TableRow();
            fila.AppendKeyCell(new Paragraph(new Run(new Text("{{{" + clave + "}}}"))));
            for (int i = 1; i < celadas; i++) fila.AppendKeyCell(new Paragraph(new Run(new Text(""))));
            tabla.Append(fila);
            return fila;
        }


        private static TableCell AppendKeyCell(this TableRow fila, Paragraph parrafo)
        {
            var celda = new TableCell();
            celda.ConBorde();
            celda.Append(parrafo);
            fila.Append(celda);
            return celda;
        }

        private static TableCell AppendCell(this TableRow fila, Paragraph parrafo)
        {
            var celda = new TableCell();
            celda.ConBorde();
            celda.Append(parrafo);
            fila.Append(celda);
            return celda;
        }

        private static void ConBorde(this TableCell celda)
        {
            var propiedadesDeLaCelada = new TableCellProperties();
            TableCellBorders bordesDeCelada = new TableCellBorders();
            BottomBorder borderInferior = new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Double), Color = "000000", Size = 12U };
            bordesDeCelada.Append(borderInferior);
            propiedadesDeLaCelada.Append(bordesDeCelada);
            celda.Append(propiedadesDeLaCelada);
        }

        private static List<string> ObtenerEtiquetas(Type tipoDtm)
        {
            var etiquetas = new List<string>
            {
                $"Etiquetas de: {tipoDtm.Name.Replace("Dtm", "")}",
                $"---------------------------------------------------"
            };

            IncluirEtiquetas(etiquetas, tipoDtm);

            var negocio = tipoDtm.NegocioDeUnDtm();
            var ampliaciones = negocio.TiposDeAmpliaciones();
            foreach (var ampliacion in ampliaciones)
            {
                var ampliacionDto = ampliacion.ToDto();
                etiquetas.Add(" ");
                etiquetas.Add($"Etiquetas de: {ampliacion.Name.Replace("Dtm", "")}");
                etiquetas.Add("---------------------------------------------------");
                IncluirEtiquetas(etiquetas, ampliacionDto);
            }

            return etiquetas;
        }

        private static List<string> EtiquetasDeTablas(Type tipoDtm)
        {
            var etiquetas = new List<string>
            {
                Environment.NewLine,
                "Otras etiquetas de para poder incluir en el encolumnado",
                "------------------------------------------------------------------------------"
            };

            var negocio = tipoDtm.NegocioDeUnDtm();
            var detalles = negocio.TiposDeDetalles();
            foreach (var detalle in detalles)
            {
                var detalleDto = detalle.ToDto();
                etiquetas.Add(" ");
                etiquetas.Add($"Etiquetas de: {detalle.Name.Replace("Dtm", "")}");
                etiquetas.Add("-- Tabla ----------------------------------");
                etiquetas.Add(detalle.Name.Replace("Dtm", ""));
                etiquetas.Add("-- Columnas--------------------------------");
                foreach (var propiedad in detalleDto.GetProperties())
                {
                    if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true)) continue;
                    etiquetas.Add(propiedad.Name);
                }
            }

            if (negocio.UsaFlujo())
            {
                etiquetas.Add(" ");
                etiquetas.Add($"Etiquetas de: {negocio.ObtenerMetadatos().HitosDtm.Name.Replace("Dtm", "")}");
                etiquetas.Add("-- Tabla ----------------------------------");
                etiquetas.Add(enumEncabezadosDeTablas.Hitos.ToString());
                etiquetas.Add("-- Columnas--------------------------------");
                foreach (var propiedad in typeof(HitoDto).GetProperties())
                {
                    if (propiedad.Name.StartsWith("Id") && (propiedad.PropertyType == typeof(int) || propiedad.PropertyType == typeof(int?)))
                        continue;
                    etiquetas.Add(propiedad.Name);
                }
            }

            if (negocio.UsaObservaciones())
            {
                etiquetas.Add(" ");
                etiquetas.Add($"Etiquetas de: {negocio.ObtenerMetadatos().ObservacionesDtm.Name.Replace("Dtm", "")}");
                etiquetas.Add("-- Tabla ----------------------------------");
                etiquetas.Add(enumEncabezadosDeTablas.Observaciones.ToString());
                etiquetas.Add("-- Columnas--------------------------------");
                foreach (var propiedad in typeof(ObservacionDto).GetProperties())
                {
                    if (propiedad.Name.StartsWith("Id") && (propiedad.PropertyType == typeof(int) || propiedad.PropertyType == typeof(int?)))
                        continue;
                    etiquetas.Add(propiedad.Name);
                }
            }

            if (negocio.UsaDirecciones())
            {
                etiquetas.Add(" ");
                etiquetas.Add($"Etiquetas de: {negocio.ObtenerMetadatos().DireccionesDtm.Name.Replace("Dtm", "")}");
                etiquetas.Add("-- Tabla ----------------------------------");
                etiquetas.Add(enumEncabezadosDeTablas.Direcciones.ToString());
                etiquetas.Add("-- Columnas--------------------------------");
                foreach (var propiedad in typeof(DireccionDto).GetProperties())
                {
                    if (propiedad.Name.StartsWith("Id") && (propiedad.PropertyType == typeof(int) || propiedad.PropertyType == typeof(int?)))
                        continue;
                    etiquetas.Add(propiedad.Name);
                }
            }

            return etiquetas;
        }

        private static void IncluirEtiquetas(List<string> etiquetas, Type tipoDtm)
        {
            var clave = tipoDtm.Name.Replace("Dtm", "");
            var tipoDto = tipoDtm.ToDto();
            var propiedades = tipoDto.GetProperties();
            foreach (PropertyInfo propiedad in propiedades)
            {
                //if (propiedad.PropertyType.HeredaDe(typeof(RegistroDtm), incluirTipo: true)) continue;
                etiquetas.Add(clave + "." + propiedad.Name);
            }
        }

        private static List<TablaPlantillaDto> DefinirTablasDto(Type tipoDtm)
        {
            var tablas = new List<TablaPlantillaDto>();
            var negocio = tipoDtm.NegocioDeUnDtm();
            var detalles = negocio.TiposDeDetalles();
            foreach (var detalle in detalles) tablas.Add(new TablaPlantillaDto(detalle.ToDto()));
            if (negocio.UsaFlujo()) tablas.Add(new TablaPlantillaDto(typeof(HitoDto), enumEncabezadosDeTablas.Hitos));
            if (negocio.UsaObservaciones()) tablas.Add(new TablaPlantillaDto(typeof(ObservacionDto), enumEncabezadosDeTablas.Observaciones));
            if (negocio.UsaDirecciones()) tablas.Add(new TablaPlantillaDto(typeof(DireccionDto), enumEncabezadosDeTablas.Direcciones));
            return tablas;
        }

    }
}
