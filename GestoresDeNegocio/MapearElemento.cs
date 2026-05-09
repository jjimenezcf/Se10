using AutoMapper;
using ModeloDeDto;

namespace GestoresDeNegocio;

public class MapearElementoDeUnProcesado : Profile
{
    public MapearElementoDeUnProcesado()
    {
       
        CreateMap<ElementoDeUnProcesoDto, ElementoMovilOutput>();
            
    }
}