using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using GestoresDeNegocio.Ventas;

namespace MVCSistemaDeElementos.Controllers
{
    public class CobrosDeFaeController : EntidadController<ContextoSe, CobroDeFaeDtm, CobroDeFaeDto>
    {
        public CobrosDeFaeController(GestorDeCobrosDeFae gestorDeCobrosDeFae, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCobrosDeFae,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeCobrosDeFae(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearCobroDeFae);

        private ParametrosDeNegocio AntesDeEjecutar_CrearCobroDeFae(CobroDeFaeDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeCobrosDeFae(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<CobroDeFaeDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<FacturaEmtDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.FacturaEmitida, Contexto, idTipo, typeof(CobroDeFaeDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.FacturaEmitida, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.FacturaEmitida.Singular()}");

            var gestor = GestorDeCobrosDeFae.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override CobroDeFaeDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeCobrosDeFae.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
