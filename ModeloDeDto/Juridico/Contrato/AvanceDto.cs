using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class AvanceDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Planificado",
           Tipo = typeof(decimal),
           Ayuda = "Planificado del contrato",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlEditar = false,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 0)
        ]
        public decimal Planificado { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Realizado",
           Tipo = typeof(decimal),
           Ayuda = "Realizado del contrato",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlEditar = false,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 1)
        ]
        public decimal Realizado { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Facturado",
           Tipo = typeof(decimal),
           Ayuda = "Facturado del contrato",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlEditar = false,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           Fila = 1,
           Columna = 0)
        ]
        public decimal Facturado { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "Cobrado del contrato",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1)
        ]
        public decimal Cobrado { get; set; }
    }
}
