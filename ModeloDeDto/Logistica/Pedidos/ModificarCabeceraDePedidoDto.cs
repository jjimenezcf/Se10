using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Datos de la cabecera que se pueden modificar una vez que el pedido ha dejado de cumplimentarse (ver GestorDePedidos.ModificarCabecera)
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ModificarCabeceraDePedidoDto : IRenombrarDto
    {
        [IUPropiedad(
            Etiqueta = "Pedido",
            Ayuda = "Pedido a modificar",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresLogistica.Pedidos),
            VistaDondeNavegar = enumVistasLogisticas.CrudPedidos
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Etiqueta = "Pedido", Visible = false)]
        public string Elemento { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Pedido",
            Ayuda = "Nombre del pedido",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "eMail",
            Ayuda = "Correo al que se envía el pedido",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = false
          )
        ]
        public string eMail { get; set; }

        [IUPropiedad(
            Etiqueta = "Guardar en el proveedor",
            Ayuda = "Si el correo es distinto al del proveedor, se guarda como su nuevo correo de contacto",
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            Fila = 2,
            Columna = 1
            )
        ]
        public bool GuardarEnElProveedor { get; set; }

        //----------------------------------------------
        [IUPropiedad(
              Etiqueta = "Entregar el"
            , Ayuda = "Fecha de entrega solicitada al proveedor"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Fila = 3
            , Columna = 0)]
        public DateTime? EntregarEl { get; set; }

        [IUPropiedad(
              Etiqueta = "Recibido el"
            , Ayuda = "Fecha de recepción, sólo una vez recibido el pedido: no puede ser posterior a hoy ni anterior a la fecha en la que se creó"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Fila = 3
            , Columna = 1)]
        public DateTime? RecibidoEl { get; set; }

        [IUPropiedad(
              Etiqueta = "Cerrado el"
            , Ayuda = "Fecha de cierre, sólo una vez cerrado el pedido: no puede ser posterior a hoy ni anterior a la de recepción"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Fila = 3
            , Columna = 2)]
        public DateTime? CerradoEl { get; set; }

        // Versión del pedido que se ve en la edición, de donde se toman los datos de la modal: si otro usuario lo ha modificado no se guarda
        [IUPropiedad(Visible = false)]
        public string RowVersion { get; set; }
    }
}
