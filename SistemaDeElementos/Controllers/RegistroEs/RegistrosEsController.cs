using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto.SistemaDocumental;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.RegistroEs;
using ModeloDeDto.RegistroEs;
using ServicioDeDatos.RegistroEs;
using System;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class RegistrosEsController : EntidadController<ContextoSe, RegistroEsDtm, RegistroEsDto>
    {
        public RegistrosEsController(GestorDeRegistrosEs gestorDeRegistrosEs, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeRegistrosEs,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudRegistrosEs()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeRegistrosEs(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
