namespace Crud {

    export class HTMLSelector extends HTMLInputElement {
    }

    export class CrudBase {

        public get Pagina(): string {
            return this.Estado.Obtener(ltrClaveDeEstado.paginaActual);
        }

        private _estado: HistorialSe.EstadoPagina = undefined;

        public get Estado(): HistorialSe.EstadoPagina {
            if (this._estado === undefined) {
                throw new Error("Debe definir la variable estado");
            }
            return this._estado;
        }


        private _inicializando = false;
        public get Inicializando(): boolean {
            return this._inicializando;
        }

        public set Inicializando(value: boolean) {
            this._inicializando = value;
        }

        protected _controlador: string;

        public get Controlador() {
            return this._controlador;
        }

        constructor(esConsultaConGuid: boolean = false) {

            if (esConsultaConGuid)
                return;

            if (!Registro.HayUsuarioDeConexion())
                Registro.RegistrarUsuarioDeConexion(this)
                    .catch(() => {
                        MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, "Error al leer el usuario de conexión");
                    });
        }

        public Inicializar(idPanelMnt: string): void {
            this._estado = EntornoSe.Historial.ObtenerEstado(idPanelMnt);
        }


        public AccionesAntesDeSalir(): void {
            // MensajesSe.Info(`Cerrando la página ${this.Pagina}`);
        }

        protected InicializarGridDeDetalles(panel: HTMLDivElement) {
            let grids: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`div[${atControl.tipo}="${ltrTipoControl.gridDeDetalle}"]`) as NodeListOf<HTMLDivElement>;
            grids.forEach((grid) => { ApiControl.BlanquearGridDeDetalle(grid); });
        }

        protected InicializarAmpliaciones(panel: HTMLDivElement) {
            let ampliaciones: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`div[${atControl.tipo}="${ltrTipoControl.ampliacion}"]`) as NodeListOf<HTMLDivElement>;
            ampliaciones.forEach((ampliacion) => { ApiPanel.BlanquearControlesDeIU(ampliacion); });
        }

        public AntesDeNavegar(valores: Diccionario<any>) {
        }

        protected BuscarVisorDeImagen(controlPadre: HTMLDivElement, propiedadDto: string): HTMLImageElement {
            let visor: NodeListOf<HTMLImageElement> = controlPadre.querySelectorAll(`img[${atControl.tipo}='${ltrTipoControl.VisorDeArchivo}']`) as NodeListOf<HTMLImageElement>;
            for (var i = 0; i < visor.length; i++) {
                var control = visor[i] as HTMLImageElement;
                var dto = control.getAttribute(atControl.propiedad);
                if (dto === propiedadDto.toLowerCase())
                    return control;
            }
            return null;
        }

        protected BuscarUrlDelArchivo(controlPadre: HTMLDivElement, propiedadDto: string): HTMLInputElement {
            let selectores: NodeListOf<HTMLInputElement> = controlPadre.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.UrlDeArchivo}"]`) as NodeListOf<HTMLInputElement>;
            for (var i = 0; i < selectores.length; i++) {
                var control = selectores[i] as HTMLInputElement;
                var dto = control.getAttribute(atControl.propiedad);
                if (dto === propiedadDto.toLowerCase())
                    return control;
            }
            return null;
        }


        public AntesDeMapearDatosDeIU(panel: HTMLDivElement, modoDeTrabajo: string): JSON {

            if (EsTrue(panel.getAttribute(atControl.esAmpliacion))) {
                var idPanelDeEdicion = ObtenerPropiedad(this, "_idPanelEdicion");
                if (Definido(idPanelDeEdicion)) {
                    let panelEdicion = document.getElementById(idPanelDeEdicion) as HTMLDivElement;
                    let input: HTMLInputElement = ApiControl.BuscarEditor(panelEdicion, literal.id);
                    return JSON.parse(`{"${ltrPropiedades.Elemento.IdElemento}":"${Number(input.value)}"}`);
                }
                return JSON.parse(`{"${literal.id}":"0"}`);
            }

            if (modoDeTrabajo === enumModoTrabajo.creando)
                return JSON.parse(`{"${literal.id}":"0"}`);

            if (modoDeTrabajo === enumModoTrabajo.editando) {
                let input: HTMLInputElement = ApiControl.BuscarEditor(panel, literal.id);
                if (Number(input.value) <= 0)
                    throw new Error(`El valor del id ${Number(input.value)} debe ser mayor a 0`);
                return JSON.parse(`{"${literal.id}":"${Number(input.value)}"}`);
            }

            throw new Error(`No se ha indicado que hacer para el modo de trabajo ${modoDeTrabajo} antes de mapear los datos de la IU`);
        }


        public DespuesDeMapearDatosDeIU(crud: CrudBase, panel: HTMLDivElement, elementoJson: JSON, modoDeTrabajo: string): JSON {
            return elementoJson;
        }

        protected FiltrosExcluyentes(operacion: string, clausulas: ClausulaDeFiltrado[]) {
            return clausulas;
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]) {
            return clausulas;
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {

            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();

            this.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);

            var controlador = datosDeEntrada.find(x => x.parametro === literal.controlador)
            ApiDePeticiones.ProcesarOpcionMf(this, Definido(controlador) ? controlador.valor : this.Controlador, idNegocio, opcion, esContextual, parametros, datosDeEntrada)
                .then((peticion) => this.DespuesDeProcesarOpcionMf(peticion))
                .catch((peticion) => this.DespuesDeProcesarOpcionMfConError(peticion)) ;
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion: string, escontextual: boolean, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>) {
            datosDeEntrada.push(new Parametro(ltrMenus.opcion, opcion));
            datosDeEntrada.push(new Parametro(ltrMenus.esContextual, escontextual));
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            return false; 
        }
        public DespuesDeProcesarOpcionMfConError(peticion: ApiDeAjax.DescriptorAjax) {
            ApiDePeticiones.EmitirError(peticion);
        }
    }


}
