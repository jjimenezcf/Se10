using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using ModeloDeDto.Guarderias;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using Gestor.Errores;
using Utilidades;

namespace GestoresDeNegocio.Guarderias
{
    public class GestorDeAulasDeGuarderia : GestorDeElementos<ContextoSe, AulaDeGuarderiaDtm, AulaDeGuarderiaDto>
    {
        public class MapearPuestoDeTrabajo : Profile
        {
            public MapearPuestoDeTrabajo()
            {
                CreateMap<AulaDeGuarderiaDtm, AulaDeGuarderiaDto>()
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg != null ? dtm.Cg.Expresion : null));
                CreateMap<AulaDeGuarderiaDto, AulaDeGuarderiaDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore());
            }
        }

        public GestorDeAulasDeGuarderia(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAulasDeGuarderia Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAulasDeGuarderia(contexto, mapeador);
        }


        protected override IQueryable<AulaDeGuarderiaDtm> AplicarJoins(IQueryable<AulaDeGuarderiaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Cg);
            return consulta;
        }

        protected override IQueryable<AulaDeGuarderiaDtm> AplicarSeguridad(IQueryable<AulaDeGuarderiaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (Contexto.DatosDeConexion.EsAdministrador)
                return consulta;

            var clase = Contexto.Set<ClasePermisoDtm>().Where(tipo => tipo.Nombre == ClaseDePermiso.ToString(enumClaseDePermiso.Elemento));
            var permisos = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && clase.Any(c => c.Id == p.Permiso.Clase.Id));
            var esConsultorDeCgs = enumNegocio.CentroGestor.EsConsultor(Contexto);
            consulta = consulta.Where(aula => Contexto.Set<CentroGestorDtm>().Any(cg => cg.Id == aula.IdCg && 
                                                                  (permisos.Any(permiso => permiso.IdPermiso == cg.IdConsultor) || 
                                                                   permisos.Any(permiso => permiso.IdPermiso == cg.IdGestor) ||
                                                                   esConsultorDeCgs
                                                                   )));
            return consulta;
        }

        protected override void ValidarPermisosDePersistencia(AulaDeGuarderiaDtm aula, ParametrosDeNegocio parametros)
        {
            base.ValidarPermisosDePersistencia(aula, parametros);
            if (Contexto.DatosDeConexion.EsAdministrador)
                return;
            if (!aula.Cg(Contexto).EsGestor(Contexto))
                GestorDeErrores.Emitir($"Necesita los permisos de '{enumTipoPermiso.Gestor.Descripcion()}' para crear una aula en el CG '{aula.Cg(Contexto).Expresion}'");
        }

        protected override void AntesDePersistir(AulaDeGuarderiaDtm aula, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(aula, parametros);
            if (parametros.Eliminando)
            {
                if (Contexto.Existen<CursoDeGuarderiaDtm>(nameof(CursoDeGuarderiaDtm.IdAula), aula.Id))
                    GestorDeErrores.Emitir($"No puede eliminar el Aula '{aula.Nombre}' ya que tiene cursos asignados");
            }
        }

        protected override void DespuesDeMapearElElemento(AulaDeGuarderiaDtm aula, AulaDeGuarderiaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(aula, elemento, parametros);
            var cg = aula.Cg(Contexto);
            elemento.ModoDeAcceso = aula.Cg(Contexto).EsGestor(Contexto) ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
            elemento.Expresion = $"{cg.Expresion}: {aula.Nombre}";
        }

    }

}
