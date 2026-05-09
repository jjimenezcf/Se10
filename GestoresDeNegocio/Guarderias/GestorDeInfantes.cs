using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using ModeloDeDto.Guarderias;
using Utilidades;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Negocio;

namespace GestoresDeNegocio.Guarderias
{
    public class GestorDeInfantes : GestorDeElementos<ContextoSe, InfanteDtm, InfanteDto>
    {
        public override enumNegocio Negocio => enumNegocio.Infante;
        public class MapearInfante : Profile
        {
            public MapearInfante()
            {
                CreateMap<InfanteDtm, InfanteDto>()
                .ForMember(dto => dto.Contacto, dtm => dtm.MapFrom(dtm => dtm.Contacto != null ? dtm.Contacto.Expresion : null))
                .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(dtm => dtm.Padre != null ? dtm.Padre.Expresion : null))
                .ForMember(dto => dto.Madre, dtm => dtm.MapFrom(dtm => dtm.Madre != null ? dtm.Madre.Expresion : null));
                CreateMap<InfanteDto, InfanteDtm>()
                .ForMember(dtm => dtm.Archivo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contacto, dto => dto.Ignore())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
                .ForMember(dtm => dtm.Agenda, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdAgenda, dto => dto.Ignore())
                .ForMember(dtm => dtm.Madre, dto => dto.Ignore());
            }
        }

        public GestorDeInfantes(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeInfantes Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeInfantes(contexto, mapeador);
        }

