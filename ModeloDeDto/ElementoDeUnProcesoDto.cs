using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Negocio;
using System.Collections.Generic;
using ServicioDeDatos.Negocio;
using System;

namespace ModeloDeDto
{

    public class ElementoDeUnProcesoDto : ElmentoAuditadoDto, IElementoDeUnProcesoDto
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

        [IUPropiedad(Etiqueta = "Id del Cg", Visible = false)]
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
            OrdenarListaDinamicaPor = $"{nameof(CentroGestorDtm.CgPadre)}.{nameof(CentroGestorDtm.Codigo)}" + Simbolos.separadorDeClausulasDeOrdenacion +
                                      $"{nameof(CentroGestorDtm.Codigo)}" + Simbolos.separadorDeClausulasDeOrdenacion +
                                      $"{nameof(CentroGestorDtm.Nombre)}" ,
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.Registro) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
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
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Neg_Tras_Seleccionar_CG) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Neg_Tras_Blanquear_CG) + "([" + nameof(enumParamTs.idLista) + "])",
            Posicion = 1)]
        public string Cg { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo del proceso", Visible = false)]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "Tipo del proceso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Posicion =0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Neg_Tras_Seleccionar_Tipo) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Neg_Tras_Blanquear_Tipo) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Tipo { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la clase del proceso", Visible = false)]
        public int IdClaseDeElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "seleccionar clase",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(ClaseDelNegocioDtm),
            Controlador = nameof(enumControladoresNegocio.ClasesDelNegocio),
            RestrictorFijo = nameof(IElmentoAuditadoDto.IdNegocio),
            RestringidoPorControl = nameof(Tipo),
            CargarBajoDemanda =true,
            GuardarEn = nameof(IdClaseDeElemento),
            MostrarExpresion = nameof(ClaseDelNegocioDto.Expresion),
            VisibleAlCrear = true,
            VisibleAlEditar = true,
            AutoSpan = true,
            Obligatorio = false,
            Fila = 0,
            Columna = 1,
            Posicion = 1,
            VisibleEnGrid = false,
            AnchoMaximoContenedor = "50%",
            EsAlmacenable =true
          )
        ]
        public string ClaseDeElemento { get; set; }

        //----------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Referencia",
            Ayuda = "Referencia del proceso",
            Tipo = typeof(string),
            Ordenar = true,
            EditableAlEditar = false,
            VisibleAlEditar = true,
            VisibleAlCrear = false,
            Fila = 1,
            Columna = 0
          )
        ]
        public string Referencia { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Estado", VisibleEnGrid = true, VisibleEnEdicion = false)]
        public string Estado { get; set; }

        [IUPropiedad(
            Etiqueta = "Estado",
            Ayuda = "situación del proceso",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Estado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            Negocio = enumNegocio.No_Definido,
            Fila = 1,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlEditar = false,
            VisibleAlEditar = true,
            VisibleAlCrear = false,
            AutoSpan = true
            )
        ]
        public int IdEstado { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "descrición breve [Negocio]",
            Tipo = typeof(string),
            Fila = 5,
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
           Ayuda = "descripción detallada [Negocio]",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 5,
           Fila = 6,
           Columna = 0,
           LongitudMaxima = 1999,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           EtiquetaGrid = "Transitado El",
           Ayuda = "última vez que se transitó",
           PosicionEnGrid = 100,
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           VisibleEnGrid = false,
           VisibleEnEdicion = false,
           Obligatorio = false
          )
        ]
        public DateTime? TransitadoEl { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           EtiquetaGrid = "Transitado Al",
           Ayuda = "Transición ejecutada",
           PosicionEnGrid = 110,
           VisibleEnGrid = false,
           VisibleEnEdicion = false,
           Obligatorio = false
          )
        ]
        public string TransitadoAl { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsInterventor { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsGestor { get; set; }

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public List<string> Etapas { get; set; }

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool UsaClasePorTipo { get; set; } = false;

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string EstadoAnterior { get; set; }

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int? IdEstadoAnterior { get; set; }
        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string TransicionAplicable { get; set; }

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int? IdTransicionAplicable { get; set; }

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int? IdEstadoDestino { get; set; }

        //---------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int TransicionesDisponibles { get; set; }
    }


}