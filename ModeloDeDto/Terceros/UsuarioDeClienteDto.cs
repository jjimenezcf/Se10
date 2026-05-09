using Utilidades;
using ServicioDeDatos;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Terceros;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class UsuarioDeClienteDto : EsUnDetalleDto
    {

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdUsuario { get; set; }


        [IUPropiedad(
            Etiqueta = "Usuario",
            Ayuda = "Seleccione el usuario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdUsuario),
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.noContiene,
            BuscarPor = ltrUsuariosDeUnCliente.FiltroPorUsuarioDeCliente,
            RestringidoPorControl = nameof(IdElemento),
            Fila = 1,
            Columna = 0,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public string Usuario { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool Activo { get; set; }

    }
}
