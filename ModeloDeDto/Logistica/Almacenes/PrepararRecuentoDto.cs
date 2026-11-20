using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Datos que se piden en la modal "Preparar recuento" del inventario (ver InventarioController.epPrepararRecuento)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Preparar recuento")]
    public class PrepararRecuentoDto
    {
        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Cómo funciona",
            Ayuda = "Explicación del proceso y del Excel que se genera",
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.AreaDeTexto,
            NumeroDeFilas = 6,
            Obligatorio = false,
            EditableAlCrear = false,
            AreaInformativa = true,
            Fila = 0,
            Columna = 0,
            AutoSpan = true,
            ValorPorDefecto =
@"PREPARAR UN RECUENTO

Se genera un Excel con el inventario del almacén a día de hoy para que el operario cuente lo que hay físicamente. Se ejecuta como un trabajo en segundo plano: el Excel se le envía por correo, se anexa al almacén y se anota una observación en el almacén con el enlace al trabajo.

Sólo se pueden rellenar las columnas Cantidad (lo contado) y Precio (propuesto con el valor medio, para ajustarlo); el resto está bloqueado. Con el Excel relleno se puede importar el recuento en una regularización.

Si la clase es 'Ajuste de precio', la Cantidad sale bloqueada con el stock de hoy y sólo se puede cambiar el Precio.

Los filtros por referencia y por nombre son opcionales y buscan por contenido. Admiten: ';' para separar varias alternativas, '%' para cualquier cadena de caracteres y '_' para un solo carácter (p.e. Mat%12_;Mat%1_50). Con '=' al inicio la coincidencia ha de ser exacta, y con '|' al inicio o al final, el texto ha de empezar o terminar así.")]
        public string Instrucciones { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del almacén", Visible = false)]
        public int IdAlmacen { get; set; }

        [IUPropiedad(
            Etiqueta = "Almacén",
            Ayuda = "Almacén del que se prepara el recuento",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdAlmacen),
            Controlador = nameof(enumControladoresLogistica.Almacenes),
            SeleccionarDe = typeof(AlmacenDto),
            VistaDondeNavegar = enumVistasLogisticas.CrudAlmacenes,
            BuscarPor = nameof(AlmacenDto.Expresion),
            MostrarExpresion = nameof(AlmacenDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            Negocio = enumNegocio.Almacen,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = true,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlCrear = true)]
        public string Almacen { get; set; }

        //------------------------------------------------------------------------
        // Para qué clase de regularización se prepara el Excel; con un ajuste de precios la cantidad sale bloqueada con el stock de hoy
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Clase de regularización para la que se prepara el Excel",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumRegularizacionAlm),
            ValorPorDefecto = nameof(enumRegularizacionAlm.Recuento),
            Obligatorio = true,
            EditableAlCrear = true,
            Fila = 1,
            Columna = 1,
            AutoSpan = true)]
        public string Clase { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Referencia",
            Ayuda = "Referencia de los materiales (opcional, ver instrucciones)",
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 2,
            Columna = 0,
            AutoSpan = true)]
        public string Referencia { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "Nombre de los materiales (opcional, ver instrucciones)",
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            AutoSpan = true)]
        public string Nombre { get; set; }
    }
}
