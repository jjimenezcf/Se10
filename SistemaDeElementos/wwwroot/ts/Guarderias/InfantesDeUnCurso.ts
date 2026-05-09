namespace Guarderias {

    export function CrearCrudInfantesDeUnCurso(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Guarderias.CrudInfantesDeUnCurso(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudInfantesDeUnCurso extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionInfantesDeUnCurso(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionInfantesDeUnCurso(this, idPanelEdicion);
        }


        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            let curso: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelFiltro, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeFiltro);
            let idCurso: number = Numero(curso.getAttribute(atRestrictor.idRestrictor));
            let parametros = new Array<Parametro>();

            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Guarderias.Cursos, idCurso, parametros, curso)
                .then((peticion) => this.AplicarEstadoDelCurso(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax): any {
            super.DespuesDeLeerFilaSeleccionada(peticion);
            let elemento = peticion.resultado.datos;
        }

        private AplicarEstadoDelCurso(peticion: ApiDeAjax.DescriptorAjax) {
            let elemento = peticion.resultado.datos;
           
            let esGestor: boolean = ModoAcceso.EsGestor(ObtenerPropiedad(elemento, ltrPropiedades.Elemento.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor))

            if (!esGestor) {
                ApiControl.CambiarLiteralDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Editar, ltrMenus.BarraDeMenu.Consultar);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Borrar);
                ApiControl.BloquearOpcionDeMenuPorNombre(Crud.crudMnt.ZonaDeMenu, ltrMenus.BarraDeMenu.Guarderia.InfantesDeUnCurso.IncluirInfantes);
            }
            else {

            }
        }

    }

    export class CrudCreacionInfantesDeUnCurso extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionInfantesDeUnCurso extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);


        }
    }
}