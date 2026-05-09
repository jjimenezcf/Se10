namespace SistemaDocumental {

    enum enumEtapasDeCircuitoDoc {
    }

    function ParsearEtapa(etapa: string): enumEtapasDeCircuitoDoc {

        MensajesSe.EmitirExcepcion("Parsear etapa de circuito documental", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaElCircuitoEnEtapa(etapas: string, etapa: enumEtapasDeCircuitoDoc): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }


    export function CrearConsultaDeCad(idPanelEdicion: string) {
        Crud.Consultor = new SistemaDocumental.CrudEdicionCircuitoDoc(null, idPanelEdicion);
    }

    export function CrearCrudDeCircuitosDoc(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new SistemaDocumental.CrudDeCircuitosDoc(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeCircuitosDoc extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionCircuitoDoc(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionCircuitoDoc(this, idPanelEdicion);
        }


    }

    export class CrudCreacionCircuitoDoc extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}

    }

    export class CrudEdicionCircuitoDoc extends Crud.CrudEdicion {

        private get _esLoteContable(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.SisDoc.CircuitosDoc.EsLoteContable)
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            return false;
        }
    }

}