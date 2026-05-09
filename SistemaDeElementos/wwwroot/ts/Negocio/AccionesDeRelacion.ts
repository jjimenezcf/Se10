namespace Negocio {

    export function CrearCrudDeAccionesDeRelacion(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDeAccionesDeRelacion(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeAccionesDeRelacion extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionAccionDeRelacion(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionAccionDeRelacion(this, idPanelEdicion);
        }
    }

    export class CrudCreacionAccionDeRelacion extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }
    export class CrudEdicionAccionDeRelacion extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

    }

    export function Negocio_TrasSeleccionarAccionDeRelacion() {
        //obtener el id de la acción
        const lista = Crud.crudMnt.EstoyCreando ?
            ApiControl.BuscarListaDinamicaPorPropiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, 'accion'):
            ApiControl.BuscarListaDinamicaPorPropiedad(Crud.crudMnt.crudDeEdicion.PanelDelDto, 'accion');
        let id: number = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
        if (id > 0) {
            ApiDePeticiones.LeerElementoPorId(lista, ltrControladores.Entorno.Acciones, id, new Array<Parametro>(), id)
                .then((peticion: ApiDeAjax.DescriptorAjax) => MapearAccion(Crud.crudMnt, peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion))
        }
    }

    function MapearAccion(crud: Negocio.CrudDeAccionesDeRelacion, peticion: ApiDeAjax.DescriptorAjax): void {
        if (crud.ModoTrabajo === enumModoTrabajo.creando) {
            let control: HTMLTextAreaElement = ApiControl.BuscarControl(crud.crudDeCreacion.PanelDeCrear, "descripcion", true) as HTMLTextAreaElement;
            control.value = ObtenerPropiedad(peticion.resultado.datos, "descripcion", "", true);
        }
    }
}