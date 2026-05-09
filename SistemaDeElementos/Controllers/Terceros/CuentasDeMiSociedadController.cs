using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestoresDeNegocio.Terceros;

namespace MVCSistemaDeElementos.Controllers
{
    public class CuentasDeMiSociedadController : EntidadController<ContextoSe, CuentaDeMiSociedadDtm, CuentaDeMiSociedadDto>
    {
        public CuentasDeMiSociedadController(GestorDeCuentasDeMiSociedad gestorDeCuentasDeMiSociedad, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCuentasDeMiSociedad,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeCuentasDeMiSociedad(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearCuentaDeMiSociedad);

        private ParametrosDeNegocio AntesDeEjecutar_CrearCuentaDeMiSociedad(CuentaDeMiSociedadDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeCuentasDeMiSociedad(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);
                
        protected override IEnumerable<CuentaDeMiSociedadDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Sociedad, typeof(CuentaDeMiSociedadDtm));

            //var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, restrictor.idElemento);
            //if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
            //    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Sociedad.Singular()}");

            var gestor = GestorDeCuentasDeMiSociedad.Gestor(Contexto, Contexto.Mapeador);
            opcionesDeMapeo[ltrParametrosNeg.AplicarJoin] = true;
            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override CuentaDeMiSociedadDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeCuentasDeMiSociedad.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
