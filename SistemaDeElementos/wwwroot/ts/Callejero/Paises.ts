namespace Callejero {

    var ltrIsoSpain = 'ES'

    export function CrearCrudDePaises(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Callejero.CrudDePaises(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePaises extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPais(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPais(this, idPanelEdicion);
        }
    }

    export class CrudCreacionPais extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionPais extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            var checkUE = ApiControl.BuscarCheck(panel, ltrPropiedades.Callejero.Pais.EsUE);
            var Iso2 = ApiControl.BuscarEditor(panel, ltrPropiedades.Callejero.Pais.Iso2);
            ApiControl.BloquearCheck(checkUE, Iso2.value === ltrIsoSpain)
        }
    }
}