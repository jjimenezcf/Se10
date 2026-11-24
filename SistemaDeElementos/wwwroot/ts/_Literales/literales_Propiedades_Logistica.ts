// Propiedades de los dto (ltrPropiedades) de logística. El namespace ltrPropiedades se fusiona con el de literales_Propiedades.ts.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const Logistica = {
        // información contable del almacén (ver AlmacenDto)
        Almacen: {
            UsaVariacionDeExistencias: 'UsaVariacionDeExistencias',
            CuentaDeCompras: 'CuentaDeCompras',
            CuentaDeDevoluciones: 'CuentaDeDevoluciones'
        },
        Pedido: {
            idExpediente: 'idexpediente',
            Expediente: 'Expediente',
            IdAlmacen: 'IdAlmacen',
            Almacen: 'Almacen',
            IdProveedor: 'idproveedor',
            Etapas: 'Etapas',
            Contrato: 'Contrato',
            IdNaturaleza: 'IdNaturaleza',
            IdUnidadDelProveedor: 'IdUnidadDelProveedor',
            ConceptoDelProveedor: 'ConceptoDelProveedor',
            BiPropuestoDelProveedor: 'BiPropuestoDelProveedor',
            UsaTarifaDelProveedor: 'UsaTarifaDelProveedor',
            SelectorNaturaleza: 'Naturaleza',
            ClaseDeLinea: 'ClaseDeLinea',
            PedidoEl: 'PedidoEl',
            EntregarEl: 'EntregarEl',
            RecibidoEl: 'RecibidoEl',
            CerradoEl: 'CerradoEl',
            eMail: 'eMail',
            Indicadores: {
                Naturaleza: 'Naturaleza',
                UnidadDeMedida: 'UnidadDeMedida',
                TipoDeLinea: 'TipoDeLinea'
            },
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeLinea: 'ImporteDeLinea',
                precio: 'precio',
                cantidad: 'cantidad',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                CantidadPendiente: 'CantidadPendiente'
            },
            RecepcionarLinea: {
                IdLineaPedido: 'IdLineaPedido',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Descuento: 'Descuento',
                ImporteEntregado: 'ImporteEntregado',
                IdAlmacen: 'IdAlmacen',
                Almacen: 'Almacen',
                IdExpediente: 'IdExpediente',
                Expediente: 'Expediente',
                RecepcionadaEl: 'RecepcionadaEl'
            },
            RecepcionarPedido: {
                IdPedido: 'IdPedido',
                RecepcionadoEl: 'RecepcionadoEl'
            },
            DevolverEntrega: {
                IdLineaPedido: 'IdLineaPedido',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Descuento: 'Descuento',
                Importe: 'Importe',
                DevueltaEl: 'DevueltaEl',
                IdAlmacen: 'IdAlmacen',
                IdExpediente: 'IdExpediente',
                Motivo: 'Motivo'
            },
            // filas del grid de entregas (EntregaDeUnPedidoDto)
            Entrega: {
                IdLinea: 'IdLinea',
                RecepcionadaEl: 'RecepcionadaEl',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Descuento: 'Descuento',
                IdAlmacen: 'IdAlmacen',
                Almacen: 'Almacen',
                IdExpediente: 'IdExpediente',
                Expediente: 'Expediente',
                CantidadDevuelta: 'CantidadDevuelta',
                Contabilizada: 'Contabilizada'
            },
            // filas del grid de devoluciones (DevolucionDeUnPedidoDto)
            Devolucion: {
                DevueltaEl: 'DevueltaEl',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Descuento: 'Descuento',
                IdAlmacen: 'IdAlmacen',
                Almacen: 'Almacen',
                IdExpediente: 'IdExpediente',
                Expediente: 'Expediente',
                Motivo: 'Motivo',
                Contabilizada: 'Contabilizada'
            }
        },
        // filtros del crud de movimientos de almacén (ver ltrDeUnMovimientoDeAlmacen)
        MovimientosDeAlmacen: {
            FiltroPorAlmacen: 'FiltroPorAlmacen'
        },
        Inventario: {
            FiltroPorFechaDeInventario: 'FiltroPorFechaDeInventario',
            FiltroPorAlmacen: 'FiltroPorAlmacen',
            // propiedad del almacén en la modal de preparar el recuento (ver PrepararRecuentoDto)
            AlmacenDelRecuento: 'Almacen',
            // clase de regularización para la que se prepara el Excel
            ClaseDelRecuento: 'Clase',
            // propiedades de la modal de venta de material (ver CrearVentaDto)
            Venta: {
                IdAlmacen: 'IdAlmacen',
                IdUnitario: 'IdUnitario',
                Cliente: 'Cliente',
                Fecha: 'Fecha',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Total: 'Total',
                Servido: 'Servido',
                Facturar: 'Facturar'
            }
        },
        Regularizacion: {
            Almacen: 'almacen',
            tipo: {
                Clase: 'ClaseDeRegularizacion'
            },
            Recuento: {
                IdRegularizacion: 'IdRegularizacion',
                Fecha: 'Fecha'
            },
            linea: {
                orden: 'orden',
                fecha: 'fecha',
                unitario: 'unitario',
                cantidad: 'cantidad',
                precio: 'precio',
                total: 'total'
            }
        },
        Venta: {
            Almacen: 'almacen',
            IdCliente: 'idcliente',
            linea: {
                orden: 'orden',
                unitario: 'unitario',
                cantidad: 'cantidad',
                precio: 'precio',
                descuento: 'descuento',
                importe: 'importe',
                cantidadDevuelta: 'cantidaddevuelta'
            },
            DevolverLinea: {
                IdLineaDeVenta: 'IdLineaDeVenta',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Descuento: 'Descuento',
                Importe: 'Importe',
                IdDevolucion: 'IdDevolucion',
                Devolucion: 'Devolucion'
            },
            // grid de devoluciones de la edición del documento de venta (ver DescriptorDeVentas.DescriptorDeDevolucionesDeUnaVenta)
            Devolucion: {
                IdDevolucion: 'IdDevolucion',
                idElemento: 'idelemento',
                aceptada: 'aceptada',
                cancelada: 'cancelada',
                devolucion: 'devolucion',
                Aceptar: 'Aceptar',
                Cancelar: 'Cancelar'
            },
            ModificarLineaServida: {
                IdLineaDeVenta: 'IdLineaDeVenta',
                Descuento: 'Descuento',
                Cantidad: 'Cantidad',
                Precio: 'Precio',
                Importe: 'Importe'
            },
            // modal de "Cambiar datos" del menú de edición (ver CambiarDatosDeVentaDto) y lo que se lee del documento de venta editado
            CambiarDatos: {
                IdElemento: 'IdElemento',
                Nombre: 'Nombre',
                Cliente: 'Cliente',
                ServidoEl: 'ServidoEl',
                tieneDevoluciones: 'tienedevoluciones',
                idFacturaEmt: 'idfacturaemt'
            }
        },
        DevolucionDeVenta: {
            FiltroEnTramitacion: 'FiltroEnTramitacion',
            FiltroPorCliente: 'FiltroPorCliente',
            linea: {
                orden: 'orden',
                venta: 'venta',
                lineaDeVenta: 'lineadeventa',
                cantidad: 'cantidad',
                precio: 'precio',
                descuento: 'descuento',
                importe: 'importe'
            }
        },
    };
}
