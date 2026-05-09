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
    public class MunicipiosController : EntidadController<ContextoSe, MunicipioDtm, MunicipioDto>
    {

        public MunicipiosController(GestorDeMunicipios gestorDeMunicipios, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeMunicipios,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudMunicipios()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeMunicipios(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
