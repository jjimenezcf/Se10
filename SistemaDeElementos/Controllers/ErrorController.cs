using System;
using AutoMapper;
using Gestor.Errores;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;

namespace SistemaDeElementos.Controllers
{
    public class ErrorController : HomeController
    {
        public ErrorController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores) :
        base(contexto, mapeador, gestorDeErrores)
        {
        }
        public new IActionResult Index()
        {
            var pathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            Exception exception = pathFeature?.Error; // Here will be the exception details.


            return RenderMensaje(GestorDeErrores.Detalle(exception));
        }
    }
}
