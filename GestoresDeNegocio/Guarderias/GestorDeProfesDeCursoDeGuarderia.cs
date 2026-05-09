using AutoMapper;
using GestorDeElementos;
using ModeloDeDto.Guarderias;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using GestorDeElementos.Extensores;
using DocumentFormat.OpenXml.Vml;
using GestoresDeNegocio.Seguridad;

namespace GestoresDeNegocio.Guarderias
{
    public class GestorDeProfesDeCursoDeGuarderia: GestorDeElementos<ContextoSe, ProfeDeCursoDeGuarderiaDtm, ProfeDeCursoDeGuarderiaDto>
    {
        public class MapearProfesDeCursoDeGuarderia : Profile
        {
            public MapearProfesDeCursoDeGuarderia()
            {
                CreateMap<ProfeDeCursoDeGuarderiaDtm, ProfeDeCursoDeGuarderiaDto>()
                .ForMember(dto => dto.Trabajador, dtm => dtm.MapFrom(dtm => dtm.Trabajador != null ? dtm.Trabajador.Expresion : null));

                CreateMap<ProfeDeCursoDeGuarderiaDto, ProfeDeCursoDeGuarderiaDtm>()
                .ForMember(dtm => dtm.Trabajador, dto => dto.Ignore());
            }
        }

        public GestorDeProfesDeCursoDeGuarderia(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeProfesDeCursoDeGuarderia Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeProfesDeCursoDeGuarderia(contexto, mapeador);
        }

        protected override void DespuesDePersistir(ProfeDeCursoDeGuarderiaDtm profes, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(profes, parametros);
            if (parametros.Insertando && profes.Trabajador(Contexto).IdUsuario is not null)
            {
                var curso = profes.DetalleDe<CursoDeGuarderiaDtm>(Contexto);
                ExtensionDePuestos.AsociarUsuarioAlPt(Contexto, curso.PuestoDeConsultor(Contexto).Id, (int)profes.Trabajador(Contexto).IdUsuario);
            }

            if (parametros.Eliminando && profes.Trabajador(Contexto).IdUsuario is not null)
            {
                var curso = profes.DetalleDe<CursoDeGuarderiaDtm>(Contexto);
                ExtensionDePuestos.DesasociarUsuarioAlPt(Contexto, curso.PuestoDeConsultor(Contexto).Id, (int)profes.Trabajador(Contexto).IdUsuario);
            }
        }

        protected override void DespuesDeMapearElElemento(ProfeDeCursoDeGuarderiaDtm profe, ProfeDeCursoDeGuarderiaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(profe, elemento, parametros);
            elemento.Trabajador = profe.Trabajador(Contexto).Expresion;
        }

    }
}
