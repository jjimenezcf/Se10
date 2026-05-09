namespace Juridico {


    export function CrearCrudDeContratos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string, claseDeContrato: string) {
        Crud.crudMnt = new Juridico.CrudDeContratos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar, claseDeContrato);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);
        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }


    export class CrudDeContratos extends Crud.CrudMnt {

        private _clase: string;
        public get ClaseDeContrato(): string { return this._clase; }
        public get ModalGenerarLosPlanificadores(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Juridico.Contratos.GenerarLosPlanificadores) as HTMLDivElement; }
        public get ModalPrepararPartesTr(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Juridico.Contratos.PrepararPartesDeTrabajo) as HTMLDivElement; }
        public get ModalPrefacturarPartesTr(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Juridico.Contratos.EmitirPrefacturasPorParteTr) as HTMLDivElement; }
        public get ModalPrefacturarContratos(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Juridico.Contratos.EmitirPrefacturasPorContrato) as HTMLDivElement; }
        public get ModalImputarFacturas(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Juridico.Contratos.ImputarFacturas) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string, claseDeContrato: string) {
            super(idPanelMnt, idModalBorrar);
            this._clase = claseDeContrato;
            this.crudDeCreacion = new CrudCreacionContrato(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionContrato(this, idPanelEdicion);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (this.ClaseDeContrato === enumClaseDeContrato.Venta && modal.id === this.ModalGenerarLosPlanificadores.id) {
                LlamarA_GenerarPlanificador(modal, (modal) => super.ModalDePedirDatos_Aceptar(modal));
            }
            else
                if (this.ClaseDeContrato === enumClaseDeContrato.Venta && modal.id === this.ModalPrepararPartesTr.id) {
                    LlamarA_PrepararPartesTr(modal, (modal) => super.ModalDePedirDatos_Aceptar(modal));
                }
                else
                    if (this.ClaseDeContrato === enumClaseDeContrato.Venta && modal.id === this.ModalPrefacturarPartesTr.id) {
                        LlamarA_EmitirPrefacturasPorPartesTr(modal, (modal) => super.ModalDePedirDatos_Aceptar(modal));
                    }
                    else
                        if (this.ClaseDeContrato === enumClaseDeContrato.Venta && modal.id === this.ModalPrefacturarContratos.id) {
                            LlamarA_EmitirPrefacturasPorContratos(modal, (modal) => super.ModalDePedirDatos_Aceptar(modal));
                        }
                        else
                            super.ModalDePedirDatos_Aceptar(modal);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Juridico.Contratos.GenerarLosPlanificadores
                || opcion === ltrMenus.eventosDeMf.Juridico.Contratos.PrepararPartesDeTrabajo
                || opcion === ltrMenus.eventosDeMf.Juridico.Contratos.EmitirPrefacturasPorParteTr
                || opcion === ltrMenus.eventosDeMf.Juridico.Contratos.EmitirPrefacturasPorContrato) {
                let idModal = Crud.crudMnt.IdCrud + '-' + opcion;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, 0);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Juridico.Contratos.ImputarFacturas) {
                let idModal = Crud.crudMnt.IdCrud + '-' + opcion;
                this.AbrirModalParaImputar(idModal);
                return true;
            }

            return (Juridico.NavegarARelaciones(peticion, ltrParametrosUrl.Juridico.IdContrato));
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);
            let propiedad: string = ltrPropiedades.Juridico.Contrato.ClaseDeContrato;
            let criterio: string = literal.filtro.criterio.igual;
            let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, this.ClaseDeContrato);
            clausulas.push(clausula);
            return clausulas;
        }

    }

    export class CrudCreacionContrato extends Crud.CrudCreacion {

        private get _claseContrato(): string { return (this.CrudDeMnt as CrudDeContratos).ClaseDeContrato; }

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            let expediente: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Juridico.Contrato.idExpediente, ltrTipoControl.restrictorDeEdicion);
            let idExpediente: number = Numero(expediente.getAttribute(atRestrictor.idRestrictor));
            if (idExpediente > 0) {
                let parametros = new Array<Parametro>();
                parametros.push(new Parametro(ltrPropiedades.Juridico.Contrato.ClaseDeContrato, this._claseContrato));
                ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Administracion.Expedientes, idExpediente, parametros, idExpediente)
                    .then((peticion) => this.MapearDatosExpediente(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else ApiDelCrud.PosicionarCrudDeCreacionConCgYTipo(this.PanelDeCrear);

            MapearAlControl.MapearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Juridico.Contrato.ClaseDeContrato), 0, this._claseContrato, true, false);
            let tipo: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Elemento.ConTipo.Tipo);
            let restrictorFijo = tipo.getAttribute(atListasDinamicas.RestrictorFijo);
            if (restrictorFijo.indexOf(this._claseContrato) === -1)
                tipo.setAttribute(atListasDinamicas.RestrictorFijo, `${restrictorFijo}|${ltrPropiedades.Juridico.Contrato.ClaseDeContrato};${this._claseContrato}`);

            if (this._claseContrato === enumClaseDeContrato.MatriculaDeGuarderia) {
                let nombre = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Elemento.Nombre);
                nombre.setAttribute(atControl.obligatorio, 'N');
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Elemento.Nombre);
                ApiPanel.MostrarFila(this.PanelDeCrear, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Infante);
            }
            else {
                const infante = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Infante);
                const curso = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Curso);
                infante.setAttribute(atControl.obligatorio, 'N');
                curso.setAttribute(atControl.obligatorio, 'N');
                ApiPanel.MostrarFila(this.PanelDeCrear, ltrPropiedades.Elemento.Nombre);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Infante);

            }
        }

        protected ParametrosParaLeerDatosDeInicializarPanel(): Array<Parametro> {
            var parametros = super.ParametrosParaLeerDatosDeInicializarPanel();
            parametros.push(new Parametro(ltrPropiedades.Juridico.Contrato.ClaseDeContrato, (Crud.crudMnt as CrudDeContratos).ClaseDeContrato));
            return parametros;
        }

        protected IncluirParametrosParaRecuperarDatosDePlantillasDeCreacion(opcion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaRecuperarDatosDePlantillasDeCreacion(opcion, parametros, datosDeEntrada);
            parametros.push(new Parametro(ltrPropiedades.Juridico.Contrato.ClaseDeContrato, (this.CrudDeMnt as CrudDeContratos).ClaseDeContrato));
        }

        protected IncluirParametrosParaGuardarDatosDeCreacion(opcion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaGuardarDatosDeCreacion(opcion, parametros, datosDeEntrada);
            parametros.push(new Parametro(ltrPropiedades.Juridico.Contrato.ClaseDeContrato, (this.CrudDeMnt as CrudDeContratos).ClaseDeContrato));
        }

        protected IncluirParametrosParaGuardarPlantillaCreacion(parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaGuardarPlantillaCreacion(parametros, datosDeEntrada);
            parametros.push(new Parametro(ltrPropiedades.Juridico.Contrato.ClaseDeContrato, (this.CrudDeMnt as CrudDeContratos).ClaseDeContrato));
        }

        private MapearDatosExpediente(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionContrato = peticion.llamador as CrudCreacionContrato;
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(creador.PanelDeCrear, peticion.resultado.datos);
            ApiControl.BuscarListaDinamicaPorPropiedad(creador.PanelDeCrear, ltrPropiedades.Elemento.ConTipo.Tipo).focus();
        }

    }

    export class CrudEdicionContrato extends Crud.CrudEdicion {


        public get ModalCopiarPlfDeVenta(): HTMLDivElement {
            return document.getElementById(this.IdModalDeCrearDetalle(ltrEspanes.Juridico.Contratos.CopiarPlfDeVenta)) as HTMLDivElement;
        }


        public get ModalEditarPlfDeVenta(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Juridico.Contratos.PlanificadorDeVentas) as HTMLDivElement;
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected InicializarAmpliaciones(panel: HTMLDivElement) {
            super.InicializarAmpliaciones(panel);
            if ((this.CrudDeMnt as CrudDeContratos).ClaseDeContrato === enumClaseDeContrato.Compra) {
                let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(this.DivDeAmpliacion(ltrAmpliaciones.contratos.avance), ltrPropiedades.Juridico.Contrato.Avance.Cobrado);
                etiqueta.innerText = ltrEtiquetas.Juridico.Contratos.Avance.Pagado;
            }
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            let parametros = super.ParametrosParaLeerElementoPorId();
            parametros.push(new Parametro(ltrPropiedades.Juridico.Contrato.ClaseDeContrato, (this.CrudDeMnt as CrudDeContratos).ClaseDeContrato));
            return parametros;
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Juridico.Contratos.VincularRegistroEntrada) {
                this.Expansor_AbrirModalDeVincular(ltrModalDeVincular.registrosDeEs);
                return true;
            }
            return false;
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            const etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));

            if (!this.EsGestor ||
                (etapa !== enumEtapasDeContratos.CTR_Etapa_En_Elaboracion && etapa !== enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga))
                ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Juridico.Contratos.lotes));

            if ((this.CrudDeMnt as CrudDeContratos).ClaseDeContrato !== enumClaseDeContrato.Compra) {
                if (!this.EsGestor ||
                    etapa === enumEtapasDeContratos.CTR_Etapa_Finalizacion ||
                    etapa === enumEtapasDeContratos.CTR_Etapa_Cancelado)
                    ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Juridico.Contratos.PlanificadorDeVentas));

            }
            else {
                const ocultar = !this.EsGestor || (etapa !== enumEtapasDeContratos.CTR_Etapa_Vigente && etapa !== enumEtapasDeContratos.CTR_Etapa_Finalizacion);
                ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Juridico.Contratos.FacturasRec, ltrEspanes.Opcion.vincular), ocultar);
            }
        }

        //override CargaSpanCompletada(span: HTMLDivElement) {
        //    super.CargaSpanCompletada(span);
        //    if (span.id === this.IdSpanDelExpansor(ltrEspanes.Juridico.Contratos.FacturasRec)) {
        //        const etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));
        //        if (!this.EsGestor || (etapa !== enumEtapasDeContratos.CTR_Etapa_Vigente && etapa !== enumEtapasDeContratos.CTR_Etapa_Finalizacion))
        //            ApiDeGrid.Expansor_OcultarPorId(this.IdGridDelExpansor(ltrEspanes.Juridico.Contratos.FacturasRec))
        //    }
        //}

        public Expansor_TrasCargarAmpliacion(ampliacion: HTMLDivElement): void {
            super.Expansor_TrasCargarAmpliacion(ampliacion);
            if (this.EsLaAmpliacionDe(ampliacion, ltrAmpliaciones.contratos.datosDeVenta)) this.Ampliaciones_BloquearControlesDeDatosDeVenta();
            if (this.EsLaAmpliacionDe(ampliacion, ltrAmpliaciones.contratos.datosDeCompra)) this.Ampliaciones_BloquearControlesDeDatosDeCompra();
            if (this.EsLaAmpliacionDe(ampliacion, ltrAmpliaciones.contratos.saldos)) this.Ampliaciones_BloquearControlesDeSaldos();
            if (this.EsLaAmpliacionDe(ampliacion, ltrAmpliaciones.contratos.avalsolicitado)) this.Ampliaciones_BloquearControlesDeAvalSolicitado();
            if (this.EsLaAmpliacionDe(ampliacion, ltrAmpliaciones.contratos.prorroga)) this.Ampliaciones_BloquearControlesDeProrroga();
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Juridico.Contratos.PlanificadorDeVentas)) {
                this.RecargarGridDeTrazas();
            }
            else if (grid.id == this.IdGridDelExpansor(ltrEspanes.Juridico.Contratos.FacturasRec)) {
                this.RecargarAmpliaciones();
            }
        }

        private Ampliaciones_BloquearControlesDeProrroga() {
            let etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));
            let ampliacion: HTMLDivElement = this.DivDeAmpliacion(ltrAmpliaciones.contratos.prorroga);
            if (etapa !== enumEtapasDeContratos.CTR_Etapa_En_Elaboracion) {
                ApiControl.BloquearListaDeValores(ampliacion, ltrPropiedades.Juridico.Contrato.Prorrogas.ClaseDeProrroga);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.Prorrogas.FechaUltimaProrroga);
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.Prorrogas.Meses);
            }
        }

        private Ampliaciones_BloquearControlesDeAvalSolicitado() {
            let etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));
            let ampliacion: HTMLDivElement = this.DivDeAmpliacion(ltrAmpliaciones.contratos.avalsolicitado);
            if (etapa !== enumEtapasDeContratos.CTR_Etapa_En_Elaboracion) {
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.AvalSolicitado.ImporteAval);
            }
        }

        private Ampliaciones_BloquearControlesDeSaldos() {
            let etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));
            let ampliacion: HTMLDivElement = this.DivDeAmpliacion(ltrAmpliaciones.contratos.saldos);
            if (etapa !== enumEtapasDeContratos.CTR_Etapa_En_Elaboracion) {
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.Saldos.Importe);
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.Saldos.Adendado);
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.Saldos.Bloqueo);
            }
        }

        private Ampliaciones_BloquearControlesDeDatosDeVenta() {
            let etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));
            let ampliacion: HTMLDivElement = this.DivDeAmpliacion(ltrAmpliaciones.contratos.datosDeVenta);
            if (etapa !== enumEtapasDeContratos.CTR_Etapa_En_Elaboracion) {
                ApiControl.BloquearListaDinamicaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.DatosDeVenta.Cliente);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.DatosDeVenta.InicioContrato);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.DatosDeVenta.FinContrato);
            }
        }

        private Ampliaciones_BloquearControlesDeDatosDeCompra() {
            let etapa: enumEtapasDeContratos = ParsearEtapa(ObtenerPropiedad(this.Registro, ltrPropiedades.Juridico.Contrato.Etapa));
            let ampliacion: HTMLDivElement = this.DivDeAmpliacion(ltrAmpliaciones.contratos.datosDeCompra);
            if (etapa !== enumEtapasDeContratos.CTR_Etapa_En_Elaboracion) {
                ApiControl.BloquearListaDinamicaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.DatosDeCompra.Proveedor);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.DatosDeCompra.InicioContrato);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(ampliacion, ltrPropiedades.Juridico.Contrato.DatosDeCompra.FinContrato);
            }
        }

        public ctr_blanquearDatosDeCliente() {
            let div = this.DivDeAmpliacion(ltrAmpliaciones.contratos.datosDeVenta);
            let contacto = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.DatosDeVenta.Contacto) as HTMLInputElement;
            let telefono = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.DatosDeVenta.Telefono) as HTMLInputElement;
            let mail = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.DatosDeVenta.eMail) as HTMLInputElement;
            contacto.value = '';
            telefono.value = '';
            mail.value = '';
        }

        public ctr_blanquearDatosDeClienteDeCuarderia() {
            let div = this.DivDeAmpliacion(ltrAmpliaciones.contratos.MatriculaDeGuarderia);
            let contacto = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Contacto) as HTMLInputElement;
            let telefono = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Telefono) as HTMLInputElement;
            let mail = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.eMail) as HTMLInputElement;
            contacto.value = '';
            telefono.value = '';
            mail.value = '';
        }

        public ctr_mapearDatosDeClienteDeCuarderia(cliente: any) {
            let div = this.DivDeAmpliacion(ltrAmpliaciones.contratos.MatriculaDeGuarderia);
            let contacto = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Contacto) as HTMLInputElement;
            let telefono = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.Telefono) as HTMLInputElement;
            let mail = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.MatriculaDeGuarderia.eMail) as HTMLInputElement;

            contacto.value = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.Expresion, '').toString();
            telefono.value = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.Telefono, '').toString();
            mail.value = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.eMail, '').toString();
        }


        public ctr_Tras_Seleccionar_CopiarPlfDeVenta(planificador: any) {
            let inicio = ApiControl.BuscarSelectorDeFecha(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Inicio) as HTMLInputElement;
            let hasta = ApiControl.BuscarSelectorDeFecha(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Hasta) as HTMLInputElement;
            if (Definido(planificador)) {
                MapearAlControl.Fecha(inicio, ObtenerPropiedad(planificador, ltrPropiedades.Juridico.CopiarPlfDeVenta.Inicio));
                MapearAlControl.Fecha(hasta, ObtenerPropiedad(planificador, ltrPropiedades.Juridico.CopiarPlfDeVenta.Hasta));
            }
            else {
                let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Contrato);
                let idcontrato = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
                if (idcontrato > 0) {
                    var contrato = OpcionesDeLasListas.ObtenerObjeto(lista);
                    MapearAlControl.Fecha(inicio, ObtenerPropiedad(contrato, ltrPropiedades.Juridico.Contrato.DatosDeVenta.InicioContrato));
                    MapearAlControl.Fecha(hasta, ObtenerPropiedad(contrato, ltrPropiedades.Juridico.Contrato.DatosDeVenta.FinContrato));
                }
            }
        }

        public ctr_blanquearDatosDeCopiarContratosDeVenta() {
            let planificador = ApiControl.BuscarListaDeElementos(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Planificador) as HTMLSelectElement;
            ApiControl.BlanquearListaDeElementos(planificador);
            let inicio = ApiControl.BuscarSelectorDeFecha(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Inicio) as HTMLInputElement;
            let hasta = ApiControl.BuscarSelectorDeFecha(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Hasta) as HTMLInputElement;
            ApiControl.BlanquearFecha(inicio);
            ApiControl.BlanquearFecha(hasta);
        }

        public ctr_mapearDatosDeCopiarContratoDeVenta(contrato: any) {
            let planificador = ApiControl.BuscarListaDeElementos(this.ModalCopiarPlfDeVenta, ltrPropiedades.Juridico.CopiarPlfDeVenta.Planificador) as HTMLSelectElement;
            MapearAlControl.ListaDeElementos(planificador, new Array<ClausulaDeFiltrado>(), 0, null);
        }


        public ctr_mapearDatosDeCliente(cliente: any) {
            let div = this.DivDeAmpliacion(ltrAmpliaciones.contratos.datosDeVenta);
            let contacto = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.DatosDeVenta.Contacto) as HTMLInputElement;
            let telefono = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.DatosDeVenta.Telefono) as HTMLInputElement;
            let mail = ApiControl.BuscarEditor(div, ltrPropiedades.Juridico.Contrato.DatosDeVenta.eMail) as HTMLInputElement;

            contacto.value = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.Expresion, '').toString();
            telefono.value = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.Telefono, '').toString();
            mail.value = ObtenerPropiedad(cliente, ltrPropiedades.Terceros.Cliente.eMail, '').toString();
        }

        public ctr_blanquearDatosDeProveedor() {
            let contacto = ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.Contrato.DatosDeCompra.Contacto) as HTMLInputElement;
            let telefono = ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.Contrato.DatosDeCompra.Telefono) as HTMLInputElement;
            let mail = ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.Contrato.DatosDeCompra.eMail) as HTMLInputElement;
            contacto.value = '';
            telefono.value = '';
            mail.value = '';
        }

        public ctr_mapearDatosDeProveedor(Proveedor: any) {
            let contacto = ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.Contrato.DatosDeCompra.Contacto) as HTMLInputElement;
            let telefono = ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.Contrato.DatosDeCompra.Telefono) as HTMLInputElement;
            let mail = ApiControl.BuscarEditor(this.PanelDeEditar, ltrPropiedades.Juridico.Contrato.DatosDeCompra.eMail) as HTMLInputElement;

            contacto.value = ObtenerPropiedad(Proveedor, ltrPropiedades.Terceros.Proveedor.Expresion, '').toString();
            telefono.value = ObtenerPropiedad(Proveedor, ltrPropiedades.Terceros.Proveedor.Telefono, '').toString();
            mail.value = ObtenerPropiedad(Proveedor, ltrPropiedades.Terceros.Proveedor.eMail, '').toString();
        }
    }

    export function Ctr_Tras_Seleccionar_Cliente(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        //let opcion: HTMLOptionElement = ApiListaDinamica.BuscarOpcion(lista, lista.value);
        var cliente = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(cliente)) {
            // var cliente = OpcionesDeLasListas.ObtenerObjeto(lista); 
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_mapearDatosDeCliente(cliente);
        }
        else {
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeCliente();
        }
    }

    export function Ctr_Tras_Blanquear_Cliente() {
        (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeCliente();
    }

    export function Ctr_Tras_Seleccionar_Proveedor(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var Proveedor = OpcionesDeLasListas.ObtenerObjeto(lista);
        //let opcion: HTMLOptionElement = ApiListaDinamica.BuscarOpcion(lista, lista.value);
        if (Definido(Proveedor)) {
            //var Proveedor = OpcionesDeLasListas.ObtenerObjeto(lista); 
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_mapearDatosDeProveedor(Proveedor);
        }
        else {
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeProveedor();
        }
    }

    export function Ctr_Tras_Blanquear_Proveedor() {
        (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeProveedor();
    }


    export function Ctr_Tras_Seleccionar_ClienteDeGuarderia(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var cliente = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(cliente)) {
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_mapearDatosDeClienteDeCuarderia(cliente);
        }
        else {
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeClienteDeCuarderia();
        }
    }

    export function Ctr_Tras_Blanquear_ClienteDeGuarderia() {
        (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeClienteDeCuarderia();
    }


    export function Ctr_Tras_Seleccionar_CopiarContratoDeVenta(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var contrato = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(contrato)) {
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_mapearDatosDeCopiarContratoDeVenta(contrato);
        }
        else {
            (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeCopiarContratosDeVenta();
        }
    }

    export function Ctr_Tras_Blanquear_CopiarContratoDeVenta() {
        (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_blanquearDatosDeCopiarContratosDeVenta();
    }

    export function Ctr_Tras_Seleccionar_CopiarPlfDeVenta() {
        var modal = (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ModalCopiarPlfDeVenta;
        var lista = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Juridico.CopiarPlfDeVenta.Planificador);
        var planificador = OpcionesDeLasListas.ObtenerObjeto(lista);
        (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato).ctr_Tras_Seleccionar_CopiarPlfDeVenta(planificador);
    }

    export function Ctr_TrasAbrirModalDeCopiarPlfDeVenta(idModal: string) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionContrato);
        var modal = editor.ModalCopiarPlfDeVenta;
        var clase = ApiControl.BuscarEditor(modal, ltrPropiedades.Juridico.CopiarPlfDeVenta.ClaseDeContrato);
        clase.value = (Crud.crudMnt as CrudDeContratos).ClaseDeContrato;
        var contratodestino = ApiControl.BuscarRestrictor(modal, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeEdicion);
        MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.Elemento.IdElemento, editor.Registro.id, editor.Registro.nombre);
    }


    export function NavegarARelaciones(peticion: ApiDeAjax.DescriptorAjax, idRestrictor: string): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Juridico.Contratos.IrAPlfDeVentas:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PlfDeVenta}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Juridico.Contratos.IrAPartesTr:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PartesTr}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Juridico.Contratos.IrAFacturasEmt:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Juridico.Contratos.IrAFacturasRec:
                urlDestino = `${window.location.origin}/${ltrUrls.Gastos.FacturasRec}?${ltrParametrosUrl.restrictores}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }

    function LlamarA_GenerarPlanificador(modal: HTMLDivElement, metodo: Function) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
        parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
        let datosDeEntrada = new Array<Parametro>();
        Juridico.epDeContratos(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Contrato.GenerarPlanificador, parametros, datosDeEntrada)
            .then((peticion) => {
                metodo(modal);
                var urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PlfDeVenta}?${ltrParametrosUrl.filtros}=[${ltrParametrosUrl.Venta.FiltroConOSinContrato}=${ltrParametrosUrl.Venta.IndicePorContrato}]`;
                EntornoSe.AbrirPestana(urlDestino);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function LlamarA_PrepararPartesTr(modal: HTMLDivElement, metodo: Function) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
        parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
        let datosDeEntrada = new Array<Parametro>();
        Juridico.epDeContratos(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Contrato.PrepararPartesTr, parametros, datosDeEntrada)
            .then((peticion) => {
                metodo(modal);
                var urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PartesTr}?${ltrParametrosUrl.filtros}=[${ltrParametrosUrl.Venta.FiltroConOSinContrato}=${ltrParametrosUrl.Venta.IndicePorContrato}]`;
                EntornoSe.AbrirPestana(urlDestino);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function LlamarA_EmitirPrefacturasPorPartesTr(modal: HTMLDivElement, metodo: Function) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
        parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
        let datosDeEntrada = new Array<Parametro>();
        Juridico.epDeContratos(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Contrato.EmitirPrefacturasPorPartesTr, parametros, datosDeEntrada)
            .then((peticion) => {
                metodo(modal);
                var urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.filtros}=[${ltrParametrosUrl.Venta.FiltroConOSinContrato}=${ltrParametrosUrl.Venta.IndicePorContrato}]`;
                EntornoSe.AbrirPestana(urlDestino);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function LlamarA_EmitirPrefacturasPorContratos(modal: HTMLDivElement, metodo: Function) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
        parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
        let datosDeEntrada = new Array<Parametro>();
        Juridico.epDeContratos(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Contrato.EmitirPrefacturasPorContrato, parametros, datosDeEntrada)
            .then((peticion) => {
                metodo(modal);
                var urlDestino = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?${ltrParametrosUrl.filtros}=[${ltrParametrosUrl.Venta.AsociadaAUnContrato}=${ltrParametrosUrl.Venta.IndicePorContrato}]`;
                EntornoSe.AbrirPestana(urlDestino);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }
}



