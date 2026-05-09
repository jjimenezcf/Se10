

using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PuestosDeUnPermisoDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Puesto);

        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Permiso",
            Ayuda = "Puestos de un permiso",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Puesto),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdPermiso { get; set; }

        [IUPropiedad(
            Etiqueta = "Permiso",
            Visible = false
            )
        ]
        public string Permiso { get; set; }

        //-------------------------------------------------------

        [IUPropiedad(Etiqueta = "Centro getor", VisibleEnGrid = false, VisibleAlCrear = false, VisibleAlEditar = true, EditableAlEditar = false, Fila = 0, Columna = 0)]
        public string CgDelPuesto { get; set; }


        //-------------------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Id del puesto",
            Visible = false
            )
        ]
        public int IdPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Puesto",
            Ayuda = "Indique el Puesto de trabajo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PuestoDto),
            GuardarEn = nameof(IdPuesto),
            RestringidoPorControl = nameof(IdPermiso),
            MostrarExpresion = nameof(PuestoDto.Expresion),
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo,
            AplicarJoin = true,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Puesto { get; set; }




    }


}
