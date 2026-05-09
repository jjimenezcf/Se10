using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    public enum enumEtapasDeRemesasFae
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


    public enum enumParametrosDeRemesasFae
    {
        [Description("Indica el nº de días que se espera por defecto para dar por cargado en mi cuenta bancaria el pago de un recibo remesado")]
        REM_DiasDeEsperaDeCargo,
        [Description("Indica el nº de meses que se le permite a un cliente devolver el cargo de un cobro remesado")]
        REM_MesesDeEsperaDeDevolucionDeCargos,
        [Description("Crear las etapas si no están definidas, asignándole como valor el pasado por defecto")]
        REM_CrearEtapa

    }

    public static class VariableDeRemesasFae
    {
        private static string etapaEnCumplimentacion => enumNegocio.RemesaFae.Parametro(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion, emitirError: false)?.Valor ?? null;
        private static string etapaGenerada => enumNegocio.RemesaFae.Parametro(enumEtapasDeRemesasFae.REM_Etapa_Generada, emitirError: false)?.Valor ?? null;
        private static string etapaDePresentacion => enumNegocio.RemesaFae.Parametro(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion, emitirError: false)?.Valor ?? null;
        private static string etapaDeCierre => enumNegocio.RemesaFae.Parametro(enumEtapasDeRemesasFae.REM_Etapa_De_Cierre, emitirError: false)?.Valor ?? null;
        private static string etapaDeCancelacion => enumNegocio.RemesaFae.Parametro(enumEtapasDeRemesasFae.REM_Etapa_Cancelada, emitirError: false)?.Valor ?? null;

        public static string Estados(this enumEtapasDeRemesasFae etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion: estados = etapaEnCumplimentacion; break;
                case enumEtapasDeRemesasFae.REM_Etapa_Generada: estados = etapaGenerada; break;
                case enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion: estados = etapaDePresentacion; break;
                case enumEtapasDeRemesasFae.REM_Etapa_De_Cierre: estados = etapaDeCierre; break;
                case enumEtapasDeRemesasFae.REM_Etapa_Cancelada: estados = etapaDeCancelacion; break;
            }

            return estados.IsNullOrEmpty() ? enumNegocio.RemesaFae.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this RemesaFaeDtm remesa, enumEtapasDeRemesasFae etapa) => etapa.Lista().Contains(remesa.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeRemesasFae> etapas, enumEtapasDeRemesasFae etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this RemesaFaeDtm remesa, List<enumEtapasDeRemesasFae> etapas)
        {
            var etapasDeLaFactura = remesa.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDeRemesasFae etapa) EstadosDeLaEtapa(this enumEtapasDeRemesasFae etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeRemesasFae etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeRemesasFae> Etapas(this RemesaFaeDtm remesa)
        {
            var etapas = new List<enumEtapasDeRemesasFae>();
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion))
                etapas.Add(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_Generada))
                etapas.Add(enumEtapasDeRemesasFae.REM_Etapa_Generada);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion))
                etapas.Add(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_De_Cierre))
                etapas.Add(enumEtapasDeRemesasFae.REM_Etapa_De_Cierre);
            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_Cancelada))
                etapas.Add(enumEtapasDeRemesasFae.REM_Etapa_Cancelada);
            return etapas;
        }

        public static List<string> ListaDeEtapas(this RemesaFaeDtm remesa) => remesa.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static string CadenaDeEtapas(this RemesaFaeDtm remesa) => string.Join(Simbolos.separadorDeEtapas, remesa.Etapas());

        public static enumEtapasDeRemesasFae Etapa(this RemesaFaeDtm remesa)
        {
            var etapas = remesa.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.RemesaFae.Singular(true)}, " +
                    $"cuando éste está en el estado {remesa.Propiedad<EstadoDtm>(typeof(EstadoDeUnaRemesaFaeDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado de la remesa {enumNegocio.RemesaFae.Singular(true)} '{remesa.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeRemesasFae etapa, bool minusculas)
        {
            switch (etapa)
            {
                case enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion: return minusculas ? "cumplimentandose" : "Cumplimentandose";
                case enumEtapasDeRemesasFae.REM_Etapa_Generada: return minusculas ? "generada" : "Generada";
                case enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion: return minusculas ? "presentada" : "Presentada";
                case enumEtapasDeRemesasFae.REM_Etapa_De_Cierre: return minusculas ? "cerrada" : "Cerrada";
                case enumEtapasDeRemesasFae.REM_Etapa_Cancelada: return minusculas ? "cancelada" : "Cancelada";
            }
            return etapa.ToString();
        }

    }

}
