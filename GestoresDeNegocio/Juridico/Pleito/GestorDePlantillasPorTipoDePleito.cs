using AutoMapper;
using Utilidades;
using ServicioDeDatos;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos.Juridico;
using ModeloDeDto;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDePlantillasPorTipoDePleito : GestorDePlantillasPorTipo<ContextoSe, PlantillaPorTipoDePleitoDtm, PlantillaPorTipoDto>
    {

        public class MapearPlantillasPorTipoDePleito : MapearPlantillasPorTipo
        {
            public MapearPlantillasPorTipoDePleito()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<PlantillaPorTipoDePleitoDtm, PlantillaPorTipoDto>());
                ReglasDeMapeoDelDtoAlDtm(CreateMap<PlantillaPorTipoDto, PlantillaPorTipoDePleitoDtm>());
            }
        }


        public GestorDePlantillasPorTipoDePleito(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Pleito)
        {

        }

        public static GestorDePlantillasPorTipoDePleito Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasPorTipoDePleito(contexto, mapeador);
        }


    }
}
