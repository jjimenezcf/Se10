/*
 Urls de documentación
 https://www.billin.net/blog/facturar-cliente-extranjero/ (tienes ROI)
 https://help.holded.com/es/articles/7263115-facturae-preguntas-frecuentes
 https://economia.gencat.cat/web/.content/factura-electronica/emetre/generador-EMIX/emix-manual-usuari-es.pdf
 https://web.araba.eus/documents/105044/5608600/fakturARABA-Manual+de+la+Aplicaci%C3%B3n+v1.6+%281%29.pdf/fffb6b34-5ecd-061f-96b5-011a99074859?t=1644391900697

 https://www.facturae.gob.es/formato/Documents/Gesti%C3%B3n%20de%20Facturaci%C3%B3n%20Electr%C3%B3nica%203.1/Guia-Usuario-Facturaev3-1.pdf
 https://www.b2brouter.net/es/facturas-electronicas-b2brouter/
*/


/*

 Cuando una operación está "con inversión del sujeto pasivo", se debe indicar en la factura que el IVA correspondiente a esa operación debe ser autoliquidado 
 por el cliente en lugar de ser repercutido por el proveedor. En resumen, al indicar que una operación está "con inversión del sujeto pasivo" en la factura electrónica, 
 se debe incluir un comentario que refleje esta condición para cumplir con los requisitos fiscales

 */

using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Xsl;
using ModeloXml.eFactura.Properties;
using ModeloXml.eFactura.Validation;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using static System.Text.Encoding;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Terceros;

namespace ModeloXml.eFactura
{
    public interface IeFactura
    {
        public object LegalEntitySeller(ContextoSe contexto, FacturaEmtDtm factura);
        public object IndividualSellerParty(ContextoSe contexto, SociedadDtm emisor);
        public object Address(ContextoSe contexto, DireccionDto direccion);
        object IndividualBuyer(ContextoSe contexto, FacturaEmtDtm factura);
        object LegalEntityBuyer(ContextoSe contexto, FacturaEmtDtm factura);
        object[] TaxesOutputs(ContextoSe contexto, FacturaEmtDtm factura);
        object[] AdministrativeCentres(ContextoSe contexto, FacturaEmtDtm factura);
        object InvoiceLine(ContextoSe contexto, FacturaEmtDtm factura, LineaDeUnaFaeDtm linea);
        string InvoiceAdditionalInformation(FacturaEmtDtm factura, List<LineaDeUnaFaeDtm> lineas);
        object PaymentDetails(ContextoSe contexto, FacturaEmtDtm factura);
    }


    [Serializable]
    public abstract class eFacturaBase<T> where T : class
    {
        private static XmlSchemaSet? _schemaSet;

        public eFacturaBase()
        {
            var _schemas = new Dictionary<string, byte[]>
            {
                {
                    "http://www.facturae.es/Facturae/2007/v3.1/Facturae",
                    UTF8.GetBytes(Resource.Facturaev3_1)
                },
                {
                    "http://www.facturae.es/Facturae/2009/v3.2/Facturae",
                    UTF8.GetBytes(Resource.Facturaev3_2)
                },
                {
                    "http://www.facturae.es/Facturae/2014/v3.2.1/Facturae",
                    UTF8.GetBytes(Resource.Facturaev3_2_1)
                },
                {
                    "http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml",
                    UTF8.GetBytes(Resource.Facturaev3_2_2)
                },
                {
                    "xmldsig-core-schema.xsd",
                    UTF8.GetBytes(Resource.xmldsig_core_schema)
                }
            };

            _schemaSet = XmlValidator.CreateXmlSchemaSet(_Namespace(), _schemas);
        }

        private static string _Namespace()
        {
            if (typeof(T).IsAssignableFrom(typeof(Facturae31.Facturae)))
                return "http://www.facturae.es/Facturae/2007/v3.1/Facturae";
            if (typeof(T).IsAssignableFrom(typeof(Facturae32.eFactura32)))
                return "http://www.facturae.es/Facturae/2009/v3.2/Facturae";
            if (typeof(T).IsAssignableFrom(typeof(Facturae321.Facturae)))
                return "http://www.facturae.es/Facturae/2014/v3.2.1/Facturae";
            if (typeof(T).IsAssignableFrom(typeof(Facturae322.eFactura322)))
                return "http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml";

            throw new Exception($"no se ha definido el esquema para '{typeof(T)}'");
        }


        public XmlReader ToXmlReader()
        {
            // Crea el atributo XmlRoot con el nombre deseado para el nodo raíz
            XmlRootAttribute root = new XmlRootAttribute("Facturae");
            root.Namespace = _Namespace();

            // Crea una instancia de XmlSerializer pasando el tipo y el atributo XmlRoot
            var xmlSerializer = new XmlSerializer(typeof(T), root);
            //var xmlSerializer = new XmlSerializer(typeof(T), _Namespace());
            var ms = new MemoryStream();
            xmlSerializer.Serialize(ms, this);
            ms.Seek(0, 0);

            return XmlReader.Create(ms);
        }

