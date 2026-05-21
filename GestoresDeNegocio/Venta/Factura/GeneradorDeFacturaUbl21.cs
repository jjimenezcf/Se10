using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Ventas;
using System.Text.RegularExpressions;

namespace GestoresDeNegocio.Ventas
{
    /// <summary>
    /// Genera facturas electrónicas en formato UBL 2.1 con perfil Peppol BIS Billing 3.0 (EN 16931).
    /// Es el formato estándar para intercambio B2B europeo y red Peppol.
    /// </summary>
    public class GeneradorDeFacturaUbl21 : GeneradorDeFacturaUbl
    {
        protected override string UblVersionID => "2.1";

        //Peppol BIS 3.0 exige este campo obligatoriamente(regla PEPPOL-EN16931-R001). Sin él, el validador no reconoce la factura como "europea"
        protected override string ProfileID => "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0";

        // Perfil Peppol BIS Billing 3.0 — el más habitual en intercambio B2B europeo.
        // Si el receptor no exige Peppol, puede simplificarse a "urn:cen.eu:en16931:2017".
        protected override string CustomizationID => "urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0";

        public GeneradorDeFacturaUbl21(ContextoSe contexto, FacturaEmtDtm factura, string rutaConFichero)
            : base(contexto, factura, rutaConFichero)
        {
        }
    }
}
