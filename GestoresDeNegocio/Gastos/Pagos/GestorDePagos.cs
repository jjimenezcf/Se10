using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Contabilidad;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Ventas;
using System;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Ventas;
using ServicioDeReportes.Gastos;
using System.IO;
using QuestPDF.Fluent;
using ServicioDeDatos.SistemaDocumental;
using System.Threading.Tasks;
using ServicioDeDatos.Negocio;
using ModeloDeDto;

namespace GestoresDeNegocio.Gastos
{

    public class GestorDePagos : GestorDeElementos<ContextoSe, PagoDtm, PagoDto>, ITotalizador<TotalesDePago>, IGeneradorDePreasiento
    {
        public class MapearPago : Profile
        {
            public MapearPago()
            {
                CreateMap<PagoDtm, PagoDto>()
                .ForMember(dto => dto.CuentaDePago, dtm => dtm.Ignore())
                .ForMember(dto => dto.CuentaDeAcreedor, dtm => dtm.Ignore())
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Proveedor, dtm => dtm.MapFrom(dtm => dtm.Proveedor == null ? null : dtm.Proveedor.Expresion))
                .ForMember(dto => dto.Trabajador, dtm => dtm.MapFrom(dtm => dtm.Trabajador == null ? null : dtm.Trabajador.Expresion));

                CreateMap<PagoDto, PagoDtm>()
                .ForMember(dtm => dtm.CuentaDeAcreedor, dto => dto.Ignore())
                .ForMember(dtm => dtm.CuentaDePago, dto => dto.Ignore())
                .ForMember(dtm => dtm.Trabajador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Pago;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDePago.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePagos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePagos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePagos(contexto, mapeador);
        }

