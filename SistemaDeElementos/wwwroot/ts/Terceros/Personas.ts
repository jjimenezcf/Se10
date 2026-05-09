namespace Terceros {

    export let crudDePersona: Terceros.CrudDePersonas = null;

    export function CrearCrudDePersonas(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDePersonas(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDePersona = Crud.crudMnt as Terceros.CrudDePersonas;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePersonas extends Crud.CrudMnt {
        private _TercerosJudiciales: boolean = undefined;

        public get TercerosJudiciales(): boolean {
            return this._TercerosJudiciales;
        }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPersona(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPersona(this, idPanelEdicion);
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._TercerosJudiciales = mapIndicadores.get(ltrPropiedades.Terceros.Persona.Indicadores.TercerosJudiciales);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            return Terceros.MostrarInterlocutores(peticion, ltrParametrosUrl.idPersona);
        }
    }

    export class CrudCreacionPersona extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            if (!EsTrue((this.CrudDeMnt as CrudDePersonas).TercerosJudiciales)) {
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearProcurador);
            }

            ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Correspondencia);
        }
        
        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            if (this.CrudDeMnt.Estado.Obtener(ltrClaveDeEstado.paginaOrigen) == ltrPaginas.Terceros.Interlocutor) {
                MapearAlControl.Check(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearInterlocutor, true, true);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearProcurador);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearProveedor);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearCliente);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearTrabajador);
                ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Contacto);
            }

            if (this.CrudDeMnt.Estado.Obtener(ltrClaveDeEstado.paginaOrigen) == ltrPaginas.Terceros.Cliente) {
                MapearAlControl.Check(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearCliente, true, true);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearProcurador);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Persona.CrearProveedor);
                ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Fiscal);
            }
        }

        protected DespuesDeCrear(peticion: ApiDeAjax.DescriptorAjax) {
            let crudCreador: CrudCreacionPersona = peticion.llamador as CrudCreacionPersona;
            if (!crudCreador.SeguirCreando) {
                var idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Persona.IdCliente));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Cliente, ltrUrls.Terceros.Cliente, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Persona.IdProveedor));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Proveedor, ltrUrls.Terceros.Proveedor, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Persona.IdAbogado));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Abogado, ltrUrls.Terceros.Abogado, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Persona.IdProcurador));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Procurador, ltrUrls.Terceros.Procurador, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Persona.IdInterlocutor));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Interlocutor, ltrUrls.Terceros.Interlocutor, idTercero);
                }
            }

            super.DespuesDeCrear(peticion);
        }

    }

    export class CrudEdicionPersona extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            return Terceros.MostrarInterlocutores(peticion, ltrParametrosUrl.idPersona);
        }
    }

    export function Persona_CrearProcurador_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Persona.CrearProcurador);
    }

    export function Persona_CrearAbogado_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Persona.CrearAbogado);
    }

    export function Persona_CrearProveedor_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Persona.CrearProveedor);
    }

    export function Persona_CrearCliente_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Persona.CrearCliente);
    }
    export function Persona_CrearTrabajador_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Persona.CrearTrabajador);
    }

    let marcado: number = 0;

    function SincronizarCheck(check: HTMLInputElement, propiedad: string) {
        let panel: HTMLDivElement = (document.getElementsByClassName(ltrCss.contenedorEdicionCuerpo) as HTMLCollectionOf<HTMLDivElement>)[0];
        let checkDeInter = ApiControl.BuscarCheck(panel, ltrPropiedades.Terceros.Persona.CrearInterlocutor);
        if (check.checked) {
            checkDeInter.checked = true;
            checkDeInter.disabled = true;
            marcado = marcado + 1;
        }
        else {
            let checkDeEntidad = ApiControl.BuscarCheck(panel, propiedad);
            marcado = marcado - 1;
            if (!checkDeEntidad.checked && marcado === 0)
                checkDeInter.disabled = false;
        }
    }

    export function Persona_CrearInterlocutor_Change(check: HTMLInputElement) {
    }


}