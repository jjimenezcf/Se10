using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using ModeloDeDto.Terceros;
using ModeloXml.eFactura.Facturae322;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using ServicioDeReportes.Gastos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Gastos
{

    public class GestorDeFacturasRec : GestorDeElementos<ContextoSe, FacturaRecDtm, FacturaRecDto>, IEsImputable, IImportadorDelCorreo, ITotalizador<TotalesDeFacturasRec>, IGeneradorDePreasiento
    {
        public class MapearFacturaRec : Profile
        {
            public MapearFacturaRec()
            {
                CreateMap<FacturaRecDtm, FacturaRecDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Naturaleza, dtm => dtm.MapFrom(dtm => dtm.Naturaleza == null ? null : dtm.Naturaleza.Nombre))
                .ForMember(dto => dto.Expediente, dtm => dtm.MapFrom(dtm => dtm.Expediente == null ? null : dtm.Expediente.Expresion))
                .ForMember(dto => dto.Proveedor, dtm => dtm.MapFrom(dtm => dtm.Proveedor == null ? null : dtm.Proveedor.Expresion))
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato == null ? null : dtm.Contrato.Expresion));

                CreateMap<FacturaRecDto, FacturaRecDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Proveedor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Expediente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.FacturaRecibida;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeFacturaRec.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeFacturasRec(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeFacturasRec Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeFacturasRec(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(FacturaRecDto dto, FacturaRecDtm dtm, ParametrosDeNegocio opciones)
        {
            if (opciones.Insertando) opciones.Parametros[nameof(FacturaRecDto)] = dto;
        }

        protected override IQueryable<FacturaRecDtm> AplicarJoins(IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Proveedor);
            consulta = consulta.Include(x => x.Contrato);
            return consulta;
        }

        protected override IQueryable<FacturaRecDtm> AplicarOrden(IQueryable<FacturaRecDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<FacturaRecDtm> AplicarFiltros(IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnaFacturaRec.IdContrato,
                ltrDeUnaFacturaRec.FacturasPosiblesDelContrato,
                ltrDeUnaFacturaRec.IdExpediente,
                ltrDeUnaFacturaRec.FacturasImputablesEnUnExpediente
            });

            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorEjercicioDeFactura(filtros);
            consulta = consulta.FiltroPorBaseImponible(filtros);
            consulta = consulta.FiltroPorTotalFactura(filtros);
            consulta = consulta.FiltroPorProveedor(Contexto, filtros);
            consulta = consulta.FiltroSiHayPreasiento(Contexto, filtros);
            consulta = consulta.FiltroPorNumeroDeFactura(filtros);
            consulta = consulta.FiltroPorAsuntoReferenciaNumeroDeFactura(Contexto, filtros);
            consulta = consulta.FiltroParaExcluirLasRectificadas(Contexto, filtros);
            consulta = consulta.FiltroPorFechaDeEmision(filtros);
            consulta = consulta.FiltroPorFechaDeVencimiento(filtros);
            consulta = consulta.FiltroSelectorDeFacturaRemesadas(Contexto, filtros);
            consulta = consulta.FiltroPorRemesa(Contexto, filtros);
            //consulta = consulta.FiltroPorEtapa(filtros);
            consulta = consulta.FiltroPorFormaDePago(Contexto, filtros);
            consulta = consulta.FiltroPorIvaRetencion(Contexto, filtros);
            consulta = consulta.FiltroFacturasPosiblesDelContrato(Contexto, filtros);
            consulta = consulta.FiltroFacturasPosiblesDeUnExpediente(Contexto, filtros, parametros);
            consulta = consulta.FiltroFacturasEnUnaEstimacion(Contexto, filtros, parametros);
            consulta = consulta.FiltroFacturasEnUnLoteContable(Contexto, filtros, parametros);
            consulta = consulta.FiltroFacturasAsociablesAUnElemento(Contexto, filtros, parametros);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(FacturaRecDtm.IdExpediente), ltrDeUnaFacturaRec.AsociadaAUnExpediente, parametros, aplicarFiltroDeEstado: false);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(FacturaRecDtm.IdContrato), ltrDeUnaFacturaRec.AsociadaAUnContrato, parametros, aplicarFiltroDeEstado: false);
            consulta = consulta.FiltroPorNaturaleza(Contexto, filtros);
            if (parametros.Peticion == enumPeticion.epTotales)
                consulta = consulta.ExcluirLasNoTotalizables();

            return consulta;
        }

        protected override IQueryable<FacturaRecDtm> AplicarSeguridad(IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<FacturaRecDtm, TipoDeFacturaRecDtm, PermisoDeLaFacturaRecDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<FacturaRecDtm, PermisoDeLaFacturaRecDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(far, parametros);
            far.IdNaturaleza = far.IdNaturaleza == 0 ? null : far.IdNaturaleza;
            far.InicializarDatosProveedor(Contexto, parametros, validarEnAlta: parametros.Insertando);
            if (parametros.Insertando)
            {
                var dto = parametros.Parametros.LeerValor(nameof(FacturaRecDto), (FacturaRecDto)null);
                if (dto != null)
                {
                    if (dto.IdArchivo is null && dto.IdJustificanteDePago is not null)
                        Emitir($"No se puede indicar un justificante de pago, y no indicar el archivo factura");

                    if (dto.Pagada == false && dto.IdJustificanteDePago is not null)
                        Emitir($"No se puede indicar un justificante de pago, y no indicar cómo es el pago");
                }
            }
            if (parametros.Modificando)
            {
                if (far.IdArchivo is null && ((FacturaRecDtm)parametros.registroEnBd).IdArchivo is not null)
                    far.IdArchivo = ((FacturaRecDtm)parametros.registroEnBd).IdArchivo;


                var contratoCambiado = far.PropiedadCambiada<int?>(nameof(FacturaRecDtm.IdContrato), parametros);
                if (contratoCambiado && far.IdContrato is not null && far.Contrato(Contexto).Datos(Contexto).IdProveedor != far.IdProveedor)
                {
                    Emitir($"El proveedor de la factura '{far.Referencia}' no coincide con el del contrato '{far.Contrato(Contexto).Referencia}'");
                }

                ValidarEtapaDeExpediente(far, parametros);
            }


            var modificandoDatos = parametros.Modificando && parametros.AccionQueSeEjecuta == ltrDeUnaFacturaRec.Accion_ModificarIva || parametros.AccionQueSeEjecuta == ltrDeUnaFacturaRec.Accion_ModificarNaturalezas;

            if (!modificandoDatos) ValidarDatosNoModificablesSiNoEstaCumplimentandose(far, parametros);

            if (far.RecibidaEl.ConFecha() && far.FacturadaEl.ConFecha() && far.FacturadaEl > far.RecibidaEl)
            {
                Emitir($"la factura no se puedo emitir después de su recepción");
            }

            if (far.RecibidaEl.ConFecha() && far.RecibidaEl.Date > DateTime.Now.Date)
                Emitir($"la fecha de recepción de la factura no puede ser mayor de la del día");
            if (far.RecibidaEl.ConFecha() && far.FacturadaEl.Date > DateTime.Now.Date)
                Emitir($"la fecha de emisión de la factura no puede ser mayor de la del día");

            if (parametros.AccionQueSeEjecuta != ltrDeUnaFacturaRec.Accion_GenerarPreasiento)
            {
                if (far.EstaEnEtapaNoContabilizada())
                {
                    var preasientoDeFactura = Contexto.Set<PreasientoDtm>().FirstOrDefault(prea => prea.IdReferenciado == far.Id && prea.NegocioReferenciado == Negocio);
                    if (preasientoDeFactura == null)
                        far.IdPreasiento = null;
                    else
                    {
                        far.IdPreasiento = preasientoDeFactura.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado) ? null : far.IdPreasiento;
                    }
                }
                else
                {
                    if (parametros.AccionQueSeEjecuta != ltrDeUnaFacturaRec.Accion_EnviarAPagar && parametros.AccionQueSeEjecuta != ltrDeUnaFacturaRec.Accion_DarPorPagada)
                        far.IdPreasiento = ((FacturaRecDtm)parametros.registroEnBd).IdPreasiento;
                }
            }
            if (!modificandoDatos) ValidacionesSiNoSeModificanDatos(far, parametros);
        }

        private void ValidacionesSiNoSeModificanDatos(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            if (far.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion))
            {
                var poniendoQuitandoContrato = parametros.AccionQueSeEjecuta != ltrDeUnaFacturaRec.Accion_QuitarContrato || parametros.AccionQueSeEjecuta != ltrDeUnaFacturaRec.Accion_ImputarContrato;

                if (!parametros.ProcesandoTransicion(far) && !poniendoQuitandoContrato)
                {
                    if (far.ContabilizadaEl is null) Emitir($"Se debe indicar la fecha de contabilización de la factura {far.Referencia}");
                    if (far.ContabilizadaEl.Fecha() < far.RecibidaEl) Emitir($"No se puede contabilizar la factura {far.Referencia} con fecha '{far.ContabilizadaEl.Fecha().ToShortDateString()}' ya que se recibió '{far.RecibidaEl.ToShortDateString()}'");
                }
            }

            var validaNumeroDeFactura = parametros.TransicionQueSeEjecuta == null || !parametros.TransicionQueSeEjecuta.EsCancelado;
            if (validaNumeroDeFactura) far.ValidarNumeroDeFactura(Contexto);

            if (parametros.Insertando || far.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
            {
                if (far.IdContrato is not null)
                {
                    var etapasValidas = far.Contrato(Contexto).EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeContratos> { enumEtapasDeContratos.CTR_Etapa_Vigente, enumEtapasDeContratos.CTR_Etapa_Finalizacion });
                    if (!etapasValidas)
                        Emitir($"Al contrato '{far.Contrato(Contexto).Referencia}' no se le puede imputar la factura '{(parametros.Insertando ? "que está creando" : far.Referencia)}' ya que ha de estar en la etapa '{enumEtapasDeContratos.CTR_Etapa_Vigente.Nombre()}' o en '{enumEtapasDeContratos.CTR_Etapa_Finalizacion.Nombre()}' ");
                }
            }

            if (far.IdExpediente is not null)
            {
                ValidarEtapaDeExpediente(far, parametros);
            }

        }

        private void ValidarEtapaDeExpediente(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            if (parametros.Modificando)
            {
                var expedienteCambiado = far.PropiedadCambiada<int?>(nameof(FacturaRecDtm.IdExpediente), parametros);

                if (expedienteCambiado && far.IdExpediente is null)
                {
                    var expedienteant = ((FacturaRecDtm)parametros.registroEnBd).Expediente(Contexto);
                    var etapas = expedienteant.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeExpedientes> { enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enumEtapasDeExpedientes.EXP_Etapa_Terminada });

                    if (!etapas)
                    {
                        Emitir($"No se puede eliminar la factura '{far.Referencia}' del expediente '{expedienteant.Referencia}' ya que ha de estar en la etapa '{enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.Nombre()}' o en '{enumEtapasDeExpedientes.EXP_Etapa_Terminada.Nombre()}' ");
                    }
                }
                return;
            }

            var etapasValidas = far.Expediente(Contexto).EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeExpedientes> { enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enumEtapasDeExpedientes.EXP_Etapa_Terminada });
            if (!etapasValidas)
                Emitir($"El Expediente '{far.Expediente(Contexto).Referencia}' no se le puede imputar la factura '{(parametros.Insertando ? "que está creando" : far.Referencia)}' ya que ha de estar en la etapa '{enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.Nombre()}' o en '{enumEtapasDeExpedientes.EXP_Etapa_Terminada.Nombre()}' ");
        }

        private void ValidarDatosNoModificablesSiNoEstaCumplimentandose(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            if (far.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
                return;

            var anterior = (FacturaRecDtm)parametros.registroEnBd;
            far.IdArchivo = anterior.IdArchivo;
            far.IdProveedor = anterior.IdProveedor;

            far.ValidarQueSonIguales(anterior, solo: new List<string> {
                nameof(FacturaRecDtm.Contacto),
                nameof(FacturaRecDtm.Telefono),
                nameof(FacturaRecDtm.eMail),
                $"En la factura '{far.Referencia}' los datos del proveedor no son modioficables, ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}"
            });

            if (far.Numero != anterior.Numero)
                Emitir($"No se puede modificar el número de la factura '{far.Referencia}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}");

            if (far.FacturadaEl.Date != anterior.FacturadaEl.Date)
                Emitir($"En la factura '{far.Referencia}' la fecha de emisión '{far.FacturadaEl.ToShortDateString()}' no es modificable por '{anterior.FacturadaEl.ToShortDateString()}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}");
            if (far.RecibidaEl.Date != anterior.RecibidaEl.Date)
                Emitir($"En la factura '{far.Referencia}' la fecha de recepción '{far.RecibidaEl.ToShortDateString()}' no es modificable por '{anterior.RecibidaEl.ToShortDateString()}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}");
            if (far.VenceEl.Date != anterior.VenceEl.Date)
                Emitir($"En la factura '{far.Referencia}' la fecha de vencimiento '{far.VenceEl.ToShortDateString()}' no es modificable por '{anterior.VenceEl.ToShortDateString()}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}");

            if (!far.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion) && far.ContabilizadaEl != anterior.ContabilizadaEl)
                if (!parametros.ProcesandoTransicion(far))
                    Emitir($"En la factura '{far.Referencia}' la fecha de contabilización '{far.ContabilizadaEl.Fecha().ToShortDateString()}' no es modificable por '{far.ContabilizadaEl.Fecha().ToShortDateString()}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Nombre(true)}");

            if (far.HayDiferenciaEntreLasBi(anterior.BaseImponible))
                Emitir($"No se puede modificar la BI de la factura '{far.Referencia}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}");

            if (far.HayDiferenciaEntreLosTotalPagar(anterior.TotalDelPago))
                Emitir($"No se puede modificar el total a pagar de la factura '{far.Referencia}' ya que no está en la etapa {enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}");
        }

        protected override void DespuesDePersistir(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(far, parametros);

            if (parametros.Insertando)
            {
                DespuesDeCrear(far, parametros);
            }
            else if (parametros.Modificando)
            {
                DespuesDeModificar(far, parametros);
            }

            if (far.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
                far.ProcesarArchivo(Contexto, parametros);
        }

        protected override void EliminarCaches(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(far, parametros);
            if (far.PropiedadCambiada<int?>(nameof(FacturaRecDtm.IdExpediente), parametros))
            {
                var indice = far.IdExpediente is null ? ((FacturaRecDtm)parametros.registroEnBd).IdExpediente.ToString() : far.IdExpediente.ToString();
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_Gastos, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_Presupuestado, indice);
            }
        }

        private void DespuesDeModificar(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            if (far.PropiedadCambiada<DateTime>(nameof(FacturaRecDtm.VenceEl), parametros))
            {
                far.ModificarEventoDeVencimiento(Contexto);
            }

            var contratoCambiado = far.PropiedadCambiada<int?>(nameof(FacturaRecDtm.IdContrato), parametros);
            if (contratoCambiado)
            {
                far.ModificarSaldosDelContrato(Contexto, (FacturaRecDtm)parametros.registroEnBd);
            }

            var biCambiada = far.PropiedadCambiada<decimal>(nameof(FacturaRecDtm.BaseImponible), parametros);
            if (biCambiada && far.IdContrato is not null)
            {
                far.AjustarSaldosDelContrato(Contexto, (FacturaRecDtm)parametros.registroEnBd);
            }

            var expedienteCambiado = far.PropiedadCambiada<int?>(nameof(FacturaRecDtm.IdExpediente), parametros);
            if (expedienteCambiado && far.IdExpediente is null)
            {
                var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>((int)((FacturaRecDtm)parametros.registroEnBd).IdExpediente);
                expediente.CrearTraza(Contexto, "Factura recibida eliminada", $"Se ha eliminado la factura '{far.Referencia}' del expediente");
            }
            ServicioDeCaches.EliminarElemento(CacheDe.Far_QuienMeRectifica, far.Id.ToString());
        }

        private void DespuesDeCrear(FacturaRecDtm far, ParametrosDeNegocio parametros)
        {
            var proveedor = far.Proveedor(Contexto);
            var direccionDelProveedor = proveedor.DireccionFiscal(Contexto, errorSiNoHay: false);
            if (direccionDelProveedor != null)
            {
                direccionDelProveedor.Id = 0;
                var gestor = GestorDeDirecciones.Gestor(Contexto, Negocio);
                var direccion = gestor.MapearRegistro(direccionDelProveedor, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                direccion.IdElemento = far.Id;
                gestor.PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } } });
            }

            var dto = parametros.Parametros.LeerValor(nameof(FacturaRecDto), (FacturaRecDto)null);
            if (dto != null)
            {
                var detalleCreado = CrearDetalle(dto, far, parametros);
                if (dto.Pagada && !detalleCreado)
                {
                    Emitir($"Para poder crear el pago, ha de dar la información necesaria para crear el detalle de la factura, revise si falta el iva o el irpf");
                }

                if (dto.Pagada)
                {
                    var pago = dto.PrepararPagoContado(Contexto);
                    if (pago != null)
                    {
                        pago.IdFacturaRec = far.Id;
                        var idJustificanteDePago = dto.IdJustificanteDePago.ToString().Entero();
                        pago.Insertar(Contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_CrearPagoAlCrearFactura, new Dictionary<string, object> {
                            { nameof(PagoDto.IdArchivoAlCrear) , idJustificanteDePago }
                        });
                    }

                    if (!parametros.Parametros.LeerValor(ltrDeUnaFacturaRec.TrazaDeCopiaDeFactura, "").IsNullOrEmpty())
                        new TrazasDeUnaFacturaRecDtm
                        {
                            IdElemento = far.Id,
                            Nombre = "Factura creada por copia",
                            Descripcion = (string)parametros.Parametros[ltrDeUnaFacturaRec.TrazaDeCopiaDeFactura]
                        }
                        .Insertar(Contexto);
                }
            }

            if (far.IdContrato is not null)
            {
                far.AjustarSaldosDelContrato(Contexto, anterior: null);
            }
        }

        protected bool CrearDetalle(FacturaRecDto dto, FacturaRecDtm dtm, ParametrosDeNegocio parametros)
        {
            var cant = dto.Cantidad ?? 0;
            var detalleCreado = false;
            if (cant == 0)
            {
                return detalleCreado;
            }

            // sin unidad de medida o sin naturaleza contable o sin BI o sin total a pagar, he de indicar que para poder crear una línea he de indicar dichos campos
            if (dto.IdUnidad.Entero() == 0 || dto.IdNaturaleza.Entero() == 0 || dto.BaseImponible == 0 || dto.TotalDelPago == 0 || dto.IdIvaS.Entero() == 0)
                Emitir($"Ha indicado una cantidad '{cant.Formatear(alineacion: false)}'. Si quiere crear una línea de factura, es necesario indicar la unidad, la naturaleza, el Iva, (irpf si lo hay) la BI y el total a pagar");

            var bi = Math.Abs(dto.BaseImponible);
            var tp = Math.Abs(dto.TotalDelPago);
            var iva = Contexto.SeleccionarPorId<IvaSoportadoDtm>(dto.IdIvaS.Entero(), aplicarJoin: true, errorSiNoHay: false);
            var irpf = Contexto.SeleccionarPorId<IrpfDtm>(dto.IdIrpf.Entero(), errorSiNoHay: false);
            var pIva = iva?.Porcentaje ?? 0;
            var pIrpf = irpf?.Porcentaje ?? 0;

            if (iva is null)
            {
                Emitir($"Si indica una cantidad, ha de indicar el iva de la factura, aun siendo '{enumClasesDeIvaSop.NSJ.Descripcion()}' o de '{enumClasesDeIvaSop.ISP.Descripcion()}' para poder crear una línea de detalle");
            }

            var totalCalculado = bi + (bi * pIva / 100) - (bi * pIrpf / 100);
            if (Math.Abs(totalCalculado - tp) > VariableDeFacturasRec.ToleranciaEnImportes())
            {
                Emitir($"Esta solicitando crear una factura con una bi de '{bi.Moneda(alineacion: false)}' y un iva del '{pIva.Porcentaje()}' y un irpf del '{pIrpf.Porcentaje()}' con un total a pagar de '{tp.Moneda(alineacion: false)}' y esto no cumple que: bi + %iva = total a pagar");
            }

            // - si viene la naturaleza contable, unidad de medida, la BI > 0 el total a pagar = BI y no hay iva e irpf --> creo una linea de BI
            if (dto.IdNaturaleza.Entero() > 0 && dto.IdUnidad.Entero() > 0 && bi > 0 && bi == tp)
            {
                if (irpf is null && iva.Porcentaje > 0)
                {
                    Emitir($"Esta solicitando crear una factura con una bi de '{bi.Moneda(alineacion: false)}' y un iva del '{pIva.Porcentaje()}' y con un total a pagar de '{tp.Moneda(alineacion: false)}' y esto no cumple que: bi + %iva = total a pagar");
                }
                else if (pIva - pIrpf != 0)
                {
                    Emitir($"Esta solicitando crear una factura con una bi de '{bi.Moneda(alineacion: false)}' , con un iva del '{pIva.Porcentaje()}' un irpf del '{pIrpf.Porcentaje()}' y con un total a pagar de '{tp.Moneda(alineacion: false)}' y esto no cumple que: bi - %irpf = total a pagar");
                }

                CrearLineaConIvaExentoIsp(dto, dtm, iva);
                detalleCreado = true;
            }

            // - si viene la naturaleza contable, unidad de medida, la BI > 0 el total a pagar y el iva  y aplicado el iva a la BI = TP --> creo dos lineas, la de la BI y la del IVA
            //   si no cuadrase con el tp, doy error
            else if (dto.IdNaturaleza.Entero() > 0 && dto.IdUnidad.Entero() > 0 && pIva > 0 && pIrpf == 0)
            {
                CrearLineaDeBI(dto, dtm);
                CrearLineaDeIva(dto, dtm, pIva);
                detalleCreado = true;
            }

            // - si viene la naturaleza contable, unidad de medida, la BI > 0 el total a pagar, el irpf y  BI - IRPF = TP --> creo dos líneas,
            //   la de la BI, la del irpf
            //   si no cuadrase con el tp, doy error
            else if (dto.IdNaturaleza.Entero() > 0 && dto.IdUnidad.Entero() > 0 && pIva == 0 && pIrpf > 0)
            {
                CrearLineaDeBI(dto, dtm);
                CrearLineaDeIrpf(dto, dtm, pIrpf);
                detalleCreado = true;
            }

            // - si viene la naturaleza contable, unidad de medida, la BI > 0 el total a pagar, el iva y el irpf y aplicado el iva a la BI - el irpf = TP --> creo tres líneas,
            //   la de la BI, la del iva y la del irpf
            //   si no cuadrase con el tp, doy error
            else if (dto.IdNaturaleza.Entero() > 0 && dto.IdUnidad.Entero() > 0 && bi > 0 && pIva > 0 && pIrpf > 0)
            {
                CrearLineaDeBI(dto, dtm);
                CrearLineaDeIva(dto, dtm, pIva);
                CrearLineaDeIrpf(dto, dtm, pIrpf);
                detalleCreado = true;
            }

            return detalleCreado;

        }

        private void CrearLineaDeIrpf(FacturaRecDto dto, FacturaRecDtm dtm, decimal pIrpf)
        {
            var orden = enumNegocio.FacturaRecibida.LeerCrearParametro(Contexto, enumParametrosDeFacturasRec.FAR_IncrementarOrdenEn, "10").Valor.Entero();
            new LineaDeUnaFarDtm
            {
                IdElemento = dtm.Id,
                Orden = orden * 3,
                Clase = Enumerados.enumClaseDeLineaFar.LineaDeIrpf,
                Concepto = dto.Nombre,
                BaseImponible = dto.BaseImponible,
                IdIrpf = dto.IdIrpf,
                PorcentajeIrpf = pIrpf
            }.Insertar(Contexto);
        }

        private void CrearLineaDeIva(FacturaRecDto dto, FacturaRecDtm dtm, decimal pIva)
        {

            var orden = enumNegocio.FacturaRecibida.LeerCrearParametro(Contexto, enumParametrosDeFacturasRec.FAR_IncrementarOrdenEn, "10").Valor.Entero();
            new LineaDeUnaFarDtm
            {
                IdElemento = dtm.Id,
                Orden = orden * 2,
                Clase = Enumerados.enumClaseDeLineaFar.LineaDeIva,
                Concepto = dto.Nombre,
                BaseImponible = dto.BaseImponible,
                IdIvaS = dto.IdIvaS,
                PorcentajeIva = pIva
            }.Insertar(Contexto);
        }

        private void CrearLineaDeBI(FacturaRecDto dto, FacturaRecDtm dtm)
        {
            var orden = enumNegocio.FacturaRecibida.LeerCrearParametro(Contexto, enumParametrosDeFacturasRec.FAR_IncrementarOrdenEn, "10").Valor.Entero();
            new LineaDeUnaFarDtm
            {
                IdElemento = dtm.Id,
                Orden = orden,
                Clase = Enumerados.enumClaseDeLineaFar.BaseImponible,
                Concepto = dto.Nombre,
                BaseImponible = dto.BaseImponible,
                Cantidad = dto.Cantidad,
                IdUnidad = dto.IdUnidad,
                IdNaturaleza = dto.IdNaturaleza
            }.Insertar(Contexto);
        }

        private void CrearLineaConIvaExentoIsp(FacturaRecDto dto, FacturaRecDtm dtm, IvaSoportadoDtm iva)
        {
            var orden = enumNegocio.FacturaRecibida.LeerCrearParametro(Contexto, enumParametrosDeFacturasRec.FAR_IncrementarOrdenEn, "10").Valor.Entero();
            new LineaDeUnaFarDtm
            {
                IdElemento = dtm.Id,
                Orden = orden,
                Clase = enumClasesDeIvaSop.NSJ == iva.Clase ? Enumerados.enumClaseDeLineaFar.BiExenta : Enumerados.enumClaseDeLineaFar.BiConIva,
                Concepto = dto.Nombre,
                BaseImponible = dto.BaseImponible,
                Cantidad = dto.Cantidad,
                IdUnidad = dto.IdUnidad,
                IdNaturaleza = dto.IdNaturaleza,
                IdIvaS = iva.Id,
                PorcentajeIva = iva.Porcentaje
            }.Insertar(Contexto);
        }
        protected override FacturaRecDtm AntesDeTransitar(FacturaRecDtm factura, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            factura = base.AntesDeTransitar(factura, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion.Estados()) ||
                transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados()))
                factura.ValidarFacturaBienFormada(Contexto, parametros);

            if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> { enumEtapasDeFacturasRec.FAR_Etapa_Anulada, enumEtapasDeFacturasRec.FAR_Etapa_Devuelta }))
                Emitir($"La factura '{factura.Referencia}' no es reactivable, vuelva a crearla");

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados()) &&
                factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> { enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion, enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion }))
                factura.AntesDeEnviarAContabilidad(Contexto, parametros);

            if (transicion.DestinoEstaEnAlgunaDeLasEtapas(new List<string> {
                enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Estados(),
                enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion.Estados(),
                enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados()
            }))
            {
                factura.QuitarPreasiento(Contexto, parametros);
            }

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion.Estados()))
                factura.AntesDeDevolverAprobar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.Estados()))
                factura.AntesDeEnviarAPagar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados()))
                factura.AntesDeDarPorPagada(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados()))
                factura.AntesDeArchivarFactura(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados()))
                factura.AntesDeDarDevolverAContabilidad(Contexto, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Anulada.Estados()))
                factura.AntesDeCancelar(Contexto, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Devuelta.Estados()))
                factura.AntesDeDevolverAlProveedor(Contexto, parametros);

            return factura;
        }

        protected override FacturaRecDtm DespuesDeTransitar(FacturaRecDtm factura, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            factura = base.DespuesDeTransitar(factura, transicion, parametros);

            if (transicion.EntrarEnLaEtapaDe(enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados()))
                factura.TrasDarPorPagada(Contexto, parametros);

            if (transicion.SalirDeLaEtapaDe(enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados()))
                factura.TrasAnularElPago(Contexto, parametros);

            var automatizar = enumNegocio.FacturaRecibida.LeerCrearParametro(Contexto, enumParametrosDeFacturasRec.FAR_Dar_Por_Contabilizada_Si_Pagada, Literal.True).Valor.EsTrue();
            if (automatizar &&
                factura.EstaPagada(Contexto) &&
                transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_Anterior_A_Contabilidad.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados()))
                factura.TransitarALaEtapa(Contexto, enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });

            if (transicion.EntreEtapas(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.Estados(), enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.Estados()))
            {
                var pagosContadosEnCurso = factura.PagosContadosEnCurso(Contexto);

                var pagada = Math.Abs(pagosContadosEnCurso.Sum(p => p.Importe) - factura.TotalDelPago) <= VariableDeFacturasRec.ToleranciaEnImportes();
                if (pagada && pagosContadosEnCurso.Count == 1)
                {
                    pagosContadosEnCurso.First().TransitarALaEtapa(Contexto, enumEtapasDePagos.PAG_Etapa_Pagado.EstadosDeLaEtapa(),
                        new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnPago.Accion_DarPorPagadoAlTransitarFactura } });
                }

                if (factura.EstaPagada(Contexto))
                    factura.TransitarALaEtapa(Contexto, enumEtapasDeFacturasRec.FAR_Etapa_Pagada.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }

            return factura;
        }

        protected override void DatosParaElMapeo(List<FacturaRecDtm> facturas, Dictionary<string, object> parametros)
        {
            base.DatosParaElMapeo(facturas, parametros);
            var idsFacturas = facturas.Select(f => f.Id).ToList();
            ExtensorDeFacturasRec.CargarCacheDeTotales(Contexto, idsFacturas);
            ExtensorDeFacturasRec.CargarCacheDeLineasMasivamente(Contexto, idsFacturas); 
            ExtensorDeFacturasRec.CargarCacheQuienMeRectificaMasivamente(Contexto, idsFacturas);
        }

        protected override void DespuesDeMapearElElemento(FacturaRecDtm factura, FacturaRecDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(factura, elemento, parametros);
            elemento.Irpf = factura.Total(Contexto, enumImporteFar.TotalIrpf);
            elemento.Iva = factura.Total(Contexto, enumImporteFar.TotalIva);

            elemento.TotalPagado = factura.ImportesDePagosConfirmados(Contexto);
            elemento.TotalPagosEnCurso = factura.PagosEnCurso(Contexto).Sum(x => x.Importe);
            elemento.TotalRectificado = factura.QuienMeRectifica(Contexto)?.TotalDelPago ?? 0;
            elemento.TotalDevuelto = factura.ImportesDevueltosConfirmados(Contexto);
            if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
            {
                elemento.BaseImponible = factura.Total(Contexto, enumImporteFar.BaseImponible);
                elemento.TotalDelPago = factura.Total(Contexto, enumImporteFar.TotalPagar);
            }

            if (parametros.LeerPorId)
            {
                elemento.EsInterventor = factura.EsInterventor<TipoDeFacturaRecDtm>(Contexto);
                elemento.IdInterlocutor = factura.Proveedor(Contexto).IdInterlocutor;
                elemento.Interlocutor = factura.Proveedor(Contexto).Interlocutor(Contexto).Expresion;
                elemento.Etapas = factura.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);
                elemento.EsIncorporada = factura.EsIncorporada(Contexto);
                elemento.Expediente = factura.Expediente(Contexto, errorSiNoHay: false)?.Expresion;
                elemento.Contrato = factura.Contrato(Contexto, errorSiNoHay: false)?.Expresion;
                elemento.Rectificada = factura.Rectificada(Contexto, errorSiNoHay: false)?.Expresion;

                if (factura.IdPreasiento is not null)
                {
                    elemento.IdPreasiento = factura.IdPreasiento;
                    elemento.Preasiento = factura.Preasiento(Contexto, errorSiNoHay: false)?.Expresion ?? "Sin preasiento";
                }

            }

            if (parametros.LeerDatosParaElGridOParaExportar)
            {
                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaRecDto.Expediente), StringComparison.InvariantCultureIgnoreCase)))
                {
                    elemento.Expediente = factura.Expediente(Contexto, errorSiNoHay: false)?.Expresion;
                }

                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaRecDto.Impuestos), StringComparison.InvariantCultureIgnoreCase)))
                    elemento.Impuestos = factura.ImpuestosToString(Contexto);

                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaRecDto.Naturalezas), StringComparison.InvariantCultureIgnoreCase)))
                    elemento.Naturalezas = factura.NaturalezasToString(Contexto);

                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaRecDto.FormasDePago), StringComparison.InvariantCultureIgnoreCase)))
                    elemento.FormasDePago = factura.FormasDePagoToString(Contexto);

            }
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idFactura = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var factura = entorno.Contexto.SeleccionarPorId<FacturaRecDtm>(idFactura);
            if (vinculado == enumNegocio.Archivos)
            {
                if (factura.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
                {
                    var archivo = entorno.Contexto.SeleccionarPorId<ArchivoDtm>(factura.IdArchivo.Entero());
                    var firmado = entorno.Contexto.Set<FirmadoDtm>().Where(x => x.IdOriginal == (int)factura.IdArchivo).FirstOrDefault();
                    if (firmado != null)
                    {
                        if (firmado.IdOriginal == factura.IdArchivo)
                            Emitir($"No puede quitar de la {enumNegocio.FacturaRecibida.Singular(true)} '{factura.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el original de la emisión");
                    }
                    else
                        Emitir($"No puede quitar de la {enumNegocio.FacturaRecibida.Singular(true)} '{factura.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el original de la emisión");
                }
            }
        }

        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idFactura = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var factura = entorno.Contexto.SeleccionarPorId<FacturaRecDtm>(idFactura);
            if (vinculado == enumNegocio.CircuitoDoc)
            {
                var idCircuito = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                var circuito = entorno.Contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
                if (circuito.VinculadosAl(entorno.Contexto).Count == 0 && circuito.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto))
                    circuito.TransitarALaEtapa(entorno.Contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.EstadosDeLaEtapa());
            }
        }

        public static void DespuesDeVincular(EntornoDeUnaAccion entorno)
        {
            var idFactura = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var factura = entorno.Contexto.SeleccionarPorId<FacturaRecDtm>(idFactura);
            if (vinculado == enumNegocio.Archivos && factura.IdArchivo.Entero() == 0)
            {
                factura.IdArchivo = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                factura.Modificar(entorno.Contexto);
            }
        }

        public (bool EstabaSinImputar, string Mensaje) Imputar(int id, enumNegocio negocio, int idDondeImputar)
        {
            return negocio == enumNegocio.Contrato ? ImputarContrato(id, idDondeImputar) : ImputarExpediente(id, idDondeImputar);
        }

        public (bool EstabaSinImputar, string Mensaje) ImputarContrato(int id, int idDondeImputar)
        {
            var contrato = Contexto.SeleccionarPorId<ContratoDtm>(idDondeImputar);

            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Cancelado))
                Emitir($"El contrato '{contrato.Referencia}' está cancelado, no se le pueden imputar facturas");

            if (!contrato.EsInterventor<TipoDeContratoDtm>(Contexto))
                Emitir($"Ha de ser interventor del contrato '{contrato.Referencia}' para poder imputarle facturas");

            var factura = Contexto.SeleccionarPorId<FacturaRecDtm>(id, aplicarJoin: true);

            if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> { enumEtapasDeFacturasRec.FAR_Etapa_Anulada, enumEtapasDeFacturasRec.FAR_Etapa_Devuelta }))
                Emitir($"La factura '{factura.Referencia}' no puede estar en la etapa de '{enumEtapasDeFacturasRec.FAR_Etapa_Anulada.Nombre()} ni en la de '{enumEtapasDeFacturasRec.FAR_Etapa_Devuelta.Nombre()}' para imputarla al contrato '{contrato.Referencia}'");

            if (factura.IdContrato is null)
            {
                factura.IdContrato = idDondeImputar;
                factura.Modificar(Contexto, nameof(ImputarContrato));
                return (true, "");
            }

            return (false, $"la factura '{factura.Referencia}' ya estaba imputada al contrato '{factura.Contrato(Contexto).Referencia}'");
        }

        public (bool EstabaSinImputar, string Mensaje) ImputarExpediente(int id, int idDondeImputar)
        {
            var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(idDondeImputar);

            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Cancelada))
                Emitir($"El expediente '{expediente.Referencia}' está cancelado, no se le pueden imputar facturas");

            if (!expediente.EsInterventor<TipoDeExpedienteDtm>(Contexto))
                Emitir($"Ha de ser interventor del expediente '{expediente.Referencia}' para poder imputarle facturas");

            var factura = Contexto.SeleccionarPorId<FacturaRecDtm>(id, aplicarJoin: true);

            if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> { enumEtapasDeFacturasRec.FAR_Etapa_Anulada, enumEtapasDeFacturasRec.FAR_Etapa_Devuelta }))
                Emitir($"La factura '{factura.Referencia}' no puede estar en la etapa de '{enumEtapasDeFacturasRec.FAR_Etapa_Anulada.Nombre()} ni en la de '{enumEtapasDeFacturasRec.FAR_Etapa_Devuelta.Nombre()}' para imputarla al expediente '{expediente.Referencia}'");

            if (factura.IdExpediente is null)
            {
                factura.IdExpediente = idDondeImputar;
                factura.Modificar(Contexto, nameof(ImputarExpediente));
                return (true, "");
            }

            return (false, $"la factura '{factura.Referencia}' ya estaba imputada al expediente '{factura.Expediente(Contexto).Referencia}'");
        }

        public void QuitarContrato(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var factura = Contexto.SeleccionarPorId<FacturaRecDtm>(id);
                    if (factura.IdContrato is null)
                        Emitir($"La factura '{factura.Referencia}' no está imputada a ningún contrato");

                    var contrato = Contexto.SeleccionarPorId<ContratoDtm>((int)factura.IdContrato);
                    if (!contrato.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeContratos> { enumEtapasDeContratos.CTR_Etapa_Vigente }) && !contrato.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor del contrato '{contrato.Referencia}' para poder quitarle facturas o el contrato a de estar vigente");

                    if (!factura.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor de la factura '{factura.Referencia}' para poder qitarle el contrato");

                    factura.IdContrato = null;
                    factura.Modificar(Contexto, ltrDeUnaFacturaRec.Accion_QuitarContrato);
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }
        public void QuitarExpediente(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var factura = Contexto.SeleccionarPorId<FacturaRecDtm>(id);
                    if (factura.IdExpediente is null)
                        Emitir($"La factura '{factura.Referencia}' no está imputada a ningún expediente");

                    var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>((int)factura.IdExpediente);

                    if (!expediente.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeExpedientes> { enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enumEtapasDeExpedientes.EXP_Etapa_Terminada }) && !expediente.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor del contrato '{expediente.Referencia}' para poder anular la imputación de facturas o el expediente ha de estar en las etapas válidas");

                    if (!factura.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor de la factura '{factura.Referencia}' para poder qitarle el expediente");

                    factura.IdExpediente = null;
                    factura.Modificar(Contexto, nameof(QuitarExpediente));
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }

        public void GenerarPreasiento(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var factura = Contexto.SeleccionarPorId<FacturaRecDtm>(id);
                    var sociedad = factura.Sociedad(Contexto);

                    if (!factura.Sociedad(Contexto).UsaPreasientos(Contexto, enumNegocio.FacturaRecibida))
                        Emitir($"La sociedad de la factura '{factura.Referencia}' no usa preasientos, configúrela en '{enumParametrosDePreasiento.SPR_Generar_Preasiento_De_FacturaRecibida}'");

                    if (!factura.EsAdministrador(Contexto))
                        Emitir($"Ha de ser administrador de la factura '{factura.Referencia}' para poder preasentarla");

                    if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> { enumEtapasDeFacturasRec.FAR_Etapa_Devuelta, enumEtapasDeFacturasRec.FAR_Etapa_Anulada, enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion, enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion, enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion }))
                        Emitir($"No se puede generar un preasiento de  '{factura.Referencia}' por no estar en la etapa correcta");

                    if (sociedad.Autonomo)
                    {
                        var circuitos = factura.Vinculados<CircuitoDocDtm>(Contexto);
                        var estados = enumNegocio.CircuitoDoc.Estados(Contexto);
                        var contabilizado = circuitos.FirstOrDefault(x => estados.Any(e => e.Terminado && e.Id == x.IdEstado));
                        if (contabilizado != null)
                            Emitir($"No se puede generar un preasiento de  '{factura.Referencia}' por estar en el lote '{contabilizado.Referencia}', ya contabilizado, anule su contabilización", enumCodigoDeError.MensajeInformativo);

                        var pendiente = circuitos.FirstOrDefault(x => estados.Any(e => !e.Terminado && !e.Cancelado && e.Id == x.IdEstado));
                        if (pendiente != null)
                        {
                            factura.Desvincular(Contexto, pendiente);
                            Contexto.Commit(trans);
                            Emitir($"Se ha excluido la factura '{factura.Referencia}' del lote '{pendiente.Referencia}', vuelva a solicitar su contabilización", enumCodigoDeError.MensajeInformativo);
                        }
                        EstimacionDirecta(factura);
                    }
                    else
                    {
                        var preasientoAnterior = factura.Preasiento(Contexto, errorSiNoHay: false);
                        var cancelado = preasientoAnterior?.Estado(Contexto).Cancelado ?? false; ;
                        if (factura.IdPreasiento is not null && !cancelado)
                        {
                            factura.CancelarPreasiento(Contexto);
                        }

                        factura = factura.Preasentar(Contexto);
                        factura.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnaFacturaRec.Accion_GenerarPreasiento);

                        if (preasientoAnterior is not null && !cancelado)
                            factura.CrearTraza(Contexto, "Preasiento regenerado", $"El preasiento '{preasientoAnterior.Referencia}' se ha desasociado de la factura{(preasientoAnterior.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado) ? ", recuerde anular el asiento contable asociado" : "")}");

                    }
                }

                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }

        private void EstimacionDirecta(FacturaRecDtm recibida)
        {
            var sociedad = recibida.Sociedad(Contexto);
            var comoContabilizar = sociedad.ContabilizarEn();
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoContabilizar.Metodo);
            if (metodo != null)
            {
                var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(ltrDeUnPreasiento.IdFacturaRecibida), enumCriteriosDeFiltrado.igual, recibida.Id) };
                var filtrosJson = JsonConvert.SerializeObject(filtros);
                var parametros = new Dictionary<string, object>
                {
                    {ltrFiltros.filtro, filtrosJson},
                    {nameof(SociedadDtm.Id), sociedad.Id},
                    {nameof(PreasientoDtm.FechaContable), null }
                };
                var contabilizar = new EntornoDeUnaAccion(Contexto, enumNegocio.FacturaRecibida, parametros);

                contabilizar.Contexto.Accion = new AccionDtm
                {
                    Dll = ApiDeEnsamblados.GestoresDeNegocio,
                    Clase = typeof(ExportacionesDePreasientos).FullName,
                    Metodo = comoContabilizar.Metodo,
                    Nombre = comoContabilizar.Nombre,
                    ClaseDeAccion = enumClaseDeAccion.DLL.ToString()
                };

                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(contabilizar);
                }
                catch (Exception e)
                {
                    GestorDeErrores.Emitir(error: e.InnerException?.Message ?? e.Message);
                }
                finally
                {
                    contabilizar.Contexto.Accion = null;
                }
            }
            else GestorDeErrores.Emitir($"No está definido el método {metodo} en la clase {typeof(ExportacionesDePreasientos).FullName}");
        }


        public int CancelarPreasientos(List<int> ids)
        {
            if (ids.Count == 0) Emitir("No ha indicado facturas a las que cancelarle el preasiento");
            var cancelados = 0;
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var factura = Contexto.SeleccionarPorId<FacturaRecDtm>(id);

                    if (!factura.EsAdministrador(Contexto))
                        Emitir($"Ha de ser administrador de la factura '{factura.Referencia}' para poder cancelar el preasiento");

                    var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: ServicioDeDatos.Literal.Cero).Valor.ToLista<int>();
                    if (!idsDeUsuarios.Contains(Contexto.DatosDeConexion.IdUsuario))
                        Emitir($"Incluya al usuario conectado en el parámetro  '{enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar}' para poder cancelar preasientos");

                    if (factura.IdPreasiento is not null)
                    {
                        factura.CancelarPreasiento(Contexto);
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

        public void CrearFarConIa(int idCg, int idTipo, int idArchivo, int? idProveedor)
        {
            TrabajosDeFacturasRec.SometerCrearFarConIa(Contexto, idCg, idTipo, idArchivo, idProveedor);
        }

        public IUsaTipoConCG ImportarDelCorreo(int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(FacturaRecDtm.BaseImponible))) throw new Exception("Debe indicar la base imponible");
            if (!parametros.ContieneClave(nameof(FacturaRecDtm.TotalDelPago))) throw new Exception("Debe indicar el total a pagar");
            if (!parametros.ContieneClave(nameof(FacturaRecDtm.Numero))) throw new Exception("Debe indicar el número de factura");
            var factura = (FacturaRecDtm)ExtensorDeElementosDeUnProceso.NuevoDtm(Negocio.TipoDtm(), idCg, idTipo, nombre, descripcion, parametros);
            factura.RecibidaEl = DateTime.Today;
            factura.Numero = parametros.LeerValor<string>(nameof(FacturaRecDtm.Numero));
            factura.BaseImponible = parametros.LeerValor<decimal>(nameof(FacturaRecDtm.BaseImponible));
            factura.TotalDelPago = parametros.LeerValor<decimal>(nameof(FacturaRecDtm.TotalDelPago));
            return factura;
        }

        public static FacturaRecDto ImportarFarDesdeXml(ContextoSe contexto, int idCg, int idTipo, int idProveedor, int idArchivo)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var fichero = Path.Combine(archivo.AlmacenadoEn, $"{archivo.Id}.{ApiDeArchivos.ExtensionSe}");
            eFactura322.Parsear(fichero);
            var facturae = new eFactura322().Validate(fichero);

            var proveedor = contexto.SeleccionarPorPropiedad<ProveedorDtm>(nameof(ltrProveedor.NIF), facturae.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber);
            if (idProveedor != proveedor.Id)
                Emitir($"El proveedor de la factura '{proveedor.Expresion}' no se corresponde con el indicado");

            var recibida = new FacturaRecDtm();
            recibida.IdTipo = idTipo;
            recibida.IdCg = idCg;
            recibida.Numero = facturae.Invoices[0].InvoiceHeader.InvoiceNumber;

            if (facturae.Invoices[0].InvoiceHeader.InvoiceClass == InvoiceClassType.OR)
                recibida.ClaseRectificativa = enumClaseDeRectificativa.OR;
            else if (facturae.Invoices[0].InvoiceHeader.InvoiceClass == InvoiceClassType.OC)
                recibida.ClaseRectificativa = enumClaseDeRectificativa.OC;
            else if (facturae.Invoices[0].InvoiceHeader.InvoiceClass != InvoiceClassType.OO)
                Emitir($"No se ha implementado cómo incorporar una facturade la clase  '{facturae.Invoices[0].InvoiceHeader.InvoiceClass}'");

            recibida.Nombre = facturae.Invoices[0].InvoiceIssueData.InvoiceDescription == null ? "(No detallado)" : facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.Left(250);
            recibida.Descripcion = facturae.Invoices[0].AdditionalData == null ? "(No detallado)" : facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.Left(2000);
            recibida.IdProveedor = proveedor.Id;
            recibida.RecibidaEl = DateTime.Now.Date;
            recibida.FacturadaEl = facturae.Invoices[0].InvoiceIssueData.IssueDate;
            recibida.VenceEl = facturae.Invoices[0].PaymentDetails == null ? facturae.Invoices[0].InvoiceIssueData.IssueDate : facturae.Invoices[0].PaymentDetails[0].InstallmentDueDate;
            recibida.BaseImponible = facturae.Invoices[0].InvoiceTotals.TotalGrossAmountBeforeTaxes.ToString().Decimal();
            recibida.TotalDelPago = facturae.Invoices[0].InvoiceTotals.InvoiceTotal.ToString().Decimal();

            recibida.IdArchivo = idArchivo;
            recibida.Insertar(contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_IncorporarFacturaE);

            if (facturae.Invoices[0].InvoiceIssueData.InvoiceDescription != null && facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.ToString().Length > 250)
                recibida.CrearObservacion(contexto, "Nombre completo en el Xml", facturae.Invoices[0].InvoiceIssueData.InvoiceDescription);

            if (facturae.Invoices[0].AdditionalData != null && facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.ToString().Length > 2000)
                recibida.CrearObservacion(contexto, "Descripción completa en el Xml", facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.Right(1000));

            ServidorDocumental.BloquearArchivo(contexto, enumNegocio.FacturaRecibida.IdNegocio(), recibida.Id, archivo.Id, $"eFactura importada", validarSiEstaTerminado: false);

            recibida.CrearTraza(contexto, ltrDeUnaFacturaRec.TrazaDeIncorporacion, $"El usuario {contexto.DatosDeConexion.Login} a incorporado la factura por importe de {recibida.BaseImponible.Moneda()}");

            string imputadoA = null;
            string informacioDeLineas = null;
            var linea = new LineaDeUnaFarDtm();
            var asignarElOrden = 10;
            linea.IdElemento = recibida.Id;
            foreach (var item in facturae.Invoices[0].Items)
            {
                linea.Orden = Convert.ToInt32(item.SequenceNumber);
                if (linea.Orden == 0)
                {
                    linea.Orden = asignarElOrden;
                    asignarElOrden = asignarElOrden + 10;
                }

                var concepto = item.ItemDescription.QuitarSubcadenaInicial(Environment.NewLine);
                concepto = concepto.QuitarSubcadenaInicial("\n");
                concepto = concepto.QuitarSubcadenaInicial("\t");
                concepto = concepto.Trim();
                concepto = concepto.QuitarDobleIntro()
                .RemplazarCaracteres(Environment.NewLine + "\t", Environment.NewLine)
                .RemplazarCaracteres(Environment.NewLine + " ", Environment.NewLine);
                if (item.Quantity > 0 && item.UnitPriceWithoutTax > 0)
                {
                    var complemento = "Cta:" + item.Quantity.ToString() + ", Pu:" + item.UnitPriceWithoutTax;
                    concepto = concepto + " (" + complemento + ")";
                }

                if (concepto.Length > 250)
                {
                    linea.Concepto = "Descripción anotada como observación de la factura";
                    recibida.CrearObservacion(contexto, $"Concepto de la línea: {linea.Orden}", concepto);
                }
                else linea.Concepto = concepto;

                if (!item.ReceiverContractReference.IsNullOrEmpty() && (imputadoA == null || !imputadoA.Contains(item.ReceiverContractReference)))
                    imputadoA = $"{(imputadoA.IsNullOrEmpty() ? "" : imputadoA + ", ")} {item.ReceiverContractReference}";

                if (!item.ReceiverTransactionReference.IsNullOrEmpty() && (imputadoA == null || !imputadoA.Contains(item.ReceiverTransactionReference)))
                    imputadoA = $"{(imputadoA.IsNullOrEmpty() ? "" : imputadoA + ", ")} {item.ReceiverTransactionReference}";

                linea.BaseImponible = Convert.ToDecimal(item.GrossAmount);

                var impuestos = item.TaxesOutputs;
                if (impuestos.Length > 1)
                    Emitir($"No se puede importar la factura, por tener la línea '{linea.Orden}' más de un impuesto definido");

                if (impuestos.Length == 0)
                {
                    linea.Clase = Enumerados.enumClaseDeLineaFar.BaseImponible;
                }
                else //Hay un impuesto
                {
                    if (linea.BaseImponible > 0)
                    {
                        var impuesto = impuestos[0];
                        linea.Clase = impuesto.TaxTypeCode == TaxTypeCodeType.Item05 ? Enumerados.enumClaseDeLineaFar.BiExenta : Enumerados.enumClaseDeLineaFar.BiConIva;
                        linea.PorcentajeIva = impuesto.TaxTypeCode == TaxTypeCodeType.Item05 ? 0 : Convert.ToDecimal(impuesto.TaxRate);
                        if (impuesto.TaxTypeCode == TaxTypeCodeType.Item05)
                        {
                            recibida.CrearObservacion(contexto, "Línea exenta", $"la línea '{linea.Orden}' está exenta de IVA");
                        }
                    }
                    if (linea.BaseImponible == 0)
                    {
                        var impuesto = impuestos[0];
                        if (impuesto.TaxTypeCode == TaxTypeCodeType.Item03)
                        {
                            linea.Clase = Enumerados.enumClaseDeLineaFar.LineaDeIrpf;
                            linea.PorcentajeIrpf = Convert.ToDecimal(impuesto.TaxRate);
                        }
                        else
                        {
                            linea.Clase = Convert.ToDecimal(impuesto.TaxRate) == 0 ? Enumerados.enumClaseDeLineaFar.BaseImponible : Enumerados.enumClaseDeLineaFar.LineaDeIva;
                            linea.PorcentajeIva = Convert.ToDecimal(impuesto.TaxRate);
                        }
                    }
                }

                if (linea.PorcentajeIrpf > 0)
                {
                    linea.IdIrpf = contexto.SeleccionarPorPropiedad<IrpfDtm>(nameof(IrpfDtm.Porcentaje), linea.PorcentajeIrpf, errorSiMasDeuno: false).Id;
                }
                if (linea.PorcentajeIva > 0)
                {
                    linea.IdIvaS = contexto.SeleccionarPorPropiedad<IvaSoportadoDtm>(nameof(IvaSoportadoDtm.Porcentaje), linea.PorcentajeIva, errorSiMasDeuno: false).Id;
                }

                informacioDeLineas = $"{(informacioDeLineas.IsNullOrEmpty() ? "" : informacioDeLineas + Environment.NewLine)}{item.AdditionalLineItemInformation}";
                linea.Id = 0;
                linea.Insertar(contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_IncorporarFacturaE);
            }
            if (!imputadoA.IsNullOrEmpty())
                recibida.CrearObservacion(contexto, "Anotaciones en las líneas", imputadoA);

            Imprimir(recibida, contexto);
            return contexto.SeleccionarDto<FacturaRecDto>(recibida.Id);
        }

        public static void Imprimir(FacturaRecDtm factura, ContextoSe contexto)
        {
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Far-{factura.Referencia}.pdf".NormalizarFichero());
            var facturaRecRpt = new GeneradorDeFacturaRecRpt(contexto, factura).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeFacturaRec(facturaRecRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(contexto, enumNegocio.FacturaRecibida, enumNegocio.Archivos, factura.Id, idArchivo);
        }

        public async Task<TotalesDeFacturasRec> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return await Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDeFacturasRec ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var facturas = Contexto.SeleccionarTodos<FacturaRecDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales}
            });
            var totales = new TotalesDeFacturasRec();


            totales.Bi = facturas.Sum(x => x.Total(Contexto, Enumerados.enumImporteFar.BaseImponible));
            totales.Iva = facturas.Sum(x => x.Total(Contexto, Enumerados.enumImporteFar.TotalIva));
            totales.Irpf = facturas.Sum(x => x.Total(Contexto, Enumerados.enumImporteFar.TotalIrpf));
            totales.Pagar = facturas.Sum(x => x.Total(Contexto, Enumerados.enumImporteFar.TotalPagar));
            totales.Total = facturas.Sum(x => x.Total(Contexto, Enumerados.enumImporteFar.TotalFactura));

            var totalesDeIrpf = facturas.TotalesPorTipoDeIrpf(Contexto);
            var totalesDeIva = facturas.TotalesPorTipoDeIva(Contexto);
            totales.TotalesPorImpuestos = FormatearImpuestosTabulados(totalesDeIva, totalesDeIrpf);

            totales.Pagado = facturas.Sum(x => x.ImportesDePagosConfirmados(Contexto));
            totales.Pendiente = totales.Pagar - totales.Pagado;
            totales.Procesados = facturas.Count();


            //var errores = new StringBuilder();
            //foreach (var f in facturas)
            //{
            //    f.Descuedre(Contexto, errores);
            //}
            //if (!errores.ToString().IsNullOrEmpty())
            //    Contexto.RegistrarLogPorFecha(CacheDeVariable.CFG_Ruta_Ficheros_De_Excepciones, "total_iva.txt", errores.ToString());
            //else
            //    Contexto.EliminarLogPorFecha(CacheDeVariable.CFG_Ruta_Ficheros_De_Excepciones, "total_iva.txt");


            return totales;
        }

        private static string FormatearImpuestosTabulados(List<ImportePorTipoDeIva> totalesDeIva, List<ImportePorTipoDeIrpf> totalesDeIrpf)
        {
            const int anchoTipo = 35;
            const int anchoBase = 25;
            const int anchoPorcentaje = 15;
            const int anchoImporte = 25;

            var sb = new StringBuilder();

            // Encabezados
            sb.AppendLine($"{"Tipo".PadRight(anchoTipo)}{"Base Imponible".PadRight(anchoBase)}{"Porcentaje".PadRight(anchoPorcentaje)}{"Importe".PadRight(anchoImporte)}");
            sb.AppendLine(new string('-', anchoTipo + anchoBase + anchoPorcentaje + anchoImporte));

            // IVA
            foreach (var item in totalesDeIva)
            {
                sb.AppendLine(
                    $"{item.Tipo.PadRight(anchoTipo)}" +
                    $"{item.BI.ToMoneda().PadRight(anchoBase)}" +
                    $"{item.Porcentaje.Porcentaje().PadRight(anchoPorcentaje)}" +
                    $"{item.Importe.ToMoneda().PadRight(anchoImporte)}"
                );
            }

            // Línea separadora
            sb.AppendLine(new string('-', anchoTipo + anchoBase + anchoPorcentaje + anchoImporte));

            // IRPF
            foreach (var item in totalesDeIrpf)
            {
                sb.AppendLine(
                    $"{item.Tipo.PadRight(anchoTipo)}" +
                    $"{item.BI.ToMoneda().PadRight(anchoBase)}" +
                    $"{item.Porcentaje.Porcentaje().PadRight(anchoPorcentaje)}" +
                    $"{item.Importe.ToMoneda().PadRight(anchoImporte)}"
                );
            }

            return sb.ToString();
        }

        public static int CopiarFar(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CopiarFarDto.IdElemento))) Emitir("No se ha indicado la factura a copiar");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.IdTipo))) Emitir("No se ha indicado el tipo de la factura");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.IdCg))) Emitir("No se ha indicado el cg de la factura");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.IdProveedor))) Emitir("No se ha indicado el proveedor");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.Nombre))) Emitir("No se ha indicado el asunto de la factura");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.Descripcion))) Emitir("No se ha indicado la descripción de la factura");


            if (!parametros.ContieneClave(nameof(CopiarFarDto.Numero))) Emitir("No se ha indicado el número de la factura");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.FacturadaEl))) Emitir("No se ha indicado la fecha de factura");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.BaseImponible))) Emitir("No se ha indicado la base imponible");
            if (!parametros.ContieneClave(nameof(CopiarFarDto.TotalDelPago))) Emitir("No se ha indicado el total del pago");

            var farOrigen = contexto.SeleccionarPorId<FacturaRecDtm>((int)(long)parametros[nameof(CopiarFarDto.IdElemento)]);


            return farOrigen.Copiar(contexto, parametros).Id;
        }

        public static int RectificarFar(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(RectificarFarDto.IdElemento))) Emitir("No se ha indicado la factura a rectificar");
            if (!parametros.ContieneClave(nameof(RectificarFarDto.Nombre))) Emitir("No se ha indicado el asunto de la factura");
            if (!parametros.ContieneClave(nameof(RectificarFarDto.Descripcion))) Emitir("No se ha indicado la descripción de la factura");

            if (!parametros.ContieneClave(nameof(RectificarFarDto.Numero))) Emitir("No se ha indicado el número de la factura");
            if (!parametros.ContieneClave(nameof(RectificarFarDto.FacturadaEl))) Emitir("No se ha indicado la fecha de factura");
            if (!parametros.ContieneClave(nameof(RectificarFarDto.BaseImponible))) Emitir("No se ha indicado la base imponible");
            if (!parametros.ContieneClave(nameof(RectificarFarDto.TotalDelPago))) Emitir("No se ha indicado el total del pago");

            var farOrigen = contexto.SeleccionarPorId<FacturaRecDtm>((int)(long)parametros[nameof(RectificarFarDto.IdElemento)]);


            return farOrigen.Rectificar(contexto, parametros).Id;
        }

    }

}
