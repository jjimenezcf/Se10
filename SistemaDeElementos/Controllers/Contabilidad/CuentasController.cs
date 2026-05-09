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
    public class CuentasController : EntidadController<ContextoSe, CuentaDtm, CuentaDto>
    {
        public CuentasController(GestorDeCuentas gestorDeCuentas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCuentas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudCuentas()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeCuentas(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
