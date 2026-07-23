
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using System.Globalization;
using System.Xml;
using Utilidades;

namespace ServicioXml
{
    public static class ApiSepa
    {
        [Obsolete("Usar GenerarSepaPain008: este método genera un XML pain.008.001.02 incompleto (CdtrAgt sin BIC real, sin ChrgBr, sin PstlAdr del deudor).")]
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

        public static void GenerarSepaPain008(this RemesaFaeDtm remesa, ContextoSe contexto, string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            if (!remesa.GeneradaEl.HasValue) GestorDeErrores.Emitir($"No se puede generar la remesa '{remesa.Referencia}' por no tener fecha de generación");
            var generadaEl = remesa.GeneradaEl.Fecha();

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
                writer.WriteElementString("CtrlSum", value: $"{remesa.Total(contexto).ToString(CultureInfo.InvariantCulture)}");
                #region Prioridad de la instrucción (PmtTpInf)
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
                EscribirDireccionPostal(writer, remesa.Sociedad(contexto).DireccionFiscal(contexto), contexto);
                writer.WriteEndElement();
                #endregion
                #region Cuenta del acreedor (CdtrAcct)
                writer.WriteStartElement("CdtrAcct");
                writer.WriteStartElement("Id");
                writer.WriteElementString("IBAN", $"{remesa.CuentaDeAbono(contexto).Cuenta(contexto).NumeroIban}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Agente de acreedor (CdtrAgt): banco propio donde se abonan los cobros
                var bicDelAcreedor = remesa.CuentaDeAbono(contexto).Cuenta(contexto).Banco(contexto, errorSiNoHay: false)?.BicSwift;
                writer.WriteStartElement("CdtrAgt");
                writer.WriteStartElement("FinInstnId");
                if (!string.IsNullOrWhiteSpace(bicDelAcreedor))
                    writer.WriteElementString("BIC", value: bicDelAcreedor.PadRight(11, 'X').Substring(0, 11));
                else
                    writer.WriteElementString("Othr", "NOTPROVIDED");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Repercusión de los gastos (ChrgBr): obligatorio en el Rulebook SEPA, "SLEV" = cada parte soporta sus propios gastos
                writer.WriteElementString("ChrgBr", "SLEV");
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
                    var factura = facturaRemesada.Factura(contexto);

                    writer.WriteStartElement("DrctDbtTxInf");
                    writer.WriteStartElement("PmtId");
                    writer.WriteElementString("InstrId", value: $"{remesa.Id}{facturaRemesada.IdFactura}");
                    writer.WriteElementString("EndToEndId", value: $"{remesa.Id}{facturaRemesada.IdFactura}");
                    writer.WriteEndElement();
                    writer.WriteStartElement("InstdAmt");
                    writer.WriteAttributeString("Ccy", "EUR");
                    writer.WriteValue(factura.APagar(contexto).ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                    writer.WriteStartElement("DrctDbtTx");
                    writer.WriteStartElement("MndtRltdInf");
                    writer.WriteElementString("MndtId", value: factura.Cliente(contexto).CuentaDeCliente(contexto, ServicioDeDatos.Contabilidad.enumClaseDeCuentaBancaria.Pago).IdArchivo.ToString());
                    writer.WriteElementString("DtOfSgntr", value: factura.Cliente(contexto).CuentaDeCliente(contexto, ServicioDeDatos.Contabilidad.enumClaseDeCuentaBancaria.Pago).CertificadoDeCuenta(contexto).FechaCreacion.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("DbtrAgt");
                    writer.WriteStartElement("FinInstnId");
                    writer.WriteElementString("BIC", value: factura.CuentaDeCargo(contexto).Banco(contexto).BicSwift.PadRight(11, 'X').Substring(0, 11));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("Dbtr");
                    writer.WriteElementString("Nm", value: factura.Cliente(contexto).Nombre.Left(70));
                    EscribirDireccionPostal(writer, factura.DireccionFiscal(contexto), contexto);
                    writer.WriteEndElement();
                    writer.WriteStartElement("DbtrAcct");
                    writer.WriteStartElement("Id");
                    writer.WriteElementString("IBAN", value: factura.CuentaDeCargo(contexto).NumeroIban);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("RmtInf");
                    writer.WriteElementString("Ustrd", value: $"Nº: {factura.NumeroDeFactura} Emitida: {factura.FacturadaEl.Fecha().ToString("yyyy-MM-dd")}");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                #endregion
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        // PstlAdr (PostalAddress6) exige respetar el orden del XSD: StrtNm, PstCd, TwnNm, CtrySubDvsn, Ctry
        private static void EscribirDireccionPostal(XmlWriter writer, DireccionDto direccion, ContextoSe contexto)
        {
            if (direccion is null) return;

            writer.WriteStartElement("PstlAdr");
            var calle = $"{direccion.Calle} {direccion.Numero}".Trim();
            if (!string.IsNullOrWhiteSpace(calle))
                writer.WriteElementString("StrtNm", calle.Left(70));
            if (!string.IsNullOrWhiteSpace(direccion.CodigoPostal))
                writer.WriteElementString("PstCd", direccion.CodigoPostal.Left(16));
            if (!string.IsNullOrWhiteSpace(direccion.Municipio))
                writer.WriteElementString("TwnNm", direccion.Municipio.Left(35));
            if (!string.IsNullOrWhiteSpace(direccion.Provincia))
                writer.WriteElementString("CtrySubDvsn", direccion.Provincia.Left(35));
            var iso2 = contexto.SeleccionarPorId<PaisDtm>(direccion.IdPais)?.ISO2;
            if (!string.IsNullOrWhiteSpace(iso2))
                writer.WriteElementString("Ctry", iso2);
            writer.WriteEndElement();
        }

        public static void GenerarSepaPain001(this RemesaPagDtm remesa, ContextoSe contexto, string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            if (!remesa.GeneradaEl.HasValue) GestorDeErrores.Emitir($"No se puede generar la remesa '{remesa.Referencia}' por no tener fecha de generación");
            var generadaEl = remesa.GeneradaEl.Fecha();

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Document", "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "schemaLocation", null, "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03 pain.001.001.03.xsd");

                writer.WriteStartElement("CstmrCdtTrfInitn");

                #region Encabezado de grupo (GrpHdr)
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

                #region Instrucciones de pago (PmtInf)
                writer.WriteStartElement("PmtInf");
                writer.WriteElementString("PmtInfId", remesa.Id.ToString().PadLeft(35, '0'));
                writer.WriteElementString("PmtMtd", "TRF");
                writer.WriteElementString("NbOfTxs", value: $"{remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count.ToString().PadLeft(15, '0')}");
                writer.WriteElementString("CtrlSum", value: $"{remesa.Total(contexto).ToString(CultureInfo.InvariantCulture)}");
                #region Prioridad de la instrucción (PmtTpInf)
                writer.WriteStartElement("PmtTpInf");
                writer.WriteStartElement("SvcLvl");
                writer.WriteElementString("Cd", value: "SEPA");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                writer.WriteElementString("ReqdExctnDt", value: remesa.PagarEl?.ToString("yyyy-MM-dd"));
                #endregion

                #region Información del deudor o pagador (Dbtr)
                writer.WriteStartElement("Dbtr");
                writer.WriteElementString("Nm", $"{remesa.Sociedad(contexto).Expresion.Left(70)}");
                EscribirDireccionPostal(writer, remesa.Sociedad(contexto).DireccionFiscal(contexto), contexto);
                writer.WriteStartElement("Id");
                writer.WriteStartElement("OrgId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", $"{remesa.Sociedad(contexto).NIF}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region Información de la cuenta deudora (DbtrAcct)
                writer.WriteStartElement("DbtrAcct");
                writer.WriteStartElement("Id");
                writer.WriteElementString("IBAN", $"{remesa.CuentaDePago(contexto).Cuenta(contexto).NumeroIban}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region Información de la entidad financiera que actua como agente del deudor (DbtrAgt)
                writer.WriteStartElement("DbtrAgt");
                writer.WriteStartElement("FinInstnId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", "NOTPROVIDED");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region Repercusión de los gastos (ChrgBr): obligatorio en el Rulebook SEPA, "SLEV" = cada parte soporta sus propios gastos
                writer.WriteElementString("ChrgBr", "SLEV");
                #endregion

                foreach (var pagoRemesado in remesa.Detalles<PagoDeUnaRemesaDtm>(contexto))
                {
                    var pago = pagoRemesado.Pago(contexto);
                    var cuentaDeAcreedor = pago.CuentaDeAcreedor(contexto);

                    //Informacion del acreedor y la deuda
                    writer.WriteStartElement("CdtTrfTxInf");
                    writer.WriteStartElement("PmtId");
                    writer.WriteElementString("InstrId", value: $"{remesa.Id}{pagoRemesado.Id}");
                    writer.WriteElementString("EndToEndId", value: $"{pago.Solicitante(contexto).Expresion(contexto)}");
                    writer.WriteEndElement();

                    writer.WriteStartElement("PmtTpInf");
                    writer.WriteStartElement("SvcLvl");
                    writer.WriteElementString("Cd", "SEPA");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("Amt");
                    writer.WriteStartElement("InstdAmt");
                    writer.WriteAttributeString("Ccy", "EUR");
                    writer.WriteValue(pago.Importe.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    var bicDelAcreedor = cuentaDeAcreedor?.Banco(contexto, errorSiNoHay: false)?.BicSwift;
                    writer.WriteStartElement("CdtrAgt");
                    writer.WriteStartElement("FinInstnId");
                    if (!string.IsNullOrWhiteSpace(bicDelAcreedor))
                        writer.WriteElementString("BIC", value: bicDelAcreedor.PadRight(11, 'X').Substring(0, 11));
                    else
                        writer.WriteElementString("Othr", "NOTPROVIDED");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    var facturaRec = pago.FacturaRec(contexto, errorSiNoHay: false);
                    var direccionDelAcreedor = facturaRec != null
                        ? facturaRec.DireccionFiscal(contexto)
                        : pago.Solicitante(contexto).DireccionFiscal(contexto);

                    writer.WriteStartElement("Cdtr");
                    writer.WriteElementString("Nm", value: pago.Solicitante(contexto).Nombre.Left(70));
                    EscribirDireccionPostal(writer, direccionDelAcreedor, contexto);
                    writer.WriteEndElement();

                    writer.WriteStartElement("CdtrAcct");
                    writer.WriteStartElement("Id");
                    writer.WriteElementString("IBAN", value: cuentaDeAcreedor.NumeroIban);
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("RmtInf");
                    writer.WriteElementString("Ustrd", value: $"Nº: {pago.Referencia} Emitida: {pago.FechaCreacion.ToString("yyyy-MM-dd")}");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                //Fin de las instrucciones de pago
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        [Obsolete("Usar GenerarSepaPain001: este método genera un XML pain.001.001.03 incompleto (sin Ccy en InstdAmt, sin CdtrAgt ni ChrgBr).")]
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