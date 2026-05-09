using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorDeArchivadorDto: ISelectorDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del archivador seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "Archivador a vincular",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            SeleccionarDe = typeof(ArchivadorDto),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores, 
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            OrdenarListaDinamicaPor = nameof(ArchivadorDtm.Referencia),
            Negocio = enumNegocio.Archivador,
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }
    }

}
