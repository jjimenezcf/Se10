namespace Venta {

    export function Ptr_InicializarModalParaCrearLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tabla: HTMLDivElement = editor.TablaDeLineas;
        let valor: number = 0;
        let tablarows = tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (tablarows.length > 1) {
            let ultimoOrden = tablarows[tablarows.length - 1].querySelector(`input[propiedad=${ltrPropiedades.Venta.ParteTr.linea.orden}]`) as HTMLInputElement;
            valor = Numero(ultimoOrden.value);
        }
        let orden = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.ParteTr.linea.orden, true) as HTMLInputElement;
        orden.value = (valor + 10).toString();

        let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.ParteTr.linea.tipoDeLinea, true) as HTMLSelectElement;
        let unitario = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.ParteTr.linea.unitario, true) as HTMLInputElement;
        unitario.focus();
        ptr_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea(tipoDeLinea.selectedIndex);
    }

    export function Ptr_InicializarModalParaEditarLineas() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr);
        let modal: HTMLDivElement = editor.ModalDeEdicionDeLineas;
        let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.ParteTr.linea.tipoDeLinea, true) as HTMLSelectElement;
        editor.AplicarTipoDeLinea(tipoDeLinea.selectedIndex);
    }

    export function Ptr_AjustarControlesDeEdicion(panelContenedor: HTMLDivElement, registro: any) {

    }

    export function Ptr_Tras_Blanquear_Unitario() {
        Ptr_Tras_Cambiar_TipoDeLinea();
    }

    export function Ptr_Tras_Cambiar_TipoDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr);
        let modal: HTMLDivElement = editor.ModalDeCreacionDeLineas;
        let tipoDeLinea = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.ParteTr.linea.tipoDeLinea, true) as HTMLSelectElement;
        ptr_InicializarModalDeLineas_interno();
        editor.AplicarTipoDeLinea(tipoDeLinea.selectedIndex);
    }

    export function Ptr_Tras_Seleccionar_Unitario(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (Definido(objeto)) {
            ptr_mapearUnitarioSeleccionado_interno(objeto);
        }
        else {
            var tieneValorAsignado = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0 && !IsNullOrEmpty(lista.value);
            if (!tieneValorAsignado)
                ptr_InicializarModalDeLineas_interno();
        }
    }

    export function Ptr_CalcularImportesDeLinea() {
        var editor = (Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr);
        let modal: HTMLDivElement = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        editor.ptr_CalcularImportesDeLinea_interno(modal);
    }

    export function Ptr_Tras_Mapear_Filtro_IdContrato(control: HTMLElement) {
        let idContrato = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idContrato == 0)
            return;
        ApiDePeticiones.LeerElementoPorId(control, ltrControladores.Juridico.Contratos, idContrato, new Array<Parametro>(), idContrato)
            .then((peticion) => mapearDatosContrato(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function Ptr_Tras_Mapear_Filtro_IdTarea(control: HTMLElement) {
        let idTarea = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idTarea == 0)
            return;
        ApiDePeticiones.LeerElementoPorId(control, ltrControladores.Administracion.Tareas, idTarea, new Array<Parametro>(), idTarea)
            .then((peticion) => mapearDatosTarea(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function Ptr_IvaRepercutidoCambiado() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr;
        let selectorIvaR = editor.SelectorDeIvaActivo;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaR);
        let iva = editor.PorcentageIvaActivo;
        if (Definido(objeto))
            AsignarValor(iva, ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje));
        else
            ApiControl.BlanquearEditor(iva);
    }

    export function Ptr_CalculaDuracionDeAsignacionPtr(control: HTMLInputElement) {
        let contenedorDto: HTMLDivElement = ApiPanel.BuscarContenedorDeTablaDto(control);
        if (control.checked) {
            ApiControl.BloquearEditorPorPropiedad(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.Duracion);
            ApiControl.BloquearListaDeValores(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.MedidoEn);
        }
        else {
            ApiControl.DesbloquearEditorPorPropiedad(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.Duracion);
            ApiControl.DesbloquearListaDeValores(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.MedidoEn);
        }
    }

    export function Ptr_CopiarFechasDeAsignacionPtr(control: HTMLInputElement) {
        let contenedorDto: HTMLDivElement = ApiPanel.BuscarContenedorDeTablaDto(control);
        if (control.checked) {
            ApiPanel.CopiarPorPropiedad(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.PlfDeInicio, ltrPropiedades.Venta.ParteTr.Asignacion.Iniciada, true);
            ApiPanel.CopiarPorPropiedad(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.PlfDeFin, ltrPropiedades.Venta.ParteTr.Asignacion.Finalizada, true);
        }
        else {
            ApiControl.DesbloquearSelectorDeFechaHoraPorPropiedad(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.Iniciada);
            ApiControl.DesbloquearSelectorDeFechaHoraPorPropiedad(contenedorDto, ltrPropiedades.Venta.ParteTr.Asignacion.Finalizada);
        }
    }

    export function AsiPtr_Tras_Mapear_Filtro_IdContrato(control: HTMLElement) {
        let idContrato = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idContrato == 0)
            return;
        //todo: mapear CG del contrato al filtro en la ventana de asignaciones
    }

    export function AsiPtr_Tras_Mapear_Filtro_IdPresupuesto(control: HTMLElement) {
        let idPresupuesto = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idPresupuesto == 0)
            return;
        //todo: mapear CG del contrato al filtro en la ventana de asignaciones
    }

    function mapearDatosContrato(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCg = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.IdCg));
        let cg = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.Cg);
        let idFiltro = Crud.crudMnt.ModalDeFiltrado(ltrModalDeFiltrado.Venta.ParteTr.FiltroDePartesTr);
        let panel = document.getElementById(idFiltro) as HTMLDivElement;
        MapearAlPanel.RestrictoresPorPropiedad(Crud.crudMnt.PanelFiltro, ltrPropiedades.Elemento.ConCg.IdCg, idCg, cg);
        ApiControl.BloquearListaDeValores(panel, ltrPropiedades.Venta.ParteTr.ConOSinContrato);

        ApiDelCrud.MapearDatosSocietariosYDepartamentales(Crud.crudMnt.crudDeCreacion.PanelDeCrear, peticion.resultado.datos);

        let idTipoContrato = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConTipo.IdTipo));
        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Elemento.ConTipo.IdTipo, idTipoContrato));
        let idContrato = Numero(ObtenerPropiedad(peticion.resultado.datos, literal.id));
        ApiDePeticiones.LeerAmpliacion(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrControladores.Juridico.DatosDelContrato, ltrNegocioSe.Enumerado.Juridico.Contrato, idContrato, parametros, peticion.resultado.datos)
            .then((peticion) => mapearDatosCliente(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function mapearDatosTarea(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCg = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.IdCg));
        let cg = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.Cg);
        let idFiltro = Crud.crudMnt.ModalDeFiltrado(ltrModalDeFiltrado.Venta.ParteTr.FiltroDePartesTr);
        let panel = document.getElementById(idFiltro) as HTMLDivElement;
        MapearAlPanel.RestrictoresPorPropiedad(Crud.crudMnt.PanelFiltro, ltrPropiedades.Elemento.ConCg.IdCg, idCg, cg);
        ApiControl.BloquearListaDeValores(panel, ltrPropiedades.Venta.ParteTr.ConOSinTarea);

        ApiDelCrud.MapearDatosSocietariosYDepartamentales(Crud.crudMnt.crudDeCreacion.PanelDeCrear, peticion.resultado.datos);
    }

    function mapearDatosCliente(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCliente = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.ParteTr.IdCliente));
        let cliente = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.ParteTr.Cliente);
        MapearAlControl.Propiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.ParteTr.Cliente, idCliente, cliente, true, false);
    }

    function ptr_mapearUnitarioSeleccionado_interno(unitario: any) {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.ParteTr.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.ParteTr.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.ParteTr.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.ParteTr.linea.unidad) as HTMLSelectElement;
        if (NoDefinido(unitario)) {
            precio.value = "";
            clase.selectedIndex = 0;
            naturaleza.selectedIndex = 0;
            unidad.selectedIndex = 0;
            return;
        }
        AsignarValor(precio, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.venta, 0));
        let claseDelUnitario = ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.clase, 0);
        MapearAlControl.ListaDeValores((clase as HTMLSelectElement), claseDelUnitario);
        MapearAlControl.FijarEnListaDeElementos(naturaleza, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idnaturaleza, 0));
        MapearAlControl.FijarEnListaDeElementos(unidad, ObtenerPropiedad(unitario, ltrPropiedades.Maestros.unitario.idunidad, 0));
    }


    function ptr_InicializarModalDeLineas_interno() {
        var editor = Crud.crudMnt.crudDeEdicion as CrudEdicionParteTr;
        var panel = editor.EstaCreandoUnaLinea ? editor.ModalDeCreacionDeLineas : editor.ModalDeEdicionDeLineas;
        let precio = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.ParteTr.linea.precio) as HTMLInputElement;
        let clase = ApiControl.BuscarListaDeValores(panel, ltrPropiedades.Venta.ParteTr.linea.clase) as HTMLSelectElement;
        let naturaleza = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.ParteTr.linea.naturaleza) as HTMLSelectElement;
        let unidad = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.ParteTr.linea.unidad) as HTMLSelectElement;
        let selectorIvaR = ApiControl.BuscarListaDeElementos(panel, ltrPropiedades.Venta.ParteTr.linea.selectorDeIvaR) as HTMLSelectElement;
        let cantidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Venta.ParteTr.linea.cantidad);
        let unitario = ApiControl.BuscarControl(panel, ltrPropiedades.Venta.ParteTr.linea.unitario, true) as HTMLInputElement;
        ApiListaDinamica.Blanquear(unitario);

        cantidad.value = "";
        selectorIvaR.selectedIndex = 0;
        precio.value = "";
        clase.selectedIndex = 0;
        naturaleza.selectedIndex = 0;
        unidad.selectedIndex = 0;
    }
}

