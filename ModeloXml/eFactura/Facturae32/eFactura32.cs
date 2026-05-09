using ModeloXml.eFactura.Schemas;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;
using GestorDeElementos;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Elemento;
using Gestor.Errores;

namespace ModeloXml.eFactura.Facturae32
{

    public partial class eFactura32 : eFacturaBase<eFactura32>, IeFactura
    {
        public eFactura32()
        {
            var fileHeader = new FileHeaderType();
            fileHeader.SchemaVersion = SchemaVersionType.Item32;
            fileHeader.Modality = ModalityType.I;
            fileHeader.InvoiceIssuerType = InvoiceIssuerTypeType.EM;

            var batch = new BatchType();
            fileHeader.Batch = batch;

            batch.InvoicesCount = 0;
            batch.BatchIdentifier = string.Empty;
            batch.TotalInvoicesAmount = new AmountType() { TotalAmount = new DoubleTwoDecimalType() };
            batch.TotalOutstandingAmount = new AmountType() { TotalAmount = new DoubleTwoDecimalType() };
            batch.TotalExecutableAmount = new AmountType() { TotalAmount = new DoubleTwoDecimalType() };
            batch.InvoiceCurrencyCode = CurrencyCodeType.EUR;

            this.FileHeader = fileHeader;
            this.Parties = new PartiesType();
            this.Invoices = new InvoiceType[] { };
        }

        public object[] AdministrativeCentres(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var centros = new List<AdministrativeCentreType>();
            var centro = factura.CentroAdministrativo(contexto);
            var contacto = contexto.SeleccionarPorId<InterlocutorDtm>(centro.IdContacto).Contacto(contexto);
            centros.Add(new AdministrativeCentreType
            {
                CentreCode = centro.CodigoDir3,
                RoleTypeCode = centro.Rol == enumRolCentroAdministrativo.OrganoGestor
                               ? RoleTypeCodeType.Item01 :
                               centro.Rol == enumRolCentroAdministrativo.UnidadTramitadora
                               ? RoleTypeCodeType.Item02 :
                               centro.Rol == enumRolCentroAdministrativo.OficinaContable
                               ? RoleTypeCodeType.Item03
                               : RoleTypeCodeType.Item04,
                Name = centro.OrganoGestor,
                FirstSurname = centro.UnidadTramitadora.IsNullOrEmpty() ? "" : centro.UnidadTramitadora,
                SecondSurname = centro.OficinaContable.IsNullOrEmpty() ? "" : centro.OficinaContable,
                Item = Address(contexto, factura.DireccionFiscal(contexto)),
                ContactDetails = new ContactDetailsType
                {
                    ElectronicMail = contacto.eMail,
                    Telephone = contacto.Telefono,
                    ContactPersons = contacto.Nombre
                }
            });
            return centros.ToArray();
        }

        public object[] TaxesOutputs(ContextoSe contexto, FacturaEmtDtm factura)
        {
            throw new NotImplementedException("Implementada como TaxesOutputs32");
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
            invoiceLine.UnitPriceWithoutTax = new DoubleSixDecimalType { Value = Math.Round(Convert.ToDouble(linea.Precio), 6) };
            if (linea.ImporteDeDto > 0)
            {
                var disconts = new List<DiscountType>();
                var discont = new DiscountType();
                discont.DiscountAmount = new DoubleSixDecimalType { Value = Math.Round(Convert.ToDouble(linea.ImporteDeDto), 6) };
                discont.DiscountRate = new DoubleFourDecimalType { Value = Math.Round(Convert.ToDouble(linea.Descuento), 4) };
                disconts.Add(discont);
                invoiceLine.DiscountsAndRebates = disconts.ToArray();
            }

            invoiceLine.TotalCost = new DoubleSixDecimalType { Value = Math.Round(Convert.ToDouble(linea.ImporteConDto), 6) };
            invoiceLine.GrossAmount = new DoubleSixDecimalType { Value = Math.Round(Convert.ToDouble(linea.ImporteSinDto), 6) };

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

