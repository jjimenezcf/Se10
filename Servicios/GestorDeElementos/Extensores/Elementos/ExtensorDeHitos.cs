
using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Utilidades;

namespace GestorDeElementos.Extensores
{


    public static class ExtensorDeHitos
    {

        public static enumNegocio Negocio(Type tipo)
        {
            if (typeof(HitosDeUnRegistroEsDtm) == tipo) return enumNegocio.Registro;
            if (typeof(HitosDeUnaTareaDtm) == tipo) return enumNegocio.Tarea;
            if (typeof(HitosDeUnExpedienteDtm) == tipo) return enumNegocio.Expediente;
            if (typeof(HitosDeUnaFacturaRecDtm) == tipo) return enumNegocio.FacturaRecibida;
            if (typeof(HitosDeUnPedidoDtm) == tipo) return enumNegocio.Pedido;
            if (typeof(HitosDeUnAlmacenDtm) == tipo) return enumNegocio.Almacen;
            if (typeof(HitosDeUnaRegularizacionDtm) == tipo) return enumNegocio.Regularizacion;
            if (typeof(HitosDeUnPreasientoDtm) == tipo) return enumNegocio.Preasiento;
            if (typeof(HitosDeUnPleitoDtm) == tipo) return enumNegocio.Pleito;
            if (typeof(HitosDeUnPresupuestoDtm) == tipo) return enumNegocio.Presupuesto;
            if (typeof(HitosDeUnContratoDtm) == tipo) return enumNegocio.Contrato;
            if (typeof(HitosDeUnPagoDtm) == tipo) return enumNegocio.Pago;
            if (typeof(HitosDeUnaRemesaPagDtm) == tipo) return enumNegocio.RemesaPag;
            if (typeof(HitosDeUnCircuitoDocDtm) == tipo) return enumNegocio.CircuitoDoc;
            if (typeof(HitosDeUnaFacturaEmtDtm) == tipo) return enumNegocio.FacturaEmitida;
            if (typeof(HitosDeUnParteTrDtm) == tipo) return enumNegocio.ParteDeTrabajo;
            if (typeof(HitosDeUnaPlanificacionDeVentaDtm) == tipo) return enumNegocio.PlanificacionDeVenta;
            if (typeof(HitosDeUnaRemesaFaeDtm) == tipo) return enumNegocio.RemesaFae;

            throw new Exception($"No se ha definido cual es el negocio para la clase de hito {tipo.Name}");
        }

        public static IQueryable<HitoDtm> Hitos(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<HitosDeUnRegistroEsDtm>().Cast<HitoDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<HitosDeUnaTareaDtm>().Cast<HitoDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<HitosDeUnExpedienteDtm>().Cast<HitoDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<HitosDeUnaFacturaRecDtm>().Cast<HitoDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<HitosDeUnPedidoDtm>().Cast<HitoDtm>();
                case enumNegocio.Almacen:
                    return contexto.Set<HitosDeUnAlmacenDtm>().Cast<HitoDtm>();
                case enumNegocio.Regularizacion:
                    return contexto.Set<HitosDeUnaRegularizacionDtm>().Cast<HitoDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<HitosDeUnPreasientoDtm>().Cast<HitoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<HitosDeUnPleitoDtm>().Cast<HitoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<HitosDeUnPresupuestoDtm>().Cast<HitoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<HitosDeUnContratoDtm>().Cast<HitoDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<HitosDeUnPagoDtm>().Cast<HitoDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<HitosDeUnaRemesaPagDtm>().Cast<HitoDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<HitosDeUnCircuitoDocDtm>().Cast<HitoDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<HitosDeUnaFacturaEmtDtm>().Cast<HitoDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<HitosDeUnParteTrDtm>().Cast<HitoDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<HitosDeUnaPlanificacionDeVentaDtm>().Cast<HitoDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<HitosDeUnaRemesaFaeDtm>().Cast<HitoDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los hitos del negocio: {negocio}");
        }

        public static HitoDtm UltimoHito<T>(this ContextoSe contexto, int idelemento)
        where T : HitoDtm
        {
            var negocio = Negocio(typeof(T));
            return HitoSql.LeerUltimoHito(contexto, negocio.TablaDeHitos(), idelemento)[0];
        }

