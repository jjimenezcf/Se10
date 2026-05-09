using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.MaestrosTecnico;
using Gestor.Errores;
using ServicioDeDatos.MaestrosTecnico;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GestoresDeNegocio.MaestrosTecnico
{
    public class GestorDeTarifas : GestorDeElementos<ContextoSe, TarifaDtm, TarifaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrTarifasDeUnUnitarios
        {
        }

        public class MapearTarifasDeUnitarios : Profile
        {
            public MapearTarifasDeUnitarios()
            {
                CreateMap<TarifaDtm, TarifaDto>()
                .ForMember(dto => dto.Proveedor, dtm => dtm.MapFrom(dtm => dtm.Proveedor.Expresion))
                .ForMember(dto => dto.Elemento, dtm => dtm.MapFrom(dtm => dtm.Elemento.Expresion));
                CreateMap<TarifaDto, TarifaDtm>()
                .ForMember(dtm => dtm.Proveedor, dto => dto.Ignore());
            }
        }

        public GestorDeTarifas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeTarifas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTarifas(contexto, mapeador);
        }

        protected override IQueryable<TarifaDtm> AplicarJoins(IQueryable<TarifaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Proveedor);
            consulta = consulta.Include(p => p.Elemento);
            return consulta;
        }

        protected override void AntesDePersistir(TarifaDtm tarifa, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tarifa, parametros);

            if (tarifa.Referencia.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe indicar la referencia del proveedor");

            if (tarifa.Tarifa <= 0)
                GestorDeErrores.Emitir($"No se puede indicar una tarifa de valor 0");
        }
    }
}
