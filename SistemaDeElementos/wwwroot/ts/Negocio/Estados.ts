namespace Negocio {

    export function CrearCrudDeEstados(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Negocio.CrudDeEstados(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeEstados extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionEstado(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionEstado(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
            let idEstado = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
            let estado = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
            switch (opcion) {
                case ltrMenus.eventosDeMf.Parametrizacion.Estados.Transiciones:
                    let url = `${window.location.origin}/${ltrUrls.Entorno.Trasiciones}?${literal.negocio}=${this.NombreDeNegocio}&${ltrParametrosUrl.filtros}=[${ltrParametrosUrl.idEstado}=${idEstado}=${estado}]`;
                    EntornoSe.AbrirPestana(url);
                    return true;
            }
            return false;
        }

        protected ParametrosOpcionalesBorrar(): Array<Parametro> {
            var p = super.ParametrosOpcionalesBorrar();
            let control = ApiControl.BuscarRestrictor(this.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            p.push(new Parametro(ltrPropiedades.Negocio.idNegocio, Numero(control.getAttribute(atControl.restrictor))));
            return p;
        }

    }


    export class CrudCreacionEstado extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            let control = ApiControl.BuscarRestrictor(this.CrudDeMnt.crudDeCreacion.PanelDeCrear, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeEdicion);
            let filtro = ApiControl.BuscarRestrictor(this.CrudDeMnt.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            control.setAttribute(atControl.restrictor, filtro.getAttribute(atControl.restrictor));
            control.value = filtro.value;
        }

    }
    export class CrudEdicionEstado extends Crud.CrudEdicion {
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            var p = super.ParametrosParaLeerElementoPorId();
            let control = ApiControl.BuscarRestrictor(this.CrudDeMnt.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            p.push(new Parametro(ltrPropiedades.Negocio.idNegocio, Numero(control.getAttribute(atControl.restrictor))));
            return p;
        }
    }

    export function EventosDeExpansores(accion: string, parametros: string): void {

        let partes: string[] = parametros.split(';');
        if (partes.length != 4)
            throw Error(`El parametro ${parametros} ha de definir el div contenedor del grid de detalle, el id de negocio, y el número de fila`);
        let idGridDeDetalleOrigen: string = partes[0];
        let idGridDeDetalleDestino: string = partes[1];
        let idNegocio = Numero(partes[2]);
        let numeroFila: number = Numero(partes[3]);

        let idDeLaTransicion: number = Crud.crudMnt.crudDeEdicion.Expansor_ObtenerIdDeLaRelacion(idGridDeDetalleOrigen, numeroFila);

        let gridDeDetalle: HTMLDivElement = document.getElementById(idGridDeDetalleDestino) as HTMLDivElement;
        MapearAlGrid.MapearGridDeDetalle(gridDeDetalle, idNegocio, idDeLaTransicion, Crud.crudMnt.Guid);
    }
}