namespace Administracion {


    export function CrearCrudDeActividades(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string, claseDeActividad: string) {
        Crud.crudMnt = new Administracion.CrudDeActividades(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeActividades extends Crud.CrudMnt {
        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionActividad(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionActividad(this, idPanelEdicion);
        }

        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            const ctrlTipo = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiListaDinamica.AsignarValor(ctrlTipo, this.MapIndicadores.get(ltrPropiedades.Expediente.Indicadores.IdTipoActividad),
                this.MapIndicadores.get(ltrPropiedades.Expediente.Indicadores.TipoActividad), true);
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);
            if (!clausulas.find(c => c.clausula === ltrPropiedades.Elemento.ConTipo.IdTipo)) {
                let propiedad: string = ltrPropiedades.Elemento.ConTipo.IdTipo;
                let criterio: string = literal.filtro.criterio.igual;
                let valor = this.MapIndicadores.get(ltrPropiedades.Expediente.Indicadores.IdTipoActividad);
                let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
                clausulas.push(clausula);
            }
            return clausulas;
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);

            return true;
        }

    }

    export class CrudCreacionActividad extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            ApiDelCrud.PosicionarCrudDeCreacionConCgYTipo(this.PanelDeCrear);
        }
    }

    export class CrudEdicionActividad extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public Expansor_TrasAbrirModal(idModal: string, acciones: string): HTMLDivElement {
            let modal = super.Expansor_TrasAbrirModal(idModal, acciones);
            if (modal.id === this._idDeModalCrearVinculo(ltrEspanes.Expedientes.ActividadesFormativas))
                ApiDelCrud.ProponerElTipo(modal, acciones)
            return modal;
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            return false;
        }


        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);

        }



    }



}
