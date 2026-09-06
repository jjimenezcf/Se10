using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.MaestrosTecnico;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Juridico;

namespace GestoresDeNegocio.MaestrosTecnico
{

    public class GestorDeUnitarios : GestorDeElementos<ContextoSe, UnitarioDtm, UnitarioDto>
    {
        public override enumNegocio Negocio => enumNegocio.Unitario;

        public class MapearUnitario : Profile
        {
            public MapearUnitario()
            {
                CreateMap<UnitarioDtm, UnitarioDto>()
                    .ForMember(dto => dto.Naturaleza, dtm => dtm.MapFrom(dtm => dtm.Naturaleza.Expresion))
                    .ForMember(dto => dto.Unidad, dtm => dtm.MapFrom(dtm => dtm.Unidad.Expresion));

                CreateMap<UnitarioDto, UnitarioDtm>()
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore());
            }
        }

        public GestorDeUnitarios(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeUnitarios Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUnitarios(contexto, mapeador); ;
        }

        protected override IQueryable<UnitarioDtm> AplicarJoins(IQueryable<UnitarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);

            consulta = consulta.Include(e => e.Naturaleza);
            consulta = consulta.Include(e => e.Unidad);

            return consulta;
        }

        protected override IQueryable<UnitarioDtm> AplicarFiltros(IQueryable<UnitarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorLote(Contexto, filtros, parametros);
            consulta = consulta.FiltrarPorClaseDeUnitario(filtros);
            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(UnitarioDto elemento, UnitarioDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (elemento.Proponer)
            {
                var filtro = new ClausulaDeFiltrado(nameof(UnitarioDtm.Referencia), enumCriteriosDeFiltrado.comienza, elemento.Referencia);
                var contar = Contar(new List<ClausulaDeFiltrado> { filtro }) + 1;
                registro.Referencia = registro.Referencia + contar.ToString().PadLeft(5, '0');
            }

        }
        protected override void DespuesDeMapearElElemento(UnitarioDtm registro, UnitarioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var idLote = (int)parametros.Parametros.LeerValor(ltrDeUnUnitario.PreciosDelLote, 0);
            if (idLote>0)
            {
                elemento.Venta = Contexto.SeleccionarPorAk<UnitariosDeUnLoteDtm>(new Dictionary<string, object> {
                                 { nameof(UnitariosDeUnLoteDtm.IdLote),idLote },
                                 { nameof(UnitariosDeUnLoteDtm.IdUnitario),registro.Id }
                             }).Venta;
            }

        }

    }
}
