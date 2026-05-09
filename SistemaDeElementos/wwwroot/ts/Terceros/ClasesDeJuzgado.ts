namespace Terceros {

    export function CrearCrudDeClasesDeJuzgado(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeClasesDeJuzgado(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeClasesDeJuzgado extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionClaseDeJuzgado(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionClaseDeJuzgado(this, idPanelEdicion);
        }
    }

    export class CrudCreacionClaseDeJuzgado extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionClaseDeJuzgado extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


    }
}