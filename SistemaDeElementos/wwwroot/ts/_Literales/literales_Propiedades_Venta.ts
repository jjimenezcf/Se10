// Propiedades de los dto (ltrPropiedades) de venta. El namespace ltrPropiedades se fusiona con el de literales_Propiedades.ts.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const Venta = {
        TipoPpt: {
            TipoFacturaEmt: 'tipofacturaemt',
            TipoParteTr: 'tipopartetr',
            ClaseDePpt: 'clasedepresupuesto'
        },
        Presupuesto: {
            id: literal.id,
            ivaPropuesto: 'iva',
            descuentoPropuesto: 'descuento',
            idExpediente: 'idexpediente',
            IdSolicitante: literal.IdSolicitante,
            Solicitante: literal.Solicitante,
            Expediente: 'Expediente',
            IdTipoFacturaPorDefecto: 'IdTipoFacturaPorDefecto',
            TipoFacturaPorDefecto: 'TipoFacturaPorDefecto',
            IdTipoPartePorDefecto: 'IdTipoPartePorDefecto',
            TipoPartePorDefecto: 'TipoPartePorDefecto',
            Etapas: 'Etapas',
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
                ivaPorLinea: 'iva',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                ElPrecioIncluyeElIva: 'ElPrecioIncluyeElIva',
                selectorDeIvaR: 'IvaRepercutido',
                precio: 'precio',
                cantidad: 'cantidad',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad'
            }
        },
        RenombraPpt: {
            IdElemento: literal.idElemento,
            Nombre: literal.nombre
        },
        PlfDeVenta: {
            Tipo: literal.Tipo,
            IdContrato: 'idcontrato',
            IdCliente: 'idcliente',
            Cliente: 'cliente',
            TipoDeParte: 'TipoDeParte',
            TipoDeFactura: 'TipoDeFactura',
            IdTipoDeFactura: 'IdTipoDeFactura',
            linea: {
                orden: 'orden',
                venta: 'venta',
                cantidad: 'cantidad',
                idnaturaleza: 'IdNaturaleza',
                idunidad: 'IdUnidad',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                unitario: 'unitario',
                selectorDeIvaR: 'IvaRepercutido',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                descuentoPorLinea: 'descuento',
                tipoDeLinea: 'tipoDeLinea',
                concepto: 'concepto',
                iva: 'iva',
            },
            ConOSinContrato: 'filtroporconosincontrato',
            ConOSinPlanificador: 'filtroporconosinplanificador'
        },
        ParteTr: {
            IdCliente: 'IdCliente',
            Cliente: 'Cliente',
            Etapa: 'Etapa',
            IdPresupuesto: 'idpresupuesto',
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                ivaPorLinea: 'iva',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                selectorDeIvaR: 'IvaRepercutido',
                precio: 'precio',
                cantidad: 'cantidad',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                iva: 'iva',
            },
            Asignacion: {
                Duracion: 'duracion',
                MedidoEn: 'medidoen',
                PlfDeInicio: 'plfdeinicio',
                Iniciada: 'iniciada',
                PlfDeFin: 'plfdefin',
                Finalizada: 'finalizada'
            },
            ConOSinContrato: 'filtroporconosincontrato',
            ConOSinTarea: 'filtroporconosintarea'
        },
        FacturaEmt: {
            Presupuesto: 'presupuesto',
            IdCliente: 'IdCliente',
            Cliente: 'Cliente',
            Contrato: 'Contrato',
            IdPresupuesto: 'idpresupuesto',
            ConOSinContrato: 'filtroporconosincontrato',
            TotalSinIva: 'totalsiniva',
            TotalIrpf: 'totalirpf',
            VenceEl: 'VenceEl',
            Etapas: 'Etapas',
            EsRectificativa: 'EsRectificativa',
            EsPeriodica: 'EsPeriodica',
            ConIrpf: 'ConIrpf',
            ClaseRectificativa: 'ClaseRectificativa',
            MotivoDeRectificacion: 'MotivoDeRectificacion',
            IdRectificativa: 'idRectificativa',
            Contacto: 'contacto',
            Telefono: 'telefono',
            eMail: 'email',
            IdParteTr: 'IdParteTr',
            UsaCentroAdministrativo: 'UsaCentroAdministrativo',
            CentroAdministrativo: 'CentroAdministrativo',
            EstaComunicandose: 'EstaComunicandose',
            tipo: {
                idNaturalezaDefecto: 'IdNaturalezaDefecto',
                idUnidadDefecto: 'IdUnidadDefecto'
            },
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                ivaPorLinea: 'iva',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                ElPrecioIncluyeElIva: 'ElPrecioIncluyeElIva',
                selectorDeIvaR: 'IvaRepercutido',
                precio: 'precio',
                cantidad: 'cantidad',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                iva: 'iva'
            },
            Irpf: {
                BiSujeta: 'BiSujeta',
                TipoIrpf: 'TipoIrpf',
                Irpf: 'Irpf',
                Importe: 'Importe'
            },
            Verifactu: {
                Url: 'url',
            },
            PorAbonar: 'PorAbonar',
            Cobro: {
                Cobrado: 'cobrado',
                Pendiente: 'pendiente',
                Clase: 'clase',
                CobradoEl: 'cobradoel',
                CuentaDeIngreso: 'CuentaDeIngreso'
            },
            Abono: {
                Abonado: 'cobrado', //usamos el mismo campo del json
                Importe: 'importe',
                Pendiente: 'pendiente',
                IdCliente: 'idcliente',
                Clase: 'clase',
                AbonadoEl: 'abonadoel',
                CuentaDeIngreso: 'CuentaDeCargo',
                CuentaDeAbono: 'CuentaDeAbono'
            },
            Indicadores: {
                UsaVerifactu: 'UsaVerifactu',
            }
        },
        PeticionDeFacturaEmt: {
            IdFactura: 'idfactura'
        },
        RemesaFae: {
            Acreedor: 'Acreedor',
            Presentador: 'Presentador',
            NifDelPresentador: 'NifDelPresentador',
            CuentaDeAbono: 'CuentaDeAbono',
            IdCuentaDeAbono: 'IdCuentaDeAbono',
            Entidad: 'Entidad',
            Oficina: 'Oficina',
            Etapas: 'Etapas',
            CargarEl: 'cargarel',
            CargadaEl: 'cargadael'
        },
        FacturasEmtDeUnaRemesa: {
            Etapas: 'Etapas',
            Factura: 'Factura',
            CargadaEl: 'cargadael',
            DevueltoEl: 'devueltoel',
            Motivo: 'motivo',
            EstaCargada: 'estacargada'
        }
    };
}
