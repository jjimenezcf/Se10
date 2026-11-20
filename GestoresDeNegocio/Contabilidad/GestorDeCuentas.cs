using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{

    public class GestorDeCuentas : GestorDeElementos<ContextoSe, CuentaDtm, CuentaDto>
    {
        public class MapearCuenta : Profile
        {
            public MapearCuenta()
            {
                CreateMap<CuentaDtm, CuentaDto>();
                CreateMap<CuentaDto, CuentaDtm>();
            }
        }

        public GestorDeCuentas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeCuentas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCuentas(contexto, mapeador); ;
        }

        protected override void DefinirOrden(List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros)
        {
            if (parametros.CargarListaDeElementos)
            { 
                orden.Clear();
                orden.Add(new ClausulaDeOrdenacion { Modo = ModoDeOrdenancion.ascendente, OrdenarPor = nameof(CuentaDtm.Codigo) } );
            }

            base.DefinirOrden(orden, parametros);
        }
        protected override IQueryable<CuentaDtm> AplicarFiltros(IQueryable<CuentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorUso(Contexto, filtros);
            consulta = consulta.FiltrarPorCodigoONombre(filtros);
            consulta = consulta.FiltrarPorVariable(filtros);
            return consulta;
        }
        protected override void ValidarPermisosDePersistencia(CuentaDtm registro, ParametrosDeNegocio parametros)
        {
            base.ValidarPermisosDePersistencia(registro, parametros);
        }
        protected override void DespuesDeMapearElElemento(CuentaDtm registro, CuentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
        }

    }

}


