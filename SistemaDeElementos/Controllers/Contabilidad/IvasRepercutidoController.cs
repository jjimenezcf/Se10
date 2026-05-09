using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using ServicioDeDatos.Contabilidad;
using ModeloDeDto.Contabilidad;
using GestoresDeNegocio.Contabilidad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class IvasRepercutidoController : EntidadController<ContextoSe, IvaRepercutidoDtm, IvaRepercutidoDto>
    {
        public IvasRepercutidoController(GestorDeIvasRepercutido gestorDeIvasRepercutido, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeIvasRepercutido,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudIvasRepercutido()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeIvasRepercutido(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
