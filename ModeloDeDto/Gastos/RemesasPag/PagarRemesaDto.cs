using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class PagarRemesaDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la remesa seleccionada",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Remesa",
            Ayuda = "remesa seleccionada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresGastos.RemesasPag),
            SeleccionarDe = typeof(RemesaPagDto),
            VistaDondeNavegar = enumVistasGastos.CrudRemesasPag,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.RemesaPag) + ";" + nameof(enumModoDeAccesoDeDatos.Interventor),
            Negocio = enumNegocio.RemesaPag,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(RemesaPagDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Rem_MapearFechaDePago) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Pagar el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 0
            , Columna = 1
            , Posicion = 0)]
        public DateTime? PagarEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Pagada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 0
            , Columna = 1
            , Posicion = 1)]
        public DateTime? PagadaEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Incluidos",
           Tipo = typeof(int),
           Ayuda = "Pagos incluidos en la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Posicion = 0
            )
        ]
        public decimal? Incluidos { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Anulados",
           Tipo = typeof(int),
           Ayuda = "Pagos no realizados de la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Posicion = 0
            )
        ]
        public decimal? Anulados { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "Importe de la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 1,
           Columna = 1,
           Posicion = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? ImporteRemesa { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagado",
           Tipo = typeof(decimal),
           Ayuda = "importe de la remesa pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           Fila = 1,
           Columna = 1,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Pagado { get; set; }

    }
}
