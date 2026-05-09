using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.MaestrosTecnico;
using GestoresDeNegocio.MaestrosTecnico;

namespace MVCSistemaDeElementos.Controllers
{
    public class UnidadesController : EntidadController<ContextoSe, UnidadDtm, UnidadDto>
    {
        public UnidadesController(GestorDeUnidades gestorDeUnidades, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeUnidades,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudUnidades()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeUnidades(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
