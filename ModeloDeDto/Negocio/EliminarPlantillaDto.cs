using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class EliminarPlantillaDto 
    {
        [IUPropiedad(Etiqueta = "Id la plantilla", Visible = false)]
        public int? IdPlantilla { get; set; }

        [IUPropiedad(
            Etiqueta = "Plantilla",
            Ayuda = "Seleccione la plantilla a eliminar",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(PlantillaDeCreacionDto),
            Controlador = nameof(enumControladoresNegocio.PlantillasDeCreacion),
            MostrarExpresion = nameof(PlantillaDeCreacionDto.Plantilla),
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

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Remplazar",
           Ayuda = "En lugar de eliminar actualiza la plantilla",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           Fila = 1, Columna = 0, Posicion = 1)
        ]
        public bool Remplazar { get; set; }
    }
}
