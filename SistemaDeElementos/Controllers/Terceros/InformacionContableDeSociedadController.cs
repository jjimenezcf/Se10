using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    // Ampliación de la sociedad con su información contable: no tiene tabla, la lógica está en ExtensorDeSociedades
    public class InformacionContableDeSociedadController : BaseController<InformacionContableDeSociedadDto>
    {
        public InformacionContableDeSociedadController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        // Si el usuario no tiene acceso se devuelve sin permiso y el cliente oculta el span
        public JsonResult epLeerAmpliacionPorIdNegocio(int idNegocio, int idElemento, string parametrosJson)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var informacion = Contexto.SeleccionarPorId<SociedadDtm>(idElemento).InformacionContable(Contexto);
                r.Datos = informacion;
                r.ModoDeAcceso = (informacion?.ModoDeAcceso ?? enumModoDeAccesoDeDatos.SinPermiso).Render();
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = "información contable de la sociedad leida";
                r.Mensaje = "";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se ha podido leer la información contable de la sociedad.");
            }

            return new JsonResult(r);
        }

        // Al modificar la sociedad, graba su información contable en los parámetros de preasientos de la sociedad
        [HttpPost]
        public JsonResult epPersistirAmpliacionPorPost(int idNegocio)
        {
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var body = ApiController.LeerBody(HttpContext);
                var informacion = JsonConvert.DeserializeObject<InformacionContableDeSociedadDto>(body.parametros[ltrParametrosEp.ElementoJson].ToString());

                Contexto.SeleccionarPorId<SociedadDtm>(informacion.IdElemento).PersistirInformacionContable(Contexto, informacion);

                Contexto.Commit(tran);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "información contable actualizada";
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido grabar la información contable de la sociedad.");
            }

            return new JsonResult(r);
        }
    }
}
