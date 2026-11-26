// Propiedades de los dto (ltrPropiedades) de sistema documental, tareas, expedientes y contabilidad. El namespace ltrPropiedades se fusiona con el de literales_Propiedades.ts.
// Depende de literales_Basicos, literales_Enumerados y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

namespace ltrPropiedades {
    export const SisDoc = {
        CopiarArchivo: {
            Enlazar: 'EnlazarArchivos',
            Copiar: 'CopiarArchivos'
        },
        PlantillasDisponibles: {
            Abrir: 'abrir',
            Plantillas: 'plantillas',
            IdPlantilla: 'idplantilla',
            Plantilla: 'plantilla',
            Clase: 'clase'
        },
        ComoArchivar: {
            IdArchivador: 'IdArchivador',
            Archivador: 'Elemento',
            IdCarpeta: 'IdCarpeta',
            CrearArchivador: 'creararchivador',
            CarpetaDestino: 'CarpetaDestino',
            AbrirAlAsociar: 'abriralasociar',
        },
        ComoVincular: {
            IdTarea: 'IdTarea',
            Tarea: 'Tarea',
            CrearTarea: 'creartarea',

            IdRegistroEs: 'IdRegistroEs',
            RegistroEs: 'RegistroEs',
            CrearRegistroEs: 'crearregistroes',

            IdFacturaRec: 'IdFacturaRec',
            FacturaRec: 'FacturaRec',
            CrearFacturaRec: 'crearfacturarec',

            IdExpediente: 'IdExpediente',
            Expediente: 'Expediente',
            CrearExpediente: 'crearexpediente',

            AbrirAlAsociar: 'abriralasociar',
            CarpetaDestino: 'CarpetaDestino',
            ArchivadorDestino: 'ArchivadorDestino',
            TareaDestino: 'TareaDestino',
            IdCarpetaDestino: 'IdCarpetaDeDestino',
            IdArchivadorDestino: 'IdArchivadorDeDestino',
            IdTareaDestino: 'IdTareaDeDestino'
        },
        Archivador: {
            IdArchivador: 'idarchivador',
            propiedad: 'archivador',
            Sincronizar: 'sincronizar',
            ConCarpetas: 'ConCarpetas',
            EsUnCorreo: 'EsUnCorreo',
            Carpetas: 'Carpetas',
            Indicadores: {
                PermiteSincronizar: 'PermiteSincronizar',
                IdTipoArchivadorDeFacturaRec: 'IdTipoArchivadorDeFacturaRec'
            }
        },
        CircuitosDoc: {
            EsLoteContable: 'EsLoteContable',
            Indicadores: {
                IdTipoActividadFormativa: 'IdTipoActividadFormativa',
                TipoActividadFormativa: 'TipoActividadFormativa',
                IdTipoEstimacionDirecta: 'IdTipoEstimacionDirecta',
                TipoEstimacionDirecta: 'TipoEstimacionDirecta',
                IdTipoLoteContable: 'IdTipoLoteContable',
                TipoLoteContable: 'TipoLoteContable',
                IdTipoFichada: 'IdTipoFichada',
                TipoFichada: 'TipoFichada',
            }
        },
        Archivo: {
            idArchivo: literal.idArchivo,
            idOriginal: 'idoriginal',
            EstaBloqueado: 'EstaBloqueado',
            EsDeUnArchivadorVinculado: 'EsDeUnArchivadorVinculado',
            Original: 'original',
            Destino: 'destino',
            ArchivadorDestino: 'ArchivadorDestino',
            IdsDeArchivos: 'IdsDeArchivos',
            IdOrigenDiferente: 'IdOrigenDiferente',
            Auditoria: 'auditoria',
            PadreBloqueado: 'PadreBloqueado',
            CaducaEl: 'CaducaEl'
        },
        ImportarZip: {
            IdArchivador: 'IdArchivador'
        },
        ProcesarFarConIa: {
            IdArchivador: 'IdArchivador',
            CarpetaSeleccionada: "CarpetaSeleccionada"
        },
        Carpeta: {
            Nombre: literal.nombre,
            Archivador: 'archivador'
        },
        Bloqueo: {
            idArchivo: literal.idArchivo,
            Motivo: 'Motivo',
            Auditoria: 'Auditoria'
        }
    };

    export const Contabilidad = {
        CrearLote: {
            Ejercicio: 'ejercicio',
            Descontabilizar: 'descontabilizar'
        },
        CierreContable: {
            IdSociedad: 'IdSociedad',
            IdLote: 'IdLote',
            EsElUltimoCierre: 'EsElUltimoCierre'
        },
        CrearCierre: {
            Sociedad: 'Sociedad',
            UltimoCierre: 'UltimoCierre',
            CerrarHasta: 'CerrarHasta',
            Contabilidad: 'Contabilidad',
            Digitos: 'Digitos',
            SistemaContable: 'SistemaContable',
            FechaUltimoCierre: 'FechaUltimoCierre',
            SinSeleccionar: '-1',
            Ncs: 'NCS',
            ContaSol: 'ContaSol'
        }
    };

    export const Tarea = {
        IdSolicitante: literal.IdSolicitante,
        Solicitante: literal.Solicitante,
        Etapas: 'Etapas',
        UsaPlanificacion: 'UsaPlanificacion',
        Responsable: 'responsable',
        IdExpediente: 'IdExpediente',
        Expediente: 'Expediente',
        IdFacturaEmt: 'IdFacturaEmt',
        Planificacion: {
            PlfDeInicio: 'PlfDeInicio',
            PlfDeFin: 'PlfDeFin',
            Iniciada: 'Iniciada',
            Finalizada: 'Finalizada',
            Duracion: 'Duracion',
            MedidoEn: 'MedidoEn'
        },
        VinculadoA: {
            IdExpediente: 'IdExpediente'
        },
        CuandoRealizar: {
            IdTareaEditada: 'IdTareaEditada',
            IdTareaAnterior: 'IdTareaAnterior',
            IdTareaPosterior: 'IdTareaPosterior',
            ExcluirCuandoRealizar: 'ExcluirCuandoRealizar'
        }
    };

    export const Expediente = {
        usaPpts: 'usappts',
        Gastos: 'gastos',
        TieneImputaciones: 'tieneimputaciones',
        Presupuestado: 'presupuestado',
        TienePresupuestos: 'tienepresupuestos',
        usaTareas: 'usaTareas',
        scDeCompra: 'ScDeCompra',
        scDeVenta: 'ScDeVenta',
        Etapas: 'Etapas',
        ClaseDeExpediente: 'ClaseDeExpediente',
        Indicadores: {
            IdTipoActividad: 'IdTipoActividad',
            TipoActividad: 'TipoActividad'
        }
    };
}
