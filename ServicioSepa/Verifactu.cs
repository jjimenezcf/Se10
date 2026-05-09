using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System.Reflection;
using System.Text;
using Utilidades;
using VeriFactu.Blockchain;
using VeriFactu.Business;
using VeriFactu.Business.Operations;
using VeriFactu.Config;
using VeriFactu.Net.Rest;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Net.Core.Src.Business.Operations;

namespace ServicioXml
{
    public static class Verifactu
    {
        public static void ValidarNif(this FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion)
        {
            facturaEmt.Cliente(contexto).ValidarCliente(contexto, certificado, numeroDeInstalacion);
        }

        public static void ValidarCliente(this ClienteDtm cliente, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion)
        {
            var nif = cliente.NIF(contexto, quitarPrefijoEs: true);
            var rs = cliente.RazonSocial(contexto);

            ValidarNif(contexto, nif, rs, certificado, numeroDeInstalacion);
        }

        public static void ValidarNif(ContextoSe contexto, string nif, string razonSocial, Certificado certificado, string numeroDeInstalacion)
        {
            AjustarSetting(certificado, numeroDeInstalacion);
            var validadorNif = new VeriFactu.Business.Validation.NIF.NifValidation(nif, razonSocial);

            var errores = validadorNif.GetErrors();
            if (errores.Count() > 0)
            {
                GestorDeErrores.Emitir($"La AEAT indica que el Nif '{nif}' de la razón social '{razonSocial}' no se ha identificado");
            }
        }
        public static void ValidarVat(this FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion)
        {
            var vat = facturaEmt.Cliente(contexto).VAT;

            AjustarSetting(certificado, numeroDeInstalacion);
            if (!VeriFactu.Business.Validation.VIES.ViesVatNumber.Validate(vat))
            {
                GestorDeErrores.Emitir($"El VAT '{vat}' del cliente con Nif '{facturaEmt.Cliente(contexto).NIF(contexto)}' no está en el ROI");
            }
        }

        public static void ValidarConexionConLaAeat(this SociedadDtm sociedad, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion, int ano, int mes)
        {
            AjustarSetting(certificado, numeroDeInstalacion);

            var invoiceQuery = new InvoiceQuery(sociedad.NIFSinIsoEs, sociedad.RazonSocial);

            // detectar si hay coexión
            var _ = invoiceQuery.GetSales(ano.ToString(), mes.ToString());
        }

        public static List<FacturaAeatDto> ConsultaDeFacturasEmitidas(this SociedadDtm sociedad, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion, int ano, int mes)
        {
            AjustarSetting(certificado, numeroDeInstalacion);

            var invoiceQuery = new InvoiceQuery(sociedad.NIFSinIsoEs, sociedad.RazonSocial);

            // Consulta facturas emitidas
            var salesResponse = invoiceQuery.GetSales(ano.ToString(), mes.ToString());

            // Lista de objetos Invoice a partir de la respuesta AEAT facturas emitidas
            var salesInvoices = InvoiceQuery.GetInvoices(salesResponse);
            var facturasAeat = new List<FacturaAeatDto>();
            if (salesInvoices.Count > 0)
            {
                foreach (var sale in salesInvoices)
                {
                    var facturaAeat = new FacturaAeatDto();

                    //rechazo por ser subsanación
                    if (facturasAeat.Find(f => f.NumeroFactura == sale.InvoiceID) is not null)
                        continue;

                    if (sale.InvoiceID.Split('-').Length != 3)
                        continue;

                    var retencion = sale.TaxItems.Where(t => t.Tax == Impuesto.OTROS && t.TaxBase < 0 && t.TaxType == CalificacionOperacion.N1).Sum(t => t.TaxBase);
                    var iva = sale.TaxItems.Where(t => t.Tax == Impuesto.IVA).Sum(t => t.TaxAmount);

                    facturaAeat.Ano = sale.InvoiceID.Split('-')[0];
                    facturaAeat.Serie = sale.InvoiceID.Split('-')[1];
                    facturaAeat.Numero = sale.InvoiceID.Split('-')[2];
                    facturaAeat.NombreCliente = sale.BuyerName;
                    facturaAeat.NifCliente = sale.BuyerID;
                    facturaAeat.FechaFactura = sale.OperationDate == null ? sale.InvoiceDate: (DateTime) sale.OperationDate;

                    facturaAeat.BI = (sale.TotalAmount - iva - retencion).Moneda();
                    facturaAeat.Impuestos = iva.Moneda();
                    facturaAeat.Retencion = retencion.Moneda();
                    facturaAeat.Total = sale.TotalAmount.Moneda();
                    facturaAeat.TipoAeat = sale.InvoiceType.ToString();
                    facturaAeat.TipoRectificativa = sale.RectificationType.ToString();
                    facturaAeat.Huella = salesResponse.GetHuellaFromQueryAeatResponse(sale.InvoiceID);

                    //rechazo por no haber sido generado en este entorno
                    var facturaEmt = sociedad.LeerFacturaEmt(contexto, facturaAeat.NumeroFactura, errorSiNoHay: false);
                    if (facturaEmt is null)
                        continue;

                    facturaAeat.Id = facturaEmt.Id;
                    facturasAeat.Add(facturaAeat);
                }
            }
            return facturasAeat.OrderBy(f => f.FechaFactura).ThenBy(f => f.Id).ToList();
        }


