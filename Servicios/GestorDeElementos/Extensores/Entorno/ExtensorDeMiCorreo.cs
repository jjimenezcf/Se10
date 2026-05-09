using ModeloDeDto.Entorno;
using Newtonsoft.Json;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilidades;
using System;

namespace GestorDeElementos.Extensores
{
    public enum enumMiCorreoModoAcceso { ApiKey, Auth2, IMAP }

    public class Adjunto
    {
        public string IdMail { get; set; }
        public string IdAdjunto { get; set; }
        public string Fichero { get; set; }
        public string TipoMime { get; set; }
        public string IdParte { get; set; }
    }
    public static class ExtensorDeMiCorreo
    {
        public const string NombreFicheroCurpoHtml = "_Cuerpo.Html";
        public const string BuzonProcesados = "Z_Procesados";

        public static MiCorreoDto SerializarAdjuntos(this MiCorreoDto correo, List<Adjunto> adjuntos)
        {
            correo.ConAdjuntos = adjuntos.Count > 0;
            correo.Adjuntos = JsonConvert.SerializeObject(adjuntos);
            return correo;
        }


        public static void Auditoria(this MiCorreoDto correo, ContextoSe contexto, enumNegocio negocio, IElementoDtm elemento, string accion)
        {
            if (negocio.UsaTrazas())
            {
                var nombre = correo.Emisor;
                Match match = Regex.Match(correo.Emisor, @"^(.*?)\s<(.*)>$");
                if (match.Success) nombre = match.Groups[1].Value;
                ((IUsaTraza)elemento).CrearTraza(contexto, $"Correo de {nombre.Replace("\"", "")}: {correo.Asunto}".Left(250), correo.Cuerpo);
            }
            var ficheros = "";
            if (!string.IsNullOrEmpty(correo.Adjuntos))
            {
                var adjuntos = JsonConvert.DeserializeObject<List<Adjunto>>(correo.Adjuntos);
                if (adjuntos != null && adjuntos.Any())
                    ficheros = string.Join(", ", adjuntos.Select(x => x.Fichero).ToList());
            }

            new MiCorreoDtm
            {
                IdMensaje = correo.IdMensaje,
                Buzon = correo.Buzon,
                Fecha = correo.Fecha,
                Emisor = correo.Emisor,
                To = correo.To,
                Asunto = correo.Asunto,
                Cuerpo = correo.Cuerpo,
                Adjuntos = ficheros,
                Accion = accion,
                IdNegocio = negocio.IdNegocio(),
                IdElemento = elemento.Id
            }.Insertar(contexto);
        }

        public static void VaciarCaches()
        {
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_filtrados);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Cantidad);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Del_Buzon);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Procesados);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_MisCorreos_Todos);
        }
    }
}
