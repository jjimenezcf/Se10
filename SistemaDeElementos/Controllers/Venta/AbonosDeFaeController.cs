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
    public class AbonosDeFaeController : EntidadController<ContextoSe, AbonoDeFaeDtm, AbonoDeFaeDto>
    {
        public AbonosDeFaeController(GestorDeAbonosDeFae gestorDeAbonosDeFae, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAbonosDeFae,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeAbonosDeFae(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearAbonoDeFae);

        private ParametrosDeNegocio AntesDeEjecutar_CrearAbonoDeFae(AbonoDeFaeDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeAbonosDeFae(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<AbonoDeFaeDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<FacturaEmtDtm>(restrictor.idElemento).IdTipo;

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.FacturaEmitida, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.FacturaEmitida.Singular()}");

            var gestor = GestorDeAbonosDeFae.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override AbonoDeFaeDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeAbonosDeFae.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
