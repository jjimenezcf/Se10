namespace Logistica {

    enum enumEtapasDeVenta {
        DSA_Etapa_Solicitud,
        DSA_Etapa_Servicio,
        DSA_Etapa_Facturacion,
        DSA_Etapa_Cancelacion
    }

    export function CrearCrudDeVentas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDeVentas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeVentas extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionVenta(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionVenta(this, idPanelEdicion);
        }

    }

    export class CrudCreacionVenta extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionVenta extends Crud.CrudEdicion {

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Logistica.Ventas.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Logistica.Ventas.Lineas);
        }

        public get GridDeLineas(): HTMLDivElement {
            return document.getElementById('grid-de-detalle-lineasdeunaventa-tabla') as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get IdReferenciaCrearLinea(): string {
            return this._idPanelEdicion + `-lineasdeunaventa-mcr-${enumPostfijoControl.Referencia}`;
        }

        // Al crear una línea y seguir creando (sin cerrar la modal), el grid de detalle se vacía
        // de forma síncrona y se recarga por ajax; en ese instante el grid está vacío por lo que no
        // sirve para calcular el siguiente orden. Se recuerda aquí el último orden propuesto.
        private _ultimoOrdenPropuesto: number = 0;

        public get UltimoOrdenPropuesto(): number {
            return this._ultimoOrdenPropuesto;
        }
        public set UltimoOrdenPropuesto(valor: number) {
            this._ultimoOrdenPropuesto = valor;
        }

        private get _estaSolicitado(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeVenta, enumEtapasDeVenta.DSA_Etapa_Solicitud);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public CargaCompletada() {
            super.CargaCompletada();
            this.AjustarLineasSegunEtapa();
        }

        public Expansor_TrasCargarDetalle(idGrid: string) {
            super.Expansor_TrasCargarDetalle(idGrid);
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Ventas.Lineas) === idGrid)
                this.AjustarLineasSegunEtapa();
        }

        // Las líneas sólo se pueden modificar mientras el documento de venta está solicitado
        private AjustarLineasSegunEtapa() {
            const idGrid = this.IdGridDelExpansor(ltrEspanes.Logistica.Ventas.Lineas);
            const puedeModificarLineas = this._estaSolicitado && ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso);

            if (puedeModificarLineas)
                ApiDeGrid.Expansor_PonerEnEdicion(idGrid);
            else
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);

            ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearLinea, puedeModificarLineas);
        }

    }
}
