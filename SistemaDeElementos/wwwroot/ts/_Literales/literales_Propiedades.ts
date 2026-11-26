// Propiedades de los dto (ltrPropiedades): las comunes; las de cada área están en literales_Propiedades_<Área>.ts (el namespace se
// fusiona). También valores de dominio y datos propuestos.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const enumTipoPermiso = 'enumtipopermiso';
    export const baja = 'baja';
    export const DelSistema = 'DelSistema';
    export const NombreModificable = 'NombreModificable';
    export const Informacion = 'Informacion';

    export const Dialogos = {
        Exportacion: {
            LasMostaradas: 'las-mostradas'
        }
    };

    export const Ia = {
        IdArchivo: "idArchivo",
        Nombre: 'nombre',
        Json: {
            Proveedor: 'proveedor',
            Nif: 'Nif',
            eMail: 'eMail',
            Numero: 'NumeroFactura',
            Concepto: 'Concepto',
            Telefono: 'Telefono',
            CodigoPostal: 'CodigoPostal',
            Pais: 'Pais',
            Provincia: 'Provincia',
            Municipio: 'Municipio',
            TipodeVia: 'TipodeVia',
            Calle: 'Calle',
            NumeroPolicia: 'NumeroPolicia',
            RestoDireccion: 'RestoDireccion',
            FormaDePago: 'FormaDePago',
            ClaseDePago: 'ClaseDePago',
            NumeroIban: 'CuentaBancaria',
            FacturadaEl: 'fecha',
            VenceEl: 'FechaVencimiento',
            Total: 'total',
            Bi: 'bi',
            Iva: 'totalIva',
            Irpf: 'totalIrpf'
        }
    };

    export const Sometidos = {
        Correo: {
            Asunto: 'asunto',
            CuerpoMensaje: 'cuerpoMensaje'
        },
        TrabajoDeUsuario: {
            estado: 'estado',
            idsometedor: 'idsometedor'
        }
    };

    export const Selector = {
        Elemento: 'Elemento',
        ReferenciadaComo: 'ReferenciadaComo',
    };

    export const Elemento = {
        Id: literal.id,
        Expresion: literal.expresion,
        RowVersion: 'RowVersion',
        Elemento: 'elemento',
        EstaCancelada: 'estacancelada',
        EstaTerminada: 'estaterminada',
        IdElemento: atControl.idElemento,
        ModoDeAcceso: literal.ModoDeAcceso,
        EsInterventor: 'esinterventor',
        IdArchivo: literal.idArchivo,
        Accion: literal.accion,
        IdCreador: 'idCreador',
        Textos: 'textos',
        FitrarPorBaja: 'filtrarporbaja',
        Baja: 'baja',
        Bloqueado: 'bloqueado',
        Bloqueador: 'bloqueador',
        DarDeAlta: 'Dar de alta',
        DarDeBaja: 'Dar de baja',
        Nombre: literal.nombre,
        ClaseDeElemento: 'ClaseDeElemento',
        IdClaseDeElemento: 'IdClaseDeElemento',
        Referencia: 'referencia',
        Descripcion: 'descripcion',
        ConCg: {
            IdCg: literal.idCg,
            Cg: literal.Cg,
            IdSociedadDelCg: 'idsociedaddelcg',
            IdSociedad: 'idsociedad',
        },
        ConLineas: {
            Anotacion: 'anotacion'
        },
        ConTipo: {
            IdTipo: literal.IdTipo,
            Tipo: literal.Tipo,
        },
        ConPrioridad: {
            Prioridad: 'Prioridad',
        },
        DeProceso: {
            IdEstado: 'idestado',
            Estado: 'estado',
            IdEstadoAnterior: 'idestadoanterior',
            EstadoAnterior: 'estadoanterior',
            IdTransicionAplicable: 'idtransicionaplicable',
            TransicionAplicable: 'transicionaplicable',
            TransicionesDisponibles: 'TransicionesDisponibles',
            MostrarModalParaAvanzar: 'MostrarModalParaAvanzar',
            IdTransicionParaDevolver: 'idtransicionparadevolver',
            TransicionParaDevolver: 'transicionparadevolver',
            MostrarModalParaDevolver: 'MostrarModalParaDevolver',
            OrdenEstado: 'ordenestado',
            Expresion: literal.expresion,
        },
        Historial: {
            Clase: 'clase',
            Suceso: 'suceso',
            Detalle: 'detalle',
            AccionJs: 'accionjs',
            IdRegistro: 'idregistro',
            IdElemento: 'idelemento',
            Negocio: literal.negocio
        },
        Detalle: {
            Titulo: 'titulo'
        }
    };

    export const TipoDeElemento = {
        IdPermisoDeAdministrador: 'IdPermisoDeAdministrador',
        IdPermisoDeGestor: 'IdPermisoDeGestor',
        IdPermisoDeConsultor: 'IdPermisoDeConsultor',
        IdPermisoDeInterventor: 'IdPermisoDeInterventor',
        Mascara: 'mascara',
        Marcador: 'Marcador',
        ModoDeAcceso: literal.ModoDeAcceso,
        HayClases: 'HayClases',
        Activo: 'Activo',
        EditarTrasCrear: 'EditarTrasCrear',
    };

    export const Transiciones = {
        DelSistema: 'delsistema',
        Origen: 'origen',
        Asunto: 'asunto',
        Detalle: 'detalleAsunto',
        Usuarios: 'Usuarios',
        Archivo: 'IdArchivo',

    };

    export const Certificado = {
        propiedad: 'certificado',
    };
}

