using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.Expediente;
using ModeloDeDto.Expediente;
using GestoresDeNegocio.Expediente;

namespace MVCSistemaDeElementos.Controllers
{
    public class ApuntesDeExpedienteController : EntidadController<ContextoSe, ApunteDeExpedienteDtm, ApunteDeExpedienteDto>
    {
        public ApuntesDeExpedienteController(GestorDeApuntesDeExpediente gestorDeApunteDeExpediente, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeApunteDeExpediente,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeApuntesDeExpediente(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearApunteDeExpediente);

        private ParametrosDeNegocio AntesDeEjecutar_CrearApunteDeExpediente(ApunteDeExpedienteDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeApuntesDeExpediente(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<ApunteDeExpedienteDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<ExpedienteDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Expediente, Contexto, idTipo, typeof(ApunteDeExpedienteDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Expediente, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Expediente.Singular()}");

            var gestor = GestorDeApuntesDeExpediente.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override ApunteDeExpedienteDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeApuntesDeExpediente.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
