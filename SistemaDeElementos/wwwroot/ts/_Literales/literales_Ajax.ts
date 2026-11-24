// Controladores y endpoints de las peticiones, parámetros de los endpoints y de las urls, claves de estado y urls de los mantenimientos.
// Depende de literales_Basicos, literales_Enumerados, literales_Controladores y literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

const Ajax = {
    Permisos: {
        controlador: 'PermisosDelElemento'
    },
    Mensajes: {
        NoMostrar: 'no_mostrar'
    },
    Entorno: {
        Home: {
            ObtenerDashboard: 'epGraficasDeNegocio',
            LeerDisposicionDashboard: 'epLeerDisposicionDashBoard',
            GrabarDisposicionDashboard: 'epGrabarGraficasDeNegocio',
            ResetearDashboard: 'epResetearDashBoard',
        },
        Ia: {
            controlador: ltrControladores.Comunes.Home,
            PreguntaConConteo: 'epPreguntaParaLaIa',
        },
        MiCorreo: {
            controlador: ltrControladores.Entorno.MiCorreo,
            PeticionDeAcceso: 'epPeticionDeAcceso',
            crudApiKey: 'CrudDeMiCorreoApiKey'
        },
        Agenda: {
            visor: {
                controlador: ltrControladores.Entorno.VisorDeAgenda,
                crud: 'MiCalendario'
            }
        },
        ArbolMenu: {
            controlador: ltrControladores.Entorno.ArbolDeMenu,
            accion: 'epSolicitarMenuHtml',
            Inicializar: 'InicializarEntorno'
        },
        Acceso: {
            controlador: ltrControladores.Entorno.Acceso,
            Logout: 'Logout',
            RegistrarConsultaConGuid: 'epRegistrarConsultaConGuid',
            ValidarConsultaPorGuid: 'epValidarConsultaPorGuid',
            Consultar: 'Consultar'
        }
    },
    SisDoc: {
        CircuitosDoc: {
            controlador: ltrControladores.SisDoc.CircuitosDoc,
            Fichar: 'epFichar'
        }
    },
    Ventas: {
        Facturas: {
            controlador: ltrControladores.Venta.FacturasEmt,
            accion: {
                DescargarDeclaracionResponsable: 'epDescargarDeclaracionResponsable',
                ValidarExisteDeclaracionResponsable: 'epValidarDeclaracionResponsable'
            }
        }
    },
    Modal: {
        controlador: ltrControladores.Comunes.Home,
        accion: 'epSolicitarModalHtml',
        parametro: 'modal'
    },
    Archivos: {
        controlador: ltrControladores.SisDoc.Archivos,
        accion: {
            SubirArchivo: 'epSubirArchivo',
            AnexarArchivo: 'epAnexarArchivo',
            QuitarAnexado: 'epQuitarAnexado',
            FirmarArchivo: 'epFirmarArchivo',
            RegistrarDescargaConGuid: 'epRegistrarDescargaConGuid',
            DescargaConGuid: 'epDescargaConGuid',
            AnularFirma: 'epAnularFirma',
            ConvertirAPdf: 'epConvertirAPdf',
            GuardarTextoEditado: 'epGuardarTextoEditado',
            ComoSeHace: 'epComoSeHace',
            LeerAnexados: 'epLeerAnexados',
            LeerAnexadosDirectos: 'epLeerAnexadosDirectos',
            LeerAnexadosPorGuid: 'epLeerAnexadosPorGuid',
            DescargarZipToHtml: 'epDescargarZipToHtml',
            DescargarDocxToHtml: 'epDescargarDocxToHtml',
            DescargarXlsxToHtml: 'epDescargarXlsxToHtml',
            DescargarRtfToHtml: 'epDescargarRtfToHtml',
            DescargarCsvToHtml: 'epDescargarCsvToHtml',
            DescargarMdToHtml: 'epDescargarMdToHtml',
            DescargarParaCrear: 'epDescargarArchivoParaCrear',
            ProcesarAccion: 'epProcesarAccion',
            DescargarHtmlSanitizado: 'epDescargarHtmlSanitizado',
            Descargar: 'epDescargarArchivo',
            DescargarPorGuid: 'epDescargarArchivoPorGuid',
            DescargarComoHtmlPorGuid: 'epDescargarComoHtmlPorGuid',
            Thumsnail: 'epDescargarThumsnail',
            LeerCertificados: 'epLeerCertificados',
            LeerDatosDeFirma: 'epLeerDatosDeFirma',
            ProcesarArchivos: 'epProcesarArchivos',
            OperacionConArchivos: 'epOperacionConArchivos',
            BloquearArchivo: 'epBloquearArchivo',
            DesbloquearArchivo: 'epDesbloquearArchivo',
            BloquearArchivos: 'epBloquearArchivos',
            DesbloquearArchivos: 'epDesbloquearArchivos',
            GenerarZip: 'epGenerarZip',
            SubirTrozo: 'epSubirTrozo',
            LeerVinculosAl: 'epLeerVinculosAl'
        },
        parametro: {
            idArchivo: 'idArchivo',
            Cantidad: '5'
        },
    },
    EndPoint: {
        ProcesarOpcionMf: 'epProcesarOpcionMf',
        ProcesarPeticion: 'epProcesarPeticion',
        ManualDeLaVista: 'epManualDeLaVista',
        CrearElementoPorPost: 'epCrearElementoPorPost',
        CrearNodo: 'epCrearNodo',
        PersistirNodo: 'epPersistirNodo',
        LeerDatosParaElGrid: 'epLeerDatosPost',
        Totales: 'epTotales',
        LeerDirecciones: 'epLeerDirecciones',
        LeerElemento: 'epLeerElemento',
        LeerElementos: 'epLeerElementos',
        LeerElementosPorGuid: 'epLeerElementosPorGuid',
        LeerLosEventosDel: 'epLeerLosEventosDel',
        LeerLosEventosPorGuid: 'epLeerLosEventosPorGuid',
        LeerPorId: 'epLeerPorId',
        LeerPorIdPorGuid: 'epLeerPorIdPorGuid',
        Bloquear: 'epBloquear',
        DarDeAlta: 'epDarDeAlta',
        DarDeBaja: 'epDarDeBaja',
        ModificarPorId: 'epModificarPorPost',
        BorrarPorId: 'epBorrarPorId',
        BorrarRelacion: 'epBorrarRelacionPorId',
        Leer: 'epLeerParaSelector',
        CargarLista: 'epCargarLista',
        CargaDinamica: 'epCargaDinamica',
        CrearRelaciones: 'epCrearRelaciones',
        Imputar: 'epImputar',
        CrearRelacion: 'epCrearRelacion',
        CrearRelacionPost: 'epCrearRelacionPost',
        CrearVinculo: 'epCrearVinculo',
        CrearDetalle: 'epCrearDetalle',
        Vincular: 'epVincular',
        BorrarVinculo: 'epBorrarVinculo',
        ModificarRelacion: 'epModificarRelacionPorPost',
        LeerDatosParaInicializarVista: 'epLeerDatosParaInicializarVista',
        LeerModoDeAccesoAlElemento: 'epLeerModoDeAccesoAlElemento',
        Exportar: 'epExportar',
        EnviarCorreo: 'epEnviarPorCorreo',
        Transitar: 'epTransitar',
        Imprimir: 'epImprimir',
        LeerJerarquia: 'epLeerJerarquia',
        LeerNodoSeleccionado: 'epLeerNodoSeleccionado',
        SometerTrabajo: 'epSometerTrabajoDeUsuario',
        LeerVinculosCon: 'epLeerVinculosCon',
        LeerVinculosConPorGuid: 'epLeerVinculosConPorGuid',
        LeerVinculosConElNegocio: 'epLeerVinculosConElNegocio',
        PersistirAmpliacion: 'epPersistirAmpliacionPorPost',
        LeerAmpliacionPorIdNegocio: 'epLeerAmpliacionPorIdNegocio',
        LeerAmpliacionPorNegocio: 'epLeerAmpliacionPorNegocio',
        Genericas: {
            LeerTipo: 'epLeerTipo',
            LeerTipoPorGuid: 'epLeerTipoPorGuid',
            ObtenerUrlAlExpediente: 'epObtenerUrlAlExpediente',
            ObtenerUrlAlProveedor: 'epObtenerUrlAlProveedor',
            ObtenerUrlAlCliente: 'epObtenerUrlAlCliente',
        },
        Entorno: {
            Correos: {
                EnviarCorreoExterno: 'epEnviarCorreoExterno'
            },
            MensajesWeb: {
                EnviarMensajeWeb: 'epEnviarMensajeWeb',
                LeerConfiguracionDeMensajesWeb: 'epLeerConfiguracionDeMensajesWeb'
            },
            MiCorreo: {
                Archivar: 'epArchivarCorreo',
                DescargarAdjunto: 'epDescargarAdjunto',
                ImprimirCorreo: 'epImprimirCorreo',
                EliminarCorreo: 'epEliminarCorreo',
                AsociarAlElemento: 'epAsociarAlElemento'
            },
            Agenda: {
                IrAlElementoDelEvento: 'epIrAlElementoDelEvento',
                EnviarEventoIcs: 'epEnviarEventoIcs'
            },
            ArbolMenu: {
                GuardarMenuAccedido: 'epGuardarMenuAccedido',
                GuardarRegistroAccedido: 'epGuardarRegistroAccedido'
            },
            Menu: {
                SeleccionarIa: 'epSeleccionarIa'
            }
        },
        Logistica: {
            LineasDeUnPedido: {
                RecibirLinea: 'epRecibirLinea'
            },
            EntregasDeUnPedido: {
                CancelarEntrega: 'epCancelarEntrega'
            },
            DevolucionesDeUnPedido: {
                DevolverEntrega: 'epDevolverEntrega',
                CancelarDevolucion: 'epCancelarDevolucion',
                ImprimirDevoluciones: 'epImprimirDevoluciones'
            },
            Pedidos: {
                RecepcionarPedido: 'epRecepcionarPedido'
            },
            Regularizaciones: {
                ImportarRecuento: 'epImportarRecuento',
                AplicarRecuento: 'epAplicarRecuento'
            },
            MovimientosDeAlmacen: {
                ObtenerUrlAlDocumento: 'epObtenerUrlAlDocumento',
                ObtenerUrlAlProveedor: 'epObtenerUrlAlProveedorDelMovimiento',
                ObtenerUrlAlPreasiento: 'epObtenerUrlAlPreasiento'
            },
            Inventario: {
                ObtenerUrlAlUnitario: 'epObtenerUrlAlUnitario',
                PrepararRecuento: 'epPrepararRecuento',
                DatosParaVender: 'epDatosParaVender'
            },
            Ventas: {
                CrearVenta: 'epCrearVenta',
                ModificarLineaServida: 'epModificarLineaServida',
                CambiarDatos: 'epCambiarDatos'
            },
            DevolucionesDeVenta: {
                DevolverLineaDeVenta: 'epDevolverLineaDeVenta',
                AceptarDevolucion: 'epAceptarDevolucion',
                CancelarDevolucion: 'epCancelarDevolucion'
            }
        },
        Contabilidad: {
            Preasientos: {
                ObtenerUrlAlOrigen: 'epObtenerUrlAlOrigen',
                ObtenerUrlAlLoteContable: 'epObtenerUrlAlLoteContable',
                CrearLoteContable: 'epCrearLoteContable'
            },
            CierresContables: {
                CrearCierre: 'epCrearCierre',
                EliminarCierre: 'epEliminarCierre'
            }
        },
        Guarderias: {
            Infantes: {
                AsociarCurso: 'epAsociarCurso'
            },
        },
        SisDoc: {
            EstimacionesDirectas: {
                CrearEstimacionDirecta: 'epCrearEstimacionDirecta'
            },
            Archivadores: {
                ImportarZip: 'epImportarZip',
                ProcesarFarConIa: 'epProcesarFarConIa',
                Copiar: 'epCopiarArchivador'
            }
        },
        MaestrosTecnico: {
            Unitario: {
                ImportarCatalogo: 'epImportarCatalogo',
                ImportarTarifa: 'epImportarTarifa'
            }
        },
        Administracion: {
            Tarea: {
                Copiar: 'epCopiarUnaTarea',
                CuandoRealizar: 'epCuandoRealizar',
                EliminarCuandoRealizar: 'epEliminarCuandoRealizar'
            }
        },
        Gasto: {
            RemesasPag: {
                PagarRemesa: 'epPagarRemesa',
                RetrocederRemesa: 'epRetrocederPagoRemesa'
            },
            FacturaRec: {
                ImportarFarXml: 'epImportarFarDesdeXml',
                ImportarPrvXml: 'epImportarPrvDesdeXml',
                CrearFarConIa: 'epCrearFarConIa',
                Generar: 'epGenerarUnaFar',
                Rectificar: 'epRectificarUnaFar',
                Renombrar: 'epRenombarUnaFar',
                CambiarProveedor: 'epCambiarProveedor'
            }
        },
        Venta: {
            Presupuesto: {
                Generar: 'epGenerarUnPpt',
                Asociar: 'epAsociarUnPpt',
                Renombrar: 'epRenombarUnPpt'
            },
            AsignacionPtr: {
                AplicarDatosDeEjecucion: 'epAplicarDatosDeEjecucion'
            },
            FacturaEmt: {
                CopiarFae: 'epCopiarUnaFae',
                CambiarVencimiento: 'epCambiarVencimiento',
                CambiarDatos: 'epCambiarDatos',
                HacerRectificativa: 'epHacerRectificativa',
                FacturarTareas: 'epFacturarTareas',
                IrARectificadaPor: 'epIrARectificadaPor',
                IrARectificoA: 'epIrARectificoA'
            },
            RemesasFae: {
                CargarRemesa: 'epCargarRemesa',
                AnularCargoRemesa: 'epAnularCargoRemesa'
            }
        },
        Contrato: {
            GenerarPlanificador: 'epGenerarPlanificadores',
            PrepararPartesTr: 'epPrepararPartesTr',
            EmitirPrefacturasPorPartesTr: 'epEmitirPrefacturasPorPartesTr',
            EmitirPrefacturasPorContrato: 'epEmitirPrefacturasPorContratos',
        },
        Terceros: {
            Sociedad: {
                AsociarCertificado: 'epAsociarCertificado',
                LeerDatosDeSociedad: 'epLeerDatosDeSociedad'
            },
            InformacionContable: {
                RelanzarActivacionDePreasientos: 'epRelanzarActivacionDePreasientos'
            },
            Interlocutor: {
                LeerCuentaDeIngreso: 'epLeerCuentaDeIngreso'
            },
            CG: {
                LeerDireccion: 'epLeerDireccion'
            },
            Juzgado: {
                ImportarCatalogo: 'epImportarCatalogo'
            },
            Proveedor: {
                ImportarTarifa: 'epImportarTarifa'
            }
        },
        Callejero: {
            Municipio: {
                ImportarCatalogo: 'epImportarCatalogo'
            }
        },
        Negocio: {
            LeerTipos: 'epLeerTipos',
            TiposDeElemento: {
                DescargarPlantilla: 'epDescargarPlantilla',
            },
            PlantillasDeNegocio: {
                DescargarPlantilla: 'epDescargarPlantilla',
                DescargarEtiquetas: 'epDescargarEtiquetas',
            },
            Estados: {
                ObtenerFlujo: 'epObtenerFlujo',
                LeerPosiciones: 'epLeerPosicionesDeEstados',
                GrabarDisposicion: 'epGrabarDisposicionDeEstados',
                LeerEtapas: 'epLeerEtapasDeUnEstado',
                AyudaDelFlujo: 'epAyudaDelFlujo',
                DescargarAyudaDelFlujo: 'epDescargarAyudaDelFlujo',
                ExplicarTransiciones: 'epExplicarTransiciones',
                AyudaDelNegocio: 'epAyudaDelNegocio',
                AyudaDelFlujoHtml: 'epAyudaDelFlujoHtml',
                GrabarEtapas: 'epGrabarEtapasDeUnEstado'
            }
        },
    },
    EpDeAcceso: {
        ReferenciarFoto: 'epReferenciarFoto',
        ValidarAcceso: 'epValidarAcceso',
        Validar2F: 'epValidar2F',
        Reenviar2F: 'epReenviar2F',
        SolicitarNuevaContrasena: 'epSolicitarNuevaContrasena',
        ActualizarContrasena: 'epActualizarContrasena'
    },
    Param: {
        // pregunta a la IA y título de la vista (Archivos/epComoSeHace, también controlador y vista: literal.controlador / literal.vista)
        Pregunta: 'pregunta',
        Titulo: 'titulo',
        elementoJson: 'elementoJson',
        opcionMf: 'opcionMf',
        peticion: 'peticion',
        esContextual: 'esContextual',
        idNegocio: 'idNegocio',
        idVista: 'idVista',
        VistaMvc: 'vistaMvc',
        idVinculado: atControl.idVinculado,
        enumerado: 'enumerado',
        idElemento1: 'idElemento1',
        idElemento2: 'idElemento2',
        VinculadosA: 'vinculadosA',
        ids: 'ids',
        idTipo: 'idTipo',
        idEstado: 'idEstado',
        modo: 'modo',
        descriptor: 'descriptor',
        accion: literal.accion,
        posicion: 'posicion',
        cantidad: 'cantidad',
        obtenerSeguridad: 'obtenerseguridad',
        cargarListaDinamica: 'cargarListaDinamica',
        leerPorIdParaEditar: 'leerPorIdParaEditar',
        BorrarRelacion: 'BorrarRelacion',
        cargarListaDeElementos: 'cargarListaDeElementos',
        creandoEnCrud: 'creandoEnCrud',
        filtro: 'filtro',
        filtroPrecomputado: 'filtroprecomputado',
        guidDeFiltros: 'guiddefiltros',
        filtrarConIa: 'filtrarConIa',
        fraseDeFiltrado: 'fraseDeFiltrado',
        nuevaPregunta: 'nuevaPregunta',
        aplicarJoin: 'AplicarJoin',
        orden: 'orden',
        ColumnasOpcionales: 'ColumnasOpcionales',
        AmpliacionesSolicitadas: 'AmpliacionesSolicitadas',
        DatosPrincipales: 'DatosPrincipales',
        seleccionados: 'seleccionados',
        id: literal.id,
        parametros: 'parametrosJson',
        filtros: 'filtrosJson',
        idsJson: 'idsJson',
        claseElemento: 'claseElemento',
        fichero: 'fichero',
        rutaDestino: 'rutaDestino',
        extensiones: 'extensionesValidas',
        login: 'login',
        password: 'password',
        codigo2F: 'codigo2F',
        esDispositivo2FA: 'esDispositivo2FA',
        nombreDeNegocio: literal.negocio,
        enumNegocio: literal.enumNegocio,
        idElemento: atControl.idElemento,
        idsDeElementos: 'idsDeElementos',
        propiedadId: 'propiedadId',
        datosPeticion: 'datosPeticion',
        json: 'json',
        nodoPadre: 'idPadre',
        operacion: 'operacion',
        ordenarPor: 'ordenarpor',
        nodoSeleccionado: 'nodoSeleccionado',
        SoloEnAlta: 'SoloEnAlta',
        idTransicion: 'idTransicion',
        fitrarPara: 'filtrarpara',
        guid: 'guid',
        idDondeImputar: 'idDondeImputar',
        idPlantilla: 'idPlantilla',
        idAdjunto: 'idAdjunto',
        idParte: 'idParte',
        idMail: 'idMail',
        Exportacion: {
            Sometido: 'sometido',
            Receptores: 'receptores'
        },
        MiCorreo: {
            Buzon: 'buzon',
        },
        parametrosDeCreacion: 'parametrosDeCreacion',
        Bloquear: 'bloquear',
        RowVersion: 'RowVersion'
    },
    Parametros: {
        MensajeSiNoHay: 'MensajeSiNoHay',
        ErrorSiNoHay: 'ErrorSiNoHay',
        ErrorSiHayMasDeUno: 'ErrorSiHayMasDeUno',
        SoloActivos: 'SoloActivos',
        ConsultarConGuid: 'ConsultarConGuid'
    },
    persistencia: {
        modificar: 'Modificar',
        eliminar: 'Eliminar',
    },
    Callejero: {
        Importacion: 'importarCallejero',
        accion: {
            importar: 'epImportarCallejero'
        },
        Parametros: {
            Calificador: 'enumCalificadorDireccion'
        }
    },
    TrabajosSometidos: {
        controlador: 'TrabajosDeUsuario',
        parametro: {
            trabajo: 'trabajo',
            idArchivador: 'idarchivador',
            parametros: 'parametros'
        },
        trabajo: {
            sincronizarArchivador: 'Sincronizar archivador'
        }
    },
    Usuarios: {
        controlador: 'usuarios',
        accion: {
            LeerUsuarioDeConexion: 'epLeerUsuarioDeConexion',
            LeerCertificados: 'epLeerCertificados',
            SubirMiCertificado: 'epSubirMiCertificado',
            CambiarPassword: 'epCambiarPassword',
            CambiarMisDatos: 'epCambiarMisDatos'
        }
    },
    Planificador: {
        idPlanificador: 'idPlanificador'
    },
    // no son de ajax: estado de las respuestas json y eventos del XMLHttpRequest
    Error: 'Error',
    jsonResultError: 1,
    jsonResultOk: 0,
    jsonUndefined: undefined,
    eventoLoad: 'load',
    eventoError: 'error',
    eventoProgreso: 'progress'
};

