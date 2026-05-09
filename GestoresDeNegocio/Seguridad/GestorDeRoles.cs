using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Elemento;
using GestorDeElementos.Extensores;
using Gestor.Errores;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDeRoles : GestorDeElementos<ContextoSe, RolDtm, RolDto>
    {
        public override enumNegocio Negocio => enumNegocio.Rol;

        public class MapearPuestoDeTrabajo : Profile
        {
            public MapearPuestoDeTrabajo()
            {
                CreateMap<RolDtm, RolDto>();
                CreateMap<RolDto, RolDtm>();
            }
        }

        public GestorDeRoles(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }


        public static GestorDeRoles Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRoles(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(RolDto elemento, RolDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (elemento.idPermiso is not null)
            {
                opciones.Parametros[nameof(RolDto.idPermiso)] = elemento.idPermiso; 
            }
        }

        protected override void DespuesDePersistir(RolDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Parametros.LeerValor(nameof(RolDto.idPermiso), 0) > 0)
            {
                new PermisosDeUnRolDtm
                {
                    IdPermiso = parametros.Parametros.LeerValor(nameof(RolDto.idPermiso), 0),
                    IdRol = registro.Id
                }.Insertar(Contexto);
            }
        }

        protected override IQueryable<RolDtm> AplicarJoins(IQueryable<RolDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Permisos);
            consulta = consulta.Include(x => x.Puestos);
            return consulta;

        }

        protected override IQueryable<RolDtm> AplicarFiltros(IQueryable<RolDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDtm.IdPuesto).ToLower())
                {
                    if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                        consulta = consulta.AplicarPredicado(filtro, i => i.Puestos.Any(r => r.IdPuesto == filtro.Valor.Entero()));

                    if (filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                        consulta = consulta.AplicarPredicado(filtro, i => !i.Puestos.Any(r => r.IdPuesto == filtro.Valor.Entero()));
                }


                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnRolDtm.IdPermiso).ToLower())
                {
                    if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                        consulta = consulta.AplicarPredicado(filtro, i => i.Permisos.Any(r => r.IdPermiso == filtro.Valor.Entero()));

                    if (filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                        consulta = consulta.AplicarPredicado(filtro, i => !i.Permisos.Any(r => r.IdPermiso == filtro.Valor.Entero()));
                }

                if (filtro.Clausula.ToLower() == nameof(PuestosDeUnUsuarioDtm.IdUsuario).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    var puestosDeUnUsuario = Contexto.Set<PuestosDeUnUsuarioDtm>().AsNoTracking().Where(p => p.IdUsuario == filtro.Valor.Entero());
                    var listaDePuestos = puestosDeUnUsuario.Select(p => p.IdPuesto);
                    var rolesDeunPuesto = Contexto.Set<RolesDeUnPuestoDtm>().AsNoTracking().Where(r => listaDePuestos.Contains(r.IdPuesto));
                    var listaDeRoles = rolesDeunPuesto.Select(r => r.IdRol);
                    consulta = consulta.Where(r => listaDeRoles.Contains(r.Id));
                    filtro.Aplicado = true;
                }

            }
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(RolDtm registro, RolDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.ModoDeAcceso = Contexto.SePuedeParametrizar() ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
        }
    }

}
