namespace Administracion {

    enum enumEtapasDeExpedientes {
        EXP_Etapa_Asociar_Tareas,
        EXP_Etapa_Asociar_Presupuestos,
        EXP_Etapa_Asociar_SC_Compra,
        EXP_Etapa_Asociar_SC_Venta
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
        public get ClaseDeExpediente(): enumClaseDeExpediente { return this._clase; }
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
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.Expedientes.ppts), peticion.resultado.datos, ltrPropiedades.Expediente.usaPpts);
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.Expedientes.apuntes), peticion.resultado.datos, ltrPropiedades.Expediente.usaPpts);
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.Expedientes.facturasRec), peticion.resultado.datos, ltrPropiedades.Expediente.usaPpts);
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.Expedientes.facturasEmt), peticion.resultado.datos, ltrPropiedades.Expediente.usaPpts);
            //if ((this.CrudDeMnt as CrudDeExpedientes).ClaseDeExpediente === enumClaseDeExpediente.juridico) {
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.Expedientes.ctrsVenta), peticion.resultado.datos, ltrPropiedades.Expediente.scDeVenta);
            ApiPanel.MostrarEspanSegunPropiedad(this.IdDeExpansor(ltrEspanes.Expedientes.ctrsCompra), peticion.resultado.datos, ltrPropiedades.Expediente.scDeCompra);
            //}

        }


        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Expediente.Etapas, false);
            if (!EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos)) {
                ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ppts));
                ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.ppts, ltrEspanes.Opcion.crearDt), true);
            }

            let ocultar = !EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.tareas, ltrEspanes.Opcion.crearRef), ocultar);
            ApiPanel.OcultarMostrarPanelPorId(this.OpcionDeExpansor(ltrEspanes.Expedientes.tareas, ltrEspanes.Opcion.vincular), ocultar);

            if (!EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra))
                ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ctrsCompra));

            if (!EstaElEnumerado(etapas, enumEtapasDeExpedientes, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta))
                ApiControl.BloquearReferenciaPostDeCreacion(this.IdDeExpansor(ltrEspanes.Expedientes.ctrsVenta));
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
            return false;
        }


        public RecargarGridDeRelacion(grid: HTMLDivElement, idnegocio: number, id: number) {
            super.RecargarGridDeRelacion(grid, idnegocio, id);
            if (grid.id === this.IdGridDelExpansor(ltrEspanes.Expedientes.apuntes)) {
                ApiDeArchivos.MostrarArchivosAnexados(this.PanelDeArchivos.id, this.CrudDeMnt.NombreDeNegocio, this.Registro.id);
            }
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
    }

    export function Exp_CalcularValoracion() {
        var modal = (Crud.crudMnt.crudDeEdicion as CrudEdicionExpediente).ModalCrearValoraciones;
        let cantidad = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.cantidad).value);
        let precio = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.precio).value);

        let ImporteDeLinea = ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.ImporteDeLinea) as HTMLInputElement;

        let importeSinDescuento: number = cantidad * precio;
        let impConElDto = importeSinDescuento

        if (importeSinDescuento > 0) {
            let descuento = Numero(ApiControl.BuscarEditor(modal, ltrPropiedades.Venta.Presupuesto.linea.descuentoPorLinea).value);
            impConElDto = importeSinDescuento - (importeSinDescuento * descuento / 100);
        }

        let selectorIvaR = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Venta.Presupuesto.linea.selectorDeIvaR);
        let objeto = OpcionesDeLasListas.ObtenerObjeto(selectorIvaR);
        if (Definido(objeto)) {
            let porcentaje = ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.IvaR.Porcentaje);
            let elIva = impConElDto * porcentaje / 100;
            AsignarValor(ImporteDeLinea, (impConElDto + elIva).toString());
        }
        else
            AsignarValor(ImporteDeLinea, '0');
    }


    export function Exp_InicializarModalParaCrearApuntes() {

    }

    export function Exp_InicializarModalParaEditarApuntes() {

    }


}




