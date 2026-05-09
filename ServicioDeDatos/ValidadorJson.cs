using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using Utilidades;

namespace ServicioDeDatos
{

    public static class ValidadorDeJson
    {
        // El 'tipoClaseValidadora' es el Type de la clase donde residen tus métodos estáticos (ej: typeof(MiClaseDeMapeo))
        public static void ValidarPropiedadesJson(string jsonString, ContextoSe contexto, Type tipoClaseValidadora)
        {
            JObject jsonObject = extJson.Deserializar(jsonString);

            // 2. Recorrer las propiedades (claves) del objeto JSON
            foreach (var parClaveValor in jsonObject)
            {
                string nombreDePropiedad = parClaveValor.Key;
                JToken valorJson = parClaveValor.Value;

                // 3. Definir el nombre esperado del método validador
                // Ej: Si la clave JSON es 'nombre', el método esperado es 'ValidarNombre'
                string nombreDelMetodoValidador = $"Validar{nombreDePropiedad}";

                // 4. Buscar el método estático en la clase validadora proporcionada
                // Los métodos deben ser estáticos, públicos, y recibir (ContextoSe, JToken)
                MethodInfo metodoValidador = tipoClaseValidadora.GetMethod(
                    nombreDelMetodoValidador,
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(ContextoSe), typeof(JToken) }, // Parámetros esperados
                    null
                );

                // 5. Validar la existencia del método
                if (metodoValidador == null)
                {
                    // Si el método NO existe, emitir el error de implementación
                    throw new InvalidOperationException(
                        $"Error de implementación: Se debe implementar un método estático público llamado " +
                        $"'{nombreDelMetodoValidador}' con la firma (ContextoSe, JToken) para validar " +
                        $"la clave JSON '{nombreDePropiedad}' en la clase '{tipoClaseValidadora.Name}'."
                    );
                }

                // Parámetros: ContextoSe y el valor JSON (JToken)
                object[] parametros = new object[] { contexto, valorJson };

                // Invocar el método estático
                metodoValidador.Invoke(null, parametros);
            }
        }
    }

}
