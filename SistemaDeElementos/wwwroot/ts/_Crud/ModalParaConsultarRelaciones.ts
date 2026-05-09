namespace Crud {

    export class ModalParaConsultarRelaciones extends ModalConGrid {

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

        constructor(crudPadre: CrudMnt, idModal: string) {
            super(idModal, document.getElementById(idModal).getAttribute(atControl.crudModal));
            this._crud = crudPadre;
        }

        public AbrirModalParaConsultarRelaciones(seleccionado: Elemento) {
            this.InicializarModalConGrid();
            this.Restrictor.value = seleccionado.Texto;
            this.Restrictor.setAttribute(atControl.restrictor, seleccionado.Id.toString());

            this.CargarGrid()
                .then((valor) => {
                    if (!valor)
                        ApiPanel.CerrarModal(this.Modal);
                })
                .catch((valor) => {
                    if (valor instanceof Error)
                        MensajesSe.Error("RecargarGrid", valor.message);
                    ApiPanel.CerrarModal(this.Modal);
                }
                );
        };

        public CerrarModalParaConsultarRelaciones() {
            this.CerrarModalConGrid();
        }


    }
}
