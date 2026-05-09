using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeCallesDeUnaZona : GestorDeRelaciones<ContextoSe, ZonasDeUnaCalleDtm, CallesDeUnaZonaDto>
    {
        public class ltrCallesDeUnZona
        {
            internal static readonly string JoinConCalles = nameof(JoinConCalles);
            internal static readonly string JoinConZonas = nameof(JoinConZonas);
        }

        public class MapearCallesDeUnZona : Profile
        {
            public MapearCallesDeUnZona()
            {
                CreateMap<ZonasDeUnaCalleDtm, CallesDeUnaZonaDto>()
                    .ForMember(dto => dto.Zona, dtm => dtm.MapFrom(dtm => dtm.Zona.Nombre))
                    .ForMember(dto => dto.Calle, dtm => dtm.MapFrom(dtm => dtm.Calle.Nombre));

                CreateMap<CallesDeUnaZonaDto, ZonasDeUnaCalleDtm>()
                    .ForMember(dtm => dtm.Zona, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Calle, dto => dto.Ignore());

            }
        }

        public GestorDeCallesDeUnaZona(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeCallesDeUnaZona Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCallesDeUnaZona(contexto, mapeador);
        }


        protected override IQueryable<ZonasDeUnaCalleDtm> AplicarJoins(IQueryable<ZonasDeUnaCalleDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrCallesDeUnZona.JoinConCalles))
                registros = registros.Include(rp => rp.Calle);

            if (parametros.HacerJoinCon(ltrCallesDeUnZona.JoinConZonas))
                registros = registros.Include(rp => rp.Zona);
            return registros;
        }

        protected override void AntesDePersistir(ZonasDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            GestorDeZonasDeUnaCalle.ValidarMismoMunicipio(Contexto, registro);
        }


    }
}
