using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    public enum enumEtapasDeRemesasPag
    {
        [Description("Ids de estados en los que una remesa está en cumplimentación")]
        REM_Etapa_De_Cumplimentacion,
        [Description("Ids de estados en los que una remesa está generada y aun no enviada al banco")]
        REM_Etapa_Generada,
        [Description("Ids de estados en los que una remesa ha sido enviada al banco")]
        REM_Etapa_De_Presentacion,
        [Description("Ids de estados en los que una remesa ya cerrada")]
        REM_Etapa_De_Cierre,
        [Description("Ids de estados en los que una remesa está cancelada")]
        REM_Etapa_Cancelada
    }

    public enum enumParametrosDeRemesasPag
    {

    }

    public static class VariableDeRemesasPag
    {
        private static string etapaEnCumplimentacion => enumNegocio.RemesaPag.Parametro(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion)?.Valor ?? null;
        private static string etapaGenerada => enumNegocio.RemesaPag.Parametro(enumEtapasDeRemesasPag.REM_Etapa_Generada)?.Valor ?? null;
        private static string etapaDePresentacion => enumNegocio.RemesaPag.Parametro(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion)?.Valor ?? null;
        private static string etapaDeCierre => enumNegocio.RemesaPag.Parametro(enumEtapasDeRemesasPag.REM_Etapa_De_Cierre)?.Valor ?? null;
        private static string etapaDeCancelacion => enumNegocio.RemesaPag.Parametro(enumEtapasDeRemesasPag.REM_Etapa_Cancelada)?.Valor ?? null;

        public enum Parametro
        {
            REM_DiasDeTransferencia
        }

        public static string Estados(this enumEtapasDeRemesasPag etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion: estados = etapaEnCumplimentacion; break;
                case enumEtapasDeRemesasPag.REM_Etapa_Generada: estados = etapaGenerada; break;
                case enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion: estados = etapaDePresentacion; break;
                case enumEtapasDeRemesasPag.REM_Etapa_De_Cierre: estados = etapaDeCierre; break;
                case enumEtapasDeRemesasPag.REM_Etapa_Cancelada: estados = etapaDeCancelacion; break;
            }
            return estados.IsNullOrEmpty() ? enumNegocio.RemesaPag.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this RemesaPagDtm remesa, enumEtapasDeRemesasPag etapa) => etapa.Lista().Contains(remesa.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeRemesasPag> etapas, enumEtapasDeRemesasPag etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this RemesaPagDtm remesa, List<enumEtapasDeRemesasPag> etapas)
        {
            var etapasDeLaFactura = remesa.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDeRemesasPag etapa) EstadosDeLaEtapa(this enumEtapasDeRemesasPag etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeRemesasPag etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeRemesasPag> Etapas(this RemesaPagDtm remesa)
        {
            var etapas = new List<enumEtapasDeRemesasPag>();
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion))
                etapas.Add(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_Generada))
                etapas.Add(enumEtapasDeRemesasPag.REM_Etapa_Generada);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion))
                etapas.Add(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Cierre))
                etapas.Add(enumEtapasDeRemesasPag.REM_Etapa_De_Cierre);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_Cancelada))
                etapas.Add(enumEtapasDeRemesasPag.REM_Etapa_Cancelada);
            return etapas;
        }

        public static List<string> ListaDeEtapas(this RemesaPagDtm remesa) => remesa.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static string CadenaDeEtapas(this RemesaPagDtm remesa) => string.Join(Simbolos.separadorDeEtapas, remesa.Etapas());

        public static enumEtapasDeRemesasPag Etapa(this RemesaPagDtm remesa)
        {
            var etapas = remesa.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.RemesaPag.Singular(true)}, " +
                    $"cuando éste está en el estado {remesa.Propiedad<EstadoDtm>(typeof(EstadoDeUnaRemesaPagDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado de la remesa {enumNegocio.RemesaPag.Singular(true)} '{remesa.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeRemesasPag etapa, bool minusculas)
        {
            switch (etapa)
            {
                case enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion: return minusculas ? "cumplimentandose" : "Cumplimentandose";
                case enumEtapasDeRemesasPag.REM_Etapa_Generada: return minusculas ? "generada" : "Generada";
                case enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion: return minusculas ? "presentada" : "Presentada";
                case enumEtapasDeRemesasPag.REM_Etapa_De_Cierre: return minusculas ? "cerrada" : "Cerrada";
                case enumEtapasDeRemesasPag.REM_Etapa_Cancelada: return minusculas ? "cancelada" : "Cancelada";
            }
            return etapa.ToString();
        }

    }

}
