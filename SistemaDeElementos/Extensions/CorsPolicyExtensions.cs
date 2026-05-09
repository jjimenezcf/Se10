using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace SistemaDeElementos.Extensions;



public static class CorsPolicyExtensions
{
    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuracion)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Dev", builder =>
            {
                builder
                    .WithOrigins(
                        configuracion["App:CorsOrigins"]
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .ToArray()
                    )
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    //services.AddCors(options =>
    //    options.AddPolicy("Dev", builder =>
    //    {
    //        // Allow multiple methods
    //        builder.WithMethods("GET", "POST", "PATCH", "DELETE", "OPTIONS")
    //            .WithHeaders(
    //                HeaderNames.Accept,
    //                HeaderNames.ContentType,
    //                HeaderNames.Authorization)
    //            .AllowCredentials()
    //            .SetIsOriginAllowed(origin => !string.IsNullOrWhiteSpace(origin) &&
    //                                          // Only add this to allow testing with localhost, remove this line in production!
    //                                          origin.ToLower().StartsWith("http://localhost"));
    //    })
    //);

    //services.AddCors(options =>
    //{
    //    options.AddPolicy("Dev", builder =>
    //    {
    //        builder
    //            //.WithOrigins("http://localhost:3000,http://10.70.1.75:3000")
    //            .WithOrigins(
    //                Configuracion["App:CorsOrigins"]
    //                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
    //                    .ToArray()
    //            )
    //            .SetIsOriginAllowedToAllowWildcardSubdomains()
    //            .AllowAnyMethod()
    //            .AllowAnyHeader()
    //            .AllowCredentials();
    //        //.SetIsOriginAllowed(origin =>
    //        //{
    //        //    if (string.IsNullOrWhiteSpace(origin)) return false;
    //        //    // Only add this to allow testing with localhost, remove this line in production!
    //        //    if (origin.ToLower().StartsWith("http://localhost")) return true;
    //        //    // Insert your production domain here.
    //        //    if (origin.ToLower().StartsWith("http://10.70.1.75")) return true;
    //        //    return false;
    //        //});
    //        //for when you're running on localhost
    //        //builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
    //        //    .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    //    });
    //});
}
