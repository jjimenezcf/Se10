using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace Utilidades
{
    public static class ExtensorDeJson
    {
        public static bool EsArchivoJsonValido(string rutaArchivo)
        {
            try
            {
                string contenido = File.ReadAllText(rutaArchivo);
                JsonDocument.Parse(contenido);
                return true;
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }

        public static bool EsJsonValido(FileStream fileStream)
        {
            try
            {
                using (JsonDocument.Parse(fileStream))
                {
                    return true;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }


        public static void ValidarJson(string json)
        {

            var cache = ServicioDeCaches.Obtener(CacheDe.ValidarJson);
            if (!cache.ContainsKey(json))
            {
                JSchemaGenerator generator = new JSchemaGenerator();
                try
                {
                    JSchema schema = generator.Generate(typeof(List<Parametro>));
                    JArray actualJson = JArray.Parse(json);
                    bool valid = actualJson.IsValid(schema, out IList<string> errorMessages);

                    if (!valid)
                    {
                        var mensaje = "";
                        foreach (var me in errorMessages)
                        {
                            mensaje = $"{mensaje}{Environment.NewLine}{me}";
                        }
                        throw new Exception($"Parámetros Json mal definido.{Environment.NewLine}{json}{Environment.NewLine}{mensaje}");
                    }
                    else cache[json] = true;
                }
                catch (Exception exc)
                {
                    if (!exc.Message.Contains("The free-quota limit"))
                        throw new Exception($"Json mal definido", exc);
                }
            }
        }

        public static string ToJson(this List<Parametro> p)
        {
            if (p == null)
                p = new List<Parametro>();

            return JsonConvert.SerializeObject(p);
        }

        public static List<Parametro> ToListaDeParametros(this string json)
        {
            // ValidarJson(json);
            return JsonConvert.DeserializeObject<List<Parametro>>(json);
        }

        public static List<T> JsonToLista<T>(this string json)
        {
            if (json.IsNullOrEmpty())
                return new List<T>();
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public static Dictionary<string, object> ToDiccionarioDeParametros(this string parametrosJson)
        {
            var parametros = new Dictionary<string, object>();
            if (!parametrosJson.IsNullOrEmpty())
            {
                var listaJson = parametrosJson.ToListaDeParametros();
                foreach (var p in listaJson)
                {
                    if (parametros.ContainsKey(p.parametro) && parametros[p.parametro].ToString() != p.valor.ToString())
                        throw new Exception($"El parámetro '{p.parametro}' ya existe");
                    if (parametros.ContainsKey(p.parametro) && parametros[p.parametro].ToString() == p.valor.ToString())
                        continue;
                    parametros.Add(p.parametro, p.valor);
                }
            }
            return parametros;
        }

        public static string ToJson(this List<string> lista)
        {
            if (lista == null)
                return null;
            return JsonConvert.SerializeObject(lista);
        }

        public static string ToJson(this List<int> lista)
        {
            if (lista == null)
                return null;
            return JsonConvert.SerializeObject(lista);
        }
        public static string ToJson(this Dictionary<string, object> dic)
        {
            var parametros = new List<Parametro>();
            foreach (var clave in dic.Keys)
            {
                var p = new Parametro(clave, dic[clave]);
                parametros.Add(p);
            }

            return parametros.ToJson();
        }

        public static void QuitarPropiedades(this JToken token, List<string> propiedades)
        {
            if (token is JObject obj)
            {
                foreach (var propiedad in propiedades)
                {
                    if (obj[propiedad] != null)
                    {
                        obj.Remove(propiedad);
                    }
                }

                foreach (var propiedad in obj.Properties())
                {
                    propiedad.Value.QuitarPropiedades(propiedades);
                }
            }
            else if (token is JArray arr)
            {
                foreach (var item in arr)
                {
                    item.QuitarPropiedades(propiedades);
                }
            }
        }

        public static object ObtenerPropiedad(this JsonDocument jsonDocument, string ruta)
        {
            JsonElement element = jsonDocument.RootElement;
            string[] propiedades = ruta.Split('.');

            foreach (var propiedad in propiedades)
            {
                if (propiedad.Contains("["))
                {
                    var arrayProp = propiedad.Split('[');
                    var arrayIndex = int.Parse(arrayProp[1].TrimEnd(']'));
                    element = element.GetProperty(arrayProp[0])[arrayIndex];
                }
                else
                {
                    if (!element.TryGetProperty(propiedad, out JsonElement childElement))
                    {
                        return null; // O manejar el caso de propiedad faltante de otra manera
                    }
                    element = childElement;
                }
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        public static bool JsonEquals(string json1, string json2)
        {
            // Una comprobación rápida de nulidad o strings vacíos
            if (string.IsNullOrWhiteSpace(json1) || string.IsNullOrWhiteSpace(json2))
                return false;

            try
            {
                var j1 = JToken.Parse(json1);
                var j2 = JToken.Parse(json2);
                return JToken.DeepEquals(j1, j2);
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static JObject Deserializar(string jsonString)
        {
            try
            {
                return JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("La cadena proporcionada no es un JSON válido.", ex);
            }
        }
    }
}
