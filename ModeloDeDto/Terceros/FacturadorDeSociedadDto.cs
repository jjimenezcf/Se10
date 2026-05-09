using ModeloDeDto.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class FacturadorDeSociedadDto : EsUnDetalleDto
    {
        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "ApiKey",
            Ayuda = "Apikey de la sociedad para solicitar la emisión de una serie por un centro de gestión",
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true,
            LongitudMaxima = 250,
            Obligatorio = false
            )
        ]
        public string Apikey { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del Cg", Visible = false)]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG emisor",
            Ayuda = "Centro gestor desdeel que se emite la factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdElemento),
            PropiedadRestrictora = nameof(ElementoDeUnProcesoDto.IdSociedadDelCg),
            SoloEnAlta = true,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Cg { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de factura emitida", Visible = false)]
        public int IdTipoDeFactura { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de factura emitir",
            Ayuda = "Tipo de la factura que se ha de emitir",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.FacturaEmitida,
            GuardarEn = nameof(IdTipoDeFactura),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeFacturaEmt,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            Obligatorio = false,
            Fila = 1,
            Columna = 1,
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string TipoDeFactura { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Mapeos",
          Ayuda = "Json que define el mapeo de campos necesarios para la factura",
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Tipo = typeof(string),
          Fila = 2,
          Columna = 0,
          NumeroDeFilas = 7,
          ValorPorDefecto = ltrDeFacturadorDeSociedad.jsonDeMapeo,
          AutoSpan = true
          )
        ]
        public string MapeosJson { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Activa",
           Ayuda = "indica si la tarjeta de la sociedad está de activa",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 3,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.ControlApilado,
           ValorPorDefecto = true,
           VisibleAlCrear = false,
           EditableAlEditar = true,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_AlCambiar_TarjetaActiva) + "(this)"
           )]
        public bool Activa { get; set; }

    }
}
