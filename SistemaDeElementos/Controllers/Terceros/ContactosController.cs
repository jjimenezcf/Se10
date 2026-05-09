using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Terceros;
using ModeloDeDto.Terceros;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using GestorDeElementos;
using System.Collections.Generic;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class ContactosController : BaseController<ContactoDto>
    {
        public ContactosController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson)
        {
            return ApiController.PersistirElemento(new GestorDeContactos(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearContacto);
        }

        private ParametrosDeNegocio AntesDeEjecutar_CrearContacto(ContactoDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson)
        {
            var r = new Resultado();
            var gestor = GestorDeContactos.Gestor(Contexto, Contexto.Mapeador);
            var tran = gestor.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(gestor.Contexto, gestor.Mapeador, HttpContext);
                var registro = gestor.LeerRegistroPorId(id, true, false, false, false);
                gestor.PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Eliminar, false));
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                gestor.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        [HttpPost]
        public JsonResult epModificarRelacionPorPost(int idNegocio)
        {
            var body = ApiController.LeerBody(HttpContext);
            return ApiController.PersistirElemento(new GestorDeContactos(Contexto,Contexto.Mapeador),
                body.parametros[ltrParametrosEp.ElementoJson].ToString(),
                HttpContext, AntesDeEjecutar_ModificarRelacion);
        }

        protected override IEnumerable<ContactoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var negocioDtm = GestorDeNegocios.LeerNegocio(Contexto, restrictor.idNegocio);

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {negocioDtm.Nombre}");

            var gestor = GestorDeContactos.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override ContactoDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeContactos.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }
    }
}
