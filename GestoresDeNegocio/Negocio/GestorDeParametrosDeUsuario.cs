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

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeParametrosDeUsuario : GestorDeElementos<ContextoSe, ParametroDeUsuarioDtm, ParametroDeUsuarioDto>
    {
        public class MapearVariables : Profile
        {
            public MapearVariables()
            {
                CreateMap<ParametroDeUsuarioDtm, ParametroDeUsuarioDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre));

                CreateMap<ParametroDeUsuarioDto, ParametroDeUsuarioDtm>()
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDeParametrosDeUsuario(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDeParametrosDeUsuario Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeParametrosDeUsuario(contexto, mapeador);
        }

        protected override void AntesDePersistir(ParametroDeUsuarioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametro, parametros);

            if (parametro.PropiedadCambiada<string>(nameof(ParametroDeUsuarioDtm.Nombre), parametros))
                GestorDeErrores.Emitir($"El nombre del parámetro '{((ParametroDeUsuarioDtm)parametros.registroEnBd).Nombre}' no se puede cambiar por el usuario");
        }

        protected override void DespuesDePersistir(ParametroDeUsuarioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_ParametrosDeUsuario, $"{NegociosDeSe.ToEnumerado(parametro.IdNegocio)}-{Contexto.DatosDeConexion.IdUsuario}-{parametro.Nombre}");
        }

        protected override IQueryable<ParametroDeUsuarioDtm> AplicarJoins(IQueryable<ParametroDeUsuarioDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.Negocio);
            return registros;
        }

        protected override void DespuesDeMapearElElemento(ParametroDeUsuarioDtm parametro, ParametroDeUsuarioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(parametro, elemento, parametros);
        }

    }
}
