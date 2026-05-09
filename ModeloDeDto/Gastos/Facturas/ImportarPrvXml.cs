using ModeloDeDto.Callejero;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarPrvXml
    {
        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo factura",
            Ayuda = "Seleccione el fichero de la factura recibida",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".xml",
            Fila = 0,
            Columna = 0,
            AutoSpan = true)]
        public int IdArchivo { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del municipio", Visible = false)]
        public int? IdMunicipio { get; set; }

        [IUPropiedad(
            Etiqueta = nameof(Municipio),
            Ayuda = "Seleccione el municipio si el del xml no es válido o no está indicado",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(MunicipioDto),
            GuardarEn = nameof(IdMunicipio),
            Controlador = nameof(enumControladoresCallejero.Municipios),
            VistaDondeNavegar = enumVistasCallejero.CrudMunicipios,
            LongitudMinimaParaBuscar = 1,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            BuscarPor = ltrDeUnMunicipio.SeleccionarParaDireccion,
            Fila = 1,
            Columna = 0,
            Obligatorio = false)
        ]
        public string Municipio { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de vía", Visible = false)]
        public int? IdTipoDeVia { get; set; }

        [IUPropiedad(
            Etiqueta = nameof(TipoDeVia),
            Ayuda = "Seleccione el tipo de vía si no está indicada en el Xml",
            VisibleEnGrid = false,
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeViaDto),
            GuardarEn = nameof(IdTipoDeVia),
            Controlador = nameof(enumControladoresCallejero.TiposDeVia),
            VistaDondeNavegar = enumVistasCallejero.CrudTiposDeVia,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 1
            )
        ]
        public string TipoDeVia { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "eMail del proveedor",
           Fila = 1,
           Columna = 2,
           LongitudMaxima = 50,
           Obligatorio = false
          )
        ]
        public string eMail { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "teléfono del proveedor",
           Fila = 1,
           Columna = 3,
           LongitudMaxima = 15,
           Obligatorio = false
          )
        ]
        public string Telefono { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del Cg",Visible = false)]
        public int? IdCgPropuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "CG propuesto",
            Ayuda = "Centro gestor donde se asociará la factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCgPropuesto),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string CgPropuesto { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id del tipo de factura", Visible = false)]
        public int? IdTipoFarPropuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo propuesto",
            Ayuda = "Tipo de la factura a crear",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipoFarPropuesto),
            SeleccionarDe = typeof(TipoDeFacturaRecDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaRecibida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasGastos.TiposDeFacturaRec,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.FacturaRecibida,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 2,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string TipoFarPropuesto { get; set; }
    }
}
