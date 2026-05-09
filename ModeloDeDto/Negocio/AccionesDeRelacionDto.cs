using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class AccionesDeRelacionDto : ElementoDto, IRelacionDto, IUsaNegocioDto
    {
        public static string ExpresionElemento = nameof(Accion);

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del negocio",
            Visible = false
            )
        ]
        public int IdNegocio { get; set; }


        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Seleccione el negocio principal",
            TipoDeControl = enumTipoControl.ListaDinamica,
            VistaDondeNavegar = enumVistasNegocio.CrudDeNegocios,
            SeleccionarDe = typeof(NegocioDto),
            GuardarEn = nameof(IdNegocio),
            Controlador = nameof(enumControladoresNegocio.Negocio),
            BuscarPor = nameof(NegocioDto.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Negocio { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del negocio vinculado",
            Visible = false
            )
        ]
        public int IdVinculado { get; set; }

        [IUPropiedad(
            Etiqueta = "Vinculado",
            Ayuda = "Indique el negocio vinculado",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(NegocioDto),
            GuardarEn = nameof(IdVinculado),
            Controlador = nameof(enumControladoresNegocio.Negocio),
            VistaDondeNavegar = enumVistasNegocio.CrudDeNegocios,
            BuscarPor = nameof(NegocioDto.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Vinculado { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Momento",
            Ayuda = "indique el momento de ejecución",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumMomentoDeRelacion),
            GuardarEn = nameof(Momento),
            Fila = 2,
            Columna = 0,
            Posicion = 0,
            EditableAlCrear = true
          )
        ]
        public string Momento { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Orden",
           Ayuda = "Orden de ejecución",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           PorAnchoMnt = 10,
           Ordenar = true,
            Fila = 2,
            Columna = 0,
            Posicion = 1)
        ]
        public int Orden { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del acción",
            Visible = false
            )
        ]
        public int IdAccion { get; set; }


        [IUPropiedad(
            Etiqueta = "Accion",
            Ayuda = "Indique el acción",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AccionDto),
            GuardarEn = nameof(IdAccion),
            Controlador = nameof(enumControladoresEntorno.Acciones),
            BuscarPor = nameof(AccionDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            VistaDondeNavegar = enumVistasEntorno.CrudDeAcciones,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Negocio) + "." + nameof(enumFunctionTs.Negocio_TrasSeleccionarAccionDeRelacion) + "()",

            Fila = 2,
            Columna = 2,
            Ordenar = true,
            AutoSpan = true,
            PorAnchoMnt =50
            )
        ]
        public string Accion { get; set; }


        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Descripción",
          Ayuda = "Describa porque de la acción",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Fila = 3,
          Columna = 0,
          VisibleEnGrid = false,
          AutoSpan = true,
          NumeroDeFilas =3
          )
        ]
        public string Descripcion { get; set; }


        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Parámetros",
          Ayuda = "Parámetros de ejecución",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Fila = 4,
          Columna = 0,
          VisibleEnGrid = false,
          Obligatorio = false,
          AutoSpan = true
          )
        ]
        public string Parametros { get; set; }

        //-----------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Está activa",
            Ayuda = "indica si la acción está activa",
            VisibleEnGrid = true,
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool Activo { get; set; }

    }
}