        protected override IQueryable<PagoDtm> AplicarOrden(IQueryable<PagoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<PagoDtm> AplicarFiltros(IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorAcreedor(filtros);
            consulta = consulta.FiltroPorFacturaRec(filtros);
            consulta = consulta.FiltroPorCuentaDePago(filtros);
            consulta = consulta.FiltroPorPagadoEl(filtros);
            consulta = consulta.FiltroPorPagarEl(filtros);
            consulta = consulta.FiltroPorImporte(filtros);
            consulta = consulta.FiltroPorFormaDePago(filtros);
            consulta = consulta.FiltroSiHayPreasiento(Contexto, filtros);
            consulta = consulta.FiltroPorRemesas(Contexto, filtros);
            consulta = consulta.FiltroPorPagosNoIncluidoEnRemesa(Contexto, filtros);
            consulta = consulta.FiltroParaSeleccionarPagoEnRemesas(Contexto, filtros);
            consulta = consulta.FiltroFacturasEnUnLoteContable(Contexto, filtros, parametros);
            return consulta;
        }

        protected override IQueryable<PagoDtm> AplicarSeguridad(IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<PagoDtm, TipoDePagoDtm, PermisoDelPagoDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<PagoDtm, PermisoDelPagoDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(PagoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);

            if (elemento.CuentaDeAcreedor.Length == 28)
            {
                var cb = elemento.CuentaDeAcreedor.Split(Simbolos.separadorDeCtaban);
                var cuenta = ExtensorDeCuentasBancarias.Leer(Contexto,
                    isoPais: cb[0].Substring(0, 2),
                    dcIban: cb[0].Substring(2, 2),
                    entidad: cb[1],
                    oficina: cb[2],
                    dc: cb[3],
                    numero: cb[4],
                    crearSiNoExiste: false);

                if (cuenta is null && elemento.IdProveedor is not null)
                    elemento.IdCuentaDeAcreedor = new GestorDeCuentasDeProveedor(Contexto, Contexto.Mapeador).PersistirElementoDto(new CuentaDeProveedorDto
                    {
                        IdElemento = (int)elemento.IdProveedor,
                        Alias = elemento.Alias,
                        Activa = true,
                        Iban = cb[0],
                        Entidad = cb[1],
                        Oficina = cb[2],
                        DcCcc = cb[3],
                        Numero = cb[4],
                        Clase = enumClaseDeCuentaBancaria.Ingreso.ToString(),

                    }, new ParametrosDeNegocio(enumTipoOperacion.Insertar)).IdCuenta;
                else
                if (cuenta is null && elemento.IdTrabajador is not null)
                    elemento.IdCuentaDeAcreedor = new GestorDeCuentasDeTrabajador(Contexto, Contexto.Mapeador).PersistirElementoDto(new CuentaDeTrabajadorDto
                    {
                        IdElemento = (int)elemento.IdTrabajador,
                        Alias = elemento.Alias,
                        Activa = true,
                        Iban = cb[0],
                        Entidad = cb[1],
                        Oficina = cb[2],
                        DcCcc = cb[3],
                        Numero = cb[4],
                        Clase = enumClaseDeCuentaBancaria.Ingreso.ToString(),

                    }, new ParametrosDeNegocio(enumTipoOperacion.Insertar)).IdCuenta;
                else
                if (cuenta is null)
                    elemento.IdCuentaDeAcreedor = new GestorDeCuentasDeInterlocutor(Contexto, Contexto.Mapeador).PersistirElementoDto(new CuentaDeInterlocutorDto
                    {
                        IdElemento = (int)elemento.IdSolicitante,
                        Alias = elemento.Alias,
                        Activa = true,
                        Iban = cb[0],
                        Entidad = cb[1],
                        Oficina = cb[2],
                        DcCcc = cb[3],
                        Numero = cb[4],
                        Clase = enumClaseDeCuentaBancaria.Ambas.ToString(),

                    }, new ParametrosDeNegocio(enumTipoOperacion.Insertar)).IdCuenta;
                else
                {
                    elemento.IdCuentaDeAcreedor = cuenta.Id;
                    AsociarCuentaAlInterlocutor(elemento, cuenta);
                }
            }
        }

        private void AsociarCuentaAlInterlocutor(PagoDto elemento, CuentaBancariaDtm cuenta)
        {
            var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(elemento.IdSolicitante);
            var proveedor = inter.Proveedor(Contexto);
            if (proveedor != null)
            {
                proveedor.AsociarCuenta(Contexto, elemento.Alias, enumClaseDeCuentaBancaria.Ingreso, cuenta);
                return;
            }
            var trabajador = inter.Trabajador(Contexto);
            if (trabajador != null)
            {
                trabajador.AsociarCuenta(Contexto, elemento.Alias, enumClaseDeCuentaBancaria.Ingreso, cuenta);
                return;
            }
            inter.AsociarCuenta(Contexto, elemento.Alias, enumClaseDeCuentaBancaria.Ambas, cuenta);
        }

        protected override void DespuesDeMapearElRegistro(PagoDto elemento, PagoDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (opciones.Insertando) opciones.Parametros[nameof(PagoDto.IdArchivoAlCrear)] = elemento.IdArchivoAlCrear;
        }

        protected override void AntesDeMapearElRegistroParaModificar(PagoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(elemento, opciones);
            if (elemento.CuentaDeAcreedor.Length == 28)
            {
                var cb = elemento.CuentaDeAcreedor.Split(Simbolos.separadorDeCtaban);
                var cuenta = ExtensorDeCuentasBancarias.Leer(Contexto,
                    isoPais: cb[0].Substring(0, 2),
                    dcIban: cb[0].Substring(2, 2),
                    entidad: cb[1],
                    oficina: cb[2],
                    dc: cb[3],
                    numero: cb[4],
                    crearSiNoExiste: false);

                if (cuenta is null)
                    GestorDeErrores.Emitir($"la cuenta de acreedor no es modificable");

                elemento.IdCuentaDeAcreedor = cuenta.Id;
            }
        }

        protected override void AntesDePersistir(PagoDtm pago, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(pago, parametros);

            if (parametros.Insertando)
            {
                var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(pago.IdSolicitante);
                var proveedor = inter.Proveedor(Contexto, crearProveedor: false);
                if (proveedor is not null)
                    pago.IdProveedor = proveedor.Id;

                var trabajador = inter.Trabajador(Contexto);
                if (trabajador is not null)
                    pago.IdTrabajador = trabajador.Id;

                pago.Nif = inter.NIF(Contexto);

                if (parametros.AccionQueSeEjecuta != ltrDeUnaFacturaRec.Accion_CrearPagoAlCrearFactura)
                    pago.ValidarFactura(Contexto);

                if (pago.Clase == enumClaseDePago.Contado
                    && (pago.ModoDePago == enumModoDePagoContado.Contado || pago.ModoDePago == enumModoDePagoContado.Domiciliacion)
                    && pago.PagadoEl is null)
                {
                    pago.PagadoEl = DateTime.Now;
                    if (pago.PagarEl == null) pago.PagarEl = pago.PagadoEl;
                }

                if (pago.Clase == enumClaseDePago.Remesa && pago.PagarEl is not null)
                {
                    GestorDeErrores.Emitir($"No puede crear el pago con clase de pago '{enumClaseDePago.Remesa.Descripcion()}' e indicar que tiene fecha de pago");
                }

                pago.ValidarPago(Contexto);

                return;
            }

            ValidarAcreedor(pago, parametros);

            ValidarFechasDelPago(pago, parametros);

            if (!parametros.Transitando)
            {
                pago.ValidarPago(Contexto);
            }


            if (pago.SeHaModificadoElCampo<string>(x => x.Name == nameof(PagoDtm.Nif), parametros))
                GestorDeErrores.Emitir($"No se puede modificar el NIF del pago '{pago.Referencia}'");

            if (pago.SeHaModificadoElCampo<enumClaseDeCobro>(x => x.Name == nameof(PagoDtm.Clase), parametros))
                GestorDeErrores.Emitir($"No se puede modificar la clase del pago '{pago.Referencia}', cancélelo y cree uno nuevo");

            var puedeModificar = parametros.AccionQueSeEjecuta == ltrDeUnaFacturaRec.Accion_ModificarIva ||
                                 parametros.AccionQueSeEjecuta == ltrDeUnPago.Accion_GenerarPreasiento ||
                                 parametros.AccionQueSeEjecuta == ltrDeUnaRemesaPag.Accion_CambiarFechaDePago;

            if (!puedeModificar && !pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente) && !parametros.ProcesandoTransicion(pago))
                GestorDeErrores.Emitir($"no se puede modificar el pago '{pago.Referencia}' por no estar en la etapa de pendiente");

            if (pago.Clase == enumClaseDePago.Transferencia && pago.IdCuentaDePago is null)
                GestorDeErrores.Emitir($"El pago '{pago.Referencia}' debe tener indicado una cuenta de pago");

            if (pago.Clase != enumClaseDePago.Transferencia && pago.IdCuentaDePago is not null && parametros.AccionQueSeEjecuta == ltrDeUnPago.IncluidaEnRemesa)
                GestorDeErrores.Emitir($"El pago '{pago.Referencia}' es '{pago.Clase.Descripcion()}', por tanto no ha de indicar cuenta de pago");

            if (parametros.AccionQueSeEjecuta != ApiDeEnsamblados.DespuesDeQuitarVinculo && pago.SeHaModificadoElCampo<int?>(x => x.Name == nameof(PagoDtm.IdFacturaRec), parametros))
                GestorDeErrores.Emitir($"No puede modificarse la factura del pago '{pago.Referencia}', cancele el pago y cree uno nuevo");

            if (parametros.AccionQueSeEjecuta != ltrDeUnPago.Accion_GenerarPreasiento)
            {
                if (!pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado))
                {
                    var preasiento = Contexto.Set<PreasientoDtm>().FirstOrDefault(prea => prea.IdReferenciado == pago.Id && prea.NegocioReferenciado == Negocio);
                    if (preasiento == null)
                        pago.IdPreasiento = null;
                    else
                    {
                        pago.IdPreasiento = preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado) ? null : pago.IdPreasiento;
                    }
                }
                else
                {
                    if (parametros.AccionQueSeEjecuta != ltrDeUnPago.Accion_Pagar)
                        pago.IdPreasiento = ((PagoDtm)parametros.registroEnBd).IdPreasiento;
                }
            }
        }

