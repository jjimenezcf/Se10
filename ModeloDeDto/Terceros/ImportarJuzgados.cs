using ModeloDeDto.Callejero;
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarJuzgados
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
            Fila = 0,
            Columna = 0,
            AutoSpan = true,
            ValorPorDefecto =
@"CÓMO FUNCIONA LA IMPORTACIÓN

Se sube un fichero Excel (.xlsx) con un listado de juzgados. Por cada fila de datos se crea un Juzgado. Se ejecuta como un trabajo en segundo plano: se le avisará cuando termine y podrá consultar el detalle en la traza del trabajo.

Las filas con errores se descartan (no se crean), pero no detienen la importación del resto del catálogo.

La cabecera puede estar en cualquier fila (se localiza automáticamente buscando el nombre de cada columna) y las columnas pueden ir en cualquier orden.

COLUMNAS OBLIGATORIAS
- Clase: nombre de la clase de juzgado (p.ej. ""Juzgados de Primera Instancia e Instrucción""). Si no existe ya una clase con ese nombre, se crea automáticamente.
- Provincia: debe existir ya en el callejero.
- Municipio: debe existir ya en el callejero, dentro de la provincia indicada.
- Calificador: texto corto (máx. 20 caracteres) que distingue al juzgado dentro de su clase y municipio (p.ej. ""Nº 3"").

Si la provincia o el municipio no existen, la fila se descarta indicando el motivo. Si ya existe un juzgado con la misma Clase, Calificador y Municipio, la fila se descarta por duplicado.

FILTRO POR PROVINCIA
Si se indica una provincia en este formulario, se importarán solo las filas del Excel de esa provincia; el resto se ignoran sin contabilizarse como error.")]
        public string Instrucciones { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Catálogo",
            Ayuda = "Seleccione el fichero Excel (.xlsx) con el catálogo de juzgados a importar",
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
            Ayuda = "Si se indica, sólo se importarán del catálogo los juzgados de esta provincia",
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
