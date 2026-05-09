using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using AutoMapper;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCSistemaDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;

namespace ValidacionesBase
{
    public static class Inicializaciones
    {
        public static IServiceScope _scope;
        public static HttpContext _httpContext;

        public static IWebHost servidorWeb;

        public static ContextoSe CrearContexto()
        {
            var generador = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = generador.Build();
            IConfigurationSection EntornoDeTest = configuration.GetSection("EntornoDeTest");
            var login = EntornoDeTest["Usuario"];
            return CrearContextoParaUsuario(login);
        }


        public static ContextoSe CrearContextoParaUsuario(string login, bool debbugar = true)
        {
            if (servidorWeb is null)
            {
                var host = WebHost.CreateDefaultBuilder(new[] { "" }).UseStartup<Startup>();
                servidorWeb = host.Build();
                _scope = servidorWeb.Services.CreateScope();
            }

            ContextoSe contexto;
            contexto = _scope.ServiceProvider.GetRequiredService<ContextoSe>();
            contexto.Mapeador = _scope.ServiceProvider.GetRequiredService<IMapper>();
            contexto.Test = true;
            _httpContext = new DefaultHttpContext();
            AsignarUsuario(contexto, login);
            return contexto;
        }


        public static UsuarioDtm AsignarUsuario(this ContextoSe contexto, string login)
        {
            contexto.AsignarLogin(login, emitirError: true);

            Claim claim = new Claim("Login", login, @"http://www.w3.org/2001/XMLSchema#string", "LOCAL AUTHORITY", "LOCAL AUTHORITY");
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new List<Claim> { claim });
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _httpContext.User = claimsPrincipal;
            return contexto.Usuario;
        }

    }

}