        private static void ValidarAcreedor(PagoDtm pago, ParametrosDeNegocio parametros)
        {
            pago.IdCliente = ((PagoDtm)parametros.registroEnBd).IdCliente;

            if (pago.SeHaModificadoElCampo<int?>(x => x.Name == nameof(PagoDtm.IdProveedor), parametros))
                GestorDeErrores.Emitir($"No se puede modificar el proveedor del pago '{pago.Referencia}'");

            if (pago.SeHaModificadoElCampo<int?>(x => x.Name == nameof(PagoDtm.IdTrabajador), parametros))
                GestorDeErrores.Emitir($"No se puede modificar el trabajador del pago '{pago.Referencia}'");
        }

        private static void ValidarFechasDelPago(PagoDtm pago, ParametrosDeNegocio parametros)
        {
            if (pago.IdFacturaRec is null && pago.PagadoEl is not null && pago.PagarEl is not null && pago.PagarEl > pago.PagadoEl)
            {
                var loPermito = parametros.EstaEjecutandoUnaAccion && (parametros.AccionQueSeEjecuta != VariableDePagos.enumMotivoTransicion.PagarRemesa.ToString() ||
                                                                       parametros.AccionQueSeEjecuta != VariableDePagos.enumMotivoTransicion.AnularAnulacion.ToString());
                if (!loPermito)
                    GestorDeErrores.Emitir($"la fecha de cuándo pagar '{pago.PagarEl.Fecha().ToShortDateString()}' el pago '{pago.Referencia}' no puede ser mayor que la de pago '{pago.PagadoEl.Fecha().ToShortDateString()}'");
            }

            if (pago.PagarEl is not null && pago.PagarEl < DateTime.Now.Date && pago.PagadoEl is null)
                if (!parametros.EsUnaTransicion)
                    GestorDeErrores.Emitir($"la fecha de cuándo pagar '{pago.PagarEl.Fecha().ToShortDateString()}' el pago '{pago.Referencia}' no puede ser menor de hoy si la fecha de pago es nula");
        }

