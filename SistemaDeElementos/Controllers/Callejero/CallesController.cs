using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using GestoresDeNegocio.Callejero;
using ModeloDeDto.Callejero;
using Utilidades;
using ModeloDeDto.Gastos;
using System;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class CallesController : EntidadController<ContextoSe, CalleDtm, CalleDto>
    {

        public CallesController(GestorDeCalles gestorDeCalles, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCalles,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudCalles()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeCalles).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Callejero}/{nameof(CrudCalles)}";
                    return base.View(destino, new DescriptorDeCalles(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CalleDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeCalles(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

    }
}
