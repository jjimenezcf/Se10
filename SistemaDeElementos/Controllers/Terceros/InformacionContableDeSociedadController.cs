using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
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

                var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(idElemento);
                var informacion = sociedad.InformacionContable(Contexto);
                // además de tras una activación con errores, se puede relanzar si hay negocios activados con documentos sin preasiento
                // (esto se calcula aquí porque ActivacionDePreasientos está en GestoresDeNegocio)
                if (informacion is not null && !informacion.PuedeRelanzarActivacionDePreasientos && informacion.ModoDeAcceso == enumModoDeAccesoDeDatos.Gestor)
                    informacion.PuedeRelanzarActivacionDePreasientos = ActivacionDePreasientos.HayPreasientosPorActivar(Contexto, sociedad);
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

        // Vuelve a someter la activación de los preasientos de la sociedad cuando la última terminó con errores (ya subsanados por el usuario)
        public JsonResult epRelanzarActivacionDePreasientos(string parametrosJson)
        {
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                var sociedad = Contexto.SeleccionarPorId<SociedadDtm>((int)parametros.LeerValor<long>(ltrParametrosEp.id));
                if (sociedad.ModoDeAccesoALaInformacionContable(Contexto) != enumModoDeAccesoDeDatos.Gestor)
                    GestorDeErrores.Emitir($"No puede relanzar la activación de los preasientos de la sociedad '{sociedad.Expresion}': ha de poder parametrizar y ser gestor de la sociedad");

                TrabajosContables.SometerRelanzarActivacionDePreasientos(Contexto, sociedad.Id);

                Contexto.Commit(tran);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"Se ha sometido el trabajo de '{enumTrabajosContables.ActivarPreasientosDeLaSociedad.Descripcion()}', al terminar deja el resultado en las trazas de la sociedad";
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido relanzar la activación de los preasientos de la sociedad.");
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
            finally
            {
                ExtensorDeSociedades.VaciarCacheDeLaInformacionContable();
            }

            return new JsonResult(r);
        }
    }
}
