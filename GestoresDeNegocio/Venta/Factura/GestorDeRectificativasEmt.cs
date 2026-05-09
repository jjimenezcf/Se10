using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeRectificativasEmt : GestorDeElementos<ContextoSe, RectificativaEmtDtm, RectificativaEmtDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnaFae
        {
        }

        public class MapearLineasDeUnaFae : Profile
        {
            public MapearLineasDeUnaFae()
            {
                CreateMap<RectificativaEmtDtm, RectificativaEmtDto>()
                .ForMember(dto => dto.Rectificada, x => x.MapFrom(dtm => dtm.Rectificada.NumeroDeFactura))
                .ForMember(dto => dto.FacturadaEl, x => x.MapFrom(dtm => dtm.Rectificada.FacturadaEl));
                CreateMap<RectificativaEmtDto, RectificativaEmtDtm>()
                .ForMember(dtm => dtm.Rectificada, dto => dto.Ignore());
            }
        }

        public GestorDeRectificativasEmt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<RectificativaEmtDtm> AplicarJoins(IQueryable<RectificativaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Rectificada);
            return consulta;
        }

        public static GestorDeRectificativasEmt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRectificativasEmt(contexto, mapeador);
        }

        protected override void AntesDePersistir(RectificativaEmtDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);
        }

        protected override void DespuesDePersistir(RectificativaEmtDtm linea, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(linea, parametros);
        }

        protected override void EliminarCaches(RectificativaEmtDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Fae_RectificaA);
            ServicioDeCaches.EliminarCache(CacheDe.Fae_RectificadaPor);
        }

        protected override void DespuesDeMapearElElemento(RectificativaEmtDtm rectificada, RectificativaEmtDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(rectificada, elemento, parametros);
            elemento.Elemento = Contexto.SeleccionarPorId<FacturaEmtDtm>(rectificada.IdElemento).Expresion;
            elemento.Rectificada = rectificada.Rectificada.Expresion;
            elemento.TotalAPagar = rectificada.Rectificada.APagar(Contexto);
            elemento.Cobrado = rectificada.Rectificada.Cobrado(Contexto);
            elemento.Pendiente = rectificada.Rectificada.PendientePorCobrar(Contexto);
        }

    }
}
