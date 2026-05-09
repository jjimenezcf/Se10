using AutoMapper;
using GestorDeElementos;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;

namespace GestoresDeNegocio.MaestrosTecnico
{

    public class GestorDeUnidades : GestorDeElementos<ContextoSe, UnidadDtm, UnidadDto>
    {

        public class ltrDeUnUnidad
        {

        }

        public class MapearUnidad : Profile
        {
            public MapearUnidad()
            {
                CreateMap<UnidadDtm, UnidadDto>();
                CreateMap<UnidadDto, UnidadDtm>();
            }
        }

        public GestorDeUnidades(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeUnidades Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUnidades(contexto, mapeador); ;
        }

    }
}
