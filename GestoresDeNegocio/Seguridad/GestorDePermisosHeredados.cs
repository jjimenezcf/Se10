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

    public class GestorDePermisosHeredados : GestorDeRelaciones<ContextoSe, PermisosHeredadosDtm, PermisosDeUnPuestoDto>
    {

        public class MapearVistaPermisosDeUnPuesto : Profile
        {
            public MapearVistaPermisosDeUnPuesto()
            {
                CreateMap<PermisosHeredadosDtm, PermisosDeUnPuestoDto>()
                    .ForMember(dto => dto.CgDelPuesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Cg.Expresion))
                    .ForMember(dto => dto.Puesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Nombre))
                    .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));

                CreateMap<PermisosDeUnPuestoDto, PermisosHeredadosDtm>();
            }
        }

        public GestorDePermisosHeredados(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }

        //public static GestorDePermisosHeredados Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDePermisosHeredados(contexto, contexto.Mapeador));

        //public static GestorDePermisosHeredados Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}

        internal static GestorDePermisosHeredados Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosHeredados(contexto, mapeador);
        }

        protected override IQueryable<PermisosHeredadosDtm> AplicarJoins(IQueryable<PermisosHeredadosDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(rp => rp.Permiso);
            consulta = consulta.Include(rp => rp.Puesto);
            consulta = consulta.Include(rp => rp.Puesto.Cg);
            return consulta;
        }

        protected override IQueryable<PermisosHeredadosDtm> AplicarFiltros(IQueryable<PermisosHeredadosDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(PermisosHeredadosDtm.IdPuesto).ToLower())
                    consulta = consulta.Where(x => x.IdPuesto == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(PermisosHeredadosDtm.IdPermiso).ToLower())
                    consulta = consulta.Where(x => x.IdPermiso == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(PermisosHeredadosDtm.Permiso).ToLower())
                    consulta = consulta.Where(x => x.Permiso.Nombre.Contains(filtro.Valor));
            }
            return consulta;

        }

    }
}
