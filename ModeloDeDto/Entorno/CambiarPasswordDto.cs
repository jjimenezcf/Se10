using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CambiarPasswordDto : ElementoDto
    {
        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Actual",
            Ayuda = "Indique la password actual",
            TipoDeControl = enumTipoControl.Password,
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 1,
            Columna = 0)]
        public string Actual { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Nueva",
            Ayuda = "Contraseña segura: 8 caracteres con al menos un número y una mayúscula",
            TipoDeControl = enumTipoControl.Password,
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 2,
            Columna = 0)]
        public string Nueva { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Repetir",
            Ayuda = "repítala",
            TipoDeControl = enumTipoControl.Password,
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 3,
            Columna = 0)]
        public string Repetida { get; set; }


    }
}
