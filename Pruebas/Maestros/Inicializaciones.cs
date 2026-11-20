using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MVCSistemaDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ValidacionesBase
{
    // Lo que daba ValidacionesContexto a los tests de NUnit, pero para un programa de consola: el host se construye con el Startup de la
    // aplicación web (contexto, mapeador, gestores...) sin ejecutarlo, así no se levantan la web ni la cola de trabajos sometidos.
    // Cada test se ejecuta en su propio scope (NuevoAmbito): dentro de él CrearContextoParaUsuario devuelve siempre el mismo contexto,
    // como hacía el scope único de ValidacionesContexto
    public static class Inicializaciones
    {
        private static IHost _host;
        private static IServiceScope _scope;
        public static HttpContext _httpContext;

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

        // Se llama antes de cada test: un contexto nuevo, sin nada de lo que dejó el anterior
        public static void NuevoAmbito()
        {
            _scope?.Dispose();
            _scope = Host.Services.CreateScope();
        }

        public static ContextoSe CrearContexto()
        {
            var configuracion = Host.Services.GetRequiredService<IConfiguration>();
            var login = configuracion.GetSection("EntornoDeTest")["Usuario"];
            return CrearContextoParaUsuario(login);
        }

        // Contexto en modo test: un Commit de los gestores no confirma nada (ver ExtensorDeContexto.Commit)
        public static ContextoSe CrearContextoParaUsuario(string login, bool debbugar = true)
        {
            if (_scope is null)
                NuevoAmbito();

            var contexto = _scope.ServiceProvider.GetRequiredService<ContextoSe>();
            contexto.Mapeador = _scope.ServiceProvider.GetRequiredService<IMapper>();
            contexto.Test = true;
            _httpContext = new DefaultHttpContext();
            AsignarUsuario(contexto, login);
            return contexto;
        }

        public static UsuarioDtm AsignarUsuario(this ContextoSe contexto, string login)
        {
            contexto.AsignarLogin(login, emitirError: true);

            var claim = new Claim("Login", login, @"http://www.w3.org/2001/XMLSchema#string", "LOCAL AUTHORITY", "LOCAL AUTHORITY");
            _httpContext ??= new DefaultHttpContext();
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));
            return contexto.Usuario;
        }
    }
}
