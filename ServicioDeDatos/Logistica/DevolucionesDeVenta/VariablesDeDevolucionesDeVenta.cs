using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumEtapasDeDevolucionDeVenta
    {
        [Description("Ids de estados en los que una devolución de venta se está tramitando")]
        DDV_Etapa_Tramitacion,
        [Description("Ids de estados en los que una devolución de venta está aceptada")]
        DDV_Etapa_Aceptada,
        [Description("Ids de estados en los que una devolución de venta está cancelada")]
        DDV_Etapa_Cancelada
    }

    public enum enumParametrosDeDevolucionesDeVenta
    {
        [Description("Indica el incremento al enumerar las filas")]
        DDV_IncrementarOrdenEn,
        [Description("Indica los datos por defecto para usar en la impresión de una devolución de venta")]
        DDV_DatosDeImpresion
    }

    public static class VariableDeDevolucionesDeVenta
    {
        private static string etapaTramitacion => enumNegocio.DevolucionVenta.Parametro(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion)?.Valor ?? null;
        private static string etapaAceptada => enumNegocio.DevolucionVenta.Parametro(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada)?.Valor ?? null;
        private static string etapaCancelada => enumNegocio.DevolucionVenta.Parametro(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada)?.Valor ?? null;

        public static string Estados(this enumEtapasDeDevolucionDeVenta etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion: estados = etapaTramitacion; break;
                case enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada: estados = etapaAceptada; break;
                case enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada: estados = etapaCancelada; break;
            }

            return estados.IsNullOrEmpty() ? enumNegocio.DevolucionVenta.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this DevolucionDeVentaDtm devolucion, enumEtapasDeDevolucionDeVenta etapa) => etapa.Lista().Contains(devolucion.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeDevolucionDeVenta> etapas, enumEtapasDeDevolucionDeVenta etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this DevolucionDeVentaDtm devolucion, List<enumEtapasDeDevolucionDeVenta> etapas)
        {
            var etapasDeLaDevolucion = devolucion.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaDevolucion.Contains(etapa)) return true;
            return false;
        }

        public static List<string> Lista(this DevolucionDeVentaDtm devolucion) => devolucion.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static (List<int> estados, enumEtapasDeDevolucionDeVenta etapa) EstadosDeLaEtapa(this enumEtapasDeDevolucionDeVenta etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeDevolucionDeVenta etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeDevolucionDeVenta> Etapas(this DevolucionDeVentaDtm devolucion)
        {
            var etapas = new List<enumEtapasDeDevolucionDeVenta>();
            if (devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                etapas.Add(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion);
            if (devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada))
                etapas.Add(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada);
            if (devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada))
                etapas.Add(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada);
            return etapas;
        }

        public static string CadenaDeEtapas(this DevolucionDeVentaDtm devolucion) => string.Join(Simbolos.separadorDeEtapas, devolucion.Etapas());

        public static enumEtapasDeDevolucionDeVenta Etapa(this DevolucionDeVentaDtm devolucion)
        {
            var etapas = devolucion.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa {enumNegocio.DevolucionVenta.ConArticulo()}, " +
                    $"cuando ésta está en el estado {devolucion.Propiedad<EstadoDtm>(typeof(EstadoDeUnaDevolucionDeVentaDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"La {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeDevolucionDeVenta etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion: return minusculas ? "en tramitación" : "En tramitación";
                case enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada: return minusculas ? "aceptada" : "Aceptada";
                case enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada: return minusculas ? "cancelada" : "Cancelada";
            }
            return etapa.ToString();
        }

    }

}
