using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;

namespace GestoresDeNegocio.Entorno
{

    public class GestorDePermisosDeUnUsuario : GestorDeElementos<ContextoSe, UsuariosDeUnPermisoDtm, PermisosDeUnUsuarioDto>
    {

        public class MapearPermisosDeUnUsuario : Profile
        {
            public MapearPermisosDeUnUsuario()
            {
                CreateMap<UsuariosDeUnPermisoDtm, PermisosDeUnUsuarioDto>()
                    .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario.Expresion))
                    .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));

                CreateMap<PermisosDeUnUsuarioDto, UsuariosDeUnPermisoDtm>();
            }
        }

        public GestorDePermisosDeUnUsuario(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }


        //public static GestorDePermisosDeUnUsuario Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDePermisosDeUnUsuario(contexto, contexto.Mapeador));

        //public static GestorDePermisosDeUnUsuario Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}

        public static GestorDePermisosDeUnUsuario Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosDeUnUsuario(contexto, mapeador);
        }

        protected override IQueryable<UsuariosDeUnPermisoDtm> AplicarJoins(IQueryable<UsuariosDeUnPermisoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(rp => rp.Permiso);
            consulta = consulta.Include(rp => rp.Usuario);

            return consulta;
        }

        protected override IQueryable<UsuariosDeUnPermisoDtm> AplicarFiltros(IQueryable<UsuariosDeUnPermisoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(UsuariosDeUnPermisoDtm.Permiso).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.contiene)
                {
                    consulta = consulta.Where(x => x.Permiso.Nombre.Contains(filtro.Valor));
                    filtro.Aplicado = true;
                }
                if (filtro.Clausula.ToLower() == nameof(UsuariosDeUnPermisoDtm.Permiso).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.esAlgunoDe)
                    consulta = consulta.AplicarFiltroPorEntero(filtro,nameof(UsuariosDeUnPermisoDtm.IdPermiso));
            }
           
            return consulta;

        }
    }
}
