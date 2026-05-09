namespace Guarderias {

    export function CrearCrudDeAulasDeGuarderia(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Guarderias.CrudDeAulasDeGuarderia(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeAulasDeGuarderia extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionAulaDeGuarderia(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionAulaDeGuarderia(this, idPanelEdicion);
        }

    }

    export class CrudCreacionAulaDeGuarderia extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
       
    }

    export class CrudEdicionAulaDeGuarderia extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


    }
}

