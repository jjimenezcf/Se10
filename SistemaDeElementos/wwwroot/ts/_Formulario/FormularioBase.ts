namespace Formulario {


    export let formulario: Base = null;

    export const literales = {
        hayFiltros: 'HayFiltros',
    };

    export class Base {
        private _idFormulario: string;
        private _estado: HistorialSe.EstadoPagina = undefined;

        protected get Pagina(): string {
            return this.Estado.Obtener(ltrClaveDeEstado.paginaActual);
        }

        protected get CabeceraDelFormulario(): HTMLDivElement {
            return document.getElementById(`cabecera-${this._idFormulario}`) as HTMLDivElement;
        }

        protected get CuerpoDelFormulario(): HTMLDivElement {
            return document.getElementById(`datos-${this._idFormulario}`) as HTMLDivElement;
        }

        protected get ModalFiltro(): HTMLDivElement {
            return document.getElementById(`filtro-${this._idFormulario}`) as HTMLDivElement;
        }

        public get Estado(): HistorialSe.EstadoPagina {
            if (this._estado === undefined) {
                throw new Error("Debe definir la variable estado");
            }
            return this._estado;
        }

        public set Estado(valor: HistorialSe.EstadoPagina) {
            this._estado = valor;
        }

        public get Controlador() {
            return this.CabeceraDelFormulario.getAttribute(literal.controlador);
        }
        public get Vista() {
            return this.CabeceraDelFormulario.getAttribute(literal.vista);
        }

        public get BotonDeAbrirModalDeFiltro(): HTMLInputElement {
            let selector = document.getElementById(`filtro-${this.IdFormulario}-abrir`) as HTMLInputElement;
            return selector;
        }

        protected get IdFormulario() {
            return this._idFormulario;
        }

        constructor(idFormulario: string) {
            this._idFormulario = idFormulario;
        }

        public AntesDeSalir(): void {
            PonerCapa();
            try {
                this.AccionesAntesDeSalir();
                this.Estado.Guardar();
                EntornoSe.Historial.Persistir();
            }
            catch (error) {
                MensajesSe.MostraExcepcion(error, "Antes de navegar", MensajesSe.enumTipoMensaje.error);
            }
            finally {
                QuitarCapa();
            }
        }

        public AccionesAntesDeSalir(): void {
            MensajesSe.Info(`Cerrando la página ${this.Pagina}`);
        }

        public InicializarFormulario(): void {
            if (EntornoSe.Historial.HayHistorial(this._idFormulario))
                this._estado = EntornoSe.Historial.ObtenerEstado(this._idFormulario);
            else
                this._estado = new HistorialSe.EstadoPagina(this._idFormulario);

            //this.CuerpoDelFormulario.style.overflowY = "scroll";
        }

        public Cerrar(): void {
            if (this.AntesDeCerrar()) EntornoSe.NavegarAtras();
        }

        protected AntesDeCerrar(): boolean {
            return true;
        }

        public Aceptar(): void {
            if (this.AntesDeAceptar()) {
                this.Cerrar();
            }
        }
        protected AntesDeAceptar(): boolean {
            return true;
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
        }

        public AplicarFiltro(): void {
            this.CerrarFiltro();
            this.Filtrar();
        }

        public CerrarFiltro(): void {
            ApiPanel.OcultarModal(this.ModalFiltro);
            if (this.HayFiltros())
                this.BotonDeAbrirModalDeFiltro.classList.add(ltrCss.cambioDeColorElBoton);
            else
                this.BotonDeAbrirModalDeFiltro.classList.remove(ltrCss.cambioDeColorElBoton);
        }

        public HayFiltros(): boolean {
            return false;
        }

        public AbrirFiltro(): void {
            ApiPanel.AbrirModal(this.ModalFiltro);
        }

        public TeclaPulsada(e): void {
            if (e.keyCode === 13 && !e.shiftKey) {
                this.AplicarFiltro();
                e.preventDefault();
            }
        }

        public ProcesarOpcionMf(opcion: string, esContextual: boolean): void {
            switch (opcion) {
                default:
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${opcion} no está definida`);
                    break;
            }
        }

        public Filtrar(): void {
        }

        public ModificarDatosDeLaModal(modal: HTMLDivElement) {
            let json: JSON = ApiPanel.MapearControlesDesdeElPanelAlJson(modal, MapearAlJson.Id(modal));
            let controlador = modal.getAttribute(literal.controlador);
            let idNegocio = Numero(modal.getAttribute(literal.idNegocio));
            ApiDePeticiones.ModificarElemento(this, controlador, Ajax.EndPoint.ModificarPorId, idNegocio, json, new Array<Parametro>(), new Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.TrasModificarConLaModal(peticion, modal))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public TrasModificarConLaModal(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): any {
            ApiPanel.CerrarModal(modal);
        }

        public AntesDeMapearDatosDeIU(panel: HTMLDivElement, modoDeTrabajo: string): JSON {
            return JSON.parse(`{"${literal.id}":"0"}`);
        }

        public DespuesDeMapearDatosDeIU(formulario: Formulario.Base, panel: HTMLDivElement, elementoJson: JSON, modoDeTrabajo: string): JSON {
            return elementoJson;
        }
    }

    export function EventosDelFormulario(accion: string, parametros: any) {
        try {
            switch (accion) {
                case ltrEventos.Formulario.Aceptar: {
                    formulario.Aceptar();
                    break;
                }
                case ltrEventos.Formulario.Cerrar: {
                    formulario.Cerrar();
                    break;
                }
                case ltrEventos.Formulario.OcultarMostrarBloque: {
                    let idHtmlBloque: string = parametros;
                    formulario.OcultarMostrarBloque(idHtmlBloque);
                    break;
                }
                case ltrEventos.Formulario.AbrirFiltro: {
                    formulario.AbrirFiltro();
                    break;
                }
                case ltrEventos.Formulario.CerrarFiltro: {
                    formulario.CerrarFiltro();
                    break;
                }
                case ltrEventos.Formulario.AplicarFiltro: {
                    formulario.AplicarFiltro();
                    break;
                }
                case ltrEventos.Formulario.TeclaPulsada: {
                    formulario.TeclaPulsada(event);
                    break;
                }
                case ltrEventos.Formulario.OpcionMenuFlotante: {
                    let parIn: Array<string> = parametros.split("#");
                    formulario.ProcesarOpcionMf(parIn[0], EsTrue(parIn[1]));
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

    export function EventosModalDeEdicion(accion: string, idModal: string): void {
        try {
            switch (accion) {
                case ltrEventos.ModalEdicion.Cerrar: {
                    ApiPanel.CerrarModalPorId(idModal);
                    break;
                }
                case ltrEventos.ModalEdicion.Modificar: {
                    let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
                    formulario.ModificarDatosDeLaModal(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Modal de edición, accion: ${accion}`, error.message);
        }
    }

    export function EventosDeListaDinamica(accion: string, idLista: string) {
        try {
            let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
            switch (accion) {
                case ltrEventos.ListaDinamica.obtenerFoco: {
                    ApiListaDinamica.ObtenerFoco(lista);
                    break;
                }
                case ltrEventos.ListaDinamica.Cargar: {
                    ApiListaDinamica.Cargar(lista);
                    break;
                }
                case ltrEventos.ListaDinamica.perderFoco: {
                    ApiListaDinamica.PerderFoco(lista);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `la opción ${accion} no está definida`);
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Error(`Lista dinámica, accion: ${accion}`, error.message);
        }
    }

}