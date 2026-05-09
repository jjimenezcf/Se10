namespace Callejero {

    export function CrearCrudDeProvincias(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Callejero.CrudDeProvincias(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeProvincias extends Crud.CrudMnt {


        protected get EditorDePais(): HTMLInputElement {
            let editor: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, Callejero.atributo.propiedad.idpais) as HTMLInputElement;
            if (NoDefinido(editor))
                MensajesSe.EmitirMensajeDeExcepcion("Propiedad EditorDePais", "No se lo caliza el editor de Pais en el filtro de provincia");
            return editor;
        };

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionProvincia(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionProvincia(this, idPanelEdicion);
        }

        public DespuesDeAplicarUnRestrictor(restrictor: Tipos.Restrictor) {
            super.DespuesDeAplicarUnRestrictor(restrictor);

            if (restrictor.Propiedad === Callejero.restrictor.codigoPostal) {
                ApiControl.BloquearInput(this.EditorDePais);
            }

        }

    }

    export class CrudCreacionProvincia extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionProvincia extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }
}