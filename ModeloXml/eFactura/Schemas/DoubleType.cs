using System;

namespace ModeloXml.eFactura.Schemas
{
    [Serializable]
    public abstract class BaseDoubleType : System.Xml.Serialization.IXmlSerializable
    {
        public double Value { get; set; }

        protected BaseDoubleType()
        {
            Value = 0;
        }

        protected BaseDoubleType(double value)
        {
            Value = value;
        }

        protected abstract int DecimalPlaces { get; }

        public override string ToString()
        {
            return Value.ToString("F" + DecimalPlaces.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture);
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            string s = reader.ReadElementString();
            double d;
            if (double.TryParse(s, out d))
            {
                Value = d;
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(ToString());
        }

        #endregion
    }

    [Serializable]
    public class DoubleTwoDecimalType : BaseDoubleType
    {
        public DoubleTwoDecimalType()
        { 
        }

        public DoubleTwoDecimalType(double value)
            : base(value)
        {
        }

        protected override int DecimalPlaces => 2;
    }

    [Serializable]
    public class DoubleFourDecimalType : BaseDoubleType
    {
        public DoubleFourDecimalType()
        { 
        }

        public DoubleFourDecimalType(double value)
            : base(value)
        {
        }

        protected override int DecimalPlaces => 4;
    }

    [Serializable]
    public class DoubleSixDecimalType : BaseDoubleType
    {
        public DoubleSixDecimalType()
        { 
        }

        public DoubleSixDecimalType(double value)
            : base(value)
        {
        }

        protected override int DecimalPlaces => 6;
    }
}
