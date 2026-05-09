using ModeloDeDto.Gastos;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ProcesarFarConIaDto
    {

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Archivador",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Archivador),
            Fila = 0,
            Columna = 0,
            AutoSpan = false,
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores
            )
        ]
        public int IdArchivador { get; set; }

        [IUPropiedad(Visible = false)]
        public string Archivador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la carpeta", Visible = false)]
        public int? IdCarpetaSeleccionada { get; set; }

        [IUPropiedad(
            Etiqueta = "Carpeta",
            Ayuda = "Seleccione la carpeta del archivador",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CarpetaDto),
            Controlador = nameof(enumControladoresSistemaDocumental.Carpetas),
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            GuardarEn = nameof(IdCarpetaSeleccionada),
            RestringidoPorControl = nameof(IdArchivador),
            CargarBajoDemanda = true,
            AutoSpan = true,
            Fila = 0,
            Columna = 1,
            Obligatorio = false
            )
        ]
        public string CarpetaSeleccionada { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del Cg", Visible = false)]
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
        [IUPropiedad(Etiqueta = "Id del tipo de factura", Visible = false)]
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


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del proveedor de la factura", Visible = false)]
        public int? IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "indica proveedor del lote de facturas, si es el mismo, si no en blanco",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Proveedor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Proveedor { get; set; }

    }
}
