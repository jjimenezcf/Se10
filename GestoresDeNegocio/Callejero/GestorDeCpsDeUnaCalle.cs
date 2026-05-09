using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Utilidades;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeCpsDeUnaCalle : GestorDeRelaciones<ContextoSe, CpsDeUnaCalleDtm, CpsDeUnaCalleDto>
    {
        public class ltrCpsDeUnaCalle
        {
            internal static readonly string JoinConCalles = nameof(JoinConCalles);
            internal static readonly string JoinConCps = nameof(JoinConCps);
        }

        public class MapearCpsDeUnaCalle : Profile
        {
            public MapearCpsDeUnaCalle()
            {
                CreateMap<CpsDeUnaCalleDtm, CpsDeUnaCalleDto>()
                    .ForMember(dto => dto.CodigoPostal, dtm => dtm.MapFrom(dtm => dtm.Cp.Codigo))
                    .ForMember(dto => dto.Calle, dtm => dtm.MapFrom(dtm => dtm.Calle.Nombre));

                CreateMap<CpsDeUnaCalleDto, CpsDeUnaCalleDtm>()
                    .ForMember(dtm => dtm.Cp, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Calle, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Hasta, dto => dto.MapFrom(dto => dto.Hasta == 0 || dto.Hasta == null ? (int ?)null : dto.Hasta));

            }
        }

        public GestorDeCpsDeUnaCalle(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeCpsDeUnaCalle Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCpsDeUnaCalle(contexto, mapeador);
        }

        protected override IQueryable<CpsDeUnaCalleDtm> AplicarJoins(IQueryable<CpsDeUnaCalleDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrCpsDeUnaCalle.JoinConCalles))
                registros = registros.Include(rp => rp.Calle);

            if (parametros.HacerJoinCon(ltrCpsDeUnaCalle.JoinConCps))
                registros = registros.Include(rp => rp.Cp);
            return registros;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(CpsDeUnaCalleDto cpDeUnaCalle, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(cpDeUnaCalle, opciones);
            if (!cpDeUnaCalle.CodigoPostal.IsNullOrEmpty() && cpDeUnaCalle.IdCp == 0)
            {
                cpDeUnaCalle.IdCp = new CodigoPostalDtm { Codigo = cpDeUnaCalle.CodigoPostal }.Insertar(Contexto).Id;
            }
        }

        protected override void DespuesDePersistir(CpsDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                GestorDeCpsDeUnMunicipio.RelacionarCpConMunicipio(Contexto, registro);


            ServicioDeCaches.EliminarElemento(CacheDe.Callejero_CpsDeUnaCalle, registro.IdCalle.ToString());
        }

        protected override void AntesDePersistir(CpsDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                if (registro.Hasta == 0) registro.Hasta = 99999;
                if (registro.Mano.IsNullOrEmpty()) registro.Mano = enumManoDeUnaCalle.Ambos.ToBd();
            }
        }

    }
}
