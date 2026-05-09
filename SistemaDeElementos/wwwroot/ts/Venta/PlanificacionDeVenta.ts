namespace Venta {

    export function CrearCrudDePlanificacionesDeVenta(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Venta.CrudDePlanificacionesDeVenta(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePlanificacionesDeVenta extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPlanificacionDeVenta(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPlanificacionDeVenta(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            return Venta.NavegarARelacionesDePlv(peticion, ltrParametrosUrl.Venta.IdPlfDeVenta);
        }
    }

    export class CrudCreacionPlanificacionDeVenta extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}
    }

    export class CrudEdicionPlanificacionDeVenta extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        public AplicarTipoDeLinea() {
            let modal = this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let tipoDeLineaCtrl = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.PlfDeVenta.linea.tipoDeLinea, true) as HTMLSelectElement;
            let tipoDeLinea: number = tipoDeLineaCtrl.selectedIndex;

            switch (tipoDeLinea) {
                case 0: {
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.PlfDeVenta.linea.concepto);
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.PlfDeVenta.linea.unitario);
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.PlfDeVenta.linea.concepto);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.PlfDeVenta.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.PlfDeVenta.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.PlfDeVenta.linea.unidad);
                    break;
                }
                case 1: {
                    tipoDeLineaCtrl.selectedIndex = 0;
                    MensajesSe.Error("AplicarTipoDeLinea", "Una planificación de ventas no acepta partidas alzadas");
                    break;
                }
                case 2: {Venta
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.PlfDeVenta.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.PlfDeVenta.linea.unitario);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.PlfDeVenta.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.PlfDeVenta.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.PlfDeVenta.linea.unidad);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${tipoDeLinea} a la modal de crear una línea de una planificacion`);
                    break;
                }
            }
            const mostrar = tipoDeLinea === 2;
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 2, mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 3, !mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 4, !mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 5, !mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 6, !mostrar);

            this.plv_CalcularImportesDeLinea_interno(modal);
        }

        public plv_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.cantidad).value);
            let venta = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.venta).value);

            let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteSinDto) as HTMLInputElement;
            let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteDeDto) as HTMLInputElement;
            let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteDeIva) as HTMLInputElement;
            let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteDeLinea) as HTMLInputElement;

            let importeSinDescuento: number = cantidad * venta;
            if (importeSinDescuento !== 0) {
                let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.descuentoPorLinea).value);
                AsignarValor(impSinDto, importeSinDescuento.toString());
                AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
                let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

                let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.iva).value);
                let elIva = impConElDto * iva / 100;
                AsignarValor(ImporteDeIva, elIva.toString());
                AsignarValor(ImporteDeLinea, (impConElDto + elIva).toString());
            }
            else {
                impSinDto.value = "";
                impDeDto.value = "";
                ImporteDeIva.value = "";
                ImporteDeLinea.value = "";
            }
        }


        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Venta.PlfDeVenta.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Venta.PlfDeVenta.Lineas);
        }

        public get TablaDeLineas(): HTMLDivElement {
            return this.TablaDelDetalle(ltrEspanes.Venta.PlfDeVenta.Lineas) as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalParaEditarRelacion(ltrEspanes.Venta.PlfDeVenta.Lineas));
        }

        public get SelectorDeIvaActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Venta.PlfDeVenta.linea.selectorDeIvaR) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.PlfDeVenta.linea.selectorDeIvaR) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.PlfDeVenta.linea.selectorDeIvaR) as HTMLSelectElement;
        }

        public get PorcentageIvaActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Venta.PlfDeVenta.linea.iva) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.PlfDeVenta.linea.iva) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.PlfDeVenta.linea.iva) as HTMLInputElement;
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            Venta.Plv_AjustarControlesDeEdicion(this.PanelDeEditar, this.Registro);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.PlfDeVenta.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
        }
    }


    export function Plv_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta);
        plv_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Plv_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        plv_CalcularImportesDeLinea_interno(modal);
    }

    export function Plv_IvaRepercutidoCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta;
        let selectorIvaR = editor.SelectorDeIvaActivo;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaR);
        let iva = editor.PorcentageIvaActivo;
        if (Definido(objeto))
            AsignarValor(iva, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje));
        else
            ApiControl.BlanquearEditor(iva);
    }

    export function Plv_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            plv_mapearUnitarioSeleccionado_interno(objeto);
        }
        else {
            var tieneValorAsignado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0 && !IsNullOrEmpty(lista.value);
            if (!tieneValorAsignado)
                plv_InicializarModalDeLineas_interno();
        }
    }

    export function Plv_Tras_Blanquear_Unitario() {
        plv_InicializarModalDeLineas_interno();
    }

    export function Plv_InicializarModalParaCrearLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.TablaDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Venta.PlfDeVenta.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.PlfDeVenta.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + 10).toString();
        let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.PlfDeVenta.linea.unitario, true) as HTMLInputElement;
        unitario.focus();
        plv_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Plv_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta);
        let modal: HTMLDivElement = editor.ModalDeEdicionDeLineas;
    }

    export function NavegarARelacionesDePlv(peticion: ApiDeAjax.DescriptorAjax, idRestrictor: string): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Venta.Planificaciones.IrAFacturasEmt:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.Planificaciones.IrAPartesTr:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PartesTr}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }

    function plv_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.PlfDeVenta.linea.venta) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.PlfDeVenta.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.PlfDeVenta.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.PlfDeVenta.linea.unidad) as HTMLSelectElement;
        let selectorIvaR = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.PlfDeVenta.linea.selectorDeIvaR) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.PlfDeVenta.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Venta.PlfDeVenta.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        selectorIvaR.selectedIndex = 0;
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }

    function plv_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
        let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.cantidad).value);
        let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.venta).value);

        let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteSinDto) as HTMLInputElement;
        let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteDeDto) as HTMLInputElement;
        let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteDeIva) as HTMLInputElement;
        let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.ImporteDeLinea) as HTMLInputElement;

        let importeSinDescuento: number = cantidad * precio;
        if (importeSinDescuento > 0) {
            let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.descuentoPorLinea).value);
            AsignarValor(impSinDto, importeSinDescuento.toString());
            AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
            let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

            let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.PlfDeVenta.linea.iva).value);
            let elIva = impConElDto * iva / 100;
            AsignarValor(ImporteDeIva, elIva.toString());
            AsignarValor(ImporteDeLinea, (impConElDto + elIva).toString());
        }
        else {
            impSinDto.value = "";
            impDeDto.value = "";
            ImporteDeIva.value = "";
            ImporteDeLinea.value = "";
        }
    }

    function plv_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificacionDeVenta;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let venta = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.PlfDeVenta.linea.venta) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.PlfDeVenta.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.PlfDeVenta.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.PlfDeVenta.linea.unidad) as HTMLSelectElement;
        if (NoDefinido(unitario)) {
            venta.value = "";
            clase.selectedIndex = 0;
            naturaleza.selectedIndex = 0;
            unidad.selectedIndex = 0;
            return;
        }
        AsignarValor(venta, ObtenerPropiedad(unitario, ltrPropiedades.Venta.PlfDeVenta.linea.venta, 0));
        let claseDelUnitario = ObtenerPropiedad(unitario, ltrPropiedades.Venta.PlfDeVenta.linea.clase, 0);
        MapearAlControl.ListaDeValores((clase as HTMLSelectElement), claseDelUnitario);
        MapearAlControl.FijarEnListaDeElementos(naturaleza, ObtenerPropiedad(unitario, ltrPropiedades.Venta.PlfDeVenta.linea.idnaturaleza, 0));
        MapearAlControl.FijarEnListaDeElementos(unidad, ObtenerPropiedad(unitario, ltrPropiedades.Venta.PlfDeVenta.linea.idunidad, 0));
    }



}