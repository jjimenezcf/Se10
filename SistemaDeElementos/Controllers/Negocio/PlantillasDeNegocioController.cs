using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using ServicioDeReportes.Base;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlantillasDeNegocioController : EntidadController<ContextoSe, PlantillaDeNegocioDtm, PlantillaDeNegocioDto>
    {

        public PlantillasDeNegocioController(GestorDePlantillasDeNegocio gestorDePlantillasDeNegocio, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDePlantillasDeNegocio, 
          gestorDeErrores
        )
        {

        }

        public FileStreamResult epDescargarPlantilla(int idArchivo)
        {
            return ApiController.DescargarArchivo(Contexto, idArchivo, enumRutas.RutaDePlantillas, usarCacheado: false);
        }

        public FileStreamResult epDescargarEtiquetas(int idNegocio)
        {
            var ruta = ApiDeEtiquetas.CrearFicheroDeEtiquetas(NegociosDeSe.TipoDtm(NegociosDeSe.ToEnumerado(idNegocio)));
            return DevolverStream(ruta);
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDePlantillasDeNegocio(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearElemento);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDePlantillasDeNegocio(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

    }

}
