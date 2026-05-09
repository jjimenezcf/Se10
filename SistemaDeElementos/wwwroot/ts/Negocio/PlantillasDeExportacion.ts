namespace Negocio {

    export function CrearCrudDePlantillasDeExportacion(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDePlantillasDeExportacion(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePlantillasDeExportacion extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionDePlantillaDeExportacion(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionDePlantillaDeExportacion(this, idPanelEdicion);
        }
    }

    export class CrudCreacionDePlantillaDeExportacion extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionDePlantillaDeExportacion extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }
}