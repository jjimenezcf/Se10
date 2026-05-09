using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Expediente
{
    public enum enumEtapasDeExpedientes
    {
        [Description("Ids de estados en los que se puede asociar un expediente a un presupuesto")]
        EXP_Etapa_Asociar_Presupuestos,
        [Description("Ids de estados en los que se puede asociar un expediente a un contrato de venta")]
        EXP_Etapa_Asociar_SC_Venta,
        [Description("Ids de estados en los que se puede asociar un expediente un contrato de compra")]
        EXP_Etapa_Asociar_SC_Compra,
        [Description("Ids de estados en los que se puede asociar un expediente una tarea")]
        EXP_Etapa_Asociar_Tareas,
        [Description("Ids de estados en los que un expediente está en ejecución")]
        EXP_Etapa_Ejecucion,
        [Description("Ids de estados en los que un expediente están terminados")]
        EXP_Etapa_Terminada,
        [Description("Ids de estados en los que un expediente están cancelados")]
        EXP_Etapa_Cancelada,
        [Description("Ids de estados en los que un expediente están iniciados")]
        EXP_Etapa_Inicial,
        [Description("Ids de estados en los que un auto judicial está presentado")]
        EXP_Etapa_En_Juzgado
    }

    public enum enumParametrosDeExpedientes
    {
        [Description("Indica el tipo de expediente suceptible a comunicar al responsable que se ha anexado un documento")]
        EXP_Tipos_Para_Comunicar_Responsable_Tras_Anexar,
        [Description("Indica el tipo de expediente para actividades formativas")]
        EXP_Tipos_Para_Actividades,
        [Description("Indica la expresión regular de como validar un NIG")]
        JUR_Expresion_Regular_De_Un_NIG,
        [Description("Indica la expresión regular de como validar el nº del Procedimiento")]
        JUR_Expresion_Regular_De_Un_Procedimiento,
    }

    public static class ParametrosDeExpedientes
    {
        private static string etapaDePresupuestacion => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, valorPorDefecto: 0).Valor;
        private static string etapaDeSCVenta => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta, valorPorDefecto: 0).Valor;
        private static string etapaDeSCCompra => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra, valorPorDefecto: 0).Valor;
        private static string etapaDeTareas => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, valorPorDefecto: 0).Valor;
        private static string etapaDeEjecucion => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, valorPorDefecto: 0).Valor;
        private static string etapaDeTerminacion => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Terminada, valorPorDefecto: 0).Valor;
        private static string etapaDeCancelacion => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Cancelada, valorPorDefecto: 0).Valor;
        private static string etapaDeInicio => enumNegocio.Expediente.Parametro(enumEtapasDeExpedientes.EXP_Etapa_Inicial, valorPorDefecto: 0).Valor;

        public static string Estados(this enumEtapasDeExpedientes etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos: estados = etapaDePresupuestacion; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta: estados = etapaDeSCVenta; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra: estados = etapaDeSCCompra; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas: estados = etapaDeTareas; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Ejecucion: estados = etapaDeEjecucion; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Terminada: estados = etapaDeTerminacion; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Cancelada: estados = etapaDeCancelacion; break;
                case enumEtapasDeExpedientes.EXP_Etapa_Inicial: estados = etapaDeInicio; break;
            }

            return estados.IsNullOrEmpty() ? enumNegocio.Expediente.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnAlgunaDeLasEtapa(this ExpedienteDtm expediente, List<enumEtapasDeExpedientes> etapas)
        {
            var etapasDelExpediente = expediente.Etapas();
            foreach (var etapa in etapas)
                if (etapasDelExpediente.Contains(etapa)) return true;
            return false;
        }

        public static bool EstaEnLaEtapa(this ExpedienteDtm expediente, enumEtapasDeExpedientes etapa) => etapa.Lista().Contains(expediente.IdEstado);

        public static List<int> Lista(this enumEtapasDeExpedientes etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeExpedientes> Etapas(this ExpedienteDtm expediente)
        {
            var etapas = new List<enumEtapasDeExpedientes>();
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Ejecucion))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Ejecucion);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Inicial))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Inicial);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Terminada))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Terminada);
            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Cancelada))
                etapas.Add(enumEtapasDeExpedientes.EXP_Etapa_Cancelada);

            if (etapas.Count == 0)
                Excepciones.EtapaNoDefinida(enumNegocio.Expediente, expediente.Propiedad<EstadoDtm>(typeof(EstadoDeUnExpedienteDtm)).Nombre);

            return etapas;
        }

        public static List<string> Lista(this ExpedienteDtm expediente) => expediente.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static string CadenaDeEtapas(this ExpedienteDtm expediente) => string.Join(Simbolos.separadorDeEtapas, expediente.Etapas());

        public static enumEtapasDeExpedientes Etapa(this ExpedienteDtm expediente)
        {
            var etapas = expediente.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.Expediente.Singular(true)}, " +
                    $"cuando éste está en el estado {expediente.Propiedad<EstadoDtm>(typeof(EstadoDeUnExpedienteDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado del {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeExpedientes etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas: return minusculas ? "tarea" : "Tarea";
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra: return minusculas ? "solicitud de compra" : "Solicitud de compra";
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos: return minusculas ? "presupuesto" : "Presupuesto";
                case enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta: return minusculas ? "solicitud de venta" : "Solicitud de Venta";
                case enumEtapasDeExpedientes.EXP_Etapa_Ejecucion: return minusculas ? "en ejecución" : "En ejecución";
                case enumEtapasDeExpedientes.EXP_Etapa_Terminada: return minusculas ? "terminada" : "Terminada";
            }
            return etapa.ToString();
        }

    }

    public static class ParametrosJuridicos
    {
        public static string ExpresionRegularDeUnNIG => enumNegocio.Expediente.Parametro(enumParametrosDeExpedientes.JUR_Expresion_Regular_De_Un_NIG, valorPorDefecto: "").Valor;
        public static string ExpresionRegularDeUnProcedimiento => enumNegocio.Expediente.Parametro(enumParametrosDeExpedientes.JUR_Expresion_Regular_De_Un_Procedimiento, valorPorDefecto: "").Valor;
    }


    public static class VariablesDeExpedientes
    {

        public static int IdDelTipoParaActividades(bool errorSiNoEstaDefinido = true)
        {
            var idtipoCad = enumNegocio.Expediente.Parametro(enumParametrosDeExpedientes.EXP_Tipos_Para_Actividades, valorPorDefecto: Literal.Cero).Valor.Entero();

            if (errorSiNoEstaDefinido && idtipoCad == 0)
            {
                enumNegocio.Expediente.IndicarQueFaltaDefinirElParámetro(enumParametrosDeExpedientes.EXP_Tipos_Para_Actividades);
            }

            return idtipoCad;
        }

    }
}
