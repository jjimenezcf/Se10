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
    public class TarjetasDeMiSociedadController : EntidadController<ContextoSe, TarjetaDeMiSociedadDtm, TarjetaDeMiSociedadDto>
    {
        public TarjetasDeMiSociedadController(GestorDeTarjetasDeMiSociedad gestorDeTarjetasDeMiSociedad, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeTarjetasDeMiSociedad,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeTarjetasDeMiSociedad(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearTarjetaDeMiSociedad);

        private ParametrosDeNegocio AntesDeEjecutar_CrearTarjetaDeMiSociedad(TarjetaDeMiSociedadDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeTarjetasDeMiSociedad(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);
                
        protected override IEnumerable<TarjetaDeMiSociedadDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Sociedad, typeof(TarjetaDeMiSociedadDtm));

            //var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, restrictor.idElemento);
            //if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
            //    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Sociedad.Singular()}");

            var gestor = GestorDeTarjetasDeMiSociedad.Gestor(Contexto, Contexto.Mapeador);
            opcionesDeMapeo[ltrParametrosNeg.AplicarJoin] = true;
            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override TarjetaDeMiSociedadDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeTarjetasDeMiSociedad.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
