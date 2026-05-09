namespace Guarderias {

    export function CrearCrudDeCursosDeGuarderia(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Guarderias.CrudDeCursosDeGuarderia(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeCursosDeGuarderia extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionCursoDeGuarderia(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionCursoDeGuarderia(this, idPanelEdicion);
        }

    }

    export class CrudCreacionCursoDeGuarderia extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
       
    }

    export class CrudEdicionCursoDeGuarderia extends Crud.CrudEdicion {

        public get PanelDeIncluirProfesor(): HTMLDivElement {
            return document.getElementById(this._idDeModalCrearRelacion('profesores')) as HTMLDivElement;
        }

        public get PanelDeIncluirInfante(): HTMLDivElement {
            return document.getElementById(this._idDeModalCrearRelacion('infantes')) as HTMLDivElement;
        }
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


    }

    export function Cursos_InicializarModalParaIncluirProfesore() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionCursoDeGuarderia;
        ApiDelCrud.ProponerPropiedad(editor.PanelDelDto, editor.PanelDeIncluirProfesor, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true);
        ApiDelCrud.ProponerPropiedad(editor.PanelDelDto, editor.PanelDeIncluirProfesor, ltrPropiedades.Elemento.ConCg.IdCg, true);
    }
    export function Cursos_InicializarModalParaIncluirInfante() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionCursoDeGuarderia;
    }
}

