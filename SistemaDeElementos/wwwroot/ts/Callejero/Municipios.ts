namespace Callejero {

    export function CrearCrudDeMunicipios(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Callejero.CrudDeMunicipios(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeMunicipios extends Crud.CrudMnt {

        protected get EditorDePais(): HTMLInputElement {
            let editor: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, Callejero.atributo.propiedad.idpais) as HTMLInputElement;
            if (NoDefinido(editor))
                MensajesSe.EmitirMensajeDeExcepcion("Propiedad EditorDePais", "No se lo caliza el editor de Pais en el filtro de Municipio");
            return editor;
        };


        protected get EditorDeProvincia(): HTMLInputElement {
            let editor: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, Callejero.atributo.propiedad.idprovincia) as HTMLInputElement;
            if (NoDefinido(editor))
                MensajesSe.EmitirMensajeDeExcepcion("Propiedad EditorDeProvincia", "No se lo caliza el editor de Provincia en el filtro de Municipio");
            return editor;
        };

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionMunicipio(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionMunicipio(this, idPanelEdicion);
        }

        public DespuesDeAplicarUnRestrictor(restrictor: Tipos.Restrictor) {
            super.DespuesDeAplicarUnRestrictor(restrictor);


            if (restrictor.Propiedad === Callejero.restrictor.codigoPostal) {
                ApiControl.BloquearInput(this.EditorDePais);
                ApiControl.BloquearInput(this.EditorDeProvincia);
            }

            if (restrictor.Propiedad === Callejero.restrictor.provincia) {
                let idProvincia: number = restrictor.Valor;
                ApiDePeticiones.LeerElementoPorId(this, Callejero.controlador.provincia, idProvincia, new Array<Parametro>(), idProvincia)
                    .then((peticion: ApiDeAjax.DescriptorAjax) => ApiCallejero.MapearPais(this, peticion))
                    .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
            }
        }
    }

    export class CrudCreacionMunicipio extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionMunicipio extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }
}