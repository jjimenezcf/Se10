namespace Venta {


    enum enumEtapasDePartesTr {
        PTR_Etapa_Pendiente,
        PTR_Etapa_Pdt_Facturar,
        PTR_Etapa_Facturado,
        PTR_Etapa_Cancelado
    }

    export function CrearCrudDePartesTr(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Venta.CrudDePartesTr(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePartesTr extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionParteTr(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionParteTr(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax) : boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            return Venta.NavegarARelacionesDePtr(peticion, ltrParametrosUrl.Venta.IdParteTr);
        }
    }

    export class CrudCreacionParteTr extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeLeerDatosParaInicializarCreacion(peticion);
            let presupuesto: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Venta.ParteTr.IdPresupuesto, ltrTipoControl.restrictorDeEdicion);
            let idPresupuesto: number = Numero(presupuesto.getAttribute(atRestrictor.idRestrictor));
            if (idPresupuesto > 0) {
                var parametros = new Array<Parametro>();
                parametros.push(new Parametro(ltrPropiedades.Venta.Presupuesto.IdTipoPartePorDefecto, true));
                ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Venta.Presupuestos, idPresupuesto, parametros, idPresupuesto)
                    .then((peticion) => this.MapearDatosPresupuesto(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
        }

        private MapearDatosPresupuesto(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionParteTr = peticion.llamador as CrudCreacionParteTr;
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(creador.PanelDeCrear, peticion.resultado.datos);
            let idTipoPartePorDefecto: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.IdTipoPartePorDefecto, 0);
            if (idTipoPartePorDefecto > 0) {
                let tipo: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(creador.PanelDeCrear, ltrPropiedades.Elemento.ConTipo.Tipo) as HTMLInputElement;
                MapearAlControl.ListaDinamica(tipo, idTipoPartePorDefecto, ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.TipoPartePorDefecto, '', true));
            }
            ApiDelCrud.MapearNombreDescripcion(creador.PanelDeCrear, peticion.resultado.datos);
            let idSolicitante: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.Presupuesto.IdSolicitante, 0, true);
            let filtros = new Array<ClausulaDeFiltrado>();
            filtros.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Cliente.IdInterlocutor, literal.filtro.criterio.igual, idSolicitante));
            ApiDePeticiones.LeerElemento(this.PanelDeCrear, ltrControladores.Terceros.Clientes, filtros, Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.MapearDatosCliente(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        private MapearDatosCliente(peticion: ApiDeAjax.DescriptorAjax): void {
            let panel = peticion.llamador as HTMLDivElement;
            let cliente: HTMLInputElement = ApiControl.BuscarControl(panel, ltrPropiedades.Venta.ParteTr.Cliente, true) as HTMLInputElement;

            let idCliente = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Cliente.Id, 0);
            if (idCliente > 0) {
                let expresion = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Cliente.Expresion, '');
                MapearAlControl.ListaDinamica(cliente, idCliente, expresion);
            }
            ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.Nombre).focus();
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}

    }

    export class CrudEdicionParteTr extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let etapa: enumEtapasDePartesTr = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.ParteTr.Etapa);

            if (!this.EsGestor ||
                etapa === enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar ||
                etapa === enumEtapasDePartesTr.PTR_Etapa_Facturado) {
                ApiDelCrud.BloquearReferenciaModalDeCreacion(this.IdDeExpansor(ltrEspanes.Venta.PartesTr.Lineas));
                ApiDelCrud.BloquearReferenciaModalDeCreacion(this.IdDeExpansor(ltrEspanes.Venta.PartesTr.Asignaciones));
            }
            else if (this.EsGestor && etapa === enumEtapasDePartesTr.PTR_Etapa_Pendiente) {
                ApiDelCrud.DesbloquearReferenciaModalDeCreacion(this.IdDeExpansor(ltrEspanes.Venta.PartesTr.Lineas));
                ApiDelCrud.DesbloquearReferenciaModalDeCreacion(this.IdDeExpansor(ltrEspanes.Venta.PartesTr.Asignaciones));
            }
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.PartesTr.Asignaciones)) {
                let gridDeEventos: HTMLDivElement = document.getElementById(ltrGridDeUnExpansor.Eventos) as HTMLDivElement;
                MapearAlGrid.MapearGridDeDetalle(gridDeEventos, idnegocio, id, this.CrudDeMnt.Guid);
            }
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Venta.PartesTr.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
        }

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrModalDeCrearRelacion.Venta.ParteTr.Lineas);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Venta.PartesTr.Lineas);
        }

        public get TablaDeLineas(): HTMLDivElement {
            return this.TablaDelDetalle(ltrEspanes.Venta.PartesTr.Lineas) as HTMLDivElement;
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalParaEditarRelacion(ltrEspanes.Venta.PartesTr.Lineas));
        }

        public get SelectorDeIvaActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Venta.ParteTr.linea.selectorDeIvaR) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.ParteTr.linea.selectorDeIvaR) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.ParteTr.linea.selectorDeIvaR) as HTMLSelectElement;
        }

        public get PorcentageIvaActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Venta.ParteTr.linea.iva) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Venta.ParteTr.linea.iva) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Venta.ParteTr.linea.iva) as HTMLInputElement;
        }

        public AplicarTipoDeLinea(tipoDeLinea: number) {
            let ocultar: boolean = false;
            let modal = this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let tipoDeLineaCtrl = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.Presupuesto.linea.tipoDeLinea, true) as HTMLSelectElement;
            tipoDeLinea = tipoDeLineaCtrl.selectedIndex;

            switch (tipoDeLinea) {
                case 0: {
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.concepto);
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.unitario);
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.precio);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.ParteTr.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.ParteTr.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.ParteTr.linea.unidad);
                    break;
                }
                case 1: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.unitario);
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.precio);
                    ApiControl.DesbloquearListaDeValores(modal, ltrPropiedades.Venta.ParteTr.linea.clase);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Venta.ParteTr.linea.naturaleza);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Venta.ParteTr.linea.unidad);
                    break;
                }
                case 2: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Venta.ParteTr.linea.unitario);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Venta.ParteTr.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.ParteTr.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Venta.ParteTr.linea.unidad);
                    ocultar = true;
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${tipoDeLinea} a la modal de crear una línea de un ptr`);
                    break;
                }
            }
            var postFijo = this.EstaCreandoUnaLinea ? 'nuevo' : 'edicion';
            var filaDeLaModal: string = 'table-lineadeunptrdto';
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

            this.ptr_CalcularImportesDeLinea_interno(modal);
        }

        public ptr_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.cantidad).value);
            let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.precio).value);

            let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.ImporteSinDto) as HTMLInputElement;
            let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.ImporteDeDto) as HTMLInputElement;
            let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.ImporteDeIva) as HTMLInputElement;
            let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.ImporteDeLinea) as HTMLInputElement;

            let importeSinDescuento: number = cantidad * precio;
            if (importeSinDescuento > 0) {
                let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.descuentoPorLinea).value);
                AsignarValor(impSinDto, importeSinDescuento.toString());
                AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
                let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

                let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.ParteTr.linea.ivaPorLinea).value);
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

    export function NavegarARelacionesDePtr(peticion: ApiDeAjax.DescriptorAjax, idRestrictor: string):boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Venta.Partes.IrAFacturasEmt:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.Partes.IrAPlanificaciones:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PlfDeVenta}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.Partes.IrAPpts:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.Presupuestos}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Venta.Partes.IrAContratos:
                urlDestino = `${window.location.origin}/${ltrUrls.Juridico.ContratosDeVenta}&${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }
}