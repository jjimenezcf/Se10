namespace Terceros {

    export let crudDeProveedor: Terceros.CrudDeProveedores = null;


    export function CrearCrudDeProveedores(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeProveedores(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeProveedor = Crud.crudMnt as Terceros.CrudDeProveedores;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeProveedores extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionProveedor(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionProveedor(this, idPanelEdicion);
        }


        public IraCrear() {
            this.CrearSociedad()
        }

        private CrearSociedad(): boolean {
            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.Sociedad);
            estado.Agregar(ltrClaveDeEstado.paraqueNavegar, enumParaQueNavegar.crear);
            estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
            estado.Guardar();
            let url = `${window.location.origin}/${ltrUrls.Terceros.Sociedades}`;
            EntornoSe.NavegarAUrl(url);
            return true;
        }

    }

    export class CrudCreacionProveedor extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionProveedor extends Crud.CrudEdicion {

        public get ModalDeCreacionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Proveedor.CuentasBancarias);
        }

        public get ModalDeEdicionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Proveedor.CuentasBancarias);
        }
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected Expansor_DespuesDeBorrarRelacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.Expansor_DespuesDeBorrarRelacion(peticion);
            let edicion = peticion.llamador as CrudEdicionProveedor;
            let funcion = this.IdArchivoMostrado == 0 ? (peticion) => this.AlTerminarDeLeerArchivos(peticion) : null;
            ApiDeArchivos.MostrarArchivosAnexados(edicion.PanelDeArchivos.id, edicion.CrudDeMnt.NombreDeNegocio, edicion.Registro.id, funcion);
            edicion.RecargarGridDeTrazas();
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
            let activa = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Proveedor.CuentaActiva, false);
            if (!activa) this.AplicarCuentaNoActiva(modalDeEdicion);
        }

        public AplicarCuentaNoActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.none;
            ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Proveedor.AliasDeCuenta);
        }

        public AplicarCuentaActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.block;
            ApiControl.DesbloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Proveedor.AliasDeCuenta);
        }

        public InicializarDatosDelPago() {
            var modosDePago = ApiControl.BuscarListaDeValores(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.ModoDePago);
            modosDePago.value = atListas.noSeleccionado;
            this.AplicarModoPagoContado();
        }

        public AplicarModoPagoDomiciliacion() {
            var cuentas = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.DomiciliadaEn)
            var tarjetas = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.Tarjeta)
            ApiControl.BloquearLaLista(tarjetas, true);
            ApiControl.BloquearLaLista(cuentas, false);
            var restringirPor = new Array<ClausulaDeFiltrado>();
            restringirPor.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Activa, atCriterio.igual, true))
            MapearAlControl.ListaDeElementos(cuentas, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        }

        public AplicarModoPagoTarjeta() {
            var tarjetas = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.Tarjeta)
            var cuentas = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.DomiciliadaEn)
            ApiControl.BloquearLaLista(cuentas, true);
            ApiControl.BloquearLaLista(tarjetas, false);
            var restringirPor = new Array<ClausulaDeFiltrado>();
            restringirPor.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Sociedad.Tarjeta.Activa, atCriterio.igual, true))
            MapearAlControl.ListaDeElementos(tarjetas, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        }

        public AplicarModoPagoContado() {
            var tarjetas = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.Tarjeta)
            var cuentas = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Terceros.Proveedor.DomiciliadaEn)
            ApiControl.BloquearLaLista(cuentas, true);
            ApiControl.BloquearLaLista(tarjetas, true);
        }
    }


    export function Prv_InicializarModalParaCrearCuentas(idModal: string) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let selector = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Terceros.Proveedor.ClaseDeCuenta);
        MapearAlControl.ListaDeValores(selector, ltrValores.Terceros.Proveedor.ClaseDeCuenta.Ingreso);
    }

    export function Prv_RecargarGridDeArchivos() {
        ApiDeArchivos.MostrarArchivosAnexados(crudDeProveedor.crudDeEdicion.PanelDeArchivos.id, crudDeProveedor.NombreDeNegocio, crudDeProveedor.crudDeEdicion.Registro.id);
        crudDeProveedor.crudDeEdicion.RecargarGridDeTrazas();
    }

    export function Prv_AlPegar_Iban(event) {
        let modal: HTMLDivElement = (crudDeProveedor.crudDeEdicion as CrudEdicionCliente).ModalDeCreacionDeCuentasBancarias;
        let cuentaBancaria: string = ValidarIbanDelPortaPapeles(event);
        MapearCuentaBancaria(modal, cuentaBancaria);
    };

    export function Prv_AlCambiar_CuentaActiva(check: HTMLInputElement) {
        let modal = (crudDeProveedor.crudDeEdicion as CrudEdicionProveedor).ModalDeEdicionDeCuentasBancarias;
        if (!check.checked) (crudDeProveedor.crudDeEdicion as CrudEdicionProveedor).AplicarCuentaNoActiva(modal);
        else (crudDeProveedor.crudDeEdicion as CrudEdicionProveedor).AplicarCuentaActiva(modal);
    }

    export function Prv_Tras_Cambiar_Modo_De_Pago() {
        const editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionProveedor);
        const cg = ApiControl.BuscarListaDinamicaPorPropiedad(editor.PanelDelDto, ltrPropiedades.Terceros.Proveedor.cgPropuesto);
        const modoDePago = ApiControl.BuscarListaDeValores(editor.PanelDelDto, ltrPropiedades.Terceros.Proveedor.ModoDePago);
        const idCg = Numero(cg.getAttribute(atListasDinamicas.idSeleccionado));

        if (modoDePago.value === enumModoDePagoContado.Domiciliacion) {
            if (idCg === 0) {
                modoDePago.value = enumModoDePagoContado.Contado
                MensajesSe.Info('Ha de seleccionar un CG para poder seleccionar la cuenta de domiciliación bancaria donde se domicilia el pago');
            }
            else
                editor.AplicarModoPagoDomiciliacion();
        }
        else if (modoDePago.value === enumModoDePagoContado.Tarjeta) {
            if (idCg === 0) {
                modoDePago.value = enumModoDePagoContado.Contado
                MensajesSe.Info('Ha de seleccionar un CG para poder seleccionar la tarjeta bancaria de con la que se paga');
            }
            else
                editor.AplicarModoPagoTarjeta();
        }
        else if (modoDePago.value === enumModoDePagoContado.Contado) {
            editor.AplicarModoPagoContado();
        }
        else {
            editor.InicializarDatosDelPago();
        }
    }

    export function Prv_Tras_Cambiar_Cg_Propuesto() {
        const editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionProveedor);
        if (!editor.CargaRealizada)
            return;
        editor.InicializarDatosDelPago();
    }
}

