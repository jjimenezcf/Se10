
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using System.Linq.Expressions;
using Utilidades;

namespace ModeloDeDto.Guarderias
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class InfanteDeUnCursoDto : EsUnDetalleDto
    {
        //----------------------------------------------
        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public int? IdCg { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del infante", Visible = false)]
        public int IdInfante { get; set; }

        [IUPropiedad(
            Etiqueta = "Niño",
            Ayuda = "seleccione el niño",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(InfanteDto),
            GuardarEn = nameof(IdInfante),
            Controlador = nameof(enumControladoresGuarderias.Infantes),
            VistaDondeNavegar = enumVistasGuarderias.CrudInfantes,
            BuscarPor = ltrDeInfante.SeleccionarParaUnCurso,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true)
        ]
        public string Infante { get; set; }
    }
}
