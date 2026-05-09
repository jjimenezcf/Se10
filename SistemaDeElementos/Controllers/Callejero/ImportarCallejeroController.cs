using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Callejero;
using AutoMapper;
using System;

namespace MVCSistemaDeElementos.Controllers
{
    public class ImportarCallejeroController : FormularioController<ContextoSe>
    {

        public ImportarCallejeroController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
         : base(contexto
               , mapeador
               , gestorDeErrores)
        {
        }


        public IActionResult ImportarCallejero()
        {
            return ViewFormulario(new DescriptorImportarCallejero(Contexto));
        }

        // END-POIN: desde el Callejero.ImportarCallejero.ts. somete la importación de ficheros csv
        public JsonResult epImportarCallejero(string parametros)
        {
            var r = new Resultado();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                GestorDePaises.SometerImportarCallejero(Contexto, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Trabajo de importación sometido correctamente";
            }
            catch(Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = e.Message;
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                    r.Mensaje = e.Message;
                else
                    r.Mensaje = "Error al someter el trabajo de importación del callejero";
            }

            return new JsonResult(r);
        }

    }
}
