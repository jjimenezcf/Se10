namespace Venta {

    export function Plv_AjustarControlesDeEdicion(panelContenedor: HTMLDivElement, registro: any) {
        if (Numero(ObtenerPropiedad(registro, ltrPropiedades.Venta.PlfDeVenta.IdTipoDeFactura, 0)) > 0) {
            let lista = ApiControl.BuscarListaDinamicaPorPropiedad(panelContenedor, ltrPropiedades.Venta.PlfDeVenta.TipoDeParte);
            let panel = ApiPanel.ContenedorDe(lista.id);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Venta.PlfDeVenta.TipoDeParte);
        }
        else {
            let lista = ApiControl.BuscarListaDinamicaPorPropiedad(panelContenedor, ltrPropiedades.Venta.PlfDeVenta.TipoDeFactura);
            let panel = ApiPanel.ContenedorDe(lista.id);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Venta.PlfDeVenta.TipoDeFactura);
        }
    }

    export function Plv_Tras_Seleccionar_TipoDeFactura(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        if (Tipos.ListaDinamica.Obtener(lista).IdSeleccionado > 0) {
            let panel = ApiPanel.ContenedorDe(idLista);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Venta.PlfDeVenta.TipoDeParte);
        }
        else
            Plv_Tras_Blanquear_TipoDeFactura(idLista);

    }
    export function Plv_Tras_Blanquear_TipoDeFactura(idLista: string) {
        let panel = ApiPanel.ContenedorDe(idLista);
        ApiControl.DesbloquearListaDinamicaPorPropiedad(panel, ltrPropiedades.Venta.PlfDeVenta.TipoDeParte);
    }

    export function Plv_Tras_Seleccionar_TipoDeParte(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        if (Tipos.ListaDinamica.Obtener(lista).IdSeleccionado > 0) {
            let panel = ApiPanel.ContenedorDe(idLista);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Venta.PlfDeVenta.TipoDeFactura);
        }
        else
            Plv_Tras_Blanquear_TipoDeParte(idLista);

    }
    export function Plv_Tras_Blanquear_TipoDeParte(idLista: string) {
        let panel = ApiPanel.ContenedorDe(idLista);
        ApiControl.DesbloquearListaDinamicaPorPropiedad(panel, ltrPropiedades.Venta.PlfDeVenta.TipoDeFactura);
    }

    export function Plv_Tras_Mapear_Filtro_IdContrato(control: HTMLElement) {
        let idContrato = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idContrato == 0)
            return;
        ApiDePeticiones.LeerElementoPorId(control, ltrControladores.Juridico.Contratos, idContrato, new Array<Parametro>(), idContrato)
            .then((peticion) => mapearDatosContrato(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function Plv_Tras_Mapear_Filtro_IdPlanificador(control: HTMLElement) {
        let idPlanificador = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (idPlanificador == 0)
            return;
        ApiDePeticiones.LeerElementoPorId(control, ltrControladores.Juridico.PlanificadorDeVentas, idPlanificador, new Array<Parametro>(), idPlanificador)
            .then((peticion) => mapearDatosPlanificador(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function mapearDatosPlanificador(peticion: ApiDeAjax.DescriptorAjax): any {
        let idFiltro = Crud.crudMnt.ModalDeFiltrado(ltrModalDeFiltrado.Venta.PlfDeVenta.FiltroDePlanificaciones);
        let panel = document.getElementById(idFiltro) as HTMLDivElement;
        ApiControl.BloquearListaDeValores(panel, ltrPropiedades.Venta.PlfDeVenta.ConOSinPlanificador);

        let idTipoDePlanificacion = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.IdTipoDePlanificacion));
        let tipoPlanificacion = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDePlanificacion);
        MapearAlControl.Propiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.Tipo, idTipoDePlanificacion, tipoPlanificacion, true, false);
        ApiControl.BloquearListaDinamicaPorPropiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.Tipo);

        let idTipoDeParte = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.IdTipoDeParte));
        if (idTipoDeParte > 0) {
            let tipoDeParte = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeParte);
            MapearAlControl.Propiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.TipoDeParte, idTipoDeParte, tipoDeParte, true, false);
        }
        else {
            let idTipoFactura = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.IdTipoDeFactura));
            let tipoFactura = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeFactura);
            MapearAlControl.Propiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.TipoDeFactura, idTipoFactura, tipoFactura, true, false);
        }
        ApiControl.BloquearListaDinamicaPorPropiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.TipoDeFactura);
        ApiControl.BloquearListaDinamicaPorPropiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.TipoDeParte);

        let idContrato = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.PlanificadorDeVenta.IdContrato));
        ApiDePeticiones.LeerElementoPorId(peticion.llamador, ltrControladores.Juridico.Contratos, idContrato, new Array<Parametro>(), idContrato)
            .then((peticion) => mapearDatosContrato(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function mapearDatosContrato(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCg = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.IdCg));
        let cg = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConCg.Cg);
        let idFiltro = Crud.crudMnt.ModalDeFiltrado(ltrModalDeFiltrado.Venta.PlfDeVenta.FiltroDePlanificaciones);
        let panel = document.getElementById(idFiltro) as HTMLDivElement;
        MapearAlPanel.RestrictoresPorPropiedad(Crud.crudMnt.PanelFiltro, ltrPropiedades.Elemento.ConCg.IdCg, idCg, cg);
        ApiControl.BloquearListaDeValores(panel, ltrPropiedades.Venta.PlfDeVenta.ConOSinContrato);
        ApiDelCrud.MapearDatosSocietariosYDepartamentales(Crud.crudMnt.crudDeCreacion.PanelDeCrear, peticion.resultado.datos);

        let idcontrato = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.Contrato.Id));
        let expresion = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Juridico.Contrato.Expresion);
        let contrato = ApiControl.BuscarRestrictor(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.IdContrato, ltrTipoControl.restrictorDeEdicion);
        MapearAlControl.Restrictor(contrato, idcontrato, expresion, true);

        let idTipo = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.ConTipo.IdTipo));
        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Elemento.ConTipo.IdTipo, idTipo));
        let idElemento = Numero(ObtenerPropiedad(peticion.resultado.datos, literal.id));
        ApiDePeticiones.LeerAmpliacion(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrControladores.Juridico.DatosDelContrato, ltrNegocioSe.Enumerado.Juridico.Contrato, idElemento, parametros, peticion.resultado.datos)
            .then((peticion) => mapearDatosCliente(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function mapearDatosCliente(peticion: ApiDeAjax.DescriptorAjax): any {
        let idCliente = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.PlfDeVenta.IdCliente));
        let cliente = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Venta.PlfDeVenta.Cliente);
        MapearAlControl.Propiedad(Crud.crudMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Venta.PlfDeVenta.Cliente, idCliente, cliente, true, false);
    }

}

