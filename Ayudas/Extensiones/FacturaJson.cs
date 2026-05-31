using System.Collections.Generic;

namespace Utilidades
{
    public class LineaFacturaJson
    {
        public string Concepto { get; set; }
        public decimal BaseImponible { get; set; } = 0;
        public decimal PorcentajeIva { get; set; } = 0;
        public decimal ImporteIva { get; set; } = 0;
        public bool Exenta { get; set; } = false;
    }

    public class IrpfFacturaJson
    {
        public decimal PorcentajeRetencion { get; set; } = 0;
        public decimal BaseRetencion { get; set; } = 0;
        public decimal ImporteRetencion { get; set; } = 0;
    }

    public class FacturaJson
    {
        public string Proveedor { get; set; }
        public string Nif { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public string NumeroFactura { get; set; }
        public string Concepto { get; set; }
        public string Fecha { get; set; }
        public string FechaVencimiento { get; set; }
        public decimal Total { get; set; } = 0;
        public decimal BaseImponible { get; set; } = 0;
        public decimal TotalIva { get; set; } = 0;
        public decimal TotalIrpf { get; set; } = 0;
        public string FormaDePago { get; set; }
        public string ClaseDePago { get; set; }
        public string CuentaBancaria { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string Municipio { get; set; }
        public string TipoDeVia { get; set; }
        public string Calle { get; set; }
        public string NumeroPolicia { get; set; }
        public string RestoDireccion { get; set; }
        /// <summary>Clase rectificativa: "OR" (rectificativa), "OC" (sustitutiva) o null (ordinaria).</summary>
        public string ClaseRectificativa { get; set; }
        public List<LineaFacturaJson> Lineas { get; set; }
        public IrpfFacturaJson Irpf { get; set; }
    }
}
