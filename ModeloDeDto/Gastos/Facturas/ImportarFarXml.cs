using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarFarXml 
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del proveedor de la factura",Visible = false )]
        public int IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "quién me factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Proveedor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Blanquear_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Seleccionar_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Proveedor { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo factura",
            Ayuda = "Seleccione el fichero de la factura recibida",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".xml",
            Fila = 0,
            Columna = 1,
            AutoSpan = true)]
        public int IdArchivo { get; set; }

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
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Posicion = 1)]
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
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string TipoFarPropuesto { get; set; }
    }
}
