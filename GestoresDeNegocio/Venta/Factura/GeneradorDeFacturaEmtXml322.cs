using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloXml.eFactura;
using ModeloXml.eFactura.Facturae322;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
namespace GestoresDeNegocio.Ventas
{
    public class GeneradorDeFacturaEmtXml322 : GeneradorDeFacturaEmtXml
    {

        public GeneradorDeFacturaEmtXml322(ContextoSe contexto, FacturaEmtDtm factura, string rutaConFichero) : base(contexto, factura, rutaConFichero)
        {

        }

        protected override string GenerarXml(IeFactura ifactura)
        {
            var eFactura = (eFactura322)ifactura;

            FileHeader(eFactura);

            SellerParty(eFactura);

            BuyerParty(eFactura);

            var cf = new InvoiceHeaderType();
            InvoiceHeader(cf);
            var df = new InvoiceIssueDataType();
            df.IssueDate = Factura.FacturadaEl.Fecha();

            var periodo = enumNegocio.FacturaEmitida.UsaLaAmpliacionDe(Contexto, Factura.IdTipo, typeof(PeriodoEmtDtm))
                ? Factura.Ampliacion<PeriodoEmtDtm>(Contexto, errorSiNoHay: false, aplicarJoin: true)
                : null;

            if (periodo is not null)
            {
                df.InvoicingPeriod = new PeriodDates
                {
                    StartDate = periodo.Inicio.Fecha(),
                    EndDate = periodo.Fin.Fecha()
                };
            }


            df.InvoiceCurrencyCode = ApiDeEnsamblados.ToEnumerado<CurrencyCodeType>(Factura.Moneda);
            df.TaxCurrencyCode = ApiDeEnsamblados.ToEnumerado<CurrencyCodeType>(Factura.Moneda);
            df.LanguageName = ApiDeEnsamblados.ToEnumerado<LanguageCodeType>(ltrIsoPaises.Spain.ToLower());

            var invoice = new InvoiceType
            {
                InvoiceHeader = cf,
                InvoiceIssueData = df
            };
            Invoice(eFactura, invoice);
            var invoices = new List<InvoiceType> { invoice };
            eFactura.Invoices = invoices.ToArray();

            eFactura.ToFile(Ruta);
            return Ruta;
        }

        private InvoiceClassType ClaseDeFactura(FacturaEmtDtm factura)
        {
            if (factura.ClaseRectificativa == null)
                GestorDeErrores.Emitir($"La factura '{factura.NumeroDeFactura}' no es una rectificativa");

            if (factura.ClaseRectificativa == enumClaseDeRectificativa.OC) return InvoiceClassType.OC;

            if (factura.ClaseRectificativa == enumClaseDeRectificativa.OR) return InvoiceClassType.OR;

            throw new Exception($"No se ha implementado cómo parsear el la clase '{factura.ClaseRectificativa.Descripcion()}' para la factura '{factura.NumeroDeFactura}' al generar una eFactura");
        }

        private ReasonCodeType CodigoDelMotivoDeLaRectificacion(FacturaEmtDtm factura)
        {
            if (factura.ClaseRectificativa == null)
                GestorDeErrores.Emitir($"La factura '{factura.NumeroDeFactura}' no es una rectificativa");

            if (factura.MotivoDeRectificacion == null)
                GestorDeErrores.Emitir($"La factura '{factura.NumeroDeFactura}' no tiene indicado el motivo de rectificación");

            if (factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImportes) return ReasonCodeType.Item07;

            if (factura.MotivoDeRectificacion == enumMotivoDeRectificacion.DatosErroneos) return ReasonCodeType.Item01;

            if (factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImpago) return ReasonCodeType.Item80;

            throw new Exception($"No se ha implementado cómo parsear el motivo de la rectificación '{factura.MotivoDeRectificacion.Descripcion()}' para la factura '{factura.NumeroDeFactura}' al generar una eFactura");
        }

        private ReasonDescriptionType MotivoDeLaRectificacion(FacturaEmtDtm factura)
        {
            if (factura.ClaseRectificativa == null)
                GestorDeErrores.Emitir($"La factura '{factura.NumeroDeFactura}' no es una rectificativa");

            if (factura.MotivoDeRectificacion == null)
                GestorDeErrores.Emitir($"La factura '{factura.NumeroDeFactura}' no tiene indicado el motivo de rectificación");

            if (factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImportes) return ReasonDescriptionType.Baseimponible;

            if (factura.MotivoDeRectificacion == enumMotivoDeRectificacion.DatosErroneos) return ReasonDescriptionType.NombreyapellidosRazónSocialReceptor;

            return ReasonDescriptionType.Clasedefactura;
        }