        protected override void DespuesDePersistir(PagoDtm pago, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(pago, parametros);
            if (pago.Clase != enumClaseDePago.Remesa)
            {
                if (pago.PagadoEl is not null && pago.PagadoEl.Fecha().Date < DateTime.Now.Date.AddDays(+1))
                {
                    if (pago.IdFacturaRec is null && !parametros.EsUnaTransicion)
                    {
                        if (pago.PagarEl is null) pago.PagarEl = pago.PagadoEl;
                        var parametrosDeTransicion = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                        if (parametros.AccionQueSeEjecuta == ltrDeUnPago.Accion_CrearAbono)
                            parametrosDeTransicion.Add(nameof(AbonoDeFaeDtm.FacturaEmt), parametros.Parametros[nameof(AbonoDeFaeDtm.FacturaEmt)]);
                        pago.TransitarALaEtapa(Contexto, enumEtapasDePagos.PAG_Etapa_Pagado.EstadosDeLaEtapa(), parametrosDeTransicion, delSistema: false);
                    }
                }
                else
                {
                    if (pago.PagarEl is not null && pago.PagarEl.Fecha() >= DateTime.Now)
                        pago.PersistirEvento(Contexto);
                    if (pago.PagarEl is null && pago.SeHaModificadoElCampo<DateTime?>(x => x.Name == nameof(PagoDtm.PagarEl), parametros))
                        pago.EliminarEvento(Contexto);
                }
            }

            if (parametros.Insertando)
            {
                var hayJustificante = parametros.Parametros.LeerValor(nameof(PagoDto.IdArchivoAlCrear), 0).ToString().Entero() > 0;
                if (hayJustificante)
                    hayJustificante = IncluirArchivoTrasCrear(pago, idArchivo: parametros.Parametros.LeerValor(nameof(PagoDto.IdArchivoAlCrear), 0).ToString().Entero());

                if (pago.IdFacturaRec is not null)
                {
                    pago = pago.Recargar(Contexto);
                    pago.Vincular(Contexto, enumNegocio.FacturaRecibida, (int)pago.IdFacturaRec);

                    if (!hayJustificante && pago.Clase == enumClaseDePago.Contado && (pago.ModoDePago == enumModoDePagoContado.Contado || pago.ModoDePago == enumModoDePagoContado.Domiciliacion))
                    {
                        var factura = pago.FacturaRec(Contexto);
                        if (factura.IdArchivo is not null)
                        {
                            var parametrosDeVinculacion = new Dictionary<string, object>();
                            if (parametros.Parametros.ContieneClave(ltrParametrosNeg.AccionQueSeEjecuta))
                                parametrosDeVinculacion.Add(ltrParametrosNeg.AccionQueSeEjecuta, parametros.Parametros.LeerValor<string>(ltrParametrosNeg.AccionQueSeEjecuta));
                            pago = pago.Vincular(Contexto, enumNegocio.Archivos, (int)factura.IdArchivo, parametros: parametrosDeVinculacion);
                        }
                        hayJustificante = true;

                    }

                    pago.IntentarCerrarPagoDeFactura(Contexto, hayJustificante);
                }
            }
        }

        protected override void EliminarCaches(PagoDtm pago, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(pago, parametros);

            var quieMeRectifica = null as FacturaRecDtm;
            if (pago.IdFacturaRec is not null)
            {
                quieMeRectifica = pago.FacturaRec(Contexto);
                EliminarCachesDePagosPorFactura(pago.FacturaRec(Contexto));
            }
            if (pago.PropiedadCambiada<int?>(nameof(PagoDtm.IdFacturaRec), parametros) && ((PagoDtm)parametros.registroEnBd).IdFacturaRec.HasValue)
            {
                quieMeRectifica = ((PagoDtm)parametros.registroEnBd).FacturaRec(Contexto);
                EliminarCachesDePagosPorFactura(((PagoDtm)parametros.registroEnBd).FacturaRec(Contexto));
            }
            if (quieMeRectifica is not null)
                EliminarCachesDePagosPorFactura(quieMeRectifica);

            if (pago.EsAbono(Contexto) && !parametros.Insertando)
            {
                var facturaAbonada = parametros.Parametros.ContieneClave(nameof(AbonoDeFaeDtm.FacturaEmt))
                ? parametros.Parametros.LeerValor<FacturaEmtDtm>(nameof(AbonoDeFaeDtm.FacturaEmt))
                : pago.FacturaAbonada(Contexto);

                ServicioDeCaches.EliminarElemento(CacheDe.Fae_Abonado, facturaAbonada.Id.ToString());
            }

            ServicioDeCaches.EliminarElemento(CacheDe.Pag_DatosDelPagoDto, pago.Id.ToString());
        }

        private void EliminarCachesDePagosPorFactura(FacturaRecDtm factura)
        {
            ServicioDeCaches.EliminarElemento(CacheDe.Far_TotalPagado, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_PagosNoCancelados, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_PagosEnCurso, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_PagosContadosEnCurso, factura.Id.ToString());
            if (factura.IdExpediente is not null) ServicioDeCaches.EliminarElemento(CacheDe.Exp_Pagos, factura.IdExpediente.ToString());
        }