        private InvoiceLineTypeTax[] BloqueDeImpuestos(InvoiceLineType invoiceLine, LineaDeUnaFaeDtm linea)
        {

            var impuestos = new List<InvoiceLineTypeTax>();
            var impuesto = new InvoiceLineTypeTax
            {
                TaxTypeCode = TaxTypeCodeType.Item01,

                TaxRate = linea.IvaRepercutido.Exento
                ? new DoubleTwoDecimalType { Value = 0.00 }
                : new DoubleTwoDecimalType { Value = Math.Round(Convert.ToDouble(linea.IvaRepercutido.Porcentaje), 2) },

                TaxableBase = new AmountType { TotalAmount = new DoubleTwoDecimalType { Value = Math.Round(Convert.ToDouble(linea.ImporteConDto), 2) } },

                TaxAmount = new AmountType
                {
                    TotalAmount = linea.IvaRepercutido.Clase == ServicioDeDatos.Contabilidad.enumClasesDeIvaRep.ISP
                ? new DoubleTwoDecimalType { Value = 0.00 }
                : new DoubleTwoDecimalType { Value = Math.Round(Convert.ToDouble(linea.ImporteDeIva), 2) }
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
            var informacionAdicional = factura.Descripcion;
            foreach (var linea in lineas.Where(l => l.TipoDeLinea == Enumerados.enumTipoDeLinea.Comentario))
            {
                informacionAdicional = informacionAdicional + Environment.NewLine + linea.Concepto + Environment.NewLine + linea.Anotacion;
            }

            if (informacionAdicional.Length >= 2500)
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
                InstallmentAmount = new DoubleTwoDecimalType { Value = Math.Round(Convert.ToDouble(factura.APagar(contexto)), 2) },
                PaymentMeans = modoDePago == enumClaseDeCobro.Transferencia
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
                    BIC = micuenta.Banco(contexto).BicSwift
                };
            }

            if (modoDePago == enumClaseDeCobro.Remesa)
            {

            }

            plazos.Add(plazo);
            return plazos.ToArray();
        }

        //public object LegalEntitySeller(ContextoSe contexto, FacturaEmtDtm factura)
        //{
        //    var datosSociedad = (ParametrosDeMiSociedadDtm)factura.Sociedad(contexto).SeleccionarAmpliacion(contexto, typeof(ParametrosDeMiSociedadDtm));

        //    return new LegalEntityType
        //    {
        //        CorporateName = factura.Sociedad(contexto).RazonSocial,
        //        TradeName = factura.Sociedad(contexto).Nombre,
        //        RegistrationData = new RegistrationDataType
        //        {
        //            Book = datosSociedad.Libro.ToString(),
        //            RegisterOfCompaniesLocation = datosSociedad.LocalizadoEn,
        //            Sheet = datosSociedad.Hoja.ToString(), // "0",
        //            Folio = datosSociedad.Folio.ToString(), //"1",
        //            Section = datosSociedad.Seccion, //"3ª",
        //            Volume = datosSociedad.Volumen, //"0",
        //            AdditionalRegistrationData = datosSociedad.Adicional //"ref.3657,Inscip.1ª"
        //        },
        //        Item = Address(contexto, factura.Sociedad(contexto).DireccionFiscal(contexto))
        //    };
        //}

