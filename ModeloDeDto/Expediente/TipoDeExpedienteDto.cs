using Utilidades;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Expediente;

namespace ModeloDeDto.Expediente
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeExpedienteDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia del expediente",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Expediente,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Estado { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase del expediente",
            Ayuda = "indique la clase del expediente",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeExpediente),
            GuardarEn = nameof(ClaseDeExpediente),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public string ClaseDeExpediente { get; set; }
 
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Interventor",
            Ayuda = "Permiso de intervención (permite cambiar la fecha de entrega)",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeInterventor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public int IdPermisoDeInterventor { get; set; }
        [IUPropiedad(Visible = false)]
        public string PermisoDeInterventor { get; set; }


        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usa tareas",
            Ayuda = "indica si el expediente usa tareas",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool UsaTareas { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usa presupuestos",
            Ayuda = "indica si el expediente usa presupuestos",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool UsaPpts { get; set; }


        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contratos de venta",
            Ayuda = "Expediente de ventas",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool ScDeVenta { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contratos de compra",
            Ayuda = "Expedientes de compras",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 3,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool ScDeCompra{ get; set; }

        //------------------------------------------------------------------------------------------- cuentas de gasto de los materiales en la obra
        // sólo para las naturalezas de clase Material; si no se indican se usa la de gasto de la naturaleza
        [IUPropiedad(Etiqueta = "Id de la cuenta de consumo", Visible = false)]
        public int? IdCuentaDeConsumo { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de consumo",
            Ayuda = "Cuenta de gasto (grupo 60) de los materiales que salen de un almacén a la obra y de los que vuelven de ella; si no se indica, la de gasto de la naturaleza",
            AyudaDeCriteriosDeBusqueda = "60x, vacía la de la naturaleza ...",
            RestrictorFijo = ltrUsoDeCuenta.RestrictorDeGastoDeMateriales,
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            VistaDondeNavegar = enumVistasContables.CrudCuentas,
            BuscarPor = nameof(CuentaDto.Expresion),
            MostrarExpresion = nameof(CuentaDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            OrdenarListaDinamicaPor = nameof(CuentaDto.Codigo),
            LongitudMinimaParaBuscar = 1,
            GuardarEn = nameof(IdCuentaDeConsumo),
            VisibleEnGrid = false,
            Obligatorio = false,
            AutoSpan = true,
            Fila = 9,
            Columna = 0
            )
        ]
        public string CuentaDeConsumo { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la cuenta de compra directa", Visible = false)]
        public int? IdCuentaDeCompraDirecta { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de compra directa",
            Ayuda = "Cuenta de gasto (grupo 60) de los materiales que el proveedor entrega directamente en la obra y de sus devoluciones; si no se indica, la de gasto de la naturaleza",
            AyudaDeCriteriosDeBusqueda = "60x, vacía la de la naturaleza ...",
            RestrictorFijo = ltrUsoDeCuenta.RestrictorDeGastoDeMateriales,
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            VistaDondeNavegar = enumVistasContables.CrudCuentas,
            BuscarPor = nameof(CuentaDto.Expresion),
            MostrarExpresion = nameof(CuentaDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            OrdenarListaDinamicaPor = nameof(CuentaDto.Codigo),
            LongitudMinimaParaBuscar = 1,
            GuardarEn = nameof(IdCuentaDeCompraDirecta),
            VisibleEnGrid = false,
            Obligatorio = false,
            AutoSpan = true,
            Fila = 9,
            Columna = 1
            )
        ]
        public string CuentaDeCompraDirecta { get; set; }
    }
}
