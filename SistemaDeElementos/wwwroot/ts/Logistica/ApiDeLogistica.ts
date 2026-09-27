namespace Logistica {

    export function Ral_Tras_Seleccionar_Almacen(): void {
        if (Crud.ModoTrabajo() !== enumModoTrabajo.creando) return;

        const panel = Crud.crudMnt.crudDeCreacion.PanelDeCrear;
        const almacenLista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Logistica.Regularizacion.Almacen) as HTMLInputElement;
        const almacen = OpcionesDeLasListas.ObtenerObjeto(almacenLista);
        if (Definido(almacen)) {
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(panel, almacen);
        }
    }

    export function Ral_Tras_Blanquear_Almacen(): void {
        if (Crud.ModoTrabajo() !== enumModoTrabajo.creando)
            return;
        ApiDelCrud.BlanquearCgOculto(Crud.crudMnt.crudDeCreacion.PanelDeCrear);
    }

    export function Ral_InicializarModalParaCrearLineas(incremento: number) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionRegularizacion);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.GridDeLineas;
        let valor: number = 0;
        if (Definido(tabla)) {
            let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
            if (tablarows.length > 1) {
                let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Logistica.Regularizacion.linea.orden}]`) as HTMLInputElement;
                valor = Numero(ultimoOrden.value);
            }
        }
        // El grid de detalle se vacía de forma síncrona al recargarse tras crear una línea (y seguir creando),
        // así que puede no reflejar todavía la última línea creada; nos quedamos con el mayor de los dos.
        valor = Math.max(valor, editor.UltimoOrdenPropuesto);

        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Regularizacion.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + incremento).toString();
        editor.UltimoOrdenPropuesto = valor + incremento;

        let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Regularizacion.linea.unitario, true) as HTMLInputElement;
        unitario.focus();

        ApiControl.BuscarEtiqueta(modal, 'idelemento').innerText = 'Almacén';
        ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Logistica.Regularizacion.linea.unitario).innerText = 'Material';

        const clase = ObtenerPropiedad(editor.Tipo, ltrPropiedades.Logistica.Regularizacion.tipo.Clase);
        if (clase === ltrValores.Logistica.Regularizacion.Clase.Inicial) {
            const cantidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.cantidad);
            cantidad.placeholder = 'Cantidad inicial';
        }
    }

    export function Ral_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionRegularizacion);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let cantidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.cantidad);
        let precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.precio);
        let total = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.total);
        AsignarValor(total, Numero(cantidad.value) * Numero(precio.value));
    }

    export function Ral_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        ral_mapearUnitarioSeleccionado_interno(objeto);
    }

    export function Ral_Tras_Blanquear_Unitario() {
        ral_mapearUnitarioSeleccionado_interno(undefined);
    }

    function ral_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionRegularizacion);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.precio);
        if (NoDefinido(unitario)) {
            precio.value = "";
        }
        else {
            AsignarValor(precio, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.coste, 0));
        }
        Ral_CalcularImportesDeLinea();
    }

    export function Ral_FiltrosPorClaseDeUnitario(): Array<ClausulaDeFiltrado> {
        let clausulas: Array<ClausulaDeFiltrado> = new Array<ClausulaDeFiltrado>();
        clausulas.push(new ClausulaDeFiltrado(ltrPropiedades.Maestros.unitario.FiltrosPorClaseDeUnitario, atCriterio.igual, ltrValores.Maestros.Unitario.Clase.Material));
        return clausulas;
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

    export function Ped_Tras_Blanquear_Unitario() {
        ped_mapearUnitarioSeleccionado_interno(undefined);
    }

    export function Ped_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        ped_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea();
    }

    export function Ped_Tras_Cambiar_Naturaleza_Del_Detalle() {
        let panel = Crud.crudMnt.crudDeCreacion.PanelDeCrear;
        ped_FijarClaseSegunNaturaleza_interno(panel, ltrPropiedades.Logistica.Pedido.SelectorNaturaleza, ltrPropiedades.Logistica.Pedido.ClaseDeLinea);
    }

    function ped_FijarClaseSegunNaturaleza_interno(panel: HTMLDivElement, propiedadNaturaleza: string, propiedadClase: string) {
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, propiedadNaturaleza) as HTMLSelectElement;
        let clase = ApiControl.BuscarListaDeValores(panel, propiedadClase) as HTMLSelectElement;

        if (naturaleza.selectedIndex <= 0) {
            clase.selectedIndex = 0;
            return;
        }

        var objeto = OpcionesDeLasListas.ObtenerObjeto(naturaleza);
        if (Definido(objeto)) {
            MapearAlControl.ListaDeValores(clase, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Naturaleza.Clase, 0));
        }
    }

    export function Ped_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        ped_mapearUnitarioSeleccionado_interno(objeto);
    }

    export function Ped_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        editor.pedido_CalcularImportesDeLinea_interno(modal);
    }

    export function Ped_Parametros_Para_Seleccionar_Unitarios(): Array<Parametro> {
        let parametros: Array<Parametro> = new Array<Parametro>();
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        let idProveedor = Numero(ObtenerPropiedad(editor.Registro, ltrPropiedades.Logistica.Pedido.IdProveedor, 0));
        if (idProveedor > 0) {
            parametros.push(new Parametro(ltrPropiedades.Maestros.Tarifa.IdProveedor, idProveedor));
            parametros.push(new Parametro(ltrPropiedades.Maestros.unitario.ObtenerTarifaProveedor, true));
        }
        return parametros;
    }

    function ped_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
        let concepto = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.concepto) as HTMLInputElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.unidad) as HTMLSelectElement;

        if (NoDefinido(unitario)) {
            precio.value = "";
            concepto.value = "";
            naturaleza.selectedIndex = 0;
            unidad.selectedIndex = 0;
            return;
        }

        AsignarValor(precio, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.venta, 0));

        let nombre = ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.nombre, '');
        let referenciaDeTarifa = ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.referenciaDeTarifa, '');
        AsignarValor(concepto, IsNullOrEmpty(referenciaDeTarifa) ? nombre : `(${referenciaDeTarifa}) ${nombre}`);

        // La clase ya no se muestra en la modal: se deriva de la naturaleza en el servidor
        MapearAlControl.FijarEnListaDeElementos(naturaleza, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idnaturaleza, 0));
        MapearAlControl.FijarEnListaDeElementos(unidad, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idunidad, 0));
    }

    function ped_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Logistica.Pedido.linea.unidad) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Logistica.Pedido.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Logistica.Pedido.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        precio.value = "";
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }
}
