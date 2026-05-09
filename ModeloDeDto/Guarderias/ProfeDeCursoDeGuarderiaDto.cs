
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Guarderias
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ProfeDeCursoDeGuarderiaDto : EsUnDetalleDto
    {
        //----------------------------------------------
        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public int? IdCg { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del trabajador", Visible = false)]
        public int IdTrabajador { get; set; }

        [IUPropiedad(
            Etiqueta = "Profesor de apoyo",
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
    }
}
