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

        public AntesDePintarLaJerarquia(): void {
            super.AntesDePintarLaJerarquia();
            this._IdSociedadUrl = ObtenerParametroUrl(ltrParametrosUrl.idSociedad, 0, false);
            if (this._IdSociedadUrl > 0) {
                let sociedad = this.jerarquia.ramas[0];
                this._nombreSociedad = sociedad.dto.nombre;
                this.Titulo.text = sociedad.dto.nombre;
                for (let i = 0; i < this.jerarquia.ramas[0].hijos.length; i++)
                    this.jerarquia.ramas.push(this.jerarquia.ramas[0].hijos[i]);
                this.jerarquia.ramas.splice(0, 1);
            }
        }

        public DespuesDePintarLaJerarquia(): void {
            if (this._IdSociedadUrl === 0) {
                super.DespuesDePintarLaJerarquia();
                return;
            }

            if (this.jerarquia.ramas.length > 0) {
                let lis = this.ContenedorDeJerarquia.querySelectorAll('li') as NodeListOf<HTMLLIElement>;
                if (Definido(lis) && lis.length > 0 && lis[0].id.indexOf("No.") < 0)
                    Formulario.NodoSeleccionado(lis[0].id);
                return;
            }
            super.DespuesDePintarLaJerarquia();                
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
                return dto.negocio !== ltrNegocioSe.Nombre.Sociedades;
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