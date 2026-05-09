namespace Gasto {

    let crudDeFacturas: CrudDeFacturasRec;
    let mapeandoDatosProveedor: boolean;

    export function CrearCrudDeFacturasRec(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Gasto.CrudDeFacturasRec(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
        crudDeFacturas = Crud.crudMnt as CrudDeFacturasRec;
    }
    export class CrudDeFacturasRec extends Crud.CrudMnt {

        private _IdDeUnidadDeMedida: number = 0;
        private _IdDeNaturaleza: number = 0;
        private _IdIva: number = 0;
        private _IdIrpf: number = 0;
        private _Concepto: string = undefined;
        private _ComoTratarLaFechaDeRecepcion: string = undefined;
        public get UnidadDeMedida(): number {
            return this._IdDeUnidadDeMedida;
        }
        public set UnidadDeMedida(value: number) {
            if (Numero(value) > 0)
                this._IdDeUnidadDeMedida = value;
            else {
                if (this.MapIndicadores.size > 0)
                    this._IdDeUnidadDeMedida = this.MapIndicadores.get(ltrPropiedades.Gasto.FacturaRec.Indicadores.UnidadDeMedida);
                else
                    this._IdDeUnidadDeMedida = 0;
            }
        }

        public get Naturaleza(): number {
            return this._IdDeNaturaleza;
        }
        public set Naturaleza(value: number) {
            if (Numero(value) > 0)
                this._IdDeNaturaleza = value;
            else {
                if (this.MapIndicadores.size > 0)
                    this._IdDeNaturaleza = this.MapIndicadores.get(ltrPropiedades.Gasto.FacturaRec.Indicadores.Naturaleza);
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

        public get IvaDelProveedor(): number {
            return this._IdIva;
        }
        public set IvaDelProveedor(value: number) {
            this._IdIva = value;
        }

        public get IrpfDelProveedor(): number {
            return this._IdIrpf;
        }
        public set IrpfDelProveedor(value: number) {
            this._IdIrpf = value;
        }

        public get ComoTratarLaFechaDeRecepcion(): string {
            return this._ComoTratarLaFechaDeRecepcion;
        }
        public get ModalImportarFarXml(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.Far_ImportarFarXml) as HTMLDivElement; }
        public get ModalImportarPrvXml(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.Far_ImportarPrvXml) as HTMLDivElement; }
        public get ModalCrearFarConIa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.Far_CrearFarConIa) as HTMLDivElement; }
        public get ModalCopiarFar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.CopiarFar) as HTMLDivElement; }
        public get ModalRectificarFar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.RectificarFar) as HTMLDivElement; }
        public get ModalParaRenombrar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.Renombrar) as HTMLDivElement; }
        public get ModalParaCambiarProveedor(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Gasto.FacturasRec.CambiarProveedor) as HTMLDivElement; }


        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionFacturaRec(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionFacturaRec(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            this.Expansor_InyectarAccesoIA();
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._IdDeUnidadDeMedida = mapIndicadores.get(ltrPropiedades.Gasto.FacturaRec.Indicadores.UnidadDeMedida);
            this._IdDeNaturaleza = mapIndicadores.get(ltrPropiedades.Gasto.FacturaRec.Indicadores.Naturaleza);
            this._ComoTratarLaFechaDeRecepcion = mapIndicadores.get(ltrPropiedades.Gasto.FacturaRec.Indicadores.ComoTratarLaFechaDeRecepcion);
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.CopiarFar) {
                let idModal = this.IdCrud + '-' + opcion;
                let id = this.InfoSelector.Seleccionados.length === 0 ? 0 : this.InfoSelector.Seleccionados[0].Id;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, id);
            }
            else if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.RectificarFar) {
                if (ObtenerPropiedad(this.InfoSelector.Seleccionados[0].Registro, ltrPropiedades.Gasto.FacturaRec.BaseImponible) <= 0)
                    MensajesSe.EmitirExcepcion("ProcesarOpcionMf", "No se puede hacer una rectificativa de una factura negativa");
                let idModal = this.IdCrud + '-' + opcion;
                let id = this.InfoSelector.Seleccionados.length === 0 ? 0 : this.InfoSelector.Seleccionados[0].Id;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, id);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.CancelarPreasientos)
                parametros.push(new Parametro(Ajax.Param.ids, this.InfoSelector.IdsSeleccionados));
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.Far_ImportarFarXml) {
                crudDeFacturas.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(crudDeFacturas.ModalImportarFarXml.id, 0);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.Far_ImportarPrvXml) {
                crudDeFacturas.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(crudDeFacturas.ModalImportarPrvXml.id, 0);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.Far_CrearFarConIa) {
                crudDeFacturas.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(crudDeFacturas.ModalCrearFarConIa.id, 0);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Gasto.FacturasRec);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.CancelarPreasientos) {
                MensajesSe.Info(peticion.resultado.consola);
                return true;
            }


            return Gasto.NavegarARelacionesDeFar(opcion, datosDeEntrada, ltrParametrosUrl.Gasto.IdFactura);
        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax): any {
            super.DespuesDeLeerFilaSeleccionada(peticion);

            let elemento = peticion.resultado.datos;
            if (Crud.crudMnt.InfoSelector.Cantidad == 1) {

                let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Gasto.FacturaRec.Etapas, false);

                let enAnulacion: boolean = EstaElEnumerado(etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_Anulada);
                let enDevolucion: boolean = EstaElEnumerado(etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_Devuelta);
                const sePuedeRectificar = !enAnulacion && !enDevolucion;
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(crudDeFacturas.ContenedorMenuIndividual, ltrMenus.eventosDeMf.Gasto.FacturasRec.RectificarFar, ltrMenus.enumOrigen.edicion, !sePuedeRectificar);
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);


            if (modal.id === this.ModalRectificarFar.id) {
                Gasto.Far_Rectificar(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.Gastos.FacturasRec}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalCopiarFar.id) {
                Gasto.Far_Generar(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.Gastos.FacturasRec}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === crudDeFacturas.ModalImportarFarXml.id) {
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Gasto.FacturaRec.ImportarFarXml, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        this.RestaurarPagina();
                        crudDeFacturas.InfoSelector.InsertarElemento(new Elemento(peticion.resultado.datos))
                        crudDeFacturas.IraEditar();
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === crudDeFacturas.ModalImportarPrvXml.id) {
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Gasto.FacturaRec.ImportarPrvXml, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        var idRegistro = peticion.resultado.datos.id;
                        var url = `${window.location.origin}/${ltrUrls.Terceros.Proveedor}?${ltrParametrosUrl.id}=${idRegistro}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === crudDeFacturas.ModalCrearFarConIa.id) {
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Gasto.FacturaRec.CrearFarConIa, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalParaRenombrar.id) {
                Gasto.RenombrarUnaFar(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        (this.crudDeEdicion as CrudEdicionFacturaRec).RecargarGridDeLineas();
                        (this.crudDeEdicion as CrudEdicionFacturaRec).RecargarGridDeTrazas();
                        (this.crudDeEdicion as CrudEdicionFacturaRec).RecargarValoresDeCabecera(this.crudDeEdicion.ElementoEditado.Id);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalParaCambiarProveedor.id) {
                Gasto.CambiarProveedor(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        (this.crudDeEdicion as CrudEdicionFacturaRec).RecargarGridDeTrazas();
                        (this.crudDeEdicion as CrudEdicionFacturaRec).RecargarValoresDeCabecera(this.crudDeEdicion.ElementoEditado.Id);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalImportarFarXml.id) {
            }
            if (modal.id === this.ModalImportarPrvXml.id) {
            }
            if (modal.id === this.ModalCopiarFar.id) {
                if (this.InfoSelector.Seleccionados.length === 1)
                    this.Far_ProponerFacturaParaCopiar(modal, this.InfoSelector.Seleccionados[0].Registro);
            }
            if (modal.id === this.ModalRectificarFar.id) {
                if (this.InfoSelector.Seleccionados.length === 1)
                    this.Far_MapearDatosDeLaRectificada(modal, this.InfoSelector.Seleccionados[0].Registro);
            }
            if (modal.id === this.ModalParaRenombrar.id) {
                ApiDelCrud.ProponerParaRenombrar(this.ModalParaRenombrar, this.crudDeEdicion.ElementoEditado.Registro);
            }
            if (modal.id === this.ModalParaCambiarProveedor.id) {
                ApiDelCrud.ProponerParaCambiarProveedor(this.ModalParaCambiarProveedor, this.crudDeEdicion.ElementoEditado.Registro);
            }
        }

        public Far_MapearDatosDeLaRectificada(modal: HTMLDivElement, registro: any) {
            let destino: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.expresion));
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Gasto.FacturaRec.Proveedor, ltrPropiedades.Gasto.FacturaRec.IdProveedor);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Gasto.FacturaRec.Numero);
            ApiDelCrud.MultiplicarPorMenos(modal, registro, ltrPropiedades.Gasto.FacturaRec.BaseImponible);
            ApiDelCrud.MultiplicarPorMenos(modal, registro, ltrPropiedades.Gasto.FacturaRec.TotalDelPago);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Elemento.Nombre);
            ApiDelCrud.ProponerEnAreaDeTexto(modal, registro, ltrPropiedades.Elemento.Descripcion);
        }

        public Far_ProponerFacturaParaCopiar(modal: HTMLDivElement, registro: any) {
            let destino: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.expresion));
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConTipo.Tipo, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Gasto.FacturaRec.Proveedor, ltrPropiedades.Gasto.FacturaRec.IdProveedor);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Gasto.FacturaRec.Expediente, ltrPropiedades.Gasto.FacturaRec.IdExpediente, false);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Gasto.FacturaRec.Contrato, ltrPropiedades.Gasto.FacturaRec.IdContrato, false);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Gasto.FacturaRec.Numero);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Gasto.FacturaRec.BaseImponible);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Gasto.FacturaRec.TotalDelPago);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Elemento.Nombre);
            ApiDelCrud.ProponerEnAreaDeTexto(modal, registro, ltrPropiedades.Elemento.Descripcion);
        }


        public MapearDatosJsonDesdeElVisor(json: object): boolean {
            if (!super.MapearDatosJsonDesdeElVisor(json)) return false;

            const proveedor = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Proveedor);
            const nif = LimpiarNif(ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Nif));
            if (Definido(nif) && Definido(proveedor)) {
                Ia_MapearProveedor(proveedor, nif, json);
            }

            const facturadaEl = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.FacturadaEl)
            Ia_MapearFechaFactura(facturadaEl);

            const venceEl = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.VenceEl)
            const claseDePago = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.ClaseDePago)
            Ia_MapearFechaVencimiento(venceEl, claseDePago);

            const total = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Total)
            Ia_MapearTotal(total);

            const bi = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Bi)
            Ia_MapearBi(bi);

            const numero = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Numero)
            Ia_MapearNumeroFactura(numero);

            this.MapearConceptoAsync(json);

            if (this.EstoyCreando) {
                const fomaDePago = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.FormaDePago)
                Ia_MapearPagoContado(claseDePago, fomaDePago);
            }

            const totalIva = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Iva)
            const totalIrpf = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Irpf)
            return true;

        }

        private async MapearConceptoAsync(json: any) {
            let intentos = 0;
            const maxIntentos = 5; // Número máximo de reintentos
            const intervalo = 1000; // Intervalo en milisegundos (1 segundo)

            while (mapeandoDatosProveedor && intentos < maxIntentos) {
                await new Promise(resolve => setTimeout(resolve, intervalo));
                intentos++;
            }

            if (!mapeandoDatosProveedor) {
                const concepto = ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Concepto);
                Ia_MapearConcepto(concepto);
            } else {
                console.error("No se pudo mapear el concepto después de varios intentos.");
            }
        }
    }

    export class CrudCreacionFacturaRec extends Crud.CrudCreacion {

        public get idProveedor() {
            var proveedor = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Proveedor);
            return Numero(proveedor.getAttribute(atListasDinamicas.idSeleccionado));
        }

        private _biPropuesta: HTMLInputElement;
        public get BiPropuesta(): HTMLInputElement {
            if (!Definido(this._biPropuesta)) {
                this._biPropuesta = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.BaseImponible) as HTMLInputElement;
            }
            return this._biPropuesta;
        }

        private _totaldelpago: HTMLInputElement;
        public get TotalDelPago(): HTMLInputElement {
            if (!Definido(this._totaldelpago)) {
                this._totaldelpago = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.TotalDelPago) as HTMLInputElement;
            }
            return this._totaldelpago;
        }

        private _modoDePago: HTMLSelectElement;
        public get ModoDePago(): HTMLSelectElement {
            if (!Definido(this._modoDePago)) {
                this._modoDePago = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.ModoDePago) as HTMLSelectElement;
            }
            return this._modoDePago;
        }

        private _selectorDeIva: HTMLSelectElement
        public get SelectorDeIva(): HTMLSelectElement {
            if (!Definido(this._selectorDeIva))
                this._selectorDeIva = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorIva);
            return this._selectorDeIva;
        }

        private _selectorDeIrpf: HTMLSelectElement
        public get SelectorDeIrpf(): HTMLSelectElement {
            if (!Definido(this._selectorDeIrpf))
                this._selectorDeIrpf = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorIrpf);
            return this._selectorDeIrpf;
        }

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            var pagada = ApiControl.BuscarCheck(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Pagada);
            this.AplicarCheckDePago(pagada);
            this.MapearNaturalezaUnidaDeMedida();
        }

        public MapearNaturalezaUnidaDeMedida() {

            if (crudDeFacturas.Naturaleza > 0) {
                var SelectorNaturaleza = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorNaturaleza);
                MapearAlControl.ListaDeElementos(SelectorNaturaleza, new Array<ClausulaDeFiltrado>(), crudDeFacturas.Naturaleza, null);
            }
            if (crudDeFacturas.UnidadDeMedida > 0) {
                var SelectorUnidad = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorUnidad);
                MapearAlControl.ListaDeElementos(SelectorUnidad, new Array<ClausulaDeFiltrado>(), crudDeFacturas.UnidadDeMedida, null);
            }
        }

        public MapearIvaIrpfProveedor() {

            if (crudDeFacturas.IvaDelProveedor > 0) {
                var SelectorIva = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorIva);
                MapearAlControl.ListaDeElementos(SelectorIva, new Array<ClausulaDeFiltrado>(), crudDeFacturas.IvaDelProveedor, null);
            }
            if (crudDeFacturas.IrpfDelProveedor > 0) {
                var SelectorIrpf = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorIrpf);
                MapearAlControl.ListaDeElementos(SelectorIrpf, new Array<ClausulaDeFiltrado>(), crudDeFacturas.IrpfDelProveedor, null);
            }
        }

        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeLeerDatosParaInicializarCreacion(peticion)
            ApiControl.BloquearListaDinamicaSi(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Contrato, this.idProveedor === 0)
            const idArchivo = ObtenerParametroUrl(ltrPropiedades.Gasto.FacturaRec.IdArchivo, 0, false);
            if (idArchivo > 0) {
                const archivo = ApiControl.BuscarSelectorDeArchivos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.IdArchivo, atControl.propiedad) as HTMLInputElement;
                const barraHtml = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
                ApiDePeticiones.TrasSubirUnArchivoAlSelectorDeFichero(archivo, barraHtml, idArchivo);
            }
        }

        public AplicarModoPagoTarjeta(idTarjeta: number = 0) {
            var tarjetas = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Tarjeta)
            ApiControl.BloquearLaLista(tarjetas, false);
            var restringirPor = new Array<ClausulaDeFiltrado>();
            restringirPor.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Sociedad.Tarjeta.Activa, atCriterio.igual, true))
            MapearAlControl.ListaDeElementos(tarjetas, new Array<ClausulaDeFiltrado>(), idTarjeta, null, restringirPor);
            if (idTarjeta > 0)
                ApiControl.ResaltarControl(tarjetas, ltrCss.Resalto.Verde);
        }

        public AplicarModoPagoDomiciliacion(idCtaBanDeMiSociedad: number = 0) {
            var cuentas = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.DomiciliadaEn)
            ApiControl.BloquearLaLista(cuentas, false);
            var restringirPor = new Array<ClausulaDeFiltrado>();
            restringirPor.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Activa, atCriterio.igual, true))
            MapearAlControl.ListaDeElementos(cuentas, new Array<ClausulaDeFiltrado>(), idCtaBanDeMiSociedad, null, restringirPor);
            if (idCtaBanDeMiSociedad > 0)
                ApiControl.ResaltarControl(cuentas, ltrCss.Resalto.Verde);
        }

        public AplicarCheckDePago(check: HTMLInputElement) {
            var modoDePago = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.ModoDePago)
            ApiControl.BloquearLaLista(modoDePago, !check.checked);
            if (!check.checked) ApiControl.QuitarResalto(modoDePago);
            modoDePago.selectedIndex = 0;
            this.AplicarModoDePagoContado(check);
        }

        public AplicarModoDePagoContado(check: HTMLInputElement) {
            var tarjetas = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Tarjeta);
            var cuentas = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.DomiciliadaEn);
            //ApiControl.BlanquearListaDeElementos(cuentas);
            //ApiControl.BlanquearListaDeElementos(tarjetas);
            ApiControl.BloquearLaLista(cuentas, true);
            ApiControl.BloquearLaLista(tarjetas, true);
        }

        public async AnalizarFactura(): Promise<boolean> {
            if (!Definido(this.IdArchivoMostrado))
                return true;

            if (!this.IntercambiarFrameResumidoDelVisor()) {
                this.SetFrameDelFicheroOriginal();
                const resultado = await ApiDelCrud.ProcesarRenderizar(this.CrudDeMnt, this.IdArchivoEnElSelector, ltrEventos.Edicion.FacturasRec.Analizar);
                if (resultado) {
                    ApiControl.IntercambiaCss(this.ImageIaArchivo, ltrCss.crud.panelCreacion.ImagenIa, ltrCss.crud.panelCreacion.ImagenArchivo);
                    this.SetFrameDelFicheroResumido();
                    return true;
                }
                this._iframeDelFicheroOriginal = undefined;
                this._contenidoPdfOriginal = undefined;
                return false;
            }
            return true
        }

        public TrasBlanquearCg(): void {
            super.TrasBlanquearCg();
            const control: HTMLInputElement = ApiControl.BuscarCheck(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Pagada)
            control.checked = false;
            (crudDeFacturas.crudDeCreacion as CrudCreacionFacturaRec).AplicarCheckDePago(control);
        }

        public CalcularTotalAPagar(bi: number): number|undefined {
            const iva = OpcionesDeLasListas.ObtenerObjeto(this.SelectorDeIva);
            const irpf = OpcionesDeLasListas.ObtenerObjeto(this.SelectorDeIrpf);
            let total: number = undefined;
            if (Definido(iva) || Definido(irpf)) {
                total = bi + (bi * Numero(ObtenerPropiedad(iva, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje, 0)) / 100)
                    - (bi * Numero(ObtenerPropiedad(irpf, ltrPropiedades.Maestros.Contabilidad.Irpf.Porcentaje, 0)) / 100);
            }
            return total;
        }
    }

    export class CrudEdicionFacturaRec extends Crud.CrudEdicion {

        private idGridDeLineas = 'lineasdeunafar'.toLocaleLowerCase();
        private idModalDeCreacionDeLinea: string = 'lineasdeunafar';
        public get IdReferenciaCrearPago(): string {
            return this._idPanelEdicion + '-pagos-' + enumPostfijoControl.CrearPost;
        }
        public get GridDeLineas(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Gasto.LineasDeFactura);
        }
        public get PanelDeLineas(): HTMLDivElement {
            let id = this._idPanelEdicion + '-' + this.idModalDeCreacionDeLinea;
            return document.getElementById(id) as HTMLDivElement;
        }
        public get IdReferenciaCrearLinea(): string {
            return this._idPanelEdicion + `-${this.idGridDeLineas}-mcr-${enumPostfijoControl.Referencia}`;
        }
        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(this.idModalDeCreacionDeLinea);
        }
        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(this.idGridDeLineas);
        }
        public get TablaDeLineas(): HTMLDivElement {
            return this.TablaDelDetalle(this.idGridDeLineas) as HTMLDivElement;
        }
        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }
        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalParaEditarRelacion(this.idGridDeLineas));
        }
        public get SelectorDeIvaActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva) as HTMLSelectElement;
        }
        public get SelectorDeIrpfActivo(): HTMLSelectElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarListaDeElementos(this.PanelDeEditar, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf) as HTMLSelectElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarListaDeElementos(this.ModalDeCreacionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf) as HTMLSelectElement :
                ApiControl.BuscarListaDeElementos(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf) as HTMLSelectElement;
        }
        public get PorcentageIvaActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIva) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIva) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIva) as HTMLInputElement;
        }
        public get PorcentageIrpfActivo(): HTMLInputElement {
            if (!this.EstaCreandoUnaLinea && !this.EstaEditandoUnaLinea)
                return ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIrpf) as HTMLInputElement;

            return this.EstaCreandoUnaLinea ?
                ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIrpf) as HTMLInputElement :
                ApiControl.BuscarEditor(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIrpf) as HTMLInputElement;
        }
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected EjecutarAccionesDelModulo(accion: string): boolean {
            if (accion === ltrEventos.Edicion.FacturasRec.Analizar) {
                if (this.CrudDeMnt.EstoyCreando)
                    (this.CrudDeMnt.crudDeCreacion as CrudCreacionFacturaRec).AnalizarFactura();
                else
                    this.AnalizarFactura();
                return true;
            }
            return false;
        }

        private AnalizarFactura(): void {
            if (!Definido(this.IdArchivoMostrado))
                return;

            ApiDelCrud.ProcesarRenderizar(this.CrudDeMnt, this.IdArchivoMostrado, ltrEventos.Edicion.FacturasRec.Analizar);
        }

        public AplicarClaseParaEditarLinea(claseDeLinea: number, lineaEditable: boolean) {

            if (claseDeLinea == 0) {
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            }
            else if (claseDeLinea == 1 || claseDeLinea == 2) {
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            }
            else if (claseDeLinea == 3) {
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            }
            else if (claseDeLinea == 4) {
                ApiPanel.MostrarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
                ApiPanel.OcultarFila(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            }
            ApiControl.BloquearListaDeElementoSi(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.selectorNaturaleza, !lineaEditable || claseDeLinea >= 3);
            ApiControl.BloquearListaDeElementoSi(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Unidad, !lineaEditable || claseDeLinea >= 3);
            ApiControl.BloquearEditorPorPropiedad(this.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Cantidad, !lineaEditable || claseDeLinea >= 3);
        }

        public AplicarClaseParaNuevaLinea(claseDeLinea: number) {
            let modal = this.ModalDeCreacionDeLineas;
            this.ResetearTrasSeleccinarClase();
            switch (claseDeLinea) {
                case 0: {
                    this.AplicarClaseBISinIva(modal);
                    break;
                }
                case 1: {
                    this.AplicarClaseBIConIva(modal);
                    break;
                }
                case 2: {
                    this.AplicarClaseBIConIva(modal);
                    break;
                }
                case 3: {
                    this.AplicarClaseLineaIva(modal);
                    break;
                }
                case 4: {
                    this.AplicarClaseLineaIrpf(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${claseDeLinea} a la modal de crear una línea de una factura`);
                    break;
                }
            }
            this.CalcularImportesDeLinea();
        }

        private AplicarClaseBISinIva(modal: HTMLDivElement): void {
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf, true);
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva, true);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorNaturaleza);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.Unidad);
            ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.Cantidad);
            this.ProponerValores(modal);
            var bi = ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible);
            AsignarValor(bi, "0");
            ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto, false);
            Gasto.far_InicializarConcepto();

        }

        private AplicarClaseBIConIva(modal: HTMLDivElement): void {
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf, true);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorNaturaleza);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.Unidad);
            ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.Cantidad);
            this.ProponerValores(modal);
            var bi = ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible);
            AsignarValor(bi, "0");
            ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto, false);
            Gasto.far_InicializarConcepto();
        }

        public ProponerValores(modal: HTMLDivElement): void {
            if (crudDeFacturas.Naturaleza > 0) {
                var SelectorNaturaleza = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.FacturaRec.SelectorNaturaleza);
                if (Definido(SelectorNaturaleza) && !SelectorNaturaleza.disabled)
                    MapearAlControl.ListaDeElementos(SelectorNaturaleza, new Array<ClausulaDeFiltrado>(), crudDeFacturas.Naturaleza, null);
            }
            if (crudDeFacturas.UnidadDeMedida > 0) {
                var SelectorUnidad = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.FacturaRec.SelectorUnidad);
                if (Definido(SelectorUnidad) && !SelectorUnidad.disabled)
                    MapearAlControl.ListaDeElementos(SelectorUnidad, new Array<ClausulaDeFiltrado>(), crudDeFacturas.UnidadDeMedida, null);
            }
            if (crudDeFacturas.IvaDelProveedor > 0) {
                var selectordeIva = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.FacturaRec.SelectorIva);
                if (Definido(selectordeIva) && !selectordeIva.disabled)
                    MapearAlControl.ListaDeElementos(selectordeIva, new Array<ClausulaDeFiltrado>(), crudDeFacturas.IvaDelProveedor, null);
            }
            if (crudDeFacturas.IrpfDelProveedor > 0) {
                var selectorDeIrpf = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Gasto.FacturaRec.IrpfDeLinea);
                if (Definido(selectorDeIrpf) && !selectorDeIrpf.disabled)
                    MapearAlControl.ListaDeElementos(selectorDeIrpf, new Array<ClausulaDeFiltrado>(), crudDeFacturas.IrpfDelProveedor, null);
            }
        }

        private AplicarClaseLineaIva(modal: HTMLDivElement): void {
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf, true);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorNaturaleza, true);
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.Unidad, true);
            ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.Cantidad, true);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            this.ProponerValores(modal);
            var bi = ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible);
            AsignarValor(bi, "0");
            var concepto = ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto, true);
            concepto.value = "";
            var etiquetaBI = ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible);
            etiquetaBI.title = ltrTitle.Gasto.FacturaRec.linea.BaseImponible.SoloIva;
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
        }

        private AplicarClaseLineaIrpf(modal: HTMLDivElement): void {
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva, true);
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorNaturaleza, true);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion);
            ApiPanel.OcultarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            ApiPanel.MostrarFila(modal, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf);
            ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.Gasto.FacturaRec.linea.Unidad, true);
            ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.Cantidad, true);
            this.ProponerValores(modal);
            var bi = ApiControl.DesbloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible);
            AsignarValor(bi, "0");
            var concepto = ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto, true);
            concepto.value = "";
            var etiquetaBI = ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible);
            etiquetaBI.title = ltrTitle.Gasto.FacturaRec.linea.BaseImponible.SoloIRPF;
        }

        public CalcularImportesDeLinea() {
            let modal = ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas) ? this.ModalDeCreacionDeLineas : this.ModalDeEdicionDeLineas;
            let claseDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Gasto.FacturaRec.linea.Clase, true) as HTMLSelectElement;
            switch (claseDeLinea.selectedIndex) {
                case 0: {
                    this.CalcularBISinIva(modal);
                    break;
                }
                case 1: {
                    this.CalcularIva(modal);
                    break;
                }
                case 2: {
                    this.CalcularIva(modal);
                    break;
                }
                case 3: {
                    this.CalcularIvaSobreLaBI(modal);
                    break;
                }
                case 4: {
                    this.CalcularIrpf(modal);
                    break;
                }
                default: {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No está definido como aplicar el tipo de línea ${claseDeLinea} a la modal de crear una línea de una factura`);
                    break;
                }
            }
        }

        public ResetearTrasSeleccinarClase() {
            var panel = this.ModalDeCreacionDeLineas;
            let bi = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible) as HTMLInputElement;
            let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Gasto.FacturaRec.linea.selectorNaturaleza) as HTMLSelectElement;
            let selectorIva = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Gasto.FacturaRec.linea.selectorIva) as HTMLSelectElement;
            let selectorIrpf = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Gasto.FacturaRec.linea.selectorIrpf) as HTMLSelectElement;
            selectorIva.selectedIndex = 0;
            selectorIrpf.selectedIndex = 0;
            naturaleza.selectedIndex = 0;
            AsignarValor(bi, "");
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIva));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.ModalDeCreacionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.PorcentajeIrpf));

            AsignarValor(ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.FacturaRec.linea.ImporteDeIva), "");
            AsignarValor(ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.FacturaRec.linea.ImporteDeIrpf), "");
        }

        private CalcularIrpf(modal: HTMLDivElement) {
            let bi = ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible) as HTMLInputElement;
            let selectorIrpf = this.SelectorDeIrpfActivo;
            let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIrpf);
            let porcentaje = Numero(ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.Irpf.Porcentaje));
            AsignarValor(ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.ImporteDeIrpf), `${porcentaje * Numero(bi.value) / 100}`);
            var concepto = ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            concepto.value = Definido(objeto) ? ObtenerPropiedad(objeto, literal.expresion) : "";
        }

        private CalcularIvaSobreLaBI(modal: HTMLDivElement) {
            this.CalcularIva(modal);
            var concepto = ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.concepto);
            let objeto = OpcionesDeLasListas.ObtenerObjeto(this.SelectorDeIvaActivo);
            concepto.value = Definido(objeto) ? ObtenerPropiedad(objeto, literal.expresion) : "";
        }

        private CalcularIva(modal: HTMLDivElement) {
            let bi = ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible) as HTMLInputElement;
            let objeto = OpcionesDeLasListas.ObtenerObjeto(this.SelectorDeIvaActivo);
            let porcentaje = Numero(ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaS.Porcentaje));
            AsignarValor(ApiControl.BuscarEditor(modal, ltrPropiedades.Gasto.FacturaRec.linea.ImporteDeIva), `${porcentaje * Numero(bi.value) / 100}`);
        }

        private CalcularBISinIva(modal: HTMLDivElement) { }

        public RecargarGridDeLineas() {
            MapearAlGrid.MapearGridDeDetalle(this.GridDeLineas, this.CrudDeMnt.IdNegocio, Numero(ObtenerPropiedad(this.Registro, literal.id)), this.CrudDeMnt.Guid);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Gasto.FacturasRec.Lineas)) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas();
            }
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            let enCumplimentacion: boolean = EstaElEnumerado(this.Etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_De_Cumplimentacion);
            let enCotabilizacion: boolean = EstaElEnumerado(this.Etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_De_Contabilizacion);
            let enPago: boolean = EstaElEnumerado(this.Etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_De_Pago);
            let enAnulacion: boolean = EstaElEnumerado(this.Etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_Anulada);
            let enDevolucion: boolean = EstaElEnumerado(this.Etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_Devuelta);

            ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Rectificada);
            if (!enCumplimentacion) ModoAcceso.DeshabilitarRef(this.PanelDeEditar, this.IdReferenciaCrearLinea);
            if (enCumplimentacion && this.EsGestor) {
                ApiPanel.MostrarFila(this.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.BICalculada);
                if (!this.CrudDeMnt.HayRestrictorPor(ltrPropiedades.Gasto.FacturaRec.IdContrato))
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Contrato);
                else
                    ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Contrato)
            }
            else {
                //ApiPanel.OcultarFila(this.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.BICalculada);
                ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Contrato);
            }

            if (!enCotabilizacion && !enCumplimentacion) {
                ModoAcceso.AjustarOpcionesDeMenu(panel, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
                ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
            }

            if ((enPago || enCumplimentacion) && ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso)) {
                //ApiControl.HabilitarReferenciaPost(this.PanelDeEditar, this.IdReferenciaCrearPago, ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, this.ModoDeAcceso));
                ApiControl.DesbloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Gasto.FacturasRec.Pagos));
            }
            else {
                //ApiControl.HabilitarReferenciaPost(this.PanelDeEditar, this.IdReferenciaCrearPago, false);
                ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Gasto.FacturasRec.Pagos));
            }

            if (enCotabilizacion) {
                ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                var fecha = ApiControl.DesbloquearSelectorDeFechaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Gasto.FacturaRec.ContabilizadaEl);
                if (!Definido(fecha.value)) MapearAlControl.FechaDate(fecha, new Date());
            }
            else {
                ApiControl.BloquearSelectorDeFechaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Gasto.FacturaRec.ContabilizadaEl);
            }

            const puedeModificarDatos = Registro.EsAdministrador() || (this.EsInterventorSinEstado && !enAnulacion && !enDevolucion)

            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.FacturasRec.Renombrar, ltrMenus.enumOrigen.edicion, !puedeModificarDatos);
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.FacturasRec.CambiarProveedor, ltrMenus.enumOrigen.edicion, !this.EsInterventorSinEstado || !enCumplimentacion);
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.FacturasRec.QuitarContrato, ltrMenus.enumOrigen.edicion, !this.EsInterventorSinEstado);
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.FacturasRec.QuitarExpediente, ltrMenus.enumOrigen.edicion, !this.EsInterventorSinEstado);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.FacturasRec.GenerarPreasieto, ltrMenus.enumOrigen.edicion, Registro.EsAdministrador() && !enAnulacion && !enDevolucion);

        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.QuitarContrato) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas()
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.QuitarExpediente) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas()
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.Renombrar) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDeFacturasRec).ModalParaRenombrar.id, this.ElementoEditado.Id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.CambiarProveedor) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDeFacturasRec).ModalParaCambiarProveedor.id, this.ElementoEditado.Id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Gasto.FacturasRec.GenerarPreasieto) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas()
                return true;
            }
            return false;
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
            var editor = (crudDeFacturas.crudDeEdicion as CrudEdicionFacturaRec);
            if (modalDeEdicion.id === editor.ModalDeEdicionDeLineas.id) {
                this.TrasMapearModalDeLineas();
            }
        }

        private TrasMapearModalDeLineas(): void {
            var editor = (crudDeFacturas.crudDeEdicion as CrudEdicionFacturaRec);
            if (editor.EstaTerminada || editor.EstaCancelada) {
                //ModoAcceso.AplicarloAlPanel(editor.ModalDeEdicionDeLineas, ModoróximoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                return;
            }
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.FacturaRec.Etapas, false);
            let enCumplimentacion: boolean = EstaElEnumerado(etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_De_Cumplimentacion);
            //let enCotabilizacion: boolean = EstaElEnumerado(etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_De_Contabilizacion);
            let esGestor: boolean = ModoAcceso.EsGestor(this.ModoDeAcceso);
            ModoAcceso.AplicarloAlPanel(editor.ModalDeEdicionDeLineas, enCumplimentacion && esGestor ? ModoAcceso.enumModoDeAccesoDeDatos.Gestor : ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);

            var lineaEditable = enCumplimentacion && esGestor && !ObtenerPropiedad(editor.Registro, ltrPropiedades.Gasto.FacturaRec.EsIncorporada);
            ApiControl.BloquearEditorPorPropiedad(editor.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.BaseImponible, !lineaEditable);
            ApiControl.BloquearEditorPorPropiedad(editor.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.concepto, !lineaEditable);
            ApiControl.BloquearAreaDeTextoPorPropiedad(editor.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Anotacion, !lineaEditable);
            var clase = ApiControl.BuscarListaDeValores(editor.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Clase);
            ApiControl.BloquearListaDeValores(editor.ModalDeEdicionDeLineas, ltrPropiedades.Gasto.FacturaRec.linea.Clase)
            editor.AplicarClaseParaEditarLinea(clase.selectedIndex, lineaEditable);
        }

        public MapearDatosJsonDesdeElVisor(json: object): boolean {
            if (!super.MapearDatosJsonDesdeElVisor(json)) return false;

            let enCumplimentacion: boolean = EstaElEnumerado(this.Etapas, enumEtapasDeFacturaRec, enumEtapasDeFacturaRec.FAR_Etapa_De_Cumplimentacion);
            if (!enCumplimentacion) {
                MensajesSe.Error("MapearDatosJsonDesdeElVisor", "No se puede mapear los datos de una factura por no estar cumplimentándose");
                return false;
            }
            return true;
        }
    }

    export function NavegarARelacionesDeFar(opcion: string, datosDeEntrada: Parametros, idRestrictor: string): boolean {

        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Gasto.FacturasRec.IrAContratos:
                urlDestino = `${window.location.origin}/${ltrUrls.Juridico.ContratosDeCompra}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Gasto.FacturasRec.IrAExpedientes:
                urlDestino = `${window.location.origin}/${ltrUrls.Administracion.Expediente}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Gasto.FacturasRec.IrAPagos:
                urlDestino = `${window.location.origin}/${ltrUrls.Gastos.Pagos}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }

    export function Far_Tras_Seleccionar_Proveedor(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        const proveedor = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (ApiPanel.ModalAbierta(crudDeFacturas.ModalImportarFarXml)) {
            MapearDatosParaImportar(proveedor);
            return;
        }

        if (crudDeFacturas.EstoyEditandoConsultando || crudDeFacturas.EstoyCreando) {
            var idSeleccionado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
            ApiControl.BloquearListaDinamicaSi(
                crudDeFacturas.EstoyEditandoConsultando ? crudDeFacturas.crudDeEdicion.PanelDelDto : crudDeFacturas.crudDeCreacion.PanelDeCrear,
                ltrPropiedades.Gasto.FacturaRec.Contrato,
                idSeleccionado === 0);

            if (!Definido(proveedor) && idSeleccionado > 0) {

                ApiDePeticiones.LeerElementoPorId(crudDeFacturas, ltrControladores.Terceros.Proveedores, idSeleccionado, new Array<Parametro>(), idSeleccionado)
                    .then((peticion) => {
                        AplicarDatosDeUnProveedor(peticion.resultado.datos);
                    })
                    .catch((peticion) => {
                        mapeandoDatosProveedor = false;
                        ApiDePeticiones.EmitirError(peticion);
                    });
            }
            else AplicarDatosDeUnProveedor(proveedor);


            return;
        }

        if (crudDeFacturas.EstoyEnMantenimiento)
            return;

        MensajesSe.EmitirExcepcion('Far_Tras_Seleccionar_Proveedor', 'No se ha definido donde afectar la selección del proveedor');
    }

    function AplicarDatosDeUnProveedor(proveedor: any) {

        if (crudDeFacturas.EstoyCreando && crudDeFacturas.crudDeCreacion.MapeandoPlantilla) {
            return;
        }

        crudDeFacturas.UnidadDeMedida = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.IdUnidad, undefined, false);
        crudDeFacturas.Naturaleza = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.IdNaturaleza, undefined, false);
        crudDeFacturas.IvaDelProveedor = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.IdIva, undefined, false);
        crudDeFacturas.IrpfDelProveedor = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.IdIrpf, undefined, false);
        crudDeFacturas.Concepto = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.Concepto, undefined, false);
        if (crudDeFacturas.EstoyCreando) {
            const creador = (crudDeFacturas.crudDeCreacion as CrudCreacionFacturaRec);
            crudDeFacturas.crudDeCreacion.ControlDeNombre.value = crudDeFacturas.Concepto;
            creador.MapearNaturalezaUnidaDeMedida();
            creador.MapearIvaIrpfProveedor();
            ApiControl.ResaltarControl(crudDeFacturas.crudDeCreacion.ControlDeNombre, ltrCss.Resalto.Verde);
            if (crudDeFacturas.Naturaleza > 0) ApiControl.ResaltarControl(ApiControl.BuscarListaDeElementos(creador.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorNaturaleza), ltrCss.Resalto.Verde);
            if (crudDeFacturas.UnidadDeMedida > 0) ApiControl.ResaltarControl(ApiControl.BuscarListaDeElementos(creador.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.SelectorUnidad), ltrCss.Resalto.Verde);


            const idCg: number = Numero(ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.idcgPropuesto, 0, false));
            if (idCg > 0) {
                const cg: string = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.cgPropuesto, '', false);
                ApiListaDinamica.AsignarValorConResaltado(creador.Cg, idCg, cg,ltrCss.Resalto.Verde);
            }

            const idTipo: number = Numero(ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.idtipoPropuesto, 0, false));
            if (idTipo > 0) {
                const tipo: string = ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.tipoPropuesto, '', false);
                ApiListaDinamica.AsignarValorConResaltado(creador.Tipo, idTipo, tipo, ltrCss.Resalto.Verde);
            }

            const bi: number = Numero(ObtenerPropiedad(proveedor, ltrPropiedades.Terceros.Proveedor.biPropuesto, 0, false));

            if (crudDeFacturas.IvaDelProveedor > 0) {
                ApiControl.ResaltarControl(creador.SelectorDeIva, ltrCss.Resalto.Verde);
            }
            if (crudDeFacturas.IrpfDelProveedor > 0) {
                ApiControl.ResaltarControl(creador.SelectorDeIrpf, ltrCss.Resalto.Verde);
            }

            if (bi > 0) {
                AsignarValorConResalto(creador.BiPropuesta, bi.toString(), ltrCss.Resalto.Verde);
                let total: number = creador.CalcularTotalAPagar(bi);                
                if (total !== 0) {
                    AsignarValorConResalto(creador.TotalDelPago, total.toString(), ltrCss.Resalto.Verde);
                }
            }

            const modoDePago = ObtenerPropiedad(proveedor, ltrPropiedades.Gasto.FacturaRec.ModoDePago)
            if (Definido(modoDePago)) {
                const control: HTMLInputElement = ApiControl.BuscarCheck(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Pagada)
                control.checked = true;
                const modo = creador.ModoDePago;
                ApiControl.DesbloquearLaListaDeValores(modo);
                MapearAlControl.ListaDeValoresConResalto(modo, modoDePago, ltrCss.Resalto.Verde);
                if (modoDePago === enumModoDePagoContado.Tarjeta) {
                    const idTarjeta = ObtenerPropiedad(proveedor, ltrPropiedades.Gasto.FacturaRec.IdTarjeta, 0);
                    creador.AplicarModoPagoTarjeta(idTarjeta);
                }
                else if (modoDePago === enumModoDePagoContado.Domiciliacion) {
                    const idCuentaDomi = ObtenerPropiedad(proveedor, ltrPropiedades.Gasto.FacturaRec.IdDomiciliadaEn, 0);
                    creador.AplicarModoPagoDomiciliacion(idCuentaDomi);
                }
            }
        }
        mapeandoDatosProveedor = false;
    }

    export function Far_Tras_Blanquear_Proveedor(idLista: string): void {
        if (ApiPanel.ModalAbierta(crudDeFacturas.ModalImportarFarXml))
            MapearDatosParaImportar(undefined);
        else if (crudDeFacturas.EstoyEditandoConsultando) {
            ApiControl.BloquearListaDinamicaPorPropiedad(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Contrato);
        }
        else if (crudDeFacturas.ModoTrabajo === enumModoTrabajo.creando) {
            ApiControl.BloquearListaDinamicaPorPropiedad(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Contrato);
            ApiDelCrud.QuitarResaltos(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrCss.Resalto.Verde);
        }
        else
            MensajesSe.EmitirExcepcion('Far_Tras_Blanquear_Proveedor', 'No se ha definido donde afectar la selección del proveedor');
    }

    export function Far_ProponerDatosDelaFarSeleccionada() {
        let modal = crudDeFacturas.ModalCopiarFar;
        let far: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
        let idFar: number = Numero(far.getAttribute(atListasDinamicas.idSeleccionado));
        if (idFar > 0) {
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Gasto.FacturasRec, idFar, new Array<Parametro>(), idFar)
                .then((peticion) => crudDeFacturas.Far_ProponerFacturaParaCopiar(modal, peticion.resultado.datos))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

    }

    export function Far_InicializarModalDeCopiado() {
        let modal = crudDeFacturas.ModalCopiarFar;
        ApiPanel.BlanquearControlesDeIU(modal);
    }

    export function RenombrarUnaFar(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Gasto.FacturaRec.Renombrar, parametros, datosDeEntrada);
    }

    export function CambiarProveedor(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Gasto.FacturaRec.CambiarProveedor, parametros, datosDeEntrada);
    }

    function MapearDatosParaImportar(proveedor: any) {
        var cg = ApiControl.BuscarListaDinamicaPorPropiedad(crudDeFacturas.ModalImportarFarXml, ltrPropiedades.Gasto.ImportarFar.Cg);
        var tipo = ApiControl.BuscarListaDinamicaPorPropiedad(crudDeFacturas.ModalImportarFarXml, ltrPropiedades.Gasto.ImportarFar.Tipo);
        if (Definido(proveedor)) {
            ApiListaDinamica.AsignarValor(cg, proveedor.idCgPropuesto, proveedor.cgPropuesto, false);
            ApiListaDinamica.AsignarValor(tipo, proveedor.idTipoFarPropuesto, proveedor.tipoFarPropuesto, false);
        }
        else {
            ApiListaDinamica.Blanquear(cg);
            ApiListaDinamica.Blanquear(tipo);
        }
    }

    function Ia_MapearProveedor(proveedor: string, nif: string, json: Object) {
        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarListaDinamicaPorPropiedad(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Proveedor)
            : ApiControl.BuscarListaDinamicaPorPropiedad(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Proveedor)

        if (control.disabled || control.value === proveedor) {
            return;
        }

        const datosDeCreacion = new Parametros();
        datosDeCreacion.push(new Parametro(ltrPropiedades.Terceros.Sociedad.CrearProveedor, true))
        datosDeCreacion.push(new Parametro(ltrPropiedades.Terceros.Sociedad.Nombre, proveedor))
        datosDeCreacion.push(new Parametro(ltrPropiedades.Terceros.Sociedad.NIF, nif))
        datosDeCreacion.push(new Parametro(ltrPropiedades.Terceros.Sociedad.eMail, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.eMail)))
        datosDeCreacion.push(new Parametro(ltrPropiedades.Terceros.Sociedad.Telefono, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Telefono)))
        datosDeCreacion.push(new Parametro(ltrPropiedades.Terceros.Proveedor.NumeroIban, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.NumeroIban)))
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Cp, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.CodigoPostal)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Pais, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Pais)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Provincia, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Provincia)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Municipio, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Municipio)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Calle, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.Calle)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.TipoDeVia, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.TipodeVia)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Numero, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.NumeroPolicia)));
        datosDeCreacion.push(new Parametro(ltrPropiedades.Callejero.Direccion.Resto, ObtenerPropiedadJson(json, ltrPropiedades.Ia.Json.RestoDireccion)));

        mapeandoDatosProveedor = true;
        ApiDePeticiones.LeerElementoPorAk(crudDeFacturas, ltrControladores.Terceros.Proveedores, ltrPropiedades.Terceros.Proveedor.Nif, nif, false, true, true, datosDeCreacion)
            .then((peticion: ApiDeAjax.DescriptorAjax) => {
                const registro = peticion.resultado.datos;
                const idProveedor = ObtenerPropiedad(registro, ltrPropiedades.Elemento.Id, 0);
                const nombre = ObtenerPropiedad(registro, ltrPropiedades.Elemento.Nombre, 0);
                if (crudDeFacturas.EstoyCreando) {
                    MapearAlControl.ListaDinamica(control, idProveedor, nombre, false);
                }
                else {
                    if (idProveedor !== Numero(control.getAttribute(atListasDinamicas.idSeleccionado)))
                        throw new Error(`El proveedor del documento factura '${ObtenerPropiedad(registro, ltrPropiedades.Elemento.Expresion, 0)}' no coincide con el de la factura '${control.value}'`);
                }
                ApiDelCrud.IndicarControlMapeadoPoIa(control);
            })
            .catch((peticion: ApiDeAjax.DescriptorAjax) => {
                mapeandoDatosProveedor = false;
                ApiDePeticiones.EmitirError(peticion);
            });
    }

    function Ia_MapearFechaFactura(fecha: string) {
        if (!Definido(fecha))
            return;
        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarSelectorDeFecha(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.FacturadaEl)
            : ApiControl.BuscarSelectorDeFecha(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.FacturadaEl);

        if (control.disabled)
            return

        MapearAlControl.Fecha(control, fecha);
        ApiDelCrud.IndicarControlMapeadoPoIa(control);
    }

    function Ia_MapearFechaVencimiento(venceEl: string, claseDePago: string) {
        if (Definido(venceEl) && venceEl.toLocaleLowerCase() === 'null')
            venceEl = null;

        if (!Definido(venceEl) && claseDePago === ltrValores.Gasto.Pago.Clase.Contado) {
            const hoy = new Date();
            venceEl = hoy.toISOString().split('T')[0].replace(/-/g, '/');
        }

        if (!Definido(venceEl))
            return;
        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarSelectorDeFecha(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.VenceEl)
            : ApiControl.BuscarSelectorDeFecha(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.VenceEl);

        if (control.disabled)
            return

        MapearAlControl.Fecha(control, venceEl);
        ApiDelCrud.IndicarControlMapeadoPoIa(control);
    }

    function Ia_MapearTotal(total: number) {
        if (!Definido(total))
            return;
        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarEditor(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.TotalDelPago)
            : ApiControl.BuscarEditor(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.TotalDelPago);


        if (control.disabled)
            return

        AsignarValor(control, total);
        ApiDelCrud.IndicarControlMapeadoPoIa(control);
    }
    function Ia_MapearBi(bi: number) {
        if (!Definido(bi))
            return;
        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarEditor(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.BaseImponible)
            : ApiControl.BuscarEditor(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.BaseImponible);

        if (control.disabled)
            return

        AsignarValor(control, bi);
        ApiDelCrud.IndicarControlMapeadoPoIa(control);
    }
    function Ia_MapearNumeroFactura(numeroFactura: string) {
        if (!Definido(numeroFactura))
            return;
        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarEditor(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Numero)
            : ApiControl.BuscarEditor(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Numero);

        if (control.disabled)
            return

        control.value = numeroFactura;
        ApiDelCrud.IndicarControlMapeadoPoIa(control);
    }


    function Ia_MapearConcepto(concepto: string) {
        if (!Definido(concepto))
            return;

        const control: HTMLInputElement = crudDeFacturas.EstoyCreando
            ? ApiControl.BuscarEditor(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Nombre)
            : ApiControl.BuscarEditor(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Gasto.FacturaRec.Nombre);

        if (control.disabled)
            return

        let mapearDescripcion = false;
        if (!IsNullOrEmpty(control.value) || concepto.length > 255) mapearDescripcion = true;

        if (mapearDescripcion) {
            const descripcionControl: HTMLTextAreaElement = crudDeFacturas.EstoyCreando
                ? ApiControl.BuscarAreaDeTexto(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Elemento.Descripcion)
                : ApiControl.BuscarAreaDeTexto(crudDeFacturas.crudDeEdicion.PanelDelDto, ltrPropiedades.Elemento.Descripcion);

            descripcionControl.value = concepto;
            ApiDelCrud.IndicarControlMapeadoPoIa(descripcionControl);
        } else {
            control.value = concepto;
            ApiDelCrud.IndicarControlMapeadoPoIa(control);
        }
    }

    function Ia_MapearPagoContado(claseDePago: string, fomaDePago: string) {
        if (!Definido(claseDePago) || claseDePago !== ltrValores.Gasto.Pago.Clase.Contado)
            return;
        const control: HTMLInputElement = ApiControl.BuscarCheck(crudDeFacturas.crudDeCreacion.PanelDeCrear, ltrPropiedades.Gasto.FacturaRec.Pagada)
        control.checked = true;
        (crudDeFacturas.crudDeCreacion as CrudCreacionFacturaRec).AplicarCheckDePago(control);
        if (claseDePago === ltrValores.Gasto.Pago.Clase.Contado && Definido(fomaDePago)) {
            const lista = (crudDeFacturas.crudDeCreacion as CrudCreacionFacturaRec).ModoDePago;
            const fijado = MapearAlControl.ListaDeValores(lista, fomaDePago);
            if (fijado)
                ApiDelCrud.IndicarControlMapeadoPoIa(lista);
            else
                MensajesSe.Info(`No se ha localizado la forma de pago '${fomaDePago}' para la clase Contado`);
        }
    }

}
