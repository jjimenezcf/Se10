using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using AutoMapper;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using GestorDeElementos;
using DocumentFormat.OpenXml.Wordprocessing;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class DireccionesController : BaseController<DireccionDto>
    {
        public DireccionesController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson)
        {
            return ApiController.PersistirElemento(new GestorDeDirecciones(Contexto, NegociosDeSe.ToEnumerado(idNegocio)), elementoJson, HttpContext, AntesDeEjecutar_CrearDireccion);
        }

        private ParametrosDeNegocio AntesDeEjecutar_CrearDireccion(DireccionDto elemento)
        {
            var b = ApiController.ValidarUrlAsync(url: elemento.Url).GetAwaiter().GetResult();
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        [HttpPost]
        public JsonResult epModificarRelacionPorPost(int idNegocio)
        {
            var body = ApiController.LeerBody(HttpContext);
            return ApiController.PersistirElemento(new GestorDeDirecciones(Contexto, NegociosDeSe.ToEnumerado(idNegocio)),
                body.parametros[ltrParametrosEp.ElementoJson].ToString(),
                HttpContext,
                AntesDeEjecutar_ModificarDireccion);
        }

        private ParametrosDeNegocio AntesDeEjecutar_ModificarDireccion(DireccionDto elemento)
        {
            //var b = ApiController.ValidarUrlAsync(url: elemento.Url).GetAwaiter().GetResult();
            return new ParametrosDeNegocio(enumTipoOperacion.Modificar);
        }

        protected override IEnumerable<DireccionDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var negocioDtm = GestorDeNegocios.LeerNegocio(Contexto, restrictor.idNegocio);

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, NegociosDeSe.ToEnumerado(negocioDtm.Nombre), restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {negocioDtm.Nombre}");

            var gestor = GestorDeDirecciones.Gestor(Contexto, NegociosDeSe.ToEnumerado(negocioDtm.Nombre));
            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override DireccionDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var negocio = ObtenerNegocio(parametros);
            var gestor = GestorDeDirecciones.Gestor(Contexto, negocio);
            return gestor.LeerElementoPorId(id,parametros);
        }

    }

}
