using System;
using ModeloDeDto.Callejero;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class CrearDireccionDto
    {
        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Calificador",
            Ayuda = "Calificador de direcciones",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumCalificadorDireccion),
            GuardarEn = nameof(Calificador),
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false,
            AutoSpan = false
          )
        ]
        public virtual string Calificador { get; set; }

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
           Columna = 1,
           Obligatorio = false,
           Ordenar = true,
           AutoSpan = false
           )
        ]
        public string Pais { get; set; }

        [IUPropiedad(Etiqueta = "Id del pais", Visible = false)] 
        public int? IdPais { get; set; }

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
            Columna = 2,
            Obligatorio = false,
            Ordenar = true,
            AutoSpan = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Direccion_Tras_Seleccionar_Provincia) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Provincia { get; set; }

        [IUPropiedad(Etiqueta = "Id de la provincia", Visible = false)]
        public int? IdProvincia { get; set; }

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
            Obligatorio = false,
            Fila = 1,
            Columna = 3,
            Ordenar = true,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeDireccion) + "." + nameof(enumFunctionTs.Direccion_Tras_Seleccionar_Municipio) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Municipio { get; set; }

        [IUPropiedad(Etiqueta = "Id del municipio", Visible = false)]
        public int? IdMunicipio { get; set; }

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
            Columna = 0,
            ColSpan = 3,
            VisibleEnGrid = false,
            Obligatorio = false,
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
        public int? IdCalle { get; set; }

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
            Fila = 2,
            Columna = 3,
            Ordenar = true,
            OrdenarListaDinamicaPor = nameof(CodigoPostalDto.Codigo),
            AutoSpan = true,
            Obligatorio = false
            )
        ]
        public string CodigoPostal { get; set; }

        [IUPropiedad(
            Etiqueta = "Id del código postal",
            Visible = false
            )
        ]
        public int? IdCp { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Numero",
           Ayuda = "Nº de policia",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 3,
           Columna = 0,
           Posicion = 0,
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
           Columna = 0,
           Posicion = 1,
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
           Columna = 0,
           Posicion = 2,
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
           Columna = 0,
           Posicion = 3,
           Obligatorio = false,
           LongitudMaxima = 15)
        ]
        public string Puerta { get; set; }

        //----------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Otros",
           Ayuda = "indique otros datos de la dirección",
           Obligatorio = false,
           Fila = 3,
           Columna = 1,
           AutoSpan = true)
        ]
        public string Otros { get; set; }
    }

}
