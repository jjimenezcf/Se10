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

    public class GestorDeIvasSoportado : GestorDeElementos<ContextoSe, IvaSoportadoDtm, IvaSoportadoDto>
    {

        public class ltrDeUnIvaSoportado
        {

        }

        public class MapearIvaSoportado : Profile
        {
            public MapearIvaSoportado()
            {
                CreateMap<IvaSoportadoDtm, IvaSoportadoDto>()
                .ForMember(dto => dto.Cuenta, x => x.MapFrom(dtm => dtm.Cuenta.Expresion)); 
                CreateMap<IvaSoportadoDto, IvaSoportadoDtm>()
                .ForMember(dtm => dtm.Cuenta, dto => dto.Ignore());
            }
        }

        public GestorDeIvasSoportado(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeIvasSoportado Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeIvasSoportado(contexto, mapeador); ;
        }

        protected override IQueryable<IvaSoportadoDtm> AplicarJoins(IQueryable<IvaSoportadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Cuenta);
            return consulta;
        }

        protected override IQueryable<IvaSoportadoDtm> AplicarFiltros(IQueryable<IvaSoportadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(IvaSoportadoDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    var clase = ApiDeEnsamblados.ToEnumerado<enumClasesDeIvaSop>(filtro.Valor, errorSiNoEsValido: false);
                    if (clase is not null)
                        consulta = consulta.AplicarPredicado(filtro, x => x.Clase == (enumClasesDeIvaSop)clase);
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Nombre.Contains(filtro.Valor));
                }
            }
            return consulta;
        }

        protected override IQueryable<IvaSoportadoDtm> AplicarOrden(IQueryable<IvaSoportadoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            consulta = base.AplicarOrden(consulta, ordenacion);
            return consulta.OrderBy(iva => iva.Nombre);
        }

    }
}
