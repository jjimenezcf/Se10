using Newtonsoft.Json;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    public enum enumEtapasDePartesTr
    {
        [Description("Ids de estados en los que un parte está pendiente")]
        PTR_Etapa_Pendiente,
        [Description("Ids de estados en los que se puede asociar un parte de trabajo a una factura")]
        PTR_Etapa_Pdt_Facturar,
        [Description("Ids de estados en los que un parte está prefacturado o facturado")]
        PTR_Etapa_Facturado,
        [Description("Ids de estados en los que un parte está cancelado")]
        PTR_Etapa_Cancelado
    }

    public enum enumParametrosDePartes
    {
        [Description("Indica para un motivo y un estado del parte de trabajo que transición se le aplica")]
        PTR_Aplicar_Transicion
    }

    public static class VariableDePartesTr
    {
        public static string FaltaDefinirParametro(string mensaje, Enum parametro)
        => 
        mensaje + $". Falta definir el parámetro: '{parametro.ToString()}' en las variables de partes de trabajo.";


        public enum enumMotivoTransicion { EliminarParteDeUnaLineaDeFactura, AnularPrefactura, FacturarParteDeUnContrato };

        public static string HorasDeJornada => CacheDeVariable.ObtenerVariable(Variable.PTR_Horas_De_Jornada, Descripciones.PTR_Horas_De_Jornada, "8");

        //public static string TransicionesPorMotivo => enumVariablesDePartes.PTR_Aplicar_Transicion.LeerVariable(JsonConvert.SerializeObject(new List<object> {
        //    new TransicionPorMotivo
        //        {
        //            Motivo = "",
        //            IdEstado = 0,
        //            IdTransicion = 0
        //        }
        //    }
        //));

        public static string TransicionesPorMotivo
        {
            get
            {
                string jsonVacio = JsonConvert.SerializeObject(new List<object> {new TransicionPorMotivo
                        {
                            Motivo = "",
                            IdEstado = 0,
                            IdTransicion = 0
                        }
                    }
                );
                var transiciones = enumNegocio.ParteDeTrabajo.Parametro(enumParametrosDePartes.PTR_Aplicar_Transicion, valorPorDefecto: jsonVacio);
                return transiciones.Valor;
            }
        }


        private static string etapaPendiente => enumNegocio.ParteDeTrabajo.Parametro(enumEtapasDePartesTr.PTR_Etapa_Pendiente)?.Valor ?? null;
        private static string etapaDePdtFacturacion => enumNegocio.ParteDeTrabajo.Parametro(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar)?.Valor ?? null;
        private static string etapaFacturado => enumNegocio.ParteDeTrabajo.Parametro(enumEtapasDePartesTr.PTR_Etapa_Facturado)?.Valor ?? null;
        private static string etapaCancelado => enumNegocio.ParteDeTrabajo.Parametro(enumEtapasDePartesTr.PTR_Etapa_Cancelado)?.Valor ?? null;

        public static List<int> Lista(this enumEtapasDePartesTr etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);
        public static (List<int> estados, enumEtapasDePartesTr etapa) EstadosDeLaEtapa(this enumEtapasDePartesTr etapa) => (etapa.Lista(), etapa);

        public static string Estados(this enumEtapasDePartesTr etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePartesTr.PTR_Etapa_Pendiente: estados = etapaPendiente; break;
                case enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar: estados = etapaDePdtFacturacion; break;
                case enumEtapasDePartesTr.PTR_Etapa_Facturado: estados = etapaFacturado; break;
                case enumEtapasDePartesTr.PTR_Etapa_Cancelado: estados = etapaCancelado; break;
            }
            return estados.IsNullOrEmpty() ? enumNegocio.ParteDeTrabajo.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }


        public static bool EstaEnAlgunaDeLasEtapa(this ParteTrDtm parteTr, List<enumEtapasDePartesTr> etapas)
        {
            var etapasDeLaFactura = parteTr.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static bool EstaEnLaEtapa(this ParteTrDtm parte, enumEtapasDePartesTr etapa) => etapa.Lista().Contains(parte.IdEstado);

        private static bool EstaEnLaEtapa(this ParteTrDtm parte, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(parte.IdEstado);

        public static List<enumEtapasDePartesTr> Etapas(this ParteTrDtm factura)
        {
            var etapas = new List<enumEtapasDePartesTr>();
            if (factura.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pendiente))
                etapas.Add(enumEtapasDePartesTr.PTR_Etapa_Pendiente);
            if (factura.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Cancelado))
                etapas.Add(enumEtapasDePartesTr.PTR_Etapa_Cancelado);
            if (factura.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar))
                etapas.Add(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar);
            if (factura.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Facturado))
                etapas.Add(enumEtapasDePartesTr.PTR_Etapa_Facturado);
            return etapas;
        }


        public static enumEtapasDePartesTr Etapa(this ParteTrDtm parte)
        {
            if (parte.EstaEnLaEtapa(etapaPendiente)) return enumEtapasDePartesTr.PTR_Etapa_Pendiente;
            else
            if (parte.EstaEnLaEtapa(etapaDePdtFacturacion)) return enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar;
            else
            if (parte.EstaEnLaEtapa(etapaFacturado)) return enumEtapasDePartesTr.PTR_Etapa_Facturado;
            else
            if (parte.EstaEnLaEtapa(etapaCancelado)) return enumEtapasDePartesTr.PTR_Etapa_Cancelado;

            throw new Exception($"No se ha definido la etapa del parte de trabajo, " +
                $"cuando éste está en el estado {parte.Propiedad<EstadoDtm>(typeof(EstadoDeUnParteTrDtm)).Nombre}");
        }
    }

}
