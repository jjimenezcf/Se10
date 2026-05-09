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
    public class NaturalezasController : EntidadController<ContextoSe, NaturalezaDtm, NaturalezaDto>
    {
        public NaturalezasController(GestorDeNaturalezas gestorDeNaturalezas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeNaturalezas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudNaturalezas()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeNaturalezas(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
    }
}
