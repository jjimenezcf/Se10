namespace Seguridad {

    export function CrearCrudDeRoles(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Seguridad.CrudDeRoles(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeRoles extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionRol(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionRol(this, idPanelEdicion);
        }
    }

    export class CrudCreacionRol extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        protected MapearRestrictoresDeFiltro() {
            super.MapearRestrictoresDeFiltro();
            var permiso = ApiControl.BuscarListaDinamicaPorPropiedad(this.CrudDeMnt.PanelFiltro, ltrPropiedades.Entorno.seguridad.rol.idpermiso)
            var idSeleccionado = Numero(permiso.getAttribute(atListasDinamicas.idSeleccionado));
            if (idSeleccionado > 0 && permiso.readOnly) {
                MapearAlControl.RestrictoresDeEdicion(this.PanelDeCrear, ltrPropiedades.Entorno.seguridad.rol.idpermiso, idSeleccionado, permiso.value);
            }
        }

    }

    export class CrudEdicionRol extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }
}