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

        // Línea que se está recepcionando, con la versión que se leyó en el grid para validarla en el servidor
        private _lineaARecepcionar: LineaDelGridDePedido = undefined;

        private get ListaDeAlmacenesDeLaRecepcion(): HTMLSelectElement {
            return ApiControl.BuscarListaDeElementos(this.ModalRecepcionarLinea, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Almacen);
        }

        private get ListaDeExpedientesDeLaRecepcion(): HTMLInputElement {
            return ApiControl.BuscarListaDinamicaPorPropiedad(this.ModalRecepcionarLinea, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Expediente);
        }

        private get BotonAceptarDeLaRecepcion(): HTMLInputElement {
            return document.getElementById(`${this.ModalRecepcionarLinea.id}-aceptar`) as HTMLInputElement;
        }

        // Sin entrega se propone lo pedido en la línea y el destino de la cabecera, el usuario indica lo que realmente se ha entregado;
        // con entrega se muestra lo entregado en consulta, sin poder aceptar
        public AbrirModalDeRecepcionarLinea(linea: LineaDelGridDePedido) {
            const modal = this.ModalRecepcionarLinea;
            this._lineaARecepcionar = linea;

            this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(modal.id, this.crudDeEdicion.ElementoEditado.Id, undefined, new Array<string>(), false);

            if (linea.entregada) {
                this.MapearDatosDeLaRecepcion(linea.cantidadRecibida, linea.precioDeRecepcion, linea.descuentoAplicado);
                this.MapearDestinoDeLaRecepcion(linea.idAlmacenDeRecepcion, linea.idExpedienteDeRecepcion, linea.expedienteDeRecepcion);
            }
            else {
                const cabecera = this.crudDeEdicion.Registro;
                let idAlmacen = Numero(ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.IdAlmacen, 0, false));
                let idExpediente = Numero(ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.idExpediente, 0, false));
                let expediente = ObtenerPropiedad(cabecera, ltrPropiedades.Logistica.Pedido.Expediente, '', false);
                // el destino por defecto es el almacén de la cabecera y, si no lo tiene, la obra de su expediente (ver ExtensorDePedidos.DestinoDeLasEntregas)
                if (idAlmacen > 0) idExpediente = 0;
                this.MapearDatosDeLaRecepcion(linea.cantidad, linea.precio, linea.descuento);
                this.MapearDestinoDeLaRecepcion(idAlmacen, idExpediente, expediente);
            }
            this.BloquearRecepcion(linea.entregada);
            if (!linea.entregada)
                ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad).focus();
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

        private MapearDestinoDeLaRecepcion(idAlmacen: number, idExpediente: number, expediente: string) {
            const almacenes = this.ListaDeAlmacenesDeLaRecepcion;
            const expedientes = this.ListaDeExpedientesDeLaRecepcion;
            if (idAlmacen > 0) MapearAlControl.ListaDeElementos(almacenes, new Array<ClausulaDeFiltrado>(), idAlmacen, null);
            else almacenes.selectedIndex = 0;
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
            if (bloquear) {
                ApiControl.BloquearListaDeElemento(modal, propiedades.Almacen);
                ApiControl.BloquearListaDinamicaPorPropiedad(modal, propiedades.Expediente);
            }
            else {
                ApiControl.DesbloquearListaDeElemento(modal, propiedades.Almacen);
                ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, propiedades.Expediente);
            }
            let aceptar = this.BotonAceptarDeLaRecepcion;
            if (Definido(aceptar)) aceptar.disabled = bloquear;
        }

        // La entrega va al almacén o a la obra del expediente, nunca a ambos: al elegir uno se blanquea el otro
        public Recepcion_TrasCambiarAlmacen() {
            if (Numero(this.ListaDeAlmacenesDeLaRecepcion.value) > 0)
                ApiListaDinamica.Blanquear(this.ListaDeExpedientesDeLaRecepcion);
        }

        public Recepcion_TrasSeleccionarExpediente() {
            if (Numero(this.ListaDeExpedientesDeLaRecepcion.getAttribute(atListasDinamicas.idSeleccionado)) > 0)
                this.ListaDeAlmacenesDeLaRecepcion.selectedIndex = 0;
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

        private RecepcionarLinea(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionPedido;
            const linea = this._lineaARecepcionar;
            if (!Definido(linea) || linea.entregada)
                return;

            let idAlmacen = Numero(this.ListaDeAlmacenesDeLaRecepcion.value);
            let idExpediente = Numero(this.ListaDeExpedientesDeLaRecepcion.getAttribute(atListasDinamicas.idSeleccionado));
            if ((idAlmacen > 0) === (idExpediente > 0)) {
                MensajesSe.Error('Recepción', 'Indique el almacén o el expediente al que va la entrega, y sólo uno de ellos');
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdLineaPedido, linea.id));
            // con el RowVersion de la línea, IncluirRowId no añade el del pedido
            parametros.push(new Parametro(Ajax.Param.RowVersion, linea.rowVersion));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdAlmacen, idAlmacen > 0 ? idAlmacen : 0));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.IdExpediente, idExpediente > 0 ? idExpediente : 0));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad, Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Cantidad).value)));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio, Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Precio).value)));
            parametros.push(new Parametro(ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento, Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.RecepcionarLinea.Descuento).value)));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.LineasDeUnPedido, Ajax.EndPoint.Logistica.LineasDeUnPedido.RecibirLinea, parametros, new Array<Parametro>())
                .then((peticion) => {
                    MensajesSe.Info(peticion.resultado.consola);
                    super.ModalDePedirDatos_Cerrar(modal);
                    editor.RecargarGridDeLineas();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id === this.ModalRecepcionarLinea.id) {
                this.RecepcionarLinea(modal);
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

        public TrasSeleccionarCg(idLista: string): void {
            super.TrasSeleccionarCg(idLista);
            this.LeerDireccionDeEntrega();
        }

        public TrasBlanquearCg() {
            super.TrasBlanquearCg();
            ApiPanel.BlanquearControlesDeIU(this.PanelDeDireccion);
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
            let grid = document.getElementById(this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Lineas)) as HTMLDivElement;
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
            let seEstaCumplimentandoOAprobando =
                seEstaCumplimentando ||
                EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Aprobacion);
            this.AplicarEtiquetaDeLaFechaDePedido(panel, !seEstaCumplimentandoOAprobando);

            // El fichero de pedido solo se puede añadir al crear o, en edición, mientras se está cumplimentando
            ApiControl.MostrarPropiedadSi(panel, ltrPropiedades.Logistica.Pedido.IdArchivoPedido, seEstaCumplimentando);

            // Fuera de cumplimentación la cabecera es de consulta, el interventor la corrige con "Modificar cabecera" mientras no esté anulado
            let estaAnulado = EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_Cancelado);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Logistica.Pedidos.ModificarCabecera, ltrMenus.enumOrigen.edicion, this.EsInterventorSinEstado && !estaAnulado);

            // En recepción el servidor mantiene el permiso sobre el pedido para poder recepcionar las líneas, pero la cabecera es de consulta
            if (EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Recepcion))
                ApiPanel.PonerEnModoConsulta(this.PanelDelDto);

            this.AplicarEtapaAlGridDeLineas();
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
        }

        public get EstaEnRecepcion(): boolean {
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            return EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Recepcion);
        }

        // Entrega parcial (Recepcionar) y total (Recibida): con el pedido solicitado o en recepción y si la línea aún no se ha entregado
        public SePuedeEntregarLaLinea(entregada: boolean): boolean {
            return this.SePuedeRecepcionar && !entregada;
        }

        // Aspa: sin entrega marca la línea como no entregada (pedido solicitado o en recepción);
        // con entrega la cancela, sólo con el pedido en recepción
        public SePuedeCancelarLaEntrega(entregada: boolean): boolean {
            return entregada ? this.EstaEnRecepcion : this.SePuedeRecepcionar;
        }

        // Recepcionar con entrega la muestra en consulta, sin ella abre la modal para entregarla
        public SePuedeAbrirLaRecepcion(entregada: boolean): boolean {
            return entregada || this.SePuedeEntregarLaLinea(entregada);
        }

        private static readonly AccionDeRecepcionar = 'Recepcionar';
        private static readonly AccionDeEntregaCompleta = 'Recibida';
        private static readonly AccionDeCancelarLaEntrega = 'NoEntregada';

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

            let tbody = grid.querySelector('.' + ltrCss.crud.tbody);
            if (!Definido(tbody)) return;
            tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila).forEach(fila => {
                const entregada = CrudEdicionPedido.LaLineaEstaEntregada(fila);
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeRecepcionar, this.SePuedeAbrirLaRecepcion(entregada));
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeEntregaCompleta, this.SePuedeEntregarLaLinea(entregada));
                CrudEdicionPedido.HabilitarAccionDeLaFila(fila, CrudEdicionPedido.AccionDeCancelarLaEntrega, this.SePuedeCancelarLaEntrega(entregada));
            });
        }

        private static LaLineaEstaEntregada(fila: HTMLDivElement): boolean {
            const propiedad = ltrPropiedades.Logistica.Pedido.linea.CantidadRecibida.toLowerCase();
            let cantidadRecibida = fila.querySelector<HTMLInputElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}='${propiedad}'] input`);
            return Definido(cantidadRecibida) && !IsNullOrEmpty(cantidadRecibida.value);
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