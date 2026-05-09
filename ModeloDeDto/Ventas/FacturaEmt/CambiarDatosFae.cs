using ModeloDeDto.Juridico;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CambiarDatosFae : IRenombrarDto
    {
        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "factura a la que cambiar datos",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Etiqueta = "Factura", Visible = false)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente de la factura",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién facturo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            SeleccionarDe = typeof(ClienteDto),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0
            )]
        public string Cliente { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id ppt de la factura",
         Visible = false
         )]
        public int? IdPresupuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "qué facturo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdPresupuesto),
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            SeleccionarDe = typeof(PresupuestoDto),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos, 
            RestrictorFijo = ltrDeUnPresupuesto.SelectorParaUnaFacturaEmt + Simbolos.PuntoComa + nameof(enumEtapasDePpts.PPT_Etapa_PermiteFacturar),
            Negocio = enumNegocio.Presupuesto,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 4,
            Columna = 0,
            antesDeBuscar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_Antes_De_Buscar_Ppt) 
            )]
        public string Presupuesto { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id contrato de la factura",
         Visible = false
         )]
        public int? IdContrato { get; set; }

        [IUPropiedad(
            Etiqueta = "Contrato",
            Ayuda = "qué contrato es afectado",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdContrato),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + Simbolos.Igual + nameof(enumClaseDeContrato.Venta),
            RestrictorFijo = ltrDeUnContrato.SelectorParaUnaFacturaEmt + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Vigente) + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Finalizacion),
            RestringidoPorControl = nameof(Cliente),
            SeleccionarDe = typeof(ContratoDto),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            Negocio = enumNegocio.Contrato,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 4,
            Columna = 1,
            antesDeBuscar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_Antes_De_Buscar_Ctt)
            )]
        public string Contrato { get; set; }
    }
}
