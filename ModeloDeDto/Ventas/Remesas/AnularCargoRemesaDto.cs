using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class AnularCargoRemesaDto : ISelectorDto
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
            Controlador = nameof(enumControladoresVentas.RemesasFae),
            SeleccionarDe = typeof(RemesaFaeDto),
            VistaDondeNavegar = enumVistasVentas.CrudRemesasFae,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.RemesaFae) + ";" + nameof(enumModoDeAccesoDeDatos.Interventor),
            Negocio = enumNegocio.RemesaFae,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(RemesaFaeDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Rem_MapearFechaDeCargo) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Cargar el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 0
            , Columna = 1
            , Posicion = 0)]
        public DateTime? CargarEl { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Cargada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 0
            , Columna = 1
            , Posicion = 1)]
        public DateTime? CargadaEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Incluidas",
           Tipo = typeof(int),
           Ayuda = "facturas incluidas en la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Posicion = 0
            )
        ]
        public decimal? Incluidas { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Devueltas",
           Tipo = typeof(int),
           Ayuda = "facturas devueltas de la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Posicion = 0
            )
        ]
        public decimal? Devueltas { get; set; }


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
           Fila =1,
           Columna = 1,
           Posicion = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? ImporteRemesa { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "importe cobrado",
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
        public decimal? Cobrado { get; set; }

    }
}
