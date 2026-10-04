using ModeloDeDto.Terceros;
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarTarifaDto
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
@"CÓMO FUNCIONA LA IMPORTACIÓN DE TARIFAS

Se sube el Excel (.xlsx) generado con la exportación ""Tarifa para proveedor"" una vez que el proveedor lo ha rellenado. Por cada fila se carga la tarifa del proveedor seleccionado para el unitario indicado; si el unitario ya tenía tarifa de ese proveedor, se actualiza. Se ejecuta como un trabajo en segundo plano: se le avisará cuando termine y podrá consultar el detalle y los errores en el log del trabajo.

Las filas con errores se anotan en el log y no detienen la importación del resto.

La cabecera puede estar en cualquier fila (se localiza buscando el nombre de cada columna) y las columnas pueden ir en cualquier orden. No cambie los títulos de las columnas.

COLUMNAS OBLIGATORIAS
- Mi Referencia: referencia del unitario (si no existe, se anota el error)
- Su Referencia: referencia del material para el proveedor (si está vacía, se anota el error)
- Tarifa: precio de tarifa del proveedor (si está vacío o no es un número mayor que 0, se anota el error)

COLUMNAS INFORMATIVAS (no se leen)
- Nombre, Descripción y Unidad")]
        public string Instrucciones { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del proveedor", Visible = false)]
        public int IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Seleccione el proveedor del que se importa la tarifa",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            BuscarPor = nameof(ProveedorDto.Expresion),
            MostrarExpresion = nameof(ProveedorDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = true,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlCrear = true,
            AutoSpan = true)]
        public string Proveedor { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Fichero",
            Ayuda = "Seleccione el fichero Excel (.xlsx) con la tarifa rellenada por el proveedor",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".xlsx",
            Fila = 2,
            Columna = 0,
            AutoSpan = true)]
        public int IdArchivo { get; set; }
    }
}
