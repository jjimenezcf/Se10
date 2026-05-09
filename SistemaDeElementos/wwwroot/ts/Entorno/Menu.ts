namespace Entorno {

    export function CrearCrudDeMenus(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Entorno.CrudDeMenus(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeMenus extends Crud.CrudMnt {
        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionMenu(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionMenu(this, idPanelEdicion);
        }
        protected DespuesDeBorrar(peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeBorrar(peticion);
            Registro.EliminarArbolDeMenu();
        }
    }

    export class CrudCreacionMenu extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        protected DespuesDeCrear(peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeCrear(peticion);
            Registro.EliminarArbolDeMenu();
        }
    }

    export class CrudEdicionMenu extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeModificar(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement) {
            super.DespuesDeModificar(peticion, modal);
            Registro.EliminarArbolDeMenu();
        }
    }
}