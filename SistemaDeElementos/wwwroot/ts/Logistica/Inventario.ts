namespace Logistica {

    // propiedad del filtro de la fecha del inventario (ver ltrDeUnMovimientoDeAlmacen.FiltroPorFechaDeInventario)
    const filtroPorFechaDeInventario: string = 'FiltroPorFechaDeInventario';

    export function CrearCrudDeInventario(idPanelMnt: string) {
        Crud.crudMnt = new Logistica.CrudDeInventario(idPanelMnt);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);
        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    // Navega a la edición del material de la fila; el id de la fila es el de su último movimiento hasta la fecha del inventario
    export function Inv_IrAlUnitario(numeroDeFila: number) {
        let idMovimiento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, idMovimiento));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Logistica.Inventario.ObtenerUrlAlUnitario, parametros, new Array<Parametro>())
            .then((peticion) => EntornoSe.AbrirPestana(`${window.location.origin}/${peticion.resultado.datos}`))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export class CrudDeInventario extends Crud.CrudMnt {
        constructor(idPanelMnt: string) {
            super(idPanelMnt, undefined);
            this.crudDeCreacion = undefined;
            this.crudDeEdicion = undefined;
        }

        // Propone la fecha de hoy si no viene ya una del estado guardado; el render del descriptor se cachea y no puede llevarla
        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            let fecha = ApiControl.BuscarEditor(this.PanelFiltro, filtroPorFechaDeInventario);
            if (Definido(fecha) && IsNullOrEmpty(fecha.value)) {
                let hoy = new Date();
                fecha.value = `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
            }
        }
    }
}
