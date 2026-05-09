using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeCallesDeUnBarrio : GestorDeRelaciones<ContextoSe, BarriosDeUnaCalleDtm, CallesDeUnBarrioDto>
    {
        public class ltrCallesDeUnBarrio
        {
            internal static readonly string JoinConCalles = nameof(JoinConCalles);
            internal static readonly string JoinConBarrios = nameof(JoinConBarrios);
        }

        public class MapearCallesDeUnBarrio : Profile
        {
            public MapearCallesDeUnBarrio()
            {
                CreateMap<BarriosDeUnaCalleDtm, CallesDeUnBarrioDto>()
                    .ForMember(dto => dto.Barrio, dtm => dtm.MapFrom(dtm => dtm.Barrio.Nombre))
                    .ForMember(dto => dto.Calle, dtm => dtm.MapFrom(dtm => dtm.Calle.Nombre));

                CreateMap<CallesDeUnBarrioDto, BarriosDeUnaCalleDtm>()
                    .ForMember(dtm => dtm.Barrio, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Calle, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Hasta, dto => dto.MapFrom(dto => dto.Hasta == 0 || dto.Hasta == null ? null : dto.Hasta));

            }
        }

        public GestorDeCallesDeUnBarrio(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeCallesDeUnBarrio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCallesDeUnBarrio(contexto, mapeador);
        }


        protected override IQueryable<BarriosDeUnaCalleDtm> AplicarJoins(IQueryable<BarriosDeUnaCalleDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrCallesDeUnBarrio.JoinConCalles))
                registros = registros.Include(rp => rp.Calle);

            if (parametros.HacerJoinCon(ltrCallesDeUnBarrio.JoinConBarrios))
                registros = registros.Include(rp => rp.Barrio);
            return registros;
        }

        protected override void AntesDePersistir(BarriosDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            GestorDeBarriosDeUnaCalle.ValidarMismoMunicipio(Contexto, registro);
        }


    }
}
