using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDeUsuariosDeUnPuesto : GestorDeRelaciones<ContextoSe, PuestosDeUnUsuarioDtm, UsuariosDeUnPuestoDto>
    {
        public class MapearUsuariosDeUnPuesto : Profile
        {
            public MapearUsuariosDeUnPuesto()
            {
                CreateMap<PuestosDeUnUsuarioDtm, UsuariosDeUnPuestoDto>()
                    .ForMember(dto => dto.Puesto, dtm => dtm.MapFrom(dtm => dtm.Puesto == null || dtm.Puesto.Cg == null ? null : $"{dtm.Puesto.Cg.Expresion} - {dtm.Puesto.Nombre}"))
                    .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario == null ? null : $"({dtm.Usuario.Login}) {dtm.Usuario.Apellido}, {dtm.Usuario.Nombre}"));

                CreateMap<UsuariosDeUnPuestoDto, PuestosDeUnUsuarioDtm>()
                    .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Puesto, dto => dto.Ignore());
            }
        }

        public GestorDeUsuariosDeUnPuesto(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        internal static GestorDeUsuariosDeUnPuesto Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUsuariosDeUnPuesto(contexto, mapeador);
        }

        protected override IQueryable<PuestosDeUnUsuarioDtm> AplicarJoins(IQueryable<PuestosDeUnUsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Usuario);
            consulta = consulta.Include(p => p.Puesto).ThenInclude(p => p.Cg);
            return consulta;
        }

        protected override IQueryable<PuestosDeUnUsuarioDtm> AplicarFiltros(IQueryable<PuestosDeUnUsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorNombreDeUsuario(filtros);
            return consulta;
        }

        protected override void AntesDePersistir(PuestosDeUnUsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Insertando) registro.ValidarQueSePuedeIncluirAlUsuarioEnPuesto(Contexto);
        }

        protected override void DespuesDePersistir(PuestosDeUnUsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            GestorDePermisos.LimpiarCachesDeUsuariosDeUnPuesto(Contexto, registro.IdUsuario);
            TrabajosDeEntorno.SometerGenerarSeguridadParaElUsuario(Contexto, registro.IdUsuario);
        }
    }
}

