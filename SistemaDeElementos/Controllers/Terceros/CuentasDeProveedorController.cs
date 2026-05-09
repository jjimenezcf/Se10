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
    public class CuentasDeProveedorController : EntidadController<ContextoSe, CuentaDeProveedorDtm, CuentaDeProveedorDto>
    {
        public CuentasDeProveedorController(GestorDeCuentasDeProveedor gestorDeCuentasDeProveedor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCuentasDeProveedor,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeCuentasDeProveedor(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearCuentaDeProveedor);

        private ParametrosDeNegocio AntesDeEjecutar_CrearCuentaDeProveedor(CuentaDeProveedorDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeCuentasDeProveedor(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<CuentaDeProveedorDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Proveedor, typeof(CuentaDeProveedorDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Proveedor, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Proveedor.Singular()}");

            var gestor = GestorDeCuentasDeProveedor.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override CuentaDeProveedorDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeCuentasDeProveedor.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
