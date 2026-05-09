using System;
using System.IO;
using System.Threading;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ServicioDeCorreos;
using ServicioDeDatos;



namespace ColaDeTrabajosSometidos
{
    public class Program
    {
        public IConfiguration Configuracion { get; }
        public static  void Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            var hostConstruido = host.Build();

            hostConstruido.Start();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);


            builder = builder
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var cadenaDeConexion = ContextoSe.ObtenerDatosDeConexion().CadenaConexion;
                    services.AddDbContext<ContextoSe>(options => options.UseSqlServer(cadenaDeConexion));
                    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
                    services.AddScoped<GestorDeTrabajosDeUsuario>();
                    services.AddScoped<GestorDeUsuarios>();
                    services.AddHostedService<BackgroundCola>();
                    services.AddHttpClient<IGraphClient, HttpGraphClient>();
                    services.AddSingleton<IGraphTokenService, GraphTokenService>();
                    services.AddTransient<IDistribuidorOfice365, Oficce365>();

                    services.AddTransient<MailJet.Tenante>();
                    services.AddTransient<IDistribuidorMailJet, MailJet>();
                });

            return builder; 
        }
    }
}
