using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using ModeloDeDto.Guarderias;
using GestorDeElementos.Extensores;
using System;
using Utilidades;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using static ServicioDeDatos.Elemento.Enumerados;
using Gestor.Errores;
using ServicioDeDatos.Terceros;

namespace GestoresDeNegocio.Guarderias
{
    public class GestorDeCursosDeGuarderia : GestorDeElementos<ContextoSe, CursoDeGuarderiaDtm, CursoDeGuarderiaDto>
    {
        public override enumNegocio Negocio => enumNegocio.CursoDeGuarderia;
        public class MapearPuestoDeTrabajo : Profile
        {
            public MapearPuestoDeTrabajo()
            {
                CreateMap<CursoDeGuarderiaDtm, CursoDeGuarderiaDto>()
                .ForMember(dto => dto.Trabajador, dtm => dtm.MapFrom(dtm => dtm.Trabajador != null ? dtm.Trabajador.Expresion : null))
                .ForMember(dto => dto.Consultor, dtm => dtm.MapFrom(dtm => dtm.Consultor != null ? dtm.Consultor.Expresion : null))
                .ForMember(dto => dto.Gestor, dtm => dtm.MapFrom(dtm => dtm.Gestor != null ? dtm.Gestor.Expresion : null))
                .ForMember(dto => dto.Aula, dtm => dtm.MapFrom(dtm => dtm.Aula != null ? dtm.Aula.Expresion : null));
                CreateMap<CursoDeGuarderiaDto, CursoDeGuarderiaDtm>()
                .ForMember(dtm => dtm.Trabajador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Consultor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Gestor, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdAgenda, dto => dto.Ignore())
                .ForMember(dtm => dtm.Agenda, dto => dto.Ignore())
                .ForMember(dtm => dtm.Aula, dto => dto.Ignore());
            }
        }

        public GestorDeCursosDeGuarderia(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCursosDeGuarderia Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCursosDeGuarderia(contexto, mapeador);
        }

        protected override IQueryable<CursoDeGuarderiaDtm> AplicarJoins(IQueryable<CursoDeGuarderiaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Trabajador);
            consulta = consulta.Include(p => p.Aula);
            consulta = consulta.Include(p => p.Consultor).ThenInclude(x => x.Cg);
            consulta = consulta.Include(p => p.Gestor).ThenInclude(x => x.Cg);
            return consulta;
        }

