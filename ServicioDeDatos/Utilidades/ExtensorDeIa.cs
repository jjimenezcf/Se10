using DocumentFormat.OpenXml.Presentation;
using Gestor.Errores;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeDatos.Utilidades
{

    public static class ExtensorDeIa
    {

        public static IIa CrearIa(IaDeEntorno ia)
        {
            try
            {
                return ia.Enumerado switch
                {
                    enumIa.IaPerplexity => new IaPerplexity(ia.ApiKey, ia.Modelo),
                    enumIa.IaMistral => new IaMistral(ia.ApiKey, ia.Modelo),
                    enumIa.IaGeminis => new IaGeminis(ia.ApiKey, ia.Modelo),
                    enumIa.IaClaude => new IaClaude(ia.ApiKey, ia.Modelo),
                    enumIa.IaApyhub => new IaApyhub(ia.ApiKey),
                    _ => throw new Exception(IIa.IA_Seleccionada_No_Valida + $". Ias disponibles: '{nameof(IaPerplexity)}, {nameof(IaMistral)}, {nameof(IaGeminis)}, {nameof(IaApyhub)}'")
                };
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith(IIa.IA_ApiKey_No_Valida) || e.Message.StartsWith(IIa.IA_Modelo_No_Valido) || e.Message.StartsWith(IIa.IA_Seleccionada_No_Valida))
                    GestorDeErrores.Emitir(e.Message);
                throw;
            }
        }

        public static string IndicePregunta(string guid, string pregunta) => $"{guid}_{pregunta.ToLower().Replace(" ", "")}";

        public async static Task<string> AnalizarTextoParaFiltros(ContextoSe contexto, enumNegocio negocio, IIa ia, string texto, string listaDeCg, string listaDeTipos, string listaDeEstados,
            string listaDeTransiciones, string etapas, string guid, bool nuevaPregunta)
        {
            if (!ApiDeEnsamblados.ImplementaInterface(ia.GetType(), typeof(IIaPromptFiltrar).FullName))
                GestorDeErrores.Emitir($"La Ia {ia.GetType().Name} no implementa la interfaz necesaria para analizar el texto para filtros ({nameof(IIaPromptFiltrar)})");



            var reglasEspecificas = CacheDeVariable.IA_Resetera_Filtros
            ? negocio.Resetear(enumParametrosDeNegocio.IA_Prompt_Filtro, negocio.FiltoPorDefecto())
            : negocio.Parametro(enumParametrosDeNegocio.IA_Prompt_Filtro, crearParametro: true, valorPorDefecto: negocio.FiltoPorDefecto()).Valor;
                
            
            var cabecera = $"{nameof(UsuarioDtm.Id)}| {nameof(UsuarioDtm.Nombre)}|{nameof(UsuarioDtm.Apellido)}|{nameof(UsuarioDtm.Login)}";
            var filas = string.Join(Environment.NewLine,
                contexto.Set<UsuarioDtm>()
                        .Select(x => new { x.Id, x.Nombre, x.Apellido, x.Login })
                        .ToList()
                        .Select(x => $"{x.Id}|{x.Nombre}|{x.Apellido}|{x.Login}"));

            var listaUsuarios = $"{cabecera}{Environment.NewLine}{filas}";


            var historial = string.Empty;
            if (!nuevaPregunta)
            {
                var cache = ServicioDeCaches.Obtener(CacheDe.Ia_Filtros);
                var preguntas = IaPreguntaSql.LeerPreguntasDelGuid(contexto, guid).ToList();
                foreach (var registro in preguntas)
                {
                    var pregunta = cache.Keys.FirstOrDefault(key => key.Equals(IndicePregunta(guid, registro.Pregunta)));
                    if (pregunta != null)
                    {
                        historial = historial + $"Pregunta: \"{registro.Pregunta}\"\nRespuesta: {registro.Respuesta}";
                    }
                }
            }

            ((IIaPromptFiltrar)ia).PromptFiltrar = IIaPromptFiltrar.Prompt.Replace(IIaPromptFiltrar.Texto, texto)
                .Replace(IIaPromptFiltrar.ListaDeCentrosGestores, Environment.NewLine +  listaDeCg)
                .Replace(IIaPromptFiltrar.ListaDeTipos, Environment.NewLine + listaDeTipos)
                .Replace(IIaPromptFiltrar.ListaDeEstados, Environment.NewLine + listaDeEstados)
                .Replace(IIaPromptFiltrar.ListaDeTransiciones, Environment.NewLine + listaDeTransiciones)
                .Replace(IIaPromptFiltrar.ReglasEspecíficas, Environment.NewLine + reglasEspecificas)
                .Replace(IIaPromptFiltrar.NegocioTratado, Environment.NewLine + negocio.Plural())
                .Replace(IIaPromptFiltrar.ListaDeUsuarios, Environment.NewLine + listaUsuarios)
                .Replace(IIaPromptFiltrar.FechaDeHoy, extFechas.FechaFormatoIso8601(DateTime.Now))
                .Replace(IIaPromptFiltrar.ListaDeEtapas, Environment.NewLine + etapas)
                .Replace(IIaPromptFiltrar.HistorialDeSesion, historial);

            contexto.IniciarTraza($"{negocio}-prompt-{contexto.Usuario.Login}", debugar: true);
            try
            {
                contexto.AnotarTraza($"Prompt: {texto}", ((IIaPromptFiltrar)ia).PromptFiltrar);
                var json = await ia.AnalizarTextoParaFiltros(texto);
                contexto.AnotarTraza($"Json de respuesta: {texto}", json);
                return json;
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public async static Task<string> ProcesarFactura(int idArchivo, string rutaArchivo, IIa ia, ProcesadorOcr.ProcesadorOcr procesadorOcr)
        {
            if (extJson.EsArchivoJsonValido(rutaArchivo))
            {
                return null;
            }

            try
            {
                if (ApiDeEnsamblados.ImplementaInterface(ia.GetType(), typeof(IIaPromptFactura).FullName))
                    ((IIaPromptFactura)ia).PromptFactura = enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.IA_Prompt, crearParametro: true, valorPorDefecto: IIaPromptFactura.Prompt).Valor;

                //if (ia is IaApyhub)
                //{
                //    return await ia.AnalizarFactura(rutaArchivo);
                //}
                //else if (ia is IaDeepSeek)
                //{
                //    return await ia.AnalizarFactura(rutaArchivo);
                //}
                //else 
                if (ia is IaPerplexity)
                {
                    return await ia.AnalizarFactura(await procesadorOcr.ProcesarFichero(idArchivo, rutaArchivo));
                }
                else if (ApiDeEnsamblados.ImplementaInterface(ia.GetType(), typeof(IIaTiposMimesAdmitidos).FullName))
                {
                    return await AnalizarFactura((IIaTiposMimesAdmitidos)ia, idArchivo, rutaArchivo, procesadorOcr);
                }

                //return await ia.AnalizarFactura(rutaArchivo);
                throw Excepciones.Emitir($"Falta por implementar como usar la Ia de {ia.GetType().Name}");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Forbidden"))
                {
                    GestorDeErrores.Emitir($"Si quiere usar más el botón de AI debe actualizar su usuario en el servicio '{CacheDeVariable.IA_Usada}'");
                }
                throw;
            }
        }

        public async static Task<string> AnalizarFactura(IIaTiposMimesAdmitidos ia, int idArchivo, string rutaArchivo, ProcesadorOcr.ProcesadorOcr procesadorOcr)
        {
            string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));
            bool esTipoMimeAdmitido = ia.TiposMimeAdmitidosParaFacturas.Contains(mimeType);

            if (esTipoMimeAdmitido)
            {
                return await ia.AnalizarFacturaConOcr(rutaArchivo);
            }

            return await ((IIa)ia).AnalizarFactura(await procesadorOcr.ProcesarFichero(idArchivo, rutaArchivo));
        }

        public async static Task<string> ResumirFichero(IIaTiposMimesAdmitidos ia, enumNegocio negocio, string rutaArchivo)
        {
            var valorPorDefecto = negocio == enumNegocio.Pago ? IIaPromptResumen.PromptDePago : IIaPromptResumen.PromptDeResumen;
            ((IIaPromptResumen)ia).PromptResumen = negocio.Parametro(enumParametrosDeNegocio.IA_Prompt_Resumen, crearParametro: true, valorPorDefecto: valorPorDefecto).Valor;
            return await ia.ResumirConOcr(rutaArchivo);
        }

        public async static Task<string> ResumirContenido(IIa ia, enumNegocio negocio, string contenido)
        {
            if (ApiDeEnsamblados.ImplementaInterface(ia.GetType(), typeof(IIaPromptResumen).FullName))
            {
                var valorPorDefecto = negocio == enumNegocio.Pago ? IIaPromptResumen.PromptDePago : IIaPromptResumen.PromptDeResumen;

                ((IIaPromptResumen)ia).PromptResumen = negocio.Parametro(enumParametrosDeNegocio.IA_Prompt_Resumen, crearParametro: true, valorPorDefecto: valorPorDefecto)
                    .Valor.Replace("[CONTENIDO]", contenido);
            }

            return await ia.Resumir(contenido);
        }


    }

    public static class IaPreguntaSql
    {
        /// <summary>
        /// Graba una pregunta de IA en la base de datos.
        /// </summary>
        public static void GrabarPregunta(ContextoSe contexto, IaPreguntaDtm pregunta)
        {
            pregunta.Fecha = DateTime.Now;
            contexto.Set<IaPreguntaDtm>().Add(pregunta);
            contexto.SaveChanges();
        }

        /// <summary>
        /// Lee todas las preguntas asociadas a un guid de sesión, ordenadas por fecha ascendente,
        /// para poder encadenarlas como historial de conversación.
        /// </summary>
        public static List<IaPreguntaDtm> LeerPreguntasDelGuid(ContextoSe contexto, string guid)
        {
            return contexto.Set<IaPreguntaDtm>()
                           .Where(p => p.Guid == guid)
                           .OrderBy(p => p.Fecha)
                           .ToList();
        }

        public static IaPreguntaDtm BuscarPregunta(ContextoSe contexto, string pregunta)
        {
            return contexto.Set<IaPreguntaDtm>()
                           .FirstOrDefault(p => p.Pregunta == pregunta);
        }
    }
}
