namespace TrabajosSometido {

    export function CrearCrudDeErroresDeUnTrabajo(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new TrabajosSometido.CrudDeErroresDeUnTrabajo(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeErroresDeUnTrabajo extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionErrorDeUnTrabajo(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionErrorDeUnTrabajo(this, idPanelEdicion);
        }
    }

    export class CrudCreacionErrorDeUnTrabajo extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionErrorDeUnTrabajo extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }


}