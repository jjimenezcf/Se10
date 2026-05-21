using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorUbl
    {
        public const string NsInvoice = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        public const string NsCac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        public const string NsCbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    }

    /// <summary>
    /// Clase base abstracta para la generación de facturas electrónicas en formato UBL.
    /// Contiene toda la lógica de construcción del XML común a todas las versiones UBL.
    /// Las subclases concretas (GeneradorDeFacturaUbl21, GeneradorDeFacturaUbl25) solo
    /// deben proporcionar los valores que difieren entre versiones.
    /// </summary>
    public abstract class GeneradorDeFacturaUbl
    {
        // ── Namespaces ────────────────────────────────────────────────────────
        // Idénticos en UBL 2.1 y 2.5: solo el namespace raíz (Invoice-2) y cac/cbc/ext comparten URN.
        protected const string NsInvoice = ExtensorUbl.NsInvoice;
        protected const string NsCac = ExtensorUbl.NsCac;
        protected const string NsCbc = ExtensorUbl.NsCbc;

        // ── Propiedades de versión — deben implementar las subclases ──────────
        /// <summary>Valor del elemento cbc:UBLVersionID (p.ej. "2.1" o "2.5").</summary>
        protected abstract string UblVersionID { get; }

        /// <summary>
        /// Valor del elemento cbc:CustomizationID.
        /// UBL 2.1 / Peppol BIS 3.0: "urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0"
        /// UBL 2.5 / EN 16931 base:   "urn:cen.eu:en16931:2017"
        /// </summary>
        protected abstract string CustomizationID { get; }

        /// <summary>
        /// Valor de cbc:ProfileID. Solo lo emiten las subclases que lo necesiten (p.ej. UBL 2.1/Peppol).
        /// Por defecto null → no se escribe el elemento.
        /// </summary>
        protected virtual string ProfileID => null;

        // ── Estado de instancia ───────────────────────────────────────────────
        protected ContextoSe Contexto { get; }
        protected FacturaEmtDtm Factura { get; }
        protected string Ruta { get; }

        protected SociedadDtm _emisor = null;
        protected SociedadDtm Emisor => _emisor ?? Factura.Sociedad(Contexto);

        // ── Constructor ───────────────────────────────────────────────────────
        protected GeneradorDeFacturaUbl(ContextoSe contexto, FacturaEmtDtm factura, string rutaConFichero)
        {
            Contexto = contexto;
            Factura = factura;
            Ruta = rutaConFichero;
        }

        // ── Punto de entrada público ──────────────────────────────────────────
        public string Generar()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            var root = doc.CreateElement("Invoice", NsInvoice);
            root.SetAttribute("xmlns:cac", NsCac);
            root.SetAttribute("xmlns:cbc", NsCbc);
            doc.AppendChild(root);

            Cbc(doc, root, "UBLVersionID", UblVersionID);
            Cbc(doc, root, "CustomizationID", CustomizationID);

            if (!string.IsNullOrEmpty(ProfileID))
                Cbc(doc, root, "ProfileID", ProfileID);

            Cbc(doc, root, "ID", Factura.NumeroDeFactura);
            Cbc(doc, root, "IssueDate", (Factura.FacturadaEl ?? DateTime.Now).ToString("yyyy-MM-dd"));
            if (Factura.VenceEl.HasValue)
                Cbc(doc, root, "DueDate", Factura.VenceEl.Value.ToString("yyyy-MM-dd"));
            Cbc(doc, root, "InvoiceTypeCode", Factura.EsRectificativa ? "381" : "380");
            Cbc(doc, root, "DocumentCurrencyCode", Factura.Moneda);

            // PEPPOL-EN16931-R003: BuyerReference o OrderReference obligatorio en Peppol
            Cbc(doc, root, "BuyerReference", Factura.Cliente(Contexto).Id.ToString());

            if (Factura.EsRectificativa)
                AgregarReferenciaFacturaRectificada(doc, root);

            AgregarEmisor(doc, root);
            AgregarComprador(doc, root);

            if (Factura.VenceEl.HasValue)
                AgregarMedioDePago(doc, root);

            AgregarTotalesImpuestos(doc, root);
            AgregarTotalesMonetarios(doc, root);
            AgregarLineas(doc, root);

            var directorio = Path.GetDirectoryName(Ruta);
            if (directorio != null && !Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);

            var settings = new XmlWriterSettings { Indent = true, Encoding = System.Text.Encoding.UTF8 };
            using var writer = XmlWriter.Create(Ruta, settings);
            doc.Save(writer);

            return Ruta;
        }

        // ── Secciones del documento ───────────────────────────────────────────

        private void AgregarReferenciaFacturaRectificada(XmlDocument doc, XmlElement root)
        {
            var rectificada = Factura.RectificaA(Contexto);
            var billing = Cac(doc, root, "BillingReference");
            var invoiceRef = Cac(doc, billing, "InvoiceDocumentReference");
            Cbc(doc, invoiceRef, "ID", rectificada.NumeroDeFactura);
            Cbc(doc, invoiceRef, "IssueDate", (rectificada.FacturadaEl ?? DateTime.Now).ToString("yyyy-MM-dd"));
        }

        private void AgregarEmisor(XmlDocument doc, XmlElement root)
        {
            var supplierParty = Cac(doc, root, "AccountingSupplierParty");
            var party = Cac(doc, supplierParty, "Party");

            // PEPPOL-EN16931-R020: EndpointID obligatorio — NIF con schemeID="0002" (ES)
            var endpointEmisor = Cbc(doc, party, "EndpointID", Emisor.NIFConIsoEs);
            endpointEmisor.SetAttribute("schemeID", "0002");

            var partyName = Cac(doc, party, "PartyName");
            Cbc(doc, partyName, "Name", Emisor.Nombre.IsNullOrEmpty() ? Emisor.RazonSocial : Emisor.Nombre);

            var postalAddress = Cac(doc, party, "PostalAddress");
            var direccionEmisor = Factura.Sociedad(Contexto).DireccionFiscal(Contexto);
            AgregarDireccion(doc, postalAddress, direccionEmisor);

            var partyTaxScheme = Cac(doc, party, "PartyTaxScheme");
            Cbc(doc, partyTaxScheme, "CompanyID", Emisor.NIFConIsoEs);
            var taxScheme = Cac(doc, partyTaxScheme, "TaxScheme");
            Cbc(doc, taxScheme, "ID", "VAT");

            var partyLegalEntity = Cac(doc, party, "PartyLegalEntity");
            Cbc(doc, partyLegalEntity, "RegistrationName", Emisor.RazonSocial);
            Cbc(doc, partyLegalEntity, "CompanyID", Emisor.NIFConIsoEs);

            if (!Emisor.Telefono.IsNullOrEmpty() || !Emisor.eMail.IsNullOrEmpty())
            {
                var contact = Cac(doc, party, "Contact");
                if (!Emisor.Telefono.IsNullOrEmpty()) Cbc(doc, contact, "Telephone", Emisor.Telefono);
                if (!Emisor.eMail.IsNullOrEmpty()) Cbc(doc, contact, "ElectronicMail", Emisor.eMail);
            }
        }

        private void AgregarComprador(XmlDocument doc, XmlElement root)
        {
            var customerParty = Cac(doc, root, "AccountingCustomerParty");
            var party = Cac(doc, customerParty, "Party");

            // PEPPOL-EN16931-R010: EndpointID obligatorio — NIF con schemeID="0002" (ES)
            var nifReceptor = Factura.Cliente(Contexto).NIF(Contexto, quitarPrefijoEs: false);
            var endpointReceptor = Cbc(doc, party, "EndpointID", nifReceptor);
            endpointReceptor.SetAttribute("schemeID", "0002");

            var partyName = Cac(doc, party, "PartyName");
            Cbc(doc, partyName, "Name", Factura.Cliente(Contexto).RazonSocial(Contexto));

            var postalAddress = Cac(doc, party, "PostalAddress");
            var direccionCliente = Factura.DireccionFiscal(Contexto);
            AgregarDireccion(doc, postalAddress, direccionCliente);

            var partyTaxScheme = Cac(doc, party, "PartyTaxScheme");
            Cbc(doc, partyTaxScheme, "CompanyID", Factura.Cliente(Contexto).NIF(Contexto, quitarPrefijoEs: false));
            var taxScheme = Cac(doc, partyTaxScheme, "TaxScheme");
            Cbc(doc, taxScheme, "ID", "VAT");

            var partyLegalEntity = Cac(doc, party, "PartyLegalEntity");
            Cbc(doc, partyLegalEntity, "RegistrationName", Factura.Cliente(Contexto).RazonSocial(Contexto));
        }

        private void AgregarDireccion(XmlDocument doc, XmlElement parent, DireccionDto direccion)
        {
            if (!direccion.NombreDireccion.IsNullOrEmpty())
                Cbc(doc, parent, "StreetName", direccion.NombreDireccion);
            if (!direccion.Municipio.IsNullOrEmpty())
                Cbc(doc, parent, "CityName", direccion.Municipio);
            if (!direccion.CodigoPostal.IsNullOrEmpty())
                Cbc(doc, parent, "PostalZone", direccion.CodigoPostal);
            if (!direccion.Provincia.IsNullOrEmpty())
                Cbc(doc, parent, "CountrySubentity", direccion.Provincia);

            var country = Cac(doc, parent, "Country");
            var iso2 = Contexto.SeleccionarPorId<PaisDtm>(direccion.IdPais).ISO2;
            Cbc(doc, country, "IdentificationCode", iso2);
        }

        private void AgregarMedioDePago(XmlDocument doc, XmlElement root)
        {
            var paymentMeans = Cac(doc, root, "PaymentMeans");
            var cuentas = Factura.Sociedad(Contexto)
                                 .Detalles<CuentaDeMiSociedadDtm>(Contexto)
                                 .Where(x => x.Activa == true)
                                 .ToList();

            if (cuentas.Count == 1)
            {
                Cbc(doc, paymentMeans, "PaymentMeansCode", "30");
                Cbc(doc, paymentMeans, "PaymentDueDate", Factura.VenceEl!.Value.ToString("yyyy-MM-dd"));
                var cuenta = cuentas[0].Cuenta(Contexto);
                var payeeAccount = Cac(doc, paymentMeans, "PayeeFinancialAccount");
                Cbc(doc, payeeAccount, "ID", cuenta.NumeroIban);
                var banco = cuenta.Banco(Contexto, errorSiNoHay: false);
                if (banco != null && !banco.BicSwift.IsNullOrEmpty())
                {
                    var financialInstitutionBranch = Cac(doc, payeeAccount, "FinancialInstitutionBranch");
                    var financialInstitution = Cac(doc, financialInstitutionBranch, "FinancialInstitution");
                    Cbc(doc, financialInstitution, "ID", banco.BicSwift.PadRight(11, 'X').Substring(0, 11));
                }
            }
            else
            {
                // BR-61: código 30 exige IBAN (PayeeFinancialAccount/ID). Sin cuenta bancaria
                // concreta usamos código 1 (instrumento no definido) que no obliga a incluirlo.
                Cbc(doc, paymentMeans, "PaymentMeansCode", "1");
                Cbc(doc, paymentMeans, "PaymentDueDate", Factura.VenceEl!.Value.ToString("yyyy-MM-dd"));
            }
        }

        private void AgregarTotalesImpuestos(XmlDocument doc, XmlElement root)
        {
            var ivas = Factura.Ivas(Contexto);
            var totalIva = Math.Round(Convert.ToDouble(ivas.Sum(x => x.Importe)), 2);

            var taxTotal = Cac(doc, root, "TaxTotal");
            CbcConAtributo(doc, taxTotal, "TaxAmount", Fmt(totalIva), "currencyID", Factura.Moneda);

            foreach (var iva in ivas)
            {
                if (iva.EsNosujeto) continue;

                var taxSubtotal = Cac(doc, taxTotal, "TaxSubtotal");
                CbcConAtributo(doc, taxSubtotal, "TaxableAmount",
                    Fmt(Math.Round(Convert.ToDouble(iva.BI), 2)), "currencyID", Factura.Moneda);
                CbcConAtributo(doc, taxSubtotal, "TaxAmount",
                    Fmt(Math.Round(Convert.ToDouble(iva.Importe), 2)), "currencyID", Factura.Moneda);

                var taxCategory = Cac(doc, taxSubtotal, "TaxCategory");
                Cbc(doc, taxCategory, "ID", CodigoCategoriaIva(iva));
                Cbc(doc, taxCategory, "Percent", Fmt(Math.Round(Convert.ToDouble(iva.Porcentaje), 2)));
                if (iva.EsIsp)
                    Cbc(doc, taxCategory, "TaxExemptionReasonCode", "VATEX-EU-AE");
                var taxScheme = Cac(doc, taxCategory, "TaxScheme");
                Cbc(doc, taxScheme, "ID", "VAT");
            }

            var irpf = Factura.Irpf(Contexto);
            if (irpf > 0)
            {
                // aplicarJoin: true para que BiSujeta e Irpf estén cargados
                var irpfEmt = Factura.IrpfEmt(Contexto, errorSiNoHay: false, aplicarJoin: true);

                var withholdingTaxTotal = Cac(doc, root, "WithholdingTaxTotal");
                // TaxAmount [1..1] a nivel de WithholdingTaxTotal
                CbcConAtributo(doc, withholdingTaxTotal, "TaxAmount",
                    Fmt(Math.Round(Convert.ToDouble(irpf), 2)), "currencyID", Factura.Moneda);

                var taxSubtotal = Cac(doc, withholdingTaxTotal, "TaxSubtotal");
                // TaxableAmount [0..1] — base imponible sujeta a retención
                if (irpfEmt?.BiSujeta != null)
                    CbcConAtributo(doc, taxSubtotal, "TaxableAmount",
                        Fmt((decimal)irpfEmt.BiSujeta), "currencyID", Factura.Moneda);
                // TaxAmount [1..1] — obligatorio en TaxSubtotal según XSD UBL 2.1 y 2.5
                CbcConAtributo(doc, taxSubtotal, "TaxAmount",
                    Fmt(Math.Round(Convert.ToDouble(irpf), 2)), "currencyID", Factura.Moneda);

                var taxCategory = Cac(doc, taxSubtotal, "TaxCategory");
                Cbc(doc, taxCategory, "ID", "S");
                // Percent [0..1] — porcentaje real de retención
                if (irpfEmt?.Irpf != null)
                    Cbc(doc, taxCategory, "Percent", Fmt((decimal)irpfEmt.Irpf));
                var taxScheme = Cac(doc, taxCategory, "TaxScheme");
                Cbc(doc, taxScheme, "ID", "IRPF");
            }
        }

        private void AgregarTotalesMonetarios(XmlDocument doc, XmlElement root)
        {
            var legalMonetaryTotal = Cac(doc, root, "LegalMonetaryTotal");
            var bi = Math.Round(Convert.ToDouble(Factura.Bi(Contexto)), 2);
            var biConIva = Math.Round(Convert.ToDouble(Factura.BiConIva(Contexto)), 2);
            var sinDescuento = Math.Round(Convert.ToDouble(Factura.SinDescuento(Contexto)), 2);
            var aPagar = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            var descuento = Math.Round(sinDescuento - bi, 2);

            CbcConAtributo(doc, legalMonetaryTotal, "LineExtensionAmount", Fmt(bi), "currencyID", Factura.Moneda);
            CbcConAtributo(doc, legalMonetaryTotal, "TaxExclusiveAmount", Fmt(bi), "currencyID", Factura.Moneda);
            CbcConAtributo(doc, legalMonetaryTotal, "TaxInclusiveAmount", Fmt(biConIva), "currencyID", Factura.Moneda);
            if (descuento > 0)
                CbcConAtributo(doc, legalMonetaryTotal, "AllowanceTotalAmount", Fmt(descuento), "currencyID", Factura.Moneda);
            CbcConAtributo(doc, legalMonetaryTotal, "PayableAmount", Fmt(aPagar), "currencyID", Factura.Moneda);
        }

        private void AgregarLineas(XmlDocument doc, XmlElement root)
        {
            var lineas = Factura.Detalles<LineaDeUnaFaeDtm>(Contexto, aplicarJoin: true);
            int lineaUbl = 0;

            foreach (var linea in lineas)
            {
                if (linea.TipoDeLinea == enumTipoDeLinea.Comentario) continue;

                lineaUbl++;
                var invoiceLine = Cac(doc, root, "InvoiceLine");
                Cbc(doc, invoiceLine, "ID", lineaUbl.ToString());

                var unidad = ObtenerCodigoUnidad(linea.Unidad?.Sigla);
                var qty = Cbc(doc, invoiceLine, "InvoicedQuantity",
                                 Fmt(Math.Round(Convert.ToDouble(linea.Cantidad ?? 0), 2)));
                qty.SetAttribute("unitCode", unidad);

                CbcConAtributo(doc, invoiceLine, "LineExtensionAmount",
                    Fmt(Math.Round(Convert.ToDouble(linea.ImporteConDto), 2)), "currencyID", Factura.Moneda);

                if (linea.ImporteDeDto > 0)
                {
                    var allowanceCharge = Cac(doc, invoiceLine, "AllowanceCharge");
                    Cbc(doc, allowanceCharge, "ChargeIndicator", "false");
                    CbcConAtributo(doc, allowanceCharge, "Amount",
                        Fmt(Math.Round(Convert.ToDouble(linea.ImporteDeDto), 2)), "currencyID", Factura.Moneda);
                    CbcConAtributo(doc, allowanceCharge, "BaseAmount",
                        Fmt(Math.Round(Convert.ToDouble(linea.ImporteSinDto), 2)), "currencyID", Factura.Moneda);
                }

                var item = Cac(doc, invoiceLine, "Item");
                if (!linea.Anotacion.IsNullOrEmpty())
                    Cbc(doc, item, "Description", linea.Anotacion);
                Cbc(doc, item, "Name", linea.Concepto);

                if (linea.IdIvaR.HasValue && linea.IvaRepercutido != null &&
                    linea.IvaRepercutido.Clase != enumClasesDeIvaRep.NSJ)
                {
                    var classifiedTaxCategory = Cac(doc, item, "ClassifiedTaxCategory");
                    Cbc(doc, classifiedTaxCategory, "ID", CodigoCategoriaIvaLinea(linea));
                    Cbc(doc, classifiedTaxCategory, "Percent",
                        Fmt(Math.Round(Convert.ToDouble(linea.IvaRepercutido.Porcentaje), 2)));
                    var taxScheme = Cac(doc, classifiedTaxCategory, "TaxScheme");
                    Cbc(doc, taxScheme, "ID", "VAT");
                }

                var price = Cac(doc, invoiceLine, "Price");
                CbcConAtributo(doc, price, "PriceAmount",
                    Fmt(Math.Round(Convert.ToDouble(linea.Precio ?? 0), 6), 6), "currencyID", Factura.Moneda);
            }
        }

        // ── Helpers estáticos de categoría de IVA ─────────────────────────────

        private static string CodigoCategoriaIva(ImportePorTipoDeIva iva)
        {
            if (iva.EsIsp) return "AE";
            if (iva.EsExportacion) return iva.EsIntraComunitario ? "K" : "G";
            if (iva.EsExento) return "E";
            return "S";
        }

        private static string CodigoCategoriaIvaLinea(LineaDeUnaFaeDtm linea)
        {
            if (linea.IvaRepercutido == null) return "S";
            if (linea.IvaRepercutido.Clase == enumClasesDeIvaRep.ISP) return "AE";
            if (linea.IvaRepercutido.Exento) return "E";
            return "S";
        }

        private static string ObtenerCodigoUnidad(string sigla)
        {
            if (sigla.IsNullOrEmpty()) return "EA";
            return sigla.ToUpper() switch
            {
                "H" or "HR" or "HOR" => "HUR",
                "D" or "DÍA" or "DIA" => "DAY",
                "MES" => "MON",
                "KG" => "KGM",
                "L" or "LT" => "LTR",
                "M" => "MTR",
                "M2" => "MTK",
                "M3" => "MTQ",
                "KM" => "KMT",
                "UD" or "UDS" or "UNI" or "U" => "EA",
                _ => "EA"
            };
        }

        // ── Helpers de construcción XML ───────────────────────────────────────

        /// <summary>Formatea decimales con cultura invariante (punto como separador).</summary>
        protected static string Fmt(double value, int decimales = 2) => Fmt((decimal)value, decimales);

        protected static string Fmt(decimal value, int decimales = 2)
            => value.Formatear(decimales: decimales, alineacion: false, separadorDecimal: ".");

        protected static XmlElement Cac(XmlDocument doc, XmlElement parent, string localName)
        {
            var element = doc.CreateElement("cac", localName, NsCac);
            parent.AppendChild(element);
            return element;
        }

        protected static XmlElement Cbc(XmlDocument doc, XmlElement parent, string localName, string value)
        {
            var element = doc.CreateElement("cbc", localName, NsCbc);
            element.InnerText = value ?? string.Empty;
            parent.AppendChild(element);
            return element;
        }

        protected static XmlElement CbcConAtributo(XmlDocument doc, XmlElement parent,
            string localName, string value, string atributo, string valorAtributo)
        {
            var element = Cbc(doc, parent, localName, value);
            element.SetAttribute(atributo, valorAtributo);
            return element;
        }
    }
}