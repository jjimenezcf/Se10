using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumEtapasDePedido
    {
        [Description("Ids de estados en los que un pedido se ha de cumplimentar")]
        PED_Etapa_De_Cumplimentacion,
        [Description("Ids de estados en los que un pedido se está aprobando")]
        PED_Etapa_De_Aprobacion,
        [Description("Ids de estados en los que un pedido se ha de solicitado")]
        PED_Etapa_De_Solicitud,
        [Description("Ids de estados en los que un pedido se esta entregando")]
        PED_Etapa_De_Recepcion,
        [Description("Ids de estados en los que un pedido está cerrado")]
        PED_Etapa_Cerrado,
        [Description("Ids de estados en los que un pedido es devuelto")]
        PED_Etapa_Devuelto,
        [Description("Ids de estados en los que un pedido es anulado")]
        PED_Etapa_Cancelado
    }

    public enum enumParametrosDePedidos
    {
        [Description("Indica el id de la unidad de medida por defecto")]
        PED_Unidad_Medida,
        [Description("Indica el id de la naturaleza por defecto")]
        PED_Naturaleza,
        [Description("Indica el tipo de línea por defecto")]
        PED_TipoDeLinea,
        [Description("Indica la clase de unitario por defecto")]
        PED_ClaseDeUnitario,
        [Description("Indica los datos por defecto para usar en la impresión de un pedido")]
        PED_DatosDeImpresion,
        [Description("Indica el incremento a aplicar a las líneas de pedido")]
        PED_IncrementarOrdenEn
    }

    public static class VariableDePedidos
    {
        private static string etapaDeCumplimentacion => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion)?.Valor ?? null;
        private static string etapaDeAprobacion => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Aprobacion)?.Valor ?? null;
        private static string etapaDeCobro => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Solicitud)?.Valor ?? null;
        private static string etapaDePago => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Recepcion)?.Valor ?? null;
        private static string etapaDeCierre => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_Cerrado)?.Valor ?? null;
        private static string etapaDeDevolucion => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_Devuelto)?.Valor ?? null;
        private static string etapaAnulada => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_Cancelado)?.Valor ?? null;

        public enum enumMotivoTransicion {  };
                

        public static string Estados(this enumEtapasDePedido etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePedido.PED_Etapa_De_Cumplimentacion: estados =etapaDeCumplimentacion; break;
                case enumEtapasDePedido.PED_Etapa_De_Aprobacion: estados =etapaDeAprobacion; break;
                case enumEtapasDePedido.PED_Etapa_De_Solicitud: estados =etapaDeCobro; break;
                case enumEtapasDePedido.PED_Etapa_De_Recepcion: estados =etapaDePago; break;
                case enumEtapasDePedido.PED_Etapa_Cerrado: estados =etapaDeCierre; break;
                case enumEtapasDePedido.PED_Etapa_Devuelto: estados =etapaDeDevolucion; break;
                case enumEtapasDePedido.PED_Etapa_Cancelado: estados =etapaAnulada; break;

            }

            return estados.IsNullOrEmpty() ? enumNegocio.Pedido.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this PedidoDtm pedido, enumEtapasDePedido etapa) => etapa.Lista().Contains(pedido.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDePedido> etapas, enumEtapasDePedido etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this PedidoDtm pedido, List<enumEtapasDePedido> etapas)
        {
            var etapasDeLaFactura = pedido.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDePedido etapa) EstadosDeLaEtapa(this enumEtapasDePedido etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDePedido etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDePedido> Etapas(this PedidoDtm pedido)
        {
            var etapas = new List<enumEtapasDePedido>();
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Aprobacion))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Aprobacion);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Solicitud);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Recepcion);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado))
                etapas.Add(enumEtapasDePedido.PED_Etapa_Cerrado);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Devuelto))
                etapas.Add(enumEtapasDePedido.PED_Etapa_Devuelto);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cancelado))
                etapas.Add(enumEtapasDePedido.PED_Etapa_Cancelado);
            return etapas;
        }

        public static string CadenaDeEtapas(this PedidoDtm pedido) => string.Join(Simbolos.separadorDeEtapas, pedido.Etapas());

        public static enumEtapasDePedido Etapa(this PedidoDtm pedido)
        {
            var etapas = pedido.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa del {enumNegocio.Pedido.Singular(true)}, " +
                    $"cuando éste está en el estado {pedido.Propiedad<EstadoDtm>(typeof(EstadoDeUnPedidoDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado del pedido {enumNegocio.Pedido.Singular(true)} '{pedido.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDePedido etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDePedido.PED_Etapa_De_Cumplimentacion: return minusculas ? "cumplimentación" : "Cumplimentación";
                case enumEtapasDePedido.PED_Etapa_De_Aprobacion: return minusculas ? "aprobación" : "Aprobada";
                case enumEtapasDePedido.PED_Etapa_De_Solicitud: return minusculas ? "solicitado" : "Solicitado";
                case enumEtapasDePedido.PED_Etapa_De_Recepcion: return minusculas ? "de entrega" : "De entrega";
                case enumEtapasDePedido.PED_Etapa_Cerrado: return minusculas ? "cerrado" : "Cerrado";
                case enumEtapasDePedido.PED_Etapa_Devuelto: return minusculas ? "devuelto" : "Devuelto";
                case enumEtapasDePedido.PED_Etapa_Cancelado: return minusculas ? "anulado" : "Anulado";
            }
            return etapa.ToString();
        }

    }

}
