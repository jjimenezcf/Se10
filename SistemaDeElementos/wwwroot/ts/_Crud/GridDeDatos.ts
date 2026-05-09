namespace Crud {

    class ClausulaDeOrdenacion {
        ordenarPor: string;
        modo: string;

        constructor(ordenarPor: string, modo: string) {
            this.ordenarPor = ordenarPor;
            this.modo = modo;
        }
    }

    class ResultadoDeLectura {
        registros: any;
        total: number;
    }

    export class DatosPeticionNavegarGrid {
        private _grid: GridDeDatos;
        private _accion: string;
        private _posicion: number;

        public get Grid(): GridDeDatos {
            return this._grid;
        }

        public get Accion(): string {
            return this._accion;
        }
        public get PosicionDesdeLaQueSeLee(): number {
            return this._posicion;
        }
        constructor(grid: GridDeDatos, accion: string, posicion: number) {
            this._grid = grid;
            this._accion = accion;
            this._posicion = posicion;
        }
    }

    class InfoNavegador {
        public cantidad: number;
        public posicion: number;
        public pagina: number;
        public leidos: number;
        public total: number;
    }

    export class Navegador {

        private id: string;
        private idInfo: string;
        private idMensaje: string;

        private esRestauracion: boolean;

        public get EsRestauracion(): boolean {
            return this.esRestauracion;
        }
        public set EsRestauracion(valor: boolean) {
            this.esRestauracion = valor;
        }

        public get Cantidad(): number {
            return Numero(this.Navegador.value);
        }
        public get Posicion(): number {
            return Numero(this.Navegador.getAttribute(atGrid.navegador.posicion));
        }
        public get NumeroDePaginaDelGrid(): number {
            return Numero(this.Navegador.getAttribute(atGrid.navegador.pagina));
        }
        public get Leidos(): number {
            return Numero(this.Navegador.getAttribute(atGrid.navegador.leidos));
        }
        public get Total(): number {
            return Numero(this.Navegador.getAttribute(atGrid.navegador.total));
        }
        public get Id(): string {
            return this.id;
        }

        public set Cantidad(valor: number) {
            this.Navegador.value = valor.toString();
        }
        public set Posicion(valor: number) {
            this.Navegador.setAttribute(atGrid.navegador.posicion, valor.toString());
        }
        public set NumeroDePaginaDelGrid(valor: number) {
            this.Navegador.setAttribute(atGrid.navegador.pagina, valor.toString());
        }
        public set Info(valor: string) {
            let div: HTMLDivElement = document.getElementById(this.idInfo) as HTMLDivElement;
            div.innerHTML = valor;
        }
        public set Mensaje(valor: string) {
            let div: HTMLDivElement = document.getElementById(this.idMensaje) as HTMLDivElement;
            div.innerHTML = valor;
        }
        public set Leidos(valor: number) {
            this.Navegador.setAttribute(atGrid.navegador.leidos, valor.toString());
        }
        public set Total(valor: number) {
            this.Navegador.setAttribute(atGrid.navegador.total, valor.toString());
        }
        public set Id(valor: string) {
            this.id = valor;
        }

        constructor(idGrid: string) {
            this.id = `${idGrid}_${atGrid.idCtrlCantidad}`;
            this.idInfo = `${idGrid}_${atGrid.idInfo}`;
            this.idMensaje = `${idGrid}_${atGrid.idMensaje}`;
        }

        public get Navegador(): HTMLInputElement {
            let input = document.getElementById(this.Id) as HTMLInputElement;
            return input;
        }

        public get Controlador(): string {
            return this.Navegador.getAttribute(atControl.controlador);
        }

        public get Datos(): InfoNavegador {
            let datos: InfoNavegador = new InfoNavegador();
            datos.cantidad = this.Cantidad;
            datos.leidos = this.Leidos;
            datos.pagina = this.NumeroDePaginaDelGrid;
            datos.posicion = this.Posicion;
            datos.total = this.Total;
            return datos;
        }

        public RestaurarDatos(datos: InfoNavegador): void {
            if (datos !== undefined) {
                this.Cantidad = datos.cantidad;
                this.Leidos = datos.leidos;
                this.NumeroDePaginaDelGrid = datos.pagina;
                this.Posicion = datos.posicion;
                this.Total = datos.total;
                this.EsRestauracion = true;
            }
            else {
                var porleer = Numero((document.getElementById(this.id) as HTMLInputElement).value);
                this.Cantidad = porleer === 0 ? 10 : porleer;
                this.Leidos = 0;
                this.NumeroDePaginaDelGrid = 1;
                this.Posicion = 0;
                this.Total = 0;
                this.esRestauracion = false;
            }
        }

        public Actualizar(accion: string, posicionDesdeLaQueSeLeyo: number, registrosLeidos: number, seleccionados: number): void {
            this.Leidos = registrosLeidos;
            this.Posicion = accion == atGrid.accion.ultima ? this.Total - registrosLeidos : posicionDesdeLaQueSeLeyo + registrosLeidos;
            let paginasTotales: number = Math.ceil(this.Total / this.Cantidad);

            let paginaAnterior: number = this.NumeroDePaginaDelGrid;
            let paginaNueva: number = 1;

            switch (accion) {
                case atGrid.accion.buscar: {
                    paginaNueva = 1;
                    this.Navegador.readOnly = false;
                    this.Navegador.classList.remove(ltrCss.soloLectura);
                    break;
                }
                case atGrid.accion.irA: {
                    paginaNueva = (posicionDesdeLaQueSeLeyo / this.Cantidad) + 1;
                    this.Navegador.readOnly = false;
                    this.Navegador.classList.remove(ltrCss.soloLectura);
                    break;
                }
                case atGrid.accion.siguiente: {
                    paginaNueva = paginaAnterior + 1;
                    break;
                }
                case atGrid.accion.anterior: {
                    paginaNueva = paginaAnterior - 1;
                    break;
                }
                case atGrid.accion.restaurar: {
                    paginaNueva = paginaAnterior;
                    break;
                }
                case atGrid.accion.ultima: {
                    posicionDesdeLaQueSeLeyo = this.Total - registrosLeidos;
                    paginaNueva = (this.Cantidad >= this.Total) ? 1 : paginasTotales;
                    break;
                }
                case atGrid.accion.seleccionadas: {
                    paginasTotales = 1;
                    paginaNueva = -1;
                    this.Navegador.readOnly = true;
                    this.Navegador.classList.add(ltrCss.soloLectura);
                    break;
                }
            }

            this.NumeroDePaginaDelGrid = paginaNueva <= 0 ? 1 : paginaNueva;
            this.Info = `Pagina ${this.NumeroDePaginaDelGrid} de ${paginasTotales}`;
            this.InformarElementosSeleccionados(seleccionados);
        }

        public InformarElementosSeleccionados(seleccionados: number): void {
            this.Mensaje = `Seleccionados ${seleccionados} de ${this.Total}`;
        }
    }

    class PaginaDelGrid {
        private _elementos: Elemento[] = [];
        private _pagina: number;
        private _posicion: number;
        private _cantidad: number;
        private _fecha: Date;
        public get fecha(): string {
            return this._fecha.toISOString();
        }
        public get Pagina(): number {
            return this._pagina;
        }
        public get Posicion(): number {
            return this._posicion;
        }
        public get Cantidad(): number {
            return this._cantidad;
        }
        public get Elementos(): Elemento[] {
            return this._elementos;
        }
        public get Registros(): any[] {
            let registros: any[] = [];
            for (let i: number = 0; i < this.Elementos.length; i++) {
                let registro: any = this.Elementos[i].Registro;
                registros.push(registro);
            }
            return registros;
        }

        constructor(pagina: number, posicion: number, cantidad: number, registros: [], expresionMostrar: string) {
            this.anadirElementos(registros, expresionMostrar);
            this._pagina = pagina;
            this._cantidad = cantidad;
            this._posicion = posicion;
            this._fecha = new Date(Date.now());
        };

        public ObtenerPorId(id: number): Elemento {
            for (let i: number = 0; i < this._elementos.length; i++) {
                if (this._elementos[i].Id === id)
                    return this._elementos[i];
            }
            return null;
        }

        public ObtenerPorPosicion(pos: number): Elemento {
            if (pos >= this._elementos.length || pos < 0)
                return null;

            return this._elementos[pos];
        }

        private anadirElementos(registros: [], expresionMostrar: string) {
            for (let i: number = 0; i < registros.length; i++) {
                let e: Elemento = new Elemento(registros[i], expresionMostrar);
                this._elementos.push(e);
            }
        }
    }

    class DatosDelGrid {
        private _paginas: PaginaDelGrid[] = [];
        private _paginaActual: number;

        public get NumeroDePaginas(): number {
            return this._paginas.length;
        }

        public set PaginaActual(numeroDePagina: number) {
            this._paginaActual = numeroDePagina;
        }
        public get PaginaActual() {
            return this._paginaActual;
        }


        public AnadirPagina(numeroDePagina: number, posicion: number, cantidad: number, registros: [], expresionMostrar: string) {
            let i: number = this.Buscar(numeroDePagina);
            if (i >= 0) {
                this._paginas.splice(i, 1);
            }
            let p: PaginaDelGrid = new PaginaDelGrid(numeroDePagina, posicion, cantidad, registros, expresionMostrar);
            this._paginas.push(p);
        }

        public InicializarCache() {
            this._paginas.splice(0, this._paginas.length);
        }

        public PaginaDelGrid(numeroDePagina: number): PaginaDelGrid {
            let i: number = this.Buscar(numeroDePagina);
            if (i >= 0) {
                return this._paginas[i];
            }
            return null;
        }

        public PaginaCacheada(indice: number): PaginaDelGrid {
            if (this._paginas.length == 0 || indice >= this._paginas.length) {
                return null;
            }
            return this._paginas[indice];
        }

        public ObtenerPorId(id: number, erroSiNoLoHay: boolean = true): Elemento {
            let p: PaginaDelGrid = this.PaginaDelGrid(this._paginaActual);
            if (p === null) {
                if (erroSiNoLoHay)
                    throw Error(`la página ${this._paginaActual} no se encuentra en la lista de páginas del grid`);
                MensajesSe.Info(`recargando datos...`);
                return null;
            }

            let e: Elemento = p.ObtenerPorId(id);
            if (e === null)
                throw Error(`La fila seleccionada ${id} no se encuentra en la página actual del grid`);
            return e;
        }

        public ObtenerPorPosicion(pos: number): Elemento {
            let p: PaginaDelGrid = this.PaginaDelGrid(this._paginaActual);
            if (p === null)
                throw Error(`la página ${this._paginaActual} no se encuentra en la lista de páginas del grid`);

            let e: Elemento = p.ObtenerPorPosicion(pos);
            if (e === null)
                throw Error(`La posición ${pos} no está definida en la página actual`);
            return e;
        }

        private Buscar(numeroDePagina: number): number {
            for (let i: number = 0; i < this._paginas.length; i++)
                if (this._paginas[i].Pagina === numeroDePagina)
                    return i;
            return -1;
        }
    }

    export class GridDeDatos extends CrudBase {

        protected Ordenacion: Tipos.Ordenacion;
        public Navegador: Navegador;

        protected tituloQueEstoyMoviendo: HTMLDivElement = undefined;
        protected moviendomeEncimaDe = undefined;
        protected toolTipMoviendose = undefined

        protected columnaQueEstoyArrastrando: HTMLDivElement = undefined;
        protected posicionInicialX: number;
        protected desplazamientoX: number;

        private _infoSelector: InfoSelector;

        private _registrosLeidos: Map<number, any> = new Map<number, any>();
        public get InfoSelector(): InfoSelector {
            return this._infoSelector;
        }

        public DatosDelGrid: DatosDelGrid = new DatosDelGrid();

        private _idGrid: string;
        public get IdGrid(): string {
            return this._idGrid;
        }

        private _busqueConPregunta: boolean;
        private get BusqueConPregunta(): boolean {
            return this._busqueConPregunta;
        }

        private set BusqueConPregunta(value: boolean) {
            this._busqueConPregunta = value;
        }


        private _pregunta: string;
        public get HayPregunta(): boolean {
            return this.EsCrud && !IsNullOrEmpty(this._pregunta);
        }

        public get Pregunta(): string {
            return this._pregunta
        }

        public set Pregunta(pregunta: string) {
            this._pregunta = pregunta
        }

        private _nuevaConversacion: boolean = false;
        public get NuevaConversacion(): boolean {
            return this._nuevaConversacion;
        }
        public set NuevaConversacion(value: boolean) {
            this._nuevaConversacion = value;
        }

        private _preguntasIa: Array<Tipos.HistoricoIA> = new Array<Tipos.HistoricoIA>();

        public get Preguntas(): Array<Tipos.HistoricoIA> {
            return this._preguntasIa;
        }

        /** Devuelve el último objeto completo (pregunta + respuesta) o null */
        public get UltimaPregunta(): Tipos.HistoricoIA {
            return this._preguntasIa.length > 0 ? this._preguntasIa[this._preguntasIa.length - 1] : null;
        }

        public set UltimaPregunta(pregunta: string) {
            this.NuevaPregunta({ pregunta: pregunta, respuesta: null } as Tipos.HistoricoIA);
        }

        public set Respuesta(respuesta: string) {
            if (this.UltimaPregunta === null || IsNullOrEmpty(this.UltimaPregunta.pregunta))
                return;

            try {
                // 1. Limpieza inicial: Si el string viene con comillas literales al principio y final
                // (común cuando se recibe un string JSON-serializado desde el servidor)
                let raw = respuesta.trim();
                if (raw.startsWith('"') && raw.endsWith('"')) {
                    // Quitamos las comillas exteriores y parseamos una vez para limpiar escapes
                    raw = JSON.parse(raw);
                }

                // 2. Intentamos parsear el contenido para convertirlo en objeto
                let objetoJson = (typeof raw === 'string') ? JSON.parse(raw) : raw;

                // 3. Si el resultado sigue siendo un string (doble serialización), parseamos de nuevo
                if (typeof objetoJson === 'string') {
                    objetoJson = JSON.parse(objetoJson);
                }

                // 4. Asignamos el string formateado
                this.UltimaPregunta.respuesta = JSON.stringify(objetoJson, null, 4);

            } catch (e) {
                // Fallback: Si todo falla, intentamos una limpieza manual de caracteres de escape
                console.error("Error parseando respuesta IA", e);
                this.UltimaPregunta.respuesta = respuesta
                    .replace(/\\n/g, '\n')
                    .replace(/\\"/g, '"')
                    .replace(/^"|"$/g, ''); // Quita comillas al inicio y final
            }
        }

        /** * Añade una nueva interacción al historial. 
         * Se asegura de no repetir la misma pregunta consecutivamente.
         */
        public NuevaPregunta(interaccion: Tipos.HistoricoIA) {
            if (interaccion && interaccion.pregunta) {
                const ultima = this.UltimaPregunta;

                // Solo lo añadimos si es distinto a la última pregunta grabada
                if (!ultima || ultima.pregunta !== interaccion.pregunta) {
                    this._preguntasIa.push(interaccion);
                }
            }
        }

        protected get EsModalDeSeleccion(): boolean {
            return this.constructor.name === ModalSeleccion.name;
        }

        protected get EsModalParaConsultarRelaciones(): boolean {
            return this.constructor.name === ModalParaConsultarRelaciones.name;
        }

        protected get EsModalParaRelacionar(): boolean {
            return this.constructor.name === ModalParaRelacionar.name;
        }

        protected get EsModalParaImputar(): boolean {
            return this.constructor.name === ModalParaImputar.name;
        }

        protected get EsModalParaSeleccionar(): boolean {
            return this.constructor.name === ModalParaSeleccionar.name;
        }
        protected get EsModalConGrid(): boolean {
            return this.EsModalParaRelacionar || this.EsModalDeSeleccion || this.EsModalParaConsultarRelaciones || this.EsModalParaSeleccionar || this.EsModalParaImputar;
        }

        public get ChecksDeSeleccion(): NodeListOf<HTMLInputElement> {
            return document.getElementsByName(`${literal.id}.${this.IdGrid}`) as NodeListOf<HTMLInputElement>;
        }
        private _idCuerpoCabecera: string;
        public get IdCuerpoCabecera(): string {
            return !this.EsHistorial ? this._idCuerpoCabecera : 'cuerpo.cabecera.' + this._idCuerpoCabecera;
        }

        public get EsCrud(): boolean {
            return EsObjetoDe(this, CrudMnt);
        }

        public get SinMenus(): boolean {
            return this.EsHistorial || this.EsModalConGrid || this.ZonaDeMenu === null;
        }

        public get EsHistorial(): boolean {
            return EsObjetoDe(this, CrudHistorial);
        }

        public get IdCrud(): string {
            return this.IdCuerpoCabecera.replace('_mantenimiento', '');
        }
        public get CuerpoCabecera(): HTMLDivElement {
            return document.getElementById(this.IdCuerpoCabecera) as HTMLDivElement;
        }
        public get CuerpoDatos(): HTMLDivElement {
            return document.getElementById(`cuerpo.datos.${this.IdCuerpoCabecera}`) as HTMLDivElement;
        }
        public get CuerpoPie(): HTMLDivElement {
            return document.getElementById(`cuerpo.pie.${this.IdCuerpoCabecera}`) as HTMLDivElement;
        }
        public get IdNegocio(): number {
            return Numero(this.CuerpoCabecera.getAttribute(atMantenimniento.idNegocio));
        }

        private _idVista: number = undefined;
        public get IdVista(): number {
            if (!Definido(this._idVista)) {
                this._idVista = Numero(this.CuerpoCabecera.getAttribute(atMantenimniento.idVista));
            }
            return this._idVista;
        }

        public get NombreDeNegocio() {
            return this.CuerpoCabecera.getAttribute(atMantenimniento.negocio);
        }
        public get EnumeradoDeNegocio() {
            return ParsearEnumerado(enumNegocio, this.CuerpoCabecera.getAttribute(literal.enumNegocio));
        }
        public get Dto() {
            return this.CuerpoCabecera.getAttribute(atMantenimniento.dto);
        }
        private _idHtmlFiltro: string;
        public get PanelFiltro(): HTMLDivElement {
            return document.getElementById(this._idHtmlFiltro) as HTMLDivElement;
        }
        public get EtiquetaMostrarOcultarFiltro(): HTMLElement {
            return document.getElementById(`mostrar.${this.IdCuerpoCabecera}.ref`) as HTMLElement;
        }
        public get ExpandirFiltro(): HTMLInputElement {
            return document.getElementById(`expandir.${this.IdCuerpoCabecera}`) as HTMLInputElement;
        }
        public get InputSeleccionadas(): HTMLInputElement {
            let idInput = this.EsCrud
                ? `div.seleccion.${this.IdGrid}.input`
                : `div.seleccion.${this.IdGrid}.input`;
            return document.getElementById(idInput) as HTMLInputElement;
        }
        public get EtiquetasSeleccionadas(): HTMLElement {
            let idRef = this.EsCrud
                ? `div.seleccion.${this.IdGrid}.ref`
                : `div.seleccion.${this.IdGrid}.ref`;
            return document.getElementById(idRef) as HTMLElement;
        }
        public get EstaMostrandoLasSeleccionadas(): Boolean {
            return EsMayorDeCero(this.InputSeleccionadas.value);
        }

        public get ContenedorDeLosMenusDelCrud(): HTMLDivElement {
            return document.getElementById(`contenedor.${this.IdCuerpoCabecera}.MenuDelMnt`) as HTMLDivElement;
        }

        protected get Grid(): HTMLDivElement {
            return document.getElementById(this.IdGrid) as HTMLDivElement;
        }

        protected get CabeceraTablaGrid(): HTMLDivElement {
            let idCabecera = this.Grid.getAttribute(atGrid.cabeceraTabla);
            return document.getElementById(idCabecera) as HTMLDivElement;
        }

        public get CuerpoTablaGrid(): HTMLDivElement {
            return document.getElementById(`${this.Grid.id}_tbody`) as HTMLDivElement;
        }

        public get ModalEnviarElemento(): HTMLDivElement {
            return document.getElementById(`${this.IdCrud}-enviar_elemento`) as HTMLDivElement;
        }
        protected get ZonaNavegador(): HTMLDivElement {
            let idNavegador = this.Grid.getAttribute(atGrid.zonaNavegador);
            return document.getElementById(idNavegador) as HTMLDivElement;
        }

        public get Tabla(): HTMLDivElement {
            let idTabla: string = this.Grid.getAttribute(atControl.tablaDeDatos);
            return document.getElementById(idTabla) as HTMLDivElement;
        }

        private _anchos: Array<Parametro> = undefined;

        public get Anchos(): Array<Parametro> {
            return this._anchos;
        }
        public set Anchos(anchos: Array<Parametro>) {
            this._anchos = anchos;
        }

        private _filaCabecara: Array<ApiDeGrid.PropiedadesDeLaFila> = undefined;

        public get FilaCabecara(): Array<ApiDeGrid.PropiedadesDeLaFila> {
            if (!Definido(this._filaCabecara))
                this._filaCabecara = ApiDeGrid.ObtenerDescriptorDeLaCabecera(this.Tabla);
            return this._filaCabecara;
        }
        public set FilaCabecara(filaCabecera: Array<ApiDeGrid.PropiedadesDeLaFila>) {
            if (EsDispositvoMovil && !crudMnt.EsHistorial) {
                this._filaCabecara = filaCabecera;
                return;
            }

            if (!Definido(filaCabecera) && Definido(this._filaCabecara))
                this._filaCabecara = filaCabecera;
        }


        public get ContenedorMenuContextual(): HTMLDivElement {
            return document.getElementById(`${this.IdCuerpoCabecera}.${ltrMenus.menu.contextual}`) as HTMLDivElement;
        }

        public get ContenedorMenuDeFiltro(): HTMLDivElement {
            return document.getElementById(`${this.IdCuerpoCabecera}.${ltrMenus.menu.filtro}`) as HTMLDivElement;
        }

        public get ContenedorMenuIndividual(): HTMLDivElement {
            return document.getElementById(`${this.IdCuerpoCabecera}.${ltrMenus.menu.individual}`) as HTMLDivElement;
        }

        public get ContenedorMenuDeRelacion(): HTMLDivElement {
            return document.getElementById(`${this.IdCuerpoCabecera}.${ltrMenus.menu.relacion}`) as HTMLDivElement;
        }


        private _idHtmlZonaMenu: string;
        public get ZonaDeMenu(): HTMLDivElement {
            return document.getElementById(this._idHtmlZonaMenu) as HTMLDivElement;
        }

        public get OpcionesPorElemento(): NodeListOf<HTMLButtonElement> {
            return this.ZonaDeMenu?.querySelectorAll(`input[${atOpcionDeMenu.clase}="${enumCssOpcionMenu.DeElemento}"]`) as NodeListOf<HTMLButtonElement> ?? undefined;
        }

        private _idModal: string;
        public set IdModal(idModal: string) { this._idModal = idModal; };
        public get IdModal(): string { return this._idModal; };
        protected get Modal(): HTMLDivElement {
            return document.getElementById(this._idModal) as HTMLDivElement;
        };

        private _ctrlPulsado: boolean = false;

        constructor(idPanelMnt: string) {
            super(false);
            this._idCuerpoCabecera = idPanelMnt;

            if (!this.EsModalConGrid && this.CuerpoCabecera === null)
                throw Error(`No se puede crear el Crud ${idPanelMnt} la cabecera es nula`);
            if (!Definido(this.CuerpoCabecera))
                throw Error(`No se puede crear el Crud ${idPanelMnt} la cabecera es nula`);
            this._controlador = this.CuerpoCabecera.getAttribute(atMantenimniento.controlador);
            this._idGrid = this.CuerpoCabecera.getAttribute(atMantenimniento.gridDelMnt);
            this._idHtmlZonaMenu = this.CuerpoCabecera.getAttribute(atMantenimniento.zonaMenu);
            this._idHtmlFiltro = this.Grid.getAttribute(atMantenimniento.zonaDeFiltro);

            this.Navegador = new Navegador(this.IdGrid);
            this.Ordenacion = new Tipos.Ordenacion();
            this._infoSelector = new InfoSelector(this.IdGrid);
        }

        // Método para limpiar el estado del arrastre
        protected LimpiarEstadoArrastre(event: MouseEvent) {

            if (this.columnaQueEstoyArrastrando) {
                this.columnaQueEstoyArrastrando.classList.remove(ltrCss.crud.arrastrando);
                this.columnaQueEstoyArrastrando = undefined;
                this.FilaCabecara = undefined;
                this.desplazamientoX = 0;
            }

            if (this.tituloQueEstoyMoviendo) {
                this.tituloQueEstoyMoviendo = undefined;
                this.moviendomeEncimaDe = undefined;


            }

            if (this.toolTipMoviendose) {
                this.toolTipMoviendose.style.display = 'none';
            }
        }

        public QuitarRegistroLeido(id: number) {
            this._registrosLeidos.delete(id);
        }

        public Inicializar(idPanelMnt: string) {
            super.Inicializar(idPanelMnt);
            this.InicializarNavegador();

            this.Tabla.addEventListener('click', (event) => {
                if (event.ctrlKey) {
                    const target = event.target as HTMLElement;
                    if (target.tagName === 'A') {
                        var th = target.parentNode?.parentNode;
                        if (Definido(th) && th instanceof HTMLDivElement && th.classList.contains(ltrCss.crud.columna)) {
                            const classes = Array.from(target.classList);
                            var clase = classes.find(x => x.startsWith('ordenada'));
                            if (clase)
                                this.OrdenarPor(th.id, event);
                        }
                    }
                }
            });

            // Llamar a la función cuando se cargue la página
            window.addEventListener('load', this.AplicarTamanosAlEncolumnado);

            // Llamar a la función cuando se redimensione la ventana
            window.addEventListener('resize', () => {
                this.AplicarTamanosAlEncolumnado();
                if (Definido(crudMnt.ContenedorDeTablaConGraficos)) {

                    if (window.innerWidth < 1000)
                        ApiDelCrud.OcultarContenedorDeGraficos();
                    else {
                        if (ApiPanel.EsVisible(crudMnt.ContenedorDeGraficos))
                            ApiDelCrud.MostrarContenedorDeGraficos();
                        else
                            ApiDelCrud.OcultarContenedorDeGraficos();
                    }
                }
            });


            if (this.Estado.Contiene(atGrid.ordenacion)) {
                let a = JSON.parse(this.Estado.Sacar(atGrid.ordenacion));
                this.Ordenacion = Tipos.DeserializarOrdenacion(a);
            }

            if (this.Ordenacion.Count() === 0)
                this.ResetearOrdenacion();
        }

        public AplicarTamanosAlEncolumnado() {
            let grid = undefined;
            if (typeof window !== 'undefined' && this instanceof Window) {
                grid = crudMnt;
            } else {
                grid = this;
            }

            var tbodyElement = grid.EsModalConGrid ? grid.CuerpoTablaGrid : crudMnt.CuerpoTablaGrid;
            if (Definido(tbodyElement)) {
                ApiDeGrid.ReajustarUltimaColumna(grid.Tabla);
            }
        }

        private InicializarNavegador() {
            let elementos: Elemento[] = this.Estado.Obtener(ltrClaveDeEstado.ElementosSeleccionados) as Elemento[];

            if (elementos !== undefined) {
                for (var i = 0; i < elementos.length; i++) {
                    let e: Elemento = new Elemento(elementos[i]["_registro"]);
                    this.InfoSelector.InsertarElemento(e);
                }
                this.Estado.Quitar(ltrClaveDeEstado.ElementosSeleccionados);
            }

            this.Navegador.RestaurarDatos(this.Estado.Obtener(atGrid.id));
        }

        public MenuGrid_AnularOrdenacion() {
            this.Ordenacion.AnularOrdenacion();
            this.CargarGrid();
        }

        public MenuGrid_InicializarOrdenacion() {
            this.ResetearOrdenacion();
            this.CargarGrid();
        }

        protected ResetearOrdenacion() {
            this.Ordenacion.AnularOrdenacion();
            let ordenacionInicial = this.CuerpoCabecera.getAttribute(atControl.ordenInicial);
            let lista = ToLista(ordenacionInicial, ";");
            let columnas: NodeListOf<HTMLDivElement> = this.CabeceraTablaGrid.querySelectorAll(".div-th") as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < columnas.length; i++) {
                let columna = columnas[i];
                let propiedad: string = columna.getAttribute(atControl.propiedad);
                for (let j: number = 0; j < lista.length; j++) {
                    if (IsNullOrEmpty(lista[j]))
                        continue;
                    let partes = lista[j].split(":");
                    if (partes.length !== 3) {
                        MensajesSe.Error("ResetearOrdenacion", `La tripleta de ordenación ${lista[j]} está mal definida, ha de tener ternas separadas por ; con el patron siguiente: (Propiedad:OrdenarPor:Modo)`);
                        return;
                    }
                    if (partes[0] === propiedad)
                        this.Ordenacion.Actualizar(columna.id, propiedad, partes[2].trim(), partes[1].trim());
                }
            }
        }

        public PosicionarGrid(): void {
            let posicionGrid: number = this.PosicionGrid();
            this.Grid.style.top = `${posicionGrid}px`;

            let alturaDelGrid: number = this.AlturaDelGrid(posicionGrid);
            this.Grid.style.height = `${alturaDelGrid}px`;

            if (Definido(this.CuerpoTablaGrid)) this.FijarAlturaCuerpoDeLaTabla(alturaDelGrid);
        }

        private PosicionGrid(): number {
            let alturaCabeceraPnlControl: number = AlturaCabeceraPnlControl();
            let alturaCabeceraMnt: number = this.CuerpoCabecera.getBoundingClientRect().height;
            let alturaFiltro: number = 0;
            if (EsMayorDeCero(this.ExpandirFiltro.value)) {
                alturaFiltro = this.PanelFiltro.getBoundingClientRect().height;
            }
            return alturaCabeceraPnlControl + alturaCabeceraMnt + alturaFiltro;
        }

        private AlturaDelGrid(posicionGrid: number): number {
            let alturaPiePnlControl: number = AlturaPiePnlControl();
            let alturaZonaNavegador: number = this.ZonaNavegador.getBoundingClientRect().height;
            return AlturaFormulario() - posicionGrid - alturaPiePnlControl - alturaZonaNavegador;
        }

        /**
         le he puesto -9 ya que le he pintado bordes al cuerpo del grid
         */
        private FijarAlturaCuerpoDeLaTabla(alturaDelGrid: number): void {
            let tBody: HTMLDivElement = this.CuerpoTablaGrid;
            if (Definido(tBody)) {
                let alturaCabecera = this.CabeceraTablaGrid.getBoundingClientRect().height;
                //this.CuerpoTablaGrid.style.height = `${alturaDelGrid - alturaCabecera - 19}px`;
                //this.CuerpoTablaGrid.style.height = `${AlturaDelGrid() - alturaCabecera - 9}px`;
                //this.CuerpoTablaGrid.style.border = 'solid'
            }
        }

        protected ActualizarNavegadorDelGrid(accion: string, posicionDesdeLaQueSeLeyo: number, registrosLeidos: number) {
            this.Navegador.Actualizar(accion, posicionDesdeLaQueSeLeyo, registrosLeidos, this.InfoSelector.Cantidad);
        }

        protected EstablecerOrdenacion(idcolumna: string) {
            let htmlColumna: HTMLDivElement = document.getElementById(idcolumna) as HTMLDivElement;
            let modo: string = htmlColumna.getAttribute(atControl.modoOrdenacion);
            if (IsNullOrEmpty(modo))
                modo = enumModoOrdenacion.ascedente;
            else if (modo === enumModoOrdenacion.ascedente)
                modo = enumModoOrdenacion.descendente;
            else if (modo === enumModoOrdenacion.descendente)
                modo = enumModoOrdenacion.sinOrden;
            else if (modo === enumModoOrdenacion.sinOrden)
                modo = enumModoOrdenacion.ascedente;

            let propiedad: string = htmlColumna.getAttribute(atControl.propiedad);
            let ordenarPor: string = htmlColumna.getAttribute(atControl.ordenarPor);
            try {
                if (this._ctrlPulsado)
                    this.Ordenacion.Actualizar(idcolumna, propiedad, modo, ordenarPor);
                else
                    this.Ordenacion.Sustituir(idcolumna, propiedad, modo, ordenarPor);
            }
            finally {
                this._ctrlPulsado = false;
                crudMnt.GuardarOrdenacionDelResultado();
            }
        }

        protected ObtenerOrdenacion() {
            var clausulas = new Array<ClausulaDeOrdenacion>();
            for (var i = 0; i < this.Ordenacion.Count(); i++) {
                let orden = this.Ordenacion.Leer(i);
                clausulas.push(new ClausulaDeOrdenacion(orden.OrdenarPor, orden.Modo));
            }
            return JSON.stringify(clausulas);
        }

        protected ObtenerFiltroPorId(id: number): string {
            var clausulas = new Array<ClausulaDeFiltrado>();
            let clausula: ClausulaDeFiltrado = this.ObtenerClausulaPorId(id);
            clausulas.push(clausula);
            return JSON.stringify(clausulas);
        }

        protected ObtenerClausulaPorId(id: number) {
            let propiedad: string = atControl.id;
            let criterio: string = literal.filtro.criterio.igual;
            let valor: any = id;
            let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor);
            return clausula;
        }


        public ObtenerFiltrosParaLasSeleccionadas(): string {
            var clausulas = new Array<ClausulaDeFiltrado>();
            const propiedad: string = atControl.id;
            const criterio: string = atCriterio.esAlgunoDe;

            let valoresSeparadosPorPipe = "";
            const seleccionados = this.InfoSelector.IdsSeleccionados;
            for (let i = 0; i < seleccionados.length; i++) {
                valoresSeparadosPorPipe += seleccionados[i].toString() + ltrSimbolos.separadorDeEnteros;
            }

            const clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valoresSeparadosPorPipe);
            clausulas.push(clausula);
            return JSON.stringify(clausulas);
        }


        public ObtenerFiltros(operacion: string): string {
            var clausulas = new Array<ClausulaDeFiltrado>();
            var clausula: ClausulaDeFiltrado = null;

            let id: number = this.EsModalConGrid ? 0 : ObtenerParametroUrl(ltrParametrosUrl.id, 0);
            if (id > 0) {
                clausula = this.ObtenerClausulaPorId(id);
                clausulas.push(clausula);
                var negocio = ApiControl.BuscarRestrictor(this.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
                if (Definido(negocio)) {
                    let idNegocio = Numero(negocio.getAttribute(atControl.restrictor));
                    if (idNegocio > 0 && (NoDefinido(clausulas.find(c => c.clausula === ltrPropiedades.Negocio.idNegocio))))
                        clausulas.push(new ClausulaDeFiltrado(ltrPropiedades.Negocio.idNegocio, atCriterio.igual, idNegocio));
                }
            }
            else {
                let arrayIds: Array<string> = this.ObtenerElIdDeLosControlesDeFiltro();
                for (let i = 0; i < arrayIds.length; i++) {
                    var control: HTMLElement = document.getElementById(`${arrayIds[i]}`);
                    clausula = this.ObtenerClausulaDeFiltradoParaElControl(control);

                    //if (clausula !== null)
                    if (Definido(clausula))
                        clausulas.push(clausula);
                }

                this.FiltrosExcluyentes(operacion, clausulas);
                this.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);
            }

            return JSON.stringify(clausulas);
        }

        private ObtenerClausulaDeFiltradoParaElControl(control: HTMLElement) {
            var clausula: ClausulaDeFiltrado = null;
            var tipo: string = control.getAttribute(atControl.tipo);
            switch (tipo) {
                case ltrTipoControl.restrictorDeFiltro: {
                    clausula = this.ObtenerClausulaRestrictor(control as HTMLInputElement);;
                    break;
                }
                case ltrTipoControl.Editor: {
                    clausula = this.ObtenerClausulaEditor(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.ConEditor: {
                    clausula = this.ObtenerClausulaEditor(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.SelectorDeFiltro: {
                    clausula = this.ObtenerClausulaSelector(control as HTMLInputElement);;
                    break;
                }
                case ltrTipoControl.ListaDeElementos: {
                    clausula = this.ObtenerClausulaListaDeELemento(control as HTMLSelectElement);
                    break;
                }
                case ltrTipoControl.ListaDinamica: {
                    clausula = this.ObtenerClausulaListaDinamica(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.Check: {
                    clausula = this.ObtenerClausulaCheck(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.FiltroEntreFechas: {
                    clausula = this.ObtenerClausulaEntreFechas(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.FiltroEntreImportes: {
                    clausula = this.ObtenerClausulaEntreImportes(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.FiltroEntreRangos: {
                    clausula = this.ObtenerClausulaEntreRangos(control as HTMLInputElement);
                    break;
                }
                case ltrTipoControl.ListaDeValores: {
                    clausula = this.ObtenerClausulaListaDeValores(control as HTMLSelectElement);
                    break;
                }
                /* de un tipo */
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está implementado como definir la cláusula de filtrado de un tipo ${tipo}`);
                }
            }
            return clausula;
        }


        private ObtenerElIdDeLosControlesDeFiltro(): Array<string> {
            var arrayIds = new Array<string>();
            ApiPanel.AnadirIdsDeControlesDeFiltroDeTipoInput(this.PanelFiltro, arrayIds);
            ApiPanel.AnadirIdsDeControlesDeFiltroDeTipoSelect(this.PanelFiltro, arrayIds);
            if (!this.EsModalConGrid) {
                var panelesDeFiltrados: NodeListOf<HTMLDivElement> = Crud.crudMnt.Cuerpo.querySelectorAll(`div[${atControl.tipo}='${enumTipoDeModal.ModalDeFiltrado}']`) as NodeListOf<HTMLDivElement>;
                for (let i = 0; i < panelesDeFiltrados.length; i++) {
                    ApiPanel.AnadirIdsDeControlesDeFiltroDeTipoInput(panelesDeFiltrados[i], arrayIds);
                    ApiPanel.AnadirIdsDeControlesDeFiltroDeTipoSelect(panelesDeFiltrados[i], arrayIds);
                }
            }
            return arrayIds;
        }

        protected ObtenerColumnasOpcionales(): Array<string> {
            var columnas = new Array<string>();
            ApiPanel.AnadirColumnasAdicionales(this.PanelFiltro, columnas);

            var panelesDeFiltrados: NodeListOf<HTMLDivElement> = Crud.crudMnt.Cuerpo.querySelectorAll(`div[${atControl.tipo}='${enumTipoDeModal.ModalDeFiltrado}']`) as NodeListOf<HTMLDivElement>;
            for (let i = 0; i < panelesDeFiltrados.length; i++)
                ApiPanel.AnadirColumnasAdicionales(panelesDeFiltrados[i], columnas);

            var estadosColumnas = ApiDeGrid.Encolumnado(this.FilaCabecara);
            var visibles = estadosColumnas.filter(x => x.visible);
            visibles.forEach(col => {
                if (!columnas.includes(col.propiedad)) {
                    columnas.push(col.propiedad);
                }
            });

            return columnas;
        }

        private ObtenerClausulaRestrictor(restrictor: HTMLInputElement): ClausulaDeFiltrado {
            let propiedad: string = restrictor.getAttribute(atControl.propiedad);
            let criterio: string = literal.filtro.criterio.igual;
            let valor = restrictor.getAttribute(atControl.restrictor);
            let clausula: ClausulaDeFiltrado = null;
            if (!IsNullOrEmpty(valor))
                //clausula = { propiedad: `${propiedad}`, criterio: `${criterio}`, valor: `${valor}` };
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);

            return clausula;
        }

        private ObtenerClausulaEditor(editor: HTMLInputElement): ClausulaDeFiltrado {
            var propiedad: string = editor.getAttribute(atControl.propiedad);
            var criterio: string = editor.getAttribute(atControl.criterio);

            ////Si el control está deshabilitado, se vee si hay id restrictor detras de el nombre mapeado en el selector, si es así es que se ha indicado un id
            ////por ejemplo al pasar navegar de CPs a Municipio se restringe por IdCP pero se visualiza un CP
            //let restrictor: string = editor.disabled ? editor.getAttribute(atControl.restrictor) : ""; Numero(restrictor) > 0 ? restrictor :

            let valor: string = editor.value;
            var clausula = null;
            if (!IsNullOrEmpty(valor))
                //clausula = { propiedad: `${propiedad}`, criterio: `${criterio}`, valor: `${valor}` };
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);

            return clausula;
        }

        private ObtenerClausulaSelector(selector: HTMLInputElement): ClausulaDeFiltrado {
            var propiedad = selector.getAttribute(atControl.propiedad);
            var criterio = selector.getAttribute(atControl.criterio);
            var valor = null;
            var clausula = null;
            if (selector.hasAttribute(atSelectorDeFiltro.ListaDeSeleccionados)) {
                var ids = selector.getAttribute(atSelectorDeFiltro.ListaDeSeleccionados);
                if (!NoDefinido(ids)) {
                    valor = ids;
                    clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);
                }
            }
            return clausula;
        }

        private ObtenerClausulaListaDinamica(input: HTMLInputElement): ClausulaDeFiltrado {
            var propiedad = input.getAttribute(atControl.propiedad);
            var criterio = literal.filtro.criterio.igual;
            let valor: number = Numero(input.getAttribute(atListasDinamicas.idSeleccionado));

            var clausula = null;
            if (Number(valor) > 0 && !IsNullOrEmpty(input.value)) {
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
            }
            else {
                input.value = "";
            }
            return clausula;
        }

        private ObtenerClausulaCheck(input: HTMLInputElement): ClausulaDeFiltrado {
            if (input.getAttribute(atCheck.MostrarColumna) == "S")
                return undefined;
            let propiedad: string = input.getAttribute(atControl.propiedad);
            let criterio: string = literal.filtro.criterio.igual;
            let filtrarPorFalse = input.getAttribute(atCheck.filtrarPorFalse);
            let valor: boolean = input.checked;

            var clausula = null;
            if ((valor || (filtrarPorFalse === "S" && !valor)) && !IsNullOrEmpty(propiedad))
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
            return clausula;
        }

        private ObtenerClausulaEntreImportes(input: HTMLInputElement): ClausulaDeFiltrado {
            let propiedad: string = input.getAttribute(atControl.propiedad);
            let criterio: string = literal.filtro.criterio.entreImportes;
            let valor: string = ApiControl.LeerEntreImportes(input);
            var clausula = null;
            if (valor.trim() !== `${literal.undefined}${ltrSimbolos.separadorDeRangos}${literal.undefined}`) {
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);
            }
            return clausula;
        }

        private ObtenerClausulaEntreRangos(input: HTMLInputElement): ClausulaDeFiltrado {
            let propiedad: string = input.getAttribute(atControl.propiedad);
            let criterio: string = literal.filtro.criterio.entreRangos;
            let valor: string = ApiControl.LeerEntreRangos(input);
            var clausula = null;
            if (valor.trim() !== `${literal.undefined}${ltrSimbolos.separadorDeRangos}${literal.undefined}`) {
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);
            }
            return clausula;
        }

        private ObtenerClausulaEntreFechas(input: HTMLInputElement): ClausulaDeFiltrado {
            let propiedad: string = input.getAttribute(atControl.propiedad);
            let criterio: string = literal.filtro.criterio.entreFechas;
            let valor: string = ApiControl.LeerEntreFechas(input);
            var clausula = null;
            if (valor.trim() !== ltrSimbolos.separadorDeDosFechas) {
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);
            }
            return clausula;
        }

        private ObtenerClausulaListaDeELemento(selet: HTMLSelectElement): ClausulaDeFiltrado {
            var propiedad = selet.getAttribute(atControl.propiedad);
            var criterio = atCriterio.igual;
            var valor = selet.value;
            var clausula = null;
            if (!IsNullOrEmpty(valor) && Number(valor) > 0) {
                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);
            }
            return clausula;
        }

        private ObtenerClausulaListaDeValores(selet: HTMLSelectElement): ClausulaDeFiltrado {
            var propiedad = selet.getAttribute(atControl.propiedad);
            var tipo = selet.getAttribute(atControl.tipo);
            var criterio = atCriterio.igual;
            var valor = selet.value;
            var clausula = null;

            if (!IsNullOrEmpty(valor) && Number(valor) != -1) {

                // Verificamos si es una lista de valores de relación (9, 5, 6)
                if (tipo === ltrTipoControl.ListaDeValores && Number(valor) === 9) {
                    // Obtenemos los valores de todas las opciones presentes en el select
                    const valoresOpciones = Array.from(selet.options).map(opt => Number(opt.value));

                    // Verificamos si la lista contiene específicamente la tríada: 9, 5 y 6
                    const esListaDeRelacion = valoresOpciones.length === 3 && valoresOpciones.includes(9) &&
                        valoresOpciones.includes(5) &&
                        valoresOpciones.includes(6);

                    if (esListaDeRelacion) {
                        return null; // Si es la opción "Todo" de una relación, no filtramos
                    }
                }

                clausula = new ClausulaDeFiltrado(propiedad, criterio, valor);
            }

            return clausula;
        }


        public ActualizarInfoSelector(grid: GridDeDatos, elemento: Elemento): void {
            grid.InfoSelector.Quitar(elemento.Id);
            elemento = grid.DatosDelGrid.ObtenerPorId(elemento.Id);
            grid.InfoSelector.InsertarElemento(elemento);
            grid.Navegador.InformarElementosSeleccionados(grid.InfoSelector.Cantidad);
            grid.AplicarModoAccesoAlElemento(elemento);
        }

        protected AnadirAlInfoSelector(grid: GridDeDatos, elemento: Elemento): void {
            grid.InfoSelector.InsertarElemento(elemento);
            grid.Navegador.InformarElementosSeleccionados(grid.InfoSelector.Cantidad);
            grid.AplicarModoAccesoAlElemento(elemento);
        }

        protected QuitarDelSelector(grid: GridDeDatos, id: number): void {
            grid.InfoSelector.Quitar(id);
            grid.Navegador.InformarElementosSeleccionados(grid.InfoSelector.Cantidad);
            //this.ResetearMenuDeTransiciones();
        }

        protected EstaMarcado(idCheck: string): boolean {
            let id: number = this.ObtenerElIdDelElementoDelaFila(idCheck);
            return this.InfoSelector.Buscar(id) >= 0 ? true : false;
        }

        private ObtenerElIdDelElementoDelaFila(idCheck: string): number {
            let columnaId: string = idCheck.replace(`.${ltrMantenimiento.CheckDeSeleccion}`, `.${literal.id}`);
            let inputId: HTMLInputElement = document.getElementById(columnaId) as HTMLInputElement;
            let id: string = inputId.value;
            return Numero(id);
        }

        public BlanquearTodosLosCheck() {
            var celdasId = this.ChecksDeSeleccion;
            var len = celdasId.length;
            for (var j = 0; j < len; j++) {
                var idCheck = celdasId[j].id.replace(`.${atControl.id}`, `.${ltrMantenimiento.CheckDeSeleccion}`);
                var check = document.getElementById(idCheck);
                (<HTMLInputElement>check).checked = false;
            }
            this.InfoSelector.QuitarTodos();
        }

        protected ActualizarInformacionDelGrid(grid: GridDeDatos) {
            let actualizarInfoSelector: boolean = true;
            if (!grid.EsModalConGrid && !grid.EsHistorial && grid.Estado.Contiene(atGrid.idSeleccionado)) {
                let idSeleccionado: number = Numero(grid.Estado.Obtener(atGrid.idSeleccionado));
                try {
                    let elemento: Elemento = this.DatosDelGrid.ObtenerPorId(idSeleccionado);
                    grid.AnadirAlInfoSelector(grid, elemento);
                    actualizarInfoSelector = false;
                }
                catch (error) {
                    MensajesSe.MostraExcepcion(error);
                }
                finally {
                    grid.Estado.Quitar(atGrid.idSeleccionado);
                    grid.Estado.Quitar(atGrid.nombreSeleccionado);
                    grid.Estado.Guardar();
                }
            }
            ApiDeGrid.MarcarElementos(grid, actualizarInfoSelector);
        }

        protected obtenerValorDeLaFilaParaLaPropiedad(id: number, propiedad: string): string {
            let fila: HTMLDivElement = this.ObtenerFila(id);
            if (fila === null)
                return null;

            let celda: HTMLDivElement = ApiDeGrid.ObtenerCelda(fila, propiedad);
            let input: HTMLInputElement = celda.querySelector("input");
            if (input === null)
                throw Error(`la celda asociada a la propiedad '${propiedad}' no tiene un control input definido`);

            return input.value;
        }

        protected ObtenerCheckDelSeleccionado(id: number): HTMLInputElement {
            let fila: HTMLDivElement = this.ObtenerFila(id);
            let idCheckSel = fila.id + '.' + ltrMantenimiento.CheckDeSeleccion;
            return document.getElementById(idCheckSel) as HTMLInputElement;
        }

        private ObtenerFila(id: number): HTMLDivElement {
            let cuerpo: HTMLDivElement = this.Tabla.querySelector('.' + ltrCss.crud.tbody);
            let filas = cuerpo.querySelectorAll('.' + ltrCss.crud.fila) as NodeListOf<HTMLDivElement>;
            for (var i = 0; i < filas.length; i++) {
                let fila: HTMLDivElement = filas[i];
                let celdas = fila.querySelectorAll('.' + ltrCss.crud.celda) as NodeListOf<HTMLDivElement>;
                for (var j = 0; j < celdas.length; j++) {
                    let celda: HTMLDivElement = celdas[j];
                    let input: HTMLInputElement = celda.querySelector("input");
                    if (input !== null) {
                        let propiedad: string = input.getAttribute(atControl.propiedad);
                        if (propiedad.toLocaleLowerCase() === atControl.id) {
                            let valor: string = input.value;
                            if (Numero(valor) === id)
                                return fila;
                        }
                    }
                }
            }
            return null;

            //throw Error(`No se ha localizado una fila con la propiedad Id definida`);
        }

        public AntesDeNavegar(valores: Diccionario<any>) {
            super.AntesDeNavegar(valores);
            this.Estado.Agregar(atGrid.id, this.Navegador.Datos);

            let restrictores: Array<Tipos.Restrictor> = valores.Obtener(ltrClaveDeEstado.restrictoresDeUnPost) as Array<Tipos.Restrictor>;
            let idSeleccionado: number = valores.Obtener(ltrClaveDeEstado.idSeleccionado) as number;
            this.Estado.Agregar(atGrid.idSeleccionado, idSeleccionado);
            this.Estado.Agregar(atGrid.nombreSeleccionado, restrictores[0].Texto);
            this.Estado.Agregar(atGrid.ordenacion, JSON.stringify(this.Ordenacion));
            this.Estado.Agregar(ltrClaveDeEstado.EditarAlVolver, Crud.crudMnt.ModoTrabajo === enumModoTrabajo.editando);

            let paginaDestino: string = valores.Obtener(ltrClaveDeEstado.paginaDestino);
            let paraqueNavegar: string = valores.Obtener(ltrClaveDeEstado.paraqueNavegar);

            let restrictoresEnElFiltro = new Array<Tipos.Restrictor>();
            this.MapearRestrictoresDeFiltrado(this.PanelFiltro, restrictoresEnElFiltro);
            this.Estado.Agregar(ltrClaveDeEstado.restrictoresDeUnPost, restrictoresEnElFiltro);

            EntornoSe.Historial.GuardarValor(paginaDestino, ltrClaveDeEstado.restrictoresDeUnPost, restrictores).Guardar();
            EntornoSe.Historial.GuardarValor(paginaDestino, ltrClaveDeEstado.paginaOrigen, this.Pagina).Guardar();
            EntornoSe.Historial.GuardarValor(paginaDestino, ltrClaveDeEstado.paraqueNavegar, paraqueNavegar).Guardar();
        }

        private MapearRestrictoresDeFiltrado(panel: HTMLDivElement, restrictoresDeEstado: Array<Tipos.Restrictor>) {
            let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeFiltro}"]`) as NodeListOf<HTMLInputElement>;
            for (var i = 0; i < editores.length; i++) {
                let restrictor: HTMLInputElement = editores[i] as HTMLInputElement;
                if (!restrictor.disabled) continue;
                restrictoresDeEstado.push(new Tipos.Restrictor(restrictor.getAttribute(atControl.propiedad),
                    Numero(restrictor.getAttribute(atControl.restrictor)),
                    restrictor.value));
            }

            let listasDinamicas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
            for (var i = 0; i < listasDinamicas.length; i++) {
                let lista: HTMLInputElement = listasDinamicas[i] as HTMLInputElement;
                let id = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
                if (id == 0) continue;
                if (!lista.disabled) continue;
                restrictoresDeEstado.push(new Tipos.Restrictor(lista.getAttribute(atControl.propiedad), id, lista.value));
            }
        }

        public IrAlCrudDeDependencias(parametrosDeEntrada: string): void {

            try {
                let datos: Tipos.DatosParaDependencias = this.PrepararParametrosDeDependencias(this._infoSelector, parametrosDeEntrada);
                if (datos.FiltroRestrictor !== null)
                    ApiRuote.NavegarADependientes(this, datos.idOpcionDeMenu, datos.idSeleccionado, datos.FiltroRestrictor,
                        crudMnt.EstoyEditando && crudMnt.crudDeEdicion.HayCambiosPendientes);
            }
            catch (error) {
                MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, error.message);
                return;
            }
        }

        // permite relacionar un elemento con diferentes entidades
        // parametros de entrada:
        // idOpcionDeMenu --> id de la opción de menú que almacena los parámetros y la acción a someter
        // relacionarCon --> entidad con la que se relaciona
        // PropiedadRestrictora --> propiedad bindeada al control de filtro de la página de destino donde se mapea el restrictor seleccionado en el grid
        public IrAlCrudDeRelacionarCon(parametrosDeEntrada: string): void {
            try {
                let datos: Tipos.DatosParaRelacionar = this.PrepararParametrosDeRelacionarCon(this._infoSelector, parametrosDeEntrada);
                if (datos.FiltroRestrictor !== null)
                    ApiRuote.NavegarARelacionar(this, datos.idOpcionDeMenu, datos.idSeleccionado, datos.FiltroRestrictor);
            }
            catch (error) {
                MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, error.message);
                return;
            }
        }

        private PrepararParametrosDeDependencias(infoSelector: InfoSelector, parametros: string): Tipos.DatosParaDependencias {

            let partes = parametros.split('#');
            if (partes.length != 4)
                throw Error("Los parámetros de dependencias están mal definidos");

            if (crudMnt.ModoTrabajo !== enumModoTrabajo.editando && infoSelector.Cantidad < 1)
                throw Error("Debe seleccionar un elemento para poder gestionar sus dependencias");

            let elemento: Elemento = crudMnt.ModoTrabajo === enumModoTrabajo.editando
                ? crudMnt.crudDeEdicion.ElementoEditado
                : infoSelector.LeerElemento(0);

            let datos: Tipos.DatosParaDependencias = new Tipos.DatosParaDependencias();
            datos.idOpcionDeMenu = partes[0].split('==')[1];
            datos.DatosDependientes = partes[1].split('==')[1];
            datos.PropiedadQueRestringe = partes[2].split('==')[1];
            datos.PropiedadRestrictora = partes[3].split('==')[1];
            datos.idSeleccionado = elemento.Id;
            datos.MostrarEnElRestrictor = elemento.Texto;

            let valor = crudMnt.ModoTrabajo === enumModoTrabajo.editando
                ? ObtenerPropiedad(crudMnt.crudDeEdicion.Registro, datos.PropiedadQueRestringe)
                : this.obtenerValorDeLaFilaParaLaPropiedad(datos.idSeleccionado, datos.PropiedadQueRestringe);

            if (valor === null)
                this.LeerElementoParaGestionarSusDependencias(datos);
            else {
                let idRestrictor: number = Numero(valor);
                let filtro: Tipos.Restrictor = new Tipos.Restrictor(datos.PropiedadRestrictora, idRestrictor, datos.MostrarEnElRestrictor);
                datos.FiltroRestrictor.push(filtro);
            }
            return datos;
        }

        private PrepararParametrosDeRelacionarCon(infoSelector: InfoSelector, parametros: string): Tipos.DatosParaRelacionar {

            if (infoSelector.Cantidad != 1)
                throw Error("Debe seleccionar un elemento para poder relacionarlo");
            let partes = parametros.split('#');
            if (partes.length != 4)
                throw Error("Los parámetros de relación están mal definidos");


            let elemento: Elemento = infoSelector.LeerElemento(0);
            let datos: Tipos.DatosParaRelacionar = new Tipos.DatosParaRelacionar();
            datos.idOpcionDeMenu = partes[0].split('==')[1];
            datos.RelacionarCon = partes[1].split('==')[1];
            datos.PropiedadQueRestringe = partes[2].split('==')[1];
            datos.PropiedadRestrictora = partes[3].split('==')[1];
            datos.idSeleccionado = elemento.Id;
            datos.MostrarEnElRestrictor = elemento.Texto;

            let valorDeLaColumna = this.obtenerValorDeLaFilaParaLaPropiedad(datos.idSeleccionado, datos.PropiedadQueRestringe);

            if (valorDeLaColumna === null)
                this.LeerElementoParaRelacionar(datos);
            else {
                let idRestrictor: number = Numero(valorDeLaColumna);
                let filtro: Tipos.Restrictor = new Tipos.Restrictor(datos.PropiedadRestrictora, idRestrictor, datos.MostrarEnElRestrictor);
                datos.FiltroRestrictor = filtro;
            }
            return datos;
        }

        private LeerElementoParaGestionarSusDependencias(datos: Tipos.DatosParaDependencias) {
            let url: string = `/${this.Controlador}/${Ajax.EndPoint.LeerPorId}?${Ajax.Param.id}=${datos.idSeleccionado}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EndPoint.LeerPorId
                , datos
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , this.TrasLeerNavegarParaGestionarSusDependencias
                , null
            );

            a.Ejecutar();
        }

        private TrasLeerNavegarParaGestionarSusDependencias(peticion: ApiDeAjax.DescriptorAjax) {
            let grid: GridDeDatos = peticion.llamador as GridDeDatos;
            let datos: Tipos.DatosParaDependencias = peticion.DatosDeEntrada as Tipos.DatosParaDependencias;
            let idRestrictor: number = Numero(peticion.resultado.datos[datos.PropiedadQueRestringe]);
            let filtro: Tipos.Restrictor = new Tipos.Restrictor(datos.PropiedadRestrictora, idRestrictor, datos.MostrarEnElRestrictor);
            datos.FiltroRestrictor.push(filtro);
            ApiRuote.NavegarADependientes(grid, datos.idOpcionDeMenu, datos.idSeleccionado, datos.FiltroRestrictor,
                crudMnt.EstoyEditando && crudMnt.crudDeEdicion.HayCambiosPendientes);
        }

        private LeerElementoParaRelacionar(datos: Tipos.DatosParaRelacionar) {
            let url: string = `/${this.Controlador}/${Ajax.EndPoint.LeerPorId}?${Ajax.Param.id}=${datos.idSeleccionado}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EndPoint.LeerPorId
                , datos
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , this.TrasLeerNavegarParaRelacionar
                , null
            );

            a.Ejecutar();
        }

        private TrasLeerNavegarParaRelacionar(peticion: ApiDeAjax.DescriptorAjax) {
            let grid: GridDeDatos = peticion.llamador as GridDeDatos;
            let datos: Tipos.DatosParaRelacionar = peticion.DatosDeEntrada as Tipos.DatosParaRelacionar;
            let idRestrictor: number = Numero(peticion.resultado.datos[datos.PropiedadQueRestringe]);
            let filtro: Tipos.Restrictor = new Tipos.Restrictor(datos.PropiedadRestrictora, idRestrictor, datos.MostrarEnElRestrictor);
            datos.FiltroRestrictor = filtro;
            ApiRuote.NavegarARelacionar(grid, datos.idOpcionDeMenu, datos.idSeleccionado, datos.FiltroRestrictor);
        }


        /*
         * 
         * métodos para mapear los registros leidos a un dbgrid 
         * 
         */

        public DefinirParametrosParaCargarElGrid(accion: string): Array<Parametro> {
            let parametros: Array<Parametro> = new Array<Parametro>();
            if (this.BusqueConPregunta) {
                if (accion !== atGrid.accion.buscar) {
                    this.Pregunta = this.UltimaPregunta.pregunta;
                }
            }

            if (this.HayPregunta) {
                parametros.push(new Parametro(Ajax.Param.filtrarConIa, true));
                parametros.push(new Parametro(Ajax.Param.fraseDeFiltrado, this.Pregunta));
                parametros.push(new Parametro(Ajax.Param.guid, Crud.crudMnt.Guid));
                parametros.push(new Parametro(Ajax.Param.nuevaPregunta, this.NuevaConversacion));
            }
            parametros.push(new Parametro(Ajax.Param.filtro, this.ObtenerFiltros(ltrOperacion.CargarDatos)));
            parametros.push(new Parametro(Ajax.Param.orden, this.ObtenerOrdenacion()));
            parametros.push(new Parametro(Ajax.Param.ColumnasOpcionales, JSON.stringify(this.ObtenerColumnasOpcionales())));
            parametros.push(new Parametro(Ajax.Param.idVista, this.IdVista))
            parametros.push(new Parametro(Ajax.Param.Vista, this.EsCrud ? Crud.crudMnt.VistaMvc : ''))
            parametros.push(new Parametro(Ajax.Param.descriptor, this.EsCrud ? Crud.crudMnt.Descriptor : ''))

            if (accion === atGrid.accion.buscar) {
                if (!IsNullOrEmpty(this.Pregunta)) {
                    this.UltimaPregunta = this.Pregunta;
                    this.BusqueConPregunta = true;
                }
                else {
                    this.BusqueConPregunta = false;
                }
            }
            else this.Pregunta = null;

            return parametros;
        }

        public CargarPagina(numeroDePagina: number) {
            let cantidad: number = this.Navegador.Cantidad;
            let posicion: number = (numeroDePagina - 1) * cantidad;
            if (posicion <= 0) posicion = 0;

            let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.irA);
            let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.irA, posicion);
            ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.irA, posicion, this.Navegador.Cantidad, datosDeEntrada, parametros)
                .then(
                    (peticion) => {
                        this.DatosDelGrid.InicializarCache();
                        this.CrearFilasEnElGrid(peticion);
                    })
                .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));
        }

        public async GuardarCantidadPorLeer() {
            if (this.IdNegocio === 0 && this.IdVista === 0)
                return;

            const params2 = {
                [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, this.IdNegocio),
                [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, this.IdVista),
                [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.CantidadALeeer)
            };
            const url2 = `/${this.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
            await fetch(url2, {
                method: 'POST',
                body: this.CantidadPorLeer(),
                keepalive: true
            });
        }

        private CantidadPorLeer() {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosParaGuardar = this.Navegador.Cantidad;
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            return JSON.stringify(parametros);
        }

        public CompartirElemento() {

            if (this.InfoSelector.Cantidad != 1) {
                MensajesSe.Info("Ha de seleccionar un sólo elemento a compartir");
                return;
            }
            // Obtén la URL base sin parámetros
            const urlBase = new URL(window.location.origin + window.location.pathname);

            // Añade el parámetro Id usando searchParams (maneja codificación automática)
            urlBase.searchParams.set("Id", this.InfoSelector.IdsSeleccionados[0].toString());

            // URL final con el parámetro
            const urlConId = urlBase.toString();
            CopiarUrlAlPortapapeles(urlConId, "Url de acceso al elemento copiada al porta papeles");
            //
        }

        public EnviarElemento() {
            if (this.InfoSelector.Cantidad != 1) {
                MensajesSe.Info("Ha de seleccionar un sólo elemento a enviar");
                return;
            }
            Crud.crudMnt.ModalDePedirDatos_TrasAbrir(this.ModalEnviarElemento);
            ApiPanel.AbrirModal(this.ModalEnviarElemento);
        }

        public ObtenerUltimos() {
            if (this.EstaMostrandoLasSeleccionadas) {
                MensajesSe.Info('Mientras muestre las seleccionadas no sé usa la paginación');
                return;
            }
            let total: number = this.Navegador.Total;
            let cantidad: number = this.Navegador.Cantidad;
            let ultimaPagina: number = Math.ceil(total / cantidad);
            if (ultimaPagina <= 1)
                return;

            let posicion: number = (ultimaPagina - 1) * cantidad;
            if (posicion >= total)
                return;

            let paginaDeDatos = this.DatosDelGrid.PaginaDelGrid(ultimaPagina + 1);
            if (paginaDeDatos !== null && paginaDeDatos.Posicion === posicion && paginaDeDatos.Cantidad === cantidad) {
                this.ActualizarNavegadorDelGrid(atGrid.accion.ultima, posicion, paginaDeDatos.Registros.length);
                this.MapearPaginaCacheada(this, paginaDeDatos.Registros);
            }
            else {
                let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.ultima);
                let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.ultima, posicion);
                ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.ultima, posicion, this.Navegador.Cantidad, datosDeEntrada, parametros)
                    .then((peticion) => this.CrearFilasEnElGrid(peticion))
                    .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));
            }
        }

        public ObtenerAnteriores() {
            if (this.EstaMostrandoLasSeleccionadas) {
                MensajesSe.Info('Mientras muestre las seleccionadas no sé usa la paginación');
                return;
            }
            let cantidad: number = this.Navegador.Cantidad;
            let pagina: number = this.Navegador.NumeroDePaginaDelGrid;
            if (pagina == 1)
                return;

            let posicion: number = (pagina - 2) * cantidad;

            if (posicion < 0)
                posicion = 0;

            let paginaDeDatos = this.DatosDelGrid.PaginaDelGrid(pagina - 1);
            if (paginaDeDatos !== null && paginaDeDatos.Posicion === posicion && paginaDeDatos.Cantidad === cantidad) {
                this.ActualizarNavegadorDelGrid(atGrid.accion.anterior, posicion, paginaDeDatos.Registros.length);
                this.MapearPaginaCacheada(this, paginaDeDatos.Registros);
            }
            else {
                let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.anterior);
                let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.anterior, posicion);
                ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.anterior, posicion, this.Navegador.Cantidad, datosDeEntrada, parametros)
                    .then((peticion) => this.CrearFilasEnElGrid(peticion))
                    .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));
            }
        }

        public ObtenerSiguientes() {
            if (this.EstaMostrandoLasSeleccionadas) {
                MensajesSe.Info('Mientras muestre las seleccionadas no sé usa la paginación');
                return;
            }
            let cantidad: number = this.Navegador.Cantidad;
            let pagina: number = this.Navegador.NumeroDePaginaDelGrid;
            let total: number = this.Navegador.Total;
            let posicion: number = pagina * cantidad;
            if (posicion >= total)
                return;

            let paginaDeDatos = this.DatosDelGrid.PaginaDelGrid(pagina + 1);
            if (paginaDeDatos !== null && paginaDeDatos.Posicion === posicion && paginaDeDatos.Cantidad === cantidad) {
                this.ActualizarNavegadorDelGrid(atGrid.accion.siguiente, posicion, paginaDeDatos.Registros.length);
                this.MapearPaginaCacheada(this, paginaDeDatos.Registros);
            }
            else {
                let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.siguiente);
                let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.siguiente, posicion);
                ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.siguiente, posicion, this.Navegador.Cantidad, datosDeEntrada, parametros)
                    .then((peticion) => this.CrearFilasEnElGrid(peticion))
                    .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));
            }
        }

        public CargarGrid(): Promise<boolean> {
            this.GuardarCantidadPorLeer();
            return new Promise<boolean>((resolve, reject) => {
                if (this.EstaMostrandoLasSeleccionadas) {
                    this.MostrarFilasSeleccionadas(this.InfoSelector);
                    resolve(true);
                } else {
                    this.Navegador.NumeroDePaginaDelGrid = 1;
                    this.Navegador.Posicion = 0;
                    let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.buscar);
                    let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.buscar, 0);
                    ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.buscar, 0, this.Navegador.Cantidad, datosDeEntrada, parametros)
                        .then((peticion) => {
                            this.DatosDelGrid.InicializarCache();
                            this.CrearFilasEnElGrid(peticion);
                            this.Respuesta = peticion.resultado.consola;
                            resolve(true);
                        })
                        .catch((peticion) => {
                            this.SiHayErrorAlCargarElGrid(peticion);
                            reject(false);
                        });
                }
            });
        }

        protected SiHayErrorAlCargarElGrid(peticion: ApiDeAjax.DescriptorAjax) {
            let grid: GridDeDatos = peticion.llamador as GridDeDatos;
            try {
                MensajesSe.Error("SiHayErrorAlCargarElGrid", peticion?.resultado?.mensaje || peticion['message'], peticion?.resultado?.consola || peticion['message'] + '\n' + peticion['stack']);
            }
            finally {
                grid.Grid.setAttribute(atGrid.cargando, 'N');
            }
        }

        protected CrearFilaDelIdEnElGrid(peticion: ApiDeAjax.DescriptorAjax): boolean {
            let datosDeEntrada: DatosPeticionNavegarGrid = (peticion.DatosDeEntrada as DatosPeticionNavegarGrid);
            let grid: GridDeDatos = datosDeEntrada.Grid;
            grid.Estado.Agregar(atGrid.idSeleccionado, ObtenerParametroUrl(ltrParametrosUrl.id, 0, false));
            return this.CrearFilasEnElGrid(peticion);
        }

        protected CrearFilasEnElGrid(peticion: ApiDeAjax.DescriptorAjax): boolean {
            let datosDeEntrada: DatosPeticionNavegarGrid = (peticion.DatosDeEntrada as DatosPeticionNavegarGrid);
            let grid: GridDeDatos = datosDeEntrada.Grid;
            let lineasCreadas: boolean = true;
            try {
                let infoObtenida: ResultadoDeLectura = peticion.resultado.datos as ResultadoDeLectura;
                var registros = infoObtenida.registros;
                if (datosDeEntrada.Accion === atGrid.accion.buscar || datosDeEntrada.Accion === atGrid.accion.historial)
                    grid.Navegador.Total = infoObtenida.total;
                grid.ActualizarNavegadorDelGrid(datosDeEntrada.Accion, datosDeEntrada.PosicionDesdeLaQueSeLee, registros.length);
                let expresionMostrar: string = grid.Grid.getAttribute(atControl.expresionElemento).toLowerCase();
                grid.DatosDelGrid.AnadirPagina(grid.Navegador.NumeroDePaginaDelGrid, datosDeEntrada.PosicionDesdeLaQueSeLee, grid.Navegador.Cantidad, infoObtenida.registros, expresionMostrar);
                grid.MapearPaginaCacheada(grid, registros);
            }
            catch (error) {
                lineasCreadas = false;
                MensajesSe.Error("CrearFilasEnElGrid", `Error al crear las filas en el grid`, error.message);
            }
            finally {
                grid.Grid.setAttribute(atGrid.cargando, 'N');
                if (!grid.EsCrud && !lineasCreadas) {
                    grid.Modal.style.display = ltrStyle.display.none;
                }
            }
            return lineasCreadas;
        }

        protected MapearPaginaCacheada(grid: GridDeDatos, registros: Elemento[]): void {

            if (!grid.EsCrud && !grid.EsHistorial) {
                grid.Modal.style.display = ltrStyle.display.block;
            }

            grid.QuitarCuerpoALaTabla(grid);
            let cuerpo: HTMLDivElement = grid.CrearCuerpoDeLaTabla(grid, registros);
            try {
                grid.AnadirCuerpoALaTabla(grid, cuerpo);
                grid.ActualizarInformacionDelGrid(grid);
            }
            finally {
                if (grid.EsHistorial)
                    return;
                ApiDeGrid.ResetearAnchoDeTabla(grid.Tabla, grid.EsCrud);
                grid.AplicarTamanosAlEncolumnado();
            }
        }

        private CrearCuerpoDeLaTabla(grid: GridDeDatos, registros: any): HTMLDivElement {


            if (EsDispositvoMovil() && this.IdNegocio > 0)
                grid.FilaCabecara = ApiDeGrid.ObtenerDescriptorDeLaCabeceraMovil(grid.Tabla)
            else if (!Definido(grid.FilaCabecara))
                grid.FilaCabecara = ApiDeGrid.ObtenerDescriptorDeLaCabecera(grid.Tabla);

            let cuerpoDeLaTabla: HTMLDivElement = ApiDeGrid.DefinirCuerpoDeLaTabla(grid.Tabla, grid.Grid.id);

            for (let i = 0; i < registros.length; i++) {
                let fila: HTMLDivElement = grid.crearFila(grid.FilaCabecara, registros[i], i);

                if (grid instanceof CrudHistorial) {
                    const detalle = ObtenerPropiedad(registros[i], ltrPropiedades.Elemento.Historial.Detalle, '');
                    fila.setAttribute('title', detalle);
                    const clase = ObtenerPropiedad(registros[i], ltrPropiedades.Elemento.Historial.Clase, '');
                    const partes = clase.split(ltrSimbolos.separadorDeCss);
                    if (partes.length === 2)
                        ApiControl.IncluirCss(fila, ltrCss.crud.PanelHistorial.Color + '-' + partes[1]);

                }
                else
                    if (grid instanceof CrudMnt) {
                        const detalle = ObtenerPropiedad(registros[i], ltrPropiedades.Elemento.Descripcion, '');
                        if (detalle) {
                            fila.setAttribute('title', detalle);
                        }
                    }

                cuerpoDeLaTabla.append(fila);
            }
            return cuerpoDeLaTabla;
        }

        private QuitarCuerpoALaTabla(grid: GridDeDatos) {
            let tabla: HTMLDivElement = grid.Grid.querySelector(".div-tabla");
            let tbody: HTMLDivElement = tabla.querySelector(".div-tbody");
            if (!(tbody === null || tbody === undefined))
                tabla.removeChild(tbody);

        }

        private AnadirCuerpoALaTabla(grid: GridDeDatos, cuerpoDeLaTabla: HTMLDivElement) {
            let tabla: HTMLDivElement = grid.Grid.querySelector(".div-tabla");
            tabla.append(cuerpoDeLaTabla);
            grid.DatosDelGrid.PaginaActual = grid.Navegador.NumeroDePaginaDelGrid;
        }


        private crearFila(filaCabecera: ApiDeGrid.PropiedadesDeLaFila[], registro: any, numeroDeFila: number): HTMLDivElement {
            let fila = document.createElement("div");
            fila.id = `${this.IdGrid}_d_tr_${numeroDeFila}`;
            fila.classList.add(ltrCss.crud.fila);
            fila.classList.add(ltrCss.filaDelGrid);

            if (ObtenerPropiedad(registro, ltrPropiedades.Elemento.EstaCancelada, false))
                fila.classList.add(ltrCss.filaCancelada);
            else if (ObtenerPropiedad(registro, ltrPropiedades.Elemento.EstaTerminada, false))
                fila.classList.add(ltrCss.filaTerminada);
            else {
                if (crudMnt.ColorearFilas) {
                    const idEstado = Numero(ObtenerPropiedad(registro, ltrPropiedades.Elemento.DeProceso.IdEstado, 0));
                    if (idEstado > 0) {
                        const estadoIndex = (idEstado % 20) + 1; // Aseguramos que el rango sea de 1 a 20
                        fila.classList.add(ltrCss.filaEstado + '-' + estadoIndex.toString());
                    }
                }
                else {
                    const idTipo = Numero(ObtenerPropiedad(registro, ltrPropiedades.Elemento.ConTipo.IdTipo, 0));
                    if (idTipo > 0) {
                        const tipoIndex = (idTipo % 20) + 1; // Aseguramos que el rango sea de 1 a 20
                        fila.classList.add(ltrCss.filaEstado + '-' + tipoIndex.toString());
                    }
                }
            }

            let idDelElemento: number = 0;

            for (let j = 0; j < filaCabecera.length; j++) {

                let columnaCabecera: ApiDeGrid.PropiedadesDeLaFila = filaCabecera[j];

                let valor: any = ObtenerPropiedad(registro, columnaCabecera.propiedad, "", false);

                //if (EsDispositvoMovil() && columnaCabecera.propiedad === literal.nombre && this.IdNegocio > 0) {
                //    const estado: any = ObtenerPropiedad(registro, ltrPropiedades.Elemento.DeProceso.Estado, "", false);
                //    if (estado) valor = valor + '\n' + estado;
                //}

                if (columnaCabecera.propiedad === atControl.id) {
                    idDelElemento = Numero(valor);
                    if (idDelElemento <= 0)
                        throw Error("El id del elemento leido debe ser numérico mayor 0");
                }

                let celdaDelTd: HTMLDivElement = this.crearCelda(fila, columnaCabecera, numeroDeFila, j, valor);
                fila.append(celdaDelTd);
            }

            fila.setAttribute(atControl.idDelElemento, idDelElemento.toString());

            return fila;
        }

        private crearCelda(fila: HTMLDivElement, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, numeroDeFila: number, numeroDeCelda: number, valor: string): HTMLDivElement {
            let celdaDelTd: HTMLDivElement = ApiDeGrid.CrearCeldaDelTd(this.IdGrid, fila, numeroDeCelda, columnaCabecera, true);

            if (columnaCabecera.propiedad === ltrMantenimiento.CheckDeSeleccion)
                this.insertarCheckEnElTd(fila.id, celdaDelTd, columnaCabecera.propiedad);
            else if (columnaCabecera.tipo == ltrTipoControl.CirculoEnCelda) {
                MapearAlGrid.InsertarUnCirculoEnLaCelda(this.IdGrid, fila.id, columnaCabecera, celdaDelTd, valor);
            }
            else if (columnaCabecera.tipo == ltrTipoControl.Referencia && Definido(valor)) {
                MapearAlGrid.InsertarUnaReferenciaEnLaCelda(numeroDeFila, columnaCabecera, celdaDelTd, valor, 'valor');
            }
            else {
                this.insertarInputEnElTd(fila.id, columnaCabecera, celdaDelTd, valor);
            }

            let idCheckDeSeleccion: string = `${fila.id}.${ltrMantenimiento.CheckDeSeleccion}`;
            let eventoOnClick: string = this.definirPulsarCheck(idCheckDeSeleccion, celdaDelTd.id);
            celdaDelTd.setAttribute(atControl.eventoJs.onclick, eventoOnClick);

            if (!columnaCabecera.visible) celdaDelTd.classList.add(ltrCss.columnaOculta);

            return celdaDelTd;
        }

        private definirPulsarCheck(idCheckDeSeleccion: string, idControlHtml: string): string {
            let a: string = '';
            if (this.EsModalDeSeleccion) {
                let idModal: string = this.Grid.getAttribute(atSelectorDeFiltro.idModal);
                a = `${GestorDeEventos.deSeleccionDeFiltro}('fila-pulsada', '${idModal}#${idCheckDeSeleccion}#${idControlHtml}');`;
            }
            else
                if (this.EsModalParaRelacionar) {
                    let idModal: string = this.Grid.getAttribute(atSelectorDeFiltro.idModal);
                    a = `${GestorDeEventos.deCrearRelaciones}('${ltrEventos.ModalParaRelacionar.FilaPulsada}', '${idModal}#${idCheckDeSeleccion}#${idControlHtml}');`;
                }
                else
                    if (this.EsModalParaImputar) {
                        let idModal: string = this.Grid.getAttribute(atSelectorDeFiltro.idModal);
                        a = `${GestorDeEventos.paraImputar}('${ltrEventos.ModalParaImputar.FilaPulsada}', '${idModal}#${idCheckDeSeleccion}#${idControlHtml}');`;
                    }
                    else
                        if (this.EsModalParaSeleccionar) {
                            let idModal: string = this.Grid.getAttribute(atSelectorDeFiltro.idModal);
                            a = `${GestorDeEventos.paraSeleccionarElementos}('${ltrEventos.ModalParaSeleccionarElementos.FilaPulsada}', '${idModal}#${idCheckDeSeleccion}#${idControlHtml}');`;
                        }
                        else
                            if (this.EsModalParaConsultarRelaciones) {
                                let idModal: string = this.Grid.getAttribute(atSelectorDeFiltro.idModal);
                                a = `${GestorDeEventos.deConsultaDeRelaciones}('fila-pulsada', '${idModal}#${idCheckDeSeleccion}#${idControlHtml}');`;
                            }
                            else
                                if (this.EsCrud || this.EsHistorial)
                                    a = `${GestorDeEventos.delMantenimiento}('fila-pulsada', '${idCheckDeSeleccion}#${idControlHtml}');`;
                                else
                                    throw Error("No se ha definido el gestor de eventos a asociar a la pulsación de una fila en el grid");

            return a;
        }

        private insertarInputEnElTd(idFila: string, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, celdaDelTd: HTMLDivElement, valor: any) {
            let input = ApiDeGrid.CrearInputDeLaCelda(this.IdGrid, idFila, columnaCabecera, celdaDelTd, valor);

            let idCheckBox = `${idFila}.${ltrMantenimiento.CheckDeSeleccion}`;
            let eventoOnClick: string = this.definirPulsarCheck(idCheckBox, input.id);
            celdaDelTd.setAttribute(atControl.eventoJs.onclick, eventoOnClick);

            celdaDelTd.append(input);

        }

        private insertarCheckEnElTd(idFila: string, celdaDelTd: HTMLDivElement, propiedad: string) {
            let checkbox: HTMLInputElement = document.createElement('input');
            checkbox.type = "checkbox";
            checkbox.id = `${idFila}.${propiedad}`;
            checkbox.name = `${propiedad}.${this.IdGrid}`;
            checkbox.setAttribute(atControl.propiedad, `${propiedad}`);

            checkbox.style.border = "0px";
            checkbox.style.textAlign = "center";
            checkbox.style.width = "100%";
            checkbox.style.backgroundColor = "inherit";
            checkbox.style.display = ltrStyle.display.none;

            let eventoOnClick: string = this.definirPulsarCheck(checkbox.id, checkbox.id);
            celdaDelTd.setAttribute(atControl.eventoJs.onclick, eventoOnClick);

            checkbox.value = literal.false;
            /**/
            checkbox.disabled = true;
            celdaDelTd.append(checkbox);
        }

        public MenuGrid_SeleccionarTodasLasFilas() {
            let len: number = this.ChecksDeSeleccion.length;
            for (var j = 0; j < len; j++) {
                var idCheck = this.ChecksDeSeleccion[j].id.replace(`.${atControl.id}`, `.${ltrMantenimiento.CheckDeSeleccion}`);
                let check: HTMLInputElement = document.getElementById(idCheck) as HTMLInputElement;
                if (EsTrue(check.checked))
                    continue;
                //check.checked = true;
                ApiDeGrid.MarcarFila(check);
                /**/
                check.disabled = true;
                this.EventoDeFilaPulsada(idCheck, idCheck, false, true, false);
            }
        }

        private DesSeleccionarTodas(grid: GridDeDatos, ocultarContenedorDeraficos: boolean) {
            const len: number = grid.ChecksDeSeleccion.length;
            for (var j = 0; j < len; j++) {
                var idCheck = grid.ChecksDeSeleccion[j].id.replace(`.${atControl.id}`, `.${ltrMantenimiento.CheckDeSeleccion}`);
                let check: HTMLInputElement = document.getElementById(idCheck) as HTMLInputElement;
                if (!EsTrue(check.checked))
                    continue;
                ApiDeGrid.DesmarcarFila(check);
                check.disabled = true;
            }

            if (grid.EstaMostrandoLasSeleccionadas) {
                grid.MenuGrid_MostrarSoloSeleccionadas(grid);
            }

            grid.InfoSelector.QuitarTodos();
            grid.Navegador.InformarElementosSeleccionados(grid.InfoSelector.Cantidad);
            grid.ResetearMenusDeElemento();
            if (ocultarContenedorDeraficos) ApiDelCrud.OcultarContenedorDeGraficos();
        }

        public MenuGrid_DeselecionarTodasLasFilas(grid: GridDeDatos = null) {
            if (grid === null) {
                grid = this;
            }
            this.DesSeleccionarTodas(grid, true);
        }

        public MarcarRango(idCheck: string) {
            let fila = ApiControl.BuscarFila(document.getElementById(idCheck));
            if (ObtenerNumeroFinal(fila.id) > 0) {
                let tabla = ApiControl.BuscarTabla(fila);
                let tbody = tabla.querySelector<HTMLDivElement>('.' + ltrCss.crud.tbody);
                let tablarows = tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
                let i = ObtenerNumeroFinal(fila.id);
                while (i > 0) {
                    let fila = (tablarows[i]);
                    if (fila.classList.contains(ltrCss.filaSeleccionada))
                        break;
                    fila.classList.add(ltrCss.filaSeleccionada);
                    let check = fila.querySelector('input[type="checkbox"]') as HTMLInputElement;
                    check.checked = true;
                    let id: number = this.ObtenerElIdDelElementoDelaFila(check.id);
                    let elemento: Elemento = this.DatosDelGrid.ObtenerPorId(id);
                    this.AnadirAlInfoSelector(this, elemento);
                    i--;
                }
            }
            else {
                event['shiftKey'] = false;
                event['ctrlKey'] = false;
                this.FilaPulsada(idCheck, null, false);
            }
        }

        public FilaPulsada(idCheck: string, idDelInput: string, mostrarDto: boolean = false) {
            this.EventoDeFilaPulsada(idCheck, idDelInput, event['shiftKey'], event['ctrlKey'], mostrarDto);
        }

        public EventoDeFilaPulsada(idCheck: string, idDelInput: string, shiftKey: boolean, ctrlKey: boolean, mostrarDto: boolean = false) {
            let check: HTMLInputElement = document.getElementById(idCheck) as HTMLInputElement;

            //Se hace porque antes ha pasado por aquí por haber pulsado en la fila
            if (idCheck !== idDelInput) {
                if (!check.checked) {
                    if (shiftKey) {
                        this.MarcarRango(idCheck);
                        crudMnt.EditarEnPanelDeGraficos(mostrarDto);
                        return;
                    }
                    if (!ctrlKey && this.InfoSelector.Cantidad === 1) {
                        var modal = ApiDelCrud.ModalAbierta();
                        this.DesSeleccionarTodas(Definido(modal) ? crudMnt.ObtenerGrid(modal.id) : this, false);
                    }
                    ApiDeGrid.MarcarFila(check);
                }
                else ApiDeGrid.DesmarcarFila(check);
            }

            let id: number = this.ObtenerElIdDelElementoDelaFila(idCheck);
            if (check.checked) {
                const elemento: Elemento = this.DatosDelGrid.ObtenerPorId(id, false);
                if (elemento)
                    this.AnadirAlInfoSelector(this, elemento);
                else {
                    this.DesSeleccionarTodas(Definido(modal) ? crudMnt.ObtenerGrid(modal.id) : this, false);
                    crudMnt.CargarGrid();
                }
            }
            else {
                this.ResetearInfoSelectorParaElId(id);
            }
            crudMnt.EditarEnPanelDeGraficos(mostrarDto);

            /**/
            check.disabled = true;
        }

        private ResetearInfoSelectorParaElId(id: number) {
            this.QuitarDelSelector(this, id);
            if (this.InfoSelector.Cantidad >= 1) {
                let e: Elemento = this.InfoSelector.LeerElemento(0);
                this.AplicarModoAccesoAlElemento(e);
            }

            if (this.InfoSelector.Cantidad === 0 && (this instanceof ModalConGrid) === false) {
                this.ResetearMenusDeElemento();
            }
        }

        private ResetearMenusDeElemento() {
            if (this.SinMenus) {
                return;
            }
            ApiDeInicializacion.InicializarOpcionesDeMenuDeElemento(this.ZonaDeMenu);
            ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuIndividual, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeElemento, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
            ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.crud, this.ContenedorMenuIndividual, false, Crud.crudMnt.ModoAccesoAlNegocio);
            ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuDeRelacion, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeElemento, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
            ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.crud, this.ContenedorMenuDeRelacion, false, Crud.crudMnt.ModoAccesoAlNegocio);
            //this.ResetearMenuDeTransiciones();
        }

        private ResetearMenuDeTransiciones() {
            if (!Definido(Crud.crudMnt.OpcionTransitar) || !Crud.crudMnt.OpcionTransitar || this.InfoSelector.Cantidad <= 0)
                return;

            var idEstadoPatron = ObtenerPropiedad(Crud.crudMnt.InfoSelector.Seleccionados[0].Registro, ltrPropiedades.Elemento.DeProceso.IdEstado);
            for (var i = 1; i < Crud.crudMnt.InfoSelector.Cantidad; i++) {
                if (idEstadoPatron !== ObtenerPropiedad(Crud.crudMnt.InfoSelector.Seleccionados[i].Registro, ltrPropiedades.Elemento.DeProceso.IdEstado)) {
                    ApiDeMenuFlotante.HabilitarOpcionMf(document.getElementById(ltrMenus.IdsDeOpcinesMf.idTransitar) as HTMLLIElement, false, ltrMenus.enumOrigen.crud);
                    return;
                }
            }
            ApiDeMenuFlotante.HabilitarOpcionMf(document.getElementById(ltrMenus.IdsDeOpcinesMf.idTransitar) as HTMLLIElement, true, ltrMenus.enumOrigen.crud);
        }

        public AplicarModoAccesoAlElemento(elemento: Elemento): void {
            if (this.EsHistorial || (this.EsCrud && crudMnt.SoloConGrid))
                return;


            var parametros = new Array<Parametro>();
            if (this.IdNegocio > 0) {
                parametros.push(new Parametro(Ajax.Param.idNegocio, this.IdNegocio));
                if (ExistePropiedad(elemento.Registro, ltrPropiedades.Elemento.IdElemento))
                    parametros.push(new Parametro(Ajax.Param.idElemento, ObtenerPropiedad(elemento.Registro, ltrPropiedades.Elemento.IdElemento)));
            }

            if (this._registrosLeidos.has(elemento.Id)) {
                this.DespuesDeLeerFilaSeleccionadaInterno(this, this._registrosLeidos.get(elemento.Id));
            }
            else {
                ApiDePeticiones.LeerElementoPorId(this, this.Controlador, elemento.Id, parametros, new Array<Parametro>())
                    .then((peticion) => {
                        this.DespuesDeLeerFilaSeleccionada(peticion);
                        elemento.Registro = peticion.resultado.datos;
                        this._registrosLeidos.set(elemento.Id, peticion.resultado.datos);

                        if (Numero(ObtenerParametroUrl(ltrParametrosUrl.id, 0, false)))
                            crudMnt.IraEditar();

                    })
                    .catch((peticion) => {
                        ApiControl.BloquearMenu(crudMnt.Cuerpo);
                        ApiDePeticiones.EmitirError(peticion);
                    });
            }

        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax) {
            this.DespuesDeLeerFilaSeleccionadaInterno(peticion.llamador, peticion.resultado.datos);
        }

        private DespuesDeLeerFilaSeleccionadaInterno(grid: GridDeDatos, elemento: any): any {

            if (this.SinMenus) {
                return;
            }

            if (Registro.EsClienteWeb()) {
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                ApiDeMenuFlotante.AplicarClienteWeb(Crud.crudMnt.ContenedorMenuIndividual, Crud.crudMnt.InfoSelector.Cantidad);
                return;
            }

            let modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos = ModoAcceso.Parsear(ObtenerPropiedad(elemento, ltrPropiedades.Elemento.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false));

            let opcionesDeElemento: NodeListOf<HTMLButtonElement> = grid.OpcionesPorElemento;
            if (Definido(opcionesDeElemento)) for (var i = 0; i < opcionesDeElemento.length; i++) {
                let opcion: HTMLButtonElement = opcionesDeElemento[i];
                ModoAcceso.AplicarModoAccesoAlElemento(opcion, grid.InfoSelector.Cantidad, modoAcceso);
            }

            let usaBaja = ExistePropiedad(elemento, ltrPropiedades.baja);
            let estaDeBaja = usaBaja ? ObtenerPropiedad(elemento, ltrPropiedades.baja, false) : false;

            if (!this.SinMenus) {
                if (Definido(grid.ContenedorDeLosMenusDelCrud))
                    ApiDeMenuFlotante.AplicarModoAcceso(ltrMenus.enumOrigen.crud, grid.ContenedorDeLosMenusDelCrud, grid.InfoSelector.Cantidad, modoAcceso);
                if (Definido(grid.ContenedorMenuIndividual))
                    ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.crud, grid.ContenedorMenuIndividual, estaDeBaja, Crud.crudMnt.ModoAccesoAlNegocio);
                if (Definido(grid.ContenedorMenuDeRelacion))
                    ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.crud, grid.ContenedorMenuDeRelacion, estaDeBaja, Crud.crudMnt.ModoAccesoAlNegocio);

                if (grid.InfoSelector.Cantidad === 1) {
                    let delSistema = ObtenerPropiedad(elemento, ltrPropiedades.DelSistema, false);
                    if (delSistema) {
                        var crud = (grid as CrudMnt);
                        ApiDeMenuFlotante.BloquearOpcionDeMenu(crud.ContenedorMenuIndividual, ltrMenus.eventosDeMf.alta, ltrMenus.enumOrigen.crud);
                        ApiDeMenuFlotante.BloquearOpcionDeMenu(crud.ContenedorMenuIndividual, ltrMenus.eventosDeMf.baja, ltrMenus.enumOrigen.crud);
                    }
                }
                grid.ResetearMenuDeTransiciones();
            }
        }

        protected LeerElementoSeleccionado(id: number): void {

            ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, new Array<Parametro>, id)
                .then(x => { })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));

            //let url: string = `/${this.Controlador}/${Ajax.EndPoint.LeerPorId}?${Ajax.Param.id}=${id}`;
            //let a = new ApiDeAjax.DescriptorAjax(this
            //    , Ajax.EndPoint.LeerPorId
            //    , id
            //    , url
            //    , ApiDeAjax.TipoPeticion.Asincrona
            //    , ApiDeAjax.ModoPeticion.Get
            //    , this.TrasLeerElementoSeleccionado
            //    , null
            //);

            //a.Ejecutar();
        }

        private TrasLeerElementoSeleccionado(peticion: ApiDeAjax.DescriptorAjax) {
            let grid: GridDeDatos = peticion.llamador as GridDeDatos;
            grid.AnadirAlInfoSelector(grid, peticion.resultado.datos);
        }

        protected AccederAlModoDeAccesoAlElemento(id: number): void {
            let url: string = this.DefinirPeticionDeLeerModoDeAccesoAlElemento(id);
            let datosDeEntrada = `{"Negocio":"${this.NombreDeNegocio}","id":"${id}"}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EndPoint.LeerModoDeAccesoAlElemento
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , this.AplicarModoDeAccesoAlElemento
                , ApiDePeticiones.EmitirError
            );

            a.Ejecutar();
        }

        private AplicarModoDeAccesoAlElemento(peticion: ApiDeAjax.DescriptorAjax) {
            let mantenimiento: CrudMnt = peticion.llamador as CrudMnt;
            let modoDeAccesoDelUsuario: string = peticion.resultado.modoDeAcceso;
        }

        private DefinirPeticionDeLeerModoDeAccesoAlElemento(id: number): string {
            let url: string = `/${this.Controlador}/${Ajax.EndPoint.LeerModoDeAccesoAlElemento}`;
            let parametros: string = `${Ajax.Param.nombreDeNegocio}=${this.NombreDeNegocio}&${Ajax.Param.id}=${id}`;
            let peticion: string = url + '?' + parametros;
            return peticion;
        }

        public OrdenarPor(columna: string, event: Event | undefined): void {

            if (Definido(event) && event['ctrlKey'] === true) {
                this._ctrlPulsado = true;
            }

            this.EstablecerOrdenacion(columna);
            this.CargarGrid();

            if (this.toolTipMoviendose) {
                this.toolTipMoviendose.style.display = 'none';
            }
        }


        public PosicionarLaColumna(idOrigen: string, idDestino) {
            if (idOrigen === idDestino)
                return;

            let thead: HTMLDivElement = this.Tabla.querySelector<HTMLDivElement>('.' + ltrCss.crud.thead);
            let encabezado: HTMLDivElement = thead.querySelector<HTMLDivElement>('.' + ltrCss.crud.encolumnado);
            let columnas: NodeListOf<HTMLDivElement> = this.Tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.columna)
            let thOrigen = document.getElementById(idOrigen).parentNode as HTMLDivElement;
            let thDestino = document.getElementById(idDestino).parentNode as HTMLDivElement;

            let haciaLaDerecha = this.EstaLaColumnaAntesQue(Array.from(columnas), thOrigen, thDestino);
            try {
                let cuerpo = this.Tabla.querySelectorAll('.' + ltrCss.crud.tbody)[0] as HTMLDivElement;
                let filas = cuerpo.querySelectorAll('.' + ltrCss.crud.fila) as NodeListOf<HTMLDivElement>;
                if (!haciaLaDerecha) {
                    if (!this.PosicionarEncabezadoIzquierda(encabezado, thOrigen, thDestino))
                        return;
                    for (let i = 0; i < filas.length; i++) {
                        let fila = filas[i];
                        this.PosicionarDatosIzquierda(fila, thOrigen, thDestino);
                    }
                }
                else {
                    if (!this.PosicionarEncabezadoDerecha(encabezado, thOrigen, thDestino))
                        return;
                    for (let i = 0; i < filas.length; i++) {
                        let fila = filas[i];
                        this.PosicionarDatosDerecha(fila, thOrigen, thDestino);
                    }
                }
            }
            finally {
                this.DatosDelGrid.InicializarCache();
                this.FilaCabecara = undefined;
            }
        }

        private EstaLaColumnaAntesQue(columnas: Array<HTMLDivElement>, thOrigen: HTMLDivElement, thDestino: HTMLDivElement): boolean {
            let indexOrigen = columnas.indexOf(thOrigen);
            let indexDestino = columnas.indexOf(thDestino);
            return indexOrigen < indexDestino;
        }

        private PosicionarEncabezadoIzquierda(encabezado: HTMLDivElement, columnaQueMuevo: HTMLDivElement, columnaSobreLaQueMuevo: HTMLDivElement): boolean {

            let posicionSobreLaQueMuevo = Array.from(encabezado.children).findIndex(child => child.id === columnaSobreLaQueMuevo.id);
            if (posicionSobreLaQueMuevo <= 0 || posicionSobreLaQueMuevo >= encabezado.children.length) {
                return false; // No se puede mover a la posición 0 o si no se encuentra thDestino
            }

            // Encontrar la posición de thOrigen
            let posicionDeLaQueMuevo = Array.from(encabezado.children).findIndex(child => child.id === columnaQueMuevo.id);
            if (posicionDeLaQueMuevo <= 0 || posicionDeLaQueMuevo >= encabezado.children.length) {
                return false; // No se puede mover desde la posición 0 o si no se encuentra thOrigen
            }

            // Mover la columna
            let columna = encabezado.removeChild(encabezado.children[posicionDeLaQueMuevo]);
            encabezado.insertBefore(columna, encabezado.children[posicionSobreLaQueMuevo]);
            return true;
        }

        private PosicionarEncabezadoDerecha(encabezado: HTMLDivElement, columnaQueMuevo: HTMLDivElement, columnaSobreLaQueMuevo: HTMLDivElement): boolean {
            let posicionSobreLaQueMuevo = Array.from(encabezado.children).findIndex(child => child.id === columnaSobreLaQueMuevo.id);
            if (posicionSobreLaQueMuevo < 0) {
                return false; // No se puede mover si no se encuentra columnaSobreLaQueMuevo o es la última
            }

            let posicionDeLaQueMuevo = Array.from(encabezado.children).findIndex(child => child.id === columnaQueMuevo.id);
            if (posicionDeLaQueMuevo <= 0 || posicionDeLaQueMuevo >= encabezado.children.length) {
                return false; // No se puede mover desde la posición 0 o si no se encuentra columnaQueMuevo
            }

            // Mover la columna
            let columna = encabezado.removeChild(encabezado.children[posicionDeLaQueMuevo]);
            encabezado.insertBefore(columna, encabezado.children[posicionSobreLaQueMuevo]);
            return true;
        }


        private PosicionarDatosIzquierda(fila: HTMLDivElement, thOrigen: HTMLDivElement, thDestino: HTMLDivElement) {
            const tdOrigen = fila.querySelector<HTMLDivElement>(`.${ltrCss.crud.celda}[headers="${thOrigen.id}"]`);
            const tdDestino = fila.querySelector<HTMLDivElement>(`.${ltrCss.crud.celda}[headers="${thDestino.id}"]`);

            if (tdOrigen && tdDestino) {
                const posicionDestino = Array.from(fila.children).findIndex(child => child === tdDestino);
                if (posicionDestino > 0) {
                    fila.insertBefore(tdOrigen, tdDestino);
                }
            }
        }

        private PosicionarDatosDerecha(fila: HTMLDivElement, thOrigen: HTMLDivElement, thDestino: HTMLDivElement) {
            const tdOrigen = fila.querySelector<HTMLDivElement>(`.${ltrCss.crud.celda}[headers="${thOrigen.id}"]`);
            const tdDestino = fila.querySelector<HTMLDivElement>(`.${ltrCss.crud.celda}[headers="${thDestino.id}"]`);
            if (tdOrigen && tdDestino) {
                const posicionDestino = Array.from(fila.children).findIndex(child => child === tdDestino);
                if (posicionDestino < fila.children.length) {
                    fila.insertBefore(tdOrigen, tdDestino.nextElementSibling);
                } else {
                    fila.appendChild(tdOrigen);
                }
            }
        }



        public MenuGrid_MostrarSoloSeleccionadas(grid: GridDeDatos): void {
            let inputDeSeleccionadas: HTMLInputElement = grid.InputSeleccionadas;
            let etiquetaSeleccionadas: HTMLElement = grid.EtiquetasSeleccionadas;

            if (grid.EstaMostrandoLasSeleccionadas) {
                inputDeSeleccionadas.value = literal.cero;
                etiquetaSeleccionadas.innerText = ltrMantenimiento.mostrarSoloSeleccionadas;
                let pagina = Numero(inputDeSeleccionadas.getAttribute(atGrid.ultimaPaginaMostrada));
                grid.CargarPagina(pagina);
            }
            else {
                inputDeSeleccionadas.value = literal.uno;
                etiquetaSeleccionadas.innerText = ltrMantenimiento.mostrarTodasLasFilas;
                inputDeSeleccionadas.setAttribute(atGrid.ultimaPaginaMostrada, this.Navegador.NumeroDePaginaDelGrid.toString());
                grid.MostrarFilasSeleccionadas(grid.InfoSelector);
            }
        }

        protected ResetearSoloSeleccionadas(grid: GridDeDatos): void {
            if (!grid.EstaMostrandoLasSeleccionadas)
                return;

            grid.InputSeleccionadas.value = literal.cero;
            grid.EtiquetasSeleccionadas.innerText = ltrMantenimiento.mostrarSoloSeleccionadas;
            grid.MostrarFilasSeleccionadas(grid.InfoSelector);
        }

        public TeclaPulsada(grid: GridDeDatos, e): void {
            if (e.keyCode === 13 && !e.shiftKey) {
                if (ObtenerParametroUrl(ltrParametrosUrl.id, 0, false) == 0) {
                    if (crudMnt.ModoTrabajo === enumModoTrabajo.historial || (crudMnt.HayHistorial && crudMnt.EstoyEditandoConsultando)) EventosDelMantenimiento(ltrEventos.Mnt.Buscar, '');
                    else {
                        grid.MenuGrid_DeselecionarTodasLasFilas(grid);
                        grid.CargarGrid();
                    }
                }
                e.preventDefault();
            }
        }

        public OcultarMostrarColumnas(propiedades: string[]): void {
            let cuerpos = this.Tabla.querySelectorAll('.' + ltrCss.crud.tbody) as NodeListOf<HTMLDivElement>;
            if (cuerpos.length === 0)
                return;
            try {
                for (let i: number = 0; i < propiedades.length; i++) {
                    ApiDeGrid.OcultarMostrarColumna(this.Tabla, propiedades[i]);
                }
                this.FilaCabecara = undefined;
            }
            finally {
                //ApiDeGrid.ReajustarTamanoColumnas(this.Tabla);
                ApiDeGrid.ReajustarUltimaColumna(this.Tabla);
            }
        }

        private MostrarFilasSeleccionadas(seleccionadas: InfoSelector): void {
            let parametros: Array<Parametro> = this.DefinirParametrosParaCargarElGrid(atGrid.accion.seleccionadas);
            let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.seleccionadas, 0);
            parametros.push(new Parametro(atGrid.accion.seleccionadas, JSON.stringify(seleccionadas.IdsSeleccionados)));
            ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.seleccionadas, 0, this.InfoSelector.Seleccionados.length, datosDeEntrada, parametros)
                .then((peticion) => this.CrearFilasEnElGrid(peticion))
                .catch((peticion) => this.SiHayErrorAlCargarElGrid(peticion));

        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            if (!esContextual) {
                if (this.InfoSelector.Cantidad === 0)
                    MensajesSe.EmitirExcepcion('IncluirParametrosParaProcesarOpcionMf', `Para ejecutar la opción ${opcion} debe seleccionar algún elemento`);

                parametros.push(new Parametro(Ajax.Param.ids, this.InfoSelector.IdsSeleccionados));
                datosDeEntrada.push(new Parametro(Ajax.Param.ids, this.InfoSelector.IdsSeleccionados));
                datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.Textos, this.InfoSelector.TextosSeleccionados));
            }

            if (esContextual && !opcion.startsWith(`${ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla}_`)) {
                parametros.push(new Parametro(Ajax.Param.filtro, this.ObtenerFiltros(ltrOperacion.CargarDatos)));
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion !== ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                let esContextual: boolean = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.esContextual);
                if (esContextual) {
                    let crud: CrudMnt = peticion.llamador as CrudMnt;
                    crud.InfoSelector.QuitarTodos();
                    crud.RestaurarPagina();
                    return super.DespuesDeProcesarOpcionMf(peticion);
                }
                else
                    if (opcion === ltrMenus.eventosDeMf.Comun.Imprimir) {
                        if (ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.SisDoc.PlantillasDisponibles.Abrir) !== false)
                            crudMnt.ModalImprimir_Abrir(this.InfoSelector.IdsSeleccionados[0], ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.SisDoc.PlantillasDisponibles.Plantillas));
                        return true
                    }
            }
            return super.DespuesDeProcesarOpcionMf(peticion);
        }

    }

}