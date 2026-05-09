namespace Negocio {

    export function CrearCrudDeTransiciones(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDeTransiciones(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeTransiciones extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionTransicion(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionTransicion(this, idPanelEdicion);
        }

        protected ParametrosOpcionalesBorrar(): Array<Parametro> {
            var p = super.ParametrosOpcionalesBorrar();
            let control = ApiControl.BuscarRestrictor(this.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            p.push(new Parametro(ltrPropiedades.Negocio.idNegocio, Numero(control.getAttribute(atControl.restrictor))));
            return p;
        }

    }

    export class CrudCreacionTransicion extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            let control = ApiControl.BuscarRestrictor(this.CrudDeMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeEdicion);
            let filtro = ApiControl.BuscarRestrictor(this.CrudDeMnt.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            control.setAttribute(atControl.restrictor, filtro.getAttribute(atControl.restrictor));
            control.value = filtro.value;
        }

    }
    export class CrudEdicionTransicion extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            var p = super.ParametrosParaLeerElementoPorId();
            let control = ApiControl.BuscarRestrictor(this.CrudDeMnt.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            p.push(new Parametro(ltrPropiedades.Negocio.idNegocio, Numero(control.getAttribute(atControl.restrictor))));
            return p;
        }
    }

    export function EsDelSistema_Change(check: HTMLInputElement) {
        console.log(`He pulsado en ${check.id}`);
    }
}