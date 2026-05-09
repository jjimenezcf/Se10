using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Terceros
{

    [IUDto(AnchoEtiqueta = 20)]
    public class SelectorDeInterDto: ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del interlocutor",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Interlocutor",
            Ayuda = "Quién actua de como interlocutor (En nombre de la sociedad o por un particular)",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Interlocutor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            OrdenarListaDinamicaPor = nameof(InterlocutorDtm.Nombre),
            Negocio = enumNegocio.Interlocutor,
            MostrarExpresion = nameof(InterlocutorDto.Expresion),
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
