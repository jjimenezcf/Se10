using System;

namespace ModeloDeDto.SistemaDocumental;

public class ArchivoMovilOutput : ElementoDto
{
    public string Nombre { get; set; }
    public DateTime CreadoEl { get; set; }
    public string Creador { get; set; }
}