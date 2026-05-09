namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class PlantillaDeFiltradoDto : ElementoDto, IPlantillaDeUsuarioDto
    {
        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Plantilla",
          Ayuda = "Indique el nombre de la plantilla de filtrado",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 0,
          Ordenar = true,
          EditableAlEditar = false
          )
        ]
        public string Plantilla { get; set; }
    }
}
