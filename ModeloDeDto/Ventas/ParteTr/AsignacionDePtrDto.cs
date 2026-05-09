using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.Terceros;
using System;
using static ServicioDeDatos.Elemento.Enumerados;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false, OpcionDeCrear = false)]
    public class AsignacionDePtrDto : EsUnDetalleDto
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
            Etiqueta = "CG",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.Trabajador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            OrdenarGridPor = nameof(Cg) + "." + nameof(CentroGestorDtm.Codigo),
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Cg { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del trabajador",Visible = false)]
        public int IdTrabajador { get; set; }

        [IUPropiedad(
            Etiqueta = "Trabajador",
            Ayuda = "Indique el trabajador",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TrabajadorDto),
            GuardarEn = nameof(IdTrabajador),
            Controlador = nameof(enumControladoresTerceros.Trabajadores),
            VistaDondeNavegar = enumVistasTerceros.CrudTrabajadores,
            BuscarPor = nameof(TrabajadorDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            AyudaDeCriteriosDeBusqueda = "Indique NIF, CIF, correo, teléfono, apellidos o nombre",
            LongitudMinimaParaBuscar = 3,
            Fila = 1,
            Columna = 1,
            AutoSpan = true,
            PorAnchoMnt = 30
            )
        ]
        public string Trabajador { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Inicio planificado",
            EtiquetaGrid ="Iniciar",
            PorAnchoSel =20,
            Ayuda = "Fecha en la que se debe iniciar el parte de trabajo",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            SelectorHasta = nameof(PlfDeFin)+":0:1",
            Fila = 2,
            Columna = 0,
            Obligatorio = false
           )
        ]
        public DateTime? PlfDeInicio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuando se terminará",
            EtiquetaGrid = "Finalizar",
            PorAnchoSel = 20,
            Ayuda = "fecha planificada de terminación",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 2,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? PlfDeFin { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Copia fechas de planificación",
            Ayuda = "indica si se pone como fechas reales las planificadas",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.CheckApilado,
            ValorPorDefecto = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Ptr_CopiarFechasDeAsignacionPtr) + "(this)",
            VisibleEnGrid = false
            )
        ]
        public bool CopiarFechasPlan { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha real de inicio",
            EtiquetaGrid = "Iniciado",
            VisibleEnGrid = false,
            PorAnchoSel = 20,
            Ayuda = "Indica cuándo se inicio realmente el parte de trabajo",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            SelectorHasta = nameof(Finalizada)+":0:1",
            Fila = 4,
            Columna = 0,
            Obligatorio = false
           )
        ]
        public DateTime? Iniciada { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha real de finalización",
            EtiquetaGrid = "Terminado",
            VisibleEnGrid = false,
            PorAnchoSel = 20,
            Ayuda = "fecha real de cuando se finalaza el trabajo asignado",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 4,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? Finalizada { get; set; }


        [IUPropiedad(
            Etiqueta = "Calcular tiempo",
            Ayuda = "indica si se ha de calcular el tiempo en función de las fechas",
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.CheckApilado,
            ValorPorDefecto = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Ptr_CalculaDuracionDeAsignacionPtr) + "(this)",
            VisibleEnGrid = false
            )
        ]
        public bool CalcularTiempo { get; set; }

        //--------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Duración",
           VisibleEnGrid = false,
            PorAnchoSel = 10,
           Ayuda = "tiempo dedicado",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 6,
           Columna = 0,
           AutoSpan = false,
           Obligatorio = false,
           Alineada = enumAliniacion.izquierda
           )
        ]
        public decimal? Duracion { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "medido en",
            VisibleEnGrid = false,
            PorAnchoSel = 10,
            Ayuda = "Indique como se mide el periodo de tiempo",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumDurabilidad),
            GuardarEn = nameof(MedidoEn),
            Fila = 6,
            Columna = 1,
            AutoSpan = false,
            Obligatorio = false,
            Alineada = enumAliniacion.izquierda
          )
        ]
        public enumDurabilidad? MedidoEn { get; set; }

        //------------------------------------------------
        [IUPropiedad(Etiqueta = "literal de Medido En", Visible = false)]
        public string LtrMedidoEn { get; set; }

    }
}
