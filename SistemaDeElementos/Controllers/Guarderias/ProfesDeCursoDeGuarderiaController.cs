using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Guarderias;
using ModeloDeDto.Guarderias;
using GestoresDeNegocio.Guarderias;

namespace MVCSistemaDeElementos.Controllers
{
    public class ProfesDeCursoDeGuarderiaController : EntidadController<ContextoSe, ProfeDeCursoDeGuarderiaDtm, ProfeDeCursoDeGuarderiaDto>
    {
        public ProfesDeCursoDeGuarderiaController(GestorDeProfesDeCursoDeGuarderia gestorDeProfesDeCursoDeGuarderia, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeProfesDeCursoDeGuarderia,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeProfesDeCursoDeGuarderia(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearProfeDeCursoDeGuarderia);

        private ParametrosDeNegocio AntesDeEjecutar_CrearProfeDeCursoDeGuarderia(ProfeDeCursoDeGuarderiaDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeProfesDeCursoDeGuarderia(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<ProfeDeCursoDeGuarderiaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            enumNegocio.CursoDeGuarderia.ValidarUsaDetalleDe(typeof(ProfeDeCursoDeGuarderiaDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.CursoDeGuarderia, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al los {enumNegocio.CursoDeGuarderia.Plural(enMinusculas: true)}");

            var gestor = GestorDeProfesDeCursoDeGuarderia.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override ProfeDeCursoDeGuarderiaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeProfesDeCursoDeGuarderia.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
