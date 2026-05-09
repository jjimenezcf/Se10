namespace Terceros {

    export let crudDeInterlocutor: Terceros.CrudDeInterlocutores = null;


    export function CrearCrudDeInterlocutores(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeInterlocutores(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeInterlocutor = Crud.crudMnt as Terceros.CrudDeInterlocutores;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeInterlocutores extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionInterlocutor(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionInterlocutor(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            return (Terceros.NavegarATercero(peticion, ltrParametrosUrl.idInterlocutor));
        }

        public CrearSociedad(): boolean {
            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Sociedad);
            estado.Agregar(ltrClaveDeEstado.paraqueNavegar, enumParaQueNavegar.crear);
            estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
            estado.Guardar();
            let url = `${window.location.origin}/${ltrUrls.Terceros.Sociedades}`;
            EntornoSe.NavegarAUrl(url);
            return true;
        }

        public CrearPersona(): boolean {
            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Persona);
            estado.Agregar(ltrClaveDeEstado.paraqueNavegar, enumParaQueNavegar.crear);
            estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
            estado.Guardar();
            let url = `${window.location.origin}/${ltrUrls.Terceros.Personas}`;
            EntornoSe.NavegarAUrl(url);
            return true;
        }

        public CrearContacto(): boolean {

            if (this.InfoSelector.Cantidad !== 1) {
                MensajesSe.Info("Debe seleccionar la sociedad para la que quiere definir el nuevo contacto");
                return;
            }

            var interlocutor = this.InfoSelector.Seleccionados[0].Registro;
            if (ObtenerPropiedad(interlocutor, ltrPropiedades.Terceros.Interlocutor.IdSociedad, 0) === 0) {
                MensajesSe.Info("El interlocutor seleccionado, debe ser societario");
                return;
            }

            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Sociedad);
            estado.Agregar(ltrClaveDeEstado.paraqueNavegar, enumParaQueNavegar.editar);
            estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
            estado.Agregar(ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearContacto, true);
            estado.Guardar();
            let url = `${window.location.origin}/${ltrUrls.Terceros.Sociedades}?Id=${interlocutor.idSociedad}`;
            EntornoSe.NavegarAUrl(url);
            return true;
        }
    }

    export class CrudCreacionInterlocutor extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }


        public ComenzarCreacion() {
            super.ComenzarCreacion();
            ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Contacto);
        }
    }

    export class CrudEdicionInterlocutor extends Crud.CrudEdicion {

        public get ModalDeCreacionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Interlocutor.CuentasBancarias);
        }
        public get ModalDeEdicionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Interlocutor.CuentasBancarias);
        }
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            Terceros.NavegarATercero(peticion, ltrParametrosUrl.idInterlocutor);
        }

        public AplicarCuentaNoActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.none;
            ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Interlocutor.AliasDeCuenta);
        }

        public AplicarCuentaActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.block;
            ApiControl.DesbloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Interlocutor.AliasDeCuenta);
        }

    }

    export function MostrarInterlocutores(peticion: ApiDeAjax.DescriptorAjax, idFiltro: string): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        switch (opcion) {
            case ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores:
                let filtrarPorBaja: Tipos.Restrictor = new Tipos.Restrictor(ltrPropiedades.Elemento.FitrarPorBaja, 2, undefined, true);
                let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Interlocutor);
                let filtros: Tipos.Restrictor[] = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
                filtros.push(filtrarPorBaja);
                estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
                estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
                estado.Guardar();
                let url = `${window.location.origin}/${ltrUrls.Terceros.Interlocutor}?${ltrParametrosUrl.filtros}=[${idFiltro}=${ids[0]}=${textos[0]}]`;
                //window.open(url, '_blank');
                EntornoSe.NavegarAUrl(url);
                return true;
        }
        return false;
    }

    export function NavegarATercero(peticion: ApiDeAjax.DescriptorAjax, idFiltro: string): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let filtrarPorBaja: Tipos.Restrictor = new Tipos.Restrictor(ltrPropiedades.Elemento.FitrarPorBaja, 2, undefined, true);
        let estado: HistorialSe.EstadoPagina = undefined;
        let filtros: Tipos.Restrictor[] = undefined;
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Maestros.Terceros.Procuradores:
                estado = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Procurador);
                filtros = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
                filtros.push(filtrarPorBaja);
                estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
                urlDestino = `${window.location.origin}/${ltrUrls.Terceros.Procurador}?${ltrParametrosUrl.filtros}=[${idFiltro}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Maestros.Terceros.Abogados:
                estado = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Abogado);
                filtros = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
                estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
                urlDestino = `${window.location.origin}/${ltrUrls.Terceros.Abogado}?${ltrParametrosUrl.filtros}=[${idFiltro}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Maestros.Terceros.Proveedores:
                estado = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Proveedor);
                filtros = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
                filtros.push(filtrarPorBaja);
                estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
                urlDestino = `${window.location.origin}/${ltrUrls.Terceros.Proveedor}?${ltrParametrosUrl.filtros}=[${idFiltro}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Maestros.Terceros.Clientes:
                estado = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Cliente);
                filtros = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
                filtros.push(filtrarPorBaja);
                estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
                urlDestino = `${window.location.origin}/${ltrUrls.Terceros.Cliente}?${ltrParametrosUrl.filtros}=[${idFiltro}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Maestros.Terceros.Trabajadores:
                estado = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Trabajador);
                filtros = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
                filtros.push(filtrarPorBaja);
                estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
                urlDestino = `${window.location.origin}/${ltrUrls.Terceros.Trabajador}?${ltrParametrosUrl.filtros}=[${idFiltro}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
        estado.Guardar();
        EntornoSe.NavegarAUrl(urlDestino);
        return true;
    }

    export function Int_InicializarModalParaCrearCuentas(idModal: string) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let selector = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Terceros.Interlocutor.ClaseDeCuenta);
        MapearAlControl.ListaDeValores(selector, ltrValores.Terceros.Interlocutor.ClaseDeCuenta.Ingreso);
    }

    export function Int_RecargarGridDeArchivos() {
        ApiDeArchivos.MostrarArchivosAnexados(crudDeInterlocutor.crudDeEdicion.PanelDeArchivos.id, crudDeInterlocutor.NombreDeNegocio, crudDeInterlocutor.crudDeEdicion.Registro.id);
        crudDeInterlocutor.crudDeEdicion.RecargarGridDeTrazas();
    }

    export function Int_AlPegar_Iban(event) {
        let modal: HTMLDivElement = (crudDeInterlocutor.crudDeEdicion as CrudEdicionInterlocutor).ModalDeCreacionDeCuentasBancarias;
        let cuentaBancaria: string = ValidarIbanDelPortaPapeles(event);
        MapearCuentaBancaria(modal, cuentaBancaria);
    };

    export function Int_AlCambiar_CuentaActiva(check: HTMLInputElement) {
        let modal = (crudDeInterlocutor.crudDeEdicion as CrudEdicionInterlocutor).ModalDeEdicionDeCuentasBancarias;
        if (!check.checked) (crudDeInterlocutor.crudDeEdicion as CrudEdicionInterlocutor).AplicarCuentaNoActiva(modal);
        else (crudDeInterlocutor.crudDeEdicion as CrudEdicionInterlocutor).AplicarCuentaActiva(modal);
    }

    export function Int_ProcesarOpcionDeMenuLista() {
        var lista = Crud.crudMnt.MenuLista;
        if (Definido(lista)) {
            if (lista.selectedIndex == 0) return;

            const opcion = lista.options[lista.selectedIndex];
            if (opcion.value == ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearSociedad)
                crudDeInterlocutor.CrearSociedad();
            else if (opcion.value == ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearPersona)
                crudDeInterlocutor.CrearPersona();
            else if (opcion.value == ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearContacto)
                crudDeInterlocutor.CrearContacto();
        }
    }
}