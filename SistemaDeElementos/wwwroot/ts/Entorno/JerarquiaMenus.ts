namespace Entorno {

    export let jerarquia: Entorno.JerarquiaMenus = null;

    export function CrearFormulario(idFormulario: string, negocio: string) {
        jerarquia = new Entorno.JerarquiaMenus(idFormulario, negocio);
        window.addEventListener("load", function () { jerarquia.InicializarJerarquia(true); }, false);

        window.onbeforeunload = function () {
            jerarquia.AntesDeSalir();
        };
    }

    const ltrJerarquiaMenus = {
        propiedades: {
            idUsuario: ltrPropiedades.Entorno.Usuario.id,
            idPuesto: ltrPropiedades.Entorno.seguridad.Puesto.id,
            idRol: ltrPropiedades.Entorno.seguridad.rol.id,
            idVista: ltrPropiedades.Entorno.Vista.id,
        }
    };

    export class JerarquiaMenus extends Formulario.Jerarquia {

        constructor(idFormulario: string, negocio: string) {
            super(idFormulario, negocio);
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            super.InicializarJerarquia(blanquearFiltros);
        }

        public MapearElDtoLeido(dto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            ApiDeInicializacion.OcultarArchivos(this.PanelDelDto, false);
            super.MapearElDtoLeido(dto, modoDeAcceso);
        }

        public ComenzarModoEdicion(dto: any, modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos, nodoSeleccionado: string): void {
            super.ComenzarModoEdicion(dto, modoAcceso, nodoSeleccionado);
            ApiControl.BloquearCheckPorPropiedad(this.PanelDelDto, ltrPropiedades.Entorno.Menu.activo, false);
        }

        public CrearNodo(): void {
            super.CrearNodo();
            Registro.EliminarArbolDeMenu();
        }

        public ModificarNodo(): void {
            super.ModificarNodo();
            Registro.EliminarArbolDeMenu();
        }

        public EliminarNodo(): void {
            super.ModificarNodo();
            Registro.EliminarArbolDeMenu();
        }

        public PrepararfiltrosParaLeerLaJerarquia(datos: Diccionario<any>): boolean {
            let hayFiltros: boolean = super.PrepararfiltrosParaLeerLaJerarquia(datos);
            if (!hayFiltros) {
                hayFiltros = datos.Obtener(ltrJerarquiaMenus.propiedades.idUsuario) > 0
                || datos.Obtener(ltrJerarquiaMenus.propiedades.idPuesto) > 0
                || datos.Obtener(ltrJerarquiaMenus.propiedades.idRol) > 0
                || datos.Obtener(ltrJerarquiaMenus.propiedades.idVista) > 0;
            }
            return hayFiltros;
        }

    }

}