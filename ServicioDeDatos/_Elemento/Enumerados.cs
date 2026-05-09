using System.ComponentModel;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Elemento
{
    public static class enumDurabilidadExt
    {
        public static string ToUnidadDeMedida(this enumDurabilidad durabilidad)
        {
            switch (durabilidad)
            {
                case enumDurabilidad.Dias: return "Dia";
                case enumDurabilidad.Jornadas: return "Jornada";
                case enumDurabilidad.Horas: return "Hora";
                case enumDurabilidad.Minutos: return "Minuto";
            }
            return durabilidad.ToString();
        }
    }

    public class Enumerados
    {
        public enum enumTipoDeLinea
        {
            [Description("Elemento del catálogo")]
            Unitario,
            [Description("Partida alzada")]
            Alzada,
            [Description("Comentario")]
            Comentario,
        }
        
        public enum enumMedidoEn_AM { [Description("Años")] Ano, [Description("Meses")] Mes }


        public enum enumPeriodicidad { Anual, Mensual, Semanal, Diaria }

        public enum enumDurabilidad
        {
            [Description("Días")] 
            Dias, 
            Jornadas,
            Horas, 
            Minutos 
        }

        public static class ltrImpuesto
        {
            public static readonly string Exento = nameof(Exento);
            public static readonly string Iva = "Iva %";
            public static readonly string Irpf = "Irpf %";
        }



        public enum enumClaseDeLineaFar
        {
            [Description("Base imponible")]
            BaseImponible,
            [Description("BI con Iva")]
            BiConIva, 
            [Description("BI Exenta")]
            BiExenta,
            [Description("Línea de IVA")]
            LineaDeIva,
            [Description("Línea de IRPF")]
            LineaDeIrpf,
        }

        public enum enumImporteFar
        {
            [Description("Base imponible")]
            BaseImponible,
            [Description("Total a pagar")]
            TotalPagar,
            [Description("Total factura")]
            TotalFactura,
            [Description("Total irpf")]
            TotalIrpf,
            [Description("Total iva")]
            TotalIva,
        }

        public enum enumImporteEmt
        {
            [Description("Sin descuento")]
            SinDescuento,
            [Description("Base imponible")]
            BaseImponible,
            [Description("Facturado")]
            BiConIva,
            [Description("Iva")]
            Iva,
            [Description("Irpf")]
            Irpf,
            [Description("A cobrar")]
            Apagar,
        }

    }
}
