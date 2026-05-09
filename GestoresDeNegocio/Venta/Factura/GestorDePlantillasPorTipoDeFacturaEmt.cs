using AutoMapper;
using Utilidades;
using ServicioDeDatos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDePlantillasPorTipoDeFacturaEmt : GestorDePlantillasPorTipo<ContextoSe, PlantillaPorTipoDeFacturaEmtDtm, PlantillaPorTipoDto>
    {

        public class MapearPlantillasPorTipoDeFacturaEmt : MapearPlantillasPorTipo
        {
            public MapearPlantillasPorTipoDeFacturaEmt()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<PlantillaPorTipoDeFacturaEmtDtm, PlantillaPorTipoDto>());
                ReglasDeMapeoDelDtoAlDtm(CreateMap<PlantillaPorTipoDto, PlantillaPorTipoDeFacturaEmtDtm>());
            }
        }


        public GestorDePlantillasPorTipoDeFacturaEmt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.FacturaEmitida)
        {

        }

        public static GestorDePlantillasPorTipoDeFacturaEmt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasPorTipoDeFacturaEmt(contexto, mapeador);
        }


    }
}
