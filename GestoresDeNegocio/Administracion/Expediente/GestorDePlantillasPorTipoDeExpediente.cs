using AutoMapper;
using Utilidades;
using ServicioDeDatos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos.Expediente;

namespace GestoresDeNegocio.Expediente
{
    public class GestorDePlantillasPorTipoDeExpediente : GestorDePlantillasPorTipo<ContextoSe, PlantillaPorTipoDeExpedienteDtm, PlantillaPorTipoDto>
    {
        public class MapearPlantillasPorTipoDeExpediente : MapearPlantillasPorTipo
        {
            public MapearPlantillasPorTipoDeExpediente()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<PlantillaPorTipoDeExpedienteDtm, PlantillaPorTipoDto>());
                ReglasDeMapeoDelDtoAlDtm(CreateMap<PlantillaPorTipoDto, PlantillaPorTipoDeExpedienteDtm>());
            }
        }

        public GestorDePlantillasPorTipoDeExpediente(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Expediente)
        {

        }

        public static GestorDePlantillasPorTipoDeExpediente Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasPorTipoDeExpediente(contexto, mapeador);
        }


    }
}
