namespace Crud {

    export class ModalParaImputar extends ModalConGrid {

        private _crud: CrudMnt;
        protected get Crud(): CrudMnt {
            return this._crud;
        }

        protected get IdNegocioDeSeleccion(): number {
            return Numero(this.Modal.getAttribute(atModalParaImputar.idNegocio));
        }

        protected get PropiedadRestrictora(): string {
            return this.Modal.getAttribute(atControl.propiedadRestrictora);
        }

        protected get FaltaRestrictor(): string {
            return this.Modal.getAttribute(atModalParaImputar.faltaRestrictor);
        }

        protected get FiltrarPor(): string {
            return this.Modal.getAttribute(atControl.filtrarPor);
        }

        protected get IdRestrictor(): number {
            //let propiedadRestrictora: string = this.PropiedadRestrictora;
            //if (IsNullOrEmpty(propiedadRestrictora))
            //    throw new Error(`la modal ${this.IdModal} no tiene definida la ${propiedadRestrictora}`);

            //let input: HTMLInputElement = this.Crud.ZonaDeFiltro.querySelector(`input[${atControl.propiedad}="${propiedadRestrictora}"]`);
            //if (input === null) {
            //    let divs: NodeListOf<HTMLDivElement> = this.Crud.Cuerpo.querySelectorAll(`div[${atControl.tipoModal}=${enumTipoDeModal.ModalDeFiltrado}`) as NodeListOf<HTMLDivElement>;
            //    for (let i: number = 0; i < divs.length; i++) {
            //        input = divs[i].querySelector(`input[${atControl.propiedad}="${propiedadRestrictora}"]`);
            //        if (Definido(input))
            //            break;
            //    }
            //    if (input === null)  throw new Error(`No se ha definido el control input asociado a la ${propiedadRestrictora}`);
            //}

            //let idRestrictor: string = input.getAttribute(atControl.tipo) === ltrTipoControl.ListaDinamica ? input.getAttribute(atListasDinamicas.idSeleccionado) : input.getAttribute(atControl.restrictor);
            //if (IsNullOrEmpty(idRestrictor))
            //    throw new Error(this.FaltaRestrictor);

            return this.Crud.InfoSelector.IdsSeleccionados[0];

        }

        constructor(crudPadre: CrudMnt, idModal: string) {
            super(idModal, document.getElementById(idModal).getAttribute(atControl.crudModal));
            this._crud = crudPadre;
        }

        public InicializarModalParaImputar() {
            super.InicializarModalConGrid();
        };

        public CerrarModalParaImputar() {
            this.CerrarModalConGrid();
        }

        public AbrirModalParaImputar() {

            if (this.Crud.InfoSelector.Seleccionados.length === 0)
                throw new Error(this.FaltaRestrictor);

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
        }


        public Imputar() {
            if (this.InfoSelector.Seleccionados.length == 0)
                throw new Error("Debe seleccionar algún registro a imputar");

            ApiDePeticiones.Imputar(this, this.Crud.Controlador, this.IdNegocioDeSeleccion, this.PropiedadRestrictora, this.IdRestrictor, this.InfoSelector.IdsSeleccionados)
                .then((peticion) => this.DespuesDeImputar(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private DespuesDeImputar(peticion: ApiDeAjax.DescriptorAjax) {
            let modlParaImputar: ModalParaImputar = peticion.llamador as ModalParaImputar;
            modlParaImputar.InfoSelector.QuitarTodos();
            modlParaImputar.CargarGrid();
            modlParaImputar.Crud.RestaurarPagina();
        }

        protected FiltrosExcluyentes(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExcluyentes(operacion, clausulas);
            //let propiedad: string = this.PropiedadRestrictora;
            let filtrarPor: string = this.FiltrarPor;
            let criterio: string = literal.filtro.criterio.diferente;
            let valor = this.IdRestrictor;
            let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(filtrarPor, criterio, valor.toString());
            clausulas.push(clausula);
            return clausulas
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);
            let criterio: string = this.Modal.getAttribute(atModalParaImputar.criterio);
            if (Definido(criterio) && criterio === literal.filtro.criterio.diferente)
                return clausulas;

            let propiedad: string = this.Modal.getAttribute(atModalParaImputar.filtrarPor);
            let valor = this.IdRestrictor;
            let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
            clausulas.push(clausula);
            return clausulas;
        }
    }
}
