using ModeloDeDto.Terceros;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Guarderias
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = ExpresionAula, OpcionDeBorrar =true)]
    public class AulaDeGuarderiaDto : ElementoDto
    {
        public const string ExpresionAula = "Expresion";

        //----------------------------------------------
        [IUPropiedad(
         Etiqueta = "",
         Oculto = true,
         Obligatorio = false,
         VisibleEnGrid = false,
         Fila = 0,
         Columna = 0,
         Posicion = 0
         )]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(
         Etiqueta = "Id del Cg",
         Visible = false
         )]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            Ordenar = true,
            OrdenarGridPor = nameof(Cg) + "."  + nameof(CentroGestorDtm.Codigo),
            VisibleEnEdicion = true,
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Cg { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Aula",
            Ayuda = "Referencia del aula",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true
            )
        ]
        public string Nombre { get; set; }

        [IUPropiedad(Visible = false)] public string Expresion { get; set; }

    }
}
