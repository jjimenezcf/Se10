namespace Negocio {

    export function CrearCrudDeParametrosDeNegocio(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDeParametrosDeNegocio(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeParametrosDeNegocio extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionDeParametroDeNegocio(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionDeParametroDeNegocio(this, idPanelEdicion);
        }
    }

    export class CrudCreacionDeParametroDeNegocio extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionDeParametroDeNegocio extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }
}