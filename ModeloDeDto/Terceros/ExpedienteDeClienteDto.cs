using Utilidades;
using ServicioDeDatos;
using ModeloDeDto.Expediente;
using ServicioDeDatos.Expediente;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ExpedienteDeClienteDto : EsUnDetalleDto
    {

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdExpediente { get; set; }

        [IUPropiedad(
            Etiqueta = "Usuario",
            Ayuda = "Seleccione el usuario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ExpedienteDto),
            GuardarEn = nameof(IdExpediente),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            MostrarExpresion = nameof(ExpedienteDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            BuscarPor = nameof(ltrDeUnExpediente.IdCliente),
            RestringidoPorControl = nameof(IdElemento),
            Fila = 0,
            Columna = 0,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public string Expediente { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "CG",
            Ayuda = "CG del expediente",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true)]
        public string Cg { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "tipo del expediente",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true)]
        public string Tipo { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Estado",
            Ayuda = "estado del expediente",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 2,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true)]
        public string Estado { get; set; }

    }
}
