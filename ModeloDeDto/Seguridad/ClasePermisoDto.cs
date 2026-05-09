

namespace ModeloDeDto.Seguridad
{
    [IUDto]
    public class ClasePermisoDto: ElementoDto
    {
        public const string MostrarExpresion = "[Nombre]([Id])";

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Clase de permiso",
            Ordenar = true
            )
        ]
        public string Nombre { get; set; }
    }
}
