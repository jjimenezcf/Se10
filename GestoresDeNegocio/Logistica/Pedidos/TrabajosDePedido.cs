using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    public enum enumTrabajosDePedido
    {
        [Description("Enviar al proveedor los pedidos planificados")]
        EnviarPedidosPlanificados
    }

    public class TrabajosDePedido
    {
        // Diario a partir de la 1 de la mañana
        public static TrabajoDeUsuarioDtm SometerEnviarPedidosPlanificados(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDePedido).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDePedido.EnviarPedidosPlanificados.Descripcion(), dll, clase, nameof(enumTrabajosDePedido.EnviarPedidosPlanificados), comunicarFin: true);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1).Date.AddHours(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        // Pasa a solicitados, con la transición automática del sistema, los pedidos en cumplimentación con importe cuya fecha de pedir ha llegado;
        // al solicitarlos se emite su pdf y se envía al proveedor (ver GestorDePedidos.DespuesDeTransitar)
        public static void EnviarPedidosPlanificados(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(EnviarPedidosPlanificados));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");

                var estadosEnCumplimentacion = enumEtapasDePedido.PED_Etapa_De_Cumplimentacion.Estados();
                if (estadosEnCumplimentacion == ltrEstados.EstadoNulo)
                {
                    entorno.CrearTraza($"la etapa {enumEtapasDePedido.PED_Etapa_De_Cumplimentacion} está sin definir, no se enviarán los pedidos planificados");
                    return;
                }

                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(PedidoDtm.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estadosEnCumplimentacion),
                    new ClausulaDeFiltrado(nameof(PedidoDtm.PedidoEl), enumCriteriosDeFiltrado.menorIgual, DateTime.Now.ToString()),
                };
                var pedidos = enumNegocio.Pedido.SeleccionarPorFiltro<PedidoDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

                //para cada pedido planificado
                foreach (var pedido in pedidos)
                {
                    if (pedido.PedidoEl is null)
                        continue;

                    if (pedido.Importe(contexto) <= 0)
                    {
                        entorno.CrearTraza($"el pedido {pedido.Referencia} no tiene importe, no se solicita");
                        continue;
                    }

                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        entorno.ActualizarTraza(trazaInfDtm, $"solicitando el pedido '{pedido.Referencia}'");
                        pedido.TransitarALaEtapa(contexto, enumEtapasDePedido.PED_Etapa_De_Solicitud.EstadosDeLaEtapa());
                        entorno.CrearTraza($"pedido '{pedido.Referencia}' solicitado");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        entorno.AnotarError($"No se ha podido solicitar el pedido '{pedido.Referencia}'", ex);
                        contexto.Rollback(tran);
                    }
                }
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }
    }
}
