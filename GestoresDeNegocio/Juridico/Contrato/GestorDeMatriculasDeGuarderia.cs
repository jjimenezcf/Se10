using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Humanizer;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeMatriculasDeGuarderia : GestorDeElementos<ContextoSe, MatriculaDeGuarderiaDtm, MatriculaDeGuarderiaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrMatriculasDeGuarderia
        {
        }

        public class MapearMatriculasDeGuarderia : Profile
        {
            public MapearMatriculasDeGuarderia()
            {
                CreateMap<MatriculaDeGuarderiaDtm, MatriculaDeGuarderiaDto>()
                    .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente == null ? null : dtm.Cliente.Expresion))
                    .ForMember(dto => dto.Infante, dtm => dtm.MapFrom(dtm => dtm.Infante == null ? null : dtm.Infante.Expresion))
                    .ForMember(dto => dto.Curso, dtm => dtm.MapFrom(dtm => dtm.Curso == null ? null : dtm.Curso.Expresion));
                CreateMap<MatriculaDeGuarderiaDto, MatriculaDeGuarderiaDtm>()
                    .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Infante, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Curso, dto => dto.Ignore());
            }
        }

        public GestorDeMatriculasDeGuarderia(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeMatriculasDeGuarderia Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeMatriculasDeGuarderia(contexto, mapeador);
        }

        protected override IQueryable<MatriculaDeGuarderiaDtm> AplicarJoins(IQueryable<MatriculaDeGuarderiaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cliente);
            consulta = consulta.Include(x => x.Curso);
            consulta = consulta.Include(x => x.Infante);
            return consulta;
        }

        protected override void AntesDePersistir(MatriculaDeGuarderiaDtm matricula, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(matricula, parametros);
        }

        protected override void DespuesDeMapearElElemento(MatriculaDeGuarderiaDtm matricula, MatriculaDeGuarderiaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(matricula, elemento, parametros);
            elemento.Cliente =  matricula.Cliente(Contexto)?.Expresion;
            elemento.Infante = matricula.Infante(Contexto)?.Expresion;
            elemento.Curso = matricula.Curso(Contexto)?.Expresion;
        }

    }
}
