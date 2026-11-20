namespace Logistica {

    export function CrearCrudDeAlmacenes(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Logistica.CrudDeAlmacenes(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeAlmacenes extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionAlmacen(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionAlmacen(this, idPanelEdicion);
        }

        // Opciones del menú del almacén que navegan a otros mantenimientos con el almacén fijado:
        //  - compras, ventas y recuentos: el almacén va como restrictor (se fija en el filtro y se propone al crear) y, si no hay ninguno,
        //    el mantenimiento pasa directamente a crear uno (nuevosivacio=1)
        //  - inventario y movimientos: el almacén se selecciona en su selector de almacenes (el inventario propone la fecha de hoy al abrir)
        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            const opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            const datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            const ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
            const textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
            const idAlmacen = ids[0];
            const restrictor = `[IdAlmacen=${idAlmacen}=${encodeURIComponent(textos[0])}]`;
            const conRestrictor = (url: string) => `${window.location.origin}/${url}?${ltrParametrosUrl.restrictores}=${restrictor}&${ltrParametrosUrl.nuevoSiVacio}=1`;
            const conSelector = (url: string) => `${window.location.origin}/${url}?${ltrPropiedades.Logistica.Inventario.FiltroPorAlmacen}=${idAlmacen}`;

            let urlDestino: string = undefined;
            switch (opcion) {
                case ltrMenus.eventosDeMf.Logistica.Almacenes.IrACompras:
                    urlDestino = conRestrictor(ltrUrls.Logistica.Pedidos);
                    break;
                case ltrMenus.eventosDeMf.Logistica.Almacenes.IrAVentas:
                    urlDestino = conRestrictor(ltrUrls.Logistica.Ventas);
                    break;
                case ltrMenus.eventosDeMf.Logistica.Almacenes.IrARecuentos:
                    urlDestino = conRestrictor(ltrUrls.Logistica.Regularizaciones);
                    break;
                case ltrMenus.eventosDeMf.Logistica.Almacenes.IrAInventario:
                    urlDestino = conSelector(ltrUrls.Logistica.Inventario);
                    break;
                case ltrMenus.eventosDeMf.Logistica.Almacenes.IrAMovimientos:
                    urlDestino = conSelector(ltrUrls.Logistica.MovimientosDeAlmacen);
                    break;
                default: return false;
            }
            EntornoSe.AbrirPestana(urlDestino);
            return true;
        }

    }

    export class CrudCreacionAlmacen extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            AjustarCuentasSegunVariacion(this.PanelDeCrear);
        }

    }

    export class CrudEdicionAlmacen extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            AjustarCuentasSegunVariacion(this.PanelDelDto);
        }

    }

    // Al marcar o desmarcar "Usa variación de existencias" (ver AlmacenDto)
    export function Alm_AlCambiar_UsaVariacion(check: HTMLInputElement) {
        const panel = Crud.crudMnt.ModoTrabajo === enumModoTrabajo.editando
            ? (Crud.crudMnt.crudDeEdicion as CrudEdicionAlmacen).PanelDelDto
            : (Crud.crudMnt.crudDeCreacion as CrudCreacionAlmacen).PanelDeCrear;
        AjustarCuentasSegunVariacion(panel, check);
    }

    // Sin variación de existencias las compras y devoluciones van a la cuenta de existencias: sus cuentas no se usan, se blanquean y bloquean
    function AjustarCuentasSegunVariacion(panel: HTMLDivElement, check: HTMLInputElement = undefined) {
        if (NoDefinido(check))
            check = ApiControl.BuscarCheck(panel, ltrPropiedades.Logistica.Almacen.UsaVariacionDeExistencias);
        if (NoDefinido(check))
            return;

        const cuentas = [ltrPropiedades.Logistica.Almacen.CuentaDeCompras, ltrPropiedades.Logistica.Almacen.CuentaDeDevoluciones];
        for (const cuenta of cuentas) {
            if (check.checked)
                ApiControl.DesbloquearListaDinamicaPorPropiedad(panel, cuenta);
            else
                ApiControl.BlanquearBloquearListaDinamicaPorPropiedad(panel, cuenta);
        }
    }
}
