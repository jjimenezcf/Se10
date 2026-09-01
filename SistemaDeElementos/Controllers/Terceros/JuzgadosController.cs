using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.MaestrosTecnico;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class JuzgadosController : EntidadController<ContextoSe, JuzgadoDtm, JuzgadoDto>
    {
        public JuzgadosController(GestorDeJuzgados gestorDeJuzgados, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeJuzgados,
           gestorDeErrores
         )
        {
            if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto))
                return;

            if (!ExtensorDePleitos.ModuloActivo(Contexto))
                Emitir(ltrDePleitos.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudJuzgados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeJuzgados(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Juz_ImportarCatalogo:
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
                if (!parametros.ContieneClave(nameof(ImportarJuzgados.IdArchivo))) throw new Exception("Debe indicar el fichero del catálogo a importar");

                var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarJuzgados.IdArchivo));
                var idProvincia = (int?)parametros.LeerValor<long?>(nameof(ImportarJuzgados.IdProvincia), valorPorDefecto: (long?)null);

                var trabajo = TrabajosParaMaestros.SometerImportarJuzgados(Contexto, idArchivo, idProvincia);

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