        public static HitoDtm HitoAnteriorAlActual<T>(this ContextoSe contexto, int idElemnto, bool errorSiNoHay = true)
        where T : HitoDtm
        {
            var negocio = Negocio(typeof(T));
            var hito = HitoSql.LeerAnteriorHito(contexto, negocio.TablaDeHitos(), idElemnto);
            if (errorSiNoHay && hito == null)
                GestorDeErrores.Emitir("El elemento solicitado sólo tiene historia inicial");
            return hito;
        }


        /// <summary>
        /// Devuelve el hito inmediatamente posterior (por Id) al hito dado,
        /// o null si el hito no tiene transición asociada.
        /// </summary>
        public static HitoDtm HitoPosterior<T>(this HitoDtm hito, ContextoSe contexto)
        where T : HitoDtm
        {
            if (!hito.IdTransicion.HasValue)
                return null;

            return hito.HitoPosterior(contexto, Negocio(typeof(T)));
        }

        /// <summary>
        /// Devuelve el UsuarioDtm que generó la transición del hito dado,
        /// buscándolo en el hito posterior.
        /// Devuelve null si el hito no tiene transición o no se encuentra el posterior.
        /// </summary>
        public static UsuarioDtm QuienLoHaTransitado(this HitoDtm hito, ContextoSe contexto, enumNegocio negocio)
        {
            var posterior = hito.HitoPosterior(contexto, negocio);
            if (posterior == null)
                return null;

            return contexto.SeleccionarPorId<UsuarioDtm>(posterior.IdUsuario);
        }

        /// <summary>
        /// Devuelve el hito inmediatamente posterior (por Id) al hito dado,
        /// o null si el hito no tiene transición asociada.
        /// </summary>
        private static HitoDtm HitoPosterior(this HitoDtm hito, ContextoSe contexto, enumNegocio negocio)
        {
            if (!hito.IdTransicion.HasValue)
                return null;

            return HitoSql.LeerHistoriaDelElemento(contexto, negocio.TablaDeHitos(), hito.IdElemento)
                          .Where(h => h.Id > hito.Id)
                          .OrderBy(h => h.Id)
                          .FirstOrDefault();
        }

        /// <summary>
        /// Notifica al creador del elemento que otro usuario lo ha transitado desde la interface.
        /// No se envía si la transición es del sistema, si no viene de una petición de transitar,
        /// si quien transita es el creador o si en la petición ya se pidió notificar al creador.
        /// Si la transición es una devolución se avisa además a quien lo transitó al estado origen,
        /// y si éste es el creador sólo recibe el mensaje de devolución.
        /// </summary>
        public static void EnviarMensajeDeTransicion(this ContextoSe contexto, enumNegocio negocio, int idElemento, int idEstadoOrigen, int idEstadoDestino, int idTransicionEjecutada, Dictionary<string, object> parametros)
        {
            try
            {
                var transicion = negocio.Transicion(contexto, idTransicionEjecutada);
                if (transicion.DelSistema)
                    return;

                if (parametros.LeerValor(ltrParametrosNeg.Peticion, enumPeticion.epSinPeticion) != enumPeticion.epTransitar)
                    return;

                var elemento = (IElementoDtm)negocio.LeerRegistro(contexto, idElemento, aplicarJoin: false, parametros: new Dictionary<string, object>
                              {
                                  {ltrParametrosNeg.ValidarPermisosDeConsulta, false },
                                  {ltrParametrosNeg.UsarLaCache, false }
                              });

                var devueltoA = contexto.EnviarMensajeDeDevolucion(negocio, elemento, idEstadoOrigen, idEstadoDestino, transicion, parametros);

                var creador = elemento.Creador(contexto);
                if (creador is null || creador.eMail.IsNullOrEmpty() || creador.Id == contexto.DatosDeConexion.IdUsuario)
                    return;

                if (parametros.LeerValor(ltrParametrosEp.usuarios, 0) == creador.Id || devueltoA?.Id == creador.Id)
                    return;

                var asunto = $"El elemento con referencia '{elemento.Referencia(contexto)}' del negocio '{negocio.Singular()}' se le ha cambiado el estado a '{negocio.Estado(contexto, idEstadoDestino).Nombre}'";
                var cuerpo = contexto.CuerpoDelMensajeDeTransicion(negocio, elemento, idEstadoOrigen, idEstadoDestino, transicion, parametros, "Transitado por");
                contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { creador.eMail }, asunto, cuerpo);
            }
            catch (Exception e)
            {
                contexto.Traza?.AnotarExcepcion(e);
            }
        }

