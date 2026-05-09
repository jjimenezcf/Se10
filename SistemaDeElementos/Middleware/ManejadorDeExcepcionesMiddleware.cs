using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace SistemaDeElementos.Middleware
{
    public class ManejadorDeExcepcionesMiddleware
    {
        private readonly RequestDelegate _next;

        public ManejadorDeExcepcionesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await ManejadorDeExcepcionAsync(context, ex);
            }
        }

        private static Task ManejadorDeExcepcionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = MimeTypeMap.ApplicationJson;
            context.Response.StatusCode = exception.Data.Contains(Datos.CodigoError)
            ? (int)(enumCodigoDeError)exception.Data[Datos.CodigoError]
            : (int)HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new { error = exception.Message });
            return context.Response.WriteAsync(result);
        }
    }
}
