using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.MaestrosTecnico;
using Gestor.Errores;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Juridico
{

    public class GestorDeUnitariosDeUnLote : GestorDeRelaciones<ContextoSe,UnitariosDeUnLoteDtm, UnitariosDeUnLoteDto>
    {

        public class MapearUnitariosDeUnLote : Profile
        {
            public MapearUnitariosDeUnLote()
            {
                CreateMap<UnitariosDeUnLoteDtm, UnitariosDeUnLoteDto>()
                    .ForMember(dto => dto.Lote, dtm => dtm.MapFrom(dtm => dtm.Lote.Expresion))
                    .ForMember(dto => dto.Unitario, dtm => dtm.MapFrom(dtm => dtm.Unitario.Expresion));

                CreateMap<UnitariosDeUnLoteDto, UnitariosDeUnLoteDtm>()
                    .ForMember(dtm => dtm.Lote, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Unitario, dto => dto.Ignore());

            }
        }

        public GestorDeUnitariosDeUnLote(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeUnitariosDeUnLote Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUnitariosDeUnLote(contexto, mapeador);
        }


        protected override IQueryable<UnitariosDeUnLoteDtm> AplicarJoins(IQueryable<UnitariosDeUnLoteDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrUnitariosDeUnLote.JoinConUnitarios))
                registros = registros.Include(rp => rp.Unitario);

            if (parametros.HacerJoinCon(ltrUnitariosDeUnLote.JoinConLotes))
                registros = registros.Include(rp => rp.Lote);
            return registros;
        }

        protected override void AntesDePersistir(UnitariosDeUnLoteDtm unitarioDeUnLote, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(unitarioDeUnLote, parametros);

            if (parametros.Insertando && parametros.Peticion == enumPeticion.epCrearRelaciones)
            {
                var unitario = Contexto.SeleccionarPorId<UnitarioDtm>(unitarioDeUnLote.IdUnitario);
                unitarioDeUnLote.Coste = unitario.Coste;
                unitarioDeUnLote.Venta = unitario.Venta;
            }

            if (parametros.Eliminando)
            {
                var hay = Contexto.Set<LineaDeUnPlfVentaDtm>().Where(x => x.IdUnitario == unitarioDeUnLote.IdUnitario &&
                             Contexto.Set<PlanificadorDeVentaDtm>().Any(y => y.Id == x.IdElemento && y.IdLote == unitarioDeUnLote.IdLote)).FirstOrDefault() != null;
                if (hay)
                    GestorDeErrores.Emitir("no se puede suprimir el unitario del lote ya que se está usado en planificaciones");
            }
        }

        protected override void ValidarPermisosDePersistencia(UnitariosDeUnLoteDtm unitarioDeUnLote, ParametrosDeNegocio parametros) 
        =>
        unitarioDeUnLote.ValidarQueLaLineaDelLoteEsModificable(Contexto);

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(UnitariosDeUnLoteDtm linea, UnitariosDeUnLoteDto elemento, ParametrosDeNegocio parametros)
        =>
        elemento.ModoDeAcceso = Contexto.SeleccionarPorId<LoteDeUnContratoDtm>(linea.IdLote).ModoDeAccesoAlLote(Contexto);
    }
}
