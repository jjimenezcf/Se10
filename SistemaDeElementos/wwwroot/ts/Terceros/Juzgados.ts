namespace Terceros {

    export let crudDeJuzgado: Terceros.CrudDeJuzgados = null;


    export function CrearCrudDeJuzgados(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeJuzgados(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeJuzgado = Crud.crudMnt as Terceros.CrudDeJuzgados;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeJuzgados extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionJuzgado(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionJuzgado(this, idPanelEdicion);
        }

        public get ModalImportarCatalogo(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Terceros.Juzgado.ImportarCatalogo) as HTMLDivElement; }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Terceros.Juzgado.ImportarCatalogo) {
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(this.ModalImportarCatalogo.id, 0);
                return true;
            }
            return false;
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id === this.ModalImportarCatalogo.id) {
                let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);

                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Terceros.Juzgado.ImportarCatalogo, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        MensajesSe.Info(peticion.resultado.consola);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionJuzgado extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionJuzgado extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }
    }

    export function Juzgado_Clase_OnBlur() {
        Juzgado_ComponerNombre();
    }

    export function Juzgado_Calificador_Change() {
        Juzgado_ComponerNombre();
    }

    export function Juzgado_Municipio_OnSelect() {
        Juzgado_ComponerNombre();
    }

    function Juzgado_ComponerNombre() {

        let indice: number = crudDeJuzgado.ModoTrabajo === enumModoTrabajo.creando ? 0 : 1;
        let panel: HTMLDivElement = (document.getElementsByClassName(ltrCss.contenedorEdicionCuerpo) as HTMLCollectionOf<HTMLDivElement>)[indice];
        let clase = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Terceros.Juzgado.clase);
        let municipio = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Terceros.Juzgado.municipio);
        let calificador = ApiControl.BuscarEditor(panel, ltrPropiedades.Terceros.Juzgado.calificador);
        let nombre = ApiControl.BuscarEditor(panel, ltrPropiedades.Terceros.Juzgado.nombre);

        let nombreClase: string = clase.selectedIndex > 0 ? clase.options[clase.selectedIndex].label : "";

        nombre.value = `${nombreClase} ${calificador.value} de ${municipio.value} `;
    }
}