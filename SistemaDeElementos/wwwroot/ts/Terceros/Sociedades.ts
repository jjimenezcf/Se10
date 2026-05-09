namespace Terceros {

    export let crudDeSociedad: Terceros.CrudDeSociedades = null;

    export function CrearCrudDeSociedades(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Terceros.CrudDeSociedades(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        Terceros.crudDeSociedad = Crud.crudMnt as Terceros.CrudDeSociedades;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeSociedades extends Crud.CrudMnt {
        private _TercerosJudiciales: boolean = undefined;

        public get TercerosJudiciales(): boolean {
            return this._TercerosJudiciales;
        }

        public get ControlNuevaSociedad(): HTMLInputElement {
            return document.getElementById(`${Crud.crudMnt.crudDeCreacion.TablaDeCreacion.id}-nombre`) as HTMLInputElement;
        }

        public get ControlNuevaRazonSocial(): HTMLInputElement {
            return document.getElementById(`${Crud.crudMnt.crudDeCreacion.TablaDeCreacion.id}-razonsocial`) as HTMLInputElement;
        }

        public get ModalAsociarCertificado(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Maestros.Terceros.AsociarCertificado) as HTMLDivElement; }
        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionSociedad(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionSociedad(this, idPanelEdicion);
        }


        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._TercerosJudiciales = mapIndicadores.get(ltrPropiedades.Terceros.Sociedad.Indicadores.TercerosJudiciales);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalAsociarCertificado.id) {
                let sociedad: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Terceros.CertificadoDeUnaSociedad.idSociedad, true) as HTMLInputElement;
                MapearAlControl.Restrictor(sociedad, this.InfoSelector.Seleccionados[0].Id, this.InfoSelector.Seleccionados[0].Texto);
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            if (modal.id === this.ModalAsociarCertificado.id) {
                LlamarA_AsociarCertificado(modal, (modal) => super.ModalDePedirDatos_Aceptar(modal));
            }
            else
                super.ModalDePedirDatos_Aceptar(modal);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return Terceros.MostrarInterlocutores(peticion, ltrParametrosUrl.idSociedad);
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.CentrosGestores) {
                return Terceros.MostrarCentrosGestores(peticion);
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.AsociarCertificado) {
                let idModal = Crud.crudMnt.IdCrud + '-' + opcion;
                crudDeSociedad.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, 0);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.CuentasBancarias) {
                crudDeSociedad.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(crudDeSociedad.crudDeEdicion._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.CuentasBancarias), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.TarjetasBancarias) {
                crudDeSociedad.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(crudDeSociedad.crudDeEdicion._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.TarjetasBancarias), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Facturador) {
                crudDeSociedad.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(crudDeSociedad.crudDeEdicion._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.Facturador), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Buzones) {
                crudDeSociedad.crudDeEdicion.Expansor_AbrirModalDeRelacionParaCrear(crudDeSociedad.crudDeEdicion._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.Buzones), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            return false;
        }


        protected DespuesDeLeerFilaSeleccionada(peticion: ApiDeAjax.DescriptorAjax): any {
            super.DespuesDeLeerFilaSeleccionada(peticion);

            let elemento = peticion.resultado.datos;
            if (Crud.crudMnt.InfoSelector.Cantidad == 1) {
                let esDeMisSociedades: boolean = ObtenerPropiedad(elemento, ltrPropiedades.Terceros.Sociedad.EsUnaDeMisSociedades);
                if (!esDeMisSociedades) {
                    ApiDeMenuFlotante.BloquearOpcionDeMenu(crudDeSociedad.ContenedorMenuIndividual, ltrMenus.eventosDeMf.Maestros.Terceros.CentrosGestores, ltrMenus.enumOrigen.edicion);
                }
            }
        }
    }

    export class CrudCreacionSociedad extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            if (!EsTrue((this.CrudDeMnt as CrudDeSociedades).TercerosJudiciales)) {
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProcurador);
            }

            ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Correspondencia);
        }

        public InicializarControlesDeCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.InicializarControlesDeCreacion(peticion);
            if (this.CrudDeMnt.Estado.Obtener(ltrClaveDeEstado.paginaOrigen) == ltrPaginas.Terceros.Cliente) {
                MapearAlControl.Check(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearCliente, true, true);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProcurador);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProveedor);
                ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Fiscal);
            }
            else if (this.CrudDeMnt.Estado.Obtener(ltrClaveDeEstado.paginaOrigen) == ltrPaginas.Terceros.Proveedor) {
                MapearAlControl.Check(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProveedor, true, true);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProcurador);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearCliente);
                ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Fiscal);
            }
            else if (this.CrudDeMnt.Estado.Obtener(ltrClaveDeEstado.paginaOrigen) == ltrPaginas.Terceros.Interlocutor) {
                MapearAlControl.Check(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearInterlocutor, true, true);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearAbogado);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProcurador);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearProveedor);
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.Terceros.Sociedad.CrearCliente);
                ApiDeDireccion.FijarCalificador(this.PanelDeCrear, ltrAmpliaciones.Comunes.CrearDireccion, enumCalificadorDireccion.Contacto);
            }
        }

        protected DespuesDeCrear(peticion: ApiDeAjax.DescriptorAjax) {
            let crudCreador: CrudCreacionSociedad = peticion.llamador as CrudCreacionSociedad;
            if (!crudCreador.SeguirCreando) {
                var idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.IdCliente));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Cliente, ltrUrls.Terceros.Cliente, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.IdProveedor));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Proveedor, ltrUrls.Terceros.Proveedor, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.IdAbogado));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Abogado, ltrUrls.Terceros.Abogado, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.IdProcurador));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Procurador, ltrUrls.Terceros.Procurador, idTercero);
                }
                idTercero = Numero(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.IdInterlocutor));
                if (idTercero > 0) {
                    return crudCreador.CrudDeMnt.NavegarAEditarElemento(ltrPaginas.Terceros.Interlocutor, ltrUrls.Terceros.Interlocutor, idTercero);
                }
            }

            super.DespuesDeCrear(peticion);
        }

    }

    export class CrudEdicionSociedad extends Crud.CrudEdicion {

        private get _AnadirContacto(): HTMLAnchorElement {
            const id = `${this._idPanelEdicion}-contactos-mcr-${enumPostfijoControl.Referencia}`;
            return document.getElementById(id) as HTMLAnchorElement;
        };


        private _crearContactoAlEntrar: boolean = false;

        public get CrearContactoAlEntrar(): boolean {
            return this._crearContactoAlEntrar;
        }
        public set CrearContactoAlEntrar(value: boolean) {
            this._crearContactoAlEntrar = value;
        }
        public get ModalDeCreacionDeBuzones(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Sociedad.Buzones);
        }

        public get ModalDeEdicionDeBuzones(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Sociedad.Buzones);
        }
        public get ModalDeCreacionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Sociedad.CuentasBancarias);
        }

        public get ModalDeEdicionDeCuentasBancarias(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Sociedad.CuentasBancarias);
        }

        public get GridDeTarjetas(): HTMLDivElement {
            return ApiPanel.BuscarGridPorControlador(this.PanelDeEditar, ltrControladores.Terceros.Sociedad.Tarjetas);
        }
        public get ModalDeCreacionDeTarjetasBancarias(): HTMLDivElement {
            return this.ModalParaCrearRelacion(ltrEspanes.Tercero.Sociedad.TarjetasBancarias);
        }

        public get ModalDeEdicionDeTarjetasBancarias(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Sociedad.TarjetasBancarias);
        }
        public get ModalDeEdicionDeContactos(): HTMLDivElement {
            return this.ModalParaEditarRelacion(ltrEspanes.Tercero.Sociedad.Contactos);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public Expansor_TrasCargarAmpliacion(ampliacion: HTMLDivElement): void {
            super.Expansor_TrasCargarAmpliacion(ampliacion);
            let esDeMisSociedades: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Terceros.Sociedad.EsUnaDeMisSociedades);
            ApiPanel.OcultarMostrarPanel(this.DivDeAmpliacion(ltrAmpliaciones.sociedades.parametros), !esDeMisSociedades);
        }


        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
            let esDeMisSociedades: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Terceros.Sociedad.EsUnaDeMisSociedades);
            ApiPanel.MostrarDetalleSi(this.DivDelExpansor(ltrEspanes.Tercero.Sociedad.Buzones), peticion.resultado.datos, esDeMisSociedades);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let idAgenda: number = ObtenerPropiedad(this.Registro, ltrPropiedades.Terceros.Sociedad.idAgenda);

            let esDeMisSociedades: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Terceros.Sociedad.EsUnaDeMisSociedades);
            let usaVerifactu: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Terceros.Sociedad.UsaVerifactu);
            let VerifactuEnProductivo: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Terceros.Sociedad.VerifactuEnProductivo);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.CuentasBancarias, esDeMisSociedades);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.TarjetasBancarias, esDeMisSociedades);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.AsociarCertificado, esDeMisSociedades);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.CrearLote, esDeMisSociedades);

            const esParametrizador = Registro.UsuarioConectado().parametrizador;
            const mostrar = esDeMisSociedades && esParametrizador;
            const mostrarVerifactu = mostrar && usaVerifactu;
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.ActivarVerifactu, mostrarVerifactu);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.RecomponerBlockChain, mostrarVerifactu);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.Facturador, mostrarVerifactu);
            ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.CatalogosJudiciales, mostrar);

            if (mostrar) {
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.CrearLote, ltrMenus.enumOrigen.edicion, this.EstaDeBaja);
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.CatalogosJudiciales, ltrMenus.enumOrigen.edicion, this.EstaDeBaja);
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.Facturador, ltrMenus.enumOrigen.edicion, this.EstaDeBaja || !usaVerifactu);
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.RecomponerBlockChain, ltrMenus.enumOrigen.edicion, this.EstaDeBaja || !usaVerifactu);

                if (usaVerifactu) {
                    ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.ActivarVerifactu, ltrMenus.enumOrigen.edicion, this.EstaDeBaja || VerifactuEnProductivo);

                    if (!VerifactuEnProductivo) {
                        ApiDeMenuFlotante.CambiarNombre(this.CrudDeMnt.crudDeEdicion.ContenedorMenu, ltrMenus.eventosDeMf.Maestros.Terceros.ActivarVerifactu, 'Verifactu de Test a Productivo');
                    }
                }

            }

            ApiPanel.OcultarContenedorDto(this.PanelDelDto, ltrPropiedades.Terceros.Sociedad.Agenda, Numero(idAgenda) === 0);
            if (this.CrudDeMnt.Estado.Contiene(ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearContacto)) {
                //this.SalvarCambios = false;
                this.CrearContactoAlEntrar = true;
                this._AnadirContacto.click();
            }
            ApiDeMenuFlotante.OcultarUltimoHr(this.CrudDeMnt.crudDeEdicion.ContenedorMenu);
        }

        protected Expansor_DespuesDeBorrarRelacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.Expansor_DespuesDeBorrarRelacion(peticion);
            let edicion = peticion.llamador as CrudEdicionSociedad;
            let funcion = this.IdArchivoMostrado == 0 ? (peticion) => this.AlTerminarDeLeerArchivos(peticion) : null;
            ApiDeArchivos.MostrarArchivosAnexados(edicion.PanelDeArchivos.id, edicion.CrudDeMnt.NombreDeNegocio, edicion.Registro.id, funcion);
            edicion.RecargarGridDeTrazas();
        }

        public Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion: string, propiedadesRestrictoras: string): void {
            super.Expansor_AbrirModalDeRelacionParaCrear(idModalDeCreacion, propiedadesRestrictoras);

            if (this.CrudDeMnt.Estado.Contiene(ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearContacto)) {
                let modal = this.ModalParaCrearRelacion('contactos');
                if (idModalDeCreacion === modal.id) {
                    MapearAlControl.Check(modal, ltrPropiedades.Terceros.Sociedad.CrearInterlocutor, true, true);
                    this.CrudDeMnt.Estado.Quitar(ltrMenus.BarraDeMenu.Terceros.Interlocutor.CrearContacto)
                    this.CrudDeMnt.Estado.Guardar();
                    EntornoSe.Historial.Persistir();
                    //this.SalvarCambios = true;
                }
            }
        }

        public Expansor_DespuesDeMapearLosDatosEditados(peticion: ApiDeAjax.DescriptorAjax, modalDeEdicion: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.Expansor_DespuesDeMapearLosDatosEditados(peticion, modalDeEdicion, modoDeAcceso);
            if (modalDeEdicion.id === this.ModalDeEdicionDeCuentasBancarias.id) {
                if (ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Activa))
                    this.AplicarCuentaActiva(this.ModalDeEdicionDeCuentasBancarias);
                else
                    this.AplicarCuentaNoActiva(this.ModalDeEdicionDeCuentasBancarias);
            }
            else if (modalDeEdicion.id === this.ModalDeEdicionDeTarjetasBancarias.id) {
                if (ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.Tarjeta.Activa))
                    this.AplicarTarjetaActiva(this.ModalDeEdicionDeTarjetasBancarias);
                else
                    this.AplicarTarjetaNoActiva(this.ModalDeEdicionDeTarjetasBancarias);
            }
            else if (modalDeEdicion.id === this.ModalDeEdicionDeContactos.id) {
                ApiControl.BloquearCheckPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Sociedad.CrearInterlocutor,
                    ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Terceros.Sociedad.EsInterlocutor, false) ||
                    ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Baja, false));
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let editor: CrudEdicionSociedad = peticion.llamador as CrudEdicionSociedad;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return Terceros.MostrarInterlocutores(peticion, ltrParametrosUrl.idSociedad);
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.CentrosGestores) {
                return Terceros.MostrarCentrosGestores(peticion);
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.CuentasBancarias) {
                editor.Expansor_AbrirModalDeRelacionParaCrear(editor._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.CuentasBancarias), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.TarjetasBancarias) {
                editor.Expansor_AbrirModalDeRelacionParaCrear(editor._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.TarjetasBancarias), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Facturador) {
                editor.Expansor_AbrirModalDeRelacionParaCrear(editor._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.Facturador), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Buzones) {
                editor.Expansor_AbrirModalDeRelacionParaCrear(editor._idDeModalCrearRelacion(ltrEspanes.Tercero.Sociedad.Buzones), ltrPropiedades.Elemento.IdElemento);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.AsociarCertificado) {
                let idModal = editor.CrudDeMnt.IdCrud + '-' + opcion;
                let id = ObtenerPropiedad(editor.Registro, literal.id);
                editor.Expansor_AbrirModalParaPedirDatos(idModal, id);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.Contabilidad.Preasientos.LoteTerceros) {
                MensajesSe.Info('Trabajo de crear lote con terceros sometido correctamente');
                return true;
            }
            return false;
        }

        public AplicarCuentaNoActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = ltrStyle.display.none;
            ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Alias);
        }

        public AplicarCuentaActiva(modalDeEdicion: HTMLDivElement) {
            let selector: HTMLAnchorElement = modalDeEdicion.querySelector(`a[${atControl.class}='${ltrCss.formulario.selectorDeArchivo} ${ltrCss.controlesDto.etiqueta}']`);
            selector.style.display = '';
            ApiControl.DesbloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Alias);
        }

        public AplicarTarjetaNoActiva(modalDeEdicion: HTMLDivElement) {
            ApiControl.BloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Sociedad.Tarjeta.Alias);
        }

        public AplicarTarjetaActiva(modalDeEdicion: HTMLDivElement) {
            ApiControl.DesbloquearEditorPorPropiedad(modalDeEdicion, ltrPropiedades.Terceros.Sociedad.Tarjeta.Alias);
        }

        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Tercero.Sociedad.CuentasBancarias)) {
                this.RecargarGridDeTrazas();
                this.RecargarGridDeTarjetas();
            }
        }

        private RecargarGridDeTarjetas() {
            var tarjetas = this.GridDeTarjetas;
            if (!Definido(tarjetas))
                return;
            MapearAlGrid.MapearGridDeDetalle(tarjetas, this.CrudDeMnt.IdNegocio, Numero(ObtenerPropiedad(this.Registro, literal.id)), this.CrudDeMnt.Guid);
        }

    }

    export function MostrarCentrosGestores(peticion: ApiDeAjax.DescriptorAjax): boolean {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let filtrarPorBaja: Tipos.Restrictor = new Tipos.Restrictor(ltrPropiedades.Elemento.FitrarPorBaja, 2, undefined, true);
        let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(ltrPaginas.Terceros.CentroGestor);
        let filtros: Tipos.Restrictor[] = estado.ObtenerObjeto<Array<Tipos.Restrictor>>(ltrClaveDeEstado.filtrosUrl, () => new Array<Tipos.Restrictor>());
        filtros.push(filtrarPorBaja);
        estado.Agregar(ltrClaveDeEstado.filtrosUrl, filtros);
        estado.Guardar();
        let url = `${window.location.origin}/${ltrUrls.Terceros.CentrosGestores}?${ltrParametrosUrl.idSociedad}=${ids[0]}`;
        EntornoSe.NavegarAUrl(url);
        return true;
    }

    export function EventosDeSociedad(accion: string, parametros: string) {
        try {
            switch (accion) {
                case ltrEventos.Sociedad.EditarContacto: {
                    let modal = document.getElementById(parametros) as HTMLDivElement;
                    let esInterlocutor = ApiControl.BuscarControl(modal, 'esInterlocutor', true) as HTMLInputElement;
                    if (esInterlocutor.checked) {
                        //esInterlocutor.parentElement.querySelector("label").textContent = "dar de baja";
                    }
                    break;
                }
            }
        }
        catch (error) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, error.message);
        }
    }

    export function Sociedad_CrearProcurador_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Sociedad.CrearProcurador);
    }

    export function Sociedad_CrearAbogado_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Sociedad.CrearAbogado);
    }

    export function Sociedad_CrearProveedor_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Sociedad.CrearProveedor);
    }

    export function Sociedad_CrearCliente_Change(check: HTMLInputElement) {
        SincronizarCheck(check, ltrPropiedades.Terceros.Sociedad.CrearCliente);
    }

    export function Sociedad_AbrirAgenda(id: number) {
        let editor = crudDeSociedad.crudDeEdicion as CrudEdicionSociedad;
        let idAgenda = ObtenerPropiedad(editor.Registro, ltrPropiedades.Terceros.Sociedad.idAgenda);
        if (Definido(idAgenda)) {
            let url = `${window.location.origin}/${ltrUrls.Entorno.VisorDeAgenda}?${ltrParametrosUrl.guid}=${generarUUID()}&${ltrParametrosUrl.idAgenda}=${idAgenda}`;
            EntornoSe.AbrirPestana(url);
        }
        else
            MensajesSe.Info('La sociedad no tiene agenda definida');

    }

    let marcado: number = 0;

    function SincronizarCheck(check: HTMLInputElement, propiedad: string) {
        let panel: HTMLDivElement = (document.getElementsByClassName(ltrCss.contenedorEdicionCuerpo) as HTMLCollectionOf<HTMLDivElement>)[0];
        let checkDeInter = ApiControl.BuscarCheck(panel, ltrPropiedades.Terceros.Sociedad.CrearInterlocutor);
        if (check.checked) {
            checkDeInter.checked = true;
            checkDeInter.disabled = true;
            marcado = marcado + 1;
        }
        else {
            let checkDeEntidad = ApiControl.BuscarCheck(panel, propiedad);
            marcado = marcado - 1;
            if (!checkDeEntidad.checked && marcado === 0)
                checkDeInter.disabled = false;
        }
    }

    export function Sociedad_CrearInterlocutor_Change(check: HTMLInputElement) {
    }

    export function Sociedad_CopiarEnRazonSocial() {
        let rz: string = Terceros.crudDeSociedad.ControlNuevaRazonSocial.value;
        let so: string = Terceros.crudDeSociedad.ControlNuevaSociedad.value;
        if (IsNullOrEmpty(rz))
            Terceros.crudDeSociedad.ControlNuevaRazonSocial.value = so;
    }

    function LlamarA_AsociarCertificado(modal: HTMLDivElement, metodo: Function) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
        parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
        let datosDeEntrada = new Array<Parametro>();
        Terceros.epAsociarCertificado(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Terceros.Sociedad.AsociarCertificado, parametros, datosDeEntrada)
            .then((peticion) => {
                metodo(modal);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function epAsociarCertificado(llamador: any, controlador: string, accion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${accion}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function Sociedad_InicializarModalParaCrearCuentas(idModal: string) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let selector = ApiControl.BuscarListaDeValores(modal, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Clase);
        MapearAlControl.ListaDeValores(selector, ltrValores.Terceros.Sociedad.CuentaBancaria.Clase.Ambas);
    }

    export function Sociedad_InicializarModalParaCrearTarjetas(idModal: string) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        //cargar lista de cuentas

        var cuentaDeCargo: HTMLSelectElement = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Terceros.Sociedad.Tarjeta.CuentaDeCargo);
        ApiControl.BlanquearListaDeElementos(cuentaDeCargo);
        var restringirPor = new Array<ClausulaDeFiltrado>();
        restringirPor.push(new ClausulaDeFiltrado(ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Activa, atCriterio.igual, true))
        MapearAlControl.ListaDeElementos(cuentaDeCargo, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
    }

    export function Sociedad_InicializarModalParaFacturador(idModal: string) {
        //const modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
    }

    export function Sociedad_RecargarGridDeArchivos() {
        ApiDeArchivos.MostrarArchivosAnexados(crudDeSociedad.crudDeEdicion.PanelDeArchivos.id, crudDeSociedad.NombreDeNegocio, crudDeSociedad.crudDeEdicion.Registro.id);
        crudDeSociedad.crudDeEdicion.RecargarGridDeTrazas();
    }

    export function Sociedad_AlPegar_Iban(event) {
        let iban: string = ValidarIbanDelPortaPapeles(event);

        let modal: HTMLDivElement = (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).ModalDeCreacionDeCuentasBancarias;
        let tbxIban = ApiControl.BuscarEditor(modal, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Iban);
        tbxIban.value = iban.substring(0, 4);

        let tbxEntidad = ApiControl.BuscarEditor(modal, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Entidad);
        tbxEntidad.value = iban.substring(4, 8);

        let tbxOficina = ApiControl.BuscarEditor(modal, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Oficina);
        tbxOficina.value = iban.substring(8, 12);

        let tbxDc = ApiControl.BuscarEditor(modal, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Dc);
        tbxDc.value = iban.substring(12, 14);

        let tbxNumero = ApiControl.BuscarEditor(modal, ltrPropiedades.Terceros.Sociedad.CuentaBancaria.Numero);
        tbxNumero.value = iban.substring(14, 24);

    };

    export function Sociedad_AlCambiar_CuentaActiva(check: HTMLInputElement) {
        let modal = (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).ModalDeEdicionDeCuentasBancarias;
        if (!check.checked) (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).AplicarCuentaNoActiva(modal);
        else (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).AplicarCuentaActiva(modal);
    }

    export function Sociedad_AlCambiar_TarjetaActiva(check: HTMLInputElement) {
        let modal = (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).ModalDeEdicionDeTarjetasBancarias;
        if (!check.checked) (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).AplicarTarjetaNoActiva(modal);
        else (crudDeSociedad.crudDeEdicion as CrudEdicionSociedad).AplicarTarjetaActiva(modal);
    }
}