const ltrDatosPropuestos = {
    CGPropuesto: 'CGPropuesto',
    TipoPropuesto: 'TipoPropuesto',
    Nombre: literal.nombre,
    Descripcion: ltrPropiedades.Elemento.Descripcion,
    Otros: 'Otros',
    Plantillas: 'Plantillas'
}

const ltrValores = {
    Crud: {
        Edicion: {
            diasDeConsulta: 'dias-para-consultar-por-gid'
        }
    },
    Elemento: {
        Historial: {
            MostrarObservacion: 'MostrarObservacion',
            MostrarEvento: 'MostrarEvento',
            MostrarCorreo: 'MostrarCorreo',
            AbrirArchivador: 'AbrirArchivador',
            AbrirTarea: 'AbrirTarea',
            AbrirPpt: 'AbrirPpt',
            Descargar: 'Descargar'
        },
    },

    Maestros: {
        Callejero: {
            Paises:
            {
                Espana: 'España'
            }
        },
        Unitario: {
            Clase: {
                Material: 'Material'
            }
        }
    },
    Logistica: {
        Regularizacion: {
            Clase: {
                Inicial: 'Inicial',
                Recuento: 'Recuento',
                AjusteDePrecio: 'AjusteDePrecio'
            }
        }
    },
    Terceros: {
        Cliente: {
            ClaseDeCuenta: {
                Pago: 'Pago'
            }
        },
        Proveedor: {
            ClaseDeCuenta: {
                Ingreso: 'Ingreso'
            }
        },
        Interlocutor: {
            ClaseDeCuenta: {
                Ingreso: 'Ingreso'
            }
        },
        Trabajador: {
            ClaseDeCuenta: {
                Ingreso: 'Ingreso'
            }
        },
        Sociedad: {
            CuentaBancaria: {
                Clase: {
                    Ambas: 'Ambas'
                }
            }
        }
    },
    Venta: {
        FacturasEmt: {
            Cobro: {
                Clase: {
                    Contado: 'Contado',
                    Transferencia: 'Transferencia',
                    CartaDePago: 'CartaDePago',
                    Remesa: 'Remesa',
                }
            },
            Abono: {
                Clase: {
                    Contado: 'Contado',
                    Transferencia: 'Transferencia',
                    Remesa: 'Remesa',
                }
            },
            Rectificativas: { Clase: { Total: 'OR', Complementaria: 'OC' } }
        }
    },
    Gasto: {
        Pago: {
            Clase: {
                Contado: 'Contado',
                Transferencia: 'Transferencia',
                Remesa: 'Remesa',
            },
            ModoDePago: {
                Contado: enumModoDePagoContado.Contado,
                Tarjeta: enumModoDePagoContado.Tarjeta,
                Domiciliacion: enumModoDePagoContado.Domiciliacion
            }
        },
        Factura: {
            ComoTratarLaFechaDeRecepcion: {
                MismaFecha: 'mismafecha',
                FechaDeHoy: 'fechadehoy',
                FechaDeHoySi15: 'fechadehoy15',
                FechaDeHoySi30: 'fechadehoy30'
            }
        }
    }
};

const ltrNegocioSe = {
    Nombre: {
        Sociedades: 'Sociedades',
        Carpetas: 'Carpetas',
        Archivadores: 'Archivadores',
        Juridico: {
            Contrato: 'Contratos'
        },
        Venta: {
            Presupuesto: 'Presupuestos'
        }
    },
    Enumerado: {
        NoDefinido: enumNegocio.No_Definido,
        Juridico: {
            Contrato: enumNegocio.Contrato
        }
    }
};
