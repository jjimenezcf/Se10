namespace Contabilidad {

    export function CrearCrudDeIvasRepercutido(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Contabilidad.CrudDeIvasRepercutido(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeIvasRepercutido extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionIvaRepercutido(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionIvaRepercutido(this, idPanelEdicion);
        }

    }

    export class CrudCreacionIvaRepercutido extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionIvaRepercutido extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            IvaRep_AlCambiar_Exento(ApiControl.BuscarCheck(this.PanelDelDto, ltrPropiedades.Maestros.Contabilidad.IvaR.Exento))
        }
    }

    export function IvaRep_AlCambiar_Exento(check: HTMLInputElement) {
        var panel = Crud.crudMnt.ModoTrabajo === enumModoTrabajo.editando
            ? (Crud.crudMnt.crudDeEdicion as CrudEdicionIvaRepercutido).PanelDelDto
            : (Crud.crudMnt.crudDeCreacion as CrudCreacionIvaRepercutido).PanelDeCrear;

        if (!check.checked) {
            ApiControl.DesbloquearEditorPorPropiedad(panel, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje);
            var motivo = ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Maestros.Contabilidad.IvaR.DescripcionFiscal);
            motivo.value = "";
        }
        else {
            let porcentaje = ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje);
            AsignarValor(porcentaje, "0");
            ApiControl.DesbloquearEditorPorPropiedad(panel, ltrPropiedades.Maestros.Contabilidad.IvaR.DescripcionFiscal);
        }
    }
}

