using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaDeElementos.Extensions;

public static class ServiceExtensions
{
    public static void AddCookieAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "AutenticacionSE";
                options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tiempo de expiración en 8 horas
                options.SlidingExpiration = true; // Habilitar el tiempo de expiración dinámico
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Headers.Origin.Any() || context.Request.Path.StartsWithSegments("/movil"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                        //context.Response.Headers["Location"] = context.RedirectUri;
                    }

                    return Task.CompletedTask;
                };
                options.Cookie.HttpOnly = true;
                // Only use this when the sites are on different domains
                options.Cookie.SameSite = SameSiteMode.Lax;
                //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/Acceso/Conectar.html";
                options.AccessDeniedPath = "/Acceso/Denegado.html";
            });
    }
    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(o =>
        {
            o.AddPolicy("SoloAdmin", policy =>
            {
                policy.RequireClaim(ClaimTypes.Role, "admin");
            });
        });
    }
}