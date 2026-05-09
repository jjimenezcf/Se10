using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.ColaDeTrabajosSometidos;

public class BackgroundCola_II : BackgroundService
{
    private readonly ILogger<BackgroundCola_II> _logger;
    private IServiceScopeFactory _serviceScopeFactory;

    public static long VecesEjecutada { get; set; } = 0;

    public UsuarioDtm Usuario { get; private set; }
    private static TimeSpan _period => TimeSpan.FromMinutes(CacheDeVariable.Cola_Tiempo_De_Espera);
    public static DateTime UltimaEjecucionExitosa => CacheDeVariable.Cola_Ultima_Ejecucion;

    public BackgroundCola_II(ILogger<BackgroundCola_II> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        ObtenerUsuarioEjecutor();
    }

    public void ObtenerUsuarioEjecutor()
    {
        var scope = _serviceScopeFactory.CreateScope();
        using var gestor = scope.ServiceProvider.GetRequiredService<GestorDeUsuarios>();
        Usuario = gestor.LeerRegistroCacheado(nameof(UsuarioDtm.Login), CacheDeVariable.Cola_LoginDeEjecutor, true, true, false);
    }

    public void EnviarCorreo(string asunto, string cuerpo)
    {
        var scope = _serviceScopeFactory.CreateScope();
        using var gestor = scope.ServiceProvider.GetRequiredService<GestorDeCorreos>();
        asunto = gestor.Contexto.DatosDeConexion.Bd +": " + asunto;
        GestorDeCorreos.CrearCorreoPara(gestor.Contexto, new List<string> { gestor.Contexto.Administrador().eMail }, asunto, cuerpo, new List<TipoDtoElmento> (), new List<string>());
    }


    public ContextoSe Contexto()
    {
        var scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ContextoSe>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var dbContextOptions = scope.ServiceProvider.GetRequiredService<DbContextOptions<ContextoSe>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        using var contexto = new ContextoSe(dbContextOptions, configuration, scope.ServiceProvider);
        contexto.Mapeador = mapper;
        contexto.IniciarTraza("Ejecución de cola", debugar: true);
        using PeriodicTimer periodicTimer = new(_period);
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(contexto, enumNegocio.Variable.IdNegocio(), 0, enumOpercionesDeSemaforo.EJEC, "").Id;

            try
            {
                await EjecutarTrabajoPendiente(contexto, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                EnviarCorreo("Fallo al ejecutar la cola", "Error al ejecutar la cola" + Environment.NewLine + GestorDeErrores.Detalle(e));
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(contexto, idSemaforo);
            }
        }
    }

    private async Task EjecutarTrabajoPendiente(ContextoSe contexto, CancellationToken stoppingToken)
    {
        var gestor = GestorDeTrabajosDeUsuario.GestorTu(contexto);

        if (CacheDeVariable.Cola_Activa && !System.Diagnostics.Debugger.IsAttached)
        {
            try
            {
                contexto.AnotarTraza("Numero de ejecuciones", $"se ha ejecutado: {VecesEjecutada++}");
                contexto.TrabajoSometido = true;
                await gestor.ProcesarCola(Usuario);
                CacheDeVariable.ResetearVariable(Variable.Cola_Ultima_Ejecucion, Descripciones.Cola_Ultima_Ejecucion, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            }
            finally
            {
                contexto.TrabajoSometido = false;
                contexto.CerrarTraza("Fin de ejecución");
            }
        }
        else
        {
            contexto.CerrarTraza("Cola no activa");
        }
    }


}
