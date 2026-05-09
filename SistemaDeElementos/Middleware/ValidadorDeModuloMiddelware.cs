using Gestor.Errores;
using GestorDeElementos.Extensores;
using Microsoft.AspNetCore.Http;
using MVCSistemaDeElementos.Controllers;
using System;
using System.Threading.Tasks;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace SistemaDeElementos.Middleware
{
    public class ValidadorDeModuloMiddelware
    {
        private readonly RequestDelegate _next;

        public ValidadorDeModuloMiddelware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsHead(context.Request.Method))
            {
                var path = context.Request.Path.Value;

                var partesDeLaRuta = path.Split(Simbolos.SeparadorDeRuta, StringSplitOptions.RemoveEmptyEntries);

                if (partesDeLaRuta.Length > 0)
                {
                    var controlador = partesDeLaRuta[0] + ltrEndPoint.Controller;

                    var activo = ValidarQueElModuloEstaActivo(controlador);
                    context.Response.StatusCode = !activo ? (int)enumCodigoDeError.ModuloNoActivo: StatusCodes.Status200OK;
                    return;
                }
            }

            // Si no es una solicitud HEAD, pasa al siguiente middleware
            await _next(context);
        }

        private static bool ValidarQueElModuloEstaActivo(string controlador)
        {
            if (controlador == nameof(InfantesController) || controlador == nameof(AulasDeGuarderiaController) ||
                controlador == nameof(CursosDeGuarderiaController) || controlador == nameof(InfantesDeUnCursoController) ||
                controlador == nameof(ProfesDeCursoDeGuarderiaController))
            {
                var a = Task.Run(ExtensorDeGuarderias.ModuloActivoAsync).Result;
                if (!a)
                {
                    return false;
                }
            }
            else
            if (controlador == nameof(ProcuradoresController) || controlador == nameof(AbogadosController) ||
                controlador == nameof(JuzgadosController) || controlador == nameof(PleitosController) ||
                controlador == nameof(ClasesDeJuzgadoController))
            {
                var a = Task.Run(ExtensorDePleitos.ModuloActivoAsync).Result;
                if (!a)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
