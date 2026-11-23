namespace Logistica {

    enum enumEtapasDeVenta {
        DSA_Etapa_Solicitud,
        DSA_Etapa_Servicio,
        DSA_Etapa_Facturacion,
        DSA_Etapa_Cancelacion
    }

    export function CrearCrudDeVentas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDeVentas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeVentas extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionVenta(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionVenta(this, idPanelEdicion);
        }

        public get ModalDevolverLinea(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Ventas.DevolverLinea) as HTMLDivElement; }

        // Línea vendida que se está devolviendo, leída del grid de líneas
        private _lineaADevolver: LineaDelGridDeVenta = undefined;

        private get CantidadPendienteDeDevolver(): number {
            const linea = this._lineaADevolver;
            return Definido(linea) ? linea.cantidad - linea.cantidadDevuelta : 0;
        }

        // Se propone lo que queda por devolver (lo vendido menos lo ya devuelto) al precio y descuento al que se vendió, y se cargan las
        // devoluciones en tramitación del cliente; si no se elige ninguna, al aceptar se crea una nueva
        public AbrirModalDeDevolverLinea(linea: LineaDelGridDeVenta) {
            const editor = this.crudDeEdicion;
            const modal = this.ModalDevolverLinea;
            const propiedades = ltrPropiedades.Logistica.Venta.DevolverLinea;
            this._lineaADevolver = linea;

            editor.Expansor_AbrirModalParaPedirDatos(modal.id, editor.ElementoEditado.Id, undefined, new Array<string>(), false);
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Cantidad), this.CantidadPendienteDeDevolver);
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Precio), linea.precio);
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Descuento), linea.descuento);
            this.CalcularImporteADevolver();

            const devoluciones = ApiControl.BuscarListaDeElementos(modal, propiedades.Devolucion);
            if (Definido(devoluciones)) {
                const filtros = new Array<ClausulaDeFiltrado>();
                filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Logistica.DevolucionDeVenta.FiltroEnTramitacion, atCriterio.igual, true));
                filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Logistica.DevolucionDeVenta.FiltroPorCliente, atCriterio.igual,
                    Numero(ObtenerPropiedad(editor.Registro, ltrPropiedades.Logistica.Venta.IdCliente, 0, false))));
                ApiDeInicializacion.RecargarListaDeElementos(devoluciones, filtros, 0);
            }
            ApiControl.BuscarEditor(modal, propiedades.Cantidad).focus();
        }

        public CalcularImporteADevolver() {
            const modal = this.ModalDevolverLinea;
            const propiedades = ltrPropiedades.Logistica.Venta.DevolverLinea;
            const cantidad = Numero(ApiControl.BuscarEditor(modal, propiedades.Cantidad).value);
            const precio = Numero(ApiControl.BuscarEditor(modal, propiedades.Precio).value);
            const descuento = Numero(ApiControl.BuscarEditor(modal, propiedades.Descuento).value);
            const importe = ApiControl.BuscarEditor(modal, propiedades.Importe);
            const importeDevuelto = cantidad * precio * (1 - descuento / 100);
            if (importeDevuelto > 0) AsignarValor(importe, importeDevuelto);
            else importe.value = "";
        }

        // Nunca se devuelve más de lo que queda por devolver, el servidor lo vuelve a validar (ver ExtensorDeDevolucionesDeVenta.ValidarLinea)
        private DevolverLinea(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionVenta;
            const linea = this._lineaADevolver;
            const propiedades = ltrPropiedades.Logistica.Venta.DevolverLinea;
            if (!Definido(linea))
                return;

            const cantidad = Numero(ApiControl.BuscarEditor(modal, propiedades.Cantidad).value);
            const pendiente = this.CantidadPendienteDeDevolver;
            if (!(cantidad > 0)) {
                MensajesSe.Error('Devolución', 'Indique la cantidad a devolver');
                return;
            }
            if (cantidad > pendiente) {
                MensajesSe.Error('Devolución', `Como mucho se pueden devolver ${pendiente}: se vendieron ${linea.cantidad} y ya se han devuelto ${linea.cantidadDevuelta}`);
                return;
            }

            const devoluciones = ApiControl.BuscarListaDeElementos(modal, propiedades.Devolucion);
            const idDevolucion = Definido(devoluciones) ? Numero(devoluciones.value) : 0;

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(propiedades.IdLineaDeVenta, linea.id));
            parametros.push(new Parametro(propiedades.Cantidad, cantidad));
            parametros.push(new Parametro(propiedades.IdDevolucion, idDevolucion > 0 ? idDevolucion : 0));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.DevolucionesDeVenta, Ajax.EndPoint.Logistica.DevolucionesDeVenta.DevolverLineaDeVenta, parametros, new Array<Parametro>())
                .then((peticion) => {
                    MensajesSe.Info(peticion.resultado.consola);
                    super.ModalDePedirDatos_Cerrar(modal);
                    this._lineaADevolver = undefined;
                    editor.RecargarGridDeLineas();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        //------------------------------------------------------------------------------- modificar lo servido (documento de venta servido)

        public get ModalModificarLineaServida(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Ventas.ModificarLineaServida) as HTMLDivElement; }

        // Línea servida que se está modificando, leída del grid de líneas
        private _lineaAModificar: LineaDelGridDeVenta = undefined;

        // Se propone lo realmente servido (lo servido menos lo ya devuelto) y el precio de venta de la línea; el descuento de la línea se usa
        // para calcular el precio total
        public AbrirModalDeModificarLineaServida(linea: LineaDelGridDeVenta) {
            const editor = this.crudDeEdicion;
            const modal = this.ModalModificarLineaServida;
            const propiedades = ltrPropiedades.Logistica.Venta.ModificarLineaServida;
            this._lineaAModificar = linea;

            editor.Expansor_AbrirModalParaPedirDatos(modal.id, editor.ElementoEditado.Id, undefined, new Array<string>(), false);
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Cantidad), linea.cantidad - linea.cantidadDevuelta);
            AsignarValor(ApiControl.BuscarEditor(modal, propiedades.Precio), linea.precio);
            this.CalcularImporteServido();
            ApiControl.BuscarEditor(modal, propiedades.Cantidad).focus();
        }

        public CalcularImporteServido() {
            const modal = this.ModalModificarLineaServida;
            const propiedades = ltrPropiedades.Logistica.Venta.ModificarLineaServida;
            const cantidad = Numero(ApiControl.BuscarEditor(modal, propiedades.Cantidad).value);
            const precio = Numero(ApiControl.BuscarEditor(modal, propiedades.Precio).value);
            const descuento = Definido(this._lineaAModificar) ? this._lineaAModificar.descuento : 0;
            const importe = ApiControl.BuscarEditor(modal, propiedades.Importe);
            const total = cantidad * precio * (1 - descuento / 100);
            if (total > 0) AsignarValor(importe, total);
            else importe.value = "";
        }

        // La cantidad es lo realmente servido; si la salida está en un periodo cerrado el servidor crea una devolución con lo que se deja de
        // servir (ver ExtensorDeVentas.ModificarLineaServida), que se acepta en el grid de devoluciones
        private ModificarLineaServida(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionVenta;
            const linea = this._lineaAModificar;
            const propiedades = ltrPropiedades.Logistica.Venta.ModificarLineaServida;
            if (!Definido(linea))
                return;

            const cantidad = Numero(ApiControl.BuscarEditor(modal, propiedades.Cantidad).value);
            const precio = Numero(ApiControl.BuscarEditor(modal, propiedades.Precio).value);
            if (!(cantidad > 0)) {
                MensajesSe.Error('Modificar lo servido', 'Indique la cantidad servida');
                return;
            }
            if (precio < 0) {
                MensajesSe.Error('Modificar lo servido', 'El precio de venta no puede ser negativo');
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(propiedades.IdLineaDeVenta, linea.id));
            parametros.push(new Parametro(propiedades.Cantidad, cantidad));
            parametros.push(new Parametro(propiedades.Precio, precio));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.Ventas, Ajax.EndPoint.Logistica.Ventas.ModificarLineaServida, parametros, new Array<Parametro>())
                .then((peticion) => {
                    MensajesSe.Info(peticion.resultado.consola);
                    super.ModalDePedirDatos_Cerrar(modal);
                    this._lineaAModificar = undefined;
                    editor.RecargarTrasModificarLoServido();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public get ModalCambiarDatos(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Logistica.Ventas.CambiarDatos) as HTMLDivElement; }

        // "Cambiar datos": se proponen el nombre, el cliente y la fecha de servicio del documento de venta editado
        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id !== this.ModalCambiarDatos.id)
                return;
            const editor = this.crudDeEdicion as CrudEdicionVenta;
            const propiedades = ltrPropiedades.Logistica.Venta.CambiarDatos;
            ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, propiedades.Nombre, false);
            ApiPanel.MapearListaDinamica(modal, editor.Registro, propiedades.Cliente, false, true);
            ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, propiedades.ServidoEl, false);
        }

        // Al aceptar se cambian los datos en el servidor (si está servido se rehacen las salidas del almacén y, si la oferta estaba impresa,
        // se reimprime) y se recargan la cabecera, las líneas y los archivos
        private CambiarDatos(modal: HTMLDivElement) {
            const editor = this.crudDeEdicion as CrudEdicionVenta;
            const propiedades = ltrPropiedades.Logistica.Venta.CambiarDatos;
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
            parametros = parametros.filter(p => p.parametro !== propiedades.IdElemento);
            parametros.push(new Parametro(propiedades.IdElemento, editor.ElementoEditado.Id));

            ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.Ventas, Ajax.EndPoint.Logistica.Ventas.CambiarDatos, parametros, new Array<Parametro>())
                .then((peticion) => {
                    MensajesSe.Info(peticion.resultado.consola);
                    super.ModalDePedirDatos_Cerrar(modal);
                    editor.RecargarTrasModificarLoServido();
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id === this.ModalCambiarDatos.id) {
                this.CambiarDatos(modal);
                return;
            }
            if (modal.id === this.ModalDevolverLinea.id) {
                this.DevolverLinea(modal);
                return;
            }
            if (modal.id === this.ModalModificarLineaServida.id) {
                this.ModificarLineaServida(modal);
                return;
            }
            super.ModalDePedirDatos_Aceptar(modal);
        }

    }

    // Datos de una línea del grid de líneas del documento de venta, para devolverla
    export interface LineaDelGridDeVenta {
        id: number;
        cantidad: number;
        precio: number;
        descuento: number;
        cantidadDevuelta: number;
    }

    function dsa_LeerLineaDelGrid_interno(parametros: string): LineaDelGridDeVenta {
        const partes = parametros.split(';');
        const idDeLaFila = `${partes[0].replace('-contenedor', '-tabla_d_tr')}_${Numero(partes[1])}`;
        const leer = (propiedad: string) => ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, propiedad);
        const aNumero = (texto: string) => IsNullOrEmpty(texto) ? 0 : Numero(texto);
        const propiedades = ltrPropiedades.Logistica.Venta.linea;
        return {
            id: Numero(leer(literal.id)),
            cantidad: aNumero(leer(propiedades.cantidad)),
            precio: aNumero(leer(propiedades.precio)),
            descuento: aNumero(leer(propiedades.descuento)),
            cantidadDevuelta: aNumero(leer(propiedades.cantidadDevuelta))
        };
    }

    // Acción Devolver de las líneas (sólo se ve con el documento de venta servido o facturado, ver CrudEdicionVenta.AjustarLineasSegunEtapa):
    //  - servido, aún sin prefacturar: se modifica lo servido (ver CrudDeVentas.AbrirModalDeModificarLineaServida)
    //  - facturado: se devuelve si queda algo por devolver (ver CrudDeVentas.AbrirModalDeDevolverLinea)
    // El servidor lo vuelve a validar
    export function Dsa_DevolverLinea(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionVenta;
        const linea = dsa_LeerLineaDelGrid_interno(parametros);
        if (editor.EstaServido) {
            (Crud.crudMnt as CrudDeVentas).AbrirModalDeModificarLineaServida(linea);
            return;
        }
        if (!editor.EstaFacturado) {
            MensajesSe.Error('Devolución', 'Sólo se pueden devolver las líneas de documentos de venta servidos o facturados');
            return;
        }
        if (linea.cantidad - linea.cantidadDevuelta <= 0) {
            MensajesSe.Error('Devolución', 'La línea ya se ha devuelto por completo');
            return;
        }
        (Crud.crudMnt as CrudDeVentas).AbrirModalDeDevolverLinea(linea);
    }

    export function Dsa_CalcularImporteADevolver() {
        (Crud.crudMnt as CrudDeVentas).CalcularImporteADevolver();
    }

    export function Dsa_CalcularImporteServido() {
        (Crud.crudMnt as CrudDeVentas).CalcularImporteServido();
    }

    // Check de aceptar del grid de devoluciones del documento de venta: acepta la devolución de la fila si aún está solicitada (ver
    // DevolucionesDeVentaController.epAceptarDevolucion) y recarga el documento de venta
    export function Dsa_AceptarDevolucion(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionVenta;
        const partes = parametros.split(';');
        const idDeLaFila = `${partes[0].replace('-contenedor', '-tabla_d_tr')}_${Numero(partes[1])}`;
        const propiedades = ltrPropiedades.Logistica.Venta.Devolucion;
        const leer = (propiedad: string) => ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, propiedad);

        if (EsTrue(leer(propiedades.aceptada))) {
            MensajesSe.Info(`La devolución '${leer(propiedades.devolucion)}' ya está aceptada`);
            return;
        }
        if (EsTrue(leer(propiedades.cancelada))) {
            MensajesSe.Info(`La devolución '${leer(propiedades.devolucion)}' está cancelada`);
            return;
        }

        let datos: Array<Parametro> = new Array<Parametro>();
        datos.push(new Parametro(propiedades.IdDevolucion, Numero(leer(propiedades.idElemento))));
        ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.DevolucionesDeVenta, Ajax.EndPoint.Logistica.DevolucionesDeVenta.AceptarDevolucion, datos, new Array<Parametro>())
            .then((peticion) => {
                MensajesSe.Info(peticion.resultado.consola);
                editor.RecargarTrasAceptarDevolucion();
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    // Cancelar de una fila del grid de devoluciones de la edición del documento de venta: en lugar de borrar la devolución se cancela
    // (DDV: Cancelar), sólo si se está tramitando; tras cancelarla se recargan la cabecera y los grids (lo cancelado no cuenta como devuelto)
    export function Dsa_CancelarDevolucion(parametros: string): void {
        const editor = Crud.crudMnt.crudDeEdicion as CrudEdicionVenta;
        const partes = parametros.split(';');
        const idDeLaFila = `${partes[0].replace('-contenedor', '-tabla_d_tr')}_${Numero(partes[1])}`;
        const propiedades = ltrPropiedades.Logistica.Venta.Devolucion;
        const leer = (propiedad: string) => ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, propiedad);
        const referencia = leer(propiedades.devolucion);

        if (EsTrue(leer(propiedades.aceptada)) || EsTrue(leer(propiedades.cancelada))) {
            MensajesSe.Info(`La devolución '${referencia}' ya está ${EsTrue(leer(propiedades.aceptada)) ? 'aceptada' : 'cancelada'}, no se puede cancelar`);
            return;
        }

        let datos: Array<Parametro> = new Array<Parametro>();
        datos.push(new Parametro(propiedades.IdDevolucion, Numero(leer(propiedades.idElemento))));
        ApiDePeticiones.EjecutarPeticion(editor, ltrControladores.Logistica.DevolucionesDeVenta, Ajax.EndPoint.Logistica.DevolucionesDeVenta.CancelarDevolucion, datos, new Array<Parametro>())
            .then((peticion) => {
                MensajesSe.Info(peticion.resultado.consola);
                editor.RecargarTrasAceptarDevolucion();
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export class CrudCreacionVenta extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionVenta extends Crud.CrudEdicion {

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Logistica.Ventas.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Logistica.Ventas.Lineas);
        }

        public get GridDeLineas(): HTMLDivElement {
            return document.getElementById('grid-de-detalle-lineasdeunaventa-tabla') as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get IdReferenciaCrearLinea(): string {
            return this._idPanelEdicion + `-lineasdeunaventa-mcr-${enumPostfijoControl.Referencia}`;
        }

        // Al crear una línea y seguir creando (sin cerrar la modal), el grid de detalle se vacía
        // de forma síncrona y se recarga por ajax; en ese instante el grid está vacío por lo que no
        // sirve para calcular el siguiente orden. Se recuerda aquí el último orden propuesto.
        private _ultimoOrdenPropuesto: number = 0;

        public get UltimoOrdenPropuesto(): number {
            return this._ultimoOrdenPropuesto;
        }
        public set UltimoOrdenPropuesto(valor: number) {
            this._ultimoOrdenPropuesto = valor;
        }

        private get _estaSolicitado(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeVenta, enumEtapasDeVenta.DSA_Etapa_Solicitud);
        }

        public get EstaServido(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeVenta, enumEtapasDeVenta.DSA_Etapa_Servicio);
        }

        public get EstaFacturado(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeVenta, enumEtapasDeVenta.DSA_Etapa_Facturacion);
        }

        // Tras devolver una línea se recargan el grid de líneas, que muestra lo ya devuelto, y el de devoluciones
        public RecargarGridDeLineas() {
            this.RecargarGrid(ltrEspanes.Logistica.Ventas.Lineas);
            this.RecargarGrid(ltrEspanes.Logistica.Ventas.Devoluciones);
        }

        private RecargarGrid(nombre: string) {
            const grid = document.getElementById(this.IdGridDelExpansor(nombre)) as HTMLDivElement;
            if (Definido(grid))
                this.RecargarGridDeRelacion(grid, this.CrudDeMnt.IdNegocio, this.ElementoEditado.Id);
        }

        // Tras modificar lo servido cambian la línea, el total de la cabecera (y su versión), las devoluciones (si la salida estaba en un
        // periodo cerrado) y, si se reimprimió la oferta, los archivos
        public RecargarTrasModificarLoServido() {
            this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
            this.RecargarGridDeLineas();
            this.RecargarArchivos();
        }

        // Tras aceptar una devolución cambian las devoluciones, lo devuelto de las líneas y, si estaba facturado, la factura del documento de
        // venta (pasa a la prefactura de sustitución)
        public RecargarTrasAceptarDevolucion() {
            this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
            this.RecargarGridDeLineas();
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public CargaCompletada() {
            super.CargaCompletada();
            this.AjustarLineasSegunEtapa();
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            this.AjustarCambiarDatos();
        }

        // "Cambiar datos" sólo en la etapa de solicitud, o en la de servicio si no tiene devoluciones (el servidor valida además la
        // prefactura, los preasientos contabilizados, los cierres contables y el permiso de gestor)
        private AjustarCambiarDatos() {
            const propiedades = ltrPropiedades.Logistica.Venta.CambiarDatos;
            const tieneDevoluciones = EsTrue(ObtenerPropiedad(this.Registro, propiedades.tieneDevoluciones, false));
            const sePuede = this._estaSolicitado || (this.EstaServido && !tieneDevoluciones);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Logistica.Ventas.CambiarDatos, ltrMenus.enumOrigen.edicion, sePuede);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            const opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Logistica.Ventas.CambiarDatos) {
                const idModal = this.CrudDeMnt.IdCrud + '-' + opcion;
                this.Expansor_AbrirModalParaPedirDatos(idModal, this.ElementoEditado.Id);
                return true;
            }
            return false;
        }

        public Expansor_TrasCargarDetalle(idGrid: string) {
            super.Expansor_TrasCargarDetalle(idGrid);
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Ventas.Lineas) === idGrid)
                this.AjustarLineasSegunEtapa();
            if (this.IdGridDelExpansor(ltrEspanes.Logistica.Ventas.Devoluciones) === idGrid)
                this.AjustarGridDeDevoluciones(idGrid);
        }

        // Aceptar y cancelar sólo están activos en las devoluciones solicitadas (en tramitación); en las aceptadas (estén o no ya facturadas) y en
        // las canceladas aparecen deshabilitados, en un verde apagado. Los
        // permisos los valida el servidor al transitar la devolución
        private AjustarGridDeDevoluciones(idGrid: string) {
            const grid = document.getElementById(idGrid);
            const tbody = Definido(grid) ? grid.querySelector('.' + ltrCss.crud.tbody) : null;
            if (!Definido(tbody))
                return;
            const propiedades = ltrPropiedades.Logistica.Venta.Devolucion;
            // aceptar y cancelar sólo mientras la devolución se tramita (ni aceptada ni cancelada)
            const valor = (fila: HTMLDivElement, propiedad: string): boolean => {
                const input = fila.querySelector<HTMLInputElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}='${propiedad}'] input`);
                return Definido(input) && input.value.toLowerCase() === 'true';
            };
            tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila).forEach(fila => {
                const aceptada = valor(fila, propiedades.aceptada);
                const cancelada = valor(fila, propiedades.cancelada);
                const enTramitacion = !aceptada && !cancelada;
                [propiedades.Aceptar, propiedades.Cancelar].forEach(accion => {
                    const celda = fila.querySelector<HTMLDivElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}='${accion}']`);
                    const a = Definido(celda) ? celda.querySelector<HTMLAnchorElement>('a') : null;
                    if (!Definido(a))
                        return;
                    if (enTramitacion) {
                        ApiControl.ExcluirCss(a, ltrCss.Detalle.AccionDeshabilitada);
                        celda.title = accion === propiedades.Aceptar ? 'Aceptar la devolución' : 'Cancelar la devolución';
                    }
                    else {
                        // la celda lleva el title: el enlace deshabilitado no recibe el ratón (pointer-events: none)
                        ApiControl.IncluirCss(a, ltrCss.Detalle.AccionDeshabilitada);
                        celda.title = aceptada ? 'La devolución ya está aceptada' : 'La devolución está cancelada';
                    }
                });
            });
        }

        // Las líneas sólo se pueden modificar mientras el documento de venta está solicitado
        private AjustarLineasSegunEtapa() {
            const idGrid = this.IdGridDelExpansor(ltrEspanes.Logistica.Ventas.Lineas);
            const puedeModificarLineas = this._estaSolicitado && ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso);

            if (puedeModificarLineas)
                ApiDeGrid.Expansor_PonerEnEdicion(idGrid);
            else
                ApiDeGrid.Expansor_PonerEnConsulta(idGrid);

            ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearLinea, puedeModificarLineas);
            this.MostrarDevolverSegunEtapa(idGrid);
        }

        // El icono de devolver sólo con el documento de venta servido o facturado: en la solicitud está la papelera de borrar la línea
        private MostrarDevolverSegunEtapa(idGrid: string) {
            const grid = document.getElementById(idGrid);
            if (!Definido(grid))
                return;
            const visible = this.EstaServido || this.EstaFacturado;
            grid.querySelectorAll<HTMLAnchorElement>(`.${ltrCss.crud.celda}[${atControl.propiedad}="Devolver"] a`)
                .forEach(a => a.style.display = visible ? '' : 'none');
        }

    }
}
