namespace Administracion {

    enum enumEtapasDeExpedientes {
        EXP_Etapa_Asociar_Tareas,
        EXP_Etapa_Asociar_Presupuestos,
        EXP_Etapa_Asociar_SC_Compra,
        EXP_Etapa_Asociar_SC_Venta,
        EXP_Etapa_Ejecucion,
        EXP_Etapa_Terminada,
        EXP_Etapa_Cancelada
    }

    enum enumClaseDeExpediente {
        administrativo = 'administrativo',
        juridico = 'juridico',
        solicitudContrato = 'solicitudContrato',
        DeCliente = 'DeCliente',
        ConValoracion = 'ConValoracion',
        NoDefinido = 'NoDefinido'
    }


    export function CrearCrudDeExpedientes(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string, claseDeExpediente: string) {
        Crud.crudMnt = new Administracion.CrudDeExpedientes(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar, claseDeExpediente);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDeExpedientes extends Crud.CrudMnt {
        private _clase: enumClaseDeExpediente;
        public get ClaseDeExpediente(): enumClaseDeExpediente {
            return this._clase;
        }

        public TiposDeProcedimientos: Array<Parametro>;


        public get ModalImputarFacturas(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Juridico.Contratos.ImputarFacturas) as HTMLDivElement; }
        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string, claseDeExpediente: string) {
            super(idPanelMnt, idModalBorrar);
            const esValido = Object.values(enumClaseDeExpediente).includes(claseDeExpediente as enumClaseDeExpediente);
            this._clase = esValido
                ? (claseDeExpediente as enumClaseDeExpediente)
                : enumClaseDeExpediente.NoDefinido;

            this.crudDeCreacion = new CrudCreacionExpediente(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionExpediente(this, idPanelEdicion);
        }

        protected DespuesDeInicializarCrud(modoAccesoAlNegocio: ModoAcceso.enumModoDeAccesoDeDatos) {
            super.DespuesDeInicializarCrud(modoAccesoAlNegocio);
            let valor = this.MapIndicadores.get(ltrPropiedades.Juridico.Procedimientos.Indicadores.IdTiposDeProcedimientos);
            this.TiposDeProcedimientos = valor ? [valor] : [];
        }

        protected FiltrosExpecificosParaCargarElGrid(operacion: string, clausulas: ClausulaDeFiltrado[]): ClausulaDeFiltrado[] {
            clausulas = super.FiltrosExpecificosParaCargarElGrid(operacion, clausulas);

            const parametros = ObtenerParametrosDeLaUrl();
            if (parametros.find(p => p.clave === Ajax.Param.nombreDeNegocio))
                return;

            const propiedad: string = ltrPropiedades.Expediente.ClaseDeExpediente;

            const criterio: string = this.ClaseDeExpediente === enumClaseDeExpediente.juridico
                ? literal.filtro.criterio.igual
                : literal.filtro.criterio.diferente;

            const clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, criterio, enumClaseDeExpediente.juridico);

            clausulas.push(clausula);
            return clausulas;
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);

