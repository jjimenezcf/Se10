using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto()]
    public class TotalesDeTareas: TotalesDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Información de totales",
           Ayuda = "muestra la información de los totales calculados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 7,
           Fila = 0,
           Columna = 0
            )
        ]
        public string Totales { get; set; }  
    }
}
