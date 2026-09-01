using ModeloDeDto.Callejero;
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarCatalogoDeJuzgadosDto
    {
        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Catálogo",
            Ayuda = "Seleccione el fichero Excel (.xlsx) con el catálogo de juzgados a importar",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".xlsx",
            Fila = 0,
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
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = true)]
        public string Provincia { get; set; }

        [IUPropiedad(Etiqueta = "Id de la provincia", Visible = false)]
        public int? IdProvincia { get; set; }
    }
}