const ltrUrls = {
    Entorno: {
        Trasiciones: 'Transiciones/CrudDeTransiciones',
        Estados: 'Estados/CrudDeEstados',
        VisorDeAgenda: 'VisorDeAgenda/VisorDeAgenda',
    },
    Terceros: {
        Personas: "Personas/CrudPersonas",
        Sociedades: "Sociedades/CrudSociedades",
        Interlocutor: "Interlocutores/CrudInterlocutores",
        Procurador: "Procuradores/CrudProcuradores",
        Proveedor: "Proveedores/CrudProveedores",
        Cliente: "Clientes/CrudClientes",
        Trabajador: "Trabajadores/CrudTrabajadores",
        Abogado: "Abogados/CrudAbogados",
        CentrosGestores: 'CentrosGestores/CrudCentrosGestores',
    },
    Callejero: {
        Barrios: 'Barrios/CrudBarrios',
        Zonas: 'Zonas/CrudZonas',
        Cps: 'CodigosPostales/CrudCodigosPostales'
    },
    SistemaDocumental: {
        Archivadores: 'Archivadores/CrudArchivadores',
        Carpetas: 'Carpetas/CrudCarpetas',
        LotesContables: 'CircuitosDoc/CrudLotesContables',
    },
    Administracion: {
        Tareas: 'Tareas/CrudTareas',
        RegistroEs: 'RegistrosEs/CrudRegistrosEs',
        Expediente: 'Expedientes/CrudExpedientes',
    },
    Juridico: {
        ContratosDeVenta: `Contratos/CrudContratos?clase=${enumClaseDeContrato.Venta}`,
        ContratosDeCompra: `Contratos/CrudContratos?clase=${enumClaseDeContrato.Compra}`,
    },
    Ventas: {
        Presupuestos: 'Presupuestos/CrudPresupuestos',
        PlfDeVenta: 'PlanificacionesDeVenta/CrudPlanificacionesDeVenta',
        PartesTr: 'PartesTr/CrudPartesDeTrabajo',
        FacturasEmt: 'FacturasEmt/CrudFacturasEmt'
    },
    Gastos: {
        FacturasRec: 'FacturasRec/CrudFacturasRec',
        Pagos: 'Pagos/CrudPagos'
    },
    Logistica: {
        Pedidos: 'Pedidos/CrudPedidos',
        Ventas: 'Ventas/CrudVentas',
        Regularizaciones: 'Regularizaciones/CrudRegularizaciones',
        Inventario: 'Inventario/CrudInventario',
        MovimientosDeAlmacen: 'MovimientosDeAlmacen/CrudMovimientosDeAlmacen'
    }
};

