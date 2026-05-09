namespace Juridico {

    export function CrearCrudDeLotes(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Juridico.CrudDeLotes(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeLotes extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionLote(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionLote(this, idPanelEdicion);
        }

    }

    export class CrudCreacionLote extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
        
        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeLeerDatosParaInicializarCreacion(peticion);
            let contrato: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Juridico.lote.idContrato, ltrTipoControl.restrictorDeEdicion);
            let idContrato: number = Numero(contrato.getAttribute(atRestrictor.idRestrictor));
            if (idContrato == 0) {
                MensajesSe.Error("ComenzarCreacion", "Debe indicar el identificador del contrato");
                return;
            }
            var ampliaciones = new Array<string>();
            ampliaciones.push(ltrAmpliaciones.contratos.DatosDelContratoDtm);
            let parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.AmpliacionesSolicitadas, JSON.stringify(ampliaciones)));
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Juridico.Contratos, idContrato, parametros, idContrato)
                .then((peticion) => this.MapearDatosContrato(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private MapearDatosContrato(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionLote = peticion.llamador as CrudCreacionLote;
            let inicio: Date = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.Contrato.DatosDeVenta.InicioContrato, 0, true);
            let fin: Date = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.Contrato.DatosDeVenta.FinContrato, undefined, true);
            ApiControl.AsignarFecha(creador.PanelDeCrear, ltrPropiedades.Juridico.lote.vigenteDesde, inicio);
            ApiControl.AsignarFecha(creador.PanelDeCrear, ltrPropiedades.Juridico.lote.vigenteHasta, fin);
            ApiControl.BuscarEditor(creador.PanelDeCrear, ltrPropiedades.Elemento.Nombre).focus();
        }
    }

    export class CrudEdicionLote extends Crud.CrudEdicion {
        private idGridDeLineas = 'unitarios';
        private idModalDeCreacionDeLinea: string = 'unitarios';

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public get ModalDeCreacionDeLineas(): HTMLDivElement {
            return this.ModalParaCrearRelacion(this.idModalDeCreacionDeLinea);
        }

        public get ModalDeEdicionDeLineas(): HTMLDivElement {
            return this.ModalParaEditarRelacion(this.idGridDeLineas);
        }

        public get EstaCreandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalDeCreacionDeLineas);
        }

        public get EstaEditandoUnaLinea(): boolean {
            return ApiPanel.ModalAbierta(this.ModalParaEditarRelacion(this.idGridDeLineas));
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return true;
            }
            return false;
        }

    }

    export function Lot_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        //let opcion: HTMLOptionElement = ApiListaDinamica.BuscarOpcion(lista, lista.value);
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            //var objeto = OpcionesDeLasListas.ObtenerObjeto(lista); // JSON.parse(opcion.getAttribute(atControl.objeto));
            lot_mapearUnitarioSeleccionado_interno(objeto);
        }
        else {
            var tieneValorAsignado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0 && !IsNullOrEmpty(lista.value);
            if (!tieneValorAsignado)
                lot_InicializarModalDeLineas_interno();
        }
    }

    export function Lot_Tras_Blanquear_Unitario() {
        lot_InicializarModalDeLineas_interno();
    }

    function lot_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionLote;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let coste = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.lote.unitarios.coste) as HTMLInputElement;
        let venta = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.lote.unitarios.venta) as HTMLInputElement;
        if (NoDefinido(unitario)) {
            venta.value = "0";
            coste.value = "0";
            return;
        }
        AsignarValor(venta, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.venta, 0));
        AsignarValor(coste, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.coste, 0));
    }


    function lot_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionLote;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let coste = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.lote.unitarios.coste) as HTMLInputElement;
        let venta = ApiControl.BuscarEditor(panel, ltrPropiedades.Juridico.lote.unitarios.venta) as HTMLInputElement;
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Juridico.lote.unitarios.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);
        venta.value = "0";
        coste.value = "0";
    }
}

