using System;
using ModeloDeDto.Callejero;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class DireccionDto : ElementoDto
    {
        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "negocio del elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }


        [IUPropiedad(Visible = false)]
        public string NombreDireccion { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "Dirección del elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = nameof(Pais),
           Ayuda = "Seleccione el país",
           TipoDeControl = enumTipoControl.ListaDinamica,
           SeleccionarDe = typeof(PaisDto),
           GuardarEn = nameof(IdPais),
           Controlador = nameof(enumControladoresCallejero.Paises),
           VistaDondeNavegar = enumVistasCallejero.CrudPaises,
           AlSeleccionarBlanquearControl = nameof(Provincia),
           CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
           LongitudMinimaParaBuscar = 1,
           Fila = 1,
           Columna = 0,
           Obligatorio = false,
           Ordenar = true,
           AutoSpan = false
           )
        ]
        public string Pais { get; set; }

        [IUPropiedad(Etiqueta = "Id del pais", Visible = false)] 
        public int IdPais { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(Provincia),
            Ayuda = "Seleccione la provincia",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ProvinciaDto),
            GuardarEn = nameof(IdProvincia),
            Controlador = nameof(enumControladoresCallejero.Provincias),
            VistaDondeNavegar = enumVistasCallejero.CrudProvincias,
            AlSeleccionarBlanquearControl = nameof(Municipio),
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(Pais),
            PropiedadRestrictora = nameof(IdPais),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            BuscarPor = ltrDeUnaProvincia.SeleccionarParaDireccion,
            Fila = 1,
            Columna = 1,
            Obligatorio = false,
            Ordenar = true,
            AutoSpan = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Direccion_Tras_Seleccionar_Provincia) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Provincia { get; set; }

        [IUPropiedad(Etiqueta = "Id de la provincia", Visible = false)]
        public int IdProvincia { get; set; }

        //----------------------------------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = nameof(Municipio),
            Ayuda = "Seleccione el municipio",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(MunicipioDto),
            GuardarEn = nameof(IdMunicipio),
            Controlador = nameof(enumControladoresCallejero.Municipios),
            VistaDondeNavegar = enumVistasCallejero.CrudMunicipios,
            AlSeleccionarBlanquearControl = nameof(Calle),
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(Provincia),
            PropiedadRestrictora = nameof(IdProvincia),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            BuscarPor = ltrDeUnMunicipio.SeleccionarParaDireccion,
            Fila = 1,
            Columna = 2,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Direccion_Tras_Seleccionar_Municipio) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Municipio { get; set; }

        [IUPropiedad(Etiqueta = "Id del municipio", Visible = false)]
        public int IdMunicipio { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Calificador",
            Ayuda = "Calificador de direcciones",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumCalificadorDireccion),
            GuardarEn = nameof(Calificador),
            Fila = 2,
            Columna = 0,
            Obligatorio = true,
            VisibleEnGrid = false,
            AutoSpan = false
          )
        ]
        public string Calificador { get; set; }

        //----------------------------------------------------------------------------------------
        //AlSeleccionarBlanquearControl = nameof(CodigoPostal) + "|" + nameof(Zona) + "|" + nameof(Barrio) + "|" + nameof(Numero) + "|" + nameof(Escalera) + "|" + nameof(Piso) + "|" + nameof(Puerta),
        [IUPropiedad(
            Etiqueta = "Calle",
            Ayuda = "Indique la calle",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CalleDto),
            GuardarEn = nameof(IdCalle),
            Controlador = nameof(enumControladoresCallejero.Calles),
            VistaDondeNavegar = enumVistasCallejero.CrudCalles,            
            RestringidoPorControl = nameof(Municipio),
            PropiedadRestrictora = nameof(IdMunicipio),
            BuscarPor = ltrCalles.SeleccionarParaDireccion,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 2,
            Columna = 1,
            AutoSpan =true,
            VisibleEnGrid = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Direccion_Tras_Seleccionar_Calle) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Direccion_Tras_Blanquear_Calle) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Calle { get; set; }

        [IUPropiedad(
            Etiqueta = "Id de la calle",
            Visible = false
            )
        ]
        public int IdCalle { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Barrio",
            Ayuda = "Indique el Barrio",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(BarrioDto),
            GuardarEn = nameof(IdBarrio),
            RestringidoPorControl = nameof(Municipio),
            PropiedadRestrictora = nameof(IdMunicipio),
            Controlador = nameof(enumControladoresCallejero.Barrios),
            VistaDondeNavegar = enumVistasCallejero.CrudBarrios,
            BuscarPor = nameof(BarrioDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = false,
            Obligatorio = false
            )
        ]
        public string Barrio { get; set; }

        [IUPropiedad(Etiqueta = "Id Barrio", Visible = false)]
        public int? IdBarrio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Zona",
            Ayuda = "Indique la zona",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ZonaDto),
            GuardarEn = nameof(IdZona),
            Controlador = nameof(enumControladoresCallejero.Zonas),
            VistaDondeNavegar = enumVistasCallejero.CrudZonas,
            RestringidoPorControl = nameof(Municipio),
            PropiedadRestrictora = nameof(IdMunicipio),
            BuscarPor = nameof(ZonaDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 4,
            Columna = 1,
            VisibleEnGrid = false,
            Obligatorio = false
            )
        ]
        public string Zona { get; set; }

        [IUPropiedad(Etiqueta = "Id Zona", Visible = false)]
        public int? IdZona { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código Postal",
            Ayuda = "Indique el CP",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CodigoPostalDto),
            GuardarEn = nameof(IdCp),
            RestringidoPorControl = nameof(Calle),
            Controlador = nameof(enumControladoresCallejero.CodigosPostales),
            VistaDondeNavegar = enumVistasCallejero.CrudCodigosPostales,
            PropiedadRestrictora = nameof(IdCalle),
            BuscarPor = nameof(CodigoPostalDtm.Codigo),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            LongitudMinimaParaBuscar = 1,
            Fila = 4,
            Columna = 3,
            Ordenar = true,
            OrdenarListaDinamicaPor = nameof(CodigoPostalDto.Codigo),
            AutoSpan = true
            )
        ]
        public string CodigoPostal { get; set; }

        [IUPropiedad(
            Etiqueta = "Id del código postal",
            Visible = false
            )
        ]
        public int IdCp { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Numero",
           Ayuda = "Nº de policia",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 3,
           Columna = 0,
           Obligatorio = false,
           LongitudMaxima = 5)
        ]
        public int?  Numero { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Escalera",
           Ayuda = "Escalera",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 1,
           Obligatorio = false,
           LongitudMaxima = 4)
        ]
        public string Escalera { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Piso",
           Ayuda = "Piso",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 2,
           Obligatorio = false,
           LongitudMaxima = 4)
        ]
        public string Piso { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Puerta",
           Ayuda = "Puerta",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 3,
           Obligatorio = false,
           LongitudMaxima = 15)
        ]
        public string Puerta { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Otros",
           Ayuda = "indique otros datos de la dirección",
           VisibleEnGrid = false,
           VisibleAlCrear = true,
           VisibleAlEditar = true,
           PermisosNecesarios = nameof(enumModoDeAccesoDeDatos.Administrador) + "|" + ServicioDeDatos.Seguridad.ModoDeAcceso.Creador,
           Obligatorio = false,
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 6,
           Columna = 0,
           AutoSpan = true)
        ]
        public string Otros { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Visualizar en Maps",
           Ayuda = "Url de Maps",
           VisibleEnGrid = false,
           VisibleAlCrear = false,
            VisibleAlEditar = true,
           EditableAlEditar = false,
           Obligatorio = false,
           TipoDeControl = enumTipoControl.Editor,
           Fila = 7,
           Columna = 0,
           AutoSpan = true,
           OnBlur = "javascript: " + nameof(enumNameSpaceTs.MapearAlControl) + "." + nameof(enumFunctionTs.DefinirLink) + "(this)",
           ConLink = true)
        ]
        public string Url { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta ="Activa",
            Ayuda = "la dirección está activa",
            VisibleEnGrid = true,
            Fila = 9,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = true,
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            EditableAlCrear = false,
            ColSpan = 4
            )
        ]
        public bool Activo { get; set; }


        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdCreador { get; set; }

        [IUPropiedad(
           Etiqueta = "Creada por",
           Ayuda = "Creada por",
           Visible = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           PropiedadRestrictora = nameof(IdCreador),
           MostrarExpresion = nameof(Creador),
           Fila = 8,
           Columna = 0,
           ColSpan = 2)
        ]
        public string Creador { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Creada el",
           Ayuda = "Fecha de creación",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.Fecha,
           Fila = 8,
           Columna = 1,
           VisibleEnGrid = false,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           ColSpan = 2
           )
        ]
        public DateTime CreadaEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Dirección",
           Ayuda = "dirección del elemento",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 9,
           Columna = 1,
           VisibleEnGrid = true,
           VisibleEnEdicion = false,
           Obligatorio = false
           )
        ]
        public string Expresion { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool IntraComunitaria { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool ExtraComunitaria { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsNacional => !ExtraComunitaria && !IntraComunitaria;
        
    }

}
