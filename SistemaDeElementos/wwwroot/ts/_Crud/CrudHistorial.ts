namespace Crud {

    const idModalMensaje = 'modal-modaldemensaje-detalledeunsucesodto';


    export class CrudHistorial extends GridDeDatos {
        private _idPanelHistorial: string;
        public PanelDeMnt: HTMLDivElement;
        public CrudDeMnt: CrudMnt;
        private _Elemento: Elemento;

        private _inscrustadoEnEdicion: boolean = false;
        public get EstaEnEdicion(): boolean {
            return this._inscrustadoEnEdicion;
        }

        public get Titulo(): HTMLDivElement {
            return document.getElementById(`${this._idPanelHistorial}.menu.del.flujo`) as HTMLDivElement;
        }
        public get ListaDeIncluidas(): HTMLSelectElement {
            return ApiControl.BuscarListaDeValores(this.PanelDeFiltro, 'excluir') as HTMLSelectElement;
        }
        public get ListaDeExcluidas(): HTMLSelectElement {
            return ApiControl.BuscarListaDeValores(this.PanelDeFiltro, 'excluidos') as HTMLSelectElement;
        }
        public get Suceso(): HTMLInputElement {
            return ApiControl.BuscarEditorDeFiltrado(this.PanelDeFiltro, 'suceso') as HTMLInputElement;
        }
        public get Referencia(): HTMLInputElement {
            return ApiControl.BuscarEditorDeFiltrado(this.PanelDeFiltro, 'referencia') as HTMLInputElement;
        }

        private _contenedorDelCuerpo = null;
        public get ContenedorDelCuerpo(): HTMLDivElement {
            if (!Definido(this._contenedorDelCuerpo)) {
                this._contenedorDelCuerpo = document.querySelector('.' + ltrCss.crud.PanelHistorial.ContenedorCuerpo) as HTMLDivElement
            }
            return this._contenedorDelCuerpo as HTMLDivElement
        }

        public get PanelDeFiltro(): HTMLDivElement {
            return this.ContenedorDelCuerpo.querySelector('.cuerpo-datos-filtro') as HTMLDivElement;
        }

        public get BloqueDeFiltro(): HTMLDivElement {
            return document.getElementById('filtro-bloque-historial') as HTMLDivElement;
        }

        public get ContenedorDeControlesDeFiltro(): HTMLDivElement {
            return this.BloqueDeFiltro.querySelector('.fila-filtro-sin-span') as HTMLDivElement;
        }

        public get PanelDeHistorial(): HTMLDivElement {
            return document.getElementById(this._idPanelHistorial) as HTMLDivElement;
        }

        public get Elemento(): Elemento {
            return this._Elemento;
        }

        public get ExpandirFiltro(): HTMLInputElement {
            return document.getElementById(`expandir.${this._idPanelHistorial}`) as HTMLInputElement;
        }
        public get PanelFiltro(): HTMLDivElement {
            return document.getElementById(`${this._idPanelHistorial}_filtro`) as HTMLDivElement;
        }
        public get EtiquetaMostrarOcultarFiltro(): HTMLElement {
            return document.getElementById(`mostrar.${this._idPanelHistorial}.ref`) as HTMLElement;
        }

        private _contenedorDeLaCabecera = null;
        public get ContenedorDeLaCabecera(): HTMLDivElement {
            if (!Definido(this._contenedorDeLaCabecera)) {
                this._contenedorDeLaCabecera = document.querySelector('.' + ltrCss.crud.PanelHistorial.ContenedorCabecera) as HTMLDivElement
            }
            return this._contenedorDeLaCabecera as HTMLDivElement
        }

        private _contenedorDelPie = null;
        public get ContenedorDelPie(): HTMLDivElement {
            if (!Definido(this._contenedorDelPie)) {
                this._contenedorDelPie = document.querySelector('.' + ltrCss.crud.PanelHistorial.ContenedorPie) as HTMLDivElement
            }
            return this._contenedorDelPie as HTMLDivElement
        }

        private _modalDeSucesos = null;
        public get ModalDeSucesos(): HTMLDivElement {
            if (!Definido(this._modalDeSucesos)) {
                this._modalDeSucesos = document.getElementById('modal-modaldemensaje-detalledeunsucesodto') as HTMLDivElement
            }
            return this._modalDeSucesos as HTMLDivElement
        }

        private _modoOriginal: string;

        constructor(crud: CrudMnt, idPanelHistorial: string) {
            super(idPanelHistorial);
            this._idPanelHistorial = idPanelHistorial;
            this.PanelDeMnt = crud.CuerpoCabecera;
            this._controlador = this.PanelDeHistorial.getAttribute(literal.controlador);
            this.CrudDeMnt = crud;
        }

        public MostrarHistorial(elemento: any) {
            this._inscrustadoEnEdicion = false;
            this._Elemento = elemento;
            this._modoOriginal = this.CrudDeMnt.ModoTrabajo
            this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.historial;
            ApiDelCrud.CambiarPanelActivoDelCrud(this.CrudDeMnt.ModoTrabajo);
            this.Titulo.innerHTML = SanitizeHTML(this.Titulo.innerHTML.replace('[Referencia]', elemento.Texto));
            this.IndicarOpciones(this.ListaDeIncluidas, 'Incluidas');
            this.IndicarOpciones(this.ListaDeExcluidas, 'Excluidas');
            ApiDeFiltro.MostrarControlDeFiltro(this.PanelDeFiltro, 'suceso');
            ApiDeFiltro.MostrarControlDeFiltro(this.PanelDeFiltro, 'excluir');
            ApiDeFiltro.MostrarControlDeFiltro(this.PanelDeFiltro, 'excluidos');
            this.ContenedorDeControlesDeFiltro.style.gridTemplateColumns = '1fr 1fr 1fr 1fr'
            const referencia = ApiControl.BuscarEditorDeFiltrado(this.PanelDeFiltro, ltrPropiedades.Elemento.Referencia);
            referencia.textContent = '';
            this.CargarHistorial();
        }

        public MostrarHistorialEnEdicion(elemento: any) {
            this._inscrustadoEnEdicion = true;
            this._Elemento = elemento;
            this._modoOriginal = this.CrudDeMnt.ModoTrabajo
            ApiDeFiltro.OcultarControlDeFiltro(this.PanelDeFiltro, 'suceso');
            ApiDeFiltro.OcultarControlDeFiltro(this.PanelDeFiltro, 'excluir');
            ApiDeFiltro.OcultarControlDeFiltro(this.PanelDeFiltro, 'excluidos');
            this.ContenedorDeControlesDeFiltro.style.gridTemplateColumns = '1fr 0fr 0fr 0fr'

            const referencia = ApiControl.BuscarEditorDeFiltrado(this.PanelDeFiltro, ltrPropiedades.Elemento.Referencia);
            referencia.value = ObtenerPropiedad(elemento._registro, ltrPropiedades.Elemento.Referencia);

            this.CargarHistorial();
        }

        public EjecutarAcciones(accion: string) {
            let cerrarHistorial: boolean = false;
            try {
                ApiDeMenuFlotante.CerrarMf(this.PanelDeHistorial);
                switch (accion) {
                    case ltrEventos.Historial.Cerrar: {
                        cerrarHistorial = true;
                        break;
                    }
                    case ltrEventos.Historial.OcultarMostrarFiltro: {
                        this.OcultarMostrarFiltro();
                        break;
                    }
                    case ltrEventos.Historial.AplicarOrdenInicial: {
                        this.InicializarOrdenacionDelHistorial();
                        break;
                    }
                    case ltrEventos.Historial.AnularOrdenacion: {
                        this.AnularOrdenacionDelHistorial();
                        break;
                    }
                    case ltrEventos.Historial.CargarHistorial: {
                        this.CargarHistorial();
                        break;
                    }
                    default: {
                        throw `la opción ${accion} no está definida`;
                    }
                }
            }
            catch (error) {
                MensajesSe.Error("EjecutarAcciones", error.message);
            }

            if (cerrarHistorial)
                this.CerrarHistorial();
        }

        private InicializarOrdenacionDelHistorial() {
            this.ResetearOrdenacion();
            this.CargarHistorial();
        }

        private AnularOrdenacionDelHistorial() {
            this.Ordenacion.AnularOrdenacion();
            this.CargarHistorial();
        }

        private CargarHistorial() {
            this.Navegador.NumeroDePaginaDelGrid = 1;
            this.Navegador.Posicion = 0;
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.filtro, this.filtroParaCargarHistorial()));
            parametros.push(new Parametro(Ajax.Param.orden, this.ObtenerOrdenacion()));
            let datosDeEntrada = new DatosPeticionNavegarGrid(this, atGrid.accion.historial, 0);
            ApiDePeticiones.CargarGrid(this, this.Navegador.Controlador, atGrid.accion.historial, 0, this.Navegador.Cantidad, datosDeEntrada, parametros)
                .then((peticion) => {
                    this.DatosDelGrid.InicializarCache();
                    this.CrearFilasEnElGrid(peticion);
                    ApiDeGrid.OcultarMostrarColumna(this.Tabla, 'elemento', !this.EstaEnEdicion, true);
                })
                .catch((peticion) => {
                    this.SiHayErrorAlCargarElGrid(peticion);
                });
        }
        
        private CerrarHistorial(): void {
            if (this._modoOriginal === enumModoTrabajo.mantenimiento)
                this.IrAlMantenimiento();
            else
                this.IrAEdicion();
        }
        
        private IrAEdicion() {
            this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.editando;
            ApiDelCrud.CambiarPanelActivoDelCrud(this.CrudDeMnt.ModoTrabajo);
            ApiDeMenuFlotante.CerrarMf(this.PanelDeHistorial);

        }

        private IrAlMantenimiento() {
            this.CrudDeMnt.ModoTrabajo = enumModoTrabajo.mantenimiento;
            ApiDelCrud.CambiarPanelActivoDelCrud(this.CrudDeMnt.ModoTrabajo);
            var ocultar = false;
            if (this.CrudDeMnt.EstaElFiltroOculto()) {
                this.CrudDeMnt.MostrarFiltro();
                ocultar = true;
            }
            ApiDeMenuFlotante.CerrarMf(this.PanelDeHistorial);

            if (ocultar)
                this.CrudDeMnt.OcultarFiltro();
        }

        public OcultarMostrarFiltro(): void {
            if (!this.EstaElFiltroOculto()) {
                this.OcultarFiltro();
            }
            else {
                this.MostrarFiltro();
            }
        }

        private EstaElFiltroOculto(): boolean {
            return !EsMayorDeCero(this.ExpandirFiltro.value);
        }

        private OcultarFiltro(): void {
            ApiPanel.OcultarPanel(this.PanelFiltro);
            this.ExpandirFiltro.value = "0";
            this.EtiquetaMostrarOcultarFiltro.innerText = "Mostrar filtro";
        }


        private MostrarFiltro(): void {
            this.ExpandirFiltro.value = "1";
            ApiPanel.MostrarPanel(this.PanelFiltro);
            this.EtiquetaMostrarOcultarFiltro.innerText = "Ocultar filtro";
        }

        public filtroParaCargarHistorial(): string {
            var clausulas = new Array<ClausulaDeFiltrado>();
            var clausula: ClausulaDeFiltrado = null;

            clausula = this.ObtenerClausulaPorId(this.Elemento.Id);
            clausulas.push(clausula);

            let valoresSeparadosPorPipe = "";

            for (let i = 0; i < this.ListaDeExcluidas.options.length; i++) {
                valoresSeparadosPorPipe += this.ListaDeExcluidas.options[i].value + ltrSimbolos.separadorDeValores;
            }

            if (this.ListaDeExcluidas.options.length > 0) {
                valoresSeparadosPorPipe = valoresSeparadosPorPipe.slice(0, -1);
            }

            clausulas.push(new ClausulaDeFiltrado(this.ListaDeIncluidas.getAttribute(atControl.propiedad), atCriterio.esAlgunoDe, valoresSeparadosPorPipe));
            clausulas.push(new ClausulaDeFiltrado(this.Suceso.getAttribute(atControl.propiedad), atCriterio.contiene, this.Suceso.value));
            clausulas.push(new ClausulaDeFiltrado(this.Referencia.getAttribute(atControl.propiedad), atCriterio.igual, this.Referencia.value));

            return JSON.stringify(clausulas);
        }

        public IndicarOpciones(selector: HTMLSelectElement, prefijoMensaje: string) {
            if (!Definido(selector))
                return;
            let opcionMenosUno: HTMLOptionElement = selector.querySelector('option[value="-1"]');
            if (opcionMenosUno) {
                opcionMenosUno.text = `${prefijoMensaje} (${selector.options.length - 1})`;
            }
        }

    }

    export function Historial_EjecutarAccionAsociada(numeroDeFila: number) {

        let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(crudMnt.crudHistorial.Tabla, numeroDeFila, literal.id));
        let suceso = ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(crudMnt.crudHistorial.Tabla, numeroDeFila, ltrPropiedades.Elemento.Historial.Suceso);
        let idRegistro = Numero(ObtenerPropiedad(crudMnt.crudHistorial.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Elemento.Historial.IdRegistro));
        let detalle = ObtenerPropiedad(crudMnt.crudHistorial.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Elemento.Historial.Detalle);
        let accion = ObtenerPropiedad(crudMnt.crudHistorial.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Elemento.Historial.AccionJs);

        if (accion === ltrValores.Elemento.Historial.MostrarObservacion || accion === ltrValores.Elemento.Historial.MostrarCorreo) {
            var modal = document.getElementById(idModalMensaje) as HTMLDivElement;

            var editorId = ApiControl.BuscarEditor(modal, literal.id);
            if (Definido(editorId)) ApiControl.OcultarEditor(editorId, true);

            var editor = ApiControl.BuscarEditor(modal, ltrPropiedades.Elemento.Historial.Suceso);
            var texto = ApiControl.BuscarAreaDeTexto(modal, ltrPropiedades.Elemento.Historial.Detalle);
            MapearAlControl.MapearEditor(editor, 0, suceso, true, false);
            MapearAlControl.MapearAreaDeTexto(texto, detalle, true);
            ApiPanel.AbrirModal(modal);
        }
        else if (accion === ltrValores.Elemento.Historial.MostrarEvento) {

            let gridDeDetalle: HTMLDivElement = document.getElementById('grid-de-detalle-eventos-contenedor') as HTMLDivElement;
            let controlador: string = 'VisorDeAgenda';

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, gridDeDetalle.id));
            datosDeEntrada.push(new Parametro('propiedadesRestrictoras', 'idelemento'));

            let idNegocio: number = Numero(gridDeDetalle.getAttribute(literal.idNegocio));
            let parametros: Array<Parametro> = crudMnt.crudDeEdicion.ParametrosParaLeerElementoPorId();
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, idNegocio));

            ApiDePeticiones.LeerElementoPorId(crudMnt.crudDeEdicion, controlador, idRegistro, parametros, datosDeEntrada)
                .then((peticion) => crudMnt.crudDeEdicion.Expansor_MapearRelacion(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));

        }
        else if (accion === ltrValores.Elemento.Historial.AbrirArchivador) {
            var url = `${window.location.origin}/${ltrUrls.SistemaDocumental.Archivadores}?${ltrParametrosUrl.id}=${idRegistro}`;
            EntornoSe.AbrirPestana(url);
        }
        else if (accion === ltrValores.Elemento.Historial.AbrirTarea) {
            var url = `${window.location.origin}/${ltrUrls.Administracion.Tareas}?${ltrParametrosUrl.id}=${idRegistro}`;
            EntornoSe.AbrirPestana(url);
        }
        else if (accion === ltrValores.Elemento.Historial.AbrirPpt) {
            var url = `${window.location.origin}/${ltrUrls.Ventas.Presupuestos}?${ltrParametrosUrl.id}=${idRegistro}`;
            EntornoSe.AbrirPestana(url);
        }
        else if (accion === ltrValores.Elemento.Historial.Descargar) {
            let idElemento = Numero(ObtenerPropiedad(crudMnt.crudHistorial.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Elemento.Historial.IdElemento));
            let negocio = ObtenerPropiedad(crudMnt.crudHistorial.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Elemento.Historial.Negocio);
            let parametros = `negocio=${negocio}`;
            parametros = `${parametros}&idElemento=${idElemento}`;
            parametros = `${parametros}&idArchivo=${idRegistro}`;
            parametros = `${parametros}&auditar=true`;

            let descargar: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}?${parametros}`;
            try {
                EntornoSe.AbrirPestana(descargar);
            }
            finally {
                MensajesSe.Info("Descarga realizada")
            }

        }

        // mapear a la modal de mensajes los valores de detalle y suceso
    }

    export function Historial_ExcluirClaseDeObjeto() {
        var listaDeIncluidas = crudMnt.crudHistorial.ListaDeIncluidas;
        var listaDeExcluidas = crudMnt.crudHistorial.ListaDeExcluidas;
        if (listaDeIncluidas.selectedIndex === -1)
            return;
        const opcion = listaDeIncluidas.options[listaDeIncluidas.selectedIndex];
        const nuevaopcion = new Option(opcion.text, opcion.value);
        listaDeExcluidas.appendChild(nuevaopcion);
        opcion.remove();
        crudMnt.crudHistorial.IndicarOpciones(listaDeIncluidas, 'Incluidas');
        crudMnt.crudHistorial.IndicarOpciones(listaDeExcluidas, 'Excluidas');
    }

    export function Historial_IncluirClaseDeObjeto() {
        var listaDeIncluidas = crudMnt.crudHistorial.ListaDeIncluidas;
        var listaDeExcluidas = crudMnt.crudHistorial.ListaDeExcluidas;
        if (listaDeExcluidas.selectedIndex === -1)
            return;
        const opcion = listaDeExcluidas.options[listaDeExcluidas.selectedIndex];
        const nuevaopcion = new Option(opcion.text, opcion.value);
        listaDeIncluidas.appendChild(nuevaopcion);
        opcion.remove();
        crudMnt.crudHistorial.IndicarOpciones(listaDeIncluidas, 'Incluidas');
        crudMnt.crudHistorial.IndicarOpciones(listaDeExcluidas, 'Excluidas');
    }
}


