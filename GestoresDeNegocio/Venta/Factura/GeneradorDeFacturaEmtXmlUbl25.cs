using GestorDeElementos.Extensores;
using ModeloDeDto.Negocio;
using ModeloXml.eFactura;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Ventas
{
    public class GeneradorDeFacturaEmtXmlUbl25 : GeneradorDeFacturaEmtXml
    {
        private const string NsInvoice = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        private const string NsCac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private const string NsCbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        public GeneradorDeFacturaEmtXmlUbl25(ContextoSe contexto, FacturaEmtDtm factura, string rutaConFichero)
            : base(contexto, factura, rutaConFichero)
        {
        }

        public string GenerarUbl()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            var root = doc.CreateElement("Invoice", NsInvoice);
            root.SetAttribute("xmlns:cac", NsCac);
            root.SetAttribute("xmlns:cbc", NsCbc);
            doc.AppendChild(root);

            Cbc(doc, root, "UBLVersionID", "2.5");
            Cbc(doc, root, "CustomizationID", "urn:cen.eu:en16931:2017");
            Cbc(doc, root, "ID", Factura.NumeroDeFactura);
            Cbc(doc, root, "IssueDate", (Factura.FacturadaEl ?? DateTime.Now).ToString("yyyy-MM-dd"));
            if (Factura.VenceEl.HasValue)
                Cbc(doc, root, "DueDate", Factura.VenceEl.Value.ToString("yyyy-MM-dd"));
            Cbc(doc, root, "InvoiceTypeCode", Factura.EsRectificativa ? "381" : "380");
            Cbc(doc, root, "DocumentCurrencyCode", Factura.Moneda);

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
            var cuentas = Factura.Sociedad(Contexto).Detalles<CuentaDeMiSociedadDtm>(Contexto).Where(x => x.Activa == true).ToList();

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
                Cbc(doc, paymentMeans, "PaymentMeansCode", "10");
                Cbc(doc, paymentMeans, "PaymentDueDate", Factura.VenceEl!.Value.ToString("yyyy-MM-dd"));
            }
        }

        private void AgregarTotalesImpuestos(XmlDocument doc, XmlElement root)
        {
            var ivas = Factura.Ivas(Contexto);
            var totalIva = Math.Round(Convert.ToDouble(ivas.Sum(x => x.Importe)), 2);

            var taxTotal = Cac(doc, root, "TaxTotal");
            CbcConAtributo(doc, taxTotal, "TaxAmount", totalIva.ToString("F2"), "currencyID", Factura.Moneda);

            foreach (var iva in ivas)
            {
                if (iva.EsNosujeto) continue;

                var taxSubtotal = Cac(doc, taxTotal, "TaxSubtotal");
                CbcConAtributo(doc, taxSubtotal, "TaxableAmount", Math.Round(Convert.ToDouble(iva.BI), 2).ToString("F2"), "currencyID", Factura.Moneda);
                CbcConAtributo(doc, taxSubtotal, "TaxAmount", Math.Round(Convert.ToDouble(iva.Importe), 2).ToString("F2"), "currencyID", Factura.Moneda);

                var taxCategory = Cac(doc, taxSubtotal, "TaxCategory");
                Cbc(doc, taxCategory, "ID", CodigoCategoriaIva(iva));
                Cbc(doc, taxCategory, "Percent", Math.Round(Convert.ToDouble(iva.Porcentaje), 2).ToString("F2"));
                if (iva.EsIsp)
                    Cbc(doc, taxCategory, "TaxExemptionReasonCode", "VATEX-EU-AE");
                var taxScheme = Cac(doc, taxCategory, "TaxScheme");
                Cbc(doc, taxScheme, "ID", "VAT");
            }

            var irpf = Factura.Irpf(Contexto);
            if (irpf > 0)
            {
                var withholdingTaxTotal = Cac(doc, root, "WithholdingTaxTotal");
                CbcConAtributo(doc, withholdingTaxTotal, "TaxAmount", Math.Round(Convert.ToDouble(irpf), 2).ToString("F2"), "currencyID", Factura.Moneda);
                var taxSubtotal = Cac(doc, withholdingTaxTotal, "TaxSubtotal");
                var taxCategory = Cac(doc, taxSubtotal, "TaxCategory");
                Cbc(doc, taxCategory, "ID", "S");
                var taxScheme = Cac(doc, taxCategory, "TaxScheme");
                Cbc(doc, taxScheme, "ID", "IRPF");
            }
        }

        private static string CodigoCategoriaIva(ImportePorTipoDeIva iva)
        {
            if (iva.EsIsp) return "AE";
            if (iva.EsExportacion) return iva.EsIntraComunitario ? "K" : "G";
            if (iva.EsExento) return "E";
            return "S";
        }

        private void AgregarTotalesMonetarios(XmlDocument doc, XmlElement root)
        {
            var legalMonetaryTotal = Cac(doc, root, "LegalMonetaryTotal");
            var bi = Math.Round(Convert.ToDouble(Factura.Bi(Contexto)), 2);
            var biConIva = Math.Round(Convert.ToDouble(Factura.BiConIva(Contexto)), 2);
            var sinDescuento = Math.Round(Convert.ToDouble(Factura.SinDescuento(Contexto)), 2);
            var aPagar = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            var descuento = Math.Round(sinDescuento - bi, 2);

            CbcConAtributo(doc, legalMonetaryTotal, "LineExtensionAmount", bi.ToString("F2"), "currencyID", Factura.Moneda);
            CbcConAtributo(doc, legalMonetaryTotal, "TaxExclusiveAmount", bi.ToString("F2"), "currencyID", Factura.Moneda);
            CbcConAtributo(doc, legalMonetaryTotal, "TaxInclusiveAmount", biConIva.ToString("F2"), "currencyID", Factura.Moneda);
            if (descuento > 0)
                CbcConAtributo(doc, legalMonetaryTotal, "AllowanceTotalAmount", descuento.ToString("F2"), "currencyID", Factura.Moneda);
            CbcConAtributo(doc, legalMonetaryTotal, "PayableAmount", aPagar.ToString("F2"), "currencyID", Factura.Moneda);
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
                var qty = Cbc(doc, invoiceLine, "InvoicedQuantity", Math.Round(Convert.ToDouble(linea.Cantidad ?? 0), 2).ToString("F2"));
                qty.SetAttribute("unitCode", unidad);

                CbcConAtributo(doc, invoiceLine, "LineExtensionAmount",
                    Math.Round(Convert.ToDouble(linea.ImporteConDto), 2).ToString("F2"), "currencyID", Factura.Moneda);

                if (linea.ImporteDeDto > 0)
                {
                    var allowanceCharge = Cac(doc, invoiceLine, "AllowanceCharge");
                    Cbc(doc, allowanceCharge, "ChargeIndicator", "false");
                    CbcConAtributo(doc, allowanceCharge, "Amount",
                        Math.Round(Convert.ToDouble(linea.ImporteDeDto), 2).ToString("F2"), "currencyID", Factura.Moneda);
                    CbcConAtributo(doc, allowanceCharge, "BaseAmount",
                        Math.Round(Convert.ToDouble(linea.ImporteSinDto), 2).ToString("F2"), "currencyID", Factura.Moneda);
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
                        Math.Round(Convert.ToDouble(linea.IvaRepercutido.Porcentaje), 2).ToString("F2"));
                    var taxScheme = Cac(doc, classifiedTaxCategory, "TaxScheme");
                    Cbc(doc, taxScheme, "ID", "VAT");
                }

                var price = Cac(doc, invoiceLine, "Price");
                CbcConAtributo(doc, price, "PriceAmount",
                    Math.Round(Convert.ToDouble(linea.Precio ?? 0), 6).ToString("F6"), "currencyID", Factura.Moneda);
            }
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

        private static XmlElement Cac(XmlDocument doc, XmlElement parent, string localName)
        {
            var element = doc.CreateElement("cac", localName, NsCac);
            parent.AppendChild(element);
            return element;
        }

        private static XmlElement Cbc(XmlDocument doc, XmlElement parent, string localName, string value)
        {
            var element = doc.CreateElement("cbc", localName, NsCbc);
            element.InnerText = value ?? string.Empty;
            parent.AppendChild(element);
            return element;
        }

        private static XmlElement CbcConAtributo(XmlDocument doc, XmlElement parent, string localName, string value, string atributo, string valorAtributo)
        {
            var element = Cbc(doc, parent, localName, value);
            element.SetAttribute(atributo, valorAtributo);
            return element;
        }
    }
}
