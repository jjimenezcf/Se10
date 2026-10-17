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

        public override MovimientoDeAlmacenDtm PersistirRegistro(MovimientoDeAlmacenDtm movimiento, ParametrosDeNegocio parametros)
        {
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
                if (parametros.AccionQueSeEjecuta != ltrDeUnMovimientoDeAlmacen.Accion_AnularEntradaDeCompra)
                    GestorDeErrores.Emitir("No se pueden eliminar movimientos de almacén");
                movimiento.CancelarPreasientoDeRecepcion(Contexto);
                return;
            }

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

            movimiento.BarrerPosteriores(Contexto);
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

        private string ObtenerDocumentoDeOrigenODestino(MovimientoDeAlmacenDtm movimiento)
        {
            if (movimiento.IdMovimiento != null) return $"Movimiento de obra Nº {movimiento.IdMovimiento}";
            if (movimiento.IdLineaDeRecepcion != null)
            {
                var linea = ExtensorDeRecepciones.LeerLineaDePedido(Contexto, (int)movimiento.IdLineaDeRecepcion);
                var pedido = linea.DetalleDe<PedidoDtm>(Contexto);
                return $"Recepción de la línea nº {linea.Orden} del pedido '{pedido.Referencia}'";
            }
            if (movimiento.IdLineaDevolucion != null) return $"Devolución Nº {movimiento.IdLineaDevolucion}";
            if (movimiento.IdLineaInventario != null) return $"Inventario Nº {movimiento.IdLineaInventario}";
            return string.Empty;
        }
    }
}
