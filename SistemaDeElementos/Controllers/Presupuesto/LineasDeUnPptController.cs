using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Presupuesto;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Presupuesto;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class LineasDeUnPptController : EntidadController<ContextoSe, LineaDeUnPptDtm, LineaDeUnPptDto>
    {
        public LineasDeUnPptController(GestorDeLineasDeUnPpt gestorDeLineasDeUnPpt, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLineasDeUnPpt,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeLineasDeUnPpt(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearLineaDeUnPpt);

        private ParametrosDeNegocio AntesDeEjecutar_CrearLineaDeUnPpt(LineaDeUnPptDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLineasDeUnPpt(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<LineaDeUnPptDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<PresupuestoDtm>(restrictor.idElemento).IdTipo;            
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Presupuesto, Contexto, idTipo, typeof(LineaDeUnPptDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Presupuesto, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Presupuesto.Singular()}");

            var gestor = GestorDeLineasDeUnPpt.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override LineaDeUnPptDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeLineasDeUnPpt.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
