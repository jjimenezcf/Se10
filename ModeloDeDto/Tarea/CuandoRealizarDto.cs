using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CuandoRealizarDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la tarea editada", Oculto = true, Fila = 1, Columna = 0)]
        public int IdTareaEditada { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuándo realizar",
            Ayuda = "Indica si la tarea editada se ha de ejecutar antes o después de la tarea seleccionada",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumCuandoRealizar),
            GuardarEn = nameof(CuandoRealizar),
            Obligatorio = true,
            Fila = 0,
            Columna = 0
            )
        ]
        public string CuandoRealizar { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la tarea seleccionada", Visible = false)]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea",
            Ayuda = "Tarea con la que se relaciona la tarea editada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            SeleccionarDe = typeof(TareaDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudTareas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Tarea,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(TareaDto.Expresion),
            Tipo = typeof(string),
            Obligatorio = true,
            Fila = 0,
            Columna = 1)
        ]
        public string Elemento { get; set; }
    }
}
