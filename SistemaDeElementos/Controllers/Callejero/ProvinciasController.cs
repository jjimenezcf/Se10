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
    public class ProvinciasController : EntidadController<ContextoSe, ProvinciaDtm, ProvinciaDto>
    {

        public ProvinciasController(GestorDeProvincias gestorDeProvincias, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeProvincias,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudProvincias()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeProvincias(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
