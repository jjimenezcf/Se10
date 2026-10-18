using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Utilidades
{
    public static class ExtensorDeDiccionarios
    {

        public static Dictionary<string, object> ToDiccionario(this string json)
        {
            var diccionario = new Dictionary<string, object>();

            var arrayDefiltros = JsonConvert.DeserializeObject<List<object>>(json);
            for (int i = 0; i < arrayDefiltros.Count; i++)
            {
                var clave = ((Newtonsoft.Json.Linq.JArray)arrayDefiltros[i])[0].ToString();

                var item = ((Newtonsoft.Json.Linq.JArray)arrayDefiltros[i]);
                var valor = ExtensorDeDiccionarios.Parsear(item[1]);

                diccionario.Add(clave, valor);
            }
            return diccionario;
        }

        private static object Parsear(Newtonsoft.Json.Linq.JToken valor)
        {
            switch (valor.Type.ToString())
            {
                case nameof(Boolean): return Convert.ToBoolean(valor);
                case "Integer": return Convert.ToInt32(valor);
                case "Date": return Convert.ToDateTime(valor);
                case nameof(String): return Convert.ToString(valor);
            }
            throw new Exception($"No se ha definido como convertir el tipo {valor.Type} asociado al valor {valor}");
        }

        public static string Clave<T>(this Dictionary<string, T> diccionario, string clave)
        {
            if (diccionario == null)
                return null;

            foreach (var key in diccionario.Keys)
                if (key.ToLower().Equals(clave.ToLower()))
                    return key;
            return null;
        }

        public static bool ContieneClave<T>(this Dictionary<string, T> diccionario, string clave)
        {
            if (diccionario == null)
                return false;

            foreach (var key in diccionario.Keys)
                if (key.ToLower().Equals(clave.ToLower()))
                    return true;
            return false;
        }

        public static string LeerCadena(this Dictionary<string, string> diccionario, string clave, string valorPorDefecto)
        {
            if (diccionario == null)
                return valorPorDefecto;

            foreach (var key in diccionario.Keys)
                if (key.Equals(clave, StringComparison.CurrentCultureIgnoreCase))
                {
                    return diccionario[key];
                }
            return valorPorDefecto;
        }


        public static T LeerValor<T>(this Dictionary<string, object> diccionario, string clave)
        {
            var elemento = diccionario.FirstOrDefault(x => x.Key.Equals(clave, StringComparison.CurrentCultureIgnoreCase));
            if (elemento.Key is not null)
            {
                if (elemento.Value is null)
                {
                    if (typeof(T).EsNulable())
                        return (T)default;
                    throw new Exception($"El valor asociado a la {clave} es nulo, y el tipo {typeof(T).Name} no es nulable");
                }

                if (elemento.Value.GetType() == typeof(string) && ((string)elemento.Value).IsNullOrEmpty() && typeof(T) != typeof(string))
                {
                    if (typeof(T).EsNulable())
                        return (T)default;
                    throw new Exception($"El valor asociado a la {clave} es '', y el tipo {typeof(T).Name} no es nulable");
                }

                if (elemento.Value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    return ((Newtonsoft.Json.Linq.JArray)elemento.Value).ToObject<T>();

                //if (elemento.Value.GetType() == typeof(string) && typeof(T) != typeof(string))
                //{
                //    try
                //    {
                //        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>((string)elemento.Value);
                //    }
                //    catch (Exception ex)
                //    {
                //        throw new Exception($"El valor de la clave '{clave}' es un string pero no se pudo deserializar al tipo {typeof(T).Name}.", ex);
                //    }
                //}

                if (typeof(T).BaseType == typeof(Enum) && elemento.Value is string)
                    return (T)Enum.Parse(typeof(T), (string)elemento.Value, ignoreCase: true);

                if (elemento.Value.GetType() == typeof(long) && typeof(T) == typeof(int))
                {
                    object value = Convert.ToInt32(elemento.Value);
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                if (elemento.Value.GetType() == typeof(string) && typeof(T) == typeof(int) && elemento.Value.ToString().EsEntero())
                {
                    object value = Convert.ToInt32(elemento.Value);
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                if ((elemento.Value.GetType() == typeof(long?) || elemento.Value.GetType() == typeof(long)) && typeof(T) == typeof(int?))
                {
                    object value = Convert.ToInt32(elemento.Value);
                    return (T)value;
                }

                if (elemento.Value.GetType() == typeof(string) && (typeof(T).IsEnum || Nullable.GetUnderlyingType(typeof(T))?.IsEnum == true))
                {
                    var valor = ApiDeEnsamblados.ToEnumerado(typeof(T), (string)elemento.Value, errorSiNoHay: false);
                    return (T)valor;
                }

                if (elemento.Value.GetType() == typeof(double) && (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?)))
                {
                    object value = Convert.ToDecimal(elemento.Value);
                    return (T)value;
                }

                if (elemento.Value.GetType() == typeof(long) && (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?)))
                {
                    object value = Convert.ToDecimal(elemento.Value);
                    return (T)value;
                }

                if (elemento.Value.GetType() == typeof(string) && (typeof(T) == typeof(List<string>)))
                {
                    var cadena = elemento.Value.ToString().Trim('[', ']');
                    string[] elementos = cadena.Split(',');
                    List<string> lista = elementos.Select(e => e.Trim('"')).ToList();
                    return (T)(object)lista;
                }

                if (elemento.Value.GetType() == typeof(double) && (typeof(T) == typeof(float) || typeof(T) == typeof(float?)))
                {
                    if (float.TryParse(elemento.Value.ToString(), out float floatValue))
                    {
                        return (T)(object)floatValue;
                    }
                    else
                    {
                        // Maneja el caso en el que la conversión falla
                        throw new InvalidOperationException($"No se pudo convertir el valor '{elemento.Value}' a {typeof(T)}.");
                    }
                }
                return (T)elemento.Value;
            }

            throw new Exception($"No se encuentra la clave '{clave}' en el diccionario");
        }


        public static T LeerValor<T>(this Dictionary<string, object> diccionario, Enum clave, T valorPorDefecto)
        {
            return LeerValor<T>(diccionario, clave.ToString(), valorPorDefecto);
        }

        public static T LeerValor<T>(this Dictionary<string, object> diccionario, string clave, T valorPorDefecto)
        {
            if (diccionario == null || !diccionario.ContieneClave(clave))
                return valorPorDefecto;
            try
            {
                return diccionario.LeerValor<T>(clave);
            }
            catch
            {
                return valorPorDefecto;
            }
        }

        public static Dictionary<string, object> Concatenar(this Dictionary<string, object> destino, Dictionary<string, object> origen)
        {
            if (origen != null)
            {
                foreach (var kvp in origen)
                {
                    if (!destino.ContainsKey(kvp.Key))
                    {
                        destino.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return destino;
        }

    }
}