        protected override IQueryable<CursoDeGuarderiaDtm> AplicarFiltros(IQueryable<CursoDeGuarderiaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorCursosActivos(Contexto, filtros);
            consulta = consulta.FiltrarPorInfante(Contexto, filtros);
            consulta = consulta.FiltrarPorPeriodo(Contexto, filtros);
            consulta = consulta.FiltrarParaAsociarCurso(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<CursoDeGuarderiaDtm> AplicarSeguridad(IQueryable<CursoDeGuarderiaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (Contexto.DatosDeConexion.EsAdministrador)
                return consulta;

            var clase = Contexto.Set<ClasePermisoDtm>().Where(tipo => tipo.Nombre == ClaseDePermiso.ToString(enumClaseDePermiso.Elemento));
            var permisos = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && clase.Any(c => c.Id == p.Permiso.Clase.Id));
            var esConsultorDeCgs = enumNegocio.CentroGestor.EsConsultor(Contexto);
            consulta = consulta.Where(curso => Contexto.Set<CentroGestorDtm>().Any(cg => cg.Id == curso.Aula.IdCg &&
                                                                  (permisos.Any(permiso => permiso.IdPermiso == cg.IdConsultor) ||
                                                                   permisos.Any(permiso => permiso.IdPermiso == cg.IdGestor) ||
                                                                   esConsultorDeCgs
                                                                   )));
            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(CursoDeGuarderiaDto elemento, CursoDeGuarderiaDtm curso, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, curso, opciones);
            if (opciones.Insertando)
            {
                curso.Nombre = $"Periodo: {curso.Inicio.Year}-{curso.Fin.Year}. " +
                               $"{curso.Aula(Contexto).Cg(Contexto).Sigla}: {curso.Aula(Contexto).Nombre}. " +
                               $"Edad de: {elemento.Edad} " +
                               $"{ApiDeEnsamblados.ToEnumerado<enumMedidoEn_AM>(elemento.MedidoEn).Descripcion()}";
            }
            else
            if (opciones.Modificando)
            {
                var cursos = Contexto.SeleccionarTodos<CursoDeGuarderiaDtm>(nameof(curso.Nombre), curso.Nombre);
                if (cursos.Count == 1 && cursos[0].Id == curso.Id) 
                    return;
                if (cursos.Count == 0)
                    return;
                curso.Nombre = curso.Nombre.Replace($" ({cursos.Count})", "");
                curso.Nombre = curso.Nombre + $" ({cursos.Count + 1})";
            }
        }

        protected override void ValidarPermisosDePersistencia(CursoDeGuarderiaDtm curso, ParametrosDeNegocio parametros)
        {
            base.ValidarPermisosDePersistencia(curso, parametros);
            if (Contexto.DatosDeConexion.EsAdministrador)
                return;
            if (!curso.Aula(Contexto).Cg(Contexto).EsGestor(Contexto))
                GestorDeErrores.Emitir($"Necesita los permisos de '{enumTipoPermiso.Gestor.Descripcion()}' para crear una curso en un aula del CG '{curso.Aula(Contexto).Cg(Contexto).Expresion}'");
        }

        protected override void AntesDePersistir(CursoDeGuarderiaDtm curso, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(curso, parametros);
            ValiadarPeriodo(curso);
            if (parametros.Insertando)
            {
                CrearAgendaDelCurso(curso);
                CrearPuestosDeTrabajo(curso);
                curso.Agenda.AsignarPermisoAlPuesto(Contexto, curso.IdConsultor, enumModoDeAccesoDeDatos.Consultor);
                curso.Agenda.AsignarPermisoAlPuesto(Contexto, curso.IdGestor, enumModoDeAccesoDeDatos.Gestor);
            }
            else
            {
                curso.IdAgenda = ((CursoDeGuarderiaDtm)parametros.registroEnBd).IdAgenda;
                curso.IdGestor = ((CursoDeGuarderiaDtm)parametros.registroEnBd).IdGestor;
                curso.IdConsultor = ((CursoDeGuarderiaDtm)parametros.registroEnBd).IdConsultor;
            }
        }

        protected override void DespuesDePersistir(CursoDeGuarderiaDtm curso, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(curso, parametros);
            if (curso.PropiedadCambiada<string>(nameof(CursoDeGuarderiaDtm.Nombre), parametros))
            {
                CambiarNombreDeAgenda(curso);
                CambiarNombreDePts(curso);
            }
            var distintoTrabajador = curso.PropiedadCambiada<int>(nameof(CursoDeGuarderiaDtm.IdTrabajador), parametros);
            if (distintoTrabajador)
            {
                var anteriorTrabajador = ((CursoDeGuarderiaDtm)parametros.registroEnBd).Trabajador(Contexto);
                if (anteriorTrabajador.IdUsuario is not null) curso.Gestor(Contexto).DesasociarUsuario(Contexto, (int)anteriorTrabajador.IdUsuario);
            }
            if (distintoTrabajador || parametros.Insertando)
            {
                var trabajador = curso.Trabajador(Contexto);
                if (trabajador.IdUsuario is not null) curso.Gestor(Contexto).AsociarUsuario(Contexto, (int)trabajador.IdUsuario);
            }
        }

        protected override void DespuesDeMapearElElemento(CursoDeGuarderiaDtm curso, CursoDeGuarderiaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(curso, elemento, parametros);
            elemento.IdCg = curso.Aula(Contexto).IdCg;
            elemento.IdSociedadDelCg = curso.Aula(Contexto).Cg(Contexto).IdSociedad;

            elemento.Gestor = curso.Gestor(Contexto).Expresion;
            elemento.Consultor = curso.Consultor(Contexto).Expresion;
            elemento.Trabajador = curso.Trabajador(Contexto).Expresion;

            DateTime hoy = DateTime.Today;
            elemento.Activo = hoy >= curso.Inicio.Date && hoy <= curso.Fin.Date;
            elemento.ModoDeAcceso = (curso.Trabajador.IdUsuario is not null && (int)curso.Trabajador.IdUsuario == Contexto.DatosDeConexion.IdUsuario) || curso.Aula(Contexto).Cg(Contexto).EsGestor(Contexto)
            ? enumModoDeAccesoDeDatos.Gestor
            : enumModoDeAccesoDeDatos.Consultor;

            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var agenda = curso.Agenda(Contexto);
                var esGestor = ApiDePermisos.TieneAlgunPermiso(Contexto, new List<int> { agenda.IdGestor, agenda.IdInterventor });
                elemento.IdAgenda = esGestor ? agenda.Id : null;
                elemento.Agenda = esGestor ? agenda.Nombre : null;
            }
        }

