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

    public class GestorDePermisosDeUnRol : GestorDeRelaciones<ContextoSe, PermisosDeUnRolDtm, PermisosDeUnRolDto>
    {

        public class MapearPermisosDeUnRol : Profile
        {
            public MapearPermisosDeUnRol()
            {
                CreateMap<PermisosDeUnRolDtm, PermisosDeUnRolDto>()
                    .ForMember(dto => dto.Rol, dtm => dtm.MapFrom(dtm => dtm.Rol.Nombre))
                    .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));

                CreateMap<PermisosDeUnRolDto, PermisosDeUnRolDtm>()
                    .ForMember(dtm => dtm.Rol, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDePermisosDeUnRol(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        //public static GestorDePermisosDeUnRol Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDePermisosDeUnRol(contexto, contexto.Mapeador));


        public static GestorDePermisosDeUnRol Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosDeUnRol(contexto, mapeador);
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

                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnRolDtm.Permiso).ToLower())
                    consulta = consulta.Where(x => x.Permiso.Nombre.Contains(filtro.Valor));
            }
            return consulta;

        }


        protected override void DespuesDePersistir(PermisosDeUnRolDtm permisoDeRol, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(permisoDeRol, parametros);

            SometerGenerarSeguridad(Contexto, permisoDeRol);
        }

        public static void SometerGenerarSeguridad(ContextoSe contexto, PermisosDeUnRolDtm permisoDeRol)
        {
            var roles = contexto.SeleccionarTodos<PermisosDeUnRolDtm>(nameof(permisoDeRol.IdPermiso), permisoDeRol.IdPermiso).Select(x => x.IdRol).ToList();
            if (roles.Count == 0) roles.Add(permisoDeRol.IdRol);
            var puestos = contexto.Set<RolesDeUnPuestoDtm>().Where(x => roles.Contains(x.IdRol)).Select(rpt => rpt.IdPuesto).Distinct().ToList();
            GestorDeRolesDeUnPuesto.SometerSeguridadParaLosPuestos(contexto, puestos);
        }
    }
}
