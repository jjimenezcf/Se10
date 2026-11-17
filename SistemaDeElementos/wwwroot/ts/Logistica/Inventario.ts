namespace Logistica {

    export function CrearCrudDeInventario(idPanelMnt: string) {
        Crud.crudMnt = new Logistica.CrudDeInventario(idPanelMnt);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);
        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    // Navega a la edición del material de la fila; el id de la fila es el de su último movimiento hasta la fecha del inventario
    export function Inv_IrAlUnitario(numeroDeFila: number) {
        let idMovimiento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, idMovimiento));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Logistica.Inventario.ObtenerUrlAlUnitario, parametros, new Array<Parametro>())
            .then((peticion) => EntornoSe.AbrirPestana(`${window.location.origin}/${peticion.resultado.datos}`))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export class CrudDeInventario extends Crud.CrudMnt {
        constructor(idPanelMnt: string) {
            super(idPanelMnt, undefined);
            this.crudDeCreacion = undefined;
            this.crudDeEdicion = undefined;
        }

        public get ModalPrepararRecuento(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Inventario.PrepararRecuento) as HTMLDivElement; }

        // "Preparar recuento": abre la modal con el almacén que se está consultando (el inventario no tiene edición, se abre directamente)
        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion !== ltrMenus.eventosDeMf.Logistica.Inventario.PrepararRecuento)
                return false;

            let modal = this.ModalPrepararRecuento;
            ApiPanel.BlanquearControlesDeIU(modal);
            let almacenes = ApiControl.BuscarControl(this.PanelFiltro, ltrPropiedades.Logistica.Inventario.FiltroPorAlmacen, false);
            if (almacenes instanceof HTMLSelectElement && Numero(almacenes.value) > 0) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Inventario.AlmacenDelRecuento) as HTMLInputElement;
                MapearAlControl.ListaDinamicaSinAcciones(lista, Numero(almacenes.value), almacenes.selectedOptions[0].text);
            }
            ApiPanel.AbrirModal(modal);
            return true;
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id !== this.ModalPrepararRecuento.id) {
                super.ModalDePedirDatos_Aceptar(modal);
                return;
            }

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);

            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Logistica.Inventario.PrepararRecuento, parametros, datosDeEntrada)
                .then((peticion) => {
                    super.ModalDePedirDatos_Cerrar(modal);
                    MensajesSe.Info(peticion.resultado.consola);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        // Propone la fecha de hoy y, si el usuario sólo tiene acceso a un almacén, lo fija; se hace antes de que el crud base cargue el grid,
        // así lo normal (una empresa con un almacén) es entrar y ver el inventario de hoy sin abrir el filtro. El render del descriptor se cachea
        // y no puede llevar la fecha; la lista de almacenes sí es la del usuario (el render se cachea por usuario y la selección aplica la seguridad)
        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            let fecha = ApiControl.BuscarEditor(this.PanelFiltro, ltrPropiedades.Logistica.Inventario.FiltroPorFechaDeInventario);
            if (Definido(fecha) && IsNullOrEmpty(fecha.value)) {
                let hoy = new Date();
                fecha.value = `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
            }

            let almacenFijado = false;
            let almacenes = ApiControl.BuscarControl(this.PanelFiltro, ltrPropiedades.Logistica.Inventario.FiltroPorAlmacen, false);
            if (almacenes instanceof HTMLSelectElement) {
                let opciones = Array.from(almacenes.options).filter(o => !IsNullOrEmpty(o.value) && Numero(o.value) > 0);
                if (opciones.length === 1)
                    almacenFijado = Numero(almacenes.value) === Numero(opciones[0].value) || MapearAlControl.ListaDeValores(almacenes, opciones[0].value);
            }

            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            if (almacenFijado)
                this.OcultarFiltro();
        }
    }
}
