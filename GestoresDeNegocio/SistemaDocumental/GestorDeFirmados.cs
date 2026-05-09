using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeFirmados : GestorDeElementos<ContextoSe, FirmadoDtm, FirmadoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrFirmados
        {
        }

        public class MapearFirmados : Profile
        {
            public MapearFirmados()
            {
                CreateMap<FirmadoDtm, FirmadoDto>();
                CreateMap<FirmadoDto, FirmadoDtm>();
            }
        }

        public GestorDeFirmados(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeFirmados Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeFirmados(contexto, mapeador);
        }



    }
}
