using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Contabilidad;
using ModeloDeDto.Contabilidad;

namespace GestoresDeNegocio.Contabilidad
{
    public class GestorDeApuntesDeUnPreasiento : GestorDeElementos<ContextoSe, ApunteDeUnPreasientoDtm, ApunteDeUnPreasientoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrCuentasDeProveedor
        {
        }

        public class MapearCuentasDeProveedor : Profile
        {
            public MapearCuentasDeProveedor()
            {
                CreateMap<ApunteDeUnPreasientoDtm, ApunteDeUnPreasientoDto>();
                CreateMap<ApunteDeUnPreasientoDto, ApunteDeUnPreasientoDtm>()
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeApuntesDeUnPreasiento(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<ApunteDeUnPreasientoDtm> AplicarJoins(IQueryable<ApunteDeUnPreasientoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta;
        }

        public static GestorDeApuntesDeUnPreasiento Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeApuntesDeUnPreasiento(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(ApunteDeUnPreasientoDto ccDto, ApunteDeUnPreasientoDtm ccDtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(ccDto, ccDtm, opciones);
        }


        protected override void AntesDePersistir(ApunteDeUnPreasientoDtm cp, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cp, parametros);
        }


        protected override void DespuesDePersistir(ApunteDeUnPreasientoDtm cp, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cp, parametros);        }

        protected override void DespuesDeMapearElElemento(ApunteDeUnPreasientoDtm cp, ApunteDeUnPreasientoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(cp, elemento, parametros);
        }

    }
}
