using ModeloDeDto.RegistroEs;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorDeRegistroEsDto: ISelectorDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del registro seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Registro",
            Ayuda = "Registro a vincular",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresAdministrativos.RegistrosEs),
            SeleccionarDe = typeof(RegistroEsDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudRegistrosEs,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Registro) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            OrdenarListaDinamicaPor = nameof(RegistroEsDtm.Referencia),
            Negocio = enumNegocio.Registro,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(RegistroEsDto.Expresion),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }
    }

}
