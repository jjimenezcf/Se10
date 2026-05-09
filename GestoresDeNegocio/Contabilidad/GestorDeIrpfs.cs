using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{

    public class GestorDeIrpfs : GestorDeElementos<ContextoSe, IrpfDtm, IrpfDto>
    {

        public class ltrDeUnIrpf
        {

        }

        public class MapearIrpf : Profile
        {
            public MapearIrpf()
            {
                CreateMap<IrpfDtm, IrpfDto>()
                .ForMember(dto => dto.Cuenta, x => x.MapFrom(dtm => dtm.Cuenta.Expresion)); 
                CreateMap<IrpfDto, IrpfDtm>()
                .ForMember(dtm => dtm.Cuenta, dto => dto.Ignore());
            }
        }

        public GestorDeIrpfs(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeIrpfs Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeIrpfs(contexto, mapeador); ;
        }

        protected override IQueryable<IrpfDtm> AplicarJoins(IQueryable<IrpfDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Cuenta);
            return consulta;
        }

        protected override IQueryable<IrpfDtm> AplicarFiltros(IQueryable<IrpfDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(IrpfDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (filtro.Valor.EsEntero())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Codigo.StartsWith(filtro.Valor));
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Codigo.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor));
                }
            }
            return consulta;
        }

    }
}