        protected override IQueryable<InfanteDtm> AplicarJoins(IQueryable<InfanteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Contacto);
            consulta = consulta.Include(p => p.Madre);
            consulta = consulta.Include(p => p.Padre);
            return consulta;
        }

        protected override IQueryable<InfanteDtm> AplicarFiltros(IQueryable<InfanteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorInfanteSinCurso(Contexto, filtros);
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(InfanteDtm infante, InfanteDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(infante, elemento, parametros);
            var curso = infante.CursoEnElQueEsta(Contexto);

            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var agenda = infante.Agenda(Contexto);
                var esGestor = ApiDePermisos.TieneAlgunPermiso(Contexto, new List<int> { agenda.IdGestor, agenda.IdInterventor });
                elemento.IdAgenda = esGestor ? agenda.Id : null;
                elemento.Agenda = esGestor ? agenda.Nombre : null;
            }

            if (curso is not null)
            {
                elemento.Curso = curso.Expresion;
                elemento.IdCurso = curso.Id;
            }
        }

        protected override void AntesDePersistir(InfanteDtm infante, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(infante, parametros);
            if (parametros.Insertando)
            {
                CrearAgendaDelInfante(infante);
            }
            else
            {
                infante.IdAgenda = ((InfanteDtm)parametros.registroEnBd).IdAgenda;
            }
        }

        protected override void DespuesDePersistir(InfanteDtm infante, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(infante, parametros);
            if (infante.PropiedadCambiada<string>(nameof(InfanteDtm.Nombre), parametros))
            {
                CambiarNombreDeAgenda(infante);
            }
        }

        protected override void AlDarDeBaja(InfanteDtm infante, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(infante, parametros);

            var filtros = new Dictionary<string, object> { { nameof(MatriculaDeGuarderiaDtm.IdInfante), infante.Id } };
            var matriculas = Contexto.SeleccionarTodos<MatriculaDeGuarderiaDtm>(filtros);
            foreach (var matricula in matriculas)
            {
                var contrato = matricula.AmpliacionDe<ContratoDtm>(Contexto);
                if (contrato.EstaCancelado() || contrato.EstaTerminado())
                    continue;
                GestorDeErrores.Emitir($"El niño/a '{infante.Nombre}' está asociado a la matrícula '{contrato.Expresion}', cancélela primero");
            }

            var curso = infante.CursoEnElQueEsta(Contexto);
            if (curso != null)
            {
                GestorDeErrores.Emitir($"El niño/a '{infante.Nombre}' está asignado en el curso '{curso.Nombre}', quítelo primero del curso");
            }
        }

        private void CrearAgendaDelInfante(InfanteDtm infante)
        {
            var agenda = new AgendaDtm() { Nombre = infante.NombreDeAgenda }.InsertarComoAdministrador(Contexto);
            infante.IdAgenda = agenda.Id;
            infante.Agenda = agenda;
            infante.ActualizarGestorDeAgendas(Contexto);
        }

        private void CambiarNombreDeAgenda(InfanteDtm infante)
        {
            infante.Agenda(Contexto).Nombre = infante.NombreDeAgenda;
            infante.Agenda(Contexto).ModificarComoAdministrador(Contexto);
            infante.ActualizarGestorDeAgendas(Contexto);
        }

        public List<DireccionDto> LeerDireccionesDeFamiliares(int idInfante)
        {
            var infante = Contexto.SeleccionarPorId<InfanteDtm>(idInfante);
            var direcConta = infante.Contacto(Contexto).Direcciones(Contexto);
            var direcMama = infante.Madre(Contexto)?.Direcciones(Contexto);
            var direcPapa = infante.Padre(Contexto)?.Direcciones(Contexto);

            if (direcMama is not null) foreach (var item in direcMama)
                    if (direcConta.Buscar(item, comparaCalificador: false) == null)
                        direcConta.Add(item);
            if (direcPapa is not null) foreach (var item in direcPapa)
                    if (direcConta.Buscar(item, comparaCalificador: false) == null)
                        direcConta.Add(item);

            var direcciones = GestorDeDirecciones.Gestor(Contexto, enumNegocio.Infante)
                .MapearElementos(direcConta, new ParametrosDeNegocio(enumTipoOperacion.MapearElDtoAlDtm)
                {
                    Parametros = new Dictionary<string, object> { { ltrParametrosNeg.EnConsulta, true } }
                });
            var direcDto = direcciones.OrderBy(dto => dto.NombreDireccion).ToList();

            return direcDto;
        }
        public List<InfanteDto> LeerInfantesTutelados(int idNegocio, int idElemento)
        {
            var filtro = NegociosDeSe.ToEnumerado(idNegocio) == enumNegocio.Persona ?
                new ClausulaDeFiltrado
                {
                    Clausula = $"{nameof(InfanteDtm.IdMadre)}{Simbolos.Or}{nameof(InfanteDtm.IdPadre)}",
                    Criterio = enumCriteriosDeFiltrado.porDiferentesPropiedades,
                    Valor = idElemento.ToString()
                }
                : new ClausulaDeFiltrado
                {
                    Clausula = $"{nameof(InfanteDtm.IdContacto)}",
                    Criterio = enumCriteriosDeFiltrado.igual,
                    Valor = idElemento.ToString()
                };
            var infantes = LeerElementos(0, -1, new List<ClausulaDeFiltrado> { filtro}, new List<ClausulaDeOrdenacion>(), new Dictionary<string, object>());

            return infantes.ToList();
        }

        public static void PuedenCambiarseDeCurso(ContextoSe contexto, List<int> idsDeInfantes)
        {
            var infantes = contexto.Set<InfanteDtm>().Where(infante => idsDeInfantes.Contains(infante.Id));
            foreach (var infante in infantes)
            {
                if (infante.Baja)
                    GestorDeErrores.Emitir($"El niño/a '{infante.Nombre}' no se le puede incluir en un curso, por estar de baja");
            }
        }

        public static CursoDeGuarderiaDtm AsociarCurso(ContextoSe contexto, int idInfante, int idCurso)
        {
            var infante = contexto.SeleccionarPorId<InfanteDtm>(idInfante);
            var cursoActual = infante.CursoEnElQueEsta(contexto);
            var cursoNuevo = contexto.SeleccionarPorId<CursoDeGuarderiaDtm>(idCurso);
            var filtros = new Dictionary<string, object> { { nameof(MatriculaDeGuarderiaDtm.IdInfante), infante.Id } };
            if (cursoActual is not null)
            {
                var cursoDelInfante = cursoActual.Detalles<InfanteDeUnCursoDtm>(contexto, aplicarJoin: true).First();
                cursoActual.CrearTraza(contexto, "Niño/a cambiado de curso", $"El usuario '{contexto.DatosDeConexion.Login}' ha cambiado al niño/a '{infante.Nombre}' al curso '{cursoNuevo.Nombre}'");
                infante.CrearTraza(contexto, "Curso cambiado", $"El usuario '{contexto.DatosDeConexion.Login}' ha cambiado del curso '{cursoActual.Nombre}' al curso '{cursoNuevo.Nombre}'");
                cursoDelInfante.EliminarComoAdministrador(contexto);
                filtros.Add(nameof(MatriculaDeGuarderiaDtm.IdCurso), cursoActual.Id);
            }
            var cursoasociado = new InfanteDeUnCursoDtm { IdElemento = cursoNuevo.Id, IdInfante = infante.Id }.InsertarComoAdministrador(contexto);

            var matriculas = contexto.SeleccionarTodos<MatriculaDeGuarderiaDtm>(filtros);
            foreach (var matricula in matriculas)
            {
                var contrato = matricula.AmpliacionDe<ContratoDtm>(contexto);
                if (contrato.EstaCancelado() || contrato.EstaTerminado())
                    continue;

                contrato.CrearTraza(contexto, $"{(cursoActual is null ? "Asociación del curso " : "Cambio de curso ")}al niño/a", cursoActual is null
                    ? $"El usuario '{contexto.DatosDeConexion.Login}' ha asignado el curso '{cursoNuevo.Nombre}'"
                    : $"El usuario '{contexto.DatosDeConexion.Login}' ha cambiado del curso '{cursoActual.Nombre}' al curso '{cursoNuevo.Nombre}'");
                matricula.IdCurso = cursoNuevo.Id;
                matricula.ModificarComoAdministrador(contexto);

            }

            return cursoNuevo;
        }

    }

}
