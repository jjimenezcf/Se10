namespace SistemaDocumental {

    enum enumEtapasDeCircuitoDoc {
    }

    function ParsearEtapa(etapa: string): enumEtapasDeCircuitoDoc {

        MensajesSe.EmitirExcepcion("Parsear etapa de registro de ActividadesFormativas", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaLaActividadFormativaEnEtapa(etapas: string, etapa: enumEtapasDeCircuitoDoc): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function CrearCrudDeActividadesFormativas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new SistemaDocumental.CrudDeActividadesFormativas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () {
            Crud.crudMnt.Inicializar(idPanelMnt);
        }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeActividadesFormativas extends Crud.CrudMnt {

        public get ModalContabilizar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Contabilidad.Preasientos.CrearLoteContable) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionActividadesFormativa(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionActividadesFormativa(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            const ctrlTipo = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiListaDinamica.AsignarValor(ctrlTipo, this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.IdTipoActividadFormativa),
                                                    this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.TipoActividadFormativa), true);
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);

            if (!clausulas.find(c => c.clausula === ltrPropiedades.Elemento.ConTipo.IdTipo)) {
                let propiedad: string = ltrPropiedades.Elemento.ConTipo.IdTipo;
                let criterio: string = literal.filtro.criterio.igual;
                let valor = this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.IdTipoActividadFormativa);
                let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
                clausulas.push(clausula);
            }
            return clausulas;
        }
    }

    export class CrudCreacionActividadesFormativa extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionActividadesFormativa extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
        }
    }

}