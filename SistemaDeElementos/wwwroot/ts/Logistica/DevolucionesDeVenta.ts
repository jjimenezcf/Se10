namespace Logistica {

    enum enumEtapasDeDevolucionDeVenta {
        DDV_Etapa_Tramitacion,
        DDV_Etapa_Aceptada,
        DDV_Etapa_Cancelada
    }

    export function CrearCrudDeDevolucionesDeVenta(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDeDevolucionesDeVenta(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeDevolucionesDeVenta extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionDevolucionDeVenta(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionDevolucionDeVenta(this, idPanelEdicion);
        }

    }

    export class CrudCreacionDevolucionDeVenta extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionDevolucionDeVenta extends Crud.CrudEdicion {

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Logistica.DevolucionesDeVenta.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Logistica.DevolucionesDeVenta.Lineas);
        }

        public get GridDeLineas(): HTMLDivElement {
            return document.getElementById(`grid-de-detalle-${ltrEspanes.Logistica.DevolucionesDeVenta.Lineas}-tabla`) as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get ModalDeLineas(): HTMLDivElement {
            return this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
        }

        public get IdReferenciaCrearLinea(): string {
            return this._idPanelEdicion + `-${ltrEspanes.Logistica.DevolucionesDeVenta.Lineas}-mcr-${enumPostfijoControl.Referencia}`;
        }

        // ver CrudEdicionVenta.UltimoOrdenPropuesto: al seguir creando, el grid se recarga por ajax y no sirve para calcular el orden
        private _ultimoOrdenPropuesto: number = 0;

        public get UltimoOrdenPropuesto(): number {
            return this._ultimoOrdenPropuesto;
        }
        public set UltimoOrdenPropuesto(valor: number) {
            this._ultimoOrdenPropuesto = valor;
        }

        private get _estaEnTramitacion(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeDevolucionDeVenta, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion);
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
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.DevolucionesDeVenta.Lineas) === idGrid)
                this.AjustarLineasSegunEtapa();
        }

        // Las líneas sólo se pueden modificar mientras la devolución se tramita
        private AjustarLineasSegunEtapa() {
            const idGrid = this.IdGridDelExpansor(ltrEspanes.Logistica.DevolucionesDeVenta.Lineas);
            const puedeModificarLineas = this._estaEnTramitacion && ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso);

            if (puedeModificarLineas)
                ApiDeGrid.Expansor_PonerEnEdicion(idGrid);
            else
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);

            ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearLinea, puedeModificarLineas);
        }

    }

    function ddv_Editor(): CrudEdicionDevolucionDeVenta {
        return Crud.crudMnt.crudDeEdicion as CrudEdicionDevolucionDeVenta;
    }

    export function Ddv_InicializarModalParaCrearLineas(incremento: number) {
        const editor = ddv_Editor();
        const modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        const tabla: HTMLDivElement = editor.GridDeLineas;
        let valor: number = 0;
        if (Definido(tabla)) {
            const tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
            if (tablarows.length > 1) {
                const ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Logistica.DevolucionDeVenta.linea.orden}]`) as HTMLInputElement;
                valor = Numero(ultimoOrden.value);
            }
        }
        valor = Math.max(valor, editor.UltimoOrdenPropuesto);

        const orden = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + incremento).toString();
        editor.UltimoOrdenPropuesto = valor + incremento;

        ddv_MapearLineaDeVenta_interno(modal, undefined);
        const venta = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.venta);
        if (Definido(venta)) venta.focus();
    }

    // Al seleccionar el documento de venta se cargan en la lista sus líneas (LineasDeUnaVentaController.LeerElementos exige el negocio y
    // el elemento)
    export function Ddv_Tras_Seleccionar_Venta() {
        const modal = ddv_Editor().ModalDeLineas;
        const ventaLista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.venta);
        const venta = OpcionesDeLasListas.ObtenerObjeto(ventaLista);
        const lineas = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.lineaDeVenta);
        ddv_MapearLineaDeVenta_interno(modal, undefined);
        if (NoDefinido(venta) || NoDefinido(lineas))
            return;

        const filtros = new Array<ClausulaDeFiltrado>();
        filtros.push(new ClausulaDeFiltrado(literal.enumNegocio.toLowerCase(), atCriterio.igual, enumNegocio.Venta));
        filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Elemento.IdElemento, atCriterio.igual, ObtenerPropiedad(venta, 'id', 0)));
        ApiDeInicializacion.RecargarListaDeElementos(lineas, filtros, 0);
    }

    export function Ddv_Tras_Blanquear_Venta() {
        const modal = ddv_Editor().ModalDeLineas;
        const lineas = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.lineaDeVenta);
        if (Definido(lineas))
            ApiControl.QuitarOpcionesDeLalista(lineas);
        ddv_MapearLineaDeVenta_interno(modal, undefined);
    }

    // Se propone la cantidad vendida, y el precio y el descuento de la línea vendida (no editables)
    export function Ddv_Tras_Seleccionar_LineaDeVenta() {
        const modal = ddv_Editor().ModalDeLineas;
        const lineas = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.lineaDeVenta);
        const linea = Definido(lineas) ? OpcionesDeLasListas.ObtenerObjeto(lineas) : undefined;
        ddv_MapearLineaDeVenta_interno(modal, linea);
    }

    function ddv_MapearLineaDeVenta_interno(modal: HTMLDivElement, linea: any) {
        const cantidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.cantidad);
        const precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.precio);
        const descuento = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.descuento);
        if (NoDefinido(linea)) {
            cantidad.value = "";
            precio.value = "";
            descuento.value = "";
        }
        else {
            AsignarValor(cantidad, ObtenerPropiedad(linea, ltrPropiedades.Logistica.DevolucionDeVenta.linea.cantidad, 0));
            AsignarValor(precio, ObtenerPropiedad(linea, ltrPropiedades.Logistica.DevolucionDeVenta.linea.precio, 0));
            AsignarValor(descuento, ObtenerPropiedad(linea, ltrPropiedades.Logistica.DevolucionDeVenta.linea.descuento, 0));
        }
        Ddv_CalcularImporteDeLinea();
    }

    // Importe devuelto, sin iva: cantidad x precio - descuento
    export function Ddv_CalcularImporteDeLinea() {
        const modal = ddv_Editor().ModalDeLineas;
        const cantidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.cantidad);
        const precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.precio);
        const descuento = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.descuento);
        const importe = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.DevolucionDeVenta.linea.importe);
        const sinDto = Numero(cantidad.value) * Numero(precio.value);
        AsignarValor(importe, sinDto - sinDto * Numero(descuento.value) / 100);
    }
}
