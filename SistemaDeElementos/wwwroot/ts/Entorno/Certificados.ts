namespace Entorno {

    const ltrCertificado = {
        clases: {
            personal: 'Personal'
        },
        propiedad: {
            clase: 'clase'
        }        
    }

    export function CrearCrudDeCertificados(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Entorno.CrudDeCertificados(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeCertificados extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionCertificado(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionCertificado(this, idPanelEdicion);
        }
    }

    export class CrudCreacionCertificado extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            ApiDelCrud.InicializarDatosDelCertificado(Crud.crudMnt.crudDeCreacion.PanelDeCrear);
        }

    }

    export class CrudEdicionCertificado extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax): void {
            super.AntesDeMapearElementoDevuelto(peticion);
            ApiDelCrud.InicializarDatosDelCertificado(Crud.crudMnt.crudDeEdicion.PanelDeEditar);
        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            if (ObtenerPropiedad(peticion.resultado.datos, ltrCertificado.propiedad.clase) === ltrCertificado.clases.personal) ApiPanel.PonerEnModoConsulta(panel);
        }
    }

    export function BlanquearPassword() {
        let panel: HTMLDivElement = Crud.crudMnt.ModoTrabajo === enumModoTrabajo.creando ? Crud.crudMnt.crudDeCreacion.PanelDeCrear : Crud.crudMnt.crudDeEdicion.PanelDeEditar;
        ApiDelCrud.BlanquearPasswordDeCertificado(panel);
    }


}