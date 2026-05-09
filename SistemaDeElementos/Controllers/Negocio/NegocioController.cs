using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using GestorDeElementos;
using System;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class NegocioController : EntidadController<ContextoSe, NegocioDtm, NegocioDto>
    {

        public NegocioController(GestorDeNegocios gestorDeNegocios, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDeNegocios, 
          gestorDeErrores
        )
        {

        }

        
        public IActionResult CrudDeNegocios()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeNegocio(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_CrearElemento(NegocioDto elemento)
        {
            throw new Exception("No se pueden crear negocios desde la aplicación");
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_BorrarPorId(NegocioDto elemento)
        {
            throw new Exception("No se pueden borrar negocios desde la aplicación");
        }


    }

}