        /// <summary>
        /// Si al transitar de e1 a ef resulta que ef es el estado e0 desde el que se llegó a e1, se considera una devolución
        /// y se avisa al usuario que transitó de e0 a e1, salvo que sea quien devuelve o que ya se le haya notificado en la petición.
        /// Devuelve el usuario al que se ha enviado el mensaje, o null si no se ha enviado.
        /// </summary>
        private static UsuarioDtm EnviarMensajeDeDevolucion(this ContextoSe contexto, enumNegocio negocio, IElementoDtm elemento, int idEstadoOrigen, int idEstadoDestino, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            // historia ordenada por id desc: [0] hito abierto en ef, [1] hito de e1 cerrado ahora, [2] hito de e0
            var historia = HitoSql.LeerHistoriaDelElemento(contexto, negocio.TablaDeHitos(), elemento.Id);
            if (historia.Count < 3 || historia[1].IdEstado != idEstadoOrigen || historia[2].IdEstado != idEstadoDestino)
                return null;

            // El hito de e1 lo creó quien transitó de e0 a e1 (al cerrar un hito no se modifica su usuario)
            var receptor = contexto.SeleccionarPorId<UsuarioDtm>(historia[1].IdUsuario, errorSiNoHay: false);
            if (receptor is null || receptor.eMail.IsNullOrEmpty() || receptor.Id == contexto.DatosDeConexion.IdUsuario)
                return null;

            if (parametros.LeerValor(ltrParametrosEp.usuarios, 0) == receptor.Id)
                return null;

            var asunto = $"Se ha devuelto el elemento con referencia '{elemento.Referencia(contexto)}' del negocio '{negocio.Singular()}'";
            var cuerpo = contexto.CuerpoDelMensajeDeTransicion(negocio, elemento, idEstadoOrigen, idEstadoDestino, transicion, parametros, "Devuelto por");
            contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { receptor.eMail }, asunto, cuerpo);
            return receptor;
        }

        private static string CuerpoDelMensajeDeTransicion(this ContextoSe contexto, enumNegocio negocio, IElementoDtm elemento, int idEstadoOrigen, int idEstadoDestino, TransicionDtm transicion, Dictionary<string, object> parametros, string accionRealizadaPor)
        {
            var motivo = transicion.Asunto is null ? parametros.LeerValor(ltrParametrosEp.asunto, "") : transicion.Asunto;
            var detalle = parametros.LeerValor(ltrParametrosEp.detalleAsunto, "");
            var idArchivo = parametros.LeerValor(nameof(IUsaArchivo.IdArchivo), 0);
            var archivo = idArchivo > 0 ? contexto.SeleccionarPorId<ArchivoDtm>(idArchivo, errorSiNoHay: false) : null;

            var cuerpo = $"{negocio.Singular()}: {WebUtility.HtmlEncode(elemento.Nombre)}{Simbolos.br}" +
                         $"{accionRealizadaPor}: {WebUtility.HtmlEncode(UsuarioDtm.NombreCompleto(contexto.Usuario))}{Simbolos.br}" +
                         $"Estado anterior: {negocio.Estado(contexto, idEstadoOrigen).Nombre}{Simbolos.br}" +
                         $"Estado actual: {negocio.Estado(contexto, idEstadoDestino).Nombre}{Simbolos.br}" +
                         $"Transición ejecutada: {transicion.Nombre}{Simbolos.br}";

            if (!motivo.IsNullOrEmpty())
                cuerpo += $"Motivo: {WebUtility.HtmlEncode(motivo)}{Simbolos.br}";

            if (!detalle.IsNullOrEmpty())
                cuerpo += $"Detalle: {WebUtility.HtmlEncode(detalle).Replace(Environment.NewLine, Simbolos.br).Replace("\n", Simbolos.br)}{Simbolos.br}";

            if (archivo is not null)
                cuerpo += $"Archivo añadido: {WebUtility.HtmlEncode(archivo.Nombre)}{Simbolos.br}";

            cuerpo += $"{Simbolos.br}Enlace al elemento: {elemento.CrearHref(contexto)}{Simbolos.br}";

            if (archivo is not null)
                cuerpo += $"Enlace al archivo: {archivo.HrefDeDescargaConGuid(contexto)}{Simbolos.br}";

            return cuerpo;
        }

    }
}
