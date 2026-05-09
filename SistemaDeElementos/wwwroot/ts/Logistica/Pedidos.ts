namespace Logistica {

    export function CrearCrudDePedidos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDePedidos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePedidos extends Crud.CrudMnt {

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
            this.crudDeCreacion = new CrudCreacionPedido(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPedido(this, idPanelEdicion);
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._IdDeUnidadDeMedida = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.UnidadDeMedida);
            this._IdDeNaturaleza = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.Naturaleza);
            this._ClaseDeUnitario = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.ClaseDeUnitario);
            this._TipoDeLinea = mapIndicadores.get(ltrPropiedades.Logistica.Pedido.Indicadores.TipoDeLinea);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            super.ModalDePedirDatos_Aceptar(modal);
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

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
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
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.clase);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                    ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                    break;
                }
                case 1: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.unitario);
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.precio);
                    ApiControl.DesbloquearListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.clase);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                    ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                    if (this.EstaCreandoUnaLinea) {
                        if ((this.CrudDeMnt as CrudDePedidos).Naturaleza > 0) {
                            var SelectorNaturaleza = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.naturaleza);
                            MapearAlControl.ListaDeElementos(SelectorNaturaleza, new Array<ClausulaDeFiltrado>(), (this.CrudDeMnt as CrudDePedidos).Naturaleza, null);
                        }
                        if ((this.CrudDeMnt as CrudDePedidos).UnidadDeMedida > 0) {
                            var SelectorUnidad = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Logistica.Pedido.linea.unidad);
                            MapearAlControl.ListaDeElementos(SelectorUnidad, new Array<ClausulaDeFiltrado>(), (this.CrudDeMnt as CrudDePedidos).UnidadDeMedida, null);
                        }
                        if (Definido((this.CrudDeMnt as CrudDePedidos).ClaseDeUnitario)) {
                            var SelectorDeClase = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.clase);
                            MapearAlControl.ListaDeValores(SelectorDeClase, (this.CrudDeMnt as CrudDePedidos).ClaseDeUnitario);
                        }
                    }
                    break;
                }
                case 2: {
                    ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.concepto);
                    ApiControl.BloquearListaDinamicaPorPropiedad(modal, ltrPropiedades.Logistica.Pedido.linea.unitario);
                    ApiControl.BloquearListaDeValores(modal, ltrPropiedades.Logistica.Pedido.linea.clase);
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


}