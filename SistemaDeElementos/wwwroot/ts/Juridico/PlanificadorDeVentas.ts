namespace Juridico {


    export function CrearCrudDePlanificadorDeVentas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Juridico.CrudDePlanificadorDeVentas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePlanificadorDeVentas extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPlanificadorDeVenta(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPlanificadorDeVenta(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;            
            return Juridico.NavegarARelacionesDelPlfdor(peticion, ltrParametrosUrl.Juridico.IdPlanificador);
        }

    }

    export class CrudCreacionPlanificadorDeVenta extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeLeerDatosParaInicializarCreacion(peticion);
            let contrato: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Juridico.lote.idContrato, ltrTipoControl.restrictorDeEdicion);
            let idContrato: number = Numero(contrato.getAttribute(atRestrictor.idRestrictor));
            if (idContrato == 0) {
                MensajesSe.Error("ComenzarCreacion", "Debe indicar el identificador del contrato");
                return;
            }
            var ampliaciones = new Array<string>();
            ampliaciones.push(ltrAmpliaciones.contratos.DatosDelContratoDtm);
            let parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.AmpliacionesSolicitadas, JSON.stringify(ampliaciones)));
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Juridico.Contratos, idContrato, parametros, idContrato)
                .then((peticion) => this.MapearDatosContrato(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }


        private MapearDatosContrato(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionPlanificadorDeVenta = peticion.llamador as CrudCreacionPlanificadorDeVenta;
            let inicio: Date = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.Contrato.DatosDeVenta.InicioContrato, 0, true);
            let fin: Date = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.Contrato.DatosDeVenta.FinContrato, undefined, true);
            ApiControl.AsignarFecha(creador.PanelDeCrear, ltrPropiedades.Juridico.PlanificadorDeVenta.inicio, inicio);
            ApiControl.AsignarFecha(creador.PanelDeCrear, ltrPropiedades.Juridico.PlanificadorDeVenta.hasta, fin);
            creador.MapearCgDeLasPlanificaciones(creador.PanelDeCrear, peticion.resultado.datos);
            ApiControl.BuscarEditor(creador.PanelDeCrear, ltrPropiedades.Elemento.Nombre).focus();
        }

        private MapearCgDeLasPlanificaciones(panel: HTMLDivElement, datos: any,): void {
            let idSociedad: number = ObtenerPropiedad(datos, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, 0, true);
            let idCg: number = ObtenerPropiedad(datos, literal.idCg, 0, true);
            let cg: string = ObtenerPropiedad(datos, literal.Cg, "", true);
            let cgLista: HTMLInputElement = ApiControl.BuscarControl(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.CgDeLaPlanificacion, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(cgLista, idCg, cg);
            let sociedad = ApiControl.BuscarControl(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.IdSociedadDelCg, true) as HTMLInputElement;
            sociedad.value = idSociedad.toString();
        }

    }

    export class CrudEdicionPlanificadorDeVenta extends Crud.CrudEdicion {
        private idGridDeLineas = 'lineasdeunplfventa';
        private idModalDeCreacionDeLinea: string = 'lineasDeUnPlfVenta';

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(this.idModalDeCreacionDeLinea);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(this.idGridDeLineas);
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalParaEditarRelacion(this.idGridDeLineas));
        }

        public get TablaDeLineas(): HTMLDivElement {
            return this.TablaDelDetalle(this.idGridDeLineas) as HTMLDivElement;
        }

        public get IdPlanificador(): number {
            return this.ElementoEditado.Id;
        }


        public get SelectorDeIvaActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.selectorDeIvaR) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.selectorDeIvaR) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.selectorDeIvaR) as HTMLSelectElement;
        }

        public get PorcentageIvaActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.iva) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.iva) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.iva) as HTMLInputElement;
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public AplicarTipoDeLinea() {
            let modal = this.EstaCreandoUnaLinea ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let tipoDeLineaCtrl = ApiControl.BuscarControl(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.tipoDeLinea, true) as HTMLSelectElement;
            let tipoDeLinea: number = tipoDeLineaCtrl.selectedIndex;

            switch (tipoDeLinea) {
                case 0: {
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.concepto);
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unitario);
                    ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.concepto);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unidad);
                    break;
                }
                case 1: {
                    tipoDeLineaCtrl.selectedIndex = 0;
                    MensajesSe.Error("AplicarTipoDeLinea", "Un planificador de ventas no acepta partidas alzadas");
                    break;
                }
                case 2: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unitario);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.unidad);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${tipoDeLinea} a la modal de crear una línea de un planificador`);
                    break;
                }
            }
            const mostrar = tipoDeLinea === 2;
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 2, mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 3, !mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 4, !mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 5, !mostrar);
            ApiPanel.MostrarOcultarFilaDtoSi(modal, 6, !mostrar);

            this.plv_CalcularImportesDeLinea_interno(modal);
        }

        public plv_CalcularImportesDeLinea_interno(modal: HTMLDivElement) {
            let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.cantidad).value);
            let venta = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.venta).value);

            let impSinDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteSinDto) as HTMLInputElement;
            let impDeDto = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteDeDto) as HTMLInputElement;
            let ImporteDeIva = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteDeIva) as HTMLInputElement;
            let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.ImporteDeLinea) as HTMLInputElement;

            let importeSinDescuento: number = cantidad * venta;
            if (importeSinDescuento !== 0) {
                let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.descuentoPorLinea).value);
                AsignarValor(impSinDto, importeSinDescuento.toString());
                AsignarValor(impDeDto, (importeSinDescuento * descuento / 100).toString());
                let impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

                let iva = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.PlanificadorDeVenta.linea.iva).value);
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

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Juridico.Planificador.GenerarPlanificaciones) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas()
                return true;
            }
            return false;
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            Juridico.Plv_AjustarControlesDeEdicion(this.PanelDeEditar, this.Registro);

            setTimeout(() => {
                this.MapearFechasDelPlanificador();
            }, 2000);
        }

        private async MapearFechasDelPlanificador() {
            var inicio = ApiControl.BuscarSelectorDeFecha(this.PanelDelDto, ltrPropiedades.Juridico.PlanificadorDeVenta.inicio);
            MapearAlControl.Fecha(inicio, ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.PlanificadorDeVenta.inicio));
            var hasta = ApiControl.BuscarSelectorDeFecha(this.PanelDelDto, ltrPropiedades.Juridico.PlanificadorDeVenta.hasta);
            MapearAlControl.Fecha(hasta, ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.PlanificadorDeVenta.hasta));
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Juridico.PlfdorDeVenta.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
            }
        }
    }


}

