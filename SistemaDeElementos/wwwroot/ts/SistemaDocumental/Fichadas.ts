namespace SistemaDocumental {

    enum enumEtapasDeCircuitoDoc {
    }

    function ParsearEtapa(etapa: string): enumEtapasDeCircuitoDoc {

        MensajesSe.EmitirExcepcion("Parsear etapa de registro de fichadas", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaLaFichadaEnEtapa(etapas: string, etapa: enumEtapasDeCircuitoDoc): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function CrearCrudDeFichadas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new SistemaDocumental.CrudDeFichadas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () {
            Crud.crudMnt.Inicializar(idPanelMnt);
        }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeFichadas extends Crud.CrudMnt {

        public get ModalContabilizar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Contabilidad.Preasientos.CrearLoteContable) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionFichada(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionFichada(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            const ctrlTipo = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiListaDinamica.AsignarValor(ctrlTipo, this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.IdTipoFichada),
                this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.TipoFichada), true);
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);

            if (!clausulas.find(c => c.clausula === ltrPropiedades.Elemento.ConTipo.IdTipo)) {
                let propiedad: string = ltrPropiedades.Elemento.ConTipo.IdTipo;
                let criterio: string = literal.filtro.criterio.igual;
                let valor = this.MapIndicadores.get(ltrPropiedades.SisDoc.CircuitosDoc.Indicadores.IdTipoFichada);
                let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
                clausulas.push(clausula);
            }
            return clausulas;
        }
    }

    export class CrudCreacionFichada extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionFichada extends Crud.CrudEdicion {

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