        public XmlDocument ToXmlDocument()
        {
            var doc = new XmlDocument();
            doc.Load(ToXmlReader());
            return doc;
        }

        public void ToFile(string rutaConFichero)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            var doc = ToXmlDocument();

            var ruta = Path.GetDirectoryName(rutaConFichero);
            if (ruta != null && Directory.Exists(ruta) == false)
            {
                Directory.CreateDirectory(ruta);
            }

            //XmlElement newRoot = doc.CreateElement("fe", "Facturae", _Namespace());
            //newRoot.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            //newRoot.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");

            //if (doc.DocumentElement is null)
            //    throw new Exception($"El documento proporcionado no define elementos");

            //// Mueve todos los hijos del elemento raíz original al nuevo elemento
            //foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            //{
            //    newRoot.AppendChild(node.CloneNode(true));
            //}

            //// Reemplaza el elemento raíz original con el nuevo elemento
            //doc.RemoveAll();
            //doc.AppendChild(newRoot);

            // https://face.gob.es/es/facturas/validar-visualizar-facturas
            if (_schemaSet is not null)
                XmlValidator.Validate(UTF8.GetBytes(doc.OuterXml), _schemaSet);

            using (XmlWriter writer = XmlWriter.Create(rutaConFichero, settings))
            {
                doc.Save(writer);
            }
        }

        public override string ToString()
        {
            using (var reader = ToXmlReader())
            {
                reader.MoveToContent();
                return reader.ReadOuterXml();
            }
        }

        public static T? FromFile(string xmlPath)
        {
            //var xr = new XmlRootAttribute { ElementName = "Facturae", Namespace = _Namespace() };
            //var serializer = new XmlSerializer(typeof(T), xr);

            //var fs = new StreamReader(xmlPath);
            //var xmlReader = XmlReader.Create(fs);
            //return serializer.Deserialize(xmlReader) as T;

            var xr = new XmlRootAttribute { ElementName = "Facturae", Namespace = _Namespace() };
            var serializer = new XmlSerializer(typeof(T), xr);

            using (var fs = new StreamReader(xmlPath))
            using (var xmlReader = XmlReader.Create(fs))
            {
                return serializer.Deserialize(xmlReader) as T;
            }
        }

        public static T? FromXml(string xml)
        {
            var xr = new XmlRootAttribute { ElementName = "Facturae", Namespace = _Namespace() };
            var serializer = new XmlSerializer(typeof(T), xr);

            var fs = new StreamReader(new MemoryStream(UTF8.GetBytes(xml)));
            var xmlReader = XmlReader.Create(fs);
            return serializer.Deserialize(xmlReader) as T;
        }

        private byte[] _Transform(string xsl, string logoPath)
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            XmlReaderSettings settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

            XsltArgumentList xslArgs = new XsltArgumentList();
            if (!string.IsNullOrEmpty(logoPath))
                xslArgs.AddParam("logoPath", string.Empty, logoPath);

            var numberFormatInfo = new CultureInfo("es-ES").NumberFormat;
            var numberFormat =
                $"###{numberFormatInfo.NumberGroupSeparator}###{numberFormatInfo.CurrencyDecimalSeparator}00";
            xslArgs.AddParam("numberFormat", string.Empty, numberFormat);

            var invoiceBytes = UTF8.GetBytes(ToString());
            var xslBytes = UTF8.GetBytes(xsl);

            using (MemoryStream memXsl = new MemoryStream(xslBytes))
            {
                xslt.Load(XmlReader.Create(memXsl, settings));
            }

            using (MemoryStream memOut = new MemoryStream())
            {
                using (MemoryStream memXml = new MemoryStream(invoiceBytes))
                {
                    xslt.Transform(XmlReader.Create(memXml, settings), xslArgs, XmlWriter.Create(memOut, xslt.OutputSettings));
                }

                var bytes = memOut.ToArray();
                if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                    bytes = bytes.Skip(3).ToArray();

                return bytes;
            }
        }

        public string ToHtml(string logoPath)
        {
            return UTF8.GetString(_Transform(Resource.facturae_html, logoPath));
        }

        public string Transform(string xsl, string logoPath)
        {
            var xslToUse = xsl;
            if (string.IsNullOrEmpty(xslToUse))
                xslToUse = Resource.facturae_html;

            return UTF8.GetString(_Transform(xsl, logoPath));
        }


        private void ValidateSchema()
        {
            if (_schemaSet is not null)
                XmlValidator.Validate(UTF8.GetBytes(ToString()), _schemaSet);
        }

        public void Validate() => ValidateSchema();

        public T? Validate(string fichero)
        {
            byte[] xmlDoc;

            using (FileStream fileStream = new FileStream(fichero, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    xmlDoc = binaryReader.ReadBytes((int)fileStream.Length);
                }
            }
            if (_schemaSet is not null)
                XmlValidator.Validate(xmlDoc, _schemaSet);

            return FromFile(fichero);
        }



    }
}
