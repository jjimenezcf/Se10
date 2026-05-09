using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class VariablesController : EntidadController<ContextoSe, VariableDtm, VariableDto>
    {

        public VariablesController(GestorDeVariables gestorDeVariables, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeVariables,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudVariable()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeVariable(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
