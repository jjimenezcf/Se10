

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

        private _nominatimResultado: any = null;

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public override InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            this.InicializarAsistenteDeMapas();
        }

        private get IdAsistente(): string {
            return `${this.PanelDeCrear.id}-asistente-maps`;
        }

        private InicializarAsistenteDeMapas(): void {
            if (document.getElementById(this.IdAsistente)) return;

            const divCuerpo = this.PanelDeCrear.querySelector('.' + ltrCss.contenedorEdicionCuerpo) as HTMLDivElement;
            if (!divCuerpo) return;

            // Hacer que el row del cuerpo ocupe el espacio disponible en lugar de auto
            const divCuerpoCreacion = divCuerpo.closest('.' + ltrCss.crud.creacion) as HTMLDivElement;
            if (divCuerpoCreacion) {
                ApiControl.IncluirCss(divCuerpo, ltrCss.crud.panelCreacion.CuerpoCreacionConMapa);
            }

            ApiControl.IncluirCss(divCuerpo, ltrCss.crud.panelCreacion.CreacionConMapa);

            // El primer hijo (formulario) ocupa la mitad
            const primerHijo = divCuerpo.firstElementChild as HTMLElement;
            if (primerHijo) {
                ApiControl.IncluirCss(primerHijo, ltrCss.crud.panelCreacion.DtoConMapa);
            }

            const divAsistente = document.createElement('div');
            divAsistente.id = this.IdAsistente;
            ApiControl.IncluirCss(divAsistente, ltrCss.crud.panelCreacion.AsistenteDelMapa);

            // Sub-div 1: barra de búsqueda
            const divCabecera = document.createElement('div');
            ApiControl.IncluirCss(divCabecera, ltrCss.crud.panelCreacion.barraBusquedaMapa);

            const inputDireccion = document.createElement('input');
            inputDireccion.type = 'text';
            inputDireccion.id = `${this.IdAsistente}-input`;
            inputDireccion.placeholder = 'Escriba la dirección a buscar...';
            inputDireccion.addEventListener('keydown', (e) => { if (e.key === 'Enter') this.BuscarEnMaps(); });

            const btnBuscar = document.createElement('input');
            btnBuscar.type = 'button';
            btnBuscar.value = 'Buscar';
            btnBuscar.className = enumCssOpcionMenu.DeElemento;
            btnBuscar.onclick = () => this.BuscarEnMaps();

            const btnMapear = document.createElement('input');
            btnMapear.type = 'button';
            btnMapear.id = `${this.IdAsistente}-btn-mapear`;
            btnMapear.value = 'Mapear';
            btnMapear.title = 'mapea los datos de la calle marcada para poder crear'
            btnMapear.className = enumCssOpcionMenu.DeElemento;
            btnMapear.style.display = 'none';
            btnMapear.onclick = () => this.MapearDesdeAsistente();

            divCabecera.appendChild(inputDireccion);
            divCabecera.appendChild(btnBuscar);
            divCabecera.appendChild(btnMapear);

            // Sub-div 2: mapa (ocupa el espacio restante del asistente)
            const divMapa = document.createElement('div');
            divMapa.id = `${this.IdAsistente}-mapa`;
            ApiControl.IncluirCss(divMapa, ltrCss.crud.panelCreacion.divMapa);

            divAsistente.appendChild(divCabecera);
            divAsistente.appendChild(divMapa);

            divCuerpo.appendChild(divAsistente);

            // Mapa inicial: España completa con embed limpio (solo mapa, sin UI de Nominatim)
            GestorDeMapas.MostrarFrameStreetViewPorTexto(divMapa, 'España', 5);
        }

        private async BuscarEnMaps(): Promise<void> {
            const input = document.getElementById(`${this.IdAsistente}-input`) as HTMLInputElement;
            if (!input || IsNullOrEmpty(input.value)) {
                MensajesSe.Info('Indique una dirección para buscar');
                return;
            }
            const divMapa = document.getElementById(`${this.IdAsistente}-mapa`) as HTMLDivElement;
            const btnMapear = document.getElementById(`${this.IdAsistente}-btn-mapear`) as HTMLInputElement;
            this._nominatimResultado = null;
            if (btnMapear) btnMapear.style.display = 'none';

            GestorDeMapas.MostrarFrameStreetViewPorTexto(divMapa, input.value, 0.002);

            const resultado = await ApiCallejero.BuscarDireccionEnNominatim(input.value);
            if (resultado) {
                this._nominatimResultado = resultado;
                if (btnMapear) btnMapear.style.display = '';
            }
        }

        private ParsearRuta(ruta: string): { tipoDeVia: string, nombre: string } {
            if (IsNullOrEmpty(ruta))
                return { tipoDeVia: '', nombre: '' };
            const partes = ruta.trim().split(' ');
            if (partes.length <= 1)
                return { tipoDeVia: '', nombre: ruta };
            return { tipoDeVia: partes[0], nombre: partes.slice(1).join(' ') };
        }

        private async MapearDesdeAsistente(): Promise<void> {
            if (!this._nominatimResultado) return;
            const addr = this._nominatimResultado.address;
            const rutaCompleta = addr.road || '';
            const { tipoDeVia, nombre } = this.ParsearRuta(rutaCompleta);
            const municipioNombre = addr.city || addr.town || addr.village || addr.municipality || '';
            const cpCodigo = addr.postcode || '';

            ApiCallejero.ValidarSiExisteCalle(this.PanelDeCrear,tipoDeVia, nombre, municipioNombre, cpCodigo)
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