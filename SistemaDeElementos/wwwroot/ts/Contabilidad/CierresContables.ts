namespace Contabilidad {

    export function CrearCrudDeCierresContables(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Contabilidad.CrudDeCierresContables(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    // Navega a la sociedad cerrada desde la columna del grid
    export function CierreContable_IrALaSociedad(numeroDeFila: number) {
        let idSociedad = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Contabilidad.CierreContable.IdSociedad));
        EntornoSe.AbrirPestana(`${window.location.origin}/${ltrUrls.Terceros.Sociedades}?${ltrParametrosUrl.id}=${idSociedad}`);
    }

    // Navega al lote contable del cierre desde la columna del grid
    export function CierreContable_IrAlLote(numeroDeFila: number) {
        let idLote = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Contabilidad.CierreContable.IdLote));
        EntornoSe.AbrirPestana(`${window.location.origin}/${ltrUrls.SistemaDocumental.LotesContables}?${ltrParametrosUrl.id}=${idLote}`);
    }

    const digitosDeNcs: number = 11;
    const digitosPorDefectoDeContasol: number = 8;

    // Sistema contable, dígitos y fecha del último cierre de la sociedad seleccionada en la modal (los devuelve el gestor de sociedades al leerla para un cierre)
    let sistemaDeLaSociedad: string = null;
    let digitosDeLaSociedad: number = 0;

    function ModalCrearCierre(): HTMLDivElement {
        return (Crud.crudMnt as CrudDeCierresContables).ModalCrearCierre;
    }

    function Hoy(): Date {
        let ahora = new Date();
        return new Date(ahora.getFullYear(), ahora.getMonth(), ahora.getDate());
    }

    function PrimeroDeEnero(): Date {
        return new Date(new Date().getFullYear(), 0, 1);
    }

    // Fecha (sin hora) de un selector de fecha, cuyo valor es 'aaaa-mm-dd'; null si está vacío
    function LeerFecha(modal: HTMLDivElement, propiedad: string): Date {
        let valor = ApiControl.BuscarFecha(modal, propiedad)?.value;
        if (IsNullOrEmpty(valor))
            return null;
        let partes = valor.split('-');
        return new Date(Numero(partes[0]), Numero(partes[1]) - 1, Numero(partes[2]));
    }

    function FechaComoTexto(fecha: Date): string {
        return `${PadLeft(fecha.getDate().toString(), "00")}/${PadLeft((fecha.getMonth() + 1).toString(), "00")}/${fecha.getFullYear()}`;
    }

    function FechaComoValor(fecha: Date): string {
        return `${fecha.getFullYear()}-${PadLeft((fecha.getMonth() + 1).toString(), "00")}-${PadLeft(fecha.getDate().toString(), "00")}`;
    }

    // Último cierre bloqueado y cerrar hasta por defecto hoy, limitado entre el día siguiente al último cierre y hoy
    function AsignarFechas(modal: HTMLDivElement, ultimoCierre: Date) {
        let hasta = ApiControl.BuscarFecha(modal, ltrPropiedades.Contabilidad.CrearCierre.CerrarHasta);
        ApiControl.AsignarFecha(modal, ltrPropiedades.Contabilidad.CrearCierre.UltimoCierre, ultimoCierre);
        ApiControl.AsignarFecha(modal, ltrPropiedades.Contabilidad.CrearCierre.CerrarHasta, Hoy());

        let diaSiguiente = new Date(ultimoCierre.getFullYear(), ultimoCierre.getMonth(), ultimoCierre.getDate() + 1);
        hasta.min = FechaComoValor(diaSiguiente);
        hasta.max = FechaComoValor(Hoy());
    }

    // Cerrar hasta ha de ser posterior al último cierre y no posterior a hoy; devuelve el motivo si no se cumple
    function MotivoFechaDeCierreNoValida(modal: HTMLDivElement): string {
        let hasta = LeerFecha(modal, ltrPropiedades.Contabilidad.CrearCierre.CerrarHasta);
        if (!Definido(hasta))
            return 'Ha de indicar la fecha hasta la que se cierra la sociedad';

        if (hasta > Hoy())
            return `No se puede cerrar hasta el '${FechaComoTexto(hasta)}' por ser posterior al día de hoy`;

        let ultimoCierre = LeerFecha(modal, ltrPropiedades.Contabilidad.CrearCierre.UltimoCierre) ?? PrimeroDeEnero();
        if (hasta <= ultimoCierre)
            return `No se puede cerrar hasta el '${FechaComoTexto(hasta)}', ha de ser posterior a la fecha del último cierre '${FechaComoTexto(ultimoCierre)}'`;

        return null;
    }

    // Al seleccionar la sociedad: se pone su contabilidad si la tiene configurada (si no, queda "Seleccionar..." para elegirla),
    // se ajustan los dígitos y se muestran la fecha de su último cierre y hasta dónde se cierra (hoy)
    export function CierreContable_TrasSeleccionarSociedad() {
        let modal = ModalCrearCierre();
        let lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Contabilidad.CrearCierre.Sociedad);
        let idSociedad = Tipos.ListaDinamica.Obtener(lista)?.IdSeleccionado ?? 0;
        if (idSociedad <= 0)
            return;

        let sociedad = OpcionesDeLasListas.ObtenerObjeto(lista);
        sistemaDeLaSociedad = Definido(sociedad) ? ObtenerPropiedad(sociedad, ltrPropiedades.Contabilidad.CrearCierre.SistemaContable, null) : null;
        digitosDeLaSociedad = Definido(sociedad) ? Numero(ObtenerPropiedad(sociedad, ltrPropiedades.Contabilidad.CrearCierre.Digitos, 0)) : 0;
        let fechaUltimoCierre = Definido(sociedad) ? ObtenerPropiedad(sociedad, ltrPropiedades.Contabilidad.CrearCierre.FechaUltimoCierre, null) : null;

        let contabilidad = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Contabilidad.CrearCierre.Contabilidad);
        contabilidad.value = IsNullOrEmpty(sistemaDeLaSociedad) ? ltrPropiedades.Contabilidad.CrearCierre.SinSeleccionar : sistemaDeLaSociedad;
        AjustarDigitos(modal);

        let ultimoCierre = Definido(fechaUltimoCierre) ? new Date(fechaUltimoCierre) : PrimeroDeEnero();
        AsignarFechas(modal, new Date(ultimoCierre.getFullYear(), ultimoCierre.getMonth(), ultimoCierre.getDate()));
    }

    // Sin sociedad la contabilidad vuelve a "Seleccionar...", los dígitos se vacían y las fechas vuelven a las de por defecto
    export function CierreContable_TrasBlanquearSociedad() {
        let modal = ModalCrearCierre();
        BlanquearContabilidad(modal);
        AsignarFechas(modal, PrimeroDeEnero());
    }

    export function CierreContable_TrasCambiarContabilidad() {
        AjustarDigitos(ModalCrearCierre());
    }

    function BlanquearContabilidad(modal: HTMLDivElement) {
        sistemaDeLaSociedad = null;
        digitosDeLaSociedad = 0;
        let contabilidad = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Contabilidad.CrearCierre.Contabilidad);
        contabilidad.value = ltrPropiedades.Contabilidad.CrearCierre.SinSeleccionar;
        AjustarDigitos(modal);
    }

    // Ncs: 11 dígitos y bloqueado; ContaSol: editable con los de la sociedad (8 si no los tiene); sin contabilidad: vacío y bloqueado
    function AjustarDigitos(modal: HTMLDivElement) {
        let contabilidad = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Contabilidad.CrearCierre.Contabilidad).value;
        let digitos = ApiControl.BuscarEditor(modal, ltrPropiedades.Contabilidad.CrearCierre.Digitos);

        if (contabilidad === ltrPropiedades.Contabilidad.CrearCierre.Ncs) {
            digitos.value = digitosDeNcs.toString();
            digitos.readOnly = true;
        }
        else if (contabilidad === ltrPropiedades.Contabilidad.CrearCierre.ContaSol) {
            let propuestos = sistemaDeLaSociedad === ltrPropiedades.Contabilidad.CrearCierre.ContaSol && digitosDeLaSociedad > 0
                ? digitosDeLaSociedad
                : digitosPorDefectoDeContasol;
            digitos.value = propuestos.toString();
            digitos.readOnly = false;
        }
        else {
            digitos.value = '';
            digitos.readOnly = true;
        }
    }

    export class CrudDeCierresContables extends Crud.CrudMnt {

        public get ModalCrearCierre(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Contabilidad.CierresContables.CrearCierre) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionCierreContable(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionCierreContable(this, idPanelEdicion);
        }

        // Las opciones de cierre no pasan por el servidor como opción de menú: abren el modal o llaman directamente a su endpoint
        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.Contabilidad.CierresContables.CrearCierre) {
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(this.ModalCrearCierre.id, 0);
                return;
            }

            if (opcion === ltrMenus.eventosDeMf.Contabilidad.CierresContables.EliminarCierre) {
                this.EliminarCierre();
                return;
            }

            super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        // Eliminar cierre solo se activa si el seleccionado es el último cierre realizado de su sociedad
        public AplicarModoAccesoAlElemento(elemento: Elemento): void {
            super.AplicarModoAccesoAlElemento(elemento);
            this.AjustarEliminarCierre(elemento.Registro);
        }

        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax) {
            super.DespuesDeLeerFilaSeleccionada(peticion);
            this.AjustarEliminarCierre(peticion.resultado.datos);
        }

        protected QuitarDelSelector(grid: Crud.GridDeDatos, id: number): void {
            super.QuitarDelSelector(grid, id);
            if (this.InfoSelector.Cantidad === 0)
                this.AjustarEliminarCierre(null);
        }

        private AjustarEliminarCierre(registro: any) {
            if (!Definido(this.ContenedorMenuIndividual))
                return;

            let esElUltimo: boolean = EsTrue(ObtenerPropiedad(registro, ltrPropiedades.Contabilidad.CierreContable.EsElUltimoCierre, false));
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenuIndividual, ltrMenus.eventosDeMf.Contabilidad.CierresContables.EliminarCierre, ltrMenus.enumOrigen.crud,
                this.InfoSelector.Cantidad === 1 && esElUltimo);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalCrearCierre.id) {
                BlanquearContabilidad(modal);
                AsignarFechas(modal, PrimeroDeEnero());
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id !== this.ModalCrearCierre.id) {
                super.ModalDePedirDatos_Aceptar(modal);
                return;
            }

            // la misma regla se vuelve a validar en el servidor al someter y al iniciar el trabajo
            let fechaNoValida = MotivoFechaDeCierreNoValida(modal);
            if (Definido(fechaNoValida)) {
                MensajesSe.Error('Realizar cierre', fechaNoValida);
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Contabilidad.CierresContables.CrearCierre, parametros, new Array<Parametro>())
                .then((peticion) => {
                    super.ModalDePedirDatos_Cerrar(modal);
                    MensajesSe.Info(peticion.resultado.mensaje);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private EliminarCierre() {
            if (this.InfoSelector.Cantidad !== 1) {
                MensajesSe.Info("Seleccione el cierre a eliminar");
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(literal.id, this.InfoSelector.IdsSeleccionados[0]));
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Contabilidad.CierresContables.EliminarCierre, parametros, new Array<Parametro>())
                .then((peticion) => MensajesSe.Info(peticion.resultado.mensaje))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
    }

    export class CrudCreacionCierreContable extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export class CrudEdicionCierreContable extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

    }
}
