namespace Juridico {

    export function CrearCrudDePleitos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Juridico.CrudDePleitos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export function InicializarModalParaCrearMinuta(idModal: string) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        ApiControl.AsignarFecha(modal, ltrPropiedades.Juridico.Minuta.creadoEl, new Date());
        let tabla: HTMLDivElement = document.getElementById('grid-de-detalle-minuta-tabla') as HTMLDivElement;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector('input[propiedad = orden]') as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Juridico.Minuta.orden, true) as HTMLInputElement;
        orden.value = (valor + 10).toString();

        let concepto = ApiControl.BuscarControl(modal, ltrPropiedades.Juridico.Minuta.concepto, true) as HTMLInputElement;
        concepto.focus();
    }

    export class CrudDePleitos extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPleito(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPleito(this, idPanelEdicion);
        }

    }

    export class CrudCreacionPleito extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionPleito extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Juridico.Pleitos.VincularRegistroEntrada) {
                let idModal = `${this.PanelDeEditar.id}-registroses-${enumPostfijoTipoModal.ModalParaVincular}`.toLowerCase();
                    this.Expansor_AbrirModalDeCrearVinculoCon(idModal, 0);
                    return true;
            }
            return false;
        }

    }
}

