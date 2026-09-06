using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeLineasDeUnaRegularizacion : GestorDeElementos<ContextoSe, LineasDeUnaRegularizacionDtm, LineasDeUnaRegularizacionDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearLineasDeUnaRegularizacion : Profile
        {
            public MapearLineasDeUnaRegularizacion()
            {
                CreateMap<LineasDeUnaRegularizacionDtm, LineasDeUnaRegularizacionDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion));
                CreateMap<LineasDeUnaRegularizacionDto, LineasDeUnaRegularizacionDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaRegularizacion(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeLineasDeUnaRegularizacion Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaRegularizacion(contexto, mapeador);
        }

        protected override IQueryable<LineasDeUnaRegularizacionDtm> AplicarJoins(IQueryable<LineasDeUnaRegularizacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            return consulta;
        }

        protected override void AntesDePersistir(LineasDeUnaRegularizacionDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);
            // TODO: completar cuando se conozca el detalle funcional de las líneas de una regularización
        }
    }
}
