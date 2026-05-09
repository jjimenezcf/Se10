namespace Entorno {

    let MiCorreo: CrudDeMiCorreo;
    export function CrearCrudDeMiCorreo(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Entorno.CrudDeMiCorreo(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        MiCorreo = Crud.crudMnt as CrudDeMiCorreo;
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    let _numeroDeFila: number = 0;
    let _idMsjInterno: number = 0;
    let _idElemento: number = 0;

    let _idModalParaCrearArchivador = null;
    let _idModalParaCrearTarea = null;
    let _idModalParaCrearRegistroEs = null;
    let _idModalParaCrearFacturaRec = null;
    let _idModalParaCrearExpediente = null;
    let _idModalComoArchivar = null;
    let _idModalComoVincular = null;

    export class CrudDeMiCorreo extends Crud.CrudMnt {

        private _pulsadoAceptar = false
        public get ModalParaCrearArchivador(): HTMLDivElement { return document.getElementById(_idModalParaCrearArchivador) as HTMLDivElement; }
        public get ModalParaCrearTarea(): HTMLDivElement { return document.getElementById(_idModalParaCrearTarea) as HTMLDivElement; }
        public get ModalParaCrearRegistroEs(): HTMLDivElement { return document.getElementById(_idModalParaCrearRegistroEs) as HTMLDivElement; }
        public get ModalParaCrearFacturaRec(): HTMLDivElement { return document.getElementById(_idModalParaCrearFacturaRec) as HTMLDivElement; }
        public get ModalParaCrearExpediente(): HTMLDivElement { return document.getElementById(_idModalParaCrearExpediente) as HTMLDivElement; }
        public get ModalDeComoArchivar(): HTMLDivElement { return document.getElementById(_idModalComoArchivar) as HTMLDivElement; }
        public get ModalDeComoVincular(): HTMLDivElement { return document.getElementById(_idModalComoVincular) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionDeMiCorreo(this, idPanelCreacion);
            this.crudDeEdicion = new CrudDeEdicionDeMiCorreo(this, idPanelEdicion);
        }

        public DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            _idModalParaCrearArchivador = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.Archivar;
            _idModalParaCrearTarea = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.Tarea;
            _idModalParaCrearRegistroEs = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.RegistroEs;
            _idModalParaCrearFacturaRec = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.FacturaRec;
            _idModalParaCrearExpediente = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.Expediente;
            _idModalComoArchivar = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.ComoArchivar;
            _idModalComoVincular = Crud.crudMnt.IdCrud + '-' + ltrMenus.eventosDeMf.Entorno.MiCorreo.ComoVincular;

            var div = this.ModalDeComoVincular.querySelector(`.${ltrCss.contenidoModal}`);
            div.classList.add(ltrCss.contenidoModalAmpliado);
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);
            let propiedad: string = Ajax.Param.guid;
            let criterio: string = literal.filtro.criterio.igual;
            let valor = this.Guid;
            let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, valor.toString());
            clausulas.push(clausula);
            return clausulas;
        }

        public ModalDePedirDatos_TrasAbrir(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_TrasAbrir(modal);
            this._pulsadoAceptar = false;
            if (modal.id === this.ModalDeComoArchivar.id || modal.id === this.ModalDeComoVincular.id) {
                this.AjustarModalesDeComoAdjuntar();
                return;
            }
            if (modal.id === this.ModalParaCrearArchivador.id) {
                let id = Numero(this.ModalParaCrearArchivador.getAttribute(atModal.idElemento1));
                ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, new Array<Parametro>(), this.ModalParaCrearArchivador)
                    .then((peticion) => this.MapearDatosDelCorreoParaCrear(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
            if (modal.id === this.ModalParaCrearTarea.id) {
                let id = Numero(this.ModalParaCrearTarea.getAttribute(atModal.idElemento1));
                ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, new Array<Parametro>(), this.ModalParaCrearTarea)
                    .then((peticion) => this.MapearDatosDelCorreoParaCrear(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
            if (modal.id === this.ModalParaCrearRegistroEs.id) {
                let id = Numero(this.ModalParaCrearRegistroEs.getAttribute(atModal.idElemento1));
                ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, new Array<Parametro>(), this.ModalParaCrearRegistroEs)
                    .then((peticion) => this.MapearDatosDelCorreoParaCrear(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
            if (modal.id === this.ModalParaCrearFacturaRec.id) {
                let id = Numero(this.ModalParaCrearFacturaRec.getAttribute(atModal.idElemento1));
                ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, new Array<Parametro>(), this.ModalParaCrearFacturaRec)
                    .then((peticion) => this.MapearDatosDelCorreoParaCrear(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
            if (modal.id === this.ModalParaCrearExpediente.id) {
                let id = Numero(this.ModalParaCrearExpediente.getAttribute(atModal.idElemento1));
                ApiDePeticiones.LeerElementoPorId(this, this.Controlador, id, new Array<Parametro>(), this.ModalParaCrearExpediente)
                    .then((peticion) => this.MapearDatosDelCorreoParaCrear(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                return;
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {

            this._pulsadoAceptar = true;

            if (modal.id === this.ModalDeComoArchivar.id) {
                let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(this.Tabla, _numeroDeFila, literal.id));
                Entorno.MiCorreo_AsociarEnArchivador(id);
                return;
            }
            if (modal.id === this.ModalDeComoVincular.id) {
                let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(this.Tabla, _numeroDeFila, literal.id));
                Entorno.MiCorreo_Vincular(id);
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);
            parametros = parametros.filter(parametro => parametro.parametro !== ltrPropiedades.Elemento.Descripcion);
            parametros.push(new Parametro(ltrPropiedades.Entorno.MiCorreo.Id, _idMsjInterno));
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            if (modal.id === this.ModalParaCrearArchivador.id) {
                parametros.push(new Parametro(literal.enumNegocio, enumNegocio.Archivador));
            }
            if (modal.id === this.ModalParaCrearTarea.id) {
                parametros.push(new Parametro(literal.enumNegocio, enumNegocio.Tarea));
                const registro = ApiControl.BuscarListaDinamicaPorPropiedad(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.RegistroEs);
                const idRegistro = Numero(registro.getAttribute(atListasDinamicas.idSeleccionado));
                if (idRegistro > 0) {
                    const objetoRegistro = OpcionesDeLasListas.ObtenerObjeto(registro);
                    parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdRegistroEs, objetoRegistro.id));
                }
            }
            if (modal.id === this.ModalParaCrearRegistroEs.id) {
                parametros.push(new Parametro(literal.enumNegocio, enumNegocio.Registro));
            }
            if (modal.id === this.ModalParaCrearFacturaRec.id) {
                parametros.push(new Parametro(literal.enumNegocio, enumNegocio.FacturaRecibida));
            }
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Entorno.MiCorreo.Archivar, parametros, datosDeEntrada)
                .then((peticion) => {
                    super.ModalDePedirDatos_Cerrar(modal);
                    this.RestaurarPagina();
                    var ctrlAcc = modal.id === this.ModalParaCrearArchivador.id
                        ? ltrUrls.SistemaDocumental.Archivadores
                        : modal.id === this.ModalParaCrearTarea.id
                            ? ltrUrls.Administracion.Tareas
                            : modal.id === this.ModalParaCrearRegistroEs.id
                                ? ltrUrls.Administracion.RegistroEs
                                : modal.id === this.ModalParaCrearFacturaRec.id
                                    ? ltrUrls.Gastos.FacturasRec
                                    : ltrUrls.Administracion.Expediente
                    var idRegistro = peticion.resultado.datos.id;
                    var url = `${window.location.origin}/${ctrlAcc}?${ltrParametrosUrl.id}=${idRegistro}`;
                    EntornoSe.AbrirPestana(url);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
        }

        public ModalDePedirDatos_Cerrar(modal: HTMLDivElement): void {
            super.ModalDePedirDatos_Cerrar(modal);
            if (modal.id !== _idModalComoArchivar && modal.id !== _idModalComoVincular && !this._pulsadoAceptar) {
                if (modal.id === _idModalParaCrearArchivador) {
                    MiCorreo_ComoArchivar(_numeroDeFila)
                }
                else {
                    MiCorreo_ComoVincular(_numeroDeFila)
                }
            }
            this._pulsadoAceptar = false;
            _numeroDeFila = 0;
            _idMsjInterno = 0;
            //_idElemento = 0;
        }

        private MapearDatosDelCorreoParaCrear(peticion: ApiDeAjax.DescriptorAjax) {
            var crudMnt = peticion.llamador as CrudDeMiCorreo;
            var modal = peticion.DatosDeEntrada as HTMLDivElement;

            _idMsjInterno = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.Id);
            let idMensaje: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.IdMensaje);
            let asunto = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.Asunto);
            let cuerpo = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.Cuerpo);
            let conAdjuntos = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.ConAdjuntos);
            let emisor = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.Emisor);
            let fecha = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.Fecha);

            let match = emisor.match(/<(.*?)>/);
            let nombre = `Correo incorporado de: ${((match && match.length > 1) ? match[1] : emisor)} `;
            ApiControl.MapearEditor(modal, ltrPropiedades.Elemento.Nombre, nombre);


            let adjuntos = JSON.parse(ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Entorno.MiCorreo.Adjuntos));
            let fileNames: string = Definido(adjuntos) ? adjuntos.map(e => e.Fichero).join(',') : '';
            var textoCuerpo = `Enviado por: ${emisor}\nAsunto: ${asunto}\n\nCuerpo\n${(IsNullOrEmpty(cuerpo) ? '(sin cuerpo)' : cuerpo)}\n\nAdjuntos: ${fileNames} \n\nFecha de recepción: ${fecha} \nIdentificador: ${idMensaje}`
            var control = ApiControl.MapearAreaDeTexto(modal, ltrPropiedades.Elemento.Descripcion, textoCuerpo);
            ApiControl.BloquearAreaDeTexto(control);

            const expediente = ApiControl.BuscarListaDinamicaPorPropiedad(crudMnt.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.Expediente);
            const idExpediente = Numero(expediente.getAttribute(atListasDinamicas.idSeleccionado));
            if (idExpediente > 0) {
                const objetoExpediente = OpcionesDeLasListas.ObtenerObjeto(expediente);
                MapearAlControl.RestrictoresDeEdicion(this.ModalParaCrearTarea, ltrPropiedades.Tarea.IdExpediente, idExpediente, expediente.value);
                ApiDelCrud.ProponerEnListaDinamica(modal, objetoExpediente, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
            } else {
                const registro = ApiControl.BuscarListaDinamicaPorPropiedad(crudMnt.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.RegistroEs);
                const idRegistro = Numero(registro.getAttribute(atListasDinamicas.idSeleccionado));
                if (idRegistro > 0) {
                    const objetoRegistro = OpcionesDeLasListas.ObtenerObjeto(registro);
                    ApiDelCrud.ProponerEnListaDinamica(modal, objetoRegistro, ltrPropiedades.Elemento.ConCg.Cg, ltrPropiedades.Elemento.ConCg.IdCg);
                }
            }

        }

        private AjustarModalesDeComoAdjuntar() {
            let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(this.Tabla, _numeroDeFila, literal.id));
            var referencia;
            ApiControl.BloquearListaDeElemento(this.ModalDeComoArchivar, ltrPropiedades.SisDoc.ComoArchivar.CarpetaDestino);
            ApiControl.BloquearListaDeElemento(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.ArchivadorDestino);
            ApiControl.BloquearListaDeElemento(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.TareaDestino);
            ApiControl.BloquearListaDeElemento(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CarpetaDestino);
            var aSustituir = _idElemento === 0 ? atModal.idElemento : _idElemento.toString();
            referencia = ApiControl.BuscarReferencia(this.ModalDeComoArchivar, ltrPropiedades.SisDoc.ComoArchivar.CrearArchivador);
            referencia.href = remplazar(referencia.href, aSustituir, id.toString());
            referencia = ApiControl.BuscarReferencia(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearTarea);
            referencia.href = remplazar(referencia.href, aSustituir, id.toString());
            referencia = ApiControl.BuscarReferencia(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearRegistroEs);
            referencia.href = remplazar(referencia.href, aSustituir, id.toString());
            referencia = ApiControl.BuscarReferencia(this.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearFacturaRec);
            referencia.href = remplazar(referencia.href, aSustituir, id.toString());
            //referencia = ApiControl.BuscarReferencia(modal, ltrPropiedades.SisDoc.ComoVincular.CrearExpediente);
            //referencia.href = remplazar(referencia.href, atModal.idElemento, id.toString());
            _idElemento = id;
        }
    }

    export class CrudDeEdicionDeMiCorreo extends Crud.CrudEdicion {

        public Adjuntos: any;
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            this.Adjuntos = undefined;
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            var divDeAdjuntos = ApiPanel.InicializarDivEscrolable(this.PanelDelDto, 'enlaces_adjuntos');
            var adjuntos = ObtenerPropiedad(peticion.resultado.datos, 'adjuntos');
            const resultado = EsJsonValido(adjuntos);
            if (resultado.esValido)
                this.crearEnlacesAdjuntos(resultado.json, divDeAdjuntos)
        }

        private crearEnlacesAdjuntos(adjuntosJson: any, div: HTMLDivElement): void {
            if (!Definido(adjuntosJson)) return;

            const ul = document.createElement("ul");
            this.Adjuntos = Array.isArray(adjuntosJson) ? adjuntosJson : Object.values(adjuntosJson);

            for (const adjunto of this.Adjuntos) {
                if (typeof adjunto === 'object' && adjunto !== null) {
                    const li = document.createElement("li");
                    const a = document.createElement("a");
                    a.href = "#";
                    a.textContent = adjunto.Fichero || 'Adjunto sin nombre';
                    a.addEventListener("click", () => this.DescargarAdjunto(adjunto.IdMail, adjunto.IdAdjunto, adjunto.IdParte));
                    li.appendChild(a);
                    ul.appendChild(li);
                }
            }

            if (ul.childNodes.length > 0) {
                div.appendChild(ul);
            }
        }


        private DescargarAdjunto(idMail: string, idAdjunto: string, idParte: string): void {
            idMail = Encriptar(literal.ClaveDeEncriptacion, idMail);
            idAdjunto = Encriptar(literal.ClaveDeEncriptacion, idAdjunto);
            idParte = Encriptar(literal.ClaveDeEncriptacion, idParte);
            let parametros = `${Ajax.Param.idMail}=${idMail}&${Ajax.Param.idAdjunto}=${idAdjunto}&${Ajax.Param.idParte}=${idParte}`;
            let descargar: string = `/${Ajax.Entorno.MiCorreo.controlador}/${Ajax.EndPoint.Entorno.MiCorreo.DescargarAdjunto}?${parametros}`;
            try {
                EntornoSe.AbrirPestana(descargar, false);
            }
            finally {
                MensajesSe.Info("Descarga realizada")
            };
        }
    }
    export class CrudCreacionDeMiCorreo extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

    }

    export function MiCorreo_ComoArchivar(numeroDeFila: number) {
        var crudMnt = Crud.crudMnt;
        let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(crudMnt.Tabla, numeroDeFila, literal.id));
        _numeroDeFila = numeroDeFila;
        ApiDelCrud.ModalParaPedirDatos_Abrir(_idModalComoArchivar, id);
    }
    export function MiCorreo_ComoVincular(numeroDeFila: number) {
        var crudMnt = Crud.crudMnt;
        let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(crudMnt.Tabla, numeroDeFila, literal.id));
        _numeroDeFila = numeroDeFila;
        ApiDelCrud.ModalParaPedirDatos_Abrir(_idModalComoVincular, id);
    }

    export function MiCorreo_CrearArchivador(idMensaje: number) {
        var crudMnt = Crud.crudMnt;
        (crudMnt as CrudDeMiCorreo).ModalDePedirDatos_Cerrar((crudMnt as CrudDeMiCorreo).ModalDeComoArchivar);
        (crudMnt as CrudDeMiCorreo).crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(_idModalParaCrearArchivador, idMensaje);
    }

    export function MiCorreo_CrearTarea(idMensaje: number) {
        var crudMnt = Crud.crudMnt;
        (crudMnt as CrudDeMiCorreo).ModalDePedirDatos_Cerrar((crudMnt as CrudDeMiCorreo).ModalDeComoVincular);
        (crudMnt as CrudDeMiCorreo).crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(_idModalParaCrearTarea, idMensaje);
        BloquearRestoDeSelectores(undefined);
    }

    export function MiCorreo_CrearRegistroEs(idMensaje: number) {
        var crudMnt = Crud.crudMnt;
        (crudMnt as CrudDeMiCorreo).ModalDePedirDatos_Cerrar((crudMnt as CrudDeMiCorreo).ModalDeComoVincular);
        (crudMnt as CrudDeMiCorreo).crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(_idModalParaCrearRegistroEs, idMensaje);
        BloquearRestoDeSelectores(undefined);
    }

    export function MiCorreo_CrearFacturaRec(idMensaje: number) {
        var crudMnt = Crud.crudMnt;
        (crudMnt as CrudDeMiCorreo).ModalDePedirDatos_Cerrar((crudMnt as CrudDeMiCorreo).ModalDeComoVincular);
        (crudMnt as CrudDeMiCorreo).crudDeEdicion.Expansor_AbrirModalDeCrearVinculoCon(_idModalParaCrearFacturaRec, idMensaje);
        BloquearRestoDeSelectores(undefined);
    }


    export function MiCorreo_AsociarEnArchivador(idMensaje: number) {
        var crudMnt = Crud.crudMnt;
        var modal = document.getElementById(_idModalComoArchivar) as HTMLDivElement;
        var archivador = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.ComoArchivar.Archivador);
        var idArchivador = Numero(archivador.getAttribute(atListasDinamicas.idSeleccionado));
        let carpetas = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.ComoArchivar.CarpetaDestino);
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Entorno.MiCorreo.Id, idMensaje));
        parametros.push(new Parametro(literal.enumNegocio, enumNegocio.Archivador));
        parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoArchivar.IdArchivador, idArchivador));
        parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoArchivar.IdCarpeta, Definido(carpetas.selectedOptions) ? Numero(carpetas.selectedOptions[0].value) : 0));
        let datosDeEntrada = new Array<Parametro>();
        ApiDePeticiones.EjecutarPeticion(this, Ajax.Entorno.MiCorreo.controlador, Ajax.EndPoint.Entorno.MiCorreo.AsociarAlElemento, parametros, datosDeEntrada)
            .then(() => {
                var modal = MiCorreo.ModalDeComoArchivar;
                var abrir = ApiControl.BuscarCheck(modal, ltrPropiedades.SisDoc.ComoArchivar.AbrirAlAsociar);
                MiCorreo.ModalDePedirDatos_Cerrar((crudMnt as CrudDeMiCorreo).ModalDeComoArchivar);
                MiCorreo.RestaurarPagina();
                if (abrir.checked) {
                    var url = '';
                    var ctrlAcc = ltrUrls.SistemaDocumental.Archivadores
                    var url = `${window.location.origin}/${ctrlAcc}?${ltrParametrosUrl.id}=${idArchivador}`;
                    EntornoSe.AbrirPestana(url);
                }
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function MiCorreo_Vincular(idMensaje: number) {
        var modal = document.getElementById(_idModalComoVincular) as HTMLDivElement;

        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Entorno.MiCorreo.Id, idMensaje));

        var tarea = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.ComoVincular.Tarea);
        var idTarea = Numero(tarea.getAttribute(atListasDinamicas.idSeleccionado));
        var registroEs = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.ComoVincular.RegistroEs);
        var idregistroEs = Numero(registroEs.getAttribute(atListasDinamicas.idSeleccionado));
        var factura = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.ComoVincular.FacturaRec);
        var idFactura = Numero(factura.getAttribute(atListasDinamicas.idSeleccionado));
        var expediente = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.ComoVincular.Expediente);
        var idExpediente = Numero(expediente.getAttribute(atListasDinamicas.idSeleccionado));

        var idArchivadoDestino = Numero(ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.ComoVincular.ArchivadorDestino).value);
        var idTareaDestino = Numero(ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.ComoVincular.TareaDestino).value);
        var idCarpetaDestino = Numero(ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.ComoVincular.CarpetaDestino).value);
        parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdTareaDestino, idTareaDestino));
        parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdArchivadorDestino, idArchivadoDestino));
        parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdCarpetaDestino, idCarpetaDestino));

        if (idTarea === 0 && idregistroEs === 0 && idFactura === 0 && idExpediente === 0)
            MensajesSe.EmitirExcepcion('MiCorreo_Vincular', 'Debe indicar un elemento al que vincular el mensaje');

        const variables = [idTarea, idregistroEs, idFactura, idExpediente];
        var soloUnaConValor = variables.some(variable => variable > 0) && variables.filter(variable => variable > 0).length === 1;
        if (!soloUnaConValor)
            MensajesSe.EmitirExcepcion('MiCorreo_Vincular', 'Debe indicar solo un elemento al que asociar el mensaje');

        if (idTarea > 0) {
            parametros.push(new Parametro(literal.enumNegocio, enumNegocio.Tarea));
            parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdTarea, idTarea));
            MiCorreo_AsociarAlElemento(parametros, enumNegocio.Tarea, idTarea);
        }
        else if (idFactura > 0) {
            RemplazarParametro(parametros, literal.enumNegocio, enumNegocio.FacturaRecibida);
            parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdFacturaRec, idFactura));
            MiCorreo_AsociarAlElemento(parametros, enumNegocio.FacturaRecibida, idFactura);
        }
        else if (idExpediente > 0) {
            RemplazarParametro(parametros, literal.enumNegocio, enumNegocio.Expediente);
            parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdExpediente, idExpediente));
            MiCorreo_AsociarAlElemento(parametros, enumNegocio.Expediente, idExpediente);
        }
        else {
            RemplazarParametro(parametros, literal.enumNegocio, enumNegocio.Registro);
            parametros.push(new Parametro(ltrPropiedades.SisDoc.ComoVincular.IdRegistroEs, idregistroEs));
            MiCorreo_AsociarAlElemento(parametros, enumNegocio.Registro, idregistroEs);
        }
    }

    export function MiCorreo_AsociarAlElemento(parametros: Array<Parametro>, negocio: string, idRegistro: number) {
        var crudMnt = Crud.crudMnt;
        let datosDeEntrada = new Array<Parametro>();
        ApiDePeticiones.EjecutarPeticion(this, Ajax.Entorno.MiCorreo.controlador, Ajax.EndPoint.Entorno.MiCorreo.AsociarAlElemento, parametros, datosDeEntrada)
            .then(() => {
                var modal = (crudMnt as CrudDeMiCorreo).ModalDeComoVincular;
                var abrir = ApiControl.BuscarCheck(modal, ltrPropiedades.SisDoc.ComoVincular.AbrirAlAsociar);
                MiCorreo.ModalDePedirDatos_Cerrar((crudMnt as CrudDeMiCorreo).ModalDeComoVincular);
                MiCorreo.RestaurarPagina();
                if (abrir.checked) {
                    var url = '';
                    var ctrlAcc = negocio === enumNegocio.Tarea
                        ? ltrUrls.Administracion.Tareas
                        : negocio === enumNegocio.Registro
                            ? ltrUrls.Administracion.RegistroEs
                            : negocio === enumNegocio.FacturaRecibida
                                ? ltrUrls.Gastos.FacturasRec
                                : ltrUrls.Administracion.Expediente
                    var url = `${window.location.origin}/${ctrlAcc}?${ltrParametrosUrl.id}=${idRegistro}`;
                    EntornoSe.AbrirPestana(url);
                }
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function MiCorreo_Eliminar(numeroDeFila: number) {
        let id = ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Entorno.MiCorreo.Id);
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Entorno.MiCorreo.Id, id));
        let datosDeEntrada = new Array<Parametro>();
        ApiDePeticiones.EjecutarPeticion(this, Ajax.Entorno.MiCorreo.controlador, Ajax.EndPoint.Entorno.MiCorreo.EliminarCorreo, parametros, datosDeEntrada)
            .then(() => {
                (Crud.crudMnt as CrudDeMiCorreo).RestaurarPagina();
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function MiCorreo_Imprimir(numeroDeFila: number) {
        let id = ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Entorno.MiCorreo.IdMensaje);
        id = encodeURIComponent(id);
        let buzon = ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Entorno.MiCorreo.Buzon);
        let parametros = `${Ajax.Param.MiCorreo.Buzon}=${Encriptar(literal.ClaveDeEncriptacion, buzon)}&${Encriptar(literal.ClaveDeEncriptacion, Ajax.Param.idMail)}=${id}`;
        let descargar: string = `/${Ajax.Entorno.MiCorreo.controlador}/${Ajax.EndPoint.Entorno.MiCorreo.ImprimirCorreo}?${parametros}`;
        try {
            EntornoSe.AbrirPestana(descargar, false);
        }
        finally {
            MensajesSe.Info("Descarga realizada")
        };
    }

    export function MiCorreo_Tras_Blanquear_Archivador(idLista: string): void {
        var lista = document.getElementById(idLista) as HTMLInputElement;
        var modal = ApiPanel.BuscarModalContenedora(lista);
        let carpetas = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.ComoArchivar.CarpetaDestino);
        ApiControl.BlanquearListaDeElementos(carpetas);
        ApiControl.BloquearLaLista(carpetas, true);

        var ref = ApiControl.BuscarReferencia(modal, ltrPropiedades.SisDoc.ComoArchivar.CrearArchivador);
        //ApiControl.OcultarHtmlAnchor(ref, false);
    }

    export function MiCorreo_Tras_Seleccionar_Archivador(idLista: string): void {
        var ref = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoArchivar, ltrPropiedades.SisDoc.ComoArchivar.CrearArchivador);
        let carpetas = ApiControl.BuscarListaDeElementos(MiCorreo.ModalDeComoArchivar, ltrPropiedades.SisDoc.ComoArchivar.CarpetaDestino);
        //ApiControl.OcultarHtmlAnchor(ref, true);
        ApiControl.BloquearLaLista(carpetas, false);
        ApiControl.BlanquearListaDeElementos(carpetas);
        MapearAlControl.ListaDeElementos(carpetas, new Array<ClausulaDeFiltrado>(), 0);
    }

    export function MiCorreo_Tras_Blanquear_Tarea(idLista: string): void {
        BlanquearBloquearArchivadorDestino();
        BlanquearBloquearCarpetaDestino();
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Seleccionar_Tarea(idLista: string): void {
        var tarea = document.getElementById(idLista) as HTMLInputElement;
        var idTarea = Numero(tarea.getAttribute(atListasDinamicas.idSeleccionado));
        var archivador = BlanquearBloquearArchivadorDestino(false);
        var restringirPor = new Array<ClausulaDeFiltrado>();
        restringirPor.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.Tarea}${ltrSimbolos.separadorDeValores}${idTarea}`))
        MapearAlControl.ListaDeElementos(archivador, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Blanquear_RegistroEs(idLista: string): void {
        BlanquearBloquearTareaDestino();
        BlanquearBloquearArchivadorDestino();
        BlanquearBloquearCarpetaDestino();
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Seleccionar_RegistroEs(idLista: string): void {
        var registroEs = document.getElementById(idLista) as HTMLInputElement;
        var idRegistroEs = Numero(registroEs.getAttribute(atListasDinamicas.idSeleccionado));
        var restringirPor = new Array<ClausulaDeFiltrado>();
        restringirPor.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.Registro}${ltrSimbolos.separadorDeValores}${idRegistroEs}`))

        var tareaDestino = BlanquearBloquearTareaDestino(false);
        MapearAlControl.ListaDeElementos(tareaDestino, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);

        var archivadorDestino = BlanquearBloquearArchivadorDestino(false);
        MapearAlControl.ListaDeElementos(archivadorDestino, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Blanquear_FacturaRec(idLista: string): void {
        BlanquearBloquearArchivadorDestino();
        BlanquearBloquearCarpetaDestino();
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Seleccionar_Expediente(idLista: string): void {
        var expediente = document.getElementById(idLista) as HTMLInputElement;
        var idExpediente = Numero(expediente.getAttribute(atListasDinamicas.idSeleccionado));
        var restringirPor = new Array<ClausulaDeFiltrado>();
        restringirPor.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.Expediente}${ltrSimbolos.separadorDeValores}${idExpediente}`))

        var tareaDestino = BlanquearBloquearTareaDestino(false);
        MapearAlControl.ListaDeElementos(tareaDestino, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);

        var archivadorDestino = BlanquearBloquearArchivadorDestino(false);
        MapearAlControl.ListaDeElementos(archivadorDestino, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Blanquear_Expediente(idLista: string): void {
        BlanquearBloquearTareaDestino();
        BlanquearBloquearArchivadorDestino();
        BlanquearBloquearCarpetaDestino();
        BloquearRestoDeSelectores(document.getElementById(idLista) as HTMLInputElement);
    }

    export function MiCorreo_Tras_Seleccionar_FacturaRec(idLista: string): void {
        var factura = document.getElementById(idLista) as HTMLInputElement;
        var idFactura = Numero(factura.getAttribute(atListasDinamicas.idSeleccionado));
        var archivador = BlanquearBloquearArchivadorDestino(false);
        var restringirPor = new Array<ClausulaDeFiltrado>();
        restringirPor.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.FacturaRecibida}${ltrSimbolos.separadorDeValores}${idFactura}`))
        MapearAlControl.ListaDeElementos(archivador, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        BloquearRestoDeSelectores(factura);
    }

    export function MiCorreo_Tras_Seleccionar_ArchivadorDestino(selector: HTMLSelectElement) {
        if (!(selector instanceof HTMLSelectElement))
            return;
        var carpeta: HTMLSelectElement = ApiControl.BuscarListaDeElementos(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CarpetaDestino);
        var tareaDestino: HTMLSelectElement = ApiControl.BuscarListaDeElementos(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.TareaDestino);
        var idRegistro = Numero(ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.RegistroEs).getAttribute(atListasDinamicas.idSeleccionado));
        var idExpediente = Numero(ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.Expediente).getAttribute(atListasDinamicas.idSeleccionado));
        var idArchivador = Numero(selector.value);
        ApiControl.BlanquearListaDeElementos(carpeta);
        ApiControl.BloquearLaLista(carpeta, idArchivador === 0);

        BlanquearBloquearCarpetaDestino(idArchivador === 0)

        ApiControl.BloquearLaLista(tareaDestino, (idArchivador > 0 && Numero(tareaDestino.value) === 0) || (idRegistro === 0 && idExpediente == 0));
        if (idArchivador > 0) {
            ApiControl.BloquearLaLista(carpeta, false);
            var restringirPor = new Array<ClausulaDeFiltrado>();
            restringirPor.push(new ClausulaDeFiltrado(Ajax.Param.idElemento, atCriterio.igual, idArchivador))
            MapearAlControl.ListaDeElementos(carpeta, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
            //tarea.selectedIndex = 0;
        }
    }

    export function MiCorreo_Tras_Seleccionar_TareaDestino(selector: HTMLSelectElement) {
        if (!(selector instanceof HTMLSelectElement))
            return;
        var archivador = BlanquearBloquearArchivadorDestino(false);
        var idTareaDestino = Numero(selector.value);
        if (idTareaDestino > 0) {
            var restringirPor = new Array<ClausulaDeFiltrado>();
            restringirPor.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.Tarea}${ltrSimbolos.separadorDeValores}${idTareaDestino}`))
            MapearAlControl.ListaDeElementos(archivador, new Array<ClausulaDeFiltrado>(), 0, null, restringirPor);
        }
        else {
            var expediente = ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.Expediente) as HTMLInputElement;
            var idExpediente = Numero(expediente.getAttribute(atListasDinamicas.idSeleccionado));

            var restringirPorSeleccionado = new Array<ClausulaDeFiltrado>();
            if (idExpediente > 0) {
                restringirPorSeleccionado.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.Expediente}${ltrSimbolos.separadorDeValores}${idExpediente}`))
                MapearAlControl.ListaDeElementos(archivador, new Array<ClausulaDeFiltrado>(), 0, null, restringirPorSeleccionado);
            }
            else {
                var registroEs = ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.RegistroEs) as HTMLInputElement;
                var idRegistroEs = Numero(registroEs.getAttribute(atListasDinamicas.idSeleccionado));
                restringirPorSeleccionado.push(new ClausulaDeFiltrado(Ajax.Param.VinculadosA, atCriterio.igual, `${enumNegocio.Registro}${ltrSimbolos.separadorDeValores}${idRegistroEs}`))
                MapearAlControl.ListaDeElementos(archivador, new Array<ClausulaDeFiltrado>(), 0, null, restringirPorSeleccionado);
            }
        }
    }

    function BlanquearBloquearTareaDestino(criterio: boolean = true): HTMLSelectElement {
        var tarea: HTMLSelectElement = ApiControl.BuscarListaDeElementos(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.TareaDestino);
        ApiControl.BlanquearListaDeElementos(tarea);
        ApiControl.BloquearLaLista(tarea, criterio);
        return tarea;
    }

    function BlanquearBloquearArchivadorDestino(criterio: boolean = true): HTMLSelectElement {
        var archivador: HTMLSelectElement = ApiControl.BuscarListaDeElementos(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.ArchivadorDestino);
        ApiControl.BlanquearListaDeElementos(archivador);
        ApiControl.BloquearLaLista(archivador, criterio);
        return archivador;
    }

    function BlanquearBloquearCarpetaDestino(criterio: boolean = true) {
        var carpeta: HTMLSelectElement = ApiControl.BuscarListaDeElementos(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CarpetaDestino);
        ApiControl.BlanquearListaDeElementos(carpeta);
        ApiControl.BloquearLaLista(carpeta, criterio);
    }

    function BloquearRestoDeSelectores(selector: HTMLInputElement) {
        var tarea = ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.Tarea);
        var registro = ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.RegistroEs);
        var factura = ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.FacturaRec);
        var expediente = ApiControl.BuscarListaDinamicaPorPropiedad(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.Expediente);
        if (!Definido(selector)) {
            DesbloquearListaDinamicas(tarea, registro, factura, expediente);
            return;
        }
        var id = Numero(selector.getAttribute(atListasDinamicas.idSeleccionado));
        if (id > 0) {
            if (selector.id !== tarea.id) ApiListaDinamica.BloquearBlanquear(tarea);
            if (selector.id !== registro.id) ApiListaDinamica.BloquearBlanquear(registro);
            if (selector.id !== factura.id) ApiListaDinamica.BloquearBlanquear(factura);
            if (selector.id !== expediente.id) ApiListaDinamica.BloquearBlanquear(expediente);


            const refFac = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearFacturaRec);
            ApiControl.IncluirCss(refFac.parentElement, ltrCss.divNoVisible);
            const refRec = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearRegistroEs);
            ApiControl.IncluirCss(refRec.parentElement, ltrCss.divNoVisible);

            if (selector.id !== expediente.id) {
                const refTar = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearTarea);
                ApiControl.IncluirCss(refTar.parentElement, ltrCss.divNoVisible);
            }


            return;
        }
        DesbloquearListaDinamicas(tarea, registro, factura, expediente);
    }

    function DesbloquearListaDinamicas(tarea: HTMLInputElement, registro: HTMLInputElement, factura: HTMLInputElement, expediente: HTMLInputElement) {
        ApiControl.DesbloquearListaDinamica(tarea);
        ApiControl.DesbloquearListaDinamica(registro);
        ApiControl.DesbloquearListaDinamica(factura);
        ApiControl.DesbloquearListaDinamica(expediente);

        const refFac = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearFacturaRec);
        ApiControl.ExcluirCss(refFac.parentElement, ltrCss.divNoVisible);
        const refRec = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearRegistroEs);
        ApiControl.ExcluirCss(refRec.parentElement, ltrCss.divNoVisible);
        const refTar = ApiControl.BuscarReferencia(MiCorreo.ModalDeComoVincular, ltrPropiedades.SisDoc.ComoVincular.CrearTarea);
        ApiControl.ExcluirCss(refTar.parentElement, ltrCss.divNoVisible);
    }

    export function ExpandirContenido() {
        let htmlContent = ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Entorno.MiCorreo.CuerpoHtml);
        htmlContent = reemplazarImagenesEnHtml(htmlContent, (Crud.crudMnt.crudDeEdicion as CrudDeEdicionDeMiCorreo).Adjuntos);
        const newWindow = window.open('', '_blank');
        if (newWindow) {
            newWindow.document.write(htmlContent);
            newWindow.document.close();
        } else {
            alert('El navegador ha bloqueado la apertura de la ventana emergente. Por favor, permite las ventanas emergentes para este sitio.');
        }
    }

    function reemplazarImagenesEnHtml(htmlContent, adjuntos) {

        if (!Definido(adjuntos))
            return;

        // 1. Parsear el contenido HTML a un documento DOM
        const parser = new DOMParser();
        const doc = parser.parseFromString(htmlContent, 'text/html');

        // 2. Obtener todas las etiquetas <img> del documento
        const imagenes = doc.querySelectorAll('img');

        const dominio = window.location.hostname;
        const puerto = window.location.port;
        const protocolo = window.location.protocol;

        // 3. Iterar sobre las imágenes y reemplazar los valores de src si corresponde
        for (const img of imagenes) {
            const src = img.getAttribute('src'); // Obtener el atributo src de la imagen

            // Verificar si el src comienza con "cid:"
            if (src && src.startsWith('cid:')) {
                const cid = src.substring(4); // Extraer el CID (eliminando "cid:")

                // Buscar el adjunto correspondiente en la lista de adjuntos
                const adjunto = adjuntos.find(adj => adj.IdAdjunto === `<${cid}>`);

                if (adjunto) {

                    const urlDeDescarga = `${protocolo}//${dominio}${(IsNullOrEmpty(puerto) ? "" : `:${puerto}`)}/${ltrControladores.Entorno.MiCorreo}/${Ajax.Entorno.MiCorreo.DescargarAdjunto}?idMail=${adjunto.IdMail}&idAdjunto=${encodeURIComponent(adjunto.IdAdjunto)}&idParte=${adjunto.IdParte}`;

                    // Reemplazar el atributo src de la imagen
                    img.setAttribute('src', urlDeDescarga);
                }
            }
        }

        // 4. Serializar nuevamente el contenido HTML modificado
        return doc.body.innerHTML;
    }

}



