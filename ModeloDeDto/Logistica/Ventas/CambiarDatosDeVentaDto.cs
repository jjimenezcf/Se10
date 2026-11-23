using System;
using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Logistica
{
    // Opción "Cambiar datos" del menú de edición del documento de venta: nombre, cliente y fecha de servicio, mientras está solicitado o
    // servido sin prefacturar ni devoluciones. Al aceptar se llama a VentasController.epCambiarDatos (ver ExtensorDeVentas.CambiarDatos): si
    // está servido se rehacen las salidas del almacén a la nueva fecha y, si la oferta estaba impresa, se reimprime
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Cambiar datos")]
    public class CambiarDatosDeVentaDto
    {
        [IUPropiedad(Etiqueta = "Id del documento de venta", Visible = false)]
        public int IdElemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Nombre"
            , Ayuda = "Nombre del documento de venta"
            , TipoDeControl = enumTipoControl.Editor
            , Obligatorio = true
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 0
            , Columna = 0
            , AutoSpan = true)]
        public string Nombre { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del cliente", Visible = false)]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién se le vende",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            SeleccionarDe = typeof(ClienteDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            // el cliente ocupa el resto de la fila y la fecha de servicio, a la derecha, un ancho fijo de 200px
            GridTemplateColumnsDeLaFila = "1fr 200px",
            Obligatorio = true,
            EditableAlCrear = true,
            EditableAlEditar = true)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Servido el"
            , Ayuda = "Fecha en la que se sirve (la de las salidas del almacén): entre la fecha de solicitud y hoy"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 1
            , Columna = 1)]
        public DateTime? ServidoEl { get; set; }
    }
}
