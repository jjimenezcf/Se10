using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Negocio;
using ModeloDeDto.Negocio;
using GestoresDeNegocio.Negocio;
using System.Collections.Generic;

namespace MVCSistemaDeElementos.Controllers
{
    public class AccionesDeNegocioController : EntidadController<ContextoSe, AccionDeNegocioDtm, AccionDeNegocioDto>
    {

        public AccionesDeNegocioController(GestorDeAccionesDeNegocio gestorDeAcciones, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAcciones,
           gestorDeErrores
         )
        {
        }

        protected override IEnumerable<AccionDeNegocioDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            return base.LeerElementos(posicion, cantidad, filtros, orden, parametros);
        }
    }
}
