using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using Gestor.Errores;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeRecobros : GestorDeElementos<ContextoSe, RecobroDtm, RecobroDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrRecobros
        {
        }

        public class MapearRecobros : Profile
        {
            public MapearRecobros()
            {
                CreateMap<RecobroDtm, RecobroDto>();
                CreateMap<RecobroDto, RecobroDtm>();
            }
        }

        public GestorDeRecobros(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeRecobros Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRecobros(contexto, mapeador);
        }

        protected override void AntesDePersistir(RecobroDtm recobro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(recobro, parametros);

            if (recobro.FechaDeDeuda >= System.DateTime.Today)
            {
                GestorDeErrores.Emitir("La fecha de la deuda debe ser anterior a la del día de hoy");
            }
        }

    }
}
