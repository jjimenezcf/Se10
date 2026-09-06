using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumEtapasDeRegularizacion
    {
        [Description("Ids de estados en los que una regularización está abierta")]
        RAL_Inicial,
        [Description("Ids de estados en los que una regularización se está recontando")]
        RAL_Recontando,
        [Description("Ids de estados en los que una regularización está aplicada")]
        RAL_Cerrado,
        [Description("Ids de estados en los que una regularización está cancelada")]
        RAL_Cancelar
    }

    public enum enumParametrosDeRegularizaciones
    {
        [Description("Indica el incremento al enumerar las filas")]
        RAL_IncrementarOrdenEn
    }

    public static class VariableDeRegularizaciones
    {
        private static string etapaInicia => enumNegocio.Regularizacion.Parametro(enumEtapasDeRegularizacion.RAL_Inicial)?.Valor ?? null;
        private static string etapaRecontando => enumNegocio.Regularizacion.Parametro(enumEtapasDeRegularizacion.RAL_Recontando)?.Valor ?? null;
        private static string etapaCerrado => enumNegocio.Regularizacion.Parametro(enumEtapasDeRegularizacion.RAL_Cerrado)?.Valor ?? null;
        private static string etapaCancelado => enumNegocio.Regularizacion.Parametro(enumEtapasDeRegularizacion.RAL_Cancelar)?.Valor ?? null;

        public enum enumMotivoTransicion { };


        public static string Estados(this enumEtapasDeRegularizacion etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeRegularizacion.RAL_Inicial: estados = etapaInicia; break;
                case enumEtapasDeRegularizacion.RAL_Recontando: estados = etapaRecontando; break;
                case enumEtapasDeRegularizacion.RAL_Cerrado: estados = etapaCerrado; break;
                case enumEtapasDeRegularizacion.RAL_Cancelar: estados = etapaCancelado; break;

            }

            return estados.IsNullOrEmpty() ? enumNegocio.Regularizacion.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this RegularizacionDtm regularizacion, enumEtapasDeRegularizacion etapa) => etapa.Lista().Contains(regularizacion.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeRegularizacion> etapas, enumEtapasDeRegularizacion etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this RegularizacionDtm regularizacion, List<enumEtapasDeRegularizacion> etapas)
        {
            var etapasDeLaRegularizacion = regularizacion.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaRegularizacion.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDeRegularizacion etapa) EstadosDeLaEtapa(this enumEtapasDeRegularizacion etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeRegularizacion etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeRegularizacion> Etapas(this RegularizacionDtm regularizacion)
        {
            var etapas = new List<enumEtapasDeRegularizacion>();
            if (regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Inicial))
                etapas.Add(enumEtapasDeRegularizacion.RAL_Inicial);
            if (regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Recontando))
                etapas.Add(enumEtapasDeRegularizacion.RAL_Recontando);
            if (regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Cerrado))
                etapas.Add(enumEtapasDeRegularizacion.RAL_Cerrado);
            if (regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Cancelar))
                etapas.Add(enumEtapasDeRegularizacion.RAL_Cancelar);
            return etapas;
        }

        public static string CadenaDeEtapas(this RegularizacionDtm regularizacion) => string.Join(Simbolos.separadorDeEtapas, regularizacion.Etapas());

        public static enumEtapasDeRegularizacion Etapa(this RegularizacionDtm regularizacion)
        {
            var etapas = regularizacion.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.Regularizacion.Singular(true)}, " +
                    $"cuando ésta está en el estado {regularizacion.Propiedad<EstadoDtm>(typeof(EstadoDeUnaRegularizacionDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"La {enumNegocio.Regularizacion.Singular(true)} '{regularizacion.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeRegularizacion etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeRegularizacion.RAL_Inicial: return minusculas ? "iniciada" : "Iniciada";
                case enumEtapasDeRegularizacion.RAL_Recontando: return minusculas ? "recontando" : "Recontando";
                case enumEtapasDeRegularizacion.RAL_Cerrado: return minusculas ? "aplicada" : "Aplicada";
                case enumEtapasDeRegularizacion.RAL_Cancelar: return minusculas ? "cancelada" : "Cancelada";
            }
            return etapa.ToString();
        }

    }

}
