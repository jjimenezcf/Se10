namespace Logistica {

    enum enumEtapasDePedido {
        PED_Etapa_De_Cumplimentacion,
        PED_Etapa_De_Aprobacion,
        PED_Etapa_De_Solicitud,
        PED_Etapa_De_Recepcion,
        PED_Etapa_Cerrado,
        PED_Etapa_Devuelto,
        PED_Etapa_Cancelado
    }

    let crudDePedidos: CrudDePedidos;
    export function CrearCrudDePedidos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDePedidos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };

        crudDePedidos = Crud.crudMnt as CrudDePedidos;
    }

    // Un pedido sólo puede ir a almacenes de su sociedad (ver ExtensorDePedidos.ValidarSociedadDelDestino): se leen los almacenes de la
    // sociedad y se fija el indicado si está entre ellos (como Pag_Tras_Seleccionar_Acreedor con las cuentas del acreedor); sin sociedad
    // (al crear, antes de elegir el CG) la lista queda vacía
    function CargarAlmacenesDeLaSociedad(lista: HTMLSelectElement, idSociedad: number, idAlmacen: number) {
        if (!Definido(lista))
            return;

        // si la lista se está cargando se espera a que acabe, para que su respuesta no se mezcle con la de los almacenes de la sociedad
        if (EsTrue(lista.getAttribute(atListasDeElemento.Cargando))) {
            setTimeout(() => CargarAlmacenesDeLaSociedad(lista, idSociedad, idAlmacen), 500);
            return;
        }

        ApiControl.BlanquearListaDeElementos(lista);
        if (!(idSociedad > 0))
            return;

        const filtros = new Array<ClausulaDeFiltrado>();
        filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, atCriterio.igual, idSociedad.toString()));

        const parametros = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.aplicarJoin, false));
        parametros.push(new Parametro(Ajax.Param.cantidad, -1));
        parametros.push(new Parametro(Ajax.Param.obtenerSeguridad, false));
        parametros.push(new Parametro(Ajax.Param.cargarListaDeElementos, true));

        const claseElemento = lista.getAttribute(atListas.claseElemento);
        const datosDeEntrada = `{"ClaseDeElemento":"${claseElemento}", "IdLista":"${lista.id}", "IdFijar":${idAlmacen > 0 ? idAlmacen : 0}}`;

        lista.setAttribute(atListasDeElemento.Cargando, "S");
        ApiDePeticiones.LeerElementos(lista, lista.getAttribute(atControl.controlador), Ajax.EndPoint.LeerElementos, filtros, parametros, datosDeEntrada)
            .then((peticion: ApiDeAjax.DescriptorAjax) => MapearAlControl.MapearElementosEnLista(peticion))
            .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion))
            .finally(() => lista.setAttribute(atListasDeElemento.Cargando, "N"));
    }

    export class CrudDePedidos extends Crud.CrudMnt {

        private _IdDeUnidadDeMedida: number = 0;
        private _IdDeNaturaleza: number = 0;
        private _TipoDeLinea: string = undefined;
        private _Concepto: string = undefined;

        public get UnidadDeMedida(): number { return this._IdDeUnidadDeMedida; }
        public get Naturaleza(): number { return this._IdDeNaturaleza; }
        public get TipoDeLinea(): string { return this._TipoDeLinea; }

        public set Naturaleza(value: number) {
            if (Numero(value) > 0)
                this._IdDeNaturaleza = value;
            else {
                if (this.MapIndicadores.size > 0)
                    this._IdDeNaturaleza = this.MapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.Naturaleza);
                else
                    this._IdDeNaturaleza = 0;
            }
        }


        public get Concepto(): string {
            return this._Concepto;
        }
        public set Concepto(value: string) {
            this._Concepto = value;
        }


        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPedido(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPedido(this, idPanelEdicion);
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._IdDeUnidadDeMedida = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.UnidadDeMedida);
            this._IdDeNaturaleza = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.Naturaleza);
            this._TipoDeLinea = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.TipoDeLinea);
        }

        public get ModalModificarCabecera(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Pedidos.ModificarCabecera) as HTMLDivElement; }

        public get ModalRecepcionarLinea(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Pedidos.RecepcionarLinea) as HTMLDivElement; }

        public get ModalRecepcionarPedido(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Pedidos.RecepcionarPedido) as HTMLDivElement; }

        public get ModalDevolverEntrega(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Pedidos.DevolverEntrega) as HTMLDivElement; }

        // Entrega que se está devolviendo, con la versión que se leyó en el grid de entregas para validarla en el servidor;
        // si se está consultando una devolución, la modal sólo la muestra
        private _entregaADevolver: EntregaDelGridDePedido = undefined;
        private _consultandoUnaDevolucion: boolean = false;

        // Las dos modales de recepción (línea y pedido completo) tienen el mismo destino, se trabaja con la que esté abierta
        private get ModalDeRecepcionAbierta(): HTMLDivElement {
            return ApiPanel.ModalAbierta(this.ModalRecepcionarPedido) ? this.ModalRecepcionarPedido : this.ModalRecepcionarLinea;
        }

        // Línea que se está recepcionando, con la versión que se leyó en el grid para validarla en el servidor; si se está
        // consultando una entrega (desde el grid de entregas), la modal sólo la muestra
        private _lineaARecepcionar: LineaDelGridDePedido = undefined;
        private _consultandoUnaEntrega: boolean = false;

        private ListaDeAlmacenes(modal: HTMLDivElement): HTMLSelectElement {
            return ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Almacen);
        }

        private ListaDeExpedientes(modal: HTMLDivElement): HTMLInputElement {
            return ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Expediente);
        }

        private BotonAceptar(modal: HTMLDivElement): HTMLInputElement {
            return document.getElementById(`${modal.id}-aceptar`) as HTMLInputElement;
        }

        private static Hoy(): Date {
            let hoy = new Date();
            hoy.setHours(0, 0, 0, 0);
            return hoy;
        }

        private FechaDeLaCabecera(propiedad: string): Date {
            let valor = ObtenerPropiedad(this.crudDeEdicion.Registro, propiedad, null, false);
            if (IsNullOrEmpty(valor)) return null;
            let fecha = new Date(valor);
            return EsFechaValida(fecha) ? fecha : null;
        }

        // Si ya pasó la fecha de entrega del pedido se propone ésta, si no (o no la tiene) la de hoy
        private FechaDeEntregaPropuesta(): Date {
            const hoy = CrudDePedidos.Hoy();
            const entregarEl = this.FechaDeLaCabecera(ltrPropiedades.Logistica.Pedido.EntregarEl);
            return this.NoAnteriorAlPedido(Definido(entregarEl) && entregarEl < hoy ? entregarEl : hoy);
        }

        // En recepción se propone la fecha de recepción de la cabecera; solicitado, la de entrega o la de hoy
        private FechaDeRecepcionDeLineaPropuesta(): Date {
            if ((this.crudDeEdicion as CrudEdicionPedido).EstaEnRecepcion) {
                const recibidoEl = this.FechaDeLaCabecera(ltrPropiedades.Logistica.Pedido.RecibidoEl);
                if (Definido(recibidoEl)) return this.NoAnteriorAlPedido(recibidoEl);
            }
            return this.FechaDeEntregaPropuesta();
        }

        // Una recepción no puede ser anterior a la fecha en la que se pidió (ver ExtensorDeRecepciones.RegistrarRecepcion):
        // si la fecha propuesta lo es, se propone la del pedido
        private NoAnteriorAlPedido(fecha: Date): Date {
            const pedidoEl = this.FechaDeLaCabecera(ltrPropiedades.Logistica.Pedido.PedidoEl);
            if (!Definido(pedidoEl)) return fecha;
            pedidoEl.setHours(0, 0, 0, 0);
            return fecha < pedidoEl ? pedidoEl : fecha;
        }

        private MapearFecha(modal: HTMLDivElement, propiedad: string, fecha: Date) {
            MapearAlControl.FechaDate(ApiControl.BuscarControl(modal, propiedad, true) as HTMLInputElement, fecha);
        }

        // Se propone lo pedido en la línea, el destino de la cabecera y la fecha de recepción; el usuario indica lo que realmente se ha entregado
        public AbrirModalDeRecepcionarLinea(linea: LineaDelGridDePedido) {
            const modal = this.ModalRecepcionarLinea;
            this._lineaARecepcionar = linea;
            this._consultandoUnaEntrega = false;

            this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(modal.id, this.crudDeEdicion.ElementoEditado.Id, undefined, new Array<string>(), false);
            this.MapearDatosDeLaRecepcion(linea.pendiente, linea.precio, linea.descuento);
            this.MapearDestinoDeLaCabecera(modal);
            this.MapearFecha(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.RecepcionadaEl, this.FechaDeRecepcionDeLineaPropuesta());
            this.BloquearRecepcion(false);
            ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad).focus();
        }

        // Una entrega registrada sólo se consulta: para cambiarla hay que cancelarla y volver a entregar la línea
        public ConsultarEntrega(entrega: EntregaDelGridDePedido) {
            const modal = this.ModalRecepcionarLinea;
            this._lineaARecepcionar = undefined;
            this._consultandoUnaEntrega = true;

            this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(modal.id, this.crudDeEdicion.ElementoEditado.Id, undefined, new Array<string>(), false);
            this.MapearDatosDeLaRecepcion(entrega.cantidad, entrega.precio, entrega.descuento);
            this.MapearDestino(modal, entrega.idAlmacen, entrega.idExpediente, entrega.expediente);
            this.MapearFecha(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.RecepcionadaEl, IsNullOrEmpty(entrega.recepcionadaEl) ? null : CrearFecha(entrega.recepcionadaEl));
            this.BloquearRecepcion(true);
        }

        // Cancela la entrega: el servidor valida su versión, que no tenga devoluciones y que el pedido esté en recepción
        public CancelarEntrega(entrega: EntregaDelGridDePedido) {
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(literal.id, entrega.id));
            // con el RowVersion de la entrega, IncluirRowId no añade el del pedido
            parametros.push(new Parametro(Ajax.Param.RowVersion, entrega.rowVersion));
            ApiDePeticiones.EjecutarPeticion(this.crudDeEdicion, ltrControladores.Logistica.EntregasDeUnPedido, Ajax.EndPoint.Logistica.EntregasDeUnPedido.CancelarEntrega, parametros, new Array<Parametro>())
                .then((peticion) => this.RecargarTrasEntregarODevolver(undefined, peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        // Se propone lo que queda por devolver (lo entregado menos lo ya devuelto) valorado al precio y descuento de la entrega, la fecha de hoy
        // y el destino de la entrega; se editan la cantidad, la fecha y el motivo
        public AbrirModalDeDevolverEntrega(entrega: EntregaDelGridDePedido) {
            const modal = this.ModalDevolverEntrega;
            this._entregaADevolver = entrega;
            this._consultandoUnaDevolucion = false;

            this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(modal.id, this.crudDeEdicion.ElementoEditado.Id, undefined, new Array<string>(), false);
            this.MapearDevolucion(this.CantidadPendienteDeDevolver, entrega.precio, entrega.descuento, CrudDePedidos.Hoy(),
                entrega.idAlmacen, entrega.almacen, entrega.idExpediente, entrega.expediente, '');
            this.BloquearDevolucion(false);
            ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.DevolverEntrega.Cantidad).focus();
        }

        // Una devolución registrada sólo se consulta: para cambiarla hay que cancelarla y volver a devolver la entrega
        public ConsultarDevolucion(devolucion: DevolucionDelGridDePedido) {
            const modal = this.ModalDevolverEntrega;
            this._entregaADevolver = undefined;
            this._consultandoUnaDevolucion = true;

            this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(modal.id, this.crudDeEdicion.ElementoEditado.Id, undefined, new Array<string>(), false);
            this.MapearDevolucion(devolucion.cantidad, devolucion.precio, devolucion.descuento, IsNullOrEmpty(devolucion.devueltaEl) ? null : CrearFecha(devolucion.devueltaEl),
                devolucion.idAlmacen, devolucion.almacen, devolucion.idExpediente, devolucion.expediente, devolucion.motivo);
            this.BloquearDevolucion(true);
        }

        // Cancela la devolución: el servidor valida su versión y, si lo devuelto salió del almacén, borra su salida
        public CancelarDevolucion(devolucion: DevolucionDelGridDePedido) {
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(literal.id, devolucion.id));
            // con el RowVersion de la devolución, IncluirRowId no añade el del pedido
            parametros.push(new Parametro(Ajax.Param.RowVersion, devolucion.rowVersion));
            ApiDePeticiones.EjecutarPeticion(this.crudDeEdicion, ltrControladores.Logistica.DevolucionesDeUnPedido, Ajax.EndPoint.Logistica.DevolucionesDeUnPedido.CancelarDevolucion, parametros, new Array<Parametro>())
                .then((peticion) => this.RecargarTrasEntregarODevolver(undefined, peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        // Imprime todas las devoluciones del pedido; el servidor anexa el pdf a los archivos del pedido, que se recargan
        public ImprimirDevoluciones() {
            const editor = this.crudDeEdicion as CrudEdicionPedido;
            if (!editor.HayDevoluciones) {
                MensajesSe.Error('Devoluciones', 'El pedido no tiene devoluciones que imprimir');
                return;
            }
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(literal.id, editor.ElementoEditado.Id));
            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.DevolucionesDeUnPedido, Ajax.EndPoint.Logistica.DevolucionesDeUnPedido.ImprimirDevoluciones, parametros, new Array<Parametro>())
                .then((peticion) => {
                    MensajesSe.Info(peticion.resultado.consola);
                    editor.RecargarArchivos();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private MapearDevolucion(cantidad: number, precio: number, descuento: number, devueltaEl: Date, idAlmacen: number, almacen: string, idExpediente: number, expediente: string, motivo: string) {
            const modal = this.ModalDevolverEntrega;
            const propiedades = ltrPropiedades.Logistica.Pedido.DevolverEntrega;
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Cantidad), cantidad);
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Precio), precio);
            let editorDeDescuento = ApiControl.BuscarEditor(modal, propiedades.Descuento);
            if (descuento > 0) AsignarValor(editorDeDescuento, descuento);
            else editorDeDescuento.value = "";
            this.CalcularImporteDevuelto();

            this.MapearFecha(modal, propiedades.DevueltaEl, devueltaEl);
            MapearAlControl.Restrictor(ApiControl.BuscarRestrictor(modal, propiedades.IdAlmacen, ltrTipoControl.restrictorDeEdicion), idAlmacen, almacen ?? '', true);
            MapearAlControl.Restrictor(ApiControl.BuscarRestrictor(modal, propiedades.IdExpediente, ltrTipoControl.restrictorDeEdicion), idExpediente, expediente ?? '', true);

            let editorDelMotivo = ApiControl.BuscarControl(modal, propiedades.Motivo, true) as HTMLTextAreaElement;
            if (Definido(editorDelMotivo)) editorDelMotivo.value = motivo ?? '';
        }

        // Al consultar una devolución no se edita nada ni se puede aceptar
        private BloquearDevolucion(bloquear: boolean) {
            const modal = this.ModalDevolverEntrega;
            const propiedades = ltrPropiedades.Logistica.Pedido.DevolverEntrega;
            if (bloquear) ApiControl.BloquearEditorPorPropiedad(modal, propiedades.Cantidad);
            else ApiControl.DesbloquearEditorPorPropiedad(modal, propiedades.Cantidad);
            let editorDelMotivo = ApiControl.BuscarControl(modal, propiedades.Motivo, true) as HTMLTextAreaElement;
            if (Definido(editorDelMotivo)) editorDelMotivo.readOnly = bloquear;
            let fecha = ApiControl.BuscarControl(modal, propiedades.DevueltaEl, true) as HTMLInputElement;
            if (Definido(fecha)) fecha.readOnly = bloquear;
            let aceptar = this.BotonAceptar(modal);
            if (Definido(aceptar)) aceptar.disabled = bloquear;
        }

        private get CantidadPendienteDeDevolver(): number {
            const entrega = this._entregaADevolver;
            return Definido(entrega) ? entrega.cantidad - entrega.cantidadDevuelta : 0;
        }

        public CalcularImporteDevuelto() {
            const modal = this.ModalDevolverEntrega;
            const propiedades = ltrPropiedades.Logistica.Pedido.DevolverEntrega;
            let cantidad = Numero(ApiControl.BuscarEditor(modal, propiedades.Cantidad).value);
            let precio = Numero(ApiControl.BuscarEditor(modal, propiedades.Precio).value);
            let descuento = Numero(ApiControl.BuscarEditor(modal, propiedades.Descuento).value);
            let importe = ApiControl.BuscarEditor(modal, propiedades.Importe);
            let importeDevuelto = cantidad * precio * (1 - descuento / 100);
            if (importeDevuelto > 0) AsignarValor(importe, importeDevuelto);
            else importe.value = "";
        }

        // Nunca se devuelve más de lo que queda por devolver, el servidor lo vuelve a validar (ver ExtensorDeDevolucionesDePedido.ValidarDevolucion)
        private DevolverEntrega(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionPedido;
            const entrega = this._entregaADevolver;
            const propiedades = ltrPropiedades.Logistica.Pedido.DevolverEntrega;
            if (this._consultandoUnaDevolucion || !Definido(entrega))
                return;

            let cantidad = Numero(ApiControl.BuscarEditor(modal, propiedades.Cantidad).value);
            const pendiente = this.CantidadPendienteDeDevolver;
            if (!(cantidad > 0)) {
                MensajesSe.Error('Devolución', 'Indique la cantidad a devolver');
                return;
            }
            if (cantidad > pendiente) {
                MensajesSe.Error('Devolución', `Como mucho se pueden devolver ${pendiente}: se entregaron ${entrega.cantidad} y ya se han devuelto ${entrega.cantidadDevuelta}`);
                return;
            }
            let motivo = ApiControl.BuscarControl(modal, propiedades.Motivo, true) as HTMLTextAreaElement;
            if (!Definido(motivo) || IsNullOrEmpty(motivo.value.trim())) {
                MensajesSe.Error('Devolución', 'Indique el motivo de la devolución');
                return;
            }
            // entre la entrega y hoy; el servidor además no la deja anterior al último cierre contable (ver ExtensorDeDevolucionesDePedido.ValidarFechaDeDevolucion)
            let fecha = ApiControl.BuscarControl(modal, propiedades.DevueltaEl, true) as HTMLInputElement;
            if (!Definido(fecha) || IsNullOrEmpty(fecha.value)) {
                MensajesSe.Error('Devolución', 'Indique la fecha de la devolución');
                return;
            }
            const [ano, mes, dia] = fecha.value.split("-").map(Number);
            const devueltaEl = new Date(ano, mes - 1, dia);
            devueltaEl.setHours(0, 0, 0, 0);
            if (devueltaEl > CrudDePedidos.Hoy()) {
                MensajesSe.Error('Devolución', 'La fecha de la devolución no puede ser posterior a hoy');
                return;
            }
            if (!IsNullOrEmpty(entrega.recepcionadaEl)) {
                const entregadaEl = CrearFecha(entrega.recepcionadaEl);
                entregadaEl.setHours(0, 0, 0, 0);
                if (devueltaEl < entregadaEl) {
                    MensajesSe.Error('Devolución', `La fecha de la devolución no puede ser anterior a la de la entrega (${entregadaEl.toLocaleDateString()})`);
                    return;
                }
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            // la fecha se envía como la de los selectores de fecha: el día local en UTC (ver ApiDeMapeos.ObtenerFecha)
            MapearALaListaDeParametros.Fechas(modal, parametros);
            parametros.push(new Parametro(literal.id, entrega.id));
            // con el RowVersion de la entrega, IncluirRowId no añade el del pedido
            parametros.push(new Parametro(Ajax.Param.RowVersion, entrega.rowVersion));
            parametros.push(new Parametro(propiedades.Cantidad, cantidad));
            parametros.push(new Parametro(propiedades.Motivo, motivo.value.trim()));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.DevolucionesDeUnPedido, Ajax.EndPoint.Logistica.DevolucionesDeUnPedido.DevolverEntrega, parametros, new Array<Parametro>())
                .then((peticion) => this.RecargarTrasEntregarODevolver(modal, peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        // Entrega completa: se propone el destino de la cabecera y la fecha de entrega o la de hoy
        public AbrirModalDeRecepcionarPedido() {
            const modal = this.ModalRecepcionarPedido;
            const editor = this.crudDeEdicion;
            editor.Expansor_AbrirModalParaPedirDatos(modal.id, editor.ElementoEditado.Id, undefined, new Array<string>(), false);

            let pedido = ApiControl.BuscarRestrictor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarPedido.IdPedido, ltrTipoControl.restrictorDeEdicion);
            MapearAlControl.Restrictor(pedido, editor.ElementoEditado.Id, ObtenerPropiedad(editor.Registro, literal.nombre, '', false), true);
            this.MapearDestinoDeLaCabecera(modal);
            this.MapearFecha(modal, ltrPropiedades.Logistica.Pedido.RecepcionarPedido.RecepcionadoEl, this.FechaDeEntregaPropuesta());
        }

        private MapearDatosDeLaRecepcion(cantidad: number, precio: number, descuento: number) {
            const modal = this.ModalRecepcionarLinea;
            AsignarValor(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad), cantidad);
            AsignarValor(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio), precio);
            let editorDeDescuento = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento);
            if (descuento > 0) AsignarValor(editorDeDescuento, descuento);
            else editorDeDescuento.value = "";
            this.CalcularImporteEntregado();
        }

        // El destino por defecto es el almacén de la cabecera y, si no lo tiene, la obra de su expediente (ver ExtensorDeRecepciones.DestinoDeLasEntregas)
        private MapearDestinoDeLaCabecera(modal: HTMLDivElement) {
            const cabecera = this.crudDeEdicion.Registro;
            let idAlmacen = Numero(ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.IdAlmacen, 0, false));
            let idExpediente = Numero(ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.idExpediente, 0, false));
            let expediente = ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.Expediente, '', false);
            if (idAlmacen > 0) idExpediente = 0;
            this.MapearDestino(modal, idAlmacen, idExpediente, expediente);
        }

        // El almacén y el expediente sólo se eligen entre los de la sociedad del pedido: el control oculto de la sociedad restringe el
        // selector de expedientes y la lista de almacenes se carga con los de la sociedad
        private MapearDestino(modal: HTMLDivElement, idAlmacen: number, idExpediente: number, expediente: string) {
            const idSociedad = Numero(ObtenerPropiedad(this.crudDeEdicion.Registro, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, 0, false));
            const sociedad = ApiControl.BuscarEditor(modal, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg) as HTMLInputElement;
            if (Definido(sociedad)) sociedad.value = idSociedad > 0 ? idSociedad.toString() : '';

            const almacenes = this.ListaDeAlmacenes(modal);
            const expedientes = this.ListaDeExpedientes(modal);
            CargarAlmacenesDeLaSociedad(almacenes, idSociedad, idAlmacen);
            if (idExpediente > 0) MapearAlControl.ListaDinamica(expedientes, idExpediente, expediente, false);
            else ApiListaDinamica.Blanquear(expedientes);
        }

        // Una entrega ya registrada sólo se consulta: para cambiarla hay que cancelarla y volver a entregar la línea
        private BloquearRecepcion(bloquear: boolean) {
            const modal = this.ModalRecepcionarLinea;
            const propiedades = ltrPropiedades.Logistica.Pedido.RecepcionarLinea;
            for (const editor of [propiedades.Cantidad, propiedades.Precio, propiedades.Descuento]) {
                if (bloquear) ApiControl.BloquearEditorPorPropiedad(modal, editor);
                else ApiControl.DesbloquearEditorPorPropiedad(modal, editor);
            }
            let fecha = ApiControl.BuscarControl(modal, propiedades.RecepcionadaEl, true) as HTMLInputElement;
            if (bloquear) {
                ApiControl.BloquearListaDeElemento(modal, propiedades.Almacen);
                ApiControl.BloquearListaDinamicaPorPropiedad(modal, propiedades.Expediente);
            }
            else {
                ApiControl.DesbloquearListaDeElemento(modal, propiedades.Almacen);
                ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, propiedades.Expediente);
            }
            if (Definido(fecha)) fecha.readOnly = bloquear;
            let aceptar = this.BotonAceptar(modal);
            if (Definido(aceptar)) aceptar.disabled = bloquear;
        }

        // La entrega va al almacén o a la obra del expediente, nunca a ambos: al elegir uno se blanquea el otro
        // Al consultar una entrega ya hecha no se toca el destino: el almacén se fija al terminar de cargarse su lista, después de
        // bloquear la modal, y blanquear el expediente lo desbloquearía (ApiListaDinamica.Blanquear habilita las listas editables)
        public Recepcion_TrasCambiarAlmacen() {
            const modal = this.ModalDeRecepcionAbierta;
            if (this.ConsultandoUnaEntrega(modal)) return;
            if (Numero(this.ListaDeAlmacenes(modal).value) > 0)
                ApiListaDinamica.Blanquear(this.ListaDeExpedientes(modal));
        }

        public Recepcion_TrasSeleccionarExpediente() {
            const modal = this.ModalDeRecepcionAbierta;
            if (this.ConsultandoUnaEntrega(modal)) return;
            if (Numero(this.ListaDeExpedientes(modal).getAttribute(atListasDinamicas.idSeleccionado)) > 0)
                this.ListaDeAlmacenes(modal).selectedIndex = 0;
        }

        private ConsultandoUnaEntrega(modal: HTMLDivElement): boolean {
            return modal.id === this.ModalRecepcionarLinea.id && this._consultandoUnaEntrega;
        }

        public CalcularImporteEntregado() {
            const modal = this.ModalRecepcionarLinea;
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad).value);
            let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio).value);
            let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento).value);
            let importe = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.ImporteEntregado);
            let importeEntregado = cantidad * precio * (1 - descuento / 100);
            if (importeEntregado > 0) AsignarValor(importe, importeEntregado);
            else importe.value = "";
        }

        // Añade a los parámetros el destino y la fecha de la modal; false si falta alguno
        private AnadirDestinoYFecha(modal: HTMLDivElement, propiedadDeLaFecha: string, parametros: Array<Parametro>): boolean {
            let idAlmacen = Numero(this.ListaDeAlmacenes(modal).value);
            let idExpediente = Numero(this.ListaDeExpedientes(modal).getAttribute(atListasDinamicas.idSeleccionado));
            if ((idAlmacen > 0) === (idExpediente > 0)) {
                MensajesSe.Error('Recepción', 'Indique el almacén o el expediente al que va la entrega, y sólo uno de ellos');
                return false;
            }
            let fecha = ApiControl.BuscarControl(modal, propiedadDeLaFecha, true) as HTMLInputElement;
            if (IsNullOrEmpty(fecha.value)) {
                MensajesSe.Error('Recepción', 'Indique la fecha de recepción');
                return false;
            }
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdAlmacen, idAlmacen > 0 ? idAlmacen : 0));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdExpediente, idExpediente > 0 ? idExpediente : 0));
            MapearALaListaDeParametros.Fechas(modal, parametros);
            return true;
        }

        // Tras una entrega, una devolución o su cancelación se recargan las líneas (con ellas la cabecera, que puede haber pasado a
        // recepción), las entregas, las devoluciones y las trazas
        private RecargarTrasEntregarODevolver(modal: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax) {
            MensajesSe.Info(peticion.resultado.consola);
            if (Definido(modal)) super.ModalDePedirDatos_Cerrar(modal);
            (this.crudDeEdicion as CrudEdicionPedido).RecargarDetallesDelPedido();
        }

        // Entrega completa de la línea (visto verde): sin modal, con lo pedido en la línea, el destino de la cabecera y la fecha de
        // recepción que se propondría en la modal; el servidor valida la etapa y si está solicitado pasa el pedido a recepción
        public EntregarLineaCompleta(linea: LineaDelGridDePedido) {
            const editor = this.crudDeEdicion as CrudEdicionPedido;
            const cabecera = editor.Registro;
            let idAlmacen = Numero(ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.IdAlmacen, 0, false));
            let idExpediente = idAlmacen > 0 ? 0 : Numero(ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.idExpediente, 0, false));
            // la fecha se envía como la de los selectores de fecha: el día local en UTC (ver ApiDeMapeos.ObtenerFecha)
            const fecha = this.FechaDeRecepcionDeLineaPropuesta();
            const recepcionadaEl = new Date(Date.UTC(fecha.getFullYear(), fecha.getMonth(), fecha.getDate()));

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdLineaPedido, linea.id));
            // con el RowVersion de la línea, IncluirRowId no añade el del pedido
            parametros.push(new Parametro(Ajax.Param.RowVersion, linea.rowVersion));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad, linea.pendiente));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio, linea.precio));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento, linea.descuento));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdAlmacen, idAlmacen));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdExpediente, idExpediente));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.RecepcionadaEl, recepcionadaEl));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.LineasDeUnPedido, Ajax.EndPoint.Logistica.LineasDeUnPedido.RecibirLinea, parametros, new Array<Parametro>())
                .then((peticion) => this.RecargarTrasEntregarODevolver(undefined, peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private RecepcionarLinea(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionPedido;
            const linea = this._lineaARecepcionar;
            if (this._consultandoUnaEntrega || !Definido(linea))
                return;

            // nunca se entrega más de lo pedido, el servidor lo vuelve a validar (ver ExtensorDeRecepciones.RegistrarRecepcion)
            const cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad).value);
            if (cantidad > linea.pendiente) {
                MensajesSe.Error('Recepción', `Como mucho se pueden entregar ${linea.pendiente}, lo que queda pendiente de lo pedido`);
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            if (!this.AnadirDestinoYFecha(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.RecepcionadaEl, parametros))
                return;
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdLineaPedido, linea.id));
            // con el RowVersion de la línea, IncluirRowId no añade el del pedido
            parametros.push(new Parametro(Ajax.Param.RowVersion, linea.rowVersion));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad, Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad).value)));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio, Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio).value)));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento, Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento).value)));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.LineasDeUnPedido, Ajax.EndPoint.Logistica.LineasDeUnPedido.RecibirLinea, parametros, new Array<Parametro>())
                .then((peticion) => this.RecargarTrasEntregarODevolver(modal, peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        // El llamador es la edición, así IncluirRowId añade el RowVersion del pedido y el servidor lo valida
        private RecepcionarPedido(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionPedido;
            let parametros: Array<Parametro> = new Array<Parametro>();
            if (!this.AnadirDestinoYFecha(modal, ltrPropiedades.Logistica.Pedido.RecepcionarPedido.RecepcionadoEl, parametros))
                return;
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarPedido.IdPedido, editor.ElementoEditado.Id));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.Pedidos, Ajax.EndPoint.Logistica.Pedidos.RecepcionarPedido, parametros, new Array<Parametro>())
                .then((peticion) => this.RecargarTrasEntregarODevolver(modal, peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id === this.ModalRecepcionarLinea.id) {
                this.RecepcionarLinea(modal);
                return;
            }
            if (modal.id === this.ModalRecepcionarPedido.id) {
                this.RecepcionarPedido(modal);
                return;
            }
            if (modal.id === this.ModalDevolverEntrega.id) {
                this.DevolverEntrega(modal);
                return;
            }
            if (modal.id === this.ModalModificarCabecera.id) {
                const editor = this.crudDeEdicion;
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                parametros.push(new Parametro(Ajax.Param.ids, [editor.ElementoEditado.Id]));
                ApiDePeticiones.ProcesarOpcionMf(this, this.Controlador, this.IdNegocio, ltrMenus.eventosDeMf.Logistica.Pedidos.GuardarCabecera, false, parametros, new Array<Parametro>())
                    .then(() => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        editor.RecargarValoresDeCabecera(editor.ElementoEditado.Id);
                        editor.RecargarGridDeTrazas();
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
            super.ModalDePedirDatos_Aceptar(modal);
        }

        // La modal de modificar la cabecera se propone con lo que el usuario ve en la edición, no con lo que haya ahora en el servidor
        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalModificarCabecera.id) {
                const panel = this.crudDeEdicion.PanelDelDto;
                ApiDelCrud.ProponerParaRenombrar(modal, this.crudDeEdicion.Registro);
                ApiDelCrud.ProponerPropiedad(panel, modal, literal.nombre, false);
                ApiDelCrud.ProponerPropiedad(panel, modal, ltrPropiedades.Logistica.Pedido.eMail, false);
                ApiDelCrud.ProponerPropiedad(panel, modal, ltrPropiedades.Logistica.Pedido.EntregarEl, false);
                ApiDelCrud.ProponerPropiedad(panel, modal, ltrPropiedades.Logistica.Pedido.RecibidoEl, false);
                ApiDelCrud.ProponerPropiedad(panel, modal, ltrPropiedades.Logistica.Pedido.CerradoEl, false);
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Logistica.Pedidos);
                return true;
            }
            return false;
        }
    }

    export class CrudCreacionPedido extends Crud.CrudCreacion {

        
        public get PanelDeDireccion(): HTMLDivElement {
            return this.PanelDeAmpliacion(ltrAmpliaciones.Comunes.CrearDireccion.toLowerCase()) as HTMLDivElement;
        }

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            this.AplicarSociedadAlDestino(false);
        }

        public TrasSeleccionarCg(idLista: string): void {
            super.TrasSeleccionarCg(idLista);
            this.LeerDireccionDeEntrega();
            this.AplicarSociedadAlDestino(true);
        }

        public TrasBlanquearCg() {
            super.TrasBlanquearCg();
            ApiPanel.BlanquearControlesDeIU(this.PanelDeDireccion);
            this.AplicarSociedadAlDestino(true);
        }

        // El almacén y el expediente han de ser de la sociedad del CG: al cambiar de CG se quitan y los almacenes se cargan con los de su
        // sociedad (el selector de expedientes se restringe con el control de la sociedad, ver PedidoDto.Expediente)
        private AplicarSociedadAlDestino(cambiaElCg: boolean) {
            if (cambiaElCg) {
                const expediente = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Logistica.Pedido.Expediente) as HTMLInputElement;
                if (Definido(expediente)) ApiListaDinamica.Blanquear(expediente);
            }
            const almacen = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Logistica.Pedido.Almacen);
            const idSociedad = Definido(this.Sociedad) ? Numero(this.Sociedad.value) : 0;
            CargarAlmacenesDeLaSociedad(almacen, idSociedad, cambiaElCg || !Definido(almacen) ? 0 : Numero(almacen.value));
        }


        private LeerDireccionDeEntrega() {
            var ctrlCg = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Elemento.ConCg.Cg)
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.id, Numero(ctrlCg.getAttribute(atListasDinamicas.idSeleccionado))));
            parametros.push(new Parametro(Ajax.Callejero.Parametros.Calificador, enumCalificadorDireccion.Entrega));
            ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, ltrControladores.Terceros.CentrosGestores, Ajax.EndPoint.Terceros.CG.LeerDireccion, parametros, new Array<Parametro>(), false)
                .then(
                    (peticion: ApiDeAjax.DescriptorAjax) => {
                        this.MapearDireccionDeEntrega(peticion);
                    })
                .catch((peticion: ApiDeAjax.DescriptorAjax) => {
                    ApiDePeticiones.EmitirError(peticion)
                });
        }

        private MapearDireccionDeEntrega(peticion: ApiDeAjax.DescriptorAjax): void {
            if (!Definido(peticion.resultado.datos)) {
                ApiPanel.BlanquearControlesDeIU(this.PanelDeDireccion);
                return;
            }
            MapearAlPanel.ElObjeto(this.PanelDeDireccion, peticion.resultado.datos, ModoAcceso.enumModoDeAccesoDeDatos.Gestor, new Array<string>());
        }
    }

    export class CrudEdicionPedido extends Crud.CrudEdicion {

        private static readonly EtiquetaDePedidoEl = 'Pedido el';
        private static readonly AyudaDePedidoEl = 'Fecha en la que se solicitó el pedido al proveedor, la fija el sistema al pasar el pedido a solicitado';

        // Etiqueta y ayuda con las que se renderiza la fecha de pedido (las del Dto), para restaurarlas al editar otro pedido
        private _EtiquetaDePedirEl: string = undefined;
        private _AyudaDePedirEl: string = undefined;

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Logistica.Pedidos.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Logistica.Pedidos.Lineas);
        }

        public get GridDeLineas(): HTMLDivElement {
            return document.getElementById('grid-de-detalle-lineasdeunpedido-tabla') as HTMLDivElement;
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeEdicionDeLineas);
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public RecargarGridDeLineas() {
            this.RecargarGridDelPedido(ltrEspanes.Logistica.Pedidos.Lineas);
        }

        // Tras una entrega o una devolución: las líneas (al recargarlas se recarga la cabecera), las entregas, las devoluciones y las trazas
        public RecargarDetallesDelPedido() {
            this.RecargarGridDelPedido(ltrEspanes.Logistica.Pedidos.Lineas);
            this.RecargarGridDelPedido(ltrEspanes.Logistica.Pedidos.Entregas);
            this.RecargarGridDelPedido(ltrEspanes.Logistica.Pedidos.Devoluciones);
            this.RecargarGridDeTrazas();
        }

        private RecargarGridDelPedido(nombre: string) {
            let grid = document.getElementById(this.IdGridDelExpansor(nombre)) as HTMLDivElement;
            if (Definido(grid))
                this.RecargarGridDeRelacion(grid, this.CrudDeMnt.IdNegocio, this.ElementoEditado.Id);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            let seEstaCumplimentando = EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Cumplimentacion);

            this.AplicarSociedadAlAlmacen(seEstaCumplimentando);
            let seEstaCumplimentandoOAprobando =
                seEstaCumplimentando ||
                EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Aprobacion);
            this.AplicarEtiquetaDeLaFechaDePedido(panel, !seEstaCumplimentandoOAprobando);

            // El fichero de pedido solo se puede añadir al crear o, en edición, mientras se está cumplimentando
            ApiDeArchivos.MostrarSelectorDeArchivoSi(this.PanelDelDto, seEstaCumplimentando);

            // Fuera de cumplimentación la cabecera es de consulta, el interventor la corrige con "Modificar cabecera" mientras no esté anulado
            let estaAnulado = EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_Cancelado);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Logistica.Pedidos.ModificarCabecera, ltrMenus.enumOrigen.edicion, this.EsInterventorSinEstado && !estaAnulado);

            // Solicitado y en recepción el servidor mantiene el permiso sobre el pedido para poder recepcionar, pero la cabecera es de consulta
            if (EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Solicitud) ||
                EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Recepcion))
                ApiPanel.PonerEnModoConsulta(this.PanelDelDto);

            this.AplicarEtapaAlGridDeLineas();
            this.AplicarEtapaAlGridDeEntregas();
            this.AplicarEtapaAlGridDeDevoluciones();
        }
        // En cumplimentación la cabecera se puede cambiar: se leen los almacenes de la sociedad del CG del pedido y se fija el de la cabecera.
        // En el resto de etapas la cabecera es de consulta y basta con el almacén mapeado, salvo que la lista se cargara antes con los
        // almacenes de otra sociedad (al editar otro pedido) y no lo tenga
        private AplicarSociedadAlAlmacen(seEstaCumplimentando: boolean) {
            const lista = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Logistica.Pedido.Almacen);
            if (!Definido(lista))
                return;

            const idAlmacen = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.IdAlmacen, 0, false));
            const estaEnLaLista = Array.from(lista.options).some(opcion => Numero(opcion.value) === idAlmacen);
            if (!seEstaCumplimentando && (idAlmacen === 0 || estaEnLaLista || EsTrue(lista.getAttribute(atListasDeElemento.Cargando))))
                return;

            CargarAlmacenesDeLaSociedad(lista, Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, 0, false)), idAlmacen);
        }

        public get IdReferenciaCrearLinea(): string {
            return this._idPanelEdicion + `-lineasdeunpedido-mcr-${enumPostfijoControl.Referencia}`;
        }

        // Las líneas solo se crean, modifican y borran mientras el pedido se cumplimenta, en el resto de etapas se consultan
        public get SePuedenModificarLasLineas(): boolean {
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            return EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Cumplimentacion) &&
                ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso);
        }

        // Las acciones de recepción de las líneas solo están activas con el pedido solicitado o en recepción
        public get SePuedeRecepcionar(): boolean {
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            return EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Solicitud) ||
                EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Recepcion);
        }

        public Expansor_TrasCargarDetalle(idGrid: string) {
            super.Expansor_TrasCargarDetalle(idGrid);
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Lineas) === idGrid)
                this.AplicarEtapaAlGridDeLineas();
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Entregas) === idGrid)
                this.AplicarEtapaAlGridDeEntregas();
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Devoluciones) === idGrid)
                this.AplicarEtapaAlGridDeDevoluciones();
        }

        public get EstaCerrado(): boolean {
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            return EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_Cerrado);
        }

        // Devolver una entrega: quien gestiona el pedido, en recepción o cerrado, si queda algo por devolver
        public SePuedeDevolverLaEntrega(pendienteDeDevolver: number): boolean {
            return this.EsGestorDelPedido && pendienteDeDevolver > 0 && (this.EstaEnRecepcion || this.EstaCerrado);
        }

        // Cancelar una devolución: quien gestiona el pedido, en recepción o cerrado, si su preasiento no está contabilizado
        public SePuedeCancelarLaDevolucion(contabilizada: boolean): boolean {
            return this.EsGestorDelPedido && !contabilizada && (this.EstaEnRecepcion || this.EstaCerrado);
        }

        public get EstaEnRecepcion(): boolean {
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            return EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Recepcion);
        }

        // Entrega parcial (Recepcionar) y total (Recibida): con el pedido solicitado o en recepción y si la línea aún no se ha entregado
        public SePuedeEntregarLaLinea(pendiente: number): boolean {
            return this.SePuedeRecepcionar && pendiente > 0;
        }

        // Cancelar una entrega (grid de entregas): sólo con el pedido en recepción (solicitado no puede haber nada entregado), si no
        // tiene devoluciones y si su preasiento no está contabilizado (en un lote contable)
        public SePuedeCancelarLaEntrega(contabilizada: boolean, cantidadDevuelta: number): boolean {
            return this.EsGestorDelPedido && this.EstaEnRecepcion && !contabilizada && !(cantidadDevuelta > 0);
        }

        // Las entregas van al almacén si los datos principales lo tienen indicado (ver ExtensorDeRecepciones.DestinoDeLasEntregas):
        // entonces sólo se piden materiales del catálogo (ver GestorDeLineasDeUnPedido.AntesDePersistir)
        public get ElDestinoEsUnAlmacen(): boolean {
            let almacen = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Logistica.Pedido.Almacen);
            return Definido(almacen) && Numero(almacen.value) > 0;
        }

        public get EsGestorDelPedido(): boolean {
            return ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso);
        }

        // Entrega completa de la línea (visto verde): la entrega con lo pedido sin pedir datos; quien gestiona el pedido,
        // con el pedido solicitado o en recepción y si la línea aún no se ha entregado
        public SePuedeEntregarLaLineaCompleta(pendiente: number): boolean {
            return this.EsGestorDelPedido && this.SePuedeEntregarLaLinea(pendiente);
        }

        // "Dar por entregado" (menú de edición): entrega lo pendiente de todas las líneas; quien lo gestiona, solicitado o en recepción y si
        // a alguna línea le queda algo por entregar
        public get SePuedeRecepcionarElPedido(): boolean {
            return this.EsGestorDelPedido && this.SePuedeRecepcionar && this.HayLineasPendientes;
        }

        public get HayLineasPendientes(): boolean {
            return this.FilasDelGrid(ltrEspanes.Logistica.Pedidos.Lineas).some(fila => CrudEdicionPedido.PendienteDeLaLinea(fila) > 0);
        }

        private static readonly AccionDeRecepcionar = 'Recepcionar';
        private static readonly AccionDeEntregaCompleta = 'Recibida';
        private static readonly AccionDeConsultar = 'Consultar';
        private static readonly AccionDeCancelar = 'Cancelar';
        private static readonly AccionDeDevolver = 'Devolver';

        private AplicarEtapaAlGridDeLineas() {
            const idGrid = this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Lineas);
            let grid = document.getElementById(idGrid);
            if (!Definido(grid)) return;

            const sePuedenModificarLasLineas = this.SePuedenModificarLasLineas;
            if (sePuedenModificarLasLineas)
                ApiDeGrid.Expansor_PonerEnEdicion(idGrid);
            else
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);
            ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearLinea, sePuedenModificarLasLineas);

            // "Dar por entregado" depende de que quede algo pendiente, se ajusta cada vez que se carga el grid
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Logistica.Pedidos.RecepcionarPedido, ltrMenus.enumOrigen.edicion, this.SePuedeRecepcionarElPedido);

            let tbody = grid.querySelector('.' + ltrCss.crud.tbody);
            if (!Definido(tbody)) return;
            tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila).forEach(fila => {
                const pendiente = CrudEdicionPedido.PendienteDeLaLinea(fila);
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeRecepcionar, this.SePuedeEntregarLaLinea(pendiente));
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeEntregaCompleta, this.SePuedeEntregarLaLineaCompleta(pendiente));
            });
        }

        // Cada entrega se consulta siempre; se cancela o se devuelve según la etapa, lo devuelto y si está contabilizada
        private AplicarEtapaAlGridDeEntregas() {
            const propiedades = ltrPropiedades.Logistica.Pedido.Entrega;
            this.FilasDelGrid(ltrEspanes.Logistica.Pedidos.Entregas).forEach(fila => {
                const devuelto = CrudEdicionPedido.NumeroDeLaFila(fila, propiedades.CantidadDevuelta);
                const pendienteDeDevolver = CrudEdicionPedido.NumeroDeLaFila(fila, propiedades.Cantidad) - devuelto;
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeConsultar, true);
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeCancelar, this.SePuedeCancelarLaEntrega(CrudEdicionPedido.EsTrueEnLaFila(fila, propiedades.Contabilizada), devuelto));
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeDevolver, this.SePuedeDevolverLaEntrega(pendienteDeDevolver));
            });
        }

        // Cada devolución se consulta siempre; se cancela según la etapa y si está contabilizada
        private AplicarEtapaAlGridDeDevoluciones() {
            const propiedades = ltrPropiedades.Logistica.Pedido.Devolucion;
            this.FilasDelGrid(ltrEspanes.Logistica.Pedidos.Devoluciones).forEach(fila => {
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeConsultar, true);
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeCancelar, this.SePuedeCancelarLaDevolucion(CrudEdicionPedido.EsTrueEnLaFila(fila, propiedades.Contabilizada)));
            });
        }

        // para imprimirlas (ver CrudDePedidos.ImprimirDevoluciones)
        public get HayDevoluciones(): boolean {
            return this.FilasDelGrid(ltrEspanes.Logistica.Pedidos.Devoluciones).length > 0;
        }

        private FilasDelGrid(nombre: string): Array<HTMLDivElement> {
            let grid = document.getElementById(this.IdGridDelExpansor(nombre));
            let tbody = Definido(grid) ? grid.querySelector('.' + ltrCss.crud.tbody) : null;
            return Definido(tbody) ? Array.from(tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila)) : new Array<HTMLDivElement>();
        }

        private static EsTrueEnLaFila(fila: HTMLDivElement, propiedad: string): boolean {
            let celda = fila.querySelector<HTMLInputElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}='${propiedad.toLowerCase()}'] input`);
            return Definido(celda) && celda.value.toLowerCase() === 'true';
        }

        private static NumeroDeLaFila(fila: HTMLDivElement, propiedad: string): number {
            let celda = fila.querySelector<HTMLInputElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}='${propiedad.toLowerCase()}'] input`);
            return Definido(celda) && !IsNullOrEmpty(celda.value) ? Numero(celda.value) : 0;
        }

        private static PendienteDeLaLinea(fila: HTMLDivElement): number {
            return CrudEdicionPedido.NumeroDeLaFila(fila, ltrPropiedades.Logistica.Pedido.linea.CantidadPendiente);
        }

        private static HabilitarAccionDeLaFila(fila: HTMLDivElement, accion: string, habilitar: boolean) {
            let a = fila.querySelector<HTMLAnchorElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}='${accion}'] a`);
            if (!Definido(a)) return;
            if (habilitar) ApiControl.ExcluirCss(a, ltrCss.Detalle.AccionDeshabilitada);
            else ApiControl.IncluirCss(a, ltrCss.Detalle.AccionDeshabilitada);
        }

        // "Pedir el" mientras el pedido se está cumplimentando o aprobando: es la fecha planificada para solicitarlo;
        // en el resto de etapas ya se ha solicitado, así que la etiqueta pasa a "Pedido el" y la ayuda explica que es la fecha de solicitud
        private AplicarEtiquetaDeLaFechaDePedido(panel: HTMLDivElement, seHaSolicitado: boolean) {
            let etiqueta = ApiControl.BuscarEtiqueta(panel, ltrPropiedades.Logistica.Pedido.PedidoEl);
            let fecha = ApiControl.BuscarControl(panel, ltrPropiedades.Logistica.Pedido.PedidoEl, true) as HTMLInputElement;

            if (!Definido(this._EtiquetaDePedirEl)) {
                this._EtiquetaDePedirEl = etiqueta.innerText;
                this._AyudaDePedirEl = etiqueta.title;
            }

            etiqueta.innerText = seHaSolicitado ? CrudEdicionPedido.EtiquetaDePedidoEl : this._EtiquetaDePedirEl;
            etiqueta.title = seHaSolicitado ? CrudEdicionPedido.AyudaDePedidoEl : this._AyudaDePedirEl;
            fecha.placeholder = etiqueta.title;
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Logistica.Pedidos.ModificarCabecera) {
                // la cabecera se propone desde la edición (ver CrudDePedidos.ModalDePedirDatos_TrasAbrir)
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDePedidos).ModalModificarCabecera.id, this.ElementoEditado.Id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Logistica.Pedidos.CopiarPedido) {
                // el servidor devuelve el pedido creado, en el estado inicial, y se abre su edición en otra pestaña (como al copiar una factura emitida)
                const idPedido = ObtenerPropiedad(peticion.resultado.datos, literal.id);
                EntornoSe.AbrirPestana(`${window.location.origin}/${ltrUrls.Logistica.Pedidos}?${literal.id}=${idPedido}`);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Logistica.Pedidos.RecepcionarPedido) {
                // "Dar por entregado": al aceptar la modal se llama a epRecepcionarPedido (ver CrudDePedidos.ModalDePedirDatos_Aceptar)
                if (!this.SePuedeRecepcionarElPedido)
                    MensajesSe.Error('Dar por entregado', !this.HayLineasPendientes
                        ? 'El pedido no tiene nada pendiente de entregar'
                        : 'Solo se pueden dar por entregados pedidos solicitados o en recepción');
                else
                    (this.CrudDeMnt as CrudDePedidos).AbrirModalDeRecepcionarPedido();
                return true;
            }
            return false;
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);

        }

        public AplicarTipoDeLinea() {
            let ocultar: boolean = false;

            let modal = this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Pedido.linea.tipoDeLinea, true) as HTMLSelectElement;

            switch (tipoDeLinea.selectedIndex) {
                case 0: {
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.concepto);
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.unitario);
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.precio);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                    break;
                }
                case 1: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.unitario);
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.precio);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                    if (this.EstaCreandoUnaLinea) {
                        // La naturaleza a proponer ya viene resuelta desde el servidor: la del proveedor de la cabecera
                        // si la tiene definida, si no la del indicador de negocio, si no en blanco (ver GestorDePedidos.DespuesDeMapearElElemento)
                        let idNaturalezaPropuesta = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.IdNaturaleza, 0));
                        if (idNaturalezaPropuesta > 0) {
                            var SelectorNaturaleza = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                            MapearAlControl.ListaDeElementos(SelectorNaturaleza, new Array<ClausulaDeFiltrado>(), idNaturalezaPropuesta, null);
                        }
                        if ((this.CrudDeMnt as CrudDePedidos).UnidadDeMedida > 0) {
                            var SelectorUnidad = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                            MapearAlControl.ListaDeElementos(SelectorUnidad, new Array<ClausulaDeFiltrado>(), (this.CrudDeMnt as CrudDePedidos).UnidadDeMedida, null);
                        }
                        // La clase ya no se muestra ni se propone en la modal: siempre se deriva de la naturaleza en el servidor

                        // El proveedor del pedido, si tiene unidad/concepto propios definidos, prevalece sobre los valores por defecto del negocio
                        let idUnidadDelProveedor = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.IdUnidadDelProveedor, 0));
                        if (idUnidadDelProveedor > 0) {
                            var SelectorUnidadProveedor = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                            MapearAlControl.ListaDeElementos(SelectorUnidadProveedor, new Array<ClausulaDeFiltrado>(), idUnidadDelProveedor, null);
                        }
                        let conceptoDelProveedor = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.ConceptoDelProveedor, '');
                        if (!IsNullOrEmpty(conceptoDelProveedor)) {
                            let conceptoCtrl = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.concepto) as HTMLInputElement;
                            AsignarValor(conceptoCtrl, conceptoDelProveedor);
                        }

                        // Tarifa propuesta: la base imponible del proveedor (los pedidos van sin iva)
                        let biPropuestoDelProveedor = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.BiPropuestoDelProveedor, 0));
                        if (biPropuestoDelProveedor > 0) {
                            let precioCtrl = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
                            AsignarValor(precioCtrl, biPropuestoDelProveedor.toString());
                        }
                    }
                    break;
                }
                case 2: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.unitario);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                    ocultar = true;
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${tipoDeLinea.selectedIndex} a la modal de crear una línea de un pedido`);
                    break;
                }
            }
            var postFijo = this.EstaCreandoUnaLinea ? 'nuevo' : 'edicion';
            var filaDeLaModal: string = 'table-lineadeunpedidodto';
            if (ocultar) {
                document.getElementById(`${filaDeLaModal}-${postFijo}-3`).classList.add(ltrCss.divNoVisible);
                document.getElementById(`${filaDeLaModal}-${postFijo}-4`).classList.add(ltrCss.divNoVisible);
                document.getElementById(`${filaDeLaModal}-${postFijo}-5`).classList.add(ltrCss.divNoVisible);
                document.getElementById(`${filaDeLaModal}-${postFijo}-6`).classList.add(ltrCss.divNoVisible);
            }
            else {
                document.getElementById(`${filaDeLaModal}-${postFijo}-3`).classList.remove(ltrCss.divNoVisible);
                document.getElementById(`${filaDeLaModal}-${postFijo}-4`).classList.remove(ltrCss.divNoVisible);
                document.getElementById(`${filaDeLaModal}-${postFijo}-5`).classList.remove(ltrCss.divNoVisible);
                document.getElementById(`${filaDeLaModal}-${postFijo}-6`).classList.remove(ltrCss.divNoVisible);
            }

            this.pedido_CalcularImportesDeLinea_interno(modal);
        }

        public pedido_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.cantidad).value);
            let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.precio).value);

            let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.ImporteSinDto) as HTMLInputElement;
            let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.ImporteDeDto) as HTMLInputElement;
            let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.ImporteDeLinea) as HTMLInputElement;

            let importeSinDescuento: number = cantidad * precio;
            if (importeSinDescuento > 0) {
                let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.descuentoPorLinea).value);
                AsignarValor(impSinDto, importeSinDescuento.toString());
                AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
                let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);
                AsignarValor(ImporteDeLinea, impConElDto.toString());
            }
            else {
                impSinDto.value = "";
                impDeDto.value = "";
                ImporteDeLinea.value = "";
            }
        }

    }


    export function Ped_Tras_Seleccionar_Proveedor(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        const proveedor = OpcionesDeLasListas.ObtenerObjeto(lista);

        if (crudDePedidos.EstoyEditandoConsultando || crudDePedidos.EstoyCreando) {
            var idSeleccionado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
            ApiControl.BloquearListaDinamicaSi(
                crudDePedidos.EstoyEditandoConsultando ? crudDePedidos.crudDeEdicion.PanelDelDto : crudDePedidos.crudDeCreacion.PanelDeCrear,
                ltrPropiedades.Logistica.Pedido.Contrato,
                idSeleccionado === 0);

            if (!Definido(proveedor) && idSeleccionado > 0) {

                ApiDePeticiones.LeerElementoPorId(crudDePedidos, ltrControladores.Terceros.Proveedores, idSeleccionado, new Array<Parametro>(), idSeleccionado)
                    .then((peticion) => {
                        AplicarDatosDeUnProveedor(peticion.resultado.datos);
                    })
                    .catch((peticion) => {
                        ApiDePeticiones.EmitirError(peticion);
                    });
            }
            else AplicarDatosDeUnProveedor(proveedor);


            return;
        }

        if (crudDePedidos.EstoyEnMantenimiento)
            return;

        MensajesSe.EmitirExcepcion('Ped_Tras_Seleccionar_Proveedor', 'No se ha definido donde afectar la selección del proveedor');
    }

    export function Ped_Tras_Blanquear_Proveedor(idLista: string): void {

        if (crudDePedidos.EstoyEditandoConsultando) {
            ApiControl.BloquearListaDinamicaPorPropiedad(crudDePedidos.crudDeEdicion.PanelDelDto, ltrPropiedades.Logistica.Pedido.Contrato);
        }
        else if (crudDePedidos.ModoTrabajo === enumModoTrabajo.creando) {
            ApiControl.BloquearListaDinamicaPorPropiedad(crudDePedidos.crudDeCreacion.PanelDeCrear, ltrPropiedades.Logistica.Pedido.Contrato);
            ApiDelCrud.QuitarResaltos(crudDePedidos.crudDeCreacion.PanelDeCrear, ltrCss.Resalto.Verde);
        }
        else
            MensajesSe.EmitirExcepcion('Ped_Tras_Blanquear_Proveedor', 'No se ha definido donde afectar la selección del proveedor');
    }


    function AplicarDatosDeUnProveedor(proveedor: any) {

        if (crudDePedidos.EstoyCreando && crudDePedidos.crudDeCreacion.MapeandoPlantilla) {
            return;
        }

        crudDePedidos.Naturaleza = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.IdNaturaleza, undefined, false);
        crudDePedidos.Concepto = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.Concepto, undefined, false);
        if (crudDePedidos.EstoyCreando) {
            const creador = (crudDePedidos.crudDeCreacion as CrudCreacionPedido);
            crudDePedidos.crudDeCreacion.ControlDeNombre.value = crudDePedidos.Concepto;

            ApiControl.ResaltarControl(crudDePedidos.crudDeCreacion.ControlDeNombre, ltrCss.Resalto.Verde);
            if (crudDePedidos.Naturaleza > 0) {
                var SelectorNaturaleza = ApiControl.BuscarListaDeElementos(creador.PanelDeCrear, ltrPropiedades.Logistica.Pedido.SelectorNaturaleza);
                MapearAlControl.ListaDeElementos(SelectorNaturaleza, new Array<ClausulaDeFiltrado>(), crudDePedidos.Naturaleza, null);
                ApiControl.ResaltarControl(ApiControl.BuscarListaDeElementos(creador.PanelDeCrear, ltrPropiedades.Logistica.Pedido.SelectorNaturaleza), ltrCss.Resalto.Verde);
                Ped_Tras_Cambiar_Naturaleza_Del_Detalle();
            }

            const idCg: number = Numero(ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.idcgPropuesto, 0, false));
            if (idCg > 0) {
                const cg: string = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.cgPropuesto, '', false);
                ApiListaDinamica.AsignarValorConResaltado(creador.Cg, idCg, cg, ltrCss.Resalto.Verde);
            }

            const idTipo: number = Numero(ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.idtipoPropuesto, 0, false));
            if (idTipo > 0) {
                const tipo: string = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.tipoPropuesto, '', false);
                ApiListaDinamica.AsignarValorConResaltado(creador.Tipo, idTipo, tipo, ltrCss.Resalto.Verde);
            }

            const bi: number = Numero(ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.biPropuesto, 0, false));
        }
    }


}