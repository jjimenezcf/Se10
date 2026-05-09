namespace Guarderias {

    export function CrearCrudDeInfantes(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Guarderias.CrudDeInfantes(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export function CrearConsultaDeInfante(idPanelEdicion: string) {
        Crud.Consultor = new Guarderias.CrudEdicionInfante(null, idPanelEdicion);
    }

    export class CrudDeInfantes extends Crud.CrudMnt {

        public get ModalAsociarCurso(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Guarderias.Infante.AsociarCurso) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionInfante(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionInfante(this, idPanelEdicion);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalAsociarCurso.id) {
                let idInfante = ObtenerPropiedad(this.crudDeEdicion.Registro, literal.id);
                let infante = ObtenerPropiedad(this.crudDeEdicion.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.Guarderias.Infante.IdInfante, idInfante, infante);
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            if (modal.id === this.ModalAsociarCurso.id) {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
                let datosDeEntrada = new Array<Parametro>();
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Guarderias.Infantes.AsociarCurso, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else
                super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionInfante extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionInfante extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            if (this.PaginaDeConsultaConGuid)
                return;
            var idCurso = ObtenerPropiedad(this.Registro, ltrPropiedades.Guarderias.Infante.IdCurso);
            ApiDeMenuFlotante.CambiarNombre(this.ContenedorMenu, ltrMenus.eventosDeMf.Guarderias.Infante.AsociarCurso, Numero(idCurso) === 0 ? 'Asignar curso' : 'Cambiar de curso')
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Guarderias.Infante.AsociarCurso, ltrMenus.enumOrigen.edicion, !this.EsInterventor);

        }


        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);

            if (opcion === ltrMenus.eventosDeMf.Guarderias.Infante.AsociarCurso) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDeInfantes).ModalAsociarCurso.id, this.Registro.id);
                return true;
            }
            return false;
        }
    }

}

