using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using Utilidades;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeTiposDeVia : GestorDeElementos<ContextoSe, TipoDeViaDtm, TipoDeViaDto>
    {
        public override enumNegocio Negocio => enumNegocio.TipoDeVia;
        class archivoParaImportar
        {
            public string parametro { get; set; }
            public int valor { get; set; }
        }

        public class ltrDeUnTipoDeVia
        {
            public const string ParametroTipoDeVia = "csvTipoDeVias";
        }

        public class MapearTipoDeVia : Profile
        {
            public MapearTipoDeVia()
            {
                CreateMap<TipoDeViaDtm, TipoDeViaDto>();
                CreateMap<TipoDeViaDto, TipoDeViaDtm>();
            }
        }

        public GestorDeTiposDeVia(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeTiposDeVia Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeVia(contexto, mapeador); ;
        }

    }
}
