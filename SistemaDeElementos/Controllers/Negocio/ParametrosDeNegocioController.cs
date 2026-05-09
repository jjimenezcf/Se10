using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class ParametrosDeNegocioController : EntidadController<ContextoSe, ParametroDeNegocioDtm, ParametroDeNegocioDto>
    {

        public ParametrosDeNegocioController(GestorDeParametrosDeNegocio gestorDeParametrosDeNegocio, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDeParametrosDeNegocio, 
          gestorDeErrores
        )
        {

        }

        
        public IActionResult CrudDeParametrosDeNegocio()
        {            
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeParametrosDeNegocio(Contexto, ModoDescriptor.Mantenimiento));
        }


    }

}
