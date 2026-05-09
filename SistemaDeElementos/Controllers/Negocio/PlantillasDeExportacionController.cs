using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;
using System.Dynamic;
using ServicioDeDatos.Entorno;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlantillasDeExportacionController : EntidadController<ContextoSe, PlantillaDeExportacionDtm, PlantillaDeExportacionDto>
    {

        public PlantillasDeExportacionController(GestorDePlantillasDeExportacion gestorDePlantillasDeExportacion, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDePlantillasDeExportacion, 
          gestorDeErrores
        )
        {

        }

        public IActionResult CrudDePlantillasDeExportacion()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDePlantillasDeExportacion(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override dynamic ProcesarPeticion(enumNegocio negocio, VistaMvcDtm vista, string peticion, Dictionary<string, object> parametros)
        {
            switch (peticion)
            {
                case eventosDeMf.Comun_LeerDatosParaExportacion:
                    dynamic myObject = new ExpandoObject();
                    myObject.listaDeCgs = enumNegocio.Archivador.CentrosGestorsConAccesoDe(Contexto, ServicioDeDatos.Terceros.enumTipoPermiso.Gestor);
                    myObject.listaDePlantillas = Contexto.SeleccionarTodos<PlantillaDeExportacionDtm>(new Dictionary<string, object>
                    {
                        {nameof(PlantillaDeExportacionDtm.IdNegocio), negocio.IdNegocio() }
                    });

                    return myObject;
            }
            return base.ProcesarPeticion(negocio, vista, peticion, parametros);
        }


    }

}