        private void CambiarNombreDeAgenda(CursoDeGuarderiaDtm curso)
        {
            var agenda = Contexto.SeleccionarPorId<AgendaDtm>(curso.IdAgenda);
            agenda.Nombre = curso.NombreDeAgenda;
            agenda.ModificarComoAdministrador(Contexto);
        }


        private void CambiarNombreDePts(CursoDeGuarderiaDtm curso)
        {
            var consultor = Contexto.SeleccionarPorId<PuestoDtm>(curso.IdConsultor);
            consultor.Nombre = curso.NombrePt(enumModoDeAccesoDeDatos.Consultor);
            consultor.ModificarComoAdministrador(Contexto);

            var gestor = Contexto.SeleccionarPorId<PuestoDtm>(curso.IdGestor);
            gestor.Nombre = curso.NombrePt(enumModoDeAccesoDeDatos.Gestor);
            gestor.ModificarComoAdministrador(Contexto);
        }

        private void CrearAgendaDelCurso(CursoDeGuarderiaDtm curso)
        {
            var agenda = new AgendaDtm() { Nombre = curso.NombreDeAgenda }.InsertarComoAdministrador(Contexto);
            curso.IdAgenda = agenda.Id;
            curso.Agenda = agenda;
        }

        private void CrearPuestosDeTrabajo(CursoDeGuarderiaDtm curso)
        {
            curso.Consultor = CrearPuesto(curso, enumModoDeAccesoDeDatos.Consultor);
            curso.Gestor = CrearPuesto(curso, enumModoDeAccesoDeDatos.Gestor);

            curso.IdGestor = curso.Gestor.Id;
            curso.IdConsultor = curso.Consultor.Id;
        }

        private PuestoDtm CrearPuesto(CursoDeGuarderiaDtm curso, enumModoDeAccesoDeDatos modo)
        {
            var puesto = new PuestoDtm
            {
                IdCg = ExtensorDeGuarderias.CgParaPtsDeCursosDeGuarderia(Contexto).Id,
                Nombre = curso.NombrePt(modo)
            }
            .InsertarComoAdministradorSiNoExiste(Contexto, new List<string> { nameof(CursoDeGuarderiaDtm.Nombre) });

            if (modo == enumModoDeAccesoDeDatos.Gestor)
                curso.IdGestor = puesto.Id;
            else
                curso.IdConsultor = puesto.Id;

            return puesto;
        }

        private void ValiadarPeriodo(CursoDeGuarderiaDtm curso)
        {
            var cursosActivos = Contexto.SeleccionarTodos<CursoDeGuarderiaDtm>(new List<ClausulaDeFiltrado> {
                new ClausulaDeFiltrado(nameof(CursoDeGuarderiaDtm.IdAula), enumCriteriosDeFiltrado.igual, curso.IdAula),
                new ClausulaDeFiltrado(ltrDeCursosDeGuarderia.FiltrarPorActivo, enumCriteriosDeFiltrado.igual, true),
                new ClausulaDeFiltrado(ltrDeCursosDeGuarderia.FiltrarPorPeriodo, enumCriteriosDeFiltrado.contiene, $"{curso.Inicio}{Simbolos.separadorDeFechas}{curso.Fin}")
            });

            if (cursosActivos.Count > 0)
                GestorDeErrores.Emitir($"El curso '{curso.Expresion}' está activo en el aula '{curso.Aula(Contexto).Expresion}' para el periodo '{curso.Inicio.Date.ToString("dd/MM/yyyy")}'-'{curso.Fin.Date.ToString("dd/MM/yyyy")}' ");
        }

    }

}
