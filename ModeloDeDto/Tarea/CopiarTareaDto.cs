using ModeloDeDto.Expediente;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CopiarTareaDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id de la tarea seleccionada",Visible = false )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea",
            Ayuda = "Tarea a copiar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            SeleccionarDe = typeof(TareaDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudTareas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Tarea,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(TareaDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Tar_ProponerDatosDelaTareaSeleccionada) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Tar_InicializarModalDeCopiado) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //--------------------------------------------------------------------------------------------------------
        [IUPropiedad( Oculto = true, Fila = 1,Columna = 0)]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad( Etiqueta = "Id del Cg", Visible = false )]
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
        [IUPropiedad(Etiqueta = "Id del tipo de tarea", Visible = false )]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo tarea",
            Ayuda = "Tipo de tarea",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            SeleccionarDe = typeof(TipoDeTareaDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasAdministrativo.TiposDeTarea,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Tarea,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Tipo { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id del solicitante del Tarea", Visible = false)]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "indica solicitante de la tarea",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSolicitante),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Interlocutor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Interlocutor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Solicitante { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "descripción breve ...",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 1,
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
           Ayuda = "descripción de la tarea",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 4,
           Fila = 4,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del expediente seleccionado", Visible = false )]
        public int? idExpediente { get; set; }


        [IUPropiedad(
            Etiqueta = "Expediente propuesto",
            Ayuda = "expediente de la nueva tarea",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(idExpediente),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            SeleccionarDe = typeof(ExpedienteDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Expediente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            Negocio = enumNegocio.Expediente,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(TareaDto.Expresion),
            Tipo = typeof(string),
            Fila = 5,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = false)]
        public string Expediente { get; set; }

        //----------------------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            EtiquetaGrid = "",
            Etiqueta = "Archivo tarea",
            Ayuda = "Seleccione el fichero de la tarea recibida",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 5,
            Columna = 1,
            AutoSpan = true)]
        public int? IdArchivoAlCopiar { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Enlazar archivos",
           Ayuda = "indica si han de asociar los mismo archivos que los de la original",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 6,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.CheckApilado,
           ValorPorDefecto = false,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           AnchoMaximo = "100px",
           EsAlmacenable = true
           )]
        public bool EnlazarArchivos { get; set; }



    }
}
