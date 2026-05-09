using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Juridico;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Juridico;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using System;
using System.Collections.Generic;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class PleitosController : EntidadController<ContextoSe, PleitoDtm, PleitoDto>
    {
        public PleitosController(GestorDePleitos gestorDePleitos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePleitos,
           gestorDeErrores
         )
        {
            if (!ExtensorDePleitos.ModuloActivo(Contexto))
                Emitir(ltrDePleitos.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudPleitos()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDePleitos(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Plt_VincularRegistroEntrada:
                    return true;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

    }
}
