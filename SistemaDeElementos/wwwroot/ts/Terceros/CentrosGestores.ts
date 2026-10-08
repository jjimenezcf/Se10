namespace Terceros {

    export let JerarquiaDeCgs: Terceros.CentrosGestores = null;

    export function CrearFormulario(idFormulario: string, negocio: string) {
        JerarquiaDeCgs = new Terceros.CentrosGestores(idFormulario, negocio);
        window.addEventListener("load", function () { JerarquiaDeCgs.InicializarJerarquia(true); }, false);

        window.onbeforeunload = function () {
            JerarquiaDeCgs.AntesDeSalir();
        };
    }

    const ltrCentrosGestores = {
        propiedades: {
            negocioAccedidos: ltrPropiedades.Negocio.idNegocio,
            NegocioNoSeleccionado: literal.menos1,
            idUsuario: ltrPropiedades.Entorno.Usuario.id,
            idPuesto: ltrPropiedades.Entorno.seguridad.Puesto.id,
            idRol: ltrPropiedades.Entorno.seguridad.rol.id,
            enumTipoPermiso: ltrPropiedades.enumTipoPermiso
        }
    };

    export class CentrosGestores extends Formulario.Jerarquia {
        _IdSociedadUrl: number;
        _nombreSociedad: string;

        private get _GridDePuestos(): HTMLDivElement {
        return document.getElementById('detalle-puestos') as HTMLDivElement;
        }

        private get _GridDeNegocios(): HTMLDivElement {
            return document.getElementById('detalle-negocios') as HTMLDivElement
        }
        private get _GridDeAuditoria(): HTMLDivElement {
            return document.getElementById('detalle-audt') as HTMLDivElement;
        }
        constructor(idFormulario: string, negocio: string) {
            super(idFormulario, negocio);
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            super.InicializarJerarquia(blanquearFiltros);
        }

        // cada sociedad es una raíz del árbol, con sus centros gestores colgando de ella: no se muestra el nodo
        // "Centros de gestión" que las agrupaba
        public AntesDePintarLaJerarquia(): void {
            super.AntesDePintarLaJerarquia();
            this.Titulo.style.display = ltrStyle.display.none;
            this._IdSociedadUrl = ObtenerParametroUrl(ltrParametrosUrl.idSociedad, 0, false);
            if (this._IdSociedadUrl > 0 && this.jerarquia.ramas.length > 0)
                this._nombreSociedad = this.jerarquia.ramas[0].dto.nombre;
        }

        public DespuesDePintarLaJerarquia(): void {
            if (this._IdSociedadUrl === 0) {
                super.DespuesDePintarLaJerarquia();
                return;
            }

            // al llegar desde una sociedad se edita su primer centro gestor
            let primerCg = this.ContenedorDeJerarquia.querySelector(`li.${ltrCss.nodoDeJerarquia}:not([id^="No."])`) as HTMLLIElement;
            if (Definido(primerCg)) {
                Formulario.NodoSeleccionado(primerCg.id);
                return;
            }
            super.DespuesDePintarLaJerarquia();
        }

        // un click en una sociedad pliega o despliega sus centros gestores, y el doble click abre su edición en otra pestaña
        public AccionDelNodo(nodoDto: Tipos.NodoDeJerarquiaDto, idLi: string): string {
            if (this.EsSociedad(nodoDto.dto))
                return `javascript: ApiDeJerarquia.NodoPulsado('${idLi}', ()=>Terceros.EditarSociedad(${nodoDto.dto.id}))`;
            return super.AccionDelNodo(nodoDto, idLi);
        }

        private EsSociedad(dto: Tipos.NodoDto): boolean {
            return dto.negocio === ltrNegocioSe.Nombre.Sociedades;
        }

        public MapearElDtoLeido(dto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            ApiDeInicializacion.OcultarArchivos(this.PanelDelDto, false);
            super.MapearElDtoLeido(dto, modoDeAcceso);
        }

        public ComenzarModoNuevo(): void {
            super.ComenzarModoNuevo();

            ApiPanel.OcultarPanel(this._GridDeAuditoria);
            ApiPanel.OcultarPanel(this._GridDeNegocios);
            ApiPanel.OcultarPanel(this._GridDePuestos);

            if (this._IdSociedadUrl > 0)
                MapearAlControl.Propiedad(this.PanelDelDto, ltrPropiedades.Terceros.Sociedad.sociedad, this._IdSociedadUrl, this._nombreSociedad, true, true);
            else {
                var lista = ApiControl.BlanquearListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Terceros.Sociedad.sociedad);
                var lista = ApiControl.BlanquearListaDinamicaPorPropiedad(this.PanelDelDto, ltrPropiedades.Terceros.Sociedad.responsable);
                if (Registro.EsAdministrador())
                    ApiControl.DesbloquearListaDinamica(lista);
                else
                    ApiControl.BloquearListaDinamica(lista);
            }

            ApiDeInicializacion.Archivos(this.PanelDelDto);
            ApiDeInicializacion.OcultarArchivos(this.PanelDelDto, true);
            ApiPanel.OcultarPanel(this._GridDeAuditoria);
            ApiPanel.OcultarPanel(this._GridDeNegocios);
            ApiPanel.OcultarPanel(this._GridDePuestos);
        }

        public ComenzarModoEdicion(dto: any, modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos, nodoSeleccionado: string): void {
            super.ComenzarModoEdicion(dto, modoAcceso, nodoSeleccionado);
            if (this._IdSociedadUrl > 0)
                MapearAlControl.Propiedad(this.PanelDelDto, ltrPropiedades.Terceros.Sociedad.sociedad, this._IdSociedadUrl, this._nombreSociedad, true, true);
            else {
                var sociedad = ApiControl.BuscarControl(this.PanelDelDto, ltrPropiedades.Terceros.Sociedad.sociedad, true) as HTMLInputElement;
                ApiControl.BloquearInput(sociedad);
            }
            ApiDeMenuFlotante.MostrarLosMf(this.CabeceraDelFormulario);
            ApiPanel.MostrarPanel(document.getElementById('detalle-puestos') as HTMLDivElement);
            ApiPanel.MostrarPanel(document.getElementById('detalle-negocios') as HTMLDivElement);
            ApiPanel.MostrarPanel(document.getElementById('detalle-audt') as HTMLDivElement);
            //ApiControl.BloquearCheckPorPropiedad(this.PanelDelDto, ltrPropiedades.baja, false);

            let estaDeBaja = ObtenerPropiedad(dto, ltrPropiedades.baja, false);
            ApiDeMenuFlotante.InicializarMenuFlotante(this.MenuFormulario, ltrMenus.enumOrigen.formulario, enumCssOpcionMenu.DeElemento, modoAcceso);
            ApiDeMenuFlotante.AplicarBaja(ltrMenus.enumOrigen.formulario, this.MenuFormulario, estaDeBaja,
                                          this.ModoTrabajo === enumModoTrabajo.editando ?
                                          ModoAcceso.enumModoDeAccesoDeDatos.Gestor :
                                          Registro.EsAdministrador() ?
                                          ModoAcceso.enumModoDeAccesoDeDatos.Gestor :
                                          ModoAcceso.enumModoDeAccesoDeDatos.Consultor);

        }


        public EsNodoSeleccionable(dto: Tipos.NodoDto): boolean {
            if (super.EsNodoSeleccionable(dto)) {
                return !this.EsSociedad(dto);
            }
            return false;
        }

        public PrepararfiltrosParaLeerLaJerarquia(datos: Diccionario<any>): boolean {
            let hayFiltros: boolean = super.PrepararfiltrosParaLeerLaJerarquia(datos);
            if (!hayFiltros) {
                hayFiltros =
                    datos.Obtener(ltrCentrosGestores.propiedades.negocioAccedidos) !== ltrCentrosGestores.propiedades.NegocioNoSeleccionado
                || Numero(datos.Obtener(ltrCentrosGestores.propiedades.enumTipoPermiso)) > -1
                || datos.Obtener(ltrCentrosGestores.propiedades.idUsuario) > 0
                || datos.Obtener(ltrCentrosGestores.propiedades.idPuesto) > 0
                || datos.Obtener(ltrCentrosGestores.propiedades.idRol) > 0;
            }

            let idSociedad = ObtenerParametroUrl(ltrParametrosUrl.idSociedad, 0, false);
            if (Numero(idSociedad) > 0) datos.Agregar(ltrParametrosUrl.idSociedad.toLowerCase(), Numero(idSociedad));
            return hayFiltros;
        }


    }


    // abre en una nueva pestaña el crud de sociedades editando la sociedad indicada
    export function EditarSociedad(idSociedad: number) {
        EntornoSe.AbrirPestana(`${window.location.origin}/${ltrUrls.Terceros.Sociedades}?${ltrParametrosUrl.id}=${idSociedad}`);
    }

    export function CG_Tras_Blanquear_Responsable() {
        let panel: HTMLDivElement = JerarquiaDeCgs.PanelDelDto;
        let email = ApiControl.BuscarEditor(panel, ltrPropiedades.Terceros.Cg.eMail);
        email.value = '';
    }

    export function CG_Tras_Seleccionar_Responsable() {
        let panel: HTMLDivElement = JerarquiaDeCgs.PanelDelDto;
        let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Terceros.Cg.Responsable);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        let email = ApiControl.BuscarEditor(panel, ltrPropiedades.Terceros.Cg.eMail);
        email.value = ObtenerPropiedad(objeto, ltrPropiedades.Entorno.Usuario.eMail);
    }


}