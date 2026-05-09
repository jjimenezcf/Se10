namespace Venta {

    export function CrearCrudFacturasEmtDeUnaRemesa(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Venta.CrudFacturasEmtDeUnaRemesa(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudFacturasEmtDeUnaRemesa extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionFacturasEmtDeUnaRemesa(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionFacturasEmtDeUnaRemesa(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            let remesa: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelFiltro, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeFiltro);
            let idRemesa: number = Numero(remesa.getAttribute(atRestrictor.idRestrictor));
            let parametros = new Array<Parametro>();

            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Venta.RemesasFae, idRemesa, parametros, remesa)
                .then((peticion) => this.AplicarEtapaDeLaRemesa(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax): any {
            super.DespuesDeLeerFilaSeleccionada(peticion);
            let elemento = peticion.resultado.datos;
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.Etapas, false);
        }

        private AplicarEtapaDeLaRemesa(peticion: ApiDeAjax.DescriptorAjax) {
            let elemento = peticion.resultado.datos;
            let etapas: Array<string> = ObtenerPropiedad(elemento, ltrPropiedades.Venta.RemesaFae.Etapas, false);
            let esGestor: boolean = ModoAcceso.EsGestor(ObtenerPropiedad(elemento, ltrPropiedades.Elemento.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor))

            if (!esGestor) {
                ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Editar, ltrMenus.BarraDeMenu.Consultar);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Ventas.FacturasDeUnaRemesa.IncluirFacturas);
            }
            else {
                var enumerados = new Array<enumEtapasDeRemesaFae>(enumEtapasDeRemesaFae.REM_Etapa_Cancelada, enumEtapasDeRemesaFae.REM_Etapa_De_Cumplimentacion, enumEtapasDeRemesaFae.REM_Etapa_De_Cierre, enumEtapasDeRemesaFae.REM_Etapa_Generada);
                if (EstaAlgunEnumerado(etapas, enumEtapasDeRemesaFae, enumerados))
                    ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Editar, ltrMenus.BarraDeMenu.Consultar);
                else
                    ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Consultar, ltrMenus.BarraDeMenu.Editar);

                if (!EstaElEnumerado(etapas, enumEtapasDeRemesaFae, enumEtapasDeRemesaFae.REM_Etapa_De_Cumplimentacion)) {
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                    ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Ventas.FacturasDeUnaRemesa.IncluirFacturas);
                }
                else {
                    ApiControl.DesbloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Crear);
                    ApiControl.DesbloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                    ApiControl.DesbloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Ventas.FacturasDeUnaRemesa.IncluirFacturas);
                }
            }
        }

    }

    export class CrudCreacionFacturasEmtDeUnaRemesa extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionFacturasEmtDeUnaRemesa extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Venta.RemesaFae.Etapas, false);
            if (!this.EsGestor) return;

            if (!EstaElEnumerado(etapas, enumEtapasDeRemesaFae, enumEtapasDeRemesaFae.REM_Etapa_De_Presentacion)) {
                ApiPanel.PonerEnModoConsulta(this.PanelDeEditar);
            }
            else {
                ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.Factura);
                ApiControl.BloquearSelectorDeFechaPorPropiedad(this.PanelDeEditar, ltrPropiedades.Venta.FacturasEmtDeUnaRemesa.CargadaEl);
            }

        }
    }
}