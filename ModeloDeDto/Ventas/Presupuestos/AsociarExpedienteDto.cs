using ModeloDeDto.Expediente;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class AsociarExpedienteDto: ISelectorDto
    {
        //----------------------------------------------
        [IUPropiedad(
         Etiqueta = "",
         Oculto = true,
         Obligatorio = false,
         VisibleEnGrid = false,
         Fila = 0,
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
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.Presupuesto) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Posicion = 1)]
        public string Cg { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del expediente seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Expediente",
            Ayuda = "Expediente a vincular",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            BuscarPor = ltrDeUnPresupuesto.AsociarExpediente,
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            SeleccionarDe = typeof(ExpedienteDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestringidoPorControl = nameof(Cg),
            PropiedadRestrictora = nameof(IdCg),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Expediente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Expediente,
            MostrarExpresion = nameof(ExpedienteDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }
    }

}
