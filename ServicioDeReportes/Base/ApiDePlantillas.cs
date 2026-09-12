using DocumentFormat.OpenXml.Wordprocessing;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using pltMapeosDeTabla = System.Collections.Generic.Dictionary<string, string>;
using pltFilaDeTabla = System.Collections.Generic.Dictionary<string, string>;
using pltDatosDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using pltFormulasDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using DocumentFormat.OpenXml;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Utilidades;
using DocumentFormat.OpenXml.Packaging;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace ServicioDeReportes.Base
{
    public static class ApiDePlantillas
    {

        public static string CrearLaPlantilla(string nombre)
        {
            if (!Path.Exists(enumRutas.RutaDePlantillas))
                Directory.CreateDirectory(enumRutas.RutaDePlantillas);
            string plantilla = Path.Combine(enumRutas.RutaDePlantillas, nombre);

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(plantilla, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                mainPart.Document.Body = new Body();
            }
            return plantilla;
        }


        public static void EliminarLaPlantilla(this IPlantillaConAccion plantilla) => File.Delete(Path.Combine(enumRutas.RutaDePlantillas, plantilla.fichero));

        public static void ProcesarParte(OpenXmlCompositeElement parte, pltFormulasDePlantilla formulasDePlantilla, Dictionary<string, pltDatosDePlantilla> datosDePlantilla, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            ProcesarFormatosDeDto(parte, datosDelObjeto);
            ProcesarEtiquetasDeUnPa(parte, datosDePlantilla);
            ProcesarEtiquetasDelDto(parte, datosDelObjeto);
            ProcesarFormulas(parte, formulasDePlantilla);
        }

        // {{{Prefijo.Campo,N}}} -> si el valor de esa propiedad del Dto/ampliación es numérico (int/decimal/
        // float/double) y N es un entero >= 0, se sustituye por el valor redondeado a N decimales (con la
        // cultura del servidor). Si el valor NO es numérico, o N no es un entero válido, la etiqueta se deja
        // tal cual -- no se sustituye "a medias" ni con un número sin redondear.
        private static readonly Regex PatronDeEtiquetaConFormato = new Regex(
            @"^(?<prefijo>\w+)\.(?<campo>\w+)\s*,\s*(?<decimales>[^,}]*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Pasada independiente de ProcesarEtiquetasDelDto (que no toca): resuelve el sufijo ",N" de formato
        /// numérico sobre las mismas etiquetas {{{Prefijo.Campo}}} del propio elemento y sus ampliaciones
        /// (ver 4.a del manual). Se ejecuta primero para que, si el formato no aplica (valor no numérico o N
        /// inválido), la etiqueta quede intacta y sea ProcesarEtiquetasDelDto quien decida qué hacer con ella
        /// (de hecho no la tocará tampoco, porque ",N" no es ni "." ni "}}}" tras la clave -- se queda tal
        /// cual, que es exactamente lo pedido).
        /// </summary>
        private static void ProcesarFormatosDeDto(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            var textos = parte.Descendants<Text>().ToList();
            for (int i = 0; i < textos.Count; i++)
            {
                if (!textos[i].Text.Contains('{')) continue;

                var (textoCompleto, finIndex) = ObtenerTextoCompletoParaMaestros(textos, i);
                if (string.IsNullOrEmpty(textoCompleto)) continue;

                var resultado = new StringBuilder();
                var pos = 0;
                var huboSustituciones = false;
                while (true)
                {
                    var posInicio = textoCompleto.IndexOf(Simbolos.PltInicio, pos, StringComparison.OrdinalIgnoreCase);
                    if (posInicio < 0) { resultado.Append(textoCompleto, pos, textoCompleto.Length - pos); break; }

                    var posCierre = textoCompleto.IndexOf(Simbolos.PltCierre, posInicio);
                    if (posCierre < 0) { resultado.Append(textoCompleto, pos, textoCompleto.Length - pos); break; }

                    resultado.Append(textoCompleto, pos, posInicio - pos);
                    var etiqueta = textoCompleto.Substring(posInicio + Simbolos.PltInicio.Length, posCierre - posInicio - Simbolos.PltInicio.Length).Trim();

                    if (ResolverEtiquetaConFormato(datosDelObjeto, etiqueta, out var textoFormateado))
                    {
                        resultado.Append(textoFormateado);
                        huboSustituciones = true;
                    }
                    else
                        resultado.Append(Simbolos.PltInicio).Append(etiqueta).Append(Simbolos.PltCierre);

                    pos = posCierre + Simbolos.PltCierre.Length;
                }

                if (huboSustituciones)
                {
                    textos[i].Text = resultado.ToString();
                    for (int j = i + 1; j <= finIndex; j++) textos[j].Text = string.Empty;
                }
                i = finIndex;
            }
        }

        private static bool ResolverEtiquetaConFormato(Dictionary<string, Dictionary<string, object>> datosDelObjeto, string etiqueta, out string textoFormateado)
        {
            textoFormateado = null;

            var match = PatronDeEtiquetaConFormato.Match(etiqueta);
            if (!match.Success) return false;

            if (!int.TryParse(match.Groups["decimales"].Value.Trim(), out var decimales) || decimales < 0)
                return false;

            var prefijo = match.Groups["prefijo"].Value;
            var entrada = datosDelObjeto.FirstOrDefault(kv => kv.Key.Equals(prefijo, StringComparison.OrdinalIgnoreCase));
            if (entrada.Key is null) return false;

            var campo = match.Groups["campo"].Value;
            var propiedad = entrada.Value.FirstOrDefault(kv => kv.Key.Equals(campo, StringComparison.OrdinalIgnoreCase));
            if (propiedad.Key is null) return false;

            decimal numero;
            switch (propiedad.Value)
            {
                case decimal d: numero = d; break;
                case int n: numero = n; break;
                case float f: numero = (decimal)f; break;
                case double db: numero = (decimal)db; break;
                default: return false; // null, string, bool, enum, fecha... no es un número
            }

            textoFormateado = numero.ToString("N" + decimales);
            return true;
        }

        public static void ProcesarMapeosDeTablasDelPa(OpenXmlCompositeElement parte, Dictionary<string, pltMapeosDeTabla> descriptorDeMapeos, Dictionary<string, List<pltFilaDeTabla>> filasDeTablas)
        {
            foreach (var descriptor in descriptorDeMapeos)
            {
                var texto = BuscarMarcadoresFusionados(parte.Descendants<Text>().ToList(), descriptor.Key).FirstOrDefault();
                if (texto == null) continue;
                var elemento = texto.Parent;
                TableRow? filaMarcador = null;
                while (elemento != null)
                {
                    if (elemento == null) return;
                    if (elemento is TableRow) filaMarcador = (TableRow)elemento;
                    if (elemento is Table) break;
                    elemento = elemento.Parent;
                }
                if (filasDeTablas.ContainsKey(descriptor.Key))
                {
                    CrearLineasEnLaTabla(elemento as Table, descriptor.Value, filasDeTablas[descriptor.Key]);
                    filaMarcador?.Remove();
                }
            }
        }

        // {{{maestro.<clave>.<campo>}}}                                    -> propiedad directa de DatosPrincipales
        // {{{maestro.<clave>.direccion.<campo>}}}                          -> <campo> de la primera dirección
        // {{{maestro.<clave>.direccion.[<calificador>].<campo>}}}          -> <campo> de la primera dirección cuyo Calificador coincida
        // {{{maestro.<clave>.cuentabancaria.<campo>}}}                     -> <campo> de la primera cuenta bancaria
        // {{{maestro.<clave>.cuentabancaria.[<clase>].<campo>}}}           -> <campo> de la primera cuenta bancaria cuya Clase coincida
        private static readonly Regex PatronDeEtiquetaDeMaestro = new Regex(
            @"^maestro\.(?<clave>\w+)\.(?:(?<coleccion>direccion|cuentabancaria)\.(?:\[(?<filtro>[^\]]+)\]\.)?(?<campo>\w+)|(?<campoplano>\w+))$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Sustituye las etiquetas {{{maestro....}}} con los datos de DatosMaestros (Cliente/Proveedor/Solicitante
        /// del elemento que se imprime, y los que se vayan añadiendo a DefinicionesDeMaestros.Registro). Si una
        /// etiqueta no se puede resolver (alias, colección, filtro o campo no encontrados) se deja tal cual en el
        /// documento -- igual que el resto de etiquetas del sistema -- y se anota en la traza para poder revisarlo,
        /// sin bloquear la generación del documento.
        /// </summary>
        public static void ProcesarEtiquetasDeMaestros(OpenXmlCompositeElement parte, Dictionary<string, DatosDeUnMaestro> maestros, ContextoSe contexto)
        {
            var textos = parte.Descendants<Text>().ToList();
            for (int i = 0; i < textos.Count; i++)
            {
                // Disparador de un único carácter "{" -- ni siquiera basta con buscar "{{{" completo:
                // Word puede partir el delimitador en fragmentos de 1-2 caracteres ("{" + "{{"), así que
                // "{{{maestro...}}}" puede no aparecer entero en NINGÚN <w:t> individual. Con cualquier
                // "{" ya merece la pena fusionar hasta el final del párrafo y comprobar qué hay.
                if (!textos[i].Text.Contains('{')) continue;

                var (textoCompleto, finIndex) = ObtenerTextoCompletoParaMaestros(textos, i);
                if (string.IsNullOrEmpty(textoCompleto)) continue;

                // Un mismo bloque de texto (tras juntar los Text que comparten una etiqueta partida en varios
                // <w:t>) puede contener más de una etiqueta {{{...}}} -- p.ej. "{{{maestro.x.a}}} - {{{maestro.x.b}}}"
                // va en un único <w:t> si se escribió del tirón. Se recorren TODAS las ocurrencias de esa pasada,
                // no solo la primera.
                var resultado = new StringBuilder();
                var pos = 0;
                while (true)
                {
                    var posInicio = textoCompleto.IndexOf(Simbolos.PltInicio, pos, StringComparison.OrdinalIgnoreCase);
                    if (posInicio < 0) { resultado.Append(textoCompleto, pos, textoCompleto.Length - pos); break; }

                    var posCierre = textoCompleto.IndexOf(Simbolos.PltCierre, posInicio);
                    if (posCierre < 0) { resultado.Append(textoCompleto, pos, textoCompleto.Length - pos); break; }

                    resultado.Append(textoCompleto, pos, posInicio - pos);
                    var etiqueta = textoCompleto.Substring(posInicio + Simbolos.PltInicio.Length, posCierre - posInicio - Simbolos.PltInicio.Length).Trim();

                    if (etiqueta.StartsWith("maestro.", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = PatronDeEtiquetaDeMaestro.Match(etiqueta);
                        var (encontrado, valor) = match.Success ? ResolverEtiquetaDeMaestro(maestros, match) : (false, (object)null);
                        if (encontrado)
                        {
                            resultado.Append(FormatearValorDeMaestro(valor));
                        }
                        else
                        {
                            contexto.AnotarTraza(nameof(ProcesarEtiquetasDeMaestros), $"No se ha localizado el valor de la etiqueta '{Simbolos.PltInicio}{etiqueta}{Simbolos.PltCierre}'");
                            resultado.Append(Simbolos.PltInicio).Append(etiqueta).Append(Simbolos.PltCierre);
                        }
                    }
                    else
                        resultado.Append(Simbolos.PltInicio).Append(etiqueta).Append(Simbolos.PltCierre);

                    pos = posCierre + Simbolos.PltCierre.Length;
                }

                textos[i].Text = resultado.ToString();
                for (int j = i + 1; j <= finIndex; j++) textos[j].Text = string.Empty;
                i = finIndex;
            }
        }

        /// <summary>
        /// Fusiona el texto desde "inicio" hasta el final del párrafo que lo contiene. No basta con ir
        /// buscando "{{{"/"}}}" completos nodo a nodo: Word puede partir el propio delimitador en
        /// fragmentos de 1-2 caracteres ("{" + "{{"), así que "{{{...}}}" puede no aparecer entero en
        /// NINGÚN &lt;w:t&gt; individual, ni siquiera parcialmente reconocible hasta fusionar. El límite de
        /// párrafo es seguro porque ninguna etiqueta lo cruza (lo escribe siempre quien diseña la
        /// plantilla dentro de una misma línea/celda).
        /// </summary>
        private static (string textoCompleto, int finIndex) ObtenerTextoCompletoParaMaestros(List<Text> textos, int inicio)
        {
            var finIndex = inicio;
            var parrafo = textos[inicio].Ancestors<Paragraph>().FirstOrDefault();
            var ultimoTextoDelParrafo = parrafo?.Descendants<Text>().LastOrDefault();
            if (ultimoTextoDelParrafo != null)
            {
                var indice = textos.IndexOf(ultimoTextoDelParrafo);
                if (indice >= inicio) finIndex = indice;
            }

            var sb = new StringBuilder();
            for (int j = inicio; j <= finIndex; j++) sb.Append(textos[j].Text);
            return (sb.ToString(), finIndex);
        }

        /// <summary>
        /// Localiza el/los Text cuyo contenido, una vez fusionados los &lt;w:t&gt; partidos (ver
        /// ObtenerTextoCompletoParaMaestros), forma exactamente "{{{clave}}}" -- el mismo caso que rompía
        /// ProcesarEtiquetasDeMaestros pero para los marcadores de tabla ({{{lineasdefactura}}}, {{{Hitos}}},
        /// {{{LineaDeUnaFae}}}...), que antes se buscaban con una igualdad exacta sobre un único nodo y por
        /// eso no se encontraban si Word había partido el marcador en varios &lt;w:t&gt; (le pasa con
        /// frecuencia a nombres largos o con mayúsculas raras, tipo "LineaDeUnaFae").
        /// </summary>
        private static List<Text> BuscarMarcadoresFusionados(List<Text> textos, string clave)
        {
            var objetivo = (Simbolos.PltInicio + clave + Simbolos.PltCierre).ToLowerInvariant();
            var encontrados = new List<Text>();
            for (int i = 0; i < textos.Count; i++)
            {
                if (!textos[i].Text.Contains('{')) continue;
                var (textoCompleto, _) = ObtenerTextoCompletoParaMaestros(textos, i);
                if (textoCompleto.Trim().ToLowerInvariant() == objetivo) encontrados.Add(textos[i]);
            }
            return encontrados;
        }

        private static string FormatearValorDeMaestro(object valor)
        {
            if (valor is null || valor.EsCadena()) return $"{valor}";
            if (valor.EsEntero()) return ((int)valor).ToString();
            if (valor.EsDecimal()) return ((decimal)valor).ToString();
            if (valor.EsFecha()) return ((DateTime)valor).ToString();
            if (valor.EsBooleano()) return (bool)valor ? "Si" : "No";
            if (valor.EsEnumerado()) return ((Enum)valor).Descripcion();
            return $"{valor}";
        }

        private static (bool encontrado, object valor) ResolverEtiquetaDeMaestro(Dictionary<string, DatosDeUnMaestro> maestros, Match match)
        {
            var clave = match.Groups["clave"].Value;
            if (!maestros.TryGetValue(clave, out var maestro)) return (false, null);

            if (match.Groups["campoplano"].Success)
                return BuscarCampo(maestro.DatosPrincipales, match.Groups["campoplano"].Value);

            var esDireccion = match.Groups["coleccion"].Value.Equals("direccion", StringComparison.OrdinalIgnoreCase);
            var lista = esDireccion ? maestro.Direcciones : maestro.CuentasBancarias;
            var campoDeFiltro = esDireccion ? "Calificador" : "Clase";
            var campo = match.Groups["campo"].Value;

            Dictionary<string, object> item;
            if (match.Groups["filtro"].Success)
            {
                var filtro = match.Groups["filtro"].Value;
                item = lista.FirstOrDefault(d => BuscarCampo(d, campoDeFiltro) is (true, var v) && $"{v}".Equals(filtro, StringComparison.OrdinalIgnoreCase));
            }
            else
                item = lista.FirstOrDefault();

            return item is null ? (false, null) : BuscarCampo(item, campo);
        }

        private static (bool encontrado, object valor) BuscarCampo(Dictionary<string, object> datos, string campo)
        {
            var entrada = datos.FirstOrDefault(kv => kv.Key.Equals(campo, StringComparison.OrdinalIgnoreCase));
            return entrada.Key is null ? (false, null) : (true, entrada.Value);
        }

        public static void ProcesarMapeosDeDetalles(OpenXmlCompositeElement parte, DetallesDelObjeto detalles)
        {
            foreach (var informacionDeDetalle in detalles.Detalles)
            {
                var textos = parte.Descendants<Text>().ToList();
                if (textos is null || textos.Count == 0) return;
                var encabezados = BuscarMarcadoresFusionados(textos, informacionDeDetalle.Key);
                foreach (var item in encabezados)
                {
                    ProcesarItemSiEsTabla(item, informacionDeDetalle.Value);
                }
            }
        }

        public static void ProcesarMapeosDeExtensiones<T>(OpenXmlCompositeElement parte, List<T> detalles, enumEncabezadosDeTablas encabezado)
        {
            var textos = parte.Descendants<Text>().ToList();
            if (textos is null || textos.Count == 0) return;
            var encabezados = BuscarMarcadoresFusionados(textos, encabezado.ToString());
            foreach (var item in encabezados)
            {
                ProcesarItemSiEsTabla(item, detalles);
            }
        }

        private static void ProcesarItemSiEsTabla<T>(Text item, List<T> detalles)
        {
            var elemento = item.Parent;
            TableRow? filaMarcador = null;
            while (elemento != null)
            {
                if (elemento == null) return;
                if (elemento is TableRow) filaMarcador = (TableRow)elemento;
                if (elemento is Table) break;
                elemento = elemento.Parent;
            }
            if (elemento != null)
            {
                CrearFilasEnLaTabla((Table)elemento, detalles);
                filaMarcador?.Remove();
            }

        }

        private static void ProcesarFormulas(OpenXmlCompositeElement documento, pltFormulasDePlantilla formulasDePlantilla)
        {
            var formulas = documento.Descendants<Text>().Where(t => t.Text.ToLower().Contains($"{Simbolos.PltInicio}formula.")).ToList();
            foreach (Text formula in formulas)
            {
                var clave = formula.Text.Split(".")[1].Replace(Simbolos.PltCierre, "").Trim();
                if (!formulasDePlantilla.ContainsKey(clave)) continue;
                if (formulasDePlantilla[clave] == "DateTime.Now")
                    formula.Text = formula.Text.Replace(Simbolos.PltInicio + "formula." + clave + Simbolos.PltCierre, DateTime.Now.ToString("dd-MM-yyyy"));
            }
        }

        private static void ProcesarEtiquetasDeUnPaXX(OpenXmlCompositeElement parte, Dictionary<string, pltDatosDePlantilla> datosDePlantilla)
        {
            foreach (var agrupacion in datosDePlantilla)
            {
                var parrafos = parte.Descendants<Text>().Where(t => t.InnerText.ToLower().Contains(Simbolos.PltInicio + agrupacion.Key.ToLower() + ".")).ToList();
                foreach (Text parrafo in parrafos)
                {
                    bool parrafoProcesado = false;
                    var posicion = 0;
                    while (!parrafoProcesado)
                    {
                        var posInicial = parrafo.Text.Length > posicion ? parrafo.Text.IndexOf(Simbolos.PltInicio, posicion) : -1;
                        if (posInicial >= 0)
                        {
                            var posFinal = parrafo.Text.IndexOf(Simbolos.PltCierre, posicion);
                            var etiqueta = parrafo.Text.Substring(posInicial + Simbolos.PltInicio.Length, posFinal - posInicial - Simbolos.PltInicio.Length);
                            var partes = etiqueta.Split(".");
                            if (partes.Length > 1)
                            {
                                var clave = partes[1].Trim();
                                if (agrupacion.Value.ContainsKey(clave))
                                {
                                    parrafo.Text = parrafo.Text.Replace(Simbolos.PltInicio + agrupacion.Key + "." + clave + Simbolos.PltCierre, agrupacion.Value[clave]);
                                }
                                posicion = posInicial + 3;
                            }
                            else posicion = posFinal;
                        }
                        else parrafoProcesado = true;
                    }
                }
            }

        }

        private static void ProcesarEtiquetasDeUnPaX(OpenXmlCompositeElement parte, Dictionary<string, pltDatosDePlantilla> datosDePlantilla)
        {
            var parrafos = parte.Descendants<Text>().ToList();
            foreach (Text parrafo in parrafos)
            {
                foreach (var agrupacion in datosDePlantilla)
                {
                    var inicio = 0;
                    while (inicio < parrafo.Text.Length - Simbolos.PltInicio.Length - Simbolos.PltCierre.Length)
                    {
                        var posicion = parrafo.Text.IndexOf(Simbolos.PltInicio + agrupacion.Key, inicio, StringComparison.CurrentCultureIgnoreCase);
                        if (posicion < 0) break;
                        var final = parrafo.Text.IndexOf(Simbolos.PltCierre, inicio);
                        if (final < 0) break;
                        inicio = final + Simbolos.PltCierre.Length;
                        var etiqueta = parrafo.Text.Substring(posicion + Simbolos.PltInicio.Length, final - posicion - Simbolos.PltCierre.Length);
                        if (etiqueta.IndexOf(".") < 0) break;
                        var clave = etiqueta.Split(".")[1];
                        var dic = datosDePlantilla[agrupacion.Key.ToString()];
                        if (dic.ContieneClave(clave))
                            parte.InnerXml = parte.InnerXml.ToLower().Replace($"{Simbolos.PltInicio}{agrupacion.Key}.{clave}{Simbolos.PltCierre}".ToLower(), dic[dic.Clave(clave)]);
                    }
                }
            }
        }


        /// <summary>
        /// {{{alias.campo}}} del PA (punto 4.b del manual). Reescrito para fusionar primero los &lt;w:t&gt;
        /// partidos por Word (igual que ProcesarFormatosDeDto/ProcesarEtiquetasDeMaestros, vía
        /// ObtenerTextoCompletoParaMaestros) en vez de aplicar una expresión regular directamente sobre
        /// parte.InnerXml: si Word partía el delimitador "{{{alias.campo}}}" en varios &lt;w:t&gt; --algo
        /// habitual con el corrector ortográfico, sobre todo con alias largos o con mayúsculas poco
        /// frecuentes-- la etiqueta nunca aparecía contigua en el XML y la sustitución no se producía NUNCA,
        /// aunque el alias y el campo estuvieran bien escritos. Es el mismo fallo que describe el punto 4.g
        /// del manual para las etiquetas {{{maestro...}}}, pero aquí no estaba corregido todavía. De paso,
        /// alias y campo se comparan sin distinguir mayúsculas de minúsculas (el diccionario de valores
        /// guarda el nombre de columna del SELECT siempre en minúsculas, ExtensorDePlantillas.MapearCampoDatos).
        /// </summary>
        private static void ProcesarEtiquetasDeUnPa(OpenXmlCompositeElement parte, Dictionary<string, pltDatosDePlantilla> datosDePlantilla)
        {
            var textos = parte.Descendants<Text>().ToList();
            for (int i = 0; i < textos.Count; i++)
            {
                if (!textos[i].Text.Contains('{')) continue;

                var (textoCompleto, finIndex) = ObtenerTextoCompletoParaMaestros(textos, i);
                if (string.IsNullOrEmpty(textoCompleto)) continue;

                var resultado = new StringBuilder();
                var pos = 0;
                var huboSustituciones = false;
                while (true)
                {
                    var posInicio = textoCompleto.IndexOf(Simbolos.PltInicio, pos, StringComparison.OrdinalIgnoreCase);
                    if (posInicio < 0) { resultado.Append(textoCompleto, pos, textoCompleto.Length - pos); break; }

                    var posCierre = textoCompleto.IndexOf(Simbolos.PltCierre, posInicio);
                    if (posCierre < 0) { resultado.Append(textoCompleto, pos, textoCompleto.Length - pos); break; }

                    resultado.Append(textoCompleto, pos, posInicio - pos);
                    var etiqueta = textoCompleto.Substring(posInicio + Simbolos.PltInicio.Length, posCierre - posInicio - Simbolos.PltInicio.Length).Trim();

                    if (ResolverEtiquetaDeUnPa(datosDePlantilla, etiqueta, out var valor))
                    {
                        resultado.Append(valor);
                        huboSustituciones = true;
                    }
                    else
                        resultado.Append(Simbolos.PltInicio).Append(etiqueta).Append(Simbolos.PltCierre);

                    pos = posCierre + Simbolos.PltCierre.Length;
                }

                if (huboSustituciones)
                {
                    textos[i].Text = resultado.ToString();
                    for (int j = i + 1; j <= finIndex; j++) textos[j].Text = string.Empty;
                }
                i = finIndex;
            }
        }

        private static bool ResolverEtiquetaDeUnPa(Dictionary<string, pltDatosDePlantilla> datosDePlantilla, string etiqueta, out string valor)
        {
            valor = null;
            var posPunto = etiqueta.IndexOf('.');
            if (posPunto < 0) return false;

            var alias = etiqueta.Substring(0, posPunto);
            var campo = etiqueta.Substring(posPunto + 1).Trim().ToLowerInvariant();

            var entrada = datosDePlantilla.FirstOrDefault(kv => kv.Key.Equals(alias, StringComparison.OrdinalIgnoreCase));
            if (entrada.Key is null) return false;

            return entrada.Value.TryGetValue(campo, out valor);
        }

        private static void ProcesarEtiquetasDelDto(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            var textos = parte.Descendants<Text>().ToList();
            foreach (var (entradaKey, entradaValue) in datosDelObjeto)
            {
                foreach (var (claveKey, claveValue) in entradaValue)
                {
                    var sustituir = $"{entradaKey}.{claveKey}".ToLowerInvariant().Trim();
                    ProcesarSustitucion(textos, sustituir, claveValue);
                }
            }
        }

        private static void ProcesarSustitucion(List<Text> textos, string sustituir, object valor)
        {
            for (int i = 0; i < textos.Count; i++)
            {
                if (!textos[i].Text.Contains(Simbolos.PltInicio)) continue;

                var (textoCompleto, finIndex) = ObtenerTextoCompleto(textos, i);
                if (string.IsNullOrEmpty(textoCompleto)) continue;

                var textoLower = textoCompleto.ToLowerInvariant().Replace(" ", "");
                if (!textoLower.Contains(sustituir)) continue;

                if (DebeRealizarSustitucion(textoLower, sustituir))
                {
                    RealizarSustitucion(textos, i, finIndex, valor);
                }

                i = finIndex; // Saltar al final del bloque procesado
            }
        }

        private static (string textoCompleto, int finIndex) ObtenerTextoCompleto(List<Text> textos, int inicio)
        {
            var sb = new StringBuilder();
            for (int j = inicio; j < textos.Count; j++)
            {
                sb.Append(textos[j].Text);
                if (textos[j].Text.Contains(Simbolos.PltCierre))
                {
                    return (sb.ToString(), j);
                }
            }
            return (string.Empty, inicio);
        }

        private static bool DebeRealizarSustitucion(string textoLower, string sustituir)
        {
            int posicion = textoLower.IndexOf(sustituir);
            int posicionSiguiente = posicion + sustituir.Length;
            return posicionSiguiente < textoLower.Length &&
                   (textoLower[posicionSiguiente] == '.' || textoLower[posicionSiguiente] == '}');
        }

        private static void RealizarSustitucion(List<Text> textos, int inicio, int fin, object valor)
        {
            var posDeCierre = textos[fin].Text.IndexOf(Simbolos.PltCierre);
            var cadenaFinal = textos[fin].Text.Substring(posDeCierre + Simbolos.PltCierre.Length);

            SustituirTexto(textos[inicio], valor, cadenaFinal);
            for (int j = inicio + 1; j <= fin; j++)
            {
                textos[j].Text = string.Empty;
            }
        }



        private static void ProcesarEtiquetasDelDtoXX(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            var p = parte.Descendants<Text>().ToList();
            foreach (var entrada in datosDelObjeto)
            {
                foreach (var clave in entrada.Value)
                {
                    var sustituir = (entrada.Key + "." + clave.Key).ToLower().Trim();
                    var itemInicio = -1;
                    var itemfin = -1;
                    for (int i = 0; i < p.Count; i++)
                    {
                        //item de inicio 
                        if (p[i].Text.Contains(Simbolos.PltInicio))
                            itemInicio = i;
                        if (itemInicio > -1 && p[i].Text.Contains(Simbolos.PltCierre))
                            itemfin = i;
                        if (itemfin > -1)
                        {
                            var texto = "";
                            for (int j = itemInicio; j <= itemfin; j++)
                                texto = texto + p[j].Text;

                            texto = texto.ToLower().Replace(" ", "");
                            if (texto.Contains(sustituir))
                            {
                                int posicion = texto.IndexOf(sustituir);
                                int posicionSiguiente = posicion + sustituir.Length;
                                if (posicionSiguiente < texto.Length)
                                {
                                    var caracter = texto[posicionSiguiente];
                                    if (caracter == '.' || caracter == '}')
                                    {
                                        var posDeCierre = p[itemfin].Text.IndexOf(Simbolos.PltCierre);
                                        var cadenaFinal = p[itemfin].Text.Substring(posDeCierre + Simbolos.PltCierre.Length);

                                        SustituirTexto(p[itemInicio], clave.Value, cadenaFinal);
                                        for (int j = itemInicio+1; j <= itemfin; j++)
                                            p[j].Text = "";
                                    }
                                }
                            }
                            itemfin = -1;
                            itemInicio = -1;
                        }

                    }
                }
            }
        }


        private static void ProcesarEtiquetasDelDtoX(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            foreach (var entrada in datosDelObjeto)
            {
                var parrafos = parte.Descendants<Text>().Where(t => t.InnerText.ToLower().Contains(Simbolos.PltInicio + entrada.Key.ToLower() + ".")).ToList();
                foreach (Text parrafo in parrafos)
                {
                    string pattern = @"{{{(.*?)\.(.*?)}}}";
                    Match match = Regex.Match(parrafo.Text, pattern);
                    while (match.Success)
                    {
                        var claveDeEntrada = match.Groups[1].Value.ToLower().Trim();
                        if (entrada.Key.ToLower() == claveDeEntrada)
                        {
                            var claveDelCampo = match.Groups[2].Value.Trim();
                            var campos = entrada.Value;
                            if (campos.Keys.Any(x => x.Contains(claveDelCampo, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                var sustituir = Simbolos.PltInicio + entrada.Key + "." + claveDelCampo + Simbolos.PltCierre;
                                var formato = parrafo.Text.Siguientes(match.Index + match.Length, 3) == ".F(" ? parrafo.Text.Siguientes(match.Index + match.Length, ")") : "";
                                SustituirEtiqueta(parrafo, sustituir.ToLower(), campos.LeerValor(claveDelCampo, (object?)""));
                            }
                            else break;
                        }
                        match = Regex.Match(parrafo.Text, pattern);
                    }
                }
            }
        }


        private static void CrearFilasEnLaTabla<T>(Table tabla, List<T> detalles)
        {

            List<TableRow> filas = tabla.Descendants<TableRow>().ToList();
            var ultimaFila = filas.Last(x => !x.InnerText.IsNullOrEmpty());
            foreach (var detalle in detalles)
            {
                if (detalle is null) continue;
                TableRow nuevaFila = (TableRow)ultimaFila.CloneNode(true);
                var cells = nuevaFila.Elements<TableCell>().ToList();
                foreach (TableCell celda in cells)
                {
                    // Ojo: si Word ha partido la etiqueta de la celda en varios <w:t> (le pasa con
                    // frecuencia a nombres largos tipo "BaseImponible"), coger solo el primer Run/Text
                    // (como se hacía antes) deja el resto de fragmentos sin tocar, mostrando basura tipo
                    // "733,88 €BaseImponible}}}". Se cogen TODOS los Text de la celda; si su unión coincide
                    // con la etiqueta buscada, se fusionan en el primero (para que SustituirEtiqueta encuentre
                    // el texto completo) y se vacían los demás.
                    var textosDeLaCelda = celda.Descendants<Text>().ToList();
                    var texto = textosDeLaCelda.FirstOrDefault();
                    if (texto == null) continue;
                    foreach (PropertyInfo propiedad in detalle.GetType().GetProperties())
                    {
                        var sustituir = $"{Simbolos.PltInicio}{propiedad.Name}{Simbolos.PltCierre}".ToLower();
                        if (celda.InnerText.ToLower() == sustituir)
                        {
                            var valor = propiedad.GetValue(detalle);
                            texto.Text = celda.InnerText;
                            SustituirEtiqueta(texto, sustituir, valor is null ? "" : valor);
                            for (int k = 1; k < textosDeLaCelda.Count; k++) textosDeLaCelda[k].Text = string.Empty;
                            break;
                        }
                    }
                }
                tabla.InsertBefore(nuevaFila, ultimaFila);
            }


            while (tabla.Elements<TableRow>().LastOrDefault(x => x.InnerText.IsNullOrEmpty()) != null)
            {
                tabla.Elements<TableRow>()?.LastOrDefault()?.Remove();
            }
            var lastRow = tabla.Elements<TableRow>().LastOrDefault();
            if (lastRow != null) lastRow.Remove();
        }

        private static void CrearLineasEnLaTabla(Table? tabla, pltMapeosDeTabla descriptor, List<pltFilaDeTabla> datos)
        {
            if (tabla != null)
            {
                List<TableRow> filas = tabla.Descendants<TableRow>().ToList();
                var ultimaFila = filas.Last(x => !x.InnerText.IsNullOrEmpty());
                foreach (pltFilaDeTabla linea in datos)
                {
                    TableRow nuevaFila = (TableRow)ultimaFila.CloneNode(true);
                    var cells = nuevaFila.Elements<TableCell>().ToList();
                    foreach (TableCell cell in cells)
                    {
                        var mapeo = descriptor.FirstOrDefault(x => Simbolos.PltInicio + x.Key.ToLower() + Simbolos.PltCierre == cell.InnerText.ToLower());
                        if (mapeo.Key != null) RemplazarMarcador(cell, linea, mapeo);
                    }
                    tabla.InsertBefore(nuevaFila, ultimaFila);
                }
                while (tabla.Elements<TableRow>().LastOrDefault(x => x.InnerText.IsNullOrEmpty()) != null)
                {
                    tabla.Elements<TableRow>()?.LastOrDefault()?.Remove();
                }
                var lastRow = tabla.Elements<TableRow>().LastOrDefault();
                if (lastRow != null) lastRow.Remove();
            }
        }

        private static void RemplazarMarcador(TableCell celda, pltFilaDeTabla linea, KeyValuePair<string, string> mapeo)
        {
            if (!linea.ContainsKey(mapeo.Value)) return;

            // Mismo caso que en CrearFilasEnLaTabla: si la celda ("{{{col0}}}"...) llegó partida en varios
            // <w:t>, escribir solo en el primero y no vaciar el resto deja fragmentos sueltos en el resultado.
            var textosDeLaCelda = celda.Descendants<Text>().ToList();
            var texto = textosDeLaCelda.FirstOrDefault();
            if (texto == null) return;
            texto.Text = linea[mapeo.Value];
            for (int k = 1; k < textosDeLaCelda.Count; k++) textosDeLaCelda[k].Text = string.Empty;
        }

        private static void SustituirTexto(Text texto, object? valor, string cadenafinal)
        {
            var posIni = texto.Text.IndexOf(Simbolos.PltInicio);
            var cadenaInicial = texto.Text.Substring(0, posIni);


            if (valor is null || valor.EsCadena())
                texto.Text = $"{valor}";
            else if (valor.EsEntero()) texto.Text = ((int)valor).ToString();
            else if (valor.EsDecimal()) texto.Text = ((decimal)valor).ToString();
            else if (valor.EsFecha())
            {
                texto.Text = ((DateTime)valor).ToString();
            }
            else if (valor.EsBooleano()) texto.Text =  (bool)valor ? "Si": "No";
            else if (valor.EsEnumerado()) texto.Text = ((Enum)valor).Descripcion();

            texto.Text = cadenaInicial + texto.Text + cadenafinal;
        }


        private static void SustituirEtiqueta(Text etiqueta, string sustituir, object? valor)
        {
            if (valor is null || valor.EsCadena())
                etiqueta.Text = AsignarCadena(sustituir, etiqueta.Text.ToLower(), $"{valor}");
            else if (valor.EsEntero()) etiqueta.Text = AsignarEntero(sustituir, etiqueta.Text.ToLower(), (int)valor);
            else if (valor.EsDecimal()) etiqueta.Text = AsignarDecimal(sustituir, etiqueta.Text.ToLower(), (decimal)valor);
            else if (valor.EsFecha())
            {

                etiqueta.Text = AsignarFecha(sustituir, etiqueta.Text.ToLower(), (DateTime)valor);
            }
            else if (valor.EsBooleano()) etiqueta.Text = AsignarBooleano(sustituir, etiqueta.Text.ToLower(), (bool)valor);
            else if (valor.EsEnumerado()) etiqueta.Text = AsignarEnumerado(sustituir, etiqueta.Text.ToLower(), (Enum)valor);
        }

        private static bool EsEntero(this object valor) => valor is null ? false : valor.GetType() == typeof(int) || valor.GetType() == typeof(int?);
        private static bool EsDecimal(this object valor) => valor is null ? false : valor.GetType() == typeof(decimal) || valor.GetType() == typeof(decimal?);
        private static bool EsCadena(this object valor) => valor is null ? true : valor.GetType() == typeof(string);
        private static bool EsFecha(this object valor) => valor is null ? false : valor.GetType() == typeof(DateTime) || valor.GetType() == typeof(DateTime?);
        private static bool EsBooleano(this object valor) => valor is null ? false : valor.GetType() == typeof(bool) || valor.GetType() == typeof(bool?);
        private static bool EsEnumerado(this object valor)
        {
            var tipo = valor.GetType();
            if (tipo.IsEnum) return true;

            return tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(Nullable<>) && tipo.GetGenericArguments()[0].IsEnum;
        }

        private static string AsignarEntero(string sustituir, string texto, int numero)
        {
            var cadena = "";
            string pattern = @"Formatear\(N([0-9]|1[0-9]|20)\.([0-6])\)$";
            if (Regex.IsMatch(texto, pattern))
            {
                cadena = Regex.Replace(texto, pattern, m => numero.ToString($"N{m.Groups[1].Value},{m.Groups[2].Value}"));
            }
            else cadena = numero.ToString();

            return texto.Replace(sustituir, cadena);
        }

        private static string AsignarDecimal(string sustituir, string texto, decimal numero)
        {
            var cadena = "";
            string pattern = @"Formatear\(N([0-9]|1[0-9]|20)\.([0-6])\)$";
            if (Regex.IsMatch(texto, pattern))
            {
                cadena = Regex.Replace(texto, pattern, m => numero.ToString($"N{m.Groups[1].Value},{m.Groups[2].Value}"));
            }
            else cadena = numero.ToString("N2");

            return texto.Replace(sustituir, cadena);
        }

        private static string AsignarCadena(string sustituir, string texto, string valor) => texto.Replace(sustituir, valor.IsNullOrEmpty() ? "" : valor);

        private static string AsignarFecha(string sustituir, string texto, DateTime fecha) => texto.Replace(sustituir, fecha.ToString("dd-MM-yyyy"));
        private static string AsignarBooleano(string sustituir, string texto, bool valor) => texto.Replace(sustituir, valor ? extCadenas.enumCadenas.Si.ToString() : extCadenas.enumCadenas.No.ToString());

        private static string AsignarEnumerado(string sustituir, string texto, Enum valor) => texto.Replace(sustituir, valor.Descripcion());

    }
}
