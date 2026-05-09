
namespace ModeloDeDto.Terceros
{

    public static class ltrClasesDeJuzgado
    {
        public static readonly string Clases = "Clases de juzgado";
        public static readonly string Clase = "Clase de juzgado";
    }



    [IUDto]
    public class ClaseDeJuzgadoDto: ElementoDto
    {
        public const string MostrarExpresion = "[Nombre]";

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Clase de juzgado",
            EditableAlEditar =true,
            Ordenar = true
            )
        ]
        public string Nombre { get; set; }
    }
}
