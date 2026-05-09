using Utilidades;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PuestoDeClienteDto : EsUnDetalleDto
    {

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdSociedad { get; set; }


        [IUPropiedad(
            Etiqueta = "Sociedad",
            Ayuda = "Sociedad a la que el cliente puede acceder",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(SociedadDto),
            GuardarEn = nameof(IdSociedad),
            Controlador = nameof(enumControladoresTerceros.Sociedades),
            VistaDondeNavegar = enumVistasTerceros.CrudSociedades,
            MostrarExpresion = nameof(SociedadDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            BuscarPor = ltrDeSociedad.FiltroParaSociedadesDeClientes,
            RestringidoPorControl = nameof(IdElemento),
            Fila = 1,
            Columna = 0,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public string Sociedad { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdCg { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Nombre del puesto",
            Ayuda = "PT a crear en ([COD.CTBL]" +  "." + ltrDeSociedad.CodigoCgClienteWeb + ") " + ltrDeSociedad.CentroGestorDeClientesWeb,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            LongitudMaxima = 20,
            Ordenar = true
            )
        ]
        public string Puesto { get; set; }
    }
}
