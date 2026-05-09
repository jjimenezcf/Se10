using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;

namespace MVCSistemaDeElementos.Controllers
{
    public class LineasDeUnaPlfVentaController : EntidadController<ContextoSe, LineaDeUnaPlfVentaDtm, LineaDeUnaPlfVentaDto>
    {
        public LineasDeUnaPlfVentaController(GestorDeLineasDeUnaPlfVenta gestorDeLineasDeUnaPlfVenta, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLineasDeUnaPlfVenta,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeLineasDeUnaPlfVenta(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearLineaDeUnPlfVenta);

        private ParametrosDeNegocio AntesDeEjecutar_CrearLineaDeUnPlfVenta(LineaDeUnaPlfVentaDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLineasDeUnaPlfVenta(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<LineaDeUnaPlfVentaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.PlanificacionDeVenta, typeof(LineaDeUnaPlfVentaDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.PlanificacionDeVenta, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.PlanificacionDeVenta.Singular()}");

            var gestor = GestorDeLineasDeUnaPlfVenta.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override LineaDeUnaPlfVentaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeLineasDeUnaPlfVenta.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
