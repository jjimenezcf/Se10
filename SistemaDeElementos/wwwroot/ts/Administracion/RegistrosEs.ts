namespace Administracion {

    export function CrearCrudDeRegistrosEs(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Administracion.CrudDeRegistrosEs(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }


    export class CrudDeRegistrosEs extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionRegistroEs(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionRegistroEs(this, idPanelEdicion);
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Administracion.RegistroEs.CrearTareas) {
                let idModal = this.IdDeModalCrearVincoloCon('tareas'); 
                this.crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(idModal, this.InfoSelector.Seleccionados[0].Id);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }
    }

    export class CrudCreacionRegistroEs extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionRegistroEs extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public Expansor_TrasAbrirModal(idModal: string, acciones: string): HTMLDivElement {
            let modal = super.Expansor_TrasAbrirModal(idModal, acciones);
            if (acciones.indexOf(ltrAccionesModal.ProponerSolicitante) >= 0)
                ApiDelCrud.ProponerSolicitante(this, modal);
            return modal;
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Administracion.RegistroEs.CrearTareas) {
                let idModal = this.CrudDeMnt.IdDeModalCrearVincoloCon('tareas'); 
                this.Expansor_AbrirModalDeCrearVinculoCon(idModal, 0);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }
    }

}