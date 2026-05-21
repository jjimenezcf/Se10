using ServicioDeDatos;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Ventas
{
    /// <summary>
    /// Genera facturas electrónicas en formato UBL 2.5 (OASIS CS01, abril 2026).
    /// Perfil EN 16931 base, sin extensión Peppol.
    /// Formato de referencia para la copia fiel exigida por la solución pública de la AEAT
    /// según el borrador de Orden Ministerial de la Ley Crea y Crece.
    /// </summary>
    public class GeneradorDeFacturaUbl25 : GeneradorDeFacturaUbl
    {
        protected override string UblVersionID    => "2.5";
        protected override string CustomizationID => "urn:cen.eu:en16931:2017";

        public GeneradorDeFacturaUbl25(ContextoSe contexto, FacturaEmtDtm factura, string rutaConFichero)
            : base(contexto, factura, rutaConFichero)
        {
        }
    }
}