const ltrParametrosNeg = {
    ObtenerCertificado: 'ObtenerCertificado',
    Administracion: {
        Tareas: {
            VincularA: {
                Expediente: 'VincularAlExpediente'
            }
        }
    }
};

const ltrOperacion = {
    nameOf: 'ltrOperacion',
    CargarDatos: 'CargarDatos',
    Exportar: 'Exportar',
    ModificoParaTransitar: 'ModificoParaTransitar',
    ModificoParaImprimir: 'ModificoParaImprimir',
    ModificoParaNavegar: 'ModificoParaNavegar',
    ModificarPorId: 'ModificarPorId',
    AccionTrasGuardar: 'AccionTrasGuardar',
    HayCambios: 'hayCambios'
}

const ltrClaveDeEstado = {
    historial: 'historial',
    restrictoresDeUnPost: 'restrictoresDeUnPost',
    restrictoresUrl: 'restrictores',
    idSeleccionado: 'idSeleccionado',
    paginaDestino: 'pagina-destino',
    paginaActual: 'pagina-actual',
    guid: 'guid',
    paginaOrigen: 'pagina-origen',
    urlActual: 'url-actual',
    //soloMapearEnElFiltro: 'solo-mapear-en-el-filtro',
    filtrosUrl: 'filtros',
    filtrosDeUnPost: 'filtrosDeUnPost',
    paraqueNavegar: atNavegar.paraqueNavegar,
    EditarAlVolver: 'EditarAlVolver',
    ElementosSeleccionados: 'elementos_seleccionados',
    SeguirRetrocediendo: 'SeguirRetrocediendo'
};

