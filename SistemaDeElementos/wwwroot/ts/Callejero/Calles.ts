

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

        // al seleccionar una o varias filas en el grid de Calles, si el panel de detalle
        // (splitter + div-graficos) estaba cerrado, se abre automáticamente para poder ver de
        // inmediato dónde se sitúa la calle (ver DespuesDeMapearDatosPrincipales). Solo afecta
        // a Calles: el resto de cruds conserva el comportamiento de base (no se abre solo).
        public EditarEnPanelDeGraficos(mostrarDto: boolean): void {
            const cantidad = this.InfoSelector.Seleccionados.length;

            if (cantidad > 0 && this.VisorDeDetalle?.classList.contains(ltrCss.crud.mostrarDetalle)) {
                ApiControl.ExcluirCss(this.VisorDeDetalle, ltrCss.crud.mostrarDetalle);
                ApiControl.IncluirCss(this.VisorDeDetalle, ltrCss.crud.ocultarDetalle);
            }

            // Calles no tiene panel de totales ni de archivos: con varias filas seleccionadas,
            // la lógica de la base (pensada para negocios con totales) alternaba entre mostrar
            // y ocultar el panel según la paridad del nº de seleccionados, sin llegar nunca a
            // MostrarDatosPrincipales. Aquí, con más de una fila, se sigue mostrando sin más el
            // mapa de la última fila pulsada.
            if (cantidad > 1 && mostrarDto) {
                if (!this.EstoyEnMantenimiento || !Definido(this.ContenedorDeTablaConGraficos) || EsDispositvoMovil())
                    return;
                this.AsegurarPanelDeGraficosVisible();
                this.MostrarDatosPrincipales(this.InfoSelector.IdsSeleccionados[this.InfoSelector.IdsSeleccionados.length - 1]);
                return;
            }

            super.EditarEnPanelDeGraficos(mostrarDto);
        }

        private AsegurarPanelDeGraficosVisible(): void {
            if (!Definido(this.ContenedorDeGraficos)) return;
            ApiControl.ExcluirCss(this.ContenedorDeGraficos, ltrCss.divNoVisible);
            ApiControl.ExcluirCss(this.Splitter, ltrCss.divNoVisible);
            ApiVisorDeArchivos.MostrarContenedorDeGraficos();
        }

        // tras mapear los datos principales de la calle seleccionada, se retira la tabla de
        // propiedades de div-graficos (igual que hace manualmente el botón de mostrar/ocultar
        // detalle) y se monta en su lugar el mismo asistente de mapa que usa la edición,
        // apuntando a esa calle. Cada selección sustituye el marcador anterior (no se acumulan).
        protected DespuesDeMapearDatosPrincipales(peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearDatosPrincipales(peticion);

            const divMapa = this.MontarMapaEnPanelDeGraficos();
            if (!divMapa) return;

            this.PonerElDtoEnEdicion();

            const pais: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Pais).substring(6);
            const provincia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Provincia);
            const municipio: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Municipio);
            const tipoDeVia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.TipoDeVia);
            const calle: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Nombre);
            const zona: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Zona, '');
            const cp: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Cp, '');

            GestorDeMapas.MostrarVisorDeOpenStreetView(divMapa, pais, provincia, municipio, zona, tipoDeVia, calle, cp);
        }

        private MontarMapaEnPanelDeGraficos(): HTMLDivElement {
            const contenedor = this.ContenedorDeGraficos;
            if (!Definido(contenedor)) return null;

            const idAsistente = 'div-graficos-calle-asistente-maps';
            let divAsistente = document.getElementById(idAsistente) as HTMLDivElement;
            if (!Definido(divAsistente)) {
                divAsistente = document.createElement('div');
                divAsistente.id = idAsistente;
                ApiControl.IncluirCss(divAsistente, ltrCss.crud.panelCreacion.AsistenteDelMapa);
                ApiControl.IncluirCss(divAsistente, ltrCss.crud.panelDeEdicion.AsistenteMapaEnEdicion);

                const divMapa = document.createElement('div');
                divMapa.id = `${idAsistente}-mapa`;
                ApiControl.IncluirCss(divMapa, ltrCss.crud.panelCreacion.divMapa);
                divAsistente.appendChild(divMapa);
            }
            contenedor.appendChild(divAsistente);
            return document.getElementById(`${idAsistente}-mapa`) as HTMLDivElement;
        }

    }

    export class CrudCreacionCalle extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public override InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            ApiCallejero.InicializarAsistenteDeMapas(Crud.crudMnt);
        }
    }

    export class CrudEdicionCalle extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected MapearOtraInformacion(peticion: ApiDeAjax.DescriptorAjax, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
            super.MapearOtraInformacion(peticion, modoDeAcceso);

            let pais: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Pais).substring(6);
            let provincia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Provincia);
            let municipio: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Municipio);
            let tipoDeVia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.TipoDeVia);
            let calle: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Nombre);
            let zona: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Zona, '');
            let cp: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Callejero.Calle.Cp, '');

            ApiCallejero.InicializarAsistenteDeMapas(Crud.crudMnt);

            const idAsistente = `${this.PanelDeEditar.id}-asistente-maps`;
            const divMapa = document.getElementById(`${idAsistente}-mapa`) as HTMLDivElement;
            if (divMapa) {
                GestorDeMapas.MostrarVisorDeOpenStreetView(divMapa, pais, provincia, municipio, zona, tipoDeVia, calle, cp);
            }

            const inputAsistente = document.getElementById(`${idAsistente}-input`) as HTMLInputElement;
            if (inputAsistente) {
                const provinciaLimpia = provincia.replace(/\(\d+\)\s*/, '');
                const partes = [`${tipoDeVia} ${calle}`, municipio, provinciaLimpia, cp, pais].filter(Boolean);
                inputAsistente.value = partes.join(', ');
            }

            // let mapaGm: HTMLDivElement = document.getElementById(`${this.PanelDeEditar.id}-mapas-gmaps-cuerpo-detalle`) as HTMLDivElement;
            // if (mapaGm) {
            //     mapaGm.style.height = "400px";
            //     mapaGm.style.width = "100%";
            //     mapaGm.style.display = ltrStyle.display.block;
            //     GestorDeMapas.MostrarFrameGoogleMaps(mapaGm, pais, provincia, municipio, zona, tipoDeVia, calle, cp);
            // }
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