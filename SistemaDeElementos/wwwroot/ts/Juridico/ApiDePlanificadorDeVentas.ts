namespace Juridico {

    export function Plv_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        plv_CalcularImportesDeLinea_interno(modal);
    }

    export function Plv_IvaRepercutidoCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta;
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
        //let opcion: HTMLOptionElement = ApiListaDinamica.BuscarOpcion(lista, lista.value);
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            //var objeto = OpcionesDeLasListas.ObtenerObjeto(lista); // JSON.parse(opcion.getAttribute(atControl.objeto));
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

    export function Plv_Antes_De_Buscar_Unitarios(filtros: Array<ClausulaDeFiltrado>, parametros: Array<Parametro>) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta
        parametros.push(new Parametro(Ajax.Planificador.idPlanificador, editor.IdPlanificador));
    }

    export function Plv_InicializarModalParaCrearLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.TablaDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Juridico.PlanificadorDeVenta.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + 10).toString();

        let lotePlv = ApiControl.BuscarListaDeElementos(editor.PanelDelDto, ltrPropiedades.Juridico.PlanificadorDeVenta.lote);
        MapearAlControl.Propiedad(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.idlote, Numero(lotePlv.value), lotePlv.options[lotePlv.options.selectedIndex].label, true, false);

        let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.tipoDeLinea, true) as HTMLSelectElement;
        tipoDeLinea.selectedIndex = 0;
        let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unitario, true) as HTMLInputElement;
        unitario.focus();
        plv_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Plv_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta);
        let modal: HTMLDivElement = editor.ModalDeEdicionDeLineas;
    }

    export function Plv_Tras_Seleccionar_Lote() {
        let panel: HTMLDivElement = Crud.crudMnt.EnumeradoDeNegocio === enumNegocio.PlanificadorDeVenta ? Crud.crudMnt.PanelDto : (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ModalEditarPlfDeVenta;
        plv_mapearDatosDelLoteSeleccionado(panel);
    }


    export function Plv_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta);
        plv_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    function plv_mapearDatosDelLoteSeleccionado(panel: HTMLDivElement) {
        let lote = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.lote) as HTMLSelectElement;
        let inicio = ApiControl.BuscarSelectorDeFecha(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.inicio);
        let hasta = ApiControl.BuscarSelectorDeFecha(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.hasta);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lote);
        if (Definido(objeto)) {
            inicio.value = ObtenerPropiedad(objeto, ltrPropiedades.Juridico.lote.vigenteDesde).substring(0, 10);
            hasta.value = ObtenerPropiedad(objeto, ltrPropiedades.Juridico.lote.vigenteHasta).substring(0, 10);
        }
        else {
            inicio.value = '';
            hasta.value = '';
        }
    }


    function plv_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let venta = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.venta) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unidad) as HTMLSelectElement;
        if (NoDefinido(unitario)) {
            venta.value = "";
            clase.selectedIndex = 0;
            naturaleza.selectedIndex = 0;
            unidad.selectedIndex = 0;
            return;
        }
        AsignarValor(venta, ObtenerPropiedad(unitario, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.venta, 0));
        let claseDelUnitario = ObtenerPropiedad(unitario, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.clase, 0);
        MapearAlControl.ListaDeValores((clase as HTMLSelectElement), claseDelUnitario);
        MapearAlControl.FijarEnListaDeElementos(naturaleza, ObtenerPropiedad(unitario, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.idnaturaleza, 0));
        MapearAlControl.FijarEnListaDeElementos(unidad, ObtenerPropiedad(unitario, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.idunidad, 0));
    }

    function plv_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPlanificadorDeVenta;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.venta) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unidad) as HTMLSelectElement;
        let selectorIvaR = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.selectorDeIvaR) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        selectorIvaR.selectedIndex = 0;
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }

    function plv_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
        let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.cantidad).value);
        let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.venta).value);

        let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteSinDto) as HTMLInputElement;
        let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteDeDto) as HTMLInputElement;
        let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteDeIva) as HTMLInputElement;
        let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteDeLinea) as HTMLInputElement;

        let importeSinDescuento: number = cantidad * precio;
        if (importeSinDescuento > 0) {
            let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.descuentoPorLinea).value);
            AsignarValor(impSinDto, importeSinDescuento.toString());
            AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
            let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

            let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.iva).value);
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

    export function NavegarARelacionesDelPlfdor(peticion: ApiDeAjax.DescriptorAjax, idRestrictor: string): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Juridico.Planificador.IrAPlfDeVentas:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PlfDeVenta}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }
}

