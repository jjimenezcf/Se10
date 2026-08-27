using Utilidades;

namespace ModeloDeDto.Logistica
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class AlmacenDto : ElementoDeUnProcesoDto
    {

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Direcciones",
           EtiquetaGrid = "Direcciones",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnEdicion = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15
            )
        ]
        public string Direcciones { get; set; }

    }


}
