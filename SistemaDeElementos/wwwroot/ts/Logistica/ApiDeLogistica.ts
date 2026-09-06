namespace Logistica {

    export function Ral_Tras_Seleccionar_Almacen(): void {
        if (Crud.ModoTrabajo() !== enumModoTrabajo.creando) return;

        const panel = Crud.crudMnt.crudDeCreacion.PanelDeCrear;
        const almacenLista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Logistica.Regularizacion.Almacen) as HTMLInputElement;
        const almacen = OpcionesDeLasListas.ObtenerObjeto(almacenLista);
        if (Definido(almacen)) {
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(panel, almacen);
        }
    }

    export function Ral_Tras_Blanquear_Almacen(): void {
        if (Crud.ModoTrabajo() !== enumModoTrabajo.creando)
            return;
        ApiDelCrud.BlanquearCgOculto(Crud.crudMnt.crudDeCreacion.PanelDeCrear);
    }

    export function Ral_InicializarModalParaCrearLineas(incremento: number) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionRegularizacion);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.GridDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Logistica.Regularizacion.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Regularizacion.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + incremento).toString();

        let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Logistica.Regularizacion.linea.unitario, true) as HTMLInputElement;
        unitario.focus();

        ApiControl.BuscarEtiqueta(modal, 'idelemento').innerText = 'Almacén';
        ApiControl.BuscarEtiqueta(modal, ltrPropiedades.Logistica.Regularizacion.linea.unitario).innerText = 'Material';

        const clase = ObtenerPropiedad(editor.Tipo, ltrPropiedades.Logistica.Regularizacion.tipo.Clase);
        if (clase === ltrValores.Logistica.Regularizacion.Clase.Inicial) {
            const cantidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.cantidad);
            cantidad.placeholder = 'Cantidad inicial';
        }
    }

    export function Ral_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionRegularizacion);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let cantidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.cantidad);
        let precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.precio);
        let total = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.total);
        AsignarValor(total, Numero(cantidad.value) * Numero(precio.value));
    }

    export function Ral_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        ral_mapearUnitarioSeleccionado_interno(objeto);
    }

    export function Ral_Tras_Blanquear_Unitario() {
        ral_mapearUnitarioSeleccionado_interno(undefined);
    }

    function ral_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionRegularizacion);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(modal, ltrPropiedades.Logistica.Regularizacion.linea.precio);
        if (NoDefinido(unitario)) {
            precio.value = "";
        }
        else {
            AsignarValor(precio, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.coste, 0));
        }
        Ral_CalcularImportesDeLinea();
    }

    export function Ral_FiltrosPorClaseDeUnitario(lista: HTMLInputElement): Array<ClausulaDeFiltrado> {
        let clausulas: Array<ClausulaDeFiltrado> = new Array<ClausulaDeFiltrado>();
        clausulas.push(new ClausulaDeFiltrado(ltrPropiedades.Maestros.unitario.FiltrosPorClaseDeUnitario, atCriterio.igual, ltrValores.Maestros.Unitario.Clase.Material));
        return clausulas;
    }
}
