using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PuestosDeUnRolDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Puesto);

        [IUPropiedad(Etiqueta = "Rol",
            Ayuda = "Puesto de un rol",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Rol),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdRol { get; set; }

        [IUPropiedad(
            Etiqueta = "Rol",
            Visible = false
            )
        ]
        public string Rol { get; set; }

        //-------------------------------------------------------

        [IUPropiedad(Etiqueta = "Centro getor",VisibleEnGrid = true, Ordenar = true, VisibleAlCrear = false, VisibleAlEditar = true, EditableAlEditar = false, Fila = 1, Columna = 0)]
        public string CgDelPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Id del puesto",
            Visible = false
            )
        ]
        public int IdPuesto { get; set; }


        [IUPropiedad(
            Etiqueta = "Puesto",
            Ayuda = "Indique el puesto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PuestoDto),
            GuardarEn = nameof(IdPuesto),
            RestringidoPorControl = nameof(IdRol),
            MostrarExpresion = nameof(PuestoDto.Expresion),
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo,
            AplicarJoin = true,
            Ordenar = true,
            OrdenarListaDinamicaPor = $"{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Codigo)}" + Simbolos.separadorDeClausulasDeOrdenacion +
                                      $"{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Nombre)}" + Simbolos.separadorDeClausulasDeOrdenacion +
                                      $"{nameof(PuestoDtm.Nombre)}",
            Fila = 2,
            Columna = 0
            )
        ]
        public string Puesto { get; set; }


    }
}
