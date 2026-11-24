// Propiedades de los dto (ltrPropiedades) de entorno, maestros, negocio y callejero. El namespace ltrPropiedades se fusiona con el de literales_Propiedades.ts.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const Entorno = {
        Usuario: {
            id: 'idusuario',
            IdArchivoCertificado: 'idarchivodelcertificado',
            PassworDelCertificado: 'passwordelcertificado',
            DatosCertificado: 'datoscertificado',
            eMail: 'email',
            Activo: 'activo',
            Administrador: 'EsAdministrador'
        },
        EventoDeAgenda: {
            inicio: 'inicio',
            fin: 'fin',
            eventoDeDia: 'eventodedia',
            EsDelSistema: 'esdelsistema',
            Agenda: 'agenda',
            IdAgenda: 'idagenda',
            Correos: 'correos'
        },
        Menu: {
            activo: 'activo',
            iaSeleccionada: 'iaSeleccionada'
        },
        Vista: {
            id: 'idvistamvc',
            Indicadores: {
                SiempreEnConsulta: 'SiempreEnConsulta',
                CapaConVinculados: 'CapaConVinculados',
                TamanoDelVisor: 'tamano-del-visor',
                TamanoDeGraficos: 'tamano-de-graficos',
                IaUsada: 'IA_Usada',
                MostrarVisorAlIniciar: 'MostrarVisorAlIniciar',
                UsaTotalizador: 'UsaTotalizador',
                UsaDirecciones: 'UsaDirecciones',
                PermitirFichas: 'PermitirFichas',
                UsaPrioridades: 'UsaPrioridades',
                ModoDePresentacion: 'ModoDePresentacion',
                // tamaño máximo en bytes de un archivo que se sube o se anexa (IndCrud.ArchivosMaximoTamano)
                ArchivosMaximoTamano: 'ArchivosMaximoTamano'
            }
        },
        seguridad: {
            rol: {
                id: 'idrol',
                idpermiso: 'idpermiso'
            },
            Puesto: {
                id: 'idpuesto',
            },
        },
        MiCorreo: {
            Id: 'id',
            Asunto: 'Asunto',
            Cuerpo: 'Cuerpo',
            CuerpoHtml: 'CuerpoHtml',
            Fecha: 'Fecha',
            IdMensaje: 'IdMensaje',
            Adjuntos: 'Adjuntos',
            ConAdjuntos: 'ConAdjuntos',
            Emisor: 'Emisor',
            Buzon: 'Buzon'
        },
        EnviarElemento: {
            Usuario: 'usuario',
            Asunto: 'Asunto',
            Cuerpo: 'Cuerpo',
            OtorgarGestor: 'OtorgarGestor'
        },
        EnviarCorreoExterno: {
            eMail: 'eMail',
            Para: 'Para',
            IdElemento: 'idElemento'
        },
        EnviarMensajeWeb: {
            Telefono: 'Telefono',
            Para: 'Para',
            Mensaje: 'Mensaje',
            IdElemento: 'idElemento',
            // respuestas de epLeerConfiguracionDeMensajesWeb y epEnviarMensajeWeb
            Canal: 'Canal',
            Modo: 'Modo',
            Url: 'Url',
            ModoCliente: 'CLIENTE',
            CanalWhatsApp: 'WHATSAPP',
            CanalTelegram: 'TELEGRAM',
            CanalSms: 'SMS'
        },
    };

    export const Maestros = {
        unitario: {
            venta: 'venta',
            coste: 'coste',
            idnaturaleza: 'IdNaturaleza',
            idunidad: 'IdUnidad',
            nombre: 'nombre',
            referenciaDeTarifa: 'ReferenciaDeTarifa',
            FiltrosPorClaseDeUnitario: 'FiltrosPorClaseDeUnitario',
            ObtenerTarifaProveedor: 'ObtenerTarifaProveedor'
        },
        Tarifa: {
            IdProveedor: 'IdProveedor'
        },
        Naturaleza: {
            Clase: 'Clase'
        },
        Contabilidad: {
            IvaR: {
                Porcentaje: 'Porcentaje',
                Exento: 'exento',
                DescripcionFiscal: 'DescripcionFiscal'
            },
            IvaS: {
                Porcentaje: 'Porcentaje',
                Exento: 'exento',
            },
            Irpf: {
                Porcentaje: 'Porcentaje'
            },
            Cuenta: {
                Id: 'id'
            },
            CuentaBancaria: {
                Iban: 'Iban',
                Entidad: 'entidad',
                Oficina: 'oficina',
                Dc: 'dcccc',
                Numero: 'numero',
                NumeroIban: 'NumeroIban',
                Alias: 'Alias',
                Banco: 'Banco',
                Activa: 'Activa'
            }
        },
    };

    export const Negocio = {
        idNegocio: 'idnegocio',
        nombre: literal.nombreNegocio,
        propiedad: literal.negocio,
        PlantillaDeExportacion: {
            Plantilla: 'Nombre',
            IdCg: literal.idCg,
            listaDePlantillas: 'listaDePlantillas',
            listaDeCgs: 'listaDeCgs',
            NombreArchivador: 'archivador',
            DescripcionArchivador: 'motivo'
        },
        PlantillaDeCreacion: {
            Plantilla: 'Plantilla',
            IdPlantilla: 'Id',
            Remplazar: 'Remplazar'
        },
        PlantillaDeFiltrado: {
            Plantilla: 'Plantilla',
            IdPlantilla: 'Id',
            Valor: 'valor',
            Vista: 'Vista',
            DisposicionDelEncolumnado: 'disposicion-del-encolumnado',
            OrdenacionDelResultado: 'ordenacion-del-resultado',
            GuardarColumnasDelGrid: 'guardar-columnas-del-grid',
            TamanoDelEncolumnado: 'tamano-del-encolumnado',
            OrdenacionGuardada: atGrid.accion.ordenar
        },
        PlantillaPorTipo: {
            Id: literal.id,
            IdPermiso: 'idpermiso',
            IdAccion: 'idaccion',
            Plantilla: 'plantilla'
        },
        PlantillaDeNegocio: {
            IdPermiso: 'idpermiso',
            IdAccion: 'idaccion',
            Plantilla: 'plantilla'
        },
        Totales: {
            Comun: {
                Procesados: 'Procesados',
            }
        }
    };

    export const Callejero = {
        Direccion: {
            Pais: 'pais',
            IdPais: 'idpais',
            Provincia: 'provincia',
            IdProvincia: 'idprovincia',
            Municipio: 'Municipio',
            IdMunicipio: 'idMunicipio',
            Barrio: 'barrio',
            IdBarrio: 'idbarrio',
            Zona: 'zona',
            IdZona: 'idzona',
            Numero: 'numero',
            Escalera: 'escalera',
            Piso: 'piso',
            Puerta: 'puerta',
            Cp: 'codigopostal',
            IdCp: 'idcp',
            Calle: 'calle',
            TipoDeVia: 'tipoDeVia',
            Resto: 'RestoDeDireccion',
            Url: 'url',
            Calificador: 'calificador'
        },
        Chincheta: {
            Pais: 'pais',
            Provincia: 'provincia',
            Municipio: 'Municipio',
            Numero: 'numero',
            Cp: 'codigopostal',
            Calle: 'calle',
            TipoDeVia: 'tipoDeVia',
            Informacion: 'Informacion',
            hRef: 'href',
            Calificador: 'Calificador'
        },
        Calle: {
            Pais: 'pais',
            Provincia: 'provincia',
            Municipio: 'Municipio',
            IdMunicipio: 'IdMunicipio',
            TipoDeVia: 'tipodevia',
            IdPais: 'idpais',
            IdProvincia: 'idprovincia',
            Zona: 'zona',
            Cp: 'codigopostal',
            Nombre: literal.nombre
        },
        Pais: {
            Iso2: 'ISO2',
            EsUE: 'EsUE'
        }
    };
}
