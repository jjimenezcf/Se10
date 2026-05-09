namespace Venta {

    export function CrearCrudDeFacturasAeat(idPanelMnt: string) {
        Crud.crudMnt = new Venta.CrudDeFacturasAeat(idPanelMnt);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };

    }

    export class CrudDeFacturasAeat extends Crud.CrudMnt {

        constructor(idPanelMnt: string) {
            super(idPanelMnt, undefined);
            this.crudDeCreacion = undefined;
            this.crudDeEdicion = undefined;
        }
    }

}

