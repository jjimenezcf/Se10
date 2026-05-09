using Microsoft.Extensions.DependencyInjection;

namespace Gestor.Errores;

public static class ServiceExtensions
{
    public static void ConfigureErrors(this IServiceCollection services)
    {
        services.AddScoped<GestorDeErrores>();

    }
}