namespace Negocio {

    export function CrearCrudDeAuditoria(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDeAuditoria(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir() ;
        };
    }

    export class CrudDeAuditoria extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionAuditoria(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionAuditoria(this, idPanelEdicion);
        }
    }
    export class CrudCreacionAuditoria extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }
    export class CrudEdicionAuditoria extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro>  {
            let parametros: Array<Parametro> = super.ParametrosParaLeerElementoPorId();
            let parametro: Parametro = new Parametro(ltrPropiedades.Negocio.idNegocio, this.CrudDeMnt.IdNegocio);
            parametros.push(parametro);

            let input = ApiControl.BuscarRestrictor(this.PanelDeEditar, atControl.idElemento, ltrTipoControl.restrictorDeEdicion);
            parametros.push(new Parametro(ltrPropiedades.Elemento.IdElemento, Numero(input.getAttribute(atControl.restrictor))));

            return parametros;
        }
    }
}