using ModeloDeDto.Expediente;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CopiarPptDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del presupuesto seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "Presupuesto a copiar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            SeleccionarDe = typeof(PresupuestoDto),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Presupuesto) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Presupuesto,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(PresupuestoDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_ProponerDatosDelPptSeleccionado) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_InicializarModalDeCopiado) + "()",
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
              Etiqueta = "Id del tipo de presupuesto",
              Visible = false
              )]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo presupuesto",
            Ayuda = "Tipo de presupuesto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            SeleccionarDe = typeof(TipoDePresupuestoDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Presupuesto) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDePresupuesto,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Presupuesto,
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
         Etiqueta = "Id del expediente seleccionado",
         Visible = false
         )]
        public int idExpediente { get; set; }

        [IUPropiedad(
            Etiqueta = "Expediente propuesto",
            Ayuda = "expediente del nuevo presupuesto",
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
            MostrarExpresion = nameof(PresupuestoDto.Expresion),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = false)]
        public string Expediente { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del solicitante del Presupuesto",
         Visible = false
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "Quién solicita el Presupuesto",
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
            Columna = 1,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Solicitante { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "nombre del presupuesto",
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
           Ayuda = "descripción del presupuesto",
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
