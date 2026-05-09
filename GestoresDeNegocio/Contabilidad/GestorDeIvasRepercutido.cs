using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{

    public class GestorDeIvasRepercutido : GestorDeElementos<ContextoSe, IvaRepercutidoDtm, IvaRepercutidoDto>
    {

        public class ltrDeUnIvaRepercutido
        {

        }

        public class MapearIvaRepercutido : Profile
        {
            public MapearIvaRepercutido()
            {
                CreateMap<IvaRepercutidoDtm, IvaRepercutidoDto>()
                .ForMember(dto => dto.Cuenta, x => x.MapFrom(dtm => dtm.Cuenta.Expresion)); 
                CreateMap<IvaRepercutidoDto, IvaRepercutidoDtm>()
                .ForMember(dtm => dtm.Cuenta, dto => dto.Ignore());
            }
        }

        public GestorDeIvasRepercutido(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeIvasRepercutido Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeIvasRepercutido(contexto, mapeador); ;
        }

        protected override IQueryable<IvaRepercutidoDtm> AplicarJoins(IQueryable<IvaRepercutidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Cuenta);
            return consulta;
        }

        protected override IQueryable<IvaRepercutidoDtm> AplicarFiltros(IQueryable<IvaRepercutidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(IvaRepercutidoDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    var codigo = ApiDeEnsamblados.ToEnumerado<enumClasesDeIvaRep>(filtro.Valor, errorSiNoEsValido: false);
                    if (codigo is not null)
                        consulta = consulta.AplicarPredicado(filtro, x => x.Clase == (enumClasesDeIvaRep)codigo);
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Nombre.Contains(filtro.Valor));
                }
            }
            return consulta;
        }

        protected override IQueryable<IvaRepercutidoDtm> AplicarOrden(IQueryable<IvaRepercutidoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            consulta =  base.AplicarOrden(consulta, ordenacion);
            return consulta.OrderBy(x => x.Clase).ThenBy(iva => iva.Nombre);
        }

        protected override void AntesDePersistir(IvaRepercutidoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (registro.Exento && registro.DescripcionFiscal.IsNullOrEmpty())
                GestorDeErrores.Emitir("Un Iva exento ha de tener una descripción fiscal de por qué lo es");
        }

    }
}
