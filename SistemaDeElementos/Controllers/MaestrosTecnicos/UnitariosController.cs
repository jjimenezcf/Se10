using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
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

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Uni_ImportarCatalogo:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epImportarCatalogo(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(ImportarCatalogoDeUnitariosDto.IdArchivo))) throw new Exception("Debe indicar el fichero del catálogo a importar");

                var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarCatalogoDeUnitariosDto.IdArchivo));

                var trabajo = TrabajosParaMaestros.SometerImportarCatalogoDeUnitarios(Contexto, idArchivo);

                r.Consola = "Se ha sometido la importación del catálogo, se le notificará cuando finalice";
                r.Datos = trabajo;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al someter la importación del catálogo.");
            }
            return new JsonResult(r);
        }
    }
}