            if (opcion === ltrMenus.eventosDeMf.Administracion.Expedientes.ImputarFacturas) {
                let idModal = Crud.crudMnt.IdCrud + '-' + opcion;
                this.AbrirModalParaImputar(idModal);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Administracion.Expedientes);
                return true;
            }

            Administracion.NavegarARelaciones(peticion, ltrParametrosUrl.Administracion.idExpediente);
            return true;
        }

    }

    export class CrudCreacionExpediente extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public ComenzarCreacion() {
            super.ComenzarCreacion();
            ApiDelCrud.PosicionarCrudDeCreacionConCgYTipo(this.PanelDeCrear);
        }
    }

    export class CrudEdicionExpediente extends Crud.CrudEdicion {

        public get ModalCrearValoraciones(): HTMLDivElement {
            return document.getElementById(this.IdModalDeCrearDetalle(ltrEspanes.Expedientes.CrearValoracion)) as HTMLDivElement;
        }

        public get EsUnProcedimientoJudicial(): boolean {
            return (this.CrudDeMnt as CrudDeExpedientes).TiposDeProcedimientos.length > 0 && (this.CrudDeMnt as CrudDeExpedientes).TiposDeProcedimientos.some(t => this.IdTipo);
        }

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public Expansor_TrasAbrirModal(idModal: string, acciones: string): HTMLDivElement {
            let modal = super.Expansor_TrasAbrirModal(idModal, acciones);
            if (acciones.indexOf(ltrAccionesModal.ProponerSolicitante) >= 0)
                ApiDelCrud.ProponerSolicitante(this, modal);
            return modal;
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.tareas), peticion.resultado.datos, ltrPropiedades.Expediente.usaTareas);
            // Los spans de presupuestos, contratos, facturas y apuntes se ocultan si están vacíos (OcultarSiVacio en DescriptorDeExpedientes):
            // los muestra la lectura de su grid si tiene datos. Aquí sólo se ocultan los que el tipo no usa, nunca se muestran, porque al
            // recargar sólo la cabecera (RecargarValoresDeCabecera, tras imputar algo) no se vuelven a leer los grids y un span vacío quedaría visible
            this.OcultarEspanSiNoLoUsa(ltrEspanes.Expedientes.ppts, peticion.resultado.datos, ltrPropiedades.Expediente.usaPpts);
            if ((this.CrudDeMnt as CrudDeExpedientes).ClaseDeExpediente === enumClaseDeExpediente.juridico && !this.EsUnProcedimientoJudicial) {
                this.OcultarEspanSiNoLoUsa(ltrEspanes.Expedientes.ctrsVenta, peticion.resultado.datos, ltrPropiedades.Expediente.scDeVenta);
                this.OcultarEspanSiNoLoUsa(ltrEspanes.Expedientes.ctrsCompra, peticion.resultado.datos, ltrPropiedades.Expediente.scDeCompra);
            }

        }


        private OcultarEspanSiNoLoUsa(espan: string, datos: any, propiedad: string) {
            const div = document.getElementById(this.IdDeExpansor(espan)) as HTMLDivElement;
            if (Definido(div) && !ObtenerPropiedad(datos, propiedad, false))
                ApiPanel.MostrarDetalleSi(div, datos, false);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            // la fila de importes (presupuestado, gastos, ingresos, pagado y cobrado) sólo se muestra si el expediente tiene presupuestos o algo
            // imputado; sin presupuestos, "Presupuestado" se oculta manteniendo su hueco para que el resto no se desplace
            const datos = peticion.resultado.datos;
            const tienePresupuestos: boolean = ObtenerPropiedad(datos, ltrPropiedades.Expediente.TienePresupuestos, false);
            if (tienePresupuestos || ObtenerPropiedad(datos, ltrPropiedades.Expediente.TieneImputaciones, false))
                ApiPanel.MostrarFila(this.PanelDelDto, ltrPropiedades.Expediente.Gastos);
            else
                ApiPanel.OcultarFila(this.PanelDelDto, ltrPropiedades.Expediente.Gastos);

            const presupuestado = ApiPanel.BuscarCeldaPorPropiedad(this.PanelDelDto, ltrPropiedades.Expediente.Presupuestado);
            if (Definido(presupuestado))
                presupuestado.style.visibility = tienePresupuestos ? '' : 'hidden';

            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Expediente.Etapas, false);
            // valoraciones y presupuestos sólo se crean o asocian en la etapa de presupuestación (ver GestorDePresupuestos y
            // ExtensorDePresupuestos.CrearValoracion); para admitirlos en otro estado, se incluye ese estado en la etapa
            const admitePpts = EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos);
            if (admitePpts && this.EsGestor)
                ApiControl.DesbloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ppts));
            else
                ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ppts));
            // en el span sólo se deja "Crear presupuesto": "Añadir valoración" y "Asociar presupuesto" están en el menú de la edición, que
            // pulsa estas referencias ocultas (ver PulsarReferenciaDelExpansor)
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.ppts, ltrEspanes.Opcion.crearDt), true);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.ppts, ltrEspanes.Opcion.vincular), true);

            // un expediente terminado o cancelado no admite nuevas facturas recibidas ni apuntes
            const cerrado = EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Terminada) ||
                EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Cancelada);
            // "Asociar factura" sólo desde el menú de la edición (pulsa esta referencia oculta)
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.facturasRec, ltrEspanes.Opcion.vincular), true);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.facturasRec, ltrEspanes.Opcion.crearRef), cerrado);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.apuntes, ltrEspanes.Opcion.crearRelacion), cerrado);

            // las opciones del menú hacen lo mismo que las referencias de los spans (que no se ven si están vacíos) y siguen sus reglas
            const menuExp = ltrMenus.eventosDeMf.Administracion.Expedientes;
            // bloque de presupuestos: oculto, con su raya, si el tipo no usa presupuestos
            const usaPpts: boolean = ObtenerPropiedad(this.Registro, ltrPropiedades.Expediente.usaPpts, false);
            for (const opcion of [menuExp.CrearValoracion, menuExp.CrearPresupuesto, menuExp.VincularPresupuesto]) {
                ApiDeMenuFlotante.MostrarOpcionDeMenuSi(this.ContenedorMenu, opcion, usaPpts);
                ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, opcion, ltrMenus.enumOrigen.edicion, !this.EsGestor || !admitePpts);
            }
            document.querySelectorAll(`hr[bloque-mf="presupuestos"]`).forEach(hr => {
                if (usaPpts) ApiControl.ExcluirCss(hr as HTMLElement, enumCssOpcionMenu.OcultarHr);
                else ApiControl.IncluirCss(hr as HTMLElement, enumCssOpcionMenu.OcultarHr);
            });
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, menuExp.VincularFacturaRec, ltrMenus.enumOrigen.edicion, !this.EsGestor || cerrado);
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, menuExp.CrearFacturaRec, ltrMenus.enumOrigen.edicion, !this.EsGestor || cerrado);
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, menuExp.AnadirApunte, ltrMenus.enumOrigen.edicion, !this.EsGestor || cerrado);

            let ocultar = !EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.tareas, ltrEspanes.Opcion.crearRef), ocultar);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.tareas, ltrEspanes.Opcion.vincular), ocultar);
            if (!this.EsUnProcedimientoJudicial) {
                if (!EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra))
                    ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ctrsCompra));

                if (!EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta))
                    ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ctrsVenta));
            }
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            var opcion = ObtenerParametroDeUnaUrl(peticion.Url, Ajax.Param.opcionMf, '', false);
            if (opcion === ltrMenus.eventosDeMf.Maestros.Terceros.Interlocutores) {
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Administracion.Expedientes.VincularRegistroEntrada) {
                this.Expansor_AbrirModalDeVincular(ltrModalDeVincular.registrosDeEs);
                return true;
            }

            const menuExp = ltrMenus.eventosDeMf.Administracion.Expedientes;
            const referencias = {
                [menuExp.VincularFacturaRec]: [ltrEspanes.Expedientes.facturasRec, ltrEspanes.Opcion.vincular],
                [menuExp.CrearFacturaRec]: [ltrEspanes.Expedientes.facturasRec, ltrEspanes.Opcion.crearRef],
                [menuExp.AnadirApunte]: [ltrEspanes.Expedientes.apuntes, ltrEspanes.Opcion.crearRelacion],
                [menuExp.VincularPresupuesto]: [ltrEspanes.Expedientes.ppts, ltrEspanes.Opcion.vincular],
                [menuExp.CrearValoracion]: [ltrEspanes.Expedientes.ppts, ltrEspanes.Opcion.crearDt],
                [menuExp.CrearPresupuesto]: [ltrEspanes.Expedientes.ppts, ltrEspanes.Opcion.crearRef],
            };
            if (Definido(referencias[opcion])) {
                this.PulsarReferenciaDelExpansor(referencias[opcion][0], referencias[opcion][1]);
                return true;
            }
            return false;
        }

        // Ejecuta la referencia del span (aunque el span esté oculto por no tener datos): vincular, crear o añadir
        private PulsarReferenciaDelExpansor(espan: string, opcion: string) {
            const id = `${this.IdDeExpansor(espan)}-${opcion}.ref`.toLowerCase();
            const referencia = document.getElementById(id) as HTMLElement;
            if (NoDefinido(referencia))
                MensajesSe.EmitirExcepcion("PulsarReferenciaDelExpansor", `no se ha localizado la referencia ${id}`);
            referencia.click();
        }


        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Expedientes.apuntes)) {
                ApiDeArchivos.MostrarArchivosAnexados(this.PanelDeArchivos.id, this.CrudDeMnt.NombreDeNegocio, this.Registro.id);
            }

            // lo que se imputa desde la edición cambia los importes de los datos generales (las entregas y devoluciones se imputan desde el pedido)
            const imputables = [ltrEspanes.Expedientes.apuntes, ltrEspanes.Expedientes.facturasRec, ltrEspanes.Expedientes.facturasEmt,
                                ltrEspanes.Expedientes.ppts, ltrEspanes.Expedientes.ctrsVenta, ltrEspanes.Expedientes.ctrsCompra];
            if (imputables.some(espan => grid.id === this.IdGridDelExpansor(espan)))
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
        }

    }

    export function Exp_TrasAbrirModalDeCrearValoracion(idModal: string) {
        var modal = document.getElementById(idModal) as HTMLDivElement;
        MapearAlPanel.MapearPropiedadDesdeOtroPanel(Crud.crudMnt.crudDeEdicion.PanelDelDto, modal, ltrPropiedades.Elemento.ConCg.Cg, true);
        MapearAlPanel.MapearPropiedadDesdeOtroPanel(Crud.crudMnt.crudDeEdicion.PanelDelDto, modal, ltrPropiedades.Venta.Presupuesto.Solicitante, false);
        MapearAlPanel.MapearPropiedadDesdeOtroPanel(Crud.crudMnt.crudDeEdicion.PanelDelDto, modal, ltrPropiedades.Elemento.Descripcion, false);

        ApiControl.MapearEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.concepto, ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Nombre));

        let idSociedad: number = ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg);
        let sociedad = ApiControl.BuscarControl(modal, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, true) as HTMLInputElement;
        sociedad.value = idSociedad.toString();

        let idExpediente: number = ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Id);
        let expediente = ApiControl.BuscarControl(modal, ltrPropiedades.Venta.Presupuesto.idExpediente, true) as HTMLInputElement;
        expediente.value = idExpediente.toString();
        ApiDeInicializacion.InicializarListasDeElementos(modal);
        var parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Venta.TipoPpt.ClaseDePpt, Venta.enumClaseDePresupuesto.Venta))
        parametros.push(new Parametro(Ajax.Param.enumNegocio, enumNegocio.Presupuesto))
        parametros.push(new Parametro(Ajax.Param.modo, ModoAcceso.ModoDeAccesoDeDatos.Gestor))
        ApiDePeticiones.LeerTipos(ltrControladores.Venta.Presupuestos, Ajax.EndPoint.Negocio.LeerTipos, parametros, this, new Array<Parametro>()).
            then((peticion) => {
                if (Definido(peticion.resultado.datos) && peticion.resultado.datos.length === 1) {
                    var tipo = ObtenerPropiedad(peticion.resultado.datos[0], ltrPropiedades.Elemento.DeProceso.Expresion);
                    var idtipo = ObtenerPropiedad(peticion.resultado.datos[0], ltrPropiedades.Elemento.Id);
                    var tipoctrl = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Elemento.ConTipo.Tipo);
                    ApiListaDinamica.AsignarValor(tipoctrl, idtipo, tipo);
                    ApiDelCrud.RenderClasePorTipo(modal, tipo);
                    ApiPanel.PosicionarEn(modal);
                }
                else
                    ApiDelCrud.RenderClasePorTipo(modal, undefined);
            }).
            catch((peticion) => ApiDePeticiones.EmitirError(peticion));

        let elPrecioIncluyeElIva = ApiControl.BuscarCheck(modal, ltrPropiedades.Venta.Presupuesto.linea.ElPrecioIncluyeElIva) as HTMLInputElement;
        if (Definido(elPrecioIncluyeElIva)) {
            elPrecioIncluyeElIva.checked = false;
            ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeLinea, true);
        }
    }

    export function Exp_CalcularValoracion() {
        var modal = (Crud.crudMnt.crudDeEdicion as CrudEdicionExpediente).ModalCrearValoraciones;
        let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.cantidad).value);
        let precioHtml = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.precio) as HTMLInputElement;
        let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.descuentoPorLinea).value);

        let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeLinea) as HTMLInputElement;

        let selectorIvaR = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaR);
        let porcentajeIva: number = Definido(objeto) ? Numero(ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje)) : 0;

        let elPrecioIncluyeElIva = ApiControl.BuscarCheck(modal, ltrPropiedades.Venta.Presupuesto.linea.ElPrecioIncluyeElIva) as HTMLInputElement;
        if (Definido(elPrecioIncluyeElIva) && elPrecioIncluyeElIva.checked) {
            let importeDeLinea = Numero(ImporteDeLinea.value);
            let divisor = cantidad * (1 - descuento / 100) * (1 + porcentajeIva / 100);
            if (importeDeLinea > 0 && divisor !== 0)
                AsignarValor(precioHtml, (importeDeLinea / divisor).toString());
            else
                precioHtml.value = "";
            return;
        }

        let precio = Numero(precioHtml.value);
        let importeSinDescuento: number = cantidad * precio;
        let impConElDto = importeSinDescuento;

        if (importeSinDescuento > 0)
            impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);

        if (Definido(objeto)) {
            let elIva = impConElDto * porcentajeIva / 100;
            AsignarValor(ImporteDeLinea, (impConElDto + elIva).toString());
        }
        else
            AsignarValor(ImporteDeLinea, '0');
    }

    export function Exp_Tras_Cambiar_ElPrecioIncluyeElIva() {
        var modal = (Crud.crudMnt.crudDeEdicion as CrudEdicionExpediente).ModalCrearValoraciones;
        let check = ApiControl.BuscarCheck(modal, ltrPropiedades.Venta.Presupuesto.linea.ElPrecioIncluyeElIva) as HTMLInputElement;
        if (!Definido(check))
            return;

        let marcado = check.checked;
        ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.precio, marcado);
        ApiControl.BloquearEditorPorPropiedad(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeLinea, !marcado);
    }


    export function Exp_InicializarModalParaCrearApuntes() {

    }

    export function Exp_InicializarModalParaEditarApuntes() {

    }


}




