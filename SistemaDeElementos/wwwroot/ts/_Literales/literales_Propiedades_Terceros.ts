// Propiedades de los dto (ltrPropiedades) de terceros. El namespace ltrPropiedades se fusiona con el de literales_Propiedades.ts.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const Terceros = {
        Cg: {
            idSociedad: 'idsociedad',
            Responsable: 'responsable',
            eMail: 'email'
        },
        Trabajador: {
            eMail: 'email',
            Telefono: 'telefono',
            Cuenta: 'Cuenta',
            AliasDeCuenta: 'alias'
        },
        Persona: {
            CrearInterlocutor: 'CrearInterlocutor',
            CrearProcurador: 'CrearProcurador',
            CrearAbogado: 'CrearAbogado',
            CrearProveedor: 'CrearProveedor',
            CrearCliente: 'CrearCliente',
            CrearTrabajador: 'CrearTrabajador',
            IdCliente: 'idcliente',
            IdInterlocutor: 'idinterlocutor',
            IdProveedor: 'idProveedor',
            IdAbogado: 'idabogado',
            IdProcurador: 'idprocurador',
            Indicadores: {
                TercerosJudiciales: 'TercerosJudiciales',
            }
        },
        Sociedad: {
            Expresion: literal.expresion,
            idAgenda: 'IdAgenda',
            Agenda: 'agenda',
            EsUnaDeMisSociedades: 'EsUnaDeMisSociedades',
            UsaVerifactu: 'UsaVerifactu',
            GenerarPreasientos: ['GenerarEmitidas', 'GenerarRecibidas', 'GenerarCobros', 'GenerarPagos', 'GenerarAlmacen', 'GenerarPedidos', 'GenerarExpedientes'],
            InformacionContable: {
                Contabilidad: 'Contabilidad',
                Digitos: 'Digitos',
                Ncs: 'NCS',
                ContaSol: 'ContaSol',
                DigitosDeNcs: 11,
                DigitosPorDefectoDeContaSol: 8,
                MinimoDeDigitosDeContaSol: 4,
                MaximoDeDigitosDeContaSol: 10,
                RelanzarActivacionDePreasientos: 'RelanzarActivacionDePreasientos'
            },
            VerifactuEnProductivo: 'VerifactuEnProductivo',
            Nombre: literal.nombre,
            NIF: 'Nif',
            sociedad: 'sociedad',
            Telefono: 'telefono',
            eMail: 'eMail',
            CrearInterlocutor: 'CrearInterlocutor',
            CrearProcurador: 'CrearProcurador',
            CrearAbogado: 'CrearAbogado',
            CrearProveedor: 'CrearProveedor',
            CrearCliente: 'CrearCliente',
            responsable: 'responsable',
            IdCliente: 'idcliente',
            IdInterlocutor: 'idinterlocutor',
            IdProveedor: 'idProveedor',
            IdAbogado: 'idabogado',
            IdProcurador: 'idprocurador',
            EsInterlocutor: 'esinterlocutor',
            TipoDeTercero: 'TipoDeTercero',
            CuentaBancaria: {
                Activa: 'activa',
                Alias: 'alias',
                Clase: 'clase',
                Iban: 'Iban',
                Entidad: 'entidad',
                Oficina: 'oficina',
                Dc: 'dcccc',
                Numero: 'numero',
            },
            Tarjeta: {
                Activa: 'activa',
                Alias: 'alias',
                CuentaDeCargo: 'CuentaDeCargo',
                IdCuentaDeCargo: 'IdCuentaDeCargo'
            },
            Indicadores: {
                TercerosJudiciales: 'TercerosJudiciales',
            }
        },
        CertificadoDeUnaSociedad: {
            idSociedad: 'idsociedad',
        },
        Juzgado: {
            clase: 'Clase',
            municipio: 'Municipio',
            calificador: 'Calificador',
            nombre: 'Nombre'
        },
        Cliente: {
            Expresion: literal.expresion,
            Telefono: 'Telefono',
            eMail: 'eMail',
            Id: literal.id,
            IdInterlocutor: 'IdInterlocutor',
            ClaseDeCuenta: 'clase',
            CuentaActiva: 'activa',
            AliasDeCuenta: 'alias',
            Vat: 'VAT',
            EsIntraComunitario: 'EsIntraComunitario',
            TipoDeTercero: 'TipoDeTercero'
        },
        Proveedor: {
            Nif: 'NIF',
            Expresion: 'Expresion',
            Telefono: 'Telefono',
            eMail: 'eMail',
            ClaseDeCuenta: 'clase',
            CuentaActiva: 'activa',
            AliasDeCuenta: 'alias',
            NumeroIban: 'NumeroIban',
            IdUnidad: 'IdUnidad',
            IdNaturaleza: 'IdNaturaleza',
            Concepto: 'Concepto',
            IdIva: 'IdIvaS',
            IdIrpf: 'IdIrpf',
            cgPropuesto: 'cgPropuesto',
            tipoPropuesto: 'tipoFarPropuesto',
            idcgPropuesto: 'idcgPropuesto',
            idtipoPropuesto: 'idtipoFarPropuesto',
            biPropuesto: 'biPropuesto',
            ModoDePago: 'ModoDePago',
            DomiciliadaEn: 'DomiciliadaEn',
            Tarjeta: 'Tarjeta'

        },
        Interlocutor: {
            ClaseDeCuenta: 'clase',
            AliasDeCuenta: 'alias',
            IdSociedad: 'IdSociedad'
        }
    };
}
