// Clases css.
// Depende de literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

const ltrCss = {
    Mensajes: {
        Informativo: 'mensaje-informativo',
        Error: 'mensaje-error'
    },
    EnviarPorCorreo: {
        Boton: 'boton-enviar-por-correo',
        InputConBoton: 'con-boton-de-correo',
        ListaDeArchivos: 'archivos-para-enviar',
        TituloDeLaLista: 'titulo-archivos-para-enviar'
    },
    // Div con los enlaces de lo que se adjunta en la modal de enviar elemento
    EnviarElemento: {
        Adjuntos: 'adjuntos',
        IdDeLaTabla: 'table-enviarelementodto-nuevo'
    },
    // Botón (con el sobre de EnviarPorCorreo) junto a una lista de usuarios, delante de su botón de navegación
    EnviarCorreoInterno: {
        Boton: 'boton-enviar-correo-interno',
        InputConBoton: 'con-boton-de-correo-interno'
    },
    // Botón junto al teléfono: lleva la clase común y la del canal (el icono lo pone la clase del canal, ver CrudEdicion.css)
    EnviarPorMensajeWeb: {
        Boton: 'boton-enviar-mensaje-web',
        InputConBoton: 'con-boton-de-mensaje-web',
        BotonDelCanal: {
            WHATSAPP: 'boton-enviar-por-whatsapp',
            TELEGRAM: 'boton-enviar-por-telegram',
            SMS: 'boton-enviar-por-sms'
        }
    },
    crud: {
        cuerpo: 'cuerpo',
        cabecera: 'cuerpo-cabecera',
        // cabecera de las páginas de formulario (DescriptorDeFormulario: árboles de centros gestores, carpetas, menús)
        cabeceraFormulario: 'cuerpo-cabecera-formulario',
        datos: 'cuerpo-datos',
        filtro: {
            expansor:'div-mnt-filtro-expansor',
            oculto: 'filtro-oculto'
        },
        grid: {
            filtro: 'cuerpo-datos-filtro',
            grid: 'cuerpo-datos-grid',
            tabla: 'cuerpo-datos-tabla',
            TablaConGraficos: 'contenedor-tabla-con-graficos',
            Splitter: 'splitter-tabla',
            ContenedorTabla: 'div-grid-tabla',
            Graficos: 'div-graficos',
            GraficoDeTotales: 'div-graficos-totales',
            AlternarVistaFichas: 'alternar-vista-fichas',
            AlternarVistaFichasPulsada: 'alternar-vista-fichas-pulsada',
            VistaFichas: 'div-vista-fichas',
            ColumnaFichas: 'columna-de-fichas',
            CabeceraColumnaFichas: 'cabecera-columna-fichas',
            TarjetaFicha: 'tarjeta-ficha',
            TarjetaFichaSeleccionada: 'ficha-seleccionada',
            AccionesFicha: 'acciones-ficha',
            AccionesFichaGrupo: 'acciones-ficha-grupo',
            BotonObservacionFicha: 'boton-observacion-ficha',
            ScrollHorizontalEnGraficos: 'graficos-con-scroll-horizontal',
            divDeUnaColumna: 'div-de-columna'
        },
        menuDeDetalle: 'menu-de-detalle',
        mostrarDetalle: 'menu-de-detalle-oculto',
        ocultarDetalle: 'menu-de-detalle-visible',
        mostrarTotales: 'menu-de-totales-visible',
        mostrarArchivos: 'menu-de-archivos-visible',
        pie: 'cuerpo-pie',
        contenedorEdicionCreacion: 'contenedor-edicion-cabecera',
        creacion: 'cuerpo-creacion',
        edicion: 'cuerpo-edicion',
        edicionSoloConsulta: 'cuerpo-edicion-solo-consulta',
        PanelHistorial: {
            historial: 'cuerpo-historial',
            ContenedorCabecera: 'contenedor-historial-cabecera',
            ContenedorCuerpo: 'contenedor-historial-cuerpo',
            ContenedorPie: 'contenedor-historial-pie',
            Color: 'color'
        },
        editarTrasCrear: 'editar-tras-crear',
        datosPrincipales: 'datos-principales-dto',
        padreContenedorDatosPrincipales: 'padre-contenedor-datos-principales',
        contenedorDatosPrincipales: 'contenedor-datos-principales',
        editando: 'editando',
        mostrandoHistorial: 'mostrando-historial',
        creando: 'creando',
        panelCreacion: {
            VisorOculto: 'visor-oculto',
            ContenedorDeDatosMasVisor: 'contenedor-creacion-cuerpo-datos-visor',
            ContenedorVisor: 'contenedor-edicion-editor-visor',
            ContenedorDeDatosDto: 'contenedor-edicion-editor-datos',
            VisorDeCreacion: 'visor-de-edicion',
            ImagenIa: 'img-ia',
            ImagenOcr: 'img-ocr',
            ImagenArchivo: 'img-archivo',
            CuerpoCreacionConMapa: 'cuerpo-creacion-con-mapa',
            CreacionConMapa: 'creacion-con-mapa',
            DtoConMapa: 'dto-con-mapa',
            AsistenteDelMapa: 'asistente-mapa',
            barraBusquedaMapa: 'barra-busqueda-mapa',
            divMapa: 'div-mapa-creacion',
            SplitterMapa: 'splitter-mapa'
        },
        panelDeEdicion: {
            ContenedorDeAcciones: 'acciones-del-panel-edicion',
            VisorOculto: 'visor-oculto',
            VisorDeEdicion: 'visor-de-edicion',
            ContenedorVisor: 'contenedor-edicion-editor-visor',
            ContenedorDelVisorDeArchivoConHistorial: 'contenedor-del-visor-de-archivo-con-historial',
            ContenedorHistorial: 'contenedor-edicion-editor-historial',
            VisorDeHistorial: 'visor-edicion-historial',
            MenuDeHistorial: 'menu-edicion-historial',
            ContenedorCabecera: 'contenedor-edicion-cabecera',
            ContenedorDeDatosDto: 'contenedor-edicion-editor-datos',
            ContenedorDeDatosMasVisor: 'contenedor-edicion-editor',
            AsistenteMapaEnEdicion: 'asistente-mapa-edicion',
            VisorDeNombreAnexados: 'visor-nombre-archivo',
            NavegadorDeEdicionCuerpoVisor: 'navegador-edicion-editor-visor',
            FiltroSelectorDeArchivos: 'filtro-selector-archivos',
            ProcesarConIa: 'procesar-con-ia',
            PasarOcr: 'pasar-ocr',
            ConvertirAPdf: 'convertir-a-pdf',
            EditarTexto: 'editar-texto',
            Acciones: {
                plegar: 'img-detalle-dto-plegar',
                Desplegar: 'img-detalle-dto-desplegar',
                Visor: 'img-detalle-dto-visor',
                Devolver: 'img-detalle-dto-retroceder',
                Avanzar: 'img-detalle-dto-avanzar',
                Observacion: 'img-detalle-dto-observacion',
                MostrarVisor: 'mostrar-visor',
                OcultarVisor: 'ocultar-visor',
                SinVisor: 'sin-visor',
                // con el historial abierto el icono del visor muestra el del historial y no responde
                HistorialMostrado: 'historial-mostrado',
                // el botón Historial del menú de edición se ve hundido mientras el historial está abierto
                HistorialPulsado: 'historial-pulsado',
                SelectorBotonHistorial: '.botones-de-menu input[type="button"][value="Historial"]'
            },
            OcultarBoton: 'ocultar-boton',
            ConsultaConGuid: 'consulta-con-guid',
            ojoTachado: 'tachado',
            iconoConsultaConGuid: 'icono-consultar-con-guid'
        },
        contenedorTitulo: 'contenedor-titulo',
        tabla: 'div-tabla',
        thead: 'div-thead',
        tbody: 'div-tbody',
        columna: 'div-th',
        encolumnado: 'div-tr',
        fila: 'div-tr',
        celda: 'div-td',
        tituloColumna: 'div-de-columna',
        tituloResaltado: 'div-de-columna-resaltado',
        arrastrando: 'arrastrando',
        modal: {
            dinamica: 'modal-mostrar-propiedad',
            boton: 'btn-modal',
            pie: 'grafica-conteo-botones',
            cuerpo: 'modal-body',
            ContenedorDeHistorialIa: 'contenedor-historial-ia',
            LineaDelhistorialIa: 'item-historial-ia',
            conversandoIa: 'modal-ia-conversando',
            // modal para editar el texto mostrado en el visor (ApiDeArchivos.AbrirModalDeEditarTexto); el resto de sus clases son las comunes de ltrCss.flujo
            editarTexto: 'modal-editar-texto'
        }
    },
    formulario: {
        selectorDeArchivo: 'formulario-selector-archivo',
        dto: 'formulario-contenedor-edicion-dto',
        archivoSobreLi: 'archivo-sobre-li'
    },
    ia: {
        mapeado: 'ia-mapeado',
        Seleccionada: 'ia-seleccionada'
    },
    PanelDeControl: {
        Menu: {
            Oculto: 'menu-pnlctr-oculto',
        },
        DashBoard: {
            // contenedor principal del panel (grid de tarjetas)
            PanelGraficas: 'panel-graficas',
            // tarjeta individual de cada negocio
            GraficaContenedor: 'grafica-contenedor',
            // tarjeta con ancho completo (única visible)
            GraficaAnchoCompleto: 'grafica-ancho-completo',
            // título de la tarjeta
            GraficaTitulo: 'grafica-titulo',
            // botón crear (+) junto al título
            GraficaCrear: 'grafica-crear',
            // contenedor de la visualización activa
            GraficaVistaContenedor: 'grafica-vista-contenedor',
            // botón cerrar (✕)
            GraficaCerrar: 'grafica-cerrar',
            // botón cambio de modo (tarta / barras / matriz)
            GraficaModo: 'grafica-modo',
            // botón de ayuda (?) junto a la ✕: abre la documentación del proceso del negocio
            GraficaAyuda: 'grafica-ayuda',
            // botones de movimiento (flechas)
            GraficaPreguntar: 'grafica-preguntar',
            GraficaMover: 'grafica-mover',
            GraficaMoverDer: 'grafica-mover-der',
            GraficaMoverIzq: 'grafica-mover-izq',
            // ── modo matriz ──
            GraficaMatrizContenedor: 'grafica-matriz-contenedor',
            GraficaMatriz: 'grafica-matriz',
            CeldaTipo: 'celda-tipo',
            CeldaValor: 'celda-valor',
            // ── modo tartas ──
            GraficaTartasContenedor: 'grafica-tartas-contenedor',
            GraficaTartaItem: 'grafica-tarta-item',
            GraficaTartaTitulo: 'grafica-tarta-titulo',
            GraficaTartaSvg: 'grafica-tarta-svg',
            GraficaLeyenda: 'grafica-leyenda',
            GraficaLeyendaItem: 'grafica-leyenda-item',
            GraficaLeyendaColor: 'grafica-leyenda-color',
            // ── modo barras ──
            GraficaBarrasContenedor: 'grafica-barras-contenedor',
            GraficaBarraItem: 'grafica-barra-item',
            GraficaBarraSvg: 'grafica-barra-svg',
        },
        // diálogo de preguntar a la IA (robot de las tarjetas y "¿Cómo se hace...?" del manual de una vista), PanelDeControl.css
        Conteo: {
            Overlay: 'grafica-conteo-overlay',
            Modal: 'grafica-conteo-modal',
            Cabecera: 'grafica-conteo-cabecera',
            Titulo: 'grafica-conteo-titulo',
            Consultar: 'grafica-conteo-consultar',
            Pregunta: 'grafica-conteo-pregunta',
            RespuestaCabecera: 'grafica-conteo-respuesta-cabecera',
            Respuesta: 'grafica-conteo-respuesta',
            // la respuesta de "¿Cómo se hace...?" es texto (no un json de filtros): ajusta líneas con la fuente normal
            RespuestaDeTexto: 'grafica-conteo-respuesta-texto',
            Botones: 'grafica-conteo-botones',
            MismaConversacion: 'grafica-conteo-misma-conv',
            Segundero: 'grafica-conteo-segundero',
            // el diálogo se abre encima de otra modal (la del manual, z-index 9999)
            SobreModal: 'grafica-conteo-sobre-modal'
        },
    },
    crtlValido: 'propiedad-valida',
    crtlNoValido: 'propiedad-no-valida',
    filaNoVisible: 'fila-no-visible',
    contenedorDeUnArchivo: 'contenedor-de-un-archivo',
    celdaNoVisible: 'celda-no-visible',
    divVisible: 'div-visible',
    divNoVisible: 'div-no-visible',
    tdPropiedad: atNombre.tdPropiedad,
    contenedorDeEtiqueta: 'contenedor-etiqueta',
    ordenAscendente: 'ordenada-ascendente',
    ordenDescendente: 'ordenada-descendente',
    sinOrden: 'ordenada-sin-orden',
    barraVerde: 'barra-verde',
    barraRoja: 'barra-roja',
    barraAzul: 'barra-azul',
    contenedorModal: 'contenedor-modal',
    contenidoModal: 'contenido-modal',
    contenidoModalAmpliado: 'contenido-modal-ampliado',
    soloLectura: 'solo-lectura',
    columnaOculta: 'columna-oculta',
    tablaEnVertical: 'tabla-en-vertical',
    columnaAccion: atGridDeDetalle.EsAccion,
    columnaCabecera: 'columna-cabecera',
    filaDelGrid: 'cuerpo-datos-tbody-tr',
    filaSeleccionada: 'fila-seleccionada',
    filaCancelada: 'fila-cancelada',
    filaTerminada: 'fila-terminada',
    filaEstado: 'fila-estado',
    textoFilaEstado: 'texto-fila-estado',
    cuerpoDeLaTabla: 'cuerpo-datos-tbody',
    cuerpoDeLaTablaGrid: 'cuerpo-datos-grid-tbody',
    sinCapaDeBloqueo: 'sin-capa-de-bloqueo',
    conCapaDeBloqueo: 'con-capa-de-bloqueo',
    nodoDeJerarquia: 'nodo-de-jerarquia',
    nodoDeJerarquiaDeBaja: 'nodo-de-jerarquia-de-baja',
    cambioDeColorElBoton: 'cambio-de-color',
    nodoSeleccionado: 'nodo-seleccionado',
    contenedorDeArchivos: 'contenedor-de-archivos',
    archivosSeleccionado: 'archivos-seleccionados',
    infoArchivo: 'formulario-visor-datos-archivo',
    contenedorInfoArchivo: 'contenedor-visor-info-archivo',
    contenedorDeOpcion: 'contenedor-de-opcion',
    contenedorVisor: 'contenedor-de-visor',
    contenedorVisorOculto: 'contenedor-de-visor-oculto',
    contenedorVisorRef: 'contenedor-de-visor-ref',
    contenedorVisorImg: 'contenedor-de-visor-img',
    imagen100_100: 'imagen100_100',
    refJs: 'refJs',
    archivoFirmado: 'archivo-firmado',
    borrarAnexado: 'borrar-anexado',
    firmarArchivo: 'firmar-archivo',
    bloquearArchivo: 'bloquear-archivo',
    desbloquearArchivo: 'desbloquear-archivo',
    descargarAnexado: 'descargar-anexado',
    eliminarSeleccionado: 'eliminar-seleccionado',
    contenedorEdicionCuerpo: 'contenedor-edicion-cuerpo',
    contenedorConMasDeUnControl: 'div-con-mas-propiedades',
    mostrarSoloElPrimerControl: 'mostrar-solo-primer-control',
    controlOculto: 'control-oculto',
    SemaforoEnGrid: 'semaforo-grid',
    Modal: {
        Boton: 'boton-modal',
        BotonPorDefecto: 'boton-por-defecto',
        ContenidoCuerpo: 'contenido-cuerpo',
        ContenedorEdicionCuerpo: 'contenedor-edicion-cuerpo',
        BotonPrincipal: 'boton-modal btn btn-primary boton-por-defecto',
        BotonSecundario: 'boton-modal btn btn-secondary',
        // aparición suave de una modal dinámica (se incluye tras añadirla al body y se excluye antes de quitarla)
        FadeIn: 'fade-in',
        // robot verde de la IA en la cabecera de la modal del manual de una vista (CrudModal.css), a la izquierda de la ×
        BotonIa: 'boton-ia-de-modal',
        MostrarDireccion: {
            Css: 'modal-mostrar-direccion',
            Dialogo: 'modal-mostrar-direccion-dialogo',
            Mapa: 'modal-mostrar-direccion-mapa',
            Menu: 'modal-mostrar-direccion-menu',
            FiltroGrupo: 'modal-direccion-filtro-grupo',
            FiltroDesplegable: 'modal-direccion-filtro-desplegable',
            FiltroOpciones: 'modal-direccion-filtro-opciones',
            FiltroOpcion: 'modal-direccion-filtro-opcion',
            FiltroBtnAplicar: 'modal-direccion-btn-filtrar'
        }
    },
    controlesDto: {
        hora: 'hora-dto',
        etiqueta: 'etiqueta-dto'
    },
    Agenda: {
        EventoAjustadoAlContenedor: 'evento-ajustado-al-contenedor'
    },
    modalesDeFiltro: {
        conFiltro: 'con-filtro'
    },
    Bloque: {
        Contenedor: 'grid-span'
    },
    Ampliacion: {
        Contenedor: 'ampliacion-dto'
    },
    Detalle: {
        Contenedor: 'detalle-dto',
        CerrarDetalle: 'img-detalle-dto-cerrar',
        AbrirDetalle: 'img-detalle-dto-abrir',
        ColumnaAccion: atGridDeDetalle.EsAccion,
        AccionDeshabilitada: 'accion-deshabilitada'
    },
    Espan: {
        Cabecera: 'grid-span-cabecera',
        noVisible: 'espan-no-visible',
        conContenido: 'con-Contenido',
        cssExpansor: 'span-expansor',
        cssVisorArchivos: 'contenedor-de-visor',
        barra: 'barra-de-carga',
        animarBarra: 'barra-animada',
        valorTrue: 'valor-true',
        valorFalse: 'valor-false',
        Columnas: {
            resize: 'resizer-borde',
            resizeEnProgreso: 'resizing-en-progreso',
        }
    },
    Archivos: {
        SelectorDeArchivos: ltrTipoControl.SelectorDeArchivos,
        SelectorDeArchivosEnConsulta: 'archivos-en-consulta',
        ContenedorDeArchivo: 'contenedor-de-archivos',
        UnaColumna: 'columnas-de-archivos-1',
        DosColumnas: 'columnas-de-archivos-2',
        TresColumnas: 'columnas-de-archivos-3',
        CuatroColumnas: 'columnas-de-archivos-4',
        CancelarOperacion: 'cancelar-operacion',
        Pegar: 'pegar-archivos',
        Copiar: 'copiar-archivos',
        Mover: 'mover-archivos',
        Cancelado: 'archivo-cancelado',
        ComponenteParaAnexar: 'componente-para-anexar'
    },
    Archivador: {
        CrearFactura: 'crear-factura-rec',
        VerFactura: 'ver-factura-rec'
    },
    Resalto: {
        ConResalto: 'con-resalto',
        Verde: 'resalto-verde',
        Violeta: 'resalto-violeta'
    },
    // etapas en las que está un estado (expansor del editor de estados, ver Estados.ts y Flujo.css)
    etapas: {
        contenedor: 'etapas-contenedor',
        opcion: 'etapas-opcion',
        opcionDeshabilitada: 'etapas-opcion-deshabilitada',
        vacio: 'etapas-vacio'
    },
    flujo: {
        // clases comunes de las modales (CrudModal.css)
        contenedorModal: 'contenedor-modal',
        contenidoModal: 'contenido-modal flujo-modal',
        cabeceraDeModal: 'contenido-cabecera cabecera-de-modal',
        rotuloDeModal: 'rotulo-de-modal',
        iconoDeModal: 'icono-de-modal',
        cerrarDeModal: 'cerrar-de-modal',
        accionesDeCabecera: 'flujo-acciones-cabecera',
        // ayuda de las transiciones (el ? entre las flechas del panel de edición)
        ayudaDeTransiciones: 'flujo-ayuda-transiciones',
        contenidoAyuda: 'contenido-modal flujo-ayuda-modal',
        cuerpoAyuda: 'contenido-cuerpo flujo-ayuda-cuerpo',
        ayudaApartado: 'flujo-ayuda-apartado',
        ayudaAvanzar: 'flujo-ayuda-avanzar',
        ayudaRetroceder: 'flujo-ayuda-retroceder',
        ayudaTexto: 'flujo-ayuda-texto',
        cuerpo: 'contenido-cuerpo flujo-cuerpo',
        pie: 'contenido-pie pie-solo-botones',
        botonGrabar: 'boton-modal btn btn-primary',
        botonRecargar: 'boton-modal btn btn-secondary',
        // clases propias del gráfico (Flujo.css)
        grafico: 'flujo-grafico',
        svg: 'flujo-svg',
        nodoCirculo: 'flujo-nodo-circulo',
        nodoCirculoArrastrando: 'flujo-nodo-circulo-arrastrando',
        nodoTexto: 'flujo-nodo-texto',
        lineaHit: 'flujo-linea-hit',
        linea: 'flujo-linea',
        lineaAvance: 'flujo-linea-avance',
        lineaRetroceso: 'flujo-linea-retroceso',
        lineaHover: 'flujo-linea-hover',
        grupoFlecha: 'flujo-grupo-flecha',
        marcadorHover: 'flujo-marcador-hover',
        marcadorHoverVisible: 'flujo-marcador-hover-visible',
        tooltip: 'flujo-tooltip',
        tooltipVisible: 'flujo-tooltip-visible',
        tooltipRect: 'flujo-tooltip-rect',
        tooltipTexto: 'flujo-tooltip-texto'
    }
};

const enumCssOpcionMenu = {
    DeElemento: 'opcion-menu-de-elemento',
    DeVista: 'opcion-menu-de-vista',
    Basico: 'opcion-menu-basica',
    Bloqueada: 'opcion-mf-bloqueada',
    OcultarLi: 'ocultar-li',
    OcultarHr: 'ocultar-hr',
};
