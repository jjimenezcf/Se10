using System;
using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Logistica
{
    // Venta de material desde la consulta de inventario (opción "Venta de material", ver DescriptorDeInventario e Inventario.ts).
    // El almacén y el material son los de la fila seleccionada y sólo se muestran; se pide el cliente, la fecha (sin hora, la de los
    // movimientos de almacén), la cantidad y el precio (propuesto con el de venta del material); el total se calcula en el cliente.
    // Al aceptar se llama a VentasController.epCrearVenta
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Venta de material")]
    public class CrearVentaDto
    {
        //--------------------------------------------
        // Fila 0: lo que se vende y desde dónde, sólo se muestra
        [IUPropiedad(
            Etiqueta = "Almacén",
            Ayuda = "Almacén desde el que se vende",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Almacen),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Obligatorio = true,
            Controlador = nameof(enumControladoresLogistica.Almacenes),
            VistaDondeNavegar = enumVistasLogisticas.CrudAlmacenes
            )
        ]
        public int IdAlmacen { get; set; }

        [IUPropiedad(Etiqueta = "Almacén", Visible = false)]
        public string Almacen { get; set; }

        [IUPropiedad(
            Etiqueta = "Material",
            Ayuda = "Material que se vende",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Unitario),
            Fila = 0,
            Columna = 1,
            VisibleEnGrid = false,
            Obligatorio = true,
            Controlador = nameof(enumControladoresMt.Unitarios),
            VistaDondeNavegar = enumVistasMts.CrudUnitarios
            )
        ]
        public int IdUnitario { get; set; }

        [IUPropiedad(Etiqueta = "Material", Visible = false)]
        public string Unitario { get; set; }

        //--------------------------------------------
        // Fila 1: a quién se vende
        [IUPropiedad(
         Etiqueta = "Id del cliente",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién se le vende",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            SeleccionarDe = typeof(ClienteDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            EditableAlCrear = true,
            AutoSpan = true)]
        public string Cliente { get; set; }

        //--------------------------------------------
        // Fila 2: fecha, cantidad y precio de la venta
        [IUPropiedad(
              Etiqueta = "Fecha"
            , Ayuda = "Fecha de la venta y de la salida del almacén: no puede ser posterior a hoy ni anterior al último cierre contable"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Obligatorio = true
            , Fila = 2
            , Columna = 0)]
        public DateTime? Fecha { get; set; }

        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad que se vende, como mucho el stock del material en el almacén a la fecha",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularTotalDeLaVenta) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_4,
           Fila = 2,
           Columna = 1)
        ]
        public decimal? Cantidad { get; set; }

        [IUPropiedad(
           Etiqueta = "Precio",
           Tipo = typeof(decimal),
           Ayuda = "precio de venta unitario sin iva, se propone el del material",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularTotalDeLaVenta) + "()",
           Obligatorio = true,
           Formato = enumFormato.Moneda,
           Fila = 2,
           Columna = 2)
        ]
        public decimal? Precio { get; set; }

        //--------------------------------------------
        // Fila 3, a la derecha (bajo el precio): total de la venta (cantidad por precio), sólo se muestra
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe de la venta sin iva (cantidad por precio)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           AnchoMaximo = "300px",
           MantenerHuecoDeLaIzquierda = true,
           Fila = 3,
           Columna = 2)
        ]
        public decimal? Total { get; set; }

        //--------------------------------------------
        // Filas 4 y 5, a la derecha bajo el total y uno debajo del otro (CheckApilado, sin margen superior): qué más se hace tras crear el
        // documento de venta. Facturar exige servido: al marcar facturar se marca servido y al desmarcar servido se desmarca facturar
        // (ver Inventario.ts)
        [IUPropiedad(
            Etiqueta = "Servido",
            Ayuda = "Tras crear el documento de venta se pasa a la etapa de servicio",
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.CheckApilado,
            VisibleEnGrid = false,
            Obligatorio = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_AlCambiarServido) + "(this)",
            MantenerHuecoDeLaIzquierda = true,
            Fila = 4,
            Columna = 2
            )
        ]
        public bool Servido { get; set; }

        [IUPropiedad(
            Etiqueta = "Facturar",
            Ayuda = "Tras servirlo se crea la prefactura al cliente y el documento de venta pasa a la etapa de facturación",
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.CheckApilado,
            VisibleEnGrid = false,
            Obligatorio = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_AlCambiarFacturar) + "(this)",
            MantenerHuecoDeLaIzquierda = true,
            Fila = 5,
            Columna = 2
            )
        ]
        public bool Facturar { get; set; }
    }
}
