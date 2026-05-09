
using GestorDeElementos;
using Microsoft.AspNetCore.Http;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SistemaDeElementos.Middleware
{
    public class EliminarFicherosMiddelware
    {
        private readonly RequestDelegate _next;

        public class Ficheros
        {
            internal const string PendienteDeBorrar = nameof(PendienteDeBorrar);
            internal List<string> Rutas { get; set; } = new List<string>();

            public Ficheros(HttpContext httpContext, string fichero)
            : this(httpContext)
            {
                Rutas.Add(fichero);
            }
            public Ficheros(HttpContext httpContext)
            {
                if (!httpContext.Items.ContainsKey(PendienteDeBorrar))
                    httpContext.Items[PendienteDeBorrar] = Rutas;
            }

            public void Add(string fichero)
            {
                if (fichero.IndexOf(ApiDeArchivos.FicheroNoEncontrado) > 0)
                    return;

                if (fichero.IndexOf(ApiDeArchivos.FicheroBloqueado) > 0)
                    return;

                if (!Rutas.Contains(fichero))
                    Rutas.Add(fichero);
            }


        }


        public EliminarFicherosMiddelware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Llamar al siguiente middleware en la cadena
            await _next(context);

            // Después de que la respuesta se haya enviado, eliminar los archivos
            if (context.Items.TryGetValue(Ficheros.PendienteDeBorrar, out var rutas) && rutas is List<string>)
            {
                foreach (var path in (List<string>)rutas)
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (IOException ex)
                        {
                            try
                            {
                                var ruta = CacheDeVariable.CFG_Ruta_Ficheros_De_Debug;
                                if (!Directory.Exists(ruta))
                                    Directory.CreateDirectory(ruta);

                                var nombreFichero = $"MiddelWare_EliminarFichero_{DateTime.Now:yyyy-MM-dd}.log";
                                var rutaCompleta = Path.Combine(ruta, nombreFichero);
                                var linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No se pudo eliminar '{path}': {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

                                await File.AppendAllTextAsync(rutaCompleta, linea);
                            }
                            catch
                            {
                                // Si falla el registro tampoco podemos hacer nada más
                            }
                        }
                    }
                }

                // Limpiar el elemento de context.Items
                context.Items.Remove(Ficheros.PendienteDeBorrar);
            }
        }


    }
}
