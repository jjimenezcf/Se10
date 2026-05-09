using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CopiarFaeDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Factura a copiar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            SeleccionarDe = typeof(FacturaEmtDto),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaEmitida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.FacturaEmitida,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(FacturaEmtDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_ProponerDatosDeLaFaeSeleccionada) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_InicializarModalDeCopiado) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //----------------------------------------------
        [IUPropiedad(
         Etiqueta = "",
         Oculto = true,
         Obligatorio = false,
         VisibleEnGrid = false,
         Fila = 1,
         Columna = 0,
         Posicion = 0
         )]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(
         Etiqueta = "Id del Cg",
         Visible = false
         )]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG propuesto",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Posicion = 1)]
        public string Cg { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de factura",
              Visible = false
              )]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de factura",
            Ayuda = "Tipo de la factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            SeleccionarDe = typeof(TipoDeFacturaEmtDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaEmitida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeFacturaEmt,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.FacturaEmitida,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Tipo { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente de la factura",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "a quién se le factura",
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
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Cliente { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "nombre de la factura",
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "descripción de la factura",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 5,
           Fila = 4,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }
    }
}
