namespace Entorno {


    export function CrearCrudDeMiCalendario(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Entorno.CrudDeMiCalendario(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeMiCalendario extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudDeCreacionDeEvento(this, idPanelCreacion);
            this.crudDeEdicion = new CrudDeEdicionDeEvento(this, idPanelEdicion);
        }
        
        protected InicializarPanelDeFiltro(div: HTMLDivElement): void {
            super.InicializarPanelDeFiltro(div);
            let inicio = ApiControl.BuscarFiltroEntreFechas(this.PanelFiltro, ltrPropiedades.Entorno.EventoDeAgenda.inicio) as HTMLInputElement;
            MapearAlControl.FechaDate(inicio, new Date());
        }

    }

    export class CrudDeCreacionDeEvento extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudDeEdicionDeEvento extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }


    export function MiCalendario_IrAlElemento(numeroDeFila: number) {
        let idEvento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        MiCalendario_IrAlElementoDelEvento(idEvento);
    }

    export function MiCalendario_IrAlElementoDelEvento(id: number) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Entorno.Agenda.IrAlElementoDelEvento, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }


}