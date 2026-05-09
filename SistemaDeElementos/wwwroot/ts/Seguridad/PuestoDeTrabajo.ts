namespace Seguridad {

    export function CrearCrudDePuestosDeTrabajo(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Seguridad.CrudDePuestosDeTrabajo(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePuestosDeTrabajo extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPuestoDeTrabajo(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPuestoDeTrabajo(this, idPanelEdicion);
        }
    }

    export class CrudCreacionPuestoDeTrabajo extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionPuestoDeTrabajo extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);

            if (grid.id === this.IdGridDelExpansor('roles_1')) {
                let heredado: HTMLDivElement = document.getElementById(this.IdGridDelExpansor('heredados_1')) as HTMLDivElement;
                MapearAlGrid.MapearGridDeDetalle(heredado, idnegocio, id, this.CrudDeMnt.Guid);
            }
        }
        

    }
}