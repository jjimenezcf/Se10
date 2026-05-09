using System.Xml;
using System.Xml.Schema;

namespace ModeloXml.eFactura.Validation
{
    /// <summary>
    /// Static class used to validate an XML file.
    /// </summary>
    public static class XmlValidator
    {
        // Validation Error Count
        private static int _errorsCount;

        // Validation Error Message
        private static string _errorMessage = string.Empty;

        private static void validationHandler(object sender, ValidationEventArgs args)
        {
            _errorMessage = _errorMessage + args.Message + "\r\n";
            _errorsCount++;
        }

        public static void Validate(byte[] xmlDoc, XmlSchemaSet schemas)
        {
            _errorsCount = 0;
            _errorMessage = string.Empty;

            // Declare local objects

            // Create your fragment reader
            var xmlReader = new XmlTextReader(new MemoryStream(xmlDoc));

            // Add the schema to your reader settings
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(schemas);

            // Add validation event handler
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += validationHandler;

            // Create your reader with the validation
            var reader = XmlReader.Create(xmlReader, settings);

            // Validate XML data
            while (reader.Read())
                reader.Close();

            // Raise exception, if XML validation fails
            if (_errorsCount > 0)
            {
                throw new XmlException(_errorMessage);
            }
        }

        /// <summary>
        /// Create a precompiled XMLSchemaSet.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="schemas"></param>
        /// <returns></returns>
        public static XmlSchemaSet CreateXmlSchemaSet(string schemaName, Dictionary<string, byte[]> schemas)
        {
            //Create and compile XmlSchemaSet
            using (var xsdReader = new XmlTextReader(new MemoryStream(schemas[schemaName])))
            {
                var schemaSet = new XmlSchemaSet {XmlResolver = new SchemaResolver(schemas)};
                schemaSet.Add(null, xsdReader);
                schemaSet.Compile();
                return schemaSet;
            }
        }

    }
}
