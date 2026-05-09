namespace Venta {


    export const enumClaseDePresupuesto = {
        Venta: 'venta',
        Gasto: 'gasto',
        inversion: 'inversion'
    }; 

    export let JerarquiaPpt: Venta.TiposDePpt = null;

    export function CrearFormulario(idFormulario: string) {
        JerarquiaPpt = new Venta.TiposDePpt(idFormulario);
        window.addEventListener("load", function () { JerarquiaPpt.InicializarJerarquia(true); }, false);

        window.onbeforeunload = function () {
            JerarquiaPpt.AntesDeSalir();
        };
    }

    export class TiposDePpt extends Negocio.TiposDeElemento {

        constructor(idFormulario: string) {
            super(idFormulario, ltrNegocioSe.Nombre.Venta.Presupuesto);
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            super.InicializarJerarquia(blanquearFiltros);
        }

        public MapearElDtoLeido(dto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.MapearElDtoLeido(dto, modoDeAcceso);
            let clase = ApiControl.BuscarListaDeValores(this.PanelDelDto, ltrPropiedades.Venta.TipoPpt.ClaseDePpt);
            Ppt_TrasSeleccionarClaseDePpt(clase);
        }

        public ComenzarModoNuevo(): void {
            super.ComenzarModoNuevo();
            let clase = ApiControl.BuscarListaDeValores(this.PanelDelDto, ltrPropiedades.Venta.TipoPpt.ClaseDePpt);
            Ppt_TrasSeleccionarClaseDePpt(clase);
        }

    }

    export function Ppt_TrasSeleccionarClaseDePpt(selector: HTMLSelectElement) {
        if (!(selector instanceof HTMLSelectElement))
            return;
        let tipoFactura: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(JerarquiaPpt.PanelDelDto, ltrPropiedades.Venta.TipoPpt.TipoFacturaEmt);
        let tipoParte: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(JerarquiaPpt.PanelDelDto, ltrPropiedades.Venta.TipoPpt.TipoParteTr);
        if (selector.value === enumClaseDePresupuesto.Venta) {
            ApiControl.DesbloquearListaDinamica(tipoFactura);
            ApiControl.DesbloquearListaDinamica(tipoParte);
        }
        else {
            ApiControl.BloquearListaDinamica(tipoFactura);
            ApiControl.BloquearListaDinamica(tipoParte);
        }
    }
}