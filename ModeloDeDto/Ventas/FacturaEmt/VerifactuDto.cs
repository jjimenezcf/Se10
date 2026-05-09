using Utilidades;


namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class VerifactuDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "Archivo con la auditoría de comunicación",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Archivador),
            Fila = 0,
            Columna = 0,
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public int IdArchivador { get; set; }

        [IUPropiedad(Visible = false)]
        public string Archivador { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "BlockChain",
            Ayuda = "Archivador con los blockchain generados en el ejercicio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(BlockChain),
            Fila = 0,
            Columna = 1,
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public int IdBlockChain { get; set; }

        [IUPropiedad(Visible = false)]
        public string BlockChain { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "CSV",
           Tipo = typeof(string),
           Ayuda = "Codigo CSV generado por la AEAT",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0, Posicion = 0)
        ]
        public string Csv { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Url de verificación",
           Tipo = typeof(string),
           Ayuda = "Para validar que todo es ok",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1, Posicion = 0)
        ]
        public string Url { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Huella",
           Tipo = typeof(string),
           Ayuda = "Huella registrada en la AEAT",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1, Posicion = 1)
        ]
        public string Huella { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Respuesta de registro",
           Tipo = typeof(string),
           Ayuda = "Respuesta dada al registrar en la AEAT",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 0)
        ]
        public string Respuesta { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Validar en AEAT",
            Ayuda = "navega a la AEAT para validar la emisión",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_ValidarEnAeat) + "()"
            )
        ]
        public string ValidarEnAeat { get; } = "Validar en AEAT";


        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cancelado",
            Ayuda = "indica si se ha cancelado el registro de la AEAT",
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Fila = 3,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool Cancelada { get; set; }

    }
}
