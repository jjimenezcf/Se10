namespace Gasto {

    export enum enumEtapasDeRemesaPag {
        REM_Etapa_De_Cumplimentacion,
        REM_Etapa_Generada,
        REM_Etapa_De_Presentacion,
        REM_Etapa_De_Cierre,
        REM_Etapa_Cancelada
    }

    export enum enumEtapasDePago {
        PAG_Etapa_Pendiente,
        PAG_Etapa_Pagado,
        PAG_Etapa_Remesado,
        PAG_Etapa_Cancelado,
        PAG_Etapa_Devuelto,
    }

    export enum enumEtapasDeFacturaRec {
        FAR_Etapa_De_Cumplimentacion,
        FAR_Etapa_De_Aprobacion,
        FAR_Etapa_De_Contabilizacion,
        FAR_Etapa_De_Pago,
        FAR_Etapa_Pagada,
        FAR_Etapa_Devuelta,
        FAR_Etapa_Anulada
    }

    function ParsearEtapa(etapa: string): enumEtapasDeRemesaPag {
        MensajesSe.EmitirExcepcion("Parsear etapa de remesas", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaElCircuitoEnEtapa(etapas: string, etapa: enumEtapasDeRemesaPag): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function Pag_AlPegar_Iban(event) {
        var creador = (Crud.crudMnt.crudDeCreacion as CrudCreacionPago);
        let cuentaBancaria: string = ValidarIbanDelPortaPapeles(event);
        MapearCuentaBancaria(creador.PanelDeCrear, cuentaBancaria);
    };

    export function Pag_Tras_Cambiar_Clase_De_Pago() {
        var creador = (Crud.crudMnt.crudDeCreacion as CrudCreacionPago);
        if (creador.MapeandoPlantilla) {
            return;
        }
        creador.Tras_Cambiar_Clase_De_Pago();
    }

    export function Pag_Tras_Cambiar_Modo_De_Pago() {
        var creador = (Crud.crudMnt.crudDeCreacion as CrudCreacionPago);
        if (creador.MapeandoPlantilla) {
            return;
        }
        creador.Tras_Cambiar_Modo_De_Pago();
    }


    export function Pag_Tras_Seleccionar_Acreedor(idLista: string) {
        (Crud.crudMnt.crudDeCreacion as CrudCreacionPago).Tras_Seleccionar_Acreedor();
    }

    export function Pag_Tras_Blanquear_Acreedor(idLista: string) {
        (Crud.crudMnt.crudDeCreacion as CrudCreacionPago).Tras_Blanquear_Acreedor();
    }

    export function Pag_Tras_Seleccionar_Cuenta_Pago() {
        if (Crud.crudMnt.EstoyCreando)
            tras_Seleccionar_CuentaDePago(Crud.crudMnt.crudDeCreacion.PanelDeCrear);
        else
            tras_Seleccionar_CuentaDePago(Crud.crudMnt.crudDeEdicion.PanelDeEditar);
    }


    function tras_Seleccionar_CuentaDePago(modal: HTMLDivElement) {
        let cuenta = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.Pago.CuentaDePago) as HTMLSelectElement;
        let banco = ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.Pago.BancoDePago);
        if (!Definido(banco))
            return;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(cuenta);
        if (Definido(objeto)) {
            banco.value = ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Banco);
        }
        else {
            banco.value = '';
        }
    }

    export function Pag_Tras_Seleccionar_Tarjeta_Pago() {
        if (Crud.crudMnt.EstoyCreando)
            tras_Seleccionar_Tarjeta_Pago(Crud.crudMnt.crudDeCreacion.PanelDeCrear);
        else
            tras_Seleccionar_Tarjeta_Pago(Crud.crudMnt.crudDeEdicion.PanelDeEditar);
    }

    function tras_Seleccionar_Tarjeta_Pago(modal: HTMLDivElement) {
        let tarjeta = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.Pago.TarjetaDePago) as HTMLSelectElement;
        let cuentadepago = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.Pago.CuentaDePago);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(tarjeta);
        if (Definido(objeto)) {
            MapearAlControl.ListaDeElementos(cuentadepago, new Array<ClausulaDeFiltrado>(), ObtenerPropiedad(objeto, ltrPropiedades.Terceros.Sociedad.Tarjeta.IdCuentaDeCargo));
        }
    }

    export function Rem_PagarRemesa(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Gasto.RemesasPag.PagarRemesa, parametros, datosDeEntrada);
    }

    export function Rem_RetrocederRemesa(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Gasto.RemesasPag.RetrocederRemesa, parametros, datosDeEntrada);
    }

    export function Far_InicializarModalParaCrearLineas() {

        var editor = (Crud.crudMnt.crudDeEdicion as Gasto.CrudEdicionFacturaRec);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.TablaDeLineas;
        let valor: number = 0;
        let filas = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (filas.length > 1) {
            let ultimoOrden = filas[filas.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Gasto.FacturaRec.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }

        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Gasto.FacturaRec.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + 10).toString();

        let claseDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Gasto.FacturaRec.linea.Clase, true) as HTMLSelectElement;
        editor.AplicarClaseParaNuevaLinea(claseDeLinea.selectedIndex);
        far_InicializarConcepto();
    }

    export function Far_Al_Entrar_En_Total_A_Pagar() {
        if (!Crud.crudMnt.EstoyCreando)
            return;

        const creador = (Crud.crudMnt.crudDeCreacion as CrudCreacionFacturaRec);
        if (creador.MapeandoPlantilla) {
            return;
        }
        var bi = Numero(ApiControl.Valor(creador.BiPropuesta));

        if (Definido(bi) && bi !== 0) {
          AsignarValor(creador.TotalDelPago, creador.CalcularTotalAPagar(bi));
        }

    }

    export function Far_Tras_Cambiar_Modo_De_Pago() {
        const creador = (Crud.crudMnt.crudDeCreacion as CrudCreacionFacturaRec);
        if (creador.MapeandoPlantilla) {
            return;
        }

        const pagada = ApiControl.BuscarCheck(creador.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Pagada);
        creador.AplicarModoDePagoContado(pagada);
        if (!pagada.checked) {
            return;
        }
        const modoDePago = ApiControl.BuscarListaDeValores(creador.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.ModoDePago);
        if (modoDePago.value === enumModoDePagoContado.Domiciliacion)
            creador.AplicarModoPagoDomiciliacion();
        else if (modoDePago.value === enumModoDePagoContado.Tarjeta)
            creador.AplicarModoPagoTarjeta();
    }

    export function Far_AlCambiar_Pagada(check: HTMLInputElement) {
        var creador = (Crud.crudMnt.crudDeCreacion as CrudCreacionFacturaRec);
        var cg = ApiControl.BuscarListaDinamicaPorPropiedad(creador.PanelDeCrear, ltrPropiedades.Elemento.ConCg.Cg);
        if (check.checked && Numero(cg.getAttribute(atListasDinamicas.idSeleccionado)) === 0) {
            MensajesSe.Info("Debe indicar el cg antes de indicar que la factura está pagada");
            check.checked = false;
        }

        if (!creador.MapeandoPlantilla)
            creador.AplicarCheckDePago(check);
    }

    export function Far_Tras_Cambiar_ClaseDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaRec);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let clase = ApiControl.BuscarControl(modal, ltrPropiedades.Gasto.FacturaRec.linea.Clase, true) as HTMLSelectElement;
        if (ApiPanel.ModalAbierta(modal)) {
            editor.AplicarClaseParaNuevaLinea(clase.selectedIndex);
            far_InicializarConcepto();
        }
    }

    export function Far_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaRec);
        let modal: HTMLDivElement = editor.ModalDeEdicionDeLineas;
        ApiPanel.PonerEnModoConsulta(modal, false);
        //let clase = ApiControl.BuscarControl(modal, ltrPropiedades.Gasto.FacturaRec.linea.Clase, true) as HTMLSelectElement;
        //editor.AplicarClaseDeLinea(clase.selectedIndex);        
    }

    export function Far_CalcularImpuesto() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaRec;
        editor.CalcularImportesDeLinea()
    }

    export function Far_IvaSoportadoCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaRec;
        let selectorIvaS = editor.SelectorDeIvaActivo;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaS);
        let iva = editor.PorcentageIvaActivo;
        if (Definido(objeto))
            AsignarValor(iva, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaS.Porcentaje));
        else
            ApiControl.BlanquearEditor(iva);
        editor.CalcularImportesDeLinea();
    }

    export function Far_IrpfCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaRec;
        let selectorIrpf = editor.SelectorDeIrpfActivo;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIrpf);
        let irpf = editor.PorcentageIrpfActivo;
        if (Definido(objeto))
            AsignarValor(irpf, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.Irpf.Porcentaje));
        else
            ApiControl.BlanquearEditor(irpf);
        editor.CalcularImportesDeLinea();
    }

    export function Far_Generar(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Gasto.FacturaRec.Generar, parametros, datosDeEntrada);
    }
    export function Far_Rectificar(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Gasto.FacturaRec.Rectificar, parametros, datosDeEntrada);
    }
    export function Far_Tras_Indicar_Fecha_De_Emision() {
        if (!Crud.crudMnt.EstoyCreando)
            return;

        let recibidaEl = ApiControl.BuscarSelectorDeFecha(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.RecibidaEl) as HTMLInputElement;
        let emitidaEl = ApiControl.BuscarSelectorDeFecha(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.FacturadaEl) as HTMLInputElement;

        const fechaEmitida = new Date(emitidaEl.value);
        const hoy = new Date();

        if (fechaEmitida instanceof Date && !isNaN(fechaEmitida.getTime()) && recibidaEl.value === hoy.toISOString().split('T')[0]) {

            if ((Crud.crudMnt as CrudDeFacturasRec).ComoTratarLaFechaDeRecepcion === ltrValores.Gasto.Factura.ComoTratarLaFechaDeRecepcion.FechaDeHoy)
                return;

            const quinceDiasAtras = new Date(hoy.getTime() - 15 * 24 * 60 * 60 * 1000);
            if ((Crud.crudMnt as CrudDeFacturasRec).ComoTratarLaFechaDeRecepcion === ltrValores.Gasto.Factura.ComoTratarLaFechaDeRecepcion.FechaDeHoySi15 && fechaEmitida < quinceDiasAtras) {
                const fechaPropuesta = new Date(fechaEmitida.getTime() + 24 * 60 * 60 * 1000);
                recibidaEl.value = fechaPropuesta.toISOString().split('T')[0];
                return;
            }

            const treintaDiasAtras = new Date(hoy.getTime() - 30 * 24 * 60 * 60 * 1000);
            if ((Crud.crudMnt as CrudDeFacturasRec).ComoTratarLaFechaDeRecepcion === ltrValores.Gasto.Factura.ComoTratarLaFechaDeRecepcion.FechaDeHoySi30 && fechaEmitida < treintaDiasAtras) {
                const fechaPropuesta = new Date(fechaEmitida.getTime() + 24 * 60 * 60 * 1000);
                recibidaEl.value = fechaPropuesta.toISOString().split('T')[0];
                return;
            }

            if ((Crud.crudMnt as CrudDeFacturasRec).ComoTratarLaFechaDeRecepcion === ltrValores.Gasto.Factura.ComoTratarLaFechaDeRecepcion.MismaFecha) {
                const fechaPropuesta = new Date(fechaEmitida.getTime());
                recibidaEl.value = fechaPropuesta.toISOString().split('T')[0];
                return;
            }
        }
    }

    export function far_InicializarConcepto() {
        const editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaRec);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let concepto = ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
        if (IsNullOrEmpty(concepto.value)) {
            let asunto = ApiControl.BuscarEditor(editor.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Nombre) as HTMLInputElement;
            concepto.value = asunto.value;
        }
    }


}


