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
    public class InscritosEnActividadesController : EntidadController<ContextoSe, InscritosEnActividadDtm, InscritosEnActividadDto>
    {
        public InscritosEnActividadesController(GestorDeInscritosEnActividades gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeInscritosEnActividades(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearInscritoEnActividad);

        private ParametrosDeNegocio AntesDeEjecutar_CrearInscritoEnActividad(InscritosEnActividadDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeInscritosEnActividades(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<InscritosEnActividadDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<CircuitoDocDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.CircuitoDoc, Contexto, idTipo, typeof(InscritosEnActividadDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.CircuitoDoc, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.CircuitoDoc.Singular()}");

            var gestor = GestorDeInscritosEnActividades.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override InscritosEnActividadDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeInscritosEnActividades.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
