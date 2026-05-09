using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestoresDeNegocio.Terceros;

namespace MVCSistemaDeElementos.Controllers
{
    public class CuentasDeInterlocutorController : EntidadController<ContextoSe, CuentaDeInterlocutorDtm, CuentaDeInterlocutorDto>
    {
        public CuentasDeInterlocutorController(GestorDeCuentasDeInterlocutor gestorDeCuentasDeInterlocutor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCuentasDeInterlocutor,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeCuentasDeInterlocutor(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearCuentaDeInterlocutor);

        private ParametrosDeNegocio AntesDeEjecutar_CrearCuentaDeInterlocutor(CuentaDeInterlocutorDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeCuentasDeInterlocutor(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<CuentaDeInterlocutorDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Interlocutor, typeof(CuentaDeInterlocutorDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Interlocutor, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Interlocutor.Singular()}");

            var gestor = GestorDeCuentasDeInterlocutor.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override CuentaDeInterlocutorDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeCuentasDeInterlocutor.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
