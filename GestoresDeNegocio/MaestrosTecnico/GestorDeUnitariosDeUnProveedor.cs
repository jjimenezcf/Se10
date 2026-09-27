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
    /// <summary>
    /// Gestiona las tarifas (TarifaDtm) vistas desde el proveedor: restringidas por el proveedor y seleccionando el unitario
    /// </summary>
    public class GestorDeUnitariosDeUnProveedor : GestorDeElementos<ContextoSe, TarifaDtm, UnitariosDeUnProveedorDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearUnitariosDeUnProveedor : Profile
        {
            public MapearUnitariosDeUnProveedor()
            {
                CreateMap<TarifaDtm, UnitariosDeUnProveedorDto>()
                .ForMember(dto => dto.Proveedor, dtm => dtm.MapFrom(dtm => dtm.Proveedor.Expresion))
                .ForMember(dto => dto.Elemento, dtm => dtm.MapFrom(dtm => dtm.Elemento.Expresion));
                CreateMap<UnitariosDeUnProveedorDto, TarifaDtm>()
                .ForMember(dtm => dtm.Proveedor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore());
            }
        }

        public GestorDeUnitariosDeUnProveedor(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeUnitariosDeUnProveedor Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUnitariosDeUnProveedor(contexto, mapeador);
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

            if (tarifa.IdProveedor == 0)
                GestorDeErrores.Emitir($"Debe indicar el proveedor");

            if (tarifa.IdElemento == 0)
                GestorDeErrores.Emitir($"Debe indicar el unitario");

            if (tarifa.Referencia.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe indicar la referencia del proveedor");

            if (tarifa.Tarifa <= 0)
                GestorDeErrores.Emitir($"No se puede indicar una tarifa de valor 0");
        }
    }
}
