using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Logistica;
using ModeloDeDto.Logistica;
using ServicioDeDatos.Logistica;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class LineasDeUnaDevolucionDeVentaController : EntidadController<ContextoSe, LineaDeUnaDevolucionDeVentaDtm, LineaDeUnaDevolucionDeVentaDto>
    {
        public LineasDeUnaDevolucionDeVentaController(GestorDeLineasDeUnaDevolucionDeVenta gestorDeLineas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLineas,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeLineasDeUnaDevolucionDeVenta(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearLinea);

        private ParametrosDeNegocio AntesDeEjecutar_CrearLinea(LineaDeUnaDevolucionDeVentaDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLineasDeUnaDevolucionDeVenta(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<LineaDeUnaDevolucionDeVentaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            // Expansor 'Devoluciones' de la edición del documento de venta: filtra por IdVenta (no por IdElemento de la devolución), así
            // que el acceso se valida sobre la venta y el filtro lo aplica GestorDeLineasDeUnaDevolucionDeVenta.AplicarFiltros (FiltroPorVenta)
            var filtroPorVenta = filtros.Find(f => f.Clausula.ToLower() == ltrDeUnaDevolucionDeVenta.FiltroPorVenta.ToLower());
            if (filtroPorVenta != null)
            {
                var idVenta = filtroPorVenta.Valor.Entero();
                if (idVenta == 0)
                    GestorDeErrores.Emitir($"Debe indicar el {enumNegocio.Venta.Singular(true)}");
                if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Venta, idVenta) == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Venta.Singular()}");

                return GestorDeLineasDeUnaDevolucionDeVenta.Gestor(Contexto, Contexto.Mapeador).LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
            }

            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<DevolucionDeVentaDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.DevolucionVenta, Contexto, idTipo, typeof(LineaDeUnaDevolucionDeVentaDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.DevolucionVenta, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.DevolucionVenta.Singular()}");

            var gestor = GestorDeLineasDeUnaDevolucionDeVenta.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override LineaDeUnaDevolucionDeVentaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeLineasDeUnaDevolucionDeVenta.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
