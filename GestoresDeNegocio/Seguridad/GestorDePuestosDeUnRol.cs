using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{
    public class GestorDePuestosDeUnRol : GestorDeRelaciones<ContextoSe, RolesDeUnPuestoDtm, PuestosDeUnRolDto>
    {

        public class MapearPuestosDeUnRol : Profile
        {
            public MapearPuestosDeUnRol()
            {
                CreateMap<RolesDeUnPuestoDtm, PuestosDeUnRolDto>()
                    .ForMember(dto => dto.CgDelPuesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Cg.Expresion))
                    .ForMember(dto => dto.Puesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Nombre))
                    .ForMember(dto => dto.Rol, dtm => dtm.MapFrom(dtm => dtm.Rol.Nombre));

                CreateMap<PuestosDeUnRolDto, RolesDeUnPuestoDtm>()
                    .ForMember(dtm => dtm.Rol, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Puesto, dto => dto.Ignore());
            }
        }

        public GestorDePuestosDeUnRol(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        //public static GestorDePuestosDeUnUsuario Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDePuestosDeUnUsuario(contexto, contexto.Mapeador));

        //public static GestorDePuestosDeUnUsuario Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}

        internal static GestorDePuestosDeUnUsuario Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePuestosDeUnUsuario(contexto, mapeador);
        }

        protected override IQueryable<RolesDeUnPuestoDtm> AplicarJoins(IQueryable<RolesDeUnPuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Rol);
            consulta = consulta.Include(p => p.Puesto);
            consulta = consulta.Include(p => p.Puesto.Cg);
            return consulta;
        }

        protected override IQueryable<RolesDeUnPuestoDtm> AplicarFiltros(IQueryable<RolesDeUnPuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDtm.IdPuesto).ToLower())
                    consulta = consulta.Where(x => x.IdPuesto == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDtm.IdRol).ToLower())
                    consulta = consulta.Where(x => x.IdRol == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(PuestosDeUnRolDto.Puesto).ToLower())
                    consulta = consulta.Where(x => x.Puesto.Nombre.Contains(filtro.Valor));
            }

            return consulta;
        }

        protected override void DespuesDePersistir(RolesDeUnPuestoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            GestorDeRolesDeUnPuesto.SometerGenerarSegurida(Contexto, registro);
        }

    }
}

