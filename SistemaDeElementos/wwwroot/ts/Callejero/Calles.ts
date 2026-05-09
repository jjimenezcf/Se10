

namespace Callejero {
    // declare let google: any;

    export function CrearCrudDeCalles(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Callejero.CrudDeCalles(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeCalles extends Crud.CrudMnt {

        protected get EditorDePais(): HTMLInputElement {
            let editor: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, Callejero.atributo.propiedad.idpais) as HTMLInputElement;
            if (NoDefinido(editor))
                MensajesSe.EmitirMensajeDeExcepcion("Propiedad EditorDePais", "No se lo caliza el editor de Pais en el filtro de Calle");
            return editor;
        };


        protected get EditorDeProvincia(): HTMLInputElement {
            let editor: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, Callejero.atributo.propiedad.idprovincia) as HTMLInputElement;
            if (NoDefinido(editor))
                MensajesSe.EmitirMensajeDeExcepcion("Propiedad EditorDeProvincia", "No se lo caliza el editor de Provincia en el filtro de Calle");
            return editor;
        };

        protected get EditorDeMunicipio(): HTMLInputElement {
            let editor: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelFiltro, Callejero.atributo.propiedad.idmunicipio) as HTMLInputElement;
            if (NoDefinido(editor))
                MensajesSe.EmitirMensajeDeExcepcion("Propiedad EditorDeMunicipio", "No se lo caliza el editor de Municipio en el filtro de Calle");
            return editor;
        };

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionCalle(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionCalle(this, idPanelEdicion);
        }

        public DespuesDeAplicarUnRestrictor(restrictor: Tipos.Restrictor) {
            super.DespuesDeAplicarUnRestrictor(restrictor);


            if (restrictor.Propiedad === Callejero.restrictor.codigoPostal) {
                ApiControl.BloquearInput(this.EditorDePais);
                ApiControl.BloquearInput(this.EditorDeProvincia);
                ApiControl.BloquearInput(this.EditorDeMunicipio);
            }

            if (restrictor.Propiedad === Callejero.restrictor.municipio) {
                let idMunicipio: number = restrictor.Valor;
                ApiDePeticiones.LeerElementoPorId(this, Callejero.controlador.municipio, idMunicipio, new Array<Parametro>(), idMunicipio)
                    .then((peticion: ApiDeAjax.DescriptorAjax) => ApiCallejero.MapearPaisProvincia(this, peticion))
                    .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));

            }
        }

    }

    export class CrudCreacionCalle extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionCalle extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected MapearOtraInformacion(peticion: ApiDeAjax.DescriptorAjax, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
            super.MapearOtraInformacion(peticion, modoDeAcceso);
            let mapaGm: HTMLDivElement = document.getElementById(`${this.PanelDeEditar.id}-mapas-gmaps-cuerpo-detalle`) as HTMLDivElement;
            mapaGm.style.height = "400px";
            mapaGm.style.width = "100%";
            mapaGm.style.display = ltrStyle.display.block;

            let mapaSv: HTMLDivElement = document.getElementById(`${this.PanelDeEditar.id}-mapas-street-cuerpo-detalle`) as HTMLDivElement;
            mapaSv.style.height = "400px";
            mapaSv.style.width = "100%";
            mapaSv.style.display = ltrStyle.display.block;

            let pais: string = ObtenerPropiedad(peticion.resultado.datos,ltrPropiedades.Callejero.Calle.Pais).substring(6);
            let provincia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Provincia); 
            let municipio: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Municipio);
            let tipoDeVia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.TipoDeVia);
            let calle: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Nombre); 
            let zona: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Zona,'');   
            let cp : string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Cp,'');       

            GestorDeMapas.MostrarFrameOpenStreetView(mapaSv, pais, provincia, municipio, zona, tipoDeVia, calle, cp);
            //GestorDeMapas.VisualizarMapaConGoogle(mapaGm, pais, provincia, municipio, zona, tipoDeVia, calle, cp);

            GestorDeMapas.MostrarFrameGoogleMaps(mapaGm, pais, provincia, municipio, zona, tipoDeVia, calle, cp);

        }
    }

    export function Calle_IrABarriosDeUnaCalle() {
        Calle_IrAUrlDeUnaCalle(ltrUrls.Callejero.Barrios);
    }
    export function Calle_IrAZonasDeUnaCalle() {
        Calle_IrAUrlDeUnaCalle(ltrUrls.Callejero.Zonas);
    }
    export function Calle_IrACpsDeUnaCalle() {
        Calle_IrAUrlDeUnaCalle(ltrUrls.Callejero.Cps);
    }
    function Calle_IrAUrlDeUnaCalle(url: string) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionCalle;
        let lista = ApiControl.BuscarListaDinamicaPorPropiedad(editor.PanelDeEditar, ltrPropiedades.Callejero.Calle.Municipio);
        let idMunicipio = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
        if (idMunicipio > 0) {
            let urlDestino = `${window.location.origin}/${url}?${ltrParametrosUrl.filtros}=[${ltrPropiedades.Callejero.Calle.IdMunicipio}=${idMunicipio}=${lista.value}]`;
            EntornoSe.AbrirPestana(urlDestino);
        }
        else
            MensajesSe.Info("Debe indicar el municipio");
    }
}