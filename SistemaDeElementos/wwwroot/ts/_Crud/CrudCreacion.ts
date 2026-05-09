namespace Crud {

    export class CrudCreacion extends CrudBase {

        private _idPanelCreacion: string;

        public CrudDeMnt: CrudMnt;
        Altura: number;

        public get PanelDeCrear(): HTMLDivElement {
            return document.getElementById(this._idPanelCreacion) as HTMLDivElement;
        }

        public get TablaDeCreacion(): HTMLDivElement {
            return document.getElementById(this.PanelDeCrear.getAttribute(atControl.tablaDeControles)) as HTMLDivElement;
        }

        private _contenedorCabecera = null;
        public get ContenedorDeCabecera(): HTMLDivElement {
            if (!Definido(this._contenedorCabecera)) {
                this._contenedorCabecera = this.PanelDeCrear.querySelector('.' + ltrCss.crud.contenedorEdicionCreacion) as HTMLDivElement
            }
            return this._contenedorCabecera as HTMLDivElement
        }
        private _contenedorDeDatosMasVisor = null;
        public get ContenedorDeDatosMasVisor(): HTMLDivElement {
            if (!Definido(this._contenedorDeDatosMasVisor)) {
                this._contenedorDeDatosMasVisor = document.querySelector('.' + ltrCss.crud.panelCreacion.ContenedorDeDatosMasVisor) as HTMLDivElement
            }
            return this._contenedorDeDatosMasVisor as HTMLDivElement
        }

        private _sociedad: HTMLInputElement;
        public get Sociedad(): HTMLInputElement {
            if (!Definido(this._sociedad)) {
               this._sociedad = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg) as HTMLInputElement;
            }
            return this._sociedad;
        }

        private _cg: HTMLInputElement;
        public get Cg(): HTMLInputElement {
            if (!Definido(this._cg)) {
                this._cg = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Elemento.ConCg.Cg) as HTMLInputElement;
            }
            return this._cg;
        }

        private _tipo: HTMLInputElement;
        public get Tipo(): HTMLInputElement {
            if (!Definido(this._tipo)) {
                this._tipo = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Elemento.ConTipo.Tipo) as HTMLInputElement;
            }
            return this._tipo;
        }

        private _nombre: HTMLInputElement;
        public get ControlDeNombre(): HTMLInputElement {
            if (!Definido(this._nombre)) {
                this._nombre = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Elemento.Nombre) as HTMLInputElement;
            }
            return this._nombre;
        }

        private _contenedorDeDatos = null;
        public get ContenedorDeDatos(): HTMLDivElement {
            if (!Definido(this._contenedorDeDatos)) {
                this._contenedorDeDatos = this.ContenedorDeDatosMasVisor.querySelector('.' + ltrCss.crud.panelCreacion.ContenedorDeDatosDto) as HTMLDivElement
            }
            return this._contenedorDeDatos as HTMLDivElement
        }

        private _contenedorVisor = null;
        public get ContenedorDelVisor(): HTMLDivElement {
            if (!Definido(this._contenedorVisor)) {
                this._contenedorVisor = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelCreacion.ContenedorVisor) as HTMLDivElement
            }
            return this._contenedorVisor as HTMLDivElement
        }

        private _DivVisor = null;
        public get DivVisor(): HTMLDivElement {
            if (!Definido(this._DivVisor)) {
                this._DivVisor = this.ContenedorDeDatosMasVisor?.querySelector('.' + ltrCss.crud.panelCreacion.VisorDeCreacion) as HTMLDivElement
            }
            return this._DivVisor as HTMLDivElement
        }

        private _imagenIaArchivo = null;
        public get ImageIaArchivo(): HTMLImageElement {
            if (!Definido(this._imagenIaArchivo)) {
                this._imagenIaArchivo = document.getElementById(this.ContenedorDeDatosMasVisor.id + '_img_ia_archivo') as HTMLImageElement;
            }
            return this._imagenIaArchivo as HTMLImageElement
        }

        private _imagenOcrArchivo = null;
        public get ImageOcrArchivo(): HTMLImageElement {
            if (!Definido(this._imagenOcrArchivo)) {
                this._imagenOcrArchivo = document.getElementById(this.ContenedorDeDatosMasVisor.id + '_img_ocr_archivo') as HTMLImageElement;
            }
            return this._imagenOcrArchivo as HTMLImageElement
        }
        protected _contenidoPdfOriginal: string | undefined;
        protected _iframeDelFicheroOriginal: HTMLIFrameElement = undefined;
        protected SetFrameDelFicheroOriginal() {
            const iframeOriginal = this.DivVisor.querySelector('iframe');
            this._iframeDelFicheroOriginal = iframeOriginal.cloneNode(true) as HTMLIFrameElement;
            const pdfObject = iframeOriginal.contentDocument?.querySelector('object[type="application/pdf"]');
            if (pdfObject) {
                this._contenidoPdfOriginal = pdfObject.getAttribute('data');
            } else {
                this._contenidoPdfOriginal = undefined;
            }
            this._iframeDelFicheroOriginal.srcdoc = iframeOriginal.srcdoc || iframeOriginal.contentWindow.document.documentElement.outerHTML;
        }

        private _iframeDelFicheroResumido: HTMLIFrameElement = undefined;
        protected SetFrameDelFicheroResumido() {
            const iframeResumido = this.DivVisor.querySelector('iframe');
            this._iframeDelFicheroResumido = iframeResumido.cloneNode(true) as HTMLIFrameElement;
            this._iframeDelFicheroResumido.srcdoc = iframeResumido.srcdoc || iframeResumido.contentWindow.document.documentElement.outerHTML;
        }

        private _iframeDelFicheroOcr: HTMLIFrameElement = undefined;
        protected SetFrameDelFicheroOcr() {
            const iframeOcr = this.DivVisor.querySelector('iframe');
            this._iframeDelFicheroOcr = iframeOcr.cloneNode(true) as HTMLIFrameElement;
            this._iframeDelFicheroOcr.srcdoc = iframeOcr.srcdoc || iframeOcr.contentWindow.document.documentElement.outerHTML;
        }

        private _splitter = null;
        public get Splitter(): HTMLDivElement {
            if (!Definido(this._splitter)) {
                this._splitter = this.PanelDeCrear.querySelector('.splitter') as HTMLDivElement;
            }
            return this._splitter as HTMLDivElement
        }

        private _IdArchivoEnElSelector: number = undefined;
        public get IdArchivoEnElSelector(): number {
            return this._IdArchivoEnElSelector;
        }
        private set IdArchivoEnElSelector(value: number) {
            this._IdArchivoEnElSelector = value;
        }

        private _idArchivoMostrado: number = undefined;
        public get IdArchivoMostrado(): number {
            return Definido(this._idArchivoMostrado) ? this._idArchivoMostrado : 0;
        }
        public set IdArchivoMostrado(value: number) {
            if (!Definido(value) || value == 0) {
                ApiControl.IncluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
            }
            else {
                ApiControl.ExcluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelDeEdicion.VisorOculto);
            }
            this._idArchivoMostrado = value;
        }

        public AsignarIdArchivo(id: number, ajustarVisor: boolean) {
            this.IdArchivoMostrado = id;
            if (ajustarVisor)
                ApiDelCrud.CalcularTamanoDelVisor();
        }

        public get EsModal(): boolean {
            return this.PanelDeCrear.className === ltrCss.contenedorModal;
        }

        public get ModalParaCrearPlantilla(): HTMLDivElement { return document.getElementById(this.CrudDeMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Parametrizacion.Negocio.GuardarPlantillaCreacion) as HTMLDivElement; }
        public get ModalParaEliminarPlantilla(): HTMLDivElement { return document.getElementById(this.CrudDeMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion) as HTMLDivElement; }
        public get PanelDeContenidoModal(): HTMLDivElement {
            return document.getElementById(`${this._idPanelCreacion}_contenido`) as HTMLDivElement;
        }

        private get HeEntradoDirectamente(): boolean {
            return this.CrudDeMnt.ParaqueNavegar === enumParaQueNavegar.crear;
        }

        protected get SeguirCreando(): boolean {
            let check: HTMLInputElement = document.getElementById(`${this._idPanelCreacion}-crear-mas`) as HTMLInputElement;
            if (!Definido(check)) return false;
            return !check.checked;
        }

        protected get ContenedorMenu(): HTMLDivElement {
            return document.getElementById(`${this._idPanelCreacion}.${ltrMenus.menu.creacion}`) as HTMLDivElement;
        }

        private _mapearPlantilla: boolean;
        public get MapeandoPlantilla(): boolean {
            return this._mapearPlantilla;
        }
        public set MapeandoPlantilla(valor: boolean) {
            this._mapearPlantilla = valor;
        }

        constructor(crud: CrudMnt, idPanelCreacion: string) {
            super(false);

            if (IsNullOrEmpty(idPanelCreacion))
                throw Error("No se puede construir un objeto del tipo CrudCreacion sin indica el panel de creación");

            this._idPanelCreacion = idPanelCreacion;
            if (this.PanelDeCrear !== null)
                this._controlador = this.PanelDeCrear.getAttribute(literal.controlador);
            this.CrudDeMnt = crud;
        }

        protected PanelDeAmpliacion(id: string): HTMLDivElement {
            return document.getElementById(this.PanelDeCrear.id + '_ampliacion_' + id.toLowerCase()) as HTMLDivElement;
        }

        public EjecutarAcciones(accion: string) {

            if (accion === ltrEventos.Creacion.Crear)
                this.Crear();
            else
                if (accion === ltrEventos.Creacion.Cerrar)
                    this.CerrarCreacion();
                else
                    throw `la opción ${accion} no está definida`;

            ApiPanel.QuitarClaseDeCtrlNoValido(this.PanelDeCrear);
            ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDeCrear);

        }

        private handleResize() {
            ApiDelCrud.AjustarAnchoDeDatosMasVisor();
        }


        public ComenzarCreacion(): void {
            this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.creando;
            this.CrudDeMnt.MenuGrid_DeselecionarTodasLasFilas();
            if (this.EsModal) {
                this.PanelDeCrear.style.display = ltrStyle.display.block;
                this.Altura = this.PanelDeContenidoModal.getBoundingClientRect().height;
                EntornoSe.AjustarModalesAbiertas();
            }
            else {
                //ApiDelCrud.MostrarPanelDeCreacion();
                ApiDelCrud.CambiarPanelActivoDelCrud(enumModoTrabajo.creando);
                if (Definido(this.DivVisor)) {
                    ApiDelCrud.ConfigurarEventosDeCambioDelAnchoContenedorDeDatos();
                    this.handleResize = this.handleResize.bind(this);
                    window.addEventListener('resize', this.handleResize);
                }
            }
            this.InicializarPanel();
        }

        public PosicionarCreacion(): void {
            //this.PanelDeCrear.style.position = 'fixed';
            //this.PanelDeCrear.style.top = `${AlturaCabeceraPnlControl()}px`;
            //this.PanelDeCrear.style.height = `${AlturaFormulario() - AlturaPiePnlControl() - AlturaCabeceraPnlControl()}px`;
        }

        private InicializarPanel() {
            ApiPanel.BlanquearControlesDeIU(this.PanelDeCrear);
            ApiPanel.BlanquearControlesOcultos(this.PanelDeCrear);
            ApiDelCrud.OcultarClaseDeElemento(this.PanelDeCrear);
            ApiDelCrud.QuitarResaltos(this.PanelDeCrear);
            var parametros = this.ParametrosParaLeerDatosDeInicializarPanel();
            ApiDePeticiones.LeerDatosParaInicializarVista(this, this.Controlador, this.CrudDeMnt.NombreDeNegocio, parametros)
                .then((peticion) => this.DespuesDeLeerDatosParaInicializarCreacion(peticion))
                .catch((peticion) => this.DesactivarMenuDeCreacion(peticion));
        }


        protected ParametrosParaLeerDatosDeInicializarPanel(): Array<Parametro> {
            var parametros = Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.modo, enumModoTrabajo.creando));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.CrudDeMnt.Titulo));
            return parametros;
        }

        protected DesactivarMenuDeCreacion(peticion: any): void {
            ApiDeAjax.ErrorTrasPeticion("Al leer el modo de acceso al negocio", peticion);
            ApiControl.BloquearMenu(this.PanelDeCrear);
        }

        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            var creador = peticion.llamador;
            MapearAlPanel.MapearControlesDesdeOtroPanel(creador.CrudDeMnt.PanelFiltro, creador.PanelDeCrear);
            creador.InicializarControlesDeCreacion(peticion);
            ApiPanel.PosicionarEn(this.PanelDeCrear);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            let creador: CrudCreacion = peticion.llamador as CrudCreacion;
            let datosPropuestos = peticion.resultado.datos;

            this.MapearDatosPropuestos(creador.PanelDeCrear, datosPropuestos);
            this.IncluirPlantillasDeCreacion(datosPropuestos);
            this.MapearRestrictoresDeFiltro();
            ApiDeInicializacion.InicializarNavegadoresDeRestrictoresDeEdicion(creador.PanelDeCrear);
            ApiDeInicializacion.InicializarSelectoresDeFecha(creador.PanelDeCrear);
        }

        protected MapearRestrictoresDeFiltro() {
        }

        public TrasSeleccionarCg(idLista: string): void {
            var objeto = OpcionesDeLasListas.ObtenerObjeto(this.Cg);
            if (Definido(objeto)) {
                ApiDelCrud.MapearIdSociedad(this.PanelDeCrear, objeto);
            }
        }

        public TrasBlanquearCg(): void {
                this.Sociedad.value = "";
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            if (opcion === ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion || opcion === ltrMenus.eventosDeMf.Parametrizacion.Negocio.GuardarPlantillaCreacion) {
                datosDeEntrada.push(new Parametro(literal.controlador, ltrControladores.Negocio.PlantillasDeCreacion));
                parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.CrudDeMnt.Titulo));
            }
        }

        private IncluirPlantillasDeCreacion(datosPropuestos: any) {
            var plantillas = ObtenerPropiedad(datosPropuestos, ltrDatosPropuestos.Plantillas) as Array<any>
            let menu = document.getElementById(ltrMenus.menu.creacion) as HTMLUListElement;
            let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
            if (opcionesLi.length == 2 && plantillas.length > 0) {
                ApiDeMenuFlotante.IncluirOpcion(menu, `${ltrMenus.menu.creacion}.${ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion}`, ltrMenus.Texto.borrarPlantilla, ltrMenus.enumOrigen.creacion, enumCssOpcionMenu.DeElemento, ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion);
                menu.appendChild(document.createElement("hr"));
                for (let i = 0; i < plantillas.length; i++) {
                    let id = `${ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla}_${ObtenerPropiedad(plantillas[i], literal.id)}`;
                    let titulo = ObtenerPropiedad(plantillas[i], ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla);
                    ApiDeMenuFlotante.IncluirOpcion(menu, id, titulo, ltrMenus.enumOrigen.creacion, enumCssOpcionMenu.DeElemento,);
                }
            }
            ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenu, ltrMenus.enumOrigen.creacion, enumCssOpcionMenu.DeElemento, this.CrudDeMnt.ModoAccesoAlNegocio);
        }

        public GuardarPlantillaCreacion(): void {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();

            this.IncluirParametrosParaGuardarPlantillaCreacion(parametros, datosDeEntrada);

            ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeCreacion, this.CrudDeMnt.IdNegocio, ltrMenus.eventosDeMf.Parametrizacion.Negocio.GuardarPlantillaCreacion, parametros, datosDeEntrada)
                .then((peticion) => this.DespuesDeCrearPlantilla(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected IncluirParametrosParaGuardarPlantillaCreacion(parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): void {
            parametros.push(new Parametro(Ajax.Param.datosPeticion, ApiDelCrud.MapearDatosDeCreacionSalvables(this.PanelDeCrear)));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla, ApiControl.BuscarEditor(this.ModalParaCrearPlantilla, ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla).value.trim()));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.CrudDeMnt.Titulo));
        }

        public EliminarPlantillaCreacion(): void {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            let lista = ApiControl.BuscarListaDeElementos(this.ModalParaEliminarPlantilla, ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla);
            if (lista.selectedIndex === 0)
                MensajesSe.EmitirExcepcion('EliminarPlantillaCreacion', 'Debe seleccionar una plantilla a borrar');


            var chkRemplazar = ApiControl.BuscarCheck(this.ModalParaEliminarPlantilla, ltrPropiedades.Negocio.PlantillaDeCreacion.Remplazar);
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeCreacion.IdPlantilla, Numero(lista.value)));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeCreacion.Remplazar, chkRemplazar.checked));
            if (chkRemplazar.checked) {
                this.IncluirParametrosParaGuardarPlantillaCreacion(parametros, datosDeEntrada);
                parametros = parametros.filter(param => param.parametro !== ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla);
                parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla, lista.options[lista.selectedIndex].getAttribute('label').trim()));
            }

            ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeCreacion, this.CrudDeMnt.IdNegocio, ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion, parametros, datosDeEntrada)
                .then((peticion) => this.DespuesDeEliminarPlantilla(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private MapearDatosPropuestos(panel: HTMLDivElement, datosPropuestos: any) {
            if (!Definido(datosPropuestos))
                return;

            if (Definido(this.Cg)) {
                let idSeleccionado: number = Numero(this.Cg.getAttribute(atListasDinamicas.idSeleccionado));
                if (idSeleccionado === 0) {
                    let cgPropuesto = ObtenerPropiedad(datosPropuestos, ltrDatosPropuestos.CGPropuesto) as any;
                    if (Definido(cgPropuesto)) {
                        ApiDelCrud.MapearIdSociedad(panel, cgPropuesto);
                        OpcionesDeLasListas.AgregarOpcion(this.Cg.id, cgPropuesto)
                        MapearAlControl.ListaDinamica(this.Cg, Numero(ObtenerPropiedad(cgPropuesto, literal.id)), ObtenerPropiedad(cgPropuesto, literal.expresion, false));
                        ApiControl.ResaltarControl(this.Cg, ltrCss.Resalto.Violeta)
                    }
                }
            }
            
            if (Definido(this.Tipo)) {
                let idSeleccionado: number = Numero(this.Tipo.getAttribute(atListasDinamicas.idSeleccionado));
                if (idSeleccionado === 0) {
                    let tipoPropuesto = ObtenerPropiedad(datosPropuestos, ltrDatosPropuestos.TipoPropuesto) as any;
                    if (Definido(tipoPropuesto)) {
                        OpcionesDeLasListas.AgregarOpcion(this.Tipo.id, tipoPropuesto)
                        MapearAlControl.ListaDinamica(this.Tipo, Numero(ObtenerPropiedad(tipoPropuesto, literal.id)), ObtenerPropiedad(tipoPropuesto, literal.expresion), false);
                        ApiControl.ResaltarControl(this.Tipo, ltrCss.Resalto.Violeta)
                    }
                }
            }

            let nombre = ObtenerPropiedad(datosPropuestos, ltrDatosPropuestos.Nombre) as string;
            if (!IsNullOrEmpty(nombre)) ApiControl.MapearEditorConResalto(panel, ltrDatosPropuestos.Nombre, nombre, ltrCss.Resalto.Violeta);

            let descripcion = ObtenerPropiedad(datosPropuestos, ltrDatosPropuestos.Descripcion) as string;
            if (!IsNullOrEmpty(descripcion)) ApiControl.MapearAreaDeTextoConResalto(panel, ltrDatosPropuestos.Descripcion, descripcion, ltrCss.Resalto.Violeta);

            MapearAlPanel.DatosPropuestos(panel, ObtenerPropiedad(datosPropuestos, ltrDatosPropuestos.Otros));
        }


        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Comun.GuardarDatosCreacion) {
                this.GuardarDatosDeCreacion(idNegocio, opcion);
                return;
            }

            if (opcion.startsWith(`${ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla}_`)) {
                this.ProcesarPeticionRecuperarDatosDePlantillasDeCreacion(idNegocio, opcion);
                return;
            }

            super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        private GuardarDatosDeCreacion(idNegocio: number, opcion: string): void {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            this.IncluirParametrosParaGuardarDatosDeCreacion(opcion, parametros, datosDeEntrada);

            ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeCreacion, idNegocio, opcion, parametros, datosDeEntrada)
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected IncluirParametrosParaGuardarDatosDeCreacion(opcion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): void {
            let datosParaGuardar = ApiDelCrud.MapearDatosDeCreacionSalvables(this.PanelDeCrear);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            datosDeEntrada.push(new Parametro(ltrMenus.opcion, opcion));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.CrudDeMnt.Titulo));
        }


        private ProcesarPeticionRecuperarDatosDePlantillasDeCreacion(idNegocio: number, opcion: string): void {
            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();

            this.IncluirParametrosParaRecuperarDatosDePlantillasDeCreacion(opcion, parametros, datosDeEntrada);
            ApiDePeticiones.ProcesarPeticion(this, ltrControladores.Negocio.PlantillasDeCreacion, idNegocio, opcion, parametros, datosDeEntrada)
                .then((peticion) => {
                    let creador: CrudCreacion = peticion.llamador as CrudCreacion;
                    let datosPropuestos = peticion.resultado.datos;
                    creador.MapeandoPlantilla = true;
                    ApiPanel.BlanquearControlesDeIU(creador.PanelDeCrear);
                    ApiDelCrud.QuitarResaltos(creador.PanelDeCrear);
                    this.MapearDatosPropuestos(creador.PanelDeCrear, datosPropuestos);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
                .finally(() => { // ⬅️ SIN ARGUMENTO 'peticion'

                    // 1. Obtener el creador (asumiendo que 'this' es CrudCreacion)
                    let creador: CrudCreacion = this as CrudCreacion;

                    // 2. Ejecutar la lógica de cleanup DESPUÉS de 1000ms (1 segundo)
                    setTimeout(() => {
                        creador.MapeandoPlantilla = false;
                    }, 1000); // Retraso de 1000 milisegundos
                });
        }

        protected IncluirParametrosParaRecuperarDatosDePlantillasDeCreacion(opcion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): void {
            let datosParaGuardar = ApiDelCrud.MapearDatosDeCreacionSalvables(this.PanelDeCrear);
            parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
            datosDeEntrada.push(new Parametro(ltrMenus.opcion, opcion));
            parametros.push(new Parametro(ltrPropiedades.Negocio.PlantillaDeFiltrado.Vista, this.CrudDeMnt.Titulo));
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {

            if (super.DespuesDeProcesarOpcionMf(peticion)) {
                return true;
            }

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);

            if (opcion == ltrMenus.eventosDeMf.Parametrizacion.Negocio.GuardarPlantillaCreacion) {
                let idModal = crudMnt.IdCrud + '-' + opcion;
                ApiPanel.AbrirModalDeDatos(idModal);
                return true;
            }
            if (opcion == ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion) {
                let idModal = crudMnt.IdCrud + '-' + opcion;
                var modal = ApiPanel.AbrirModalDeDatos(idModal);
                var lista = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla);
                ApiControl.QuitarOpcionesDeLalista(lista);
                MapearAlControl.MapearElementosEnLaLista(lista.id, 0, peticion.resultado.datos);
                return true;
            }
            return false;
        }

        private Crear() {
            this.ValidarAntesDeCrear();
            let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, this.PanelDeCrear, enumModoTrabajo.creando);
            let parametros: Array<Parametro> = this.ParametrosDeCreacion();
            ApiDePeticiones.CrearElementoPorPost(this, this.Controlador, Ajax.EndPoint.CrearElementoPorPost, this.CrudDeMnt.IdNegocio, json, parametros)
                .then((peticion) => this.DespuesDeCrear(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected ParametrosDeCreacion(): Parametro[] {
            var parametros = new Array<Parametro>();
            let ampliaciones: NodeListOf<HTMLDivElement> = this.PanelDeCrear.querySelectorAll(`div[${atControl.esAmpliacion}="true"]`) as NodeListOf<HTMLDivElement>;
            for (let i = 0; i < ampliaciones.length; i++) {
                let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(this, ampliaciones[i], enumModoTrabajo.creando);
                parametros.push(new Parametro(ampliaciones[i].getAttribute(ltrAmpliaciones.AccionTrasCrear), json));
            }
            return parametros;
        }

        protected ValidarAntesDeCrear(): void {

        }

        public CerrarCreacion(irAEdicion: boolean = false) {
            if (Definido(this.ContenedorMenu)) {
                var idMenu = this.ContenedorMenu.getAttribute(atControl.MenuFlotante);
                ApiDeMenuFlotante.OcultarMenu(document.getElementById(idMenu));
            }

            if (Definido(this.DivVisor)) {
                window.removeEventListener('resize', this.handleResize);
            }

            ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDeCrear);

            if (this.HeEntradoDirectamente) {
                if (irAEdicion)
                    this.CrudDeMnt.IraEditar();
                else
                    EntornoSe.NavegarAtras();
            }
            else {
                this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.mantenimiento;
                if (this.EsModal) {
                    ApiPanel.CerrarModal(this.PanelDeCrear);
                    EntornoSe.AjustarDivs();
                }
                else {
                    //ApiDelCrud.OcultarPanelDeCreacion();
                    ApiDelCrud.CambiarPanelActivoDelCrud(this.CrudDeMnt.ModoTrabajo);
                }
                if (irAEdicion)
                    this.CrudDeMnt.IraEditar();
                else {
                    //this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.mantenimiento;
                    this.CrudDeMnt.CargarGrid();
                }
            }
        }

        protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (Definido(this.Tipo)) {
                let objeto = OpcionesDeLasListas.ObtenerObjeto(this.Tipo);
                if (Definido(objeto))
                    return EsTrue(ObtenerPropiedad(objeto, ltrPropiedades.TipoDeElemento.EditarTrasCrear));
            }
            return this.PanelDeCrear.classList.contains(ltrCss.crud.editarTrasCrear)
        }

        protected DespuesDeCrear(peticion: ApiDeAjax.DescriptorAjax) {
            let crudCreador: CrudCreacion = peticion.llamador as CrudCreacion;
            ApiPanel.BlanquearEditores(crudCreador.PanelDeCrear);
            ApiPanel.BlanquearArchivos(crudCreador.PanelDeCrear);
            if (crudCreador.SeguirCreando) {
                crudCreador.InicializarPanel();
            }
            else {
                let irAEdicion: boolean = this.AlCerrarIrAEdicion(peticion);
                if (irAEdicion) crudCreador.CrudDeMnt.InfoSelector.InsertarElemento(new Elemento(peticion.resultado.datos));
                crudCreador.CerrarCreacion(irAEdicion);
            }
        }

        protected DespuesDeCrearPlantilla(peticion: ApiDeAjax.DescriptorAjax) {
            let menu = document.getElementById(ltrMenus.menu.creacion) as HTMLUListElement;
            let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
            if (opcionesLi.length == 2) {
                ApiDeMenuFlotante.IncluirOpcion(menu, `${ltrMenus.menu.creacion}.${ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion}`, ltrMenus.Texto.borrarPlantilla, ltrMenus.enumOrigen.creacion, enumCssOpcionMenu.DeElemento, ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion);
                menu.appendChild(document.createElement("hr"));
            }
            let id = `${ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla}_${ObtenerPropiedad(peticion.resultado.datos, literal.id)}`;
            let titulo = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla);
            ApiDeMenuFlotante.IncluirOpcionPosteriorALaPosicion(menu, 5, id, titulo, ltrMenus.enumOrigen.creacion, enumCssOpcionMenu.DeElemento);

            var listaDelMnt = document.getElementById(this.CrudDeMnt.ZonaDeMenu.id + '-otras') as HTMLSelectElement;
            if (Definido(listaDelMnt)) {
                ApiControl.AgregarOpcionAlfabeticamente(listaDelMnt, ObtenerPropiedad(peticion.resultado.datos, literal.id), titulo);
            }
        }

        protected DespuesDeEliminarPlantilla(peticion: ApiDeAjax.DescriptorAjax) {
            let menu = document.getElementById(ltrMenus.menu.creacion) as HTMLUListElement;
            let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;

            if (ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla, undefined, false) !== undefined) {
                return;
            }

            let id = `${ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla}_${ObtenerPropiedad(peticion.resultado.datos, literal.id)}`;
            ApiDeMenuFlotante.QuitarOpcion(menu, id);
            if (opcionesLi.length == 4) {
                ApiDeMenuFlotante.QuitarOpcion(menu, `${ltrMenus.menu.creacion}.${ltrMenus.eventosDeMf.Parametrizacion.Negocio.EliminarPlantillaCreacion}`);
                ApiDeMenuFlotante.QuitarUltimoHr(menu);
            }
            var listaDelMnt = document.getElementById(this.CrudDeMnt.ZonaDeMenu.id + '-otras') as HTMLSelectElement;

            if (Definido(listaDelMnt)) {
                var valorAQuitar = ObtenerPropiedad(peticion.resultado.datos, literal.id);
                for (let i = 0; i < listaDelMnt.options.length; i++) {
                    if (listaDelMnt.options[i].value === valorAQuitar.toString()) {
                        listaDelMnt.remove(i);
                        break;
                    }
                }
            }


        }


        public ResetearIa(nombreIa: string): void {
            if (IsNullOrEmpty(nombreIa))
                return;
            const partes = this.ImageIaArchivo.title.split(':');
            if (partes.length != 2)
                return;
            this.ImageIaArchivo.title = partes[0] + ':' + `'${nombreIa}'`;

            if (Numero(this.IdArchivoEnElSelector)===0)
                return;
            if (this.IdArchivoMostrado === 0)
                return;
            if (this._contenidoPdfOriginal) {
                ApiPanel.RenderizarContenidoPdf(this.DivVisor, this._contenidoPdfOriginal);
            }
            ApiControl.RemplazarCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenArchivo, ltrCss.crud.panelCreacion.ImagenIa);
            ApiControl.RemplazarCss(this.ImageOcrArchivo, ltrCss.crud.panelCreacion.ImagenArchivo, ltrCss.crud.panelCreacion.ImagenOcr);
            this.ResetearFramesDelVisor();
        }


        public MostrarArchivo(archivo: HTMLInputElement, idArchivo: number, nombreArchivo = null): void {
            this.IdArchivoEnElSelector = idArchivo;

            let tipoControl = archivo.getAttribute(atControl.tipo);
            if (tipoControl !== ltrTipoControl.SelectorDeUnArchivo)
                return
            if (!EsTrue(archivo.getAttribute(atArchivo.visibleEnVisorAlCrear)))
                return;
            ApiControl.ExcluirCss(this.ContenedorDeDatosMasVisor, ltrCss.crud.panelCreacion.VisorOculto);
            ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDeCrear);
            ApiControl.RemplazarCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenArchivo, ltrCss.crud.panelCreacion.ImagenIa);
            ApiControl.RemplazarCss(this.ImageOcrArchivo, ltrCss.crud.panelCreacion.ImagenArchivo, ltrCss.crud.panelCreacion.ImagenOcr);
            this._contenidoPdfOriginal = undefined;
            this.ResetearFramesDelVisor();
            ApiDelCrud.RenderizarUrlsEnVisor(this.CrudDeMnt, idArchivo, !IsNullOrEmpty(nombreArchivo) ? nombreArchivo : archivo.value, this.IdArchivoMostrado === 0);
        }

        protected IntercambiarFrameResumidoDelVisor(): boolean {
            if (this._iframeDelFicheroOriginal && this._iframeDelFicheroResumido) {
                ApiControl.IntercambiaCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenIa, ltrCss.crud.panelCreacion.ImagenArchivo);
                this.DivVisor.innerHTML = '';
                if (this.ImageIaArchivo.classList.contains(ltrCss.crud.panelCreacion.ImagenIa)) {

                    if (this._contenidoPdfOriginal) {
                        ApiPanel.RenderizarContenidoPdf(this.DivVisor, this._contenidoPdfOriginal);
                    }
                    else this.DivVisor.appendChild(this._iframeDelFicheroOriginal.cloneNode(true));
                } else {
                    this.DivVisor.appendChild(this._iframeDelFicheroResumido.cloneNode(true));
                }
                return true;
            }
            return false;
        }

        public ResetearFramesDelVisor(): void {
            this._iframeDelFicheroOriginal = undefined;
            this._iframeDelFicheroOcr = undefined;
            this._iframeDelFicheroResumido = undefined;
        }

        public PonerElFrameConElDocumentoOrigina(): void {
            ApiControl.IncluirCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenIa);
            ApiControl.ExcluirCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenArchivo);
            if (this._contenidoPdfOriginal) {
                ApiPanel.RenderizarContenidoPdf(this.DivVisor, this._contenidoPdfOriginal);
            }
        }

        protected IntercambiarFrameOcrDelVisor(): boolean {
            if (this._iframeDelFicheroOriginal && this._iframeDelFicheroOcr) {
                ApiControl.IntercambiaCss(this.ImageOcrArchivo, ltrCss.crud.panelCreacion.ImagenOcr, ltrCss.crud.panelCreacion.ImagenArchivo);
                this.DivVisor.innerHTML = '';
                if (this.ImageOcrArchivo.classList.contains(ltrCss.crud.panelCreacion.ImagenOcr)) {

                    if (this._contenidoPdfOriginal) {
                        ApiPanel.RenderizarContenidoPdf(this.DivVisor, this._contenidoPdfOriginal);
                    }
                    else this.DivVisor.appendChild(this._iframeDelFicheroOriginal.cloneNode(true));
                } else {
                    this.DivVisor.appendChild(this._iframeDelFicheroOcr.cloneNode(true));
                }
                return true;
            }
            return false;
        }

        public async ResumirArchivo(): Promise<boolean> {
            if (!Definido(this.IdArchivoMostrado))
                return true;

            if (this.IntercambiarFrameResumidoDelVisor())
                return true;

            this.SetFrameDelFicheroOriginal();
            if (await ApiDelCrud.ProcesarRenderizar(this.CrudDeMnt, this.IdArchivoMostrado, ltrEventos.Edicion.ResumirArchivo)) {
                ApiControl.IntercambiaCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenIa, ltrCss.crud.panelCreacion.ImagenArchivo);
                this.SetFrameDelFicheroResumido();
                return true;
            }
            return false;
        }


        public async PasarOcr(): Promise<boolean> {
            if (!Definido(this.IdArchivoMostrado))
                return true;

            if (this.IntercambiarFrameOcrDelVisor())
                return true;

            this.SetFrameDelFicheroOriginal();

            if (await ApiDelCrud.ProcesarRenderizar(this.CrudDeMnt, this.IdArchivoMostrado, ltrEventos.Edicion.PasarOcr)) {
                ApiControl.IntercambiaCss(this.ImageOcrArchivo, ltrCss.crud.panelCreacion.ImagenOcr, ltrCss.crud.panelCreacion.ImagenArchivo);
                this.SetFrameDelFicheroOcr();
                return true;
            }
            return false;
        }

        public MapearDatosJsonDesdeElVisor(json: object): boolean {
            ApiPanel.QuitarClaseDeMapeadoPoIa(this.PanelDeCrear);
            return true;
        }
    }


}
