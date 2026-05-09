namespace Negocio {

    export let JerarquiaDeTipos: Negocio.TiposDeElemento = null;

    export function CrearFormulario(idFormulario: string, negocio: string) {
        JerarquiaDeTipos = new Negocio.TiposDeElemento(idFormulario, negocio);
        window.addEventListener("load", function () { JerarquiaDeTipos.InicializarJerarquia(true); }, false);

        window.onbeforeunload = function () {
            JerarquiaDeTipos.AntesDeSalir();
        };
    }

    const ltrTiposDeElemento = {
        propiedades: {
            modoDeAcceso: ltrPropiedades.TipoDeElemento.ModoDeAcceso.toLowerCase(),
            modoDeAccesoNoSeleccionado: literal.menos1
        }
    };

    export class TiposDeElemento extends Formulario.Jerarquia {

        private get _EspanDePlantillas(): HTMLDivElement {
            return document.getElementById(ltrEspanes.TiposDeElemento.Plantillas) as HTMLDivElement;
        }

        private get _EspanDeClases(): HTMLDivElement {
            return document.getElementById(ltrEspanes.TiposDeElemento.Clases) as HTMLDivElement;
        }

        constructor(idFormulario: string, negocio: string) {
            super(idFormulario, negocio);
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            super.InicializarJerarquia(blanquearFiltros);
            MapearAlControl.Propiedad(this.PanelDelDto, literal.negocio, 0, this.Negocio, true, true);
            ApiDeInicializacion.AsignarAtributo(this.PanelDelDto, literal.padre, literal.negocio, this.Negocio);
        }

        public ComenzarModoNuevo(): void {
            super.ComenzarModoNuevo();
            this.OcultarPermisos();
            if (Definido(this._EspanDePlantillas)) ApiPanel.OcultarPanel(this._EspanDePlantillas);
            if (Definido(this._EspanDeClases)) ApiPanel.OcultarPanel(this._EspanDeClases);
        }

        public ComenzarModoEdicion(dto: any, modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos, nodoSeleccionado: string): void {
            super.ComenzarModoEdicion(dto, modoAcceso, nodoSeleccionado);
            var esAdministrador = ModoAcceso.EsAdministrador(ObtenerPropiedad(dto, ltrPropiedades.TipoDeElemento.ModoDeAcceso));
            if (Definido(this._EspanDePlantillas)) {
                ApiPanel.MostrarPanel(document.getElementById(ltrEspanes.TiposDeElemento.Plantillas) as HTMLDivElement);
                ApiControl.MostrarMcrRefSi(ltrEspanes.TiposDeElemento.ref.CrearPlantilla, esAdministrador);
            }

            if (Definido(this._EspanDeClases)) {
                ApiPanel.MostrarPanel(document.getElementById(ltrEspanes.TiposDeElemento.Clases) as HTMLDivElement);
                ApiControl.MostrarMcrRefSi(ltrEspanes.TiposDeElemento.ref.CrearClase, esAdministrador);
            }

            var activo = ObtenerPropiedad(this.RegistroEditado, ltrPropiedades.TipoDeElemento.Activo);
            var idOpcion = this.IdDeLaOpcionDeMenu(Formulario.ltrJerarquia.opcionesDeMenu.eliminar)
            ApiControl.CambiarLiteralDeIdMenu(idOpcion, !activo ? ltrMenus.BarraDeMenu.Reactivar : ltrMenus.BarraDeMenu.Eliminar);
        }

        public MapearElDtoLeido(dto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.MapearElDtoLeido(dto, modoDeAcceso);
            this.OcultarPermisos(false);
        }

        private OcultarPermisos(ocultar: boolean = true): void {
            ApiPanel.OcultarContenedorDto(this.PanelDelDto, ltrPropiedades.TipoDeElemento.IdPermisoDeAdministrador, ocultar);
            ApiPanel.OcultarContenedorDto(this.PanelDelDto, ltrPropiedades.TipoDeElemento.IdPermisoDeGestor, ocultar);
            ApiPanel.OcultarContenedorDto(this.PanelDelDto, ltrPropiedades.TipoDeElemento.IdPermisoDeConsultor, ocultar);
            if (Definido(ApiControl.BuscarControl(this.PanelDelDto, ltrPropiedades.TipoDeElemento.IdPermisoDeInterventor, false))) {
                ApiPanel.OcultarContenedorDto(this.PanelDelDto, ltrPropiedades.TipoDeElemento.IdPermisoDeInterventor, ocultar);
            }
        }

        public PrepararfiltrosParaLeerLaJerarquia(datos: Diccionario<any>): boolean {
            let hayFiltros: boolean = super.PrepararfiltrosParaLeerLaJerarquia(datos);
            if (!hayFiltros) {
                hayFiltros = datos.Elementos > 2 || datos.Obtener(ltrTiposDeElemento.propiedades.modoDeAcceso) !== ltrTiposDeElemento.propiedades.modoDeAccesoNoSeleccionado;
            }
            return hayFiltros;
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
            var referencia = ApiControl.BuscarReferencia(modalDeEdicion, ltrPropiedades.Negocio.PlantillaPorTipo.Plantilla)
            var objeto = peticion.resultado.datos;
            referencia.innerText = ltrEtiquetas.Negocio.PlantillaPorTipo.Plantilla + ': ' + ObtenerPropiedad(objeto, ltrPropiedades.Negocio.PlantillaPorTipo.Plantilla);
        }

    }

    export function Negocio_TrasAbrirModalDePlantillasPorTipo(idModal: string, modo: string) {
        var modal = document.getElementById(idModal) as HTMLDivElement;
        ApiPanel.MostrarOcultarCelda(modal, ltrPropiedades.Negocio.PlantillaPorTipo.IdPermiso, modo !== ltrEspanes.Opcion.crear);
        ApiPanel.MostrarOcultarCelda(modal, ltrPropiedades.Negocio.PlantillaPorTipo.IdAccion, modo !== ltrEspanes.Opcion.crear);
    }

    export function Negocio_DescargarPlantillaPorTipo(idArchivo: number) {
        let parametros = `negocio=${JerarquiaDeTipos.Negocio}`;
        let idModal = ApiDeExpansor.IdDeModalEditarRelacion(ltrModalDeEditarRelacion.Negocio.TipoDeElemento.Plantillas);
        let objeto = Negocio.JerarquiaDeTipos.ObjetoDeExpansor(idModal);
        parametros = `${parametros}&idPlantilla=${ObtenerPropiedad(objeto, ltrPropiedades.Negocio.PlantillaPorTipo.Id)}`;
        parametros = `${parametros}&idArchivo=${idArchivo}`;
        let descargar: string = `/${ltrControladores.Negocio.TiposDeElemento}/${Ajax.EndPoint.Negocio.TiposDeElemento.DescargarPlantilla}?${parametros}`;
        EntornoSe.AbrirPestana(descargar);
    }
}