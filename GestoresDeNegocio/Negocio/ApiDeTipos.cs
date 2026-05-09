using System;
using System.Collections.Generic;
using Utilidades;
using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using Newtonsoft.Json;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System.Collections;
using System.Linq;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ModeloDeDto;

namespace GestoresDeNegocio.Negocio
{
    public static class ApiDeTipos
    {

        public static TipoDeElementoDto PersistirTipoDto(ContextoSe contexto, enumNegocio negocio, string tipoJson, ParametrosDeNegocio parametros)
        {
            var tipoDto = JsonConvert.DeserializeObject(tipoJson, negocio.ObtenerMetadatos().TipoDto);

            if (parametros.Operacion == enumTipoOperacion.Eliminar  && negocio.Existen(contexto, nameof(IUsaTipo.IdTipo), ((TipoDeElementoDto)tipoDto).Id))
            {
                ((TipoDeElementoDto)tipoDto).Activo = !((TipoDeElementoDto)tipoDto).Activo;
                return (TipoDeElementoDto)negocio.CrearGestorDeTipo(contexto).PersistirElementoDto(tipoDto, new ParametrosDeNegocio(enumTipoOperacion.Modificar.ToTipoOperacion(), aplicarJoin: true) { Peticion = enumPeticion.epModificarPorId });
            }

            return (TipoDeElementoDto)negocio.CrearGestorDeTipo(contexto).PersistirElementoDto(tipoDto, parametros);
        }

        public static TipoDeElementoDto LeerTipoDtoPorId(ContextoSe contexto, enumNegocio negocio, int id, string filtrosJson)
        {
            return (TipoDeElementoDto)negocio.CrearGestorDeTipo(contexto).LeerElementoPorId(id, new Dictionary<string, object>
            {
                {ltrParametrosNeg.Peticion, enumPeticion.epLeerPorId }
            });
        }

        public static JerarquiaDto LeerJerarquia(ContextoSe contexto, enumNegocio negocio, int idPadre, string filtrosJson)
        {
            return negocio.CrearGestorDeTipo(contexto).LeerJerarquia(negocio, idPadre, filtrosJson);
        }

        public static IEnumerable<T> LeerElementos<T>(ContextoSe contexto, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        where T : TipoDeElementoDto
        {
            if (!parametros.ContainsKey(ltrParametrosEp.negocio))
                GestorDeErrores.Emitir("Debe indicar el negocio con el que se está tratando");

            enumNegocio negocio = NegociosDeSe.ToEnumerado(parametros[ltrParametrosEp.negocio].ToString());

            return (IEnumerable<T>)negocio.CrearGestorDeTipo(contexto).LeerElementos(parametros.LeerValor(ltrParametrosEp.Posicion, 0), Convert.ToInt32(parametros[ltrParametrosEp.cantidad]), filtros, orden, parametros);
        }

        public static List<TipoDeElementoDtm> TiposConPermisoDeGestor(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> parametros)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_TiposConGestion);

            Enum clase =
            negocio == enumNegocio.Contrato && parametros.ContieneClave(nameof(TipoDeContratoDtm.ClaseDeContrato))
            ? parametros.LeerValor<enumClaseDeContrato>(nameof(TipoDeContratoDtm.ClaseDeContrato))
            : negocio == enumNegocio.Presupuesto && parametros.ContieneClave(nameof(TipoDePresupuestoDtm.ClaseDePresupuesto))
            ? parametros.LeerValor<enumClaseDePresupuesto>(nameof(TipoDePresupuestoDtm.ClaseDePresupuesto))
            : enumClaseDeElementoPorTipo.Sin_Clase;

            var indice = $"{negocio.IdNegocio()}-{contexto.DatosDeConexion.IdUsuario}{(TipoDeElementoDtm.EsSinClase(clase) ? "" : clase)}";

            if (!cache.ContainsKey(indice))
            {
                var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(clausula: nameof(NegociosDeUnCgDtm.Negocio), criterio: enumCriteriosDeFiltrado.igual, valor: $"{negocio}{Simbolos.PuntoComa}{enumModoDeAccesoDeDatos.Gestor}") };
                if (!TipoDeElementoDtm.EsSinClase(clase) && (negocio == enumNegocio.Contrato || negocio == enumNegocio.Presupuesto))
                {
                    var clausula = negocio == enumNegocio.Contrato ? nameof(TipoDeContratoDtm.ClaseDeContrato) : nameof(TipoDePresupuestoDtm.ClaseDePresupuesto);
                    var filtro = new ClausulaDeFiltrado(clausula, enumCriteriosDeFiltrado.igual, valor: clase);
                    filtros.Add(filtro);
                }
                cache[indice] = negocio.CrearGestorDeTipo(contexto).LeerRegistros(0, -1, filtros, aplicarJoin: false);
            }
            return ((IEnumerable)cache[indice]).Cast<TipoDeElementoDtm>().ToList();
        }

        public static List<TipoDeElementoDtm> TiposConPermisoDeConsultor(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> parametros)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_TiposConsultor);

            Enum clase =
            negocio == enumNegocio.Contrato
            ? parametros.LeerValor<enumClaseDeContrato>(nameof(TipoDeContratoDtm.ClaseDeContrato))
            : negocio == enumNegocio.Presupuesto
            ? parametros.LeerValor<enumClaseDePresupuesto>(nameof(TipoDePresupuestoDtm.ClaseDePresupuesto))
            : enumClaseDeElementoPorTipo.Sin_Clase;

            var indice = $"{negocio.IdNegocio()}-{contexto.DatosDeConexion.IdUsuario}{(TipoDeElementoDtm.EsSinClase(clase) ? "" : clase)}";

            if (!cache.ContainsKey(indice))
            {
                var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(clausula: nameof(NegociosDeUnCgDtm.Negocio), criterio: enumCriteriosDeFiltrado.igual, valor: $"{negocio}{Simbolos.PuntoComa}{enumModoDeAccesoDeDatos.Consultor}") };
                if (negocio == enumNegocio.Contrato || negocio == enumNegocio.Presupuesto)
                {
                    var clausula = negocio == enumNegocio.Contrato ? nameof(TipoDeContratoDtm.ClaseDeContrato) : nameof(TipoDePresupuestoDtm.ClaseDePresupuesto);
                    var filtro = new ClausulaDeFiltrado(clausula, enumCriteriosDeFiltrado.igual, valor: clase);
                    filtros.Add(filtro);
                }
                cache[indice] = negocio.CrearGestorDeTipo(contexto).LeerRegistros(0, -1, filtros, aplicarJoin: false);
            }
            return ((IEnumerable)cache[indice]).Cast<TipoDeElementoDtm>().ToList();
        }

    }
}
