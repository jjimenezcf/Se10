using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using System;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    // Ampliación ficticia "Enlaces" de la edición de una factura emitida (ver EnlacesDeFaeDto y DescriptorDeFacturasEmt): no tiene tabla,
    // se calcula al leerla con los documentos relacionados con la factura y no se persiste (es de sólo consulta)
    public class EnlacesDeFaeController : BaseController<EnlacesDeFaeDto>
    {
        public EnlacesDeFaeController(GestorDeErrores gestorDeErrores, ContextoSe contexto, IMapper mapeador)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        public JsonResult epLeerAmpliacionPorIdNegocio(int idNegocio, int idElemento, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerAmpliacionPorIdNegocio));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.FacturaEmitida, idElemento);
                if (modoDeAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                {
                    // se oculta la ampliación en el cliente
                    r.Datos = null;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
                }
                else
                {
                    r.Datos = Enlaces(idElemento);
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                }
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = "enlaces de la factura leídos";
                r.Mensaje = "";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se han podido leer los enlaces de la factura.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Es de sólo consulta: el cliente no la persiste (ver AmpliacionDeEdicion.SoloConsulta), si llegase no se hace nada
        public JsonResult epPersistirAmpliacion(int idNegocio, string elementoJson)
            => new JsonResult(new Resultado { Estado = enumEstadoPeticion.Ok, Mensaje = "" });

        [HttpPost]
        public JsonResult epPersistirAmpliacionPorPost(int idNegocio)
            => new JsonResult(new Resultado { Estado = enumEstadoPeticion.Ok, Mensaje = "" });

        private EnlacesDeFaeDto Enlaces(int idFactura)
        {
            var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura, usarLaCache: false);
            var presupuesto = factura.Presupuesto(Contexto);
            var parte = factura.ParteTr(Contexto);
            var contrato = factura.Contrato(Contexto);
            var preasiento = factura.Preasiento(Contexto, errorSiNoHay: false);
            var venta = Contexto.Set<VentaDtm>().FirstOrDefault(v => v.IdFacturaEmt == idFactura);

            return new EnlacesDeFaeDto
            {
                Id = factura.Id,
                IdElemento = factura.Id,
                IdPresupuesto = presupuesto?.Id,
                Presupuesto = presupuesto?.Expresion,
                IdParteTr = parte?.Id,
                ParteTr = parte?.Expresion,
                IdContrato = contrato?.Id,
                Contrato = contrato?.Expresion,
                IdPreasiento = preasiento?.Id,
                Preasiento = preasiento?.Expresion,
                IdVenta = venta?.Id,
                Venta = venta?.Expresion,
                ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor
            };
        }
    }
}
