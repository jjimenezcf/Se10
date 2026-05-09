namespace Juridico {



    export let JerarquiaCtr: Juridico.TiposDeCtr = null;

    export function CrearFormulario(idFormulario: string) {
        JerarquiaCtr = new Juridico.TiposDeCtr(idFormulario);
        window.addEventListener("load", function () { JerarquiaCtr.InicializarJerarquia(true); }, false);

        window.onbeforeunload = function () {
            JerarquiaCtr.AntesDeSalir();
        };
    }

    export class TiposDeCtr extends Negocio.TiposDeElemento {

        constructor(idFormulario: string) {
            super(idFormulario, ltrNegocioSe.Nombre.Juridico.Contrato);
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            super.InicializarJerarquia(blanquearFiltros);
        }

        public MapearElDtoLeido(dto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.MapearElDtoLeido(dto, modoDeAcceso);
            let clase = ApiControl.BuscarListaDeValores(this.PanelDelDto, ltrPropiedades.Juridico.TipoCtr.ClaseDeCtr);
            Ctr_TrasSeleccionarClaseDeCtr(clase);
        }

        public ComenzarModoNuevo(): void {
            super.ComenzarModoNuevo();
            let clase = ApiControl.BuscarListaDeValores(this.PanelDelDto, ltrPropiedades.Juridico.TipoCtr.ClaseDeCtr);
            Ctr_TrasSeleccionarClaseDeCtr(clase);
        }

    }

    export function Ctr_TrasSeleccionarClaseDeCtr(selector: HTMLSelectElement) {
        if (!(selector instanceof HTMLSelectElement))
            return;
        let tipoFactura: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(JerarquiaCtr.PanelDelDto, ltrPropiedades.Juridico.TipoCtr.TipoFacturaEmt);
        if (selector.value === enumClaseDeContrato.Venta) {
            ApiControl.DesbloquearListaDinamica(tipoFactura);
        }
        else {
            ApiControl.BloquearListaDinamica(tipoFactura);
        }
    }
}