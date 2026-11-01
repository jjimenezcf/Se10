using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Utilidades;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeLineasDeUnaVenta : GestorDeElementos<ContextoSe, LineaDeUnaVentaDtm, LineaDeUnaVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearLineasDeUnaVenta : Profile
        {
            public MapearLineasDeUnaVenta()
            {
                CreateMap<LineaDeUnaVentaDtm, LineaDeUnaVentaDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Importe, x => x.MapFrom(dtm => dtm.Importe))
                .ForMember(dto => dto.PrecioAlmacen, x => x.Ignore())
                .ForMember(dto => dto.Margen, x => x.Ignore());
                CreateMap<LineaDeUnaVentaDto, LineaDeUnaVentaDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeLineasDeUnaVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaVenta(contexto, mapeador);
        }

        protected override IQueryable<LineaDeUnaVentaDtm> AplicarJoins(IQueryable<LineaDeUnaVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            return consulta;
        }

        protected override void AntesDePersistir(LineaDeUnaVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);
            // TODO: completar cuando se conozca el detalle funcional de las líneas de un documento de venta
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnaVentaDtm linea, LineaDeUnaVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);
            elemento.PrecioAlmacen = linea.PrecioAlmacen(Contexto);
            elemento.Margen = linea.Margen(elemento.PrecioAlmacen);
        }
    }
}
