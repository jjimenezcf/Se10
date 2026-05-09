using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;

namespace MVCSistemaDeElementos.Controllers
{
    public class MinutasController : EntidadController<ContextoSe, MinutaDtm, MinutaDto>
    {
        public MinutasController(GestorDeMinutas gestorDeMinutas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeMinutas,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeMinutas(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearMinuta);

        private ParametrosDeNegocio AntesDeEjecutar_CrearMinuta(MinutaDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeMinutas(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<MinutaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<PleitoDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(Utilidades.enumNegocio.Pleito, Contexto, idTipo, typeof(MinutaDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, Utilidades.enumNegocio.Pleito, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {Utilidades.enumNegocio.Pleito}");

            var gestor = GestorDeMinutas.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override MinutaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeMinutas.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id);
        }

    }
}
