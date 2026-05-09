namespace Presupuesto {

    enum enumEtapasDePresupuestos {
        PPT_Etapa_Elaboracion,
        PPT_Etapa_Pendiente,
        PPT_Etapa_Aceptado,
        PPT_Etapa_PermiteFacturar,
        PPT_Etapa_Rechazo,
        PPT_Etapa_Cancelado,
        PPT_Etapa_AsociarParteTr
    }

    export function CrearCrudDePresupuestos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Presupuesto.CrudDePresupuestos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePresupuestos extends Crud.CrudMnt {

        public get ModalAsociarExpediente(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.Presupuestos.AsociarUnExpediente) as HTMLDivElement; }

        public get ModalCopiarPpt(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.Presupuestos.CopiarPpt) as HTMLDivElement; }

        public get ModalParaRenombrar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Venta.Presupuestos.Renombrar) as HTMLDivElement; }

        private _IdDeUnidadDeMedida: number = 0;
        private _IdDeNaturaleza: number = 0;
        private _ClaseDeUnitario: string = undefined;
        private _TipoDeLinea: string = undefined;

        public get UnidadDeMedida(): number { return this._IdDeUnidadDeMedida; }
        public get Naturaleza(): number { return this._IdDeNaturaleza; }
        public get ClaseDeUnitario(): string { return this._ClaseDeUnitario; }
        public get TipoDeLinea(): string { return this._TipoDeLinea; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPresupuesto(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPresupuesto(this, idPanelEdicion);
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._IdDeUnidadDeMedida = mapIndicadores.get(ltrPropiedades.Venta.Presupuesto.Indicadores.UnidadDeMedida);
            this._IdDeNaturaleza = mapIndicadores.get(ltrPropiedades.Venta.Presupuesto.Indicadores.Naturaleza);
            this._ClaseDeUnitario = mapIndicadores.get(ltrPropiedades.Venta.Presupuesto.Indicadores.ClaseDeUnitario);
            this._TipoDeLinea = mapIndicadores.get(ltrPropiedades.Venta.Presupuesto.Indicadores.TipoDeLinea);
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Venta.Presupuestos.CopiarPpt) {
                let idModal = this.IdCrud + '-' + opcion;
                let id = this.InfoSelector.Seleccionados.length === 0 ? 0 : this.InfoSelector.Seleccionados[0].Id;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, id);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;

            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);

            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Venta.Presupuestos);
                return true;
            }

            return Presupuesto.NavegarARelacionesDelPpt(peticion, ltrParametrosUrl.Venta.IdPresupuesto);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            let parametros: Array<Parametro> = new Array<Parametro>();
            let datosDeEntrada = new Array<Parametro>();

            if (modal.id === this.ModalCopiarPpt.id) {
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                Presupuesto.GenerarUnPpt(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.Ventas.Presupuestos}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalAsociarExpediente.id) {
                    let idExpediente: number = Numero(ApiControl.BuscarListaDinamicaPorPropiedad(this.ModalAsociarExpediente, ltrPropiedades.Selector.Elemento).getAttribute(atListasDinamicas.idSeleccionado));
                    parametros.push(new Parametro(ltrPropiedades.Venta.Presupuesto.id, this.crudDeEdicion.ElementoEditado.Id));
                    parametros.push(new Parametro(ltrPropiedades.Venta.Presupuesto.idExpediente, idExpediente));
                    Presupuesto.AsociarUnPpt(this, this.Controlador, parametros, datosDeEntrada)
                        .then((peticion) => super.ModalDePedirDatos_Aceptar(modal))
                        .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                }
            else if (modal.id === this.ModalParaRenombrar.id) {                        
                parametros.push(new Parametro(ltrPropiedades.Venta.RenombraPpt.IdElemento, this.crudDeEdicion.ElementoEditado.Id));
                parametros.push(new Parametro(ltrPropiedades.Venta.RenombraPpt.Nombre, ApiControl.BuscarEditor(this.ModalParaRenombrar, literal.nombre).value));
                    Presupuesto.RenombrarUnPpt(this, this.Controlador, parametros, datosDeEntrada)
                        .then((peticion) => super.ModalDePedirDatos_Aceptar(modal))
                        .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                    }
            else
               super.ModalDePedirDatos_Aceptar(modal);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalAsociarExpediente.id) {
                ApiDelCrud.ProponerPropiedad(this.crudDeEdicion.PanelDeEditar, modal, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true);
                ApiDelCrud.ProponerPropiedad(this.crudDeEdicion.PanelDeEditar, modal, ltrPropiedades.Elemento.ConCg.Cg, false);
            }

            if (modal.id === this.ModalParaRenombrar.id) {
                ApiDelCrud.ProponerParaRenombrar(this.ModalParaRenombrar, this.crudDeEdicion.ElementoEditado.Registro);
            }

            if (modal.id === this.ModalCopiarPpt.id) {
                if (this.InfoSelector.Seleccionados.length === 1)
                    this.Ppt_ProponerPresupuestoParaCopiar(modal, this.InfoSelector.Seleccionados[0].Registro);
            }
        }

        public Ppt_ProponerPresupuestoParaCopiar(modal: HTMLDivElement, registro: any) {
            let destino: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.expresion));
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConTipo.Tipo, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Venta.Presupuesto.Expediente, ltrPropiedades.Venta.Presupuesto.idExpediente);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Venta.Presupuesto.Solicitante, ltrPropiedades.Venta.Presupuesto.IdSolicitante);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Elemento.Nombre);
            ApiDelCrud.ProponerEnAreaDeTexto(modal, registro, ltrPropiedades.Elemento.Descripcion);
        }

    }

    export class CrudCreacionPresupuesto extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            let expediente: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Venta.Presupuesto.idExpediente, ltrTipoControl.restrictorDeEdicion);
            let idExpediente: number = Numero(expediente.getAttribute(atRestrictor.idRestrictor));
            if (idExpediente > 0) {
                ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Administracion.Expedientes, idExpediente, new Array<Parametro>(), idExpediente)
                    .then((peticion) => this.MapearDatosExpediente(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else ApiDelCrud.PosicionarCrudDeCreacionConCgYTipo(this.PanelDeCrear);
            ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Ejecucion);
        }

        private MapearDatosExpediente(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionPresupuesto = peticion.llamador as CrudCreacionPresupuesto;
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(creador.PanelDeCrear, peticion.resultado.datos);

            let solicitante: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.Solicitante, 0, true);
            let idSolicitante: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.IdSolicitante, 0, true);
            let solicitanteLista: HTMLInputElement = ApiControl.BuscarControl(creador.PanelDeCrear, ltrPropiedades.Venta.Presupuesto.Solicitante, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(solicitanteLista, idSolicitante, solicitante);
            ApiPanel.PosicionarEn(creador.PanelDeCrear);
            //ApiControl.BuscarListaDinamicaPorPropiedad(creador.PanelDeCrear, ltrPropiedades.Elemento.ConTipo.Tipo).focus();
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}
    }

    export class CrudEdicionPresupuesto extends Crud.CrudEdicion {

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Venta.Presupuesto.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Venta.Presupuestos.Lineas);
        }

        public get GridDeLineas(): HTMLDivElement {
            return document.getElementById('grid-de-detalle-lineasdeunppt-tabla') as HTMLDivElement;
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeEdicionDeLineas);
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get SelectorDeIvaActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR) as HTMLSelectElement;
        }

        public get PorcentageIvaActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Venta.Presupuesto.linea.ivaPorLinea) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.Presupuesto.linea.ivaPorLinea) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.Presupuesto.linea.ivaPorLinea) as HTMLInputElement;
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.Presupuesto.Etapas);
            ApiDelCrud.BloquearDesbloquearReferenciaPostDeCreacion(etapas, enumEtapasDePresupuestos, enumEtapasDePresupuestos.PPT_Etapa_PermiteFacturar, this.IdDeExpansor(ltrEspanes.Venta.Presupuestos.Facturas));
            ApiDelCrud.BloquearDesbloquearReferenciaPostDeCreacion(etapas, enumEtapasDePresupuestos, enumEtapasDePresupuestos.PPT_Etapa_AsociarParteTr, this.IdDeExpansor(ltrEspanes.Venta.Presupuestos.Partes));


            if (!this.EstaTerminada && !this.EstaCancelada) {

                if (this.EsInterventorSinEstado) {
                    let estaPendiente: boolean = EstaElEnumerado(etapas, enumEtapasDePresupuestos, enumEtapasDePresupuestos.PPT_Etapa_Pendiente);
                    let estaEnEjecucion: boolean = EstaElEnumerado(etapas, enumEtapasDePresupuestos, enumEtapasDePresupuestos.PPT_Etapa_Aceptado) || EstaElEnumerado(etapas, enumEtapasDePresupuestos, enumEtapasDePresupuestos.PPT_Etapa_AsociarParteTr);
                    let estaEnFacturacion: boolean = EstaElEnumerado(etapas, enumEtapasDePresupuestos, enumEtapasDePresupuestos.PPT_Etapa_PermiteFacturar);
                    ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.Presupuestos.Renombrar, ltrMenus.enumOrigen.edicion, !estaPendiente && !estaEnEjecucion && !estaEnFacturacion)
                }
                else
                    ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.Venta.Presupuestos.Renombrar, ltrMenus.enumOrigen.edicion);

                ModoAcceso.AplicarModoAccesoAlSelectorDeArchivos(this.PanelDeEditar, peticion.resultado.datos.esInterventor
                    ? ModoAcceso.enumModoDeAccesoDeDatos.Gestor
                    : ModoAcceso.enumModoDeAccesoDeDatos.Consultor
                );
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Venta.Presupuestos.AsociarUnExpediente) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDePresupuestos).ModalAsociarExpediente.id, this.ElementoEditado.Id);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Venta.Presupuestos.Renombrar) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDePresupuestos).ModalParaRenombrar.id, this.ElementoEditado.Id);
                return true;
            }

            return false;
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.Presupuestos.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
        }

        public AplicarTipoDeLinea() {
            let ocultar: boolean = false;

            let modal = this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.Presupuesto.linea.tipoDeLinea, true) as HTMLSelectElement;

            switch (tipoDeLinea.selectedIndex) {
                case 0: {
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.concepto);
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.unitario);
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.precio);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.Presupuesto.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.Presupuesto.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.Presupuesto.linea.unidad);
                    break;
                }
                case 1: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.unitario);
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.precio);
                    ApiControl.DesbloquearListaDeValores(modal, ltrPropiedades.Venta.Presupuesto.linea.clase);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Venta.Presupuesto.linea.naturaleza);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Venta.Presupuesto.linea.unidad);
                    if (this.EstaCreandoUnaLinea) {
                        if ((this.CrudDeMnt as CrudDePresupuestos).Naturaleza > 0) {
                            var SelectorNaturaleza = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Venta.Presupuesto.linea.naturaleza);
                            MapearAlControl.ListaDeElementos(SelectorNaturaleza, new Array<ClausulaDeFiltrado>(), (this.CrudDeMnt as CrudDePresupuestos).Naturaleza, null);
                        }
                        if ((this.CrudDeMnt as CrudDePresupuestos).UnidadDeMedida > 0) {
                            var SelectorUnidad = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Venta.Presupuesto.linea.unidad);
                            MapearAlControl.ListaDeElementos(SelectorUnidad, new Array<ClausulaDeFiltrado>(), (this.CrudDeMnt as CrudDePresupuestos).UnidadDeMedida, null);
                        }
                        if (Definido((this.CrudDeMnt as CrudDePresupuestos).ClaseDeUnitario)) {
                            var SelectorDeClase = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.Presupuesto.linea.clase);
                            MapearAlControl.ListaDeValores(SelectorDeClase, (this.CrudDeMnt as CrudDePresupuestos).ClaseDeUnitario);
                        }
                    }
                    break;
                }
                case 2: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.unitario);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.Presupuesto.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.Presupuesto.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.Presupuesto.linea.unidad);
                    ocultar = true;
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${tipoDeLinea.selectedIndex} a la modal de crear una línea de un ppt`);
                    break;
                }
            }
            var postFijo = this.EstaCreandoUnaLinea ? 'nuevo' : 'edicion';
            var filaDeLaModal: string = 'table-lineadeunpptdto';
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

            this.ppt_CalcularImportesDeLinea_interno(modal);
        }

        public ppt_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.cantidad).value);
            let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.precio).value);

            let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteSinDto) as HTMLInputElement;
            let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeDto) as HTMLInputElement;
            let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeIva) as HTMLInputElement;
            let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeLinea) as HTMLInputElement;

            let importeSinDescuento: number = cantidad * precio;
            if (importeSinDescuento > 0) {
                let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.descuentoPorLinea).value);
                AsignarValor(impSinDto, importeSinDescuento.toString());
                AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
                let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

                let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ivaPorLinea).value);
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

    }

    export function Ppt_InicializarModalParaCrearLineas(incremento: number) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.GridDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Venta.Presupuesto.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.Presupuesto.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + incremento).toString();

        if (Definido((Crud.crudMnt as CrudDePresupuestos).TipoDeLinea)) {
            var SelectorDeTipo = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Venta.Presupuesto.linea.tipoDeLinea);
            MapearAlControl.ListaDeValores(SelectorDeTipo, (Crud.crudMnt as CrudDePresupuestos).TipoDeLinea);
        }
        else {
            let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.Presupuesto.linea.unitario, true) as HTMLInputElement;
            unitario.focus();
            ppt_InicializarModalDeLineas_interno();
            ppt_MapearPorcentajes(editor.ModalDeCreacionDeLineas);
            editor.AplicarTipoDeLinea();
        }
    }

    export function Ppt_TrasCargarIvas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto);
        if (!editor.EstaCreandoUnaLinea)
            return;
        ppt_MapearPorcentajes(editor.ModalDeCreacionDeLineas);
    }

    export function Ppt_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto);
        editor.AplicarTipoDeLinea();
    }

    export function Ppt_Tras_Blanquear_Unitario() {
        Ppt_Tras_Cambiar_TipoDeLinea();
    }

    export function Ppt_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        ppt_InicializarModalDeLineas_interno();
        ppt_MapearPorcentajes(modal);
        editor.AplicarTipoDeLinea();
    }

    export function Ppt_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            ppt_mapearUnitarioSeleccionado_interno(objeto);
        }
        else {
            var tieneValorAsignado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0 && !IsNullOrEmpty(lista.value);
            if (!tieneValorAsignado)
                ppt_InicializarModalDeLineas_interno();
        }
    }

    export function Ppt_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        editor.ppt_CalcularImportesDeLinea_interno(modal);
    }

    export function Ppt_IvaRepercutidoCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto;
        let selectorIvaR = editor.SelectorDeIvaActivo;
        //let opcion: HTMLOptionElement = selectorIvaR.selectedOptions[0];
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaR); // JSON.parse(opcion.getAttribute(atControl.objeto));
        let iva = editor.PorcentageIvaActivo;
        if (Definido(objeto))
            AsignarValor(iva, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje));
        else
            ApiControl.BlanquearEditor(iva);
    }

    export function GenerarUnPpt(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion( llamador,controlador, Ajax.EndPoint.Venta.Presupuesto.Generar, parametros, datosDeEntrada);
    }

    export function AsociarUnPpt(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.Presupuesto.Asociar, parametros, datosDeEntrada);
    }

    export function RenombrarUnPpt(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.Presupuesto.Renombrar, parametros, datosDeEntrada);
    }

    export function Ppt_ProponerDatosDelPptSeleccionado() {
        let modal = (Crud.crudMnt as CrudDePresupuestos).ModalCopiarPpt;
        let ppt: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
        let idPpt: number = Numero(ppt.getAttribute(atListasDinamicas.idSeleccionado));
        if (idPpt > 0) {
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Venta.Presupuestos, idPpt, new Array<Parametro>(), idPpt)
                .then((peticion) => (Crud.crudMnt as CrudDePresupuestos).Ppt_ProponerPresupuestoParaCopiar(modal, peticion.resultado.datos))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

    }

    export function Ppt_InicializarModalDeCopiado() {
        let modal = (Crud.crudMnt as CrudDePresupuestos).ModalCopiarPpt;
        ApiPanel.BlanquearControlesDeIU(modal);
    }

    function ppt_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.Presupuesto.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.Presupuesto.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.Presupuesto.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.Presupuesto.linea.unidad) as HTMLSelectElement;
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

    function ppt_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.Presupuesto.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.Presupuesto.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.Presupuesto.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.Presupuesto.linea.unidad) as HTMLSelectElement;
        let selectorIvaR = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.Presupuesto.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Venta.Presupuesto.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        selectorIvaR.selectedIndex = 0;
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }

    function ppt_MapearPorcentajes(modal: HTMLDivElement) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionPresupuesto;
        let selectorIvaPropuesto = ApiControl.BuscarListaDeElementos(editor.PanelDeEditar, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR) as HTMLSelectElement;
        if (!Definido(selectorIvaPropuesto))
            return;
        let selectorIvaR = editor.SelectorDeIvaActivo;
        selectorIvaR.selectedIndex = selectorIvaPropuesto.selectedIndex;

        let ivaPropuesto = ApiControl.BuscarEditor(Crud.crudMnt.crudDeEdicion.PanelDeEditar, ltrPropiedades.Venta.Presupuesto.ivaPropuesto) as HTMLInputElement;
        let ivaPorlinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ivaPorLinea) as HTMLInputElement;
        AsignarValor(ivaPorlinea, ivaPropuesto.value);

        let descuentoPropuesto = ApiControl.BuscarEditor(Crud.crudMnt.crudDeEdicion.PanelDeEditar, ltrPropiedades.Venta.Presupuesto.descuentoPropuesto) as HTMLInputElement;
        let descuentoPorlinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.descuentoPorLinea) as HTMLInputElement;
        AsignarValor(descuentoPorlinea, descuentoPropuesto.value);
    }

    export function NavegarARelacionesDelPpt(peticion: ApiDeAjax.DescriptorAjax, idRestrictor: string): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Venta.Presupuestos.IrAPartesTr:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PartesTr}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.Presupuestos.IrATareas:
                urlDestino = `${window.location.origin}/${ltrUrls.Administracion.Tareas}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.Presupuestos.IrAFacturasEmt:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }

}