        private void Invoice(eFactura322 efactura, InvoiceType invoice)
        {
            invoice.TaxesOutputs = (TaxOutputType[])efactura.TaxesOutputs(Contexto, Factura);
            if (Factura.TienenIrpf(Contexto)) invoice.TaxesWithheld = efactura.Retenciones(Contexto, Factura);
            invoice.InvoiceTotals = new InvoiceTotalsType
            {
                TotalGrossAmount = Math.Round(Convert.ToDouble(Factura.SinDescuento(Contexto)), 2),
                TotalGrossAmountBeforeTaxes = Math.Round(Convert.ToDouble(Factura.Bi(Contexto)), 2),
                TotalTaxOutputs = Math.Round(Convert.ToDouble(Factura.Ivas(Contexto).Sum(x => x.Importe)), 2),
                TotalTaxesWithheld = Math.Round(Convert.ToDouble(Factura.Irpf(Contexto)), 2),
            };
            invoice.InvoiceTotals.InvoiceTotal = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            invoice.InvoiceTotals.TotalOutstandingAmount = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            invoice.InvoiceTotals.TotalExecutableAmount = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);

            var lineas = Factura.Detalles<LineaDeUnaFaeDtm>(Contexto, aplicarJoin: true);
            var invoiceLines = new List<InvoiceLineType>();
            foreach (var linea in lineas.Where(l => l.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario))
                invoiceLines.Add((InvoiceLineType)efactura.InvoiceLine(Contexto, Factura, linea));

            invoice.Items = invoiceLines.ToArray();
            if (Factura.VenceEl is not null)
            {
                invoice.PaymentDetails = (InstallmentType[])efactura.PaymentDetails(Contexto, Factura);
            }

            var informacionAdicional = efactura.InvoiceAdditionalInformation(Factura, lineas);
            if (!informacionAdicional.IsNullOrEmpty()) invoice.AdditionalData = new AdditionalDataType
            {
                RelatedInvoice = Factura.NumeroDeFactura,
                InvoiceAdditionalInformation = informacionAdicional
            };

            invoice.LegalLiterals = LiteralesLegales();
        }

        /*
         * Estos códigos representan los diferentes tipos de documentos de factura que se pueden utilizar en una factura electrónica de acuerdo al estándar Facturae 3.2.2.
         FC
         Significa "Factura", y se refiere a una factura regular.
         FA
         Significa "Factura Abreviada", y se refiere a una factura abreviada.
         AF
         Significa "Autofactura", y se refiere a una autofactura.
         Estos códigos permiten identificar el tipo de documento de factura que se está emitiendo, lo cual es importante para fines de contabilidad, auditoría y cumplimiento normativo. Por ejemplo, una factura abreviada tiene requisitos de información diferentes a una factura regular.
         * 
         Tipos de Clase de Factura (InvoiceClassType)
         Estos valores representan los diferentes tipos de clase de factura que se pueden utilizar en una factura electrónica de acuerdo al estándar Facturae 3.2.2.
         OO
         Significa "Original Original", y se refiere a una factura original.
         OR
         Significa "Original Rectificativa", y se refiere a una factura original rectificativa.
         OC
         Significa "Original Complementaria", y se refiere a una factura original complementaria.
         CO
         Significa "Copia Original", y se refiere a una copia de una factura original.
         CR
         Significa "Copia Rectificativa", y se refiere a una copia de una factura rectificativa.
         CC
         Significa "Copia Complementaria", y se refiere a una copia de una factura complementaria.
        */


