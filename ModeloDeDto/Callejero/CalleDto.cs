using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{
    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5
         , MostrarExpresion = "Expresion")]
    public class CalleDto : ElmentoAuditadoDto
    {
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
            Fila = 0,
            Columna = 0,
            Obligatorio = false,
            Ordenar = true,
            AutoSpan = false,
            EditableAlEditar = false,
            TamanoFijo = "10em"
            )
         ]
        public string Pais { get; set; }

        [IUPropiedad(Etiqueta = "Id del pais", Visible = false)]
        public int IdPais { get; set; }

        //----------------------------------------------
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
            Fila = 0,
            Columna = 1,
            Obligatorio = false,
            Ordenar = true,
            AutoSpan = false,
            EditableAlEditar = false,
            TamanoFijo = "12em",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Calle_Tras_Seleccionar_Provincia) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Provincia { get; set; }

        [IUPropiedad(Etiqueta = "Id de la provincia", Visible = false)]
        public int IdProvincia { get; set; }
        //----------------------------------------------
        
        [IUPropiedad(
            Etiqueta = nameof(Municipio),
            Ayuda = "Seleccione el municipio",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(MunicipioDto),
            GuardarEn = nameof(IdMunicipio),
            Controlador = nameof(enumControladoresCallejero.Municipios),
            VistaDondeNavegar = enumVistasCallejero.CrudMunicipios,
            AlSeleccionarBlanquearControl = nameof(CodigoPostal),
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(Provincia),
            PropiedadRestrictora = nameof(IdProvincia),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            BuscarPor = ltrDeUnMunicipio.SeleccionarParaDireccion,
            Fila = 0,
            Columna = 2,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true,
            EditableAlEditar = false,
            TamanoFijo = "12em",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Calle_Tras_Seleccionar_Municipio) + "([" + nameof(enumParamTs.idLista) + "])"

            )
        ]
        public string Municipio { get; set; }

        [IUPropiedad(Etiqueta = "Id del municipio", Visible = false)]
        public int IdMunicipio { get; set; }

        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = nameof(TipoDeVia),
            Ayuda = "Seleccione el tipo de vía",
            VisibleEnGrid = false,
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeViaDto),
            GuardarEn = nameof(IdTipoDeVia),
            Controlador = nameof(enumControladoresCallejero.TiposDeVia),
            VistaDondeNavegar = enumVistasCallejero.CrudTiposDeVia,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            AutoSpan = false
            )
        ]
        public string TipoDeVia { get; set; }

        [IUPropiedad(Etiqueta = "Id del tipo de vía", Visible = false)]
        public int IdTipoDeVia { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Calle",
            Ayuda = "Indique el nombre de la calle",
            Tipo = typeof(string),
            VisibleEnGrid = false,
            Fila = 1,
            Columna = 1,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Expresión de calle",
            EtiquetaGrid = "Dirección",
            Ayuda = "Expresión de la calle",
            Tipo = typeof(string),
            Ordenar = true,
            OrdenarGridPor = nameof(CalleDto.Nombre),
            Fila = 2,
            Columna = 0,
            VisibleEnGrid = true,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            AutoSpan = true
          )
        ]
        public new string Expresion { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código Postal",
            Ayuda = "Indique el CP",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CodigoPostalDto),
            GuardarEn = nameof(CpsDeUnaCalleDtm.IdCp),
            RestringidoPorControl = nameof(Municipio),
            Controlador = nameof(enumControladoresCallejero.CodigosPostales),
            VistaDondeNavegar = enumVistasCallejero.CrudCodigosPostales,
            PropiedadRestrictora = nameof(IdMunicipio),
            BuscarPor = nameof(CodigoPostalDtm.Codigo),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            LongitudMinimaParaBuscar = 1,
            Fila = 3,
            Columna = 0,
            Ordenar = true,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            BlanquearAlSalir = false,
            LongitudMaxima = 10,
            Obligatorio = false
            )
        ]
        public string CodigoPostal { get; set; }
        
        [IUPropiedad(Etiqueta = "Id CP", Visible = false)]
        public int? IdCp { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Mano",
           Ayuda = "Lado de la calle",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 1,
           Obligatorio = false,
           ValorPorDefecto = ParseosDeManosDeUnaCalle.Ambos,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
            VisibleEnGrid = false,
           LongitudMaxima = 1)
        ]
        public string ManoCp { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Desde",
           Ayuda = "Nº de policía de inicio",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 3,
           Columna = 2,
           Obligatorio = false,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           VisibleEnGrid = false,
           ValorPorDefecto = 0,
           LongitudMaxima = 5)
        ]
        public int? DesdeCp { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Hasta",
           Ayuda = "Nº de policía de fin",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 3,
           Columna = 3,
           Obligatorio = false,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           VisibleEnGrid = false,
           LongitudMaxima = 5)
        ]
        public int? HastaCp { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Barrio",
            Ayuda = "Indique el Barrio",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(BarrioDto),
            GuardarEn = nameof(IdBarrio),
            RestringidoPorControl = nameof(Municipio),
            Controlador = nameof(enumControladoresCallejero.Barrios),
            VistaDondeNavegar = enumVistasCallejero.CrudBarrios,
            PropiedadRestrictora = nameof(IdMunicipio),
            BuscarPor = nameof(BarrioDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 4,
            Columna = 0,
            Ordenar = true,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            BlanquearAlSalir = false,
            Obligatorio = false
            )
        ]
        public string Barrio { get; set; }

        [IUPropiedad(Etiqueta = "Id Barrio", Visible = false)]
        public int? IdBarrio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Mano",
           Ayuda = "Lado de la calle",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 4,
           Columna = 1,
            Obligatorio = false,
            ValorPorDefecto = 'A',
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           VisibleEnGrid = false,
           LongitudMaxima = 1)
        ]
        public string ManoBarrio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Desde",
           Ayuda = "Nº de policía de inicio",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 4,
           Columna = 2,
           Obligatorio = false,
           ValorPorDefecto = 0,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           VisibleEnGrid = false,
           LongitudMaxima = 5)
        ]
        public int? DesdeBarrio { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Hasta",
           Ayuda = "Nº de policía de fin",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 4,
           Columna = 3,
           Obligatorio = false,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           VisibleEnGrid = false,
           LongitudMaxima = 5)
        ]
        public int? HastaBarrio { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Zona",
            Ayuda = "Indique la zona",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ZonaDto),
            GuardarEn = nameof(IdZona),
            RestringidoPorControl = nameof(Municipio),
            Controlador = nameof(enumControladoresCallejero.Zonas),
            VistaDondeNavegar = enumVistasCallejero.CrudZonas,
            PropiedadRestrictora = nameof(IdMunicipio),
            BuscarPor = nameof(ZonaDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 5,
            Columna = 0,
            Ordenar = true,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            BlanquearAlSalir = false,
            Obligatorio = false
            )
        ]
        public string Zona { get; set; }

        [IUPropiedad(Etiqueta = "Id Zona", Visible = false)]
        public int? IdZona { get; set; }


    }
}
