using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDePeriodosEmt : GestorDeElementos<ContextoSe, PeriodoEmtDtm, PeriodoEmtDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrPeriodosEmt
        {
        }

        public class MapearPeriodosEmt : Profile
        {
            public MapearPeriodosEmt()
            {
                CreateMap<PeriodoEmtDtm, PeriodoEmtDto>();
                CreateMap<PeriodoEmtDto, PeriodoEmtDtm>();
            }
        }

        public GestorDePeriodosEmt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDePeriodosEmt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePeriodosEmt(contexto, mapeador);
        }

        protected override void AntesDePersistir(PeriodoEmtDtm periodoEmt, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(periodoEmt, parametros);

            if (parametros.Modificando)
            {
                var factura = periodoEmt.AmpliacionDe<FacturaEmtDtm>(Contexto);
                if (factura.EsRectificativa)
                {
                    var rectificada = factura.RectificaA(Contexto);
                    var original = rectificada.Ampliacion<PeriodoEmtDtm>(Contexto);
                    if (original.Inicio != periodoEmt.Inicio || original.Fin != periodoEmt.Fin || original.Notacion != periodoEmt.Notacion)
                        GestorDeErrores.Emitir($"La factura '{factura.Referencia}' es rectificativa, por tanto no puede modificarse el periodo de facturación");

                }
            }

        }

    }
}
