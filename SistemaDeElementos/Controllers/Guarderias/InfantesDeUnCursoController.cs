using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Guarderias;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Guarderias;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class InfantesDeUnCursoController : RelacionController<ContextoSe, InfanteDeUnCursoDtm, InfanteDeUnCursoDto>
    {
        public InfantesDeUnCursoController(GestorDeInfantesDeUnCurso gestorDeInfantesDeUnCurso, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeInfantesDeUnCurso,
           gestorDeErrores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudInfantesDeUnCurso()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeInfantesDeUnCurso(Contexto, ModoDescriptor.Mantenimiento));
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeInfantesDeUnCurso(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearInfanteDeUnCurso);

        private ParametrosDeNegocio AntesDeEjecutar_CrearInfanteDeUnCurso(InfanteDeUnCursoDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        //public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        //ApiController.BorrarPorId(new GestorDeInfantesDeUnCurso(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<InfanteDeUnCursoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            enumNegocio.CursoDeGuarderia.ValidarUsaDetalleDe(typeof(InfanteDeUnCursoDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.CursoDeGuarderia, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al los {enumNegocio.CursoDeGuarderia.Plural(enMinusculas: true)}");

            var gestor = GestorDeInfantesDeUnCurso.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override InfanteDeUnCursoDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeInfantesDeUnCurso.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
