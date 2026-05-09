using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using GestorDeElementos.Extensores;
using Gestor.Errores;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeAvances : GestorDeElementos<ContextoSe, AvanceDtm, AvanceDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrAvances
        {
        }

        public class MapearAvances : Profile
        {
            public MapearAvances()
            {
                CreateMap<AvanceDtm, AvanceDto>();
                CreateMap<AvanceDto, AvanceDtm>();
            }
        }

        public GestorDeAvances(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAvances Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAvances(contexto, mapeador);
        }


        protected override void AntesDePersistir(AvanceDtm avance, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(avance, parametros);
            if (avance.Cobrado < 0) avance.Cobrado = 0;
            if (avance.Facturado < 0) avance.Facturado = 0;
            avance.ValidarPorcentageDeBloqueo(Contexto);
        }

        protected override void DespuesDeMapearElElemento(AvanceDtm avance, AvanceDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(avance, elemento, parametros);
        }

    }
}
