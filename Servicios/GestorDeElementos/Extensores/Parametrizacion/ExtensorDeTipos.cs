using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using System.Collections.Generic;
using System.Linq;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
using System;
using Utilidades;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Contabilidad;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeTipos
    {
        public static IQueryable<TipoDeElementoDtm> Tipos<T>(this ContextoSe contexto)
        where T : TipoDeElementoDtm
        =>
        contexto.Set<T>().Cast<T>();


        public static T PersistirPorSigla<T>(this T tipo, ContextoSe contexto, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : TipoDeElementoDtm
        {
            if (tipo.Id > 0)
                return tipo.Modificar(contexto);

            var leido = contexto.SeleccionarPorPropiedad<T>(nameof(TipoDeElementoDtm.Sigla), tipo.Sigla, errorSiNoHay: errorSiYaExiste);
            if (leido == null)
                return tipo.Insertar(contexto, parametros, aplicarJoin);

            tipo.Id = leido.Id;
            return tipo.Modificar(contexto, parametros, aplicarJoin);
        }


        public static List<ITipoDeElementoDtm> Tipos(this enumNegocio negocio, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Tipos);
            if (!cache.ContainsKey(negocio.ToString()))
            {
                cache[negocio.ToString()] = negocio.TiposInterna(contexto).ToList();
            }
            return (List<ITipoDeElementoDtm>)cache[negocio.ToString()];
        }

        private static IQueryable<ITipoDeElementoDtm> TiposInterna(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<TipoDeRegistroEsDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<TipoDeTareaDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<TipoDeExpedienteDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<TipoDePleitoDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<TipoDePresupuestoDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<TipoDeContratoDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Archivador:
                    return contexto.Set<TipoDeArchivadorDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<TipoDeCircuitoDocDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<TipoDeFacturaEmtDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<TipoDeParteTrDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<TipoDeRemesaFaeDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<TipoDeFacturaRecDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<TipoDePedidoDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<TipoDePreasientoDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<TipoDePagoDtm>().Cast<ITipoDeElementoDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<TipoDeRemesaPagDto>().Cast<ITipoDeElementoDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los tipos del negocio: {negocio}");
        }

        public static IQueryable<TipoConFlujoDtm> TiposConFlujo(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<TipoDeRegistroEsDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<TipoDeTareaDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<TipoDeExpedienteDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<TipoDePleitoDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<TipoDePresupuestoDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<TipoDeContratoDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<TipoDeCircuitoDocDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<TipoDeFacturaEmtDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<TipoDeParteTrDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<TipoDeRemesaFaeDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<TipoDeFacturaRecDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<TipoDePedidoDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<TipoDePreasientoDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<TipoDePagoDtm>().Cast<TipoConFlujoDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<TipoDeRemesaPagDto>().Cast<TipoConFlujoDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los tipos con flujo del negocio: {negocio}");
        }


        public static ITipoDeElementoDtm Tipo(this IUsaTipo elemento, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Tipo);
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!cache.ContainsKey($"{negocio}-{elemento.IdTipo}"))
            {
                var gestor = NegociosDeSe.CrearGestorDeTipo(negocio, contexto);
                cache[$"{negocio}-{elemento.IdTipo}"] = (ITipoDeElementoDtm)gestor.LeerRegistroPorId(elemento.IdTipo, aplicarJoin: false,
                    parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
            }
            return (ITipoDeElementoDtm)cache[$"{negocio}-{elemento.IdTipo}"];
        }

        public static T Tipo<T>(this IUsaTipo elemento, ContextoSe contexto)
        where T : TipoDeElementoDtm
        =>
        (T)elemento.Tipo(contexto);


        public static List<int> ObtenerIdsDelTipoConHijos(this enumNegocio negocio, ContextoSe contexto, int idTipo)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_IdsDelTipoConHijos);
            var i = $"{negocio}-{idTipo}";
            if (!cache.ContainsKey(i))
            {
                var ids = TipoDeElementoSql.TiposDependientes(contexto, negocio.ObtenerMetadatos().TipoDtm, idTipo).Select(r => r.Id).ToList();
                ids.Add(idTipo);
                cache[i] = ids;
            }
            return (List<int>)cache[i];
        }

    }
}
