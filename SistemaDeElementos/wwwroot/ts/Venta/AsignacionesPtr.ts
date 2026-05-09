namespace Venta {

    export function CrearCrudDeAsignacionesPtr(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Venta.CrudDeAsignacionesPtr(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeAsignacionesPtr extends Crud.CrudMnt {

        public get ModalSolicitarFechaDeEjecucion(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.Partes.SolicitarFechaDeEjecucion) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionAsignacionPtr(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionAsignacionPtr(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);

            if (opcion === ltrMenus.eventosDeMf.Venta.Partes.SolicitarFechaDeEjecucion) {
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(this.ModalSolicitarFechaDeEjecucion.id, 0);
                return true;
            }
            return false;
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            if (modal.id === this.ModalSolicitarFechaDeEjecucion.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                parametros.push(new Parametro(Ajax.Param.ids, this.InfoSelector.IdsSeleccionados));
                let datosDeEntrada = new Array<Parametro>();
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Venta.AsignacionPtr.AplicarDatosDeEjecucion, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else
                super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionAsignacionPtr extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionAsignacionPtr extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Venta.PartesTr.Lineas);
        }
    }
}