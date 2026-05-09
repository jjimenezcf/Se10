namespace Gasto {

    export function CrearCrudDeRemesasPag(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Gasto.CrudDeRemesasPag(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeRemesasPag extends Crud.CrudMnt {


        public get ModalPagarRemesa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.RemesasPag.DarPorPagado) as HTMLDivElement; }

        public get ModalRetrocederRemesa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.RemesasPag.RetrocederPago) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionRemesaPag(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionRemesaPag(this, idPanelEdicion);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            if (modal.id === this.ModalPagarRemesa.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Gasto.Rem_PagarRemesa(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalRetrocederRemesa.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Gasto.Rem_RetrocederRemesa(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionRemesaPag extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}

    }

    export class CrudEdicionRemesaPag extends Crud.CrudEdicion {

        private idModalDeIncluirPagos: string = 'pagos';
        public get PanelDePagos(): HTMLDivElement {
            let id = this._idPanelEdicion + '-' + this.idModalDeIncluirPagos;
            return document.getElementById(id) as HTMLDivElement;
        }
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Gasto.RemesasPag.Pagos)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let idCuentaDePago = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.RemesaPag.IdCuentaDePago);
            MapearCuentasDeudor(panel, idCuentaDePago, () => MapearDatosDeCuentaDePago(panel));
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.RemesaPag.Etapas, true);

            if (!EstaElEnumerado(etapas, enumEtapasDeRemesaPag, enumEtapasDeRemesaPag.REM_Etapa_De_Cumplimentacion)) {
                ModoAcceso.AjustarOpcionesDeMenu(panel, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
                ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                ModoAcceso.AplicarloAlPanel(this.PanelDePagos, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
            }

            if (!this.EsInterventor || !EstaElEnumerado(etapas, enumEtapasDeRemesaPag, enumEtapasDeRemesaPag.REM_Etapa_De_Presentacion)) {
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.RemesasPag.RetrocederPago, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.RemesasPag.DarPorPagado, ltrMenus.enumOrigen.edicion);
            }
            else {
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.RemesasPag.RetrocederPago, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.RemesasPag.DarPorPagado, ltrMenus.enumOrigen.edicion);
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            let editor: CrudEdicionRemesaPag = peticion.llamador as CrudEdicionRemesaPag;
            if (opcion === ltrMenus.eventosDeMf.Gasto.RemesasPag.DarPorPagado) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id, peticion.resultado);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Gasto.RemesasPag.RetrocederPago) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id, peticion.resultado);
                return true;
            }
            return false;
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);

            let elemento = peticion.resultado.datos;
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Gasto.RemesaPag.Etapas);
            let modo = ObtenerPropiedad(elemento, ltrPropiedades.Elemento.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            let estaPagado: boolean = ObtenerPropiedad(elemento, ltrPropiedades.Gasto.PagosDeUnaRemesa.EstaPagado);
            let estaAnulado: boolean = ObtenerPropiedad(elemento, ltrPropiedades.Gasto.PagosDeUnaRemesa.EstaAnulado);
            let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(modalDeEdicion, ltrPropiedades.Gasto.PagosDeUnaRemesa.PagadoEl);
            if (estaPagado) {
                etiqueta.innerText = ltrEtiquetas.Gasto.Remesas.PagadoEl;
            }
            else {
                const hoy = new Date();
                const pagarEl = CrearFecha(ObtenerPropiedad(elemento, ltrPropiedades.Gasto.PagosDeUnaRemesa.PagarEl));
                if (hoy >= pagarEl)
                    etiqueta.innerText = ltrEtiquetas.Gasto.Remesas.SeDeberiaHaberPagadoEl;
                else
                    etiqueta.innerText = ltrEtiquetas.Gasto.Remesas.SePagaraEl;
            }

            if (!this.EsGestor) return;


            if (!EstaElEnumerado(etapas, enumEtapasDeRemesaPag, enumEtapasDeRemesaPag.REM_Etapa_De_Presentacion)) {
                ApiPanel.PonerEnModoConsulta(modalDeEdicion);
            }
            else {
                ModoAcceso.AplicarloAlPanel(modalDeEdicion, modo, false);
                ApiControl.BloquearListaDinamicaPorPropiedad(modalDeEdicion, ltrPropiedades.Gasto.PagosDeUnaRemesa.Pago);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(modalDeEdicion, ltrPropiedades.Gasto.PagosDeUnaRemesa.PagadoEl);
                let esInterventor: boolean = this.EsInterventorSinEstado;

                let bloquear: boolean = true;
                if (esInterventor && (estaAnulado || estaPagado)) bloquear = false;

                if (bloquear) {
                   ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Gasto.PagosDeUnaRemesa.Motivo);
                   ApiControl.BloquearSelectorDeFechaPorPropiedad(modalDeEdicion, ltrPropiedades.Gasto.PagosDeUnaRemesa.AnuladoEl);
                }
            }
        }
    }

    export function Rem_Mapear_Datos_Deudor() {
        let panel: HTMLDivElement = Crud.crudMnt.PanelDto;
        let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Elemento.ConCg.Cg);
        if (NoDefinido(lista)) return;

        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            MapearDatosDeSociedad(panel, objeto);
        }
    }

    export function Rem_Blanquear_Datos_Deudor() {
        let panel: HTMLDivElement = Crud.crudMnt.PanelDto;
        let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Elemento.ConCg.Cg);
        if (NoDefinido(lista)) return;
        BlanquearDatosDeSociedad(panel);
    }

    export function Rem_Tras_Seleccionar_Cuenta_Pago() {
        let panel: HTMLDivElement = Crud.crudMnt.PanelDto;
        MapearDatosDeCuentaDePago(panel);
    }

    function MapearDatosDeCuentaDePago(panel: HTMLDivElement) {
        let cuenta = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Gasto.RemesaPag.CuentaDePago) as HTMLSelectElement;
        let oficina = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Oficina);
        let entidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Entidad);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(cuenta);
        entidad.value = Definido(objeto) ? ObtenerPropiedad(objeto, ltrPropiedades.Gasto.RemesaPag.Entidad) : "";
        oficina.value = Definido(objeto) ? ObtenerPropiedad(objeto, ltrPropiedades.Gasto.RemesaPag.Oficina) : "";
    }
    function MapearDatosDeSociedad(panel: HTMLDivElement, cg: any) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, ObtenerPropiedad(cg, ltrPropiedades.Terceros.Cg.idSociedad)));
        ApiDePeticiones.EjecutarPeticion(panel, ltrControladores.Terceros.Sociedades, Ajax.EndPoint.Terceros.Sociedad.LeerDatosDeSociedad, parametros, new Array<Parametro>())
            .then((peticion: ApiDeAjax.DescriptorAjax) => mapearDatosSociedad(peticion))
            .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
    }

    function mapearDatosSociedad(peticion: ApiDeAjax.DescriptorAjax): void {
        let panel = peticion.llamador as HTMLDivElement;
        let deudor = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Deudor);
        MapearAlControl.MapearEditor(deudor, -1, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.Expresion), true, false);
        let presentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Presentador);
        MapearAlControl.MapearEditor(presentador, -1, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.Nombre), true, false);
        let NifDelPresentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.NifDelPresentador);
        MapearAlControl.MapearEditor(NifDelPresentador, -1, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.NIF), true, false);

        let idSociedad: number = ObtenerPropiedad(peticion.resultado.datos, literal.id, 0, true);
        let sociedad = ApiControl.BuscarControl(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true) as HTMLInputElement;
        sociedad.value = idSociedad.toString();

        MapearCuentasDeudor(panel, 0);
    };

    function MapearCuentasDeudor(panel: HTMLDivElement, id: number, trasMapear: Function = null) {
        let cuentas = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Gasto.RemesaPag.CuentaDePago);
        ApiControl.BlanquearListaDeElementos(cuentas);
        BlanquearDatosBancarios(panel);
        MapearAlControl.ListaDeElementos(cuentas, new Array<ClausulaDeFiltrado>(), id, trasMapear);
    }

    function BlanquearDatosDeSociedad(panel: HTMLDivElement) {
        let deudor = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Deudor);
        let presentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Presentador);
        let NifDelPresentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.NifDelPresentador);
        ApiDeInicializacion.Editor(deudor);
        ApiDeInicializacion.Editor(presentador);
        ApiDeInicializacion.Editor(NifDelPresentador);
        let cuentas = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Gasto.RemesaPag.CuentaDePago);
        ApiControl.BlanquearListaDeElementos(cuentas);
        BlanquearDatosBancarios(panel);
        let sociedad = ApiControl.BuscarControl(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true) as HTMLInputElement;
        sociedad.value = "";
    }

    function BlanquearDatosBancarios(panel: HTMLDivElement) {
        let oficina = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Oficina);
        let entidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.RemesaPag.Entidad);
        entidad.value = "";
        oficina.value = "";
    }

}