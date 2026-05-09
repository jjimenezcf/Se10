using Gestor.Errores;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    public enum enumEtapasDePpts
    {
        [Description("Ids de estados en los que un ppt está elaborándose")]
        PPT_Etapa_Elaboracion,
        [Description("Ids de estados en los que un ppt está pendiente de respuesta del cliente")]
        PPT_Etapa_Pendiente,
        [Description("Ids de estados en los que un ppt está aceptado")]
        PPT_Etapa_Aceptado,
        [Description("Ids de estados en los que un ppt puede ser facturado")]
        PPT_Etapa_PermiteFacturar,
        [Description("Ids de estados en los que un ppt ha sido rechazado")]
        PPT_Etapa_Rechazo,
        [Description("Ids de estados en los que un ppt está cancelado")]
        PPT_Etapa_Cancelado,
        [Description("Ids de estados en los que se puede asociar un ppt a un parte de trabajo")]
        PPT_Etapa_AsociarParteTr
    }

    public enum enumParametrosDePresupuesto
    {
        [Description("Indica el id de la unidad de medida por defecto")]
        PPT_Unidad_Medida,
        [Description("Indica el id de la naturaleza por defecto")]
        PPT_Naturaleza,
        [Description("Indica el tipo de línea por defecto")]
        PPT_TipoDeLinea,
        [Description("Indica la clase de unitario por defecto")]
        PPT_ClaseDeUnitario,
        [Description("Indica los datos por defecto para usar en la impresión de un ppt")]
        PPT_DatosDeImpresion,
        [Description("Indica el incremento al enumerar las filas")]
        PPT_IncrementarOrdenEn
    }

    public static class VariableDePpts
    {
        private static string etapaElaboracion => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_Elaboracion)?.Valor ?? null;
        private static string etapaPendiente => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_Pendiente)?.Valor ?? null;
        private static string etapaDeAceptado => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_Aceptado)?.Valor ?? null;
        private static string etapaDePermiteFacturar => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_PermiteFacturar)?.Valor ?? null;
        private static string etapaRechazado => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_Rechazo)?.Valor ?? null;
        private static string etapaAsociarParteTr => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_AsociarParteTr)?.Valor ?? null;
        private static string etapaCancelado => enumNegocio.Presupuesto.Parametro(enumEtapasDePpts.PPT_Etapa_Cancelado)?.Valor ?? null;

        public static string Estados(this enumEtapasDePpts etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePpts.PPT_Etapa_Elaboracion: estados = etapaElaboracion; break;
                case enumEtapasDePpts.PPT_Etapa_Pendiente: estados = etapaPendiente; break;
                case enumEtapasDePpts.PPT_Etapa_Aceptado: estados = etapaDeAceptado; break;
                case enumEtapasDePpts.PPT_Etapa_PermiteFacturar: estados = etapaDePermiteFacturar; break;
                case enumEtapasDePpts.PPT_Etapa_Rechazo: estados = etapaRechazado; break;
                case enumEtapasDePpts.PPT_Etapa_AsociarParteTr: estados = etapaAsociarParteTr; break;
                case enumEtapasDePpts.PPT_Etapa_Cancelado: estados = etapaCancelado; break;
            }
            return estados.IsNullOrEmpty() ? enumNegocio.Presupuesto.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this PresupuestoDtm ppt, enumEtapasDePpts etapa) => etapa.Lista().Contains(ppt.IdEstado);

        public static bool EstaEnAlgunaDeLasEtapa(this PresupuestoDtm ppt, List<enumEtapasDePpts> etapas)
        {
            var etapasDeLaTarea = ppt.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaTarea.Contains(etapa)) return true;
            return false;
        }

        public static List<int> Lista(this enumEtapasDePpts etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDePpts> Etapas(this PresupuestoDtm ppt)
        {
            var etapas = new List<enumEtapasDePpts>();

            if (ppt.EstaEnLaEtapa(etapaElaboracion)) etapas.Add(enumEtapasDePpts.PPT_Etapa_Elaboracion);

            if (ppt.EstaEnLaEtapa(etapaPendiente)) etapas.Add(enumEtapasDePpts.PPT_Etapa_Pendiente);

            if (ppt.EstaEnLaEtapa(etapaDePermiteFacturar)) etapas.Add(enumEtapasDePpts.PPT_Etapa_PermiteFacturar);

            if (ppt.EstaEnLaEtapa(etapaCancelado)) etapas.Add(enumEtapasDePpts.PPT_Etapa_Cancelado);

            if (ppt.EstaEnLaEtapa(etapaDeAceptado)) etapas.Add(enumEtapasDePpts.PPT_Etapa_Aceptado);

            if (ppt.EstaEnLaEtapa(etapaRechazado)) etapas.Add(enumEtapasDePpts.PPT_Etapa_Rechazo);

            if (ppt.EstaEnLaEtapa(etapaAsociarParteTr)) etapas.Add(enumEtapasDePpts.PPT_Etapa_AsociarParteTr);

            if (etapas.Count == 0)
                Excepciones.EtapaNoDefinida(enumNegocio.Presupuesto, ppt.Propiedad<EstadoDtm>(typeof(EstadoDeUnPresupuestoDtm)).Nombre);

            return etapas;
        }

        public static enumEtapasDePpts Etapa(this PresupuestoDtm ppt)
        {
            var etapas = ppt.Etapas();
            if (etapas.Count == 0)
                Excepciones.EtapaNoDefinida(enumNegocio.Preasiento, ppt.Propiedad<EstadoDtm>(typeof(EstadoDeUnPreasientoDtm)).Nombre);

            if (etapas.Count > 1)
                Excepciones.MasDeUnaEtapa(enumNegocio.Preasiento, ppt.Referencia, etapas);

            return etapas[0];
        }

        public static List<string> ListaDeEtapas(this PresupuestoDtm ppt) => ppt.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);


        public static string Nombre(this enumEtapasDePpts etapa, bool minusculas)
        {
            switch (etapa)
            {
                case enumEtapasDePpts.PPT_Etapa_AsociarParteTr: return minusculas ? "asociación de partes" : "Asociación de partes";
                case enumEtapasDePpts.PPT_Etapa_PermiteFacturar: return minusculas ? "facturación" : "Facturación";
                case enumEtapasDePpts.PPT_Etapa_Elaboracion: return minusculas ? "elaboración" : "Elaboración";
                case enumEtapasDePpts.PPT_Etapa_Pendiente: return minusculas ? "pendiente de cliente" : "Pendiente de cliente";
                case enumEtapasDePpts.PPT_Etapa_Aceptado: return minusculas ? "aceptado" : "Aceptado";
            }
            return etapa.ToString();
        }

        private static string CadenaDeEtapas(this PresupuestoDtm ppt) => string.Join(Simbolos.separadorDeEtapas, ppt.Etapas());

        private static bool EstaEnLaEtapa(this PresupuestoDtm ppt, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(ppt.IdEstado);

    }

}
