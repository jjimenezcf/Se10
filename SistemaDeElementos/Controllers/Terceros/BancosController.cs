using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestoresDeNegocio.Terceros;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class BancosController : EntidadController<ContextoSe, BancoDtm, BancoDto>
    {

        public BancosController(GestorDeBancos gestorDeBancos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeBancos,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudBancos()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeBancos(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
