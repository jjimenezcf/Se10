using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDePlanificacionDeVentaDto : TipoDeElementoDto
    {
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia la planificación de venta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.PlanificacionDeVenta,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Estado { get; set; }
    }
}
