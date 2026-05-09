namespace Venta {

    export function CrearCrudDeRemesasFae(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Venta.CrudDeRemesasFae(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeRemesasFae extends Crud.CrudMnt {

        public get ModalCargarRemesa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.RemesasFae.Cargar) as HTMLDivElement; }

        public get ModalAnularCargoRemesa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.RemesasFae.AnularCargo) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionRemesaFae(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionRemesaFae(this, idPanelEdicion);
        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax): any {
            super.DespuesDeLeerFilaSeleccionada(peticion);
            let elemento = peticion.resultado.datos;
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Venta.RemesaFae.Etapas, false);
            if (Crud.crudMnt.InfoSelector.Cantidad == 1) {
                if (!EstaElEnumerado(etapas, enumEtapasDeRemesaFae, enumEtapasDeRemesaFae.REM_Etapa_De_Cumplimentacion))
                    ApiDeMenuFlotante.BloquearOpcionDeMenu(Crud.crudMnt.ContenedorMenuIndividual, ltrMenus.eventosDeMf.Comun.Imprimir, ltrMenus.enumOrigen.crud);
                else
                    ApiDeMenuFlotante.DesbloquearOpcionDeMenu(Crud.crudMnt.ContenedorMenuIndividual, ltrMenus.eventosDeMf.Comun.Imprimir, ltrMenus.enumOrigen.crud);
            }
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalCargarRemesa.id || modal.id === this.ModalAnularCargoRemesa.id) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Elemento.Elemento);
                let idRemesa = ObtenerPropiedad(this.crudDeEdicion.Registro, literal.id);
                let remesa = ObtenerPropiedad(this.crudDeEdicion.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.ListaDinamica(lista, idRemesa, remesa, true);
                ApiDelCrud.ProponerPropiedad(this.crudDeEdicion.PanelDeEditar, modal, ltrPropiedades.Venta.RemesaFae.CargarEl, true);
                if (modal.id === this.ModalAnularCargoRemesa.id)
                    ApiDelCrud.ProponerPropiedad(this.crudDeEdicion.PanelDeEditar, modal, ltrPropiedades.Venta.RemesaFae.CargadaEl, true);
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            if (modal.id === this.ModalCargarRemesa.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.CargarRemesa(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalAnularCargoRemesa.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.AnularCargoRemesa(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionRemesaFae extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}

    }

    export class CrudEdicionRemesaFae extends Crud.CrudEdicion {

        private idModalDeIncluirFacturas: string = 'facturas';

        public get PanelDeFacturas(): HTMLDivElement {
            let id = this._idPanelEdicion + '-' + this.idModalDeIncluirFacturas;
            return document.getElementById(id) as HTMLDivElement;
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.RemesasFae.Facturas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let idCuentaAcreedor = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.RemesaFae.IdCuentaDeAbono);
            MapearCuentasAcreedor(panel, idCuentaAcreedor, () => MapearDatosDeCuentaAcreedora(panel));
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.RemesaFae.Etapas, true);

            if (!EstaElEnumerado(etapas, enumEtapasDeRemesaFae, enumEtapasDeRemesaFae.REM_Etapa_De_Cumplimentacion)) {
                ModoAcceso.AjustarOpcionesDeMenu(panel, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
                ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                ModoAcceso.AplicarloAlPanel(this.PanelDeFacturas, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
            }

            if (!this.EsInterventor || !EstaElEnumerado(etapas, enumEtapasDeRemesaFae, enumEtapasDeRemesaFae.REM_Etapa_De_Presentacion)) {
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.RemesasFae.AnularCargo, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.RemesasFae.Cargar, ltrMenus.enumOrigen.edicion);
            }
            else {
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.RemesasFae.AnularCargo, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.RemesasFae.Cargar, ltrMenus.enumOrigen.edicion);
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            let editor: CrudEdicionRemesaFae = peticion.llamador as CrudEdicionRemesaFae;
            if (opcion === ltrMenus.eventosDeMf.Venta.RemesasFae.Cargar) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id, peticion.resultado);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Venta.RemesasFae.AnularCargo) {
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
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Venta.RemesaFae.Etapas, false);
            let modo = ObtenerPropiedad(elemento, ltrPropiedades.Elemento.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            let estaCargada: boolean = ObtenerPropiedad(elemento, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.EstaCargada);
            let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(modalDeEdicion, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.CargadaEl);
            if (estaCargada) {
                etiqueta.innerText = ltrEtiquetas.Venta.Remesas.CargadaEl;
            }
            else {
                const hoy = new Date();
                const cargadael = CrearFecha(ObtenerPropiedad(elemento, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.CargadaEl));
                if (hoy >= cargadael)
                    etiqueta.innerText = ltrEtiquetas.Venta.Remesas.SeDeberiaHaberCargadoEl;
                else
                    etiqueta.innerText = ltrEtiquetas.Venta.Remesas.SeCargaraEl;
            }

            if (!this.EsGestor) return;


            if (!EstaElEnumerado(etapas, enumEtapasDeRemesaFae, enumEtapasDeRemesaFae.REM_Etapa_De_Presentacion)) {
                ApiPanel.PonerEnModoConsulta(modalDeEdicion);
            }
            else {
                ModoAcceso.AplicarloAlPanel(modalDeEdicion, modo, false);
                ApiControl.BloquearListaDinamicaPorPropiedad(modalDeEdicion, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.Factura);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(modalDeEdicion, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.CargadaEl);
                let esInterventor: boolean = this.EsInterventorSinEstado; 
                if (!esInterventor && !estaCargada) {
                    ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.Motivo);
                    ApiControl.BloquearSelectorDeFechaPorPropiedad(modalDeEdicion, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.DevueltoEl);
                }
            }
        }
    }

    export function Rem_Mapear_Datos_Acreedor() {
        let panel: HTMLDivElement = Crud.crudMnt.PanelDto;
        let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Elemento.ConCg.Cg);
        if (NoDefinido(lista)) return;

        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            MapearDatosDeSociedad(panel, objeto);
        }
    }

    export function Rem_Blanquear_Datos_Acreedor() {
        let panel: HTMLDivElement = Crud.crudMnt.PanelDto;
        let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Elemento.ConCg.Cg);
        if (NoDefinido(lista)) return;
        BlanquearDatosDeSociedad(panel);
    }

    export function Rem_Tras_Seleccionar_Cuenta_Abono() {
        let panel: HTMLDivElement = Crud.crudMnt.PanelDto;
        MapearDatosDeCuentaAcreedora(panel);
    }

    function MapearDatosDeCuentaAcreedora(panel: HTMLDivElement) {
        let cuenta = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.RemesaFae.CuentaDeAbono) as HTMLSelectElement;
        let oficina = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Oficina);
        let entidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Entidad);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(cuenta);
        entidad.value = Definido(objeto) ? ObtenerPropiedad(objeto, ltrPropiedades.Venta.RemesaFae.Entidad) : "";
        oficina.value = Definido(objeto) ? ObtenerPropiedad(objeto, ltrPropiedades.Venta.RemesaFae.Oficina) : "";
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
        let acreedor = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Acreedor);
        MapearAlControl.MapearEditor(acreedor, -1, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.Expresion), true, false);
        let presentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Presentador);
        MapearAlControl.MapearEditor(presentador, -1, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.Nombre), true, false);
        let NifDelPresentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.NifDelPresentador);
        MapearAlControl.MapearEditor(NifDelPresentador, -1, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.NIF), true, false);

        let idSociedad: number = ObtenerPropiedad(peticion.resultado.datos, literal.id, 0, true);
        let sociedad = ApiControl.BuscarControl(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true) as HTMLInputElement;
        sociedad.value = idSociedad.toString();

        MapearCuentasAcreedor(panel, 0);
    };

    function MapearCuentasAcreedor(panel: HTMLDivElement, id: number, trasMapear: Function = null) {
        let cuentas = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.RemesaFae.CuentaDeAbono);
        ApiControl.BlanquearListaDeElementos(cuentas);
        BlanquearDatosBancarios(panel);
        MapearAlControl.ListaDeElementos(cuentas, new Array<ClausulaDeFiltrado>(), id, trasMapear);
    }

    function BlanquearDatosDeSociedad(panel: HTMLDivElement) {
        let acreedor = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Acreedor);
        let presentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Presentador);
        let NifDelPresentador = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.NifDelPresentador);
        ApiDeInicializacion.Editor(acreedor);
        ApiDeInicializacion.Editor(presentador);
        ApiDeInicializacion.Editor(NifDelPresentador);
        let cuentas = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.RemesaFae.CuentaDeAbono);
        ApiControl.BlanquearListaDeElementos(cuentas);
        BlanquearDatosBancarios(panel);
        let sociedad = ApiControl.BuscarControl(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true) as HTMLInputElement;
        sociedad.value = "";
    }

    function BlanquearDatosBancarios(panel: HTMLDivElement) {
        let oficina = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Oficina);
        let entidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.RemesaFae.Entidad);
        entidad.value = "";
        oficina.value = "";
    }
}