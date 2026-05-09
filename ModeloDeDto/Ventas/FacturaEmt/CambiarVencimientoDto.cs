using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CambiarVencimientoDto: ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura seleccionada",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "factura seleccionada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            SeleccionarDe = typeof(FacturaEmtDto),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaEmitida) + ";" + nameof(enumModoDeAccesoDeDatos.Interventor),
            Negocio = enumNegocio.FacturaEmitida,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(FacturaEmtDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_MapearFechaDeVencimiento) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Vence el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 0
            , Columna = 1
            , Posicion = 0)]
        public DateTime? VenceEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Nuevo vencimiento"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 0
            , Columna = 1
            , Posicion = 1)]
        public DateTime? NuevoVencimiento { get; set; }

    }
}
