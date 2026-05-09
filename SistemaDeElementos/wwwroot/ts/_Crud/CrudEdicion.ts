namespace Crud {


    export class CrudEdicion extends CrudBase {

        protected _idPanelEdicion: string;
        private _infoSelectorEdicion;
        private _modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos = undefined;
        private _objetosDeUnExpansor: Diccionario<any> = new Diccionario<any>();
        private idModalDeCreacionDeArchivadores: string = 'archivadores';
        private idGridDeArchivadores = 'archivadores'.toLocaleLowerCase();
        private _diccionarioDeCarga: Map<string, boolean>;
        private _registro: any;
        private _paginaDeConsultaConGuid: boolean = false;
        private _guidDeConsulta: string = null;
        private _idDeConsulta: number = null;

        public get PaginaDeConsultaConGuid(): boolean {
            return this._paginaDeConsultaConGuid;
        }

        public get GuidDeConsulta(): string {
            return this._guidDeConsulta;
        }

        public get IdDeConsulta(): number {
            return this._idDeConsulta;
        }
        public get Registro(): any {
            return this._registro;
        }

        public get RegistroId(): number {
            return this.Registro[literal.id]
        }


        public get IdTipo(): number {
            return ObtenerPropiedad(this._registro, ltrPropiedades.Elemento.ConTipo.IdTipo, 0, false);
        }


        public get Id(): number {
            return ObtenerPropiedad(this._registro, ltrPropiedades.Elemento.Id, 0, false);
        }

        private _etapas: Array<string> = undefined;
        protected get Etapas(): Array<string> {
            if (!this._etapas)
                this._etapas = ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.FacturaRec.Etapas, false);
            return this._etapas;
        }

        private _ampliacionesPorCargar: number = 0;

        public get AmpliacionesPorCargar(): number {
            return this._ampliacionesPorCargar;
        }

        public set AmpliacionesPorCargar(value: number) {
            if (value === 0)
                this.IndicarCargaRealizada(ltrCrud.Enumerados.Edicion.Carga.Ampliaciones);
            this._ampliacionesPorCargar = value;
        }

        public get CargaRealizada(): boolean {
            if (!Definido(this._diccionarioDeCarga)) {
                this.ModoTrabajo = enumModoTrabajo.consultando;
                return true;
            }
            return Array.from(this._diccionarioDeCarga.values()).every(value => value === true);
        }

        public get EstoyEditando(): boolean {
            if (this.PaginaDeConsultaConGuid)
                return false;
            return this.CrudDeMnt.EstoyEditando
        }


        public set Registro(value: any) {
            this._registro = value;
            this._sociedadDelCg = undefined;
            this._etapas = undefined;
        }
        private _sociedadDelCg: any;

        public async SociedadDelCg(): Promise<any> {
            if (!Definido(this._sociedadDelCg) && Definido(this.Registro) && ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, 0) > 0) {
                try {
                    const peticion = await ApiDePeticiones.LeerElementoPorId(
                        this,
                        ltrControladores.Terceros.Sociedades,
                        this.Registro.idSociedadDelCg,
                        new Array<Parametro>(),
                        null
                    );
                    this._sociedadDelCg = peticion.resultado.datos;
                }
                catch (peticion) {
                    ApiDePeticiones.EmitirError(peticion);
                }
            }
            return this._sociedadDelCg;
        }

        public ValoresInicales: Map<string, Map<string, any>> = new Map<string, Map<string, any>>();

        public ObjetoDeExpansor(idModal: string): any {
            return this._objetosDeUnExpansor.Obtener(idModal);
        }
        public AsignarObjetoDeExpansor(idModal: string, objeto): any {
            return this._objetosDeUnExpansor.Agregar(idModal, objeto);
        }

        public get HayCambiosPendientes(): boolean {
            if (!this.EstoyEditando)
                return false;

            if (!this.CargaRealizada) {
                MensajesSe.Info('Aun está cargando, espere')
                return;
            }

            if (this.ModoDeAcceso === ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso
                || this.ModoDeAcceso === ModoAcceso.enumModoDeAccesoDeDatos.Consultor)
                return false;

            let valores = this.ValoresInicales.get(this.PanelDelDto.id);
            var hay = !Definido(valores) ? true : ApiPanel.HayCambios(this.PanelDelDto, valores);
            if (hay) return true;
            let ampliaciones: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`div[${atControl.esAmpliacion}="true"]`) as NodeListOf<HTMLDivElement>;
            for (let i = 0; i < ampliaciones.length; i++) {
                let valores = this.ValoresInicales.get(ampliaciones[i].id);
                var hay = !Definido(valores) ? true : ApiPanel.HayCambios(ampliaciones[i], valores);
                if (hay) return true;
            }
            return false;
        }

        private _esDeProceso: boolean = undefined;
        public get EsDeProceso(): boolean {
            if (this._esDeProceso === undefined) {
                this._esDeProceso = this.EsModal ? false : ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.IdEstado, 0) > 0;
            }
            return this._esDeProceso;
        }

        public get IdEstado(): number {
            return this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.IdEstado, 0) : undefined;
        }

        private _idSociedadDelCg: number = undefined;
        public get IdSociedadDelCg(): number | null {
            if (this._idSociedadDelCg === undefined) {
                this._idSociedadDelCg = this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, 0) : null;
            }
            return this._idSociedadDelCg;
        }

        public get EstadoActual(): string {
            return this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.Estado, '') : undefined;
        }

        public get IdEstadoAnterior(): number {
            return this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.IdEstadoAnterior, 0) : undefined;
        }

        public get EstadoAnterior(): string {
            return this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.EstadoAnterior, 0) : undefined;
        }


        public get IdTransicionAplicable(): number {
            return this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.IdTransicionAplicable, 0) : undefined;
        }

        public get TransicionAplicable(): string {
            return this.EsDeProceso ? ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.TransicionAplicable, 0) : undefined;
        }

        public get HayMasDeUnaTransicionParaAvanzar(): boolean {
            return this.EsDeProceso ? Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.DeProceso.TransicionesDisponibles, 0)) > 1 : undefined;
        }

        public get EstaTerminada(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.EstaTerminada, false);
        }

        public get EstaCancelada(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.EstaCancelada, false);
        }

        public get EstaDeBaja(): boolean {
            return this.UsaBaja && ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.Baja, false);
        }

        public get UsaBaja(): boolean {
            return ExistePropiedad(this.Registro, ltrPropiedades.baja);
        }

        public get EsAdministrador(): boolean {
            return ModoAcceso.EsAdministrador(this.ModoDeAcceso)
        }

        public get EsInterventor(): boolean {
            return ModoAcceso.EsInterventor(this.ModoDeAcceso)
        }

        public get EsInterventorSinEstado(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.EsInterventor, false);
        }

        public get EsGestor(): boolean {
            if (this.PaginaDeConsultaConGuid || this.CrudDeMnt.SiempreEnConsulta)
                return false;
            return ModoAcceso.EsGestor(this.ModoDeAcceso)
        }

        public get EsConsultor(): boolean {
            if (this.PaginaDeConsultaConGuid || this.CrudDeMnt.SiempreEnConsulta)
                return true;
            return ModoAcceso.EsConsultor(this.ModoDeAcceso)
        }

        public get SinPermiso(): boolean {
            return !ModoAcceso.EsConsultor(this.ModoDeAcceso)
        }

        public get ModoDeAcceso(): ModoAcceso.enumModoDeAccesoDeDatos {
            if (this._modoDeAcceso === undefined)
                this._modoDeAcceso = this.PaginaDeConsultaConGuid ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor : ObtenerPropiedad(this.Registro, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
            return this._modoDeAcceso;
        }

        public set ModoDeAcceso(modo: ModoAcceso.enumModoDeAccesoDeDatos) {
            if (modo === ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso)
                this._modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso;
            else if (this.PaginaDeConsultaConGuid || this.CrudDeMnt.SiempreEnConsulta)
                this._modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;
            else
                this._modoDeAcceso = modo;
        }

        public get IdNegocio(): number {
            if (this.PaginaDeConsultaConGuid) {
                const panelDeEdicion = document.getElementsByClassName(ltrCss.crud.edicionSoloConsulta)[0] as HTMLDivElement;
                return Numero(panelDeEdicion.getAttribute(atMantenimniento.idNegocio));
            }
            return this.CrudDeMnt.IdNegocio;
        }


        public get NombreDeNegocio(): string {
            if (this.PaginaDeConsultaConGuid) {
                const panelDeEdicion = document.getElementsByClassName(ltrCss.crud.edicionSoloConsulta)[0] as HTMLDivElement;
                return panelDeEdicion.getAttribute(atMantenimniento.negocio);
            }
            return this.CrudDeMnt.NombreDeNegocio;
        }

        public get ModoAccesoAlNegocio(): ModoAcceso.enumModoDeAccesoDeDatos {
            if (this.PaginaDeConsultaConGuid)
                return ModoAcceso.enumModoDeAccesoDeDatos.Consultor;
            return this.CrudDeMnt.ModoAccesoAlNegocio;
        }

        public get ModoTrabajo(): string {
            if (this.PaginaDeConsultaConGuid) {
                return ModoAcceso.ModoDeAccesoDeDatos.Consultor;
            }
            return this.CrudDeMnt.ModoTrabajo
        }

        public set ModoTrabajo(modo: string) {
            if (!this.PaginaDeConsultaConGuid) {
                this.CrudDeMnt.ModoTrabajo = modo;
            }
        }

        private _permitirSubirArchivos: boolean;
        public get PermitirSubirArchivos(): boolean {
            if (!this.PaginaDeConsultaConGuid)
                return this._permitirSubirArchivos;
            return false;
        }
        private set PermitirSubirArchivos(permitir: boolean) {
            this._permitirSubirArchivos = this.PaginaDeConsultaConGuid ? false : permitir;
        }

        private _contenedorDeDatosMasVisor = null;
        public get ContenedorDeDatosMasVisor(): HTMLDivElement {
            if (!Definido(this._contenedorDeDatosMasVisor)) {
                this._contenedorDeDatosMasVisor = document.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorDeDatosMasVisor) as HTMLDivElement
            }
            return this._contenedorDeDatosMasVisor as HTMLDivElement
        }

        private _contenedorCabecera = null;
        public get ContenedorDeCabecera(): HTMLDivElement {
            if (!Definido(this._contenedorCabecera)) {
                this._contenedorCabecera = this.PanelDeEditar.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorCabecera) as HTMLDivElement
            }
            return this._contenedorCabecera as HTMLDivElement
        }
        private _contenedorDeDatos = null;
        public get ContenedorDeDatos(): HTMLDivElement {
            if (!Definido(this._contenedorDeDatos)) {
                this._contenedorDeDatos = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorDeDatosDto) as HTMLDivElement
            }
            return this._contenedorDeDatos as HTMLDivElement
        }

        private _padreContenedorDeDatosPrincipales = null;
        public get PadreContenedorDeDatosPrincipales(): HTMLDivElement {
            if (!Definido(this._padreContenedorDeDatosPrincipales)) {
                this._padreContenedorDeDatosPrincipales = document.getElementsByClassName(ltrCss.crud.padreContenedorDatosPrincipales)[0] as HTMLDivElement
            }
            return this._padreContenedorDeDatosPrincipales as HTMLDivElement
        }

        private _contenedorDeDatosPrincipales = null;
        public get ContenedorDeDatosPrincipales(): HTMLDivElement {
            if (!Definido(this._contenedorDeDatosPrincipales)) {
                this._contenedorDeDatosPrincipales = document.getElementsByClassName(ltrCss.crud.contenedorDatosPrincipales)[0] as HTMLDivElement
            }
            return this._contenedorDeDatosPrincipales as HTMLDivElement
        }

        private _contenedorVisor = null;
        public get ContenedorDelVisor(): HTMLDivElement {
            if (!Definido(this._contenedorVisor)) {
                this._contenedorVisor = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorVisor) as HTMLDivElement
            }
            return this._contenedorVisor as HTMLDivElement
        }

        private _contenedorDelHistorial = null;
        public get ContenedorDelHistorial(): HTMLDivElement {
            if (!Definido(this._contenedorDelHistorial)) {
                this._contenedorDelHistorial = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorHistorial) as HTMLDivElement
            }
            return this._contenedorDelHistorial as HTMLDivElement
        }

        private _visorDelHistorial = null;
        public get VisorDelHistorial(): HTMLDivElement {
            if (!Definido(this._visorDelHistorial)) {
                ////document.querySelector('.visor-edicion-historial');
                this._visorDelHistorial = this.ContenedorDelHistorial?.querySelector('.' + ltrCss.crud.panelDeEdicion.VisorDeHistorial) as HTMLDivElement
            }
            return this._visorDelHistorial as HTMLDivElement
        }

        private _menuDelHistorial = null;
        public get MenuDelHistorial(): HTMLDivElement {
            if (!Definido(this._menuDelHistorial)) {
                ////document.querySelector('.menu-edicion-historial')
                this._menuDelHistorial = this.ContenedorDelHistorial?.querySelector('.' + ltrCss.crud.panelDeEdicion.MenuDeHistorial) as HTMLDivElement
            }
            return this._menuDelHistorial as HTMLDivElement
        }


        private _ayudaDelBotonIa = null;
        private _botonIa: HTMLButtonElement = null;
        public get BotonIa(): HTMLButtonElement {
            if (!Definido(this._botonIa))
                this._botonIa = this.ContenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.ProcesarConIa)[0] as HTMLButtonElement;
            return this._botonIa;
        }
        protected get AyudaDelBotonIa() {
            return this._ayudaDelBotonIa;
        }
        protected set AyudaDelBotonIa(value: string) {
            this._ayudaDelBotonIa = value;
        }


        private _DivVisor = null;
        public get DivVisor(): HTMLDivElement {
            if (!Definido(this._DivVisor)) {
                this._DivVisor = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelDeEdicion.VisorDeEdicion) as HTMLDivElement
            }
            return this._DivVisor as HTMLDivElement
        }


        private _ContenedorDelVisorDeArchivoConHistorial = null;
        public get ContenedorDelVisorDeArchivoConHistorial(): HTMLDivElement {
            if (!Definido(this._DivVisor)) {
                this._ContenedorDelVisorDeArchivoConHistorial = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorDelVisorDeArchivoConHistorial) as HTMLDivElement
            }
            return this._ContenedorDelVisorDeArchivoConHistorial as HTMLDivElement
        }

        private _splitter = null;
        public get Splitter(): HTMLDivElement {
            if (!Definido(this._splitter)) {
                this._splitter = this.PanelDeEditar.querySelector('.splitter') as HTMLDivElement;
            }
            return this._splitter as HTMLDivElement
        }

        public get ArchivosRenderizables(): number {
            var panel = this.PanelDeArchivos;
            if (!Definido(panel))
                return 0;

            let ficheros = panel.querySelectorAll(`div[${atControl.class}=${ltrCss.Espan.cssVisorArchivos}]`) as NodeListOf<HTMLDivElement>;
            return ficheros.length;
        }

        private _idArchivoMostrado: number = undefined;
        public get IdArchivoMostrado(): number {
            return Definido(this._idArchivoMostrado) ? this._idArchivoMostrado : 0;
        }

        public set IdArchivoMostrado(value: number) {
            if (Definido(this.ContenedorDeDatosMasVisor)) {
                if (!Definido(value) || value == 0) {
                    ApiControl.IncluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
                    this.IdDelUltimoArchivoRenderizado = undefined;
                }
                else {
                    ApiControl.ExcluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
                }
                this._idArchivoMostrado = value;
            }
        }

        public AsignarIdArchivo(id: number, ajustarVisor: boolean) {
            this.IdArchivoMostrado = id;
            this.IdDelUltimoArchivoRenderizado = id;
            if (ajustarVisor)
                ApiDelCrud.CalcularTamanoDelVisor();
        }

        private _botonVisor: HTMLImageElement = undefined;
        public get BotonVisor(): HTMLImageElement {
            if (!this._botonVisor) {
                let contenedorAcciones = document.getElementsByClassName(ltrCss.crud.panelDeEdicion.ContenedorDeAcciones)[0] as HTMLDivElement;
                this._botonVisor = contenedorAcciones?.querySelector('.' + ltrCss.crud.panelDeEdicion.Acciones.Visor) ?? undefined;
            }
            return this._botonVisor;
        }

        private _botonDevolver: HTMLImageElement = undefined;
        public get BotonDevolver(): HTMLImageElement {
            if (!this._botonDevolver) {
                let contenedorAcciones = document.getElementsByClassName(ltrCss.crud.panelDeEdicion.ContenedorDeAcciones)[0] as HTMLDivElement;
                this._botonDevolver = contenedorAcciones?.querySelector('.' + ltrCss.crud.panelDeEdicion.Acciones.Devolver) ?? undefined;
            }
            return this._botonDevolver;
        }
        private _botonAvanzar: HTMLImageElement = undefined;
        public get BotonAvanzar(): HTMLImageElement {
            if (!this._botonAvanzar) {
                let contenedorAcciones = document.getElementsByClassName(ltrCss.crud.panelDeEdicion.ContenedorDeAcciones)[0] as HTMLDivElement;
                this._botonAvanzar = contenedorAcciones?.querySelector('.' + ltrCss.crud.panelDeEdicion.Acciones.Avanzar) ?? undefined;
            }
            return this._botonAvanzar;
        }


        public get VisorVisible(): boolean {
            return this.BotonVisor.classList.contains(ltrCss.crud.panelDeEdicion.Acciones.OcultarVisor)
        }

        private OcultarBotonVisor(): void {
            ApiControl.ExcluirCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.MostrarVisor);
            ApiControl.ExcluirCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.OcultarVisor);
            ApiControl.IncluirCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.SinVisor);
        }

        private _mostrarVisorAlIniciar: boolean = true;
        public get MostrarVisorAlIniciar(): boolean {
            this._mostrarVisorAlIniciar = this.CrudDeMnt.MapIndicadores.get(ltrPropiedades.Entorno.Vista.Indicadores.MostrarVisorAlIniciar)
            if (Definido(this._mostrarVisorAlIniciar))
                this.CrudDeMnt.MapIndicadores.delete(ltrPropiedades.Entorno.Vista.Indicadores.MostrarVisorAlIniciar);
            return this._mostrarVisorAlIniciar;
        }

        private _idDelUltimoArchivoRenderizado: number = undefined;
        protected get IdDelUltimoArchivoRenderizado(): number {
            return Definido(this._idDelUltimoArchivoRenderizado) ? this._idDelUltimoArchivoRenderizado : 0;
        }
        private set IdDelUltimoArchivoRenderizado(value: number) {
            this._idDelUltimoArchivoRenderizado = value;
        }

        public CrudDeMnt: CrudMnt;
        protected PanelDeMnt: HTMLDivElement;

        private _estoyMostrandoHistorial = false;
        public get EstoyMostrandoHistorial(): boolean {
            return this._estoyMostrandoHistorial;
        }
        public set EstoyMostrandoHistorial(valor: boolean) {
            this._estoyMostrandoHistorial = valor
            if (!valor && !ApiControl.EsVisible(this.BotonVisor)) this.OcultameElVisor();
        }

        public get PanelDeEditar(): HTMLDivElement {
            return document.getElementById(this._idPanelEdicion) as HTMLDivElement;
        }

        public get CuerpoDePaginaDeConsulta(): HTMLDivElement {
            return document.getElementById('cuerpo-de-pagina') as HTMLDivElement;
        }
        public get ControlDeNombre(): HTMLInputElement {
            return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Elemento.Nombre);
        }

        public get ContenedorMenuIndividual(): HTMLDivElement {
            return this.PanelDeEditar.querySelector(`${ltrMenus.menu.individual}`) as HTMLDivElement;
        }

        public get PanelDelDto(): HTMLDivElement {
            let id = this.EsModal
                ? 'contenedor_edicion_cuerpo_' + this._idPanelEdicion
                : 'contenedor_dto_' + this._idPanelEdicion + '-dp';
            return document.getElementById(id) as HTMLDivElement;
        }

        public get PanelDeCabecera(): HTMLDivElement {
            let id = this.EsModal
                ? this._idPanelEdicion + '_cabecera'
                : this._idPanelEdicion + '.cabecera';
            return document.getElementById(id) as HTMLDivElement;
        }

        public get SelectorDeArchivos(): HTMLDivElement {
            return !this.EsModal
                ? document.querySelector(`.${ltrCss.Archivos.SelectorDeArchivos}`)
                : undefined;
        }

        public get PanelDeArchivos(): HTMLDivElement {
            return !this.EsModal
                ? document.getElementById(`contenedor-${this._idPanelEdicion}-archivos`) as HTMLDivElement
                : undefined;
        }

        public get FiltroDeArchivos(): HTMLInputElement {
            return this.PanelDeEditar.querySelector(`input.${ltrCss.crud.panelDeEdicion.FiltroSelectorDeArchivos}`) as HTMLInputElement;
        }

        public get TablaDeArchivadores(): HTMLDivElement {
            return this.TablaDelDetalle(this.idGridDeArchivadores) as HTMLDivElement;
        }

        public get PanelDeArchivadores(): HTMLDivElement {
            let id = this._idPanelEdicion + '-' + this.idModalDeCreacionDeArchivadores;
            return document.getElementById(id) as HTMLDivElement;
        }

        public get Titulo(): string {
            var titulo = this.PanelDeEditar.querySelector(`div.${ltrCss.crud.contenedorTitulo}`);
            return Definido(titulo) ? titulo.innerHTML.trim() : '';
        }
        public set Titulo(titulo: string) {
            var control = this.PanelDeEditar.querySelector(`div.${ltrCss.crud.contenedorTitulo}`);
            if (Definido(control)) control.innerHTML = SanitizeHTML(titulo);
        }

        public get EsModal(): boolean {
            return this.PanelDeEditar.className === ltrCss.contenedorModal;
        }

        public get PanelDeContenidoModal(): HTMLDivElement {
            return document.getElementById(`${this._idPanelEdicion}_contenido`) as HTMLDivElement;
        }

        public get CheckDeBloqueado(): HTMLInputElement {
            return ApiControl.BuscarCheck(this.PanelDelDto, ltrPropiedades.Elemento.Bloqueado);
        }

        public get EstaBloqueado(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.Bloqueado, false);
        }

        public get QuienBloqueo(): string {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.Bloqueador);
        }

        private get InfoSelectorEdicion(): InfoSelector {
            return this._infoSelectorEdicion;
        }

        public get ContenedorMenu(): HTMLDivElement {
            return document.getElementById(`${this._idPanelEdicion}.${ltrMenus.menu.edicion}`) as HTMLDivElement;
        }

        private set InfoSelectorEdicion(info: InfoSelector) {
            this._infoSelectorEdicion = info;
            this.TotalSeleccionados = info.Cantidad;
            this.Posicionador = 1;
        }

        private get Posicionador(): number {
            let control: HTMLInputElement = document.getElementById(`${this._idPanelEdicion}-posicionador`) as HTMLInputElement;
            return Numero(control.value);
        }

        private set Posicionador(posicionador: number) {
            let control: HTMLInputElement = document.getElementById(`${this._idPanelEdicion}-posicionador`) as HTMLInputElement;
            control.value = posicionador.toString();
        }

        private get TotalSeleccionados(): number {
            return this.InfoSelectorEdicion.Cantidad;
        }

        private set TotalSeleccionados(cantidad: number) {
            let control: HTMLInputElement = document.getElementById(`${this._idPanelEdicion}-total-seleccionados`) as HTMLInputElement;
            control.value = cantidad.toString();
        }

        public get ElementoEditado(): Elemento {
            if (this.Posicionador === 0)
                this.Posicionador = 1;
            return this.InfoSelectorEdicion.Seleccionados[this.Posicionador - 1];
        }

        public get IdEditor(): HTMLInputElement {
            var control = ApiControl.BuscarEditor(this.PanelDeEditar, literal.id);

            if (control == null) {
                MensajesSe.Error("IdEditor", "No está definido el control para mostrar el id del elemento");
                this.CerrarEdicion();
            }

            return control as HTMLInputElement;
        }

        public get HtmlAnchorCrearEvento(): HTMLAnchorElement {
            return document.getElementById(this._idPanelEdicion + '-eventos-' + ltrEspanes.Opcion.crearRef + '.ref') as HTMLAnchorElement;
        }

        public get GridDeTrazas(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Comunes.Trazas);
        }

        public get GridDeArchivadores(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Comunes.Archivadores);
        }

        public get GridDeHitos(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Comunes.Hitos);
        }

        public get GridDeObservaciones(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Comunes.Observaciones);
        }

        public get ModalParaEditarEventoDeAgenda(): HTMLDivElement {
            return document.getElementById(this._idDeModalEditarRelacion(ltrEspanes.Entorno.Eventos)) as HTMLDivElement;
        }

        constructor(crud: CrudMnt, idPanelEdicion: string) {
            super(crud === null);

            if (IsNullOrEmpty(idPanelEdicion))
                throw Error("No se puede construir un objeto del tipo CrudEdicion sin indica el panel de edición");

            this._idPanelEdicion = idPanelEdicion;
            this.PanelDeMnt = crud?.CuerpoCabecera;
            this._controlador = this.PanelDeEditar.getAttribute(literal.controlador);
            this.CrudDeMnt = crud;

            // Función auxiliar para esperar a que los elementos existan
            const ejecutarCuandoEsteListo = (selector: string, callback: () => void) => {
                if (document.querySelector(selector)) {
                    callback();
                } else {
                    setTimeout(() => ejecutarCuandoEsteListo(selector, callback), 50);
                }
            };

            if (!Definido(crud)) {
                // LÓGICA DE CONSULTA (Read-only)
                // Esperamos a que existan los controles antes de aplicar readonly/disabled
                ejecutarCuandoEsteListo('input, select', () => {
                    const controles = document.querySelectorAll('input, textarea, select');
                    controles.forEach(control => {
                        const el = control as HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement;
                        if (el.tagName === 'SELECT' || (el as HTMLInputElement).type === 'checkbox') {
                            el.disabled = true;
                        } else {
                            (el as HTMLInputElement).readOnly = true;
                        }
                    });

                    document.querySelectorAll('a.boton-de-navegacion, button.btn-pegar-portapapeles').forEach(el => {
                        (el as HTMLElement).style.display = 'none';
                    });
                });

                this._paginaDeConsultaConGuid = true;
                this.ConsultarSeleccionado();
            }
            else {
                // LÓGICA DE EDICIÓN
                // Esperamos específicamente por el ID del input
                ejecutarCuandoEsteListo('#' + ltrValores.Crud.Edicion.diasDeConsulta, () => {
                    const input = document.getElementById(ltrValores.Crud.Edicion.diasDeConsulta) as HTMLInputElement;

                    input.addEventListener('blur', () => {
                        const boton = input.nextElementSibling as HTMLElement;
                        const icono = boton?.querySelector('.' + ltrCss.crud.panelDeEdicion.iconoConsultaConGuid) as HTMLElement;

                        if (Numero(input.value) === 0) {
                            if (icono) ApiControl.IncluirCss(icono, ltrCss.crud.panelDeEdicion.ojoTachado);
                        } else {
                            if (icono) ApiControl.ExcluirCss(icono, ltrCss.crud.panelDeEdicion.ojoTachado);
                        }
                    });
                });
            }
        }

        public IdAmpliacion(nombreDadoEnElDescriptor: string): string {
            return `mostrar.${this._idPanelEdicion}_ampliacion_${nombreDadoEnElDescriptor}`.toLowerCase();
        }

        public _idDeModalEditarRelacion(nombreDadoEnElDescriptor: string): string {
            return `${ltrTipoControl.gridDeDetalle}-${nombreDadoEnElDescriptor}-${enumPostfijoTipoModal.ModalDeEditarRelacion}`.toLowerCase();
        }

        public _idDelGridDetalle(nombreDadoEnElDescriptor: string): string {
            return `${ltrTipoControl.gridDeDetalle}-${nombreDadoEnElDescriptor}-tabla`.toLowerCase();
        }

        public _idDeModalCrearRelacion(nombreDadoEnElDescriptor: string): string {
            return `${this._idPanelEdicion}-${nombreDadoEnElDescriptor}-${enumPostfijoTipoModal.ModalDeCrearRelacion}`.toLowerCase();
        }

        public _idDeModalCrearVinculo(nombreDadoEnElDescriptor: string): string {
            return `${this._idPanelEdicion}-${nombreDadoEnElDescriptor}-${enumPostfijoTipoModal.ModalDeCrearVinculo}`.toLowerCase();
        }

        public DivDeAmpliacion(nombreDadoEnElDescriptor: string): HTMLDivElement {
            return document.getElementById(this.IdAmpliacion(nombreDadoEnElDescriptor)) as HTMLDivElement;
        }

        public ModalParaEditarRelacion(nombreDadoEnElDescriptor: string): HTMLDivElement {
            return BuscarElementoPorIdGenerado(nombreDadoEnElDescriptor, (nombre, i) => i === 0 ? this._idDeModalEditarRelacion(nombre) : this._idDeModalEditarRelacion(nombre + '_' + i));
        }

        public ModalParaCrearRelacion(nombreDadoEnElDescriptor: string): HTMLDivElement {
            return BuscarElementoPorIdGenerado(nombreDadoEnElDescriptor, (nombre, i) => i === 0 ? this._idDeModalCrearRelacion(nombre) : this._idDeModalCrearRelacion(nombre + '_' + i));
        }

        public TablaDelDetalle(nombreDadoEnElDescriptor: string): HTMLDivElement {
            return document.getElementById(this._idDelGridDetalle(nombreDadoEnElDescriptor)) as HTMLDivElement;
        }

        protected EsLaAmpliacionDe(ampliacion: HTMLDivElement, nombreDadoEnElDescriptor: string): boolean {
            if (ampliacion.id.replace('contenedor_dto_', 'mostrar.') === this.IdAmpliacion(nombreDadoEnElDescriptor))
                return true;
            else
                return false;
        }

        public IdGridDelExpansor(nombreDadoEnElDescriptor: string): string {
            let idGrid = `${ltrTipoControl.gridDeDetalle}-${nombreDadoEnElDescriptor}-contenedor`.toLowerCase();
            return idGrid;
        }

        public IdSpanDelExpansor(nombreDadoEnElDescriptor: string): string {
            let idSpan = `${this.PanelDeEditar.id}-${nombreDadoEnElDescriptor}-cabecera`.toLowerCase();
            return idSpan;
        }
        public IdDeExpansor(nombreDadoEnElDescriptor: string): string {
            return `${this._idPanelEdicion}-${nombreDadoEnElDescriptor}`.toLowerCase();
        }

        public DivDelExpansor(nombreDadoEnElDescriptor: string): HTMLDivElement {
            var id = 'mostrar.' + this._idPanelEdicion + '-' + nombreDadoEnElDescriptor;
            return document.getElementById(id.toLowerCase()) as HTMLDivElement;
        }

        public OpcionDeExpansor(nombreDadoEnElDescriptor: string, opcion: string): string {
            //contenedor.crud_expedientedto_panel-editor-tareas-ref_1.ref
            return `contenedor.${this.IdDeExpansor(nombreDadoEnElDescriptor)}-${opcion}.ref`.toLowerCase();
        }

        public IdModalDeVincular(nombreDadoEnElDescriptor: string): string {
            return `${this._idPanelEdicion}-${nombreDadoEnElDescriptor}-${enumPostfijoTipoModal.ModalParaVincular}`.toLowerCase();
        }

        public IdModalDeCrearDetalle(nombreDadoEnElDescriptor: string): string {
            return `${this._idPanelEdicion}-${nombreDadoEnElDescriptor}-${enumPostfijoTipoModal.ModalDeCrearDetalle}`.toLowerCase();
        }

        public EjecutarAcciones(accion: string, modal: HTMLDivElement) {
            let cerrarEdicion: boolean = false;
            try {
                ApiDeMenuFlotante.CerrarMf(this.PanelDeEditar);
                switch (accion) {
                    case ltrEventos.Edicion.Modificar: {
                        if (Definido(modal))
                            this.ModificarDatosDeLaModal(modal);
                        else
                            this.Modificar(ltrOperacion.ModificarPorId);
                        break;
                    }
                    case ltrEventos.Edicion.Cerrar: {
                        cerrarEdicion = true;
                        break;
                    }
                    case ltrEventos.Edicion.MostrarPrimero: {
                        this.EditarSeleccionado(1);
                        break;
                    }
                    case ltrEventos.Edicion.MostrarSiguiente: {
                        this.EditarSeleccionado(this.Posicionador + 1);
                        break;
                    }
                    case ltrEventos.Edicion.MostrarAnterior: {
                        this.EditarSeleccionado(this.Posicionador - 1);
                        break;
                    }
                    case ltrEventos.Edicion.MostrarUltimo: {
                        this.EditarSeleccionado(this.TotalSeleccionados);
                        break;
                    }
                    case ltrEventos.Edicion.SiguienteArchivo: {
                        this.SiguienteArchivo();
                        break;
                    }
                    case ltrEventos.Edicion.AnteriorArchivo: {
                        this.AnteriorArchivo();
                        break;
                    }
                    case ltrEventos.Edicion.Retroceder: {
                        this.Transitar(ltrEventos.Edicion.Retroceder);
                        break;
                    }
                    case ltrEventos.Edicion.Avanzar: {
                        this.Transitar(ltrEventos.Edicion.Avanzar);
                        break;
                    }
                    case ltrEventos.Edicion.ResumirArchivo: {
                        if (this.CrudDeMnt.EstoyCreando) {
                            this.CrudDeMnt.crudDeCreacion.ResumirArchivo();
                            return;
                        }
                        else
                            this.ResumirArchivo();
                        break;
                    }
                    case ltrEventos.Edicion.PasarOcr: {
                        if (this.CrudDeMnt.EstoyCreando) {
                            this.CrudDeMnt.crudDeCreacion.PasarOcr();
                            return;
                        }
                        else
                            this.PasarOcr();
                        break;
                    }
                    case ltrEventos.Edicion.DescargarArchivo: {
                        this.DescargarArchivo();
                        break;
                    }
                    case ltrEventos.Edicion.CompartirConWhatsApp: {
                        this.CompartirConWhatsApp();
                        break;
                    }
                    case ltrEventos.Edicion.CompartirConGuid: {
                        this.CompartirConGuid();
                        break;
                    }
                    case ltrEventos.Edicion.CompartirElemento: {
                        this.CompartirElemento();
                        break;
                    }
                    case ltrEventos.Edicion.ConsultarConGuid: {
                        this.ConsultarConGuid();
                        break;
                    }
                    case ltrEventos.Edicion.PlegarTodo: {
                        this.ContraerExpansores();
                        break;
                    }
                    case ltrEventos.Edicion.DesplegarTodo: {
                        this.ExpandirExpansores();
                        break;
                    }
                    case ltrEventos.Edicion.MostrarOcultarVisor: {
                        this.MostrarOcultarVisor();
                        break;
                    }
                    default: {
                        if (!this.EjecutarAccionesDelModulo(accion))
                            throw `la opción ${accion} no está definida`;
                    }
                }
            }
            catch (error) {
                MensajesSe.MostraExcepcion(error, "EjecutarAcciones");
            }

            if (cerrarEdicion)
                this.CerrarEdicion();
        }

        protected EjecutarAccionesDelModulo(accion: string): boolean {
            return false;
        }

        private handleResize() {
            ApiDelCrud.AjustarAnchoDeDatosMasVisor();
        }

        public ComenzarEdicion(infSel: InfoSelector) {
            this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.editando;
            if (this.ContenedorDeDatosPrincipales?.parentElement !== this.PadreContenedorDeDatosPrincipales)
                this.PadreContenedorDeDatosPrincipales.appendChild(this.ContenedorDeDatosPrincipales)
            this.InfoSelectorEdicion = infSel;

            if (this.EsModal) {
                this.PanelDeEditar.style.display = ltrStyle.display.block;
                EntornoSe.AjustarModalesAbiertas();
            }
            else {
                ApiDelCrud.CambiarPanelActivoDelCrud(this.CrudDeMnt.ModoTrabajo);
                if (Definido(this.ContenedorDelVisorDeArchivoConHistorial)) {
                    ApiDelCrud.ConfigurarEventosDeCambioDelAnchoContenedorDeDatos();
                    this.handleResize = this.handleResize.bind(this);
                    window.addEventListener('resize', this.handleResize);
                }
            }
            this.EditarSeleccionado(1);
        }

        public PosicionarEdicion(): void {
        }

        public MostrarOcultarHistorial(): void {
            if (!this.EstoyMostrandoHistorial) {
                this.MostrarHistorial();
                this.EstoyMostrandoHistorial = true;
            }
            else {
                this.OcultarHistorial();
                this.EstoyMostrandoHistorial = false;
            }
        }

        public MostrarHistorial() {
            ApiControl.ExcluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
            ApiControl.IncluirCss(this.ContenedorDelVisor, ltrCss.divNoVisible);
            ApiControl.ExcluirCss(this.ContenedorDelHistorial, ltrCss.divNoVisible);
        }

        private OcultarHistorial() {
            //if (this.IdArchivoMostrado === 0)
            //    ApiControl.IncluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
            ApiControl.IncluirCss(this.ContenedorDelHistorial, ltrCss.divNoVisible);
            ApiControl.ExcluirCss(this.ContenedorDelVisor, ltrCss.divNoVisible);
        }

        public MostrarOcultarVisor(): void {
            if (!ApiControl.EsVisible(this.BotonVisor)) {
                return;
            }
            if (this.BotonVisor.classList.contains(ltrCss.crud.panelDeEdicion.Acciones.MostrarVisor)) {
                this.MuestrameElVisor();

            }
            else {
                this.OcultameElVisor();
            }
        }

        private MuestrameElVisor(): void {
            ApiControl.RemplazarCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.MostrarVisor, ltrCss.crud.panelDeEdicion.Acciones.OcultarVisor);
            ApiControl.ExcluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
            this.RenderizarElPrimeroRenderizable(this.FiltroDeArchivos);
            ApiDelCrud.GuardarMostrarVisorAlIniciar(this.CrudDeMnt, true);
        }


        private OcultameElVisor(): void {
            ApiControl.RemplazarCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.OcultarVisor, ltrCss.crud.panelDeEdicion.Acciones.MostrarVisor);
            ApiControl.IncluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
            ApiDelCrud.GuardarMostrarVisorAlIniciar(this.CrudDeMnt, false);
        }

        protected ExpandirExpansores(): void {
            let espanDelDto = document.getElementById(this.PanelDeEditar.id + '-dp') as HTMLDivElement;
            ApiDelCrud.ExpandirExpansor(espanDelDto.id);

            let detalles: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`.${ltrCss.Detalle.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < detalles.length; i++) {
                let expansor: HTMLDivElement = document.getElementById(detalles[i].id.replace('mostrar.', '')) as HTMLDivElement;
                ApiDelCrud.ExpandirExpansor(expansor.id);
            }

            let ampliaciones: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`.${ltrCss.Ampliacion.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < ampliaciones.length; i++) {
                let expansor: HTMLDivElement = document.getElementById(ampliaciones[i].id.replace('mostrar.', '')) as HTMLDivElement;
                ApiDelCrud.ExpandirExpansor(expansor.id);
            }

            let bloques: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`.${ltrCss.Bloque.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < bloques.length; i++) {
                ApiDelCrud.ExpandirExpansor(bloques[i].id);
            }
        }

        protected ContraerExpansores(): void {
            let espanDelDto = document.getElementById(this.PanelDeEditar.id + '-dp') as HTMLDivElement;
            ApiDelCrud.ContraerExpansor(espanDelDto.id);

            let detalles: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`.${ltrCss.Detalle.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < detalles.length; i++) {
                let expansor: HTMLDivElement = document.getElementById(detalles[i].id.replace('mostrar.', '')) as HTMLDivElement;
                ApiDelCrud.ContraerExpansor(expansor.id);
            }

            let ampliaciones: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`.${ltrCss.Ampliacion.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < ampliaciones.length; i++) {
                let expansor: HTMLDivElement = document.getElementById(ampliaciones[i].id.replace('mostrar.', '')) as HTMLDivElement;
                ApiDelCrud.ContraerExpansor(expansor.id);
            }

            let bloques: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`.${ltrCss.Bloque.Contenedor}`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < bloques.length; i++) {
                ApiDelCrud.ContraerExpansor(bloques[i].id);
            }
        }

        public Recargar() {
            let editando = Numero(this.PanelDeEditar.getAttribute(ltrEdicion.Editando));
            this.EditarSeleccionado(editando);
        }

        public RecargarGridDeTrazas() {
            var trazas = this.GridDeTrazas;
            if (!Definido(trazas))
                return;
            MapearAlGrid.MapearGridDeDetalle(trazas, this.CrudDeMnt.IdNegocio, Numero(ObtenerPropiedad(this.Registro, literal.id)), this.CrudDeMnt.Guid);
        }

        public RecargarGridDeArchivadores() {
            var archivadores = this.GridDeArchivadores;
            if (archivadores.classList.contains(ltrCss.crud.contenedorDatosPrincipales))
                return;
            if (!Definido(archivadores))
                return;
            MapearAlGrid.MapearGridDeDetalle(archivadores, this.CrudDeMnt.IdNegocio, Numero(ObtenerPropiedad(this.Registro, literal.id)), this.CrudDeMnt.Guid);
        }

        public RecargarGridDeHitos() {
            MapearAlGrid.MapearGridDeDetalle(this.GridDeHitos, this.CrudDeMnt.IdNegocio, Numero(ObtenerPropiedad(this.Registro, literal.id)), this.CrudDeMnt.Guid);
        }

        private EditarSeleccionado(seleccionado: number) {
            if (this.TotalSeleccionados === 0) {
                MensajesSe.Error("EditarSeleccionado", "No hay elementos a editar.");
                this.CerrarEdicion();
            }

            if (seleccionado === 0)
                seleccionado = this.TotalSeleccionados;

            if (seleccionado > this.TotalSeleccionados)
                seleccionado = 1;

            //if (this.HayCambiosPendientes) {
            //    this.Modificar(ltrOperacion.ModificarPorId);
            //}
            this.PanelDeEditar.setAttribute(ltrEdicion.Editando, seleccionado.toString());
            this.InicializarGridDeDetalles(this.PanelDeEditar);
            this.InicializarAmpliaciones(this.PanelDeEditar);
            this.Posicionador = seleccionado;
            this.InicializarValores(this.ElementoEditado.Id);
        }

        public ConsultarSeleccionado() {
            this._guidDeConsulta = ObtenerParametroUrl(ltrParametrosUrl.guid, enumNegocio.No_Definido, false);
            this._idDeConsulta = Numero(ObtenerParametroUrl(ltrParametrosUrl.id, 0, false));

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.guid, this.GuidDeConsulta));
            ApiDePeticiones.EjecutarAccion(this, this.Controlador, Ajax.Entorno.Acceso.ValidarConsultaPorGuid, this.IdDeConsulta, parametros, new Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => {
                    this.PanelDeEditar.setAttribute(ltrEdicion.Editando, this.IdDeConsulta.toString());
                    this.InicializarGridDeDetalles(this.PanelDeEditar);
                    this.InicializarAmpliaciones(this.PanelDeEditar);
                    this.InicializarValores(this.IdDeConsulta);
                })
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        private Transitar(accion: string): void {

            if (accion === ltrEventos.Edicion.Avanzar && this.HayMasDeUnaTransicionParaAvanzar) {
                crudMnt.ModalTransitar_Abrir(this.Id, this.IdTransicionAplicable);
                return;
            }

            let parametros = this.ParametrosParaTransitar(accion);
            if (this.HayCambiosPendientes)
                this.TransitarTrasModificar(parametros);
            else
                ApiDePeticiones.Transitar(this, this.Controlador, Ajax.EndPoint.Transitar, parametros, null)
                    .then((peticion) => {
                        let crudEdicion: CrudEdicion = peticion.llamador as CrudEdicion;
                        crudEdicion.Recargar();
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected ParametrosParaTransitar(accion: string): Array<Parametro> {
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, this.CrudDeMnt.IdNegocio));
            parametros.push(new Parametro(ltrPropiedades.Elemento.IdElemento, this.Id));
            parametros.push(new Parametro(ltrPropiedades.Transiciones.Origen, this.IdEstado));
            if (accion === ltrEventos.Edicion.Retroceder)
                parametros.push(new Parametro(ltrPropiedades.Transiciones.Destino, this.IdEstadoAnterior));
            else
                parametros.push(new Parametro(Ajax.Param.idTransicion, this.IdTransicionAplicable));
            return parametros;
        }

        protected async CerrarEdicion() {
            if (this.HayCambiosPendientes) {
                const confirmar = window.confirm("Tienes cambios pendientes. ¿Estás seguro de que quieres salir sin guardar?");
                if (!confirmar) {
                    return; // Si el usuario cancela, salimos del método
                }
                if (Definido(this.ContenedorDelVisorDeArchivoConHistorial)) {
                    window.removeEventListener('resize', this.handleResize);
                }

                ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDelDto);
            }

            this.PanelDeEditar.setAttribute(ltrEdicion.Editando, literal.menos1);

            if (this.CrudDeMnt.ParaqueNavegar === enumParaQueNavegar.crear || this.CrudDeMnt.ParaqueNavegar === enumParaQueNavegar.editar) {

                var origen = this.CrudDeMnt.Estado.Obtener(ltrClaveDeEstado.paginaOrigen);
                if (!IsNullOrEmpty(origen) && origen !== this.CrudDeMnt.Pagina) {
                    let estadoPaginaAnterior: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(origen);
                    var paginaActualDeLaAnterior = estadoPaginaAnterior.Obtener(ltrClaveDeEstado.paginaActual);
                    var paginaOrigenDeLaAnterior = estadoPaginaAnterior.Obtener(ltrClaveDeEstado.paginaOrigen);
                    if (!IsNullOrEmpty(paginaOrigenDeLaAnterior) && !IsNullOrEmpty(paginaActualDeLaAnterior) && paginaActualDeLaAnterior !== paginaOrigenDeLaAnterior) {
                        estadoPaginaAnterior.Agregar(ltrClaveDeEstado.SeguirRetrocediendo, true);
                        estadoPaginaAnterior.Guardar();
                    }
                }

                EntornoSe.NavegarAtras();
            }
            else {
                this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.mantenimiento;
                if (this.EsModal) {
                    ApiPanel.CerrarModal(this.PanelDeEditar);
                    EntornoSe.AjustarDivs();
                }
                else {
                    //ApiDelCrud.OcultarPanelDeEdicion();
                    ApiDelCrud.CambiarPanelActivoDelCrud(this.CrudDeMnt.ModoTrabajo);
                    var ocultar = false;
                    if (this.CrudDeMnt.EstaElFiltroOculto()) {
                        this.CrudDeMnt.MostrarFiltro();
                        ocultar = true;
                    }
                    ApiDeMenuFlotante.CerrarMf(this.PanelDeEditar);
                }
                this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.mantenimiento;
                this.CrudDeMnt.CargarGrid().then(() => this.CrudDeMnt.EditarEnPanelDeGraficos(true));
                if (ocultar)
                    this.CrudDeMnt.OcultarFiltro();
            }
        }

        protected InicializarValores(id: number) {
            this.IdEditor.value = id.toString();
            let parametros: Array<Parametro> = this.ParametrosParaLeerElementoPorId();
            //this.SalvarCambios = false;
            ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDelDto);
            MensajesSe.Info('Leyendo ...')
            ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, parametros, id)
                .then((peticion) => {
                    this.MapearElementoDevuelto(peticion, true, true, true);
                }
                )
                .catch((peticion) => this.SiHayErrorAlLeerElemento(peticion));
        }

        private InicializarDiccionarioDeCarga(): void {
            this._diccionarioDeCarga = new Map<string, boolean>();
            this._diccionarioDeCarga.set(ltrCrud.Enumerados.Edicion.Carga.Principal, false)
            this._diccionarioDeCarga.set(ltrCrud.Enumerados.Edicion.Carga.Ampliaciones, false)
        }

        public AlmacenarValoresInicialesAsync(div: HTMLDivElement): void {
            setTimeout(() => {
                this.AlmacenarValoresIniciales(div);
                //this.SalvarCambios = this.AmpliacionesPorCargar <= 0;
            }, 2000)
        }

        public IndicarCargaRealizada(entrada: string): void {
            this._diccionarioDeCarga.set(entrada, true);

            const todasTrue = Array.from(this._diccionarioDeCarga.values()).every(value => value === true);

            if (todasTrue) {
                this.CargaCompletada();
            }
        }


        public RecargarValoresDeCabecera(id: number) {
            this.IdEditor.value = id.toString();
            let parametros: Array<Parametro> = this.ParametrosParaLeerElementoPorId();
            ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, parametros, id)
                .then((peticion) => this.MapearElementoDevuelto(peticion, false, false, false))
                .catch((peticion) => this.SiHayErrorAlLeerElemento(peticion));
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            const parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.leerPorIdParaEditar, true));
            parametros.push(new Parametro(Ajax.Parametros.ConsultarConGuid, this.PaginaDeConsultaConGuid));
            parametros.push(new Parametro(Ajax.Param.guid, this.GuidDeConsulta));
            parametros.push(new Parametro(Ajax.Param.id, this.IdDeConsulta));
            return parametros;
        }

        public ParametrosParaBorrarRelacion(): Array<Parametro> {
            const parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.BorrarRelacion, true));
            return parametros;
        }

        private MapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax, cargarGidDeDetalle: boolean, cargarAmpliaciones: boolean, cargarOtraInformacion: boolean) {
            let edicion: CrudEdicion = peticion.llamador as CrudEdicion;
            let panel = edicion.PanelDeEditar;
            const esRecargaDeDatosPrincipales = !cargarAmpliaciones && !cargarGidDeDetalle && !cargarOtraInformacion
            this.InicializarDiccionarioDeCarga();
            if (!esRecargaDeDatosPrincipales)
                edicion.IdArchivoMostrado = undefined;
            edicion.AntesDeMapearElementoDevuelto(peticion);
            let propiedades = ToLista(panel.getAttribute(atControl.NoMapeablesAlDto), ';');

            if (this.ModoDeAcceso === ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso) {
                MensajesSe.Info("El usuario conectado no tiene permiso al elemento editado");
                this.CerrarEdicion();
            }

            MapearAlPanel.ElObjeto(panel, peticion.resultado.datos, this.ModoDeAcceso, propiedades);

            if (cargarGidDeDetalle) {
                MapearAlGrid.MapearLosGridDeDetalle(edicion.PaginaDeConsultaConGuid ? edicion.CuerpoDePaginaDeConsulta : panel, edicion.IdNegocio, peticion.resultado.datos, this.PaginaDeConsultaConGuid ? this.GuidDeConsulta : this.CrudDeMnt.Guid);
                if (!this.EsGestor || Registro.EsClienteWeb())
                    edicion.ModoTrabajo = enumModoTrabajo.consultando;
                else
                    edicion.ModoTrabajo = enumModoTrabajo.editando;
            }

            var estaDeBaja = (this.EstaDeBaja || this.EstaCancelada)
            if (edicion.PermitirSubirArchivos && estaDeBaja) edicion.PermitirSubirArchivos = false;

            ModoAcceso.AplicarloAlPanel(panel,
                edicion.ModoTrabajo === enumModoTrabajo.consultando
                    ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor
                    : this.ModoDeAcceso, estaDeBaja || this.EstaTerminada);


            if (cargarAmpliaciones) {
                MapearAlPanel.MapearLasAmpliaciones(edicion,
                    edicion.IdNegocio,
                    ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Id),
                    ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConTipo.IdTipo, 0, false));
            }
            else this.IndicarCargaRealizada(ltrCrud.Enumerados.Edicion.Carga.Ampliaciones);


            ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenu, ltrMenus.enumOrigen.edicion, enumCssOpcionMenu.DeElemento, this.ModoDeAcceso);
            if (this.UsaBaja && !this.PaginaDeConsultaConGuid)
                ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.edicion, this.ContenedorMenu, estaDeBaja, edicion.ModoAccesoAlNegocio);

            if (Registro.EsClienteWeb()) ApiPanel.OcultarPanel(edicion.ContenedorMenu);

            if (cargarOtraInformacion)
                edicion.MapearOtraInformacion(peticion, estaDeBaja ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor : this.ModoDeAcceso);

            ModoAcceso.AplicarPermisosAEditores(panel, peticion.resultado.datos);
            ModoAcceso.AplicarPermisosAReferenciasPost(panel, peticion.resultado.datos);
            ModoAcceso.AplicarPermisosAReferencias(panel, peticion.resultado.datos);
            edicion.DespuesDeMapearElementoDevuelto(panel, peticion);
            edicion.IndicarCargaRealizada(ltrCrud.Enumerados.Edicion.Carga.Principal);
            edicion.AlmacenarValoresInicialesAsync(this.PanelDelDto);
        }

        public CargaSpanCompletada(span: HTMLDivElement) {
            if (span.id === this.IdDeExpansor(ltrEspanes.observaciones)) {
                // 1. Buscamos todas las filas dentro del grid que tengan la clase específica
                const filas = span.querySelectorAll('.' + ltrCss.filaDelGrid);

                // 2. Iteramos por cada fila encontrada
                filas.forEach((nodoFila) => {
                    // Convertimos el nodo a HTMLDivElement para que sea compatible con nuestra función
                    const fila = nodoFila as HTMLDivElement;

                    // 3. Llamamos a la función de conversión
                    this.convertirInputEnEnlace(fila, literal.nombre);
                });
            }
        }

        private convertirInputEnEnlace(fila: HTMLDivElement, propiedadDestino: string): void {
            const textoTitle = fila.getAttribute("title");
            if (!textoTitle) return;

            // Expresión regular para buscar la primera URL que empiece por http o https
            const regexUrl = /(https?:\/\/[^\s]+)/g;
            const coincidencias = textoTitle.match(regexUrl);

            // Si no hay coincidencias de URL, salimos
            if (!coincidencias || coincidencias.length === 0) {
                return;
            }

            const urlEncontrada = coincidencias[0]; // Tomamos la primera encontrada

            const selector = `input[propiedad="${propiedadDestino}"]`;
            const inputOriginal = fila.querySelector(selector) as HTMLInputElement;

            if (inputOriginal) {
                const anchor = document.createElement("a");

                // Configuración del enlace con la URL extraída por el Regex
                anchor.textContent = inputOriginal.value;
                anchor.href = urlEncontrada;
                anchor.target = "_blank";

                // Estilos para mantener la estética y que parezca un link
                anchor.className = inputOriginal.className;
                anchor.style.textDecoration = "underline";
                anchor.style.color = "#0056b3";
                anchor.style.textAlign = "left";
                anchor.style.width = "100%";

                // Reemplazo en el DOM
                inputOriginal.parentNode?.replaceChild(anchor, inputOriginal);
            }
        }

        public RecargarAmpliaciones() {
            MapearAlPanel.MapearLasAmpliaciones(this,
                this.CrudDeMnt.IdNegocio,
                this.Id,
                ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.ConTipo.IdTipo, 0, false));
        }

        public CargaCompletada() {
            if (!this.PaginaDeConsultaConGuid) {
                this.ResetearValoresIniciales()
                this.AplicarBloqueo();
                const urlSinParametros: string = window.location.origin + window.location.pathname;
                let url: string = `${urlSinParametros}?id=${this.ElementoEditado.Id}`;
                GuardarRegistroAccedido(this.CrudDeMnt.EnumeradoDeNegocio as string, this.CrudDeMnt.IdVista, this.ElementoEditado.Id, url);
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax): void {
            let edicion: CrudEdicion = peticion.llamador as CrudEdicion;
            ApiPanel.BlanquearControlesDeIU(edicion.PanelDelDto, false);
            edicion.Registro = peticion.resultado.datos;
            edicion.ModoDeAcceso = ModoAcceso.Parsear(peticion.resultado.modoDeAcceso);

            if (ModoAcceso.EsGestor(edicion.ModoDeAcceso) && EsTrue(edicion.PanelDeEditar.getAttribute(atControl.soloConsulta)))
                edicion.ModoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;

            this.InicializarControlesDeProceso();

            if (Registro.EsClienteWeb()) {
                this.PermitirSubirArchivos = ModoAcceso.EsGestor(edicion.ModoDeAcceso);
                edicion.ModoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;
            }
            else {
                this.PermitirSubirArchivos = ModoAcceso.EsGestor(edicion.ModoDeAcceso);
            }
            let expresion = ObtenerPropiedad(edicion.Registro, ltrPropiedades.Elemento.Expresion, "");

            if (!IsNullOrEmpty(expresion))
                edicion.Titulo = `${edicion.Titulo.split(' - ')[0]} - ${expresion}`

            this.EstaCancelada

        }

        protected InicializarControlesDeProceso(): void {
            if (this.PaginaDeConsultaConGuid) return;

            if (!this.EsDeProceso || Registro.EsClienteWeb()) {
                ApiControl.IncluirCss(this.BotonAvanzar, ltrCss.crud.panelDeEdicion.OcultarBoton);
                ApiControl.IncluirCss(this.BotonDevolver, ltrCss.crud.panelDeEdicion.OcultarBoton);
                return
            }

            if (!Definido(this.EstadoAnterior)) {
                ApiControl.DeshabilitarRef(this.BotonDevolver.parentElement as HTMLAnchorElement);
                this.BotonDevolver.title = '';
            }
            else {
                ApiControl.HabilitarRef(this.BotonDevolver.parentElement as HTMLAnchorElement);
                this.BotonDevolver.title = `Devolver al estado '${this.EstadoAnterior}'`;
            }

            if (!Definido(this.TransicionAplicable)) {
                ApiControl.DeshabilitarRef(this.BotonAvanzar.parentElement as HTMLAnchorElement);
                this.BotonAvanzar.title = '';
            }
            else {
                ApiControl.HabilitarRef(this.BotonAvanzar.parentElement as HTMLAnchorElement);
                this.BotonAvanzar.title = `${this.TransicionAplicable}`;
            }
        }

        protected MapearOtraInformacion(peticion: ApiDeAjax.DescriptorAjax, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
            if (Definido(this.PanelDeArchivos)) {
                ApiDeArchivos.MostrarArchivosAnexados(this.PanelDeArchivos.id, this.NombreDeNegocio, peticion.resultado.datos.id,
                    this.PaginaDeConsultaConGuid ? null : (peticion) => this.AlTerminarDeLeerArchivos(peticion));

                if (this.PaginaDeConsultaConGuid) {
                    ApiControl.IncluirCss(this.SelectorDeArchivos, ltrCss.crud.panelDeEdicion.ConsultaConGuid);
                }
                else {
                    if (Registro.EsClienteWeb() && this.PermitirSubirArchivos) {
                        ModoAcceso.AplicarModoAccesoAlSelectorDeArchivos(this.PanelDeEditar, ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
                        ApiControl.OcultarMostrarOpcionDeMenuPorId(this.PanelDeArchivos.id + '-' + ltrTipoControl.SelectorDeArchivos + '-' + atArchivo.Operacion.Enlazar, true);
                        ApiControl.OcultarMostrarOpcionDeMenuPorId(this.PanelDeArchivos.id + '-' + ltrTipoControl.SelectorDeArchivos + '-' + atArchivo.Operacion.Mover, true);
                        ApiControl.OcultarMostrarOpcionDeMenuPorId(this.PanelDeArchivos.id + '-' + ltrTipoControl.SelectorDeArchivos + '-' + atArchivo.Operacion.Copiar, true);
                    }
                    ApiDeArchivos.AplicarDisposicionDeArchivos(this.PanelDeArchivos);
                }
            }

        }

        public AlTerminarDeLeerArchivos(peticion: ApiDeAjax.DescriptorAjax): void {
            let archivosDto = peticion.resultado.datos as Array<any>;
            if (archivosDto.length === 0) {
                if (this.IdArchivoMostrado === 0 && this.ArchivosRenderizables === 0)
                    ApiControl.IncluirCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.SinVisor);
                return;
            }

            if (this.IdArchivoMostrado > 0)
                return;

            ApiControl.ExcluirCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.SinVisor);
            ApiControl.RemplazarCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.OcultarVisor, ltrCss.crud.panelDeEdicion.Acciones.MostrarVisor);

            if (this.MostrarVisorAlIniciar === false) {

                return;
            }

            this.OcultarHistorial();
            for (let archivo of archivosDto) {
                let nombreFichero = ObtenerPropiedad(archivo, ltrPropiedades.Elemento.Nombre);
                if (!EsRenderizable(nombreFichero)) {
                    continue;
                }
                let idArchivo: number = ObtenerPropiedad(archivo, ltrPropiedades.Elemento.Id);
                this.RenderizarArchivo(idArchivo, nombreFichero);
                break;
            }
        }

        public RenderizarElSeleccionado(idArchivo: number, nombreArchivo: string) {
            this.RenderizarArchivo(idArchivo, nombreArchivo);
        }

        public RenderizarElPrimeroRenderizable(filtro: HTMLInputElement) {
            const contenedor = document.getElementById(filtro.id.replace('-selector-de-archivos-filtrar', ''));
            const visores = contenedor.querySelectorAll<HTMLDivElement>(`div.${ltrCss.contenedorVisor}`);

            let hayvisibles = false;
            for (let visor of visores) {

                if (visor.classList.contains(ltrCss.contenedorVisorOculto))
                    continue;
                hayvisibles = true;
                const aTag = visor.querySelector('a');
                let nombreFichero = aTag.textContent;
                if (!EsRenderizable(nombreFichero)) {
                    continue;
                }

                const idArchivo: number = Numero(aTag.id.replace('refJs-', ''));
                if (this.VisorVisible)
                    this.RenderizarArchivo(idArchivo, nombreFichero);
                break;
            }

            if (!hayvisibles) {
                this.OcultameElVisor();
                return;
            }


        }
        private _renderizando = false;
        private RenderizarArchivo(idArchivo: number, nombreFichero: string): void {
            if (this._renderizando)
                return;
            this._renderizando = true;
            ApiDelCrud.RenderizarUrlsEnVisor(this.CrudDeMnt, idArchivo, nombreFichero, true)
                .then(() => {
                    this.IdDelUltimoArchivoRenderizado = idArchivo;
                    if (!this.VisorVisible) this.MostrarOcultarVisor();
                    this.LeerVinculosAl(idArchivo);
                })
                .catch(() => {
                    this.AsignarIdArchivo(0, false);
                })
                .finally(
                    () => {
                        this._renderizando = false
                    }
                );
        }


        public DespuesDeAnexarMostrarArchivo(archivoDto: any): void {
            if (this.IdArchivoMostrado > 0)
                return;

            var nombreFichero = ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Nombre);
            if (!EsRenderizable(nombreFichero))
                return;
            let idArchivo: number = ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Id);
            ApiDelCrud.RenderizarUrlsEnVisor(this.CrudDeMnt, idArchivo, nombreFichero, true);
            ApiControl.RemplazarCss(this.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.MostrarVisor, ltrCss.crud.panelDeEdicion.Acciones.OcultarVisor);
        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            let delSistema: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.DelSistema, false);
            var editor = (peticion.llamador as CrudEdicion);
            if (delSistema) {
                ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Elemento.Nombre, delSistema);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(editor.ContenedorMenu, ltrMenus.eventosDeMf.alta, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(editor.ContenedorMenu, ltrMenus.eventosDeMf.baja, ltrMenus.enumOrigen.edicion);
            }
            let nombreModificable: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.NombreModificable, true);
            if (!nombreModificable) {
                ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Elemento.Nombre, true);
            }

            let informacion: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Informacion, undefined);
            if (!IsNullOrEmpty(informacion)) MensajesSe.Info(informacion, informacion);
        }

        protected AplicarBloqueo() {
            if (this.EstaBloqueado) {
                this.BloquearIu();
            }
        }

        public Expansor_TrasCargarAmpliacion(ampliacion: HTMLDivElement): void {
            this.AlmacenarValoresInicialesAsync(ampliacion);
            this.AmpliacionesPorCargar--;
        }

        private ResetearValoresIniciales(): void {
            this.AlmacenarValoresIniciales(this.PanelDelDto);
            let ampliaciones: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`div[${atControl.esAmpliacion}="true"]`) as NodeListOf<HTMLDivElement>;
            for (let i = 0; i < ampliaciones.length; i++) {
                this.AlmacenarValoresIniciales(ampliaciones[i]);
            }
        }

        private async AlmacenarValoresIniciales(panel: HTMLDivElement) {
            let controles: NodeListOf<HTMLSelectElement> = panel.querySelectorAll('select') as NodeListOf<HTMLSelectElement>;
            for (var i = 0; i < controles.length; i++) {
                var control = controles[i];
                if (EsTrue(control.getAttribute(atListasDeElemento.Cargando))) {
                    await ApiListaDinamica.EsperarCarga(control);
                }
            }
            var valores = this.ValoresInicales.get(panel.id);
            valores = new Map<string, any>();
            ApiPanel.AlmacenarValoresDelPanel(panel, valores);
            this.ValoresInicales.set(panel.id, valores);
        }

        private SiHayErrorAlLeerElemento(peticion: ApiDeAjax.DescriptorAjax) {
            let edicion: CrudEdicion = peticion.llamador as CrudEdicion;
            if (Definido(peticion.resultado))
                MensajesSe.Error("SiHayErrorAlLeerElemento", peticion.resultado.mensaje, peticion.resultado.consola);
            else
                MensajesSe.Error("SiHayErrorAlLeerElemento", 'error al acceder a los datos', "No se ha podido resolver la petición para obtener los datos");
            if (!this.PaginaDeConsultaConGuid) {
                edicion.ModoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso;
                edicion.CrudDeMnt.BlanquearTodosLosCheck();
                edicion.CerrarEdicion();
            }
        }

        protected ModificarDatosDeLaModal(modal: HTMLDivElement) {
            let json: JSON = ApiPanel.MapearControlesDesdeElPanelAlJson(modal, MapearAlJson.Id(modal));
            let controlador = modal.getAttribute(literal.controlador);
            ApiDePeticiones.ModificarElemento(this, controlador, Ajax.EndPoint.ModificarPorId, this.CrudDeMnt.IdNegocio, json, new Array<Parametro>(), new Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesDeModificar(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public TransitarTrasModificar(parametros: Array<Parametro>) {
            let jsonDatosPrincipales: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, this.PanelDeEditar, enumModoTrabajo.editando);
            parametros.push(new Parametro(Ajax.Param.DatosPrincipales, jsonDatosPrincipales));
            let controlador = this.PanelDeEditar.getAttribute(literal.controlador);
            let ampliaciones: NodeListOf<HTMLDivElement> = this.PanelDeEditar.querySelectorAll(`div[${atControl.esAmpliacion}="true"]`) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < ampliaciones.length; i++) {
                let ampliacion: HTMLDivElement = ampliaciones[i] as HTMLDivElement;
                let bloqueDeAmpliacion = ampliacion.parentNode.parentNode as HTMLDivElement;
                if (!bloqueDeAmpliacion.classList.contains(ltrCss.divNoVisible)) {
                    let jsonAmpliacion: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, ampliacion, enumModoTrabajo.editando);
                    parametros.push(new Parametro(ampliaciones[i].getAttribute(ltrAmpliaciones.tipoDto), jsonAmpliacion));
                }
            }
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrOperacion.nameOf, ltrOperacion.ModificoParaTransitar));
            ApiDePeticiones.ProcesarPeticion(this, controlador, this.CrudDeMnt.IdNegocio, ltrOperacion.ModificoParaTransitar, parametros, datosDeEntrada)
                .then((peticion) => {
                    let crudEdicion: CrudEdicion = peticion.llamador as CrudEdicion;
                    crudEdicion.Recargar();
                    crudEdicion.CrudDeMnt.ModalTransitar_Cerrar();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public Modificar(operacion: string, parametros: Array<Parametro> = null, datosDeEntrada: Array<Parametro> = null) {
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, this.PanelDeEditar, enumModoTrabajo.editando);
            let controlador = this.PanelDeEditar.getAttribute(literal.controlador);
            if (!Definido(datosDeEntrada)) datosDeEntrada = new Array<Parametro>();
            {
                datosDeEntrada.push(new Parametro(ltrOperacion.nameOf, operacion));
            }

            if (parametros === null) parametros = new Array<Parametro>();
            parametros.push(new Parametro(ltrOperacion.ModificoParaTransitar, operacion === ltrOperacion.ModificoParaTransitar));
            parametros.push(new Parametro(ltrOperacion.ModificoParaImprimir, operacion === ltrOperacion.ModificoParaImprimir));
            let valores = this.ValoresInicales.get(this.PanelDelDto.id);
            datosDeEntrada.push(new Parametro(ltrOperacion.HayCambios, !Definido(valores) ? true : ApiPanel.HayCambios(this.PanelDelDto, valores)));
            ApiDePeticiones.ModificarElemento(this, controlador, Ajax.EndPoint.ModificarPorId, this.CrudDeMnt.IdNegocio, json, parametros, datosDeEntrada)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesDeModificar(peticion, null))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected DespuesDeModificar(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement) {
            let crudEdicion: CrudEdicion = peticion.llamador as CrudEdicion;
            this.AlmacenarValoresIniciales(crudEdicion.PanelDelDto);
            crudEdicion.CrudDeMnt.QuitarRegistroLeido(crudEdicion.ElementoEditado.Id);
            if (Definido(modal)) {
                crudEdicion.TrasModificarConLaModal(peticion, modal);
            }
            else {
                var operacion = ObtenerPropiedad(peticion.DatosDeEntrada, ltrOperacion.nameOf, undefined);
                var accionTrasGuardar = ObtenerPropiedad(peticion.DatosDeEntrada, ltrOperacion.AccionTrasGuardar, undefined);
                var accionParaInvocar = undefined;
                if (Definido(accionTrasGuardar)) {
                    accionParaInvocar = accionTrasGuardar;
                }
                else if (operacion === ltrOperacion.ModificoParaImprimir) {
                    accionParaInvocar = () => crudEdicion.CrudDeMnt.ModalImprimir_Imprimir(false);
                }
                else if (operacion === ltrOperacion.ModificoParaTransitar) {
                    accionParaInvocar = () => {
                        crudEdicion.Recargar();
                        crudEdicion.CrudDeMnt.ModalTransitar_Cerrar();
                    }
                }
                else if (crudEdicion.TotalSeleccionados === 1) {
                    accionParaInvocar = () => {
                        crudEdicion.CerrarEdicion();
                    }
                }
                else {
                    accionParaInvocar = () => {
                        crudEdicion.EjecutarAcciones(ltrEventos.Edicion.MostrarSiguiente, this.PanelDeEditar);
                    }
                }


                this.PersistirAmpliaciones(crudEdicion, accionParaInvocar);
            }
        }

        private PersistirAmpliaciones(crudEdicion: CrudEdicion, accionTrasFinalizar: Function) {
            let ampliaciones: NodeListOf<HTMLDivElement> = crudEdicion.PanelDeEditar.querySelectorAll(`div[${atControl.esAmpliacion}="true"]`) as NodeListOf<HTMLDivElement>;
            crudEdicion.PersistirAmpliacion(crudEdicion, ampliaciones, 0, accionTrasFinalizar);
        }

        private PersistirAmpliacion(crudEdicion: CrudEdicion, ampliaciones: NodeListOf<HTMLDivElement>, indice: number, accionTrasFinalizar: Function) {
            if (ampliaciones.length > indice) {
                let ampliacion: HTMLDivElement = ampliaciones[indice] as HTMLDivElement;
                let bloqueDeAmpliacion = ampliacion.parentNode.parentNode as HTMLDivElement;
                let valores = this.ValoresInicales.get(ampliacion.id);
                let hayCambios = !Definido(valores) ? true : ApiPanel.HayCambios(ampliacion, valores);
                if (!bloqueDeAmpliacion.classList.contains(ltrCss.divNoVisible) && hayCambios) {
                    let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(crudEdicion, ampliacion, enumModoTrabajo.editando);
                    let controlador = ampliacion.getAttribute(ltrAmpliaciones.controlador);
                    ApiDePeticiones.PersistirAmpliacion(crudEdicion, controlador, Ajax.EndPoint.PersistirAmpliacion, crudEdicion.CrudDeMnt.IdNegocio, json)
                        .then((peticion: ApiDeAjax.DescriptorAjax) => {
                            this.AlmacenarValoresIniciales(ampliacion);
                            crudEdicion.PersistirAmpliacion(crudEdicion, ampliaciones, indice + 1, accionTrasFinalizar)
                        })
                        .catch(
                            (peticion) => {
                                this.ResetearValoresIniciales();
                                ApiDePeticiones.EmitirError(peticion)
                            }
                        );
                }
                else
                    crudEdicion.PersistirAmpliacion(crudEdicion, ampliaciones, indice + 1, accionTrasFinalizar);
            }
            else
                accionTrasFinalizar();
        }

        public TrasModificarConLaModal(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement) {
            if (modal.getAttribute(atControl.tipoModal) === enumTipoDeModal.ModalDeVisorDeArchivos)
                ApiDeArchivos.MostrarNuevoNombre(modal);
            ApiPanel.CerrarModal(modal);
        }

        public Expansor_AbrirModalParaVincular(idModal: string, grabar: boolean = true): void {
            if (this.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.Expansor_AbrirModalParaVincular(idModal, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('Expansor_AbrirModalParaVincular', `la modal para vincular '${idModal}' no está definida`);
            ApiPanel.BlanquearControlesDeIU(modal);
            //modal.setAttribute(atModal.idNegocioVinculado, idNegocioPorVincular.toString());
            modal.setAttribute(atModal.idElemento1, this.ElementoEditado.Id.toString());
            ApiPanel.AbrirModal(modal);
        }

        public Expansor_AbrirModalDeCrearVinculo(idModal: string, idNegocioPorVincular: number, idElemento: number): void {
            if (idNegocioPorVincular === 0)
                MensajesSe.EmitirExcepcion("Expansor_AbrirModalDeCrearVinculo", "Debe indicar el id del negocio con el que vincular");

            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('Expansor_AbrirModalDeCrearVinculo', `la modal de crear vinculos '${idModal}' no está definida`);
            ApiPanel.BlanquearControlesDeIU(modal);
            modal.setAttribute(atModal.idNegocioVinculado, idNegocioPorVincular.toString());
            modal.setAttribute(atModal.idElemento1, idElemento === 0 ? this.ElementoEditado.Id.toString() : idElemento.toString());
            ApiPanel.AbrirModal(modal);
        }

        public Expansor_AbrirModalDeVincular(nombreDadoEnElDescriptor: string): void {
            let idModal = this.IdModalDeVincular(nombreDadoEnElDescriptor);
            this.Expansor_AbrirModalDeCrearVinculoCon(idModal, 0);
        }

        public Expansor_AbrirModalDeCrearVinculoCon(idModal: string, idElemento: number, grabar: boolean = true): void {
            if (this.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.Expansor_AbrirModalDeCrearVinculoCon(idModal, idElemento, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('Expansor_AbrirModalDeCrearVinculoCon', `la modal de crear vinculos con '${idModal}' no está definida`);
            ApiPanel.BlanquearControlesDeIU(modal);
            modal.setAttribute(atModal.idElemento1, idElemento === 0 ? this.ElementoEditado.Id.toString() : idElemento.toString());
            ApiPanel.AbrirModal(modal);
        }

        public Expansor_AbrirModalDeCrearDetalle(idModal: string, idElemento: number, grabar: boolean = true): void {
            if (this.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.Expansor_AbrirModalDeCrearDetalle(idModal, idElemento, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('Expansor_AbrirModalDeCrearDetalle', `la modal de crear detalle con '${idModal}' no está definida`);
            ApiPanel.BlanquearControlesDeIU(modal);
            modal.setAttribute(atModal.idElemento1, idElemento === 0 ? this.ElementoEditado.Id.toString() : idElemento.toString());
            ApiPanel.AbrirModal(modal);
        }

        public Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion: string, propiedadesRestrictoras: string, grabar: boolean = true): void {
            if (this.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => this.Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion, propiedadesRestrictoras, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }

            if (this.CrudDeMnt.ModoTrabajo === enumModoTrabajo.mantenimiento) {
                if (this.CrudDeMnt.InfoSelector.Cantidad !== 1)
                    MensajesSe.Error('Expansor_AbrirModalDeRelacionParaCrear', 'Debe seleccionar un sólo elemento para poder relacionarle elementos')
                else {
                    ApiDelCrud.AbrirModalDeRelacionParaCrear(idModalDeCreacion,
                        propiedadesRestrictoras,
                        this.CrudDeMnt.InfoSelector.Seleccionados[0].Id,
                        this.CrudDeMnt.InfoSelector.Seleccionados[0].Texto, this.CrudDeMnt.IdNegocio, this.CrudDeMnt.NombreDeNegocio);
                }
            }
            else
                ApiDelCrud.AbrirModalDeRelacionParaCrear(idModalDeCreacion, propiedadesRestrictoras, this.ElementoEditado.Id, this.ElementoEditado.Texto, this.CrudDeMnt.IdNegocio, this.CrudDeMnt.NombreDeNegocio);
        }

        public Expansor_AbrirModalParaPedirDatos(idModal: string, idElemento: number, resultado: ApiDeAjax.ResultadoJson = undefined, noMapear: Array<string> = new Array<string>(), grabar: boolean = true): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('Expansor_AbrirModalParaPedirDatos', `la modal para pedir datos '${idModal}' no está definida`);

            if (this.HayCambiosPendientes && grabar) {

                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar,
                    () => this.Expansor_AbrirModalParaPedirDatos(idModal, idElemento, resultado, noMapear, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }

            ApiPanel.BlanquearControlesDeIU(modal);
            modal.setAttribute(atModal.idElemento1, idElemento.toString());
            if (Definido(resultado) && Definido(resultado.datos)) MapearAlPanel.ElObjeto(modal, resultado.datos, ModoAcceso.Parsear(resultado.modoDeAcceso), noMapear, false);
            ApiPanel.AbrirModal(modal);
        }

        public Expansor_TrasAbrirModal(idModal: string, acciones: string): HTMLDivElement {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            if (Definido(acciones) && (acciones.indexOf(ltrAccionesModal.ProponerCg) >= 0 || acciones.indexOf(ltrAccionesModal.BloquearCg) >= 0)) {
                if (this.CrudDeMnt.ModoTrabajo === enumModoTrabajo.mantenimiento) {
                    if (!ExistePropiedad(this.CrudDeMnt.InfoSelector.Seleccionados[0].Registro, literal.idCg)) return;
                    this.MapearCGDesdeMantenimiento(modal, this.CrudDeMnt.InfoSelector.Seleccionados[0].Registro, acciones.indexOf(ltrAccionesModal.BloquearCg) >= 0);
                }
                else {
                    if (!ExistePropiedad(this.Registro, literal.idCg)) return;
                    this.MapearCGDesdeEdicion(this.PanelDeEditar, modal, this.Registro, acciones.indexOf(ltrAccionesModal.BloquearCg) >= 0);
                }
            }

            if (modal.getAttribute(atControl.tipoModal) === enumTipoDeModal.ModalDeCrearDetalle) {
                ApiDelCrud.MapearElementoEnModalDeDetalle(this, modal);
            }

            return modal;
        }

        public Expansor_TrasCargarDetalle(idGrid: string) {
            if (!ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso))
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);
        }

        protected MapearCGDesdeMantenimiento(panelDestino: HTMLDivElement, registro: any, bloquear: boolean) {
            let idCg: number = ObtenerPropiedad(registro, literal.idCg, 0);
            let cg: string = ObtenerPropiedad(registro, literal.Cg, "");
            let cgDeEdicion: HTMLInputElement = ApiControl.BuscarControl(panelDestino, literal.Cg, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(cgDeEdicion, idCg, cg, bloquear);
        }

        protected MapearCGDesdeEdicion(panelOrigen: HTMLDivElement, panelDestino: HTMLDivElement, registro: any, bloquear: boolean) {
            ApiDelCrud.ProponerPropiedad(panelOrigen, panelDestino, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true);
            let cgDeDestino: HTMLInputElement = ApiControl.BuscarControl(panelDestino, literal.Cg, false) as HTMLInputElement;
            if (NoDefinido(cgDeDestino))
                ApiDelCrud.ProponerEnRestrictor(panelDestino, registro, literal.Cg, literal.idCg);
            else
                ApiDelCrud.ProponerPropiedad(panelOrigen, panelDestino, literal.Cg, bloquear);
        }

        public Expansor_AbrirModalDeRelacionParaEditar(idGridDeDetalle: string, propiedadesRestrictoras: string, numeroFila: number, grabar: boolean = true): void {
            if (this.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar,
                    () => this.Expansor_AbrirModalDeRelacionParaEditar(idGridDeDetalle, propiedadesRestrictoras, numeroFila, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }
            let idDeLaRelacion: number = this.Expansor_ObtenerIdDeLaRelacion(idGridDeDetalle, numeroFila);

            let gridDeDetalle: HTMLDivElement = document.getElementById(idGridDeDetalle) as HTMLDivElement;
            let controlador: string = gridDeDetalle.getAttribute(atGridDeDetalle.controlador);

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, gridDeDetalle.id));
            datosDeEntrada.push(new Parametro('propiedadesRestrictoras', propiedadesRestrictoras));

            let idNegocio: number = Numero(gridDeDetalle.getAttribute(literal.idNegocio));
            let parametros: Array<Parametro> = this.ParametrosParaLeerElementoPorId();
            if (idNegocio > 0) {
                if (parametros.length === 0 || NoDefinido(parametros.find(parametro => parametro.parametro === ltrPropiedades.Negocio.idNegocio)))
                    parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, idNegocio));
            }

            ApiDePeticiones.LeerElementoPorId(this, controlador, idDeLaRelacion, parametros, datosDeEntrada)
                .then((peticion) => this.Expansor_MapearRelacion(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public Expansor_BorrarRelacion(idGridDeDetalle: string, numeroFila: number, accionDeBorrado: string, grabar: boolean = true): void {
            if (this.HayCambiosPendientes && grabar) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar,
                    () => this.Expansor_BorrarRelacion(idGridDeDetalle, numeroFila, accionDeBorrado, false)));
                this.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
                return;
            }
            ApiDeExpansor.BorrarRelacion(this, idGridDeDetalle, numeroFila, accionDeBorrado);
        }

        public Expansor_DarDeAlta(idGridDeDetalle: string, numeroFila: number) {
            let idDelRegistro: number = this.Expansor_ObtenerIdDeLaRelacion(idGridDeDetalle, numeroFila);

            let gridDeDetalle: HTMLDivElement = document.getElementById(idGridDeDetalle) as HTMLDivElement;
            let controlador: string = gridDeDetalle.getAttribute(atGridDeDetalle.controlador);

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, gridDeDetalle.id));
            datosDeEntrada.push(new Parametro(atControl.idDelElemento, this.ElementoEditado.Id));

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, gridDeDetalle.getAttribute(atGridDeDetalle.idNegocio)));

            ApiDePeticiones.EjecutarAccion(this, controlador, Ajax.EndPoint.DarDeAlta, idDelRegistro, parametros, datosDeEntrada)
                .then((peticion) => this.Expansor_RecargarGridDeRelacion(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public Expansor_DarDeBaja(idGridDeDetalle: string, numeroFila: number) {
            let idDelRegistro: number = this.Expansor_ObtenerIdDeLaRelacion(idGridDeDetalle, numeroFila);

            let gridDeDetalle: HTMLDivElement = document.getElementById(idGridDeDetalle) as HTMLDivElement;
            let controlador: string = gridDeDetalle.getAttribute(atGridDeDetalle.controlador);

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, gridDeDetalle.id));
            datosDeEntrada.push(new Parametro(atControl.idDelElemento, this.ElementoEditado.Id));

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, Numero(gridDeDetalle.getAttribute(atGridDeDetalle.idNegocio))));

            ApiDePeticiones.EjecutarAccion(this, controlador, Ajax.EndPoint.DarDeBaja, idDelRegistro, parametros, datosDeEntrada)
                .then((peticion) => this.Expansor_RecargarGridDeRelacion(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public Expansor_ObtenerIdDeLaRelacion(idGridDeDetalle: string, numeroFila: number): number {
            let idDeLaFila = `${idGridDeDetalle.replace('-contenedor', '-tabla_d_tr')}_${numeroFila}`;
            let idDeLaRelacion: number = 0;
            try {
                let id = ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, atControl.id);
                idDeLaRelacion = Numero(id);
                if (idDeLaRelacion === 0) {
                    throw new Error(`No es válido el id: ${id} de la fila ${idDeLaFila}`);
                }
            }
            catch (error) {
                MensajesSe.EmitirExcepcion("Borrar relación", 'No se ha podido obtener el Id de la relación', error);
            }
            return idDeLaRelacion;
        }

        public Expansor_ObtenerPropiedad(idGridDeDetalle: string, numeroFila: number, propiedad: string): string {
            let idDeLaFila = `${idGridDeDetalle.replace('-contenedor', '-tabla_d_tr')}_${numeroFila}`;
            let valor = ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, propiedad);
            return valor;
        }

        protected Expansor_DespuesDeBorrarRelacion(peticion: ApiDeAjax.DescriptorAjax): void {
            let edicion = peticion.llamador as CrudEdicion;
            edicion.Expansor_RecargarGridDeRelacion(peticion)
        }

        private Expansor_RecargarGridDeRelacion(peticion: ApiDeAjax.DescriptorAjax): any {
            let edicion = peticion.llamador as CrudEdicion;
            let idGrid: string = peticion.DatosDeEntrada[0].valor;
            let idDelElemento: number = Definido(peticion.DatosDeEntrada.find(x => x.parametro === atControl.idDelElemento))
                ? peticion.DatosDeEntrada.find(x => x.parametro === atControl.idDelElemento).valor
                : 0;

            let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
            this.RecargarGridDeRelacion(grid, edicion.CrudDeMnt.IdNegocio, idDelElemento);
        }

        public Expansor_MapearRelacion(peticion: ApiDeAjax.DescriptorAjax): any {
            let edicion = peticion.llamador as CrudEdicion;
            let idGrid: string = peticion.DatosDeEntrada[0].valor;
            let propiedadesRestrictoras: string = peticion.DatosDeEntrada[1].valor;
            let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
            let idModalEdicion = grid.getAttribute(atGridDeDetalle.modalParaEditarRelacion);
            let modalDeEdicion: HTMLDivElement = document.getElementById(idModalEdicion) as HTMLDivElement;


            let modoDeAcceso = ObtenerPropiedad(peticion.resultado.datos, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);

            if (edicion.ModoTrabajo === enumModoTrabajo.consultando)
                modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;

            if (Definido(edicion.ModalParaEditarEventoDeAgenda) && modalDeEdicion.id === edicion.ModalParaEditarEventoDeAgenda.id) {
                let esDelSistema: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.EventoDeAgenda.EsDelSistema);
                if (esDelSistema) modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;
            }

            ApiPanel.AbrirModal(modalDeEdicion);
            let usaBloqueo = ExistePropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Bloqueado);
            if (usaBloqueo) ApiPanel.OcultarFila(modalDeEdicion, ltrPropiedades.Elemento.Bloqueado);

            let usaBaja = ExistePropiedad(peticion.resultado.datos, ltrPropiedades.baja);
            let estaDeBaja = usaBaja ? ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.baja, false) : false;
            let estaBloqueado = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Bloqueado, false)
            MapearAlPanel.ElObjeto(modalDeEdicion, peticion.resultado.datos, modoDeAcceso);
            this._objetosDeUnExpansor.Agregar(idModalEdicion, peticion.resultado.datos);


            ModoAcceso.AplicarloAlPanel(modalDeEdicion, modoDeAcceso, estaDeBaja || estaBloqueado);
            ApiPanel.BloquearControlesPorPropieda(modalDeEdicion, propiedadesRestrictoras);
            edicion.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso)
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            let delSistema: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.DelSistema, false);
            let nombreModificable: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.NombreModificable, true);
            if (delSistema || !nombreModificable) {
                ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Elemento.Nombre, true);
            }
        }

        public Expansor_NavegarDesdeEdicion(url: string, paginaDestino: string): void {
            this.CrudDeMnt.Estado.Agregar(ltrClaveDeEstado.EditarAlVolver, true);
            this.CrudDeMnt.Estado.Agregar(atGrid.id, this.CrudDeMnt.Navegador.Datos);
            this.CrudDeMnt.Estado.Agregar(ltrClaveDeEstado.ElementosSeleccionados, this.CrudDeMnt.InfoSelector.Seleccionados);
            this.CrudDeMnt.Estado.Guardar();

            let datos: Tipos.Restrictor[] = [];
            let negocio: Tipos.Restrictor = new Tipos.Restrictor('idnegocio', this.CrudDeMnt.IdNegocio, this.CrudDeMnt.NombreDeNegocio);
            let elemento: Tipos.Restrictor = new Tipos.Restrictor('idelemento', this.ElementoEditado.Id, this.ElementoEditado.Texto);

            datos.push(negocio);
            datos.push(elemento);
            EntornoSe.PushRestrictores(paginaDestino, datos);
            EntornoSe.PushPaginaOrigen(paginaDestino, this.CrudDeMnt.Pagina);
            EntornoSe.NavegarAUrl(url);
        }

        public Expansor_NavegarAEditar(idGrid: string, pagina: string, propiedadRrestrictora: string, fila: number): void {
            let url = '';
            if (pagina.indexOf(ltrPaginas.Callejero.googleMaps) >= 0) {
                url = ApiDeInicializacion.ComponerDireccion(idGrid, fila);
            }
            else {
                url = `${window.location.origin}/${pagina}`;
                if (pagina.indexOf(literal.id) > 0) {
                    let parametrosUrl = pagina.substring(pagina.indexOf('?'));
                    propiedadRrestrictora = ObtenerParametroDeUnaUrl(url, propiedadRrestrictora, '', true);
                    let valor: number = Numero(ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, propiedadRrestrictora));
                    url = url.replace(propiedadRrestrictora, valor.toString());
                }
                else {
                    let valor: number = Numero(ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, propiedadRrestrictora));
                    url = `${url}${(pagina.indexOf('?') > 0 ? '&' : '?')}${propiedadRrestrictora}=${valor}`;
                }
            }
            EntornoSe.AbrirPestana(url);
        }

        public Expansor_MostrarPropiedad(idGrid: string, fila: number, propiedad: string, titulo:string): void {
            const valor = this.Expansor_ObtenerPropiedad(idGrid, fila, propiedad);

            // Usamos /\|/g para que reemplace todas las ocurrencias, no solo la primera
            const valorFormateado = valor ? valor.replace(/\|/g, '<br>') : '';

            // 1. Crear el nodo
            const modalOverlay = document.createElement('div');
            modalOverlay.className = ltrCss.crud.modal.dinamica;

            // 2. HTML con el botón a la izquierda
            modalOverlay.innerHTML = `
             <div>
                 <h2>${titulo}</h2>
                 
                 <div class="${ltrCss.crud.modal.cuerpo}">
                     <strong>${valorFormateado}</strong>
                 </div>
         
                 <div class="${ltrCss.crud.modal.pie}">
                     <button class="${ltrCss.crud.modal.boton}" id="btn-cerrar-dinamico">
                         Cerrar
                     </button>
                 </div>
             </div>
         `;

            document.body.appendChild(modalOverlay);

            // 3. Animación de entrada
            setTimeout(() => modalOverlay.classList.add('fade-in'), 10);

            // 4. Lógica de cierre
            const cerrar = () => {
                modalOverlay.classList.remove('fade-in');
                setTimeout(() => modalOverlay.remove(), 300);
            };

            modalOverlay.querySelector('#btn-cerrar-dinamico')?.addEventListener('click', cerrar);

            modalOverlay.onclick = (e: MouseEvent) => {
                if (e.target === modalOverlay) cerrar();
            };
        }

        public Vincular(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            let json: JSON = ApiPanel.MapearControlesDesdeElPanelAlJson(modal, JSON.parse(`{}`));
            let controlador: string = modal.getAttribute(atControl.controlador);
            let idElemento1 = Numero(modal.getAttribute(atModal.idElemento1));
            let idNegocioVinculado = Numero(modal.getAttribute(atModal.idNegocioVinculado));

            ApiDePeticiones.Vincular(this, controlador, this.CrudDeMnt.IdNegocio, idNegocioVinculado, idElemento1, json, new Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesVincular(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public CrearVinculo(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, modal, enumModoTrabajo.creando);
            let controlador: string = modal.getAttribute(atControl.controlador);
            let idElemento1 = Numero(modal.getAttribute(atModal.idElemento1));
            let idNegocioVinculado = Numero(modal.getAttribute(atModal.idNegocioVinculado));

            ApiDePeticiones.CrearVinculo(this, controlador, this.CrudDeMnt.IdNegocio, idNegocioVinculado, idElemento1, json, new Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesVincular(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public CrearDetalle(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, modal, enumModoTrabajo.creando);
            let controlador: string = modal.getAttribute(atControl.controlador);
            let accion: string = modal.getAttribute(atModal.accion);
            let idElemento1 = Numero(modal.getAttribute(atModal.idElemento1));

            if (!Definido(accion))
                ApiDePeticiones.CrearDetalle(this, controlador, Ajax.EndPoint.CrearDetalle, this.CrudDeMnt.IdNegocio, json, new Array<Parametro>())
                    .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesVincular(peticion, modal))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            else {
                var parametros = new Array<Parametro>();
                parametros.push(new Parametro(ltrPropiedades.Elemento.Elemento, json))
                ApiDePeticiones.CrearDependiente(this, controlador, accion, this.CrudDeMnt.IdNegocio, idElemento1, parametros)
                    .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesVincular(peticion, modal))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
        }

        public CrearRelacion(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, modal, enumModoTrabajo.creando);
            let controlador: string = modal.getAttribute(atModal.controlador);
            let accion: string = modal.getAttribute(atModal.accion);

            ApiDePeticiones.CreaRelacion(this, controlador, Definido(accion) ? accion : Ajax.EndPoint.CrearRelacion, this.CrudDeMnt.IdNegocio, json)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesVincular(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public ModificarRelacion(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, modal, enumModoTrabajo.editando);
            let controlador: string = modal.getAttribute(atControl.controlador);

            ApiDePeticiones.ModificarRelacion(this, controlador, Ajax.EndPoint.ModificarRelacion, this.CrudDeMnt.IdNegocio, json)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesDeModificarRelacion(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public CerrarRelacion(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

            ApiPanel.BlanquearControlesDeIU(modal, false);
            ApiPanel.OcultarModal(modal);
        }

        private DespuesVincular(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): any {
            let edicion: CrudEdicion = peticion.llamador as CrudEdicion;

            //blanquear los controles de la interface
            ApiPanel.BlanquearControlesDeIU(modal);

            //recargar el grid de relaciones del expansor
            if (edicion.CrudDeMnt.ModoTrabajo !== enumModoTrabajo.mantenimiento) {
                let idGrid: string = modal.getAttribute(atGridDeDetalle.gridDeRelacionAsociado);
                let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
                let campoRestrictor: string = grid.getAttribute(atGridDeDetalle.campoRestrictor);
                this.RecargarGridDeRelacion(grid, edicion.CrudDeMnt.IdNegocio, ObtenerCampoRestrictor(edicion.ElementoEditado.Registro, campoRestrictor));
            }

            if (peticion.nombre === Ajax.EndPoint.CrearRelacion) {
                let accion = modal.getAttribute(atModal.trasAceptar);
                if (Definido(accion))
                    Evaluar('CrudEdicion.DespuesVincular', accion, accion.includes('this') ? modal : undefined);
            }

            //si no hay que seguir creando cerrar la ventana modal
            ApiPanel.OcultarModal(modal);

            // ver si hay que seguir creando
            let check: HTMLInputElement = document.getElementById(`${modal.id}-crear-mas`) as HTMLInputElement;
            if (!check.checked)
                ApiPanel.AbrirModal(modal);

        }

        private DespuesDeModificarRelacion(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): any {
            let edicion: CrudEdicion = peticion.llamador as CrudEdicion;

            //blanquear los controles de la interface
            ApiPanel.BlanquearControlesDeIU(modal);
            ApiPanel.OcultarModal(modal);

            if (peticion.nombre === Ajax.EndPoint.ModificarRelacion) {
                let accion = modal.getAttribute(atModal.trasModificar);
                if (Definido(accion))
                    Evaluar('CrudEdicion.DespuesDeModificarRelacion', accion, accion.includes('this') ? modal : undefined);
            }

            //recargar grid de archivos si se ha modificado
            if (peticion.Url.includes(ltrControladores.Comunes.Observaciones) && Numero(peticion.DatosDeEntrada['idarchivo']) > 0) {
                ApiDeArchivos.MostrarArchivosAnexados(edicion.PanelDeArchivos.id, edicion.CrudDeMnt.NombreDeNegocio, edicion.Id);
            }

            //recargar el grid de relaciones del expansor
            let idGrid: string = modal.getAttribute(atGridDeDetalle.gridDeRelacionAsociado);
            let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
            let campoRestrictor: string = grid.getAttribute(atGridDeDetalle.campoRestrictor);
            this.RecargarGridDeRelacion(grid, edicion.CrudDeMnt.IdNegocio, ObtenerCampoRestrictor(edicion.ElementoEditado.Registro, campoRestrictor));
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            MapearAlGrid.MapearGridDeDetalle(grid, idnegocio, id, this.CrudDeMnt.Guid);

            if (grid.id === this.IdGridDelExpansor(ltrEspanes.direcciones)) {
                this.RecargarGridDeTrazas();
            }
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            if (!esContextual) {
                let ids = new Array<number>();
                ids.push(this.ElementoEditado.Id);
                parametros.push(new Parametro(Ajax.Param.ids, ids));
                datosDeEntrada.push(new Parametro(Ajax.Param.ids, ids));

                let textos = new Array<string>();
                textos.push(this.ElementoEditado.Texto);
                datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.Textos, textos));
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {

            if (super.DespuesDeProcesarOpcionMf(peticion)) {
                let editor: CrudEdicion = peticion.llamador as CrudEdicion;
                editor.Recargar();
                return true;
            }

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);

            if (opcion == ltrMenus.eventosDeMf.Comun.Imprimir) {
                if (ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.SisDoc.PlantillasDisponibles.Abrir) === false) {
                    let funcion = this.IdArchivoMostrado == 0 ? (peticion) => this.AlTerminarDeLeerArchivos(peticion) : null;
                    ApiDeArchivos.MostrarArchivosAnexados(this.PanelDeArchivos.id, this.CrudDeMnt.NombreDeNegocio, this.Id, funcion);
                    return true;
                }
                crudMnt.ModalImprimir_Abrir(this.Registro, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.SisDoc.PlantillasDisponibles.Plantillas));
                return true;
            }
            return false;
        }

        public BloquearDesbloquear() {
            this.ValidarBloqueoDesbloqueo();
            var bloquear = this.CheckDeBloqueado.checked
            if (bloquear) {
                this.CheckDeBloqueado.checked = false;
                if (this.HayCambiosPendientes) {
                    MensajesSe.EmitirExcepcion("ValidarBloqueoDesbloqueo", "Antes de bloquear debe almacenar los cambios del registro editado");
                }
                this.CheckDeBloqueado.checked = true;
            }
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.idElemento, this.ElementoEditado.Id));
            parametros.push(new Parametro(Ajax.Param.Bloquear, bloquear));
            parametros.push(new Parametro(Ajax.Param.RowVersion, ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.RowVersion)));
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Bloquear, parametros, new Array<Parametro>(), true)
                .then((peticion) => {
                    this.DespuesDeBloquearDesbloquear(peticion);
                    this.RecargarGridDeTrazas();
                })
                .catch((peticion) => {
                    this.CheckDeBloqueado.checked = !this.CheckDeBloqueado.checked;
                    this.SiHayErrorAlLeerElemento(peticion)
                });
        }

        protected DespuesDeBloquearDesbloquear(peticion: ApiDeAjax.DescriptorAjax) {
            var editor = peticion.llamador as CrudEdicion;
            editor.RecargarValoresDeCabecera(editor.ElementoEditado.Id);
        }

        protected BloquearIu() {
            ModoAcceso.AplicarloAlPanel(this.PanelDeEditar, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
            if (this.EsGestor)
                ApiControl.BloquearCheck(this.CheckDeBloqueado, false);
            ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.baja, ltrMenus.enumOrigen.edicion);
        }

        protected DesbloquearIu() {
            ModoAcceso.AplicarloAlPanel(this.PanelDeEditar, this.ModoDeAcceso, false);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.baja, ltrMenus.enumOrigen.edicion);
        }

        private ValidarBloqueoDesbloqueo(): void {
            if (!this.CrudDeMnt.EstoyEditando)
                MensajesSe.EmitirExcepcion("ValidarBloqueoDesbloqueo", "El elemento debe estar en edición");

            if (this.EstaBloqueado && Registro.UsuarioConectado().login != this.QuienBloqueo && !ModoAcceso.EsInterventor(this.ModoDeAcceso)) {
                MensajesSe.EmitirExcepcion("ValidarBloqueoDesbloqueo", "Para poder desbloquear necesita permisos de interventor");
            }

            if (!this.EstaBloqueado && !ModoAcceso.EsGestor(this.ModoDeAcceso)) {
                MensajesSe.EmitirExcepcion("ValidarBloqueoDesbloqueo", "Para poder bloquear necesita permisos de gestor");
            }
        }

        public SiguienteArchivo(): void {
            const { divVisor, yaNoEsta } = this.PosicionarseEnArchivoMostrado();
            if (!Definido(divVisor))
                return;


            let buscarVisorVisible = true;
            let siguienteDiv: HTMLDivElement = divVisor;
            while (buscarVisorVisible) {
                siguienteDiv = siguienteDiv.nextElementSibling as HTMLDivElement;

                // Si no hay siguiente, obtén el primer hijo
                if (!siguienteDiv) {
                    siguienteDiv = divVisor.parentElement.firstElementChild as HTMLDivElement;
                }

                if (!siguienteDiv.classList.contains(ltrCss.contenedorVisorOculto) || divVisor === siguienteDiv)
                    buscarVisorVisible = false;
            }

            // Si el siguiente (o el primero) es el mismo que el actual
            if (!yaNoEsta && siguienteDiv === divVisor) {
                return;
            }


            var idArchivo = Numero(siguienteDiv.id.replace('visor-del-archivo-', ''));
            var ref = siguienteDiv.querySelector('#refJs-' + idArchivo.toString()) as HTMLAnchorElement;
            // Devuelve el id del siguiente div
            ApiDelCrud.RenderizarUrlsEnVisor(this.CrudDeMnt, idArchivo, ref.text, false);
            this.IdDelUltimoArchivoRenderizado = idArchivo;
            this.LeerVinculosAl(idArchivo);
        }

        public AnteriorArchivo(): void {
            const { divVisor, yaNoEsta } = this.PosicionarseEnArchivoMostrado();
            if (!Definido(divVisor))
                return;

            let buscarVisorVisible = true;
            let anteriorDiv: HTMLDivElement = divVisor;
            while (buscarVisorVisible) {
                anteriorDiv = anteriorDiv.previousElementSibling as HTMLDivElement;

                if (!anteriorDiv) {
                    anteriorDiv = divVisor.parentElement.lastElementChild as HTMLDivElement;
                }
                if (!anteriorDiv.classList.contains(ltrCss.contenedorVisorOculto) || divVisor === anteriorDiv)
                    buscarVisorVisible = false;
            }

            // Si el ultimo (o el primero) es el mismo que el actual, devuelve undefined
            if (!yaNoEsta && anteriorDiv === divVisor) {
                return;
            }

            // Si el siguiente (o el primero) es el mismo que el actual
            if (!yaNoEsta && anteriorDiv === divVisor) {
                return;
            }
            var idArchivo = Numero(anteriorDiv.id.replace('visor-del-archivo-', ''));
            var ref = anteriorDiv.querySelector('#refJs-' + idArchivo.toString()) as HTMLAnchorElement;

            ApiDelCrud.RenderizarUrlsEnVisor(this.CrudDeMnt, idArchivo, ref.text, false);
            this.IdDelUltimoArchivoRenderizado = idArchivo;
            this.LeerVinculosAl(idArchivo);
        }

        protected async LeerVinculosAl(idArchivo: number) {
        }

        private PosicionarseEnArchivoMostrado(): { divVisor: HTMLDivElement | undefined, yaNoEsta: boolean } {
            let visoresDeArchivos = this.PanelDeEditar.querySelectorAll('.' + ltrCss.contenedorVisor) as NodeListOf<HTMLDivElement>;
            if (visoresDeArchivos.length === 0) {
                this.OcultarBotonVisor();
                this.IdArchivoMostrado = 0;
                return { divVisor: undefined, yaNoEsta: false };
            }

            if (this.IdDelUltimoArchivoRenderizado === 0) {
                this.OcultarBotonVisor();
                return { divVisor: undefined, yaNoEsta: false };
            }

            let idVisorDelArchivo = `${atArchivo.visorArchivo}-${this.IdDelUltimoArchivoRenderizado}`;
            let divVisor = document.getElementById(idVisorDelArchivo) as HTMLDivElement;

            var yaNoEsta = false
            if (!divVisor) {
                divVisor = document.getElementById(visoresDeArchivos[0].id) as HTMLDivElement;
                yaNoEsta = true;
            }

            if (!divVisor || !divVisor.parentElement) {
                this.IdArchivoMostrado = 0;
                this.OcultarBotonVisor();
            };

            return { divVisor, yaNoEsta };
        }


        public ResumirArchivo(): void {
            if (!Definido(this.IdArchivoMostrado))
                return;

            ApiDelCrud.ProcesarRenderizar(this.CrudDeMnt, this.IdArchivoMostrado, ltrEventos.Edicion.ResumirArchivo);
        }


        public PasarOcr(): void {
            if (!Definido(this.IdArchivoMostrado))
                return;

            ApiDelCrud.ProcesarRenderizar(this.CrudDeMnt, this.IdArchivoMostrado, ltrEventos.Edicion.PasarOcr);
        }

        public CompartirConWhatsApp(): void {
            if (!Definido(this.IdArchivoMostrado))
                return;

            let idArchivo: number = this.IdArchivoMostrado;
            let caducaEl = new Date();
            caducaEl.setHours(caducaEl.getHours() + 2);
            ApiDePeticiones.RegistrarDescargaConGuid(this.ContenedorDelVisor, idArchivo, null)
                .then((peticion) => this.DespuesDeRegistrarDescargaConGuid(peticion, false))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public ConsultarConGuid(): void {
            // 1. Obtenemos el valor del input como string para poder limpiarlo
            const inputElement = document.getElementById(ltrValores.Crud.Edicion.diasDeConsulta) as HTMLInputElement;
            let valorSucio = inputElement.value;

            // 2. Limpieza profunda:
            // - Quitamos todo lo que no sea un dígito: [^\d]
            // - Si el resultado es vacío (porque eran solo letras o signos), usamos "0"
            let valorLimpio = valorSucio.replace(/[^\d]/g, '') || "0";

            if (valorSucio != valorLimpio) {
                MensajesSe.Advertencia(`El valor asignado '${valorSucio}', no es váido (solo enteros no negativos), corríjalo`);
                return;
            }

            // 3. Convertimos a número (ya estamos seguros de que es entero y positivo)
            const incremento = Number(valorLimpio);

            // 4. Actualizamos el input para que el usuario vea que se ha corregido (opcional pero recomendado)
            inputElement.value = incremento.toString();

            const caducaEl = new Date();
            caducaEl.setDate(caducaEl.getDate() + incremento);

            ApiDePeticiones.RegistrarConsultaConGuid(this.ContenedorDelVisor, this.Controlador, this.RegistroId, caducaEl)
                .then((peticion) => this.DespuesDeRegistrarConsultaConGuid(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public CompartirConGuid(): void {
            if (!Definido(this.IdArchivoMostrado))
                return;

            let idArchivo: number = this.IdArchivoMostrado;
            let caducaEl = new Date();
            caducaEl.setHours(caducaEl.getHours() + 2);
            ApiDePeticiones.RegistrarDescargaConGuid(this.ContenedorDelVisor, idArchivo, null)
                .then((peticion) => this.DespuesDeRegistrarDescargaConGuid(peticion, true))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private CompartirElemento() {
            const urlBase = new URL(window.location.origin + window.location.pathname);
            urlBase.searchParams.set(literal.id, this.Id.toString());
            const urlConId = urlBase.toString();
            CopiarUrlAlPortapapeles(urlConId, "Url de acceso al elemento copiada al porta papeles");
        }

        private DespuesDeRegistrarDescargaConGuid(peticion: ApiDeAjax.DescriptorAjax, compartirConGuid: boolean): any {
            const idArchivo = peticion.DatosDeEntrada[ltrPropiedades.SisDoc.Archivo.idArchivo]
            const guid = peticion.resultado.datos;
            if (guid) {
                const dominio = window.location.hostname;
                const puerto = window.location.port;
                const protocolo = window.location.protocol;
                const urlDeDescarga = `${protocolo}//${dominio}${(IsNullOrEmpty(puerto) ? "" : `:${puerto}`)}/${ltrControladores.SisDoc.Archivos}/${Ajax.Archivos.accion.DescargaConGuid}?guid=${guid}&id=${idArchivo}`;
                if (!compartirConGuid) {
                    var url = "https://wa.me/?text=" + encodeURIComponent(urlDeDescarga);
                    EntornoSe.AbrirPestana(url);
                }
                else {
                    CopiarUrlAlPortapapeles(urlDeDescarga);
                }
            }
        }


        private DespuesDeRegistrarConsultaConGuid(peticion: ApiDeAjax.DescriptorAjax): any {
            const idElemento = peticion.DatosDeEntrada['idElemento']
            const guid = peticion.resultado.datos;
            if (guid) {
                const dominio = window.location.hostname;
                const puerto = window.location.port;
                const protocolo = window.location.protocol;
                const urlDeDescarga = `${protocolo}//${dominio}${(IsNullOrEmpty(puerto) ? "" : `:${puerto}`)}/${this.Controlador}/${Ajax.Entorno.Acceso.Consultar}?${Ajax.Param.guid}=${guid}&${Ajax.Param.id}=${idElemento}`;
                CopiarUrlAlPortapapeles(urlDeDescarga, Ajax.Mensajes.NoMostrar);
                this.RecargarGridDeTrazas();
            }
        }

        public DescargarArchivo(): void {
            if (!Definido(this.IdArchivoMostrado))
                return;

            let idArchivo: number = this.IdArchivoMostrado;
            let parametros = `negocio=${this.CrudDeMnt.NombreDeNegocio}`;
            parametros = `${parametros}&idElemento=${this.ElementoEditado.Id}`;
            parametros = `${parametros}&idArchivo=${idArchivo}`;
            parametros = `${parametros}&auditar=true`;

            let descargar: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}?${parametros}`;
            let idOpcionDescargar = `${ltrEventos.Archivo.Descargar}-${idArchivo}`
            ApiDeArchivos.DescargarAnexado(idOpcionDescargar, descargar)
        }

        public MapearDatosJsonDesdeElVisor(json: object): boolean {
            ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDeEditar);
            return true;
        }

    }

    export function Neg_Tras_Pulsar_Bloquear() {
        if (Crud.crudMnt.EstoyEditando) {
            Crud.crudMnt.crudDeEdicion.BloquearDesbloquear();
        }
    }
}


async function Esperar_N_Segundos(tiempo: number) {
    await new Promise(resolve => setTimeout(resolve, 2000));
}

