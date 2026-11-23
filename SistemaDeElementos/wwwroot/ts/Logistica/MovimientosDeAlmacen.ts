namespace Logistica {

    export function CrearCrudDeMovimientosDeAlmacen(idPanelMnt: string) {
        Crud.crudMnt = new Logistica.CrudDeMovimientosDeAlmacen(idPanelMnt);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);
        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    // Navega a la edición del documento que generó el movimiento (pedido, regularización...), sea su origen o su destino
    export function Mal_IrAlDocumento(numeroDeFila: number) {
        let idMovimiento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, idMovimiento));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Logistica.MovimientosDeAlmacen.ObtenerUrlAlDocumento, parametros, new Array<Parametro>())
            .then((peticion) => EntornoSe.AbrirPestana(`${window.location.origin}/${peticion.resultado.datos}`))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    // Navega a la edición del proveedor del pedido recepcionado; si el movimiento no es de una recepción la celda está vacía
    export function Mal_IrAlProveedor(numeroDeFila: number) {
        let idMovimiento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, idMovimiento));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Logistica.MovimientosDeAlmacen.ObtenerUrlAlProveedor, parametros, new Array<Parametro>())
            .then((peticion) => EntornoSe.AbrirPestana(`${window.location.origin}/${peticion.resultado.datos}`))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    // Navega a la edición del preasiento contable del movimiento; si no lo tiene la celda está vacía
    export function Mal_IrAlPreasiento(numeroDeFila: number) {
        let idMovimiento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, idMovimiento));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Logistica.MovimientosDeAlmacen.ObtenerUrlAlPreasiento, parametros, new Array<Parametro>())
            .then((peticion) => EntornoSe.AbrirPestana(`${window.location.origin}/${peticion.resultado.datos}`))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export class CrudDeMovimientosDeAlmacen extends Crud.CrudMnt {
        constructor(idPanelMnt: string) {
            super(idPanelMnt, undefined);
            this.crudDeCreacion = undefined;
            this.crudDeEdicion = undefined;
        }

        // El filtro empieza plegado; se despliega con la etiqueta "Mostrar filtro" en la web o con el embudo de la barra en el móvil.
        // Se pliega al acabar de inicializar el crud, que es cuando el mantenimiento fija si el filtro se ve (ver OcultarMostrarFiltro)
        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            // Se fija el primer almacén de la lista antes de que el crud base cargue el grid, así se entra viendo los movimientos de un almacén y
            // no los de todos mezclados (el stock y el valor se calculan por almacén y material). El usuario puede cambiarlo; si la url o el estado
            // guardado traen un almacén, el crud base lo aplica después y prevalece (ver Inventario.ts, que hace algo parecido)
            const almacenes = ApiControl.BuscarControl(this.PanelFiltro, ltrPropiedades.Logistica.MovimientosDeAlmacen.FiltroPorAlmacen, false);
            if (almacenes instanceof HTMLSelectElement && Numero(almacenes.value) <= 0) {
                const primero = Array.from(almacenes.options).find(o => !IsNullOrEmpty(o.value) && Numero(o.value) > 0);
                if (Definido(primero))
                    MapearAlControl.ListaDeValores(almacenes, primero.value);
            }

            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            this.OcultarFiltro();
        }
    }
}
