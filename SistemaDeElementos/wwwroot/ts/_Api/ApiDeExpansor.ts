namespace ApiDeExpansor {

    export function IdGridDelExpansor(nombreDadoEnElDescriptor: string): string {
        let idGrid = `${ltrTipoControl.gridDeDetalle}-${nombreDadoEnElDescriptor}-contenedor`.toLowerCase();
        return idGrid;
    }
        
    export function IdDeModalEditarRelacion(nombreDadoEnElDescriptor: string): string {
        return `${ltrTipoControl.gridDeDetalle}-${nombreDadoEnElDescriptor}-${enumPostfijoTipoModal.ModalDeEditarRelacion}`.toLowerCase();
    }

    export function ObtenerIdDeLaRelacion(idGridDeDetalle: string, numeroFila: number): number {
        let idDeLaFila = `${idGridDeDetalle.replace('-contenedor', '-tabla_d_tr')}_${numeroFila}`;
        let idDeLaRelacion: number = 0;
        try {
            let id = ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, atControl.id);
            idDeLaRelacion = Numero(id);
            if (idDeLaRelacion === 0) {
                throw new Error(`No es válido el id: ${id} de la fila ${idDeLaFila}`);
            }
        }
        catch (error) {
            MensajesSe.EmitirExcepcion("Borrar relación", 'No se ha podido obtener el Id de la relación', error);
        }
        return idDeLaRelacion;
    }

    export function ObtenerElValorDeLaPropiedadDeLaFila(idHtmlDeLaFila: string, propiedad: string): string {
        let fila: HTMLDivElement = document.getElementById(idHtmlDeLaFila) as HTMLDivElement;
        if (NoDefinido(fila))
            throw new Error(`La fila ${idHtmlDeLaFila} no está definida`);

        let filacells = fila.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.celda);
        for (let i: number = 0; i < filacells.length; i++) {
            let celda: HTMLDivElement = filacells[i];
            let p = celda.getAttribute(atControl.propiedad);
            if (p == propiedad.toLocaleLowerCase()) {
                let input: HTMLInputElement = celda.querySelector("input") as HTMLInputElement;
                return input.value;
            }
        }

        throw new Error(`La propiedad ${propiedad} no está en la fila ${fila}`);
    }


    export function AbrirModalDeRelacionParaEditar(llamador: Crud.CrudEdicion | Formulario.Jerarquia, idGridDeDetalle: string, propiedadesRestrictoras: string, numeroFila: number): void {
        let idDeLaRelacion: number = ObtenerIdDeLaRelacion(idGridDeDetalle, numeroFila);

        let gridDeDetalle: HTMLDivElement = document.getElementById(idGridDeDetalle) as HTMLDivElement;
        let controlador: string = gridDeDetalle.getAttribute(atGridDeDetalle.controlador);

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, gridDeDetalle.id));
        datosDeEntrada.push(new Parametro('propiedadesRestrictoras', propiedadesRestrictoras));

        let idNegocio: number = Numero(gridDeDetalle.getAttribute(literal.idNegocio));
        let parametros: Array<Parametro> = llamador.ParametrosParaLeerElementoPorId();
        if (idNegocio > 0) {
            if (parametros.length === 0 || NoDefinido(parametros.find(parametro => parametro.parametro === ltrPropiedades.Negocio.idNegocio)))
                parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, idNegocio));
        }

        var accionDeLeerPorId = gridDeDetalle.getAttribute(atGridDeDetalle.accionDeLeerPorId);

        ApiDePeticiones.LeerElementoPorId(llamador, controlador, idDeLaRelacion, parametros, datosDeEntrada, accionDeLeerPorId)
            .then((peticion) => Expansor_MapearRelacion(peticion, llamador))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function Expansor_MapearRelacion(peticion: ApiDeAjax.DescriptorAjax, llamador: Crud.CrudEdicion | Formulario.Jerarquia): any {

        let idGrid: string = peticion.DatosDeEntrada[0].valor;
        let propiedadesRestrictoras: string = peticion.DatosDeEntrada[1].valor;
        let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
        let idModalEdicion = grid.getAttribute(atGridDeDetalle.modalParaEditarRelacion);
        let modalDeEdicion: HTMLDivElement = document.getElementById(idModalEdicion) as HTMLDivElement;
        let modoTrabajo = llamador instanceof Crud.CrudEdicion ? llamador.CrudDeMnt.ModoTrabajo : llamador.ModoTrabajo;


        let modoDeAcceso = ObtenerPropiedad(peticion.resultado.datos, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);

        if (modoTrabajo === enumModoTrabajo.consultando)
            modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;

        if (llamador instanceof Crud.CrudEdicion) {
            if (Definido(llamador.ModalParaEditarEventoDeAgenda) && modalDeEdicion.id === llamador.ModalParaEditarEventoDeAgenda.id) {
                let esDelSistema: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.EventoDeAgenda.EsDelSistema);
                if (esDelSistema) modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;
            }
        }

        ApiPanel.AbrirModal(modalDeEdicion);
        let usaBaja = ExistePropiedad(peticion.resultado.datos, ltrPropiedades.baja);
        let estaDeBaja = usaBaja ? ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.baja, false) : false;
        MapearAlPanel.ElObjeto(modalDeEdicion, peticion.resultado.datos, modoDeAcceso);
        llamador.AsignarObjetoDeExpansor(idModalEdicion, peticion.resultado.datos);

        ModoAcceso.AplicarloAlPanel(modalDeEdicion, modoDeAcceso, estaDeBaja);
        ApiPanel.BloquearControlesPorPropieda(modalDeEdicion, propiedadesRestrictoras);
        llamador.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso)
    }

    export function BorrarRelacion(llamador: Crud.CrudEdicion | Formulario.Jerarquia, idGridDeDetalle: string, numeroFila: number, accionDeBorrado: string) {

        let idElemento = llamador instanceof Crud.CrudEdicion ? llamador.ElementoEditado.Id : ObtenerPropiedad(llamador.RegistroEditado, literal.id);
        let idNegocioDellamador = llamador instanceof Crud.CrudEdicion ? llamador.CrudDeMnt.IdNegocio : llamador.IdNegocio;

        let idDelRegistro: number = ApiDeExpansor.ObtenerIdDeLaRelacion(idGridDeDetalle, numeroFila);

        let gridDeDetalle: HTMLDivElement = document.getElementById(idGridDeDetalle) as HTMLDivElement;
        let controlador: string = gridDeDetalle.getAttribute(atGridDeDetalle.controlador);
        let idNegocio: number = Numero(gridDeDetalle.getAttribute(literal.idNegocio));
        let idNegocioVinculado = Numero(gridDeDetalle.getAttribute(atGridDeDetalle.IdNegocioVinculado));

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, gridDeDetalle.id));
        datosDeEntrada.push(new Parametro(atControl.idDelElemento, idElemento));

        if (idNegocioVinculado > 0) {
            ApiDePeticiones.BorrarVinculo(llamador, controlador, idNegocioDellamador, idNegocioVinculado, idElemento, idDelRegistro, new Array<Parametro>(), datosDeEntrada)
                .then((peticion: ApiDeAjax.DescriptorAjax) => DespuesDeBorrarRelacion(peticion, llamador))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            return;
        }

        let parametros: Array<Parametro> = llamador.ParametrosParaBorrarRelacion();
        if (idNegocio > 0) {
            if (parametros.length === 0 || NoDefinido(parametros.find(parametro => parametro.parametro === ltrPropiedades.Negocio.idNegocio)))
                parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, idNegocio));
        }

        ApiDePeticiones.BorrarRelacionPorId(llamador, controlador, idDelRegistro, parametros, datosDeEntrada, accionDeBorrado)
            .then((peticion) => DespuesDeBorrarRelacion(peticion, llamador))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function DespuesDeBorrarRelacion(peticion: ApiDeAjax.DescriptorAjax, llamador: Crud.CrudEdicion | Formulario.Jerarquia): void {

        let idGrid: string = peticion.DatosDeEntrada[0].valor;
        let idDelElemento: number = Definido(peticion.DatosDeEntrada.find(x => x.parametro === atControl.idDelElemento))
            ? peticion.DatosDeEntrada.find(x => x.parametro === atControl.idDelElemento).valor
            : 0;

        let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
        RecargarGridDeRelacion(llamador, grid, llamador instanceof Crud.CrudEdicion ? llamador.CrudDeMnt.IdNegocio : llamador.IdNegocio, idDelElemento);
    }

    export function ModificarRelacion(llamador: Crud.CrudEdicion | Formulario.Jerarquia, idModal: string): void {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let json: JSON = ApiDelCrud.MapearControlesDesdeElCrudAlJson(llamador, modal, enumModoTrabajo.editando);
        let controlador: string = modal.getAttribute(atControl.controlador);
        let accion: string = modal.getAttribute(atModal.accion);
        var idNegocio = llamador instanceof Crud.CrudEdicion ? llamador.CrudDeMnt.IdNegocio : llamador.IdNegocio;

        ApiDePeticiones.ModificarRelacion(llamador, controlador, Definido(accion) ? accion : Ajax.EndPoint.ModificarRelacion, idNegocio, json)
            .then((peticion: ApiDeAjax.DescriptorAjax) => DespuesDeModificarRelacion(llamador, peticion, modal))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function DespuesDeModificarRelacion(llamador: Crud.CrudEdicion | Formulario.Jerarquia, peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): any {

        //blanquear los controles de la interface
        ApiPanel.BlanquearControlesDeIU(modal);
        ApiPanel.OcultarModal(modal);

        var idNegocio = llamador instanceof Crud.CrudEdicion ? llamador.CrudDeMnt.IdNegocio : llamador.IdNegocio;
        var registro = llamador instanceof Crud.CrudEdicion ? llamador.ElementoEditado.Registro : llamador.RegistroEditado;

        if (peticion.nombre === Ajax.EndPoint.ModificarRelacion) {
            let accion = modal.getAttribute(atModal.trasModificar);
            if (Definido(accion))
                Evaluar('ApiDeExpansor.DespuesDeModificarRelacion', accion, accion.includes('this') ? modal : undefined);
        }

        //recargar el grid de relaciones del expansor
        let idGrid: string = modal.getAttribute(atGridDeDetalle.gridDeRelacionAsociado);
        let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
        let campoRestrictor: string = grid.getAttribute(atGridDeDetalle.campoRestrictor);
        RecargarGridDeRelacion(llamador, grid, idNegocio, ObtenerCampoRestrictor(registro, campoRestrictor));
    }
    
    function RecargarGridDeRelacion(llamador: Crud.CrudEdicion | Formulario.Jerarquia, grid: HTMLDivElement, idnegocio: number, id: number) {

        if (llamador instanceof Crud.CrudEdicion) {
            llamador.RecargarGridDeRelacion(grid, llamador.CrudDeMnt.IdNegocio, id);
        }
        else {
            MapearAlGrid.MapearGridDeDetalle(grid, idnegocio, id, llamador.Guid);
        }
    }

}