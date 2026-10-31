using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Logistica;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Negocio;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeMovimientosDeAlmacen : GestorDeElementos<ContextoSe, MovimientoDeAlmacenDtm, MovimientoDeAlmacenDto>, IGeneradorDePreasiento
    {

        public class MapearMovimientosDeAlmacen : Profile
        {
            public MapearMovimientosDeAlmacen()
            {
                CreateMap<MovimientoDeAlmacenDtm, MovimientoDeAlmacenDto>()
                .ForMember(dto => dto.TipoMovimiento, x => x.MapFrom(dtm => dtm.TipoMovimiento.Nombre))
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion));

                CreateMap<MovimientoDeAlmacenDto, MovimientoDeAlmacenDtm>()
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoMovimiento, dto => dto.Ignore())
                .ForMember(dtm => dtm.Preasiento, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public GestorDeMovimientosDeAlmacen(ContextoSe contexto, IMapper mapeador) : base(contexto, mapeador)
        {
        }

        public static GestorDeMovimientosDeAlmacen Gestor(ContextoSe contexto, IMapper mapeador) => new GestorDeMovimientosDeAlmacen(contexto, mapeador);

        // Regenera el preasiento de los movimientos (ver GestorDePreasientos al regenerar): cancela el actual y lo vuelve a generar sin
        // recalcular el inventario ni barrer, por eso se guarda con EstoyBarriendo; el preasiento nuevo lo engancha Preasentar
        public void GenerarPreasiento(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var movimiento = Contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(id, usarLaCache: false);

                    if (!movimiento.Almacen(Contexto).Sociedad(Contexto).UsaPreasientos(Contexto, enumNegocio.Almacen))
                        GestorDeErrores.Emitir($"La sociedad del movimiento de almacén '{movimiento.Referencia(Contexto)}' no usa preasientos de almacén, configúrela en '{enumParametrosDePreasiento.SPR_Generar_Preasiento_De_Almacen}'");

                    movimiento.CancelarPreasiento(Contexto);
                    movimiento.Preasentar(Contexto);
                    movimiento.ModificarComoAdministrador(Contexto, parametros: new Dictionary<string, object> { { ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, true } });
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarJoins(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Almacen);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.TipoMovimiento);
            return consulta;
        }

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarFiltros(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorAlmacen(filtros);
            consulta = consulta.FiltroPorTipoMovimiento(filtros);
            consulta = consulta.FiltroPorUnitario(filtros);
            consulta = consulta.FiltroPorRealizadoEl(filtros);
            return consulta;
        }

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarOrden(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
            => base.AplicarOrden(consulta, ordenacion);

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarSeguridad(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
            => base.AplicarSeguridad(consulta, filtros, parametros);

        protected override void DespuesDeMapearElRegistro(MovimientoDeAlmacenDto elemento, MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, movimiento, opciones);
            movimiento.Referencia(Contexto);
        }

        protected override void DespuesDeLeer(List<MovimientoDeAlmacenDtm> registros, ParametrosDeNegocio parametros)
        {
            base.DespuesDeLeer(registros, parametros);
            foreach (var movimiento in registros)
                movimiento.Referencia(Contexto);
        }

        public override MovimientoDeAlmacenDtm PersistirRegistro(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            // los movimientos que se insertan o barren desde los extensores no pasan por el mapeo; sin referencia no se valida el registro
            movimiento.Referencia(Contexto);

            var barriendo = parametros.Parametros.LeerValor(ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, false);
            var idSemaforo = !barriendo ? PonerSemaforo(movimiento) : 0;
            try
            {
                movimiento = base.PersistirRegistro(movimiento, parametros);
            }
            finally
            {
                if (!barriendo)
                    SemaforoDeProcesoSql.QuitarSemaforo(Contexto, idSemaforo);
            }
            return movimiento;
        }

        private int PonerSemaforo(MovimientoDeAlmacenDtm movimiento)
        {
            int idSemaforo;
            var almacenABloquear = movimiento.Almacen(Contexto);

            var etapaNecesaria = movimiento.TipoDeMovimiento(Contexto).ClaseMovimiento == enumClaseDeMovimiento.Inicial
            ? enumEtapasDeAlmacen.ALM_Etapa_En_Inventario
            : enumEtapasDeAlmacen.ALM_Etapa_Activo;

            if (!almacenABloquear.EstaEnLaEtapa(etapaNecesaria))
                GestorDeErrores.Emitir($"El almacén '{almacenABloquear.Referencia}' no está en la etapa '{etapaNecesaria.Nombre()}', no se pueden generar movimientos de '{movimiento.TipoDeMovimiento(Contexto).ClaseMovimiento.Descripcion()}'");

            idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(Contexto, enumNegocio.Almacen.IdNegocio(), almacenABloquear.Id, enumOpercionesDeSemaforo.PMA, almacenABloquear.Referencia + ":" + movimiento.Unitario(Contexto).Referencia).Id;
            return idSemaforo;
        }

        protected override void AntesDePersistir(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(movimiento, parametros);

            if (parametros.Parametros.LeerValor(ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, false))
                return;

            // sólo se borran al cancelar la entrega de una recepción en un almacén; los posteriores se barren en DespuesDePersistir
            if (parametros.Eliminando)
            {
                if (parametros.AccionQueSeEjecuta != ltrDeUnMovimientoDeAlmacen.Accion_AnularEntradaDeCompra
                    && parametros.AccionQueSeEjecuta != ltrDeUnMovimientoDeAlmacen.Accion_AnularDevolucionDeCompra)
                    GestorDeErrores.Emitir("No se pueden eliminar movimientos de almacén");
                movimiento.CancelarPreasientoDeRecepcion(Contexto);
                return;
            }

            movimiento.CalcularTotalesDelMovimiento(Contexto);
        }

        protected override void DespuesDePersistir(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(movimiento, parametros);

            if (parametros.Parametros.LeerValor(ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, false))
                return;

            if (parametros.Insertando)
            {
                var preasiento = movimiento.Preasiento(Contexto);
                if (preasiento is not null && preasiento.IdReferenciado != movimiento.Id)
                {
                    preasiento.IdReferenciado = movimiento.Id;
                    preasiento.ModificarComoAdministrador(Contexto);
                }
            }

            movimiento.Barrer(Contexto);
        }

        protected override void EliminarCaches(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(movimiento, parametros);
            if (movimiento.IdLineaDeRecepcion != null) ServicioDeCaches.EliminarElemento(CacheDe.Alm_Movimiento_Recepcion, movimiento.Id.ToString());
        }

        protected override void DespuesDeMapearElElemento(MovimientoDeAlmacenDtm movimiento, MovimientoDeAlmacenDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(movimiento, elemento, parametros);

            var claseDeMovimiento = movimiento.TipoDeMovimiento(Contexto).ClaseMovimiento;
            elemento.Proveedor = movimiento.Proveedor(Contexto, emitirErrorSiNoEsRecepcion: false)?.Referencia(Contexto) ?? string.Empty;
            elemento.Preasiento = movimiento.Preasiento(Contexto)?.Referencia ?? string.Empty;

            // el origen es la operación que generó el movimiento, sea entrada o salida; al pulsarlo se navega a su edición
            // (ver ExtensorDeAlmacenes.UrlDelDocumento)
            elemento.Origen = ObtenerDocumentoDeOrigenODestino(movimiento, claseDeMovimiento);
        }

        private string ObtenerDocumentoDeOrigenODestino(MovimientoDeAlmacenDtm movimiento, enumClaseDeMovimiento claseDeMovimiento)
        {
            if (movimiento.IdMovimiento != null) return $"Movimiento de obra Nº {movimiento.IdMovimiento}";
            // la devolución de compra también lleva la entrega devuelta (IdLineaDeRecepcion), por eso se mira antes
            if (movimiento.IdLineaDeRecepcion != null)
            {
                var linea = ExtensorDeRecepciones.LineaDePedido(Contexto, (int)movimiento.IdLineaDeRecepcion);
                var pedido = linea.DetalleDe<PedidoDtm>(Contexto);
                return movimiento.IdLineaDevolucion != null
                    ? $"Devolución de la línea nº {linea.Orden} del pedido '{pedido.Referencia}'"
                    : $"Recepción de la línea nº {linea.Orden} del pedido '{pedido.Referencia}'";
            }
            if (movimiento.IdLineaDevolucion != null) return $"Devolución Nº {movimiento.IdLineaDevolucion}";
            if (movimiento.IdLineaInventario != null)
            {
                var linea = Contexto.SeleccionarPorId<LineasDeUnaRegularizacionDtm>((int)movimiento.IdLineaInventario);
                var regularizacion = linea.DetalleDe<RegularizacionDtm>(Contexto);
                return $"{claseDeMovimiento.Descripcion()} '{regularizacion.Referencia}', línea nº {linea.Orden}";
            }
            return string.Empty;
        }
    }
}
