
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using System.Globalization;
using System.Xml;
using Utilidades;

namespace ServicioXml
{
    public static class ApiSepa
    {
        public static void GenerarSepaQ19(this RemesaFaeDtm remesa, ContextoSe contexto, string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            if (!remesa.GeneradaEl.HasValue) GestorDeErrores.Emitir($"No se puede generar la remesa '{remesa.Referencia}' por no tener fecha de generación");
            var generadaEl = remesa.GeneradaEl.Fecha();
            //remesa.ValidarFechaDeCargo(contexto);

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Document", "urn:iso:std:iso:20022:tech:xsd:pain.008.001.02");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "schemaLocation", null, "urn:iso:std:iso:20022:tech:xsd:pain.008.001.02 pain.008.001.02.xsd");

                writer.WriteStartElement("CstmrDrctDbtInitn");
                #region Encabezado de grupo (GrpHdr) 
                writer.WriteStartElement("GrpHdr");
                writer.WriteElementString("MsgId", $"{remesa.Referencia}");
                writer.WriteElementString("CreDtTm", value: $"{generadaEl.ToString("s")}");
                writer.WriteElementString("NbOfTxs", value: $"{remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Count.ToString().PadLeft(15, '0')}");
                writer.WriteElementString("CtrlSum", value: $"{remesa.Total(contexto).ToString(CultureInfo.InvariantCulture)}");
                writer.WriteStartElement("InitgPty");
                writer.WriteElementString("Nm", $"{remesa.Sociedad(contexto).Expresion.Left(70)}");
                writer.WriteStartElement("Id");
                writer.WriteStartElement("OrgId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", $"ES00000{remesa.NifDelAcreedor}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Código interno (PmtInf)
                writer.WriteStartElement("PmtInf");
                writer.WriteElementString("PmtInfId", remesa.Id.ToString().PadLeft(35, '0'));
                writer.WriteElementString("PmtMtd", "DD");
                writer.WriteElementString("NbOfTxs", value: $"{remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Count.ToString().PadLeft(15, '0')}");
                writer.WriteElementString("CtrlSum", value: $"{remesa.Total(contexto)}");
                #region Prioridad de l instrucción (PmtTpInf)
                writer.WriteStartElement("PmtTpInf");
                writer.WriteStartElement("SvcLvl");
                writer.WriteElementString("Cd", value: "SEPA");
                writer.WriteEndElement();
                writer.WriteStartElement("LclInstrm");
                writer.WriteElementString("Cd", value: "COR1");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                writer.WriteElementString("ReqdColltnDt", value: remesa.CargarEl?.ToString("yyyy-MM-dd"));
                #region Acreedor (Cdtr)
                writer.WriteStartElement("Cdtr");
                writer.WriteElementString("Nm", $"{remesa.Sociedad(contexto).Expresion.Left(70)}");
                writer.WriteEndElement();
                #endregion
                #region Cuenta del acreedor (CdtrAcct)
                writer.WriteStartElement("CdtrAcct");
                writer.WriteStartElement("Id");
                writer.WriteElementString("IBAN", $"{remesa.CuentaDeAbono(contexto).Cuenta(contexto).NumeroIban}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Agente de acreedor (CdtrAgt):  Identificación de institución financiera de agente acreedor
                writer.WriteStartElement("CdtrAgt");
                writer.WriteStartElement("FinInstnId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", "NOTPROVIDED");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Agente de acreedor (CdtrSchmeId): Id del acreedor (AT-02): Las pos [1 , 2] --> código de país, [3, 4] DC, [5 a 7]: código comercial y [8 a 35]: id específico del país.
                writer.WriteStartElement("CdtrSchmeId");
                writer.WriteStartElement("Id");
                writer.WriteStartElement("PrvtId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", $"ES00000{remesa.NifDelAcreedor}");
                writer.WriteStartElement("SchmeNm");
                writer.WriteElementString("Prtry", value: "SEPA");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                foreach (var facturaRemesada in remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto))
                {

                    writer.WriteStartElement("DrctDbtTxInf");
                    writer.WriteStartElement("PmtId");
                    writer.WriteElementString("InstrId", value: $"{remesa.Id}{facturaRemesada.IdFactura}");
                    writer.WriteElementString("EndToEndId", value: $"{remesa.Id}{facturaRemesada.IdFactura}");
                    writer.WriteEndElement();
                    writer.WriteStartElement("InstdAmt");
                    writer.WriteAttributeString("Ccy", "EUR");
                    writer.WriteValue(facturaRemesada.Factura(contexto).APagar(contexto).ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                    writer.WriteStartElement("DrctDbtTx");
                    writer.WriteStartElement("MndtRltdInf");
                    writer.WriteElementString("MndtId", value: facturaRemesada.Factura(contexto).Cliente(contexto).CuentaDeCliente(contexto, ServicioDeDatos.Contabilidad.enumClaseDeCuentaBancaria.Pago).IdArchivo.ToString());
                    writer.WriteElementString("DtOfSgntr", value: facturaRemesada.Factura(contexto).Cliente(contexto).CuentaDeCliente(contexto, ServicioDeDatos.Contabilidad.enumClaseDeCuentaBancaria.Pago).CertificadoDeCuenta(contexto).FechaCreacion.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("DbtrAgt");
                    writer.WriteStartElement("FinInstnId");
                    writer.WriteElementString("BIC", value: facturaRemesada.Factura(contexto).CuentaDeCargo(contexto).Banco(contexto).BicSwift.PadRight(11, 'X').Substring(0, 11));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("Dbtr");
                    writer.WriteElementString("Nm", value: $"{facturaRemesada.Factura(contexto).Cliente(contexto).Nombre}");
                    writer.WriteEndElement();
                    writer.WriteStartElement("DbtrAcct");
                    writer.WriteStartElement("Id");
                    writer.WriteElementString("IBAN", value: facturaRemesada.Factura(contexto).CuentaDeCargo(contexto).NumeroIban);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("RmtInf");
                    writer.WriteElementString("Ustrd", value: $"Nº: {facturaRemesada.Factura(contexto).NumeroDeFactura} Emitida: {facturaRemesada.Factura(contexto).FacturadaEl.Fecha().ToString("yyyy-MM-dd")}");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                #endregion
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static void GenerarSepaQ14(this RemesaPagDtm remesa, ContextoSe contexto, string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            if (!remesa.GeneradaEl.HasValue) GestorDeErrores.Emitir($"No se puede generar la remesa '{remesa.Referencia}' por no tener fecha de generación");
            var generadaEl = remesa.GeneradaEl.Fecha();

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                // Write the XML declaration
                writer.WriteStartDocument();

                // Write the root element
                writer.WriteStartElement("Document", "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03");

                // Write the CstmrCdtTrfInitn element
                writer.WriteStartElement("CstmrCdtTrfInitn");

                #region Write the GrpHdr element
                writer.WriteStartElement("GrpHdr");
                writer.WriteElementString("MsgId", $"{remesa.Referencia}");
                writer.WriteElementString("CreDtTm", value: $"{generadaEl.ToString("s")}");
                writer.WriteElementString("NbOfTxs", value: $"{remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count.ToString().PadLeft(15, '0')}");
                writer.WriteElementString("CtrlSum", value: $"{remesa.Total(contexto).ToString(CultureInfo.InvariantCulture)}");
                writer.WriteStartElement("InitgPty");
                writer.WriteElementString("Nm", $"{remesa.Sociedad(contexto).Expresion.Left(70)}");
                writer.WriteStartElement("Id");
                writer.WriteStartElement("OrgId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", $"ES00000{remesa.NifDelDeudor}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region instrucciones de pago
                writer.WriteStartElement("PmtInf");
                writer.WriteElementString("PmtInfId", remesa.Id.ToString().PadLeft(35, '0'));
                writer.WriteElementString("PmtMtd", "TRF");
                writer.WriteElementString("NbOfTxs", value: $"{remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count.ToString().PadLeft(15, '0')}");
                writer.WriteElementString("CtrlSum", value: $"{remesa.Total(contexto)}");
                writer.WriteElementString("ReqdExctnDt", value: remesa.PagarEl?.ToString("yyyy-MM-dd"));
                #endregion

                #region Información del deudor o pagador
                writer.WriteStartElement("Dbtr");
                writer.WriteElementString("Nm", $"{remesa.Sociedad(contexto).Expresion.Left(70)}");
                writer.WriteStartElement("Id");
                writer.WriteStartElement("OrgId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", $"{remesa.Sociedad(contexto).NIF}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region información de la cuenta deudora
                writer.WriteStartElement("DbtrAcct");
                writer.WriteStartElement("Id");
                writer.WriteElementString("IBAN", $"{remesa.CuentaDePago(contexto).Cuenta(contexto).NumeroIban}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region información de la entidad financiera que actua como agente del deudor
                writer.WriteStartElement("DbtrAgt");
                writer.WriteStartElement("FinInstnId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", "NOTPROVIDED");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                foreach (var pagoRemesado in remesa.Detalles<PagoDeUnaRemesaDtm>(contexto))
                {
                    //Informacion del acreedor y la deuda
                    writer.WriteStartElement("CdtTrfTxInf");
                    writer.WriteStartElement("PmtId");
                    writer.WriteElementString("EndToEndId", value: $"{pagoRemesado.Pago(contexto).Solicitante(contexto).Expresion(contexto)}");
                    writer.WriteEndElement();

                    writer.WriteStartElement("PmtTpInf");
                    writer.WriteStartElement("SvcLvl");
                    writer.WriteElementString("Cd", "SEPA");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("Amt");
                    writer.WriteElementString("InstdAmt", value: pagoRemesado.Pago(contexto).Importe.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();

                    writer.WriteStartElement("CdtrAgt");
                    writer.WriteStartElement("FinInstnId");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("Cdtr");
                    writer.WriteElementString("Nm", value: pagoRemesado.Pago(contexto).Solicitante(contexto).Nombre.Left(137));
                    writer.WriteEndElement();

                    writer.WriteStartElement("CdtrAcct");
                    writer.WriteStartElement("Id");
                    writer.WriteElementString("IBAN", value: pagoRemesado.Pago(contexto).CuentaDeAcreedor(contexto).NumeroIban);
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("RmtInf");
                    writer.WriteElementString("Ustrd", value: $"Nº: {pagoRemesado.Pago(contexto).Referencia} Emitida: {pagoRemesado.Pago(contexto).FechaCreacion.ToString("yyyy-MM-dd")}");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                //Fin de las instrucciones de pago
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

    }
}