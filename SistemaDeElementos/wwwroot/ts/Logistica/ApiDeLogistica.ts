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

        // Si el proveedor del pedido tiene tarifas definidas (TarifaDtm), se propone elegir un elemento del maestro
        // de unitarios en vez de partida alzada (ver GestorDePedidos.DespuesDeMapearElElemento)
        let usaTarifaDelProveedor = ObtenerPropiedad(editor.Registro, ltrPropiedades.Logistica.Pedido.UsaTarifaDelProveedor, false);
        // si las entregas van al almacén sólo se piden materiales del catálogo
        let tipoDeLineaPropuesto = usaTarifaDelProveedor || editor.ElDestinoEsUnAlmacen ? enumTipoDeLinea.Unitario : (Crud.crudMnt as CrudDePedidos).TipoDeLinea;
        ped_AplicarDestinoAlUnitario_interno(modal);

        if (Definido(tipoDeLineaPropuesto)) {
            var SelectorDeTipo = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.tipoDeLinea);
            MapearAlControl.ListaDeValores(SelectorDeTipo, tipoDeLineaPropuesto);
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
        ped_AplicarDestinoAlUnitario_interno(editor.ModalDeEdicionDeLineas);
        editor.AplicarTipoDeLinea();
    }

    // Con destino almacén sólo se seleccionan materiales: el unitario se etiqueta como material y se filtra por esa clase
    function ped_AplicarDestinoAlUnitario_interno(modal: HTMLDivElement) {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        let etiqueta = ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Logistica.Pedido.linea.unitario);
        if (Definido(etiqueta))
            etiqueta.innerText = editor.ElDestinoEsUnAlmacen ? ltrEtiquetas.Logistica.Pedido.Material : ltrEtiquetas.Logistica.Pedido.Unitario;
    }

    export function Ped_FiltrosPorClaseDeUnitario(): Array<ClausulaDeFiltrado> {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        return editor.ElDestinoEsUnAlmacen ? Ral_FiltrosPorClaseDeUnitario() : new Array<ClausulaDeFiltrado>();
    }

    export function Ped_Tras_Blanquear_Unitario() {
        ped_mapearUnitarioSeleccionado_interno(undefined);
    }

    export function Ped_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPedido);
        // con destino almacén no se piden partidas alzadas: se avisa y se vuelve a unitario (lo que vuelve a llamar a este evento)
        if (editor.ElDestinoEsUnAlmacen) {
            let modal = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
            let tipoDeLinea = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.tipoDeLinea);
            if (tipoDeLinea.value === enumTipoDeLinea.Alzada) {
                MensajesSe.Error('Líneas del pedido', 'Cuando el destino es un almacén no se pueden seleccionar partidas alzadas, seleccione un material');
                MapearAlControl.ListaDeValores(tipoDeLinea, enumTipoDeLinea.Unitario);
                return;
            }
        }
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

    //---------------------------------------------------------------------------------------------------------------------------
    // Grid de líneas: recepcionar (modal) y entrega completa (visto verde). Lo entregado se consulta, cancela y devuelve desde el
    // grid de entregas, y lo devuelto se consulta y cancela desde el de devoluciones
    //---------------------------------------------------------------------------------------------------------------------------

    // Abre la modal para entregar parte o todo lo que queda pendiente de la línea; sus entregas se consultan desde el grid de entregas
    export function Pedido_Recepcionar(parametros: string): void {
        let linea = ped_LeerLineaDelGrid_interno(parametros);
        if (!ped_SePuedeEntregar_interno(linea)) return;
        // modal para pedir datos de RecepcionarLineaDto (ver DescriptorDePedidos), su Aceptar está en CrudDePedidos.ModalDePedirDatos_Aceptar
        (Crud.crudMnt as CrudDePedidos).AbrirModalDeRecepcionarLinea(linea);
    }

    export function Pedido_CalcularImporteEntregado() {
        (Crud.crudMnt as CrudDePedidos).CalcularImporteEntregado();
    }

    export function Pedido_Recepcion_TrasCambiarAlmacen() {
        (Crud.crudMnt as CrudDePedidos).Recepcion_TrasCambiarAlmacen();
    }

    export function Pedido_Recepcion_TrasSeleccionarExpediente() {
        (Crud.crudMnt as CrudDePedidos).Recepcion_TrasSeleccionarExpediente();
    }

    // Entrega completa de la línea (visto verde): sin modal, entrega lo que queda pendiente de la línea al destino de la cabecera
    // (ver CrudDePedidos.EntregarLineaCompleta). El pedido completo se da por entregado desde el menú de edición
    export function Pedido_RecibirLinea(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        let linea = ped_LeerLineaDelGrid_interno(parametros);
        if (!editor.SePuedeEntregarLaLineaCompleta(linea.pendiente)) {
            MensajesSe.Error('Recepción', linea.pendiente <= 0
                ? 'La línea ya se ha entregado por completo'
                : !editor.EsGestorDelPedido
                    ? 'Ha de ser gestor del pedido para entregar sus líneas'
                    : 'Solo se pueden recepcionar líneas de pedidos solicitados o en recepción');
            return;
        }
        (Crud.crudMnt as CrudDePedidos).EntregarLineaCompleta(linea);
    }

    //---------------------------------------------------------------------------------------------------------------------------
    // Grid de entregas: consultar (modal de recepción en consulta), cancelar y devolver
    //---------------------------------------------------------------------------------------------------------------------------

    export function Pedido_ConsultarEntrega(parametros: string): void {
        (Crud.crudMnt as CrudDePedidos).ConsultarEntrega(ped_LeerEntregaDelGrid_interno(parametros));
    }

    // Cancela la entrega; con devoluciones, contabilizada o con el pedido fuera de recepción no se llama al servidor
    export function Pedido_CancelarEntrega(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        let entrega = ped_LeerEntregaDelGrid_interno(parametros);
        if (!editor.SePuedeCancelarLaEntrega(entrega.contabilizada, entrega.cantidadDevuelta)) {
            MensajesSe.Error('Recepción', !editor.EsGestorDelPedido
                ? 'Ha de ser gestor del pedido para cancelar sus entregas'
                : entrega.cantidadDevuelta > 0
                    ? 'La entrega tiene devoluciones, cancélelas antes de cancelar la entrega'
                    : entrega.contabilizada
                        ? 'La entrega está contabilizada (su preasiento está en un lote contable), no se puede cancelar'
                        : 'Solo se puede cancelar una entrega con el pedido en recepción');
            return;
        }
        (Crud.crudMnt as CrudDePedidos).CancelarEntrega(entrega);
    }

    // Abre la modal para devolver parte o todo lo que queda por devolver de la entrega (ver CrudDePedidos.AbrirModalDeDevolverEntrega);
    // el servidor vuelve a validar que no se devuelva más de lo entregado
    export function Pedido_DevolverEntrega(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        let entrega = ped_LeerEntregaDelGrid_interno(parametros);
        const pendiente = entrega.cantidad - entrega.cantidadDevuelta;
        if (!editor.SePuedeDevolverLaEntrega(pendiente)) {
            MensajesSe.Error('Devolución', !editor.EsGestorDelPedido
                ? 'Ha de ser gestor del pedido para devolver sus entregas'
                : pendiente <= 0
                    ? 'La entrega ya se ha devuelto por completo'
                    : 'Solo se pueden devolver entregas de pedidos en recepción o cerrados');
            return;
        }
        (Crud.crudMnt as CrudDePedidos).AbrirModalDeDevolverEntrega(entrega);
    }

    export function Pedido_CalcularImporteDevuelto() {
        (Crud.crudMnt as CrudDePedidos).CalcularImporteDevuelto();
    }

    //---------------------------------------------------------------------------------------------------------------------------
    // Grid de devoluciones: consultar (modal de devolución en consulta) y cancelar
    //---------------------------------------------------------------------------------------------------------------------------

    export function Pedido_ConsultarDevolucion(parametros: string): void {
        (Crud.crudMnt as CrudDePedidos).ConsultarDevolucion(ped_LeerDevolucionDelGrid_interno(parametros));
    }

    export function Pedido_CancelarDevolucion(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPedido;
        let devolucion = ped_LeerDevolucionDelGrid_interno(parametros);
        if (!editor.SePuedeCancelarLaDevolucion(devolucion.contabilizada)) {
            MensajesSe.Error('Devolución', !editor.EsGestorDelPedido
                ? 'Ha de ser gestor del pedido para cancelar sus devoluciones'
                : devolucion.contabilizada
                    ? 'La devolución está contabilizada (su preasiento está en un lote contable), no se puede cancelar'
                    : 'Solo se pueden cancelar devoluciones de pedidos en recepción o cerrados');
            return;
        }
        (Crud.crudMnt as CrudDePedidos).CancelarDevolucion(devolucion);
    }

    // Opción "Imprimir" del pie del grid de devoluciones (ver CrudDePedidos.ImprimirDevoluciones)
    export function Pedido_ImprimirDevoluciones(): void {
        (Crud.crudMnt as CrudDePedidos).ImprimirDevoluciones();
    }

    //---------------------------------------------------------------------------------------------------------------------------
    // Lectura de las filas de los grids de líneas, entregas y devoluciones
    //---------------------------------------------------------------------------------------------------------------------------

    // Lo pedido en la línea y lo que queda por entregar
    export interface LineaDelGridDePedido {
        id: number;
        rowVersion: string;
        cantidad: number;
        precio: number;
        descuento: number;
        pendiente: number;
    }

    // Una entrega (recepción) de una línea, con lo ya devuelto
    export interface EntregaDelGridDePedido {
        id: number;
        rowVersion: string;
        idLinea: number;
        recepcionadaEl: string;
        cantidad: number;
        precio: number;
        descuento: number;
        idAlmacen: number;
        almacen: string;
        idExpediente: number;
        expediente: string;
        cantidadDevuelta: number;
        contabilizada: boolean;
    }

    // Una devolución de una entrega
    export interface DevolucionDelGridDePedido {
        id: number;
        rowVersion: string;
        devueltaEl: string;
        cantidad: number;
        precio: number;
        descuento: number;
        idAlmacen: number;
        almacen: string;
        idExpediente: number;
        expediente: string;
        motivo: string;
        contabilizada: boolean;
    }

    // No se usa ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila porque da error si la celda está vacía (p.e. una línea sin descuento)
    function ped_LectorDeLaFila_interno(parametros: string): (propiedad: string) => string {
        let partes = parametros.split(';');
        let idGrid: string = partes[0];
        let fila: number = Numero(partes[1]);
        let idDeLaFila = `${idGrid.replace('-contenedor', '-tabla_d_tr')}_${fila}`;
        return (propiedad: string) => ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, propiedad);
    }

    function ped_ANumero_interno(texto: string): number {
        return IsNullOrEmpty(texto) ? 0 : Numero(texto);
    }

    function ped_LeerLineaDelGrid_interno(parametros: string): LineaDelGridDePedido {
        const leer = ped_LectorDeLaFila_interno(parametros);
        return {
            id: Numero(leer(literal.id)),
            rowVersion: leer(ltrPropiedades.Elemento.RowVersion),
            cantidad: ped_ANumero_interno(leer(ltrPropiedades.Logistica.Pedido.linea.cantidad)),
            precio: ped_ANumero_interno(leer(ltrPropiedades.Logistica.Pedido.linea.precio)),
            descuento: ped_ANumero_interno(leer(ltrPropiedades.Logistica.Pedido.linea.descuentoPorLinea)),
            pendiente: ped_ANumero_interno(leer(ltrPropiedades.Logistica.Pedido.linea.CantidadPendiente))
        };
    }

    function ped_LeerEntregaDelGrid_interno(parametros: string): EntregaDelGridDePedido {
        const leer = ped_LectorDeLaFila_interno(parametros);
        const propiedades = ltrPropiedades.Logistica.Pedido.Entrega;
        return {
            id: Numero(leer(literal.id)),
            rowVersion: leer(ltrPropiedades.Elemento.RowVersion),
            idLinea: ped_ANumero_interno(leer(propiedades.IdLinea)),
            // en el grid con formato dd/MM/yyyy
            recepcionadaEl: leer(propiedades.RecepcionadaEl),
            cantidad: ped_ANumero_interno(leer(propiedades.Cantidad)),
            precio: ped_ANumero_interno(leer(propiedades.Precio)),
            descuento: ped_ANumero_interno(leer(propiedades.Descuento)),
            idAlmacen: ped_ANumero_interno(leer(propiedades.IdAlmacen)),
            almacen: leer(propiedades.Almacen),
            idExpediente: ped_ANumero_interno(leer(propiedades.IdExpediente)),
            expediente: leer(propiedades.Expediente),
            cantidadDevuelta: ped_ANumero_interno(leer(propiedades.CantidadDevuelta)),
            contabilizada: (leer(propiedades.Contabilizada) ?? '').toLowerCase() === 'true'
        };
    }

    function ped_LeerDevolucionDelGrid_interno(parametros: string): DevolucionDelGridDePedido {
        const leer = ped_LectorDeLaFila_interno(parametros);
        const propiedades = ltrPropiedades.Logistica.Pedido.Devolucion;
        return {
            id: Numero(leer(literal.id)),
            rowVersion: leer(ltrPropiedades.Elemento.RowVersion),
            // en el grid con formato dd/MM/yyyy
            devueltaEl: leer(propiedades.DevueltaEl),
            cantidad: ped_ANumero_interno(leer(propiedades.Cantidad)),
            precio: ped_ANumero_interno(leer(propiedades.Precio)),
            descuento: ped_ANumero_interno(leer(propiedades.Descuento)),
            idAlmacen: ped_ANumero_interno(leer(propiedades.IdAlmacen)),
            almacen: leer(propiedades.Almacen),
            idExpediente: ped_ANumero_interno(leer(propiedades.IdExpediente)),
            expediente: leer(propiedades.Expediente),
            motivo: leer(propiedades.Motivo),
            contabilizada: (leer(propiedades.Contabilizada) ?? '').toLowerCase() === 'true'
        };
    }

    function ped_SePuedeEntregar_interno(linea: LineaDelGridDePedido): boolean {
        if ((Crud.crudMnt.crudDeEdicion as CrudEdicionPedido).SePuedeEntregarLaLinea(linea.pendiente))
            return true;
        MensajesSe.Error('Recepción', linea.pendiente <= 0
            ? 'La línea ya se ha entregado por completo: sus entregas se consultan en el grid de entregas'
            : 'Solo se pueden recepcionar líneas de pedidos solicitados o en recepción');
        return false;
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
