namespace Terceros {

    export let crudDeBanco: Terceros.CrudDeBancos = null;


    export function CrearCrudDeBancos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeBancos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeBanco = Crud.crudMnt as Terceros.CrudDeBancos;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeBancos extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionBanco(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionBanco(this, idPanelEdicion);
        }

    }

    export class CrudCreacionBanco extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionBanco extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }
}