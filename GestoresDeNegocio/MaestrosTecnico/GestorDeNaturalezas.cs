using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using System.Collections.Generic;
using System.Linq;

namespace GestoresDeNegocio.MaestrosTecnico
{

    public class GestorDeNaturalezas : GestorDeElementos<ContextoSe, NaturalezaDtm, NaturalezaDto>
    {

        public class ltrDeUnNaturalezaContable
        {

        }

        public class MapearNaturalezaContable : Profile
        {
            public MapearNaturalezaContable()
            {
                CreateMap<NaturalezaDtm, NaturalezaDto>()
                .ForMember(dto => dto.CuentaDeGasto, x => x.MapFrom(dtm => dtm.CuentaDeGasto.Expresion))
                .ForMember(dto => dto.CuentaDeIngreso, x => x.MapFrom(dtm => dtm.CuentaDeIngreso.Expresion));
                CreateMap<NaturalezaDto, NaturalezaDtm>()
                .ForMember(dtm => dtm.CuentaDeIngreso, dto => dto.Ignore())
                .ForMember(dtm => dtm.CuentaDeGasto, dto => dto.Ignore());
            }
        }

        public GestorDeNaturalezas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeNaturalezas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeNaturalezas(contexto, mapeador); ;
        }

        protected override IQueryable<NaturalezaDtm> AplicarJoins(IQueryable<NaturalezaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta  = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.CuentaDeGasto);
            consulta = consulta.Include(x => x.CuentaDeIngreso);
            return consulta;
        }

    }
}
