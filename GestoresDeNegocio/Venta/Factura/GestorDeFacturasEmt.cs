using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Venta.Factura;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Ventas;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using ServicioDeReportes.Ventas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;
using static Utilidades.Ampliaciones;

namespace GestoresDeNegocio.Ventas
{

    public class GestorDeFacturasEmt : GestorDeElementos<ContextoSe, FacturaEmtDtm, FacturaEmtDto>, ITotalizador<TotalesDeFacturasEmt>, IGeneradorDePreasiento
    {
        public class MapearFacturaEmt : Profile
        {
            public MapearFacturaEmt()
            {
                CreateMap<FacturaEmtDtm, FacturaEmtDto>()
                .DtmToDto()
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente == null ? null : dtm.Cliente.Expresion))
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato == null ? null : dtm.Contrato.Expresion))
                .ForMember(dto => dto.ParteTr, dtm => dtm.MapFrom(dtm => dtm.ParteTr == null ? null : dtm.ParteTr.Expresion))
                .ForMember(dto => dto.Presupuesto, dtm => dtm.MapFrom(dtm => dtm.Presupuesto == null ? null : dtm.Presupuesto.Expresion));

                CreateMap<FacturaEmtDto, FacturaEmtDtm>()
                .DtoToDtm()
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                .ForMember(dtm => dtm.ParteTr, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore())
                .ForMember(dtm => dtm.Presupuesto, dto => dto.Ignore())
                .ForMember(dtm => dtm.ClaseRectificativa, dto => dto.Ignore())
                .ForMember(dtm => dtm.MotivoDeRectificacion, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeFacturaEmt.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeFacturasEmt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeFacturasEmt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeFacturasEmt(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(FacturaEmtDto dto, FacturaEmtDtm dtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(dto, dtm, opciones);
            if (!dto.ClaseRectificativa.IsNullOrEmpty())
            {
                dtm.ClaseRectificativa = ExtensorDeEnum.Enumerado<enumClaseDeRectificativa>(dto.ClaseRectificativa);
                dtm.MotivoDeRectificacion = ExtensorDeEnum.Enumerado<enumMotivoDeRectificacion>(dto.MotivoDeRectificacion);
            }
        }

        protected override IQueryable<FacturaEmtDtm> AplicarJoins(IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cliente);
            consulta = consulta.Include(x => x.Contrato);
            consulta = consulta.Include(x => x.ParteTr);
            consulta = consulta.Include(x => x.Presupuesto);
            return consulta;
        }

        protected override IQueryable<FacturaEmtDtm> AplicarOrden(IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            for (var i = 0; i < ordenacion.Count; i++)
            {
                var orden = ordenacion[i];
                if (orden.OrdenarPor.Equals(nameof(FacturaEmtDto.NumeroFactura), StringComparison.CurrentCultureIgnoreCase))
                {
                    var lista = new List<ClausulaDeOrdenacion>
                    {
                        new ClausulaDeOrdenacion { OrdenarPor = nameof(FacturaEmtDtm.Ano), Modo = orden.Modo },
                        new ClausulaDeOrdenacion { OrdenarPor = nameof(FacturaEmtDtm.Serie), Modo = orden.Modo == ModoDeOrdenancion.ascendente ? ModoDeOrdenancion.descendente : ModoDeOrdenancion.ascendente },
                        new ClausulaDeOrdenacion { OrdenarPor = nameof(FacturaEmtDtm.Numero), Modo = orden.Modo }
                    };
                    ordenacion.InsertRange(i + 1, lista);
                    ordenacion.RemoveAt(i);
                }
            }
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<FacturaEmtDtm> AplicarFiltros(IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {

            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> {
                ltrDeUnaFacturaEmt.IdContrato,
                ltrDeUnaFacturaEmt.IdParteTr,
                ltrDeUnParteTr.IdPresupuesto,
                ltrDeUnaFacturaEmt.IdPlfDeVenta });

            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorVarios(Contexto, filtros);
            consulta = consulta.FiltroPorExpediente(Contexto, filtros);
            consulta = consulta.FiltroPorPresupuestos(filtros);
            consulta = consulta.FiltroPorPlanificacionesDeVenta(Contexto, filtros);
            consulta = consulta.FiltroPorPartesTr(Contexto, filtros);
            consulta = consulta.FiltroPorContratos(filtros);
            consulta = consulta.FiltroPorCliente(filtros);
            consulta = consulta.FiltroPorRemesas(Contexto, filtros);
            consulta = consulta.FiltroPorDeudor(Contexto, filtros);
            consulta = consulta.FiltroPorImportesSinIva(Contexto, filtros);
            consulta = consulta.FiltroPorCobrado(Contexto, filtros);
            consulta = consulta.FiltroPorFechaDeFacturacion(filtros);
            consulta = consulta.FiltroPorFechaDeVencimiento(filtros);
            consulta = consulta.FiltroPorNumerosDeFacturas(filtros);
            consulta = consulta.FiltroPorPrefacturasDeUnPpt(filtros);
            consulta = consulta.FiltroPorFacturaNoIncluidaEnRemesa(Contexto, filtros);
            consulta = consulta.FiltroParaSeleccionarFacturaEnRemesas(Contexto, filtros);
            consulta = consulta.FiltroParaSeleccionarFacturaDeUnaTarea(Contexto, filtros);
            consulta = consulta.FiltroFacturasEnUnaEstimacion(Contexto, filtros, parametros);
            consulta = consulta.FiltroFacturasEnUnLoteContable(Contexto, filtros, parametros);
            //consulta = consulta.FiltroPorEtapa(filtros);
            consulta = consulta.FiltroPorExpresion(filtros);
            consulta = consulta.FiltroPorSociedad(filtros);
            consulta = consulta.FiltroPorNumeroFactura(filtros);
            consulta = consulta.FiltroDeFacturasRectificadas(Contexto, filtros);

            return consulta;
        }

        protected override IQueryable<FacturaEmtDtm> AplicarSeguridad(IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<FacturaEmtDtm, TipoDeFacturaEmtDtm, PermisoDeLaFacturaEmtDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<FacturaEmtDtm, PermisoDeLaFacturaEmtDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(FacturaEmtDtm fae, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(fae, parametros);
            if (parametros.Insertando)
            {
                fae.InicializarPrefactura();
                fae.ClaseDeEmision = fae.Cliente(Contexto).ClaseDeEmision();
                fae.ValidarClaseDeEmision(Contexto, errorSiNoValida: $"No puede indicar que la clase de emisión para el cliente '{fae.Cliente(Contexto).Referencia(Contexto)}' es '{enumClaseDeEmision.Impresa}' e indicar que el sistema usa verifactu, actualice el parámetro '{enumParametrosDeCliente.Cli_Como_Emitir_Factura}'");
            }

            if (parametros.Modificando)
            {
                fae.ClaseRectificativa = fae.ClaseRectificativa ?? ((FacturaEmtDtm)parametros.registroEnBd).ClaseRectificativa;
                fae.MotivoDeRectificacion = fae.MotivoDeRectificacion ?? ((FacturaEmtDtm)parametros.registroEnBd).MotivoDeRectificacion;
                var idArchivoDeBd = ((FacturaEmtDtm)parametros.registroEnBd).IdArchivadorParaLaReclamacion;
                if (idArchivoDeBd is not null) fae.IdArchivadorParaLaReclamacion = idArchivoDeBd;
                if (fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                    fae.ValidarClaseDeEmision(Contexto);
            }

            if (parametros.AccionQueSeEjecuta != ltrDeUnaFacturaEmt.Accion_CambiarDatos)
                fae.InicializarDatosCliente(Contexto, parametros);
            else
            {
                fae.CambiarDatosCliente(Contexto);
            }
            if (fae.EsRectificativa)
            {
                if (fae.ClaseRectificativa is null)
                    GestorDeErrores.Emitir($"Debe indicar la clase de rectificativa");
                if (fae.MotivoDeRectificacion is null)
                    GestorDeErrores.Emitir($"Debe indicar el motivo de rectificación");


                var rectificada = parametros.Insertando
                ? parametros.Parametros.LeerValor<FacturaEmtDtm>(ltrDeFacturaRectificada.Rectificada, null)
                : fae.RectificaA(Contexto, errorSiNoHay: false);

                var validar = !(parametros.Transitando && parametros.EstadoDestino != null && parametros.EstadoDestino.Cancelado);
                if (validar)
                {
                    if (fae.IdCliente != rectificada.IdCliente || fae.Contacto != rectificada.Contacto || fae.Telefono != rectificada.Telefono || fae.eMail != rectificada.eMail)
                        GestorDeErrores.Emitir($"los datos del cliente de la factura '{fae.Referencia}' han de ser igual a los de la rectificativa '{rectificada.Referencia}'");

                    if (rectificada.EsRectificativa)
                        GestorDeErrores.Emitir($"la factura '{fae.Referencia}' no puede rectificar a la rectificada '{rectificada.Referencia}'");

                    if (parametros.Insertando && rectificada.EstaRectificada(Contexto))
                        GestorDeErrores.Emitir($"no puede rectificar la factura '{rectificada.Referencia}' por estar ya rectificada por '{rectificada.RectificadaPor(Contexto).Referencia}'");

                    if (rectificada.TienenIrpf(Contexto) && fae.ClaseRectificativa == enumClaseDeRectificativa.OC)
                        GestorDeErrores.Emitir($"La factura rectificada '{rectificada.Referencia}' tiene IRPF, solo puede rectificarse haciendo una rectificativa '{enumClaseDeRectificativa.OR.Descripcion()}' no '{enumClaseDeRectificativa.OC.Descripcion()}'");
                }

            }
            ValidarContrato(fae, parametros);
            ValidarAsociarPresupuesto(fae, parametros);
            ValidarAsociarParteDeTrabajo(fae, parametros);
            if (parametros.AccionQueSeEjecuta == ltrDeUnaFacturaEmt.Accion_AsociarArchivo)
                return;
            if (parametros.AccionQueSeEjecuta == ltrDeUnaFacturaEmt.Accion_CrearArchivadorDeReclamacion)
                return;

            if (parametros.Modificando)
            {
                if (parametros.AccionQueSeEjecuta != VariableDeFacturasEmt.enumMotivoTransicion.RectificarFactura.Descripcion()
                    && fae.EstaRectificada(Contexto)
                    && fae.RectificadaPor(Contexto).EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                    GestorDeErrores.Emitir($"No se puede transitar la factura '{fae.Referencia}' por estar siendo rectificada");

                fae.ClaseRectificativa = ((FacturaEmtDtm)parametros.registroEnBd).ClaseRectificativa;
                fae.MotivoDeRectificacion = ((FacturaEmtDtm)parametros.registroEnBd).MotivoDeRectificacion;
                fae.IdArchivadorParaLaReclamacion = ((FacturaEmtDtm)parametros.registroEnBd).IdArchivadorParaLaReclamacion;
            }

            if (parametros.AccionQueSeEjecuta == ltrDeUnaFacturaEmt.Accion_CambiarVencimiento)
            {
                fae.IdArchivo = ((FacturaEmtDtm)parametros.registroEnBd).IdArchivo;
                return;
            }

            if (parametros.AccionQueSeEjecuta != nameof(GenerarPreasiento))
            {
                if (fae.EstaEnEtapaNoContabilizada())
                {
                    var preasientoDeFactura = Contexto.Set<PreasientoDtm>().FirstOrDefault(prea => prea.IdReferenciado == fae.Id && prea.NegocioReferenciado == Negocio);
                    if (preasientoDeFactura == null)
                        fae.IdPreasiento = null;
                    else
                    {
                        fae.IdPreasiento = preasientoDeFactura.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado) ? null : fae.IdPreasiento;
                    }
                }
                else
                {
                    if (parametros.AccionQueSeEjecuta != ltrDeUnaFacturaEmt.Accion_EmitirFactura)
                        fae.IdPreasiento = ((FacturaEmtDtm)parametros.registroEnBd).IdPreasiento;
                }
            }

            if (parametros.Modificando)
            {
                if (!fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                    fae.IdCentroAdministrativo = ((FacturaEmtDtm)parametros.registroEnBd).IdCentroAdministrativo;
                else
                {
                    if (fae.TieneCentroAdministrativo(Contexto) && !fae.CentroAdministrativo(Contexto).Activa)
                        GestorDeErrores.Emitir($"El centro administrativo '{fae.CentroAdministrativo(Contexto).Expresion}' seleccionado no está activo");

                    //if (fae.DebeTenerIrpf(Contexto) && !fae.TienenIrpf(Contexto))
                    //{
                    //    GestorDeErrores.Emitir($"La factura '{fae.Referencia}' debe tener Irpf");
                    //}
                }
            }

            ValidarElValorDeIdArchivo(fae, parametros);


            if (!(parametros.ProcesandoTransicion(fae) || parametros.EsUnaTransicion) && !fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) && !parametros.EstaEjecutandoUnaAccion)
                GestorDeErrores.Emitir($"La {Negocio.Singular(true)} '{fae.Referencia}' no es modificable");

            if (fae.IdParteTr != default && fae.IdContrato != default && Contexto.SeleccionarPorId<ParteTrDtm>((int)fae.IdParteTr).IdContrato != fae.IdContrato)
                GestorDeErrores.Emitir($"La factura '{fae.Referencia}' ha de tener el mismo contrato que su parte de trabajo");
        }

        private void ValidarContrato(FacturaEmtDtm fae, ParametrosDeNegocio parametros)
        {
            if (fae.IdContrato is null) return;

            var ctt = fae.Contrato(Contexto);

            if (!ctt.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeContratos> { enumEtapasDeContratos.CTR_Etapa_Finalizacion, enumEtapasDeContratos.CTR_Etapa_Vigente }))
                GestorDeErrores.Emitir($"El contrato '{ctt.Referencia}' no puede ser asociado a la factura '{fae.Referencia}' por estar en estado '{ctt.Estado(Contexto).Nombre}', ha de estar en la etapa '{enumEtapasDeContratos.CTR_Etapa_Vigente.Nombre()}' o'{enumEtapasDeContratos.CTR_Etapa_Finalizacion.Nombre()}'");

            if (ctt.Sociedad(Contexto).Id != fae.Sociedad(Contexto).Id)
                GestorDeErrores.Emitir($"El contrato '{ctt.Referencia}' no puede ser asociado a la factura '{fae.Referencia}' por pertenecer a la sociedad '{ctt.Sociedad(Contexto).Referencia(Contexto)}' y la factura a la sociedad '{fae.Sociedad(Contexto).Referencia(Contexto)}'");

            if (ctt.Cliente(Contexto).Id != fae.Cliente(Contexto).Id)
                GestorDeErrores.Emitir($"El contrato '{ctt.Referencia}' no puede ser asociado a la factura '{fae.Referencia}' por pertenecer al cliente '{ctt.Cliente(Contexto).Referencia(Contexto)}' y la factura al cliente '{fae.Cliente(Contexto).Referencia(Contexto)}'");

        }

        private void ValidarElValorDeIdArchivo(FacturaEmtDtm fae, ParametrosDeNegocio parametros)
        {
            //si estoy insertando --> el archivo en blanco
            if (parametros.Insertando) fae.IdArchivo = null;
            else
                //si estoy modificando pero no transito y la etapa es cumplimentación y cancelada --> archivo en blanco, sino el de la BD
                if (parametros.Modificando && !parametros.EsUnaTransicion)
                {
                    var esta = fae.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada });
                    if (esta) fae.IdArchivo = null;
                    else fae.IdArchivo = ((FacturaEmtDtm)parametros.registroEnBd).IdArchivo;
                }
                else
                    //si estoy transitando y el estado destino NO está en la etapa de prefacturación -->  copio el archivo de BD
                    if (parametros.EsUnaTransicion)
                    {
                        if (!fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura)) fae.IdArchivo = ((FacturaEmtDtm)parametros.registroEnBd).IdArchivo;
                    }

        }

        protected override void DespuesDePersistir(FacturaEmtDtm fae, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(fae, parametros);
            AccionesAlPpt(fae, parametros);

            if (parametros.Insertando)
            {
                var direccionFiscal = fae.EsRectificativa
                ? parametros.Parametros.LeerValor<FacturaEmtDtm>(ltrDeFacturaRectificada.Rectificada).DireccionFiscal(Contexto)
                : fae.Cliente(Contexto).DireccionFiscal(Contexto);
                if (direccionFiscal != null)
                {
                    direccionFiscal.Id = 0;
                    var gestor = GestorDeDirecciones.Gestor(Contexto, enumNegocio.FacturaEmitida);
                    var direccion = gestor.MapearRegistro(direccionFiscal, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                    direccion.IdElemento = fae.Id;
                    gestor.PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } } });
                }

                if (fae.Tipo<TipoDeFacturaEmtDtm>(Contexto).ConPeriodoEmt)
                    new PeriodoEmtDtm { IdElemento = fae.Id }.Insertar(Contexto, parametros: parametros.Parametros);
            }

            if (parametros.Modificando && ((FacturaEmtDtm)parametros.registroEnBd).VenceEl is not null && fae.VenceEl is not null && fae.PropiedadCambiada<DateTime?>(nameof(FacturaEmtDtm.VenceEl), parametros))
            {
                fae.CrearTraza(Contexto, "Modificación del vencimiento", $"El usuario '{Contexto.DatosDeConexion.Login}' ha modificado el " +
                    $"vencimiento '{((DateTime)((FacturaEmtDtm)parametros.registroEnBd).VenceEl).ToShortDateString()}' " +
                    $"por el de '{((DateTime)fae.VenceEl).ToShortDateString()}'");

                fae.ModificarEventoDeVencimiento(Contexto);
            }

            if (!((string)parametros.Parametros.LeerValor(ltrDeUnaFacturaEmt.CambioDePpt, "")).IsNullOrEmpty())
                new TrazasDeUnaFacturaEmtDtm
                {
                    IdElemento = fae.Id,
                    Nombre = "Modificación de la factura",
                    Descripcion = (string)parametros.Parametros[ltrDeUnaFacturaEmt.CambioDePpt]
                }
                .Insertar(Contexto);

            if (!((string)parametros.Parametros.LeerValor(ltrDeUnaFacturaEmt.CambioDeParteEnFae, "")).IsNullOrEmpty())
                new TrazasDeUnaFacturaEmtDtm
                {
                    IdElemento = fae.Id,
                    Nombre = "Modificación de la factura",
                    Descripcion = (string)parametros.Parametros[ltrDeUnaFacturaEmt.CambioDeParteEnFae]
                }
                .Insertar(Contexto);
        }

        protected override void EliminarCaches(FacturaEmtDtm fae, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(fae, parametros);

            if (fae.PropiedadCambiada<int?>(nameof(FacturaEmtDtm.IdPresupuesto), parametros))
            {
                var indice = fae.IdPresupuesto is null ? ((FacturaEmtDtm)parametros.registroEnBd).IdPresupuesto.ToString() : fae.IdPresupuesto.ToString();
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_TieneFacturas, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Facturado, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Ejecutando, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Ejecutado, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Prefacturado, indice);

                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(indice.Entero());

                if (ppt.IdExpediente is not null) ServicioDeCaches.EliminarElemento(CacheDe.Exp_Ingresos, ppt.IdExpediente.ToString());
            }
            if (fae.Numero.HasValue) ServicioDeCaches.EliminarElementos(CacheDe.Fae_FacturaPorNumero, patron: fae.NumeroDeFactura);
        }


        protected override FacturaEmtDtm AntesDeTransitar(FacturaEmtDtm factura, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            factura = base.AntesDeTransitar(factura, transicion, parametros);

            var accion = parametros.LeerValor(ltrParametrosNeg.AccionQueSeEjecuta, "");
            if (accion != VariableDeFacturasEmt.enumMotivoTransicion.RectificarFactura.Descripcion()
                && factura.EstaRectificada(Contexto)
                && factura.RectificadaPor(Contexto).EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                GestorDeErrores.Emitir($"No se puede transitar la factura '{factura.Referencia}' por estar siendo rectificada por la factura '{factura.RectificadaPor(Contexto).Referencia}'");

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Anulada.Estados()))
                factura = factura.AntesDeAnular(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados()))
            {
                if (SemaforoDeProcesoSql.SemaforosColocados(Contexto, IdNegocio) > 1)
                    GestorDeErrores.Emitir($"No se puede emitir la factura '{factura.Referencia}' por estar otra factura emitíendose, inténtelo de nuevo");

                factura.AntesDeEmitirFactura(Contexto, parametros);
            }

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados()))
            {
                if (SemaforoDeProcesoSql.SemaforosColocados(Contexto, IdNegocio) > 1)
                    GestorDeErrores.Emitir($"No se puede corregir la factura '{factura.Referencia}' por estar procesándose otra factura, inténtelo de nuevo");
                factura.IdArchivo = Contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id, usarLaCache: true).IdArchivo;
                factura.AntesDeDevolverAPrefactura(Contexto, parametros);
            }

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada.Estados()))
                factura.AntesDeRectificar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Abonada.Estados()))
                factura.AntesDeAbonarUnaRectificativa(Contexto, parametros);


            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Estados()))
                factura.AntesDeReclamar(Contexto, parametros);

            return factura;
        }

        protected override FacturaEmtDtm DespuesDeTransitar(FacturaEmtDtm factura, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            factura = base.DespuesDeTransitar(factura, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados()))
                factura = factura.TrasEmitirFactura(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados()))
                factura = factura.DespuesDeDevolverAPrefactura(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Estados()))
                factura = factura.DespuesDePasaAReclamacion(Contexto, parametros);

            return factura;

        }

        private void ValidarAsociarPresupuesto(FacturaEmtDtm factura, ParametrosDeNegocio parametros)
        {
            if (factura.IdPresupuesto is null) return;
            if (factura.IdParteTr is not null) return;

            var ppt = factura.Presupuesto(Contexto);

            if (ppt.Sociedad(Contexto).Id != factura.Sociedad(Contexto).Id)
                GestorDeErrores.Emitir($"Las sociedades del {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' y la {Negocio.Singular(true)} '{factura.Referencia}' han de ser la misma");

            if (!ppt.EstaEnLaEtapa(enumEtapasDePpts.PPT_Etapa_PermiteFacturar))
                GestorDeErrores.Emitir($"No se puede {parametros.Operacion.Descripcion(true)} una {Negocio.Singular(true)} por no estar el {enumNegocio.Presupuesto.Singular(true)} en la etapa '{enumEtapasDePpts.PPT_Etapa_PermiteFacturar.Nombre(true)}'");

            if (factura.PropiedadCambiada<int?>(nameof(FacturaEmtDtm.IdPresupuesto), parametros))
            {
                var pptAnterior = ((FacturaEmtDtm)parametros.registroEnBd).Presupuesto(Contexto);

                if (pptAnterior != null && !pptAnterior.EstaEnLaEtapa(enumEtapasDePpts.PPT_Etapa_PermiteFacturar))
                    GestorDeErrores.Emitir($"No se puede modificar la {Negocio.Singular(true)} por no estar el {enumNegocio.Presupuesto.Singular(true)} ya asociado en la etapa '{enumEtapasDePpts.PPT_Etapa_PermiteFacturar.Nombre(true)}'");
            }
        }

        private void ValidarAsociarParteDeTrabajo(FacturaEmtDtm factura, ParametrosDeNegocio parametros)
        {
            if (factura.IdParteTr.Entero() == 0) return;

            var parte = factura.ParteTr(Contexto);

            if (parte.Sociedad(Contexto).Id != factura.Sociedad(Contexto).Id)
                GestorDeErrores.Emitir("Las sociedades del parte de trabajo y la factura han de ser la misma");

            if (parte.IdContrato != null && factura.IdContrato != null && parte.IdContrato != factura.IdContrato)
                GestorDeErrores.Emitir("El contrato del parte de trabajo y de la factura han de ser el mismo");

            bool cambiandoParteTr = parametros.Modificando
                  && factura.PropiedadCambiada<int?>((FacturaEmtDtm)parametros.registroEnBd, nameof(FacturaEmtDtm.IdParteTr));

            if (parametros.Insertando || cambiandoParteTr || parametros.Eliminando)
            {
                if (!ExtensorDeElementos.EstaEnEtapa(Contexto, enumNegocio.ParteDeTrabajo, (int)factura.IdParteTr, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados()))
                    GestorDeErrores.Emitir("No se puede asociar la factura por no estar el parte de trabajo en la etapa de aceptación de facturas");
            }

            if (cambiandoParteTr)
            {
                int idPermiso = factura.Tipo<TipoDeFacturaEmtDtm>(Contexto).IdPermisoInterventor;

                if (!ApiDePermisos.TieneElPermiso(Contexto, idPermiso))
                    GestorDeErrores.Emitir($"Para un cambio de {enumNegocio.ParteDeTrabajo.Singular(true)} se necesitan permisos de intervención");

                var idAnterior = (int)((FacturaEmtDtm)parametros.registroEnBd).IdParteTr;
                var ptrAnterior = ((FacturaEmtDtm)parametros.registroEnBd).ParteTr(Contexto);

                if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.ParteDeTrabajo, idAnterior) == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir($"No se puede modificar la {Negocio.Singular(true)} por no tener acceso al {enumNegocio.ParteDeTrabajo.Singular(true)}");

                if (!ExtensorDeElementos.EstaEnEtapa(Contexto, enumNegocio.ParteDeTrabajo, idAnterior, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados()))
                    GestorDeErrores.Emitir($"No se puede desasociar una  {Negocio.Singular(true)} por no estar el {enumNegocio.ParteDeTrabajo.Singular(true)}  asociado en la etapa de desasociar una factura");

                parametros.Parametros[ltrDeUnaFacturaEmt.CambioDeParteEnFae] = $"Se ha susutituido el {enumNegocio.ParteDeTrabajo.Singular(true)} '{ptrAnterior.Referencia}' por el '{parte.Referencia}'";
            }
        }

        private void AccionesAlPpt(FacturaEmtDtm fae, ParametrosDeNegocio parametros)
        {
            if (parametros.Insertando && fae.IdPresupuesto.Entero() > 0 && !parametros.Copiando)
            {
                fae.CopiarLineasDelPpt(Contexto);
                fae.FacturarTareas(Contexto);
            }
        }

        protected override void DatosParaElMapeo(List<FacturaEmtDtm> facturas, Dictionary<string, object> parametros)
        {
            base.DatosParaElMapeo(facturas, parametros);
            var idsFacturas = facturas.Select(f => f.Id).ToList();
            ExtensorDeFacturasEmt.CargarCacheDeTotales(Contexto, idsFacturas);
            ExtensorDeFacturasEmt.CargarCacheDeAbonos(Contexto, idsFacturas);
            ExtensorDeFacturasEmt.CargarCacheDeCobros(Contexto, idsFacturas);
            ExtensorDeFacturasEmt.CargarCacheDeRectificadaPor(Contexto, idsFacturas);
            ExtensorDeFacturasEmt.CargarCacheDeRectificadaA(Contexto, idsFacturas);
        }

        protected override void DespuesDeMapearElElemento(FacturaEmtDtm factura, FacturaEmtDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(factura, elemento, parametros);

            elemento.TotalSinIva = factura.Bi(Contexto);
            elemento.APagar = factura.APagar(Contexto);
            elemento.TotalIva = factura.TotalDeIva(Contexto);
            elemento.TotalIrpf = factura.TotalDeIrpf(Contexto);

            if (parametros.Insertando || factura.Serie.IsNullOrEmpty()) elemento.Serie = factura.Tipo<TipoDeFacturaEmtDtm>(Contexto).Serie;
            if (factura.FacturadaEl == default) elemento.Ano = DateTime.Now.Year;

            if (parametros.LeerPorId)
            {
                elemento.UsaCentroAdministrativo = factura.Cliente(Contexto).HayDetallesDe<CentroAdministrativoDtm>(Contexto);
                elemento.EstaComunicandose = factura.EstaComunicandose(Contexto);

                elemento.EsInterventor = factura.EsInterventor<TipoDeFacturaEmtDtm>(Contexto);
                elemento.Etapas = factura.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);
                elemento.EsPeriodica = factura.EsPeriodica(Contexto);
                elemento.ConIrpf = factura.Ampliacion<IrpfEmtDtm>(Contexto, errorSiNoHay: false) is not null;
                elemento.EsRectificativa = factura.ClaseRectificativa is not null;

                if (factura.IdPreasiento is not null)
                {
                    elemento.IdPreasiento = factura.IdPreasiento;
                    elemento.Preasiento = factura.Preasiento(Contexto, errorSiNoHay: false)?.Expresion ?? "Sin preasiento";
                }

                if (factura.ClaseRectificativa is not null)
                {
                    elemento.enumClaseRectificativa = factura.ClaseRectificativa;
                    elemento.ClaseRectificativa = factura.ClaseRectificativa.Descripcion();
                    elemento.MotivoDeRectificacion = factura.MotivoDeRectificacion?.Descripcion() ?? enumMotivoDeRectificacion.DatosErroneos.Descripcion();
                }

                if (factura.EstaRectificada(Contexto))
                {
                    var rectificativa = factura.RectificadaPor(Contexto);
                    elemento.enumClaseRectificativa = rectificativa.ClaseRectificativa;
                    elemento.IdRectificativa = rectificativa.Id;
                    elemento.Rectificativa = rectificativa.Expresion;
                    elemento.ClaseRectificativa = rectificativa.ClaseRectificativa.Descripcion();
                    elemento.MotivoDeRectificacion = rectificativa.MotivoDeRectificacion?.Descripcion() ?? enumMotivoDeRectificacion.DatosErroneos.Descripcion();

                }

                elemento.EsExportacion = factura.Tipo<TipoDeFacturaEmtDtm>(Contexto).EsExportacion;
                var direccion = factura.DireccionFiscal(Contexto, erroSiNoHay: false);
                elemento.EsIntraComunitaria = direccion is null ? null : direccion.IntraComunitaria;

                if (elemento.UsaCentroAdministrativo && factura.IdCentroAdministrativo is not null)
                {
                    var ca = Contexto.SeleccionarPorId<CentroAdministrativoDtm>(factura.IdCentroAdministrativo.Entero());
                    elemento.CentroAdministrativo = ca.Expresion;
                }
            }

            if (parametros.LeerPorId ||
               (parametros.CargarGridDeRelacion && parametros.Filtros.Any(x => x.Clausula == ltrDeUnaFacturaEmt.IdExpediente) && factura.IdPresupuesto.HasValue) ||
               (parametros.LeerDatosParaElGridOParaExportar && parametros.ColumnasDelGrid.Any(item => item == nameof(elemento.Cobrado).ToLowerInvariant() ||
                                            item == nameof(elemento.Pendiente).ToLowerInvariant() ||
                                            item == nameof(elemento.EstaCobrada).ToLowerInvariant())))

            {
                elemento.Cobrado = factura.EsRectificativa ? factura.Abonado(Contexto) : factura.Cobrado(Contexto);
                elemento.Pendiente = factura.EsRectificativa ? factura.PendientePorAbonar(Contexto) : factura.PendientePorCobrar(Contexto);
                elemento.PorAbonar = factura.PendientePorAbonar(Contexto);
                elemento.EstaCobrada = factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) || factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Anulada) ? false : factura.EstaCobrada(Contexto);
            }

            if (parametros.LeerDatosParaElGridOParaExportar)
            {
                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaEmtDto.IrAlExpediente), StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (factura.IdPresupuesto is null)
                        elemento.IrAlExpediente = null;
                    else
                    {
                        var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>((int)factura.IdPresupuesto);
                        if (ppt.IdExpediente is not null)
                            elemento.IrAlExpediente = ppt.Expediente(Contexto).Referencia;
                    }
                }

                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaEmtDto.IrARectificadaPor), StringComparison.InvariantCultureIgnoreCase)))
                {
                    var rectificadaPor = factura.RectificadaPor(Contexto, errorSiNoHay: false);
                    elemento.IrARectificadaPor = rectificadaPor == null ? null : rectificadaPor.Referencia + ": " + (rectificadaPor.NumeroDeFactura.IsNullOrEmpty() ? "no emitida" : rectificadaPor.NumeroDeFactura);
                }

                if (parametros.ColumnasDelGrid.Any(e => e.Equals(nameof(FacturaEmtDto.IrARectificoA), StringComparison.InvariantCultureIgnoreCase)))
                {
                    var rectificoA = factura.RectificaA(Contexto, errorSiNoHay: false);
                    elemento.IrARectificoA = rectificoA == null ? null : rectificoA.Referencia + ": " + rectificoA.NumeroDeFactura;
                }
            }

            if (parametros.LeerPorIdParaEditar)
            {
                if (elemento.IdTransicionAplicable is not null)
                {
                    if (enumEtapasDeFacturasEmt.FAE_Etapa_Abonada.Lista().Contains(elemento.IdEstadoDestino.Entero()) && !factura.EsRectificativa)
                    {
                        elemento.IdTransicionAplicable = null;
                        elemento.TransicionAplicable = null;
                    }
                }

                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                {
                    if (!factura.EsRectificativa && elemento.IdEstadoDestino is not null && enumEtapasDeFacturasEmt.FAE_Etapa_Abonada.EstadosDeLaEtapa().estados.Contains((int)elemento.IdEstadoDestino))
                    {
                        elemento.IdTransicionAplicable = null;
                        elemento.TransicionAplicable = null;
                        elemento.IdEstadoDestino = null;
                    }
                    if (VariableDeFacturasEmt.Fae_Sii_Activo() && elemento.IdEstadoAnterior is not null && enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.EstadosDeLaEtapa().estados.Contains((int)elemento.IdEstadoAnterior))
                    {
                        elemento.IdEstadoAnterior = null;
                        elemento.EstadoAnterior = null;
                    }
                }

                if (elemento.EstaComunicandose)
                {
                    elemento.EstadoAnterior = null;
                    elemento.IdEstadoAnterior = null;
                    elemento.IdTransicionAplicable = null;
                    elemento.TransicionAplicable = null;
                    elemento.IdEstadoDestino = null;
                }

                //elemento.ImportePorAbonar = factura.PendientePorAbonar(Contexto);
                //elemento.ImportePorCobrar = factura.PendientePorCobrar(Contexto);
            }
        }


        public static void FacturarTareas(ContextoSe contexto, int idFactura, Dictionary<string, object> parametros)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);

            var etapas = factura.Etapas();
            if (!etapas.Contains(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' debe ser una prefactura");

            var idExpediente = parametros.LeerValor<int?>(nameof(FacturarTareasDto.IdExpediente), null);
            var plfDeInicio = parametros.LeerValor<DateTime?>(nameof(FacturarTareasDto.PlfDeInicio), null);
            var plfDeFin = parametros.LeerValor<DateTime?>(nameof(FacturarTareasDto.PlfDeFin), null);

            if (idExpediente.Entero() == 0 && (plfDeFin is null || plfDeInicio is null))
                GestorDeErrores.Emitir($"Ha de indicar un expediente a facturar o una ventana de planificación");

            if (idExpediente.Entero() > 0 && (plfDeFin is not null || plfDeInicio is not null))
                GestorDeErrores.Emitir($"Ha de indicar un expediente a facturar o una ventana de planificación, no ambas");

            var crearLineaPorTarea = parametros.LeerValor<bool>(nameof(FacturarTareasDto.CrearLineasPorTarea));
            var sumarizarEnUnaLinea = parametros.LeerValor<bool>(nameof(FacturarTareasDto.CrearLineaPorTotal));
            if (sumarizarEnUnaLinea && crearLineaPorTarea)
                GestorDeErrores.Emitir($"Hay que indicar si la factura es por total de las tareas o desglosada por ellas");
            if (!sumarizarEnUnaLinea && !crearLineaPorTarea)
                GestorDeErrores.Emitir($"Para poder facturar ha de indicarse si es por tarea o se totaliza la selección");

            var medidoEn = parametros.LeerValor<enumDurabilidad?>(nameof(FacturarTareasDto.MedidoEn));
            if (medidoEn is null)
                GestorDeErrores.Emitir($"Para poder facturar ha de indicarse la unida a facturar");
            var unidad = contexto.SeleccionarPorNombre<UnidadDtm>(((enumDurabilidad)medidoEn).ToUnidadDeMedida(), errorSiNoHay: false);
            if (unidad == null)
                GestorDeErrores.Emitir($"Para poder facturar ha de crear la unida de medidad '{medidoEn.Descripcion()}'");

            var idNaturaleza = parametros.LeerValor<int?>(nameof(FacturarTareasDto.IdNaturaleza));
            if (idNaturaleza.Entero() == 0)
            {
                idNaturaleza = factura.Tipo<TipoDeFacturaEmtDtm>(contexto).IdNaturalezaDefecto;
                if (idNaturaleza.Entero() == 0)
                    GestorDeErrores.Emitir($"Para poder facturar ha de indicar la naturaleza de a aplicar a las tareas");
            }

            var precio = parametros.LeerValor<decimal?>(nameof(FacturarTareasDto.Precio));
            if (precio is null)
                GestorDeErrores.Emitir($"Para poder facturar ha de indicarse el precio por unidad");

            var idtipo = parametros.LeerValor<int?>(nameof(FacturarTareasDto.IdTipoTarea));


            List<TareaDtm> tareas;
            ExpedienteDtm expediente = null;
            if (idExpediente.Entero() > 0)
            {
                expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)idExpediente);
                tareas = expediente.TareasTerminadas(contexto).Where(t => (idtipo == null || t.IdTipo == (int)idtipo) && t.IdFacturaEmt == null).ToList();
            }
            else
            {
                var filtros = new List<ClausulaDeFiltrado>
                     {
                         new ClausulaDeFiltrado { Clausula = ltrParametrosNeg.QueMostrar, Criterio = enumCriteriosDeFiltrado.igual, Valor = ltrParametrosNeg.Terminados.ToString() },
                         new ClausulaDeFiltrado { Clausula = ltrPlfDeTarea.FiltroPorPlfDeInicio, Criterio = enumCriteriosDeFiltrado.mayorIgual, Valor = plfDeInicio.ToString() },
                         new ClausulaDeFiltrado { Clausula = ltrPlfDeTarea.FiltroPorPlfDeFin, Criterio = enumCriteriosDeFiltrado.menorIgual, Valor = plfDeFin.ToString() },
                         new ClausulaDeFiltrado { Clausula = nameof(TareaDtm.IdFacturaEmt), Criterio = enumCriteriosDeFiltrado.esNulo}
                     };
                if (idtipo.Entero() > 0)
                    filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(IUsaTipo.IdTipo), Criterio = enumCriteriosDeFiltrado.igual, Valor = idtipo.ToString() });
                tareas = contexto.SeleccionarTodos<TareaDtm>(filtros, aplicarJoin: true);
                tareas = tareas.Where(t => t.Estado.Terminado).ToList();
            }

            if (tareas.Count == 0)
                GestorDeErrores.Emitir($"Con los criterios indicados no se han localizado tareas terminadas pendiente de facturar");

            var idIvaRep = factura.Tipo<TipoDeFacturaEmtDtm>(contexto).IdIvaRDefecto;
            var porcentajeDeIva = idIvaRep is null ? (decimal?)null : contexto.SeleccionarPorId<IvaRepercutidoDtm>(idIvaRep.Entero()).Porcentaje;
            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto);
                var orden = lineas.Count() == 0 ? 10 : lineas.Max(x => x.Orden) + 10;
                foreach (var tarea in tareas)
                {
                    tarea.IdFacturaEmt = idFactura;
                    tarea.Modificar(contexto, accionEjecutada: nameof(ltrDeUnaTarea.Accion_IncluirEnLaFactura));
                    if (crearLineaPorTarea)
                    {
                        CrearLineaDeFactura(contexto, factura, tarea, orden, (decimal)precio, (enumDurabilidad)medidoEn, unidad,
                            idNaturaleza.Entero(), idIvaRep, porcentajeDeIva);
                        orden = orden + 10;
                    }
                }
                if (sumarizarEnUnaLinea)
                {
                    var concepto = expediente is null ? "tareas realizadas" : $"tareas del expediente '{expediente.Referencia}'";
                    CrearLineaPorTotal(contexto, factura, tareas, (decimal)precio, (enumDurabilidad)medidoEn, unidad,
                              idNaturaleza.Entero(), orden, idIvaRep, porcentajeDeIva, concepto);
                }
                contexto.Commit(transaccion);
            }
            catch (Exception ex)
            {
                contexto.Rollback(transaccion, ex);
                throw;
            }

        }

        private static void CrearLineaPorTotal(ContextoSe contexto, FacturaEmtDtm factura, List<TareaDtm> tareas, decimal precio,
            enumDurabilidad medidoEn, UnidadDtm unidad, int idNaturaleza, int orden, int? idIvaRep, decimal? porcentajeDeIva, string concepto)
        {
            decimal? cantidad = 0;
            foreach (var tarea in tareas)
            {
                var planificacion = tarea.Ampliacion<PlfDeTareaDtm>(contexto);
                cantidad = cantidad + planificacion.DuracionEn(medidoEn) ?? 0;
            }
            new LineaDeUnaFaeDtm
            {
                IdElemento = factura.Id,
                Orden = orden,
                TipoDeLinea = enumTipoDeLinea.Alzada,
                Concepto = concepto.Left(IDominio.Longitud(IDominio.VARCHAR_250)),
                Cantidad = cantidad,
                Precio = precio,
                IdIvaR = idIvaRep,
                Clase = enumClaseUnitario.Servicio,
                Iva = porcentajeDeIva,
                IdUnidad = unidad.Id,
                IdNaturaleza = idNaturaleza
            }.Insertar(contexto, nameof(CrearLineaPorTotal));

            factura.CrearTraza(contexto, $"Se han facturado las tareas seleccionadas",
                $"El Usuario '{contexto.DatosDeConexion.Login}' ha facturado {Environment.NewLine}" +
                $"Cantidad: {cantidad.Formatear().Trim()}{Environment.NewLine}" +
                $"Precio: {precio.Formatear().Trim()}{Environment.NewLine}" +
                $"Tareas:{string.Join(Environment.NewLine, tareas.Select(t => t.Referencia))}");
        }

        private static void CrearLineaDeFactura(ContextoSe contexto, FacturaEmtDtm factura, TareaDtm tarea,
            int orden, decimal precio, enumDurabilidad medidoEn, UnidadDtm unidad, int idNaturaleza, int? idIvaRep, decimal? porcentajeDeIva)
        {
            var planificacion = tarea.Ampliacion<PlfDeTareaDtm>(contexto);
            new LineaDeUnaFaeDtm
            {
                IdElemento = factura.Id,
                Orden = orden,
                TipoDeLinea = enumTipoDeLinea.Alzada,
                Concepto = tarea.Expresion.Left(IDominio.Longitud(IDominio.VARCHAR_250)),
                Cantidad = planificacion.DuracionEn(medidoEn),
                Precio = precio,
                IdIvaR = idIvaRep,
                Iva = porcentajeDeIva,
                Clase = enumClaseUnitario.Servicio,
                IdUnidad = unidad.Id,
                IdNaturaleza = idNaturaleza
            }.Insertar(contexto, nameof(CrearLineaDeFactura));

            factura.CrearTraza(contexto, $"Se ha facturado la tarea: {tarea.Referencia}", $"El Usuario '{contexto.DatosDeConexion.Login}' ha facturado la cantidad de {planificacion.DuracionEn(medidoEn).Formatear()} al precio de {precio.Formatear()}");
        }

        public static void CambiarVencimiento(ContextoSe contexto, int idFactura, DateTime nuevoVencimiento)
        {
            var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);

            if (nuevoVencimiento <= DateTime.Now.Date)
                GestorDeErrores.Emitir($"El nuevo vencimiento '{nuevoVencimiento.ToShortDateString()}' de la factura '{fae.Referencia}' ha de ser mayor al día de hoy");

            var etapas = fae.Etapas();
            if (!etapas.Contains(enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro))
                GestorDeErrores.Emitir($"La factura '{fae.Referencia}' ha de estar en la etapa de cobro'");

            if (fae.VenceEl >= nuevoVencimiento)
                GestorDeErrores.Emitir($"El nuevo vencimiento '{nuevoVencimiento.ToShortDateString()}' de la factura '{fae.Referencia}' ha de ser mayor al que tiene '{((DateTime)fae.VenceEl).ToShortDateString()}'");
            fae.VenceEl = nuevoVencimiento;
            var transaccion = contexto.IniciarTransaccion();
            try
            {
                fae = fae.Modificar(contexto, accionEjecutada: ltrDeUnaFacturaEmt.Accion_CambiarVencimiento);
                if (fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion))
                {
                    var transicionesAplicables = TransicionAplicable.Transiciones(VariableDeFacturasEmt.TransicionesPorMotivo, VariableDeFacturasEmt.enumMotivoTransicion.ModificarVencimiento, errorSiNoHay: true);
                    ValidarQueSoloHayUnaTransicion(contexto, fae, transicionesAplicables);
                    fae.Transitar(contexto, transicionesAplicables[0].IdTransicion);
                }
                contexto.Commit(transaccion);
            }
            catch (Exception ex)
            {
                contexto.Rollback(transaccion, ex);
                throw;
            }

        }

        public static void CambiarDatos(ContextoSe contexto, int idFactura, CambiarDatosFae nuevosDatos)
        {
            var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura, usarLaCache: false);
            var etapas = fae.Etapas();

            if (nuevosDatos.IdCliente == fae.IdCliente && nuevosDatos.IdPresupuesto == fae.IdPresupuesto && nuevosDatos.IdContrato == fae.IdContrato)
                GestorDeErrores.Emitir($"Los datos proporcionados para modificar la factura '{fae.Referencia}' son los mismo que tiene'");

            if (etapas.Contains(enumEtapasDeFacturasEmt.FAE_Etapa_Anulada))
                GestorDeErrores.Emitir($"La factura '{fae.Referencia}' está anulada, no es modificable'");

            var clienteAnterior = fae.Cliente(contexto);
            if (!etapas.Contains(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) && clienteAnterior.Id != nuevosDatos.IdCliente)
                GestorDeErrores.Emitir($"La factura '{fae.Referencia}' ha de estar en etapa de prefacturación para poder cambiar los datos del cliente'");

            var heQuitadoContrato = fae.IdContrato.HasValue && nuevosDatos.IdContrato is null || fae.IdContrato.HasValue && nuevosDatos.IdContrato.HasValue && fae.IdContrato != nuevosDatos.IdContrato;
            var hePuestoContrato = fae.IdContrato is null && nuevosDatos.IdContrato.HasValue || fae.IdContrato.HasValue && nuevosDatos.IdContrato.HasValue && fae.IdContrato != nuevosDatos.IdContrato;
            var contratoAnterior = fae.IdContrato.HasValue ? fae.Contrato(contexto) : null;


            var heQuitadoPresupuesto = fae.IdPresupuesto.HasValue && nuevosDatos.IdPresupuesto is null || fae.IdPresupuesto.HasValue && nuevosDatos.IdPresupuesto.HasValue && fae.IdPresupuesto != nuevosDatos.IdPresupuesto;
            var hePuestoPresupuesto = fae.IdPresupuesto is null && nuevosDatos.IdPresupuesto.HasValue || fae.IdPresupuesto.HasValue && nuevosDatos.IdPresupuesto.HasValue && fae.IdPresupuesto != nuevosDatos.IdPresupuesto;
            var pptAnterior = fae.IdPresupuesto.HasValue ? fae.Presupuesto(contexto) : null;

            if ((hePuestoContrato || heQuitadoContrato) && fae.IdParteTr.HasValue)
            {
                var parte = fae.ParteTr(contexto);
                GestorDeErrores.Emitir($"El contrato de la factura no es modificable si tiene parte de trabajo asociado");
            }

            fae.IdContrato = nuevosDatos.IdContrato;
            fae.IdPresupuesto = nuevosDatos.IdPresupuesto;
            fae.IdCliente = nuevosDatos.IdCliente;

            if (fae.IdContrato.HasValue)
            {
                if (fae.Contrato(contexto).Cliente(contexto).Id != fae.Cliente(contexto).Id)
                    GestorDeErrores.Emitir($"El contrato de la factura ha de ser del mismo cliente que la factura");
                if (fae.IdParteTr.HasValue && fae.ParteTr(contexto).IdContrato.HasValue && fae.IdContrato != fae.ParteTr(contexto).IdContrato)
                    GestorDeErrores.Emitir($"El contrato de la factura ha de ser el mismo que el del parte de trabajo '{fae.ParteTr(contexto).Referencia}'");
                if (fae.Planificacion(contexto) is not null && fae.Planificacion(contexto).IdContrato.HasValue && fae.IdContrato != fae.Planificacion(contexto).IdContrato)
                    GestorDeErrores.Emitir($"El contrato de la factura ha de ser el mismo que el de la planificación '{fae.Planificacion(contexto).Referencia}'");
            }

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                fae = fae.Modificar(contexto, accionEjecutada: ltrDeUnaFacturaEmt.Accion_CambiarDatos);
                if (fae.Etapa() != enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura)
                    RecalcularSaldosContratos(contexto, fae, heQuitadoContrato, hePuestoContrato, contratoAnterior);

                var t = $"{(clienteAnterior.Id != fae.IdCliente ? $"Cliente anterior: {clienteAnterior.Expresion}{Environment.NewLine}" : "")}" +
                        $"{(hePuestoContrato && !heQuitadoContrato ? $"Contrato añadido{Environment.NewLine}" : contratoAnterior?.Id != fae.IdContrato ? $"Contrato anterior: {contratoAnterior.Expresion}{Environment.NewLine}" : "")}" +
                        $"{(hePuestoPresupuesto && !heQuitadoPresupuesto ? "Presupuesto añadido" : pptAnterior?.Id != fae.IdPresupuesto ? $"Presupuesto anterior: {pptAnterior.Expresion}{Environment.NewLine}" : "")}";

                fae.CrearTraza(contexto, $"Datos modificados por '{contexto.Usuario.Login}'", $"Se han modificado los datos de la factura {Environment.NewLine}{t}");
                contexto.Commit(transaccion);
            }
            catch (Exception ex)
            {
                contexto.Rollback(transaccion, ex);
                throw;
            }
        }

        private static void RecalcularSaldosContratos(ContextoSe contexto, FacturaEmtDtm fae, bool heQuitadoContrato, bool hePuestoContrato, ContratoDtm contratoAnterior)
        {
            var planificacion = fae.Planificacion(contexto);
            if (heQuitadoContrato)
            {
                if (planificacion == null)
                    contratoAnterior.RecalcularAvance(contexto, enumAvaceOperacion.FacturarContrato, -1 * fae.Bi(contexto), 0);
                else
                    contratoAnterior.RecalcularAvance(contexto, enumAvaceOperacion.FacturarPlanificado, -1 * fae.Bi(contexto), planificacion.Total(contexto, conIva: false));
            }
            if (hePuestoContrato)
            {
                if (planificacion == null)
                    fae.Contrato(contexto).RecalcularAvance(contexto, enumAvaceOperacion.FacturarContrato, fae.Bi(contexto), 0);
                else
                    fae.Contrato(contexto).RecalcularAvance(contexto, enumAvaceOperacion.FacturarPlanificado, fae.Bi(contexto), planificacion.Total(contexto, conIva: false));
            }
        }

        private static void ValidarQueSoloHayUnaTransicion(ContextoSe contexto, FacturaEmtDtm fae, List<TransicionAplicable> transicionesAplicables)
        {
            var posibles = transicionesAplicables.Count(x => x.IdEstado == fae.IdEstado);
            if (fae.EstaParcialmenteCobrada(contexto))
            {
                if (posibles == 0)
                    GestorDeErrores.Emitir($"No hay transicion definida para cambiar el estado '{fae.Estado(contexto).Nombre}' de la factura '{fae.Referencia}' a la etapa {enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial}");

                if (posibles > 2)
                    GestorDeErrores.Emitir($"Hay más de una transicion definida para cambiar el estado '{fae.Estado(contexto).Nombre}' de la factura '{fae.Referencia}' a la etapa {enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial}");
            }
            else
            {
                if (posibles == 0)
                    GestorDeErrores.Emitir($"No hay transicion definida para cambiar el estado '{fae.Estado(contexto).Nombre}' de la factura '{fae.Referencia}' a la etapa {enumEtapasDeFacturasEmt.FAE_Etapa_Emitida}");

                if (posibles > 2)
                    GestorDeErrores.Emitir($"Hay más de una transicion definida para cambiar el estado '{fae.Estado(contexto).Nombre}' de la factura '{fae.Referencia}' a la etapa {enumEtapasDeFacturasEmt.FAE_Etapa_Emitida}");
            }
        }

        public static FacturaEmtDto CopiarLa(ContextoSe contexto, int idFacturaOriginal)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFacturaOriginal);
            Dictionary<string, object> parametros = new Dictionary<string, object>
            {
                {nameof(CopiarFaeDto.IdElemento), factura.Id },
                {nameof(CopiarFaeDto.IdTipo), factura.IdTipo },
                {nameof(CopiarFaeDto.IdCg), factura.IdCg },
                {nameof(CopiarFaeDto.IdCliente), factura.IdCliente },
                {nameof(CopiarFaeDto.Nombre), factura.Nombre },
                {nameof(CopiarFaeDto.Descripcion), factura.Descripcion }
            };
            var copia = CopiarInterno(contexto, parametros);
            return contexto.SeleccionarDto<FacturaEmtDto, FacturaEmtDtm>(copia.Id);
        }

        public static int CopiarFae(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CopiarFaeDto.IdElemento))) GestorDeErrores.Emitir("No se ha indicado la factura a copiar");
            if (!parametros.ContieneClave(nameof(CopiarFaeDto.IdTipo))) GestorDeErrores.Emitir("No se ha indicado el tipo de la nueva factura");
            if (!parametros.ContieneClave(nameof(CopiarFaeDto.IdCg))) GestorDeErrores.Emitir("No se ha indicado el cg de la nueva factura");
            if (!parametros.ContieneClave(nameof(CopiarFaeDto.IdCliente))) GestorDeErrores.Emitir("No se ha indicado el cliente de la nueva factura");
            if (!parametros.ContieneClave(nameof(CopiarFaeDto.Nombre))) GestorDeErrores.Emitir("No se ha indicado el nombre de la nueva factura");
            if (!parametros.ContieneClave(nameof(CopiarFaeDto.Descripcion))) GestorDeErrores.Emitir("No se ha indicado la descripción de la nueva factura");
            return CopiarInterno(contexto, parametros).Id;
        }

        private static FacturaEmtDtm CopiarInterno(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            parametros[ltrParametrosNeg.Copiando] = true;
            var faeOrigen = contexto.SeleccionarPorId<FacturaEmtDtm>(parametros.LeerValor<int>(nameof(CopiarFaeDto.IdElemento)));
            return faeOrigen.Copiar(contexto, parametros);
        }


        public static void GenerarPruebaDeEFactura(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Prueba-{factura.Referencia}.xml".NormalizarFichero());
            new GeneradorDeFacturaEmtXml(contexto, factura, rutaConFichero).Generar();
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            AsociarArchivoFactura(contexto, factura, idArchivo, original: false);
        }

        public static bool EnviarFacturaAeat(ContextoSe contexto, int Idfactura, bool someterEnvio)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(Idfactura);
            var anterior = factura.Anterior(contexto);

            if ((anterior is not null && anterior.Verifactu(contexto, errorSiNoHay: false) is not null) || anterior is null)
            {
                new LogDeEnvioDeFacturaDtm { IdFactura = factura.Id }.Insertar(contexto, new Dictionary<string, object> { { ltrDeLogDeEnvioDeFactura.SometerEnvio, someterEnvio } });
                return true;
            }

            var loteDeEnvio = new List<int> { factura.Id };
            while (anterior is not null)
            {
                var ficheroCsv = new GeneradorSii(contexto, anterior).FicheroDondeEstaLaFactura();
                if (!ficheroCsv.IsNullOrEmpty())
                {
                    GestorDeErrores.Emitir(ltrSii.FacturaEnviada.Replace(nameof(FacturaEmtDtm.Referencia), anterior.Referencia).Replace(ltrSii.ficheroCsv, ficheroCsv));
                }

                loteDeEnvio.Insert(0, anterior.Id);
                anterior = anterior.Anterior(contexto);
                if (anterior.Verifactu(contexto, errorSiNoHay: false) is not null)
                    break;
                if (anterior.Ano < factura.Ano)
                    break;
            }
            TrabajosDeFacturasEmt.SometerEnvioDeLoteDeFacturaAeat(contexto, loteDeEnvio, factura.Sociedad(contexto).Id, idsemaforo: 0);
            return false;
        }

        public static void EmitirPdfFactura(ContextoSe contexto, FacturaEmtDto factura)
        {
            var facturaDtm = contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
            if (facturaDtm.ClaseDeEmision != enumClaseDeEmision.Impresa)
                GenerarFacturaE(contexto, factura);
            
            string rutaConFichero;
            if (facturaDtm.ClaseDeEmision == enumClaseDeEmision.Impresa)
                rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Fac-{factura.Referencia}.{enumExtensiones.pdf}".NormalizarFichero());
            else
                rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"COPIA-{factura.Referencia}-{factura.ClaseDeEmision.ToString().Replace("eFactura", "")}.{enumExtensiones.pdf}".NormalizarFichero());

            var facturaEmtRpt = (FacturaEmtRpt)new GeneradorDeFacturaEmtRpt(contexto, facturaDtm).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeFacturaEmt(facturaEmtRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
            AsociarArchivoFactura(contexto, fae, idArchivo, original: factura.ClaseDeEmision == enumClaseDeEmision.Impresa);
        }

        private static void GenerarFacturaE(ContextoSe contexto, FacturaEmtDto factura)
        {
            var facturaDtm = contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
            var nombrePropuesto = facturaDtm.ProponerNombreDeArchivo(contexto, $"Fac-{factura.Referencia}.xml");
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            new GeneradorDeFacturaEmtXml(contexto, facturaDtm, rutaConFichero).Generar();
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero);
            var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
            AsociarArchivoFactura(contexto, fae, idArchivo, original: true);
        }

        public void GenerarUbl(Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(ltrParametrosEp.ids)) GestorDeErrores.Emitir("No se ha indicado la factura de la que generar el Ubl");
            var idFacturasUbl = (List<int>)parametros[ltrParametrosEp.ids];
            if (idFacturasUbl.Count != 1) GestorDeErrores.Emitir("Debe indicar una única factura de la que generar el Ubl");
            var idFactura = idFacturasUbl[0];
            var trans = Contexto.IniciarTransaccion();
            try
            {
                var facturaDtm = Contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
                GenerarUbl(Contexto, facturaDtm);
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }

            GestorDeErrores.Emitir($"Fichero Uml generado", GestorDeErrores.enumCodigoDeError.MensajeInformativo);
        }


        private static void GenerarUbl(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var facturaDtm = contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
            var usarPepplo = factura.Cliente(contexto).UsarPeppol();
            var nombrePropuesto = facturaDtm.ProponerNombreDeArchivo(contexto, $"Ubl{(usarPepplo ? "21" : "25")}-{factura.Referencia}.xml");
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);

            GeneradorDeFacturaUbl gen = usarPepplo
                        ? new GeneradorDeFacturaUbl21(contexto, factura, rutaConFichero)
                        : new GeneradorDeFacturaUbl25(contexto, factura, rutaConFichero);

            gen.Generar();
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero);
            var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
            AsociarArchivoFactura(contexto, fae, idArchivo, original: false);
        }

        public static void GenerarFacturaE32(ContextoSe contexto, int idFactura)
        {
            var facturaDtm = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            var nombrePropuesto = facturaDtm.ProponerNombreDeArchivo(contexto, $"Fac-{facturaDtm.Referencia}.xml");
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            new GeneradorDeFacturaEmtXml(contexto, facturaDtm, rutaConFichero).Generar();
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(facturaDtm.Id);
            AsociarArchivoFactura(contexto, fae, idArchivo, original: true);
        }

        private static void AsociarArchivoFactura(ContextoSe contexto, FacturaEmtDtm factura, int idArchivo, bool original)
        {

            if (original)
            {
                factura.IdArchivo = idArchivo;
                factura = factura.Modificar(contexto, esUnaAccion: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaFacturaEmt.Accion_AsociarArchivo } });
            }
            GestorDeVinculos.Vincular(contexto, enumNegocio.FacturaEmitida, enumNegocio.Archivos, factura.Id, idArchivo);

            var certificadosDeUnaSociedad = GestorDeVinculos.RegistrosVinculados<CertificadoDtm>(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, factura.Cg(contexto).IdSociedad);
            if (certificadosDeUnaSociedad.Count == 1)
            {
                var password = ApiDeCertificados.LeerPasswordDeCertificado(contexto, certificadosDeUnaSociedad[0].Id);
                try
                {
                    GestorDeArchivos.Gestor(contexto, contexto.Mapeador).FirmarAnexado(enumNegocio.FacturaEmitida, factura.Id, idArchivo, certificadosDeUnaSociedad[0].Id, password,
                            new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }
                catch (Exception exc)
                {
                    factura.CrearTraza(contexto, "No se ha podido firmar la factura", $"Se ha producido un error al firmar la factura:{Environment.NewLine}{exc.Message}");
                }
            }
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idFactura = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var factura = entorno.Contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            if (vinculado == enumNegocio.Archivador)
            {
                var idArchivador = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                var archivador = entorno.Contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
                if (factura.IdArchivadorParaLaReclamacion.Entero() == idArchivador)
                    GestorDeErrores.Emitir($"No puede quitar de la {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}' el {enumNegocio.Archivador.Singular(true)} '{archivador.Referencia}'");
            }
            if (vinculado == enumNegocio.Archivos)
            {
                if (factura.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
                {
                    var archivo = entorno.Contexto.SeleccionarPorId<ArchivoDtm>(factura.IdArchivo.Entero());
                    var firmado = entorno.Contexto.Set<FirmadoDtm>().Where(x => x.IdOriginal == (int)factura.IdArchivo).FirstOrDefault();
                    if (firmado != null)
                    {
                        if (firmado.IdOriginal == factura.IdArchivo)
                            GestorDeErrores.Emitir($"No puede quitar de la {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el original de la emisión");
                    }
                    else
                        GestorDeErrores.Emitir($"No puede quitar de la {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el original de la emisión");
                }
            }
        }

        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idFactura = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var factura = entorno.Contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            if (vinculado == enumNegocio.CircuitoDoc)
            {
                var idCircuito = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                var circuito = entorno.Contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
                if (circuito.VinculadosAl(entorno.Contexto).Count == 0 && circuito.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto))
                    circuito.TransitarALaEtapa(entorno.Contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.EstadosDeLaEtapa());
            }
        }

        public static bool EstaEnEtapaDeConsulta(ContextoSe contexto, FacturaEmtDtm factura)
        {
            return !factura.Etapas().Any(x => x == enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura);
        }

        public static int CrearRectificativa(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(HacerRectificativaDto.IdElemento))) GestorDeErrores.Emitir("No se ha indicado la factura a rectificar");
            if (!parametros.ContieneClave(nameof(HacerRectificativaDto.ClaseRectificativa))) GestorDeErrores.Emitir("Debe indicar la clase de rectificativa");
            if (!parametros.ContieneClave(nameof(HacerRectificativaDto.Motivo))) GestorDeErrores.Emitir("Debe indicar el motivo de rectificación");
            parametros[ltrParametrosNeg.Copiando] = true;
            var aRectificar = contexto.SeleccionarPorId<FacturaEmtDtm>((int)(long)parametros[nameof(HacerRectificativaDto.IdElemento)]);
            var rectificativa = aRectificar.CrearRectificativa(contexto, parametros);
            return rectificativa.Id;
        }

        public async Task<TotalesDeFacturasEmt> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return await Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDeFacturasEmt ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.FiltroPorEtapa.ToLower() || x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.InvariantCultureIgnoreCase));
            if (filtro == null)
                filtros.Add(new ClausulaDeFiltrado
                {
                    Clausula = ltrFiltros.FiltroPorEtapa,
                    Criterio = enumCriteriosDeFiltrado.noEsNingunoDe,
                    Valor = enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.ToString()
                }
            );

            var facturas = Contexto.SeleccionarTodos<FacturaEmtDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales},
            });

            DatosParaElMapeo(facturas, new Dictionary<string, object> { });

            var totales = new TotalesDeFacturasEmt();
            totales.Bi = facturas.Sum(x => x.Bi(Contexto));
            totales.Iva = facturas.Sum(x => x.TotalDeIva(Contexto));
            totales.Irpf = facturas.Sum(x => x.TotalDeIrpf(Contexto));
            totales.APagar = facturas.Sum(x => x.APagar(Contexto));
            totales.PendienteDeCobro = facturas.Sum(x => x.PendientePorCobrar(Contexto));
            totales.CobrosParciales = facturas.Where(x => x.EstaParcialmenteCobrada(Contexto)).Sum(x => x.Cobrado(Contexto));
            totales.CobrosTotales = facturas.Where(x => x.EstaCobrada(Contexto)).Sum(x => x.Cobrado(Contexto));
            totales.Cobrado = facturas.Sum(x => x.Cobrado(Contexto));
            totales.PorAbonar = facturas.Sum(x => x.PendientePorAbonar(Contexto));
            totales.Abonado = facturas.Sum(x => x.Abonado(Contexto));


            totales.Procesados = facturas.Count();
            return totales;
        }

        public void GenerarPreasiento(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(id);
                    var sociedad = factura.Sociedad(Contexto);

                    if (!sociedad.UsaPreasientos(Contexto, enumNegocio.FacturaEmitida))
                        GestorDeErrores.Emitir($"La sociedad de la factura '{factura.Referencia}' no usa preasientos, configúrela en '{enumParametrosDePreasiento.SPR_Generar_Preasiento_De_FacturaEmitida}'");

                    if (!factura.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir($"Ha de ser administrador de la factura '{factura.Referencia}' para poder preasentarla");

                    if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada }))
                        GestorDeErrores.Emitir($"No se puede generar un preasiento de  '{factura.Referencia}' por no estar en la etapa correcta");

                    if (sociedad.Autonomo)
                    {
                        var circuitos = factura.Vinculados<CircuitoDocDtm>(Contexto);
                        var estados = enumNegocio.CircuitoDoc.Estados(Contexto);
                        var contabilizado = circuitos.FirstOrDefault(x => estados.Any(e => e.Terminado && e.Id == x.IdEstado));
                        if (contabilizado != null)
                            GestorDeErrores.Emitir($"No se puede generar un preasiento de  '{factura.Referencia}' por estar en el lote '{contabilizado.Referencia}', ya contabilizado, anule su contabilización", GestorDeErrores.enumCodigoDeError.MensajeInformativo);

                        var pendiente = circuitos.FirstOrDefault(x => estados.Any(e => !e.Terminado && !e.Cancelado && e.Id == x.IdEstado));
                        if (pendiente != null)
                        {
                            factura.Desvincular(Contexto, pendiente);
                            Contexto.Commit(trans);
                            GestorDeErrores.Emitir($"Se ha excluido la factura '{factura.Referencia}' del lote '{pendiente.Referencia}', vuelva a solicitar su contabilización", GestorDeErrores.enumCodigoDeError.MensajeInformativo);
                        }
                        EstimacionDirecta(factura);
                    }
                    else
                    {
                        var preasientoAnterior = factura.Preasiento(Contexto, errorSiNoHay: false);
                        var cancelado = preasientoAnterior?.Estado(Contexto).Cancelado ?? false;
                        if (factura.IdPreasiento is not null && !cancelado)
                        {
                            factura.CancelarPreasiento(Contexto);
                            //var cancelados = enumNegocio.Preasiento.Estados(Contexto).Where(e => e.Cancelado).Select(e => e.Id).ToList();
                            //foreach (var cobro in factura.Detalles<CobroDeFaeDtm>(Contexto))
                            //{
                            //    if (cobro.IdPreasiento is null)
                            //    {
                            //        var preasiento = Contexto.Set<PreasientoDtm>().FirstOrDefault(x => x.NegocioReferenciado == enumNegocio.Cobro && x.IdReferenciado == cobro.Id && !cancelados.Contains(x.IdEstado));
                            //        cobro.IdPreasiento = preasiento?.Id ?? null;
                            //        cobro.Preasiento = preasiento ?? null;
                            //    }
                            //    cobro.CancelarPreasiento(Contexto);
                            //}
                        }
                        factura.Preasentar(Contexto);
                        factura = factura.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: nameof(GenerarPreasiento));
                        var gestor = GestorDeCobrosDeFae.Gestor(Contexto, Mapeador);
                        foreach (var cobro in factura.Detalles<CobroDeFaeDtm>(Contexto))
                        {
                            gestor.GenerarPreasiento(new List<int> { cobro.Id });
                            //cobro.Preasentar(Contexto);
                            //cobro.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: nameof(GenerarPreasiento));
                        }
                        if (preasientoAnterior is not null)
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
            GestorDeErrores.Emitir($"Lote de contabilización o preasientos creados para las facturas indicadas", GestorDeErrores.enumCodigoDeError.MensajeInformativo);
        }


        private void EstimacionDirecta(FacturaEmtDtm emitida)
        {
            var sociedad = emitida.Sociedad(Contexto);
            var comoContabilizar = sociedad.ContabilizarEn();
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoContabilizar.Metodo);
            if (metodo != null)
            {
                var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(ltrDeUnPreasiento.IdFacturaEmitida), enumCriteriosDeFiltrado.igual, emitida.Id) };
                var filtrosJson = JsonConvert.SerializeObject(filtros);
                var parametros = new Dictionary<string, object>
                {
                    {ltrFiltros.filtro, filtrosJson},
                    {nameof(SociedadDtm.Id), sociedad.Id},
                    {nameof(PreasientoDtm.FechaContable), null }
                };
                var contabilizar = new EntornoDeUnaAccion(Contexto, enumNegocio.FacturaEmitida, parametros);

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

        public string ValidarFactura(string nif, string numeroFactura, string fecha, string importe)
        {
            if (!ApiDeTerceros.NifCifValido(nif))
                throw new Exception($"El nif de la factura '{nif}' no es válido");

            var sociedad = Contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif, errorSiNoHay: false);
            if (sociedad == null)
                throw new Exception($"El nif '{nif}' no está en la BD");

            var factura = sociedad.LeerFacturaEmt(Contexto, numeroFactura);

            var importeBd = factura.APagar(Contexto).Formatear(alineacion: false, separadorDecimal: Simbolos.Punto);
            if (importeBd != importe)
                throw new Exception($"La factura '{numeroFactura}' tiene un importe '{importeBd}' y es diferente al indicado '{importe}'");

            var fechaBd = factura.FacturadaEl.Fecha().ToString("dd-MM-yyyy");
            if (fechaBd.Fecha() != fecha.Fecha())
                throw new Exception($"La factura '{numeroFactura}' se emitió el '{fechaBd}' y es diferente a la indicada '{fecha}'");

            if (factura.Firma == null)
                return $"La factura '{numeroFactura}' no fue firmada pero sus datos corresponden con los del sistema";

            if (!factura.ValidarFirma(Contexto))
                throw new Exception($"La firma de la factura '{numeroFactura}' no corresponde con la de BD");

            return Convert.ToBase64String(factura.Firma);
        }

        public void SincronizarConDatosDeLaAeat(ContextoSe contexto, int idFactura)
        {
            if (!Contexto.SePuedeParametrizar())
                GestorDeErrores.Emitir("El usuario ha de ser parametrizadaro para ejecutar la sincronización con la Aeat");

            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada }))
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' debe estar emitida");

            var tran = contexto.IniciarTransaccion();
            try
            {
                new GeneradorSii(contexto, factura).SincronizarConDatosDeLaAeat();
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

    }

}
