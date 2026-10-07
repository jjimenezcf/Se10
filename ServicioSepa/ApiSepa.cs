
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Utilidades;

namespace ServicioXml
{
    public static class ApiSepa
    {
        public static void GenerarSepaPain008(this RemesaFaeDtm remesa, ContextoSe contexto, string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            if (!remesa.GeneradaEl.HasValue) GestorDeErrores.Emitir($"No se puede generar la remesa '{remesa.Referencia}' por no tener fecha de generación");
            var generadaEl = remesa.GeneradaEl.Fecha();
            var sociedad = remesa.Sociedad(contexto);
            var facturas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto);
            var total = FormatearImporte(remesa.Total(contexto));
            var cuentaDelAcreedor = remesa.CuentaDeAbono(contexto).Cuenta(contexto);
            var bicDelAcreedor = NormalizarBic(cuentaDelAcreedor.Banco(contexto, errorSiNoHay: false)?.BicSwift);

            // Identificadores SEPA (AT-02): ES + dígitos de control + sufijo + NIF. Si no hay presentador se usa el acreedor
            var idDelAcreedor = IdentificadorSepa(remesa.NifDelAcreedor, remesa.SufijoAcreedor);
            var idDelPresentador = string.IsNullOrWhiteSpace(remesa.NifDelPresentador)
                ? idDelAcreedor
                : IdentificadorSepa(remesa.NifDelPresentador, remesa.SufijoPresentador);

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Document", "urn:iso:std:iso:20022:tech:xsd:pain.008.001.08");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