        protected override PagoDtm AntesDeTransitar(PagoDtm pago, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            pago = base.AntesDeTransitar(pago, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDePagos.PAG_Etapa_Pendiente.Estados(), enumEtapasDePagos.PAG_Etapa_Pagado.Estados()))
                return pago.AntesDePagar(Contexto, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Cancelado.Estados()))
                return pago.AntesDeCancelar(Contexto, parametros);

            if (transicion.OrigenEstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Cancelado.Estados()))
                GestorDeErrores.Emitir($"El pago '{pago.Referencia}' está cancelado, no se puede reactivar");

            if (transicion.EntreEtapas(enumEtapasDePagos.PAG_Etapa_Pagado.Estados(), enumEtapasDePagos.PAG_Etapa_Pendiente.Estados()))
                return pago.AntesDeReabrir(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePagos.PAG_Etapa_Pagado.Estados(), enumEtapasDePagos.PAG_Etapa_Anulacion.Estados()))
                return pago.AntesDeAnular(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePagos.PAG_Etapa_Pendiente.Estados(), enumEtapasDePagos.PAG_Etapa_Remesado.Estados()))
                return pago.AntesDeRemesar(Contexto, parametros);

            return pago;
        }

        protected override PagoDtm DespuesDeTransitar(PagoDtm pago, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            pago = base.DespuesDeTransitar(pago, transicion, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado.Estados()))
            {
                if (parametros.LeerValor(ltrParametrosNeg.AccionQueSeEjecuta, "") != ltrDeUnPago.Accion_DarPorPagadoAlTransitarFactura &&
                    parametros.LeerValor(ltrParametrosNeg.AccionQueSeEjecuta, "") != ltrDeUnPago.Accion_Pagar)
                    pago.TrasPagar(Contexto);
            }
            else
            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente.Estados()) || transicion.DestinoEstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Anulacion.Estados()))
                pago.TrasDevolverPago(Contexto);
            else
            if (transicion.EntreEtapas(enumEtapasDePagos.PAG_Etapa_Pagado.Estados(), enumEtapasDePagos.PAG_Etapa_Remesado.Estados()))
                pago.TrasDevolverPago(Contexto);
            else
            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Cancelado.Estados()))
            {
                pago.TrasCancelarPago(Contexto);
            }

            return pago;
        }

        protected override void DespuesDeMapearElElemento(PagoDtm pago, PagoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(pago, elemento, parametros);

            elemento.EsAbono = pago.IdCliente.Entero() > 0;
            MapearDatosDeAcreedor(pago, elemento);
            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {

                elemento.Etapas = pago.ListaDeEtapas();
                if (pago.IdCuentaDeAcreedor is not null)
                {
                    MapearDatosDeLaCuentaAcreedora(pago, elemento);
                }
                if (pago.IdTarjetaDePago is not null)
                {
                    pago.IdCuentaDePago = pago.Tarjeta(Contexto).IdCuentaDeCargo;
                }
                if (pago.IdCuentaDePago is not null)
                {
                    var cbp = pago.CuentaBancariaDePago(Contexto);
                    elemento.CuentaDePago = cbp.NumeroIban;
                    elemento.BancoDePago = cbp.Banco(Contexto).Nombre;
                }
                if (pago.IdFacturaRec is not null) elemento.FacturaRec = Contexto.SeleccionarPorId<FacturaRecDtm>((int)pago.IdFacturaRec, aplicarPermisos: false).Expresion;
                if (pago.EsAbono(Contexto))
                {
                    var fae = Contexto.SeleccionarPorId<FacturaEmtDtm>((int)pago.FacturaAbonada(Contexto).Id, aplicarPermisos: false, errorSiNoHay: false);
                    //elemento.FacturaEmt =fae?.Expresion ?? string.Empty;
                    elemento.IdFacturaEmt = fae?.Id ?? null;
                }


                if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado))
                {
                    elemento.IdPreasiento = pago.IdPreasiento;
                    elemento.Preasiento = pago.Preasiento(Contexto, errorSiNoHay: false)?.Expresion ?? "Sin preasiento";
                }

            }
            var cache = ServicioDeCaches.Obtener(CacheDe.Pag_DatosDelPagoDto);
            cache[elemento.Id.ToString()] = elemento;
        }

        private bool IncluirArchivoTrasCrear(PagoDtm pago, int idArchivo)
        {
            var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            if (ApiDeArchivos.EsUnComprimido(archivo.Nombre))
            {
                return TrabajosDelSistemaDocumental.SometerImportarZip(Contexto,
                    pago.Id, idArchivo,
                    remplazar: true,
                    renombrar: false,
                    eliminarArchivo: false,
                    eliminarCarpeta: false) == null;
            }

            GestorDeVinculos.Vincular(Contexto, Negocio, enumNegocio.Archivos, pago.Id, archivo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
            var factura = pago.FacturaRec(Contexto, errorSiNoHay: false);
            if (factura != null && factura.IdArchivo != archivo.Id)
            {
                ServidorDocumental.ProcesarOperacion(Contexto, enumOperacionesConArchivos.Enlazar, Negocio.IdNegocio(), pago.Id, enumNegocio.FacturaRecibida, factura.Id, new List<long> { archivo.Id }, validarPermisos: false, errorSiNoVinculado: false);
                if (factura.IdArchivo != null)
                {
                    var anexados = pago.LeerAnexados(Contexto);
                    if (!anexados.Any(x => x.Id == factura.IdArchivo))
                    {
                        ServidorDocumental.ProcesarOperacion(Contexto, enumOperacionesConArchivos.Enlazar, enumNegocio.FacturaRecibida.IdNegocio(), factura.Id, Negocio, pago.Id,
                            new List<long> { factura.IdArchivo.Entero() }, validarPermisos: false, errorSiNoVinculado: false);
                    }
                }
            }
            return true;
        }

        private void MapearDatosDeLaCuentaAcreedora(PagoDtm pago, PagoDto elemento)
        {
            var cba = pago.CuentaBancariaAcreedora(Contexto);
            elemento.MapearCuentaBancaria(cba);
            var cache = ServicioDeCaches.Obtener(CacheDe.Pag_DatosDelPagoDto);
            if (!cache.ContainsKey(elemento.Id.ToString()))
            {
                MapearDatosDeCuentaAcreedora(pago, elemento);
                elemento.BancoAcreedor = cba.Banco(Contexto).Nombre;
            }
            else
            {
                elemento.BancoAcreedor = ((PagoDto)cache[elemento.Id.ToString()]).BancoAcreedor;
                elemento.Activa = ((PagoDto)cache[elemento.Id.ToString()]).Activa;
                elemento.Alias = ((PagoDto)cache[elemento.Id.ToString()]).Alias;
                elemento.BancoAcreedor = ((PagoDto)cache[elemento.Id.ToString()]).BancoAcreedor;
            }
        }

        private void MapearDatosDeAcreedor(PagoDtm pago, PagoDto elemento)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Pag_DatosDelPagoDto);
            if (!cache.ContainsKey(elemento.Id.ToString()))
            {
                elemento.Solicitante = pago.Solicitante(Contexto).Expresion;
                if (!pago.EsAbono(Contexto))
                {
                    if (pago.IdFacturaRec is not null)
                    {
                        elemento.Proveedor = pago.Proveedor(Contexto, errorSiNoHay: false)?.Expresion ?? null;
                    }
                    else
                    {
                        elemento.Trabajador = pago.Trabajador(Contexto, errorSiNoHay: false)?.Expresion ?? null;
                    }
                }
                else
                {
                    elemento.Cliente = pago.Cliente(Contexto).Expresion;
                }
                cache[elemento.Id.ToString()] = elemento;
            }
            else
            {
                elemento.Solicitante = ((PagoDto)cache[elemento.Id.ToString()]).Solicitante;
                elemento.Proveedor = ((PagoDto)cache[elemento.Id.ToString()]).Proveedor;
                elemento.Trabajador = ((PagoDto)cache[elemento.Id.ToString()]).Trabajador;
                elemento.Cliente = ((PagoDto)cache[elemento.Id.ToString()]).Cliente;
            }
        }

        private void MapearDatosDeCuentaAcreedora(PagoDtm pago, PagoDto elemento)
        {
            if (!pago.EsAbono(Contexto))
            {
                if (pago.IdProveedor is not null)
                {
                    var proveedor = Contexto.SeleccionarPorId<ProveedorDtm>((int)pago.IdProveedor);
                    var cbDeProveedor = proveedor.CuentaDeProveedor(Contexto, enumClaseDeCuentaBancaria.Ingreso, soloLasActivas: false, errorSiNoHay: false);
                    if (cbDeProveedor is null)
                    {
                        cbDeProveedor = proveedor.AsociarCuenta(Contexto, elemento.Alias, enumClaseDeCuentaBancaria.Ingreso, pago.CuentaDeAcreedor);
                        //GestorDeErrores.Emitir($"Se ha indicado un proveedor de pago, y la cuenta acreedora {pago.CuentaBancariaAcreedora(Contexto).NumeroIban} no es una cuenta de proveedor");
                    }
                    MapearDatosDeCuenta(elemento, cbDeProveedor);
                    return;
                }
                if (pago.IdTrabajador is not null)
                {
                    var trabajador = Contexto.SeleccionarPorId<TrabajadorDtm>((int)pago.IdTrabajador);
                    var cbDeTrabajador = trabajador.CuentaDeTrabajador(Contexto, enumClaseDeCuentaBancaria.Ingreso, errorSiNoHay: false);
                    if (cbDeTrabajador is null)
                        GestorDeErrores.Emitir($"Se ha indicado un trabajador para el pago, y la cuenta acreedora {pago.CuentaBancariaAcreedora(Contexto).NumeroIban} no es una cuenta de un trabajador");
                    MapearDatosDeCuenta(elemento, cbDeTrabajador);
                    return;
                }
            }

            var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(pago.IdSolicitante);
            if (pago.EsAbono(Contexto))
            {
                var cliente = inter.Cliente(Contexto, crearCliente: false);
                var cuentaDeCliente = cliente.CuentaDeCliente(Contexto, enumClaseDeCuentaBancaria.Ingreso);
                MapearDatosDeCuenta(elemento, cuentaDeCliente);
                return;
            }
            var cbDeAcreedor = inter.CuentaDeInterlocutor(Contexto, enumClaseDeCuentaBancaria.Ingreso, errorSiNoHay: false);
            if (cbDeAcreedor is null)
                GestorDeErrores.Emitir($"Se ha indicado un acreedor para el pago, y la cuenta acreedora {pago.CuentaBancariaAcreedora(Contexto).NumeroIban} no es una cuenta de acreedor");
            MapearDatosDeCuenta(elemento, cbDeAcreedor);
        }

        private void MapearDatosDeCuenta(PagoDto elemento, IUsaCuentaBancaria cuenta)
        {
            elemento.Activa = cuenta.Activa;
            elemento.Alias = cuenta.Alias;
            elemento.BancoAcreedor = cuenta.Cuenta(Contexto).Banco(Contexto).Nombre;
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (vinculado == enumNegocio.Archivador || vinculado == enumNegocio.Archivos)
            {
                var idPago = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
                var pago = entorno.Contexto.SeleccionarPorId<PagoDtm>(idPago);
                if (entorno.Parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, true) && !pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente))
                {
                    GestorDeErrores.Emitir($"No pueden quitar del {enumNegocio.Pago.Singular(true)} '{pago.Referencia}' archivadores ni archivos por no estar en la etapa '{enumEtapasDePagos.PAG_Etapa_Pendiente.Nombre(true)}'");
                }
            }
        }

        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idPago = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var pago = entorno.Contexto.SeleccionarPorId<PagoDtm>(idPago);
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (vinculado == enumNegocio.FacturaRecibida)
            {
                pago.IdFacturaRec = null;
                pago.Modificar(entorno.Contexto, accionEjecutada: ApiDeEnsamblados.DespuesDeQuitarVinculo);
            }
            else if (vinculado == enumNegocio.Archivos)
            {
                var idarchivo = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                if (pago.IdFacturaRec is not null && pago.FacturaRec(entorno.Contexto).IdArchivo != idarchivo)
                {
                    GestorDeVinculos.BorrarVinculo(entorno.Contexto, enumNegocio.FacturaRecibida, enumNegocio.Archivos, (int)pago.IdFacturaRec, idarchivo,
                        new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }
            }
            else if (vinculado == enumNegocio.CircuitoDoc)
            {
                var idCircuito = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                var circuito = entorno.Contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
                if (circuito.VinculadosAl(entorno.Contexto).Count == 0 && circuito.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto))
                    circuito.TransitarALaEtapa(entorno.Contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.EstadosDeLaEtapa());
            }
        }


        public static void DespuesDeVincular(EntornoDeUnaAccion entorno)
        {
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (vinculado == enumNegocio.Archivos)
            {
                var contexto = entorno.Contexto;
                var idPago = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
                var pago = contexto.SeleccionarPorId<PagoDtm>(idPago);
                if (pago.IdFacturaRec != null)
                {
                    var idArchivo = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));

                    var anexadosFactura = pago.FacturaRec(contexto).LeerAnexados(contexto);
                    if (anexadosFactura.Count > 0 && !anexadosFactura.Any(a => a.Id == idArchivo))
                        GestorDeVinculos.Vincular(contexto, enumNegocio.FacturaRecibida, enumNegocio.Archivos, (int)pago.IdFacturaRec, idArchivo, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });

                    var archivosDelPago = pago.LeerAnexados(contexto);
                    if (archivosDelPago.Count == 1 && archivosDelPago.Any(a => a.Id == idArchivo))
                    {
                        pago.IntentarCerrarPagoDeFactura(contexto, hayJustificante: true);
                    }
                }
            }
        }

        public static void GenerarJustificante(ContextoSe contexto, int id)
        {
            var pago = contexto.SeleccionarPorId<PagoDtm>(id);

            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Pag-{pago.Referencia}.{enumExtensiones.pdf}".NormalizarFichero());
            var PagoRpt = new GeneradorDePagoRpt(contexto, pago).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDePago(PagoRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(contexto, enumNegocio.Pago, enumNegocio.Archivos, pago.Id, idArchivo);

        }

        public async Task<TotalesDePago> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return await Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDePago ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorEtapa.ToLower() || x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.InvariantCultureIgnoreCase));
            if (filtro == null) filtros.Add(new ClausulaDeFiltrado
            {
                Clausula = ltrFiltros.FiltroPorEtapa,
                Criterio = enumCriteriosDeFiltrado.noEsNingunoDe,
                Valor = enumEtapasDePagos.PAG_Etapa_Pendiente.ToString()
            }
            );

            var pagos = Contexto.SeleccionarTodos<PagoDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales},
            });
            var totales = new TotalesDePago();
            totales.TotalPagos = pagos.Where(x => !x.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Cancelado)).Sum(x => x.Importe);
            totales.TotalPagado = pagos.Where(x => x.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado)).Sum(x => x.Importe);
            totales.TotalPendiente = pagos.Where(x => !x.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePagos> {
                enumEtapasDePagos.PAG_Etapa_Pagado,
                enumEtapasDePagos.PAG_Etapa_Cancelado
            })).Sum(x => x.Importe);
            totales.PagosContado = pagos.Where(x => x.Clase == enumClaseDePago.Contado && x.ModoDePago == enumModoDePagoContado.Contado).Sum(x => x.Importe);
            totales.PagosDomiciliados = pagos.Where(x => x.Clase == enumClaseDePago.Contado && x.ModoDePago == enumModoDePagoContado.Domiciliacion).Sum(x => x.Importe);
            totales.PagosTarjeta = pagos.Where(x => x.Clase == enumClaseDePago.Contado && x.ModoDePago == enumModoDePagoContado.Tarjeta).Sum(x => x.Importe);
            totales.PagosRemesados = pagos.Where(x => x.Clase == enumClaseDePago.Remesa).Sum(x => x.Importe);
            totales.PagosTransferidos = pagos.Where(x => x.Clase == enumClaseDePago.Transferencia).Sum(x => x.Importe);
            totales.Procesados = pagos.Count();
            return totales;
        }

        public void GenerarPreasiento(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var pago = Contexto.SeleccionarPorId<PagoDtm>(id);

                    if (!pago.Sociedad(Contexto).UsaPreasientos(Contexto, enumNegocio.Pago))
                        GestorDeErrores.Emitir($"La sociedad del pago '{pago.Referencia}' no usa preasiento de pago, configúrela en '{enumParametrosDePreasiento.SPR_Generar_Preasiento_De_Pago}'");

                    if (!pago.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir($"Ha de ser administrador del pago '{pago.Referencia}' para poder preasentarla");

                    if (!pago.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePagos> { enumEtapasDePagos.PAG_Etapa_Pagado }))
                        GestorDeErrores.Emitir($"No se puede generar un preasiento de '{pago.Referencia}' por no estar en la etapa correcta");

                    var preasientoAnterior = pago.Preasiento(Contexto, errorSiNoHay: false);
                    var cancelado = preasientoAnterior?.Estado(Contexto).Cancelado ?? false;
                    if (pago.IdPreasiento is not null && !cancelado)
                    {
                        pago.CancelarPreasiento(Contexto);
                    }

                    pago = pago.Preasentar(Contexto);
                    pago.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnPago.Accion_GenerarPreasiento);

                    if (preasientoAnterior is not null && !cancelado)
                        pago.CrearTraza(Contexto, "Preasiento regenerado", $"El preasiento '{preasientoAnterior.Referencia}' se ha desasociado {preasientoAnterior.NegocioReferenciado.ConArticulo()}{(preasientoAnterior.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado) ? ", recuerde anular el asiento contable asociado" : "")}");

                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }

        public int CancelarPreasientos(List<int> ids)
        {
            if (ids.Count == 0) GestorDeErrores.Emitir("No ha indicado pagos a las que cancelarle el preasiento");
            var cancelados = 0;
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var pagos = Contexto.SeleccionarPorId<PagoDtm>(id);

                    if (!pagos.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir($"Ha de ser administrador de la pago '{pagos.Referencia}' para poder cancelar el preasiento");

                    var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
                    if (!idsDeUsuarios.Contains(Contexto.DatosDeConexion.IdUsuario))
                        GestorDeErrores.Emitir($"Incluya al usuario conectado en el parámetro  '{enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar}' para poder cancelar preasientos");

                    if (pagos.IdPreasiento is not null)
                    {
                        pagos.CancelarPreasiento(Contexto);
                        cancelados++;
                    }
                }
                Contexto.Commit(trans);
                return cancelados;
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }


    }
}
