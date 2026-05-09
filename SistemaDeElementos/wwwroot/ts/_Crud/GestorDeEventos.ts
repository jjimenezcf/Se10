namespace Crud {


    function EventosSoloConsulta(accion: string, parametros: any): void {
        try {
            switch (accion) {
                case ltrEventos.Mnt.OcultarMostrarAmpliacion: {
                    let idHtmlBloque: string = parametros;
                    ApiDelCrud.OcultarMostrarExpansor(idHtmlBloque);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Mantenimiento, accion: ${accion}`, error.message);
        }
    }

    export function EventosDelMantenimiento(accion: string, parametros: any): void {
        if (!Definido(crudMnt)) {
            EventosSoloConsulta(accion, parametros);
            return;
        }

        try {
            if (!crudMnt.SoloConGrid) ApiDeMenuFlotante.CerrarMf(crudMnt.ContenedorDeLosMenusDelCrud);
            EntornoSe.OcultarMenusRapidos();
            switch (accion) {
                case ltrEventos.Mnt.Crear: {
                    crudMnt.IraCrear();
                    break;
                }
                case ltrEventos.Mnt.Editar: {
                    crudMnt.IraEditar();
                    break;
                }
                case ltrEventos.Mnt.Historial: {
                    crudMnt.IraHistorial();
                    break;
                }
                case ltrEventos.Mnt.Exportar: {
                    crudMnt.ModalExportacion_Abrir();
                    break;
                }
                case ltrEventos.Mnt.EnviarCorreo: {
                    crudMnt.ModalEnviarCorreo_Abrir();
                    break;
                }
                case ltrEventos.Mnt.Borrar: {
                    crudMnt.ModalDeBorrado_Abrir();
                    break;
                }
                case ltrEventos.Mnt.Dependencias: {
                    let parametrosParaDependencias: string = parametros;
                    crudMnt.IrAlCrudDeDependencias(parametrosParaDependencias);
                    break;
                }
                case ltrEventos.Mnt.Relacionar: {
                    let parametrosParaRelacionar: string = parametros;
                    crudMnt.IrAlCrudDeRelacionarCon(parametrosParaRelacionar);
                    break;
                }
                case ltrEventos.Mnt.AbrirModalParaRelacionar: {
                    let idModal: string = parametros;
                    crudMnt.AbrirModalParaRelacionar(idModal);
                    break;
                }
                case ltrEventos.Mnt.AbrirModalParaImputar: {
                    let idModal: string = parametros;
                    crudMnt.AbrirModalParaImputar(idModal);
                    break;
                }
                case ltrEventos.Mnt.AbrirModalParaConsultarRelaciones: {
                    let idModal: string = parametros;
                    crudMnt.AbrirModalParaConsultarRelaciones(idModal);
                    break;
                }
                case ltrEventos.Mnt.Buscar: {
                    if (crudMnt.ModoTrabajo === enumModoTrabajo.historial || (crudMnt.HayHistorial && crudMnt.EstoyEditandoConsultando)) {
                        EjecutarMenuHistorial(ltrEventos.Historial.CargarHistorial);
                    }
                    else {
                        crudMnt.MenuGrid_DeselecionarTodasLasFilas();
                        crudMnt.CargarGrid();
                    }
                    break;
                }
                case ltrEventos.Mnt.ObtenerSiguientes: {
                    crudMnt.ObtenerSiguientes();
                    break;
                }
                case ltrEventos.Mnt.ObtenerAnteriores: {
                    crudMnt.ObtenerAnteriores();
                    break;
                }
                case ltrEventos.Mnt.ObtenerUltimos: {
                    crudMnt.ObtenerUltimos();
                    break;
                }
                case ltrEventos.Mnt.CompartirElemento: {
                    crudMnt.CompartirElemento();
                    break;
                }
                case ltrEventos.Mnt.EnviarElemento: {
                    crudMnt.EnviarElemento();
                    break;
                }
                case ltrEventos.Mnt.FilaPulsada: {
                    let parIn: Array<string> = parametros.split("#");
                    if (crudMnt.ModoTrabajo === enumModoTrabajo.historial || (crudMnt.HayHistorial && crudMnt.EstoyEditandoConsultando))
                        crudMnt.crudHistorial.FilaPulsada(parIn[0], parIn[1]);
                    else
                        Crud.ManejadorDelClickEnElGrid(parIn[0], parIn[1], event)
                    break;
                }
                case ltrEventos.Mnt.OrdenarPor: {
                    crudMnt.OrdenarPor(parametros, event);
                    break;
                }
                case ltrEventos.Mnt.CambiarSelector: {
                    crudMnt.CambiarValorDelSelector(parametros);
                    break;
                }
                case ltrEventos.Mnt.OcultarMostrarFiltro: {
                    crudMnt.OcultarMostrarFiltro();
                    break;
                }
                case ltrEventos.Mnt.OcultarMostrarBloque: {
                    let idHtmlBloque: string = parametros;
                    crudMnt.OcultarMostrarBloque(idHtmlBloque);
                    break;
                }
                case ltrEventos.Mnt.OcultarMostrarAmpliacion: {
                    let idHtmlBloque: string = parametros;
                    crudMnt.OcultarMostrarExpansor(idHtmlBloque);
                    crudMnt.GuardarSituacionDeEspanes();
                    break;
                }
                case ltrEventos.Mnt.OcultarMostrarDetalle: {
                    let idHtmlBloque: string = parametros;
                    crudMnt.OcultarMostrarExpansor(idHtmlBloque);
                    crudMnt.GuardarSituacionDeEspanes();
                    break;
                }
                case ltrEventos.Mnt.MostrarOcultarVisorDeDetalle: {
                    let idHtmlBloque: string = parametros;
                    crudMnt.MostrarOcultarVisorDeDetalle();
                    crudMnt.GuardarSituacionDeEspanes();
                    break;
                }
                case ltrEventos.Mnt.TeclaPulsada: {
                    if (IsNullOrEmpty(parametros)) crudMnt.TeclaPulsada(crudMnt, event);
                    else {
                        let modal: ModalParaSeleccionar = crudMnt.ObtenerModalParaSeleccionar(parametros);
                        if (modal === undefined) {
                            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Modal ${parametros} no definida al pulsar una tecla`);
                            return;
                        }
                        modal.TeclaPulsada(modal, event);
                    }
                    break;
                }
                case ltrEventos.Mnt.OcultarMostrarColumnas: {
                    let columnas: Array<string> = parametros.split(ltrSimbolos.separadorDeParametrosJs);
                    let propiedades = [];
                    for (let i: number = 0; i < columnas.length; i++) {
                        propiedades.push(columnas[i]);
                    }
                    crudMnt.OcultarMostrarColumnas(propiedades);
                    break;
                }
                case ltrEventos.Mnt.OpcionMenuFlotante: {
                    let parIn: Array<string> = parametros.split("#");
                    ProcesarOpcionMf(crudMnt, null, parIn[0], EsTrue(parIn[1]));
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción '${accion}' no está definida en el gestor de eventos del mantenimiento`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Mantenimiento, accion: ${accion}`, error.message);
        }
    }

    function ProcesarOpcionMf(crud: CrudMnt, editor: CrudEdicion, opcion: string, esContextual: boolean): void {
        switch (opcion) {
            case ltrMenus.eventosDeMf.AbrirEnviarCorreo: {
                crud.ModalEnviarCorreo_Abrir();
                break;
            }
            case ltrMenus.eventosDeMf.AbrirTransitar: {
                EventosModalDeTransitar(ltrEventos.ModalTransitar.Abrir, crud.ModoTrabajo === enumModoTrabajo.mantenimiento ? 'EsCrud' : 'EsEdicion');
                break;
            }
            case ltrMenus.eventosDeMf.ModalDeExportar: {
                crud.ModalExportacion_Abrir();
                break;
            }
            case ltrMenus.eventosDeMf.ModalDeCrearObservacion: {
                NuevaObservacion(crud, editor);
                break;
            }
            case ltrMenus.eventosDeMf.Entorno.Agenda.ModalDeCrearEvento: {
                NuevoEventoDeAgenda(crud, editor);
                break;
            }
            case ltrMenus.eventosDeMf.ModalDeCrearArchivador: {
                NuevoArchivador(crud, editor);
                break;
            }
            case ltrMenus.eventosDeMf.ModalDeCrearDirecciones: {
                NuevaDireccion(crud, editor);
                break;
            }
            case ltrMenus.eventosDeMf.Comun.PermisosDeElemento: {
                AbrirModalDePermisosDeElemento(crud, editor);
                break;
            }
            case ltrMenus.eventosDeMf.alta: {
                DarDeAlta(crud, editor);
                break;
            }
            case ltrMenus.eventosDeMf.baja: {
                DarDeBaja(crud, editor);
                break;
            }
            default:
                if (Definido(editor) && crud.EstoyEditandoConsultando)
                    editor.ProcesarOpcionMf(crud.IdNegocio, opcion, esContextual);
                else
                    crud.ProcesarOpcionMf(crud.IdNegocio, opcion, esContextual);
                break;
        }
    }

    function NuevoEventoDeAgenda(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}-eventos-${enumPostfijoTipoModal.ModalDeCrearVinculo}`
            : `${editor.PanelDeEditar.id}-eventos-${enumPostfijoTipoModal.ModalDeCrearVinculo}`;
        if (Definido(editor))
            EventosDeExpansores(ltrEventos.Expansores.AbrirModalParaCrearYVincular, `${idModal}`);
        else crud.crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(idModal, crud.InfoSelector.Seleccionados[0].Id);
    }

    function NuevaObservacion(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}-observaciones-${enumPostfijoTipoModal.ModalDeCrearRelacion}`
            : `${editor.PanelDeEditar.id}-observaciones-${enumPostfijoTipoModal.ModalDeCrearRelacion}`;
        if (Definido(editor))
            EventosDeExpansores(ltrEventos.Expansores.AbrirModalDeRelacionParaCrear, `${idModal};${atControl.idElemento.toLowerCase()}`);
        else ApiDelCrud.AbrirModalDeRelacionParaCrear(idModal, atControl.idElemento.toLowerCase()
            , crud.InfoSelector.Seleccionados[0].Id
            , crud.InfoSelector.Seleccionados[0].Texto
            , crud.IdNegocio
            , crud.NombreDeNegocio);
    }

    function NuevoArchivador(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}-archivadores-${enumPostfijoTipoModal.ModalDeCrearVinculo}`
            : `${editor.PanelDeEditar.id}-archivadores-${enumPostfijoTipoModal.ModalDeCrearVinculo}`;
        if (Definido(editor))
            EventosDeExpansores(ltrEventos.Expansores.AbrirModalParaCrearYVincular, `${idModal}`);
        else crud.crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(idModal, crud.InfoSelector.Seleccionados[0].Id);
    }

    function NuevaDireccion(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}-direcciones-${enumPostfijoTipoModal.ModalDeCrearRelacion}`
            : `${editor.PanelDeEditar.id}-direcciones-${enumPostfijoTipoModal.ModalDeCrearRelacion}`;
        if (Definido(editor))
            EventosDeExpansores(ltrEventos.Expansores.AbrirModalDeRelacionParaCrear, `${idModal};${atControl.idElemento.toLowerCase()}`);
        else ApiDelCrud.AbrirModalDeRelacionParaCrear(idModal, atControl.idElemento.toLowerCase()
            , crud.InfoSelector.Seleccionados[0].Id
            , crud.InfoSelector.Seleccionados[0].Texto
            , crud.IdNegocio
            , crud.NombreDeNegocio);
    }

    function AbrirModalDePermisosDeElemento(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}.PermisosDelElementoDto`.toLowerCase()
            : `${editor.PanelDeEditar.id}.PermisosDelElementoDto`.toLowerCase();
        EventosModalDeEdicion(ltrEventos.ModalEdicion.AbrirModalDePermisos, `${idModal}`);
    }

    function DarDeAlta(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}.PermisosDelElementoDto`.toLowerCase()
            : `${editor.PanelDeEditar.id}.PermisosDelElementoDto`.toLowerCase();
        EventosModalDeEdicion(ltrEventos.ModalEdicion.DarDeAlta, `${idModal}`);
    }

    function DarDeBaja(crud: CrudMnt, editor: CrudEdicion) {
        let idModal = Definido(crud)
            ? `${crud.crudDeEdicion.PanelDeEditar.id}.PermisosDelElementoDto`.toLowerCase()
            : `${editor.PanelDeEditar.id}.PermisosDelElementoDto`.toLowerCase();
        EventosModalDeEdicion(ltrEventos.ModalEdicion.DarDeBaja, `${idModal}`);
    }

    export function EventosMenuDelGrid(accion: string, idModal: string): void {
        let grid: GridDeDatos = crudMnt.ObtenerGrid(idModal);

        try {
            switch (accion) {
                case ltrEventos.OpcionesDelGrid.SeleccionarTodo: {
                    grid.MenuGrid_SeleccionarTodasLasFilas();
                    break;
                }
                case ltrEventos.OpcionesDelGrid.AnularSeleccion: {
                    grid.MenuGrid_DeselecionarTodasLasFilas(grid);
                    break;
                }
                case ltrEventos.OpcionesDelGrid.AnularOrden: {
                    if (crudMnt.ModoTrabajo === enumModoTrabajo.historial || (crudMnt.HayHistorial && crudMnt.EstoyEditandoConsultando)) {
                        EjecutarMenuHistorial(ltrEventos.Historial.AnularOrdenacion);
                    }
                    else {
                        grid.MenuGrid_AnularOrdenacion();
                    }
                    break;
                }
                case ltrEventos.OpcionesDelGrid.AplicarOrdenInicial: {
                    if (crudMnt.ModoTrabajo === enumModoTrabajo.historial || (crudMnt.HayHistorial && crudMnt.EstoyEditandoConsultando)) {
                        EjecutarMenuHistorial(ltrEventos.Historial.AplicarOrdenInicial);
                    }
                    else {
                        grid.MenuGrid_InicializarOrdenacion();
                    }
                    break;
                }
                case ltrEventos.OpcionesDelGrid.MostrarLasSeleccionadas: {
                    crudMnt.MenuGrid_MostrarSoloSeleccionadas(grid);
                    break;
                }
                case ltrEventos.OpcionesDelGrid.RecargarGrid: {
                    if (crudMnt.ModoTrabajo === enumModoTrabajo.historial || (crudMnt.HayHistorial && crudMnt.EstoyEditandoConsultando)) {
                        EjecutarMenuHistorial(ltrEventos.Historial.CargarHistorial);
                    }
                    else {
                        grid.MenuGrid_DeselecionarTodasLasFilas(grid);
                        grid.CargarGrid();

                    }
                    break;
                }
                case ltrEventos.OpcionesDelGrid.ResetearVista: {
                    crudMnt.ModalDeOcultarColumnas_Resetear();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de selección`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeSeleccion(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: ModalSeleccion = crudMnt.ObtenerModalDeSeleccion(parIn[0]);
        if (modal === undefined) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Modal ${parIn[0]} no definida`);
            return;
        }

        try {
            switch (accion) {
                case ltrEventos.ModalSeleccionDeFiltro.Abrir: {
                    modal.AbrirModalDeSeleccion();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.Cerrar: {
                    modal.CerrarModalDeSeleccion();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.Seleccionar: {
                    modal.SeleccionarElementos();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.FilaPulsada: {
                    let idCheck: string = parIn[1];
                    let idOrigen: string = parIn[2]; // si se ha pulsado en el check o en la fila
                    modal.FilaPulsada(idCheck, idOrigen, false);
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.Buscar: {
                    EventosMenuDelGrid(ltrEventos.OpcionesDelGrid.RecargarGrid, parIn[0]);
                    //modal.RecargarGrid();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.ObtenerSiguientes: {
                    modal.ObtenerSiguientes();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.ObtenerAnteriores: {
                    modal.ObtenerAnteriores();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.ObtenerUltimos: {
                    modal.ObtenerUltimos();
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.OrdenarPor: {
                    let columna: string = parIn[1];
                    modal.OrdenarPor(columna, event);
                    break;
                }
                case ltrEventos.ModalSeleccionDeFiltro.TeclaPulsada: {
                    modal.TeclaPulsada(modal, event);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de selección`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeCrearRelaciones(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: ModalParaRelacionar = crudMnt.ObtenerModalParaRelacionar(parIn[0]);
        if (modal === undefined) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Modal ${parIn[0]} no definida`);
            return;
        }

        try {
            switch (accion) {
                case ltrEventos.ModalParaRelacionar.Cerrar: {
                    modal.CerrarModalParaRelacionar();
                    break;
                }
                case ltrEventos.ModalParaRelacionar.Relacionar: {
                    modal.CrearRelaciones();
                    break;
                }
                case ltrEventos.ModalParaRelacionar.Buscar: {
                    EventosMenuDelGrid(ltrEventos.OpcionesDelGrid.RecargarGrid, parIn[0]);
                    //modal.RecargarGrid();
                    break;
                }
                case ltrEventos.ModalParaRelacionar.ObtenerSiguientes: {
                    modal.ObtenerSiguientes();
                    break;
                }
                case ltrEventos.ModalParaRelacionar.ObtenerAnteriores: {
                    modal.ObtenerAnteriores();
                    break;
                }
                case ltrEventos.ModalParaRelacionar.ObtenerUltimos: {
                    modal.ObtenerUltimos();
                    break;
                }
                case ltrEventos.ModalParaRelacionar.OrdenarPor: {
                    let columna: string = parIn[1];
                    modal.OrdenarPor(columna, event);
                    break;
                }
                case ltrEventos.ModalParaRelacionar.FilaPulsada: {
                    let idCheck: string = parIn[1];
                    let idOrigen: string = parIn[2]; // si se ha pulsado en el check o en la fila
                    modal.FilaPulsada(idCheck, idOrigen, false);
                    break;
                }
                case ltrEventos.ModalParaRelacionar.TeclaPulsada: {
                    modal.TeclaPulsada(modal, event);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de relación`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de crear relaciones, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalParaImputar(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: ModalParaImputar = crudMnt.ObtenerModalParaImputar(parIn[0]);
        if (modal === undefined) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Modal ${parIn[0]} no definida`);
            return;
        }

        try {
            switch (accion) {
                case ltrEventos.ModalParaImputar.Cerrar: {
                    modal.CerrarModalParaImputar();
                    break;
                }
                case ltrEventos.ModalParaImputar.Imputar: {
                    modal.Imputar();
                    break;
                }
                case ltrEventos.ModalParaImputar.Buscar: {
                    EventosMenuDelGrid(ltrEventos.OpcionesDelGrid.RecargarGrid, parIn[0]);
                    //modal.RecargarGrid();
                    break;
                }
                case ltrEventos.ModalParaImputar.ObtenerSiguientes: {
                    modal.ObtenerSiguientes();
                    break;
                }
                case ltrEventos.ModalParaImputar.ObtenerAnteriores: {
                    modal.ObtenerAnteriores();
                    break;
                }
                case ltrEventos.ModalParaImputar.ObtenerUltimos: {
                    modal.ObtenerUltimos();
                    break;
                }
                case ltrEventos.ModalParaImputar.OrdenarPor: {
                    let columna: string = parIn[1];
                    modal.OrdenarPor(columna, event);
                    break;
                }
                case ltrEventos.ModalParaRelacionar.FilaPulsada: {
                    let idCheck: string = parIn[1];
                    let idOrigen: string = parIn[2]; // si se ha pulsado en el check o en la fila
                    modal.FilaPulsada(idCheck, idOrigen, false);
                    break;
                }
                case ltrEventos.ModalParaRelacionar.TeclaPulsada: {
                    modal.TeclaPulsada(modal, event);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de relación`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de crear relaciones, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalParaSeleccionar(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");


        let modal: ModalParaSeleccionar = crudMnt.ObtenerModalParaSeleccionar(parIn[0]);
        if (modal === undefined) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Modal ${parIn[0]} no definida`);
            return;
        }
        try {
            switch (accion) {
                case ltrEventos.ModalParaSeleccionarElementos.FilaPulsada: {
                    let idCheck: string = parIn[1];
                    let idOrigen: string = parIn[2]; // si se ha pulsado en el check o en la fila
                    modal.FilaPulsada(idCheck, idOrigen, false);
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.Buscar: {
                    EventosMenuDelGrid(ltrEventos.OpcionesDelGrid.RecargarGrid, parIn[0]);
                    //modal.RecargarGrid();
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.ObtenerSiguientes: {
                    modal.ObtenerSiguientes();
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.ObtenerAnteriores: {
                    modal.ObtenerAnteriores();
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.ObtenerUltimos: {
                    modal.ObtenerUltimos();
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.OrdenarPor: {
                    let columna: string = parIn[0];
                    modal.OrdenarPor(columna, event);
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.Cerrar: {
                    modal.CerrarModalParaSeleccionar();
                    break;
                }
                case ltrEventos.ModalParaSeleccionarElementos.TeclaPulsada: {
                    modal.TeclaPulsada(modal, event);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos para seleccionar`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de crear relaciones, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeConsultaDeRelaciones(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: ModalParaConsultarRelaciones = crudMnt.ObtenerModalParaConsultarRelaciones(parIn[0]);
        if (modal === undefined) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Modal ${parIn[0]} no definida`);
            return;
        }

        try {
            switch (accion) {
                case ltrEventos.ModalParaConsultaDeRelaciones.Cerrar: {
                    modal.CerrarModalParaConsultarRelaciones();
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.Buscar: {
                    EventosMenuDelGrid(ltrEventos.OpcionesDelGrid.RecargarGrid, parIn[0]);
                    //modal.RecargarGrid();
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.ObtenerSiguientes: {
                    modal.ObtenerSiguientes();
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.ObtenerAnteriores: {
                    modal.ObtenerAnteriores();
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.ObtenerUltimos: {
                    modal.ObtenerUltimos();
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.OrdenarPor: {
                    let columna: string = parIn[1];
                    modal.OrdenarPor(columna, event);
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.FilaPulsada: {
                    let idCheck: string = parIn[1];
                    let idOrigen: string = parIn[2]; // si se ha pulsado en el check o en la fila
                    modal.FilaPulsada(idCheck, idOrigen, false);
                    break;
                }
                case ltrEventos.ModalParaConsultaDeRelaciones.TeclaPulsada: {
                    modal.TeclaPulsada(modal, event);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de relación`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de consulta de relaciones, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeBorrar(accion: string): void {
        try {
            switch (accion) {
                case ltrEventos.ModalBorrar.Cerrar: {
                    crudMnt.ModalDeBorrado_Cerrar();
                    break;
                }
                case ltrEventos.ModalBorrar.Borrar: {
                    crudMnt.ModalDeBorrado_Borrar();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de borrado`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de borrado, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeCreacion(accion: string): void {
        try {
            switch (accion) {
                case ltrEventos.ModalCreacion.Cerrar: {
                    crudMnt.CerrarModalDeCreacion();
                    break;
                }
                case ltrEventos.ModalCreacion.Crear: {
                    crudMnt.CrearElemento();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de creación`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de creación, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeEdicion(accion: string, parametros: string): void {
        try {
            switch (accion) {
                case ltrEventos.ModalEdicion.Cerrar: {

                    if (IsNullOrEmpty(parametros))
                        crudMnt.CerrarModalDeEdicion();
                    else
                        ApiPanel.CerrarModalPorId(parametros);
                    break;
                }
                case ltrEventos.ModalEdicion.Modificar: {
                    crudMnt.ModificarElemento(parametros);
                    break;
                }
                case ltrEventos.ModalEdicion.AbrirModalDePermisos: {
                    crudMnt.AbrirModalDePermisos(parametros);
                    break;
                }
                case ltrEventos.ModalEdicion.DarDeAlta: {
                    crudMnt.EjecutarAccion(Ajax.EndPoint.DarDeAlta);
                    break;
                }
                case ltrEventos.ModalEdicion.DarDeBaja: {
                    crudMnt.EjecutarAccion(Ajax.EndPoint.DarDeBaja);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de edición`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeExportacion(accion: string): void {
        try {
            switch (accion) {
                case ltrEventos.ModalExportacion.Cerrar: {
                    crudMnt.ModalExportacion_Cerrar();
                    break;
                }
                case ltrEventos.ModalExportacion.PulsarSometer: {
                    crudMnt.ModalExportacion_CheckSometerPulsado();
                    break;
                }
                case ltrEventos.ModalExportacion.SalirListaCorreos: {
                    crudMnt.ModalExportacion_SalirDeListaDeCorreos();
                    break;
                }
                case ltrEventos.ModalExportacion.Exportar: {
                    crudMnt.ModalExportacion_Exportar();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de exportación`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeImprimir(accion: string, parametros: string): void {
        let parIn: Array<string> = parametros.split("#");
        try {
            switch (accion) {
                case ltrEventos.ModalImprimir.Abrir: {
                    if (parIn[0] === 'EsCrud')
                        crudMnt.ModalImprimir_Abrir(crudMnt.InfoSelector.IdsSeleccionados[0], new Array<PlantillaDeImpresion>());
                    else
                        crudMnt.ModalImprimir_Abrir(Numero(crudMnt.crudDeEdicion.PanelDeEditar.getAttribute(ltrEdicion.Editando)), new Array<PlantillaDeImpresion>());
                    break;
                }
                case ltrEventos.ModalImprimir.Cerrar: {
                    crudMnt.ModalImprimir_Cerrar();
                    break;
                }
                case ltrEventos.ModalImprimir.Imprimir: {
                    crudMnt.ModalImprimir_Imprimir(crudMnt.EstoyEditando ? ModoAcceso.EsGestor(crudMnt.crudDeEdicion.ModoDeAcceso) : false);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de impresión`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeTransitar(accion: string, parametros: string): void {
        let parIn: Array<string> = parametros.split("#");
        try {
            switch (accion) {
                case ltrEventos.ModalTransitar.Abrir: {
                    if (parIn[0] === 'EsCrud')
                        crudMnt.ModalTransitar_Abrir(crudMnt.InfoSelector.IdsSeleccionados[0]);
                    else
                        crudMnt.ModalTransitar_Abrir(Numero(crudMnt.crudDeEdicion.ElementoEditado.Id));
                    break;
                }
                case ltrEventos.ModalTransitar.Cerrar: {
                    crudMnt.ModalTransitar_Cerrar();
                    break;
                }
                case ltrEventos.ModalTransitar.Transitar: {
                    crudMnt.ModalTransitar_Transitar(crudMnt.EstoyEditando ? ModoAcceso.EsGestor(crudMnt.crudDeEdicion.ModoDeAcceso) : false);
                    break;
                }
                case ltrEventos.ModalTransitar.Seleccionar: {
                    crudMnt.ModalTransitar_Seleccionar();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de transitar`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeEnviarCorreo(accion: string, parametros: string): void {
        let parIn: Array<string> = parametros.split("#");
        try {
            switch (accion) {
                case ltrEventos.ModalEnviarCorreo.Cerrar: {
                    crudMnt.ModalEnviarCorreo_Cerrar();
                    break;
                }
                case ltrEventos.ModalEnviarCorreo.Enviar: {
                    crudMnt.ModalEnviarCorreo_Enviar();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de correo`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EjecutarMenuHistorial(accion: string): void {
        try {
            crudMnt.crudHistorial.EjecutarAcciones(accion);
        }
        catch (error) {
            MensajesSe.Error(`Historial, accion: ${accion}`, error.message);
        }
    }

    export function EjecutarMenuEdt(accion: string): void {
        try {
            crudMnt.crudDeEdicion.EjecutarAcciones(accion, null);
        }
        catch (error) {
            MensajesSe.Error(`Edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosDeEdicion(accion: string): void {
        try {
            crudMnt.crudDeEdicion.EjecutarAcciones(accion, null);
        }
        catch (error) {
            MensajesSe.Error(`Eventos de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosDelMfDeEdicion(opcion: string, esContextual: boolean): void {
        ProcesarOpcionMf(crudMnt, crudMnt.crudDeEdicion, opcion, esContextual);
    }

    export function EventosDelMfDeCreacion(opcion: string, esContextual: boolean): void {
        crudMnt.crudDeCreacion.ProcesarOpcionMf(crudMnt.IdNegocio, opcion, esContextual);
    }

    export function EjecutarMenuCrt(accion: string): void {
        try {
            crudMnt.crudDeCreacion.EjecutarAcciones(accion);
        }
        catch (error) {
            MensajesSe.Error(`Creacion, accion: ${accion}`, error.message);
        }
    }

    export function EventosDeListaDinamica(accion: string, idLista: string) {
        try {
            let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
            switch (accion) {
                case ltrEventos.ListaDinamica.obtenerFoco: {
                    if (idLista.includes('nuevo-cg') && IsNullOrEmpty(lista.value) && Definido(crudMnt) && Definido(crudMnt.crudDeCreacion) && Definido(crudMnt.crudDeCreacion.Sociedad))
                        crudMnt.crudDeCreacion.Sociedad.value = "";
                    ApiListaDinamica.ObtenerFoco(lista);
                    break;
                }
                case ltrEventos.ListaDinamica.Cargar: {
                    if (idLista.includes('nuevo-cg') && IsNullOrEmpty(lista.value) && Definido(crudMnt) && Definido(crudMnt.crudDeCreacion) && Definido(crudMnt.crudDeCreacion.Sociedad))
                        crudMnt.crudDeCreacion.Sociedad.value = "";
                    ApiListaDinamica.Cargar(lista);
                    break;
                }
                case ltrEventos.ListaDinamica.perderFoco: {
                    ApiListaDinamica.PerderFoco(lista);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida para listas dinámicas`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`EventosDeListaDinamica`, error.message + 'Accion: ' + accion + ',idLista: ' + idLista + '.');
        }
    }

    export function EventosModalDeRelacion(accion: string, parametros: string) {
        try {
            switch (accion) {
                case ltrEventos.ModalDeRelacionar.Cerrar: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal de creación dto para poderla cerrar`);
                    let idModal: string = parametros;
                    if (Definido(Consultor))
                        Consultor.CerrarRelacion(idModal);
                    else
                        crudMnt.crudDeEdicion.CerrarRelacion(idModal)
                    break;
                }

                case ltrEventos.ModalDeRelacionar.CrearRelacion: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para crear la relación`);
                    let idModal: string = parametros;
                    crudMnt.crudDeEdicion.CrearRelacion(idModal);
                    break;
                }

                case ltrEventos.ModalDeRelacionar.ModificarRelacion: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para modificar la relación`);
                    let idModal: string = parametros;
                    crudMnt.crudDeEdicion.ModificarRelacion(idModal);
                    break;
                }

                case ltrEventos.ModalDeRelacionar.CrearVinculo: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para crear el elemento y vincularlo`);
                    let idModal: string = parametros;
                    crudMnt.crudDeEdicion.CrearVinculo(idModal);
                    break;
                }

                case ltrEventos.ModalDeRelacionar.CrearDetalle: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para crear el detalle`);
                    let idModal: string = parametros;
                    crudMnt.crudDeEdicion.CrearDetalle(idModal);
                    break;
                }
                case ltrEventos.ModalDeRelacionar.Vincular: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para crear un vínculo`);
                    let idModal: string = parametros;
                    crudMnt.crudDeEdicion.Vincular(idModal);
                    break;
                }

                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de relación`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Creador Dto, accion: ${accion}`, error.message);
        }
    }

    export function EventosDeExpansores(accion: string, parametros: string) {
        try {

            if (Definido(crudMnt) && Definido(crudMnt.crudDeEdicion))
                ApiDeMenuFlotante.CerrarMf(crudMnt.crudDeEdicion.PanelDeEditar);

            switch (accion) {
                case ltrEventos.Expansores.OcultarMostrarBloque: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2)
                        throw Error(`El parametro ${parametros} ha de definir el bloque expansor y el bloque que expande`);
                    let idHtmlExpansor: string = partes[0];
                    let idHtmlBloque: string = partes[1];
                    ApiControl.Expansor_OcultarMostrar(idHtmlExpansor, idHtmlBloque);
                    if (Definido(crudMnt)) {
                        crudMnt.GuardarSituacionDeEspanes();
                    }
                    break;
                }
                case ltrEventos.Expansores.MostrarBloque: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2)
                        throw Error(`El parametro '${parametros}'' ha de definir el bloque expansor y el bloque que expande`);
                    let idHtmlExpansor: string = partes[0];
                    let idHtmlBloque: string = partes[1];
                    ApiControl.Expansor_Mostrar(idHtmlExpansor, idHtmlBloque);
                    break;
                }
                case ltrEventos.Expansores.MostrarPropiedad: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 4)
                        throw Error(`El parametro '${parametros}'' ha de definir el bloque expansor, nº de fila, la propiedad y título a mostrar`);
                    let idHtmlExpansor: string = partes[0];
                    let numeroFila: number = Numero(partes[1]);
                    let propiedad: string = partes[2];
                    let titulo: string = partes[3];
                    if (Definido(Crud.Consultor))
                        Crud.Consultor.Expansor_MostrarPropiedad(idHtmlExpansor, numeroFila, propiedad, titulo);
                    else
                        Crud.crudMnt.crudDeEdicion.Expansor_MostrarPropiedad(idHtmlExpansor, numeroFila, propiedad, titulo);
                    break;
                }
                case ltrEventos.Expansores.NavegarDesdeEdicion: {
                    let partes = parametros.split(';');
                    crudMnt.crudDeEdicion.Expansor_NavegarDesdeEdicion(partes[0], partes[1]);
                    break;
                }
                case ltrEventos.Expansores.NavegarAEditar: {
                    let partes = parametros.split(';');
                    let idGridDeDetalle: string = partes[0];
                    let pagina: string = partes[1];
                    let propiedadRestrictora: string = partes[2];
                    let numeroFila: number = Numero(partes[3]);
                    crudMnt.crudDeEdicion.Expansor_NavegarAEditar(idGridDeDetalle, pagina, propiedadRestrictora, numeroFila);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalParaCrearYVincular: {
                    let idModalDeCreacion: string = parametros;
                    crudMnt.crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(idModalDeCreacion, 0);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalParaCrearDetalle: {
                    let idModalDeCreacion: string = parametros;
                    crudMnt.crudDeEdicion.Expansor_AbrirModalDeCrearDetalle(idModalDeCreacion, 0);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalParaVincular: {
                    let idModalParaVincular: string = parametros;
                    crudMnt.crudDeEdicion.Expansor_AbrirModalParaVincular(idModalParaVincular);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalDeRelacionParaCrear: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2)
                        throw Error(`El parametro ${parametros} ha de definir el id de la modal que se ha de abrir y el nombre de la propiedad restrictora`);
                    let idModalDeCreacion: string = partes[0];
                    let propiedadRestrictora: string = partes[1];

                    crudMnt.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion, propiedadRestrictora);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalDeRelacionParaEditar: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 3)
                        throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle, el restrictor y el número de fila`);
                    let idGridDeDetalle: string = partes[0];
                    let propiedadRestrictora: string = partes[1];
                    let numeroFila: number = Numero(partes[2]);
                    if (Definido(crudMnt))
                        crudMnt.crudDeEdicion.Expansor_AbrirModalDeRelacionParaEditar(idGridDeDetalle, propiedadRestrictora, numeroFila);
                    else
                        Consultor.Expansor_AbrirModalDeRelacionParaEditar(idGridDeDetalle, propiedadRestrictora, numeroFila);
                    break;
                }
                case ltrEventos.Expansores.TrasAbrirModal: {
                    let partes: string[] = parametros.split(';');
                    let idModal: string = partes[0];
                    let acciones: string = partes[1];
                    crudMnt.crudDeEdicion.Expansor_TrasAbrirModal(idModal, acciones);
                    break;
                }
                case ltrEventos.Expansores.TrasCargarExpansor: {
                    let idGridDeDetalle: string = parametros;
                    crudMnt.crudDeEdicion.Expansor_TrasCargarDetalle(idGridDeDetalle);
                    break;
                }
                case ltrEventos.Expansores.BorrarRelacion: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2 && partes.length != 3)
                        throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle y el número de fila y opcionalmente la opción de borrado`);

                    let idGridDeDetalle: string = partes[0];
                    let numeroFila: number = Numero(partes[1]);
                    var accionDeBorrado = partes.length === 3 ? partes[2] : undefined;
                    crudMnt.crudDeEdicion.Expansor_BorrarRelacion(idGridDeDetalle, numeroFila, accionDeBorrado);
                    break;
                }
                case ltrEventos.ModalEdicion.DarDeAlta: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2)
                        throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle y el número de fila`);
                    let idGridDeDetalle: string = partes[0];
                    let numeroFila: number = Numero(partes[1]);
                    crudMnt.crudDeEdicion.Expansor_DarDeAlta(idGridDeDetalle, numeroFila);
                    break;
                }
                case ltrEventos.ModalEdicion.DarDeBaja: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2)
                        throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle y el número de fila`);
                    let idGridDeDetalle: string = partes[0];
                    let numeroFila: number = Numero(partes[1]);
                    crudMnt.crudDeEdicion.Expansor_DarDeBaja(idGridDeDetalle, numeroFila);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida para los expansores`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Expansor, accion: ${accion}`, error.message);
        }
    }

    export function EventosDeSelectorDeElementosEnModal(accion: string, parametros: string) {
        try {
            let parIn: Array<string> = parametros.split("#");
            switch (accion) {
                case ltrEventos.SelectorDeElementos.Seleccionar: {
                    if (parIn.length !== 3)
                        throw new Error(`No se han definido los parámetros de entrada correctos para el evento ${ltrEventos.SelectorDeElementos.Seleccionar}`);
                    crudMnt.AbrirModalParaSeleccionarDesdeUnaModal(parIn[0], parIn[1], parIn[2]);
                    break;
                }
                case ltrEventos.SelectorDeElementos.PerderFoco: {
                    if (parIn.length !== 3)
                        throw new Error(`No se han definido los parámetros de entrada correctos para el evento ${ltrEventos.SelectorDeElementos.Seleccionar}`);
                    crudMnt.PerderElFocoEnUnSelectorDesdeUnaModal(parIn[0], parIn[1], parIn[2]);
                    break;
                }
                case ltrEventos.SelectorDeElementos.ObtenerFoco: {
                    if (parIn.length !== 1)
                        throw new Error(`No se han definido los parámetros de entrada correctos para el evento ${ltrEventos.SelectorDeElementos.Seleccionar}`);
                    crudMnt.ObtenerFocoEnSelector(parIn[0]);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en la modal de selección`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeFiltrado(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: HTMLDivElement = document.getElementById(parIn[0]) as HTMLDivElement;

        try {
            switch (accion) {
                case ltrEventos.ModalDeFiltrado.Abrir: {
                    ApiPanel.AbrirModal(modal);
                    break;
                }
                case ltrEventos.ModalDeFiltrado.Cerrar: {
                    crudMnt.CerrarModalDeFiltro(modal);
                    break;
                }
                case ltrEventos.ModalDeFiltrado.AplicarFiltro: {
                    crudMnt.RestaurarPagina();
                    crudMnt.CerrarModalDeFiltro(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de selección`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDePedirDatos(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: HTMLDivElement = document.getElementById(parIn[0]) as HTMLDivElement;

        try {
            switch (accion) {
                case ltrEventos.ModalDePedirDatos.TrasAbrir: {
                    crudMnt.ModalDePedirDatos_TrasAbrir(modal);
                    break;
                }
                case ltrEventos.ModalDePedirDatos.AlCerrar: {
                    crudMnt.ModalDePedirDatos_Cerrar(modal);
                    break;
                }
                case ltrEventos.ModalDePedirDatos.AlAceptar: {
                    crudMnt.ModalDePedirDatos_Aceptar(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de selección`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeOcultarColumnas(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: HTMLDivElement = document.getElementById(parIn[0]) as HTMLDivElement;

        try {
            switch (accion) {
                case ltrEventos.ModalDeOcultarColumnas.TrasAbrir: {
                    crudMnt.ModalDeOcultarColumnas_TrasAbrir(modal);
                    break;
                }
                case ltrEventos.ModalDeOcultarColumnas.AlCerrar: {
                    crudMnt.ModalDeOcultarColumnas_Cerrar(modal);
                    break;
                }
                case ltrEventos.ModalDeOcultarColumnas.AlAceptar: {
                    crudMnt.ModalDeOcultarColumnas_Aceptar(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de 'EventosModalDeOcultarColumnas'`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }

    export function EventosModalDeTotales(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split("#");
        let modal: HTMLDivElement = document.getElementById(parIn[0]) as HTMLDivElement;

        try {
            switch (accion) {
                case ltrEventos.ModalDeTotales.TrasAbrir: {
                    crudMnt.ModalDeTotales_TrasAbrir(modal);
                    break;
                }
                case ltrEventos.ModalDeTotales.AlCerrar: {
                    crudMnt.ModalDeTotales_Cerrar(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos 'EventosModalDeTotales'`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }


    export function EventosModalDeMensaje(accion: string, parametros: string): void {

        let parIn: Array<string> = parametros.split(ltrSimbolos.separadorDeParametrosJs);
        let modal: HTMLDivElement = document.getElementById(parIn[0]) as HTMLDivElement;

        try {
            switch (accion) {
                case ltrEventos.ModalDeMensaje.AlCerrar: {
                    crudMnt.ModalDeMensaje_Cerrar(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida en el gestor de eventos de selección`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de selección, accion: ${accion}`, error.message);
        }
    }
}

