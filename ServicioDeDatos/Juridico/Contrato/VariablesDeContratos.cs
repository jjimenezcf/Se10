using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    public enum enumParametrosDeContratos
    {
        [Description("Indica cuando se avisará al usuario conectado que sobrepasa el porcentaje del saldo del contrato")]
        CTR_Porcentaje_Aviso,
        [Description("Indica cuando se bloqueará el uso del contrato por haber sobrepasado el saldo")]
        CTR_Porcentaje_Bloqueo,
        [Description("Indica para un motivo y un estado del contrato que transición se le aplica")]
        CTR_Aplicar_Transicion
    }
    public enum enumEtapasDeContratos
    {
        [Description("Ids de estados en los que un contrato está en elaboración")]
        CTR_Etapa_En_Elaboracion,
        [Description("Ids de estados en los que un contrato esta vigente")]
        CTR_Etapa_Vigente,
        [Description("Ids de estados en los que un contrato esta pendiente de ser prorrogado")]
        CTR_Etapa_Pdt_Prorroga,
        [Description("Ids de estados en los que un contrato esta derogado")]
        CTR_Etapa_Derogado,
        [Description("Ids de estados en los que un contrato esta fianlizado")]
        CTR_Etapa_Finalizacion,
        [Description("Ids de estados en los que un contrato esta cancelado")]
        CTR_Etapa_Cancelado
    }

    public static class VariablesDeContratos
    {
        public static string porcentageAviso => enumNegocio.Contrato.Parametro(enumParametrosDeContratos.CTR_Porcentaje_Aviso)?.Valor ?? null;
        public static string porcentageBloqueo => enumNegocio.Contrato.Parametro(enumParametrosDeContratos.CTR_Porcentaje_Bloqueo)?.Valor ?? null;


        public static string etapaDeElaboracion => enumNegocio.Contrato.Parametro(enumEtapasDeContratos.CTR_Etapa_En_Elaboracion)?.Valor ?? null;
        public static string etapaVigente => enumNegocio.Contrato.Parametro(enumEtapasDeContratos.CTR_Etapa_Vigente)?.Valor ?? null;
        public static string etapaFinalizacion => enumNegocio.Contrato.Parametro(enumEtapasDeContratos.CTR_Etapa_Finalizacion)?.Valor ?? null;
        public static string etapaPdtProrroga => enumNegocio.Contrato.Parametro(enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga)?.Valor ?? null;
        public static string etapaDerogado => enumNegocio.Contrato.Parametro(enumEtapasDeContratos.CTR_Etapa_Derogado)?.Valor ?? null;
        public static string etapaCancelado => enumNegocio.Contrato.Parametro(enumEtapasDeContratos.CTR_Etapa_Cancelado)?.Valor ?? null;

        public enum enumMotivoTransicion { PdtProrroga, Prorrogar, Iniciar, Finalizar };

        public static string TransicionesPorMotivo => enumNegocio.Contrato.Parametro(enumParametrosDeContratos.CTR_Aplicar_Transicion)?.Valor ?? null;

        public static string Estados(this enumEtapasDeContratos etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeContratos.CTR_Etapa_En_Elaboracion: estados = etapaDeElaboracion; break;
                case enumEtapasDeContratos.CTR_Etapa_Vigente: estados = etapaVigente; break;
                case enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga: estados = etapaPdtProrroga; break;
                case enumEtapasDeContratos.CTR_Etapa_Derogado: estados = etapaDerogado; break;
                case enumEtapasDeContratos.CTR_Etapa_Finalizacion: estados = etapaFinalizacion; break;
                case enumEtapasDeContratos.CTR_Etapa_Cancelado: estados = etapaCancelado; break;
            }

            return estados.IsNullOrEmpty() ? enumNegocio.Contrato.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static (List<int> estados, enumEtapasDeContratos etapa) EstadosDeLaEtapa(this enumEtapasDeContratos etapa) => (etapa.Lista(), etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this ContratoDtm contrato, List<enumEtapasDeContratos> etapas)
        {
            var etapasDeLaFactura = contrato.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static List<enumEtapasDeContratos> Etapas(this ContratoDtm contrato)
        {
            var etapas = new List<enumEtapasDeContratos>();
            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_En_Elaboracion))
                etapas.Add(enumEtapasDeContratos.CTR_Etapa_En_Elaboracion);
            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Vigente))
                etapas.Add(enumEtapasDeContratos.CTR_Etapa_Vigente);
            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Finalizacion))
                etapas.Add(enumEtapasDeContratos.CTR_Etapa_Finalizacion);
            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga))
                etapas.Add(enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga);
            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Derogado))
                etapas.Add(enumEtapasDeContratos.CTR_Etapa_Derogado);
            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Cancelado))
                etapas.Add(enumEtapasDeContratos.CTR_Etapa_Cancelado);
            return etapas;
        }

        public static bool EstaEnLaEtapa(this ContratoDtm contrato, enumEtapasDeContratos etapa) => etapa.Lista().Contains(contrato.IdEstado);

        public static List<int> Lista(this enumEtapasDeContratos etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static string Nombre(this enumEtapasDeContratos etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeContratos.CTR_Etapa_En_Elaboracion: return minusculas ? "en elaboración" : "En elaboración";
                case enumEtapasDeContratos.CTR_Etapa_Vigente: return minusculas ? "vigente" : "Vigente";
                case enumEtapasDeContratos.CTR_Etapa_Finalizacion: return minusculas ? "finalización" : "Finalización";
                case enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga: return minusculas ? "pendiente de prórroga" : "Pendiente de prórroga";
                case enumEtapasDeContratos.CTR_Etapa_Derogado: return minusculas ? "derogado" : "Derogado";
                case enumEtapasDeContratos.CTR_Etapa_Cancelado: return minusculas ? "cancelado" : "Cancelado";
            }
            return etapa.ToString();
        }

    }

}
