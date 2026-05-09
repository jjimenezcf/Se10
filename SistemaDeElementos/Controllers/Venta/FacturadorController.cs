using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
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
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
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
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string referer = HttpContext.Request.Headers["Referer"].ToString();
            string facturaJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            var tran = Contexto.IniciarTransaccion();
            Contexto.IniciarTraza(nameof(epSolicitarFacturador));
            var r = new Resultado();
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                PeticionDeFacturaEmtDtm facturador = Facturador.ObtenerFacturador(Contexto, nif, apiKey, enumOperacionFacturador.CrearFactura);
                try
                {
                    var resultado = Facturador.CrearFactura(Contexto, facturador, facturaJson);
                    r.Datos = resultado;
                    r.Consola = resultado.Mensaje;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                    r.Estado = resultado.Mensaje.Contains(ltrFacturador.SometidoEnvioDeFactura) ||
                               resultado.Mensaje.Contains(ltrFacturador.SometidoLoteDeEnvio) ||
                               resultado.Mensaje.Contains(ltrFacturador.NoUsaVerifactu)
                               ? enumEstadoPeticion.Ok : enumEstadoPeticion.Error;
                    Contexto.Commit(tran);
                }
                catch (Exception e)
                {
                    Contexto.Rollback(tran);
                    ApiController.PrepararError(e, r, "Error en la solicitud.");
                    ExtensorDelFacturador.RegistrarExcepcion(Contexto, facturador.Guid, e);
                }
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
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string referer = HttpContext.Request.Headers["Referer"].ToString();
            string facturaJson = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEnd();
            var tran = Contexto.IniciarTransaccion();
            Contexto.IniciarTraza(nameof(epSolicitarFacturador));
            var r = new Resultado();
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var peticion = Facturador.CrearFactura(Contexto, nif, guid, facturaJson);
                r.Datos = peticion;
                r.Consola = peticion.Mensaje;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = peticion.Mensaje.Contains(ltrFacturador.SometidoEnvioDeFactura) ||
                           peticion.Mensaje.Contains(ltrFacturador.SometidoLoteDeEnvio) ||
                           peticion.Mensaje.Contains(ltrFacturador.NoUsaVerifactu)
                           ? enumEstadoPeticion.Ok : enumEstadoPeticion.Error;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error en la solicitud.");
                ExtensorDelFacturador.RegistrarExcepcion(Contexto, Guid.Parse(guid), e);
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