        public static AuditoriaSii AltaDeFacturaSIF(this FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion, Func<string, ServicioDeDatos.TrabajosSometidos.TrazaDeUnTrabajoDtm> crearTraza)
        {
            var sociedad = facturaEmt.Sociedad(contexto);
            AjustarSetting(certificado, numeroDeInstalacion);

            var ivas = facturaEmt.Ivas(contexto);
            var taxas = CrearImpuestosFactura(ivas);
            taxas.IncluirRetencionesFactura(facturaEmt.Irpfs(contexto));

            var datosInvoce = new InvoiceData
            {
                InvoiceID = facturaEmt.NumeroDeFactura,
                InvoiceDate = facturaEmt.EmitidaEl.Fecha(),
                OperationDate = facturaEmt.FacturadaEl.Fecha(),
                SellerID = sociedad.NIFSinIsoEs,
                InvoiceType = TipoFactura.F1,
                SellerName = sociedad.RazonSocial,
                BuyerID = facturaEmt.Cliente(contexto).NIF(contexto, quitarPrefijoEs: true),
                BuyerName = facturaEmt.Cliente(contexto).RazonSocial(contexto),
                Text = facturaEmt.Nombre,
                TaxItems = taxas
            };

            // Creamos una instacia de la clase factura
            var invoice = new Invoice(datosInvoce);

            return EnvioAEAT(contexto, invoice, crearTraza);
        }

        public static void CancelarFacturaSIF(this FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion)
        {
            var vendedorNif = facturaEmt.Sociedad(contexto).NIFSinIsoEs;
            AjustarSetting(certificado, numeroDeInstalacion);
            var invoice = new Invoice(facturaEmt.NumeroDeFactura, facturaEmt.FacturadaEl.Fecha(), vendedorNif)
            {
                SellerName = facturaEmt.Sociedad(contexto).RazonSocial,
            };

            // Creamos la cancelación de la factura
            var invoiceCancellation = new InvoiceCancellation(invoice);

            // Guardamos la cancelación factura
            var rutaLog = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, invoice.SellerID);
            invoiceCancellation.Save(rutaLog, $"{nameof(CancelarFacturaSIF)}_{facturaEmt.NumeroDeFactura}.txt");

            if (invoiceCancellation.Status != ltrSii.Respuesta.Correcta)
            {
                throw new Exception($"Error al cancelar la factura {facturaEmt.NumeroDeFactura}: {invoiceCancellation.ErrorCode} - {invoiceCancellation.ErrorDescription}");
            }
        }

        public static AuditoriaSii RectificarFacturaPorSustitucionSIF(this FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion, Func<string, ServicioDeDatos.TrabajosSometidos.TrazaDeUnTrabajoDtm> crearTraza)
        {
            var bi = facturaEmt.RectificaA(contexto).Ivas(contexto).Sum(i => i.BI);
            var iva = facturaEmt.RectificaA(contexto).Ivas(contexto).Sum(i => i.Importe);
            var invoice = CrearInvoiceRectificativa(facturaEmt, contexto, certificado, numeroDeInstalacion, TipoFactura.R1, TipoRectificativa.S);
            invoice.RectificationTaxBase = Math.Round(bi, 2);
            invoice.RectificationTaxAmount = Math.Round(iva, 2);
            return EnvioAEAT(contexto, invoice, crearTraza);
        }

