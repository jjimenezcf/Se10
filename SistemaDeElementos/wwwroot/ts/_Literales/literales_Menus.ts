// Menús, opciones del menú flotante, etiquetas y títulos.
// Depende de literales_Eventos y literales_Propiedades.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

const ltrEtiquetas = {
    Negocio: {
        PlantillaPorTipo: {
            Plantilla: 'Plantilla'
        },
        PlantillaDeNegocio: {
            Plantilla: 'Plantilla'
        }
    },
    Juridico: {
        Contratos: {
            Avance: {
                Pagado: 'Pagado'
            }
        }
    },
    SisDoc: {
        Archivador: {
            GestionarCarpetas: 'Mostrar carpetas',
            CrearCarpetas: 'Crear carpetas'
        }
    },
    Gasto: {
        Pagos: {
            VenceEl: 'Vence el',
            PagarEl: 'Pagar el',
            AbonadoEl: 'Abonado el',
            PagadoEl: 'Pagado el'
        },
        Remesas: {
            PagadoEl: 'Pagado el',
            SeDeberiaHaberPagadoEl: 'Se debería haber pagado el',
            SePagaraEl: '(Pdte.) Se pagará el',
            SeFijaraAlGenerar: '(Pdte.) Se fijará al generar',
            AnuladoSeIbaAPagarEl: 'Anulado, se iba a pagar el'
        }
    },
    Venta: {
        Remesas: {
            CargadaEl: 'Cargada el',
            SeDeberiaHaberCargadoEl: 'Se debería haber cargado el',
            SeCargaraEl: '(Pdte.) Se cargará el'
        },
        Cobros: {
            Cobrar: 'Cobrar'
        },
        Abonos: {
            Abonar: 'Abonar'
        },
        FacturaEmt: {
            Cobrado: 'Cobrado',
            Abonado: 'Abonado',
        }
    },
    Logistica: {
        Pedido: {
            Material: 'Material',
            Unitario: 'Unitario'
        }
    }
}

const ltrTitle = {
    Gasto: {
        FacturaRec: {
            linea: {
                BaseImponible: {
                    SoloIva: "Indica la BI que le afecta al iva a seleccionar",
                    SoloIRPF: "Indica la BI que le afecta al irpf a seleccionar",
                }
            }
        }
    }
}

