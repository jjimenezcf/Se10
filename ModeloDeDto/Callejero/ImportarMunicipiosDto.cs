using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Callejero
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarMunicipiosDto
    {
        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Cómo preparar el Excel",
            Ayuda = "Explicación del proceso de importación y de las columnas que debe tener el fichero",
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.AreaDeTexto,
            NumeroDeFilas = 8,
            Obligatorio = false,
            EditableAlCrear = false,
            AreaInformativa = true,
            Fila = 0,
            Columna = 0,
            AutoSpan = true,
            ValorPorDefecto =
@"CÓMO FUNCIONA LA IMPORTACIÓN

Se sube el fichero Excel (.xlsx) oficial del INE ""Relación de municipios y códigos por provincias"" (una hoja por provincia). Por cada fila de datos de cada hoja se crea o actualiza un Municipio. Se ejecuta como un trabajo en segundo plano: se le avisará cuando termine y podrá consultar el detalle en la traza del trabajo.

Las filas con errores se descartan (no se crean ni actualizan), pero no detienen la importación del resto del catálogo. Las hojas sin las columnas esperadas (p.ej. una portada) se ignoran.

La cabecera puede estar en cualquier fila de cada hoja (se localiza automáticamente buscando el nombre de cada columna) y las columnas pueden ir en cualquier orden.

COLUMNAS OBLIGATORIAS (nombres tal cual los usa el INE)
- CPRO: código de provincia (2 dígitos). Debe existir ya en el callejero (Provincia.Codigo).
- CMUN: código del municipio dentro de la provincia (3 dígitos).
- DC: dígito de control del municipio (1 dígito).
- NOMBRE: nombre del municipio.

El DC que se guarda en el Municipio es la concatenación de CMUN + DC (p.ej. CMUN=043 y DC=4 en Yecla da como resultado '0434').

Si la provincia (CPRO) no existe, la fila se descarta indicando el motivo. Si el municipio ya existe en esa provincia, se actualiza su DC cuando sea distinto del calculado; si ya coincide no se hace nada. Si el municipio no existe, se crea.

FILTRO POR PROVINCIA
Si se indica una provincia en este formulario, se importarán solo las filas (de cualquier hoja) de esa provincia; el resto se ignoran sin contabilizarse como error.")]
        public string Instrucciones { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Catálogo",
            Ayuda = "Seleccione el fichero Excel (.xlsx) con el catálogo de municipios a importar",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".xlsx",
            Fila = 1,
            Columna = 0,
            AutoSpan = true)]
        public int IdArchivo { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Provincia",
            Ayuda = "Si se indica, sólo se importarán del catálogo los municipios de esta provincia",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ProvinciaDto),
            GuardarEn = nameof(IdProvincia),
            Controlador = nameof(enumControladoresCallejero.Provincias),
            VistaDondeNavegar = enumVistasCallejero.CrudProvincias,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            LongitudMinimaParaBuscar = 1,
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = true)]
        public string Provincia { get; set; }

        [IUPropiedad(Etiqueta = "Id de la provincia", Visible = false)]
        public int? IdProvincia { get; set; }
    }
}
