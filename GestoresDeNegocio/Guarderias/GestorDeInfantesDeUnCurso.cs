using AutoMapper;
using GestorDeElementos;
using ModeloDeDto.Guarderias;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Seguridad;
using Gestor.Errores;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Utilidades;

namespace GestoresDeNegocio.Guarderias
{
    public class GestorDeInfantesDeUnCurso : GestorDeRelaciones<ContextoSe, InfanteDeUnCursoDtm, InfanteDeUnCursoDto>
    {
        public class MapearInfantesDeUnCurso : Profile
        {
            public MapearInfantesDeUnCurso()
            {
                CreateMap<InfanteDeUnCursoDtm, InfanteDeUnCursoDto>()
                .ForMember(dto => dto.Elemento, dtm => dtm.MapFrom(dtm => dtm.Elemento != null ? dtm.Elemento.Expresion : null))
                .ForMember(dto => dto.Infante, dtm => dtm.MapFrom(dtm => dtm.Infante != null ? dtm.Infante.Expresion : null));

                CreateMap<InfanteDeUnCursoDto, InfanteDeUnCursoDtm>()
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore())
                .ForMember(dtm => dtm.Infante, dto => dto.Ignore());
            }
        }

        public GestorDeInfantesDeUnCurso(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeInfantesDeUnCurso Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeInfantesDeUnCurso(contexto, mapeador);
        }

        protected override IQueryable<InfanteDeUnCursoDtm> AplicarJoins(IQueryable<InfanteDeUnCursoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Infante);
            return consulta;
        }

        protected override IQueryable<InfanteDeUnCursoDtm> AplicarFiltros(IQueryable<InfanteDeUnCursoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override IQueryable<InfanteDeUnCursoDtm> AplicarOrden(IQueryable<InfanteDeUnCursoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override void ValidarPermisosDePersistencia(InfanteDeUnCursoDtm infanteDElCurso, ParametrosDeNegocio parametros)
        {
            base.ValidarPermisosDePersistencia(infanteDElCurso, parametros);
            if (!infanteDElCurso.Infante(Contexto).EsGestor(Contexto))
            {
                GestorDeErrores.Emitir($"Para incluir a '{infanteDElCurso.Infante(Contexto).Nombre}' en el curso '{infanteDElCurso.Curso(Contexto).Nombre}', debe ser gestor de '{enumNegocio.Infante.Singular()}'");
            }
        }

        protected override void AntesDePersistir(InfanteDeUnCursoDtm infanteDeUnCurso, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(infanteDeUnCurso, parametros);
            if (parametros.Insertando)
            {
                var curso = ExtensorDeGuarderias.CursoEnElQueEsta(Contexto, infanteDeUnCurso.IdInfante);
                if (curso != null)
                {
                    var infante = infanteDeUnCurso.Infante(Contexto);
                    GestorDeErrores.Emitir($"El niño/a '{infante.Nombre}' ya está matriculado en el curso '{curso.Nombre}', eliminelo primero del curso");
                }
            }
        }

        protected override void DespuesDePersistir(InfanteDeUnCursoDtm infanteDeUnCurso, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(infanteDeUnCurso, parametros);

            if (parametros.Insertando)
            {
                var curso = infanteDeUnCurso.DetalleDe<CursoDeGuarderiaDtm>(Contexto, aplicarJoin: true);
                infanteDeUnCurso.Infante(Contexto).Agenda(Contexto).AsignarPermisoAlPuesto(Contexto, curso.IdConsultor, enumModoDeAccesoDeDatos.Consultor);
                infanteDeUnCurso.Infante(Contexto).Agenda(Contexto).AsignarPermisoAlPuesto(Contexto, curso.IdGestor, enumModoDeAccesoDeDatos.Gestor);
            }
            else if (parametros.Eliminando)
            {
                var curso = infanteDeUnCurso.DetalleDe<CursoDeGuarderiaDtm>(Contexto);
                infanteDeUnCurso.Infante(Contexto).Agenda(Contexto).DesasignarPermisoAlPuesto(Contexto, curso.IdConsultor, enumModoDeAccesoDeDatos.Consultor);
                infanteDeUnCurso.Infante(Contexto).Agenda(Contexto).DesasignarPermisoAlPuesto(Contexto, curso.IdGestor, enumModoDeAccesoDeDatos.Gestor);
            }
        }

        protected override void DespuesDeMapearElElemento(InfanteDeUnCursoDtm infante, InfanteDeUnCursoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(infante, elemento, parametros);
            elemento.Infante = infante.Infante(Contexto).Expresion;
        }

    }
}
