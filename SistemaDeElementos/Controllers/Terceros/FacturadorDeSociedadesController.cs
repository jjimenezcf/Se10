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
    public class FacturadorDeSociedadesController : EntidadController<ContextoSe, FacturadorDeSociedadDtm, FacturadorDeSociedadDto>
    {
        public FacturadorDeSociedadesController(GestorDeFacturadorDeSociedades gestorDeFacturadorDeSociedades, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeFacturadorDeSociedades,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeFacturadorDeSociedades(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearFacturadorDeSociedad);

        private ParametrosDeNegocio AntesDeEjecutar_CrearFacturadorDeSociedad(FacturadorDeSociedadDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeFacturadorDeSociedades(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);
                
        protected override IEnumerable<FacturadorDeSociedadDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Sociedad, typeof(FacturadorDeSociedadDtm));

            //var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, restrictor.idElemento);
            //if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
            //    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Sociedad.Singular()}");

            var gestor = GestorDeFacturadorDeSociedades.Gestor(Contexto, Contexto.Mapeador);
            opcionesDeMapeo[ltrParametrosNeg.AplicarJoin] = true;
            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override FacturadorDeSociedadDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeFacturadorDeSociedades.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