        public static AuditoriaSii RectificarFacturaPorDiferenciaSIF(this FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion, Func<string, ServicioDeDatos.TrabajosSometidos.TrazaDeUnTrabajoDtm> crearTraza)
        {
            // Diferencias: TipoFactura.R1 y TipoRectificativa.I
            var invoice = CrearInvoiceRectificativa(facturaEmt, contexto, certificado, numeroDeInstalacion, TipoFactura.R1, TipoRectificativa.I);
            return EnvioAEAT(contexto, invoice, crearTraza);
        }

        private static Invoice CrearInvoiceRectificativa(FacturaEmtDtm facturaEmt, ContextoSe contexto, Certificado certificado, string numeroDeInstalacion, TipoFactura tipoFactura, TipoRectificativa tipoRectificativa)
        {
            var sociedad = facturaEmt.Sociedad(contexto);
            var vendedorNif = sociedad.NIFSinIsoEs;
            AjustarSetting(certificado, numeroDeInstalacion);

            var ivas = facturaEmt.Ivas(contexto);
            var taxas = CrearImpuestosFactura(ivas);
            taxas.IncluirRetencionesFactura(facturaEmt.Irpfs(contexto));
            var datosInvoice = new InvoiceData
            {
                InvoiceID = facturaEmt.NumeroDeFactura,
                InvoiceDate = facturaEmt.EmitidaEl.Fecha(),
                OperationDate = facturaEmt.FacturadaEl.Fecha(),
                SellerID = sociedad.NIFSinIsoEs,
                InvoiceType = tipoFactura,
                RectificationType = tipoRectificativa,
                RectificationItems = new List<RectificationItem>
                {
                    new RectificationItem
                    {
                        InvoiceID = facturaEmt.RectificaA(contexto).NumeroDeFactura,
                        InvoiceDate = facturaEmt.RectificaA(contexto).EmitidaEl.Fecha()
                    }
                },
                SellerName = sociedad.RazonSocial,
                BuyerID = facturaEmt.Cliente(contexto).NIF(contexto, quitarPrefijoEs: true),
                BuyerName = facturaEmt.Cliente(contexto).RazonSocial(contexto),
                Text = facturaEmt.Nombre,
                TaxItems = taxas
            };

            var invoice = new Invoice(datosInvoice);

            return invoice;
        }

        private static AuditoriaSii EnvioAEAT(ContextoSe contexto, Invoice invoice, Func<string, ServicioDeDatos.TrabajosSometidos.TrazaDeUnTrabajoDtm> crearTraza)
        {
            var rutaLog = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, invoice.SellerID);
            // Creamos la entrada de la factura
            try
            {
                var invoiceEntry = new InvoiceEntry(invoice);

                // Guardamos la factura
                invoiceEntry.Save(rutaLog, $"{nameof(AltaDeFacturaSIF)}_{invoice.InvoiceID}.txt");

                // Consultamos el estado
                if (invoiceEntry.Status == ltrSii.Respuesta.Correcta || invoiceEntry.Status == ltrSii.Respuesta.ParcialmenteCorrecta)
                {
                    var auditoria = new AuditoriaSii
                    {
                        InvoiceEntryID = invoiceEntry.InvoiceEntryID,
                        EncodedInvoiceID = invoiceEntry.EncodedInvoiceID,
                        FicheroEnviado = ObtenerRutaQueComience(invoiceEntry.InvoiceFilePath),
                        FacturaEnviada = invoiceEntry.InvoiceEntryFilePath,
                        CSV = invoiceEntry.CSV,
                        FicheroDeRespuesta = ObtenerRutaQueComience(invoiceEntry.ResponseFilePath),
                        RutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, invoice.SellerID),
                        Respuesta = invoiceEntry.Status,
                        Codigo = invoiceEntry.ErrorCode,
                        Error = invoiceEntry.ErrorDescription,
                        Huella = invoiceEntry.Registro.Huella
                    };

                    var contenido = new StringBuilder();

                    contenido.AppendLine($"Nuevo Id: {invoiceEntry.InvoiceEntryID.Replace("0", "")}");
                    contenido.AppendLine($"Nueva factura registrada: {invoice.InvoiceID}");
                    contenido.AppendLine($"CSV:{invoiceEntry.CSV}");
                    contenido.AppendLine($"Huella:{invoiceEntry.Registro.Huella}");

                    contenido.AppendLine($"InvoiceEntryID:{invoiceEntry.InvoiceEntryID}");
                    contenido.AppendLine($"EncodedInvoiceID:{invoiceEntry.EncodedInvoiceID}");
                    contenido.AppendLine($"DirectorioDeEnvio:{invoiceEntry.InvoicePath}");
                    contenido.AppendLine($"FicheroEnviado:{invoiceEntry.InvoiceFilePath}");
                    contenido.AppendLine($"FacturaEnviada:{invoiceEntry.InvoiceEntryFilePath}");
                    contenido.AppendLine($"FicheroDeRespuesta:{invoiceEntry.ResponseFilePath}");
                    contenido.AppendLine($"RutaBlockChain:{Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, invoice.SellerID)}");

                    crearTraza(contenido.ToString());

                    return auditoria;
                }

