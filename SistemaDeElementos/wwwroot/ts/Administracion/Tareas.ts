namespace Administracion {


    enum enumEtapasDeTareas {
        TAR_Etapa_Inicial,
        TAR_Etapa_Asignada,
        TAR_Etapa_En_Resolucion,
        TAR_Etapa_En_Espera,
        TAR_Etapa_Validacion,
        TAR_Etapa_Terminada,
        TAR_Etapa_Cancelado
    }


    export function CrearConsultaDeTarea(idPanelEdicion: string) {
        Crud.Consultor = new Administracion.CrudEdicionTarea(null, idPanelEdicion);
        //Crud.Consultor.ConsultarSeleccionado();

    }

    export function CrearCrudDeTareas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Administracion.CrudDeTareas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeTareas extends Crud.CrudMnt {

        public get ModalCopiarTarea(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Administracion.Tareas.CopiarTarea) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionTarea(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionTarea(this, idPanelEdicion);
        }

        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            this.Expansor_InyectarAccesoIA();
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Administracion.Tareas.CopiarTarea) {
                let idModal = this.IdCrud + '-' + opcion;
                let id = this.InfoSelector.Seleccionados.length === 0 ? 0 : this.InfoSelector.Seleccionados[0].Id;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, id);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);

            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Administracion.Tareas);
                return true;
            }

            return Administracion.NavegarARelaciones(peticion, ltrParametrosUrl.Venta.IdTarea);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);

            if (modal.id === this.ModalCopiarTarea.id) {
                Tarea_Copiar(this, this.Controlador, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.Administracion.Tareas}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }


        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalCopiarTarea.id) {
                if (this.InfoSelector.Seleccionados.length === 1)
                    this.Tar_ProponerTareaParaCopiar(modal, this.InfoSelector.Seleccionados[0].Registro);
            }
        }


        //public DefinirParametrosParaCargarElGrid(): Array<Parametro> {
        //    let parametros: Array<Parametro> = super.DefinirParametrosParaCargarElGrid();
        //    parametros.push(new Parametro('filtrarConIa', true));
        //    const filtro = parametros.find(p => p.parametro === Ajax.Param.filtro);
        //    filtro.valor = "obtenme las tareas del centro gestor de femdek y Arkenos y el tipo Evolutivo, Denominacion no es ninguna clausula de filtrado";
        //    return parametros;
        //}


        public Tar_ProponerTareaParaCopiar(modal: HTMLDivElement, registro: any) {
            let destino: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.expresion));
            ApiDelCrud.MapearDatosSocietarios(modal, registro);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConTipo.Tipo, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Tarea.Solicitante, ltrPropiedades.Tarea.IdSolicitante);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Tarea.Expediente, ltrPropiedades.Tarea.IdExpediente, false);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Elemento.Nombre);
            ApiDelCrud.ProponerEnAreaDeTexto(modal, registro, ltrPropiedades.Elemento.Descripcion);
        }

    }

    export class CrudCreacionTarea extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        protected MapearRestrictoresDeFiltro() {
            super.MapearRestrictoresDeFiltro();

            var modalDeFiltrado = document.getElementById(this.CrudDeMnt.ModalDeFiltrado(ltrModalDeFiltrado.Administracion.Tarea.FiltrosDeRelacion)) as HTMLDivElement;
            var expediente = ApiControl.BuscarListaDinamicaPorPropiedad(modalDeFiltrado, ltrPropiedades.Tarea.VinculadoA.IdExpediente)
            var idSeleccionado = Numero(expediente.getAttribute(atListasDinamicas.idSeleccionado));
            if (idSeleccionado > 0 && expediente.readOnly) {
                ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Administracion.Expedientes, idSeleccionado, new Array<Parametro>(), idSeleccionado)
                    .then((peticion: ApiDeAjax.DescriptorAjax) => {
                        MapearAlControl.RestrictoresDeEdicion(this.PanelDeCrear, ltrPropiedades.Tarea.VinculadoA.IdExpediente, idSeleccionado, expediente.value);
                        ApiDelCrud.MapearDatosDeUnExpediente(this.PanelDeCrear, peticion.resultado.datos);
                    })
                    .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
            }
        }

        protected ParametrosDeCreacion(): Parametro[] {
            var parametros = super.ParametrosDeCreacion();
            var mnt = this.CrudDeMnt;
            var modalDeFiltrado = document.getElementById(mnt.ModalDeFiltrado(ltrModalDeFiltrado.Administracion.Tarea.FiltrosDeRelacion)) as HTMLDivElement;
            var expediente = ApiControl.BuscarListaDinamicaPorPropiedad(modalDeFiltrado, ltrPropiedades.Tarea.VinculadoA.IdExpediente)
            var idSeleccionado = Numero(expediente.getAttribute(atListasDinamicas.idSeleccionado));
            if (idSeleccionado > 0 && expediente.readOnly) {
                parametros.push(new Parametro(ltrParametrosNeg.Administracion.Tareas.VincularA.Expediente, idSeleccionado));
            }
            return parametros;
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            ApiPanel.MostrarOcultarCelda(this.PanelDeCrear, ltrPropiedades.Tarea.IdFacturaEmt, false);
            ApiControl.ColSpan(this.PanelDeCrear, ltrPropiedades.Tarea.IdExpediente, 2);
        }
    }

    export class CrudEdicionTarea extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            if (ObtenerPropiedad(this.Registro, ltrPropiedades.Tarea.UsaPlanificacion)) {
                let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Tarea.Etapas);

                let permitirCambiarAsignado = this.EsGestor && EstaElEnumerado(etapas, enumEtapasDeTareas, enumEtapasDeTareas.TAR_Etapa_Inicial);
                if (permitirCambiarAsignado)
                    ApiControl.DesbloquearListaDinamicaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Tarea.Responsable);
                else
                    ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Tarea.Responsable);
            }

            let idFactura = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Tarea.IdFacturaEmt, 0));
            let idExpediente = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Tarea.IdExpediente, 0));
            let incColSpanDeResponsable = idExpediente === 0 && idFactura === 0 ? 3 : idExpediente === 0 ? 2 : 0;
            ApiPanel.MostrarOcultarCelda(this.PanelDeEditar, ltrPropiedades.Tarea.IdFacturaEmt, idFactura > 0);
            ApiPanel.MostrarOcultarCelda(this.PanelDeEditar, ltrPropiedades.Tarea.IdExpediente, idExpediente > 0)

            ApiControl.ColSpan(this.PanelDeEditar, ltrPropiedades.Tarea.IdExpediente, idFactura === 0 ? 2 : 0);
            ApiControl.ColSpan(this.PanelDeEditar, ltrPropiedades.Tarea.Responsable, incColSpanDeResponsable);

        }

        public Expansor_TrasCargarAmpliacion(ampliacion: HTMLDivElement): void {
            super.Expansor_TrasCargarAmpliacion(ampliacion);
            if (this.EsLaAmpliacionDe(ampliacion, ltrAmpliaciones.tareas.planificacion)) this.Ampliaciones_BloquearControlesDePlanificacion();
        }

        private Ampliaciones_BloquearControlesDePlanificacion() {
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Tarea.Etapas);
            let ampliacion: HTMLDivElement = this.DivDeAmpliacion(ltrAmpliaciones.tareas.planificacion);
            if (EstaElEnumerado(etapas, enumEtapasDeTareas, enumEtapasDeTareas.TAR_Etapa_Inicial)) {
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Iniciada);
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Finalizada);
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Duracion);
                ApiControl.BloquearListaDeValores(ampliacion, ltrPropiedades.Tarea.Planificacion.MedidoEn);
            }

            if (EstaElEnumerado(etapas, enumEtapasDeTareas, enumEtapasDeTareas.TAR_Etapa_Asignada)) {
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Finalizada);
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Duracion);
                ApiControl.BloquearListaDeValores(ampliacion, ltrPropiedades.Tarea.Planificacion.MedidoEn);
            }

            if (EstaElEnumerado(etapas, enumEtapasDeTareas, enumEtapasDeTareas.TAR_Etapa_Validacion) || EstaElEnumerado(etapas, enumEtapasDeTareas, enumEtapasDeTareas.TAR_Etapa_En_Espera)) {
                bloquearDivDePlanificacion();
            }

            if (EstaElEnumerado(etapas, enumEtapasDeTareas, enumEtapasDeTareas.TAR_Etapa_En_Resolucion)) {
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.PlfDeInicio);
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.PlfDeFin);
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Iniciada);
                ApiControl.DesbloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Finalizada);
            }

            function bloquearDivDePlanificacion() {
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.PlfDeInicio);
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.PlfDeFin);
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Iniciada);
                ApiControl.BloquearSelectorDeFechaHoraPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Finalizada);
                ApiControl.BloquearEditorPorPropiedad(ampliacion, ltrPropiedades.Tarea.Planificacion.Duracion);
                ApiControl.BloquearListaDeValores(ampliacion, ltrPropiedades.Tarea.Planificacion.MedidoEn);
            }
        }

    }

    export function Tarea_Copiar(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticionPost(llamador, controlador, Ajax.EndPoint.Administracion.Tarea.Copiar, parametros, datosDeEntrada);
    }

    export function Tar_ProponerDatosDelaTareaSeleccionada() {
        let modal = (Crud.crudMnt as CrudDeTareas).ModalCopiarTarea;
        let tarea: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
        let idTarea: number = Numero(tarea.getAttribute(atListasDinamicas.idSeleccionado));
        if (idTarea > 0) {
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Administracion.Tareas, idTarea, new Array<Parametro>(), idTarea)
                .then((peticion) => (Crud.crudMnt as CrudDeTareas).Tar_ProponerTareaParaCopiar(modal, peticion.resultado.datos))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

    }

    export function Tar_InicializarModalDeCopiado() {
        let modal = (Crud.crudMnt as CrudDeTareas).ModalCopiarTarea;
        ApiPanel.BlanquearControlesDeIU(modal);
    }


}