using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using Utilidades;

namespace SistemaDeElementos.Controllers.Venta
{
    public class FacturadorController : EntidadController<ContextoSe, PeticionDeFacturaEmtDtm, PeticionDeFacturaEmtDto>
    {
        public FacturadorController(Facturador gestor, GestorDeErrores gestorDeErrores)
        : base(gestor, gestorDeErrores)
        {
        }

        //Invoke-WebRequest -Method GET "https://localhost:44396/Facturador/epSolicitarFacturador?nifEmisor=00811725D&apiKey=[xxxx]&peticion=CrearFactura"
        [AllowAnonymous]
        public JsonResult epSolicitarFacturador(string nifEmisor, string apiKey, string peticion)
        {
            string ipAddress = ApiController.ObtenerIpDelCliente(HttpContext) ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string referer = HttpContext.Request.Headers["Referer"].ToString();
            string validadorJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            var tran = Contexto.IniciarTransaccion();
            Contexto.IniciarTraza(nameof(epSolicitarFacturador));
            var r = new Resultado();
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var operacion = ApiDeEnsamblados.ToEnumerado<enumOperacionFacturador>(peticion);
                r.Datos = Facturador.ObtenerFacturador(Contexto, nifEmisor, apiKey, operacion, validadorJson).Guid;
                r.Consola = $"Solicitud de operación registrada correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error en la solicitud.");
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        /*
         $facturaJson = @'{
  "NifDelCliente": "A87654321",
  "Nombre": "Cliente Ejemplo SL",
  "Descripcion": "Factura por servicios de consultoría...",
  "Lineas": [
    { "Orden": 1, "TipoDeLinea": "PartidaAlzada", "Concepto": "Licencia", "Cantidad": 1.00, "Precio": 1250.00, "Iva": "21", "Irpf": "0", "Unidad": "Ud", "Naturaleza": "SER" },
    { "Orden": 2, "TipoDeLinea": "Comentario", "Concepto": "NOTA", "Cantidad": null, "Precio": null, "Iva": null, "Irpf": null, "Unidad": null, "Naturaleza": null }
  ]
}'@ 

        $nif = "00811725D"
$guidObtenido = "TU_GUID_VALIDO_AQUI" # Debes obtenerlo primero de epSolicitarFacturador

# Comando para enviar la petición POST
Invoke-WebRequest -Method POST `
    -Uri "https://localhost:44396/PeticionesDeFacturasEmt/epCrearFactura?nif=$nif&guid=$guidObtenido" `
    -Body $facturaJson `
    -ContentType "application/json" `
    -SkipCertificateCheck
         
         */

