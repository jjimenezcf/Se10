using AutoMapper;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Logistica;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    // Consulta de inventario: para un almacén y una fecha devuelve, por cada unitario, su último movimiento hasta el fin de ese día,
    // que es el que tiene el stock y el precio medio (Valor) vigentes a esa fecha. Sólo lee, los movimientos los persiste GestorDeMovimientosDeAlmacen
    public class GestorDeInventario : GestorDeElementos<ContextoSe, MovimientoDeAlmacenDtm, InventarioDto>
    {

        public class MapearInventario : Profile
        {
            public MapearInventario()
            {
                CreateMap<MovimientoDeAlmacenDtm, InventarioDto>()
                .ForMember(dto => dto.Unitario, x => x.Ignore())
                .ForMember(dto => dto.Referencia, x => x.Ignore())
                .ForMember(dto => dto.PrecioMedio, x => x.MapFrom(dtm => dtm.Valor))
                .ForMember(dto => dto.Valor, x => x.MapFrom(dtm => dtm.Stock * dtm.Valor));

                CreateMap<InventarioDto, MovimientoDeAlmacenDtm>()
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoMovimiento, dto => dto.Ignore())
                .ForMember(dtm => dtm.Preasiento, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.No_Definido;

        // fin del día consultado, fijado al filtrar; con él se busca la última compra de cada fila
        private DateTime _hasta = DateTime.Today.AddDays(1);

        public GestorDeInventario(ContextoSe contexto, IMapper mapeador) : base(contexto, mapeador)
        {
        }

        public static GestorDeInventario Gestor(ContextoSe contexto, IMapper mapeador) => new GestorDeInventario(contexto, mapeador);

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarJoins(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            return consulta;
        }

        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarFiltros(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            // sin almacén no hay inventario que mostrar (el grid se carga vacío al entrar, hasta que se elige uno)
            var filtroAlmacen = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnMovimientoDeAlmacen.FiltroPorAlmacen.ToLower());
            if (filtroAlmacen is null)
                return consulta.Where(x => false);

            var idAlmacen = filtroAlmacen.Valor.Entero();
            filtroAlmacen.Aplicado = true;

            // se incluye el día consultado entero; sin fecha, el inventario es el de hoy
            var filtroFecha = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnMovimientoDeAlmacen.FiltroPorFechaDeInventario.ToLower());
            var fecha = filtroFecha?.Valor.ParsearFecha() ?? DateTime.Today;
            if (filtroFecha is not null) filtroFecha.Aplicado = true;
            var hasta = fecha.Date.AddDays(1);
            _hasta = hasta;

            // el último movimiento de cada unitario, con el mismo orden que el cálculo del inventario (ver ExtensorDeAlmacenes.ObtenerMovimientoAnterior)
            var movimientos = Contexto.Set<MovimientoDeAlmacenDtm>();
            consulta = consulta.Where(m => m.IdAlmacen == idAlmacen
                && m.RealizadoEl < hasta
                && m.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                && !movimientos.Any(p => p.IdAlmacen == m.IdAlmacen
                                      && p.IdUnitario == m.IdUnitario
                                      && p.RealizadoEl < hasta
                                      && p.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                                      && (p.RealizadoEl > m.RealizadoEl || (p.RealizadoEl == m.RealizadoEl && p.Id > m.Id))));

            consulta = consulta.FiltroPorUnitario(filtros);
            return consulta;
        }

        // por el nombre del material, salvo que el usuario ordene por otra columna
        protected override IQueryable<MovimientoDeAlmacenDtm> AplicarOrden(IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
            => ordenacion.Count == 0
               ? consulta.OrderBy(x => x.Unitario.Nombre).ThenBy(x => x.Id)
               : base.AplicarOrden(consulta, ordenacion);

        protected override void DespuesDeMapearElElemento(MovimientoDeAlmacenDtm movimiento, InventarioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(movimiento, elemento, parametros);

            var unitario = movimiento.Unitario(Contexto);
            elemento.Unitario = unitario.Nombre;
            elemento.Referencia = unitario.Referencia;
            elemento.Naturaleza = unitario.Naturaleza(Contexto).Expresion;
            elemento.Unidad = unitario.Unidad(Contexto).Expresion;
            elemento.Coste = unitario.Coste;
            elemento.Venta = unitario.Venta;
            elemento.UltimaCompra = UltimaCompra(movimiento);
            elemento.MargenSobreValor = Margen(unitario.Venta, movimiento.Valor);
            elemento.MargenSobreCompra = elemento.UltimaCompra == 0 ? 0 : Margen(unitario.Venta, elemento.UltimaCompra);
        }

        // precio de entrada de la última compra del material en el almacén hasta la fecha consultada; 0 si no se ha comprado
        private decimal UltimaCompra(MovimientoDeAlmacenDtm movimiento)
        {
            var idCompra = enumTiposDeMovimiento.Compra.LeerTipoDeMovimiento(Contexto).Id;
            return Contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(p => p.IdAlmacen == movimiento.IdAlmacen && p.IdUnitario == movimiento.IdUnitario && p.IdTipoMovimiento == idCompra && p.RealizadoEl < _hasta)
                .OrderByDescending(p => p.RealizadoEl).ThenByDescending(p => p.Id)
                .Select(p => (decimal?)p.Precio)
                .FirstOrDefault() ?? 0;
        }

        // (venta - precio) / venta, en porcentaje; 0 si no hay precio de venta
        private static decimal Margen(decimal venta, decimal precio)
            => venta == 0 ? 0 : Math.Round((venta - precio) / venta * 100, 2);
    }
}
