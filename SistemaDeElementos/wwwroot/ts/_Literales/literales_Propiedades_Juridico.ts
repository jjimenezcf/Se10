// Propiedades de los dto (ltrPropiedades) de jurídico y guarderías. El namespace ltrPropiedades se fusiona con el de literales_Propiedades.ts.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const Guarderias = {
        Infante: {
            IdCurso: 'idcurso',
            IdInfante: 'idInfante'
        },
    };

    export const Juridico = {
        Procedimientos: {
            Indicadores: {
                IdTiposDeProcedimientos: 'IdTiposDeProcedimientos'
            }
        },
        CopiarPlfDeVenta: {
            ClaseDeContrato: 'ClaseDeContrato',
            Planificador: 'Planificador',
            Contrato: 'Contrato',
            Inicio: 'Inicio',
            Hasta: 'Hasta'
        },
        PlanificadorDeVenta: {
            IdContrato: 'idcontrato',
            IdTipoDePlanificacion: 'idtipodeplanificacion',
            TipoDePlanificacion: 'tipodeplanificacion',
            IdTipoDeParte: 'idtipodeparte',
            TipoDeParte: 'tipodeparte',
            IdTipoDeFactura: 'idtipodefactura',
            TipoDeFactura: 'tipodeparte',
            inicio: 'inicio',
            hasta: 'hasta',
            IdSociedadDelCg: 'IdSociedadDelCg',
            CgDeLaPlanificacion: 'CgDeLaPlanificacion',
            lote: 'lote',
            idlote: 'idlote',
            linea: {
                orden: 'orden',
                venta: 'venta',
                cantidad: 'cantidad',
                naturaleza: 'Naturaleza',
                idnaturaleza: 'IdNaturaleza',
                idunidad: 'IdUnidad',
                unidad: 'Unidad',
                unitario: 'unitario',
                selectorDeIvaR: 'IvaRepercutido',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                descuentoPorLinea: 'descuento',
                iva: 'iva',
                tipoDeLinea: 'tipoDeLinea',
                concepto: 'concepto'
            }
        },
        lote: {
            idContrato: 'idcontrato',
            vigenteDesde: 'vigentedesde',
            vigenteHasta: 'vigentehasta',
            unitarios: {
                unitario: 'unitario',
                coste: 'coste',
                venta: 'venta'
            }
        },
        Minuta: {
            creadoEl: 'creadoel',
            orden: 'orden',
            concepto: 'concepto'
        },
        TipoCtr: {
            TipoFacturaEmt: 'tipofacturaemt',
            ClaseDeCtr: 'clasedecontrato'
        },
        Contrato: {
            Id: literal.id,
            Expresion: literal.expresion,
            ClaseDeContrato: 'ClaseDeContrato',
            Etapa: 'Etapa',
            idExpediente: 'idexpediente',
            DatosDeVenta: {
                Cliente: 'Cliente',
                Contacto: 'Contacto',
                Telefono: 'Telefono',
                eMail: 'eMail',
                InicioContrato: 'InicioContrato',
                FinContrato: 'FinContrato'
            },
            DatosDeCompra: {
                Proveedor: 'Proveedor',
                Contacto: 'Contacto',
                Telefono: 'Telefono',
                eMail: 'eMail',
                InicioContrato: 'InicioContrato',
                FinContrato: 'FinContrato'
            },
            MatriculaDeGuarderia: {
                Contacto: 'Contacto',
                Telefono: 'Telefono',
                eMail: 'eMail',
                Infante: 'Infante',
                Curso: 'Curso'
            },
            Saldos: {
                Importe: 'Importe',
                Adendado: 'Adendado',
                Bloqueo: 'bloqueo'
            },
            Prorrogas: {
                ClaseDeProrroga: 'ClaseDeProrroga',
                FechaUltimaProrroga: 'FechaUltimaProrroga',
                Meses: 'Meses'
            },
            AvalSolicitado: {
                ImporteAval: 'ImporteAval'
            },
            Avance: {
                Cobrado: 'Cobrado'
            }
        }
    };
}
