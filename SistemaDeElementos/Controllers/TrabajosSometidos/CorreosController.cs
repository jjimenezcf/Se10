using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using GestoresDeNegocio.TrabajosSometidos;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CorreosController : EntidadController<ContextoSe, CorreoDtm, CorreoDto>
    {

        public CorreosController(GestorDeCorreos gestorDeCorreos, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDeCorreos, 
          gestorDeErrores
        )
        {

        }
                
        public IActionResult CrudDeCorreos()
        { 
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeCorreos(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            var ids = parametros.LeerValor(ltrParametrosEp.ids, (List<int>)null);
            switch (opcion)
            {
                case eventosDeMf.Correo_Enviar:
                    ((GestorDeCorreos)_GestorDeElementos).EnviarCorreos(ids);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }


    }

}
