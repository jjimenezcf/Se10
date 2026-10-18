using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilidades
{
    public enum enumTamanoDePagina { A4, A3 }

    public enum enumOrientacionDePagina { Vertical, Horizontal }

    // Tamaño y orientación de la página con la que se genera un pdf (de momento, al convertir una hoja de cálculo)
    public class OpcionesDePaginaPdf
    {
        public enumTamanoDePagina Tamano { get; set; } = enumTamanoDePagina.A4;

        public enumOrientacionDePagina Orientacion { get; set; } = enumOrientacionDePagina.Vertical;

        public bool EsHorizontal => Orientacion == enumOrientacionDePagina.Horizontal;

        // Valores de paperSize de Excel: A4 = 9 y A3 = 8
        public uint PapelDeExcel => Tamano == enumTamanoDePagina.A3 ? 8u : 9u;

        // Medidas de la página en vertical, en veinteavos de punto (twips), que es la unidad de Word
        public (uint ancho, uint alto) MedidasEnTwips => Tamano == enumTamanoDePagina.A3 ? (16838u, 23811u) : (11906u, 16838u);

        // Lee las opciones del JSON { "tamano": "A4", "orientacion": "Horizontal" } que llega del cliente.
        // Devuelve null si no se ha indicado nada. El contenido no se acepta tal cual: ambas propiedades son obligatorias y sólo se admiten
        // los nombres de los enumerados (no sus números), sin distinguir mayúsculas. Si algo no es válido lanza ArgumentException con un
        // mensaje que se puede mostrar al usuario.
        public static OpcionesDePaginaPdf Parsear(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return null;

            Dictionary<string, string> propiedades;
            try
            {
                var leido = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                propiedades = new Dictionary<string, string>(leido ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                throw new ArgumentException("Las opciones de la página no tienen un formato válido");
            }

            return new OpcionesDePaginaPdf
            {
                Tamano = LeerEnumerado<enumTamanoDePagina>(propiedades, "tamano", "el tamaño de la hoja"),
                Orientacion = LeerEnumerado<enumOrientacionDePagina>(propiedades, "orientacion", "la orientación")
            };
        }

        private static T LeerEnumerado<T>(Dictionary<string, string> propiedades, string propiedad, string descripcion)
        where T : struct, Enum
        {
            var valores = string.Join(", ", Enum.GetNames(typeof(T)));

            if (!propiedades.TryGetValue(propiedad, out var texto) || string.IsNullOrWhiteSpace(texto))
                throw new ArgumentException($"Falta indicar {descripcion} (valores admitidos: {valores})");

            var nombre = Enum.GetNames(typeof(T)).FirstOrDefault(n => n.Equals(texto.Trim(), StringComparison.OrdinalIgnoreCase));
            if (nombre == null)
                throw new ArgumentException($"El valor '{texto}' no es válido para {descripcion} (valores admitidos: {valores})");

            return (T)Enum.Parse(typeof(T), nombre);
        }
    }
}
