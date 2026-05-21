namespace Venta {

    enum enumEtapasDeFacturaEmt {
        FAE_Etapa_Prefactura,
        FAE_Etapa_Emitida,
        FAE_Etapa_De_Cobro,
        FAE_Etapa_De_Reclamacion,
        FAE_Etapa_No_Cobrable,
        FAE_Etapa_Cobrada,
        FAE_Etapa_Anulada,
        FAE_Etapa_Pago_Parcial,
        FAE_Etapa_Rectificada,
        FAE_Etapa_Remesada,
        FAE_Etapa_Devuelta,
        FAE_Etapa_Abonada,
        FAE_Etapa_Abonable
    }

    export function CrearCrudDeFacturasEmt(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Venta.CrudDeFacturasEmt(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeFacturasEmt extends Crud.CrudMnt {

        public get ModalCambiarVencimiento(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento) as HTMLDivElement; }

        public get ModalCambiarDatos(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarDatos) as HTMLDivElement; }

        public get ModalCopiarFae(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarFae) as HTMLDivElement; }

        public get ModalHacerRectificativa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.FacturasEmt.Rectificativa) as HTMLDivElement; }

        public get ModalFacturarTareas(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.FacturasEmt.FacturarTareas) as HTMLDivElement; }

        private _usaVerifactu: boolean
        public get UsaVerifactu(): boolean {
            return this._usaVerifactu;
        }
        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionFacturaEmt(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionFacturaEmt(this, idPanelEdicion);
        }

        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            this.Expansor_InyectarAccesoIA();
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._usaVerifactu = mapIndicadores.get(ltrPropiedades.Venta.FacturaEmt.Indicadores.UsaVerifactu);
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarFae) {
                let idModal = this.IdCrud + '-' + opcion;
                let id = this.InfoSelector.Seleccionados.length === 0 ? 0 : this.InfoSelector.Seleccionados[0].Id;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, id);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);

            //if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarFae) {
            //    this._faeSeleccionada = this.InfoSelector.Seleccionados.length === 1 ? this.InfoSelector.Seleccionados[0].Registro : null;
            //    let idModal = this.IdCrud + '-' + opcion;
            //    this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, this._faeSeleccionada === null ? 0 : this._faeSeleccionada.id);
            //    return true;
            //}

            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Venta.FacturasEmt);
                return true;
            }

            return Venta.NavegarARelacionesDeFae(opcion, datosDeEntrada, ltrParametrosUrl.Venta.IdFactura);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            const editor = this.crudDeEdicion as CrudEdicionFacturaEmt;
            if (modal.id === this.ModalCambiarVencimiento.id) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Elemento.Elemento);
                let idFactura = ObtenerPropiedad(editor.Registro, literal.id);
                let factura = ObtenerPropiedad(editor.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.ListaDinamica(lista, idFactura, factura, true);
                ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, ltrPropiedades.Venta.FacturaEmt.VenceEl, true);
            }
            else if (modal.id === this.ModalCambiarDatos.id) {
                let idFactura = ObtenerPropiedad(editor.Registro, literal.id);
                let factura = ObtenerPropiedad(editor.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.Elemento.IdElemento, idFactura, factura);

                ApiPanel.MapearListaDinamica(this.ModalCambiarDatos, editor.Registro, ltrPropiedades.Venta.FacturaEmt.Cliente, false, true);
                ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, ltrPropiedades.Venta.FacturaEmt.Contacto, false);
                ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, ltrPropiedades.Venta.FacturaEmt.eMail, false);
                ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, ltrPropiedades.Venta.FacturaEmt.Telefono, false);

                ApiPanel.MapearListaDinamica(this.ModalCambiarDatos, editor.Registro, ltrPropiedades.Venta.FacturaEmt.Contrato, editor.IdParteTr > 0, false);
                ApiPanel.MapearListaDinamica(this.ModalCambiarDatos, editor.Registro, ltrPropiedades.Venta.FacturaEmt.Presupuesto, false, false);
            }            
            else if (modal.id === this.ModalCopiarFae.id) {
                if (this.InfoSelector.Seleccionados.length === 1) {
                    this.Fae_ProponerFacturaParaCopiar(modal, this.InfoSelector.Seleccionados[0].Registro);
                }
            }
            else if (modal.id === this.ModalHacerRectificativa.id || modal.id == this.ModalFacturarTareas.id) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Elemento.Elemento);
                let idFactura = ObtenerPropiedad(editor.Registro, literal.id);
                let factura = ObtenerPropiedad(editor.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.ListaDinamica(lista, idFactura, factura, true);
                if (modal.id === this.ModalHacerRectificativa.id)
                {
                    let clase = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.ClaseRectificativa);
                    MapearAlControl.ListaDeValores(clase, ltrValores.Venta.FacturasEmt.Rectificativas.Clase.Total);
                }
            }
        }

        public Fae_ProponerFacturaParaCopiar(modal: HTMLDivElement, registro: any) {
            let destino: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.expresion));
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConTipo.Tipo, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Venta.FacturaEmt.Cliente, ltrPropiedades.Venta.FacturaEmt.IdCliente);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Elemento.Nombre);
            ApiDelCrud.ProponerEnAreaDeTexto(modal, registro, ltrPropiedades.Elemento.Descripcion);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            if (modal.id === this.ModalCopiarFae.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.CopiarUnaFae(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalCambiarVencimiento.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.CambiarFechaDeVencimiento(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => super.ModalDePedirDatos_Aceptar(modal))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            } else if (modal.id === this.ModalCambiarDatos.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.CambiarDatos(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => super.ModalDePedirDatos_Aceptar(modal))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalHacerRectificativa.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.HacerRectificativa(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalFacturarTareas.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                Venta.FacturarTareas(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionFacturaEmt extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeLeerDatosParaInicializarCreacion(peticion);
            let presupuesto: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Venta.FacturaEmt.IdPresupuesto, ltrTipoControl.restrictorDeEdicion);
            let idPresupuesto: number = Numero(presupuesto.getAttribute(atRestrictor.idRestrictor));
            if (idPresupuesto > 0) {
                var parametros = new Array<Parametro>();
                parametros.push(new Parametro(ltrPropiedades.Venta.Presupuesto.IdTipoFacturaPorDefecto, true));
                ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Venta.Presupuestos, idPresupuesto, parametros, idPresupuesto)
                    .then((peticion) => this.MapearDatosPresupuesto(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
        }

        private MapearDatosPresupuesto(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionFacturaEmt = peticion.llamador as CrudCreacionFacturaEmt;
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(creador.PanelDeCrear, peticion.resultado.datos);
            let idTipoFacturaPorDefecto: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.IdTipoFacturaPorDefecto, 0);
            if (idTipoFacturaPorDefecto > 0) {
                let tipo: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(creador.PanelDeCrear, ltrPropiedades.Elemento.ConTipo.Tipo) as HTMLInputElement;
                MapearAlControl.ListaDinamica(tipo, idTipoFacturaPorDefecto, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.TipoFacturaPorDefecto, '', true));
            }
            ApiDelCrud.MapearNombreDescripcion(creador.PanelDeCrear, peticion.resultado.datos);
            let idSolicitante: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.IdSolicitante, 0, true);
            let filtros = new Array<ClausulaDeFiltrado>();
            filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Cliente.IdInterlocutor, literal.filtro.criterio.igual, idSolicitante));
            let parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Parametros.MensajeSiNoHay, 'El solicitante del presupuesto no está dado de alta como cliente'));
            parametros.push(new Parametro(Ajax.Parametros.ErrorSiNoHay, false));
            ApiDePeticiones.LeerElemento(this.PanelDeCrear, ltrControladores.Terceros.Clientes, filtros, parametros)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.MapearDatosCliente(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        private MapearDatosCliente(peticion: ApiDeAjax.DescriptorAjax): void {
            let panel = peticion.llamador as HTMLDivElement;
            let cliente: HTMLInputElement = ApiControl.BuscarControl(panel, ltrPropiedades.Venta.FacturaEmt.Cliente, true) as HTMLInputElement;

            let idCliente = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Cliente.Id, 0);
            if (idCliente > 0) {
                let expresion = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Cliente.Expresion, '');
                MapearAlControl.ListaDinamica(cliente, idCliente, expresion);
            }
            else
                MensajesSe.Info(peticion.resultado.mensaje);
            ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.Nombre).focus();
        }

    }

    export class CrudEdicionFacturaEmt extends Crud.CrudEdicion {

        private idGridDeLineas = 'lineasDeUnaFae'.toLocaleLowerCase();
        private idGridDeRectificadas = 'rectificadas';
        private idGridDeCobros = 'CobrosDeFae'.toLocaleLowerCase();
        private idGridDeAbonos = 'AbonosDeFae'.toLocaleLowerCase();
        private idModalDeCreacionDeLinea: string = 'lineas-de-una-fae';
        private idModalDeCreacionDeCobro: string = 'CobrosDeFae';
        private idModalDeCreacionDeAbonos: string = 'AbonosDeFae';
        private idPeriodoEmt = 'periodoEmt';
        private idIrpfEmt = 'irpfEmt';
        private idVerifactu = 'verifactu';

        public get GridDeLineas(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Venta.LineasDeFactura);
        }

        public get PanelDeLineas(): HTMLDivElement {
            let id = this._idPanelEdicion + '-' + this.idModalDeCreacionDeLinea;
            return document.getElementById(id) as HTMLDivElement;
        }

        public get CheckPanelDeRectificadas(): HTMLDivElement {
            let id = 'mostrar.' + this._idPanelEdicion + '-' + this.idGridDeRectificadas;
            return document.getElementById(id) as HTMLDivElement;
        }

        public get PanelDeRectificadas(): HTMLDivElement {
            let id = this._idPanelEdicion + '-' + this.idGridDeRectificadas;
            return document.getElementById(id) as HTMLDivElement;
        }

        public get PanelDeIrpf(): HTMLDivElement {
            return this.DivDeAmpliacion(this.idIrpfEmt);
        }

        public get PanelDeVerifactu(): HTMLDivElement {
            return this.DivDeAmpliacion(this.idVerifactu);
        }

        public get IdReferenciaCrearCobro(): string {
            return this._idPanelEdicion + `-cobrosdefae-mcr-${enumPostfijoControl.Referencia}`;
        }

        public get IdReferenciaCrearAbono(): string {
            return this._idPanelEdicion + `-abonosdefae-mcr-${enumPostfijoControl.Referencia}`;
        }
        public get IdReferenciaVicularRectificada(): string {
            return this._idPanelEdicion + `-rectificadas-vincular-${enumPostfijoControl.Referencia}`;
        }

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(this.idModalDeCreacionDeLinea);
        }

        public get ModalDeCreacionDeCobros(): HTMLDivElement {
            return this.ModalParaCrearRelacion(this.idModalDeCreacionDeCobro);
        }

        public get ModalDeCreacionDeAbonos(): HTMLDivElement {
            return this.ModalParaCrearRelacion(this.idModalDeCreacionDeAbonos);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(this.idGridDeLineas);
        }

        public get ModalDeEdicionDeRectificadas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(this.idGridDeRectificadas);
        }

        public get ModalDeEdicionDeCobros(): HTMLDivElement {
            return this.ModalParaEditarRelacion(this.idGridDeCobros);
        }

        public get TablaDeLineas(): HTMLDivElement {
            return this.TablaDelDetalle(this.idGridDeLineas) as HTMLDivElement;
        }

        public get TablaDeCobros(): HTMLDivElement {
            return this.TablaDelDetalle(this.idGridDeCobros) as HTMLDivElement;
        }

        public get TablaDeAbonos(): HTMLDivElement {
            return this.TablaDelDetalle(this.idGridDeAbonos) as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalParaEditarRelacion(this.idGridDeLineas));
        }

        public get SelectorDeIvaActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Venta.FacturaEmt.linea.selectorDeIvaR) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.FacturaEmt.linea.selectorDeIvaR) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.FacturaEmt.linea.selectorDeIvaR) as HTMLSelectElement;
        }

        public get PorcentageIvaActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Venta.FacturaEmt.linea.iva) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.FacturaEmt.linea.iva) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.FacturaEmt.linea.iva) as HTMLInputElement;
        }


        public get Cliente(): { id: number, nombre: string } {
            return {
                id: ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.IdCliente),
                nombre: ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.Cliente)
            };
        }

        private get _estaComunicandose(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EstaComunicandose, false);
        }

        private get _esPrefactura(): boolean {
            return EstaElEnumerado(this.Etapas, enumEtapasDeFacturaEmt, enumEtapasDeFacturaEmt.FAE_Etapa_Prefactura);
        }

        private get _esRectificativa(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);
        }

        public get IdParteTr(): number {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.IdParteTr, 0);
        }

        private get _esCobrable(): boolean {

            if (this.EstaCancelada || this._estaComunicandose || this._esRectificativa || this._esPrefactura)
                return false;

            return true; 
        }

        private get _esAbonable(): boolean {

            if (this.EstaCancelada || this._estaComunicandose || !this._esRectificativa)
                return false;

            const etapaAbonable = new Array<enumEtapasDeFacturaEmt>(enumEtapasDeFacturaEmt.FAE_Etapa_Emitida, enumEtapasDeFacturaEmt.FAE_Etapa_Abonable)

            return EstaAlgunEnumerado(this.Etapas, enumEtapasDeFacturaEmt, etapaAbonable);
        }



        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public AplicarTipoDeLinea() {
            let ocultar: boolean = false;
            let modal = this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let tipoDeLineaCtrl = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.FacturaEmt.linea.tipoDeLinea, true) as HTMLSelectElement;
            let tipoDeLinea: number = tipoDeLineaCtrl.selectedIndex;

            switch (tipoDeLinea) {
                case 0: {
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.concepto);
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.unitario);
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.precio);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.linea.unidad);
                    break;
                }
                case 1: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.unitario);
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.precio);
                    ApiControl.DesbloquearListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.linea.clase);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.linea.naturaleza);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.linea.unidad);
                    break;
                }
                case 2: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.FacturaEmt.linea.unitario);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.FacturaEmt.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.FacturaEmt.linea.unidad);
                    ocultar = true;
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${tipoDeLinea} a la modal de crear una línea de una factura`);
                    break;
                }
            }
            var postFijo = this.EstaCreandoUnaLinea ? 'nuevo' : 'edicion';
            var filaDeLaModal: string = 'table-lineadeunafaedto'; 
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

            this.fae_CalcularImportesDeLinea_interno(modal);
        }

        public fae_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.cantidad).value);
            let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.precio).value);

            let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.ImporteSinDto) as HTMLInputElement;
            let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.ImporteDeDto) as HTMLInputElement;
            let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.ImporteDeIva) as HTMLInputElement;
            let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.ImporteDeLinea) as HTMLInputElement;

            let importeSinDescuento: number = cantidad * precio;
            if (importeSinDescuento !== 0) {
                let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.descuentoPorLinea).value);
                AsignarValor(impSinDto, importeSinDescuento.toString());
                AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
                let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

                let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.FacturaEmt.linea.ivaPorLinea).value);
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

        public RecargarGridDeLineas() {
            MapearAlGrid.MapearGridDeDetalle(this.GridDeLineas, this.CrudDeMnt.IdNegocio, Numero(ObtenerPropiedad(this.Registro, literal.id)), this.CrudDeMnt.Guid);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Cobros)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeHitos();
                this.RecargarGridDeLineas();
                this.RecargarGridDeTrazas();
            }
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeHitos();
                this.RecargarGridDeLineas();
                this.RecargarGridDeTrazas();
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);

            let esRectificativa: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);
            let estaRectificada: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.IdRectificativa, 0) > 0;
            let usaCtrAdm: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.UsaCentroAdministrativo, false);


            ApiPanel.MostrarDetalleSi(this.CheckPanelDeRectificadas, peticion.resultado.datos, esRectificativa);
            ApiControl.MostrarPropiedadSi(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.ClaseRectificativa, esRectificativa);
            ApiControl.MostrarPropiedadSi(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.MotivoDeRectificacion, esRectificativa);

            ApiControl.MostrarPropiedadSi(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.IdRectificativa, estaRectificada);
            ApiControl.ColSpan(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.IdRectificativa, esRectificativa ? 0 : 2);

            let lista: HTMLSelectElement = ApiControl.BuscarListaDeElementos(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.CentroAdministrativo);
            var control = ApiControl.OcultarControlSi(lista, !usaCtrAdm);
            if ((usaCtrAdm && !estaRectificada) || (!usaCtrAdm && estaRectificada)) {
                var nodo = control
                while (Definido(nodo)) {
                    nodo = nodo.parentElement;
                    if (nodo.classList.contains(ltrCss.contenedorConMasDeUnControl)) {
                        if (!nodo.classList.contains(ltrCss.mostrarSoloElPrimerControl))
                            nodo.classList.add(ltrCss.mostrarSoloElPrimerControl);
                        break;
                    }
                }
            }
            if ((!usaCtrAdm && !estaRectificada) || (usaCtrAdm && estaRectificada)) {
                var nodo = control
                while (Definido(nodo)) {
                    nodo = nodo.parentElement;
                    if (nodo.classList.contains(ltrCss.contenedorConMasDeUnControl)) {
                        if (nodo.classList.contains(ltrCss.mostrarSoloElPrimerControl))
                            nodo.classList.remove(ltrCss.mostrarSoloElPrimerControl);
                        break;
                    }
                }
            }
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            ApiDeMenuFlotante.OcultarOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.SincronizarConLaAeat);
            if (this.EstaCancelada || this._estaComunicandose) {
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.FacturarTareas, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarDatos, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.Rectificativa, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarLa, ltrMenus.enumOrigen.edicion);

                if (this._estaComunicandose) {
                    MensajesSe.Info('La factura está comunicándose a la Aeat');
                    ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.SincronizarConLaAeat, Registro.EsAdministrador());
                }

                return;
            }


            let esRectificativa: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);
            let estaRectificada: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.IdRectificativa, 0) > 0;

            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarDatos, ltrMenus.enumOrigen.edicion,
                this._esPrefactura && this.EsInterventor && !esRectificativa && !estaRectificada);

            //Si NO es una prefactura
            if (!this._esPrefactura) {
                //ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Comun.Imprimir, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.FacturarTareas, ltrMenus.enumOrigen.edicion);

                if (this.EstaTerminada)
                    ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento, ltrMenus.enumOrigen.edicion);
                else
                    ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento, ltrMenus.enumOrigen.edicion);

                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.Rectificativa, ltrMenus.enumOrigen.edicion);

                ModoAcceso.AplicarModoAccesoAlSelectorDeArchivos(this.PanelDeEditar, peticion.resultado.datos.esInterventor
                    ? ModoAcceso.enumModoDeAccesoDeDatos.Gestor
                    : ModoAcceso.enumModoDeAccesoDeDatos.Consultor
                );
                ModoAcceso.AjustarOpcionesDeMenu(panel, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
                ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                ModoAcceso.AplicarloAlPanel(this.PanelDeLineas, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                ApiControl.OcultarHtmlAnchor(this.HtmlAnchorCrearEvento, false);
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Entorno.Agenda.ModalDeCrearEvento, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.GenerarPreasieto, ltrMenus.enumOrigen.edicion, Registro.EsAdministrador());
                ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.GenerarUbl, ltrMenus.enumOrigen.edicion, Registro.EsAdministrador());
            }

            //Si es una prefactura
            else {
                //ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Comun.Imprimir, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.FacturarTareas, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.Rectificativa, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.GenerarPreasieto, ltrMenus.enumOrigen.edicion, true);
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.GenerarUbl, ltrMenus.enumOrigen.edicion, true);
            }

            if (esRectificativa) {
                ApiDeGrid.Expansor_MostrarPorId(this.IdGridDelExpansor(this.idGridDeRectificadas));
                ModoAcceso.HabilitarRefSi(this.PanelDeRectificadas, this.IdReferenciaVicularRectificada, false);
                if (this._esPrefactura) {
                    ApiControl.BloquearEditorPorPropiedad(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.Contacto);
                    ApiControl.BloquearEditorPorPropiedad(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.Telefono);
                    ApiControl.BloquearEditorPorPropiedad(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.eMail);
                }
            }
            else {
                ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(this.idGridDeRectificadas));
            }


            if (esRectificativa)
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarLa, ltrMenus.enumOrigen.edicion);
            else
                ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarLa, ltrMenus.enumOrigen.edicion);

            if (esRectificativa || estaRectificada) {
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.Rectificativa, ltrMenus.enumOrigen.edicion);
                ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.FacturasEmt.FacturarTareas, ltrMenus.enumOrigen.edicion);
            }
        }

        public CargaCompletada() {
            super.CargaCompletada();
            if (this._esPrefactura || this.EstaCancelada || !(this.CrudDeMnt as CrudDeFacturasEmt).UsaVerifactu) ApiPanel.OcultarMostrarPanel(this.PanelDeVerifactu, true);

            let etiqueta = ApiControl.BuscarEtiqueta(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.Cobro.Cobrado);
            etiqueta.innerText = this._esRectificativa ? ltrEtiquetas.Venta.FacturaEmt.Abonado : ltrEtiquetas.Venta.FacturaEmt.Cobrado;

            this.ResetearGridDeCobro();
            this.ResetearGridDeAbonos();
        }

        public Expansor_TrasCargarAmpliacion(ampliacion: HTMLDivElement): void {
            super.Expansor_TrasCargarAmpliacion(ampliacion);
            if (ampliacion.parentElement.parentElement.id === this.IdAmpliacion(this.idPeriodoEmt)) {
                let esPeriodica: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsPeriodica, false);
                if (!esPeriodica) {
                    ApiPanel.OcultarPanel(this.DivDeAmpliacion(this.idPeriodoEmt));
                    return;
                }
                let esRectificativa: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);
                if (esRectificativa)
                    ModoAcceso.AplicarloAlPanel(this.DivDeAmpliacion(this.idPeriodoEmt), ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                else
                    ModoAcceso.AplicarloAlPanel(this.DivDeAmpliacion(this.idPeriodoEmt), this.ModoDeAcceso, false);
            }

            if (ampliacion.parentElement.parentElement.id === this.IdAmpliacion(this.idIrpfEmt)) {
                var listaCliente = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Venta.FacturaEmt.Cliente)
                ApiControl.ObtenerObjetoListaDinamica(listaCliente, ltrControladores.Terceros.Clientes,
                    (id, cliente) => this.MostrarPanelIrpf(id, cliente))

                if (this._esRectificativa) {
                    ApiPanel.PonerEnModoConsulta(ampliacion);
                }
            }
        }

        public async MostrarPanelIrpf(idLista: string, cliente: any) {
            OpcionesDeLasListas.AgregarOpcion(idLista, cliente);
            var sociedad = await this.SociedadDelCg();
            let mostrarIrpf: boolean = false;
            let esPrefactura: boolean = this._esPrefactura
            let ponerEnCosulta: boolean = !esPrefactura;
            if (Definido(sociedad) && ObtenerPropiedad(sociedad, ltrPropiedades.Terceros.Sociedad.TipoDeTercero) === enumTipoDeTercero.Autonomo) {
                var tipoDeTercero = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.TipoDeTercero);
                if (tipoDeTercero === enumTipoDeTercero.Empresa || tipoDeTercero === enumTipoDeTercero.Autonomo) {
                    let conIrpf: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.ConIrpf, false);
                    let esRectificativa: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);
                    mostrarIrpf = (esPrefactura && !esRectificativa) || conIrpf;
                    ponerEnCosulta = !esPrefactura && mostrarIrpf;
                }
            }

            if (!mostrarIrpf && Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.TotalIrpf)) > 0) mostrarIrpf = true

            ApiPanel.OcultarMostrarPanel(this.PanelDeIrpf, !mostrarIrpf);
            if (mostrarIrpf && ponerEnCosulta) ModoAcceso.AplicarloAlPanel(this.PanelDeIrpf, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
        }

        public Expansor_TrasCargarDetalle(idGrid: string) {
            super.Expansor_TrasCargarDetalle(idGrid);
            if (this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Cobros) === idGrid) {
                let esRectificativa: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);

                if (esRectificativa || this._esPrefactura) {
                    ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Cobros));
                    ModoAcceso.DeshabilitarRef(this.PanelDeEditar, this.IdReferenciaCrearCobro);
                }

                this.ResetearGridDeCobro();
            }

            if (this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos) === idGrid) {
                let esRectificativa: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.EsRectificativa, false);

                if (!esRectificativa || this._esPrefactura) {
                    ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos));
                    ModoAcceso.DeshabilitarRef(this.PanelDeEditar, this.IdReferenciaCrearAbono);
                }

                this.ResetearGridDeAbonos();
            }

            if (this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Lineas) === idGrid) {
                let modal: HTMLDivElement = this.ModalDeCreacionDeLineas;
                if (ApiPanel.ModalAbierta(modal)) {
                    Fae_InicializarModalParaCrearLineas();
                }
            }
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            let editor: CrudEdicionFacturaEmt = peticion.llamador as CrudEdicionFacturaEmt;
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarVencimiento) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.CambiarDatos) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.Rectificativa) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.FacturarTareas) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.GenerarPreasieto) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas()
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.GenerarUbl) {
                this.RecargarArchivos();
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Venta.FacturasEmt.CopiarLa) {
                var idFactura = ObtenerPropiedad(peticion.resultado.datos, literal.id);
                var urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${literal.id}=${idFactura}`;
                EntornoSe.AbrirPestana(urlDestino);
                return true;
            }
            return false;
        }

        private ResetearGridDeCobro() {
            if (this._esRectificativa) {
                ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Cobros));
                ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearCobro, false);
                return;
            }

            if (this._esCobrable) {
                ApiDeGrid.Expansor_MostrarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Cobros));
                const esEditable: boolean = this.Registro.esInterventor &&
                    (EstaElEnumerado(this.Etapas, enumEtapasDeFacturaEmt, enumEtapasDeFacturaEmt.FAE_Etapa_Cobrada) ||
                        EstaElEnumerado(this.Etapas, enumEtapasDeFacturaEmt, enumEtapasDeFacturaEmt.FAE_Etapa_De_Cobro))

                if (esEditable) ApiDeGrid.Expansor_PonerEnEdicion(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Cobros));
                ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearCobro, ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.Cobro.Pendiente, 0) > 0);
            }
        }

        private ResetearGridDeAbonos() {
            if (!this._esRectificativa) {
                ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos));
                ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearAbono, false);
                return;
            }

            if (this._esAbonable) {
                ApiDeGrid.Expansor_MostrarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos));

                if (this.Registro.esInterventor && (EstaElEnumerado(this.Etapas, enumEtapasDeFacturaEmt, enumEtapasDeFacturaEmt.FAE_Etapa_Abonada)
                    || EstaElEnumerado(this.Etapas, enumEtapasDeFacturaEmt, enumEtapasDeFacturaEmt.FAE_Etapa_Abonable))) {
                    ApiDeGrid.Expansor_PonerEnEdicion(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos));
                }
                const porAbonar = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.Abono.Pendiente, 0));
                ModoAcceso.HabilitarRefSi(this.PanelDeEditar, this.IdReferenciaCrearAbono, porAbonar > 0);
            }
            else {
                const abonado = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.FacturaEmt.Abono.Abonado, 0));
                if (abonado === 0)
                    ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(ltrEspanes.Venta.FacturasEmt.Abonos));
            }
        }



    }

    export function NavegarARelacionesDeFae(opcion: string, datosDeEntrada: Parametros, idRestrictor: string): boolean {
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Venta.FacturasEmt.IrAPartesTr:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PartesTr}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.FacturasEmt.IrAPpts:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.Presupuestos}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.FacturasEmt.IrAContratos:
                urlDestino = `${window.location.origin}/${ltrUrls.Juridico.ContratosDeVenta}&${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }

    export function Fae_ValidarEnAeat(): boolean {
        let urlDestino: HTMLInputElement = ApiControl.BuscarEditor((Crud.crudMnt.crudDeEdicion as CrudEdicionFacturaEmt).PanelDeVerifactu, ltrPropiedades.Venta.FacturaEmt.Verifactu.Url);
        EntornoSe.AbrirPestana(urlDestino.value);
        return true;
    }

    export function Fae_IrARectificadaPor(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        IrAFactura(idRegistro, Ajax.EndPoint.Venta.FacturaEmt.IrARectificadaPor);
    }

    export function Fae_IrARectificoA(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        IrAFactura(idRegistro, Ajax.EndPoint.Venta.FacturaEmt.IrARectificoA);
    }
    function IrAFactura(id: number, endPoint: string) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, endPoint, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

}