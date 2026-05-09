using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Utilidades
{
    public static class Simbolos
    {
        private static NumberFormatInfo nfi { get; set; } = CultureInfo.CurrentCulture.NumberFormat;
        public static string SeparadorDecimal => nfi.NumberDecimalSeparator;
        public static string separadorDeMiles => nfi.NumberGroupSeparator;
        public static string signoNegativo => nfi.NegativeSign;

        public const string br = "<br>";

        public const string nbsp = "&nbsp;";

        public const string retorno = "\n";

        public const string tabulador = "\t";

        public static string tabuladorHtml => $"{nbsp}{nbsp}{nbsp}{nbsp}";

        public const string Punto = ".";

        public const string Coma = ",";

        public const string Pipe = "|";

        public const string Guion = "-";

        public const string Igual = "=";

        public const string PuntoComa = ";";

        public const string DosPuntos = ":";

        public const string Porcentaje = "%";

        public const string DosPuntosConEspacio = $"{DosPuntos} ";

        public const string Subrrallado = "_";
        public const string SeparadorDeRuta = "/";
        public static string separadorDeEnteros => Coma;
        public static string separadorCss => PuntoComa;
        public static string separadorDeParametrosJs => "#";
        public static string separadorDeRangos => PuntoComa;
        public static string separadorDeFechas => Guion;
        public static string separadorDeCtaban => Guion;
        public static string separadorDePartesHorarias => DosPuntos;
        public static string separadorDeEtapas => Pipe;
        public static string separadorDeColumnas => "#";
        public static string separadorDeValores => Pipe;
        public static string separadorParaMostrarEncolumnado => $" {Pipe} ";
        public static string separadorDeCadenasDeFiltrado => PuntoComa;
        public const string separadorDeClausulasDeOrdenacion = PuntoComa;
        public static string separadorDePeriodos => PuntoComa;
        public static string separadorDePropiedades => Pipe;
        public static string separadorDeCorreos => PuntoComa;

        public const string Descarte = "_";

        public const string PltInicio = "{{{";

        public const string PltCierre = "}}}";
        public const string ValorNuloDeUnParametro = "null";
        public const string Or = Pipe;

        public const string ArchivoCancelado = "(_)";

        public const string directorioActual = @".\";
        public const string directorioPadre = @"..\";

        public const string filtroPorDistinto = "!";

    }

    public static class ValoresPorDefecto
    {
        public const string Hoy = nameof(Hoy);
        public const string ActualizarFechaHasta = nameof(ActualizarFechaHasta);
    }

    public class Parametro
    {
        public string parametro { get; set; }
        public object valor { get; set; }

        public Parametro(string parametro, object valor)
        {
            this.parametro = parametro;
            this.valor = valor;
        }
    }

    public class Posicion
    {
        public int fila { get; set; }
        public int columna { get; set; }
        public Posicion()
        {
        }
        public Posicion(int f, int c)
        : this()
        {
            fila = f;
            columna = c;
        }
    }


    public class PropiedaJson
    {
        public string propiedad { get; set; }

        /// <summary>
        /// indica cómo se ha de ordenar el grid al renderizarlo la primera vez
        /// </summary>
        public string OrdenacionInicial { get; set; }

        /// <summary>
        /// Posición dentro del panel de edición o creación
        /// </summary>
        public Posicion posicion { get; set; }

        /// <summary>
        /// Indica el número de columnas del dto que ocupará la propiedad
        /// </summary>
        public int? colspan { get; set; }

        /// <summary>
        /// Indica si se expandira (si puede)
        /// </summary>
        public bool? autospan { get; set; }

        /// <summary>
        /// indica cómo las propiedades de cómo se ha de ordenar. Formato ejemplo: Tipo.Nombre
        /// </summary>
        public string ordenarGridPor { get; set; }

        public int PosicionEnGrid { get; set; }
        public short PosicionEnElDiv { get; set; }

        public string EtiquetaGrid { get; set; }
        public string Etiqueta { get; set; }

        public int PorAnchoMnt { get; set; }
        public string TamanoFijo { get; set; }

        /* propiedades para las lístas ínámicas y restrictores*/
        public string SeleccionarDe { get; set; }
        public string Controlador { get; set; }
        public string VistaDondeNavegar { get; set; }
        public string RestrictorFijo { get; set; }
        public string Negocio { get; set; }

        //css del control que se pinta
        public string css { get; set; }

        public bool? VisibleAlCrear { get; set; }
        public bool? VisibleAlEditar { get; set; }
        public bool? VisibleAlConsultar { get; set; }
        public bool? VisibleEnGrid { get; set; }
        public bool? EditableAlEditar { get; set; }
        public bool? EditableAlCrear { get; set; }

        //Eventos de listas (tras seleccionar o tras blanquear)
        public string trasSeleccionar { get; set; }
        public string trasBlanquear { get; set; }


        //Para indicar el Formato de la fecha o numero
        public enumFormato? Formato { get; set; }

        //Para indicar el ancho máximo del control
        public string AnchoMaximo { get; set; }
        public bool? EsAlmacenable { get; set; }
        public string Ayuda { get; set; }

    }

    public static class ApiClasesComunes
    {
        public static List<PropiedaJson> ObtenerAtributosJson(Type tipo, string rutaFichero, bool usarCache)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.DescriptoresJson);

            if (!cache.ContainsKey(tipo.FullName) || !usarCache)
            {
                if (!Directory.Exists(rutaFichero))
                {
                    Directory.GetCurrentDirectory();
                    throw new Exception($"No existe la ruta {rutaFichero}, defínala y almacene ahí los descriptores.json");
                }

                cache[tipo.FullName] = new List<PropiedaJson>();
                var nombre = $@"{rutaFichero}\{tipo.Name}.json";

                if (File.Exists(nombre))
                {
                    string jsonString = File.ReadAllText(nombre, Encoding.UTF8);
                    cache[tipo.FullName] = JsonConvert.DeserializeObject<List<PropiedaJson>>(jsonString);
                }
            }
            return (List<PropiedaJson>)cache[tipo.FullName];
        }

        public static bool EsNulo(string cadena)
        {
            return cadena.IsNullOrEmpty() || cadena.ToLower() == Simbolos.ValorNuloDeUnParametro;
        }
    }

}
