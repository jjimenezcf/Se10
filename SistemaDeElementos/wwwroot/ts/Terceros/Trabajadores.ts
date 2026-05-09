namespace Terceros {

    export let crudDeTrabajador: Terceros.CrudDeTrabajadores = null;

    export function CrearCrudDeTrabajadores(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeTrabajadores(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeTrabajador = Crud.crudMnt as Terceros.CrudDeTrabajadores;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeTrabajadores extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionTrabajador(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionTrabajador(this, idPanelEdicion);
        }

    }

    export class CrudCreacionTrabajador extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public MapearDatosDeSociedad(cg: any) {
            ApiDePeticiones.LeerElementoPorId(this.PanelDeCrear, ltrControladores.Terceros.Sociedades, ObtenerPropiedad(cg, ltrPropiedades.Terceros.Cg.idSociedad), new Array<Parametro>(), cg)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.mapearDatosSociedad(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        public ProponerCuentaContable() {
            let filtros = new Array<ClausulaDeFiltrado>();
            filtros.push(new ClausulaDeFiltrado(literal.filtro.PorVariable, literal.filtro.criterio.igual, ltrVariables.Cuentas.Sueldos))
            ApiDePeticiones.LeerElemento(this.PanelDeCrear, ltrControladores.Contabilidad.Cuentas, filtros, Array<Parametro>())
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.mapearCuentaContable(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        private mapearDatosSociedad(peticion: ApiDeAjax.DescriptorAjax): void {
            let panel = peticion.llamador as HTMLDivElement;
            let telefono = ApiControl.BuscarEditor(panel, ltrPropiedades.Terceros.Trabajador.Telefono);
            telefono.value = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.Telefono);
        }

        private mapearCuentaContable(peticion: ApiDeAjax.DescriptorAjax): void {
            let panel = peticion.llamador as HTMLDivElement;
            let cuenta = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Terceros.Trabajador.Cuenta);
            let idCuenta = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Maestros.Contabilidad.Cuenta.Id, 0);
            if (idCuenta > 0)
                MapearAlControl.FijarEnListaDeElementos(cuenta, idCuenta);
        }
    }

    export class CrudEdicionTrabajador extends Crud.CrudEdicion {

        public get ModalDeCreacionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Trabajador.CuentasBancarias);
        }
        public get ModalDeEdicionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Trabajador.CuentasBancarias);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public AplicarCuentaNoActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.none;
            ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Trabajador.AliasDeCuenta);
        }

        public AplicarCuentaActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.block;
            ApiControl.DesbloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Trabajador.AliasDeCuenta);
        }


    }

    export function Trb_ProponerDatosDelUsuarioSeleccionado(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        //let opcion: HTMLOptionElement = ApiListaDinamica.BuscarOpcion(lista, lista.value);
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            //var objeto = OpcionesDeLasListas.ObtenerObjeto(lista); // JSON.parse(opcion.getAttribute(atControl.objeto));
            if (Crud.crudMnt.EstoyCreando) {
                let email = ApiControl.BuscarEditor(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Terceros.Trabajador.eMail);
                email.value = ObtenerPropiedad(objeto, ltrPropiedades.Entorno.Usuario.eMail);
            }

            if (Crud.crudMnt.EstoyEditando) {
                let email = ApiControl.BuscarEditor(Crud.crudMnt.crudDeEdicion.PanelDeEditar, ltrPropiedades.Terceros.Trabajador.eMail);
                email.value = ObtenerPropiedad(objeto, ltrPropiedades.Entorno.Usuario.eMail);
            }
        }
    }

    export function Trb_ProponerDatosDelCg(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            if (crudDeTrabajador.EstoyCreando) {
                ((Crud.crudMnt as CrudDeTrabajadores).crudDeCreacion as CrudCreacionTrabajador).MapearDatosDeSociedad(objeto);
            }
        }
    }

    export function Trb_TrasCargarCuentasContables() {
        if (Crud.crudMnt.EstoyCreando) 
            ((Crud.crudMnt as CrudDeTrabajadores).crudDeCreacion as CrudCreacionTrabajador).ProponerCuentaContable();
    }

    export function Trb_RecargarGridDeArchivos() {
        ApiDeArchivos.MostrarArchivosAnexados(crudDeTrabajador.crudDeEdicion.PanelDeArchivos.id, crudDeTrabajador.NombreDeNegocio, crudDeTrabajador.crudDeEdicion.Registro.id);
        crudDeTrabajador.crudDeEdicion.RecargarGridDeTrazas();
    }

    export function Trb_AlPegar_Iban(event) {
        let modal: HTMLDivElement = (crudDeTrabajador.crudDeEdicion as CrudEdicionTrabajador).ModalDeCreacionDeCuentasBancarias;
        let cuentaBancaria: string = ValidarIbanDelPortaPapeles(event);
        MapearCuentaBancaria(modal, cuentaBancaria);
    };

    export function Trb_AlCambiar_CuentaActiva(check: HTMLInputElement) {
        let modal = (crudDeTrabajador.crudDeEdicion as CrudEdicionTrabajador).ModalDeEdicionDeCuentasBancarias;
        if (!check.checked) (crudDeTrabajador.crudDeEdicion as CrudEdicionTrabajador).AplicarCuentaNoActiva(modal);
        else (crudDeTrabajador.crudDeEdicion as CrudEdicionTrabajador).AplicarCuentaActiva(modal);
    }
}