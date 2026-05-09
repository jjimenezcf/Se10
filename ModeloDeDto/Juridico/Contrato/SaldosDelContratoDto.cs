using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class SaldosDelContratoDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe contrato",
           Tipo = typeof(decimal),
           Ayuda = "Importe inicial del contrato",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlEditar = true,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 0)
        ]
        public decimal Importe { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe adendado",
           Tipo = typeof(decimal),
           Ayuda = "Importe adendado al contrato",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlEditar = false,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           ValorPorDefecto = 0,
           Fila = 0,
           Columna = 1)
        ]
        public decimal Adendado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Saldo del contrato",
           Tipo = typeof(decimal),
           Ayuda = "Resto del contrato",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 0,
           Columna = 2
            )
        ]
        public decimal Saldo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Avisar al sobrepasar el (%) del importe",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de aviso",
           TipoDeControl = enumTipoControl.Editor,
           Formato = enumFormato.Porcentaje,
           Fila = 1,
           Columna = 0)
        ]
        public decimal Aviso { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Bloquear al sobrepasar el (%) del importe",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de bloqueo",
           TipoDeControl = enumTipoControl.Editor,
           Formato = enumFormato.Porcentaje,
           Fila = 1,
           Columna = 1)
        ]
        public decimal Bloqueo { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Aviso enviado",
            Ayuda = "indica si se ha enviado el aviso de sobrepasar el %",
            Fila = 2,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            css = enumCssControles.ControlApilado,
            Alineada = enumAliniacion.derecha,
            MantenerHuecoDeLaIzquierda = true,
            ValorPorDefecto = false,
            EditableAlEditar = false
            )
        ]
        public bool? Notificado { get; set; }
    }
}
