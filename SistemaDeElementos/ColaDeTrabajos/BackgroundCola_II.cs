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
        using PeriodicTimer periodicTimer = new(_period);

        bool trazar = CacheDeVariable.Cola_Trazar;
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            // Cola_Activa ya contempla !Debugger.IsAttached: se comprueba antes de tocar
            // traza/semáforo para que una instancia en depuración (o con la cola desactivada
            // desde BD) ni siquiera intente poner el semáforo, y así no compita con la
            // instancia productiva por el mismo semáforo global. El bucle sigue vivo (continue,
            // no se rompe el while), así que si se reactiva la cola en caliente se retoma sola.
            if (!CacheDeVariable.Cola_Activa)
                continue;

            var idSemaforo = 0;
            try
            {
                // IniciarTraza y PonerSemaforo van dentro del try: ambas hacen I/O (fichero de
                // traza / fila de semáforo en BD) y pueden lanzar excepción. Fuera del try, esa
                // excepción se escaparía de ExecuteAsync sin capturar y, al estar el host
                // configurado con BackgroundServiceExceptionBehavior=StopHost, tumbaría toda la
                // aplicación en vez de limitarse a saltar este ciclo.
                contexto.IniciarTraza(Literal.TrabajosSometidos.NombreFicheroDebug, debugar: trazar);
                idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(contexto, enumNegocio.Variable.IdNegocio(), 0, enumOpercionesDeSemaforo.EJEC, "").Id;
                await EjecutarTrabajoPendiente(contexto, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                contexto.AnotarExcepcion(e);
                try
                {
                    // si el propio aviso por correo falla (p.ej. error de BD al persistirlo),
                    // que no se escape: ya se ha anotado la excepción original en la traza, y
                    // dejar que esta se propague tumbaría el host igual que lo hacía el fallo
                    // original antes de este arreglo.
                    EnviarCorreo("Fallo al ejecutar la cola", "Error al ejecutar la cola" + Environment.NewLine + GestorDeErrores.Detalle(e));
                }
                catch (Exception eCorreo)
                {
                    contexto.AnotarExcepcion(eCorreo);
                }
            }
            finally
            {
                // idSemaforo sigue a 0 si PonerSemaforo falló (QuitarSemaforo ya ignora id=0),
                // así que el semáforo solo existe mientras la cola se ejecuta de verdad, tal
                // como lo necesita 01-Descomprimir y parar servicios.cmd para decidir si puede
                // parar el IIS durante un despliegue.
                SemaforoDeProcesoSql.QuitarSemaforo(contexto, idSemaforo);
                contexto.CerrarTraza();
            }
        }
    }

    private async Task EjecutarTrabajoPendiente(ContextoSe contexto, CancellationToken stoppingToken)
    {
        var gestor = GestorDeTrabajosDeUsuario.GestorTu(contexto);

        // Cola_Activa ya se comprobó en ExecuteAsync antes de llegar aquí (y antes de poner el
        // semáforo), así que si se ha llegado a este punto la cola está activa de verdad.
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
            contexto.AnotarTraza("Fin de ejecuci�n", "Ejecuci�n de cola finalizada");
        }
    }


}
