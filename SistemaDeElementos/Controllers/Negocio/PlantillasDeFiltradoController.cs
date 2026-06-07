using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlantillasDeFiltradoController : EntidadController<ContextoSe, PlantillaDeFiltradoDtm, PlantillaDeFiltradoDto>
    {

        public PlantillasDeFiltradoController(GestorDePlantillasDeFiltrado gestorDePlantillasDeFiltrado, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDePlantillasDeFiltrado, 
          gestorDeErrores
        )
        {

        }

        protected override dynamic ProcesarPeticion(enumNegocio negocio, VistaMvcDtm vista, string peticion, Dictionary<string, object> parametros)
        {
            switch (peticion)
            {
                case eventosDeMf.Comun_GuardarPlantillaFiltrado:
                    string urlNavegador = HttpContext.Request.Headers["Referer"].ToString(); 
                    if (string.IsNullOrEmpty(urlNavegador))
                    {
                        GestorDeErrores.Emitir("No se ha podido determinar la URL de origen desde el navegador.");
                    }
                    var uri = new Uri(urlNavegador);
                    var segmentos = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segmentos.Length < 2)
                    {
                        GestorDeErrores.Emitir("La URL no tiene el formato esperado para guardar la plantilla de filtrado.");
                    }
                    return GestorDePlantillasDeFiltrado.GuardarPlantillasDeFiltrado(Contexto, negocio, controlador: segmentos[0], accion: segmentos[1], parametros);
                case eventosDeMf.Comun_EliminarPlantillaFiltrado:
                    return Contexto.EliminarPorId<PlantillaDeFiltradoDtm>((int)parametros.LeerValor<long>(nameof(PlantillaDeFiltradoDtm.Id)));
            }
            var partes = peticion.Split(Simbolos.Subrrallado);
            if (partes.Length == 2 && partes[0] == nameof(PlantillaDeFiltradoDto.Plantilla))
            {
                return Contexto.SeleccionarPorId<PlantillaDeFiltradoDtm>(partes[1].Entero());
            }
            return base.ProcesarPeticion(negocio, vista, peticion, parametros);
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Comun_GuardarPlantillaFiltrado:
                    return null;
                case eventosDeMf.Comun_EliminarPlantillaFiltrado:
                    return Contexto.SeleccionarDtos<PlantillaDeFiltradoDto, PlantillaDeFiltradoDtm>(new Dictionary<string, object>
                    {
                        {nameof(PlantillaDeFiltradoDtm.IdNegocio), negocio.IdNegocio() },
                        {nameof(PlantillaDeFiltradoDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario },
                        {nameof(PlantillaDeFiltradoDtm.Vista), parametros.LeerValor<string>(nameof(PlantillaDeFiltradoDtm.Vista))}
                    });
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }
    }

}
