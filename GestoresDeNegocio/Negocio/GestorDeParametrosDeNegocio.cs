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
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeParametrosDeNegocio : GestorDeElementos<ContextoSe, ParametroDeNegocioDtm, ParametroDeNegocioDto>
    {
        public class MapearParametrosDeNegocio : Profile
        {
            public MapearParametrosDeNegocio()
            {
                CreateMap<ParametroDeNegocioDtm, ParametroDeNegocioDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre));

                CreateMap<ParametroDeNegocioDto, ParametroDeNegocioDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDeParametrosDeNegocio(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDeParametrosDeNegocio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeParametrosDeNegocio(contexto, mapeador);
        }

        protected override void AntesDePersistir(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametro, parametros);

            if (parametros.EsUnaPeticion && (parametros.Insertando || parametros.Eliminando))
            {
                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir($"No se puede crear y eliminar parámetros desde la interface");
            }

            if (parametros.EsUnaPeticion && parametros.Modificando && parametro.Nombre.ToLower() == enumParametrosDeFacturasEmt.FAE_SII_Activo.ToString().ToLower())
                GestorDeErrores.Emitir($"No se puede modificar el parámetro '{enumParametrosDeFacturasEmt.FAE_SII_Activo}' desde la interface, use la opción de menú en el mantenimiento de sociedades");

            if (parametros.EsUnaPeticion && parametros.Modificando && parametro.Nombre.ToLower() == enumParametrosDeFacturasEmt.FAE_SII_En_Productivo.ToString().ToLower())
                GestorDeErrores.Emitir($"No se puede modificar el parámetro '{enumParametrosDeFacturasEmt.FAE_SII_En_Productivo}' desde la interface, use la opción de menú en el mantenimiento de sociedades");

            if (parametro.PropiedadCambiada<string>(nameof(ParametroDeNegocioDtm.Nombre), parametros))
                GestorDeErrores.Emitir($"El nombre del parámetro '{((ParametroDeNegocioDtm)parametros.registroEnBd).Nombre}' no se puede cambiar por el usuario");
        }

        protected override void DespuesDePersistir(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_ParametrosDeNegocio, $"{NegociosDeSe.ToEnumerado(parametro.IdNegocio)}-{parametro.Nombre}");
            if (parametro.Nombre == enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil.ToString())
                ServicioDeCaches.EliminarCache(CacheDe.ListasDeNegocios);
            else if (parametro.Nombre == enumParametrosDeNegocio.IA_Prompt_Filtro.ToString())
                ServicioDeCaches.EliminarCache(CacheDe.Ia_Filtros);
        }

        protected override IQueryable<ParametroDeNegocioDtm> AplicarJoins(IQueryable<ParametroDeNegocioDtm> parametro, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametro = base.AplicarJoins(parametro, filtros, parametros);
            parametro = parametro.Include(p => p.Negocio);
            return parametro;
        }

        protected override void DespuesDeMapearElElemento(ParametroDeNegocioDtm parametro, ParametroDeNegocioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(parametro, elemento, parametros);

            var negocio = NegociosDeSe.ToEnumerado(parametro.IdNegocio);
            elemento.Descripcion = negocio.ObtenerDescripcionDelParametro(parametro.Nombre);
            if (elemento.Descripcion == string.Empty)
                elemento.Descripcion = negocio.ObtenerDescripcionDeEtapa(parametro.Nombre);
        }
    }
}
