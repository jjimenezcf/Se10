using DocumentFormat.OpenXml;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;


namespace Utilidades
{
    public class ValorAttribute : Attribute
    {
        public string Value { get; }

        public ValorAttribute(string value)
        {
            Value = value;
        }
    }

    public class CssAttribute : Attribute
    {
        public string Value { get; }

        public CssAttribute(string value)
        {
            Value = value;
        }
    }


    public static class ExtensorDeEnum
    {
        public static string Descripcion(this Enum valorEnumerado, bool enMinusculas = false)
        {
            var type = valorEnumerado.GetType();
            if (!type.IsEnum)
            {
                throw new Exception($"{nameof(valorEnumerado)} debe ser un valor de enumerado");
            }
            var memberInfo = type.GetMember(valorEnumerado.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enMinusculas ? valorEnumerado.ToString().ToLower() : valorEnumerado.ToString();
        }

        public static string Valor(this Enum valorEnumerado)
        {
            var type = valorEnumerado.GetType();
            var memberInfo = type.GetMember(valorEnumerado.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(ValorAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((ValorAttribute)attrs[0]).Value;
                }
            }
            return valorEnumerado.ToString();
        }

        public static string Css(this Enum cssEnumerado)
        {
            var type = cssEnumerado.GetType();
            var memberInfo = type.GetMember(cssEnumerado.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(CssAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((CssAttribute)attrs[0]).Value;
                }
            }
            return cssEnumerado.ToString();
        }

        public static string ObtenerLaDescripcioDeUnEnumerado(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static bool Existe<T>(string descripcion)
        where T : Enum
        =>
        Enum.GetValues(typeof(T)).Cast<T>().Any(valor => valor.Descripcion() == descripcion);


        public static T Enumerado<T>(string descripcion)
        where T : Enum
        =>
        Enum.GetValues(typeof(T)).Cast<T>().First(valor => valor.Descripcion() == descripcion);

    }
}
