namespace Entorno {

    export function DespuesDeMapearEventos() {
        console.log("Eventos cargados");
        let visor = document.getElementById('scheduler_here') as HTMLDivElement;
        let eventos = visor.querySelectorAll('.dhx_cal_event') as NodeListOf<HTMLDivElement>;
        for (let i: number = 0; i < eventos.length; i++) {
            var contenedorDelHref = eventos[i].querySelector(`.${ltrCss.Agenda.EventoAjustadoAlContenedor}`) as HTMLDivElement;
            var span = contenedorDelHref.querySelector('span');

            var nuevoSpan = document.createElement('span') as HTMLSpanElement;
            nuevoSpan.innerHTML = SanitizeHTML(span.innerHTML);
            nuevoSpan.classList.add('tooltiptextDto');

            eventos[i].classList.add('tooltipDto');
            eventos[i].appendChild(nuevoSpan);
        }

    }

    export function DespuesDeMostrarAgenda() {
        console.log("Agenda Mostrada");
    }

    export function CrearCrudDeAgendas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Entorno.CrudDeAgendas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeAgendas extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionAgenda(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionAgenda(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
            let idAgenda = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
            switch (opcion) {
                case ltrMenus.eventosDeMf.Entorno.Agenda.AbrirAgenda:
                    let url = `${window.location.origin}/${ltrUrls.Entorno.VisorDeAgenda}?${ltrParametrosUrl.guid}=${generarUUID()}&${ltrParametrosUrl.idAgenda}=${idAgenda}`;
                    EntornoSe.AbrirPestana(url);
                    return true;
            }
            return false;
        }

    }

    export class CrudCreacionAgenda extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionAgenda extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }

    export function Agenda_AbrirAgendaSeleccionada(numeroDeFila: number) {
        let id = ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id);
        var url = `${window.location.origin}/${ltrUrls.Entorno.VisorDeAgenda}?${ltrParametrosUrl.guid}=${generarUUID()}&${ltrParametrosUrl.idAgenda}=${id}`
        EntornoSe.AbrirPestana(url);
    }

}