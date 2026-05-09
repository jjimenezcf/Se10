namespace Gasto {

    export function CrearCrudPagosDeUnaRemesa(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Gasto.CrudPagosDeUnaRemesa(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudPagosDeUnaRemesa extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPagosDeUnaRemesa(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPagosDeUnaRemesa(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            let remesa: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelFiltro, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeFiltro);
            let idRemesa: number = Numero(remesa.getAttribute(atRestrictor.idRestrictor));
            let parametros = new Array<Parametro>();

            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Gasto.RemesasPag, idRemesa, parametros, remesa)
                .then((peticion) => this.AplicarEtapaDeLaRemesa(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax): any {
            super.DespuesDeLeerFilaSeleccionada(peticion);
            let elemento = peticion.resultado.datos;
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Gasto.PagosDeUnaRemesa.Etapas, false);
        }

        private AplicarEtapaDeLaRemesa(peticion: ApiDeAjax.DescriptorAjax) {
            let elemento = peticion.resultado.datos;
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Gasto.RemesaPag.Etapas, false);
            let esGestor: boolean = ModoAcceso.EsGestor(ObtenerPropiedad(elemento, ltrPropiedades.Elemento.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor))

            if (!esGestor) {
                ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Editar, ltrMenus.BarraDeMenu.Consultar);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Gastos.PagosDeUnaRemesa.IncluirPagos);
            }
            else {
                var enumerados = new Array<enumEtapasDeRemesaPag>(enumEtapasDeRemesaPag.REM_Etapa_Cancelada, enumEtapasDeRemesaPag.REM_Etapa_De_Cumplimentacion, enumEtapasDeRemesaPag.REM_Etapa_De_Cierre, enumEtapasDeRemesaPag.REM_Etapa_Generada);
                if (EstaAlgunEnumerado(etapas, enumEtapasDeRemesaPag, enumerados))
                    ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Editar, ltrMenus.BarraDeMenu.Consultar);
                else
                    ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Consultar, ltrMenus.BarraDeMenu.Editar);

                if (!EstaElEnumerado(etapas, enumEtapasDeRemesaPag, enumEtapasDeRemesaPag.REM_Etapa_De_Cumplimentacion)) {
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Gastos.PagosDeUnaRemesa.IncluirPagos);
                }
                else {
                    ApiControl.DesbloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                    ApiControl.DesbloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                    ApiControl.DesbloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Gastos.PagosDeUnaRemesa.IncluirPagos);
                }
            }
        }

    }

    export class CrudCreacionPagosDeUnaRemesa extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionPagosDeUnaRemesa extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.RemesaPag.Etapas, false);
            if (!this.EsGestor) return;

            if (!EstaElEnumerado(etapas, enumEtapasDeRemesaPag, enumEtapasDeRemesaPag.REM_Etapa_De_Presentacion)) {
                ApiPanel.PonerEnModoConsulta(this.PanelDeEditar);
            }
            else {
                ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Gasto.PagosDeUnaRemesa.Pago);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Gasto.PagosDeUnaRemesa.PagadoEl);
            }

        }
    }
}