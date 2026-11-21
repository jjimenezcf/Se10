using ServicioDeDatos.Juridico;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    // Ampliación de sólo consulta de la edición de una factura emitida, bloque "Enlaces" debajo del detalle: los documentos con los que está
    // relacionada, para navegar a ellos. No tiene tabla, se calcula al leerla (ver EnlacesDeFaeController) y no se persiste nunca
    // (la ampliación se marca como SoloConsulta en DescriptorDeFacturasEmt)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class EnlacesDeFaeDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        // Fila 0: de dónde viene la factura
        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "presupuesto facturado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Presupuesto),
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos,
            Fila = 0,
            Columna = 0,
            EditableAlEditar = false,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdPresupuesto { get; set; }
        [IUPropiedad(Visible = false)]
        public string Presupuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Parte de trabajo",
            Ayuda = "parte de trabajo facturado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(ParteTr),
            Controlador = nameof(enumControladoresVentas.PartesTr),
            VistaDondeNavegar = enumVistasVentas.CrudPartesDeTrabajo,
            Fila = 0,
            Columna = 1,
            EditableAlEditar = false,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdParteTr { get; set; }
        [IUPropiedad(Visible = false)]
        public string ParteTr { get; set; }

        [IUPropiedad(
            Etiqueta = "Contrato",
            Ayuda = "contrato asociado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Contrato),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta),
            Fila = 0,
            Columna = 2,
            EditableAlEditar = false,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdContrato { get; set; }
        [IUPropiedad(Visible = false)]
        public string Contrato { get; set; }

        //--------------------------------------------
        // Fila 1: el preasiento contable y el documento de salida de material (documento de venta) que la factura prefactura
        [IUPropiedad(
            Etiqueta = "Preasiento",
            Ayuda = "preasiento contable",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Preasiento),
            Controlador = nameof(enumControladoresContables.Preasientos),
            VistaDondeNavegar = enumVistasContables.CrudPreasientos,
            Fila = 1,
            Columna = 0,
            EditableAlEditar = false,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdPreasiento { get; set; }
        [IUPropiedad(Visible = false)]
        public string Preasiento { get; set; }

        [IUPropiedad(
            Etiqueta = "Documento de salida",
            Ayuda = "documento de salida de material (documento de venta) facturado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Venta),
            Controlador = nameof(enumControladoresLogistica.Ventas),
            VistaDondeNavegar = enumVistasLogisticas.CrudVentas,
            Fila = 1,
            Columna = 1,
            EditableAlEditar = false,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdVenta { get; set; }
        [IUPropiedad(Visible = false)]
        public string Venta { get; set; }
    }
}