                writer.WriteStartElement("CstmrDrctDbtInitn");
                #region Encabezado de grupo (GrpHdr)
                writer.WriteStartElement("GrpHdr");
                writer.WriteElementString("MsgId", $"{remesa.Referencia}");
                writer.WriteElementString("CreDtTm", value: $"{generadaEl.ToString("s")}");
                writer.WriteElementString("NbOfTxs", value: facturas.Count.ToString());
                writer.WriteElementString("CtrlSum", value: total);
                writer.WriteStartElement("InitgPty");
                writer.WriteElementString("Nm", NombreDelPresentador(remesa.Presentador, sociedad.RazonSocial).Left(70));
                EscribirIdentificacion(writer, idDelPresentador);
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Información del cobro (PmtInf)
                writer.WriteStartElement("PmtInf");
                writer.WriteElementString("PmtInfId", remesa.Id.ToString());
                writer.WriteElementString("PmtMtd", "DD");
                writer.WriteElementString("NbOfTxs", value: facturas.Count.ToString());
                writer.WriteElementString("CtrlSum", value: total);
                #region Prioridad de la instrucción (PmtTpInf): esquema CORE y tipo de secuencia
                writer.WriteStartElement("PmtTpInf");
                writer.WriteStartElement("SvcLvl");
                writer.WriteElementString("Cd", value: "SEPA");
                writer.WriteEndElement();
                writer.WriteStartElement("LclInstrm");
                writer.WriteElementString("Cd", value: "CORE");
                writer.WriteEndElement();
                // RCUR para todos los cobros: desde nov-2016 el esquema CORE admite RCUR también en el primer cobro de un mandato (FRST ya no es obligatorio)
                writer.WriteElementString("SeqTp", value: "RCUR");
                writer.WriteEndElement();
                #endregion
                writer.WriteElementString("ReqdColltnDt", value: remesa.CargarEl?.ToString("yyyy-MM-dd"));
                #region Acreedor (Cdtr)
                writer.WriteStartElement("Cdtr");
                writer.WriteElementString("Nm", sociedad.RazonSocial.Left(70));
                EscribirDireccionPostal(writer, sociedad.DireccionFiscal(contexto), contexto);
                writer.WriteEndElement();
                #endregion
                #region Cuenta del acreedor (CdtrAcct)
                writer.WriteStartElement("CdtrAcct");
                writer.WriteStartElement("Id");
                writer.WriteElementString("IBAN", LimpiarIban(cuentaDelAcreedor.NumeroIban));
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Agente de acreedor (CdtrAgt): banco propio donde se abonan los cobros. Obligatorio, BICFI o NOTPROVIDED
                writer.WriteStartElement("CdtrAgt");
                writer.WriteStartElement("FinInstnId");
                if (bicDelAcreedor != null)
                    writer.WriteElementString("BICFI", bicDelAcreedor);
                else
                {
                    writer.WriteStartElement("Othr");
                    writer.WriteElementString("Id", "NOTPROVIDED");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                #region Repercusión de los gastos (ChrgBr): "SLEV" = cada parte soporta sus propios gastos
                writer.WriteElementString("ChrgBr", "SLEV");
                #endregion
                #region Identificación del acreedor (CdtrSchmeId, AT-02)
                writer.WriteStartElement("CdtrSchmeId");
                writer.WriteStartElement("Id");
                writer.WriteStartElement("PrvtId");
                writer.WriteStartElement("Othr");
                writer.WriteElementString("Id", idDelAcreedor);
                writer.WriteStartElement("SchmeNm");
                writer.WriteElementString("Prtry", value: "SEPA");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                foreach (var facturaRemesada in facturas)
                {
                    var factura = facturaRemesada.Factura(contexto);
                    var cliente = factura.Cliente(contexto);
                    var cuentaDelCliente = cliente.CuentaDeCliente(contexto, ServicioDeDatos.Contabilidad.enumClaseDeCuentaBancaria.Pago);
                    var cuentaDeCargo = factura.CuentaDeCargo(contexto);
                    var bicDelDeudor = NormalizarBic(cuentaDeCargo.Banco(contexto, errorSiNoHay: false)?.BicSwift);

                    writer.WriteStartElement("DrctDbtTxInf");
                    writer.WriteStartElement("PmtId");
                    writer.WriteElementString("InstrId", value: $"{remesa.Id}{facturaRemesada.IdFactura}");
                    writer.WriteElementString("EndToEndId", value: $"{factura.NumeroDeFactura}".Left(35));
                    writer.WriteEndElement();
                    writer.WriteStartElement("InstdAmt");
                    writer.WriteAttributeString("Ccy", "EUR");
                    writer.WriteValue(FormatearImporte(factura.APagar(contexto)));
                    writer.WriteEndElement();
                    writer.WriteStartElement("DrctDbtTx");
                    writer.WriteStartElement("MndtRltdInf");
                    writer.WriteElementString("MndtId", value: cuentaDelCliente.IdArchivo.ToString());
                    writer.WriteElementString("DtOfSgntr", value: cuentaDelCliente.CertificadoDeCuenta(contexto).FechaCreacion.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    #region Agente del deudor (DbtrAgt): obligatorio, BICFI o NOTPROVIDED
                    writer.WriteStartElement("DbtrAgt");
                    writer.WriteStartElement("FinInstnId");
                    if (bicDelDeudor != null)
                        writer.WriteElementString("BICFI", bicDelDeudor);
                    else
                    {
                        writer.WriteStartElement("Othr");
                        writer.WriteElementString("Id", "NOTPROVIDED");
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    #endregion
                    writer.WriteStartElement("Dbtr");
                    writer.WriteElementString("Nm", value: cliente.RazonSocial(contexto).Left(70));
                    EscribirDireccionPostal(writer, factura.DireccionFiscal(contexto), contexto);
                    writer.WriteEndElement();
                    writer.WriteStartElement("DbtrAcct");
                    writer.WriteStartElement("Id");
                    writer.WriteElementString("IBAN", value: LimpiarIban(cuentaDeCargo.NumeroIban));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("RmtInf");
                    writer.WriteElementString("Ustrd", value: $"Factura: {factura.NumeroDeFactura} Emitida: {factura.FacturadaEl.Fecha().ToString("yyyy-MM-dd")}".Left(140));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                #endregion
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        // Identificador SEPA (AT-02) = "ES" + 2 dígitos de control + sufijo (3) + NIF.
        // Dígitos de control: ISO 7064 mod 97-10 sobre NIF + "ES" + "00" (sin el sufijo), con A=10 ... Z=35. Ej.: 47690558N -> ES23ZZZ47690558N
        private static string IdentificadorSepa(string nif, string sufijo)
        {
            nif = (nif ?? "").Replace("-", "").Replace(" ", "").ToUpperInvariant();
            var resto = 0;
            foreach (var c in nif + "ES00")
            {
                var cifras = char.IsDigit(c) ? c.ToString() : (c - 'A' + 10).ToString();
                foreach (var d in cifras)
                    resto = (resto * 10 + (d - '0')) % 97;
            }
            return $"ES{98 - resto:00}{sufijo}{nif}";
        }

        // Nombre de quien presenta el fichero (InitgPty): el de la remesa, junto a su NIF y sufijo; si no lo tiene, la razón social de la sociedad
        private static string NombreDelPresentador(string presentador, string razonSocialDeLaSociedad) =>
            string.IsNullOrWhiteSpace(presentador) ? razonSocialDeLaSociedad : presentador.Trim();

        // IBAN2007Identifier no admite guiones ni espacios: [A-Z]{2,2}[0-9]{2,2}[a-zA-Z0-9]{1,30}
        private static string LimpiarIban(string iban) => iban?.Replace("-", "").Replace(" ", "");

        // Los bancos españoles exigen 2 decimales exactos en los importes SEPA, aunque el XSD admita hasta 5
        private static string FormatearImporte(decimal importe) => importe.ToString("F2", CultureInfo.InvariantCulture);

        // BICFI: 8 u 11 caracteres [A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?. Si no es válido se trata como no informado
        private static string NormalizarBic(string bic)
        {
            if (string.IsNullOrWhiteSpace(bic)) return null;
            bic = bic.Trim().ToUpperInvariant();
            return Regex.IsMatch(bic, "^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$") ? bic.PadRight(11, 'X') : null;
        }

        // Id/OrgId/Othr/Id: identificación de una organización (en transferencias: NIF + sufijo)
        private static void EscribirIdentificacion(XmlWriter writer, string identificador)
        {
            writer.WriteStartElement("Id");
            writer.WriteStartElement("OrgId");
            writer.WriteStartElement("Othr");
            writer.WriteElementString("Id", identificador);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        // PstlAdr (PostalAddress) exige respetar el orden del XSD: StrtNm, BldgNb, PstCd, TwnNm, CtrySubDvsn, Ctry
        private static void EscribirDireccionPostal(XmlWriter writer, DireccionDto direccion, ContextoSe contexto)
        {
            if (direccion is null) return;

            writer.WriteStartElement("PstlAdr");
            if (!string.IsNullOrWhiteSpace(direccion.Calle))
                writer.WriteElementString("StrtNm", direccion.Calle.Trim().Left(70));
            if (direccion.Numero.HasValue)
                writer.WriteElementString("BldgNb", direccion.Numero.Value.ToString());
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
            var sociedad = remesa.Sociedad(contexto);
            var pagos = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
            var total = FormatearImporte(remesa.Total(contexto));
            var cuentaDelDeudor = remesa.CuentaDePago(contexto).Cuenta(contexto);
            var bicDelDeudor = NormalizarBic(cuentaDelDeudor.Banco(contexto, errorSiNoHay: false)?.BicSwift);

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Document", "urn:iso:std:iso:20022:tech:xsd:pain.001.001.09");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

                writer.WriteStartElement("CstmrCdtTrfInitn");

                #region Encabezado de grupo (GrpHdr)
                writer.WriteStartElement("GrpHdr");
                writer.WriteElementString("MsgId", $"{remesa.Referencia}");
                writer.WriteElementString("CreDtTm", value: $"{generadaEl.ToString("s")}");
                writer.WriteElementString("NbOfTxs", value: pagos.Count.ToString());
                writer.WriteElementString("CtrlSum", value: total);
                writer.WriteStartElement("InitgPty");
                writer.WriteElementString("Nm", NombreDelPresentador(remesa.Presentador, sociedad.RazonSocial).Left(70));
                // Identificación del presentador: NIF + sufijo (código de 3 cifras que asigna el banco), p.ej. A30054209000
                EscribirIdentificacion(writer, $"{remesa.NifDelPresentador}{remesa.SufijoPresentador}");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region Instrucciones de pago (PmtInf)
                writer.WriteStartElement("PmtInf");
                writer.WriteElementString("PmtInfId", remesa.Id.ToString());
                writer.WriteElementString("PmtMtd", "TRF");
                writer.WriteElementString("NbOfTxs", value: pagos.Count.ToString());
                writer.WriteElementString("CtrlSum", value: total);
                #region Prioridad de la instrucción (PmtTpInf)
                writer.WriteStartElement("PmtTpInf");
                writer.WriteStartElement("SvcLvl");
                writer.WriteElementString("Cd", value: "SEPA");
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion
                writer.WriteStartElement("ReqdExctnDt");
                writer.WriteElementString("Dt", value: remesa.PagarEl?.ToString("yyyy-MM-dd"));
                writer.WriteEndElement();
                #endregion

                #region Información del deudor o pagador (Dbtr)
                writer.WriteStartElement("Dbtr");
                writer.WriteElementString("Nm", sociedad.RazonSocial.Left(70));
                EscribirDireccionPostal(writer, sociedad.DireccionFiscal(contexto), contexto);
                // Identificación del deudor: NIF + sufijo, igual que en el presentador
                EscribirIdentificacion(writer, $"{sociedad.NIF}{remesa.SufijoDeudor}");
                writer.WriteEndElement();
                #endregion

                #region Información de la cuenta deudora (DbtrAcct)
                writer.WriteStartElement("DbtrAcct");
                writer.WriteStartElement("Id");
                writer.WriteElementString("IBAN", LimpiarIban(cuentaDelDeudor.NumeroIban));
                writer.WriteEndElement();
                writer.WriteElementString("Ccy", "EUR");
                writer.WriteEndElement();
                #endregion

                #region Información de la entidad financiera que actua como agente del deudor (DbtrAgt): obligatorio, BICFI o NOTPROVIDED
                writer.WriteStartElement("DbtrAgt");
                writer.WriteStartElement("FinInstnId");
                if (bicDelDeudor != null)
                    writer.WriteElementString("BICFI", bicDelDeudor);
                else
                {
                    writer.WriteStartElement("Othr");
                    writer.WriteElementString("Id", "NOTPROVIDED");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                #endregion

                #region Repercusión de los gastos (ChrgBr): obligatorio en el Rulebook SEPA, "SLEV" = cada parte soporta sus propios gastos
                writer.WriteElementString("ChrgBr", "SLEV");
                #endregion

                foreach (var pagoRemesado in pagos)
                {
                    var pago = pagoRemesado.Pago(contexto);
                    var solicitante = pago.Solicitante(contexto);
                    var cuentaDeAcreedor = pago.CuentaDeAcreedor(contexto);
                    var bicDelAcreedor = NormalizarBic(cuentaDeAcreedor?.Banco(contexto, errorSiNoHay: false)?.BicSwift);

                    var facturaRec = pago.FacturaRec(contexto, errorSiNoHay: false);
                    var direccionDelAcreedor = facturaRec != null
                        ? facturaRec.DireccionFiscal(contexto)
                        : solicitante.DireccionFiscal(contexto);

                    //Informacion del acreedor y la deuda
                    writer.WriteStartElement("CdtTrfTxInf");
                    writer.WriteStartElement("PmtId");
                    writer.WriteElementString("InstrId", value: $"{remesa.Id}{pagoRemesado.Id}");
                    writer.WriteElementString("EndToEndId", value: $"{pago.Referencia}");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Amt");
                    writer.WriteStartElement("InstdAmt");
                    writer.WriteAttributeString("Ccy", "EUR");
                    writer.WriteValue(FormatearImporte(pago.Importe));
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    // CdtrAgt es opcional: sólo se informa si se conoce un BICFI válido
                    if (bicDelAcreedor != null)
                    {
                        writer.WriteStartElement("CdtrAgt");
                        writer.WriteStartElement("FinInstnId");
                        writer.WriteElementString("BICFI", bicDelAcreedor);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }

                    writer.WriteStartElement("Cdtr");
                    writer.WriteElementString("Nm", value: solicitante.RazonSocial(contexto).Left(70));
                    EscribirDireccionPostal(writer, direccionDelAcreedor, contexto);
                    writer.WriteEndElement();

                    writer.WriteStartElement("CdtrAcct");
                    writer.WriteStartElement("Id");
                    writer.WriteElementString("IBAN", value: LimpiarIban(cuentaDeAcreedor.NumeroIban));
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteStartElement("RmtInf");
                    writer.WriteElementString("Ustrd", value: $"Ref: {pago.Referencia} Emitida: {pago.FechaCreacion.ToString("yyyy-MM-dd")}".Left(140));
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