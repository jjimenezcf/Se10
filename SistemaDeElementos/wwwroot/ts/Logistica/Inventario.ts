namespace Logistica {

    // propiedad del filtro de la fecha del inventario (ver ltrDeUnMovimientoDeAlmacen.FiltroPorFechaDeInventario)
    const filtroPorFechaDeInventario: string = 'FiltroPorFechaDeInventario';
    const filtroPorAlmacen: string = 'FiltroPorAlmacen';

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

        // Propone la fecha de hoy y, si el usuario sólo tiene acceso a un almacén, lo fija; se hace antes de que el crud base cargue el grid,
        // así lo normal (una empresa con un almacén) es entrar y ver el inventario de hoy sin abrir el filtro. El render del descriptor se cachea
        // y no puede llevar la fecha; la lista de almacenes sí es la del usuario (el render se cachea por usuario y la selección aplica la seguridad)
        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            let fecha = ApiControl.BuscarEditor(this.PanelFiltro, filtroPorFechaDeInventario);
            if (Definido(fecha) && IsNullOrEmpty(fecha.value)) {
                let hoy = new Date();
                fecha.value = `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
            }

            let almacenFijado = false;
            let almacenes = ApiControl.BuscarControl(this.PanelFiltro, filtroPorAlmacen, false);
            if (almacenes instanceof HTMLSelectElement) {
                let opciones = Array.from(almacenes.options).filter(o => !IsNullOrEmpty(o.value) && Numero(o.value) > 0);
                if (opciones.length === 1)
                    almacenFijado = Numero(almacenes.value) === Numero(opciones[0].value) || MapearAlControl.ListaDeValores(almacenes, opciones[0].value);
            }

            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            if (almacenFijado)
                this.OcultarFiltro();
        }
    }
}
