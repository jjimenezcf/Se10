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
    public class IrpfsController : EntidadController<ContextoSe, IrpfDtm, IrpfDto>
    {
        public IrpfsController(GestorDeIrpfs gestorDeIrpfs, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeIrpfs,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudIrpfs()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeIrpfs(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
