using System;
using System.Collections.Generic;
using System.Linq;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace GestorDeElementos
{
    public static class ApiParaDtos
    {
        public static E SeleccionarDto<E>(this ContextoSe contexto, int id, bool errorSiNoHay = true, ParametrosDeNegocio parametros = null)
        where E : ElementoDto
        {
            var negocio = NegociosDeSe.LeerNegocioPorDto(typeof(E));
            return (E) NegociosDeSe.ToEnumerado(negocio.Id).LeerElemento(contexto, id, errorSiNoHay: errorSiNoHay,  parametros: parametros == null ? null : parametros.Parametros);
        }

        public static E SeleccionarDto<E, R>(this ContextoSe contexto, int id, bool errorSiNoHay = true, ParametrosDeNegocio parametros = null)
        where E : ElementoDto
        where R : RegistroDtm
        {
            var registro = contexto.SeleccionarPorPropiedad<R>(nameof(IRegistro.Id), id.ToString(), errorSiNoHay, errorSiMasDeuno: true, aplicarJoin: true);
            return registro.MapearDto<E, R>(contexto, parametros);
        }

        public static E SeleccionarDtoPorAk<E, R>(this ContextoSe contexto, string ak, object valor, bool errorSiNoHay = true, ParametrosDeNegocio parametros = null)
        where E : ElementoDto
        where R : RegistroDtm
        {
            var registro = contexto.SeleccionarPorPropiedad<R>(ak, valor.ToString(), errorSiNoHay, errorSiMasDeuno: true, aplicarJoin: true);
            return registro == null ? null : registro.MapearDto<E, R>(contexto, parametros);
        }

        public static List<E> SeleccionarDtos<E, R>(this ContextoSe contexto, Dictionary<string, object> filtros, ParametrosDeNegocio parametros = null)
        where E : ElementoDto
        where R : RegistroDtm
        {
            var elementos = new List<E>();
            var registros = contexto.SeleccionarTodos<R>(filtros, aplicarJoin: true);
            foreach (var registro in registros)
            {
                elementos.Add(registro.MapearDto<E, R>(contexto, parametros));
            }
            return elementos;
        }

        public static E SeleccionarDtoPorAk<E, R>(this ContextoSe contexto, Dictionary<string, object> filtrosPorAk, bool errorSiNoHay = true, ParametrosDeNegocio parametros = null)
        where E : ElementoDto
        where R : RegistroDtm
        {
            var registro = contexto.SeleccionarPorAk<R>(filtrosPorAk, errorSiNoHay, aplicarJoin: true);
            return registro == null ? null : registro.MapearDto<E, R>(contexto, parametros);
        }

        public static TElemento MapearDto<TElemento, TRegistro>(this TRegistro registro, ContextoSe contexto, ParametrosDeNegocio parametros = null)
        where TElemento : ElementoDto
        where TRegistro : RegistroDtm
        => (TElemento)NegociosDeSe.CrearGestor(contexto, typeof(TRegistro), typeof(TElemento)).MapearDto(registro, parametros == null ? new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo) : parametros);

        public static TElemento MapearDto<TElemento>(this IRegistro registro, ContextoSe contexto, ParametrosDeNegocio parametros = null)
        where TElemento : ElementoDto
        => (TElemento)NegociosDeSe.CrearGestor(contexto, registro.GetType(), typeof(TElemento)).MapearDto(registro, parametros == null 
            ? new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.Peticion, enumPeticion.epLeerPorId } })
            : parametros);

        public static IElementoDto MapearDto(this IRegistro registro, ContextoSe contexto, enumNegocio negocio, ParametrosDeNegocio parametros = null)
        {
            var tipoDto = negocio.TipoDto();
            var gestor = NegociosDeSe.CrearGestor(contexto, registro.GetType(), tipoDto);
            var elementoDto = (IElementoDto)gestor.MapearDto(registro, parametros == null
            ? new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.Peticion, enumPeticion.epLeerPorId } })
            : parametros);
            return elementoDto;
        }

        public static DireccionDto MapearDto(this DireccionDtm registro, ContextoSe contexto, enumNegocio negocio, ParametrosDeNegocio parametros = null)
        => GestorDeDirecciones.Gestor(contexto, negocio).MapearElemento(registro, parametros == null ? new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo) : parametros);

        public static UriBuilder ComponerUrlPorId(this enumNegocio negocio, ContextoSe contexto, int id)
        {
            var ruta = ExtensionesDto.UrlBaseDeUnDto(negocio.TipoDto(), vista: "", errorSiMasDeUno: true);

            UriBuilder builder = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = ruta,
                Query = $"id={id}"
            };
            return builder;
        }

        public static string ComponerUrl(this TipoDtoElmento elemento)
        {
            var ruta = ExtensionesDto.UrlBaseDeUnDto(elemento.ClaseDto(), elemento.Vista);

            //var link = string.Join("/", CacheDeVariable.Cfg_UrlBase.Trim('/'), url.Trim('/')) + $"?id={elemento.IdElemento}";
            UriBuilder builder = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = ruta,
                Query = $"id={elemento.IdElemento}"
            };
            var refHtml = $@"<a href='{builder.Uri}' target='_blank' idelemento='{elemento.IdElemento}'>{elemento.Referencia}</a>";
            return refHtml;
        }

        public static string PatronUrl(this Type claseDto)
        {
            var ruta = ExtensionesDto.UrlBaseDeUnDto(claseDto, vista: "", errorSiMasDeUno: false);

            //var link = string.Join("/", CacheDeVariable.Cfg_UrlBase.Trim('/'), url.Trim('/')) + $"?id={elemento.IdElemento}";
            UriBuilder builder = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = ruta,
                Query = $"id=[IdElemento]"
            };
            var refHtml = $@"<a href='{builder.Uri}' target='_blank' idelemento='[IdElemento]'>[Referencia]</a>";
            return refHtml;
        }


        public static List<ClaseDelTipoDto> LeerClasesDelTipoDto(this enumNegocio negocio, ContextoSe contexto, int idTipo)
        {
            var tipoDeClaseDelTipo = negocio.ObtenerMetadatos()?.ClasesDelTipoDtm;
            if (tipoDeClaseDelTipo is null)
                return null;

            var orden = new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion(ordenarPor: $"{nameof(ClaseDelTipoDtm.Clase)}.{nameof(ClaseDelNegocioDtm.Referencia)}", modo: ModoDeOrdenancion.ascendente) };
            var filtro = new ClausulaDeFiltrado(nameof(ClaseDelTipoDtm.IdTipo), enumCriteriosDeFiltrado.igual, idTipo);
            var gestor = NegociosDeSe.CrearGestor(contexto, tipoDeClaseDelTipo, typeof(ClaseDelTipoDto));
            return (List<ClaseDelTipoDto>)gestor.LeerElementos(0, -1, filtros: new List<ClausulaDeFiltrado> { filtro }, orden: orden, opcionesDeMapeo: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } });
        }


        public static bool ImplementaITotalizador(Type tipoGestor)
        {
            // 1. Obtener la definición genérica de la interfaz ITotalizador<>
            Type tipoITotalizadorGenerico = typeof(ITotalizador<>);

            // 2. Buscar en las interfaces implementadas por el tipoGestor
            return tipoGestor.GetInterfaces()
                // Filtrar las interfaces que son genéricas y su definición genérica es ITotalizador<>
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == tipoITotalizadorGenerico);
        }
    }

}