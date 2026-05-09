using System;
using System.Threading;
using System.Threading.Tasks;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;

namespace ColaDeTrabajosSometidos
{
    public class BackgroundCola : BackgroundService
    {
        private IServiceProvider _servicios;

        public UsuarioDtm Usuario { get; private set; }

        public ContextoSe Contexto { get; private set; }

        public BackgroundCola(IServiceProvider services)
        {
            _servicios = services;
            ObtenerUsuarioEjecutor();
        }

        public void ObtenerUsuarioEjecutor()
        {
            var scope = _servicios.CreateScope();
            using (var gestor = scope.ServiceProvider.GetRequiredService<GestorDeUsuarios>())
            {
                Contexto = gestor.Contexto; 
                Usuario = gestor.LeerRegistroCacheado(nameof(UsuarioDtm.Login), CacheDeVariable.Cola_LoginDeEjecutor, true, true, false);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var minutos = CacheDeVariable.Cola_Usa_Gestor_De_Colas ? 0 : CacheDeVariable.Cola_Tiempo_De_Espera;
            Contexto.IniciarTraza("Bucle de ejecución de cola", debugar: true);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Contexto.AnotarTraza("Entro en el bucle", $"Entro a las {DateTime.Now}");
                    var scope = _servicios.CreateScope();
                    using (var gestor = scope.ServiceProvider.GetRequiredService<GestorDeTrabajosDeUsuario>())
                    {
                        Contexto.AnotarTraza("llamo a procesar cola", $"llamo a las {DateTime.Now}");
                        gestor.ProcesarCola(Usuario).Wait();
                        Contexto.AnotarTraza("Termino de procesar cola", $"termino a las {DateTime.Now}");
                        if (minutos == 0)
                        {
                            break;
                        }
                        else await Task.Delay(minutos*60*1000, stoppingToken);

                    }
                }
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }


    }

}
