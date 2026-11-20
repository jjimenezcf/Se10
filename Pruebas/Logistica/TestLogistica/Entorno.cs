using System;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MVCSistemaDeElementos;
using ServicioDeDatos;
using Utilidades;

namespace TestLogistica
{
    // Entorno de la aplicación web sin arrancarla: el host se construye con su Startup (contexto, mapeador, gestores...) pero no se ejecuta,
    // así no se levantan la web ni la cola de trabajos sometidos
    public static class Entorno
    {
        private static IHost _host;

        private static IHost Host
        {
            get
            {
                _host ??= Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .UseContentRoot(AppContext.BaseDirectory)
                    .ConfigureLogging(logging => logging.ClearProviders())
                    .ConfigureWebHostDefaults(web => web.UseStartup<Startup>())
                    .Build();
                return _host;
            }
        }

        // Servidor y BD de la cadena de conexión de appsettings.json
        public static (string servidor, string baseDeDatos) Conexion()
        {
            var configuracion = Host.Services.GetRequiredService<IConfiguration>();
            var cadena = new SqlConnectionStringBuilder(configuracion.GetConnectionString(Literal.CadenaDeConexion));
            return (cadena.DataSource, cadena.InitialCatalog);
        }

        // Contexto del administrador en modo test: un Commit de los gestores no confirma nada (ver ExtensorDeContexto.Commit)
        public static ContextoSe CrearContexto()
        {
            var scope = Host.Services.CreateScope();
            var contexto = scope.ServiceProvider.GetRequiredService<ContextoSe>();
            contexto.Mapeador = scope.ServiceProvider.GetRequiredService<IMapper>();
            contexto.Test = true;
            contexto.AsignarLogin(ContextoSe.Login_Admin, emitirError: true);
            return contexto;
        }

        // Ejecuta la prueba en una transacción que siempre se deshace: la BD queda como estaba
        public static void EjecutarConRollback(string nombre, Action<ContextoSe, Informe> prueba)
        {
            var informe = new Informe(nombre);
            var contexto = CrearContexto();
            var transaccion = contexto.IniciarTransaccion();
            try
            {
                prueba(contexto, informe);
            }
            catch (Exception e)
            {
                informe.Excepcion(e);
            }
            finally
            {
                contexto.Rollback(transaccion);
                ServicioDeCaches.EliminarTodas();
                informe.Resumen();
                Console.WriteLine("Rollback realizado: la BD queda como estaba.");
            }
        }
    }
}
