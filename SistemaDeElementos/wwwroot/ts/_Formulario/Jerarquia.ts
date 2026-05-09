namespace Formulario {

    export const atJerarquia = {
        NoMapeablesAlDto: atControl.NoMapeablesAlDto
    }

    export const ltrJerarquia = {
        opcionesDeMenu: {
            eliminar: "eliminar",
            crear: "crear",
            copiar: "copiar",
            modificar: "modificar",
            cancelar: "cancelar"
        },
        arbol: {
            nodoSeleccionado: 'nodoseleccionado'
        },
        propiedades: {
            dto: 'dto'
        }

    };

    //const contenedorArbol = document.querySelector('.contenedor-arbol');
    //const formularioContenedor = document.querySelector('.formulario-contenedor-de-primer-bloque-anexados') as HTMLDivElement;

    //const observer = new ResizeObserver(entries => {
    //    for (let entry of entries) {
    //        if (entry.target === contenedorArbol) {
    //            formularioContenedor.style.width = `${entry.contentRect.width}px`;
    //        }
    //    }
    //});

    export function NodoSeleccionado(idLi: string) {
        let panelJer = (formulario as Jerarquia).PanelDeJerarquia;
        ApiDeJerarquia.DesSeleccionarNodo(panelJer)
        let lis: NodeListOf<HTMLLIElement> = panelJer.querySelectorAll(`li`) as NodeListOf<HTMLLIElement>;
        for (var i = 0; i < lis.length; i++) lis[i].classList.remove(ltrCss.nodoSeleccionado);
        let li: HTMLLIElement = panelJer.querySelector(`li[${literal.id}="${idLi}"]`) as HTMLLIElement;
        li.classList.add(ltrCss.nodoSeleccionado);

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(Ajax.Param.nodoSeleccionado, idLi));

        let filtros = new Diccionario<any>();

        let idElemento: number = Numero(li.getAttribute(atControl.idElemento));
        if (idElemento > 0 && !(formulario as Jerarquia).LeyendoNodo) {
            (formulario as Jerarquia).LeyendoNodo = true;
            ApiDePeticiones.LeerNodoSeleccionado(formulario as Jerarquia
                , formulario.Controlador
                , Ajax.EndPoint.LeerNodoSeleccionado
                , (formulario as Jerarquia).Negocio
                , idElemento
                , filtros
                , datosDeEntrada)
                .then((peticion) => (formulario as Jerarquia).DespuesDeLeerLeerNodoPorId(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
                .finally(() => {
                    (formulario as Jerarquia).LeyendoNodo = false
                });
        }
        else
            MensajesSe.Info(`La opción seleccionada no tiene id, ${li.id}`);
    }

    export class Jerarquia extends Formulario.Base {

        public get HePulsadoCopiar(): boolean {
            return this.ModoTrabajo === enumModoTrabajo.editando;
        }

        //*******************************************************************************************************//
        //  propiedades para referenciar los contonedores donde se encuadran la jerarquia y el dto editado
        //*******************************************************************************************************//
        public get idJerarquia(): string {
            return `datos-${this.IdFormulario}-jerarquia`;
        }
        public get ContenedorDeJerarquia(): HTMLDivElement {
            return document.getElementById(`datos-${this.IdFormulario}-jerarquia`) as HTMLDivElement;
        }

        private _leyendoNodo: boolean = false;

        public get LeyendoNodo(): boolean {
            return this._leyendoNodo;
        }

        public set LeyendoNodo(value: boolean) {
            this._leyendoNodo = value;
        }

        public get PanelDeJerarquia(): HTMLDivElement {
            return document.getElementById(this.idJerarquia) as HTMLDivElement;
        }
        public get PanelDelDetalle(): HTMLDivElement {
            return document.getElementById(`datos-${this.IdFormulario}-detalle`) as HTMLDivElement;
        }
        public get PanelDelDto(): HTMLDivElement {
            return document.getElementById(`datos-${this.IdFormulario}-dto`) as HTMLDivElement;
        }
        public get ContenedorDelDto(): HTMLDivElement {
            return document.getElementById(`${this.PanelDelDto.id}-contenedor`) as HTMLDivElement;
        }

        public get TablaDelDto(): HTMLDivElement {
            return this.PanelDelDto.querySelector("table") as HTMLDivElement;
        }

        public get ContenedorDelId(): HTMLDivElement {
            return document.getElementById(`${this.ContenedorDelDto.id}-id`) as HTMLDivElement;
        }

        public get PanelPie(): HTMLDivElement {
            return document.getElementById(`pie-${this.IdFormulario}`) as HTMLDivElement;
        }

        public get PanelPieMenu(): HTMLDivElement {
            return document.getElementById(`menu-${this.PanelPie.id}`) as HTMLDivElement;
        }

        public get Raiz(): HTMLUListElement {
            return document.getElementById(`${this.IdFormulario}.jerarquia.ul`) as HTMLUListElement;
        }

        public get Titulo(): HTMLAnchorElement {
            return document.getElementById(`${this.IdFormulario}.jerarquia.ref`) as HTMLAnchorElement;
        }

        public get MenuFormulario(): HTMLDivElement {
            return document.getElementById(`${this.CabeceraDelFormulario.id}.${ltrMenus.menu.formulario}`) as HTMLDivElement;
        }

        //*******************************************************************************************************//
        //  propiedades para referenciar los controles de las opciones de menu
        //*******************************************************************************************************//

        public get IdMenuPie(): string {
            return `menu-pie-${this.IdFormulario}`;
        }
        public get OpcionModificar(): HTMLInputElement {
            return document.getElementById(this.IdMenuPie + '-modificar') as HTMLInputElement;
        }
        public get OpcionCrear(): HTMLInputElement {
            return document.getElementById(this.IdMenuPie + '-crear') as HTMLInputElement;
        }

        protected IdDeLaOpcionDeMenu(opcion: string): string {
            return this.IdMenuPie + '-' + opcion;
        }

        public get PanelDeFiltrado(): HTMLDivElement {
            return document.getElementById(`filtro-${this.IdFormulario}_cuerpo`) as HTMLDivElement;
        }
        public get CheckDeJerarquia(): HTMLInputElement {
            return document.getElementById(`filtro-${this.IdFormulario}-opciones-arbol`) as HTMLInputElement;
        }

        //*******************************************************************************************************//
        //  propiedades para controlar si se muestra y coómo se filtra en la jerarquía
        //*******************************************************************************************************//
        public get HayQueMostrarJerarquia(): boolean {
            let check = this.CheckDeJerarquia;
            return check.checked === true && IsNullOrEmpty(this.FiltroPorNombre);
        }

        public get ControlFiltroPorNombre(): HTMLInputElement {
            return document.getElementById(`filtro-${this.IdFormulario}-opciones-nombre`) as HTMLInputElement;
        }
        public get FiltroPorNombre(): string {
            let nombre = this.ControlFiltroPorNombre;
            return nombre.value;
        }

        public set FiltroPorNombre(valor: string) {
            let nombre = document.getElementById(`filtro-${this.IdFormulario}-opciones-nombre`) as HTMLInputElement;
            nombre.value = valor;
        }

        private _modoTrabajo: string;

        public get ModoTrabajo(): string {
            return this._modoTrabajo;
        }
        protected set ModoTrabajo(modo: string) {
            this._modoTrabajo = modo;
        }

        private _registro: any = undefined
        public get RegistroEditado(): any {
            return this._registro;
        }

        private _objetosDeUnExpansor: Diccionario<any> = new Diccionario<any>();
        public ObjetoDeExpansor(idModal: string): any {
            return this._objetosDeUnExpansor.Obtener(idModal);
        }
        public AsignarObjetoDeExpansor(idModal: string, objeto): any {
            return this._objetosDeUnExpansor.Agregar(idModal, objeto);
        }


        private _Guid: any;
        public get Guid(): any {
            return this._Guid;
        }
        /*
         * variables que indican el negocio principal con el que se trabaja en la jerarquía
         * */
        private _negocio: string;
        private _posicionarEnElNodo: string;

        public jerarquia: any;

        protected ModoAcceso: ModoAcceso.enumModoDeAccesoDeDatos;

        public get Negocio(): string {
            return this._negocio;
        }

        public get IdNegocio(): number {
            return Numero(this.PanelDelDto.getAttribute(literal.idNegocio));
        }

        public get IdVista(): number {
            return Numero(this.PanelDelDto.getAttribute(atMantenimniento.idVista));
        }

        private IdEditado: number;
        private Expresion: string;
        /**
         * Constructor y métodos de la clase base de jerarquías
         * @param idFormulario
         */
        constructor(idFormulario: string, negocio: string) {
            super(idFormulario);
            this._negocio = negocio;
            this._Guid = generarUUID();
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            this.InicializarFormulario();
            Formulario.formulario = this;
            //observer.observe(contenedorArbol);
            let ancho = this.ContenedorDeJerarquia.style.width;
            this.CabeceraDelFormulario.style.setProperty("grid-template-columns", `0px ${ancho}`);
            this.RecargarJerarquia(blanquearFiltros);
        }

        private RecargarJerarquia(blanquearFiltros: boolean) {
            //ApiDeInicializacion.Checkes(this.CabeceraDelFormulario);
            if (blanquearFiltros) {
                ApiPanel.BlanquearControlesDeIU(this.PanelDeFiltrado);
            }

            if (this.Estado.Contiene(ltrClaveDeEstado.filtrosUrl)) {
                MapearAlCrud.FiltrosPasadosEnEstado(this);
            }
            this.AplicarFiltro();
        }

        public AccionesAntesDeSalir(): void {
            super.AccionesAntesDeSalir();
            MapearAlDiccionario.Filtros(this.CabeceraDelFormulario, this.Estado);
            MapearAlDiccionario.Filtros(this.ModalFiltro, this.Estado);
        }

        public EsNodoSeleccionable(dto: Tipos.NodoDto): boolean {
            return true;
        }

        public Filtrar() {
            super.Filtrar();
            this.MostrarJerarquia();
        }

        public MostrarJerarquia() {
            this.LeerJerarquia();
        }

        public PlegarJerarquia() {
            for (let i = 0; i < this.jerarquia.ramas.length; i++)
                ApiDeJerarquia.NodoPulsado(`${this.jerarquia.ramas[i].dto.id}.li`
                    , () => Formulario.NodoSeleccionado(`${this.jerarquia.ramas[i].dto.id}.li`));
        }


        public HayFiltros(): boolean {
            let hayFiltros: boolean = super.HayFiltros();
            if (!hayFiltros) {
                let datos: Diccionario<any> = new Diccionario<any>();
                return this.PrepararfiltrosParaLeerLaJerarquia(datos);
            }
            return hayFiltros;
        }

        public PrepararfiltrosParaLeerLaJerarquia(datos: Diccionario<any>): boolean {
            ApiPanel.MapearControlesDesdeElPanelAlDiccionario(this.PanelDeFiltrado, datos);
            return datos.Obtener(ltrPropiedades.TipoDeElemento.Activo) === true;
        }

        public LeerJerarquia(): void {
            let datos: Diccionario<any> = new Diccionario<any>();
            let hayFiltros: boolean = this.PrepararfiltrosParaLeerLaJerarquia(datos);

            if ((hayFiltros && this.HayQueMostrarJerarquia) || !this.HayQueMostrarJerarquia)
                this.CheckDeJerarquia.checked = false;

            datos.Agregar(Formulario.literales.hayFiltros, hayFiltros);
            datos.Agregar(this.CheckDeJerarquia.getAttribute(atControl.propiedad), this.HayQueMostrarJerarquia);
            datos.Agregar(this.ControlFiltroPorNombre.getAttribute(atControl.propiedad), this.FiltroPorNombre);
            ApiDePeticiones.LeerJerarquia(this, this.Controlador, Ajax.EndPoint.LeerJerarquia, this.Negocio, null, datos)
                .then((peticion) => this.DespuesDeLeerJerarquia(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public DespuesDeLeerJerarquia(peticion: ApiDeAjax.DescriptorAjax): void {

            let datos: Diccionario<any> = peticion.DatosDeEntrada as Diccionario<any>;
            let formulario: Formulario.Jerarquia = peticion.llamador as Formulario.Jerarquia;
            this.jerarquia = peticion.resultado.datos as Tipos.JerarquiaDto;
            formulario.pintarJerarquia();
        }

        public pintarJerarquia(): void {
            this.AntesDePintarLaJerarquia();
            for (let i: number = 0; i < this.jerarquia.ramas.length; i++) {
                this.AntesDePintarLaRama(this.Raiz, this.jerarquia.ramas[i]);
                this.pintarNodo(this.Raiz, this.jerarquia.ramas[i]);
            }
            this.DespuesDePintarLaJerarquia();
        }

        public DespuesDePintarLaJerarquia(): void {
            this.ComenzarModoNuevo();
        }

        public AntesDePintarLaJerarquia(): void {
            this.Raiz.innerHTML = ""
        }

        protected AntesDePintarLaRama(raiz: HTMLUListElement, nodoDto: Tipos.NodoDeJerarquiaDto) {
        }

        public pintarNodo(raiz: HTMLUListElement, nodoDto: Tipos.NodoDeJerarquiaDto): void {
            let alSeleccionar: string = this.EsNodoSeleccionable(nodoDto.dto)
                ? `javascript: ApiDeJerarquia.NodoPulsado('${nodoDto.dto.id}.li', ()=>Formulario.NodoSeleccionado('${nodoDto.dto.id}.li'))`
                : '';

            let id: string = this.EsNodoSeleccionable(nodoDto.dto)
                ? `${nodoDto.dto.id}.li`
                : `No.${nodoDto.dto.id}.li`;

            let li: HTMLLIElement = ApiControl.CrearLiEnUl(raiz
                , id
                , nodoDto.dto.nombre
                , alSeleccionar
                , ''
                , JSON.stringify(nodoDto.dto));

            li.classList.add(ltrCss.nodoDeJerarquia);
            li.setAttribute(atControl.idElemento, nodoDto.dto.id.toString());
            this.AplicarCssAlNodo(nodoDto, li);

            for (let i: number = 0; i < nodoDto.hijos.length; i++) {
                let ul: HTMLUListElement = ApiControl.CrearUlVacioEnLi(li, `${nodoDto.dto.id}.ul`);
                this.pintarNodo(ul, nodoDto.hijos[i]);
            }
        }

        public AplicarCssAlNodo(nodoDto: Tipos.NodoDeJerarquiaDto, li: HTMLLIElement): void {
            if (ObtenerPropiedad(nodoDto[Formulario.ltrJerarquia.propiedades.dto], ltrPropiedades.TipoDeElemento.Activo) === false)
                li.classList.add(ltrCss.nodoDeJerarquiaDeBaja);
        }

        public ComenzarModoNuevo() {
            this._registro = undefined;
            this.ModoTrabajo = enumModoTrabajo.creando;
            this.PanelDelDto.setAttribute(Ajax.Param.nodoSeleccionado, '');
            this.IdEditado = 0;
            ApiDeJerarquia.DesSeleccionarNodo(this.PanelDeJerarquia);
            ApiControl.OcultarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.eliminar));
            ApiControl.OcultarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.modificar));
            ApiControl.OcultarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.cancelar));
            if (ApiControl.MostrarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.crear)))
                ApiControl.CambiarLiteralDeMenuPorNombre(this.PanelPieMenu, ltrMenus.BarraDeMenu.Copiar, ltrMenus.BarraDeMenu.Crear);

            this.ContenedorDelId.style.display = ltrStyle.display.none;

            ApiPanel.BlanquearControlesDeIU(this.PanelDelDto);
            ApiControl.DesbloquearEditorPorPropiedad(this.PanelDelDto, literal.nombre);

            this.AplicarPermisoDeCreacion();
            ApiDeMenuFlotante.OcultarLosMf(this.CabeceraDelFormulario);
            if (Definido(this._posicionarEnElNodo)) {
                NodoSeleccionado(this._posicionarEnElNodo);
                this._posicionarEnElNodo = null;
            }
        }

        protected AplicarPermisoDeCreacion(): void {
            if (!Registro.EsAdministrador()) {
                ApiControl.BloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.crear));
                ApiPanel.DesactivarPanel(this.PanelDelDto);
            }
        }

        public ComenzarModoEdicion(dto: any, modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos, nodoSeleccionado: string) {

            this._registro = dto;

            this.ModoTrabajo = ModoAcceso.EsGestor(modoAcceso)
                ? enumModoTrabajo.editando
                : enumModoTrabajo.consultando;

            ApiPanel.QuitarClaseDeCtrlNoValido(this.PanelDelDto);
            let usaBaja = ExistePropiedad(dto, ltrPropiedades.baja);
            let estaDeBaja = usaBaja ? ObtenerPropiedad(dto, ltrPropiedades.baja, false) : false;
            if (estaDeBaja) this.ModoTrabajo = enumModoTrabajo.consultando;

            ApiPanel.BlanquearControlesDeIU(this.PanelDelDto);
            this.MapearElDtoLeido(dto, modoAcceso);

            ModoAcceso.AplicarPermisosAEditores(this.PanelDelDto, dto);
            let contenedorDeArchivos = ApiDeArchivos.MostrarArchivosAnexados(`contenedor-${literal.ExpanDeArchivos}`, this.Negocio, dto[literal.id]);
            if (Definido(contenedorDeArchivos))
                ApiDeArchivos.AplicarDisposicionDeArchivos(contenedorDeArchivos);

            ModoAcceso.AplicarModoAccesoAlSelectorDeArchivos(this.PanelDelDto,
                this.ModoTrabajo === enumModoTrabajo.editando
                    ? ModoAcceso.enumModoDeAccesoDeDatos.Gestor
                    : ModoAcceso.enumModoDeAccesoDeDatos.Consultor)

            this.AjustarOpcionesDelModoEdicion();
            this.PanelDelDto.setAttribute(Ajax.Param.nodoSeleccionado, nodoSeleccionado);
            this.IdEditado = dto[literal.id];
            this.Expresion = ObtenerPropiedad(dto, literal.expresion, undefined, false);
            if (NoDefinido(this.Expresion)) this.Expresion = ObtenerPropiedad(dto, literal.nombre, "No definida");
        }

        public MapearElDtoLeido(dto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            let propiedades = ToLista(this.PanelDelDto.getAttribute(Formulario.atJerarquia.NoMapeablesAlDto), ';');
            MapearAlPanel.ElObjeto(this.PanelDelDto, dto, modoDeAcceso, propiedades, false);

            let delSistema: boolean = ObtenerPropiedad(dto, ltrPropiedades.DelSistema, false);
            if (delSistema || this.ModoTrabajo === enumModoTrabajo.consultando) {
                ApiControl.BloquearCheckPorPropiedad(this.PanelDelDto, ltrPropiedades.NombreModificable, true, false);
            }

            MapearAlGrid.MapearLosGridDeDetalle(this.PanelDelDto, this.IdNegocio, dto, null);
        }

        public AjustarOpcionesDelModoEdicion() {
            ApiControl.CambiarLiteralDeMenuPorNombre(this.PanelPieMenu, ltrMenus.BarraDeMenu.Crear, ltrMenus.BarraDeMenu.Copiar);
            ApiControl.MostrarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.modificar));
            ApiControl.MostrarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.eliminar));
            ApiControl.MostrarOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.cancelar));

            if (this.ModoTrabajo === enumModoTrabajo.consultando) {
                ApiControl.BloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.crear));
                ApiControl.BloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.modificar));
                ApiControl.BloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.eliminar));
                //ApiPanel.DesactivarPanel(this.PanelDelDto);
            }
            else {
                ApiControl.DesbloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.crear));
                ApiControl.DesbloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.modificar));
                ApiControl.DesbloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(ltrJerarquia.opcionesDeMenu.eliminar));
            }

            //this.ContenedorDelDto.style.minHeight = `${this.TablaDelDto.tBodies[0].clientHeight.toString()}px`;
            this.ContenedorDelId.style.display = ltrStyle.display.block;
        }

        public CancelarModificacion(): void {
            this.ComenzarModoNuevo();
        }

        public CrearNodo(): void {
            let json: JSON = ApiPanel.MapearControlesDesdeElPanelAlJson(this.PanelDelDto, JSON.parse(`{}`));
            if (json.hasOwnProperty("id"))
                delete json['id'];

            let datos: Diccionario<any> = new Diccionario();
            ApiDePeticiones.CrearNodo(this, this.Controlador, Ajax.EndPoint.CrearNodo, this.Negocio, json, datos)
                .then((peticion) => this.InicializarJerarquia(false))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public ModificarNodo(): void {
            let json: JSON = ApiPanel.MapearControlesDesdeElPanelAlJson(this.PanelDelDetalle, JSON.parse(`{}`));

            let datos: Diccionario<any> = new Diccionario();
            datos.Agregar(Ajax.Param.operacion, Ajax.persisitencia.modificar);
            ApiDePeticiones.PersistirNodo(this, this.Controlador, Ajax.EndPoint.PersistirNodo, this.Negocio, json, datos)
                .then((peticion) => this.InicializarJerarquia(false))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public EliminarNodo(): void {
            let json: JSON = ApiPanel.MapearControlesDesdeElPanelAlJson(this.PanelDelDetalle, JSON.parse(`{}`));

            let datos: Diccionario<any> = new Diccionario();
            datos.Agregar(Ajax.Param.operacion, Ajax.persisitencia.eliminar);
            ApiDePeticiones.PersistirNodo(this, this.Controlador, Ajax.EndPoint.PersistirNodo, this.Negocio, json, datos)
                .then((peticion) => this.InicializarJerarquia(false))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }


        public DespuesDeLeerLeerNodoPorId(peticion: ApiDeAjax.DescriptorAjax) {
            let formulario: Formulario.Jerarquia = peticion.llamador as Formulario.Jerarquia;
            let nodoSeleccionado: string = peticion.DatosDeEntrada[0].valor;
            let nodoDto: any = peticion.resultado.datos as any;
            let modoDeAcceso = ObtenerPropiedad(peticion.resultado.datos, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            this.ComenzarModoEdicion(nodoDto, modoDeAcceso, nodoSeleccionado);
        }


        public ProcesarOpcionMf(opcion: string, esContextual: boolean): void {
            switch (opcion) {
                case ltrMenus.eventosDeMf.alta: {
                    this.EjecutarAccion(Ajax.EndPoint.DarDeAlta);
                    break;
                }
                case ltrMenus.eventosDeMf.baja: {
                    this.EjecutarAccion(Ajax.EndPoint.DarDeBaja);
                    break;
                }
                default:
                    super.ProcesarOpcionMf(opcion, esContextual);
                    break;
            }
        }

        public EjecutarAccion(accion: string): void {
            if (this.IdEditado > 0) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                datosDeEntrada.push(new Parametro(Ajax.Param.nodoSeleccionado, this.PanelDelDto.getAttribute(Ajax.Param.nodoSeleccionado)));

                let parametros: Array<Parametro> = new Array<Parametro>();
                parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, this.IdNegocio));
                ApiDePeticiones.EjecutarAccion(this, this.Controlador, accion, this.IdEditado, parametros, datosDeEntrada)
                    .then((peticion: ApiDeAjax.DescriptorAjax) => this.Recargar(peticion))
                    .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
            }
            else {
                MensajesSe.Info(`Esta opción, ${accion}, sólo es válida para elementos editados`)
            }
        }

        public Recargar(peticion: ApiDeAjax.DescriptorAjax): any {
            let formulario: Jerarquia = peticion.llamador as Jerarquia;
            formulario._posicionarEnElNodo = peticion.DatosDeEntrada[0].valor;
            formulario.RecargarJerarquia(true);
        }

        public Expansor_NavegarDesdeFormulario(url: string, paginaDestino: string) {
            this.Estado.Agregar(ltrClaveDeEstado.EditarAlVolver, true);
            this.Estado.Agregar(Ajax.Param.nodoSeleccionado, this._posicionarEnElNodo);
            this.Estado.Guardar();

            let datos: Tipos.Restrictor[] = [];
            let negocio: Tipos.Restrictor = new Tipos.Restrictor('idnegocio', this.IdNegocio, this.Negocio);
            let elemento: Tipos.Restrictor = new Tipos.Restrictor('idelemento', this.IdEditado, this.Expresion);

            datos.push(negocio);
            datos.push(elemento);

            EntornoSe.PushRestrictores(paginaDestino, datos);
            EntornoSe.NavegarAUrl(url);
        }

        public Expansor_TrasCargarExpansor(idGrid: string) {
            if (this.ModoTrabajo !== enumModoTrabajo.editando)
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);
        }

        public Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion: string, propiedadesRestrictoras: string): void {
            ApiDelCrud.AbrirModalDeRelacionParaCrear(idModalDeCreacion, propiedadesRestrictoras
                , ObtenerPropiedad(this.RegistroEditado, literal.id)
                , ObtenerPropiedad(this.RegistroEditado, literal.nombre)
                , this.IdNegocio
                , this.Negocio)
        }

        public Expansor_BorrarRelacion(idGridDeDetalle: string, numeroFila: number, accionDeBorrado: string) {
            ApiDeExpansor.BorrarRelacion(this, idGridDeDetalle, numeroFila, accionDeBorrado);
        }


        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            return new Array<Parametro>();
        }

        public ParametrosParaBorrarRelacion(): Array<Parametro> {
            return new Array<Parametro>();
        }

        public CerrarRelacion(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

            ApiPanel.BlanquearControlesDeIU(modal, false);
            ApiPanel.OcultarModal(modal);
        }

        public CrearRelacion(idModal: string): void {
            let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, modal, enumModoTrabajo.creando);
            let controlador: string = modal.getAttribute(atModal.controlador);
            let accion: string = modal.getAttribute(atModal.accion);

            ApiDePeticiones.CreaRelacion(this, controlador, Definido(accion) ? accion : Ajax.EndPoint.CrearRelacion, this.IdNegocio, json)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesVincular(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        DespuesVincular(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): any {
            let jerarquia: Jerarquia = peticion.llamador as Jerarquia;

            //blanquear los controles de la interface
            ApiPanel.BlanquearControlesDeIU(modal);

            //recargar el grid de relaciones del expansor
            let idGrid: string = modal.getAttribute(atGridDeDetalle.gridDeRelacionAsociado);
            let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
            let campoRestrictor: string = grid.getAttribute(atGridDeDetalle.campoRestrictor);
            MapearAlGrid.MapearGridDeDetalle(grid, jerarquia.IdNegocio, ObtenerCampoRestrictor(jerarquia.RegistroEditado, campoRestrictor), jerarquia.Guid);

            if (peticion.nombre === Ajax.EndPoint.CrearRelacion) {
                let accion = modal.getAttribute(atModal.trasAceptar);
                if (Definido(accion))
                    Evaluar('Jerarquia.DespuesVincular', accion, accion.includes('this') ? modal : undefined);
            }

            ApiPanel.OcultarModal(modal);

            // ver si hay que seguir creando
            let check: HTMLInputElement = document.getElementById(`${modal.id}-crear-mas`) as HTMLInputElement;
            if (!check.checked)
                ApiPanel.AbrirModal(modal);

        }


        public ModificarRelacion(idModal: string): void { ApiDeExpansor.ModificarRelacion(this, idModal); }

        public Expansor_AbrirModalDeRelacionParaEditar(idGridDeDetalle: string, propiedadRestrictora: string, numeroFila: number) {
            ApiDeExpansor.AbrirModalDeRelacionParaEditar(this, idGridDeDetalle, propiedadRestrictora, numeroFila);
        }


        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        }
    }

    export function EventosDeJerarquia(accion: string, parametros: string) {
        try {
            switch (accion) {
                case ltrEventos.Jerarquia.CrearNodo: {
                    (formulario as Jerarquia).CrearNodo();
                    break;
                }
                case ltrEventos.Jerarquia.ModificarNodo: {
                    (formulario as Jerarquia).ModificarNodo();
                    break;
                }
                case ltrEventos.Jerarquia.EliminarNodo: {
                    (formulario as Jerarquia).EliminarNodo();
                    break;
                }
                case ltrEventos.Jerarquia.CancelarModificacion: {
                    (formulario as Jerarquia).CancelarModificacion();
                    break;
                }
                case ltrEventos.Jerarquia.MostarJerarquia: {
                    (formulario as Jerarquia).FiltroPorNombre = "";
                    (formulario as Jerarquia).MostrarJerarquia();
                    break;
                }
                case ltrEventos.Jerarquia.PlegarJerarquia: {
                    (formulario as Jerarquia).PlegarJerarquia();
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, error.message);
        }
    }


    export function EventosDeExpansores(accion: string, parametros: string) {
        try {
            switch (accion) {
                case ltrEventos.Expansores.NavegarDesdeEdicion: {
                    let partes = parametros.split(';');
                    (formulario as Jerarquia).Expansor_NavegarDesdeFormulario(partes[0], partes[1]);
                    break;
                }
                case ltrEventos.Expansores.TrasCargarExpansor: {
                    let idGridDeDetalle: string = parametros;
                    (formulario as Jerarquia).Expansor_TrasCargarExpansor(idGridDeDetalle);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalDeRelacionParaCrear: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2)
                        throw Error(`El parametro ${parametros} ha de definir el id de la modal que se ha de abrir y el nombre de la propiedad restrictora`);
                    let idModalDeCreacion: string = partes[0];
                    let propiedadRestrictora: string = partes[1];

                    (formulario as Jerarquia).Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion, propiedadRestrictora);
                    break;
                }
                case ltrEventos.Expansores.AbrirModalDeRelacionParaEditar: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 3)
                        throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle, el restrictor y el número de fila`);
                    let idGridDeDetalle: string = partes[0];
                    let propiedadRestrictora: string = partes[1];
                    let numeroFila: number = Numero(partes[2]);

                    (formulario as Jerarquia).Expansor_AbrirModalDeRelacionParaEditar(idGridDeDetalle, propiedadRestrictora, numeroFila);
                    break;
                }
                case ltrEventos.Expansores.BorrarRelacion: {
                    let partes: string[] = parametros.split(';');
                    if (partes.length != 2 && partes.length != 3)
                        throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle y el número de fila y opcionalmente la opción de borrado`);
                    let idGridDeDetalle: string = partes[0];
                    let numeroFila: number = Numero(partes[1]);

                    var accionDeBorrado = partes.length === 3 ? partes[2] : undefined;

                    (formulario as Jerarquia).Expansor_BorrarRelacion(idGridDeDetalle, numeroFila, accionDeBorrado);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Expansor, accion: ${accion}`, error.message);
        }
    }


    export function EventosModalDeRelacion(accion: string, parametros: string) {

        try {
            switch (accion) {
                case ltrEventos.ModalDeRelacionar.Cerrar: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal de creación dto para poderla cerrar`);
                    let idModal: string = parametros;
                    (formulario as Jerarquia).CerrarRelacion(idModal)
                    break;
                }

                case ltrEventos.ModalDeRelacionar.CrearRelacion: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para crear la relación`);
                    let idModal: string = parametros;
                    (formulario as Jerarquia).CrearRelacion(idModal);
                    break;
                }

                case ltrEventos.ModalDeRelacionar.ModificarRelacion: {
                    if (IsNullOrEmpty(parametros))
                        throw Error(`Debe indicar el id de la modal para modificar la relación`);
                    let idModal: string = parametros;
                    (formulario as Jerarquia).ModificarRelacion(idModal);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`ModalDeRelacion, accion: ${accion}`, error.message);
        }
    }
}
