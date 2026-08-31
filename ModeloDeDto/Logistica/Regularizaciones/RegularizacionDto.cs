using Utilidades;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Logistica
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class RegularizacionDto : ElementoDeUnProcesoDto
    {

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public new string Cg { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del almacén",
            Visible = false
            )
        ]
        public int IdAlmacen { get; set; }

        [IUPropiedad(
            Etiqueta = "Almacén",
            Ayuda = "Almacén al que se refiere la regularización",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AlmacenDto),
            GuardarEn = nameof(IdAlmacen),
            MostrarExpresion = nameof(AlmacenDtm.Expresion),
            Controlador = nameof(enumControladoresLogistica.Almacenes),
            VistaDondeNavegar = enumVistasLogisticas.CrudAlmacenes,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Almacen) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            BuscarPor = ltrDeUnAlmacen.FiltrarParaRegularizar,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.porReferencia,
            Negocio = enumNegocio.Almacen,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            VisibleEnGrid = true,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_Tras_Seleccionar_Almacen) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_Tras_Blanquear_Almacen) + "()"
            )
        ]
        public string Almacen { get; set; }

    }


}
