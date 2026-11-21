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

    // Recalcula el total de la modal de venta de material al salir de la cantidad o del precio (ver CrearVentaDto)
    export function Dsa_CalcularTotalDeLaVenta() {
        (Crud.crudMnt as CrudDeInventario).CalcularTotalDeLaVenta();
    }

    // Facturar exige servido: al marcar facturar se marca servido (ver CrearVentaDto)
    export function Dsa_AlCambiarFacturar(check: HTMLInputElement) {
        if (!check.checked) return;
        let servido = ApiControl.BuscarCheck((Crud.crudMnt as CrudDeInventario).ModalVenderMaterial, ltrPropiedades.Logistica.Inventario.Venta.Servido);
        if (Definido(servido)) servido.checked = true;
    }

    // al desmarcar servido no se puede facturar: se desmarca facturar
    export function Dsa_AlCambiarServido(check: HTMLInputElement) {
        if (check.checked) return;
        let facturar = ApiControl.BuscarCheck((Crud.crudMnt as CrudDeInventario).ModalVenderMaterial, ltrPropiedades.Logistica.Inventario.Venta.Facturar);
        if (Definido(facturar)) facturar.checked = false;
    }

    export class CrudDeInventario extends Crud.CrudMnt {
        constructor(idPanelMnt: string) {
            super(idPanelMnt, undefined);
            this.crudDeCreacion = undefined;
            this.crudDeEdicion = undefined;
        }

        public get ModalPrepararRecuento(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Inventario.PrepararRecuento) as HTMLDivElement; }

        public get ModalVenderMaterial(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Inventario.VenderMaterial) as HTMLDivElement; }

        // "Venta de material": no pasa por el servidor como opción de menú, se piden los datos de la fila seleccionada y se abre la modal
        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion !== ltrMenus.eventosDeMf.Logistica.Inventario.VenderMaterial) {
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
                return;
            }

            if (this.InfoSelector.Seleccionados.length !== 1) {
                MensajesSe.Error('Venta de material', 'Seleccione la línea del material que quiere vender');
                return;
            }
            this.AbrirModalDeVenta(this.InfoSelector.Seleccionados[0].Id);
        }

        // El id de la fila es el de su último movimiento: el servidor devuelve su almacén, su material y el precio de venta del material
        private AbrirModalDeVenta(idMovimiento: number) {
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(literal.id, idMovimiento));
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Logistica.Inventario.DatosParaVender, parametros, new Array<Parametro>())
                .then((peticion) => {
                    const datos = peticion.resultado.datos;
                    const modal = this.ModalVenderMaterial;
                    const propiedades = ltrPropiedades.Logistica.Inventario.Venta;
                    ApiPanel.BlanquearControlesDeIU(modal);
                    MapearAlControl.Restrictor(ApiControl.BuscarRestrictor(modal, propiedades.IdAlmacen, ltrTipoControl.restrictorDeEdicion),
                        Numero(ObtenerPropiedadJson(datos, 'idAlmacen')), ObtenerPropiedadJson(datos, 'almacen') ?? '', true);
                    MapearAlControl.Restrictor(ApiControl.BuscarRestrictor(modal, propiedades.IdUnitario, ltrTipoControl.restrictorDeEdicion),
                        Numero(ObtenerPropiedadJson(datos, 'idUnitario')), ObtenerPropiedadJson(datos, 'unitario') ?? '', true);
                    // la fecha va sin hora (la de los movimientos de almacén) y se propone hoy
                    let hoy = new Date();
                    hoy.setHours(0, 0, 0, 0);
                    MapearAlControl.FechaDate(ApiControl.BuscarControl(modal, propiedades.Fecha, true) as HTMLInputElement, hoy);
                    ApiControl.BuscarEditor(modal, propiedades.Cantidad).value = '';
                    AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Precio), Numero(ObtenerPropiedadJson(datos, 'precio')));
                    this.CalcularTotalDeLaVenta();
                    for (const propiedad of [propiedades.Servido, propiedades.Facturar]) {
                        let check = ApiControl.BuscarCheck(modal, propiedad);
                        if (Definido(check)) check.checked = false;
                    }
                    ApiPanel.AbrirModal(modal);
                    let cliente = ApiControl.BuscarListaDinamicaPorPropiedad(modal, propiedades.Cliente) as HTMLInputElement;
                    if (Definido(cliente)) cliente.focus();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private ValorNumerico(modal: HTMLDivElement, propiedad: string): number {
            let editor = ApiControl.BuscarEditor(modal, propiedad);
            if (!Definido(editor) || IsNullOrEmpty(editor.value)) return 0;
            return Numero(ApiControl.Valor(editor));
        }

        // total = cantidad * precio, sólo se muestra (el servidor lo vuelve a calcular con la línea de la venta)
        public CalcularTotalDeLaVenta() {
            const modal = this.ModalVenderMaterial;
            const propiedades = ltrPropiedades.Logistica.Inventario.Venta;
            let total = this.ValorNumerico(modal, propiedades.Cantidad) * this.ValorNumerico(modal, propiedades.Precio);
            let editorDelTotal = ApiControl.BuscarEditor(modal, propiedades.Total);
            if (total > 0) AsignarValor(editorDelTotal, Math.round(total * 100) / 100);
            else editorDelTotal.value = '';
        }

        // Crea el documento de venta con su línea y la salida del almacén (y, según los checks, lo sirve y lo prefactura); después edita en
        // otra pestaña la venta, o la prefactura si se ha facturado (el servidor devuelve la url), y recarga el inventario
        private CrearVenta(modal: HTMLDivElement) {
            const propiedades = ltrPropiedades.Logistica.Inventario.Venta;
            let cliente = ApiControl.BuscarListaDinamicaPorPropiedad(modal, propiedades.Cliente) as HTMLInputElement;
            if (!Definido(cliente) || Numero(cliente.getAttribute(atListasDinamicas.idSeleccionado)) <= 0) {
                MensajesSe.Error('Venta de material', 'Seleccione el cliente al que vende');
                return;
            }
            let fecha = ApiControl.BuscarControl(modal, propiedades.Fecha, true) as HTMLInputElement;
            if (!Definido(fecha) || IsNullOrEmpty(fecha.value)) {
                MensajesSe.Error('Venta de material', 'Indique la fecha de la venta');
                return;
            }
            if (!(this.ValorNumerico(modal, propiedades.Cantidad) > 0)) {
                MensajesSe.Error('Venta de material', 'Indique la cantidad que vende');
                return;
            }

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);

            ApiDePeticiones.EjecutarPeticion(this, ltrControladores.Logistica.Ventas, Ajax.EndPoint.Logistica.Ventas.CrearVenta, parametros, datosDeEntrada)
                .then((peticion) => {
                    super.ModalDePedirDatos_Cerrar(modal);
                    MensajesSe.Info(peticion.resultado.consola);
                    EntornoSe.AbrirPestana(`${window.location.origin}/${ObtenerPropiedadJson(peticion.resultado.datos, 'url')}`);
                    this.CargarGrid();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        // "Preparar recuento": abre la modal con el almacén que se está consultando (el inventario no tiene edición, se abre directamente)
        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion !== ltrMenus.eventosDeMf.Logistica.Inventario.PrepararRecuento)
                return false;

            let modal = this.ModalPrepararRecuento;
            ApiPanel.BlanquearControlesDeIU(modal);
            // por defecto se prepara un recuento (el blanqueo deja el desplegable en la primera clase)
            let clase = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Inventario.ClaseDelRecuento, false);
            if (clase instanceof HTMLSelectElement)
                MapearAlControl.ListaDeValores(clase, ltrValores.Logistica.Regularizacion.Clase.Recuento);
            let almacenes = ApiControl.BuscarControl(this.PanelFiltro, ltrPropiedades.Logistica.Inventario.FiltroPorAlmacen, false);
            if (almacenes instanceof HTMLSelectElement && Numero(almacenes.value) > 0) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Inventario.AlmacenDelRecuento) as HTMLInputElement;
                MapearAlControl.ListaDinamicaSinAcciones(lista, Numero(almacenes.value), almacenes.selectedOptions[0].text);
            }
            ApiPanel.AbrirModal(modal);
            return true;
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id === this.ModalVenderMaterial.id) {
                this.CrearVenta(modal);
                return;
            }

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
