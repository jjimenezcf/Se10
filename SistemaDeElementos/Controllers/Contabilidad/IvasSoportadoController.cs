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
    public class IvasSoportadoController : EntidadController<ContextoSe, IvaSoportadoDtm, IvaSoportadoDto>
    {
        public IvasSoportadoController(GestorDeIvasSoportado gestorDeIvasSoportado, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeIvasSoportado,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudIvasSoportado()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeIvasSoportado(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
