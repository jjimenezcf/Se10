using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDeRolesDeUnPermiso : GestorDeRelaciones<ContextoSe, PermisosDeUnRolDtm, RolesDeUnPermisoDto>
    {

        public class MapearRolesDeUnPermiso : Profile
        {
            public MapearRolesDeUnPermiso()
            {
                CreateMap<PermisosDeUnRolDtm, RolesDeUnPermisoDto>()
                    .ForMember(dto => dto.Rol, dtm => dtm.MapFrom(dtm => dtm.Rol.Nombre))
                    .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));

                CreateMap<RolesDeUnPermisoDto, PermisosDeUnRolDtm>()
                    .ForMember(dtm => dtm.Rol, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDeRolesDeUnPermiso(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }


        //public static GestorDeRolesDeUnPermiso Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDeRolesDeUnPermiso(contexto, contexto.Mapeador));

        //public static GestorDeRolesDeUnPermiso Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}

        internal static GestorDeRolesDeUnPermiso Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRolesDeUnPermiso(contexto, mapeador);
        }

        protected override IQueryable<PermisosDeUnRolDtm> AplicarJoins(IQueryable<PermisosDeUnRolDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(rp => rp.Permiso);
            consulta = consulta.Include(rp => rp.Rol);
            return consulta;
        }

        protected override IQueryable<PermisosDeUnRolDtm> AplicarFiltros(IQueryable<PermisosDeUnRolDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnRolDtm.IdRol).ToLower())
                    consulta = consulta.Where(x => x.IdRol == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnRolDtm.IdPermiso).ToLower())
                    consulta = consulta.Where(x => x.IdPermiso == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPermisoDto.Rol).ToLower())
                    consulta = consulta.Where(x => x.Rol.Nombre.Contains(filtro.Valor));
            }

            return consulta;
        }

        protected override void DespuesDePersistir(PermisosDeUnRolDtm permisoDeUnRol, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(permisoDeUnRol, parametros);
            GestorDePermisosDeUnRol.SometerGenerarSeguridad(Contexto, permisoDeUnRol);
            GestorDePermisos.ActualizarCachesDePermisos();
        }
    }
}
