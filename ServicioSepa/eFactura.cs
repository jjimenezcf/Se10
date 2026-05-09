using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using System.Xml;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Elemento;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using System.Xml.Linq;
using ServicioDeDatos.Terceros;

namespace ServicioXml
{
    public static class eFactura
    {

        public static void GenerarFacturaE(this FacturaEmtDtm factura, ContextoSe contexto, string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XNamespace fe = "http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml";
            XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            /*
              
               <fe:Facturae xmlns:ds="http://www.w3.org/2000/09/xmldsig#" xmlns:fe="http://www.facturae.es/Facturae/2014/v3.2.1/Facturae">
            
                XNamespace fe = "http://www.facturae.es/Facturae/2014/v3.2.1/Facturae";
                XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

                writer.WriteStartElement("Facturae", "http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml");
                writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
             */

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("fe", "Facturae", fe.ToString());
                writer.WriteAttributeString("xmlns", "xsi", null, xsi.ToString());
                writer.WriteAttributeString("xmlns", "xsd", null, xsd.ToString());

                #region Escribir la sección FileHeader
                writer.WriteStartElement("FileHeader");
                writer.WriteElementString("SchemaVersion", "3.2.2");
                writer.WriteElementString("Modality", "I");
                writer.WriteElementString("InvoiceIssuerType", "EM");
                writer.WriteStartElement("Batch");
                writer.WriteElementString("BatchIdentifier", "");
                writer.WriteElementString("InvoicesCount", "1");
                writer.WriteStartElement("TotalInvoicesAmount");
                writer.WriteElementString("TotalAmount", factura.Bi(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteEndElement();
                writer.WriteStartElement("TotalOutstandingAmount");
                writer.WriteElementString("TotalAmount", factura.Bi(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteEndElement();
                writer.WriteStartElement("TotalExecutableAmount");
                writer.WriteElementString("TotalAmount", factura.Bi(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteEndElement();
                writer.WriteElementString("InvoiceCurrencyCode", "EUR");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region Escribir la sección Parties
                writer.WriteStartElement("Parties");

                #region Escribir la etiqueta SellerParty
                writer.WriteStartElement("SellerParty");
                writer.WriteStartElement("TaxIdentification");
                writer.WriteElementString("PersonTypeCode", "J");
                writer.WriteElementString("ResidenceTypeCode", "R");
                writer.WriteElementString("TaxIdentificationNumber", factura.Sociedad(contexto).NIF);
                writer.WriteEndElement();
                writer.WriteStartElement("LegalEntity");
                writer.WriteElementString("CorporateName", factura.Sociedad(contexto).RazonSocial);
                writer.WriteElementString("TradeName", factura.Sociedad(contexto).Nombre);
                writer.WriteStartElement("RegistrationData");
                writer.WriteElementString("Book", "171");
                writer.WriteElementString("RegisterOfCompaniesLocation", "Murcia");
                writer.WriteElementString("Sheet", "0");
                writer.WriteElementString("Folio", "1");
                writer.WriteElementString("Section", "3ª");
                writer.WriteElementString("Volume", "0");
                writer.WriteElementString("AdditionalRegistrationData", "ref.3657,Inscip.1ª");
                writer.WriteEndElement();
                writer.WriteStartElement("AddressInSpain");
                writer.WriteElementString("Address", factura.Sociedad(contexto).DireccionFiscal(contexto).NombreDireccion);
                writer.WriteElementString("PostCode", factura.Sociedad(contexto).DireccionFiscal(contexto).CodigoPostal);
                writer.WriteElementString("Town", factura.Sociedad(contexto).DireccionFiscal(contexto).Municipio);
                writer.WriteElementString("Province", factura.Sociedad(contexto).DireccionFiscal(contexto).Provincia);
                writer.WriteElementString("CountryCode", contexto.SeleccionarPorId<PaisDtm>(factura.Sociedad(contexto).DireccionFiscal(contexto).IdPais).Codigo);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                writer.BuyerParty(contexto, factura);
                writer.WriteEndElement();
                #endregion

                #region Escribir la sección Invoices
                writer.WriteStartElement("Invoices");
                #region Factura
                writer.WriteStartElement("Invoice");

                writer.WriteStartElement("InvoiceHeader");
                writer.WriteElementString("InvoiceNumber", factura.NumeroDeFactura);
                writer.WriteElementString("InvoiceDocumentType", "FC");
                writer.WriteElementString("InvoiceClass", "OO");
                writer.WriteEndElement();

                writer.WriteStartElement("InvoiceIssueData");
                writer.WriteElementString("IssueDate", factura.FacturadaEl.Fecha().ToString("yyyy-MM-dd"));
                writer.WriteElementString("InvoiceCurrencyCode", "EUR");
                writer.WriteElementString("TaxCurrencyCode", "EUR");
                writer.WriteElementString("LanguageName", "es");
                writer.WriteElementString("InvoiceDescription", factura.Nombre);
                writer.WriteEndElement();

                writer.TaxesOutputs(contexto, factura);
                //writer.AdditionalInformation(contexto, factura);

                #region totales
                writer.WriteStartElement("InvoiceTotals");
                writer.WriteElementString("TotalGrossAmount", factura.SinDescuento(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteElementString("TotalGrossAmountBeforeTaxes", factura.Bi(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteElementString("TotalTaxOutputs", factura.Ivas(contexto).Sum(x => x.Importe).Formatear(alineacion: false, separadorDecimal: "."));
                //IRPF
                writer.WriteElementString("TotalTaxesWithheld", "0");
                writer.WriteElementString("InvoiceTotal", factura.BiConIva(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteElementString("TotalOutstandingAmount", factura.BiConIva(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteElementString("TotalExecutableAmount", factura.APagar(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteEndElement();
                #endregion

                #region detalle de factura
                var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, aplicarJoin: true);
                foreach (var linea in lineas)
                {
                    writer.WriteStartElement("Items");
                    writer.WriteStartElement("InvoiceLine");
                    writer.WriteElementString("ReceiverContractReference", factura.Contrato(contexto)?.Referencia ?? factura.Presupuesto(contexto)?.Referencia ?? "");
                    writer.WriteElementString("ReceiverTransactionReference", linea.ParteTr(contexto)?.Referencia ?? factura.ParteTr(contexto)?.Referencia ?? factura.Contrato(contexto)?.Referencia ?? factura.Presupuesto(contexto)?.Referencia ?? "");
                    writer.WriteElementString("ItemDescription", linea.Concepto);
                    writer.WriteElementString("Quantity", linea.Cantidad.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteElementString("UnitPriceWithoutTax", linea.Precio.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteElementString("TotalCost", linea.ImporteSinDto.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteElementString("GrossAmount", linea.ImporteConDto.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteStartElement("TaxesOutputs");
                    writer.WriteStartElement("Tax");
                    writer.WriteElementString("TaxTypeCode", "01");
                    writer.WriteElementString("TaxRate", linea.IvaRepercutido.Porcentaje.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteStartElement("TaxableBase");
                    writer.WriteElementString("TotalAmount", linea.ImporteConDto.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteEndElement();
                    writer.WriteStartElement("TaxAmount");
                    writer.WriteElementString("TotalAmount", linea.ImporteDeIva.Formatear(alineacion: false, separadorDecimal: "."));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                #endregion

                #region datos del pago
                writer.WriteStartElement("PaymentDetails");
                writer.WriteStartElement("Installment");
                writer.WriteElementString("InstallmentDueDate", factura.VenceEl?.ToString("yyyy-MM-dd") ?? "");
                writer.WriteElementString("InstallmentAmount", factura.APagar(contexto).Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteElementString("PaymentMeans", "02");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                writer.WriteEndElement();
                #endregion
                writer.WriteEndElement();
                #endregion

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private static void AdditionalInformation(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            var datosDeFactura = (ParametrosDeMiSociedadDtm)factura.Sociedad(contexto).SeleccionarAmpliacion(contexto, typeof(ParametrosDeMiSociedadDtm));
            writer.WriteStartElement("AdditionalInformation");
            writer.WriteStartElement("AdditionalProperty");
            writer.WriteElementString("Name", "Observaciones");
            writer.WriteElementString("Value", datosDeFactura.PieDeFactura);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void TaxesOutputs(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            writer.WriteStartElement("TaxesOutputs");
            writer.WriteStartElement("Tax");
            writer.WriteElementString("TaxTypeCode", "01");

            var ivas = factura.Ivas(contexto);
            foreach (var iva in ivas)
            {
                writer.WriteElementString("TaxRate", iva.Porcentaje.Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteStartElement("TaxableBase");
                writer.WriteElementString("TotalAmount", iva.BI.Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteEndElement();
                writer.WriteStartElement("TaxAmount");
                writer.WriteElementString("TotalAmount", iva.Importe.Formatear(alineacion: false, separadorDecimal: "."));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void BuyerParty(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            #region Escribir la etiqueta BuyerParty
            writer.WriteStartElement("BuyerParty");
            writer.TaxIdentification(contexto, factura);
            if (factura.Cliente(contexto).Interlocutor(contexto).EsPersona)
                writer.Individual(contexto, factura);
            else
                writer.LegacyEntity(contexto, factura);
            writer.WriteEndElement();
            #endregion
        }

        private static void TaxIdentification(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            var pais = factura.Direccion(contexto, enumCalificadorDireccion.fiscal).Pais(contexto);
            writer.WriteStartElement("TaxIdentification");
            writer.WriteElementString("PersonTypeCode", factura.Cliente(contexto).Interlocutor(contexto).EsPersona ? "F" : "J");
            writer.WriteElementString("ResidenceTypeCode", pais.ISO2 == ltrIsoPaises.Spain ? "R" : pais.EsUE ? "U": "E");
            writer.WriteElementString("TaxIdentificationNumber", factura.Cliente(contexto).NIF(contexto, quitarPrefijoEs: true));
            writer.WriteEndElement();
        }

        private static void Individual(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            var persona = ApiDeTerceros.InferirNombreConApellidos(factura.Contacto);
            writer.WriteStartElement("Individual");
            writer.WriteElementString("Name", persona.Nombre);
            writer.WriteElementString("FirstSurname", persona.Ape1);
            writer.WriteElementString("SecondSurname", persona.Ape2);
            writer.AddressInSpain(contexto, factura);
            writer.WriteEndElement();
        }

        private static void LegacyEntity(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            writer.WriteStartElement("LegalEntity");
            writer.WriteElementString("CorporateName", factura.Contacto);
            writer.AddressInSpain(contexto, factura);
            writer.WriteEndElement();
        }

        private static void AddressInSpain(this XmlWriter writer, ContextoSe contexto, FacturaEmtDtm factura)
        {
            var direccion = factura.DireccionFiscal(contexto);
            writer.WriteStartElement("AddressInSpain");
            writer.WriteElementString("Address", direccion.Calle);
            writer.WriteElementString("PostCode", direccion.CodigoPostal);
            writer.WriteElementString("Town", direccion.Municipio);
            writer.WriteElementString("Province", direccion.Provincia);
            writer.WriteElementString("CountryCode", contexto.SeleccionarPorId<PaisDtm>(direccion.IdPais).Codigo);
            writer.WriteEndElement();
        }

    }
}
