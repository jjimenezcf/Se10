namespace Negocio {

    export function CrearCrudDeNegocios(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDeNegocios(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeNegocios extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionNegocio(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionNegocio(this, idPanelEdicion);
        }
    }

    export class CrudCreacionNegocio extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionNegocio extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
            if (modalDeEdicion.id === ApiDeExpansor.IdDeModalEditarRelacion(ltrModalDeEditarRelacion.Negocio.PlantillasDeNegocio)) {
                var referencia = ApiControl.BuscarReferencia(modalDeEdicion, ltrPropiedades.Negocio.PlantillaDeNegocio.Plantilla)
                var objeto = peticion.resultado.datos;
                referencia.innerText = ltrEtiquetas.Negocio.PlantillaDeNegocio.Plantilla + ': ' + ObtenerPropiedad(objeto, ltrPropiedades.Negocio.PlantillaDeNegocio.Plantilla);
            }
        }

    }


    export function Negocio_TrasAbrirModalDePlantillasDeNegocio(idModal: string, modo: string) {
        var modal = document.getElementById(idModal) as HTMLDivElement;
        ApiPanel.MostrarOcultarCelda(modal, ltrPropiedades.Negocio.PlantillaDeNegocio.IdPermiso, modo !== ltrEspanes.Opcion.crear);
        ApiPanel.MostrarOcultarCelda(modal, ltrPropiedades.Negocio.PlantillaDeNegocio.IdAccion, modo !== ltrEspanes.Opcion.crear);
    }

    export function Negocio_DescargarPlantillaDeNegocio(idArchivo: number) {
        let parametros = `idArchivo=${idArchivo}`;
        let descargar: string = `/${ltrControladores.Negocio.PlantillasDeNegocio}/${Ajax.EndPoint.Negocio.PlantillasDeNegocio.DescargarPlantilla}?${parametros}`;
        try {
            EntornoSe.AbrirPestana(descargar);
        }
        finally {
            MensajesSe.Info("Descarga realizada")
        }
    }

    export function Negocio_DescargarEtiquetasDeNegocio(idNegocio: number) {
        let parametros = `idNegocio=${idNegocio}`;
        let descargar: string = `/${ltrControladores.Negocio.PlantillasDeNegocio}/${Ajax.EndPoint.Negocio.PlantillasDeNegocio.DescargarEtiquetas}?${parametros}`;
        try {
            EntornoSe.AbrirPestana(descargar);
        }
        finally {
            MensajesSe.Info("Descarga realizada")
        }
    }
}