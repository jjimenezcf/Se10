using Microsoft.Extensions.Hosting;
using ServicioDeDatos;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MVCSistemaDeElementos.ColaDeTrabajosSometidos;
public class MonitorBackgroundCola : BackgroundService
{
    private IServiceScopeFactory _serviceScopeFactory;
    private static TimeSpan _checkPeriod = TimeSpan.FromMinutes(30); 

    private ContextoSe _contexto
    {
        get
        {
            var scope = _serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ContextoSe>();
        }
    }

    public MonitorBackgroundCola(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new(_checkPeriod);
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            _contexto.IniciarTraza(nameof(MonitorBackgroundCola));
            try
            {
                CheckColaExecution();
            }
            catch (Exception e)
            {
                _contexto.AnotarExcepcion(e);
            }
            finally
            {
                _contexto.CerrarTraza();
            }
        }
       
    }

    private void CheckColaExecution()
    {
        var fechaDeAlerta = DateTime.Now.AddMinutes(-CacheDeVariable.Cola_Tiempo_De_Espera);
        if (BackgroundCola_II.UltimaEjecucionExitosa < fechaDeAlerta)
        {
            EnviarAlerta($"La cola no se ha vuelto a ejecutar desde: {BackgroundCola_II.UltimaEjecucionExitosa}");
        }
    }

    private void EnviarAlerta(string cuerpo)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var dbContextOptions = scope.ServiceProvider.GetRequiredService<DbContextOptions<ContextoSe>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        using var contexto = new ContextoSe(dbContextOptions, configuration, scope.ServiceProvider);

        var gestor = GestorDeCorreos.Gestor(contexto, mapper);

        var esperas = 0;
        while (contexto.Database.CurrentTransaction != null)
        {
            if (esperas > 10)
            {
                contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { "jjimenezcf@gmail.com" }
                    , "Fallo al testear si la cola está activa"
                    , $"Error en:{Environment.NewLine}" +
                      $"Servidor '{contexto.DatosDeConexion.Bd}'{Environment.NewLine}" +
                      $"El objeto contexto tiene una transacción activa"
                    );
                return;
            }
            Thread.Sleep(TimeSpan.FromSeconds(60));
            esperas++;
        }
        var asunto = contexto.DatosDeConexion.Bd + ": " + "Error al monotoriazar si la cola está activa";
        GestorDeCorreos.CrearCorreoPara(contexto, new List<string> { contexto.Administrador().eMail }, asunto,cuerpo, new List<TipoDtoElmento>(), new List<string>());
    }

}

