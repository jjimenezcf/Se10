using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Datos que se piden en la modal "Importar recuento" de la edición de una regularización (ver RegularizacionesController.epImportarRecuento)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Importar recuento")]
    public class RecuentoDto
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

Se sube un fichero Excel (.xlsx) con lo contado en el almacén. Por cada fila de datos se crea una línea en la regularización que se está editando. Se ejecuta como un trabajo en segundo plano: se le avisará cuando termine y podrá consultar el detalle en la traza del trabajo.

La regularización ha de estar recontando y las líneas se numeran de 10 en 10 a continuación de las que ya tenga. Todas las líneas importadas llevan la fecha del recuento que se indica en la modal (no puede ser posterior a hoy ni anterior al último cierre contable): al aplicar el recuento, los movimientos de almacén se generan a esa fecha.

Las filas con errores se descartan (no se crean), pero no detienen la importación del resto del fichero. Es un error que la referencia no exista, que no sea de un material, que ya esté en el recuento, o que la cantidad o el precio no sean números válidos.

La cabecera puede estar en cualquier fila (se localiza automáticamente buscando el nombre de cada columna) y las columnas pueden ir en cualquier orden.

COLUMNAS OBLIGATORIAS
- Referencia unitario: referencia del material
- Cantidad: unidades contadas
- Precio: precio unitario

COLUMNAS OPCIONALES (se ignoran)
- Nombre
- Total")]
        public string Instrucciones { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Recuento",
            Ayuda = "Seleccione el fichero Excel (.xlsx) con el recuento a importar",
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
            Etiqueta = "Fecha del recuento",
            Ayuda = "Día en el que se hizo el recuento: es la fecha de todas las líneas que se importan",
            Tipo = typeof(DateTime),
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Formato = enumFormato.Fecha,
            Obligatorio = true,
            Fila = 2,
            Columna = 0,
            AutoSpan = true)]
        public DateTime Fecha { get; set; }
    }
}
