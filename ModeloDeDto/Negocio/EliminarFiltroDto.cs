using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class EliminarFiltroDto 
    {
        [IUPropiedad(Etiqueta = "Id la plantilla", Visible = false)]
        public int? IdPlantilla { get; set; }

        [IUPropiedad(
            Etiqueta = "Plantilla",
            Ayuda = "Seleccione la plantilla a eliminar",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(PlantillaDeFiltradoDto),
            Controlador = nameof(enumControladoresNegocio.PlantillasDeFiltrado),
            MostrarExpresion = nameof(PlantillaDeFiltradoDto.Plantilla),
            GuardarEn = nameof(IdPlantilla),
            CargarBajoDemanda = true,
            AutoSpan = true,
            Fila = 0,
            Columna = 0,
            Obligatorio = false,
            AutoPosicionamiento = true
            )
        ]
        public string Plantilla { get; set; }
    }
}
