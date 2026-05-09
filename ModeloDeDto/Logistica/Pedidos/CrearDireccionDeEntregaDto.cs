using ModeloDeDto.Negocio;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    public class CrearDireccionDeEntregaDto: CrearDireccionDto
    {        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Calificador",
            Ayuda = "Calificador de direcciones",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumCalificadorDireccion),
            GuardarEn = nameof(Calificador),
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false,
            AutoSpan = false,
            EditableAlCrear = false,
            ValorPorDefecto = enumCalificadorDireccion.entrega
          )
        ]
        public override string Calificador { get; set; }
    }
}
