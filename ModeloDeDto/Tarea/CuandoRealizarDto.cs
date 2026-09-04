using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CuandoRealizarDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la tarea editada", Oculto = true, Fila = 1, Columna = 0)]
        public int IdTareaEditada { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la tarea anterior", Visible = false)]
        public int IdTareaAnterior { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea anterior",
            Ayuda = "Tarea que se ha de ejecutar antes que la tarea editada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTareaAnterior),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            SeleccionarDe = typeof(TareaDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudTareas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor) + "|" +
                             ltrFiltros.FiltroPorEtapa + ";" + nameof(enumEtapasDeTareas.TAR_Etapa_Inicial),
            OtrosParametrosDeFiltrado = "javascript: " + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Tar_FiltrosParaCuandoRealizar) + "(this)",
            Negocio = enumNegocio.Tarea,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(TareaDto.Expresion),
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 0,
            Columna = 0)
        ]
        public string TareaAnterior { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la tarea posterior", Visible = false)]
        public int IdTareaPosterior { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea posterior",
            Ayuda = "Tarea que se ha de ejecutar después que la tarea editada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTareaPosterior),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            SeleccionarDe = typeof(TareaDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudTareas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor) + "|" +
                             ltrFiltros.FiltroPorEtapa + ";" + nameof(enumEtapasDeTareas.TAR_Etapa_Inicial),
            OtrosParametrosDeFiltrado = "javascript: " + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Tar_FiltrosParaCuandoRealizar) + "(this)",
            Negocio = enumNegocio.Tarea,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(TareaDto.Expresion),
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 0,
            Columna = 1)
        ]
        public string TareaPosterior { get; set; }
    }
}
