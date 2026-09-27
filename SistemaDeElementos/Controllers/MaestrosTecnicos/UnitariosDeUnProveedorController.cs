using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.MaestrosTecnico;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    /// <summary>
    /// Tarifas de un proveedor: se fija el proveedor y se gestionan las tarifas de sus unitarios
    /// </summary>
    public class UnitariosDeUnProveedorController : EntidadController<ContextoSe, TarifaDtm, UnitariosDeUnProveedorDto>
    {
        public UnitariosDeUnProveedorController(GestorDeUnitariosDeUnProveedor gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

        public JsonResult epCrearTarifa(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(GestorDeUnitariosDeUnProveedor.Gestor(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearTarifa);

        private ParametrosDeNegocio AntesDeEjecutar_CrearTarifa(UnitariosDeUnProveedorDto elemento)
        {
            ValidarPermisoDeGestion(elemento.IdProveedor);
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epModificarTarifa(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(GestorDeUnitariosDeUnProveedor.Gestor(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_ModificarTarifa);

        private ParametrosDeNegocio AntesDeEjecutar_ModificarTarifa(UnitariosDeUnProveedorDto elemento)
        {
            ValidarPermisoDeGestion(elemento.IdProveedor);
            return new ParametrosDeNegocio(enumTipoOperacion.Modificar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(GestorDeUnitariosDeUnProveedor.Gestor(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override ParametrosDeNegocio AntesDeEjecutar_BorrarPorId(UnitariosDeUnProveedorDto elemento)
        {
            ValidarPermisoDeGestion(elemento.IdProveedor);
            return base.AntesDeEjecutar_BorrarPorId(elemento);
        }

        protected override IEnumerable<UnitariosDeUnProveedorDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var idProveedor = 0;
            foreach (var filtro in filtros)
                if (filtro.Clausula.Equals(nameof(UnitariosDeUnProveedorDto.IdProveedor), System.StringComparison.InvariantCultureIgnoreCase))
                    idProveedor = filtro.Valor.Entero();

            if (idProveedor == 0)
                GestorDeErrores.Emitir($"Para leer las tarifas de un proveedor se ha de indicar el proveedor");

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Proveedor, idProveedor);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Proveedor.Singular()}");

            var gestor = GestorDeUnitariosDeUnProveedor.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override UnitariosDeUnProveedorDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeUnitariosDeUnProveedor.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

        private void ValidarPermisoDeGestion(int idProveedor)
        {
            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Proveedor, idProveedor);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso || modoAcceso == enumModoDeAccesoDeDatos.Consultor)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene permisos de gestión sobre el {enumNegocio.Proveedor.Singular()}");
        }

    }
}
