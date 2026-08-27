using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumEtapasDeAlmacen
    {
        [Description("Ids de estados en los que un almacén está activo")]
        ALM_Etapa_Activo,
        [Description("Ids de estados en los que un almacén está en inventario")]
        ALM_Etapa_En_Inventario,
        [Description("Ids de estados en los que un almacén está cerrado")]
        ALM_Etapa_Cerrado,
        [Description("Ids de estados en los que un almacén es cancelado")]
        ALM_Etapa_Cancelado
    }

    public enum enumParametrosDeAlmacenes
    {
    }

    public static class VariableDeAlmacenes
    {
        private static string etapaActivo => enumNegocio.Almacen.Parametro(enumEtapasDeAlmacen.ALM_Etapa_Activo)?.Valor ?? null;
        private static string etapaEnInventario => enumNegocio.Almacen.Parametro(enumEtapasDeAlmacen.ALM_Etapa_En_Inventario)?.Valor ?? null;
        private static string etapaCerrado => enumNegocio.Almacen.Parametro(enumEtapasDeAlmacen.ALM_Etapa_Cerrado)?.Valor ?? null;
        private static string etapaCancelado => enumNegocio.Almacen.Parametro(enumEtapasDeAlmacen.ALM_Etapa_Cancelado)?.Valor ?? null;

        public enum enumMotivoTransicion { };


        public static string Estados(this enumEtapasDeAlmacen etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeAlmacen.ALM_Etapa_Activo: estados = etapaActivo; break;
                case enumEtapasDeAlmacen.ALM_Etapa_En_Inventario: estados = etapaEnInventario; break;
                case enumEtapasDeAlmacen.ALM_Etapa_Cerrado: estados = etapaCerrado; break;
                case enumEtapasDeAlmacen.ALM_Etapa_Cancelado: estados = etapaCancelado; break;

            }

            return estados.IsNullOrEmpty() ? enumNegocio.Almacen.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this AlmacenDtm almacen, enumEtapasDeAlmacen etapa) => etapa.Lista().Contains(almacen.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeAlmacen> etapas, enumEtapasDeAlmacen etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this AlmacenDtm almacen, List<enumEtapasDeAlmacen> etapas)
        {
            var etapasDelAlmacen = almacen.Etapas();
            foreach (var etapa in etapas)
                if (etapasDelAlmacen.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDeAlmacen etapa) EstadosDeLaEtapa(this enumEtapasDeAlmacen etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeAlmacen etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeAlmacen> Etapas(this AlmacenDtm almacen)
        {
            var etapas = new List<enumEtapasDeAlmacen>();
            if (almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo))
                etapas.Add(enumEtapasDeAlmacen.ALM_Etapa_Activo);
            if (almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_En_Inventario))
                etapas.Add(enumEtapasDeAlmacen.ALM_Etapa_En_Inventario);
            if (almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cerrado))
                etapas.Add(enumEtapasDeAlmacen.ALM_Etapa_Cerrado);
            if (almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cancelado))
                etapas.Add(enumEtapasDeAlmacen.ALM_Etapa_Cancelado);
            return etapas;
        }

        public static string CadenaDeEtapas(this AlmacenDtm almacen) => string.Join(Simbolos.separadorDeEtapas, almacen.Etapas());

        public static enumEtapasDeAlmacen Etapa(this AlmacenDtm almacen)
        {
            var etapas = almacen.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa del {enumNegocio.Almacen.Singular(true)}, " +
                    $"cuando éste está en el estado {almacen.Propiedad<EstadoDtm>(typeof(EstadoDeUnAlmacenDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado del almacén {enumNegocio.Almacen.Singular(true)} '{almacen.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeAlmacen etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeAlmacen.ALM_Etapa_Activo: return minusculas ? "activo" : "Activo";
                case enumEtapasDeAlmacen.ALM_Etapa_En_Inventario: return minusculas ? "en inventario" : "En inventario";
                case enumEtapasDeAlmacen.ALM_Etapa_Cerrado: return minusculas ? "cerrado" : "Cerrado";
                case enumEtapasDeAlmacen.ALM_Etapa_Cancelado: return minusculas ? "cancelado" : "Cancelado";
            }
            return etapa.ToString();
        }

    }

}
