namespace Contabilidad {

    export function CrearCrudDeIvasSoportado(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Contabilidad.CrudDeIvasSoportado(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeIvasSoportado extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionIvaSoportado(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionIvaSoportado(this, idPanelEdicion);
        }

    }

    export class CrudCreacionIvaSoportado extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
       
    }

    export class CrudEdicionIvaSoportado extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            IvaSop_AlCambiar_Exento(ApiControl.BuscarCheck(this.PanelDelDto, ltrPropiedades.Maestros.Contabilidad.IvaS.Exento))
        }

    }

    export function IvaSop_AlCambiar_Exento(check: HTMLInputElement) {
        var panel = Crud.crudMnt.ModoTrabajo === enumModoTrabajo.editando
            ? (Crud.crudMnt.crudDeEdicion as CrudEdicionIvaSoportado).PanelDelDto
            : (Crud.crudMnt.crudDeCreacion as CrudCreacionIvaSoportado).PanelDeCrear;

        if (!check.checked)
            ApiControl.DesbloquearEditorPorPropiedad(panel, ltrPropiedades.Maestros.Contabilidad.IvaS.Porcentaje);
        else {
            let porcentaje = ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Maestros.Contabilidad.IvaS.Porcentaje);
            AsignarValor(porcentaje, "0");
        }
    }
}

