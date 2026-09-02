using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.MaestrosTecnico;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Callejero;
using ModeloDeDto.Callejero;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class MunicipiosController : EntidadController<ContextoSe, MunicipioDtm, MunicipioDto>
    {

        public MunicipiosController(GestorDeMunicipios gestorDeMunicipios, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeMunicipios,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudMunicipios()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeMunicipios(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Mun_ImportarCatalogo:
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
                if (!parametros.ContieneClave(nameof(ImportarMunicipiosDto.IdArchivo))) throw new Exception("Debe indicar el fichero del catálogo a importar");

                var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarMunicipiosDto.IdArchivo));
                var idProvincia = (int?)parametros.LeerValor<long?>(nameof(ImportarMunicipiosDto.IdProvincia), valorPorDefecto: (long?)null);

                var trabajo = TrabajosParaMaestros.SometerImportarMunicipios(Contexto, idArchivo, idProvincia);

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
