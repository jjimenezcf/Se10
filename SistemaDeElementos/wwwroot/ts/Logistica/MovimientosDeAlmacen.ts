namespace Logistica {

    export function CrearCrudDeMovimientosDeAlmacen(idPanelMnt: string) {
        Crud.crudMnt = new Logistica.CrudDeMovimientosDeAlmacen(idPanelMnt);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);
        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeMovimientosDeAlmacen extends Crud.CrudMnt {
        constructor(idPanelMnt: string) {
            super(idPanelMnt, undefined);
            this.crudDeCreacion = undefined;
            this.crudDeEdicion = undefined;
        }
    }
}
