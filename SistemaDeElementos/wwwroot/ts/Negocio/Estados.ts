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
            let idEstado = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids)[0];
            let estado = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
            switch (opcion) {
                case ltrMenus.eventosDeMf.Parametrizacion.Estados.Transiciones:
                    let url = `${window.location.origin}/${ltrUrls.Entorno.Trasiciones}?${literal.negocio}=${this.NombreDeNegocio}&${ltrParametrosUrl.filtros}=[${ltrParametrosUrl.idEstado}=${idEstado}=${estado}]`;
                    EntornoSe.AbrirPestana(url);
                    return true;
                case ltrMenus.eventosDeMf.Parametrizacion.Estados.Flujo:
                    Flujo.CargarYMostrarFlujo(this, estado, this.NombreDeNegocio, Numero(idEstado));
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
        // etapas marcadas al cargar el estado, y si el usuario puede cambiarlas (sólo un parametrizador)
        private _etapasAlCargar: Array<string> = [];
        private _puedeModificarEtapas: boolean = false;

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            var p = super.ParametrosParaLeerElementoPorId();
            let control = ApiControl.BuscarRestrictor(this.CrudDeMnt.PanelFiltro, ltrPropiedades.Negocio.idNegocio, ltrTipoControl.restrictorDeFiltro);
            p.push(new Parametro(ltrPropiedades.Negocio.idNegocio, Numero(control.getAttribute(atControl.restrictor))));
            return p;
        }

        // El ? de la cabecera: un estado no es un elemento de proceso (no tiene transiciones que aplicar), así que en vez de
        // explicar las de las flechas se abre la ayuda del flujo al que pertenece el estado que se está editando
        public AyudaDeTransiciones(): void {
            const estado = ObtenerPropiedad(this.Registro, 'nombre', '');
            if (IsNullOrEmpty(estado)) {
                MensajesSe.Info('Seleccione un estado para ver la ayuda de su flujo');
                return;
            }
            const url = `/${ltrControladores.Negocio.Estados}/${Ajax.EndPoint.Negocio.Estados.AyudaDelFlujo}`
                + `?negocio=${encodeURIComponent(this.CrudDeMnt.NombreDeNegocio)}&estado=${encodeURIComponent(estado)}`;
            window.open(url, '_blank');
        }

        // ── Etapas del estado ────────────────────────────────────────────────────────────────────
        private get ContenedorDeEtapas(): HTMLDivElement {
            return this.PanelDeEditar.querySelector(`.${ltrCss.etapas.contenedor}`) as HTMLDivElement;
        }

        // Al mostrar un estado se leen las etapas del negocio y se marcan las que lo incluyen
        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            this.CargarEtapasDelEstado(Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Id, 0)));
        }

        private CargarEtapasDelEstado(idEstado: number): void {
            const contenedor = this.ContenedorDeEtapas;
            if (!Definido(contenedor)) return;

            contenedor.innerHTML = '';
            this._etapasAlCargar = [];
            this._puedeModificarEtapas = false;
            if (idEstado <= 0) return;

            const url = `/${ltrControladores.Negocio.Estados}/${Ajax.EndPoint.Negocio.Estados.LeerEtapas}`
                      + `?negocio=${encodeURIComponent(this.CrudDeMnt.NombreDeNegocio)}&idEstado=${idEstado}`;
            fetch(url, { credentials: 'same-origin' })
                .then(r => r.json())
                .then(d => {
                    if (d.estado !== 'Ok') {
                        MensajesSe.Error('Etapas del estado', d.mensaje || 'No se han podido leer las etapas del estado');
                        return;
                    }
                    this.PintarEtapas(contenedor, d.datos.etapas || [], d.datos.puedeModificar === true);
                })
                .catch(() => MensajesSe.Error('Etapas del estado', 'No se han podido leer las etapas del estado'));
        }

        // Una casilla por etapa, igual que el selector de calificadores de la modal de direcciones
        private PintarEtapas(contenedor: HTMLDivElement, etapas: Array<{ nombre: string, descripcion: string, incluida: boolean }>, puedeModificar: boolean): void {
            contenedor.innerHTML = '';
            this._puedeModificarEtapas = puedeModificar;
            this._etapasAlCargar = etapas.filter(e => e.incluida).map(e => e.nombre);

            if (etapas.length === 0) {
                const vacio = document.createElement('span');
                ApiControl.IncluirCss(vacio, ltrCss.etapas.vacio);
                vacio.innerText = 'El negocio no define etapas';
                contenedor.appendChild(vacio);
                return;
            }

            etapas.forEach(etapa => {
                const label = document.createElement('label');
                ApiControl.IncluirCss(label, ltrCss.etapas.opcion);
                label.title = etapa.descripcion;
                const chk = document.createElement('input');
                chk.type = 'checkbox';
                chk.value = etapa.nombre;
                chk.checked = etapa.incluida;
                if (!puedeModificar) {
                    chk.disabled = true;
                    ApiControl.IncluirCss(label, ltrCss.etapas.opcionDeshabilitada);
                }
                label.appendChild(chk);
                label.appendChild(document.createTextNode(etapa.nombre));
                contenedor.appendChild(label);
            });
        }

        // Etapas marcadas ahora, o undefined si no se han tocado (o no se pueden modificar)
        private EtapasMarcadasSiHanCambiado(): Array<string> {
            const contenedor = this.ContenedorDeEtapas;
            if (!Definido(contenedor) || !this._puedeModificarEtapas) return undefined;

            const marcadas = Array.from(contenedor.querySelectorAll('input[type="checkbox"]') as NodeListOf<HTMLInputElement>)
                .filter(c => c.checked).map(c => c.value);
            const iguales = marcadas.length === this._etapasAlCargar.length
                && marcadas.every(m => this._etapasAlCargar.includes(m));
            return iguales ? undefined : marcadas;
        }

        // Marcar o desmarcar una etapa cuenta como cambio pendiente (el editor avisa al cerrar o cambiar de estado)
        public get HayCambiosPendientes(): boolean {
            if (super.HayCambiosPendientes) return true;
            return this.EstoyEditando && this.CargaRealizada && Definido(this.EtapasMarcadasSiHanCambiado());
        }

        // Tras guardar el estado se graban las etapas si han cambiado; si el estado queda en más de una se avisa, no es un error
        protected DespuesDeModificar(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): void {
            const marcadas = this.EtapasMarcadasSiHanCambiado();
            const idEstado = Numero(this.ElementoEditado.Id);
            const terminar = () => super.DespuesDeModificar(peticion, modal);
            if (!Definido(marcadas)) {
                terminar();
                return;
            }

            const url = `/${ltrControladores.Negocio.Estados}/${Ajax.EndPoint.Negocio.Estados.GrabarEtapas}`
                      + `?negocio=${encodeURIComponent(this.CrudDeMnt.NombreDeNegocio)}&idEstado=${idEstado}`;
            // mismo formato que EjecutarPeticionPost: encodeURIComponent(JSON.stringify([{parametro, valor}]))
            const body = encodeURIComponent(JSON.stringify([{ parametro: 'etapas', valor: marcadas }]));
            let aviso: string = '';
            let error: string = '';
            fetch(url, { method: 'POST', credentials: 'same-origin', body })
                .then(r => r.json())
                .then(d => {
                    if (d.estado === 'Ok') aviso = d.mensaje || '';
                    else error = d.mensaje || 'No se han podido grabar las etapas del estado';
                })
                .catch(() => { error = 'No se han podido grabar las etapas del estado'; })
                .finally(() => {
                    terminar();
                    if (!IsNullOrEmpty(error)) MensajesSe.Error('Etapas del estado', error);
                    else if (!IsNullOrEmpty(aviso)) MensajesSe.Advertencia(aviso);
                });
        }
    }

    // El icono (i) de una fila de las transiciones del estado: explica qué hace esa transición desde el estado que se edita
    export function AyudaDeUnaTransicion(idGridDeDetalle: string, numeroFila: number): void {
        const edicion = Crud.crudMnt.crudDeEdicion;
        const transicion = edicion.Expansor_ObtenerPropiedad(idGridDeDetalle, numeroFila, 'nombre');
        const estado = ObtenerPropiedad(edicion.Registro, 'nombre', '');
        Flujo.MostrarAyudaDeUnaTransicion(Crud.crudMnt.NombreDeNegocio, estado, transicion);
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
