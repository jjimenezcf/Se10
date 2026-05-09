using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using System;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Guarderias
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CursoDeGuarderiaDto : ElmentoAuditadoDto, IUsaNombreDto
    {
        //----------------------------------------------
        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public int? IdCg { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del aula", Visible = false)]
        public int IdAula { get; set; }

        [IUPropiedad(
            Etiqueta = "Aula",
            Ayuda = "Aula donde se imparte el curso lectivo",
            VisibleEnGrid = false,
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdAula),
            Controlador = nameof(enumControladoresGuarderias.AulasDeGuarderia),
            BuscarPor = ltrDeAulasDeGuarderia.SelectorParaCurso,
            SeleccionarDe = typeof(AulaDeGuarderiaDto),
            VistaDondeNavegar = enumVistasGuarderias.CrudAulasDeGuarderia,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string Aula { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Ordenar = true
            , Etiqueta = nameof(CursoDeGuarderiaDto.Inicio)
            , Ayuda = "Inicio del curso lectivo"
            , VisibleEnGrid = false
            , OrdenarGridPor = nameof(CursoDeGuarderiaDto.Inicio)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Fila = 0
            , Columna = 1
            , Posicion = 0)]
        public DateTime? Inicio { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Ordenar = true
            , Etiqueta = nameof(CursoDeGuarderiaDto.Fin)
            , Ayuda = "Fin del curso lectivo"
            , VisibleEnGrid = false
            , OrdenarGridPor = nameof(CursoDeGuarderiaDto.Fin)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Fila = 0
            , Columna = 1
            , Posicion = 1)]
        public DateTime? Fin { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Para la edad de",
            VisibleEnGrid = false,
            Ayuda = "indique la edad aprox. del curso",
            Tipo = typeof(int),
            Fila = 0,
            Columna = 2,
            Posicion = 0,
            VisibleAlEditar = false
          )
        ]
        public string Edad { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "medido en",
            VisibleEnGrid =false,
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumMedidoEn_AM),
            Fila = 0,
            Columna = 2,
            Posicion = 1,
            VisibleAlEditar = false
          )
        ]
        public string MedidoEn { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Curso",
            Tipo = typeof(string),
            Ordenar = true,
            VisibleAlCrear = false,
            Fila = 0,
            Columna = 2
          )
        ]
        public string Nombre { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del trabajador", Visible = false)]
        public int IdTrabajador { get; set; }

        [IUPropiedad(
            Etiqueta = "Profesor titutlar",
            Ayuda = "profesor responsable del curso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TrabajadorDto),
            GuardarEn = nameof(IdTrabajador),
            Controlador = nameof(enumControladoresTerceros.Trabajadores),
            VistaDondeNavegar = enumVistasTerceros.CrudTrabajadores,
            BuscarPor = nameof(TrabajadorDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            LongitudMinimaParaBuscar = 3,
            Fila = 1,
            Columna = 0,
            Obligatorio = true)
        ]
        public string Trabajador { get; set; }

        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Puesto de consultor",
            Ayuda = "puesto de trabajo para los consultores del curso",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Consultor),
            Fila = 1,
            Columna = 1,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo
            )
        ]
        public int IdConsultor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Consultor { get; set; }

        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Puesto de gestor",
            Ayuda = "puesto de trabajo para los consultores del curso",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Gestor),
            Fila = 1,
            Columna = 2,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo
            )
        ]
        public int IdGestor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Gestor { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Activo",
            Ayuda = "según las fechas del curso nos indica si está activo",
            Fila = 2,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public bool Activo { get; set; }


        [IUPropiedad(Visible = false)]
        public int? IdAgenda { get; set; }

        [IUPropiedad(Visible = false)]
        public string Agenda { get; set; }

    }
}
