using System;
using ModeloDeDto.Expediente;
using ServicioDeDatos.Expediente;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Recepción de todas las líneas del pedido con lo pedido en cada una, se piden en la modal de entrega completa (ver PedidosController.epRecepcionarPedido)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Recepcionar pedido")]
    public class RecepcionarPedidoDto
    {
        //--------------------------------------------
        // Fila 0: el pedido; fila 1: destino (obra del expediente o almacén); fila 2: fecha de recepción
        [IUPropiedad(
            Etiqueta = "Pedido",
            Ayuda = "Pedido a recepcionar",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Pedido),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresLogistica.Pedidos),
            VistaDondeNavegar = enumVistasLogisticas.CrudPedidos
            )
        ]
        public int IdPedido { get; set; }

        [IUPropiedad(Etiqueta = "Pedido", Visible = false)]
        public string Pedido { get; set; }

        //--------------------------------------------
        // Destino de las entregas: el almacén o la obra del expediente, nunca ambos (al seleccionar uno se blanquea el otro).
        // Se propone el de la cabecera del pedido (ver CrudDePedidos.AbrirModalDeRecepcionarPedido)
        [IUPropiedad(Etiqueta = "Id del expediente", Visible = false)]
        public int? IdExpediente { get; set; }

        [IUPropiedad(
            Etiqueta = "Obra del expediente",
            Ayuda = "expediente a cuya obra van las entregas",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ExpedienteDto),
            GuardarEn = nameof(IdExpediente),
            MostrarExpresion = nameof(ExpedienteDtm.Expresion),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrDeUnExpediente.SelectorParaUnPedido + Simbolos.PuntoComa + nameof(enumEtapasDeExpedientes.EXP_Etapa_Ejecucion) + Simbolos.separadorDeEtapas + nameof(enumEtapasDeExpedientes.EXP_Etapa_Terminada),
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Pedido_Recepcion_TrasSeleccionarExpediente) + "()"
            )
        ]
        public string Expediente { get; set; }

        [IUPropiedad(Etiqueta = "Id del almacén", Visible = false)]
        public int? IdAlmacen { get; set; }

        [IUPropiedad(
            Etiqueta = "Almacén",
            Ayuda = "Almacén al que van las entregas",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(AlmacenDto),
            Controlador = nameof(enumControladoresLogistica.Almacenes),
            GuardarEn = nameof(IdAlmacen),
            MostrarExpresion = nameof(AlmacenDto.Expresion),
            Obligatorio = false,
            Fila = 1,
            Columna = 1,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Pedido_Recepcion_TrasCambiarAlmacen) + "()"
            )
        ]
        public string Almacen { get; set; }

        //--------------------------------------------
        [IUPropiedad(
              Etiqueta = "Recepcionado El"
            , Ayuda = "Fecha en la que se ha recibido el pedido: no puede ser posterior a hoy ni anterior a la creación del pedido"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Obligatorio = true
            , Fila = 2
            , Columna = 1
            , MantenerHuecoDeLaIzquierda = true )]
        public DateTime? RecepcionadoEl { get; set; }
    }
}