const ltrMenus = {
    menu: {
        individual: 'menu.individual',
        edicion: 'menu.edicion',
        creacion: 'menu.creacion',
        historial: 'menu.historial',
        contextual: 'menu.contextual',
        relacion: 'menu.de.relaciones',
        filtro: 'menu.de.filtro',
        formulario: 'menu.formulario'
    },
    PanelDeControl: {
        UltimosMenu: 'ultimos-accesos-menu-1'
    },
    Texto: {
        borrarPlantilla: 'Remplazar o borrar plantilla'
    },
    aQuienAfectaLaOpicionDeMenu: 'origen',
    accion: 'accion-menu',
    opcion: 'opcion',
    esContextual: 'escontextual',
    enumOrigen: {
        crud: 'crud',
        edicion: 'edicion',
        creacion: 'creacion',
        formulario: 'formulario'
    },
    eventosDeMf: {
        AbrirEnviarCorreo: 'abrir-correo',
        AbrirTransitar: 'abrir-transitar',
        ModalDeExportar: 'abrir-exportar',
        ModalDeCrearObservacion: 'crear-observacion',
        ModalDeCrearArchivador: 'crear-archivador',
        ModalDeCrearDirecciones: 'crear-direccion',
        alta: ltrEventos.ModalEdicion.DarDeAlta,
        baja: ltrEventos.ModalEdicion.DarDeBaja,
        Comun: {
            EnviarCorreoExterno: 'enviar-correo-externo',
            EnviarMensajeWeb: 'enviar-mensaje-web',
            PermisosDeElemento: 'permiso-de-elemento',
            Imprimir: 'abrir-imprimir',
            GuardarEstadosDeExpansores: 'guardar-estados-de-expansores',
            GuardarDisposicionDeArchivos: 'guardar-disposicion-de-archivos',
            LeerDisposicionDeArchivos: 'leer-disposicion-de-archivos',
            CantidadALeer: atControl.cantidadPorLeer,
            ModoDePresentacion: 'modo-de-presentacion',
            TamanoDelVisor: ltrPropiedades.Entorno.Vista.Indicadores.TamanoDelVisor,
            TamanoDeGraficos: ltrPropiedades.Entorno.Vista.Indicadores.TamanoDeGraficos,
            MostrarVisorAlIniciar: ltrPropiedades.Entorno.Vista.Indicadores.MostrarVisorAlIniciar,
            DisposicionDelEncolumnado: ltrPropiedades.Negocio.PlantillaDeFiltrado.DisposicionDelEncolumnado,
            OrdenacionDelResultado: ltrPropiedades.Negocio.PlantillaDeFiltrado.OrdenacionDelResultado,
            TamanoDelEncolumnado: ltrPropiedades.Negocio.PlantillaDeFiltrado.TamanoDelEncolumnado,
            GuardarColumnasDelGrid: ltrPropiedades.Negocio.PlantillaDeFiltrado.GuardarColumnasDelGrid,
            EliminarColumnasDelGrid: 'eliminar-columnas-del-grid',
            GuardarDatosCreacion: 'guardar-datos-creacion',
            Totalizador_Mostrar: 'mostrar-totales'
        },
        Parametrizacion: {
            Negocio: {
                GuardarPlantillaCreacion: 'guardar-plantillas-creacion',
                EliminarPlantillaCreacion: 'eliminar-plantillas-creacion',
                Comun_GuardarPlantillaFiltrado: 'guardar-plantillas-filtrado',
                Comun_EliminarPlantillaFiltrado: 'eliminar-plantillas-filtrado',
                Comun_BlanquearFiltros: 'blanquear-filtros',
                Comun_OcultarColumnas: 'ocultar-columnas',
                Comun_LeerDatosParaExportacion: 'leer-datos-para-exportacion'
            },
            Estados: {
                Transiciones: 'transiciones',
                Flujo: 'flujo'
            },
        },
        Entorno: {
            Trabajo: {
                ejecutar: 'ejecutar',
                bloquear: 'bloquear',
                desbloquear: 'desbloquear',
                resometer: 'resometer'
            },
            Correo: {
                EnviarCorreo: 'Correo_Enviar'
            },
            Agenda: {
                AbrirAgenda: ltrEventos.Expansores.AbrirAgenda,
                ModalDeCrearEvento: 'crear-evento'
            },
            Usuario: {
                MiCertificado: 'mi-certificado',
                MiCorreo: 'mi-correo'
            },
            MiCorreo: {
                Archivar: 'micorreo_creararchivador',
                Tarea: 'micorreo_creartarea',
                RegistroEs: 'micorreo_crearregistroes',
                FacturaRec: 'micorreo_crearfacturarec',
                Expediente: 'micorreo_crearexpediente',
                ComoArchivar: 'micorreo_comoarchivar',
                ComoVincular: 'micorreo_comovincular'

            }
        },
        Maestros: {
            Callejero: {
                Calle: {
                    ValidarSiExiste: 'ValidarSiExisteCalle',
                }
            },
            Terceros: {
                Interlocutores: 'interlocutores',
                Proveedores: 'ir-a-proveedor',
                Clientes: 'ir-a-cliente',
                Trabajadores: 'ir-a-trabajador',
                Procuradores: 'procuradores',
                Abogados: 'abogados',
                CentrosGestores: 'cgs',
                CuentasBancarias: 'cta-bancarias',
                TarjetasBancarias: 'tar-bancarias',
                Facturador: 'facturador-sociedad',
                Buzones: 'buzones',
                AsociarCertificado: 'asociar-certificado',
                CrearLote: 'lote-terceros',
                ActivarVerifactu: 'activar-verifactu',
                RecomponerBlockChain: 'recomponer-blockchain',
                CatalogosJudiciales: 'catalogos-judiciales'
            },
            Clientes: {
                CentroAdministrativo: 'nuevo-centro-adm',
                PuestoDeTrabajo: 'nuevo-puesto-trabajo',
                NuevoClienteWeb: 'nuevo-cliente-web',
                AsociarClienteWeb: 'vincular-cliente-web'
            }
        },
        SisDoc: {
            Archivadores: {
                IrACarpetas: 'ir-a-carpetas',
                Arc_ImportarZip: 'importar-zip',
                Arc_ProcesarFarConIa: 'procesar-far-con-ia',
                Arc_Descontabilizar: 'descontabilizar-spr',
                Arc_Exportar: 'exportar-archivador',
                Arc_Copiar: 'copiar-archivador'
            }
        },
        Administracion: {
            RegistroEs: {
                CrearTareas: 'crear-tarea',
            },
            Tareas: {
                IrAPartesTr: 'ir-a-partes-trabajo',
                CopiarTarea: 'copiar-tarea',
                CuandoRealizar: 'cuando-realizar',
                EliminarCuandoRealizar: 'eliminar-cuando-realizar',
            },
            Expedientes: {
                VincularRegistroEntrada: 'vincular-re',
                IrATareas: 'ir-a-tareas',
                ImputarFacturas: 'imputar-facturas',
                IrAFacturasRec: 'ir-a-facturas-recibidas',
                VincularFacturaRec: 'vincular-far',
                CrearFacturaRec: 'crear-far',
                AnadirApunte: 'anadir-apunte',
                VincularPresupuesto: 'vincular-ppt',
                CrearValoracion: 'crear-valoracion',
                CrearPresupuesto: 'crear-ppt',
            }
        },
        Contabilidad: {
            Preasientos: {
                CrearLoteContable: 'crear-lote',
                AnularLoteContable: 'anular-lote',
                RegenerarLoteContable: 'regenerar-lote',
                AnularEstimacionDirecta: 'anular-estimacion',
                LoteTerceros: 'lote-terceros',
                ContabilizarPreasiento: 'contabilizar-preasiento',
                RegenerarPreasiento: 'regenerar-preasiento'
            },
            CierresContables: {
                CrearCierre: 'crear-cierre',
                EliminarCierre: 'eliminar-cierre'
            }
        },
        Guarderias: {
            Infante: {
                AsociarCurso: 'asociar-curso'
            }
        },
        Juridico: {
            Contratos: {
                VincularRegistroEntrada: 'vincular-re',
                IrAPlfDeVentas: 'ir-a-planificaciones-venta',
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrAFacturasEmt: 'ir-a-facturas-emitidas',
                IrAFacturasRec: 'ir-a-facturas-recibidas',
                GenerarLosPlanificadores: 'generar-planificadores',
                PrepararPartesDeTrabajo: 'preparar-partes-de-trabajo',
                EmitirPrefacturasPorParteTr: 'emitir-prefacturas-por-partetr',
                EmitirPrefacturasPorContrato: 'emitir-prefacturas-por-contrato',
                ImputarFacturas: 'imputar-facturas',
            },
            Planificador: {
                IrAPlfDeVentas: 'ir-a-planificaciones-venta',
                GenerarPlanificaciones: 'generar-planificaciones-venta'
            },
            Pleitos: {
                VincularRegistroEntrada: 'vincular-re',
            }
        },
        Gasto: {
            FacturasRec: {
                IrAContratos: 'ir-a-contratos',
                IrAExpedientes: 'ir-a-expedientes',
                IrAPagos: 'ir-a-pagos',
                IrAFacturasRec: 'ir-a-facturasRec',
                QuitarContrato: 'quitar-contrato',
                QuitarExpediente: 'quitar-expediente',
                Far_ImportarFarXml: 'importar-efactura',
                Far_ImportarPrvXml: 'importar-proveedor',
                Far_CrearFarConIa: 'crear-far-ia',
                CopiarFar: 'copiar-far',
                RectificarFar: 'rectificar-far',
                Renombrar: 'renombrar-far',
                CambiarProveedor: 'cambiar-proveedor',
                GenerarPreasieto: 'generar-preasiento',
                CancelarPreasientos: 'cancelar-preasientos'
            },
            RemesasPag: {
                Imprimir: 'abrir-imprimir',
                DarPorPagado: 'pagar-remesa',
                RetrocederPago: 'retroceder-pago'
            },
            Pagos: {
                GenerarPreasieto: 'generar-preasiento',
                CancelarPreasientos: 'cancelar-preasientos'
            }

        },
        Logistica: {
            Pedidos: {
                ModificarCabecera: 'modificar-cabecera-pedido',
                GuardarCabecera: 'guardar-cabecera-pedido',
                RecepcionarLinea: 'recepcionar-linea-pedido',
                RecepcionarPedido: 'recepcionar-pedido',
                DevolverEntrega: 'devolver-entrega-pedido',
                CopiarPedido: 'copiar-pedido',
                QuitarContrato: 'quitar-contrato',
                QuitarExpediente: 'quitar-expediente'
            },
            Regularizaciones: {
                ImportarRecuento: 'importar-recuento',
                AplicarRecuento: 'aplicar-recuento'
            },
            Almacenes: {
                IrACompras: 'ir-a-compras-del-almacen',
                IrAVentas: 'ir-a-ventas-del-almacen',
                IrARecuentos: 'ir-a-recuentos-del-almacen',
                IrAInventario: 'ir-a-inventario-del-almacen',
                IrAMovimientos: 'ir-a-movimientos-del-almacen'
            },
            Inventario: {
                PrepararRecuento: 'preparar-recuento',
                VenderMaterial: 'vender-material'
            },
            Ventas: {
                DevolverLinea: 'devolver-linea-venta',
                ModificarLineaServida: 'modificar-linea-servida',
                CambiarDatos: 'cambiar-datos-venta'
            }
        },
        MaestrosTecnico: {
            Unitario: {
                ImportarCatalogo: 'importar-catalogo-unitarios',
                ImportarTarifa: 'importar-tarifa'
            }
        },
        Terceros: {
            Juzgado: {
                ImportarCatalogo: 'importar-catalogo-juzgados'
            },
            Proveedor: {
                ImportarTarifa: 'importar-tarifa-de-proveedor'
            }
        },
        Callejero: {
            Municipio: {
                ImportarCatalogo: 'importar-catalogo-municipios'
            }
        },
        Venta: {
            FacturasEmt: {
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrAVentas: 'ir-a-documentos-de-salida',
                IrAPpts: 'ir-a-ppts',
                IrAContratos: 'ir-a-contratos',
                IrAFacturasEmt: 'ir-a-facturasEmt',
                ImprimirPrefacturas: 'abrir-imprimir',
                CambiarVencimiento: 'cambiar-vencimiento',
                CopiarFae: 'copiar-fae',
                CopiarLa: 'copiar-la',
                SincronizarConLaAeat: 'sicronizar-con-la-aeat',
                FacturarTareas: 'facturar-tareas',
                Rectificativa: 'rectificativa',
                GenerarPreasieto: 'generar-preasiento',
                GenerarUbl: 'generar-ubl',
                CambiarDatos: 'cambiar-datos-fae',
                AnadirCobro: 'anadir-cobro-mf'
            },
            RemesasFae: {
                Imprimir: 'abrir-imprimir',
                Cargar: 'cargar-remesa',
                AnularCargo: 'anular-cargo-remesa'
            },
            Partes: {
                IrAPlanificaciones: 'ir-a-planificaciones-venta',
                IrAPpts: 'ir-a-ppts',
                IrAContratos: 'ir-a-contratos',
                IrAFacturasEmt: 'ir-a-facturas-emitidas',
                DarPorRealizadasSegunPlan: 'dar-por-realizada-plan',
                DarPorRealizadasHoy: 'dar-por-realizada-hoy',
                SolicitarFechaDeEjecucion: "solicitar-fechas-ejecucion"
            },
            Presupuestos: {
                AsociarUnExpediente: 'vincular-expediente',
                Renombrar: 'renombrar-ppt',
                CopiarPpt: 'copiar-ppt',
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrATareas: 'ir-a-tareas',
                IrAFacturasEmt: 'ir-a-facturas-emitidas',
                IrAContratos: 'ir-a-contratos'
            },
            Planificaciones: {
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrAFacturasEmt: 'ir-a-facturas-emitidas'
            }
        },
    },
    IdsDeOpcionesMf: {
        idAlta: 'menu.edicion.dar-de-alta',
        idBaja: 'menu.edicion.dar-de-baja',
        idObservacion: 'menu.edicion.observaciones',
        idImprimir: 'menu.edicion.imprimir',
        idPermisos: 'menu.edicion.permisos',
        idArchivador: 'menu.edicion.archivadores',
        idTransitar: 'menu.edicion.transitar',
    },
    OpcionesMf: {
        enviarMail: 'abrir-correo'
    },
    Formulario: {
        alta: 'menu.opcion.alta',
        baja: 'menu.opcion.baja'
    },
    BarraDeMenu: {
        Nuevo: 'Nuevo',
        Editar: 'Editar',
        Consultar: 'Consultar',
        Borrar: 'Borrar',
        Eliminar: 'Eliminar',
        Copiar: 'Nuevo',
        Reactivar: 'Reactivar',
        Crear: 'Crear',
        Ventas: {
            RemesasFae: {
                Facturas: 'Facturas'
            },
            FacturasDeUnaRemesa: {
                IncluirFacturas: 'Añadir'
            }
        },
        Gastos: {
            RemesasPag: {
                Pagos: 'Pagos'
            },
            PagosDeUnaRemesa: {
                IncluirPagos: 'Añadir'
            }
        },
        Guarderia: {
            InfantesDeUnCurso: {
                IncluirInfantes: 'Añadir'
            }
        },
        Terceros: {
            Interlocutor: {
                CrearSociedad: 'cre-soc',
                CrearPersona: 'cre-per',
                CrearContacto: 'cre-ctc'
            },
            Cliente: {
                CrearSociedad: 'cre-soc',
                CrearPersona: 'cre-per'
            }
        }
    },
    MenuDeFiltrado: {
        NumeroDeOpciones: '3'
    }
};
