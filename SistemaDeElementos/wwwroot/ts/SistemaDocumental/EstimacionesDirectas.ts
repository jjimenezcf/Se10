namespace SistemaDocumental {

    enum enumEtapasDeCircuitoDoc {
    }

    function ParsearEtapa(etapa: string): enumEtapasDeCircuitoDoc {

        MensajesSe.EmitirExcepcion("Parsear etapa de circuito documental", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaLaEstimacionEnEtapa(etapas: string, etapa: enumEtapasDeCircuitoDoc): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function CrearCrudDeEstimacionesDirectas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new SistemaDocumental.CrudDeEstimacionesDirectas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () {
            Crud.crudMnt.Inicializar(idPanelMnt);
        }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeEstimacionesDirectas extends Crud.CrudMnt {

        public get ModalContabilizar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Contabilidad.Preasientos.CrearLoteContable) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionEstimacionDirecta(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionEstimacionDirecta(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            const ctrlTipo = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiListaDinamica.AsignarValor(ctrlTipo, this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.IdTipoEstimacionDirecta),
                this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.TipoEstimacionDirecta), true);
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);

            if (!clausulas.find(c => c.clausula === ltrPropiedades.Elemento.ConTipo.IdTipo)) {
                let propiedad: string = ltrPropiedades.Elemento.ConTipo.IdTipo;
                let criterio: string = literal.filtro.criterio.igual;
                let valor = this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.IdTipoEstimacionDirecta);
                let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
                clausulas.push(clausula);
            }
            return clausulas;
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);

            if (opcion === ltrMenus.eventosDeMf.Contabilidad.Preasientos.CrearLoteContable) {
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos((Crud.crudMnt as CrudDeEstimacionesDirectas).ModalContabilizar.id, 0);
                return true;
            }
        }



        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalContabilizar.id) {
                var ejercicioCrtl = ApiControl.BuscarEditor(this.ModalContabilizar, ltrPropiedades.Contabilidad.CrearLote.Ejercicio);
                ejercicioCrtl.value = new Date().getFullYear().toString();
                ApiControl.IncluirCss(ApiControl.BuscarCelda(ApiControl.BuscarCheck(this.ModalContabilizar, ltrPropiedades.Contabilidad.CrearLote.RespetarFechaContable)), ltrCss.celdaNoVisible);
                ApiControl.IncluirCss(ApiControl.BuscarCelda(ApiControl.BuscarCheck(this.ModalContabilizar, ltrPropiedades.Contabilidad.CrearLote.Descontabilizar)), ltrCss.celdaNoVisible);
                const contenedor = ApiPanel.OcultarContenedorDto(this.ModalContabilizar, ltrPropiedades.Contabilidad.CrearLote.FechaContable, true);
                contenedor.parentElement.style.gridTemplateColumns = "auto";
            }
        }


        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);

            if (modal.id === (Crud.crudMnt as CrudDeEstimacionesDirectas).ModalContabilizar.id) {
                parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Contabilidad.Preasientos.CrearLoteContable, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        MensajesSe.Info(peticion.resultado.mensaje);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionEstimacionDirecta extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}

    }

    export class CrudEdicionEstimacionDirecta extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax) : boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Contabilidad.Preasientos.AnularEstimacionDirecta) {
                MensajesSe.Info('Trabajo de anulación de estimación directa sometido correctamente');
                return true;
            }
            return false;
        }
    }

}