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
using System.Linq;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Elemento;

namespace MVCSistemaDeElementos.Controllers
{
    public class BuzonesDeMiSociedadController : EntidadController<ContextoSe, BuzonDeMiSociedadDtm, BuzonDeMiSociedadDto>
    {
        public BuzonesDeMiSociedadController(GestorDeBuzonesDeMiSociedad gestorDeBuzonesDeMiSociedad, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeBuzonesDeMiSociedad,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeBuzonesDeMiSociedad(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearBuzonDeMiSociedad);

        private ParametrosDeNegocio AntesDeEjecutar_CrearBuzonDeMiSociedad(BuzonDeMiSociedadDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeBuzonesDeMiSociedad(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<BuzonDeMiSociedadDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var filtroPorCliente = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(FacturaEmtDto.IdCliente).ToLower());
            if (filtroPorCliente != null && !filtros.Any(x => x.Clausula == nameof(IDetalle.IdElemento)))
                filtroPorCliente.Clausula = nameof(IDetalle.IdElemento);
            //ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Cliente, typeof(BuzonDeMiSociedadDtm));

            //var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, idCliente);
            //if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
            //    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Cliente.Singular()}");

            var gestor = GestorDeBuzonesDeMiSociedad.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override BuzonDeMiSociedadDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeBuzonesDeMiSociedad.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
