using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Negocio;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeParametrosVistaPorUsuario : GestorDeElementos<ContextoSe, ParametroVistaPorUsuarioDtm, ParametroVistaPorUsuarioDto>
    {
        public class MapearVariables : Profile
        {
            public MapearVariables()
            {
                CreateMap<ParametroVistaPorUsuarioDtm, ParametroVistaPorUsuarioDto>()
                .ForMember(dto => dto.Vista, dtm => dtm.MapFrom(dtm => dtm.Vista.Nombre));

                CreateMap<ParametroDeUsuarioDto, ParametroDeUsuarioDtm>()
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDeParametrosVistaPorUsuario(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDeParametrosVistaPorUsuario Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeParametrosVistaPorUsuario(contexto, mapeador);
        }

        protected override void AntesDePersistir(ParametroVistaPorUsuarioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametro, parametros);

            if (parametro.PropiedadCambiada<string>(nameof(ParametroVistaPorUsuarioDtm.Nombre), parametros))
                GestorDeErrores.Emitir($"El nombre del parámetro '{((ParametroDeUsuarioDtm)parametros.registroEnBd).Nombre}' no se puede cambiar por el usuario");
        }

        protected override void DespuesDePersistir(ParametroVistaPorUsuarioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametro, parametros);
        }

        protected override IQueryable<ParametroVistaPorUsuarioDtm> AplicarJoins(IQueryable<ParametroVistaPorUsuarioDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.Vista);
            return registros;
        }

    }
}
