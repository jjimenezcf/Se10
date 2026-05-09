using System;
using System.Collections.Generic;
using System.ComponentModel;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    public static class VariablesDePlfsDeVenta
    {
        public enum enumEtapasDePlfsDeVenta
        {
            [Description("Ids de estados en los que una planificación está pendiente ser generada")]
            PLF_Etapa_Pendiente,
            [Description("Ids de estados en los que una planificación está generada")]
            PLF_Etapa_Generada,
            [Description("Ids de estados en los que una planificación está anulada")]
            PLF_Etapa_Anulada
        }

        private static string etapaPendiente => enumNegocio.PlanificacionDeVenta.Parametro(enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente)?.Valor ?? null;
        private static string etapaGenerada => enumNegocio.PlanificacionDeVenta.Parametro(enumEtapasDePlfsDeVenta.PLF_Etapa_Generada)?.Valor ?? null;
        private static string etapaAnulada => enumNegocio.PlanificacionDeVenta.Parametro(enumEtapasDePlfsDeVenta.PLF_Etapa_Anulada)?.Valor ?? null;


        public static List<int> Lista(this enumEtapasDePlfsDeVenta etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);
        public static (List<int> estados, enumEtapasDePlfsDeVenta etapa) EstadosDeLaEtapa(this enumEtapasDePlfsDeVenta etapa) => (etapa.Lista(), etapa);
        public static string Estados(this enumEtapasDePlfsDeVenta etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente: estados = etapaPendiente; break;
                case enumEtapasDePlfsDeVenta.PLF_Etapa_Generada: estados = etapaGenerada; break;
                case enumEtapasDePlfsDeVenta.PLF_Etapa_Anulada: estados = etapaAnulada; break;
            }
            return estados.IsNullOrEmpty() ? enumNegocio.PlanificacionDeVenta.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }


        public static enumEtapasDePlfsDeVenta Etapa(this PlanificacionDeVentaDtm plf)
        {
            if (plf.EstaEnLaEtapa(etapaPendiente)) return enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente;
            else
            if (plf.EstaEnLaEtapa(etapaGenerada)) return enumEtapasDePlfsDeVenta.PLF_Etapa_Generada;
            else
            if (plf.EstaEnLaEtapa(etapaAnulada)) return enumEtapasDePlfsDeVenta.PLF_Etapa_Anulada;

            throw new Exception($"No se ha definido la etapa de la {enumNegocio.FacturaEmitida.Singular(true)}, " +
                $"cuando éste está en el estado {plf.Propiedad<EstadoDtm>(typeof(EstadoDeUnaFacturaEmtDtm)).Nombre}");
        }

        public static bool EstaEnLaEtapa(this PlanificacionDeVentaDtm plf, enumEtapasDePlfsDeVenta etapa) => etapa.Lista().Contains(plf.IdEstado);

        private static bool EstaEnLaEtapa(this PlanificacionDeVentaDtm plf, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(plf.IdEstado);

    }
}
