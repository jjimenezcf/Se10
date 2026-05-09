using AutoMapper;
using Utilidades;
using ServicioDeDatos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos.Tarea;

namespace GestoresDeNegocio.Tarea
{
    public class GestorDeClasesDelTipoTarea : GestorDeClasesDelTipo<ContextoSe, ClaseDelTipoTareaDtm, ClaseDelTipoDto>
    {

        public class MapearClasesDelTipoDeTarea : MapearClasesDelTipo
        {
            public MapearClasesDelTipoDeTarea()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<ClaseDelTipoTareaDtm, ClaseDelTipoDto>());
                ReglasDeMapeoDelDtoAlDtm(CreateMap<ClaseDelTipoDto, ClaseDelTipoTareaDtm>());
            }
        }


        public GestorDeClasesDelTipoTarea(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Tarea)
        {

        }

        public static GestorDeClasesDelTipoTarea Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeClasesDelTipoTarea(contexto, mapeador);
        }


    }
}
