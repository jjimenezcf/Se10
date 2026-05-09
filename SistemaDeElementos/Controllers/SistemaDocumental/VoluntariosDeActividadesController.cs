using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto.SistemaDocumental;
using GestoresDeNegocio.SistemaDocumental;

namespace MVCSistemaDeElementos.Controllers
{
    public class VoluntarioDeActividadesController : EntidadController<ContextoSe, VoluntarioDeActividadDtm, VoluntarioDeActividadDto>
    {
        public VoluntarioDeActividadesController(GestorDeVoluntariosDeActividades gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeVoluntariosDeActividades(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearInscritoEnActividad);

        private ParametrosDeNegocio AntesDeEjecutar_CrearInscritoEnActividad(VoluntarioDeActividadDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeVoluntariosDeActividades(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<VoluntarioDeActividadDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<CircuitoDocDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.CircuitoDoc, Contexto, idTipo, typeof(VoluntarioDeActividadDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.CircuitoDoc, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.CircuitoDoc.Singular()}");

            var gestor = GestorDeVoluntariosDeActividades.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override VoluntarioDeActividadDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeVoluntariosDeActividades.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
