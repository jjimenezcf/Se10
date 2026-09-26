using System;
using System.Net.Http;
using System.Xml.Linq;

namespace GestorDeElementos
{
    // Servicio web gratuito y público de la Dirección General del Catastro (OVCCallejero),
    // usado para validar si una vía existe en el callejero oficial de un municipio.
    public static class ApiDeCatastro
    {
        private const string UrlConsultaVia = "https://ovc.catastro.meh.es/ovcservweb/OVCSWLocalizacionRC/OVCCallejero.asmx/ConsultaVia";

        private static readonly XNamespace Ns = "http://www.catastro.meh.es/";

        public static bool ExisteLaVia(string provincia, string municipio, string nombreVia)
        {
            var uri = $"{UrlConsultaVia}?Provincia={Uri.EscapeDataString(provincia.ToUpper())}&Municipio={Uri.EscapeDataString(municipio.ToUpper())}&TipoVia=&NombreVia={Uri.EscapeDataString(nombreVia.ToUpper())}";

            using var http = new HttpClient();
            var xml = http.GetStringAsync(uri).GetAwaiter().GetResult();

            var numeroDeCoincidencias = (int?)XDocument.Parse(xml).Root?.Element(Ns + "control")?.Element(Ns + "cuca") ?? 0;
            return numeroDeCoincidencias > 0;
        }
    }
}