        public object LegalEntitySeller(ContextoSe contexto, FacturaEmtDtm factura)
        {
            try
            {
                // PASO 1: Obtener la sociedad. ¿Es nula?
                var sociedad = factura.Sociedad(contexto);
                if (sociedad == null)
                {
                    // Lanzamos un error claro para saber qué pasó.
                    throw new InvalidOperationException($"No se pudo encontrar la Sociedad para la factura ID: {factura.Id}");
                }

                // PASO 2: Intentar seleccionar la ampliación. ¿Devuelve null?
                var ampliacion = sociedad.SeleccionarAmpliacion(contexto, typeof(ParametrosDeMiSociedadDtm));
                if (ampliacion == null)
                {
                    // Este es el sospechoso más probable. La ampliación no existe en la BD.
                    throw new InvalidOperationException($"No se encontró la ampliación 'ParametrosDeMiSociedadDtm' para la Sociedad ID: {sociedad.Id}");
                }

                // PASO 3: Intentar hacer el casting. ¿Falla?
                var datosSociedad = (ParametrosDeMiSociedadDtm)ampliacion;
                if (datosSociedad == null)
                {
                    // Esto no debería ocurrir si ampliacion no era null, pero es una comprobación extra.
                    throw new InvalidOperationException("El casting de la ampliación a 'ParametrosDeMiSociedadDtm' resultó en null.");
                }

                // Si llegas hasta aquí, el resto de tu código debería ejecutarse.
                return new LegalEntityType
                {
                    CorporateName = sociedad.RazonSocial,
                    TradeName = sociedad.Nombre,
                    RegistrationData = new RegistrationDataType
                    {
                        Book = datosSociedad.Libro.ToString(),
                        RegisterOfCompaniesLocation = datosSociedad.LocalizadoEn,
                        Sheet = datosSociedad.Hoja.ToString(),
                        Folio = datosSociedad.Folio.ToString(),
                        Section = datosSociedad.Seccion,
                        Volume = datosSociedad.Volumen,
                        AdditionalRegistrationData = datosSociedad.Adicional
                    },
                    Item = Address(contexto, sociedad.DireccionFiscal(contexto))
                };
            }
            catch (Exception ex)
            {
                // Pon un punto de interrupción (breakpoint) aquí.
                // Cuando el programa se detenga, inspecciona la variable 'ex'.
                // Contendrá el tipo exacto de excepción y el mensaje de error.
                GestorDeErrores.Emitir($"Error crítico en LegalEntitySeller: {ex.Message}", ex);

                // Devolvemos null para simular el comportamiento que veías,
                // pero ahora sabrás por qué.
                return null;
            }
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
            return new IndividualType
            {
                Name = "",
                FirstSurname = "",
                SecondSurname = "",
                Item = new AddressType
                {
                    Address = "",
                    PostCode = "",
                    Town = "",
                    Province = "",
                    CountryCode = ApiDeEnsamblados.ToEnumerado<CountryType>("ESP")
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


        public eFactura32 AddLegalParty(bool seller, ResidenceTypeCodeType residenceTypeCode, string taxIdentificationNumber, string corporateName,
            string address, string postalCode, string province, string town, CountryType country)
        {
            var legal = new LegalEntityType();
            legal.CorporateName = corporateName;
            legal.Item = _CreateAddress(residenceTypeCode, address, postalCode, province, town, country);

            var party = _CreateParty(PersonTypeCodeType.J, residenceTypeCode, taxIdentificationNumber, legal);

            if (seller)
                this.Parties.SellerParty = party;
            else
                this.Parties.BuyerParty = party;

            return this;
        }


        private object _CreateAddress(ResidenceTypeCodeType residenceTypeCode, string address, string postalCode, string province, string town, CountryType country)
        {
            if (residenceTypeCode == ResidenceTypeCodeType.R)
            {
                var addressLine = new AddressType();
                addressLine.Address = address;
                addressLine.PostCode = postalCode;
                addressLine.Province = province;
                addressLine.Town = town;
                addressLine.CountryCode = CountryType.ESP;
                return addressLine;
            }
            else
            {
                var addressLine = new OverseasAddressType();
                addressLine.Address = address;
                addressLine.PostCodeAndTown = postalCode + " " + town;
                addressLine.Province = province;
                addressLine.CountryCode = country;
                return addressLine;
            }
        }

        private BusinessType _CreateParty(PersonTypeCodeType personTypeCodeType, ResidenceTypeCodeType residenceTypeCode, string taxIdentificationNumber, object item)
        {
            var party = new BusinessType();
            var taxIdentification = new TaxIdentificationType();
            taxIdentification.PersonTypeCode = personTypeCodeType;
            taxIdentification.ResidenceTypeCode = residenceTypeCode;
            taxIdentification.TaxIdentificationNumber = taxIdentificationNumber;
            party.TaxIdentification = taxIdentification;
            party.Item = item;
            return party;
        }


    }
}
