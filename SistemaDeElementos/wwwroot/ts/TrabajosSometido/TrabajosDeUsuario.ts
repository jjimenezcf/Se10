namespace TrabajosSometido {

    export let crudTu: CrudDeTrabajosDeUsuario = null;

    export function CrearCrudDeTrabajosDeUsuario(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new TrabajosSometido.CrudDeTrabajosDeUsuario(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        crudTu = Crud.crudMnt as TrabajosSometido.CrudDeTrabajosDeUsuario;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }


    class TrabajoDeUsuario {
        Ejecutor: string;
        Encolado: Date;
        Estado: string;
        Id: number;
        IdEjecutor: number;
        IdSometedor: number;
        IdTrabajo: number;
        Iniciado: Date;
        ModoDeAcceso: string;
        Parametros: string;
        Periodicidad: number;
        Planificado: Date;
        Sometedor: string;
        Terminado: Date;
        Trabajo: string;
    }


    enum enumEstadoTrabajo { erroneo, pendiente, bloqueado, iniciado, terminado, conerrores }

    function ParsearEstado(estado: string): enumEstadoTrabajo {
        if (estado.toLowerCase() === 'erroneo') return enumEstadoTrabajo.erroneo;
        if (estado.toLowerCase() === 'pendiente') return enumEstadoTrabajo.pendiente;
        if (estado.toLowerCase() === 'bloqueado') return enumEstadoTrabajo.bloqueado;
        if (estado.toLowerCase() === 'iniciado') return enumEstadoTrabajo.iniciado;
        if (estado.toLowerCase() === 'terminado') return enumEstadoTrabajo.terminado;
        if (estado.toLowerCase() === 'con errores') return enumEstadoTrabajo.conerrores;

        throw Error(`No está definido el parseo para el estado ${estado}`);
    }

    export class CrudDeTrabajosDeUsuario extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionTrabajoDeUsuario(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionTrabajoDeUsuario(this, idPanelEdicion);
        }


        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (!super.DespuesDeProcesarOpcionMf(peticion)) {
                this.RestaurarPagina();
                if (ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion) === ltrMenus.eventosDeMf.Entorno.Trabajo.resometer)
                    peticion.llamador.InfoSelector.QuitarTodos();
                return true;
            }
            return false;
        }


        public DespuesDeProcesarOpcionMfConError(peticion: ApiDeAjax.DescriptorAjax) {
            try {
                super.DespuesDeProcesarOpcionMfConError(peticion);
            }
            finally {
                this.RestaurarPagina();
            }
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            for (let i: number = 0; i < this.InfoSelector.Cantidad; i++) {
                var elemento = this.InfoSelector.Seleccionados[i];
                let estado: string = elemento.Registro[ltrPropiedades.Sometidos.TrabajoDeUsuario.estado].toLowerCase();

                if (opcion === ltrMenus.eventosDeMf.Entorno.Trabajo.bloquear && ParsearEstado(estado) !== enumEstadoTrabajo.pendiente)
                    MensajesSe.EmitirExcepcion('IncluirParametrosParaProcesarOpcionMf', 'Solo se pueden bloquear trabajos en estado pendiente');

                if (opcion === ltrMenus.eventosDeMf.Entorno.Trabajo.desbloquear && ParsearEstado(estado) !== enumEstadoTrabajo.bloqueado)
                    MensajesSe.EmitirExcepcion('IncluirParametrosParaProcesarOpcionMf', 'Solo se pueden bloquear trabajos en estado bloqueados');

                if (opcion === ltrMenus.eventosDeMf.Entorno.Trabajo.resometer && (ParsearEstado(estado) !== enumEstadoTrabajo.terminado
                    && ParsearEstado(estado) !== enumEstadoTrabajo.conerrores
                    && ParsearEstado(estado) !== enumEstadoTrabajo.erroneo))
                    MensajesSe.EmitirExcepcion('IncluirParametrosParaProcesarOpcionMf', 'Solo se pueden resometer trabajos ya ejecutados');

                if (opcion === ltrMenus.eventosDeMf.Entorno.Trabajo.ejecutar && ParsearEstado(estado) !== enumEstadoTrabajo.pendiente)
                    MensajesSe.EmitirExcepcion('IncluirParametrosParaProcesarOpcionMf', 'Solo se pueden ejecutar trabajos planificados');
            }
        }

        public Inicializar(idPanelMnt: string) {
            super.Inicializar(idPanelMnt);
            this.MapearUsuarioConectado();
            if (!Registro.EsAdministrador())
                ApiControl.OcultarOpcionDeMenuPorNombre(this.ZonaDeMenu, ltrMenus.BarraDeMenu.Nuevo);
        }

        private MapearUsuarioConectado(): void {

            function usuarioNoLeido(llamador: CrudCreacionTrabajoDeUsuario): void {
                let zonaDeMenu: HTMLDivElement = llamador.CrudDeMnt.ZonaDeMenu;
                ApiControl.BloquearMenu(zonaDeMenu);
                console.error("no se ha podido leer");
            }


            let usuarioConectado: Registro.UsuarioDeConexion = Registro.UsuarioConectado();
            if (usuarioConectado.id == 0) {
                usuarioNoLeido(this.crudDeCreacion as CrudCreacionTrabajoDeUsuario);
            }
            else {
                let idUsuario: number = usuarioConectado.id;
                let usuario: string = usuarioConectado.login;
                MapearAlControl.RestrictoresDeEdicion(this.crudDeCreacion.PanelDeCrear, ltrPropiedades.Sometidos.TrabajoDeUsuario.idsometedor, idUsuario, usuario);
            }
        }

        public AplicarModoAccesoAlElemento(elemento: Elemento): void {
            super.AplicarModoAccesoAlElemento(elemento);
            let trabajo: TrabajoDeUsuario = elemento.Registro;
            let estado: string = trabajo[ltrPropiedades.Sometidos.TrabajoDeUsuario.estado].toLowerCase();
            let opcionesDeElemento: NodeListOf<HTMLButtonElement> = this.ZonaDeMenu.querySelectorAll(`input[${atOpcionDeMenu.clase}="${enumCssOpcionMenu.DeElemento}"]`) as NodeListOf<HTMLButtonElement>;
            ApiPanel.ValidarSiSeMantieneActiva(opcionesDeElemento, ['errores', 'traza'], this.InfoSelector.Cantidad);

            let menu = document.getElementById(ltrMenus.menu.individual);
            let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
            switch (estado) {
                case 'erroneo': {
                    ApiDeMenuFlotante.ValidarSiSeMantieneBloqueada(opcionesLi, ['bloquear', 'desbloquear', 'ejecutar']);
                    ApiDeMenuFlotante.ValidarSiSeMantieneActiva(opcionesLi, ['editar', 'resometer'], this.InfoSelector.Cantidad);
                    break;
                }
                case 'pendiente': {
                    ApiDeMenuFlotante.ValidarSiSeMantieneBloqueada(opcionesLi, ['desbloquear', 'resometer']);
                    ApiDeMenuFlotante.ValidarSiSeMantieneActiva(opcionesLi, ['editar', 'bloquear', 'ejecutar'], this.InfoSelector.Cantidad);
                    break;
                }
                case 'bloqueado': {
                    ApiDeMenuFlotante.ValidarSiSeMantieneBloqueada(opcionesLi, ['bloquear', 'borrar', 'ejecutar', 'resometer']);
                    ApiDeMenuFlotante.ValidarSiSeMantieneActiva(opcionesLi, ['editar', 'desbloquear'], this.InfoSelector.Cantidad);
                    break;
                }
                case 'iniciado': {
                    ApiDeMenuFlotante.ValidarSiSeMantieneBloqueada(opcionesLi, ['bloquear', 'desbloquear', 'ejecutar']);
                    ApiDeMenuFlotante.ValidarSiSeMantieneActiva(opcionesLi, ['editar', 'resometer'], this.InfoSelector.Cantidad);
                    break;
                }
                case 'terminado': {
                    ApiDeMenuFlotante.ValidarSiSeMantieneBloqueada(opcionesLi, ['bloquear', 'desbloquear', 'ejecutar']);
                    ApiDeMenuFlotante.ValidarSiSeMantieneActiva(opcionesLi, ['editar', 'resometer'], this.InfoSelector.Cantidad);
                    break;
                }
                case 'con errores': {
                    ApiDeMenuFlotante.ValidarSiSeMantieneBloqueada(opcionesLi, ['bloquear', 'desbloquear', 'ejecutar']);
                    ApiDeMenuFlotante.ValidarSiSeMantieneActiva(opcionesLi, ['editar', 'resometer'], this.InfoSelector.Cantidad);
                    break;
                }
                default: {
                    MensajesSe.Error('AjustarOpcionesDeMenuDelElemento', `No está definido que hacer con el estado ${estado} de un trabajo`);
                    ApiDeInicializacion.InicializarOpcionesDeMenuDeElemento(this.ZonaDeMenu);
                    ApiDeMenuFlotante.InicializarMenuFlotante(this.ContenedorMenuIndividual, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeElemento, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
                    //ApiDeMenuFlotante.InicializarMfs(ltrMenus.enumOrigen.crud, this.ContenedorDelMenuDelCrud, ClaseDeOpcioDeMenu.DeElemento, ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso);
                    break;
                }
            }
            ApiPanel.DesactivarConMultiSeleccion(opcionesDeElemento, this.InfoSelector.Cantidad);
        }

    }

    export class CrudCreacionTrabajoDeUsuario extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax) {
            super.InicializarControlesDeCreacion(peticion);
            if (!Registro.EsAdministrador()) {
                ApiControl.BloquearListaDinamicaPorPropiedad(this.PanelDeCrear, 'ejecutor');
                ApiControl.BloquearOpcionDeMenuPorNombre(this.PanelDeCrear, 'Crear');
            }
            else {
                ApiControl.DesbloquearListaDinamicaPorPropiedad(this.PanelDeCrear, 'ejecutor');
                ApiControl.DesbloquearOpcionDeMenuPorNombre(this.PanelDeCrear, 'Crear');
            }
        }

        public ValidarAntesDeCrear(): void {
            super.ValidarAntesDeCrear();
        }

    }

    export class CrudEdicionTrabajoDeUsuario extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax): void {
            super.AntesDeMapearElementoDevuelto(peticion);
            let estado: enumEstadoTrabajo = ParsearEstado(peticion.resultado.datos[ltrPropiedades.Sometidos.TrabajoDeUsuario.estado]);
            if (estado !== enumEstadoTrabajo.pendiente && estado !== enumEstadoTrabajo.bloqueado)
                peticion.resultado.modoDeAcceso = ModoAcceso.ModoDeAccesoDeDatos.Consultor;
        }
    }


}