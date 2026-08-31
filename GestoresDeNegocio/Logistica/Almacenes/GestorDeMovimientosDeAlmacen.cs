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
using ServicioDeDatos.MaestrosTecnico;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeMovimientosDeAlmacen : GestorDeElementos<ContextoSe, MovimientoDeAlmacenDtm, MovimientoDeAlmacenDto>
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
            // TODO: completar los filtros propios (por almacén, por unitario, por rango de fechas, etc.)
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            return consulta;
        }

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarOrden(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
            => base.AplicarOrden(consulta, ordenacion);

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarSeguridad(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
            => base.AplicarSeguridad(consulta, filtros, parametros);

        public override MovimientoDeAlmacenDtm PersistirRegistro(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            var barriendo = parametros.Parametros.LeerValor(ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, false);

            var idSemaforo = 0;
            if (!barriendo)
            {
                if (parametros.Eliminando)
                    GestorDeErrores.Emitir("No se pueden eliminar movimientos de almacén");

                var almacenABloquear = Contexto.SeleccionarPorId<AlmacenDtm>(movimiento.IdAlmacen);
                if (!almacenABloquear.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo))
                    GestorDeErrores.Emitir($"El almacén '{almacenABloquear.Referencia}' no está activo, no se pueden generar movimientos");

                var untario = Contexto.SeleccionarPorId<UnitarioDtm>(movimiento.IdUnitario);
                idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(Contexto, enumNegocio.Almacen.IdNegocio(), almacenABloquear.Id, enumOpercionesDeSemaforo.PMA, almacenABloquear.Referencia + ":" + untario.Referencia).Id;
            }
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

        protected override void AntesDePersistir(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(movimiento, parametros);

            if (parametros.Parametros.LeerValor(ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, false))
                return;

            if (parametros.Modificando && movimiento.IdPreasiento is not null)
                movimiento.QuitarPreasiento(Contexto);

            movimiento.CalcularInventarioDelMovimiento(Contexto);
            movimiento.Preasentar(Contexto);
        }

        protected override void DespuesDePersistir(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(movimiento, parametros);

            if (parametros.Parametros.LeerValor(ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, false))
                return;

            if (movimiento.IdPreasiento is not null && parametros.Insertando)
            {
                var preasiento = movimiento?.Preasiento ?? Contexto.SeleccionarPorId<PreasientoDtm>(movimiento.IdPreasiento.Value);
                preasiento.IdReferenciado = movimiento.Id;
                preasiento.ModificarComoAdministrador(Contexto);
            }

            foreach (var posterior in movimiento.ObtenerMovimientosPosteriores(Contexto))
            {
                posterior.Barrer(Contexto);
                posterior.ModificarComoAdministrador(Contexto, parametros: new Dictionary<string, object> { { ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, true } });
            }
        }

        protected override void DespuesDeMapearElElemento(MovimientoDeAlmacenDtm movimiento, MovimientoDeAlmacenDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(movimiento, elemento, parametros);

            var almacen = movimiento.Almacen?.Expresion;
            var documento = ObtenerDocumentoDeOrigenODestino(movimiento);

            switch (movimiento.TipoMovimiento?.ClaseMovimiento)
            {
                case enumClaseDeMovimiento.Entrada:
                    elemento.Origen = documento;
                    elemento.Destino = almacen;
                    break;
                case enumClaseDeMovimiento.Salida:
                    elemento.Origen = almacen;
                    elemento.Destino = documento;
                    break;
                default:
                    elemento.Origen = almacen;
                    elemento.Destino = almacen;
                    break;
            }
        }

        // TODO: sustituir por la referencia real en cuanto existan los Dtm de MovimientoDeObra/LineaDeAlbaran/LineaDeDevolucion/LineaDeInventario
        private static string ObtenerDocumentoDeOrigenODestino(MovimientoDeAlmacenDtm movimiento)
        {
            if (movimiento.IdMovimiento != null) return $"Movimiento de obra Nº {movimiento.IdMovimiento}";
            if (movimiento.IdLineaAlbaran != null) return $"Albarán Nº {movimiento.IdLineaAlbaran}";
            if (movimiento.IdLineaDevolucion != null) return $"Devolución Nº {movimiento.IdLineaDevolucion}";
            if (movimiento.IdLineaInventario != null) return $"Inventario Nº {movimiento.IdLineaInventario}";
            return string.Empty;
        }
    }
}
