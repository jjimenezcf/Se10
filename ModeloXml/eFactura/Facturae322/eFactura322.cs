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

        public FacturaRecDto Importar()
        {
            Parsear(_fichero);
            var facturae = new eFactura322().Validate(_fichero);

            var proveedor = _contexto.SeleccionarPorPropiedad<ProveedorDtm>(nameof(ltrProveedor.NIF), facturae.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber);
            if (_idProveedor != proveedor.Id)
                GestorDeErrores.Emitir($"El proveedor de la factura '{proveedor.Expresion}' no se corresponde con el indicado");

            var recibida = new FacturaRecDtm();
            recibida.IdTipo = _idTipo;
            recibida.IdCg = _idCg;
            recibida.Numero = facturae.Invoices[0].InvoiceHeader.InvoiceNumber;

            if (facturae.Invoices[0].InvoiceHeader.InvoiceClass == InvoiceClassType.OR)
                recibida.ClaseRectificativa = enumClaseDeRectificativa.OR;
            else if (facturae.Invoices[0].InvoiceHeader.InvoiceClass == InvoiceClassType.OC)
                recibida.ClaseRectificativa = enumClaseDeRectificativa.OC;
            else if (facturae.Invoices[0].InvoiceHeader.InvoiceClass != InvoiceClassType.OO)
                GestorDeErrores.Emitir($"No se ha implementado cómo incorporar una factura de la clase '{facturae.Invoices[0].InvoiceHeader.InvoiceClass}'");

            recibida.Nombre = facturae.Invoices[0].InvoiceIssueData.InvoiceDescription == null ? "(No detallado)" : facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.Left(250);
            recibida.Descripcion = facturae.Invoices[0].AdditionalData == null ? "(No detallado)" : facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.Left(2000);
            recibida.IdProveedor = proveedor.Id;
            recibida.RecibidaEl = DateTime.Now.Date;
            recibida.FacturadaEl = facturae.Invoices[0].InvoiceIssueData.IssueDate;
            recibida.VenceEl = facturae.Invoices[0].PaymentDetails == null ? facturae.Invoices[0].InvoiceIssueData.IssueDate : facturae.Invoices[0].PaymentDetails[0].InstallmentDueDate;
            recibida.BaseImponible = facturae.Invoices[0].InvoiceTotals.TotalGrossAmountBeforeTaxes.ToString().Decimal();
            recibida.TotalDelPago = facturae.Invoices[0].InvoiceTotals.InvoiceTotal.ToString().Decimal();

            recibida.IdArchivo = _idArchivo;
            recibida.Insertar(_contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_IncorporarFacturaXml);

            if (facturae.Invoices[0].InvoiceIssueData.InvoiceDescription != null && facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.ToString().Length > 250)
                recibida.CrearObservacion(_contexto, "Nombre completo en el Xml", facturae.Invoices[0].InvoiceIssueData.InvoiceDescription);

            if (facturae.Invoices[0].AdditionalData != null && facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.ToString().Length > 2000)
                recibida.CrearObservacion(_contexto, "Descripción completa en el Xml", facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.Right(1000));

            recibida.CrearTraza(_contexto, ltrDeUnaFacturaRec.TrazaDeIncorporacion, $"El usuario {_contexto.DatosDeConexion.Login} ha incorporado la factura por importe de {recibida.BaseImponible.Moneda()}");

            string imputadoA = null;
            string informacioDeLineas = null;
            var linea = new LineaDeUnaFarDtm();
            var asignarElOrden = 10;
            linea.IdElemento = recibida.Id;
            foreach (var item in facturae.Invoices[0].Items)
            {
                linea.Orden = Convert.ToInt32(item.SequenceNumber);
                if (linea.Orden == 0)
                {
                    linea.Orden = asignarElOrden;
                    asignarElOrden = asignarElOrden + 10;
                }

                var concepto = item.ItemDescription.QuitarSubcadenaInicial(Environment.NewLine);
                concepto = concepto.QuitarSubcadenaInicial("\n");
                concepto = concepto.QuitarSubcadenaInicial("\t");
                concepto = concepto.Trim();
                concepto = concepto.QuitarDobleIntro()
                    .RemplazarCaracteres(Environment.NewLine + "\t", Environment.NewLine)
                    .RemplazarCaracteres(Environment.NewLine + " ", Environment.NewLine);
                if (item.Quantity > 0 && item.UnitPriceWithoutTax > 0)
                    concepto = concepto + $" (Cta:{item.Quantity}, Pu:{item.UnitPriceWithoutTax})";

                if (concepto.Length > 250)
                {
                    linea.Concepto = "Descripción anotada como observación de la factura";
                    recibida.CrearObservacion(_contexto, $"Concepto de la línea: {linea.Orden}", concepto);
                }
                else
                    linea.Concepto = concepto;

                if (!item.ReceiverContractReference.IsNullOrEmpty() && (imputadoA == null || !imputadoA.Contains(item.ReceiverContractReference)))
                    imputadoA = $"{(imputadoA.IsNullOrEmpty() ? "" : imputadoA + ", ")} {item.ReceiverContractReference}";

                if (!item.ReceiverTransactionReference.IsNullOrEmpty() && (imputadoA == null || !imputadoA.Contains(item.ReceiverTransactionReference)))
                    imputadoA = $"{(imputadoA.IsNullOrEmpty() ? "" : imputadoA + ", ")} {item.ReceiverTransactionReference}";

                linea.BaseImponible = Convert.ToDecimal(item.GrossAmount);

                var impuestos = item.TaxesOutputs;
                if (impuestos.Length > 1)
                    GestorDeErrores.Emitir($"No se puede importar la factura, por tener la línea '{linea.Orden}' más de un impuesto definido");

                if (impuestos.Length == 0)
                {
                    linea.Clase = enumClaseDeLineaFar.BaseImponible;
                }
                else
                {
                    if (linea.BaseImponible > 0)
                    {
                        var impuesto = impuestos[0];
                        linea.Clase = impuesto.TaxTypeCode == TaxTypeCodeType.Item05 ? enumClaseDeLineaFar.BiExenta : enumClaseDeLineaFar.BiConIva;
                        linea.PorcentajeIva = impuesto.TaxTypeCode == TaxTypeCodeType.Item05 ? 0 : Convert.ToDecimal(impuesto.TaxRate);
                        if (impuesto.TaxTypeCode == TaxTypeCodeType.Item05)
                            recibida.CrearObservacion(_contexto, "Línea exenta", $"la línea '{linea.Orden}' está exenta de IVA");
                    }
                    if (linea.BaseImponible == 0)
                    {
                        var impuesto = impuestos[0];
                        if (impuesto.TaxTypeCode == TaxTypeCodeType.Item03)
                        {
                            linea.Clase = enumClaseDeLineaFar.LineaDeIrpf;
                            linea.PorcentajeIrpf = Convert.ToDecimal(impuesto.TaxRate);
                        }
                        else
                        {
                            linea.Clase = Convert.ToDecimal(impuesto.TaxRate) == 0 ? enumClaseDeLineaFar.BaseImponible : enumClaseDeLineaFar.LineaDeIva;
                            linea.PorcentajeIva = Convert.ToDecimal(impuesto.TaxRate);
                        }
                    }
                }

                if (linea.PorcentajeIrpf > 0)
                    linea.IdIrpf = _contexto.SeleccionarPorPropiedad<IrpfDtm>(nameof(IrpfDtm.Porcentaje), linea.PorcentajeIrpf, errorSiMasDeuno: false).Id;

                if (linea.PorcentajeIva > 0)
                    linea.IdIvaS = _contexto.SeleccionarPorPropiedad<IvaSoportadoDtm>(nameof(IvaSoportadoDtm.Porcentaje), linea.PorcentajeIva, errorSiMasDeuno: false).Id;

                informacioDeLineas = $"{(informacioDeLineas.IsNullOrEmpty() ? "" : informacioDeLineas + Environment.NewLine)}{item.AdditionalLineItemInformation}";
                linea.Id = 0;
                linea.Insertar(_contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_IncorporarFacturaXml);
            }

            if (!imputadoA.IsNullOrEmpty())
                recibida.CrearObservacion(_contexto, "Anotaciones en las líneas", imputadoA);

            return _contexto.SeleccionarDto<FacturaRecDto>(recibida.Id);
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

