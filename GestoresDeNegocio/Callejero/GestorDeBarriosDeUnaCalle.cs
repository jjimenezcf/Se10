using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using Utilidades;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeBarriosDeUnaCalle : GestorDeRelaciones<ContextoSe, BarriosDeUnaCalleDtm, BarriosDeUnaCalleDto>
    {
        public class ltrBarriosDeUnaCalle
        {
            internal static readonly string JoinConCalles = nameof(JoinConCalles);
            internal static readonly string JoinConBarrios = nameof(JoinConBarrios);
        }

        public class MapearBarriosDeUnaCalle : Profile
        {
            public MapearBarriosDeUnaCalle()
            {
                CreateMap<BarriosDeUnaCalleDtm, BarriosDeUnaCalleDto>()
                    .ForMember(dto => dto.Barrio, dtm => dtm.MapFrom(dtm => dtm.Barrio.Nombre));

                CreateMap<BarriosDeUnaCalleDto, BarriosDeUnaCalleDtm>()
                    .ForMember(dtm => dtm.Barrio, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Calle, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Hasta, dto => dto.MapFrom(dto => dto.Hasta == 0 || dto.Hasta == null ? null : dto.Hasta));

            }
        }

        public GestorDeBarriosDeUnaCalle(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeBarriosDeUnaCalle Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeBarriosDeUnaCalle(contexto, mapeador);
        }

        protected override void AntesDeMapearElRegistroParaInsertar(BarriosDeUnaCalleDto barrioDeUnaCalle, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(barrioDeUnaCalle, opciones);
            if (!barrioDeUnaCalle.Barrio.IsNullOrEmpty() && barrioDeUnaCalle.idBarrio == 0)
            {
                var calle = Contexto.SeleccionarPorId<CalleDtm>(barrioDeUnaCalle.IdCalle);
                barrioDeUnaCalle.idBarrio = new BarrioDtm { Nombre = barrioDeUnaCalle.Barrio, IdMunicipio = calle.IdMunicipio }.Insertar(Contexto).Id;
            }

        }

        protected override IQueryable<BarriosDeUnaCalleDtm> AplicarJoins(IQueryable<BarriosDeUnaCalleDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            if (parametros.HacerJoinCon(ltrBarriosDeUnaCalle.JoinConCalles))
                consulta = consulta.Include(rp => rp.Calle);

            if (parametros.HacerJoinCon(ltrBarriosDeUnaCalle.JoinConBarrios))
                consulta = consulta.Include(rp => rp.Barrio);
            return consulta;
        }

        protected override void AntesDePersistir(BarriosDeUnaCalleDtm barriosDeUnaCalle, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(barriosDeUnaCalle, parametros);

            ValidarMismoMunicipio(Contexto, barriosDeUnaCalle);
        }

        protected override void DespuesDePersistir(BarriosDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            ServicioDeCaches.EliminarElemento(CacheDe.Callejero_BarriosDeUnaCalle, registro.IdCalle.ToString());
        }

        internal static void ValidarMismoMunicipio(ContextoSe contexto, BarriosDeUnaCalleDtm barriosDeUnaCalle)
        {
            var muniCalle = GestorDeCalles.LeerRegistroPorId(contexto, barriosDeUnaCalle.IdCalle, aplicarJoin: true);
            var muniBarrio = GestorDeBarrios.LeerRegistroPorId(contexto, barriosDeUnaCalle.IdBarrio, aplicarJoin: true);

            if (muniCalle.IdMunicipio != muniBarrio.IdMunicipio)
            {
                GestorDeErrores.Emitir($"El municipio de la calle dada, {muniCalle.Municipio.Nombre}, ha de ser el mismo que el del barrio. El municipio del barrio es: {muniBarrio.Municipio.Nombre}");
            }
        }
    }
}
