namespace Logistica {

    enum enumEtapasDeRegularizacion {
        RAL_Inicial,
        RAL_Recontando,
        RAL_Cerrado,
        RAL_Cancelar
    }

    export function CrearCrudDeRegularizaciones(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDeRegularizaciones(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeRegularizaciones extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionRegularizacion(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionRegularizacion(this, idPanelEdicion);
        }

        public get ModalImportarRecuento(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Regularizaciones.ImportarRecuento) as HTMLDivElement; }

        // Somete el trabajo que importa el Excel como líneas de la regularización que se está editando
        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id !== this.ModalImportarRecuento.id) {
                super.ModalDePedirDatos_Aceptar(modal);
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
            parametros.push(new Parametro(ltrPropiedades.Logistica.Regularizacion.Recuento.IdRegularizacion, this.crudDeEdicion.ElementoEditado.Id));

            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Logistica.Regularizaciones.ImportarRecuento, parametros, new Array<Parametro>())
                .then((peticion) => {
                    super.ModalDePedirDatos_Cerrar(modal);
                    MensajesSe.Info(peticion.resultado.consola);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
    }

    export class CrudCreacionRegularizacion extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionRegularizacion extends Crud.CrudEdicion {

        public get ModalDeCreacionDeLineas(): HTMLDivElement {            
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Logistica.Regularizaciones.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Logistica.Regularizaciones.Lineas);
        }

        public get GridDeLineas(): HTMLDivElement {
            return document.getElementById('grid-de-detalle-lineasdeunaregularizacion-tabla') as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get IdReferenciaCrearLinea(): string {
            return this._idPanelEdicion + `-lineasdeunaregularizacion-mcr-${enumPostfijoControl.Referencia}`;
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

        private get _estaRecontando(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeRegularizacion, enumEtapasDeRegularizacion.RAL_Recontando);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public CargaCompletada() {
            super.CargaCompletada();
            this._ultimoOrdenPropuesto = 0;
            this.AjustarLineasSegunEtapa();
        }

        public Expansor_TrasCargarDetalle(idGrid: string) {
            super.Expansor_TrasCargarDetalle(idGrid);
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Regularizaciones.Lineas) === idGrid)
                this.AjustarLineasSegunEtapa();
        }

        private AjustarLineasSegunEtapa() {
            const idGrid = this.IdGridDelExpansor(ltrEspanes.Logistica.Regularizaciones.Lineas);
            const puedeModificarLineas = this._estaRecontando && ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso);

            if (puedeModificarLineas)
                ApiDeGrid.Expansor_PonerEnEdicion(idGrid);
            else
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);

            ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearLinea, puedeModificarLineas);

            // importar un recuento crea líneas, así que sólo se permite cuando se pueden modificar
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Logistica.Regularizaciones.ImportarRecuento, ltrMenus.enumOrigen.edicion, puedeModificarLineas);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Logistica.Regularizaciones.ImportarRecuento) {
                if (!this._estaRecontando)
                    MensajesSe.Error('Importar recuento', 'Solo se puede importar un recuento con la regularización recontando');
                else
                    this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDeRegularizaciones).ModalImportarRecuento.id, this.ElementoEditado.Id);
                return true;
            }
            return false;
        }

    }
}
