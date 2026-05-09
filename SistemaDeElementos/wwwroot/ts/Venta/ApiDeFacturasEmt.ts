namespace Venta {

    export function Fae_InicializarModalParaCrearLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as Venta.CrudEdicionFacturaEmt);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.TablaDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Venta.FacturaEmt.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.FacturaEmt.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + 10).toString();

        let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.FacturaEmt.linea.tipoDeLinea, true) as HTMLSelectElement;
        tipoDeLinea.selectedIndex = 0
        let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.FacturaEmt.linea.unitario, true) as HTMLInputElement;
        unitario.focus();
        fae_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Fae_InicializarModalParaCrearCobros() {
        var editor = (Crud.crudMnt.crudDeEdicion as Venta.CrudEdicionFacturaEmt);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeCobros;
        let tabla: HTMLDivElement = editor.TablaDeCobros;
        let pagado: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        for (let i = 1; i < tablarows.length; i++) {
            let pagadoHtml = tablarows[i].querySelector(`input[propiedad='${ltrPropiedades.Venta.FacturaEmt.Cobro.Cobrado}']`) as HTMLInputElement;
            pagado = pagado + Importe(pagadoHtml.value, false);
        }

        let totalPendiente = Numero(ObtenerPropiedad(editor.Registro, ltrPropiedades.Venta.FacturaEmt.Cobro.Pendiente));

        let pendienteHtml = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.Pendiente) as HTMLInputElement;
        AsignarValor(pendienteHtml, totalPendiente.toFixed(2).toString());


        let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.Cobrado);
        etiqueta.innerText = ltrEtiquetas.Venta.Cobros.Cobrar;
        let cobradoHtml = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.Cobrado) as HTMLInputElement;
        cobradoHtml.value = pendienteHtml.value;

        let claseHtml: HTMLSelectElement = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.Clase) as HTMLSelectElement;
        MapearAlControl.EliminarOpcion(claseHtml, ltrValores.Venta.FacturasEmt.Cobro.Clase.CartaDePago);
        MapearAlControl.EliminarOpcion(claseHtml, ltrValores.Venta.FacturasEmt.Cobro.Clase.Remesa);
        ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.CuentaDeIngreso);

        let fechaHtml: HTMLInputElement = ApiControl.BuscarSelectorDeFechaHora(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.CobradoEl) as HTMLInputElement;
        MapearAlControl.FechaDate(fechaHtml, new Date());
    }

    export function Fae_InicializarModalParaCrearAbonos() {
        var editor = (Crud.crudMnt.crudDeEdicion as Venta.CrudEdicionFacturaEmt);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeAbonos;
        let tabla: HTMLDivElement = editor.TablaDeAbonos;
        let pagado: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        for (let i = 1; i < tablarows.length; i++) {
            let pagadoHtml = tablarows[i].querySelector(`input[propiedad='${ltrPropiedades.Venta.FacturaEmt.Abono.Importe}']`) as HTMLInputElement;
            pagado = pagado + Importe(pagadoHtml.value, false);
        }

        let porAbonar = Numero(ObtenerPropiedad(editor.Registro, ltrPropiedades.Venta.FacturaEmt.PorAbonar));

        let pendienteHtml = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.Abono.Pendiente) as HTMLInputElement;
        AsignarValor(pendienteHtml, porAbonar.toFixed(2).toString());


        let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Venta.FacturaEmt.Abono.Importe);
        etiqueta.innerText = ltrEtiquetas.Venta.Abonos.Abonar;
        let abonadoHtml = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.Abono.Importe) as HTMLInputElement;
        abonadoHtml.value = pendienteHtml.value;

        let claseHtml: HTMLSelectElement = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.Abono.Clase) as HTMLSelectElement;
        MapearAlControl.EliminarOpcion(claseHtml, ltrValores.Venta.FacturasEmt.Abono.Clase.Remesa);
        ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.Abono.CuentaDeIngreso);
        ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.Abono.CuentaDeAbono);

        let fechaHtml: HTMLInputElement = ApiControl.BuscarSelectorDeFechaHora(modal, ltrPropiedades.Venta.FacturaEmt.Abono.AbonadoEl) as HTMLInputElement;
        MapearAlControl.FechaDate(fechaHtml, new Date());

        let cliente: HTMLInputElement = ApiControl.BuscarRestrictor(modal, ltrPropiedades.Venta.FacturaEmt.Abono.IdCliente, ltrTipoControl.restrictorDeEdicion) as HTMLInputElement;
        MapearAlControl.Restrictor(cliente, editor.Cliente.id, editor.Cliente.nombre);
    }

    export function Fae_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        editor.AplicarTipoDeLinea();
    }

    export function Fae_MapearFechaDeVencimiento() {

    }

    export function Fae_Tras_Blanquear_Unitario() {
        Fae_Tras_Cambiar_TipoDeLinea();
    }

    export function Fae_Tras_Cambiar_Clase_De_Abono() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeAbonos;
        let clase = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.Abono.Clase) as HTMLSelectElement;
        ApiControl.BloquearListaDeElementoSi(modal, ltrPropiedades.Venta.FacturaEmt.Abono.CuentaDeIngreso, clase.value !== ltrValores.Venta.FacturasEmt.Abono.Clase.Transferencia, true);
        ApiControl.BloquearListaDeElementoSi(modal, ltrPropiedades.Venta.FacturaEmt.Abono.CuentaDeAbono, clase.value !== ltrValores.Venta.FacturasEmt.Abono.Clase.Transferencia, true);
    }

    export function Fae_Tras_Cambiar_Clase_De_Cobro() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeCobros;
        let clase = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.Clase) as HTMLSelectElement;
        let cuentaDeIngreso = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Venta.FacturaEmt.Cobro.CuentaDeIngreso) as HTMLSelectElement;
        ApiControl.BloquearLaLista(cuentaDeIngreso, clase.value !== ltrValores.Venta.FacturasEmt.Cobro.Clase.Transferencia);
    }

    export function Fae_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        fae_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Fae_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            fae_mapearUnitarioSeleccionado_interno(objeto);
        }
        else {
            var tieneValorAsignado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0 && !IsNullOrEmpty(lista.value);
            if (!tieneValorAsignado)
                fae_InicializarModalDeLineas_interno();
        }
    }

    export function Fae_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        editor.fae_CalcularImportesDeLinea_interno(modal);
    }

    export function Fae_Tras_Mapear_Filtro_IdContrato(control: HTMLElement) {
        let idContrato = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idContrato == 0)
            return;
        ApiDePeticiones.LeerElementoPorId(control, ltrControladores.Juridico.Contratos, idContrato, new Array<Parametro>(), idContrato)
            .then((peticion) => mapearDatosContrato(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function Fae_IvaRepercutidoCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt;
        let selectorIvaR = editor.SelectorDeIvaActivo;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaR);
        let iva = editor.PorcentageIvaActivo;
        if (Definido(objeto))
            AsignarValor(iva, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje));
        else
            ApiControl.BlanquearEditor(iva);
    }

    export function CopiarUnaFae(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.FacturaEmt.CopiarFae, parametros, datosDeEntrada);
    }

    export function HacerRectificativa(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.FacturaEmt.HacerRectificativa, parametros, datosDeEntrada);
    }

    export function CambiarFechaDeVencimiento(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.FacturaEmt.CambiarVencimiento, parametros, datosDeEntrada);
    }

    export function CambiarDatos(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.FacturaEmt.CambiarDatos, parametros, datosDeEntrada);
    }
    export function FacturarTareas(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.FacturaEmt.FacturarTareas, parametros, datosDeEntrada);
    }

    export function Fae_ProponerDatosDeLaFaeSeleccionada() {
        let modal = (Crud.crudMnt as CrudDeFacturasEmt).ModalCopiarFae;
        let fae: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
        let idFae: number = Numero(fae.getAttribute(atListasDinamicas.idSeleccionado));
        if (idFae > 0) {
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Venta.FacturasEmt, idFae, new Array<Parametro>(), idFae)
                .then((peticion) => (Crud.crudMnt as CrudDeFacturasEmt).Fae_ProponerFacturaParaCopiar(modal, peticion.resultado.datos))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
    }

    export function Fae_InicializarModalDeCopiado() {
        let modal = (Crud.crudMnt as CrudDeFacturasEmt).ModalCopiarFae;
        ApiPanel.BlanquearControlesDeIU(modal);
    }

    export function Fae_CalcularIrpf() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        let panel: HTMLDivElement = editor.PanelDeIrpf;

        let porcentaje = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.Irpf.Irpf) as HTMLInputElement;
        let aplicar: number = Numero(porcentaje.value);

        let bi = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.Irpf.BiSujeta) as HTMLInputElement;
        let biSujeta: number = Numero(bi.value);

        let importe = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.Irpf.Importe) as HTMLInputElement;
        AsignarValor(importe, (biSujeta * aplicar / 100).toString())
    }

    export function Fae_Tras_Cambiar_Tipo_Irpf() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        let panel: HTMLDivElement = editor.PanelDeIrpf;
        let tipoIrpf = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.FacturaEmt.Irpf.TipoIrpf) as HTMLSelectElement;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(tipoIrpf);


        let porcentaje = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.Irpf.Irpf) as HTMLInputElement;

        AsignarValor(porcentaje, Definido(objeto) ? ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.Irpf.Porcentaje) : '');
    }

    export function Fae_Al_Entrar_BI_Sujeta() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt);
        let panel: HTMLDivElement = editor.PanelDeIrpf;
        if (!ApiPanel.EsVisible(panel))
            return;
        let biSujeta = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.Irpf.BiSujeta) as HTMLInputElement;
        if (Numero(biSujeta.value) > 0)
            return;

        AsignarValor(biSujeta, ObtenerPropiedad(editor.Registro, ltrPropiedades.Venta.FacturaEmt.TotalSinIva));
    }

    export function Fae_Antes_De_Buscar_Ppt(filtros: Array<ClausulaDeFiltrado>, parametros: Array<Parametro>) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt
        filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, literal.filtro.criterio.igual, editor.IdSociedadDelCg));
    }

    export function Fae_Antes_De_Buscar_Ctt(filtros: Array<ClausulaDeFiltrado>, parametros: Array<Parametro>) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt
        filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, literal.filtro.criterio.igual, editor.IdSociedadDelCg));
    }


    export function Fae_IrALaFacturaDelTercero(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Venta.PeticionDeFacturaEmt.IdFactura));
        ApiDelCrud.IrALaFactura(idRegistro);
    }

    function mapearDatosContrato(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCg = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.IdCg));
        let cg = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.Cg);
        let idFiltro = Crud.crudMnt.ModalDeFiltrado(ltrModalDeFiltrado.Venta.FacturaEmt.FiltroDeFacturasEmt);
        let panel = document.getElementById(idFiltro) as HTMLDivElement;
        MapearAlPanel.RestrictoresPorPropiedad(Crud.crudMnt.PanelFiltro, ltrPropiedades.Elemento.ConCg.IdCg, idCg, cg);
        ApiControl.BloquearListaDeValores(panel, ltrPropiedades.Venta.FacturaEmt.ConOSinContrato);

        ApiDelCrud.MapearDatosSocietariosYDepartamentales(Crud.crudMnt.crudDeCreacion.PanelDeCrear, peticion.resultado.datos);

        let idTipo = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConTipo.IdTipo));
        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Elemento.ConTipo.IdTipo, idTipo));
        let idElemento = Numero(ObtenerPropiedad(peticion.resultado.datos, literal.id));
        ApiDePeticiones.LeerAmpliacion(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrControladores.Juridico.DatosDelContrato, ltrNegocioSe.Enumerado.Juridico.Contrato, idElemento, parametros, peticion.resultado.datos)
            .then((peticion) => mapearDatosCliente(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function mapearDatosCliente(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCliente = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.FacturaEmt.IdCliente));
        let cliente = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.FacturaEmt.Cliente);
        MapearAlControl.Propiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.FacturaEmt.Cliente, idCliente, cliente, true, false);
    }

    function fae_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.FacturaEmt.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.FacturaEmt.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.FacturaEmt.linea.unidad) as HTMLSelectElement;
        if (NoDefinido(unitario)) {
            precio.value = "";
            clase.selectedIndex = 0;
            naturaleza.selectedIndex = 0;
            unidad.selectedIndex = 0;
            return;
        }
        AsignarValor(precio, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.venta, 0));
        let claseDelUnitario = ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.clase, 0);
        MapearAlControl.ListaDeValores((clase as HTMLSelectElement), claseDelUnitario);
        MapearAlControl.FijarEnListaDeElementos(naturaleza, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idnaturaleza, 0));
        MapearAlControl.FijarEnListaDeElementos(unidad, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idunidad, 0));
    }

    function fae_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.FacturaEmt.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.FacturaEmt.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.FacturaEmt.linea.unidad) as HTMLSelectElement;
        let selectorIvaR = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.FacturaEmt.linea.selectorDeIvaR) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.FacturaEmt.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Venta.FacturaEmt.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        selectorIvaR.selectedIndex = 0;
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }
}

