using System.Collections.Generic;

namespace ServicioDeDatos.Elemento
{
    public enum enumOperacionDeTotales
    {
        Cuenta,
        Suma,
        Media,
        Max,
        Min
    }

    public class MetricaDeTotales
    {
        public enumOperacionDeTotales Operacion { get; set; }
        public string Campo { get; set; }
        public string Alias { get; set; }

        // Sólo al agrupar por una clave multivaluada (p. ej. Articulo en facturas emitidas, un elemento cae en varios grupos): valor de
        // esas claves en el grupo que se está calculando, para que el campo calculado devuelva la parte del elemento de ese grupo
        // (las unidades del artículo en la factura, no las de toda la factura). Null en el resto de casos
        public Dictionary<string, string> ClavesDelGrupo { get; set; }

        public MetricaDeTotales(enumOperacionDeTotales operacion, string campo, string alias)
        {
            Operacion = operacion;
            Campo = campo;
            Alias = alias;
        }
    }

    public class AgrupacionDeTotales
    {
        public List<string> PropiedadesDeAgrupacion { get; set; }
        public List<MetricaDeTotales> Metricas { get; set; }

        public AgrupacionDeTotales(List<string> propiedadesDeAgrupacion, List<MetricaDeTotales> metricas)
        {
            PropiedadesDeAgrupacion = propiedadesDeAgrupacion;
            Metricas = metricas;
        }
    }

    public class FilaDeTotales
    {
        public Dictionary<string, object> Claves { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Totales { get; set; } = new Dictionary<string, object>();
    }

    public class BloqueDeTotales
    {
        public List<string> PropiedadesDeAgrupacion { get; set; }
        public List<FilaDeTotales> Filas { get; set; } = new List<FilaDeTotales>();
    }
}
