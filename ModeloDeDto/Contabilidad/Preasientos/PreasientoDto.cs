using ModeloDeDto.Gastos;
using System;
using Utilidades;

namespace ModeloDeDto.Contabilidad

{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), OpcionDeBorrar = false, OpcionDeCrear = false)]
    public class PreasientoDto : ElementoDeUnProcesoDto
    {
        [IUPropiedad(
           TipoDeControl = enumTipoControl.Editor,
           Etiqueta = "Diario",
           Fila = 3, Columna = 0,
           VisibleAlCrear = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           VisibleEnGrid = true)]
        public string CodigoDiario { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TipoDeControl = enumTipoControl.Editor,
           Etiqueta = "Sociedad Contable",
           Fila = 3, Columna = 1,
           VisibleAlCrear = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           VisibleEnGrid = true)]
        public string SociedadContable { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TipoDeControl = enumTipoControl.Editor,
           Etiqueta = "Ejercicio",
           Fila = 3, Columna = 2,
           VisibleAlCrear = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           VisibleEnGrid = true)]
        public int Ejercicio { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TipoDeControl = enumTipoControl.Editor,
           Etiqueta = "Importe",
           Fila = 3, Columna = 4,
           VisibleAlCrear = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           Formato = enumFormato.Moneda,
           VisibleEnGrid = true)]
        public decimal Importe { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Ir a"
         , EtiquetaGrid = "Ir a"
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.Preasiento_IrAlOrigenDeLaFila) + "(numeroDeFila)"
         , Alineada = enumAliniacion.izquierda)]
        public string Origen { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Fecha contable"
            , EtiquetaGrid = "Ctbl. el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PreasientoDto.FechaContable)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , EditableAlCrear = false
            , EditableAlEditar = true
            , Fila = 3
            , Columna = 5)]
        public DateTime FechaContable { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "descripción detallada [Negocio]",
           CssDelContenedor = enumCssControles.ContenedorAreaDeTextoCentrado,
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           VisibleAlCrear = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           Obligatorio = false,
           CssDelArea = enumCssControles.MonoSpaceText,
           NumeroDeFilas = 5,
           Fila = 6,
           Columna = 0,
           LongitudMaxima = 1999,
           AutoSpan = true
          )
        ]
        public new string Descripcion { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Ir al origen del preasiento",
            Ayuda = "navega al origen del preasiento",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 7,
            Columna = 0,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.RefApilado,
            VisibleAlCrear = false,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.Preasiento_IrAlOrigenPorId) + "([" + nameof(Id) + "])",
            Posicion = 1
            )
        ]
        public string IraOrigen { get; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Ir al lote contable",
            Ayuda = "navegar al lote contable del preasiento",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 8,
            Columna = 0,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.RefApilado,
            VisibleAlCrear = false,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.Preasiento_IrAlLoteContable) + "([" + nameof(Id) + "])",
            Posicion = 1
            )
        ]
        public string IrAlLoteContable { get; }
    }


}
