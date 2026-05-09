namespace Crud {

    export class ModalParaSeleccionar extends ModalConGrid {

        private _crud: CrudMnt;
        protected get Crud(): CrudMnt {
            return this._crud;
        }

        protected get PropiedadRestrictora(): string {
            return this.Modal.getAttribute(atControl.propiedadRestrictora);
        }

        protected get Restrictor(): HTMLInputElement {
            let propiedadRestrictora: string = this.PropiedadRestrictora;
            if (IsNullOrEmpty(propiedadRestrictora))
                throw new Error(`la modal ${this.IdModal} no tiene definida la ${propiedadRestrictora}`);

            let input: HTMLInputElement = this.PanelFiltro.querySelector(`input[${atControl.propiedad}="${propiedadRestrictora}"]`);
            if (input === null)
                throw new Error(`No se ha definido el control input asociado a la ${propiedadRestrictora}`);

            return input;
        }

        private _selector: HTMLDivElement;

        private get EditorDeFiltro(): HTMLInputElement {
            var idEditorDeFiltro: string = this._selector.getAttribute(atSelectorDeElementos.IdEditorDeFiltro);
            let editorDeFiltro: HTMLInputElement = document.getElementById(idEditorDeFiltro) as HTMLInputElement;
            if (NoDefinido(editorDeFiltro))
                throw new Error(`el editor ${idEditorDeFiltro} no está definido en la zona de filtro de la modal asociada al selector ${this._selector.id}`);
            return editorDeFiltro;
        }

        private get EditorAsociado(): HTMLInputElement {
            return ApiPanel.ObtenerEditorAsociadoAlSelector(this._selector);
        }


        constructor(crudPadre: CrudMnt, idModal: string) {
            super(idModal, document.getElementById(idModal).getAttribute(atControl.crudModal));
            this._crud = crudPadre;
        }

        protected InicializarModalParaSeleccionar(selector: HTMLDivElement) {
            this.InicializarModalConGrid();
            this._selector = selector;
            this.EditorDeFiltro.value = this.EditorAsociado.value;
        }

        public AbrirModalParaSeleccionar(selector: HTMLDivElement) {
            this.InicializarModalParaSeleccionar(selector);
            this.CargarGrid()
                .then((valor) => {
                    if (!valor) {
                        ApiPanel.CerrarModal(this.Modal);
                        let idModal: string = selector.getAttribute(atSelectorDeElementos.ModalPadre);
                        if (!NoDefinido(idModal)) ApiPanel.AbrirModalPorId(idModal);
                    }
                    else {
                        let cerrar: boolean = EsTrue(selector.getAttribute(atSelectorDeElementos.CerrarAutomaticamente));
                        if (this.Navegador.Total === 1 && this.Navegador.Cantidad >= 1 && cerrar) {
                            this.InfoSelector.InsertarElemento(this.DatosDelGrid.ObtenerPorPosicion(0));
                            this.CerrarModalParaSeleccionar();
                        }
                    }
                })
                .catch((valor) => {
                    if (valor instanceof Error)
                        MensajesSe.Error("RecargarGrid", valor.message);
                    ApiPanel.CerrarModal(this.Modal);
                    let idModal: string = selector.getAttribute(atSelectorDeElementos.ModalPadre);
                    if (!NoDefinido(idModal)) ApiPanel.AbrirModalPorId(idModal);
                }
                );
        };

        public CerrarModalParaSeleccionar() {   
            if (this.InfoSelector.Cantidad >= 1) {
                this.EditorAsociado.setAttribute(atSelectorDeElementos.Seleccionados, "");
                this.EditorAsociado.value = "";

                for (var i = this.InfoSelector.Cantidad - 1; i >= 0; i--) {
                    let elemento: Elemento = this.InfoSelector.LeerElemento(i);

                    if (IsNullOrEmpty(this.EditorAsociado.value)) {
                        this.EditorAsociado.setAttribute(atSelectorDeElementos.Seleccionados, `${elemento.Id}`);
                        this.EditorAsociado.value = elemento.Texto;
                    }
                    else {
                        let seleccionados = this.EditorAsociado.getAttribute(atSelectorDeElementos.Seleccionados);
                        this.EditorAsociado.setAttribute(atSelectorDeElementos.Seleccionados, `${seleccionados},${elemento.Id}`);
                        this.EditorAsociado.value = `${this.EditorAsociado.value} | ${elemento.Texto}`;
                    }
                }
            }
            this.CerrarModalConGrid();
            let idmodal: string = this._selector.getAttribute(atSelectorDeElementos.ModalPadre);
            if (!NoDefinido(idmodal))
                ApiPanel.AbrirModalPorId(idmodal);
        }


    }
}
