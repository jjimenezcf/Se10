namespace Logistica {

    export enum enumEtapasDePedido {
    }

    function ParsearEtapa(etapa: string): enumEtapasDePedido {
        MensajesSe.EmitirExcepcion("Parsear etapa de circuito documental", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaElCircuitoEnEtapa(etapas: string, etapa: enumEtapasDePedido): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function Ped_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        ped_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Ped_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        editor.pedido_CalcularImportesDeLinea_interno(modal);
    }

    export function Ped_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            ped_mapearUnitarioSeleccionado_interno(objeto);
        }
        else {
            var tieneValorAsignado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0 && !IsNullOrEmpty(lista.value);
            if (!tieneValorAsignado)
                ped_InicializarModalDeLineas_interno();
        }
    }

    export function Ped_Tras_Blanquear_Unitario() {
        Ped_Tras_Cambiar_TipoDeLinea();
    }

    export function Ped_Tras_Seleccionar_Proveedor(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        
        if (Crud.crudMnt.EstoyEditandoConsultando || Crud.crudMnt.ModoTrabajo === enumModoTrabajo.creando) {
            var idSeleccionado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
            ApiControl.BloquearListaDinamicaSi(
                Crud.crudMnt.EstoyEditandoConsultando ? Crud.crudMnt.crudDeEdicion.PanelDelDto : Crud.crudMnt.crudDeCreacion.PanelDeCrear,
                ltrPropiedades.Logistica.Pedido.Contrato,
                idSeleccionado === 0);
            return;
        }

        MensajesSe.EmitirExcepcion('Ped_Tras_Seleccionar_Proveedor', 'No se ha definido donde afectar la selección del proveedor');
    }

    export function Ped_Tras_Blanquear_Proveedor(idLista: string): void {
        if (Crud.crudMnt.EstoyEditandoConsultando) {
            ApiControl.BloquearListaDinamicaPorPropiedad(Crud.crudMnt.crudDeEdicion.PanelDelDto, ltrPropiedades.Logistica.Pedido.Contrato);
        }
        else if (Crud.crudMnt.ModoTrabajo === enumModoTrabajo.creando) {
            ApiControl.BloquearListaDinamicaPorPropiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Logistica.Pedido.Contrato);
        }
        else
            MensajesSe.EmitirExcepcion('Ped_Tras_Blanquear_Proveedor', 'No se ha definido donde afectar la selección del proveedor');
    }

    export function Ped_InicializarModalParaCrearLineas(incremento: number) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.GridDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Logistica.Pedido.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Pedido.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + incremento).toString();

        if (Definido((Crud.crudMnt as CrudDePedidos).TipoDeLinea)) {
            var SelectorDeTipo = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.tipoDeLinea);
            MapearAlControl.ListaDeValores(SelectorDeTipo, (Crud.crudMnt as CrudDePedidos).TipoDeLinea);
        }
        else {
            let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Pedido.linea.unitario, true) as HTMLInputElement;
            unitario.focus();
            ped_InicializarModalDeLineas_interno();
            editor.AplicarTipoDeLinea();
        }
    }

    export function Ped_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        editor.AplicarTipoDeLinea();
    }

    function ped_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Logistica.Pedido.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.unidad) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Logistica.Pedido.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Logistica.Pedido.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }

    function ped_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        if (NoDefinido(unitario)) {
            BlanquearUnitario(panel);
            return;
        }
        var ctrlProveedor = ApiControl.BuscarListaDinamicaPorPropiedad(Crud.crudMnt.PanelDto, ltrPropiedades.Logistica.Pedido.Proveedor)
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.idElemento1, Numero(ObtenerPropiedad(unitario, literal.id, 0))));
        parametros.push(new Parametro(Ajax.Param.idElemento2, Numero(ctrlProveedor.getAttribute(atListasDinamicas.idSeleccionado))));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, ltrControladores.MaestrosTecnico.Tarifas, Ajax.EndPoint.MaestrosTecnicos.Tarifa.LeerTarifa, parametros, new Array<Parametro>())
            .then(
                (peticion: ApiDeAjax.DescriptorAjax) => {
                    MapearTarifa(peticion, unitario);
                })
            .catch((peticion: ApiDeAjax.DescriptorAjax) => {
                BlanquearUnitario(panel);
                ApiDePeticiones.EmitirError(peticion)
            });
    }

    function MapearTarifa(peticion: ApiDeAjax.DescriptorAjax, unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Logistica.Pedido.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.unidad) as HTMLSelectElement;

        AsignarValor(precio, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Maestros.Tarifa.Tarifa, 0));
        let claseDelUnitario = ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.clase, 0);
        MapearAlControl.ListaDeValores((clase as HTMLSelectElement), claseDelUnitario);
        MapearAlControl.FijarEnListaDeElementos(naturaleza, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idnaturaleza, 0));
        MapearAlControl.FijarEnListaDeElementos(unidad, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idunidad, 0));
    }

    function BlanquearUnitario(panel: HTMLDivElement) {
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Logistica.Pedido.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.unidad) as HTMLSelectElement;
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }

}
