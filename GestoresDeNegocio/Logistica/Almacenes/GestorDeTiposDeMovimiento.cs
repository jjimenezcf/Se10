using AutoMapper;
using GestorDeElementos;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    // Los tipos de movimiento no tienen mantenimiento: este gestor sólo existe para olvidar la caché de tipos de movimiento
    // (ver ExtensorDeAlmacenes.TipoDeMovimiento) al crearlos, modificarlos o borrarlos
    public class GestorDeTiposDeMovimiento : GestorDeElementos<ContextoSe, TipoMovimientoDtm, ElementoDto>
    {
        public class MapearTiposDeMovimiento : Profile
        {
            public MapearTiposDeMovimiento()
            {
                CreateMap<TipoMovimientoDtm, ElementoDto>();
            }
        }

        public GestorDeTiposDeMovimiento(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override void EliminarCaches(TipoMovimientoDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Alm_TiposDeMovimiento);
        }
    }
}
