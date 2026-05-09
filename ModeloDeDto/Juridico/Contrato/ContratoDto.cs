using ModeloDeDto.Entorno;
using ModeloDeDto.Guarderias;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class ContratoDto : ElementoDeUnProcesoDto
    {
        //--------------------------------------------
        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = true, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public enumClaseDeContrato ClaseDeContrato { get; set; }


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
            Fila = 2,
            Columna = 0,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            Obligatorio = false)
        ]
        public string Infante { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del infante", Visible = false)]
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
            Fila = 2,
            Columna = 1,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            Obligatorio = false)
        ]
        public string Curso { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del usuario responsable",
            Visible = false
            )
        ]
        public int? IdResponsable { get; set; }

        [IUPropiedad(
            Etiqueta = "Responsable del",
            Ayuda = "Usuario responsable del contrato",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdResponsable),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 10,
            Columna = 0,
            PorAnchoMnt = 15,
            VisibleEnGrid = false,
            EditableAlEditar = true,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false,
            EsAlmacenable = true
            )
        ]
        public string Responsable { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Expediente",
            Ayuda = "Expediente al que pertenece el contrato",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Expediente),
            Fila = 10,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            Obligatorio = false
            )
        ]
        public int? IdExpediente { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Expediente", Obligatorio = false)]
        public string Expediente { get; set; }

        [IUPropiedad(Visible = false)]
        public enumEtapasDeContratos Etapa { get; set; }

        [IUPropiedad(Visible = false, TamanoFijo = "20em", EtiquetaGrid ="Cliente" )]
        public string Cliente { get; set; }

        [IUPropiedad(Visible = false, TamanoFijo = "20em", EtiquetaGrid = "Proveedor")]
        public string Proveedor { get; set; }

        [IUPropiedad(Visible = false, TamanoFijo = "10em", EtiquetaGrid ="Importe" )]
        public decimal Importe { get; set; }

        [IUPropiedad(Visible = false, TamanoFijo = "10em", EtiquetaGrid = "Aval")]
        public decimal ImporteAval { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 10)]
        public DateTime? InicioContrato { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 10)]
        public DateTime? FinContrato { get; set; }


    }


}
