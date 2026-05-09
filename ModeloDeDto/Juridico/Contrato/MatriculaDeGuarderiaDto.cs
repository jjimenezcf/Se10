using ModeloDeDto.Guarderias;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class MatriculaDeGuarderiaDto : EsUnaAmpliacionDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del infante", Visible = false)]
        public int IdInfante { get; set; }

        [IUPropiedad(
            Etiqueta = "Niño",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(InfanteDto),
            GuardarEn = nameof(IdInfante),
            Controlador = nameof(enumControladoresGuarderias.Infantes),
            VistaDondeNavegar = enumVistasGuarderias.CrudInfantes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Infante) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            Negocio = enumNegocio.Infante,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            Obligatorio = false)
        ]
        public string Infante { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del curso", Visible = false)]
        public int IdCurso { get; set; }

        [IUPropiedad(
            Etiqueta = "Curso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CursoDeGuarderiaDto),
            GuardarEn = nameof(IdCurso),
            Controlador = nameof(enumControladoresGuarderias.CursosDeGuarderia),
            VistaDondeNavegar = enumVistasGuarderias.CrudCursosDeGuarderia,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.CursoDeGuarderia) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            Negocio = enumNegocio.CursoDeGuarderia,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 1,
            EditableAlCrear = true,
            EditableAlEditar = true,
            Obligatorio = false)
        ]
        public string Curso { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente a facturar",
         Visible = false
         )]
        public int? IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Facturar A",
            Ayuda = "Cliente a facturar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            SeleccionarDe = typeof(ClienteDto),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2,
            EditableAlCrear = true,
            EditableAlEditar = true,
            Obligatorio = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Seleccionar_ClienteDeGuarderia) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Blanquear_ClienteDeGuarderia) + "([" + nameof(enumParamTs.idLista) + "])")]
        public string Cliente { get; set; }


        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 0
            , Etiqueta = "Contacto"
            , Obligatorio = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 1
            , Etiqueta = "Teléfono"
            , Obligatorio = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Telefono { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 2
            , Etiqueta = "eMail"
            , Obligatorio = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string eMail { get; set; }

    }
}
