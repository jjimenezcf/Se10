using AutoMapper;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestorDeElementos;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Callejero;
using System;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeJuzgados : GestorDeElementos<ContextoSe, JuzgadoDtm, JuzgadoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Juzgado;

        public class ltrJuzgados
        {
        }

        public class MapearJuzgados : Profile
        {
            public MapearJuzgados()
            {
                CreateMap<JuzgadoDtm, JuzgadoDto>()
                .ForMember(dto => dto.Municipio, dtm => dtm.MapFrom(dtm => dtm.Municipio.Nombre))
                .ForMember(dto => dto.Clase, dtm => dtm.MapFrom(dtm => dtm.Clase.Nombre));
                CreateMap<JuzgadoDto, JuzgadoDtm>()
                .ForMember(dtm => dtm.Municipio, dto => dto.Ignore());
            }
        }

        public GestorDeJuzgados(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeJuzgados Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeJuzgados(contexto, mapeador);
        }


        public static JuzgadoDto CrearJuzgado(ContextoSe contexto, ClaseDeJuzgadoDto clase, string calificador, MunicipioDto municipio)
        {
            var nombre = $"{clase.Nombre} {calificador} de {municipio.Nombre}";
            var contacto = CrearJuzgado(contexto, nombre, clase.Id, municipio.Id, calificador);
            return contacto.MapearDto<JuzgadoDto>(contexto);
        }
        private static JuzgadoDtm CrearJuzgado(ContextoSe contexto, string nombre, int idClase, int idMunicipio, string Calificador)
        {
            var juzgado = contexto.SeleccionarPorPropiedad<JuzgadoDtm>(nameof(JuzgadoDtm.Nombre), nombre, errorSiNoHay: false);
            if (juzgado == null)
            {
                juzgado = new JuzgadoDtm();
                juzgado.Nombre = nombre;
                juzgado.IdClase = idClase;
                juzgado.IdMunicipio = idMunicipio;
                juzgado.Calificador = Calificador;
                juzgado = juzgado.Insertar(contexto);
            }
            return juzgado;
        }
        protected override IQueryable<JuzgadoDtm> AplicarJoins(IQueryable<JuzgadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Municipio);
            consulta = consulta.Include(p => p.Clase);
            return consulta;
        }
    }
}
