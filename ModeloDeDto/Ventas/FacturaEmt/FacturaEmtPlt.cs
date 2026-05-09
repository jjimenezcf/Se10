using ModeloDeDto.SistemaDocumental;
using System.ComponentModel;
using Utilidades;

namespace ModeloDeDto.Ventas
{

    public enum enumPltFacturaEmtRpt
    {
        [Description("Prefactura")]
        Prefactura,
        [Description("Copia de factura")]
        CopiaDeFactura
    }

    public class PrefacturaPlt : IPlantillaPlt
    {
        public int IdPlantilla { get; set; } = -1;
        public string Plantilla { get; set; } = enumPltFacturaEmtRpt.Prefactura.Descripcion();
        public enumClaseDePlantilla Clase { get; set; } = enumClaseDePlantilla.programada;
    }

    public class CopiaDeFacturaPlt : IPlantillaPlt
    {
        public int IdPlantilla { get; set; } = -1;
        public string Plantilla { get; set; } = enumPltFacturaEmtRpt.CopiaDeFactura.Descripcion();
        public enumClaseDePlantilla Clase { get; set; } = enumClaseDePlantilla.programada;
    }


}