        private void InvoiceHeader(InvoiceHeaderType cf)
        {
            cf.InvoiceNumber = Factura.NumeroDeFactura;
            cf.InvoiceSeriesCode = Factura.Serie;
            cf.InvoiceDocumentType = InvoiceDocumentTypeType.FC;
            if (!Factura.EsRectificativa)
            {
                cf.InvoiceClass = InvoiceClassType.OO;
            }
            else
            {
                var rectificada = Factura.RectificaA(Contexto);
                var periodo = enumNegocio.FacturaEmitida.UsaLaAmpliacionDe(Contexto, rectificada.IdTipo, typeof(PeriodoEmtDtm))
                    ? Factura.Ampliacion<PeriodoEmtDtm>(Contexto)
                    : new PeriodoEmtDtm { Inicio = rectificada.FacturadaEl, Fin = rectificada.FacturadaEl };

                cf.InvoiceClass = ClaseDeFactura(Factura);
                cf.Corrective = new CorrectiveType();
                cf.Corrective.ReasonCode = CodigoDelMotivoDeLaRectificacion(Factura);
                cf.Corrective.ReasonDescription = MotivoDeLaRectificacion(Factura);
                cf.Corrective.InvoiceNumber = rectificada.NumeroDeFactura;
                cf.Corrective.InvoiceSeriesCode = rectificada.Serie;
                cf.Corrective.AdditionalReasonDescription = Factura.Descripcion;
                cf.Corrective.TaxPeriod = new PeriodDates { StartDate = periodo.Inicio.Fecha(), EndDate = periodo.Fin.Fecha() };
                //Item01: Este valor podría representar un método de corrección específico, como por ejemplo una corrección completa de la factura original.
                cf.Corrective.CorrectionMethod = CorrectionMethodType.Item01;
                cf.Corrective.CorrectionMethodDescription = Factura.ClaseRectificativa == enumClaseDeRectificativa.OR
                    ? CorrectionMethodDescriptionType.Rectificacióníntegra
                    : Factura.ClaseRectificativa == enumClaseDeRectificativa.OC
                    ? CorrectionMethodDescriptionType.Rectificaciónpordiferencias
                    : CorrectionMethodDescriptionType.AutorizadasporlaAgenciaTributaria;

                cf.Corrective.AdditionalReasonDescription = (Factura.Detalles<RectificativaEmtDtm>(Contexto)[0].Concepto + Environment.NewLine +
                    $"{Factura.ClaseRectificativa.Descripcion()}: {Factura.MotivoDeRectificacion.Descripcion()}" + Environment.NewLine +
                    Factura.Descripcion).Left(2500);
            }
        }


        private void SellerParty(eFactura322 efactura)
        {
            efactura.Parties.SellerParty = new BusinessType();
            efactura.Parties.SellerParty.TaxIdentification = new TaxIdentificationType();

            efactura.Parties.SellerParty.TaxIdentification.PersonTypeCode = Emisor.Autonomo ? PersonTypeCodeType.F : PersonTypeCodeType.J;
            efactura.Parties.SellerParty.TaxIdentification.ResidenceTypeCode = ResidenceTypeCodeType.R;
            efactura.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber = Emisor.NIFConIsoEs;
            if (Emisor.Autonomo)
                efactura.Parties.SellerParty.Item = efactura.IndividualSellerParty(Contexto, Emisor);
            else
                efactura.Parties.SellerParty.Item = efactura.LegalEntitySeller(Contexto, Factura);
        }

        private void BuyerParty(eFactura322 efactura)
        {
            efactura.Parties.BuyerParty = new BusinessType();
            efactura.Parties.BuyerParty.TaxIdentification = new TaxIdentificationType();
            efactura.Parties.BuyerParty.TaxIdentification.PersonTypeCode = Factura.Cliente(Contexto).Interlocutor(Contexto).EsPersona
            ? PersonTypeCodeType.F
            : PersonTypeCodeType.J;

            var paisCliente = Factura.Direccion(Contexto, enumCalificadorDireccion.fiscal).Pais(Contexto);
            efactura.Parties.BuyerParty.TaxIdentification.ResidenceTypeCode = paisCliente.ISO2 == ltrIsoPaises.Spain
            ? ResidenceTypeCodeType.R
            : paisCliente.EsUE
            ? ResidenceTypeCodeType.U
            : ResidenceTypeCodeType.E;

            efactura.Parties.BuyerParty.TaxIdentification.TaxIdentificationNumber = Factura.Cliente(Contexto).NIF(Contexto, quitarPrefijoEs: true);
            if (Factura.TieneCentroAdministrativo(Contexto))
                efactura.Parties.BuyerParty.AdministrativeCentres = (AdministrativeCentreType[])efactura.AdministrativeCentres(Contexto, Factura);

            if (Factura.Cliente(Contexto).Interlocutor(Contexto).EsPersona)
                efactura.Parties.BuyerParty.Item = efactura.IndividualBuyer(Contexto, Factura);
            else
                efactura.Parties.BuyerParty.Item = efactura.LegalEntityBuyer(Contexto, Factura);
        }

        private void FileHeader(eFactura322 efactura)
        {
            efactura.FileHeader.Batch.BatchIdentifier = $"{Emisor.NIFConIsoEs}-{Factura.Id}";
            efactura.FileHeader.Batch.InvoicesCount = 1;
            efactura.FileHeader.Batch.TotalInvoicesAmount.TotalAmount = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            efactura.FileHeader.Batch.TotalOutstandingAmount.TotalAmount = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            efactura.FileHeader.Batch.TotalExecutableAmount.TotalAmount = Math.Round(Convert.ToDouble(Factura.APagar(Contexto)), 2);
            efactura.FileHeader.Batch.InvoiceCurrencyCode = ApiDeEnsamblados.ToEnumerado<CurrencyCodeType>(Factura.Moneda);
        }

    }
}
