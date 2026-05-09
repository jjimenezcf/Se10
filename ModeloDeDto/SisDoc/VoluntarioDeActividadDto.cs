using ModeloDeDto.Terceros;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class VoluntarioDeActividadDto :  EsUnDetalleDto
    {
        [IUPropiedad(Etiqueta = "Id del interlocutor", Visible = false)]
        public int IdInterlocutor { get; set; }

        [IUPropiedad(
            Etiqueta = "Voluntario",
            AyudaDeCriteriosDeBusqueda = "seleccione por nombre del voluntario, dni, correo o teléfono de la persona",
            GuardarEn = nameof(IdInterlocutor),
            BuscarPor = ltrInterlocutor.BuscarPorPersona,
            MostrarExpresion = nameof(InterlocutorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            SeleccionarDe = typeof(InterlocutorDto),
            Negocio = enumNegocio.Interlocutor,
            SoloEnAlta = true,
            EditableAlCrear = true,
            EditableAlEditar = true,
            TipoDeControl = enumTipoControl.ListaDinamica,
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            AutoSpan = true
          )
        ]
        public string Interlocutor { get; set; }
    }
}
