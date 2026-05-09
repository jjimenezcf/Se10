using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class LineasDeUnPlfVentaController : EntidadController<ContextoSe, LineaDeUnPlfVentaDtm, LineaDeUnPlfVentaDto>
    {
        public LineasDeUnPlfVentaController(GestorDeLineasDeUnPlfVenta gestorDeLineasDeUnPlfVenta, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLineasDeUnPlfVenta,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeLineasDeUnPlfVenta(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearLineaDeUnPlfVenta);

        private ParametrosDeNegocio AntesDeEjecutar_CrearLineaDeUnPlfVenta(LineaDeUnPlfVentaDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLineasDeUnPlfVenta(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<LineaDeUnPlfVentaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.PlanificadorDeVenta, typeof(LineaDeUnPlfVentaDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.PlanificadorDeVenta, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.PlanificadorDeVenta.Singular()}");

            var gestor = GestorDeLineasDeUnPlfVenta.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override LineaDeUnPlfVentaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeLineasDeUnPlfVenta.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
