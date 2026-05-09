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
    public class UnitariosController : EntidadController<ContextoSe, UnitarioDtm, UnitarioDto>
    {
        public UnitariosController(GestorDeUnitarios gestorDeUnitarios, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeUnitarios,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudUnitarios()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeUnitarios(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
