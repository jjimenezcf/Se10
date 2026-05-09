using AutoMapper;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.SistemaDocumental;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeDescargasConGuid : GestorDeElementos<ContextoSe, DescargaConGuidDtm, DescargaConGuidDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public GestorDeDescargasConGuid(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeDescargasConGuid Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeDescargasConGuid(contexto, mapeador);
        }

    }
}
