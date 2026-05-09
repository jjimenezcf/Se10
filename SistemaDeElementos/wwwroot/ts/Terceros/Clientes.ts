namespace Terceros {

    export let crudDeCliente: Terceros.CrudDeClientes = null;


    export function CrearCrudDeClientes(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeClientes(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeCliente = Crud.crudMnt as Terceros.CrudDeClientes;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeClientes extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionCliente(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionCliente(this, idPanelEdicion);
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

        public CrearSociedad(): boolean {
            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Sociedad);
            estado.Agregar(ltrClaveDeEstado.paraqueNavegar, enumParaQueNavegar.crear);
            estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
            estado.Guardar();
            let url = `${window.location.origin}/${ltrUrls.Terceros.Sociedades}`;
            EntornoSe.NavegarAUrl(url);
            return true;
        }

    }

    export class CrudCreacionCliente extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionCliente extends Crud.CrudEdicion {

        public get ModalDeCreacionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Cliente.CuentasBancarias);
        }

        public get ModalDeEdicionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Cliente.CuentasBancarias);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Clientes.NuevoClienteWeb) {
                var idModal = this.IdModalDeCrearDetalle(ltrEspanes.Tercero.Cliente.ClienteWeb);
                this.Expansor_AbrirModalDeCrearDetalle(idModal, this.Registro.id);
                return true;
            }
            else if (opcion === ltrMenus.eventosDeMf.Maestros.Clientes.AsociarClienteWeb) {
                var idModal = this._idDeModalCrearRelacion(ltrEspanes.Tercero.Cliente.ClienteWeb);
                this.Expansor_AbrirModalDeRelacionParaCrear(idModal, ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            else if (opcion === ltrMenus.eventosDeMf.Maestros.Clientes.PuestoDeTrabajo) {
                var idModal = crudDeCliente.crudDeEdicion._idDeModalCrearRelacion(ltrEspanes.Tercero.Cliente.PuestoDeCliente);
                crudDeCliente.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(idModal, ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            else if (opcion === ltrMenus.eventosDeMf.Maestros.Clientes.CentroAdministrativo) {
                var idModal = crudDeCliente.crudDeEdicion._idDeModalCrearRelacion(ltrEspanes.Tercero.Cliente.CentroAdministrativo);
                crudDeCliente.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(idModal, ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            return false;
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            var cliente = peticion.resultado.datos;
            //ApiPanel.MostrarOcultarCelda(panel, ltrPropiedades.Terceros.Cliente.Vat, ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.EsIntraComunitario) === true)
            ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Terceros.Cliente.Vat, ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.EsIntraComunitario) !== true )
        }

        protected Expansor_DespuesDeBorrarRelacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.Expansor_DespuesDeBorrarRelacion(peticion);
            let edicion = peticion.llamador as CrudEdicionCliente;
            let funcion = this.IdArchivoMostrado == 0 ? (peticion) => this.AlTerminarDeLeerArchivos(peticion) : null;
            ApiDeArchivos.MostrarArchivosAnexados(edicion.PanelDeArchivos.id, edicion.CrudDeMnt.NombreDeNegocio, edicion.Registro.id, funcion);
            edicion.RecargarGridDeTrazas();
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
            if (this.ModalDeEdicionDeCuentasBancarias.id === modalDeEdicion.id) {
                let activa = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Cliente.CuentaActiva, false);
                if (!activa) this.AplicarCuentaNoActiva(modalDeEdicion);
            }

        }

        public AplicarCuentaNoActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`); //   `a[class='formulario-selector-archivo etiqueta-dto'
            selector.style.display = ltrStyle.display.none;
            ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Cliente.AliasDeCuenta);
        }

        public AplicarCuentaActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.block;
            ApiControl.DesbloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Cliente.AliasDeCuenta);
        }
    }

    export function Cli_InicializarModalParaCrearCuentas(idModal: string) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let selector = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Terceros.Cliente.ClaseDeCuenta);
        MapearAlControl.ListaDeValores(selector, ltrValores.Terceros.Cliente.ClaseDeCuenta.Pago);
    }

    export function Cli_RecargarGridDeArchivos() {
        ApiDeArchivos.MostrarArchivosAnexados(crudDeCliente.crudDeEdicion.PanelDeArchivos.id, crudDeCliente.NombreDeNegocio, crudDeCliente.crudDeEdicion.Registro.id);
        crudDeCliente.crudDeEdicion.RecargarGridDeTrazas();
    }

    export function Cli_RecargarGridDeTrazas() {
        crudDeCliente.crudDeEdicion.RecargarGridDeTrazas();
    }

    export function Cli_AlPegar_Iban(event) {
        let modal: HTMLDivElement = (crudDeCliente.crudDeEdicion as CrudEdicionCliente).ModalDeCreacionDeCuentasBancarias;
        let cuentaBancaria: string = ValidarIbanDelPortaPapeles(event);
        MapearCuentaBancaria(modal, cuentaBancaria);
    };

    export function Cli_AlCambiar_CuentaActiva(check: HTMLInputElement) {
        let modal = (crudDeCliente.crudDeEdicion as CrudEdicionCliente).ModalDeEdicionDeCuentasBancarias;
        if (!check.checked) (crudDeCliente.crudDeEdicion as CrudEdicionCliente).AplicarCuentaNoActiva(modal);
        else (crudDeCliente.crudDeEdicion as CrudEdicionCliente).AplicarCuentaActiva(modal);
    }


    export function Cli_ProcesarOpcionDeMenuLista() {
        var lista = Crud.crudMnt.MenuLista;
        if (Definido(lista)) {
            if (lista.selectedIndex == 0) return;

            const opcion = lista.options[lista.selectedIndex];
            if (opcion.value == ltrMenus.BarraDeMenu.Terceros.Cliente.CrearSociedad)
                crudDeCliente.CrearSociedad();
            else if (opcion.value == ltrMenus.BarraDeMenu.Terceros.Cliente.CrearPersona)
                crudDeCliente.CrearPersona();
        }
    }
}