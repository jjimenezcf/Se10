using AutoMapper;
using Gestor.Errores;
using GestoresDeNegocio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCSistemaDeElementos.ColaDeTrabajosSometidos;
using MVCSistemaDeElementos.Controllers;
using QuestPDF.Infrastructure;
using ServicioDeCorreos;
using ServicioDeDatos;
using SistemaDeElementos.Extensions;
using SistemaDeElementos.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Utilidades;
using WebOptimizer.Processors;
using static GestoresDeNegocio.Tarea.GestorDeTareas;

namespace MVCSistemaDeElementos;

public class Startup
{
    public IConfiguration Configuracion { get; }

    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuracion, IWebHostEnvironment env)
    {
        Configuracion = configuracion;
        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = _ => false;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddRazorPages().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        var cadenaDeConexion = Configuracion.GetConnectionString(Literal.CadenaDeConexion);
        services.AddDbContext<ContextoSe>((serviceProvider, options) =>
        {
            options.UseSqlServer(cadenaDeConexion);
        }, ServiceLifetime.Scoped);

        services.AddSingleton<IConfiguration>(Configuracion);

        services.AddCookieAuthentication();
        services.AddAuthorizationPolicies();

        services.ConfigureErrors();
        services.ConfigureGestoresDeNegocio();
        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });


        services.AddHttpClient<IPdfServerClient, PdfServerApiClient>();

        services.AddHttpClient<IGraphClient, HttpGraphClient>();
        ContextoSe.IncluirServiciosParaElCorreo(services);

        services.AddAutoMapper(cfg =>
        {
            // Forzar la carga de todos los ensamblados referenciados
            var ensamblados = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a => a.GetTypes()
                    .Any(t => t.IsClass && !t.IsAbstract && typeof(Profile).IsAssignableFrom(t)))
                .ToArray();

            foreach (var ensamblado in ensamblados)
                cfg.AddMaps(ensamblado);
        });


        services.AddCorsPolicy(Configuracion);
        services.AddControllersWithViews().AddRazorRuntimeCompilation();
        services.AddHostedService<BackgroundCola_II>();
        //services.AddHostedService<MonitorBackgroundCola>();

        services.AddHttpContextAccessor(); // Esto es opcional en versiones recientes de ASP.NET Core

        // Otros servicios...
        services.AddControllersWithViews(); // o services.AddControllers();

        if (bool.TryParse(Configuracion[ltrAppSetting.UsarBundle], out bool usarBundle) && usarBundle)
        {
            services.AddWebOptimizer(pipeline =>
            {
                var rutaJs = Path.Combine(_env.WebRootPath, "js", Path.DirectorySeparatorChar.ToString());
                var sourceFiles = new[]
                {
                    $"{rutaJs}_Literales{Path.DirectorySeparatorChar}Literales.js",
                    $"{rutaJs}Mensajes.js",
                    $"{rutaJs}Diccionario.js",
                    $"{rutaJs}_TiposDeDatos{Path.DirectorySeparatorChar}TiposDeDatos.js",
                    $"{rutaJs}_TiposDeDatos{Path.DirectorySeparatorChar}ModoAcceso.js",
                    $"{rutaJs}Utilidades.js",
                    $"{rutaJs}InfoSelector.js",
                    $"{rutaJs}_Api{System.IO.Path.DirectorySeparatorChar}ApiLocalStorage.js",
                    $"{rutaJs}Registro.js",
                    $"{rutaJs}Cookies.js",
                    $"{rutaJs}HistorialDeNavegacion.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeAjax.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeAgenda.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeCertificados.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDePassword.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeInicializacion.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeArchivos.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDragAndDrop.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeListasDinamicas.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDePeticiones.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeMapeos.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeExpansor.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeGrid.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeControles.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiPanel.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeMenuFlotante.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeJerarquia.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}MapearAlPanel.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}MapearAlControl.js",
                    $"{rutaJs}_Api{Path.DirectorySeparatorChar}ApiDeDireccion.js",
                    $"{rutaJs}ArbolDeMenu.js",
                    $"{rutaJs}EntornoSe.js",
                    $"{rutaJs}_Formulario{Path.DirectorySeparatorChar}FormularioBase.js",
                    $"{rutaJs}_Formulario{Path.DirectorySeparatorChar}Jerarquia.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ApiDelCrud.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}CrudBase.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}GridDeDatos.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}CrudCreacion.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}CrudEdicion.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}CrudHistorial.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ModalConGrid.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ModalParaRelacionar.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ModalParaImputar.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ModalParaConsultarRelaciones.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ModalParaSeleccionar.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}ModalDeSeleccion.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}CrudMantenimiento.js",
                    $"{rutaJs}_Crud{Path.DirectorySeparatorChar}GestorDeEventos.js"
                 };
                var bundle = $"{rutaJs}bundle.js";
                pipeline.AddJavaScriptBundle(bundle, new JsSettings(), sourceFiles);
                GenerarBundleManualmente(_env, bundle, sourceFiles);
            });
        }

    }


    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseCors("Dev");


        app.UseHttpsRedirection();

        if (bool.TryParse(Configuracion[ltrAppSetting.UsarBundle], out bool usarBundle) && usarBundle)
        {
            app.UseWebOptimizer();
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCookiePolicy();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMobileMiddleware(env);
        app.UseMiddleware<ValidadorDeModuloMiddelware>();
        app.UseMiddleware<EliminarFicherosMiddelware>();
        //app.UseMiddleware<ManejadorDeExcepcionesMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        });

    }
    private void GenerarBundleManualmente(IWebHostEnvironment env, string route, string[] sourceFiles)
    {
        try
        {
            var rutaBase = Path.Combine(env.WebRootPath, "js");
            var bundlePath = Path.Combine(rutaBase, "bundle.js");
            
            if (File.Exists(bundlePath))
            {
                File.Delete(bundlePath);
            }
            using (var writer = new StreamWriter(bundlePath))
            {
                writer.WriteLine($"// Generado el: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

                foreach (var file in sourceFiles)
                {
                    var fichero = file.Replace(@"~\js", "");
                    var filePath = rutaBase + fichero;
                    if (File.Exists(filePath))
                    {
                        writer.WriteLine($"// ***************************** {fichero} *************************************");
                        var content = File.ReadAllText(filePath);
                        writer.WriteLine(content);
                        writer.WriteLine("// *******************************************************************************");
                    }
                    else
                    {
                        throw new Exception($"No existe el fichero {filePath}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar el bundle manualmente: {ex.Message}");
        }
    }



}