namespace MaestrosTecnico {

    const ltrUnitarios = {
        Propiedades: {
            Clase: 'clase',
            Naturaleza: 'naturaleza',
            Referencia: 'referencia',
            Proponer: 'proponer'
        }
    } 

    export function CrearCrudDeUnitarios(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new MaestrosTecnico.CrudDeUnitarios(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeUnitarios extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionUnitario(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionUnitario(this, idPanelEdicion);
        }

    }

    export class CrudCreacionUnitario extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            let referencia: HTMLInputElement = ApiControl.BuscarEditor(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrUnitarios.Propiedades.Referencia) as HTMLInputElement;
            ApiControl.BloquearInput(referencia);
        }
    }

    export class CrudEdicionUnitario extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }


    export function Unitario_DesbloquearReferencia() {
        if (Crud.crudMnt.ModoTrabajo !== enumModoTrabajo.creando) return;
        let referencia: HTMLInputElement = ApiControl.BuscarEditor(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrUnitarios.Propiedades.Referencia) as HTMLInputElement;
        let proponer: HTMLInputElement = ApiControl.BuscarCheck(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrUnitarios.Propiedades.Proponer) as HTMLInputElement;

        if (proponer.checked) ApiControl.BloquearInput(referencia);
        else ApiControl.DesbloquearEditor(referencia);
    }

    export function Unitario_ProponerReferencia() {
        if (Crud.crudMnt.ModoTrabajo !== enumModoTrabajo.creando) return;


        let clase: HTMLSelectElement = ApiControl.BuscarListaDeValores(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrUnitarios.Propiedades.Clase) as HTMLSelectElement;
        let naturaleza: HTMLSelectElement = ApiControl.BuscarListaDeElementos(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrUnitarios.Propiedades.Naturaleza) as HTMLSelectElement;
        let referencia: HTMLInputElement = ApiControl.BuscarEditor(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrUnitarios.Propiedades.Referencia) as HTMLInputElement;

        if (naturaleza.selectedIndex === 0) {
            ApiControl.BloquearInput(referencia);
            return;
        }
        ApiControl.DesbloquearEditor(referencia);

        let siglaClase: string = clase.options[clase.selectedIndex].label.substring(0,3);
        let siglaNatur: string = naturaleza.selectedIndex > 0 ? ObtenerSubcadenas(naturaleza.options[naturaleza.selectedIndex].label, '(', ')')[0] : "";    

        let proponer = `${siglaClase}.${siglaNatur}: `;
        if (referencia.value.indexOf(proponer) > -1) return;
        referencia.value = proponer;
        Unitario_DesbloquearReferencia();
    }
}

