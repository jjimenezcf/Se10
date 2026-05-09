using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Threading.Tasks;

namespace SistemaDeElementos.Extensions
{
    public class MobileMiddleware
    {
        private RequestDelegate Next { get; }

        public MobileMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await Next(context);

            if (context.Response.StatusCode == 404 && context.Request.Path.StartsWithSegments("/m") && !Path.HasExtension(context.Request.Path.Value))
            {
                context.Request.Path = "/m/index.html";
                await Next(context);
            }
        }
    }


    public static class MobileMiddlewareExtensions
    {
        public static IApplicationBuilder UseMobileMiddleware(this IApplicationBuilder builder, IWebHostEnvironment env)
        {
            builder.UseMiddleware<MobileMiddleware>();
            return builder.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Movil")),
                RequestPath = "/m",
                ServeUnknownFileTypes = true
            });
        }
    }
}
