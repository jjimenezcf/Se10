using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace ModeloXml.eFactura.Validation
{

    public class SchemaResolver : XmlResolver
    {
        private readonly Dictionary<string, byte[]> _schemas;

        public SchemaResolver(Dictionary<string, byte[]> schemas)
        {
            _schemas = schemas;
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            string name = absoluteUri.AbsoluteUri.Split('/').Last();
            byte[] stream = _schemas[name];
            if (stream != null)
            {
                return new MemoryStream(stream);
            }
            XmlUrlResolver resolver = new XmlUrlResolver();
            return resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
        }

        public override ICredentials Credentials
        {
            set
            {
            }
        }
    }
}
