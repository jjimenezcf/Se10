using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Gastos;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ModeloXml.eFactura.Schemas;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System.Text.RegularExpressions;
using System.Xml;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloXml.eFactura.Facturae322
{

    public partial class eFactura322 : eFacturaBase<eFactura322>, IeFactura
    {
        public eFactura322()
        {
            var fileHeader = new FileHeaderType
            {

                SchemaVersion = SchemaVersionType.Item322,
                Modality = ModalityType.I,
                InvoiceIssuerType = InvoiceIssuerTypeType.EM
            };

            var batch = new BatchType();
            fileHeader.Batch = batch;

            batch.InvoicesCount = 0;
            batch.BatchIdentifier = string.Empty;
            batch.TotalInvoicesAmount = new AmountType() { TotalAmount = new DoubleTwoDecimalType().Value };
            batch.TotalOutstandingAmount = new AmountType() { TotalAmount = new DoubleTwoDecimalType().Value };
            batch.TotalExecutableAmount = new AmountType() { TotalAmount = new DoubleTwoDecimalType().Value };
            batch.InvoiceCurrencyCode = CurrencyCodeType.EUR;

            FileHeader = fileHeader;
            Parties = new PartiesType();
            Invoices = new InvoiceType[] { };
        }

        public object[] AdministrativeCentres(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var centros = new List<AdministrativeCentreType>();
            var centro = factura.CentroAdministrativo(contexto);
            var contacto = contexto.SeleccionarPorId<InterlocutorDtm>(centro.IdContacto).Contacto(contexto);
            var centroAdm = new AdministrativeCentreType
            {
                CentreCode = centro.CodigoDir3,
                RoleTypeCode = centro.Rol == enumRolCentroAdministrativo.OficinaContable
                               ? RoleTypeCodeType.Item01 :
                               centro.Rol == enumRolCentroAdministrativo.UnidadTramitadora
                               ? RoleTypeCodeType.Item02 :
                               centro.Rol == enumRolCentroAdministrativo.OrganoGestor
                               ? RoleTypeCodeType.Item03
                               : RoleTypeCodeType.Item04,
                RoleTypeCodeSpecified = true,
                Name = centro.Alias,
                Item = Address(contexto, factura.DireccionFiscal(contexto)),
                ContactDetails = new ContactDetailsType
                {
                    ElectronicMail = contacto.eMail,
                    Telephone = contacto.Telefono,
                    ContactPersons = contacto.Nombre
                }
            };
            //if (!centro.UnidadTramitadora.IsNullOrEmpty()) centroAdm.FirstSurname = centro.UnidadTramitadora;
            //if (!centro.OficinaContable.IsNullOrEmpty()) centroAdm.SecondSurname = centro.OficinaContable;
            //if (!centro.Alias.IsNullOrEmpty()) centroAdm.CentreDescription = centro.Alias;
            centros.Add(centroAdm);
            return centros.ToArray();
        }

        public object[] TaxesOutputs(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var ivas = factura.Ivas(contexto);
            var impuestos = new List<TaxOutputType>();

            foreach (var iva in ivas.Where(i => i.Porcentaje > 0))
            {
                var impuesto = new TaxOutputType { TaxTypeCode = TaxTypeCodeType.Item01 };
                impuesto.TaxRate = Math.Round(Convert.ToDouble(iva.Porcentaje), 2);
                impuesto.TaxableBase = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(iva.BI), 2) };
                impuesto.TaxAmount = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(iva.Importe), 2) };
                impuestos.Add(impuesto);
            }

            foreach (var iva in ivas.Where(i => i.Porcentaje == 0))
            {
                var exencion = new TaxOutputType { TaxTypeCode = TaxTypeCodeType.Item05 };
                exencion.TaxRate = Math.Round(Convert.ToDouble(iva.Porcentaje), 2);
                exencion.TaxableBase = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(iva.BI), 2) };
                exencion.TaxAmount = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(iva.Importe), 2) };
                impuestos.Add(exencion);
            }

            return impuestos.ToArray();
        }

        public TaxType[] Retenciones(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var irpfs = factura.Irpfs(contexto);
            var impuestos = new List<TaxType>();

            foreach (var irpf in irpfs.Where(i => i.Porcentaje > 0))
            {
                var impuesto = new TaxType { TaxTypeCode = TaxTypeCodeType.Item04 };
                impuesto.TaxRate = Math.Round(Convert.ToDouble(irpf.Porcentaje), 2);
                impuesto.TaxableBase = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(irpf.BI), 2) };
                impuesto.TaxAmount = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(irpf.Importe), 2) };
                impuestos.Add(impuesto);
            }

            return impuestos.ToArray();
        }
        public object InvoiceLine(ContextoSe contexto, FacturaEmtDtm factura, LineaDeUnaFaeDtm linea)
        {
            var invoiceLine = new InvoiceLineType();
            invoiceLine.ItemDescription = linea.Concepto;
            invoiceLine.SequenceNumber = linea.Orden;
            invoiceLine.ReceiverContractReference = factura.Contrato(contexto)?.Referencia ?? factura.Presupuesto(contexto)?.Referencia ?? "";
            invoiceLine.ReceiverTransactionReference = linea.ParteTr(contexto)?.Referencia ?? factura.ParteTr(contexto)?.Referencia ?? factura.Contrato(contexto)?.Referencia ?? factura.Presupuesto(contexto)?.Referencia ?? "";

            invoiceLine.Quantity = Math.Round(Convert.ToDouble(linea.Cantidad), 2);
            invoiceLine.UnitOfMeasure = enumUnidadDeMedidaToXml.ObtenerDesdeSigla(linea.Unidad.Sigla);
            invoiceLine.UnitPriceWithoutTax = Math.Round(Convert.ToDouble(linea.Precio), 2);
            if (linea.ImporteDeDto > 0)
            {
                var disconts = new List<DiscountType>();
                var discont = new DiscountType();
                discont.DiscountAmount = Math.Round(Convert.ToDouble(linea.ImporteDeDto), 2);
                discont.DiscountRate = Math.Round(Convert.ToDouble(linea.Descuento), 2);
                disconts.Add(discont);
                invoiceLine.DiscountsAndRebates = disconts.ToArray();
            }
            invoiceLine.TotalCost = Math.Round(Convert.ToDouble(linea.ImporteSinDto), 2);
            invoiceLine.GrossAmount = Math.Round(Convert.ToDouble(linea.ImporteConDto), 2);


            if (linea.IdIvaR.Entero() > 0 && linea.IvaRepercutido.Clase != ServicioDeDatos.Contabilidad.enumClasesDeIvaRep.NSJ)
            {
                invoiceLine.TaxesOutputs = BloqueDeImpuestos(invoiceLine, linea);
            }

            if (linea.IvaRepercutido != null && linea.IvaRepercutido.Exento)
                invoiceLine.AdditionalLineItemInformation = linea.IvaRepercutido.DescripcionFiscal;

            if (!linea.Anotacion.IsNullOrEmpty())
                invoiceLine.AdditionalLineItemInformation = !invoiceLine.AdditionalLineItemInformation.IsNullOrEmpty()
                ? invoiceLine.AdditionalLineItemInformation + Environment.NewLine + linea.Anotacion
                : linea.Anotacion;

            if (invoiceLine.AdditionalLineItemInformation != null && invoiceLine.AdditionalLineItemInformation.Length >= 2500)
                GestorDeErrores.Emitir($"La información adicional para la línea '{linea.Concepto}' no puede exceder de 2500 caracteres, y es de {invoiceLine.AdditionalLineItemInformation.Length}");

            return invoiceLine;
        }

        private static InvoiceLineTypeTax[] BloqueDeImpuestos(InvoiceLineType invoiceLine, LineaDeUnaFaeDtm linea)
        {
            var impuestos = new List<InvoiceLineTypeTax>();
            var impuesto = new InvoiceLineTypeTax
            {
                TaxTypeCode = linea.IvaRepercutido.Exento ? TaxTypeCodeType.Item05 : TaxTypeCodeType.Item01,
                TaxRate = Math.Round(Convert.ToDouble(linea.IvaRepercutido.Porcentaje), 2),
                TaxableBase = new AmountType { TotalAmount = Math.Round(Convert.ToDouble(linea.ImporteConDto), 2) },

                TaxAmount = new AmountType
                {
                    TotalAmount = linea.IvaRepercutido.Clase == ServicioDeDatos.Contabilidad.enumClasesDeIvaRep.ISP
                ? 0.00d
                : Math.Round(Convert.ToDouble(linea.ImporteDeIva), 2)
                }
            };

            if (linea.IvaRepercutido.Clase == ServicioDeDatos.Contabilidad.enumClasesDeIvaRep.ISP)
            {
                invoiceLine.SpecialTaxableEvent = new SpecialTaxableEventType
                {
                    SpecialTaxableEventCode = SpecialTaxableEventCodeType.Item01, // ISP
                    SpecialTaxableEventReason = linea.IvaRepercutido.DescripcionFiscal
                };
            }

            impuestos.Add(impuesto);
            return impuestos.ToArray();
        }

        public string InvoiceAdditionalInformation(FacturaEmtDtm factura, List<LineaDeUnaFaeDtm> lineas)
        {
            var informacionAdicional = factura.Nombre + Environment.NewLine + factura.Descripcion;
            foreach (var linea in lineas.Where(l => l.TipoDeLinea == Enumerados.enumTipoDeLinea.Comentario))
            {
                informacionAdicional = informacionAdicional + Environment.NewLine + linea.Concepto + Environment.NewLine + linea.Anotacion;
            }

            if (!informacionAdicional.IsNullOrEmpty() && informacionAdicional.Length >= 2500)
                GestorDeErrores.Emitir($"La información adicional para la factura '{factura.NumeroDeFactura}' no puede exceder de 2500 caracteres, y es de {informacionAdicional.Length}");

            return informacionAdicional;
        }

        public object PaymentDetails(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var plazos = new List<InstallmentType>();

            var cc = factura.Sociedad(contexto).Detalles<CuentaDeMiSociedadDtm>(contexto).Where(x => x.Activa == true).ToList();
            var modoDePago = cc.Count == 1 ? enumClaseDeCobro.Transferencia : enumClaseDeCobro.Contado;

            var plazo = new InstallmentType
            {
                InstallmentDueDate = factura.VenceEl.Fecha(),
                InstallmentAmount = factura.APagar(contexto).Formatear(alineacion: false, separadorDecimal: "."),
                PaymentMeans = modoDePago  == enumClaseDeCobro.Transferencia 
                ? PaymentMeansType.Item04
                : modoDePago == enumClaseDeCobro.Contado
                ? PaymentMeansType.Item01
                : modoDePago == enumClaseDeCobro.Remesa
                ? PaymentMeansType.Item12
                : PaymentMeansType.Item13,
            };

            if (modoDePago == enumClaseDeCobro.Transferencia)
            {
                var micuenta = cc[0].Cuenta(contexto);
                plazo.AccountToBeCredited = new AccountType
                {
                    Item = micuenta.NumeroIban,
                    ItemElementName = ItemChoiceType.IBAN,
                    BankCode = micuenta.Entidad,
                    BranchCode = micuenta.Oficina,
                    BIC = micuenta.Banco(contexto).BicSwift.PadRight(11, 'X').Substring(0, 11),
                };
            }

            if (modoDePago == enumClaseDeCobro.Remesa)
            {

            }

            plazos.Add(plazo);
            return plazos.ToArray();
        }

        public object LegalEntitySeller(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var datosSociedad = (ParametrosDeMiSociedadDtm)factura.Sociedad(contexto).SeleccionarAmpliacion(contexto, typeof(ParametrosDeMiSociedadDtm));

            return new LegalEntityType
            {
                CorporateName = factura.Sociedad(contexto).RazonSocial,
                TradeName = factura.Sociedad(contexto).Nombre,
                RegistrationData = new RegistrationDataType
                {
                    Book = datosSociedad.Libro.ToString(),
                    RegisterOfCompaniesLocation = datosSociedad.LocalizadoEn,
                    Sheet = datosSociedad.Hoja.ToString(), // "0",
                    Folio = datosSociedad.Folio.ToString(), //"1",
                    Section = datosSociedad.Seccion, //"3ª",
                    Volume = datosSociedad.Volumen, //"0",
                    AdditionalRegistrationData = datosSociedad.Adicional //"ref.3657,Inscip.1ª"
                },
                Item = Address(contexto, factura.Sociedad(contexto).DireccionFiscal(contexto))
            };
        }

        public object LegalEntityBuyer(ContextoSe contexto, FacturaEmtDtm factura)
        {
            return new LegalEntityType
            {
                CorporateName = factura.Cliente(contexto).RazonSocial(contexto),
                Item = Address(contexto, factura.DireccionFiscal(contexto))
            };
        }

        public object IndividualSellerParty(ContextoSe contexto, SociedadDtm emisor)
        {
            var autonomo = ApiDeTerceros.InferirNombreConApellidos(emisor.RazonSocial);
            var fiscal = emisor.DireccionFiscal(contexto);
            return new IndividualType
            {
                Name = autonomo.Nombre,
                FirstSurname = autonomo.Ape1,
                SecondSurname = autonomo.Ape2,
                Item = new AddressType
                {
                    Address = fiscal.NombreDireccion,
                    PostCode = fiscal.CodigoPostal,
                    Town = fiscal.Municipio,
                    Province = fiscal.Provincia,
                    CountryCode = ApiDeEnsamblados.ToEnumerado<CountryType>("ESP")
                },
                ContactDetails = new ContactDetailsType
                {
                    ElectronicMail = emisor.eMail,
                    Telephone = emisor.Telefono,
                }
            };
        }

        public object IndividualBuyer(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var persona = ApiDeTerceros.InferirNombreConApellidos(factura.Contacto);

            return new IndividualType
            {
                Name = persona.Nombre,
                FirstSurname = persona.Ape1,
                SecondSurname = persona.Ape2,
                Item = Address(contexto, factura.DireccionFiscal(contexto))
            };
        }

        public object Address(ContextoSe contexto, DireccionDto direccion)
        {
            string codigo = contexto.SeleccionarPorId<PaisDtm>(direccion.IdPais).Codigo;
            return new AddressType
            {
                Address = direccion.NombreDireccion,
                PostCode = direccion.CodigoPostal,
                Town = direccion.Municipio,
                Province = direccion.Provincia,
                CountryCode = ApiDeEnsamblados.ToEnumerado<CountryType>(codigo)
            };
        }


        private ContextoSe _contexto;
        private string _fichero;
        private int _idCg;
        private int _idTipo;
        private int _idProveedor;
        private int _idArchivo;

        public eFactura322(ContextoSe contexto, string fichero, int idCg, int idTipo, int idProveedor, int idArchivo)
        {
            _contexto = contexto;
            _fichero = fichero;
            _idCg = idCg;
            _idTipo = idTipo;
            _idProveedor = idProveedor;
            _idArchivo = idArchivo;
        }

        /// <summary>
        /// Parsea un fichero eFactura 3.2/3.2.1/3.2.2 y devuelve el JSON de <see cref="FacturaJson"/>
        /// sin persistir nada en base de datos.
        /// </summary>
        public static FacturaJson ParsearJson(string fichero)
        {
            Parsear(fichero);
            var facturae = new eFactura322().Validate(fichero);
            var inv = facturae.Invoices[0];

            var factura = new FacturaJson();

            // ── Cabecera ──────────────────────────────────────────────────────
            factura.NumeroFactura    = inv.InvoiceHeader.InvoiceNumber;
            factura.Fecha            = inv.InvoiceIssueData.IssueDate.ToString("yyyy-MM-dd");
            factura.Concepto         = inv.InvoiceIssueData.InvoiceDescription;

            if (inv.InvoiceHeader.InvoiceClass == InvoiceClassType.OR)
                factura.ClaseRectificativa = "OR";
            else if (inv.InvoiceHeader.InvoiceClass == InvoiceClassType.OC)
                factura.ClaseRectificativa = "OC";
            factura.FechaVencimiento = inv.PaymentDetails?.Length > 0
                ? inv.PaymentDetails[0].InstallmentDueDate.ToString("yyyy-MM-dd")
                : null;

            factura.Nif = facturae.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber;

            // BusinessType.Item es LegalEntityType o IndividualType según el atributo XmlElement discriminado
            AddressType dirProveedor = null;
            if (facturae.Parties.SellerParty.Item is LegalEntityType legalEntity)
            {
                factura.Proveedor = legalEntity.CorporateName;
                dirProveedor = legalEntity.Item as AddressType;
            }
            else if (facturae.Parties.SellerParty.Item is IndividualType individuo)
            {
                factura.Proveedor = $"{individuo.Name} {individuo.FirstSurname} {individuo.SecondSurname}".Trim();
                factura.eMail     = individuo.ContactDetails?.ElectronicMail;
                factura.Telefono  = individuo.ContactDetails?.Telephone;
                dirProveedor      = individuo.Item as AddressType;
            }
            if (dirProveedor != null)
            {
                factura.Calle        = dirProveedor.Address;
                factura.CodigoPostal = dirProveedor.PostCode;
                factura.Municipio    = dirProveedor.Town;
                factura.Provincia    = dirProveedor.Province;
                factura.Pais         = dirProveedor.CountryCode.ToString();
            }

            // ── Importes ──────────────────────────────────────────────────────
            factura.BaseImponible = Convert.ToDecimal(inv.InvoiceTotals.TotalGrossAmountBeforeTaxes);
            factura.TotalIva      = Convert.ToDecimal(inv.InvoiceTotals.TotalTaxOutputs);
            factura.TotalIrpf     = Convert.ToDecimal(inv.InvoiceTotals.TotalTaxesWithheld);
            factura.Total         = Convert.ToDecimal(inv.InvoiceTotals.InvoiceTotal);

            // ── Pago ─────────────────────────────────────────────────────────
            if (inv.PaymentDetails?.Length > 0)
            {
                var medio = inv.PaymentDetails[0].PaymentMeans;
                factura.ClaseDePago = medio == PaymentMeansType.Item04 ? enumClaseDePago.Transferencia.Descripcion()
                    : medio == PaymentMeansType.Item12 ? enumClaseDePago.Remesa.Descripcion()
                    : enumClaseDePago.Contado.Descripcion();

                if (medio == PaymentMeansType.Item04 && inv.PaymentDetails[0].AccountToBeCredited?.Item != null)
                    factura.CuentaBancaria = inv.PaymentDetails[0].AccountToBeCredited.Item.ToString();
            }

            // ── Líneas ────────────────────────────────────────────────────────
            var lineas = new List<LineaFacturaJson>();
            IrpfFacturaJson irpfObj = null;

            foreach (var item in inv.Items)
            {
                var impuestos = item.TaxesOutputs ?? Array.Empty<InvoiceLineTypeTax>();

                // Línea con retención IRPF: TaxTypeCode Item03 o Item04
                if (impuestos.Length > 0 &&
                    (impuestos[0].TaxTypeCode == TaxTypeCodeType.Item03 ||
                     impuestos[0].TaxTypeCode == TaxTypeCodeType.Item04))
                {
                    irpfObj = new IrpfFacturaJson
                    {
                        PorcentajeRetencion = Convert.ToDecimal(impuestos[0].TaxRate),
                        BaseRetencion       = impuestos[0].TaxableBase != null
                            ? Convert.ToDecimal(impuestos[0].TaxableBase.TotalAmount)
                            : factura.BaseImponible,
                        ImporteRetencion = factura.TotalIrpf
                    };
                }
                else
                {
                    var exenta = impuestos.Length > 0 && impuestos[0].TaxTypeCode == TaxTypeCodeType.Item05;
                    lineas.Add(new LineaFacturaJson
                    {
                        Concepto      = item.ItemDescription ?? string.Empty,
                        BaseImponible = Convert.ToDecimal(item.GrossAmount),
                        PorcentajeIva = !exenta && impuestos.Length > 0 ? Convert.ToDecimal(impuestos[0].TaxRate) : 0m,
                        ImporteIva    = !exenta && impuestos.Length > 0 && impuestos[0].TaxAmount != null
                            ? Convert.ToDecimal(impuestos[0].TaxAmount.TotalAmount)
                            : 0m,
                        Exenta = exenta
                    });
                }
            }

            // IRPF desde TaxesWithheld de cabecera si no se detectó en líneas
            if (factura.TotalIrpf > 0 && irpfObj == null)
            {
                var retencion = inv.TaxesWithheld?.FirstOrDefault();
                irpfObj = new IrpfFacturaJson
                {
                    PorcentajeRetencion = retencion != null ? Convert.ToDecimal(retencion.TaxRate) : 0m,
                    BaseRetencion       = retencion?.TaxableBase != null
                        ? Convert.ToDecimal(retencion.TaxableBase.TotalAmount)
                        : factura.BaseImponible,
                    ImporteRetencion = factura.TotalIrpf
                };
            }

            factura.Lineas = lineas;
            factura.Irpf   = factura.TotalIrpf > 0 ? irpfObj : null;

            return factura;
        }

        public static void Parsear(string fichero)
        {
            string newContent = "<n:Facturae xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:n=\"http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
            string newEndTag = @"/n:Facturae";
            // Leer el contenido del archivo
            string content = File.ReadAllText(fichero);

            // Encontrar el segundo '<'
            int secondOpenBracketIndex = content.IndexOf('<', content.IndexOf('<') + 1);

            // Encontrar el siguiente '>'
            int closingBracketIndex = content.IndexOf('>', secondOpenBracketIndex);

            // Reemplazar el contenido entre '<' y '>'
            string updatedContent = content.Substring(0, secondOpenBracketIndex) + newContent + content.Substring(closingBracketIndex + 1);
            updatedContent = Regex.Replace(updatedContent, @"<[^<>]+>$", $"<{newEndTag}>");
            //updatedContent = Regex.Replace(updatedContent, @"</m:Facturae>", newEndTag);

            using (StreamWriter writer = new StreamWriter(fichero))
            {
                writer.Write(updatedContent);
            }

            var doc = new XmlDocument();
            doc.Load(fichero);

            // Encontrar el nodo SchemaVersion y actualizar su valor
            var schemaVersionNode = doc.SelectSingleNode("//SchemaVersion");
            if (schemaVersionNode != null)
            {
                schemaVersionNode.InnerText = "3.2.2";

                // Guardar los cambios en el archivo XML
                doc.Save(fichero);
            }
        }
    }


}

