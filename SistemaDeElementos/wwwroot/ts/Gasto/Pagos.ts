namespace Gasto {


    function ParsearEtapa(etapa: string): enumEtapasDePago {

        MensajesSe.EmitirExcepcion("Parsear etapa de circuito documental", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaElCircuitoEnEtapa(etapas: string, etapa: enumEtapasDePago): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function CrearCrudDePagos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Gasto.CrudDePagos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePagos extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPago(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPago(this, idPanelEdicion);
        }

        public IncluirParametrosParaProcesarOpcionMf(opcion, esContextual: boolean, parametros: Parametro[], datosDeEntrada: Array<Parametro>): void {
            super.IncluirParametrosParaProcesarOpcionMf(opcion, esContextual, parametros, datosDeEntrada);
            parametros.push(new Parametro(Ajax.Param.ids, this.InfoSelector.IdsSeleccionados));
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
            let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Gasto.Pagos);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Gasto.Pagos.CancelarPreasientos) {
                MensajesSe.Info(peticion.resultado.consola);
                return true;
            }
        }

    }

    export class CrudCreacionPago extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        public TrasSeleccionarCg(idLista: string): void {
            super.TrasSeleccionarCg(idLista)
            this.Tras_Cambiar_Clase_De_Pago();
        }

        public TrasBlanquearCg(): void {
            super.TrasBlanquearCg();
            this.Tras_Cambiar_Clase_De_Pago();
        }

        public Tras_Cambiar_Clase_De_Pago(): void {
            let clase: HTMLSelectElement = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Clase) as HTMLSelectElement;
            let cg = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Elemento.ConCg.Cg) as HTMLInputElement;
            let idCg = Numero(cg.getAttribute(atListasDinamicas.idSeleccionado));
            ApiControl.BloquearLaLista(clase, idCg === 0);
            let pagarEl = ApiControl.BuscarSelectorDeFecha(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.PagarEl);
            ApiControl.AsignarFecha(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.PagadoEl, null);
            if (idCg === 0) {
                this.BloquearBlanquearPagosDeContado(true, true);
                this.BloquearBlanquearPagosPorRemesa(true);
                clase.selectedIndex = 0;
            }
            else {
                OcultarModoDePago(this.PanelDeCrear, clase.value);
                if (clase.value === ltrValores.Gasto.Pago.Clase.Contado) {
                    this.BloquearBlanquearPagosDeContado(false, true);
                    this.BloquearBlanquearPagosPorRemesa(true);
                    MapearAlControl.ProponerFechaEn(pagarEl, ltrPropiedades.Gasto.Pago.PagadoEl, 0);
                }
                else if (clase.value === ltrValores.Gasto.Pago.Clase.Transferencia) {
                    this.BloquearBlanquearCuentaTarjeta(false, true);
                    this.BloquearBlanquearPagosPorRemesa(false);
                    this.Tras_Seleccionar_Acreedor();
                }
                else {
                    this.BloquearBlanquearCuentaTarjeta(true, true);
                    this.BloquearBlanquearPagosPorRemesa(false);
                    this.Tras_Seleccionar_Acreedor();
                }
            }
        }

        public Tras_Cambiar_Modo_De_Pago(): void {
            let modo = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Modo) as HTMLSelectElement;
            if (modo.value === ltrValores.Gasto.Pago.ModoDePago.Contado)
                this.BloquearBlanquearCuentaTarjeta(true, true);
            else if (modo.value === ltrValores.Gasto.Pago.ModoDePago.Tarjeta)
                this.BloquearBlanquearCuentaTarjeta(true, false);
            else
                this.BloquearBlanquearCuentaTarjeta(false, true);
        }


        public Tras_Seleccionar_Acreedor() {
            let acreedor = ApiControl.BuscarListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Acreedor) as HTMLInputElement;
            let idAcreedor = Numero(acreedor.getAttribute(atListasDinamicas.idSeleccionado));
            let clase: HTMLSelectElement = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Clase) as HTMLSelectElement;

            if (clase.value === ltrValores.Gasto.Pago.Clase.Contado || idAcreedor === 0) {
                this.BlanquearCuentaAcredora();
                this.BloquearCuentaAcredora(true);
                return;
            }

            //leer las cuenta bancaria de ingreso de un acreedor, tras leerla mapearla si la hay               
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.id, idAcreedor));
            let datosDeEntrada = new Array<Parametro>();
            ApiDePeticiones.EjecutarPeticion(this.PanelDeCrear, ltrControladores.Terceros.Interlocutores, Ajax.EndPoint.Terceros.Interlocutor.LeerCuentaDeIngreso, parametros, datosDeEntrada)
                .then((peticion: ApiDeAjax.DescriptorAjax) => this.DespuesDeLeerCuentaDeIngreso(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }

        public Tras_Blanquear_Acreedor() {
            this.BlanquearCuentaAcredora();
            this.BloquearCuentaAcredora(true);
        }

        protected ValidarAntesDeCrear(): void {
            super.ValidarAntesDeCrear();
            let clase: HTMLSelectElement = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Clase) as HTMLSelectElement;
            if (clase.value == ltrValores.Gasto.Pago.Clase.Transferencia) {
                let cuentaDePago: HTMLSelectElement = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.CuentaDePago) as HTMLSelectElement;
                if (cuentaDePago.selectedIndex == 0) {
                    cuentaDePago.classList.remove(ltrCss.crtlValido);
                    cuentaDePago.classList.add(ltrCss.crtlNoValido);
                    MensajesSe.EmitirExcepcion("AntesDeCrear", "Un pago por transferencia debe tener una cuenta origen");
                }
                ValidarQueHayCuentaBancaria(this.PanelDeCrear);
            }
        }

        public DespuesDeLeerDatosParaInicializarCreacion(peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeLeerDatosParaInicializarCreacion(peticion);
            let factura: HTMLInputElement = ApiControl.BuscarRestrictor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.IdFacturaRec, ltrTipoControl.restrictorDeEdicion);
            let idFactura: number = Numero(factura.getAttribute(atRestrictor.idRestrictor));
            if (idFactura > 0) {
                ApiControl.MostrarControlSi(factura, true, true);
                ApiControl.MostrarPropiedadSi(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Naturaleza, false, true);
                ApiDePeticiones.LeerElementoPorId(this, ltrControladores.Gasto.FacturasRec, idFactura, new Array<Parametro>(), idFactura)
                    .then((peticion) => this.MapearDatosDeFacturas(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else {
                ApiControl.OcultarControlSi(factura, true, true);
                ApiControl.MostrarPropiedadSi(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Naturaleza, true, true);
                ApiDelCrud.PosicionarCrudDeCreacionConCgYTipo(this.PanelDeCrear);
            }
        }

        private MapearDatosDeFacturas(peticion: ApiDeAjax.DescriptorAjax) {
            let creador: CrudCreacionFacturaRec = peticion.llamador as CrudCreacionFacturaRec;
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(creador.PanelDeCrear, peticion.resultado.datos);

            let proveedor: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.Proveedor, '', true);
            let idProveedor: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.IdProveedor, 0, true);
            let proveedorLista: HTMLInputElement = ApiControl.BuscarControl(creador.PanelDeCrear, ltrPropiedades.Gasto.Pago.Proveedor, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(proveedorLista, idProveedor, proveedor);

            let acreedor: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.Interlocutor, '', true);
            let idAcreedor: number = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.IdInterlocutor, 0, true);
            let acreedorLista: HTMLInputElement = ApiControl.BuscarControl(creador.PanelDeCrear, ltrPropiedades.Gasto.Pago.Acreedor, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(acreedorLista, idAcreedor, acreedor);

            let nombre: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Nombre, '', true);
            let referencia: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Referencia, '', true);
            let numero: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.Pago.Numero, '', true);
            let totalDelPago: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.TotalDelPago, '', true);
            let totalPagado: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.TotalPagado, '', true);
            let totalPagosEnCurso: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.TotalPagosEnCurso, '', true);
            let totalRectificado: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.TotalRectificado, '', true);
            let totalDevuelto: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.TotalDevuelto, '', true);
            let esRectificativa: boolean = Numero( ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.IdRectificada, 0)) > 0;
            let venceEl: string = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.VenceEl, '', true);
            if (!EsFecha(venceEl)) {
                venceEl = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.FacturaRec.FacturadaEl, '', true);
            }

            var controlNombre = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Elemento.Nombre);
            var controlImporte = ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Importe);
            var controlDescri = ApiControl.BuscarAreaDeTexto(this.PanelDeCrear, ltrPropiedades.Elemento.Descripcion);
            controlNombre.value = `${(Numero(totalDelPago) < 0 ? "Abono" : "Pago")} de factura: ${numero} - ${referencia}`;
            controlDescri.value = `${nombre}`;
            var fechaDePago = CrearFecha(venceEl);
            ApiControl.AsignarFecha(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.PagarEl, fechaDePago)

            var pagoMaximo = Numero(totalDelPago) - Math.abs(Numero(totalRectificado)) + Math.abs(Numero(totalDevuelto)) - (Numero(totalPagado) + Numero(totalPagosEnCurso));

            const esUnAbono = Numero(totalDelPago) < 0 && Numero(totalPagado) === 0 && Numero(totalPagosEnCurso) === 0;


            if (pagoMaximo <= 0 && !esUnAbono && !esRectificativa)
                if (pagoMaximo <= 0) {
                    MensajesSe.Info(`La factura tiene pagos realizados y en curso por valor de ${Numero(totalPagado) + Numero(totalPagosEnCurso)}, está saldada, no se puede seguir pagando`);
                    ApiControl.BloquearOpcionDeMenuPorNombre(this.PanelDeCrear, ltrMenus.BarraDeMenu.Crear);
                    pagoMaximo = 0;
                }
            if (esRectificativa) {
                var totalPorDevolver = Math.abs(Numero(totalDelPago));
                var yaDevuelto = Math.abs(Numero(totalPagado));
                var devolucionesEnCurso = Math.abs(Numero(totalPagosEnCurso));
                var maximo = totalPorDevolver - yaDevuelto - devolucionesEnCurso;
                if (maximo <= 0) {
                    MensajesSe.Info(`La devolución tiene devoluciones realizadas y en curso por valor de ${Numero(yaDevuelto) + Numero(devolucionesEnCurso)}, está saldada, no se puede seguir creando devoluciones`);
                    ApiControl.BloquearOpcionDeMenuPorNombre(this.PanelDeCrear, ltrMenus.BarraDeMenu.Crear);
                    pagoMaximo = 0;
                }

            }

            AsignarValor(controlImporte, pagoMaximo.toString());

            let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.PagarEl);
            etiqueta.innerText = ltrEtiquetas.Gasto.Pagos.VenceEl;

            let clase: HTMLSelectElement = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Clase) as HTMLSelectElement;
            if (clase.value === ltrValores.Gasto.Pago.Clase.Contado) {
                ApiControl.AsignarFecha(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.PagadoEl, fechaDePago)
            }

        }

        private BloquearBlanquearPagosDeContado(bloquearModo: boolean, bloquearTarjetaCuenta: boolean): void {
            let modo = ApiControl.BuscarListaDeValores(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Modo) as HTMLSelectElement;
            ApiControl.BloquearLaLista(modo, bloquearModo);
            modo.selectedIndex = 0;
            this.BloquearBlanquearCuentaTarjeta(bloquearTarjetaCuenta, bloquearTarjetaCuenta)
        }

        private BloquearBlanquearPagosPorRemesa(bloquear: boolean): void {
            this.BlanquearCuentaAcredora();
            this.BloquearCuentaAcredora(bloquear);
        }

        private BloquearBlanquearCuentaTarjeta(bloquearCuenta: boolean, bloquearTarjeta: boolean) {
            let tarjeta: HTMLSelectElement = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.TarjetaDePago) as HTMLSelectElement;
            let cuentaDePago: HTMLSelectElement = ApiControl.BuscarListaDeElementos(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.CuentaDePago) as HTMLSelectElement;
            ApiControl.BloquearLaLista(tarjeta, bloquearTarjeta);
            ApiControl.BloquearLaLista(cuentaDePago, bloquearCuenta);
            ApiControl.BlanquearListaDeElementos(tarjeta);
            ApiControl.BlanquearListaDeElementos(cuentaDePago);
            if (!bloquearCuenta)
                MapearAlControl.ListaDeElementos(cuentaDePago, new Array<ClausulaDeFiltrado>(), 0);
            if (!bloquearTarjeta)
                MapearAlControl.ListaDeElementos(tarjeta, new Array<ClausulaDeFiltrado>(), 0);
        }

        private DespuesDeLeerCuentaDeIngreso(peticion: ApiDeAjax.DescriptorAjax): any {
            if (!Definido(peticion.resultado.datos))
                this.BlanquearCuentaAcredora();
            else {
                var objeto = peticion.resultado.datos;
                let iban = ValidarIban(ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.NumeroIban));
                let panel = peticion.llamador as HTMLDivElement;
                MapearCuentaBancaria(panel, iban);
                this.pag_mapearDatosdeLaCbDelAcreedor(panel, objeto);
                this.pag_mapearProveedor(panel, objeto);
                this.pag_mapearTrabajador(panel, objeto);
            }
        }

        private BloquearCuentaAcredora(bloquear: boolean): void {
            ApiControl.BloquearEditorPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Iban, bloquear);
            ApiControl.BloquearEditorPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Entidad, bloquear);
            ApiControl.BloquearEditorPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Oficina, bloquear);
            ApiControl.BloquearEditorPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.DcCcc, bloquear);
            ApiControl.BloquearEditorPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Numero, bloquear);
            ApiControl.BloquearEditorPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Alias, bloquear);
        }

        private BlanquearCuentaAcredora(): void {
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Iban));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Entidad));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Oficina));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.DcCcc));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Numero));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Alias));
            ApiControl.BlanquearEditor(ApiControl.BuscarEditor(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.BancoAcreedor));
            let activa = ApiControl.BuscarCheck(this.PanelDeCrear, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Activa);
            activa.checked = false;

            ApiControl.BlanquearListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Proveedor);
            ApiControl.BlanquearListaDinamicaPorPropiedad(this.PanelDeCrear, ltrPropiedades.Gasto.Pago.Trabajador);

        }


        private pag_mapearDatosdeLaCbDelAcreedor(panel: HTMLDivElement, objeto: any) {
            let alias = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Alias);
            alias.value = ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Alias);
            let banco = ApiControl.BuscarEditor(panel, ltrPropiedades.Gasto.Pago.BancoAcreedor);
            banco.value = ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Banco);
            let activa = ApiControl.BuscarCheck(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Activa);
            activa.checked = EsTrue(ObtenerPropiedad(objeto, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Activa));
        }

        private pag_mapearProveedor(panel: HTMLDivElement, objeto: any) {
            let idProveedor = ObtenerPropiedad(objeto, ltrPropiedades.Gasto.Pago.IdProveedor);
            if (Numero(idProveedor > 0)) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Gasto.Pago.Proveedor);
                let proveedor = ObtenerPropiedad(objeto, ltrPropiedades.Gasto.Pago.Proveedor);
                MapearAlControl.ListaDinamica(lista, idProveedor, proveedor, true);
            }
        }

        private pag_mapearTrabajador(panel: HTMLDivElement, objeto: any) {
            let idTrabajador = ObtenerPropiedad(objeto, ltrPropiedades.Gasto.Pago.IdTrabajador);
            if (Numero(idTrabajador > 0)) {
                let lista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Gasto.Pago.Trabajador);
                let Trabajador = ObtenerPropiedad(objeto, ltrPropiedades.Gasto.Pago.Trabajador);
                MapearAlControl.ListaDinamica(lista, idTrabajador, Trabajador, true);
            }
        }
    }

    export class CrudEdicionPago extends Crud.CrudEdicion {


        private get _esAbono(): boolean {
            return ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.Pago.EsAbono, false);
        }
        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);

            let idfactura: number = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.Pago.IdFacturaRec, true));
            let idNaturaleza: number = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.Pago.IdNaturaleza, true));
            let idPreasiento: number = Numero(ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.Pago.IdPreasiento, true));
            let etiqueta: HTMLLabelElement = ApiControl.BuscarEtiqueta(panel, ltrPropiedades.Gasto.Pago.PagarEl);
            etiqueta.innerText = idfactura > 0 ? ltrEtiquetas.Gasto.Pagos.VenceEl : ltrEtiquetas.Gasto.Pagos.PagarEl;

            let etapas: Array<string> = ObtenerPropiedad(this.Registro, ltrPropiedades.Gasto.Pago.Etapas, true);
            let bloquear: boolean = idfactura > 0 || !this.EsGestor || !EstaElEnumerado(etapas, enumEtapasDePago, enumEtapasDePago.PAG_Etapa_Pendiente);
            ApiControl.BloquearEditorPorPropiedad(panel, ltrPropiedades.Gasto.Pago.PagarEl, bloquear);

            var clase = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.Pago.Clase);
            OcultarModoDePago(panel, clase);

            ApiControl.MostrarPropiedadSi(this.PanelDelDto, ltrPropiedades.Gasto.Pago.IdFacturaRec, idfactura > 0, true);
            ApiControl.MostrarPropiedadSi(this.PanelDelDto, ltrPropiedades.Gasto.Pago.Naturaleza, idNaturaleza > 0, true);
            ApiControl.MostrarPropiedadSi(this.PanelDelDto, ltrPropiedades.Gasto.Pago.IdPreasiento, idPreasiento > 0, true);

            if (EstaElEnumerado(etapas, enumEtapasDePago, enumEtapasDePago.PAG_Etapa_Pendiente)) {
                if (clase === ltrValores.Gasto.Pago.Clase.Contado) {
                    var modo = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Gasto.Pago.Modo);
                    if (modo === ltrValores.Gasto.Pago.ModoDePago.Tarjeta) {
                        ApiControl.DesbloquearListaDeElemento(this.PanelDelDto, ltrPropiedades.Gasto.Pago.TarjetaDePago);
                    }
                }
                else {
                    ApiControl.BloquearListaDeElemento(this.PanelDelDto, ltrPropiedades.Gasto.Pago.TarjetaDePago);
                }
                if (clase === ltrValores.Gasto.Pago.Clase.Remesa)
                    ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
            }
            else {
                ModoAcceso.AplicarloAlPanel(this.PanelDelDto, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                ModoAcceso.AplicarloAlPanel(this.PanelDeArchivadores, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, false);
                ApiControl.BloquearListaDeElemento(this.PanelDelDto, ltrPropiedades.Gasto.Pago.TarjetaDePago);
            }
            ApiDeMenuFlotante.DesbloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Gasto.Pagos.GenerarPreasieto, ltrMenus.enumOrigen.edicion, Registro.EsAdministrador() && this.EstaTerminada);
        }


        public DespuesDeAnexarMostrarArchivo(archivoDto: any): void {
            super.DespuesDeAnexarMostrarArchivo(archivoDto);
            this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
        }


        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;

            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Gasto.Pagos.GenerarPreasieto) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeTrazas()
                return true;
            }
            return false;
        }


        public CargaCompletada() {
            super.CargaCompletada();
            let etiqueta = ApiControl.BuscarEtiqueta(this.PanelDelDto, ltrPropiedades.Gasto.Pago.PagadoEl);
            etiqueta.innerText = this._esAbono ? ltrEtiquetas.Gasto.Pagos.AbonadoEl : ltrEtiquetas.Gasto.Pagos.PagadoEl;
            //var facturaEmtCtl = ApiControl.BuscarRestrictor(this.PanelDelDto, ltrPropiedades.Gasto.Pago.IdFacturaEmt, ltrTipoControl.restrictorDeEdicion);
            //ApiControl.OcultarControlSi(facturaEmtCtl, !this._esAbono);
            //var facturaRecCtl = ApiControl.BuscarRestrictor(this.PanelDelDto, ltrPropiedades.Gasto.Pago.IdFacturaRec, ltrTipoControl.restrictorDeEdicion);
            //ApiControl.OcultarControlSi(facturaRecCtl, this._esAbono);
            //ApiPanel.OcultarCelda

            if (this.EstaTerminada)
                ModoAcceso.AplicarModoAccesoAlSelectorDeArchivos(this.PanelDeEditar, this.EsInterventorSinEstado ? ModoAcceso.enumModoDeAccesoDeDatos.Gestor: ModoAcceso.enumModoDeAccesoDeDatos.Consultor);

        }

    }

    function OcultarModoDePago(panel: HTMLDivElement, clase: string) {
        var contenedor = ApiPanel.OcultarContenedorDto(panel, ltrPropiedades.Gasto.Pago.Modo, clase !== ltrValores.Gasto.Pago.Clase.Contado);
        if (ltrPropiedades.Gasto.Pago.Modo, clase !== ltrValores.Gasto.Pago.Clase.Contado)
            contenedor.parentElement.style.gridTemplateColumns = 'auto';

        else
            contenedor.parentElement.style.gridTemplateColumns = 'auto auto';
    }

}