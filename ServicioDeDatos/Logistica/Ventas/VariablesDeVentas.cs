using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumEtapasDeVenta
    {
        [Description("Ids de estados en los que un documento de venta está solicitado")]
        DSA_Etapa_Solicitud,
        [Description("Ids de estados en los que un documento de venta se está sirviendo")]
        DSA_Etapa_Servicio,
        [Description("Ids de estados en los que un documento de venta está facturado")]
        DSA_Etapa_Facturacion,
        [Description("Ids de estados en los que un documento de venta está cancelado")]
        DSA_Etapa_Cancelacion
    }

    public enum enumParametrosDeVentas
    {
        [Description("Indica el incremento al enumerar las filas")]
        DSA_IncrementarOrdenEn
    }

    public static class VariableDeVentas
    {
        private static string etapaSolicitud => enumNegocio.Venta.Parametro(enumEtapasDeVenta.DSA_Etapa_Solicitud)?.Valor ?? null;
        private static string etapaServicio => enumNegocio.Venta.Parametro(enumEtapasDeVenta.DSA_Etapa_Servicio)?.Valor ?? null;
        private static string etapaFacturacion => enumNegocio.Venta.Parametro(enumEtapasDeVenta.DSA_Etapa_Facturacion)?.Valor ?? null;
        private static string etapaCancelacion => enumNegocio.Venta.Parametro(enumEtapasDeVenta.DSA_Etapa_Cancelacion)?.Valor ?? null;

        public static string Estados(this enumEtapasDeVenta etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeVenta.DSA_Etapa_Solicitud: estados = etapaSolicitud; break;
                case enumEtapasDeVenta.DSA_Etapa_Servicio: estados = etapaServicio; break;
                case enumEtapasDeVenta.DSA_Etapa_Facturacion: estados = etapaFacturacion; break;
                case enumEtapasDeVenta.DSA_Etapa_Cancelacion: estados = etapaCancelacion; break;
            }

            return estados.IsNullOrEmpty() ? enumNegocio.Venta.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this VentaDtm venta, enumEtapasDeVenta etapa) => etapa.Lista().Contains(venta.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeVenta> etapas, enumEtapasDeVenta etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this VentaDtm venta, List<enumEtapasDeVenta> etapas)
        {
            var etapasDeLaVenta = venta.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaVenta.Contains(etapa)) return true;
            return false;
        }

        public static List<string> Lista(this VentaDtm venta) => venta.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static (List<int> estados, enumEtapasDeVenta etapa) EstadosDeLaEtapa(this enumEtapasDeVenta etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeVenta etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeVenta> Etapas(this VentaDtm venta)
        {
            var etapas = new List<enumEtapasDeVenta>();
            if (venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud))
                etapas.Add(enumEtapasDeVenta.DSA_Etapa_Solicitud);
            if (venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Servicio))
                etapas.Add(enumEtapasDeVenta.DSA_Etapa_Servicio);
            if (venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion))
                etapas.Add(enumEtapasDeVenta.DSA_Etapa_Facturacion);
            if (venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Cancelacion))
                etapas.Add(enumEtapasDeVenta.DSA_Etapa_Cancelacion);
            return etapas;
        }

        public static string CadenaDeEtapas(this VentaDtm venta) => string.Join(Simbolos.separadorDeEtapas, venta.Etapas());

        public static enumEtapasDeVenta Etapa(this VentaDtm venta)
        {
            var etapas = venta.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa {enumNegocio.Venta.ConArticulo()}, " +
                    $"cuando éste está en el estado {venta.Propiedad<EstadoDtm>(typeof(EstadoDeUnaVentaDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeVenta etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeVenta.DSA_Etapa_Solicitud: return minusculas ? "solicitado" : "Solicitado";
                case enumEtapasDeVenta.DSA_Etapa_Servicio: return minusculas ? "sirviendo" : "Sirviendo";
                case enumEtapasDeVenta.DSA_Etapa_Facturacion: return minusculas ? "facturado" : "Facturado";
                case enumEtapasDeVenta.DSA_Etapa_Cancelacion: return minusculas ? "cancelado" : "Cancelado";
            }
            return etapa.ToString();
        }

    }

}
