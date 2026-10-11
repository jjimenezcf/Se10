using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using GestoresDeNegocio.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using GestorDeElementos;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Seguridad;

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

        // Envía al correo indicado el asunto y el cuerpo con un enlace de descarga con guid por cada archivo marcado del elemento
        public JsonResult epEnviarCorreoExterno(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var negocio = NegociosDeSe.ToEnumerado((int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio));
                var idElemento = (int)parametros.LeerValor<long>(ltrParametrosEp.idElemento);

                GestorDeCorreos.EnviarCorreoExterno(Contexto, negocio, idElemento
                    , parametros.LeerValor<string>(nameof(EnviarCorreoExternoDto.Para), null)
                    , parametros.LeerValor<string>(nameof(EnviarCorreoExternoDto.Asunto), null)
                    , parametros.LeerValor<string>(nameof(EnviarCorreoExternoDto.Cuerpo), null)
                    , parametros.LeerValor(ltrParametrosEp.ids, new List<int>()));

                r.Mensaje = "Correo encolado para su envío";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al enviar el correo.");
            }
            return new JsonResult(r);
        }


    }

}
