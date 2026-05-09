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

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(CuentaDto.Expresion).ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.EsEntero())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Codigo.StartsWith(filtro.Valor));
                else
                    consulta = consulta.AplicarPredicado(filtro, x => x.Codigo.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor));
                filtro.Aplicado = true;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrVariables.FiltroPorVariable);
            if (filtro != null)
            {
                var codigo = "";
                if (filtro.Valor.ToLower() == nameof(VariablesDeCuentas.Sueldos).ToLower())
                    codigo = VariablesDeCuentas.Sueldos;
                else if (filtro.Valor.ToLower() == nameof(VariablesDeCuentas.Proveedores).ToLower())
                    codigo = VariablesDeCuentas.Proveedores;
                else if (filtro.Valor.ToLower() == nameof(VariablesDeCuentas.Clientes).ToLower())
                    codigo = VariablesDeCuentas.Clientes;
                else GestorDeErrores.Emitir($"No hay variable de cuenta contable del tipo : {filtro.Valor}");

                consulta = consulta.AplicarPredicado(filtro, x => x.Codigo == codigo);
            }
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


