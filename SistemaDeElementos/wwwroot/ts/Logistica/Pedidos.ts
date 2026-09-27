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
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Logistica.Pedidos.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            // "Pedir el" mientras el pedido se está cumplimentando o aprobando (aún no se ha pedido de verdad);
            // en el resto de etapas ya se ha pedido, así que la etiqueta pasa a "Pedido el"
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.Etapas);
            let seEstaCumplimentando = EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Cumplimentacion);
            let seEstaCumplimentandoOAprobando =
                seEstaCumplimentando ||
                EstaElEnumerado(etapas, enumEtapasDePedido, enumEtapasDePedido.PED_Etapa_De_Aprobacion);
            if (!seEstaCumplimentandoOAprobando) {
                ApiControl.BuscarEtiqueta(panel, ltrPropiedades.Logistica.Pedido.PedidoEl).innerText = 'Pedido el';
            }

            // El fichero de pedido solo se puede añadir al crear o, en edición, mientras se está cumplimentando
            ApiControl.MostrarPropiedadSi(panel, ltrPropiedades.Logistica.Pedido.IdArchivoPedido, seEstaCumplimentando);
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

                        // Tarifa propuesta: la base imponible del proveedor con su iva soportado aplicado
                        let biPropuestoDelProveedor = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.BiPropuestoDelProveedor, 0));
                        if (biPropuestoDelProveedor > 0) {
                            let porcentajeIva = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Logistica.Pedido.PorcentajeIvaSoportadoDelProveedor, 0));
                            let tarifaConIva = biPropuestoDelProveedor * (1 + porcentajeIva / 100);
                            let precioCtrl = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Pedido.linea.precio) as HTMLInputElement;
                            AsignarValor(precioCtrl, tarifaConIva.toString());
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