                throw new Exception($"Respuesta de la AEAT:\n{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}");
            }
            catch (Exception exc)
            {
                contexto.IniciarTraza(rutaLog, $"Error_{invoice.InvoiceID}", debugar: true);
                contexto.AnotarTraza($"Error al registrar la factura {invoice.InvoiceID}", exc.MensajeCompleto());
                contexto.CerrarTraza();
                contexto.RegistrarConEnvio(CacheDeVariable.CFG_Ruta_Ficheros_De_Excepciones, nameof(AltaDeFacturaSIF), "Error al hacer el alta de una factura", GestorDeErrores.Detalle(exc));
                throw;
            }
            finally
            {
                if (File.Exists(Settings.Current.CertificatePath)) File.Delete(Settings.Current.CertificatePath);
            }
        }

        private static string ObtenerRutaQueComience(string ruta)
        {
            // Verificar si el fichero existe directamente
            if (File.Exists(ruta))
            {
                return ruta; // Devolver la ruta original si existe
            }

            // Extraer componentes de la ruta
            string directorio = Path.GetDirectoryName(ruta);
            string nombreBase = Path.GetFileNameWithoutExtension(ruta);

            // Validar componentes
            if (string.IsNullOrEmpty(directorio) ||
                string.IsNullOrEmpty(nombreBase) ||
                !Directory.Exists(directorio))
            {
                return string.Empty;
            }

            try
            {
                // Buscar el primer fichero que comience con el nombre base
                var directorios = Directory.EnumerateFiles(directorio);
                var ficheros = directorios.Select(Path.GetFileName);
                var primerCoincidencia = ficheros.FirstOrDefault(f => f.StartsWith(nombreBase, StringComparison.OrdinalIgnoreCase));

                return primerCoincidencia != null
                    ? Path.Combine(directorio, primerCoincidencia)
                    : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty; // Manejo seguro de excepciones
            }
        }

        private static void AjustarSetting(Certificado certificado, string numeroDeInstalacion)
        {
            Settings.SetBasePath(CacheDeVariable.CFG_Ruta_Ficheros_De_Sii);
            Settings.Current = new Settings
            {
                IDVersion = "1.0",
                InboxPath = CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox,
                OutboxPath = CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox,
                BlockchainPath = CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains,
                InvoicePath = CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices,
                LogPath = CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log,
                LoggingEnabled = true,
                CertificateSerial = "",
                CertificateThumbprint = "",
                CertificatePath = certificado.RutaDelCertificado,
                CertificatePassword = certificado.Password,
                VeriFactuEndPointPrefix = ParametrosDelSii.SII_UrlDeRegistro,
                VeriFactuEndPointValidatePrefix = ParametrosDelSii.SII_URLDeValidarQr,
                VeriFactuHashAlgorithm = TipoHuella.Sha256,
                VeriFactuHashInputEncoding = "UTF-8",
                SistemaInformatico = new SistemaInformatico()
                {
                    NIF = ParametrosDelSii.SSII_NIF,
                    NombreRazon = ParametrosDelSii.SSII_RazonSocial,
                    NombreSistemaInformatico = ParametrosDelSii.SSII_NombreDelSistema,
                    IdSistemaInformatico = "01",
                    Version = $"{Assembly.GetExecutingAssembly().GetName().Version}",
                    NumeroInstalacion = numeroDeInstalacion,
                    TipoUsoPosibleSoloVerifactu = "S",
                    TipoUsoPosibleMultiOT = "S",
                    IndicadorMultiplesOT = "S"
                },
                Api = new Api()
                {
                    EndPointCreate = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/Create",
                    EndPointCancel = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/Cancel",
                    EndPointGetQrCode = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetQrCode",
                    EndPointGetSellers = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetSellers",
                    EndPointGetRecords = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetFilteredList",
                    EndPointGetAeatInvoices = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetFilteredList",
                    ServiceKey = "1234"
                },
                SkipNifAeatValidation = true,
                SkipViesVatNumberValidation = true
            };

            Settings.Save();
            Blockchain.ReloadBlockchainsFromDisk();
        }

        private static List<TaxItem> CrearImpuestosFactura(List<ImportePorTipoDeIva> ivas)
        {
            var impuestos = new List<TaxItem>();
            foreach (var iva in ivas)
            {
                TaxItem tasa = new TaxItem();
                tasa.TaxClass = TaxClass.TaxOutput;

                if (iva.EsNosujeto)
                {
                    tasa.Tax = Impuesto.OTROS;
                    tasa.TaxType = CalificacionOperacion.N1;
                    tasa.TaxException = CausaExencion.NA;
                }
                else if (iva.EsIntraComunitario)
                {
                    // Operación intracomunitaria: No sujeta por reglas de localización (N2)
                    tasa.Tax = Impuesto.IVA;
                    tasa.TaxType = CalificacionOperacion.N2;
                    tasa.TaxScheme = ClaveRegimen.RegimenGeneral;
                    tasa.TaxException = CausaExencion.E5;
                    tasa.TaxRate = (decimal)0.00;
                    tasa.TaxAmount = (decimal)0.00;
                }
                else if (iva.EsExtraComunitario)
                {
                    // Exportación a terceros países: No sujeta por reglas de localización (N2)
                    tasa.Tax = Impuesto.IVA;
                    tasa.TaxType = CalificacionOperacion.N2;
                    tasa.TaxScheme = ClaveRegimen.Exportacion;
                    tasa.TaxException = CausaExencion.E2;
                    tasa.TaxRate = (decimal)0.00;
                    tasa.TaxAmount = (decimal)0.00;
                }
                else // Régimen General (S1 o S2)
                {
                    tasa.Tax = Impuesto.IVA;
                    tasa.TaxScheme = ClaveRegimen.RegimenGeneral;
                    tasa.TaxType = iva.EsIsp ? CalificacionOperacion.S2 : CalificacionOperacion.S1;
                    tasa.TaxException = iva.EsExento && !iva.EsIsp ? CausaExencion.E1 : CausaExencion.NA;
                    tasa.TaxRate = Math.Round(iva.Porcentaje, 2);
                    tasa.TaxAmount = Math.Round(iva.Importe, 2);
                }

                tasa.TaxBase = Math.Round(iva.BI, 2);
                impuestos.Add(tasa);
            }

            return impuestos;
        }

        private static List<TaxItem> IncluirRetencionesFactura(this List<TaxItem> taxas, List<ImportePorTipoDeIrpf> retenciones)
        {
            foreach (var retencion in retenciones)
            {
                if (retencion.Importe > 0)
                {
                    TaxItem tasa = new TaxItem();
                    tasa.Tax = Impuesto.OTROS;
                    tasa.TaxType = CalificacionOperacion.N1;
                    tasa.TaxException = CausaExencion.NA;
                    tasa.TaxBase = -1 * Math.Round(retencion.Importe, 2);
                    taxas.Add(tasa);
                }
            }
            return taxas;
        }

    }
}