const ltrParametrosUrl = {
    origenDePeticion: 'origen',
    id: 'id',
    restrictores: ltrClaveDeEstado.restrictoresUrl,
    filtros: ltrClaveDeEstado.filtrosUrl,
    // si vale '1' y el mantenimiento llega sin elementos, pasa directamente a crear uno (con los restrictores de la url ya aplicados)
    nuevoSiVacio: 'nuevosivacio',
    idSociedad: 'idSociedad',
    idPersona: 'IdPersona',
    idInterlocutor: 'IdInterlocutor',
    idEstado: 'filtroorporiddeestado',
    idAgenda: 'idAgenda',
    fecha: 'fecha',
    guid: 'guid',
    miFiltro: 'MiFiltro',
    SisDoc: {
        IdArchivador: 'idarchivador'
    },
    Juridico: {
        IdPlanificador: 'idplanificador',
        IdContrato: 'idcontrato'
    },
    Administracion: {
        idExpediente: 'idexpediente',
    },
    Venta: {
        IdTarea: 'idtarea',
        IdParteTr: 'idpartetr',
        IdFactura: 'idfacturaemt',
        IdPresupuesto: 'idpresupuesto',
        IdPlfDeVenta: 'idplfdeventa',
        FiltroConOSinContrato: 'filtroporconosincontrato',
        AsociadaAUnContrato: 'asociadaauncontrato',
        IndicePorContrato: literal.uno,
    },
    Gasto: {
        IdFactura: 'idfacturarec'
    }
};
