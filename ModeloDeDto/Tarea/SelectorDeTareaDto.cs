using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorDeTareaDto: ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la tarea seleccionada",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea",
            Ayuda = "Tarea a vincular",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            SeleccionarDe = typeof(TareaDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudTareas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            OrdenarListaDinamicaPor = nameof(TareaDtm.Referencia),
            Negocio = enumNegocio.Tarea,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(TareaDto.Expresion),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }
    }

}
