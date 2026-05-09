namespace Crud {

    export class ModalConGrid extends GridDeDatos {

        constructor(idModal: string, idCrudModal: string) {
            super(idCrudModal);
            this.IdModal = idModal;
        }

        protected InicializarModalConGrid() {
            let referenciaCheck: string = `${ltrMantenimiento.CheckDeSeleccion}.${this.IdGrid}`;
            this.blanquearCheck(referenciaCheck);
            this.InfoSelector.QuitarTodos();
        };

        protected CerrarModalConGrid() {
            try {
                this.ResetearSoloSeleccionadas(this);
                let referenciaCheck: string = `${ltrMantenimiento.CheckDeSeleccion}.${this.IdGrid}`;
                this.blanquearCheck(referenciaCheck);
                this.InfoSelector.QuitarTodos();
            }
            finally {
                ApiPanel.CerrarModal(this.Modal);
            }
        }

        private blanquearCheck(refCheckDeSeleccion: string) {
            document.getElementsByName(`${refCheckDeSeleccion}`).forEach(c => {
                let check = <HTMLInputElement>c;
                check.checked = false;
            }
            );
        }

    }
}