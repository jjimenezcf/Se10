namespace SistemaDocumental {

    export function CrearCrudDeArchivadores(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new SistemaDocumental.CrudDeArchivadores(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);

        window.addEventListener("load", function () {
            Crud.crudMnt.Inicializar(idPanelMnt);
        }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export function SometerSincronizacion(idArchivador: number) {

        function DespuesDeSometerSincronizacion(peticion: ApiDeAjax.DescriptorAjax) {
            MensajesSe.Info("Trabajo de sincronización sometido");
        }

        let control = ApiControl.BuscarEditor(Crud.crudMnt.crudDeEdicion.PanelDeEditar, 'SincronizarCon') as HTMLInputElement;
        if (IsNullOrEmpty(control.value))
            MensajesSe.Info("Debe indicar con que carpeta de windows se ha de sincronizar");
        else {
            let parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.TrabajosSometidos.parametro.idArchivador, idArchivador));
            ApiDePeticiones.SometerTrabajo(Crud.crudMnt.crudDeEdicion, Ajax.TrabajosSometidos.trabajo.sincronizarArchivador, parametros)
                .then((peticion: ApiDeAjax.DescriptorAjax) => DespuesDeSometerSincronizacion(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
    }

    export class CrudDeArchivadores extends Crud.CrudMnt {

        private _ResumirDocumento: string = undefined;
        public get ResumirDocumento(): string { return this._ResumirDocumento; }

        private _PermiteSincronizar: boolean = undefined;
        public get PermiteSincronizar(): boolean { return this._PermiteSincronizar; }

        private _IdTipoArchivadorDeFacturaRec: number = undefined;
        public get IdTipoArchivadorDeFacturaRec(): number { return this._IdTipoArchivadorDeFacturaRec; }

        public get ModalImportarZip(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ImportarZip) as HTMLDivElement; }

        public get ModalProcesarFarConIa(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ProcesarFarConIa) as HTMLDivElement; }

        public get ModalCopiarArcivador(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Copiar) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionArchivador(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionArchivador(this, idPanelEdicion);
        }

        protected AplicarIndicadores(mapIndicadores: Map<string, any>): void {
            super.AplicarIndicadores(mapIndicadores);
            this._PermiteSincronizar = mapIndicadores.get(ltrPropiedades.SisDoc.Archivador.Indicadores.PermiteSincronizar);
            this._IdTipoArchivadorDeFacturaRec = mapIndicadores.get(ltrPropiedades.SisDoc.Archivador.Indicadores.IdTipoArchivadorDeFacturaRec);
            this._ResumirDocumento = this.crudDeEdicion.BotonIa.title;
        }

        public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
            if (opcion === ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Copiar) {
                let idModal = this.IdCrud + '-' + opcion;
                let id = this.InfoSelector.Seleccionados.length === 0 ? 0 : this.InfoSelector.Seleccionados[0].Id;
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos(idModal, id);
            }
            else
                super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;

            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);

            if (ltrMenus.eventosDeMf.SisDoc.Archivadores.IrACarpetas === opcion) {
                MensajesSe.Info("Trabajo de exportación sometido correctamente, en breve recibira un correo y se creará el archivador con la exportación");
                return true;
            }

            return SistemaDocumental.Arcdor_NavegarARelacionesDeArc(opcion, datosDeEntrada, ltrParametrosUrl.SisDoc.IdArchivador);
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            if (modal.id === this.ModalImportarZip.id) {
                let idArchivador = ObtenerPropiedad(this.crudDeEdicion.Registro, literal.id);
                let archivador = ObtenerPropiedad(this.crudDeEdicion.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.SisDoc.ImportarZip.IdArchivador, idArchivador, archivador);
            }
            else if (modal.id === this.ModalProcesarFarConIa.id) {
                let idArchivador = ObtenerPropiedad(this.crudDeEdicion.Registro, literal.id);
                let archivador = ObtenerPropiedad(this.crudDeEdicion.Registro, ltrPropiedades.Elemento.Expresion);
                MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.SisDoc.ProcesarFarConIa.IdArchivador, idArchivador, archivador);
                let carpetas = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.ProcesarFarConIa.CarpetaSeleccionada);
                ApiControl.BloquearLaLista(carpetas, false);
                ApiControl.BlanquearListaDeElementos(carpetas);
                MapearAlControl.ListaDeElementos(carpetas, new Array<ClausulaDeFiltrado>(), 0);
            }
            else if (modal.id === this.ModalCopiarArcivador.id) {
                if (this.InfoSelector.Seleccionados.length === 1)
                    this.Arcdor_ProponerDatosDelArcSeleccionado(modal, this.InfoSelector.Seleccionados[0].Registro);
            }
        }

        public Arcdor_ProponerDatosDelArcSeleccionado(modal: HTMLDivElement, registro: any) {
            let destino: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.expresion));
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
            ApiDelCrud.ProponerEnListaDinamica(modal, registro, ltrPropiedades.Elemento.ConTipo.Tipo, ltrPropiedades.Elemento.ConTipo.IdTipo);
            ApiDelCrud.ProponerEnEditor(modal, registro, ltrPropiedades.Elemento.Nombre);
            ApiDelCrud.ProponerEnAreaDeTexto(modal, registro, ltrPropiedades.Elemento.Descripcion);
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);


            if (modal.id === this.ModalCopiarArcivador.id) {
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.SisDoc.Archivadores.Copiar, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Aceptar(modal);
                        let url = `${window.location.origin}/${ltrUrls.SistemaDocumental.Archivadores}?${ltrParametrosUrl.id}=${peticion.resultado.datos}`;
                        EntornoSe.AbrirPestana(url);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else if (modal.id === this.ModalImportarZip.id)
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.SisDoc.Archivadores.ImportarZip, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            else if (modal.id === this.ModalProcesarFarConIa.id)
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.SisDoc.Archivadores.ProcesarFarConIa, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            else super.ModalDePedirDatos_Aceptar(modal);
        }
    }

    export class CrudCreacionArchivador extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            if (!EsTrue((this.CrudDeMnt as CrudDeArchivadores).PermiteSincronizar)) {
                ApiPanel.OcultarFila(this.PanelDeCrear, ltrPropiedades.SisDoc.Archivador.Sincronizar);
            }

        }
    }

    export class CrudEdicionArchivador extends Crud.CrudEdicion {
        private _crearAsociarFactura = 'Crear y asociar factura';
        private _verFactura = 'Ver factura asociada';
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        public EjecutarAcciones(accion: string, modal: HTMLDivElement) {
            switch (accion) {
                case ltrEventos.Edicion.ResumirArchivo: {
                    if (this.BotonIa.classList.contains(ltrCss.Archivador.VerFactura) ||
                        this.BotonIa.classList.contains(ltrCss.Archivador.CrearFactura))
                        this.NavegarAFacturas();
                    else
                        super.EjecutarAcciones(accion, modal);
                    break;
                }
                default: {
                    super.EjecutarAcciones(accion, modal);
                }
            }
        }

        public ComenzarEdicion(infSel: InfoSelector) {
            super.ComenzarEdicion(infSel);
            if (!EsTrue((this.CrudDeMnt as CrudDeArchivadores).PermiteSincronizar)) {
                ApiPanel.OcultarFila(this.PanelDeEditar, ltrPropiedades.SisDoc.Archivador.Sincronizar);
            }

        }

        public DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let hayCarpetas: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.SisDoc.Archivador.ConCarpetas);
            //let esUnCorreo: boolean = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.SisDoc.Archivador.EsUnCorreo);
            let referencia = ApiControl.BuscarReferencia(panel, ltrPropiedades.SisDoc.Archivador.Carpetas);
            if (!hayCarpetas) {
                if (ModoAcceso.enumModoDeAccesoDeDatos.Consultor === this.ModoDeAcceso)
                    ApiPanel.OcultarFila(this.PanelDelDto, ltrPropiedades.SisDoc.Archivador.Carpetas);
                else {
                    ApiPanel.MostrarFila(this.PanelDelDto, ltrPropiedades.SisDoc.Archivador.Carpetas);
                    referencia.innerText = ltrEtiquetas.SisDoc.Archivador.CrearCarpetas;
                }
            }
            else {
                ApiPanel.MostrarFila(this.PanelDelDto, ltrPropiedades.SisDoc.Archivador.Carpetas);
                referencia.innerText = ltrEtiquetas.SisDoc.Archivador.GestionarCarpetas;
            }

            ApiControl.BloquearAreaDeTextoSi(this.PanelDelDto, ltrPropiedades.Elemento.Descripcion, !ModoAcceso.EsGestor(this.ModoDeAcceso))

            var esAdministrador = Registro.UsuarioConectado().administrador;
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Descontabilizar, ltrMenus.enumOrigen.edicion,  esAdministrador && !this.EstaDeBaja);

            this.AyudaDelBotonIa = this.BotonIa.title;
            if (this.IdTipo > 0 && this.IdTipo === (this.CrudDeMnt as CrudDeArchivadores).IdTipoArchivadorDeFacturaRec) {
                ApiControl.IncluirCss(this.BotonIa, ltrCss.Archivador.CrearFactura);
                this.BotonIa.title = this._crearAsociarFactura;
                this.BotonIa.setAttribute(ltrPropiedades.Gasto.FacturaRec.IdFacturaRec, "0");
            }
            else {
                ApiControl.ExcluirCss(this.BotonIa, ltrCss.Archivador.CrearFactura);
                ApiControl.ExcluirCss(this.BotonIa, ltrCss.Archivador.VerFactura);
                this.BotonIa.title = (this.CrudDeMnt as CrudDeArchivadores).ResumirDocumento;
                this.BotonIa.removeAttribute(ltrPropiedades.Gasto.FacturaRec.IdFacturaRec);
            }
        }

        protected async LeerVinculosAl(idArchivo: number) {
            super.LeerVinculosAl(idArchivo);
            let parametros = `idArchivo=${idArchivo}`;
            const url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.LeerVinculosAl}?${parametros}`;

            try {
                const response = await fetch(url);

                if (!response.ok) {
                    MensajesSe.Error('LeerVinculosAl', `Error al leer vínculos: ${response.status} - ${response.statusText}`);
                    return;
                }

                const data = await response.json();

                // Aquí analizas la respuesta 'data'
                if (data && data.estado === 'Ok') {
                    // Procesar los datos si la llamada fue exitosa
                    console.log("Datos recibidos:", data.resultados); // Muestra los resultados en la consola
                    this.procesarVinculos(data.datos); // Llama a una función para procesar los vínculos

                } else {
                    MensajesSe.Error('LeerVinculosAl', "La llamada fue exitosa, pero no devolvió resultados:" + data.mensajeError);
                }

            } catch (error) {
                MensajesSe.Error('LeerVinculosAl', "Error inesperado al leer vínculos:" + error);
            }
        }

        protected procesarVinculos(vinculos: any[]) {

            if (this.IdTipo > 0 && this.IdTipo === (this.CrudDeMnt as CrudDeArchivadores).IdTipoArchivadorDeFacturaRec) {

                const incluyeFacturaRecibida = vinculos && vinculos.length > 0 && vinculos.some(vinculo => vinculo.negocio1 === enumNegocio.FacturaRecibida);

                if (incluyeFacturaRecibida) {
                    const primerVinculo = vinculos.find(vinculo => vinculo.negocio1 === enumNegocio.FacturaRecibida);
                    ApiControl.IncluirCss(this.BotonIa, ltrCss.Archivador.VerFactura);
                    this.BotonIa.title = this._verFactura;
                    this.BotonIa.setAttribute(ltrPropiedades.Gasto.FacturaRec.IdFacturaRec, primerVinculo.idElemento1.toString())
                } else {
                    ApiControl.ExcluirCss(this.BotonIa, ltrCss.Archivador.VerFactura);
                    this.BotonIa.title = this._crearAsociarFactura;
                    this.BotonIa.setAttribute(ltrPropiedades.Gasto.FacturaRec.IdFacturaRec, "0");
                }
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ImportarZip) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDeArchivadores).ModalImportarZip.id, 0);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ProcesarFarConIa) {
                this.Expansor_AbrirModalParaPedirDatos((this.CrudDeMnt as CrudDeArchivadores).ModalProcesarFarConIa.id, 0);
                return true;
            }
            if (opcion === ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Exportar) {
                MensajesSe.Info("Trabajo de exportación sometido correctamente, en breve recibira un correo y se creará el archivador con la exportación");
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Descontabilizar) {
                this.Recargar();
                return true;
            }

            return super.DespuesDeProcesarOpcionMf(peticion);
        }

        protected BloquearIu() {
            super.BloquearIu();
            ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ImportarZip, ltrMenus.enumOrigen.edicion);
            ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ProcesarFarConIa, ltrMenus.enumOrigen.edicion);
            ApiDeMenuFlotante.BloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Exportar, ltrMenus.enumOrigen.edicion);
        }

        protected DesbloquearIu() {
            super.DesbloquearIu();
            ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ImportarZip, ltrMenus.enumOrigen.edicion);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_ProcesarFarConIa, ltrMenus.enumOrigen.edicion);
            ApiDeMenuFlotante.DesbloquearOpcionDeMenu(this.ContenedorMenu, ltrMenus.eventosDeMf.SisDoc.Archivadores.Arc_Exportar, ltrMenus.enumOrigen.edicion);
        }

        private NavegarAFacturas() {
            let urlDestino: string = `${window.location.origin}/${ltrUrls.Gastos.FacturasRec}`;
            if (this.BotonIa.classList.contains(ltrCss.Archivador.VerFactura)) {

                const idFactura = Numero(this.BotonIa.getAttribute(ltrPropiedades.Gasto.FacturaRec.IdFacturaRec));
                if (idFactura > 0)
                    urlDestino = urlDestino + `?id=${idFactura}`;
                else MensajesSe.Error('NavegarAFacturas', 'No se ha localizado la factura asociada');
            }
            else {
                urlDestino = urlDestino + `?${ltrClaveDeEstado.paraqueNavegar}=${enumParaQueNavegar.crear}&idArchivo=${this.IdDelUltimoArchivoRenderizado}`;
            }
            EntornoSe.AbrirPestana(urlDestino);
        }
    }
    export function Arcdor_NavegarARelacionesDeArc(opcion: string, datosDeEntrada: Parametros, idRestrictor: string): boolean {

        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.SisDoc.Archivadores.IrACarpetas:
                urlDestino = `${window.location.origin}/${ltrUrls.SistemaDocumental.Carpetas}?${idRestrictor}=${ids[0]}`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }

    export function Arcdor_AbrirCarpetas(numeroDeFila: number) {
        let id = ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id);
        var url = `${window.location.origin}/${ltrUrls.SistemaDocumental.Carpetas}?${ltrParametrosUrl.SisDoc.IdArchivador}=${id}`;
        EntornoSe.AbrirPestana(url);
    }

    export function Arcdor_ProponerDatosDelArcSeleccionado() {
        let modal = (Crud.crudMnt as CrudDeArchivadores).ModalCopiarArcivador;
        let arc: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Selector.Elemento, true) as HTMLInputElement;
        let idArc: number = Numero(arc.getAttribute(atListasDinamicas.idSeleccionado));
        if (idArc > 0) {
            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.SisDoc.Archivador, idArc, new Array<Parametro>(), idArc)
                .then((peticion) => (Crud.crudMnt as CrudDeArchivadores).Arcdor_ProponerDatosDelArcSeleccionado(modal, peticion.resultado.datos))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
    }

    export function Arcdor_InicializarModalDeCopiado() {
        let modal = (Crud.crudMnt as CrudDeArchivadores).ModalCopiarArcivador;
        ApiPanel.BlanquearControlesDeIU(modal);
    }


    export function Arcdor_Tras_Pulsar_Enlazar_Archivos(chkEnlazar: HTMLInputElement) {
        let modal = (Crud.crudMnt as CrudDeArchivadores).ModalCopiarArcivador;
        var chkCopiar = ApiControl.BuscarCheck(modal, ltrPropiedades.SisDoc.CopiarArchivo.Copiar);
        if (chkEnlazar.checked) {
            ApiControl.BloquearCheck(chkCopiar, true, true);
        }
        else {
            ApiControl.BloquearCheck(chkCopiar, false);
        }
    }

    export function Arcdor_Tras_Pulsar_Copiar_Archivos(chkCopiar: HTMLInputElement) {
        let modal = (Crud.crudMnt as CrudDeArchivadores).ModalCopiarArcivador;
        var chkEnlazar = ApiControl.BuscarCheck(modal, ltrPropiedades.SisDoc.CopiarArchivo.Enlazar);
        if (chkCopiar.checked) {
            ApiControl.BloquearCheck(chkEnlazar, true, true);
        }
        else {
            ApiControl.BloquearCheck(chkEnlazar, false);
        }
    }


}