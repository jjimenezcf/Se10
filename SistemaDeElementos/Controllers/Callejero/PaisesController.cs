using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using GestoresDeNegocio.Callejero;
using ModeloDeDto.Callejero;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class PaisesController : EntidadController<ContextoSe, PaisDtm, PaisDto>
    {

        public PaisesController(GestorDePaises gestorDePaises, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePaises,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudPaises()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePais(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
