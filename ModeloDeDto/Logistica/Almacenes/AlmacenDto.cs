using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using Utilidades;

namespace ModeloDeDto.Logistica
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class AlmacenDto : ElementoDeUnProcesoDto
    {

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Direcciones",
           EtiquetaGrid = "Direcciones",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnEdicion = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15
            )
        ]
        public string Direcciones { get; set; }

        //------------------------------------------------------------------------------------------- información contable (filas 2 a 4)
        // sin cuenta de existencias está sin definir y no se pueden preasentar sus movimientos (ver ExtensorDeAlmacenes.ValidarInformacionContable)
        [IUPropiedad(
            Etiqueta = "Usa variación de existencias",
            Ayuda = "Las compras van a la cuenta de compras (o a la de gasto de la naturaleza) y las existencias se cuadran contra la de variación en cada cierre contable; si no, cada movimiento va a la cuenta de existencias",
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.CheckApilado,
            VisibleEnGrid = false,
            Obligatorio = false,
            // sin variación de existencias no se usan las cuentas de compras ni de devoluciones: se blanquean y bloquean
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Alm_AlCambiar_UsaVariacion) + "(this)",
            Fila = 2,
            Columna = 0
            )
        ]
        public bool UsaVariacionDeExistencias { get; set; }

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la cuenta de existencias", Visible = false)]
        public int? IdCuentaDeExistencias { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de existencias",
            Ayuda = "Cuenta del grupo 30, 31 o 32 del almacén; mientras no se indique no se pueden preasentar sus movimientos",
            AyudaDeCriteriosDeBusqueda = "Grupo 30, 31 o 32 ...",
            RestrictorFijo = ltrUsoDeCuenta.RestrictorDeExistencias,
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            VistaDondeNavegar = enumVistasContables.CrudCuentas,
            BuscarPor = nameof(CuentaDto.Expresion),
            MostrarExpresion = nameof(CuentaDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            OrdenarListaDinamicaPor = nameof(CuentaDto.Codigo),
            LongitudMinimaParaBuscar = 1,
            GuardarEn = nameof(IdCuentaDeExistencias),
            VisibleEnGrid = false,
            Obligatorio = false,
            AutoSpan = true,
            Fila = 3,
            Columna = 0
            )
        ]
        public string CuentaDeExistencias { get; set; }

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la cuenta de variación de existencias", Visible = false)]
        public int? IdCuentaDeVariacion { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de variación",
            Ayuda = "Cuenta de variación de existencias (610, 611 o 612 según la de existencias); si no se indica se deduce de la de existencias y se crea si no existe",
            AyudaDeCriteriosDeBusqueda = "61x, vacía se deduce ...",
            RestrictorFijo = ltrUsoDeCuenta.RestrictorDeVariacion,
            RestringidoPorControl = nameof(CuentaDeExistencias),
            PropiedadRestrictora = nameof(IdCuentaDeExistencias),
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            VistaDondeNavegar = enumVistasContables.CrudCuentas,
            BuscarPor = nameof(CuentaDto.Expresion),
            MostrarExpresion = nameof(CuentaDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            OrdenarListaDinamicaPor = nameof(CuentaDto.Codigo),
            LongitudMinimaParaBuscar = 1,
            GuardarEn = nameof(IdCuentaDeVariacion),
            VisibleEnGrid = false,
            Obligatorio = false,
            AutoSpan = true,
            Fila = 3,
            Columna = 1
            )
        ]
        public string CuentaDeVariacion { get; set; }

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la cuenta de compras", Visible = false)]
        public int? IdCuentaDeCompras { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de compras",
            Ayuda = "Con variación de existencias, cuenta de las compras del almacén (600, 601 o 602 según la de existencias); si no se indica se usa la de gasto de la naturaleza",
            AyudaDeCriteriosDeBusqueda = "60x, vacía la de la naturaleza ...",
            RestrictorFijo = ltrUsoDeCuenta.RestrictorDeCompras,
            RestringidoPorControl = nameof(CuentaDeExistencias),
            PropiedadRestrictora = nameof(IdCuentaDeExistencias),
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            VistaDondeNavegar = enumVistasContables.CrudCuentas,
            BuscarPor = nameof(CuentaDto.Expresion),
            MostrarExpresion = nameof(CuentaDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            OrdenarListaDinamicaPor = nameof(CuentaDto.Codigo),
            LongitudMinimaParaBuscar = 1,
            GuardarEn = nameof(IdCuentaDeCompras),
            VisibleEnGrid = false,
            Obligatorio = false,
            AutoSpan = true,
            Fila = 4,
            Columna = 0
            )
        ]
        public string CuentaDeCompras { get; set; }

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la cuenta de devoluciones", Visible = false)]
        public int? IdCuentaDeDevoluciones { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de devoluciones",
            Ayuda = "Con variación de existencias, cuenta de las devoluciones a proveedor (608); si no se indica se usa la de gasto de la naturaleza",
            AyudaDeCriteriosDeBusqueda = "608, vacía la de la naturaleza ...",
            RestrictorFijo = ltrUsoDeCuenta.RestrictorDeDevoluciones,
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            VistaDondeNavegar = enumVistasContables.CrudCuentas,
            BuscarPor = nameof(CuentaDto.Expresion),
            MostrarExpresion = nameof(CuentaDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            OrdenarListaDinamicaPor = nameof(CuentaDto.Codigo),
            LongitudMinimaParaBuscar = 1,
            GuardarEn = nameof(IdCuentaDeDevoluciones),
            VisibleEnGrid = false,
            Obligatorio = false,
            AutoSpan = true,
            Fila = 4,
            Columna = 1
            )
        ]
        public string CuentaDeDevoluciones { get; set; }

    }


}
