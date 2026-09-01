using Utilidades;

namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarCatalogoDeUnitariosDto
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

Se sube un fichero Excel (.xlsx) con un catálogo de materiales. Por cada fila de datos se crea un Unitario de clase ""Material"". Se ejecuta como un trabajo en segundo plano: se le avisará cuando termine y podrá consultar el detalle en la traza del trabajo.

Las filas con errores se descartan (no se crean), pero no detienen la importación del resto del catálogo.

La cabecera puede estar en cualquier fila (se localiza automáticamente buscando el nombre de cada columna) y las columnas pueden ir en cualquier orden.

COLUMNAS OBLIGATORIAS
- Referencia: código único del material
- Nombre: nombre del material
- Unidad: sigla de la unidad de medida (debe existir ya)
- Sigla Naturaleza: sigla de la naturaleza contable
- Naturaleza: nombre de la naturaleza contable
- Coste: precio de coste
- Venta: precio de venta
- Cuenta de gasto: código de la cuenta contable de gasto
- Cuenta de ingreso: código de la cuenta contable de ingreso

COLUMNAS OPCIONALES
- Descripción
- Baja (SI/NO)

CÓMO SE RESUELVE LA NATURALEZA
1. Se busca por la Sigla Naturaleza indicada.
2. Si no existe, se busca por el Nombre. Si existe con otra sigla, la fila se descarta indicando que corrija la sigla en el Excel.
3. Si no existe ni por sigla ni por nombre, se crea una naturaleza nueva con ese nombre, esa sigla y las cuentas de gasto/ingreso indicadas (las cuentas deben existir ya en el plan contable).")]
        public string Instrucciones { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Catálogo",
            Ayuda = "Seleccione el fichero Excel (.xlsx) con el catálogo de materiales a importar",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".xlsx",
            Fila = 1,
            Columna = 0,
            AutoSpan = true)]
        public int IdArchivo { get; set; }
    }
}
