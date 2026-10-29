using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    // Los cierres contables solo se persisten desde la cola de trabajos sometidos (trabajos de crear y anular lote contable)
    public class GestorDeCierresContables : GestorDeElementos<ContextoSe, CierreContableDtm, CierreContableDto>
    {
        public class MapearCierreContable : Profile
        {
            public MapearCierreContable()
            {
                CreateMap<CierreContableDtm, CierreContableDto>()
                .ForMember(dto => dto.Sociedad, x => x.MapFrom(dtm => dtm.Sociedad.Expresion))
                .ForMember(dto => dto.Lote, x => x.MapFrom(dtm => dtm.Lote.Expresion))
                .ForMember(dto => dto.Recibidas, x => x.Ignore())
                .ForMember(dto => dto.Cobros, x => x.Ignore())
                .ForMember(dto => dto.Pagos, x => x.Ignore())
                .ForMember(dto => dto.Emitidas, x => x.Ignore())
                .ForMember(dto => dto.Almacen, x => x.Ignore())
                .ForMember(dto => dto.EsElUltimoCierre, x => x.Ignore())
                .ForMember(dto => dto.Desde, x => x.Ignore())
                .ForMember(dto => dto.CerradoEl, x => x.MapFrom(dtm => dtm.FechaCreacion));
                CreateMap<CierreContableDto, CierreContableDtm>()
                .ForMember(dtm => dtm.Sociedad, dto => dto.Ignore())
                .ForMember(dtm => dtm.Lote, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.CierreContable;

        public GestorDeCierresContables(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeCierresContables Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCierresContables(contexto, mapeador);
        }

        // Tras crear el lote contable de la sociedad, deja constancia del cierre con la fecha hasta la que se ha cerrado
        public static CierreContableDtm RegistrarCierre(ContextoSe contexto, SociedadDtm sociedad, CircuitoDocDtm lote, DateTime cerradaEl)
        {
            var cierre = new CierreContableDtm
            {
                Nombre = $"Cierre de {sociedad.RazonSocial} ({lote.Referencia})".Left(250),
                IdSociedad = sociedad.Id,
                IdLote = lote.Id,
                CerradaEl = cerradaEl
            };
            return Gestor(contexto, contexto.Mapeador).PersistirRegistro(cierre, new ParametrosDeNegocio(enumTipoOperacion.Insertar, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } }));
        }

        // Tras anular el lote contable, el cierre asociado deja de existir
        public static void EliminarCierre(ContextoSe contexto, CircuitoDocDtm lote)
        {
            var cierre = contexto.Set<CierreContableDtm>().FirstOrDefault(c => c.IdLote == lote.Id);
            if (cierre is null)
                return;

            Gestor(contexto, contexto.Mapeador).PersistirRegistro(cierre, new ParametrosDeNegocio(enumTipoOperacion.Eliminar, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } }));
        }

        public static CierreContableDtm UltimoCierre(ContextoSe contexto, int idSociedad) => ExtensorDeSociedades.UltimoCierreContable(contexto, idSociedad);

        private static IQueryable<PreasientoDtm> PreasientosPendientes(ContextoSe contexto, int idSociedad)
        {
            var pendientes = enumEtapasDePreasiento.SPR_Etapa_Pendiente.Lista();
            return contexto.Set<PreasientoDtm>().Where(p => p.Cg.IdSociedad == idSociedad && pendientes.Contains(p.IdEstado));
        }

        // Fecha hasta la que está cerrada la sociedad: la del último cierre o, si no se ha cerrado nunca, el 1 de enero del año en curso
        public static DateTime FechaDelUltimoCierre(ContextoSe contexto, int idSociedad) =>
            UltimoCierre(contexto, idSociedad)?.CerradaEl.Date ?? new DateTime(DateTime.Now.Year, 1, 1);

        // Se puede cerrar hasta una fecha no posterior a hoy y posterior a la del último cierre; devuelve el motivo si no se cumple, o null
        public static string MotivoFechaDeCierreNoValida(ContextoSe contexto, SociedadDtm sociedad, DateTime cerrarHasta)
        {
            if (cerrarHasta.Date > DateTime.Today)
                return $"No se puede cerrar la sociedad '{sociedad.Expresion}' hasta el '{cerrarHasta:dd/MM/yyyy}' por ser posterior al día de hoy";

            var ultimoCierre = FechaDelUltimoCierre(contexto, sociedad.Id);
            if (cerrarHasta.Date <= ultimoCierre)
                return $"No se puede cerrar la sociedad '{sociedad.Expresion}' hasta el '{cerrarHasta:dd/MM/yyyy}', ha de ser posterior a la fecha de su último cierre '{ultimoCierre:dd/MM/yyyy}'";

            return null;
        }

        // Fecha desde la que cerró un cierre: el 'Hasta' del cierre anterior de la sociedad o, si es el primero, el 1 de enero de su año
        public static DateTime FechaDesdeDelCierre(ContextoSe contexto, CierreContableDtm cierre)
        {
            var anterior = contexto.Set<CierreContableDtm>()
                .Where(c => c.IdSociedad == cierre.IdSociedad && c.Id != cierre.Id &&
                            (c.CerradaEl < cierre.CerradaEl || (c.CerradaEl == cierre.CerradaEl && c.Id < cierre.Id)))
                .OrderByDescending(c => c.CerradaEl).ThenByDescending(c => c.Id)
                .FirstOrDefault();

            return anterior?.CerradaEl.Date ?? new DateTime(cierre.CerradaEl.Year, 1, 1);
        }

        // El cierre traspasa los preasientos pendientes del ejercicio con fecha contable desde el último cierre (incluida) hasta la fecha
        // de cierre (no incluida); devuelve el motivo si no hay ninguno, o null si los hay
        public static string MotivoSinPreasientosParaTraspasar(ContextoSe contexto, SociedadDtm sociedad, DateTime cerrarHasta)
        {
            var ejercicio = cerrarHasta.Year;
            var desde = FechaDelUltimoCierre(contexto, sociedad.Id);
            var hasta = cerrarHasta.Date;
            if (PreasientosPendientes(contexto, sociedad.Id).Any(p => p.Ejercicio == ejercicio && p.FechaContable >= desde && p.FechaContable < hasta))
                return null;

            return $"No hay preasientos contables de la sociedad '{sociedad.Expresion}' para traspasar en el ejercicio '{ejercicio}' con fecha contable desde el '{desde:dd/MM/yyyy}' y anterior al '{hasta:dd/MM/yyyy}'";
        }

        // No se puede cerrar un ejercicio si quedan preasientos pendientes con fecha contable de ejercicios anteriores
        public static void ValidarEjercicioDelCierre(ContextoSe contexto, SociedadDtm sociedad, int ejercicio)
        {
            var anteriores = PreasientosPendientes(contexto, sociedad.Id).Where(p => p.FechaContable.Year < ejercicio);
            if (!anteriores.Any())
                return;

            var ejercicioPendiente = anteriores.Min(p => p.FechaContable.Year);
            GestorDeErrores.Emitir($"La sociedad '{sociedad.Expresion}' tiene preasientos pendientes con fecha contable anterior al ejercicio '{ejercicio}', " +
                                   $"se han de dar por contabilizados los asientos anteriores al ejercicio '{ejercicio}' o cerrar el ejercicio del año '{ejercicioPendiente}'");
        }

        // No se puede contabilizar con fecha anterior al último cierre: los preasientos pendientes anteriores pasan a la fecha del último cierre
        public static void AjustarPreasientosAnterioresAlUltimoCierre(ContextoSe contexto, int idSociedad, Action<string> anotar)
        {
            var ultimoCierre = UltimoCierre(contexto, idSociedad);
            if (ultimoCierre is null)
                return;

            var fechaDelCierre = ultimoCierre.CerradaEl.Date;
            var anteriores = PreasientosPendientes(contexto, idSociedad).Where(p => p.FechaContable < fechaDelCierre).ToList();
            foreach (var preasiento in anteriores)
            {
                ValidarQueNoEsDeUnaFacturaEmitida(preasiento, fechaDelCierre);
                anotar($"El preasiento '{preasiento.Referencia}' tenía fecha contable '{preasiento.FechaContable:dd/MM/yyyy}' anterior al último cierre, se contabiliza con fecha '{fechaDelCierre:dd/MM/yyyy}'");
                preasiento.FechaContable = fechaDelCierre;
                preasiento.Ejercicio = fechaDelCierre.Year;
                preasiento.ModificarComoAdministrador(contexto, esUnaAccion: true);
            }
        }

        // Al crear un preasiento, si la fecha del documento es anterior al último cierre de su sociedad se contabiliza con la fecha de ese
        // cierre (y su ejercicio), ya que lo anterior ya está cerrado; devuelve la fecha original si se ha ajustado, o null si no.
        // Las facturas emitidas no se ajustan: dan error (ver ValidarQueNoEsDeUnaFacturaEmitida)
        public static DateTime? AjustarFechaContableAlUltimoCierre(ContextoSe contexto, PreasientoDtm preasiento)
        {
            var idSociedad = contexto.Set<CentroGestorDtm>().Where(cg => cg.Id == preasiento.IdCg).Select(cg => cg.IdSociedad).FirstOrDefault();
            var ultimoCierre = UltimoCierre(contexto, idSociedad);
            if (ultimoCierre is null || preasiento.FechaContable.Date >= ultimoCierre.CerradaEl.Date)
                return null;

            ValidarQueNoEsDeUnaFacturaEmitida(preasiento, ultimoCierre.CerradaEl.Date);
            var fechaDelDocumento = preasiento.FechaContable;
            preasiento.FechaContable = ultimoCierre.CerradaEl.Date;
            preasiento.Ejercicio = preasiento.FechaContable.Year;
            return fechaDelDocumento;
        }

        // La factura emitida se contabiliza con su fecha de devengo, que es la que se declara en Verifactu como fecha de operación: moverla
        // llevaría el IVA repercutido a otro periodo, así que si es anterior al último cierre no se ajusta, se ha de eliminar el cierre
        private static void ValidarQueNoEsDeUnaFacturaEmitida(PreasientoDtm preasiento, DateTime fechaDelUltimoCierre)
        {
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida)
                GestorDeErrores.Emitir($"No se puede contabilizar la factura emitida '{preasiento.Nombre}' con fecha de devengo '{preasiento.FechaContable:dd/MM/yyyy}' " +
                                       $"porque es anterior al último cierre contable de su sociedad '{fechaDelUltimoCierre:dd/MM/yyyy}'; elimine los cierres posteriores a su fecha de devengo para contabilizarla en su periodo");
        }

        //------------------------------------------------------------------------------------- semáforo del cierre en curso
        private const string FormatoDeFechaDelSemaforo = "yyyy-MM-dd";

        // Mientras se ejecuta el cierre de una sociedad hay un semáforo con la fecha hasta la que se cierra; devuelve su id para quitarlo al terminar
        public static int PonerSemaforoDeCierre(ContextoSe contexto, int idSociedad, DateTime cerrarHasta) =>
            SemaforoDeProcesoSql.PonerSemaforo(contexto, enumNegocio.CierreContable.IdNegocio(), idSociedad, enumOpercionesDeSemaforo.CCT,
                cerrarHasta.ToString(FormatoDeFechaDelSemaforo)).Id;

        // Fecha hasta la que se está cerrando la sociedad en otro proceso; null si no hay ningún cierre en curso (el del propio proceso no cuenta,
        // ya que el cierre también cancela preasientos)
        public static DateTime? CierreEnCursoHasta(ContextoSe contexto, int idSociedad)
        {
            var semaforo = SemaforoDeProcesoSql.LeerSemaforos(contexto, enumNegocio.CierreContable.IdNegocio(), enumOpercionesDeSemaforo.CCT)
                .FirstOrDefault(s => s.IdElemento == idSociedad && s.IdProceso != contexto.IdOperacion);
            if (semaforo is null)
                return null;

            var fecha = semaforo.Anotacion.Right(FormatoDeFechaDelSemaforo.Length);
            return DateTime.ParseExact(fecha, FormatoDeFechaDelSemaforo, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Mientras se cierra una sociedad no se pueden crear, eliminar ni cancelar preasientos suyos con fecha contable anterior a la de cierre
        public static void ValidarQueNoHayCierreEnCurso(ContextoSe contexto, PreasientoDtm preasiento, string accion)
        {
            var idSociedad = contexto.Set<CentroGestorDtm>().Where(cg => cg.Id == preasiento.IdCg).Select(cg => cg.IdSociedad).FirstOrDefault();
            var cerrandoHasta = CierreEnCursoHasta(contexto, idSociedad);
            if (cerrandoHasta is not null && preasiento.FechaContable.Date < ((DateTime)cerrandoHasta).Date)
                GestorDeErrores.Emitir($"No se puede {accion} el preasiento '{preasiento.Referencia ?? preasiento.Nombre}' con fecha contable '{preasiento.FechaContable:dd/MM/yyyy}' " +
                                       $"porque se está ejecutando el cierre contable de su sociedad hasta el '{cerrandoHasta:dd/MM/yyyy}', inténtelo cuando termine");
        }

        // Un preasiento con fecha contable anterior al último cierre de su sociedad está en un lote contable, para el sistema está contabilizado
        public static void ValidarQueNoEstaCerrado(ContextoSe contexto, PreasientoDtm preasiento, string accion)
        {
            var idSociedad = contexto.Set<CentroGestorDtm>().Where(cg => cg.Id == preasiento.IdCg).Select(cg => cg.IdSociedad).FirstOrDefault();
            var ultimoCierre = UltimoCierre(contexto, idSociedad);
            if (ultimoCierre is not null && ultimoCierre.CerradaEl.Date > preasiento.FechaContable.Date)
                GestorDeErrores.Emitir($"No se puede {accion} el preasiento '{preasiento.Referencia ?? preasiento.Nombre}' con fecha contable '{preasiento.FechaContable:dd/MM/yyyy}' " +
                                       $"porque es anterior al último cierre contable de su sociedad '{ultimoCierre.CerradaEl:dd/MM/yyyy}'");
        }

        // Solo se puede anular el último cierre realizado de una sociedad; devuelve el motivo si no se puede, o null si el lote no tiene cierre o es el último
        public static string MotivoParaNoAnularElCierre(ContextoSe contexto, int idLote)
        {
            var cierre = contexto.Set<CierreContableDtm>().Include(c => c.Sociedad).FirstOrDefault(c => c.IdLote == idLote);
            if (cierre is null || !TieneCierresPosteriores(contexto, cierre))
                return null;

            return $"No se puede anular el cierre de la sociedad '{cierre.Sociedad.Expresion}' de fecha '{cierre.CerradaEl:dd/MM/yyyy HH:mm}' por tener cierres posteriores";
        }

        private static bool TieneCierresPosteriores(ContextoSe contexto, CierreContableDtm cierre)
        {
            return contexto.Set<CierreContableDtm>().Any(c => c.IdSociedad == cierre.IdSociedad && c.Id != cierre.Id &&
                   (c.CerradaEl > cierre.CerradaEl || (c.CerradaEl == cierre.CerradaEl && c.Id > cierre.Id)));
        }

        protected override void AntesDePersistir(CierreContableDtm registro, ParametrosDeNegocio parametros)
        {
            if (Contexto.Entorno is null)
                GestorDeErrores.Emitir("Los cierres contables solo se registran desde la cola de trabajos sometidos");

            base.AntesDePersistir(registro, parametros);
        }

        protected override IQueryable<CierreContableDtm> AplicarJoins(IQueryable<CierreContableDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(c => c.Sociedad).Include(c => c.Lote);
            return consulta;
        }

        protected override IQueryable<CierreContableDtm> AplicarFiltros(IQueryable<CierreContableDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorSociedad(filtros);
            consulta = consulta.FiltrarPorFechaDeCierre(filtros);
            consulta = consulta.FiltrarPorLote(filtros);
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(CierreContableDtm registro, CierreContableDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);

            elemento.EsElUltimoCierre = !TieneCierresPosteriores(Contexto, registro);
            elemento.Desde = FechaDesdeDelCierre(Contexto, registro);

            var asientosPorOrigen = Contexto.Set<CircuitoDocDeUnPreasientoDtm>()
                .Where(v => v.idElemento2 == registro.IdLote)
                .Join(Contexto.Set<PreasientoDtm>(), v => v.idElemento1, p => p.Id, (v, p) => p.NegocioReferenciado)
                .GroupBy(negocio => negocio)
                .Select(g => new { Negocio = g.Key, Cantidad = g.Count() })
                .ToList();

            int Cantidad(enumNegocio negocio) => asientosPorOrigen.FirstOrDefault(a => a.Negocio == negocio)?.Cantidad ?? 0;

            elemento.Recibidas = Cantidad(enumNegocio.FacturaRecibida);
            elemento.Cobros = Cantidad(enumNegocio.Cobro);
            elemento.Pagos = Cantidad(enumNegocio.Pago);
            elemento.Emitidas = Cantidad(enumNegocio.FacturaEmitida);
            elemento.Almacen = Cantidad(enumNegocio.Almacen);
        }
    }
}