        [AllowAnonymous]
        [HttpPost]
        public JsonResult epCrearFactura(string nif, string apiKey)
        {
            string ipAddress = ApiController.ObtenerIpDelCliente(HttpContext) ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string referer = HttpContext.Request.Headers["Referer"].ToString();
            string facturaJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            Contexto.IniciarTraza(nameof(epCrearFactura));
            var r = new Resultado();
            PeticionDeFacturaEmtDtm facturador = null;
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                // La petición se registra fuera de la transacción para que sobreviva al Rollback
                // y se le pueda anotar el error.
                facturador = Facturador.ObtenerFacturador(Contexto, nif, apiKey, enumOperacionFacturador.CrearFactura);

                var tran = Contexto.IniciarTransaccion();
                try
                {
                    // CrearFactura captura los errores de negocio y los devuelve en el Mensaje: en ese
                    // caso se hace Commit igualmente para conservar la prefactura creada.
                    var resultado = Facturador.CrearFactura(Contexto, facturador, facturaJson);
                    Contexto.Commit(tran);
                    r.Datos = resultado;
                    r.Consola = resultado.Mensaje;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                    r.Estado = resultado.Mensaje.Contains(ltrFacturador.SometidoEnvioDeFactura) ||
                               resultado.Mensaje.Contains(ltrFacturador.SometidoLoteDeEnvio) ||
                               resultado.Mensaje.Contains(ltrFacturador.NoUsaVerifactu)
                               ? enumEstadoPeticion.Ok : enumEstadoPeticion.Error;
                }
                catch
                {
                    Contexto.Rollback(tran);
                    throw;
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error en la solicitud.");
                if (facturador != null)
                    ExtensorDelFacturador.RegistrarExcepcion(Contexto, facturador.Guid, e);
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult epCrearCliente(string nif, string apiKey)
        {
            string clienteJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            Contexto.IniciarTraza(nameof(epCrearCliente));
            var r = new Resultado();
            PeticionDeFacturaEmtDtm facturador = null;
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                facturador = Facturador.ObtenerFacturador(Contexto, nif, apiKey, enumOperacionFacturador.CrearCliente);
                var datos = ClienteFacturadorJson.Parsear(clienteJson);

                var tran = Contexto.IniciarTransaccion();
                try
                {
                    var cliente = GestorDeClientes.CrearClienteCompleto(Contexto, datos);
                    Contexto.Commit(tran);
                    r.Datos = new { cliente.Id, NIF = datos.NIF, cliente.Nombre };
                    r.Consola = $"Cliente '{cliente.Nombre}' disponible para facturar";
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                    r.Estado = enumEstadoPeticion.Ok;
                }
                catch
                {
                    Contexto.Rollback(tran);
                    throw;
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error en la solicitud.");
                if (facturador != null)
                    ExtensorDelFacturador.RegistrarExcepcion(Contexto, facturador.Guid, e);
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult epCrearFacturaConGuid(string nif, string guid)
        {
            string ipAddress = ApiController.ObtenerIpDelCliente(HttpContext) ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string referer = HttpContext.Request.Headers["Referer"].ToString();
            string facturaJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            Contexto.IniciarTraza(nameof(epCrearFacturaConGuid));
            var r = new Resultado();
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));

                // la petición ya se registró (y confirmó) en epSolicitarFacturador, así que sobrevive
                // al Rollback y se le puede anotar el error
                var tran = Contexto.IniciarTransaccion();
                try
                {
                    // CrearFactura captura los errores de negocio y los devuelve en el Mensaje: en ese
                    // caso se hace Commit igualmente para conservar la prefactura creada.
                    var peticion = Facturador.CrearFactura(Contexto, nif, guid, facturaJson);
                    Contexto.Commit(tran);
                    r.Datos = peticion;
                    r.Consola = peticion.Mensaje;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                    r.Estado = peticion.Mensaje.Contains(ltrFacturador.SometidoEnvioDeFactura) ||
                               peticion.Mensaje.Contains(ltrFacturador.SometidoLoteDeEnvio) ||
                               peticion.Mensaje.Contains(ltrFacturador.NoUsaVerifactu)
                               ? enumEstadoPeticion.Ok : enumEstadoPeticion.Error;
                }
                catch
                {
                    Contexto.Rollback(tran);
                    throw;
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error en la solicitud.");
                if (Guid.TryParse(guid, out var guidDeLaPeticion))
                    ExtensorDelFacturador.RegistrarExcepcion(Contexto, guidDeLaPeticion, e);
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult epRectificarPorDe(string apiKey, string numeroFactura)
        {
            string motivo = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            Contexto.IniciarTraza(nameof(epRectificarPorDe));
            var r = new Resultado();
            PeticionDeFacturaEmtDtm facturador = null;
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var facturaOriginal = Facturador.ObtenerFacturaPorNumero(Contexto, numeroFactura);
                var nif = facturaOriginal.Cg(Contexto).Sociedad(Contexto).NIF;
                // La petición se registra fuera de la transacción para que sobreviva al Rollback
                // y se le pueda anotar el error.
                facturador = Facturador.ObtenerFacturador(Contexto, nif, apiKey, enumOperacionFacturador.RectificarPorDe);

                var tran = Contexto.IniciarTransaccion();
                try
                {
                    // RectificarPorDe captura los errores de negocio y los devuelve en el Mensaje: en
                    // ese caso se hace Commit igualmente para conservar la prefactura creada.
                    var resultado = Facturador.RectificarPorDe(Contexto, facturador, facturaOriginal, motivo);
                    Contexto.Commit(tran);
                    r.Datos = resultado;
                    r.Consola = resultado.Mensaje;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                    r.Estado = resultado.Mensaje.Contains(ltrFacturador.SometidoEnvioDeFactura) ||
                               resultado.Mensaje.Contains(ltrFacturador.SometidoLoteDeEnvio) ||
                               resultado.Mensaje.Contains(ltrFacturador.NoUsaVerifactu)
                               ? enumEstadoPeticion.Ok : enumEstadoPeticion.Error;
                }
                catch
                {
                    Contexto.Rollback(tran);
                    throw;
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error en la solicitud.");
                if (facturador != null)
                    ExtensorDelFacturador.RegistrarExcepcion(Contexto, facturador.Guid, e);
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

        [AllowAnonymous]
        public JsonResult epSolicitarPdf(string nif, string apiKey, string numeroFactura, string guid)
        {
            return SolicitarDocumento(nif, apiKey, numeroFactura, guid, enumOperacionFacturador.SolicitarPdf);
        }

        [AllowAnonymous]
        public JsonResult epSolicitarXml(string nif, string apiKey, string numeroFactura, string guid)
        {
            return SolicitarDocumento(nif, apiKey, numeroFactura, guid, enumOperacionFacturador.SolicitarXml);
        }

        private JsonResult SolicitarDocumento(string nif, string apiKey, string numeroFactura, string guid, enumOperacionFacturador operacion)
        {
            var tran = Contexto.IniciarTransaccion();
            Contexto.IniciarTraza(nameof(SolicitarDocumento));
            var r = new Resultado();
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                r.Datos = Facturador.ObtenerUrlDeDescargaDeDocumento(Contexto, nif, apiKey, numeroFactura, guid, operacion);
                r.Consola = "Url de descarga generada correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error en la solicitud.");
            }
            finally
            {
                Contexto.CerrarTraza();
                Contexto.QuitarUsuario();
            }
            return new JsonResult(r);
        }

    }
}
