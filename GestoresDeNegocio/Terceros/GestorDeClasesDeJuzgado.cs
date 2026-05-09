using AutoMapper;
using GestorDeElementos;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;

namespace GestoresDeNegocio.Terceros
{


    public class GestorDeClasesDeJuzgado : GestorDeElementos<ContextoSe, ClaseDeJuzgadoDtm, ClaseDeJuzgadoDto>
    {

        public class MapearClaseDeJuzgado : Profile
        {
            public MapearClaseDeJuzgado()
            {
                CreateMap<ClaseDeJuzgadoDtm, ClaseDeJuzgadoDto>();
                CreateMap<ClaseDeJuzgadoDto, ClaseDeJuzgadoDtm>();
            }
        }

        public GestorDeClasesDeJuzgado(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        internal static GestorDeClasesDeJuzgado Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeClasesDeJuzgado(contexto, mapeador);
        }

        public static ClaseDeJuzgadoDto CrearClaseDto(ContextoSe contexto, string clase)
        {
            var claseDeJuzgado = CrearClaseDtm(contexto, clase);
            return claseDeJuzgado.MapearDto<ClaseDeJuzgadoDto>(contexto);
        }

        private static ClaseDeJuzgadoDtm CrearClaseDtm(ContextoSe contexto, string clase)
        {
            var claseDtm = contexto.SeleccionarPorPropiedad<ClaseDeJuzgadoDtm>(nameof(ClaseDeJuzgadoDtm.Nombre), clase, errorSiNoHay: false);
            if (claseDtm == null)
            {
                claseDtm = new ClaseDeJuzgadoDtm();
                claseDtm.Nombre = clase;
                claseDtm = claseDtm.Insertar(contexto);
            }
            return claseDtm;
        }


    }
}
