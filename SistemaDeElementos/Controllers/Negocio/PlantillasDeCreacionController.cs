using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Newtonsoft.Json.Linq;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.Entorno;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlantillasDeCreacionController : EntidadController<ContextoSe, PlantillaDeCreacionDtm, PlantillaDeCreacionDto>
    {

        public PlantillasDeCreacionController(GestorDePlantillasDeCreacion gestorDePlantillasDeCreacion, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDePlantillasDeCreacion, 
          gestorDeErrores
        )
        {

        }

        protected override dynamic ProcesarPeticion(enumNegocio negocio, VistaMvcDtm vista, string peticion, Dictionary<string, object> parametros)
        {
            switch (peticion)
            {
                case eventosDeMf.Comun_GuardarDatosCreacion:
                    JObject datosPorDefecto = parametros.LeerValor<JObject>(ltrParametrosEp.datosPeticion);
                    negocio.ResetearParametroDeUsuario(Contexto, enumParametrosDeUsuario.USU_Valores_Por_Defecto, datosPorDefecto.ToString());
                    return null;
                case eventosDeMf.Comun_GuardarPlantillaCreacion:
                    return GestorDePlantillasDeCreacion.GuardarPlantillasDeCreacion(Contexto, negocio, parametros);
                case eventosDeMf.Comun_EliminarPlantillaCreacion:
                    {
                        var idPlantilla = (int)parametros.LeerValor<long>(nameof(PlantillaDeCreacionDtm.Id));
                        var remplazar = parametros.LeerValor<bool>(nameof(EliminarPlantillaDto.Remplazar));
                        if (remplazar)
                        {
                            return GestorDePlantillasDeCreacion.GuardarPlantillasDeCreacion(Contexto, negocio, parametros);
                        }
                        return Contexto.EliminarPorId<PlantillaDeCreacionDtm>(idPlantilla);
                    }
            }
            var partes = peticion.Split(Simbolos.Subrrallado);
            if (partes.Length == 2 && partes[0] == nameof(PlantillaDeCreacionDto.Plantilla))
            {
                return DatosParaInicializarLaCreacion(negocio, parametros, new DatosDeCreacion(Contexto, idPantillaDeCreacion: partes[1].Entero()));
            }
            return base.ProcesarPeticion(negocio, vista, peticion, parametros);
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Comun_GuardarPlantillaCreacion:
                    return null;
                case eventosDeMf.Comun_EliminarPlantillaCreacion:
                    return Contexto.SeleccionarDtos<PlantillaDeCreacionDto, PlantillaDeCreacionDtm>(new Dictionary<string, object>
                    {
                        {nameof(PlantillaDeCreacionDtm.IdNegocio), negocio.IdNegocio() },
                        {nameof(PlantillaDeCreacionDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario },
                        {nameof(PlantillaDeCreacionDtm.Vista), parametros.LeerValor<string>(nameof(PlantillaDeCreacionDtm.Vista))}
                    });
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

    }

}
