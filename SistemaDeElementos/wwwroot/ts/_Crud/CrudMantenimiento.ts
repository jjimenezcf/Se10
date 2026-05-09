namespace Crud {


    interface Navigator {
        sendBeacon(url: string, data?: BodyInit): boolean;
    }
    export let crudMnt: CrudMnt = null;

    export let Consultor: CrudEdicion = null;

    export let ModoTrabajo: string = Definido(crudMnt) ? crudMnt.ModoTrabajo : Definido(Consultor) ? Consultor.ModoTrabajo : ModoAcceso.ModoDeAccesoDeDatos.SinPermiso;

    export class EstadoDeEspan {
        IdDelEspan: string;
        Abierto: boolean;

        constructor(id: string, abierto: boolean) {
            this.IdDelEspan = id;
            this.Abierto = abierto;
        }
    }

    export class PlantillaDeFiltrado {
        Id: string;
        Plantilla: string;

        constructor(id: string, plantilla: string) {
            this.Id = id;
            this.Plantilla = plantilla;
        }
    }


    export class CrudMnt extends GridDeDatos {
        public crudDeCreacion: CrudCreacion;
        public crudDeEdicion: CrudEdicion;

        public crudHistorial: CrudHistorial;

        private _idModalBorrar: string;
        private _idHistorial: string;
        private _Guid: any;

        private _MostrandoTotales: boolean;
        public get MostrandoTotales(): boolean {
            return this._MostrandoTotales;
        }
        private set MostrandoTotales(valor: boolean) {
            this._MostrandoTotales = valor;
        }


        public get HayHistorial(): boolean {
            return Definido(document.getElementById('crud_historialdto_historial'));
        }

        public get OpcionTransitar(): boolean {
            return Definido(document.getElementById(ltrMenus.IdsDeOpcinesMf.idTransitar)) && !this.EsHistorial;
        }

        public get Guid(): any {
            return this._Guid;
        }

        public get SoloConGrid(): boolean {
            return this.crudDeCreacion === undefined && this.crudDeEdicion === undefined;
        }

        private _siempreEnConsulta: boolean
        public get SiempreEnConsulta(): boolean {
            return this.SoloConGrid || this._siempreEnConsulta || this._errorAlLeerDatosParaInicializarVista;
        }

        private _capaConVinculados: boolean
        public get CapaConVinculados(): boolean {
            return this._capaConVinculados || this._errorAlLeerDatosParaInicializarVista;
        }

        private _errorAlLeerDatosParaInicializarVista: boolean;
        public get LeerDatosParaInicializarVista(): boolean {
            return this._errorAlLeerDatosParaInicializarVista;
        }

        private _iaUsada: string
        public get IaUsada(): enumIa {
            switch (this._iaUsada) {
                case enumIa.IaApyHub.toString(): return enumIa.IaApyHub;
                case enumIa.IaPerplexity.toString(): return enumIa.IaPerplexity;
            }
            MensajesSe.Error("IaUsada", `El enumerado para la Ia '${this.IaUsada}' no está definido`);
        }

        private _usaTotalizador: boolean
        public get UsaTotalizador(): boolean {
            return this._usaTotalizador;
        }


        private _tamanoDelVisor: number
        public get TamanoDelVisor(): number {
            return this._tamanoDelVisor;
        }
        public set TamanoDelVisor(tamano: number) {
            this._tamanoDelVisor = Numero(tamano) === 0 ? 300 : tamano;
        }

        private _visorDeDetalle = null;
        public get VisorDeDetalle(): HTMLDivElement {
            if (!Definido(this._visorDeDetalle)) {
                this._visorDeDetalle = document.querySelector('.' + ltrCss.crud.menuDeDetalle) as HTMLDivElement
            }
            return this._visorDeDetalle as HTMLDivElement
        }

        public get EstaVisualizandoUnSeleccionado(): boolean {
            if (!Definido(this.VisorDeDetalle))
                return false;
            if (this.InfoSelector.Seleccionados.length !== 1)
                return false;
            return !this.VisorDeDetalle.classList.contains(ltrCss.crud.mostrarDetalle);
        }

        private _contenedorDeTablaConGraficos = null;
        public get ContenedorDeTablaConGraficos(): HTMLDivElement {
            if (!Definido(this._contenedorDeTablaConGraficos)) {
                this._contenedorDeTablaConGraficos = document.querySelector('.' + ltrCss.crud.grid.TablaConGraficos) as HTMLDivElement
            }
            return this._contenedorDeTablaConGraficos as HTMLDivElement
        }

        private _contenedorDeTabla = null;
        public get ContenedorDeTabla(): HTMLDivElement {
            if (!Definido(this._contenedorDeTabla)) {
                this._contenedorDeTabla = document.querySelector('.' + ltrCss.crud.grid.ContenedorTabla) as HTMLDivElement
            }
            return this._contenedorDeTabla as HTMLDivElement
        }

        private _contenedorDeGraficos = null;
        public get ContenedorDeGraficos(): HTMLDivElement {
            if (!Definido(this._contenedorDeGraficos)) {
                this._contenedorDeGraficos = document.querySelector('.' + ltrCss.crud.grid.Graficos) as HTMLDivElement
            }
            return this._contenedorDeGraficos as HTMLDivElement
        }

        public get EstaElDtoEnEdicion(): boolean {
            return this.crudDeEdicion.ContenedorDeDatosPrincipales.parentElement === this.crudDeEdicion.PadreContenedorDeDatosPrincipales
        }

        private _splitter = null;
        public get Splitter(): HTMLDivElement {
            if (!Definido(this._splitter)) {
                this._splitter = document.querySelector('.' + ltrCss.crud.grid.Splitter) as HTMLDivElement
            }
            return this._splitter as HTMLDivElement
        }

        private _modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos;
        public get ModoAccesoAlNegocio(): ModoAcceso.enumModoDeAccesoDeDatos {
            return this._modoAccesoAlNegocio;
        }
        public set ModoAccesoAlNegocio(modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            this._modoAccesoAlNegocio = modoDeAcceso;
        }
        public get Cuerpo(): HTMLDivElement {
            return document.getElementById(ltrMantenimiento.idCuerpoDePagina) as HTMLDivElement;
        };

        public get ColorearFilas(): boolean {
            const check = document.getElementById('mostrar-colores') as HTMLInputElement;
            return check ? check.checked : false;
        }

        public get MenuLista(): HTMLSelectElement {
            return document.getElementById(this.IdCuerpoCabecera + '_zona-menu-otras') as HTMLSelectElement;
        }

        private modoTrabajo: string;
        public get ModoTrabajo(): string {
            return this.modoTrabajo;
        }
        public set ModoTrabajo(modo: string) {
            this.modoTrabajo = modo;
        }

        private _mapIndicadores: Map<string, any> = undefined;
        public get MapIndicadores(): Map<string, any> {
            return this._mapIndicadores;
        }

        public get EstoyCreando(): boolean {
            return this.ModoTrabajo === enumModoTrabajo.creando;
        }

        public get EstoyConsultando(): boolean {
            return this.ModoTrabajo === enumModoTrabajo.consultando ||
                (ApiPanel.EsVisible(this.crudDeEdicion.PanelDeEditar) &&
                    (this.ModoTrabajo === enumModoTrabajo.transitando || this.ModoTrabajo === enumModoTrabajo.enviandoCorreo) &&
                    ModoAcceso.EsConsultor(this.ModoAccesoAlNegocio) ||
                    this.crudDeEdicion.EstaCancelada ||
                    this.crudDeEdicion.EstaTerminada ||
                    this.crudDeEdicion.EstaDeBaja);
        }

        public get EstoyEditandoConsultando(): boolean {
            return (this.ModoTrabajo === enumModoTrabajo.editando || this.ModoTrabajo === enumModoTrabajo.consultando) ||
                (ApiPanel.EsVisible(this.crudDeEdicion.PanelDeEditar) &&
                    (this.ModoTrabajo === enumModoTrabajo.transitando || this.ModoTrabajo === enumModoTrabajo.enviandoCorreo) &&
                    ModoAcceso.EsConsultor(this.ModoAccesoAlNegocio));
        }

        public get EstoyEnMantenimiento(): boolean {
            return this.ModoTrabajo === enumModoTrabajo.mantenimiento;
        }

        public get EstoyEditando(): boolean {
            return this.ModoTrabajo === enumModoTrabajo.editando ||
                (ApiPanel.EsVisible(this.crudDeEdicion.PanelDeEditar) &&
                    (this.ModoTrabajo === enumModoTrabajo.transitando || this.ModoTrabajo === enumModoTrabajo.enviandoCorreo) &&
                    ModoAcceso.EsGestor(this.ModoAccesoAlNegocio) &&
                    !this.crudDeEdicion.EstaCancelada &&
                    !this.crudDeEdicion.EstaTerminada &&
                    !this.crudDeEdicion.EstaDeBaja &&
                    !this.SiempreEnConsulta);
        }

        public get SeEstanPidiendoDatos(): boolean {
            return ApiPanel.HayModalAbierta(this.Cuerpo, enumTipoDeModal.ModalParaPedirDatos);
        }

        public get SeEstanCreandoVinculos(): boolean {
            return ApiPanel.HayModalAbierta(this.Cuerpo, enumTipoDeModal.ModalDeCrearVinculo);
        }

        private abriendoModalDeTransitar = false;
        protected get VoyATransitarDesdeEdicion(): boolean {
            return this.abriendoModalDeTransitar && this.EstoyEditandoConsultando;
        }

        private paraqueNavegar: string = null;

        public get ParaqueNavegar(): string {
            if (this.paraqueNavegar === null) {

                this.paraqueNavegar = ObtenerParametroUrl(ltrClaveDeEstado.paraqueNavegar, null, false);
                if (Definido(this.paraqueNavegar))
                    return this.paraqueNavegar;

                if (!this.VengoDeUnPost) {
                    console.log("NO Vengo de un post")
                    if (!this.Estado.Contiene(ltrClaveDeEstado.paraqueNavegar)) {

                        this.paraqueNavegar = undefined;
                    }
                }
                else {
                    console.log("Vengo de un post")
                }

                if (this.paraqueNavegar === null) {

                    if (!this.Estado.Contiene(ltrClaveDeEstado.paraqueNavegar)) {
                        console.log("Viniendo de un post no tengo definido para que navego")
                        this.paraqueNavegar = undefined;
                    }
                    else {
                        this.paraqueNavegar = this.Estado.Sacar(ltrClaveDeEstado.paraqueNavegar);
                        console.log("se define navegar para: " + this.paraqueNavegar)
                    }
                }
            }
            else {
                console.log("Navegar estaba definido para: " + this.paraqueNavegar)
            }
            return this.paraqueNavegar;
        }

        public get VengoDeUnPost(): boolean {
            if (this.PeticionDeMenu)
                return false;
            return !IsNullOrEmpty(this.Estado.Obtener(ltrClaveDeEstado.paginaOrigen));
        }

        private _peticionDeMenu: boolean | null = null;

        public get PeticionDeMenu(): boolean | null {
            if (this._peticionDeMenu === null) {
                this.actualizarPeticionDeMenu();
            }
            return this._peticionDeMenu;
        }

        private actualizarPeticionDeMenu(): void {
            ObtenerParametroUrlAsync(ltrParametrosUrl.origenDePeticion, enumOrigenDeNavegacion.noDefinido, false, (resultado) => {
                this._peticionDeMenu = resultado === enumOrigenDeNavegacion.menu;
            }
            );
        }

        public get HayQueIrACrear(): boolean {
            return this.ParaqueNavegar === enumParaQueNavegar.crear;
        }

        public get ModalMostrarTotales(): HTMLDivElement {
            return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) as HTMLDivElement;
        }

        public get PadreDelPanelDeTotales(): HTMLDivElement {
            const padre = this.ModalMostrarTotales;
            return padre?.querySelector('.' + ltrCss.Modal.ContenidoCuerpo) as HTMLDivElement;
        }

        private _panelDeTotales: HTMLDivElement = null;
        public get PanelDeTotales(): HTMLDivElement {
            if (this._panelDeTotales === null) {
                const padre = this.PadreDelPanelDeTotales;
                this._panelDeTotales = padre?.querySelector('.' + ltrCss.Modal.ContenedorEdicionCuerpo) as HTMLDivElement;
            }
            return this._panelDeTotales;
        }

        protected get ModalDeBorrado(): HTMLDivElement {
            return document.getElementById(this._idModalBorrar) as HTMLDivElement;
        }

        protected get ModalDeExportacion(): HTMLDivElement {
            let idModal: string = this.IdCuerpoCabecera;
            idModal = idModal.replace(ltrTipoControl.Mantenimiento, '');
            idModal = idModal + ltrTipoControl.pnlExportacion; // 'panel-exportacion';
            return document.getElementById(idModal) as HTMLDivElement;
        }

        protected get ModalDeEnviarCorreo(): HTMLDivElement {
            let idModal: string = this.IdCuerpoCabecera;
            idModal = idModal.replace(ltrTipoControl.Mantenimiento, '');
            idModal = idModal + ltrTipoControl.pnlEnviarCorreo; // 'panel-enviar-correo';
            return document.getElementById(idModal) as HTMLDivElement;
        };

        protected get ModalDeOcultarColumnas(): HTMLDivElement {
            let idModal: string = this.IdCuerpoCabecera;
            idModal = idModal.replace('_' + ltrTipoControl.Mantenimiento, '');
            idModal = idModal + '-ocultar-columnas'; // 'panel-enviar-correo';
            return document.getElementById(idModal) as HTMLDivElement;
        };

        protected get ModalDeTransitar(): HTMLDivElement {
            let idModal: string = this.IdCuerpoCabecera;
            idModal = idModal.replace(ltrTipoControl.Mantenimiento, '');
            idModal = idModal + ltrTipoControl.pnlTransitar;
            return document.getElementById(idModal) as HTMLDivElement;
        };

        protected get ModalDeImprimir(): HTMLDivElement {
            let idModal: string = this.IdCuerpoCabecera;
            idModal = idModal.replace(ltrTipoControl.Mantenimiento, '');
            idModal = idModal + ltrTipoControl.pnlImprimir;
            return document.getElementById(idModal) as HTMLDivElement;
        };

        public get ModalParaCrearFiltro(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_GuardarPlantillaFiltrado) as HTMLDivElement; }
        public get ModalParaEliminarFiltro(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_EliminarPlantillaFiltrado) as HTMLDivElement; }


        protected get ModalDeEnviarCorreo_DivDeElementos(): HTMLDivElement {
            return document.getElementById(`${this.ModalDeEnviarCorreo.id}_elementos_ref`) as HTMLDivElement;
        };

        public IdDeModalCrearVincoloCon(negocio: string): string {
            return `${this.crudDeEdicion.PanelDeEditar.id}-${negocio}-${enumPostfijoTipoModal.ModalDeCrearVinculo}`.toLowerCase();
        }

        public ModalDeFiltrado(nombreDadoEnElDescriptor: string): string {
            return `modal-filtro-${this.IdCuerpoCabecera}_filtro.${nombreDadoEnElDescriptor}`.toLowerCase();
        }

        public ModalesDeSeleccion: Array<ModalSeleccion> = new Array<ModalSeleccion>();
        public ModalesParaRelacionar: Array<ModalParaRelacionar> = new Array<ModalParaRelacionar>();
        public ModalesParaImputar: Array<ModalParaImputar> = new Array<ModalParaImputar>();
        public ModalesParaConsultarRelaciones: Array<ModalParaConsultarRelaciones> = new Array<ModalParaConsultarRelaciones>();
        public ModalesParaSeleccionar: Array<ModalParaSeleccionar> = new Array<ModalParaSeleccionar>();

        public get OpcionesGenerales(): NodeListOf<HTMLButtonElement> {
            return this.ZonaDeMenu.querySelectorAll(`input[${atOpcionDeMenu.clase}="${enumCssOpcionMenu.DeVista}"]`) as NodeListOf<HTMLButtonElement>;
        }

        public get PanelDto(): HTMLDivElement {
            if (Crud.crudMnt.EstoyCreando) {
                return this.crudDeCreacion.PanelDeCrear;
            }
            if (Crud.crudMnt.EstoyEditandoConsultando) {
                return this.crudDeEdicion.PanelDeEditar;
            }
            return undefined;
        }

        public get Titulo(): string {
            return Definido(this.ContenedorDeLosMenusDelCrud) ? this.ContenedorDeLosMenusDelCrud.querySelector(`div.${ltrCss.crud.contenedorTitulo}`).innerHTML.trim() : "";
        }

        private _descriptor: string = undefined;
        public get Descriptor(): string {
            if (!Definido(this._descriptor)) {
                this._descriptor = this.CuerpoCabecera.getAttribute(Ajax.Param.descriptor);
            }
            return this._descriptor;
        }

        private _vista: string = undefined;
        public get VistaMvc(): string {
            if (!Definido(this._vista)) {
                this._vista = this.CuerpoCabecera.getAttribute(Ajax.Param.Vista);
            }
            return this._vista;
        }

        constructor(idPanelMnt: string, idModalDeBorrado: string) {
            super(idPanelMnt);
            this._idModalBorrar = idModalDeBorrado;
            this._idHistorial = 'crud_historialdto_historial';
            this._Guid = generarUUID();

            if (this.HayHistorial) {
                this.crudHistorial = new Crud.CrudHistorial(this, this._idHistorial);
            }
        }

        public AntesDeSalir() {
            PonerCapa();
            try {
                this.AccionesAntesDeSalir();

                this.Estado.Sacar(ltrClaveDeEstado.filtrosUrl);
                this.Estado.Guardar();
                EntornoSe.Historial.Persistir();
            }
            catch (error) {
                MensajesSe.MostraExcepcion(error, "AntesDeSalir", MensajesSe.enumTipoMensaje.error);
            }
            finally {
                QuitarCapa();
            }
        }

        protected GuardarEstadosDeExpansores(): void {
            if (this.IdNegocio == 0)
                return null;

            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();

            let datosParaGuardar = ApiDelCrud.EstadosDeLosExpansores(this.crudDeEdicion.PanelDeEditar);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));

            ApiDePeticiones.ProcesarPeticion(this, this.Controlador, this.IdNegocio, ltrMenus.eventosDeMf.Comun.GuardarEstadosDeExpansores, parametros, datosDeEntrada)
                .then((peticion) => {
                    this.DespuesDeGuardarEstadosDeExpansores(peticion);
                })
                .catch((peticion) => {
                    ApiDePeticiones.EmitirError(peticion);
                });
        }

        private DespuesDeGuardarEstadosDeExpansores(peticion: ApiDeAjax.DescriptorAjax) {
        }

        public AccionesAntesDeSalir(): void {
            super.AccionesAntesDeSalir();
            MapearAlDiccionario.Filtros(this.PanelFiltro, this.Estado);
        }

        public RestrictorPor(propiedad: string): Tipos.Restrictor {
            let restrictores: Tipos.Restrictor[] = this.Estado.Obtener(ltrClaveDeEstado.restrictoresUrl) as Tipos.Restrictor[];
            if (NoDefinido(restrictores))
                return undefined;
            return restrictores.find(restrictor => restrictor.Propiedad === propiedad.toLocaleLowerCase());
        }

        public HayRestrictorPor(propiedad: string): boolean {
            return Definido(this.RestrictorPor(propiedad));
        }

        public MostrarOcultarVisorDeDetalle(): void {
            if (!Definido(this.VisorDeDetalle)) return;

            if (this.InfoSelector.Seleccionados.length === 0) {
                this.OcultarPanelDeGraficos();
                return;
            }

            if (this.InfoSelector.Seleccionados.length === 1 && this.VisorDeDetalle.classList.contains(ltrCss.crud.mostrarDetalle)) {
                ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarDetalle);
                this.EditarEnPanelDeGraficos(true);
                return;
            }


            if (this.EstaVisualizandoUnSeleccionado && Definido(this.PanelDeTotales) && !this.MostrandoTotales) {
                this.MostrarDatosTotales();
                return;
            }

            if (this.VisorDeDetalle.classList.contains(ltrCss.crud.mostrarDetalle)) {
                ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarDetalle);
                this.EditarEnPanelDeGraficos(true);
            }
            else {
                this.OcultarPanelDeGraficos();
                return;
            }
        }

        public OcultarVisorDeDetalle(): void {
            ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);
            ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.ocultarDetalle);
            if (!Definido(this.VisorDeDetalle))
                return;
            if (ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarDetalle)) {
                this.OcultarPanelDeGraficos();
            }
        }

        public EditarEnPanelDeGraficos(mostrarDto: boolean): void {
            if (this.modoTrabajo !== enumModoTrabajo.mantenimiento || !Definido(this.ContenedorDeTablaConGraficos))
                return;

            if (this.VisorDeDetalle?.classList.contains(ltrCss.crud.mostrarDetalle)) {
                if (this.InfoSelector.Seleccionados.length > 1 && Definido(this.PanelDeTotales)) {
                    ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);
                }
                else {
                    ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);
                }
                return;
            }

            if (this.InfoSelector.Seleccionados.length > 0 && mostrarDto) {
                this.MostrarPanelDeGraficos(this.InfoSelector.Seleccionados.length);

                if (this.InfoSelector.Seleccionados.length === 1) {
                    if (!this.MostrandoTotales) {
                        this.MostrarDatosPrincipales(this.InfoSelector.IdsSeleccionados[0])
                    }
                    else
                        this.MostrarDatosTotales();
                }
                else {
                    this.MostrarDatosTotales();
                }
            }
            else {
                this.OcultarPanelDeGraficos();
            }
        }

        private MostrarPanelDeGraficos(cantidad: number) {
            if (!Definido(this.VisorDeDetalle))
                return;

            if (!this.VisorDeDetalle.classList.contains(ltrCss.crud.mostrarDetalle) && cantidad === 0) {
                this.OcultarPanelDeGraficos();
                return;
            }

            ApiControl.ExcluirCss(this.ContenedorDeGraficos, ltrCss.divNoVisible);
            ApiControl.ExcluirCss(this.Splitter, ltrCss.divNoVisible);
            ApiDelCrud.MostrarContenedorDeGraficos();

            if (cantidad === 1) {
                if (!this.MostrandoTotales) this.PonerElDtoEnElVisorDeGraficos();
                return;
            }

            if (!Definido(this.PadreDelPanelDeTotales)) {
                this.OcultarVisorDeDetalle();
                return;
            }
            this.PonerElDtoEnEdicion();

            //if (this.PadreDelPanelDeTotales === this.PanelDeTotales.parentElement)
            //    this.ContenedorDeGraficos.appendChild(this.PanelDeTotales);
        }

        private OcultarPanelDeGraficos() {

            ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.ocultarDetalle);
            ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarDetalle);
            if (this.InfoSelector.Seleccionados.length > 1)
                ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);
            else
                ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);

            if (ApiControl.IncluirCss(this.ContenedorDeGraficos, ltrCss.divNoVisible)) {
                ApiControl.IncluirCss(this.Splitter, ltrCss.divNoVisible);
                this.MostrandoTotales = false;
                ApiDelCrud.OcultarContenedorDeGraficos();
            }
        }

        private MostrarDatosPrincipales(id: number) {
            const parametros: Array<Parametro> = this.crudDeEdicion.ParametrosParaLeerElementoPorId();
            ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, parametros, id)
                .then((peticion) => this.MapearDatosPrincipales(peticion))
                .catch((peticion) => this.SiHayErrorAlLeerElemento(peticion));
        }

        private async MostrarDatosTotales() {
            if (!Definido(this.PanelDeTotales))
                return;

            this.PonerElPanelDeTotalesEnElVisorDeGraficos();

            try {
                ApiPanel.BlanquearControlesDeIU(this.PanelDeTotales);
                const filtros: Array<Parametro> = new Array<Parametro>();
                const clausulas = this.ObtenerFiltrosParaLasSeleccionadas();
                filtros.push(new Parametro(Ajax.Param.filtro, clausulas));
                const peticion = await ApiDePeticiones.Totales(this, this.Controlador, 0, -1, filtros);
                const totales = peticion.resultado.datos;
                MapearAlPanel.ElObjeto(this.PanelDeTotales, totales, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
                this.MostrandoTotales = true;
            }
            catch (error) {
                this.MostrandoTotales = false;
                MensajesSe.MostraExcepcion(error)
                this.OcultarVisorDeDetalle();
            }
        }

        private PonerElPanelDeTotalesEnElVisorDeGraficos() {
            this.PonerElDtoEnEdicion();

            ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarDetalle);
            ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);
            ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.ocultarDetalle);

            if (this.PadreDelPanelDeTotales === this.PanelDeTotales.parentElement)
                this.ContenedorDeGraficos.appendChild(this.PanelDeTotales);
        }

        private PonerElDtoEnElVisorDeGraficos() {
            this.MostrandoTotales = false;
            if (this.ContenedorDeGraficos === this.PanelDeTotales?.parentElement)
                this.PadreDelPanelDeTotales.insertBefore(this.PanelDeTotales, this.PadreDelPanelDeTotales.appendChild(this.PanelDeTotales));

            if (this.EstaElDtoEnEdicion)
                this.ContenedorDeGraficos.appendChild(this.crudDeEdicion.ContenedorDeDatosPrincipales);

            if (Definido(this.PanelDeTotales))
                ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarTotales);
            else
                ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.ocultarDetalle);
        }

        private PonerElDtoEnEdicion() {
            if (!this.EstaElDtoEnEdicion)
                this.crudDeEdicion.PadreContenedorDeDatosPrincipales.appendChild(this.crudDeEdicion.ContenedorDeDatosPrincipales);
        }

        private MapearDatosPrincipales(peticion: ApiDeAjax.DescriptorAjax,) {
            const modo = ModoAcceso.Parsear(peticion.resultado.modoDeAcceso);
            if (modo === ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso) {
                MensajesSe.Info("El usuario conectado no tiene permiso al elemento editado");
                this.OcultarPanelDeGraficos();
                return;
            }
            ApiPanel.BlanquearControlesDeIU(crudMnt.ContenedorDeGraficos, false);

            MapearAlPanel.ElObjeto(crudMnt.ContenedorDeGraficos, peticion.resultado.datos, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, new Array<string>());
        }

        private SiHayErrorAlLeerElemento(peticion: ApiDeAjax.DescriptorAjax) {
            this.OcultarPanelDeGraficos();
            if (Definido(peticion.resultado))
                MensajesSe.Error("SiHayErrorAlLeerElemento", peticion.resultado.mensaje, peticion.resultado.consola);
            else
                MensajesSe.Error("SiHayErrorAlLeerElemento", 'error al acceder a los datos', "No se ha podido resolver la petición para obtener los datos");
        }

        private EstadosDeExpansores() {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosParaGuardar = ApiDelCrud.EstadosDeLosExpansores(this.crudDeEdicion.PanelDeEditar);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            return JSON.stringify(parametros);
        }

        private DisposicionDelEncolumnado() {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosParaGuardar = ApiDeGrid.DisposicionDelEncolumnado(this.Tabla);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            return JSON.stringify(parametros);
        }

        private OrdenacionDelResultado() {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosParaGuardar = ApiDeGrid.OrdenacionDelResultado(this.Ordenacion);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            return JSON.stringify(parametros);
        }

        private TamanoDelEncolumnado() {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosParaGuardar = ApiDeGrid.TamanoDelEncolumnado(this.Tabla);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            return JSON.stringify(parametros);
        }


        public Inicializar(idPanelMnt: string) {
            this.Inicializando = true;
            try {
                if (Registro.EsClienteWeb()) {
                    Crud.crudMnt.ContenedorDeLosMenusDelCrud.classList.add('div-mnt-menu-contenedor-clienteWeb')
                    var ContenedorMenuCabeceraEdicion = Crud.crudMnt.crudDeEdicion.PanelDeEditar.querySelector('.contenedor-edicion-cabecera');
                    if (Definido(ContenedorMenuCabeceraEdicion)) {
                        ContenedorMenuCabeceraEdicion.classList.add('contenedor-edicion-cabecera-clienteWeb');
                    }
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                    ApiPanel.OcultarPanel(Crud.crudMnt.ContenedorMenuDeFiltro);
                    ApiPanel.OcultarPanel(Crud.crudMnt.ContenedorMenuContextual);
                    ApiPanel.OcultarPanel(Crud.crudMnt.ContenedorMenuDeRelacion);
                }

                if (IsNullOrEmpty(idPanelMnt))
                    idPanelMnt = this.IdCuerpoCabecera;

                super.Inicializar(idPanelMnt);
                this.Estado.Agregar(ltrClaveDeEstado.guid, this.Guid);
                var seguirRetrocediendo = this.Estado.Sacar(ltrClaveDeEstado.SeguirRetrocediendo);
                if (seguirRetrocediendo)
                    EntornoSe.NavegarAtras();
                this.ModoTrabajo = enumModoTrabajo.mantenimiento;
                this.InicializarCrud();
            }
            catch (error) {
                this.Estado.Guardar();
                MensajesSe.Error("Inicializando el crud", `Error al inicializar el crud ${this.IdCuerpoCabecera}`, error.message);
            }
            finally {
                this.Inicializando = false;
            }
        }

        public async GuardarSituacionDeEspanes() {
            const params1 = {
                [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, this.IdNegocio),
                [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, this.IdVista),
                [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.GuardarEstadosDeExpansores)
            };
            const url1 = `/${this.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params1)}`;
            await fetch(url1, {
                method: 'POST',
                body: this.EstadosDeExpansores(),
                keepalive: true
            });
        }

        public async GuardarSituacionDeColumnas() {
            const params2 = {
                [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, this.IdNegocio),
                [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, this.IdVista),
                [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.DisposicionDelEncolumnado)
            };
            const url2 = `/${this.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
            await fetch(url2, {
                method: 'POST',
                body: this.DisposicionDelEncolumnado(),
                keepalive: true
            });
        }

        public async GuardarOrdenacionDelResultado() {
            if (!this.EsCrud || !Definido(this.EnumeradoDeNegocio) || this.EnumeradoDeNegocio == enumNegocio.No_Definido)
                return;

            const params2 = {
                [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, this.IdNegocio),
                [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, this.IdVista),
                [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.OrdenacionDelResultado)
            };
            const url2 = `/${this.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
            await fetch(url2, {
                method: 'POST',
                body: this.OrdenacionDelResultado(),
                keepalive: true
            });
        }

        public async GuardarTamanoDeColumnas() {
            const params2 = {
                [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, this.IdNegocio),
                [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, this.IdVista),
                [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.TamanoDelEncolumnado)
            };
            const url2 = `/${this.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
            await fetch(url2, {
                method: 'POST',
                body: this.TamanoDelEncolumnado(),
                keepalive: true
            });
        }

        protected Expansor_InyectarAccesoIA(): void {
            // Buscamos el contenedor por clase
            const contenedor = document.querySelector('.div-mnt-filtro-expansor');

            if (contenedor && !document.getElementById('acceso-ia-preguntame')) {
                // Creamos el nuevo enlace
                const linkIA = document.createElement('a');
                linkIA.id = 'acceso-ia-preguntame';
                linkIA.href = 'javascript:void(0);';
                linkIA.innerText = 'Pregúntame';
                linkIA.style.marginRight = '15px'; // Separación del "Mostrar filtro"
                linkIA.style.fontWeight = 'bold';
                linkIA.style.color = '#007bff'; // O el color corporativo que uses

                // Al hacer clic, abre la modal especial
                linkIA.onclick = () => this.Expansor_ModalPreguntarIA();

                // Lo insertamos como primer hijo del contenedor (aparece antes de "Mostrar filtro")
                contenedor.insertBefore(linkIA, contenedor.firstChild);
            }
        }

        public Expansor_ModalPreguntarIA(): void {
            const modalOverlay = document.createElement('div') as HTMLDivElement;
            ApiControl.IncluirCss(modalOverlay, ltrCss.crud.modal.dinamica);

            const historial = this.Preguntas;
            const ultima = this.UltimaPregunta;

            // 1. Generamos el HTML del historial
            let htmlHistorial = '';
            if (historial.length > 0) {
                const recientes = historial.slice(-5).reverse();
                htmlHistorial = ltrHtml.Divs.PreguntasIa.replace('[PreguntasIa]', recientes.map(q => `<div class='${ltrCss.crud.modal.LineaDelhistorialIa}' title='${q.pregunta.replace(/"/g, '&quot;')}'>${q.pregunta}</div>`).join(''));
            }

            // 2. Montamos el HTML con las dos vistas (Principal y Edición JSON)
            modalOverlay.innerHTML = ltrHtml.Modales.PreguntasIa
                .replace('[htmlHistorial]', htmlHistorial)
                .replace('[pregunta]',  '' );

            if (this.NuevaConversacion === true || historial.length === 0) {
                modalOverlay.innerHTML = modalOverlay.innerHTML.replace('[nuevaconservacion]', 'checked');
                this.NuevaConversacion = true;
            }
            document.body.appendChild(modalOverlay);

            // --- REFERENCIAS ---
            const vistaPregunta = modalOverlay.querySelector('#vista-pregunta-ia') as HTMLDivElement;
            const vistaJson = modalOverlay.querySelector('#vista-edicion-json') as HTMLDivElement;
            const txtInput = modalOverlay.querySelector('#input-pregunta-ia') as HTMLTextAreaElement;
            const txtJson = modalOverlay.querySelector('#textarea-json-ia') as HTMLTextAreaElement;

            const btnRespuesta = modalOverlay.querySelector('#btn-ver-respuesta-json');
            const btnVolver = modalOverlay.querySelector('#btn-volver-pregunta');
            const btnGrabar = modalOverlay.querySelector('#btn-grabar-json');
            const btnPreguntar = modalOverlay.querySelector('#btn-ejecutar-pregunta');
            const btnCerrar = modalOverlay.querySelector('#btn-cerrar-dinamico');

            // --- LÓGICA DE INTERCAMBIO DE VISTAS ---
            btnRespuesta?.addEventListener('click', () => {
                ApiPanel.OcultarPanel(vistaPregunta);
                ApiPanel.MostrarPanel(vistaJson);
                const respuestaOriginal = this.Preguntas.find(p => p.pregunta === txtInput.value)?.respuesta;
                ApiControl.MapearEnElAreaDeTextoUnJoson(txtJson, respuestaOriginal);
            });

            btnVolver?.addEventListener('click', () => {
                ApiPanel.OcultarPanel(vistaJson);
                ApiPanel.MostrarPanel(vistaPregunta);
            });

            // --- LÓGICA DE GRABAR ---
            btnGrabar?.addEventListener('click', () => {
                const datos = {
                    texto: txtInput.value,
                    json: txtJson.value
                };

                // Llamada al endpoint
                //Api.Post(eventosDeMf.epSalvarFiltroIa, datos).then(() => {
                //    Msg.Info("Filtro guardado correctamente");
                //    cerrar();
                //});
            });

            // --- LÓGICA EXISTENTE ---
            const itemsHistorial = modalOverlay.querySelectorAll('.' + ltrCss.crud.modal.LineaDelhistorialIa);
            itemsHistorial.forEach(item => {
                item.addEventListener('click', (e) => {
                    txtInput.value = (e.currentTarget as HTMLElement).innerText;
                    txtInput.focus();
                });
            });

            setTimeout(() => ApiControl.IncluirCss(modalOverlay, 'fade-in'), 10);
            const cerrar = () => {
                ApiControl.ExcluirCss(modalOverlay, 'fade-in');
                setTimeout(() => modalOverlay.remove(), 300);
            };

            btnPreguntar?.addEventListener('click', () => {
                const pregunta = txtInput.value.trim();
                if (pregunta) {
                    this.LanzarPregunta(pregunta);
                    cerrar();
                } else {
                    txtInput.focus();
                    txtInput.style.borderColor = 'red';
                }
            });

            btnCerrar?.addEventListener('click', cerrar);
            modalOverlay.onclick = (e) => { if (e.target === modalOverlay) cerrar(); };
            setTimeout(() => txtInput.focus(), 100);
        }

        public Expansor_ModalPreguntarIA_old(): void {
            const modalOverlay = document.createElement('div');
            modalOverlay.className = ltrCss.crud.modal.dinamica;

            // 1. Obtenemos el historial y la última pregunta
            const historial = this.Preguntas; // Array<string>
            const ultima = this.UltimaPregunta;

            // 2. Generamos el HTML del historial (solo si hay preguntas previas)
            let htmlHistorial = '';
            if (historial.length > 0) {
                // Mostramos las últimas 5 para no saturar la modal
                const recientes = historial.slice(-5).reverse();
                htmlHistorial = `
            <div class='${ltrCss.crud.modal.ContenedorDeHistorialIa}'>
                <p>Consultas recientes:</p>
                <div id='contenedor-historial-ia'>
                    ${recientes.map(q => `
                        <div class='${ltrCss.crud.modal.LineaDelhistorialIa}' 
                             title='${q.pregunta.replace(/"/g, '&quot;')}'>
                             ${q}
                        </div>
                    `).join('')}
                </div>
            </div>
        `;
            }

            // 3. Montamos el HTML completo
            modalOverlay.innerHTML = `
        <div style="min-width: 450px; max-width: 500px;">
            <h2 style="margin-top: 0;">Asistente de Filtrado</h2>
            
            <div class="${ltrCss.crud.modal.cuerpo}">
                ${htmlHistorial}
                
                <p>¿Qué estás buscando? Cuéntamelo y generaré los filtros por ti:</p>
                <textarea id="input-pregunta-ia" 
                          placeholder="Indícame nombre, tipo o centro gestor..." 
                          style="width: 100%; height: 90px; padding: 10px; border-radius: 4px; border: 1px solid #ccc; font-family: inherit; resize: none;"
                >${(ultima == null ? '' : ultima)}</textarea>
            </div>
    
            <div class="${ltrCss.crud.modal.pie}" style="display: flex; gap: 10px; margin-top: 15px;">
                <button class="${ltrCss.crud.modal.boton}" id="btn-ejecutar-pregunta" style="background-color: #28a745; color: white; padding: 8px 15px; border: none; border-radius: 4px; cursor: pointer;">
                    Preguntar
                </button>
                <button class="${ltrCss.crud.modal.boton}" id="btn-cerrar-dinamico">
                    Cancelar
                </button>
            </div>
        </div>
    `;

            document.body.appendChild(modalOverlay);

            // Referencias
            const txtInput = modalOverlay.querySelector('#input-pregunta-ia') as HTMLTextAreaElement;
            const btnPreguntar = modalOverlay.querySelector('#btn-ejecutar-pregunta');
            const btnCerrar = modalOverlay.querySelector('#btn-cerrar-dinamico');
            const itemsHistorial = modalOverlay.querySelectorAll('.' + ltrCss.crud.modal.LineaDelhistorialIa);

            // 4. Lógica para hacer clic en un elemento del historial
            itemsHistorial.forEach(item => {
                item.addEventListener('click', (e) => {
                    const texto = (e.currentTarget as HTMLElement).innerText;
                    txtInput.value = texto;
                    txtInput.focus();
                    // Opcional: Podrías disparar la pregunta directamente aquí
                });
                // Efecto hover simple
                item.addEventListener('mouseenter', () => (item as HTMLElement).style.backgroundColor = '#f0f7ff');
                item.addEventListener('mouseleave', () => (item as HTMLElement).style.backgroundColor = '#fff');
            });

            // --- El resto de tu lógica se mantiene igual ---
            setTimeout(() => modalOverlay.classList.add('fade-in'), 10);
            const cerrar = () => {
                modalOverlay.classList.remove('fade-in');
                setTimeout(() => modalOverlay.remove(), 300);
            };

            btnPreguntar?.addEventListener('click', () => {
                const pregunta = txtInput.value.trim();
                if (pregunta) {
                    this.LanzarPregunta(pregunta);
                    cerrar();
                } else {
                    txtInput.focus();
                    txtInput.style.borderColor = 'red';
                }
            });

            btnCerrar?.addEventListener('click', cerrar);
            modalOverlay.onclick = (e) => { if (e.target === modalOverlay) cerrar(); };
            setTimeout(() => txtInput.focus(), 100);
        }

        public LanzarPregunta(texto: string): void {
            this.Pregunta = texto;
            this.NuevaConversacion = (document.getElementById('chk-nueva-pregunta') as HTMLInputElement).checked;
            this.CargarGrid();
        }

        private InicializarCrud(): void {
            ApiDelCrud.CambiarPanelActivoDelCrud(this.modoTrabajo);
            this.InicializarPanelDeFiltro(this.PanelFiltro);
            this.InicializarModalesDeFiltrado();

            if (!Registro.EsMovil()) {
                ApiDelCrud.ConfigurarEventosDeCambioDelAnchoContenedorDeTablaConGraficos();
            }
            ApiDelCrud.OcultarContenedorDeGraficos();

            if (Definido(this.ZonaDeMenu)) {
                ApiDeInicializacion.InicializarOpcionesDeMenu(this.ZonaDeMenu);
                ApiDeInicializacion.InicializarOpcionesDeMenuDeElemento(this.ZonaDeMenu);
            }

            ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuIndividual, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeElemento, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
            if (Registro.EsClienteWeb())
                ApiDeMenuFlotante.AplicarClienteWeb(Crud.crudMnt.ContenedorMenuIndividual, 0);
            else
                ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.crud, this.ContenedorMenuIndividual, false, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);

            ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuDeRelacion, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeElemento, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
            ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.crud, this.ContenedorMenuDeRelacion, false, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);

            //inyectarBotonPegarFecha();

            if (this.ModoAccesoAlNegocio === undefined) {
                var parametros = new Array<Parametro>();
                parametros.push(new Parametro(Ajax.Param.modo, enumModoTrabajo.mantenimiento))
                parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.Titulo))
                parametros.push(new Parametro(Ajax.Param.descriptor, this.Descriptor))
                this._errorAlLeerDatosParaInicializarVista = false;
                ApiDePeticiones.LeerDatosParaInicializarVista(this, this.Controlador, this.NombreDeNegocio, parametros)
                    .then((peticion) => {
                        this.DespuesDeLeerDatosParaInicializarVista(peticion)
                    })
                    .catch((peticion) => {
                        this.ErrorAlLeerModoAccesoAlNegocio(peticion);
                    }
                    );
            }
            else {
                ModoAcceso.AplicarModoDeAccesoALasOpcionesDelNegocio(this.OpcionesGenerales, this.ModoAccesoAlNegocio);
                this.DespuesDeInicializarCrud(this.ModoAccesoAlNegocio);
                this.CargarGrid();
            }
        }

        private IndicarSiHayFiltrosEnAlgunaModal(): void {
            if (this.HayQueIrACrear) return;
            let divs: NodeListOf<HTMLDivElement> = this.Cuerpo.querySelectorAll(`div[${atControl.tipoModal}=${enumTipoDeModal.ModalDeFiltrado}`) as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < divs.length; i++)
                this.IndicaSiHayFiltros(divs[i]);
        }

        private InicializarModalesDeFiltrado(): void {
            let divs: NodeListOf<HTMLDivElement> = this.Cuerpo.querySelectorAll(`div[${atControl.tipoModal}=${enumTipoDeModal.ModalDeFiltrado}`) as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < divs.length; i++)
                this.InicializarPanelDeFiltro(divs[i]);
        }

        protected InicializarPanelDeFiltro(div: HTMLDivElement): void {
            ApiDeInicializacion.InicializarEditores(div);
            ApiDeInicializacion.InicializarListasDeElementos(div);
            ApiDeInicializacion.InicializarListasDinamicas(div);
            ApiDeInicializacion.InicializarSelectoresDeFecha(div);
            ApiDeInicializacion.InicializarEntreFechas(div);
            ApiDeInicializacion.Checkes(div);
            this.InicializarSelectores(div);
        }

        protected EditarRegistro() {
            let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.buscar);
            let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.buscar, 0);
            ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.buscar, 0, this.Navegador.Cantidad, datosDeEntrada, parametros)
                .then((peticion) => {
                    this.InicializarRestrictoresDeFiltradoSegunId(peticion.resultado.datos.total == 1 ? peticion.resultado.datos.registros[0] : undefined);
                    this.DatosDelGrid.InicializarCache();
                    this.CrearFilaDelIdEnElGrid(peticion);
                })
                .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));
        }


        private TrasRestaurar(): void {
            if (this.Estado.Obtener(ltrClaveDeEstado.EditarAlVolver)) {
                this.Estado.Quitar(ltrClaveDeEstado.EditarAlVolver);
                this.Estado.Guardar();
                this.IraEditar();
            }
        }

        public PosicionarPanelesDelCuerpo(): void {
            if (this.ModoTrabajo === enumModoTrabajo.mantenimiento) {
                //this.PosicionarFiltro();
                //this.PosicionarGrid();
            }
        }

        public PosicionarFiltro(): void {
            this.PanelFiltro.style.position = 'fixed';
            let posicionFiltro: number = this.PosicionFiltro();
            this.PanelFiltro.style.top = `${posicionFiltro}px`;

            let bloques = this.PanelFiltro.getElementsByClassName('cuerpo-datos-filtro-bloque') as HTMLCollectionOf<HTMLDivElement>;
            let alturaDeBloques = 0;
            for (let i = 0; i < bloques.length; i++) {
                alturaDeBloques = alturaDeBloques + bloques[i].getBoundingClientRect().height;
            }
            let alturaCalculada: number = AlturaFormulario() * 20 / 100;
            this.PanelFiltro.style.height = alturaDeBloques < alturaCalculada
                ? `${alturaDeBloques}px`
                : `${alturaCalculada}px`;
        }

        private PosicionFiltro(): number {
            let alturaCabeceraPnlControl: number = AlturaCabeceraPnlControl();
            let alturaCabeceraMnt: number = this.CuerpoCabecera.getBoundingClientRect().height;
            return alturaCabeceraPnlControl + alturaCabeceraMnt;
        }


        public DespuesDeAplicarUnRestrictor(restrictor: Tipos.Restrictor): void {
            //método para sobrecargar que analiza el tipo de restrictor aplicado y obtirnr información de si se ha de restringir o mapear más información
            //P.eje: Al mapear una provincia al filtro de municipios, mapear el país
        }

        private InicializarSelectores(div: HTMLDivElement) {
            let selectores: NodeListOf<HTMLSelector> = div.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFiltro}"]`) as NodeListOf<HTMLSelector>;
            selectores.forEach((selector) => {
                let idModal: string = selector.getAttribute(atSelectorDeFiltro.idModal);
                let modal: ModalSeleccion = new ModalSeleccion(idModal);
                modal.InicializarModalDeSeleccion();
                this.ModalesDeSeleccion.push(modal);
            });
        }

        private DespuesDeLeerDatosParaInicializarVista(peticion: ApiDeAjax.DescriptorAjax) {
            let mantenimiento: CrudMnt = peticion.llamador as CrudMnt;
            let modoDeAccesoAlNegocio = ModoAcceso.Parsear(peticion.resultado.modoDeAcceso);
            mantenimiento.ModoAccesoAlNegocio = modoDeAccesoAlNegocio;
            let estadosDelLosExpansores: Array<EstadoDeEspan> = ObtenerPropiedad(peticion.resultado.datos, ltrMantenimiento.Espanes, new Array<EstadoDeEspan>());
            if (estadosDelLosExpansores.length > 0) {
                this.AplicarExpansores(estadosDelLosExpansores);
            }
            if (Definido(this.ZonaDeMenu))
                ModoAcceso.AplicarModoDeAccesoALasOpcionesDelNegocio(mantenimiento.OpcionesGenerales, modoDeAccesoAlNegocio);
            if (Definido(this.ContenedorMenuContextual))
                ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuContextual, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeVista, modoDeAccesoAlNegocio);
            if (Definido(this.ContenedorMenuDeFiltro))
                ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuDeFiltro, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeVista, modoDeAccesoAlNegocio);

            let plantillasDeFiltrado: Array<PlantillaDeFiltrado> = ObtenerPropiedad(peticion.resultado.datos, ltrMantenimiento.Filtros, new Array<PlantillaDeFiltrado>());
            for (let i: number = 0; Definido(plantillasDeFiltrado) && i < plantillasDeFiltrado.length; i++) {
                ApiDeFiltro.IncluirPlantillaDeFiltrado(plantillasDeFiltrado[i]);
            }

            this._mapIndicadores = ObtenerPropiedad(peticion.resultado.datos, ltrMantenimiento.Indicadores, new Map<string, any>()) as Map<string, any>;
            //let mapIndicadores = new Map<string, any>(Object.entries(indicadores));
            if (this._mapIndicadores.size > 0) {
                this.AplicarIndicadores(this._mapIndicadores);
            }
            mantenimiento.DespuesDeInicializarCrud(modoDeAccesoAlNegocio);
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {

            this._capaConVinculados = mapIndicadores.get(ltrPropiedades.Entorno.Vista.Indicadores.CapaConVinculados);
            this._siempreEnConsulta = mapIndicadores.get(ltrPropiedades.Entorno.Vista.Indicadores.SiempreEnConsulta);
            this.TamanoDelVisor = mapIndicadores.get(ltrPropiedades.Entorno.Vista.Indicadores.TamanoDelVisor);
            this._iaUsada = mapIndicadores.get(ltrPropiedades.Entorno.Vista.Indicadores.IaUsada);
            this._usaTotalizador = mapIndicadores.get(ltrPropiedades.Entorno.Vista.Indicadores.UsaTotalizador);
        }

        protected AplicarExpansores(estadosDelLosExpansores: Array<EstadoDeEspan>): void {
            let idEspanDelDto = ObtenerPropiedad(estadosDelLosExpansores[0], ltrEdicion.IdDelEspan);
            let espanDelDto = document.getElementById(this.crudDeEdicion.PanelDeEditar.id + '-dp') as HTMLDivElement;
            if (idEspanDelDto === espanDelDto.id) {
                ApiDelCrud.ExpandirContraer(espanDelDto, estadosDelLosExpansores[0]);
            }

            let detalles: NodeListOf<HTMLDivElement> = this.crudDeEdicion.PanelDeEditar.querySelectorAll(`.${ltrCss.Detalle.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < detalles.length; i++) {
                ApiDelCrud.AnalizarSiExpandirContraer(detalles[i], estadosDelLosExpansores);
            }

            let ampliaciones: NodeListOf<HTMLDivElement> = this.crudDeEdicion.PanelDeEditar.querySelectorAll(`.${ltrCss.Ampliacion.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < ampliaciones.length; i++) {
                ApiDelCrud.AnalizarSiExpandirContraer(ampliaciones[i], estadosDelLosExpansores);
            }

            let bloques: NodeListOf<HTMLDivElement> = this.crudDeEdicion.PanelDeEditar.querySelectorAll(`.${ltrCss.Bloque.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < bloques.length; i++) {
                ApiDelCrud.AnalizarSiExpandirContraer(bloques[i], estadosDelLosExpansores);
            }
        }

        protected ErrorAlLeerModoAccesoAlNegocio(peticion: ApiDeAjax.DescriptorAjax): void {
            try {
                ApiDeAjax.ErrorTrasPeticion("Leer modo de acceso al negocio", peticion);
            }
            finally {
                this._errorAlLeerDatosParaInicializarVista = true;
                this._siempreEnConsulta = true;
                ApiControl.BloquearMenu(this.Cuerpo);
            }
        }


        protected iniciarArrastre(event: MouseEvent, columna: HTMLDivElement) {
            event.preventDefault();
            event.stopPropagation();

            if (!columna.classList.contains(ltrCss.crud.columna)) return;

            // console.log('comienzo arrastre ' + columna.id);
            this.columnaQueEstoyArrastrando = columna;
            this.posicionInicialX = event.clientX;
            this.columnaQueEstoyArrastrando.classList.add(ltrCss.crud.arrastrando);
        }

        protected arrastrar(event: MouseEvent) {
            event.preventDefault();
            event.stopPropagation();

            if (Definido(this.columnaQueEstoyArrastrando)) {
                this.desplazamientoX = event.clientX - this.posicionInicialX;
            }
            else if (Definido(this.tituloQueEstoyMoviendo)) {
                this.MoviendoColumna(event);
            }
        }

        protected detenerArrastre(event: MouseEvent) {
            event.preventDefault();
            event.stopPropagation();

            if (!Definido(this.columnaQueEstoyArrastrando)) return;
            if (!this.columnaQueEstoyArrastrando.classList.contains(ltrCss.crud.columna)) return;

            //console.log('termino arrastre ' + this.columnaQueEstoyArrastrando.id + ', incremeto: ' + this.desplazamientoX);
            try {
                ApiDeGrid.ModificarTamano(this.Tabla, this.columnaQueEstoyArrastrando, this.desplazamientoX);
            }
            finally {
                this.LimpiarEstadoArrastre(event);
                this.GuardarTamanoDeColumnas();
            }
        }


        protected IniciarMoverColumna(event: MouseEvent, titulo: HTMLDivElement) {
            event.preventDefault();
            event.stopPropagation();
            this.tituloQueEstoyMoviendo = titulo;

            // Crear el tooltip si no existe
            this.toolTipMoviendose = document.getElementById('columnaMoviendose');
            if (!this.toolTipMoviendose) {
                this.toolTipMoviendose = document.createElement('div');
                this.toolTipMoviendose.id = 'columnaMoviendose';
                this.toolTipMoviendose.style.position = 'absolute';
                this.toolTipMoviendose.style.display = 'none';
                this.toolTipMoviendose.style.backgroundColor = '#fff';
                this.toolTipMoviendose.style.border = '1px solid #ccc';
                this.toolTipMoviendose.style.padding = '5px';
                this.toolTipMoviendose.style.zIndex = '1000';
                document.body.appendChild(this.toolTipMoviendose);
            }

            // Configurar el contenido del tooltip
            this.toolTipMoviendose.innerText = titulo.title || titulo.innerText; // Usa el title o el texto del elemento
            this.toolTipMoviendose.style.display = 'block'; // Mostrar el tooltip
        }

        protected MoviendoColumna(event: MouseEvent) {

            if (this.moviendomeEncimaDe) this.moviendomeEncimaDe.classList.remove(ltrCss.crud.tituloResaltado);
            if (!this.tituloQueEstoyMoviendo)
                return;

            // Actualizar la posición del tooltip

            if (this.toolTipMoviendose) {
                this.toolTipMoviendose.style.left = `${event.pageX + 10}px`; // Ajusta la posición del tooltip
                this.toolTipMoviendose.style.top = `${event.pageY + 10}px`; // Ajusta la posición del tooltip
            }

            const controlSobreElQueSeArrastra = event.currentTarget;
            let tituloSobreElQueMeMuevo: HTMLDivElement | undefined;

            if (controlSobreElQueSeArrastra instanceof HTMLAnchorElement) {
                tituloSobreElQueMeMuevo = (controlSobreElQueSeArrastra as HTMLAnchorElement).parentElement as HTMLDivElement;
            } else if (controlSobreElQueSeArrastra instanceof HTMLDivElement) {
                tituloSobreElQueMeMuevo = (controlSobreElQueSeArrastra as HTMLDivElement);
            }

            if (tituloSobreElQueMeMuevo.classList.contains(ltrCss.crud.tituloColumna) && tituloSobreElQueMeMuevo.id != this.tituloQueEstoyMoviendo.id) {
                this.moviendomeEncimaDe = tituloSobreElQueMeMuevo;
                tituloSobreElQueMeMuevo.classList.add(ltrCss.crud.tituloResaltado);
            }
        }


        protected PosicionarColumna(event: MouseEvent) {
            event.preventDefault();
            event.stopPropagation();


            try {
                if (!Definido(this.moviendomeEncimaDe))
                    return;

                const controlSobreElQueSeSuelta = event.currentTarget;
                let tituloSobreElQueSeSuelta: HTMLDivElement | undefined;
                if (controlSobreElQueSeSuelta instanceof HTMLAnchorElement) {
                    tituloSobreElQueSeSuelta = (controlSobreElQueSeSuelta as HTMLAnchorElement).parentElement as HTMLDivElement;
                } else if (controlSobreElQueSeSuelta instanceof HTMLDivElement) {
                    tituloSobreElQueSeSuelta = (controlSobreElQueSeSuelta as HTMLDivElement);
                }

                if (Definido(this.moviendomeEncimaDe)) this.moviendomeEncimaDe.classList.remove(ltrCss.crud.tituloResaltado);
                if (Definido(tituloSobreElQueSeSuelta) && tituloSobreElQueSeSuelta.classList.contains(ltrCss.crud.tituloColumna)) {
                    this.PosicionarLaColumna(this.tituloQueEstoyMoviendo.id, tituloSobreElQueSeSuelta.id);
                    this.GuardarSituacionDeColumnas();
                }
            } finally {
                this.LimpiarEstadoArrastre(event);
            }
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            try {

                var cabecera = this.Tabla.querySelector<HTMLDivElement>('.' + ltrCss.crud.thead);
                const columnas = cabecera.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.columna);

                for (let i = 1; i < columnas.length; i++) {
                    const columna = columnas[i];
                    columna.addEventListener('mousedown', (event) => this.iniciarArrastre(event, columna));
                    columna.addEventListener('mousemove', (event) => this.arrastrar(event));
                    columna.addEventListener('mouseup', (event) => this.detenerArrastre(event));
                }

                const titulos = cabecera.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.tituloColumna);
                for (let i = 1; i < titulos.length; i++) {
                    const titulo = titulos[i];
                    titulo.addEventListener('mousedown', (event) => this.IniciarMoverColumna(event, titulo));
                    titulo.addEventListener('mousemove', (event) => this.arrastrar(event));
                    titulo.addEventListener('mouseup', (event) => this.PosicionarColumna(event));
                }
                // Agregar evento mouseleave a la cabecera
                cabecera.addEventListener('mouseleave', (event) => this.LimpiarEstadoArrastre(event));

                this.Estado.ExtraerParametrosDeLaUrl();
                if (this.VengoDeUnPost) {
                    MapearAlCrud.FiltrosPasadosEnEstado(this);
                    MapearAlCrud.RestrictoresPasadosEnEstado(this);
                }
                MapearAlCrud.FiltrosDeLaUrl(this);
                MapearAlCrud.RestrictoresDeLaUrl(this);
                this.IndicarSiHayFiltrosEnAlgunaModal();
                let idEnLaUrl = ObtenerParametroUrl(ltrParametrosUrl.id, 0, false);
                if (this.Navegador.EsRestauracion) {
                    if (this.Grid.getAttribute(atGrid.cargando) != 'S') {
                        this.RestaurarPagina();
                    }
                }
                else {
                    if (idEnLaUrl > 0) {
                        ApiControl.OcultarOpcionDeMenuPorNombre(this.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                        ApiControl.OcultarOpcionDeMenuPorNombre(this.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                        this.EditarRegistro();
                    }
                    else {
                        if (this.HayQueIrACrear) this.IraCrear();
                        else {
                            const parametros = ObtenerParametrosDeLaUrl()
                            for (let i = 0; i < parametros.length; i++) {
                                const control = ApiControl.BuscarControl(this.PanelFiltro, parametros[i].clave, false);
                                if (control && control instanceof HTMLSelectElement) {
                                    MapearAlControl.ListaDeValores(control, parametros[i].valor)
                                }
                            }
                            this.CargarGrid();
                        }
                    }
                }
            }
            finally {
                this.Estado.Guardar();
                ApiDeArchivos.ConfigurarDragAndDrop();
                this.OcultarMostrarFiltro();
            }
        }

        public ObtenerGrid(idModal: string): GridDeDatos {

            if (this.modoTrabajo === enumModoTrabajo.historial || (this.HayHistorial && this.EstoyEditandoConsultando))
                return this.crudHistorial;

            if (IsNullOrEmpty(idModal))
                return this;

            let grid: GridDeDatos = this.ModalDeSeleccionAsociada(idModal);
            if (Definido(grid))
                return grid;

            grid = this.ModalParaSeleccionAsociada(idModal);
            if (Definido(grid))
                return grid;

            grid = this.ModalParaConsultarRelacionesAsociada(idModal);
            if (Definido(grid))
                return grid;

            grid = this.ModalParaRelacionesAsociada(idModal);
            if (Definido(grid))
                return grid;

            grid = this.ModalParaImputarAsociada(idModal);
            if (Definido(grid))
                return grid;

            MensajesSe.EmitirMensajeDeExcepcion("ObtenerGrid", `Se busca la modal con id ${idModal} y no se ha encontrado`);
        }

        private ModalParaImputarAsociada(idModal: string): ModalParaImputar {
            for (let i: number = 0; i < this.ModalesParaImputar.length; i++) {
                let modal: ModalParaImputar = this.ModalesParaImputar[i];
                if (modal.IdModal === idModal)
                    return modal as ModalParaImputar;
            }
            return undefined;
        }

        private ModalParaRelacionesAsociada(idModal: string): ModalParaRelacionar {
            for (let i: number = 0; i < this.ModalesParaRelacionar.length; i++) {
                let modal: ModalParaRelacionar = this.ModalesParaRelacionar[i];
                if (modal.IdModal === idModal)
                    return modal as ModalParaRelacionar;
            }
            return undefined;
        }

        private ModalParaConsultarRelacionesAsociada(idModal: string): ModalParaConsultarRelaciones {
            for (let i: number = 0; i < this.ModalesParaConsultarRelaciones.length; i++) {
                let modal: ModalParaConsultarRelaciones = this.ModalesParaConsultarRelaciones[i];
                if (modal.IdModal === idModal)
                    return modal as ModalParaConsultarRelaciones;
            }
            return undefined;
        }

        private ModalParaSeleccionAsociada(idModal: string): ModalParaSeleccionar {
            for (let i: number = 0; i < this.ModalesParaSeleccionar.length; i++) {
                let modal: ModalParaSeleccionar = this.ModalesParaSeleccionar[i];
                if (modal.IdModal === idModal)
                    return modal as ModalParaSeleccionar;
            }
            return undefined;
        }

        private ModalDeSeleccionAsociada(idModal: string): ModalSeleccion {
            for (let i: number = 0; i < this.ModalesDeSeleccion.length; i++) {
                let modal: ModalSeleccion = this.ModalesDeSeleccion[i];
                if (modal.IdModal === idModal)
                    return modal as ModalSeleccion;
            }
            return undefined;
        }

        public ObtenerModalDeSeleccion(idModal: string): ModalSeleccion {
            for (let i: number = 0; i < this.ModalesDeSeleccion.length; i++) {
                let modal: ModalSeleccion = this.ModalesDeSeleccion[i];
                if (modal.IdModal === idModal)
                    return modal;
            }
            return undefined;
        }

        public ObtenerModalParaRelacionar(idModal: string): ModalParaRelacionar {
            for (let i: number = 0; i < this.ModalesParaRelacionar.length; i++) {
                let modal: ModalParaRelacionar = this.ModalesParaRelacionar[i];
                if (modal.IdModal === idModal)
                    return modal;
            }

            let modal: ModalParaRelacionar = new ModalParaRelacionar(this, idModal);
            this.ModalesParaRelacionar.push(modal);
            return modal;
        }

        public ObtenerModalParaImputar(idModal: string): ModalParaImputar {
            for (let i: number = 0; i < this.ModalesParaImputar.length; i++) {
                let modal: ModalParaImputar = this.ModalesParaImputar[i];
                if (modal.IdModal === idModal)
                    return modal;
            }

            let modal: ModalParaImputar = new ModalParaImputar(this, idModal);
            this.ModalesParaImputar.push(modal);
            return modal;
        }
        public ObtenerModalParaConsultarRelaciones(idModal: string): ModalParaConsultarRelaciones {
            for (let i: number = 0; i < this.ModalesParaConsultarRelaciones.length; i++) {
                let modal: ModalParaConsultarRelaciones = this.ModalesParaConsultarRelaciones[i];
                if (modal.IdModal === idModal)
                    return modal;
            }

            let modal: ModalParaConsultarRelaciones = new ModalParaConsultarRelaciones(this, idModal);
            this.ModalesParaConsultarRelaciones.push(modal);
            return modal;
        }

        public AbrirModalParaRelacionar(idModalParaRelacionar: string) {
            let modal: ModalParaRelacionar = this.ObtenerModalParaRelacionar(idModalParaRelacionar);
            if (modal === undefined)
                throw new Error(`Modal ${idModalParaRelacionar} no definida`);

            modal.AbrirModalDeRelacion();
        }

        public AbrirModalParaImputar(idModalParaImputar: string) {
            let modal: ModalParaImputar = this.ObtenerModalParaImputar(idModalParaImputar);
            if (modal === undefined)
                throw new Error(`Modal ${idModalParaImputar} no definida`);

            modal.AbrirModalParaImputar();
        }
        public AbrirModalParaConsultarRelaciones(idModalParaConsultar: string) {

            if (this.InfoSelector.Cantidad != 1)
                throw new Error("Debe seleccionar un elemento para poder consultar las relaciones");


            let modal: ModalParaConsultarRelaciones = this.ObtenerModalParaConsultarRelaciones(idModalParaConsultar);
            if (modal === undefined)
                throw new Error(`Modal ${idModalParaConsultar} no definida`);

            modal.AbrirModalParaConsultarRelaciones(this.InfoSelector.LeerElemento(0));
        }

        public ModalDeBorrado_Abrir() {
            if (this.InfoSelector.Cantidad == 0)
                throw new Error(`Debe seleccionar el elemento a borrar, ha seleccionado ${this.InfoSelector.Cantidad}`);

            this.modalDeBorrardo_Abrir();
        }

        private modalDeBorrardo_Abrir(): void {
            if (this.SoloConGrid) return;
            this.ModoTrabajo = enumModoTrabajo.borrando;
            //this.ModalDeBorrado.style.display = ltrStyle.display.block;
            //EntornoSe.AjustarModalesAbiertas();
            ApiPanel.AbrirModal(this.ModalDeBorrado);
            let mensaje: HTMLInputElement = document.getElementById(`${this._idModalBorrar}-mensaje`) as HTMLInputElement;
            if (this.InfoSelector.Cantidad > 1) {
                mensaje.value = "Seguro desea borrar todos los elementos seleccionados";
            }
            else {
                mensaje.value = "Seguro desea borrar el elemento seleccionado";
            }
        }

        public ModalDeBorrado_Borrar() {
            ApiDePeticiones.BorrarPorIds(this, this.Navegador.Controlador, this.InfoSelector.IdsSeleccionados, this.ParametrosOpcionalesBorrar())
                .then((peticion) => this.DespuesDeBorrar(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected ParametrosOpcionalesBorrar(): Array<Parametro> {
            return new Array<Parametro>();
        }

        protected DespuesDeBorrar(peticion: ApiDeAjax.DescriptorAjax) {
            let mantenimiento: CrudMnt = peticion.llamador as CrudMnt;
            mantenimiento.ModoTrabajo = enumModoTrabajo.mantenimiento;
            mantenimiento.ModalDeBorrado_Cerrar();
            mantenimiento.InfoSelector.QuitarTodos();
            mantenimiento.CargarGrid();
        }


        public ModalDeBorrado_Cerrar() {
            this.ModoTrabajo = enumModoTrabajo.mantenimiento;
            ApiPanel.CerrarModal(this.ModalDeBorrado);
        }

        public IraEditar() {
            if (this.SoloConGrid) return;
            ApiDeMenuFlotante.CerrarMf(this.ContenedorDeLosMenusDelCrud);
            if (this.InfoSelector.Cantidad == 0) {
                MensajesSe.Error("IraEditar", "Debe marcar el elemento a editar");
                return;
            }
            this.crudDeEdicion.ComenzarEdicion(this.InfoSelector);
        }

        public IraHistorial(grabar: boolean = true) {
            if (this.SoloConGrid) return;
            ApiDeMenuFlotante.CerrarMf(this.ContenedorDeLosMenusDelCrud);
            if (this.InfoSelector.Cantidad == 0) {
                MensajesSe.Error("IraHistorial", "Debe marcar el proceso del que ver el historial");
                return;
            }

            if (this.EstoyEditando && this.crudDeEdicion.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.IraHistorial(false)));
                this.crudDeEdicion.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }

            if (Registro.EsMovil() && this.EstoyEditandoConsultando) {
                this.crudHistorial.MostrarHistorial(this.crudDeEdicion.ElementoEditado);
                return;
            }

            const divDelVisor = this.crudDeEdicion.VisorDelHistorial;
            const divDelCuerpoDelHistorial = this.crudHistorial.ContenedorDelCuerpo;
            const divOpcionesHistorial = this.crudHistorial.ContenedorDelPie;


            if (this.EstoyEditandoConsultando && Definido(divDelVisor)) {

                if (!this.crudDeEdicion.VisorVisible) {
                    this.crudDeEdicion.MostrarOcultarVisor();
                }

                this.crudDeEdicion.MostrarOcultarHistorial();

                if (divDelCuerpoDelHistorial.parentElement !== divDelVisor) {
                    divDelVisor.appendChild(divDelCuerpoDelHistorial);
                    this.crudDeEdicion.MenuDelHistorial.appendChild(divOpcionesHistorial);
                    this.crudDeEdicion.PanelDeEditar.appendChild(this.crudHistorial.ModalDeSucesos);
                }

                this.crudHistorial.MostrarHistorialEnEdicion(this.crudDeEdicion.ElementoEditado);
            }
            else {
                if (this.crudHistorial.EstaEnEdicion) {
                    this.crudHistorial.ContenedorDeLaCabecera.after(divDelCuerpoDelHistorial);
                    divDelCuerpoDelHistorial.after(divOpcionesHistorial);
                    divOpcionesHistorial.after(this.crudHistorial.ModalDeSucesos);
                }
                this.crudHistorial.MostrarHistorial(this.InfoSelector.Seleccionados[0]);
            }
        }

        public CerrarModalDeEdicion() {
            this.crudDeEdicion.EjecutarAcciones(ltrEventos.Edicion.Cerrar, null);
        }

        public ModificarElemento(idModal: string) {
            let modal: HTMLDivElement = Definido(idModal) ? document.getElementById(idModal) as HTMLDivElement : null;
            this.crudDeEdicion.EjecutarAcciones(ltrEventos.ModalEdicion.Modificar, modal);
        }

        public IraCrear() {
            if (this.SoloConGrid) return;
            this.crudDeCreacion.ComenzarCreacion();
        }

        public CrearElemento() {
            this.crudDeCreacion.EjecutarAcciones(ltrEventos.Creacion.Crear);
        }

        public CerrarModalDeCreacion() {
            this.crudDeCreacion.EjecutarAcciones(ltrEventos.Creacion.Cerrar);
        }

        public CerrarModalDeFiltro(modal: HTMLDivElement): void {
            ApiPanel.OcultarModal(modal);
            this.IndicaSiHayFiltros(modal);
        }


        public ModalDeTotales_TrasAbrir(modal: HTMLDivElement): void {

            var encolumnado = ApiDeGrid.Encolumnado(this.FilaCabecara);
            var html = ApiDeGrid.CrearHTMLParaOcultar(encolumnado);
            var div = document.getElementById(`contenedor_creacion_cuerpo_${this.IdCuerpoCabecera.replace('_mantenimiento', '')}-ocultar-columnas`);
            div.innerHTML = html;
        }

        public ModalDeTotales_Cerrar(modal: HTMLDivElement): void {
            ApiPanel.OcultarModal(modal);
        }


        public ModalDeOcultarColumnas_TrasAbrir(modal: HTMLDivElement): void {

            var encolumnado = ApiDeGrid.Encolumnado(this.FilaCabecara);
            var html = ApiDeGrid.CrearHTMLParaOcultar(encolumnado);
            var div = document.getElementById(`contenedor_creacion_cuerpo_${this.IdCuerpoCabecera.replace('_mantenimiento', '')}-ocultar-columnas`);
            div.innerHTML = html;
        }

        public ModalDeOcultarColumnas_Aceptar(modal: HTMLDivElement): void {
            this.ModalDeOcultarColumnas_GuardarColumnasDelGrid(modal);
        }


        protected ModalDeOcultarColumnas_GuardarColumnasDelGrid(modal: HTMLDivElement): void {
            if (this.IdNegocio == 0 || EsDispositvoMovil())
                return null;

            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(Ajax.Modal.parametro, modal));
            let datosParaGuardar = ApiDeGrid.VisibilidadDeColumnas(modal);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));

            ApiDePeticiones.ProcesarPeticion(this, this.Controlador, this.IdNegocio, ltrMenus.eventosDeMf.Comun.GuardarColumnasDelGrid, parametros, datosDeEntrada)
                .then((peticion) => {
                    this.DespuesDeGuardarColumnasDelGrid(peticion);
                })
                .catch((peticion) => {
                    ApiDePeticiones.EmitirError(peticion);
                });
        }

        private DespuesDeGuardarColumnasDelGrid(peticion: ApiDeAjax.DescriptorAjax) {
            var modal = ObtenerPropiedad(peticion.DatosDeEntrada, Ajax.Modal.parametro);
            try {
                ApiPanel.OcultarModal(modal);
                ApiDeGrid.AplicarVisibilidad(crudMnt.Tabla, modal);
            }
            finally {
                crudMnt.Tabla.setAttribute(atGrid.RecalcularPorcentajes, literal.true)
                crudMnt.AplicarTamanosAlEncolumnado();
                crudMnt.FilaCabecara = undefined;
            }
        }

        public ModalDeOcultarColumnas_Cerrar(modal: HTMLDivElement): void {
            ApiPanel.OcultarModal(modal);
            this.ModalDeOcultarColumnas_Resetear();
        }

        public ModalDeOcultarColumnas_Resetear(): void {
            if (this.IdNegocio == 0)
                return null;

            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();

            ApiDePeticiones.ProcesarPeticion(this, this.Controlador, this.IdNegocio, ltrMenus.eventosDeMf.Comun.EliminarColumnasDelGrid, parametros, datosDeEntrada)
                .then((peticion) => {
                    PonerCapa();
                    window.location.reload();
                })
                .catch((peticion) => {
                    QuitarCapa();
                    ApiDePeticiones.EmitirError(peticion);
                });
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            var tipoctrl = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Elemento.ConTipo.Tipo)
            if (Definido(tipoctrl)) ApiDelCrud.Neg_Tras_Blanquear_CtrlTipo(tipoctrl);
            if (modal.id == (this.ModalEnviarElemento?.id ?? '')) {
                var restrictor = ApiControl.BuscarRestrictor(modal, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeEdicion);
                MapearAlControl.Restrictor(restrictor, this.InfoSelector.IdsSeleccionados[0], this.InfoSelector.TextosSeleccionados[0]);

                var asunto = ApiControl.BuscarEditor(modal, ltrPropiedades.Entorno.EnviarElemento.Asunto);
                if (IsNullOrEmpty(asunto.value)) asunto.value = "Envío el elemento seleccionado";

                var cuerpo = ApiControl.BuscarAreaDeTexto(modal, ltrPropiedades.Entorno.EnviarElemento.Cuerpo);
                if (IsNullOrEmpty(cuerpo.value)) cuerpo.value = "Le envío el elemento seleccionado, haga click en el enlace para acceder a él";

            }
        }

        public ModalDePedirDatos_Cerrar(modal: HTMLDivElement): void {
            ApiPanel.OcultarModal(modal);
        }

        public ModalDeMensaje_Cerrar(modal: HTMLDivElement): void {
            ApiPanel.OcultarModal(modal);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement): void {
            if (this.modoTrabajo === enumModoTrabajo.editando)
                this.crudDeEdicion.Recargar();
            else if (modal.id === (this.ModalEnviarElemento?.id ?? '')) {
                let parametros: Array<Parametro> = this.ModalEnviarElemento_LeerParametros(modal);
                ApiDePeticiones.EnviarCorreo(this, this.Controlador, parametros)
                    .then((peticion) => {
                        MensajesSe.Info(peticion.resultado.mensaje);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (this.modoTrabajo === enumModoTrabajo.mantenimiento) {
                if (modal.id === this.ModalParaCrearFiltro.id)
                    this.GuardarPlantillaFiltrado();
                else if (modal.id === this.ModalParaEliminarFiltro.id)
                    this.EliminarPlantillaFiltrado();
                else
                    this.CargarGrid();
            }
            else if (this.modoTrabajo == enumModoTrabajo.creando) {
                if (modal.id === this.crudDeCreacion.ModalParaCrearPlantilla.id)
                    this.crudDeCreacion.GuardarPlantillaCreacion();
                else if (modal.id === this.crudDeCreacion.ModalParaEliminarPlantilla.id)
                    this.crudDeCreacion.EliminarPlantillaCreacion();
            }

            this.ModalDePedirDatos_Cerrar(modal);
        }


        private ModalEnviarElemento_LeerParametros(modal: HTMLDivElement): Array<Parametro> {
            let parametros: Array<Parametro> = new Array<Parametro>();

            let receptor: string = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Entorno.EnviarElemento.Usuario).getAttribute(atListasDinamicas.idSeleccionado);
            let usuarios: Array<string> = ToLista(receptor, ',');

            let asunto: string = ApiControl.BuscarEditor(modal, ltrPropiedades.Entorno.EnviarElemento.Asunto).value;
            let cuerpo: string = ApiControl.BuscarAreaDeTexto(modal, ltrPropiedades.Entorno.EnviarElemento.Cuerpo).value;

            let adjuntos: Array<Tipos.ElementoDtoSimple> = [];
            let adjunto = new Tipos.ElementoDtoSimple(this.Dto, this.InfoSelector.IdsSeleccionados[0], this.InfoSelector.TextosSeleccionados[0]);
            adjuntos.push(adjunto);

            parametros.push(new Parametro(Ajax.Param.nombreDeNegocio, this.NombreDeNegocio));
            parametros.push(new Parametro('usuarios', usuarios));
            parametros.push(new Parametro('puestos', ''));
            parametros.push(new Parametro('asunto', asunto));
            parametros.push(new Parametro('cuerpo', cuerpo));
            parametros.push(new Parametro('adjuntos', adjuntos));
            parametros.push(new Parametro('otorgarGestor', ApiControl.BuscarCheck(modal, ltrPropiedades.Entorno.EnviarElemento.OtorgarGestor).checked));
            return parametros;
        }
        private InicializarRestrictoresDeFiltradoSegunId(objeto: any): void {
            if (!Definido(objeto)) return;
            let restrictores: NodeListOf<HTMLInputElement> = this.PanelFiltro.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeFiltro}"]`) as NodeListOf<HTMLInputElement>;
            for (let i = 0; i < restrictores.length; i++) {
                let idPropiedad = restrictores[i].getAttribute(atControl.propiedad);
                let valor = ObtenerPropiedad(objeto, idPropiedad, 0);
                let texto = ObtenerPropiedad(objeto, idPropiedad.slice(2), "");
                let restrictor = MapearAlFiltro.MapearRestrictor(this.PanelFiltro, idPropiedad.toLowerCase(), valor, texto, false);
                MapearAlDiccionario.UnRestrictor(restrictor as HTMLInputElement, this.Estado);
            }
        }

        private IndicaSiHayFiltros(modal: HTMLDivElement) {
            let referenciaDeFiltrado = modal.getAttribute(atModal.referenciaDeFiltrado);
            let referencia = document.getElementById(referenciaDeFiltrado) as HTMLAnchorElement;
            if (!Definido(referencia))
                return;
            if (this.HayFiltros(modal))
                referencia.classList.add(ltrCss.modalesDeFiltro.conFiltro);
            else
                referencia.classList.remove(ltrCss.modalesDeFiltro.conFiltro);
        }

        public HayFiltros(modal: HTMLDivElement): boolean {
            if (ApiPanel.HayEditoresConValor(modal, ltrTipoControl.Editor)) return true;
            if (ApiPanel.HayEditoresConValor(modal, ltrTipoControl.ConEditor)) return true;
            if (ApiPanel.HayRestrictoresDeFiltroConValor(modal)) return true;
            if (ApiPanel.HayListasDinamicasConValor(modal)) return true;
            if (ApiPanel.HayListasDeElementosConValor(modal)) return true;
            if (ApiPanel.HayListasDeValoresConValor(modal)) return true;
            if (ApiPanel.HayFiltroEntreFechasConValor(modal)) return true;
            if (ApiPanel.HayFiltroEntreImportesConValor(modal)) return true;
            if (ApiPanel.HayFiltroEntreRangosConValor(modal)) return true;
            return false;
        }

        public RestaurarPagina(): void {
            this.Navegador.EsRestauracion = false;
            let cantidad: number = this.Navegador.Cantidad;
            let pagina: number = this.Navegador.NumeroDePaginaDelGrid;
            let posicion: number = 0;
            let accion: string = atGrid.accion.buscar;
            if (pagina > 1) {
                posicion = (pagina - 1) * cantidad;
                accion = atGrid.accion.restaurar;
            }

            let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(accion);
            let datosDeEntrada = new DatosPeticionNavegarGrid(this, accion, posicion);
            ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, accion, posicion, cantidad, datosDeEntrada, parametros)
                .then((peticion) => {
                    this.DatosDelGrid.InicializarCache();
                    this.CrearFilasEnElGrid(peticion);
                    this.TrasRestaurar();
                })
                .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));
        }

        public CambiarValorDelSelector(idSelector: string) {
            var htmlSelector: HTMLSelector = <HTMLSelector>document.getElementById(idSelector);

            let modal: ModalSeleccion = crudMnt.ObtenerModalDeSeleccion(htmlSelector.getAttribute(atSelectorDeFiltro.idModal));
            if (IsNullOrEmpty(htmlSelector.value))
                modal.InicializarModalDeSeleccion();
            else
                modal.TextoSelectorCambiado();
        }

        private ValidarRestrictorDeFiltrado(): boolean {
            let restrictoresDeFiltro: NodeListOf<HTMLInputElement> = this.PanelFiltro.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeFiltro}"]`) as NodeListOf<HTMLInputElement>;
            if (restrictoresDeFiltro.length == 0)
                return false;
            return true;
        }

        public EstaElFiltroOculto(): boolean {
            return !EsMayorDeCero(this.ExpandirFiltro.value);
        }

        public OcultarMostrarFiltro(): void {
            if (!this.EstaElFiltroOculto()) {
                this.OcultarFiltro();
            }
            else {
                this.MostrarFiltro();
            }
        }

        public MostrarFiltro(): void {
            this.ExpandirFiltro.value = "1";
            ApiPanel.MostrarPanel(this.PanelFiltro);
            this.EtiquetaMostrarOcultarFiltro.innerText = "Ocultar filtro";
            EntornoSe.AjustarDivs();
        }


        public OcultarFiltro(): void {
            ApiPanel.OcultarPanel(this.PanelFiltro);
            this.ExpandirFiltro.value = "0";
            this.EtiquetaMostrarOcultarFiltro.innerText = "Mostrar filtro";
            this.PosicionarPanelesDelCuerpo();
        }

        public OcultarMostrarBloque(idHtmlBloque: string): void {
            let extensor: HTMLInputElement = document.getElementById(`expandir.${idHtmlBloque}.input`) as HTMLInputElement;
            if (EsMayorDeCero(extensor.value)) {
                extensor.value = "0";
                ApiPanel.OcultarPanel(document.getElementById(`${idHtmlBloque}`) as HTMLDivElement);
            }
            else {
                extensor.value = "1";
                ApiPanel.MostrarPanel(document.getElementById(`${idHtmlBloque}`) as HTMLDivElement);
            }
            this.PosicionarPanelesDelCuerpo();
        }

        public OcultarMostrarExpansor(idHtmlDetalle: string): void {
            ApiDelCrud.OcultarMostrarExpansor(idHtmlDetalle);
            this.PosicionarPanelesDelCuerpo();
        }

        public ModalImprimir_Abrir(idElemento: number, plantillas: Array<PlantillaDeImpresion>): void {
            ApiPanel.BlanquearControlesDeIU(this.ModalDeImprimir, true);
            this.ModalDeImprimir.setAttribute(atControl.idElemento, idElemento.toString());
            let selector = ApiControl.BuscarListaDeValores(this.ModalDeImprimir, ltrPropiedades.SisDoc.PlantillasDisponibles.IdPlantilla);
            MapearAlControl.MapearObjetoEnListaDeValores(selector, plantillas, ltrPropiedades.SisDoc.PlantillasDisponibles.IdPlantilla,
                ltrPropiedades.SisDoc.PlantillasDisponibles.Plantilla,
                new Array<string>(ltrPropiedades.SisDoc.PlantillasDisponibles.Clase),
                false
            );
            //this.ModalDeImprimir.style.display = ltrStyle.display.block;

            ApiPanel.AbrirModal(this.ModalDeImprimir);
        }

        public ModalImprimir_Cerrar() {
            if (!this.crudDeEdicion.PanelDeEditar.classList.contains(ltrCss.divNoVisible)) {
                this.ModoTrabajo = enumModoTrabajo.editando;
            }
            else {
                this.ModoTrabajo = enumModoTrabajo.mantenimiento;
            }
            ApiPanel.CerrarModal(this.ModalDeImprimir);
        }

        public ModalImprimir_Imprimir(guardar: boolean = false): void {

            if (!guardar) {
                this.ModalImprimir_AntesDeImprimir();
                let parametros: Array<Parametro> = this.ModalImprimir_ParametrosDeImprimir();
                let datosDeEntrada = new Array<Parametro>();
                ApiDePeticiones.Imprimir(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => this.DespuesDeImprimir(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }

            this.crudDeEdicion.Modificar(ltrOperacion.ModificoParaImprimir);
        }

        DespuesDeImprimir(peticion: ApiDeAjax.DescriptorAjax): any {
            let crud = peticion.llamador as CrudMnt;

            if (crud.EstoyEditandoConsultando) {
                let funcion = crud.crudDeEdicion.IdArchivoMostrado == 0 ? (peticion) => crud.crudDeEdicion.AlTerminarDeLeerArchivos(peticion) : null;

                ApiDeArchivos.MostrarArchivosAnexados(crud.crudDeEdicion.PanelDeArchivos.id, crud.NombreDeNegocio, crud.crudDeEdicion.Id, funcion);
            }
            else {
                crud.MenuGrid_DeselecionarTodasLasFilas();
                crud.CargarGrid();
            }

            crud.ModalImprimir_Cerrar();
        }

        protected ModalImprimir_ParametrosDeImprimir(): Parametro[] {
            //idNegocio, idElemento, idPlantilla, Plantilla

            let parametros: Array<Parametro> = new Array<Parametro>();

            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, this.IdNegocio));

            if (this.EstoyEditandoConsultando)
                parametros.push(new Parametro(ltrPropiedades.Elemento.IdElemento, Numero(this.crudDeEdicion.Registro[literal.id])));
            else
                parametros.push(new Parametro(ltrPropiedades.Elemento.IdElemento, Numero(this.ModalDeImprimir.getAttribute(atControl.idElemento))));

            let plantilla: HTMLSelectElement = ApiControl.BuscarControl(this.ModalDeImprimir, ltrPropiedades.SisDoc.PlantillasDisponibles.IdPlantilla, true) as HTMLSelectElement;
            parametros.push(new Parametro(ltrPropiedades.SisDoc.PlantillasDisponibles.IdPlantilla, Numero(plantilla.selectedOptions[0].value)));
            parametros.push(new Parametro(ltrPropiedades.SisDoc.PlantillasDisponibles.Plantilla, plantilla.selectedOptions[0].label));
            parametros.push(new Parametro(ltrPropiedades.SisDoc.PlantillasDisponibles.Clase, plantilla.selectedOptions[0].getAttribute(ltrPropiedades.SisDoc.PlantillasDisponibles.Clase)));

            return parametros;
        }

        protected ModalImprimir_AntesDeImprimir() {
            let plantilla: HTMLSelectElement = ApiControl.BuscarControl(this.ModalDeImprimir, ltrPropiedades.SisDoc.PlantillasDisponibles.IdPlantilla, true) as HTMLSelectElement;
            let opcion: HTMLOptionElement = plantilla.selectedOptions[0];
            //if (opcion.index === 0)
            //    MensajesSe.EmitirExcepcion("Al solicitar imprimir", "Debe indicar que plantilla usar");
        }

        protected ModalTransitar_AntesDeTransitar() {
            let transiciones: HTMLSelectElement = ApiControl.BuscarControl(this.ModalDeTransitar, atControl.transiciones, true) as HTMLSelectElement;
            let opcion: HTMLOptionElement = transiciones.selectedOptions[0];
            if (opcion.index === 0)
                MensajesSe.EmitirExcepcion("Al solicitar transitar", "Debe indicar que transición ejecutar");

            let datos = OpcionesDeLasListas.ObtenerObjeto(transiciones); // JSON.parse(opcion.getAttribute(atControl.objeto));
            let conObservacion = ObtenerPropiedad(datos, "conObservacion", false);
            if (conObservacion) {
                let detalle: HTMLTextAreaElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Detalle, true) as HTMLTextAreaElement;
                if (IsNullOrEmpty(detalle.value))
                    MensajesSe.EmitirExcepcion("Al solicitar transitar", "Debe indicar el detalle de porque ejecuta esta transición");
            }
        }

        public ModalTransitar_Transitar(guardar: boolean = false): void {

            this.ModalTransitar_AntesDeTransitar();
            let parametros: Array<Parametro> = this.ModalTransitar_ParametrosDeTransitar();
            let datosDeEntrada = new Array<Parametro>();
            if (!guardar) {
                ApiDePeticiones.Transitar(this, this.Controlador, Ajax.EndPoint.Transitar, parametros, datosDeEntrada)
                    .then((peticion) => this.DespuesDeTransitar(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }

            //this.crudDeEdicion.Modificar(ltrOperacion.ModificoParaTransitar, parametros);
            this.crudDeEdicion.TransitarTrasModificar(parametros);
        }

        protected DespuesDeTransitar(peticion: ApiDeAjax.DescriptorAjax): any {
            let crud = peticion.llamador as CrudMnt;

            if (crud.EstoyEditandoConsultando) {
                crud.crudDeEdicion.Recargar();
                crud.ModalTransitar_Cerrar();
            }
            else {
                var check = this.ObtenerCheckDelSeleccionado(crud.InfoSelector.IdsSeleccionados[0]);
                crud.InfoSelector.Quitar(crud.InfoSelector.IdsSeleccionados[0]);
                ApiDeGrid.DesmarcarFila(check);
                if (crud.InfoSelector.IdsSeleccionados.length > 0)
                    crud.ModalTransitar_Transitar(false)
                else {
                    crud.MenuGrid_DeselecionarTodasLasFilas();
                    crud.CargarGrid();
                    crud.ModalTransitar_Cerrar();
                }
            }
        }

        protected ModalTransitar_ParametrosDeTransitar(): Parametro[] {
            let parametros: Array<Parametro> = new Array<Parametro>();

            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, this.IdNegocio));
            let idSeleccionado = this.ModoTrabajo === enumModoTrabajo.mantenimiento && this.InfoSelector.Seleccionados.length > 0
                ? this.InfoSelector.IdsSeleccionados[0]
                : Numero(this.ModalDeTransitar.getAttribute(atControl.idElemento));

            parametros.push(new Parametro(ltrPropiedades.Elemento.IdElemento, idSeleccionado));

            let origen: HTMLInputElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Origen, true) as HTMLInputElement;
            parametros.push(new Parametro(ltrPropiedades.Transiciones.Origen, Numero(origen.getAttribute(atRestrictor.idRestrictor))));

            let transiciones: HTMLSelectElement = ApiControl.BuscarControl(this.ModalDeTransitar, atControl.transiciones, true) as HTMLSelectElement;
            let opcion: HTMLOptionElement = transiciones.selectedOptions[0];
            parametros.push(new Parametro(Ajax.Param.idTransicion, Numero(opcion.value)));

            let asunto: HTMLInputElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Asunto, true) as HTMLInputElement;
            parametros.push(new Parametro(ltrPropiedades.Transiciones.Asunto, asunto.value));

            let detalle: HTMLTextAreaElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Detalle, true) as HTMLTextAreaElement;
            parametros.push(new Parametro(ltrPropiedades.Transiciones.Detalle, detalle.value));

            let usuario: HTMLInputElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Usuarios, true) as HTMLInputElement;
            parametros.push(new Parametro(ltrPropiedades.Transiciones.Usuarios, Numero(usuario.getAttribute(atListasDinamicas.idSeleccionado))));

            let archivo: HTMLInputElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Archivo, true) as HTMLInputElement;
            parametros.push(new Parametro(ltrPropiedades.Transiciones.Archivo, Numero(archivo.getAttribute(atArchivo.idArchivo))));

            return parametros;
        }

        public ModalTransitar_Seleccionar() {
            let transiciones: HTMLSelectElement = ApiControl.BuscarControl(this.ModalDeTransitar, atControl.transiciones, true) as HTMLSelectElement;
            //let opcion: HTMLOptionElement = transiciones.selectedOptions[0];

            let detalle: HTMLTextAreaElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Detalle, true) as HTMLTextAreaElement;
            detalle.value = '';

            let datos = OpcionesDeLasListas.ObtenerObjeto(transiciones);// JSON.parse(opcion.getAttribute(atControl.objeto));
            let conObservacion = ObtenerPropiedad(datos, "conObservacion", false);
            let titulo = ObtenerPropiedad(datos, "asunto", "");
            let asunto: HTMLInputElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Asunto, true) as HTMLInputElement;
            if (Definido(titulo)) {
                MapearAlControl.MapearEditor(asunto, 0, titulo, conObservacion, false);
            }
            else {
                ApiControl.BlanquearEditor(asunto, false);
            }
        }

        public ModalTransitar_Abrir(idElemento: number, idDeTransicionPorFijar: number = 0) {
            ApiPanel.BlanquearControlesDeIU(this.ModalDeTransitar, false);
            this.abriendoModalDeTransitar = true;
            let modoActual = this.ModoTrabajo;
            try {

                let estado: HTMLInputElement = ApiControl.BuscarControl(this.ModalDeTransitar, ltrPropiedades.Transiciones.Origen, true) as HTMLInputElement;
                if (this.ModoTrabajo === enumModoTrabajo.mantenimiento && this.InfoSelector.Seleccionados.length > 1) {
                    this.ModalTransitar_Transitar_AbrirEnMultiseleccion(estado);
                }
                else {
                    this.ModalTransitar_AbrirNormal(idElemento, estado);
                }
                let transiciones: HTMLSelectElement = ApiControl.BuscarControl(this.ModalDeTransitar, atControl.transiciones, true) as HTMLSelectElement;

                let filtros: Array<ClausulaDeFiltrado> = new Array<ClausulaDeFiltrado>();
                filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Transiciones.Origen, atCriterio.igual, estado.getAttribute(atControl.restrictor)));
                filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Transiciones.DelSistema, atCriterio.igual, false));

                ApiDeInicializacion.RecargarListaDeElementos(transiciones, filtros, idDeTransicionPorFijar);
                this.ModoTrabajo = enumModoTrabajo.transitando;
                ApiPanel.AbrirModal(this.ModalDeTransitar);
            }
            finally {
                this.ModoTrabajo = modoActual;
                this.abriendoModalDeTransitar = false;
            }
        }

        public ModalTransitar_AbrirNormal(idElemento: number, estado: HTMLInputElement): void {
            this.ModalDeTransitar.setAttribute(atControl.idElemento, idElemento.toString());

            if (this.VoyATransitarDesdeEdicion) {
                MapearAlControl.Restrictor(estado, this.crudDeEdicion.IdEstado, this.crudDeEdicion.EstadoActual);
            }
            else {
                MapearAlControl.Restrictor(estado, this.InfoSelector.LeerElemento(0).Registro.idEstado, this.InfoSelector.LeerElemento(0).Registro.estado);
            }

        }

        public ModalTransitar_Transitar_AbrirEnMultiseleccion(estado: HTMLInputElement): void {
            this.ModalDeTransitar.removeAttribute(atControl.idElemento);
            MapearAlControl.Restrictor(estado, this.InfoSelector.LeerElemento(0).Registro.idEstado, this.InfoSelector.LeerElemento(0).Registro.estado);
        }

        public ModalTransitar_Cerrar() {
            if (!this.crudDeEdicion.PanelDeEditar.classList.contains(ltrCss.divNoVisible)) {
                this.ModoTrabajo = enumModoTrabajo.editando;
            }
            else {
                this.ModoTrabajo = enumModoTrabajo.mantenimiento;
            }
            //this.ModalDeEnviarCorreo_DivDeElementos.innerHTML = "";
            ApiPanel.CerrarModal(this.ModalDeTransitar);
        }


        public ModalEnviarCorreo_Abrir(grabar: boolean = true) {

            if (this.EstoyEditando && this.crudDeEdicion.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.ModalEnviarCorreo_Abrir(false)));
                this.crudDeEdicion.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }

            this.ModoTrabajo = enumModoTrabajo.enviandoCorreo;

            //this.ModalDeEnviarCorreo.style.display = ltrStyle.display.block;
            //EntornoSe.AjustarModalesAbiertas();

            ApiPanel.AbrirModal(this.ModalDeEnviarCorreo);

            ApiControl.BuscarEditor(this.ModalDeEnviarCorreo, ltrPropiedades.Sometidos.Correo.Asunto).value = "";
            ApiControl.BuscarAreaDeTexto(this.ModalDeEnviarCorreo, ltrPropiedades.Sometidos.Correo.CuerpoMensaje).value = "";

            for (let i = 0; i < this.InfoSelector.Cantidad; i++)
                ApiPanel.CrearEnlaceAlElemento(this.ModalDeEnviarCorreo_DivDeElementos, this.InfoSelector.LeerElemento(i));

        }

        public ModalEnviarCorreo_Cerrar() {
            if (!this.crudDeEdicion.PanelDeEditar.classList.contains(ltrCss.divNoVisible))
                this.ModoTrabajo = enumModoTrabajo.editando;
            else
                this.ModoTrabajo = enumModoTrabajo.mantenimiento;

            ApiControl.BuscarEditor(this.ModalDeEnviarCorreo, ltrPropiedades.Sometidos.Correo.Asunto).value = "";
            ApiControl.BuscarAreaDeTexto(this.ModalDeEnviarCorreo, ltrPropiedades.Sometidos.Correo.CuerpoMensaje).value = "";

            this.ModalDeEnviarCorreo_DivDeElementos.innerHTML = "";
            ApiPanel.CerrarModal(this.ModalDeEnviarCorreo);
        }

        public ModalEnviarCorreo_Enviar(): void {
            let parametros: Array<Parametro> = this.ModalEnviarCorreo_ParametrosDeEnviarCorreos();
            ApiDePeticiones.EnviarCorreo(this, this.Controlador, parametros)
                .then((peticion) => this.DespuesDeEnviarCorreo(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public DespuesDeEnviarCorreo(peticion: ApiDeAjax.DescriptorAjax): any {
            MensajesSe.Info(peticion.resultado.mensaje);
            peticion.llamador.ModalEnviarCorreo_Cerrar();

            if ((peticion.llamador as CrudMnt).EstoyEditando)
                (peticion.llamador as CrudMnt).crudDeEdicion.RecargarGridDeTrazas();
        }


        public ModalEnviarCorreo_ParametrosDeEnviarCorreos(): Array<Parametro> {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let idPuestos: string = this.ModalDeEnviarCorreo.id + '_selector-puestos_editor';
            let idUsuarios: string = this.ModalDeEnviarCorreo.id + '_selector-usuario_editor';
            let idAsunto: string = this.ModalDeEnviarCorreo.id + '_asunto';
            let idCuerpo: string = this.ModalDeEnviarCorreo.id + '_mensaje';
            let idAjuntos: string = this.ModalDeEnviarCorreo.id + '_elementos_ref';

            let idsDeUsuarios: string = (document.getElementById(idUsuarios) as HTMLInputElement).getAttribute(atSelectorDeElementos.Seleccionados);
            let usuarios: Array<string> = ToLista(idsDeUsuarios, ',');

            let idsDePuestos: string = (document.getElementById(idPuestos) as HTMLInputElement).getAttribute(atSelectorDeElementos.Seleccionados);
            let puestos: Array<string> = ToLista(idsDePuestos, ',');

            if (puestos.length == 0 && usuarios.length == 0)
                throw new Error("Al menos debe definir un receptor");

            let asunto: string = (document.getElementById(idAsunto) as HTMLInputElement).value;
            let cuerpo: string = (document.getElementById(idCuerpo) as HTMLInputElement).value;

            if (IsNullOrEmpty(asunto))
                throw new Error("Debe indicar el asunto");

            let divAdjuntos: HTMLDivElement = (document.getElementById(idAjuntos) as HTMLDivElement);
            let adjuntos: Array<Tipos.ElementoDtoSimple> = [];
            let refAdjuntos = divAdjuntos.querySelectorAll("a");
            for (let i: number = 0; i < refAdjuntos.length; i++) {
                let adjunto = new Tipos.ElementoDtoSimple(this.Dto, Numero(refAdjuntos[i].getAttribute(atControl.idElemento)), refAdjuntos[i].text);
                adjuntos.push(adjunto);
            }


            parametros.push(new Parametro(Ajax.Param.nombreDeNegocio, this.NombreDeNegocio));
            parametros.push(new Parametro('usuarios', usuarios));
            parametros.push(new Parametro('puestos', puestos));
            parametros.push(new Parametro('asunto', asunto));
            parametros.push(new Parametro('cuerpo', cuerpo));
            parametros.push(new Parametro('adjuntos', adjuntos));
            return parametros;
        }

        public ObtenerModalParaSeleccionar(idModal: string): ModalParaSeleccionar {
            for (let i: number = 0; i < this.ModalesParaSeleccionar.length; i++) {
                let modal: ModalParaSeleccionar = this.ModalesParaSeleccionar[i];
                if (modal.IdModal === idModal)
                    return modal;
            }

            var modalAbierta = ApiDelCrud.ModalAbierta(); {
                if (modalAbierta) for (let i: number = 0; i < this.ModalesParaSeleccionar.length; i++) {
                    let modal: ModalParaSeleccionar = this.ModalesParaSeleccionar[i];
                    if (modal.IdModal === modalAbierta.id)
                        return modal;
                }

            }

            let modal: ModalParaSeleccionar = new ModalParaSeleccionar(this, idModal);
            this.ModalesParaSeleccionar.push(modal);
            return modal;
        }

        public ObtenerFocoEnSelector(idSelector: string) {
            let selector: HTMLDivElement = ApiPanel.ObtenerSelector(idSelector);
            let editor: HTMLInputElement = ApiPanel.ObtenerEditorAsociadoAlSelector(selector);
            editor.setAttribute(atSelectorDeElementos.ValorAlEntrar, editor.value);
        }

        public PerderElFocoEnUnSelectorDesdeUnaModal(idModalQueSeAbre: string, idModalQueSeCierra: string, idSelector: string) {
            let selector: HTMLDivElement = ApiPanel.ObtenerSelector(idSelector);
            let editor: HTMLInputElement = ApiPanel.ObtenerEditorAsociadoAlSelector(selector);
            let valorAlEntrar = editor.getAttribute(atSelectorDeElementos.ValorAlEntrar);
            if (editor.value === valorAlEntrar || IsNullOrEmpty(editor.value))
                return;
            selector.setAttribute(atSelectorDeElementos.CerrarAutomaticamente, 'S');
            this.AbrirModalParaSeleccionarDesdeUnaModal(idModalQueSeAbre, idModalQueSeCierra, idSelector);
        }

        public AbrirModalParaSeleccionarDesdeUnaModal(idModalQueSeAbre: string, idModalQueSeCierra: string, idSelector: string) {

            ApiPanel.OcultarModalPorId(idModalQueSeCierra);

            let modal: ModalParaSeleccionar = this.ObtenerModalParaSeleccionar(idModalQueSeAbre);
            if (NoDefinido(modal))
                throw new Error(`Modal ${idModalQueSeAbre} no definida`);

            let selector: HTMLDivElement = ApiPanel.ObtenerSelector(idSelector);

            selector.setAttribute(atSelectorDeElementos.ModalPadre, idModalQueSeCierra);

            modal.AbrirModalParaSeleccionar(selector);
        }

        public ModalExportacion_Abrir() {
            this.ModoTrabajo = enumModoTrabajo.exportando;
            ApiPanel.AbrirModal(this.ModalDeExportacion);
            if (this.IdNegocio > 0) {
                ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeExportacion, this.IdNegocio, ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_LeerDatosParaExportacion, new Array<Parametro>(), new Array<Parametro>())
                    .then((peticion) => {
                        this.DespuesDeLeerDatosParaExportacion(peticion);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else {
                var id = this.ModalDeExportacion.id + '_cuerpo_destino';
                var div = document.getElementById(id) as HTMLDivElement;
                ApiPanel.OcultarPanel(div)
            }
        }

        protected DespuesDeLeerDatosParaExportacion(peticion: ApiDeAjax.DescriptorAjax) {
            var plantillas = ApiControl.BuscarListaDeElementos(peticion.llamador.ModalDeExportacion, ltrPropiedades.Negocio.PlantillaDeExportacion.Plantilla);
            var cgs = ApiControl.BuscarListaDeElementos(peticion.llamador.ModalDeExportacion, ltrPropiedades.Negocio.PlantillaDeExportacion.IdCg);
            ApiControl.QuitarOpcionesDeLalista(plantillas);
            ApiControl.QuitarOpcionesDeLalista(cgs);
            MapearAlControl.MapearElementosEnLaLista(plantillas.id, 0, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Negocio.PlantillaDeExportacion.listaDePlantillas));
            MapearAlControl.MapearElementosEnLaLista(cgs.id, 0, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Negocio.PlantillaDeExportacion.listaDeCgs));
        }

        public ModalExportacion_Cerrar() {
            this.ModoTrabajo = enumModoTrabajo.mantenimiento;
            ApiPanel.CerrarModal(this.ModalDeExportacion);
        }

        public ModalExportacion_SalirDeListaDeCorreos(): void {
            let idCorreos: string = this.ModalDeExportacion.id + '_correos';
            let correos: HTMLInputElement = document.getElementById(idCorreos) as HTMLInputElement;
            if (!IsNullOrEmpty(correos.value)) {
                ApiControl.AnularError(correos);
                let lista = correos.value.split(';');
                let correoMalo: string = this.ValidarListaDeCorreos(lista);
                if (!IsNullOrEmpty(correoMalo)) {
                    ApiControl.MarcarError(correos);
                    throw Error(`El correo ${correoMalo} no es válido`);
                }
            }
        }

        private ValidarListaDeCorreos(lista: Array<string>): string {
            for (let i: number = 0; i < lista.length; i++) {
                if (!EsCorreoValido(lista[i].trim())) {
                    return lista[i].trim();
                }
            }
            return '';
        }

        public ModalExportacion_CheckSometerPulsado(): void {
            let idCheck: string = this.ModalDeExportacion.id + '_sometido';
            let idCorreos: string = this.ModalDeExportacion.id + '_correos';
            let check: HTMLInputElement = document.getElementById(idCheck) as HTMLInputElement;
            let correos: HTMLInputElement = document.getElementById(idCorreos) as HTMLInputElement;
            if (check.checked) {
                ApiControl.DesbloquearEditor(correos);
                correos.value = Registro.UsuarioConectado().mail;
            }
            else {
                ApiControl.BlanquearEditor(correos);
                ApiControl.BloquearInput(correos);
            }

        }

        public ModalExportacion_Exportar(): void {
            let parametros: Array<Parametro> = this.ParametrosDeExportacion();
            var plantilla = ApiControl.BuscarListaDeElementos(this.ModalDeExportacion, ltrPropiedades.Negocio.PlantillaDeExportacion.Plantilla);
            var controlador = plantilla.getAttribute(atListasDeElemento.Controlador);
            ApiDePeticiones.Exportar(this, this.Controlador, this.IdNegocio, Numero(plantilla.selectedOptions[0].value), parametros)
                .then((peticion) => this.DespuesDeExportar(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }


        public ParametrosDeExportacion(): Array<Parametro> {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let idMostradas: string = this.ModalDeExportacion.id + '_mostradas';
            let mostradas: boolean = (document.getElementById(idMostradas) as HTMLInputElement).checked;
            let posicion = 0;
            let cantidad = -1;
            if (mostradas) {
                cantidad = this.Navegador.Cantidad;
                posicion = this.Navegador.Posicion;
                posicion = posicion - cantidad;
                if (posicion < 0) posicion = 0;
            }
            parametros.push(new Parametro(Ajax.Param.nombreDeNegocio, this.NombreDeNegocio));
            parametros.push(new Parametro(Ajax.Param.posicion, posicion));
            parametros.push(new Parametro(Ajax.Param.cantidad, cantidad));
            parametros.push(new Parametro(Ajax.Param.Exportacion.Sometido, true));
            parametros.push(new Parametro(Ajax.Param.Exportacion.Receptores, ""));
            parametros.push(new Parametro(Ajax.Param.filtro, this.ObtenerFiltros(ltrOperacion.Exportar)));
            parametros.push(new Parametro(Ajax.Param.orden, this.ObtenerOrdenacion()));
            parametros.push(new Parametro(Ajax.Param.ColumnasOpcionales, JSON.stringify(this.ObtenerColumnasOpcionales())));

            //Datos para el archivador a crear
            let idArchivador: string = this.ModalDeExportacion.id + '_' + ltrPropiedades.Negocio.PlantillaDeExportacion.NombreArchivador;
            let archivador: HTMLInputElement = document.getElementById(idArchivador) as HTMLInputElement;
            let idMotivo: string = this.ModalDeExportacion.id + '_' + ltrPropiedades.Negocio.PlantillaDeExportacion.DescripcionArchivador;
            let motivo: HTMLTextAreaElement = document.getElementById(idMotivo) as HTMLTextAreaElement;
            var cgs = ApiControl.BuscarListaDeElementos(this.ModalDeExportacion, ltrPropiedades.Negocio.PlantillaDeExportacion.IdCg);
            parametros.push(new Parametro(ltrPropiedades.Elemento.ConCg.IdCg, Numero(cgs.selectedOptions[0].value)));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeExportacion.NombreArchivador, archivador.value));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeExportacion.DescripcionArchivador, motivo.value));

            return parametros;
        }

        public DespuesDeExportar(peticion: ApiDeAjax.DescriptorAjax): any {
            if (Definido(peticion.resultado.datos))
                ApiDeArchivos.DescargarArchivo(null, peticion.resultado.datos);
            this.ModalExportacion_Cerrar();
        }

        public EjecutarAccion(accion: string, persistir: boolean = true): void {

            if (this.modoTrabajo === enumModoTrabajo.editando && this.crudDeEdicion.HayCambiosPendientes && persistir) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.EjecutarAccion(accion, false)));
                this.crudDeEdicion.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, this.IdNegocio));

            let id = this.modoTrabajo === enumModoTrabajo.mantenimiento ? this.InfoSelector.Seleccionados[0].Id : this.crudDeEdicion.ElementoEditado.Id;

            ApiDePeticiones.EjecutarAccion(this, this.Controlador, accion, id, parametros, datosDeEntrada)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.Recargar(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        public Recargar(peticion: ApiDeAjax.DescriptorAjax): any {
            let crud: CrudMnt = peticion.llamador as CrudMnt;
            if (crud.modoTrabajo == enumModoTrabajo.mantenimiento)
                crud.RestaurarPagina();
            else
                crud.crudDeEdicion.Recargar();
        }

        public AbrirModalDePermisos(idModalDePermisos: string): void {
            let modal: HTMLDivElement = document.getElementById(idModalDePermisos) as HTMLDivElement;

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro('modal', modal));

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, this.IdNegocio));

            let id = this.modoTrabajo === enumModoTrabajo.mantenimiento ? this.InfoSelector.Seleccionados[0].Id : this.crudDeEdicion.ElementoEditado.Id;

            ApiDePeticiones.LeerElementoPorId(this, Ajax.Permisos.controlador, id, parametros, datosDeEntrada)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.MapearPermisos(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }
        public MapearPermisos(peticion: ApiDeAjax.DescriptorAjax): any {
            let modal: HTMLDivElement = peticion.DatosDeEntrada[0].valor as HTMLDivElement;
            MapearAlPanel.ElObjeto(modal, peticion.resultado.datos, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            ApiPanel.AbrirModal(modal);
        }


        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion.startsWith(`${ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla}_`)) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrMenus.opcion, opcion));

                ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeFiltrado, idNegocio, opcion, parametros, datosDeEntrada)
                    .then((peticion) => {
                        this.DespuesDeProcesarPeticion(peticion);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
            super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.Titulo));

            if (opcion === ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_EliminarPlantillaFiltrado || opcion === ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_GuardarPlantillaFiltrado)
                datosDeEntrada.push(new Parametro(literal.controlador, ltrControladores.Negocio.PlantillasDeFiltrado));
        }

        public DespuesDeProcesarPeticion(peticion: ApiDeAjax.DescriptorAjax): boolean {
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion.startsWith(`${ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla}_`)) {
                peticion.llamador.AplicarPlantillaDeFiltrado(peticion.llamador, peticion.resultado.datos);
            }
            peticion.llamador.IndicarSiHayFiltrosEnAlgunaModal();
            peticion.llamador.CargarGrid();
            return super.DespuesDeProcesarOpcionMf(peticion);

        }

        public AplicarPlantillaDeFiltrado(crud: CrudMnt, filtros: any) {
            var valor = ObtenerPropiedad(filtros, ltrPropiedades.Negocio.PlantillaDeFiltrado.Valor);
            crud.InicializarPanelDeFiltro(crud.PanelFiltro);
            crud.InicializarModalesDeFiltrado();
            let ordenado: boolean = false;
            this.Ordenacion.Clonar(new Tipos.Ordenacion());

            //en los valor puede venir también las columnas que ha de mostrar, el tamano y orden de las columnas

            let bloques: Diccionario<Array<Tipos.Filtro> | Tipos.Ordenacion | Array<ApiDeGrid.VisibilidadDeColumna> | Array<ApiDeGrid.DisposicionDeColumna> | Array<ApiDeGrid.TamanoDeColumna>> = JsonToDiccionario(valor);
            try {
                for (let i: number = 0; i < bloques.Elementos; i++) {
                    let bloque = bloques.Elemento(i);
                    if (bloque.clave === ltrPropiedades.Negocio.PlantillaDeFiltrado.OrdenacionGuardada) {
                        let ordenacion = bloque.valor as Tipos.Ordenacion;
                        this.Ordenacion.Clonar(Tipos.DeserializarOrdenacion(ordenacion));
                        ordenado = true;
                    }
                    else if (bloque.clave === ltrPropiedades.Negocio.PlantillaDeFiltrado.GuardarColumnasDelGrid) {
                        let visibilidad = bloque.valor as Array<ApiDeGrid.VisibilidadDeColumna>;
                        for (var v = 0; v < visibilidad.length; v++) {
                            ApiDeGrid.OcultarMostrarColumna(this.Tabla, visibilidad[v].Propiedad, visibilidad[v].Visible);
                        }
                    }
                    else if (bloque.clave === ltrPropiedades.Negocio.PlantillaDeFiltrado.DisposicionDelEncolumnado) {
                        let disposicion = bloque.valor as Array<ApiDeGrid.DisposicionDeColumna>;
                        ApiDeGrid.AplicarDisposicionDelEncolumnado(this.Tabla, disposicion);

                    }
                    else if (bloque.clave === ltrPropiedades.Negocio.PlantillaDeFiltrado.TamanoDelEncolumnado) {
                        let tamanos = bloque.valor as Array<ApiDeGrid.TamanoDeColumna>;
                        ApiDeGrid.AplicarTamanosALaTabla(this.Tabla, tamanos);

                    }
                    else for (let j = 0; j < (bloque.valor as Array<Tipos.Filtro>).length; j++) {
                        var filtro = bloque.valor[j];
                        ApiDeFiltro.MapearFiltrosPasados(Tipos.ClonarFiltro(filtro as Tipos.Filtro));
                    }
                }
                if (!ordenado) this.ResetearOrdenacion();
            }
            finally {
                this.FilaCabecara = undefined;
                this.AplicarTamanosAlEncolumnado();
                crudMnt.GuardarTamanoDeColumnas();
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);

            if (opcion == ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_OcultarColumnas) {
                let idModal = crudMnt.IdCrud + '-' + opcion;
                ApiPanel.AbrirModalDeDatos(idModal);
                return true;
            }
            if (opcion == ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_GuardarPlantillaFiltrado) {
                let idModal = crudMnt.IdCrud + '-' + opcion;
                ApiPanel.AbrirModalDeDatos(idModal);
                return true;
            }
            if (opcion == ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_EliminarPlantillaFiltrado) {
                let idModal = crudMnt.IdCrud + '-' + opcion;
                var modal = ApiPanel.AbrirModalDeDatos(idModal);
                var lista = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla);
                ApiControl.QuitarOpcionesDeLalista(lista);
                MapearAlControl.MapearElementosEnLaLista(lista.id, 0, peticion.resultado.datos);
                return true;
            }

            return super.DespuesDeProcesarOpcionMf(peticion);
        }

        public async MostrarPanelDeTotales(controlador: string) {

            this.OcultarVisorDeDetalle();
            if (this.ContenedorDeGraficos === this.PanelDeTotales.parentElement)
                this.PadreDelPanelDeTotales.insertBefore(this.PanelDeTotales, this.PadreDelPanelDeTotales.appendChild(this.PanelDeTotales));

            const modal: HTMLDivElement = this.ModalMostrarTotales;
            if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('MostrarPanelDeTotales', `la modal para mostrar totales no está definida`);
            ApiPanel.BlanquearControlesDeIU(modal);
            const filtros: Array<Parametro> = new Array<Parametro>();
            filtros.push(new Parametro(Ajax.Param.filtro, this.ObtenerFiltros(ltrOperacion.CargarDatos)));
            var totales = await ApiDelCrud.Totales(controlador, 0, -1, filtros);
            MapearAlPanel.ElObjeto(modal, totales, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            const infoDeTotales = ApiControl.BuscarEditor(modal, ltrPropiedades.Negocio.Totales.Comun.Procesados);
            AsignarValor(infoDeTotales, ObtenerPropiedad(totales, ltrPropiedades.Negocio.Totales.Comun.Procesados))
            ApiPanel.AbrirModal(modal);
        }

        public GuardarPlantillaFiltrado(): void {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.datosPeticion, this.DefinirPlantillaDeFiltrado()));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla, ApiControl.BuscarEditor(this.ModalParaCrearFiltro, ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla).value.trim()));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.Titulo));
            ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeFiltrado, this.IdNegocio, ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_GuardarPlantillaFiltrado, parametros, datosDeEntrada)
                .then((peticion) => this.DespuesDeGuardarPlantillaFiltrado(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        DespuesDeGuardarPlantillaFiltrado(peticion: ApiDeAjax.DescriptorAjax): any {
            ApiDeFiltro.IncluirPlantillaDeFiltrado(peticion.resultado.datos);
        }

        public EliminarPlantillaFiltrado(): void {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            let lista = ApiControl.BuscarListaDeElementos(this.ModalParaEliminarFiltro, ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla);
            if (lista.selectedIndex === 0)
                MensajesSe.EmitirExcepcion('EliminarPlantillaFiltrado', 'Debe seleccionar una plantilla a borrar');

            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.IdPlantilla, Numero(lista.value)));

            ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeFiltrado, this.IdNegocio, ltrMenus.eventosDeMf.Parametrizacion.Negocio.Comun_EliminarPlantillaFiltrado, parametros, datosDeEntrada)
                .then((peticion) => this.DespuesDeEliminarPlantillaFiltrado(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public DespuesDeEliminarPlantillaFiltrado(peticion: ApiDeAjax.DescriptorAjax): any {
            let menu = document.getElementById(ltrMenus.menu.filtro) as HTMLUListElement;
            let id = `${ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla}_${ObtenerPropiedad(peticion.resultado.datos, literal.id)}`;
            ApiDeMenuFlotante.QuitarOpcion(menu, id);

            if (menu.children.length === Numero(ltrMenus.MenuDeFiltrado.NumeroDeOpciones) + 1)
                ApiDeMenuFlotante.QuitarUltimoHr(menu);
        }

        public DefinirPlantillaDeFiltrado(): any {
            var filtros = new Diccionario<Tipos.Filtro | Tipos.Ordenacion | Array<ApiDeGrid.VisibilidadDeColumna> | Array<ApiDeGrid.DisposicionDeColumna> | Array<ApiDeGrid.TamanoDeColumna>>();
            MapearAlDiccionario.Filtros(this.PanelFiltro, filtros, this.PanelFiltro.id);
            let modales: NodeListOf<HTMLDivElement> = this.Cuerpo.querySelectorAll(`div[${atControl.tipoModal}=${enumTipoDeModal.ModalDeFiltrado}`) as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < modales.length; i++)
                MapearAlDiccionario.Filtros(modales[i], filtros, modales[i].id);

            filtros.Agregar(ltrPropiedades.Negocio.PlantillaDeFiltrado.OrdenacionGuardada, this.Ordenacion)
            filtros.Agregar(ltrPropiedades.Negocio.PlantillaDeFiltrado.GuardarColumnasDelGrid, ApiDeGrid.VisibilidadDeColumnas(this.ModalDeOcultarColumnas));
            filtros.Agregar(ltrPropiedades.Negocio.PlantillaDeFiltrado.DisposicionDelEncolumnado, ApiDeGrid.DisposicionDelEncolumnado(this.Tabla));
            filtros.Agregar(ltrPropiedades.Negocio.PlantillaDeFiltrado.TamanoDelEncolumnado, ApiDeGrid.TamanoDelEncolumnado(this.Tabla));
            return JSON.stringify(filtros);
        }

        public NavegarAEditarElemento(pagina: string, url: string, idElemento: number) {
            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(pagina);
            let urlDestino: string = `${window.location.origin}/${url}?${ltrParametrosUrl.id}=${idElemento}`;
            estado.Agregar(ltrClaveDeEstado.paginaOrigen, Crud.crudMnt.Pagina);
            estado.Agregar(ltrClaveDeEstado.paraqueNavegar, enumParaQueNavegar.editar);
            estado.Guardar();
            EntornoSe.NavegarAUrl(urlDestino);
            return;
        }

        public MapearDatosJsonDesdeElVisor(json: object): boolean {
            if (this.EstoyCreando)
                return this.crudDeCreacion.MapearDatosJsonDesdeElVisor(json);
            if (this.EstoyEditando)
                return this.crudDeEdicion.MapearDatosJsonDesdeElVisor(json);
            MensajesSe.Error("MapearDatosJsonDesdeElVisor", "Este método sólo es invocable para crear o editar");
            return false
        }

    }

    let clickCount = 0;
    let timer;
    let espera = 400;

    export function ManejadorDelClickEnElGrid(idCheck: string, idDelInput: string, event) {
        clickCount++;
        if (clickCount === 1) {
            timer = setTimeout(function () {
                clickCount = 0;
                crudMnt.EventoDeFilaPulsada(idCheck, idDelInput, event['shiftKey'], event['ctrlKey'], true);
            }, espera);
        } else if (clickCount === 2) {
            event = null;
            clearTimeout(timer);
            clickCount = 0;
            crudMnt.MenuGrid_DeselecionarTodasLasFilas();
            crudMnt.EventoDeFilaPulsada(idCheck, idDelInput, false, false, false);
            crudMnt.IraEditar();
        }
    }


    export function Neg_Tras_Pulsar_Mostrar_Colores(check: HTMLInputElement) {
        if (Crud.crudMnt.Inicializando)
            return;
        const filas = Crud.crudMnt.Tabla.querySelectorAll('.' + ltrCss.crud.fila) as NodeListOf<HTMLDivElement>;
        let i: number = 1;
        if (check.checked) {
            Crud.crudMnt.RestaurarPagina();
        }
        else {
            for (i = 1; i < filas.length; i++) {
                const fila = filas[i];
                const clases = fila.classList;
                clases.forEach((clase) => {
                    if (clase.startsWith(ltrCss.filaEstado)) {
                        fila.classList.remove(clase);
                    }
                });
            }
        }
    }
}
