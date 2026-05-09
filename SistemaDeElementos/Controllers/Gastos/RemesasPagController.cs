using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using System;
using Utilidades;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Gastos;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Gastos;
using Inicializador.Gastos;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using System.IO;
using ModeloDeDto;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class RemesasPagController : EntidadController<ContextoSe, RemesaPagDtm, RemesaPagDto>
    {
        public RemesasPagController(GestorDeRemesasPag gestorDeRemesasPag, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeRemesasPag,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudRemesasPag()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeRemesasPag).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Gasto}/{nameof(CrudRemesasPag)}";
                    return base.View(destino, new DescriptorDeRemesasPag(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<RemesaPagDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeRemesasPag(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            var ids = parametros.LeerValor(ltrParametrosEp.ids, (List<int>)null);
            switch (opcion)
            {
                case eventosDeMf.Rem_Pag_ModalDeImprimir:
                    ImprimirRemesaPag(ids);
                    return new ServicioDePlantillas(Contexto, enumNegocio.RemesaPag).Plantillas();
                case eventosDeMf.Rem_Pag_RetrodePago:
                    if (ids.Count != 1) GestorDeErrores.Emitir("Con esta opción, sólo se puede anular los pagos de una remesa");
                    var datosDeRetroceder = GestorDeRemesasPag.InformacionDeRetrocederPago(Contexto, ids[0]);
                    return new Resultado { Datos = datosDeRetroceder, ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render() };
                case eventosDeMf.Rem_Pag_Pagar:
                    if (ids.Count != 1) GestorDeErrores.Emitir("Con esta opción, sólo se puede anular los pagos de una remesa");
                    var datosDePago = GestorDeRemesasPag.InformacionDePagar(Contexto, ids[0]);
                    return new Resultado { Datos = datosDePago, ModoDeAcceso = enumModoDeAccesoDeDatos.Gestor.Render() };
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public IActionResult MaestrosDeRemesasPag()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzRemesasPag.ModeloDeRemesasPag(Contexto);
                TrabajosDeRemesasPag.SometerProcesosDeRemesasPag(Contexto);
                ViewBag.Mensaje = "Maestros inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar los maestros.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
        }

        public JsonResult epPagarRemesa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(PagarRemesaDto.IdElemento))) GestorDeErrores.Emitir("Debe indicar la remesa");
                if (!parametros.ContieneClave(nameof(PagarRemesaDto.PagadaEl))) GestorDeErrores.Emitir("Debe indicar la fecha en la que se ha producido el cargo");

                var idRemesa = (int)parametros.LeerValor<long>(nameof(PagarRemesaDto.IdElemento));
                var pagadaEl = parametros.LeerValor<DateTime>(nameof(PagarRemesaDto.PagadaEl));

                GestorDeRemesasPag.AdelantarPago(Contexto, idRemesa, pagadaEl);

                r.Consola = $"Remesa pagada correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede pagar la remesa.");
            }
            return new JsonResult(r);
        }

        public JsonResult epRetrocederPagoRemesa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(RetrocederPagoDto.IdElemento))) throw new Exception("Debe indicar la remesa");
                if (!parametros.ContieneClave(nameof(RetrocederPagoDto.PagadaEl))) throw new Exception("Debe indicar la fecha del nuevo cargo");

                var idRemesa = (int)parametros.LeerValor<long>(nameof(RetrocederPagoDto.IdElemento));
                var cargadoEl = parametros.LeerValor<DateTime>(nameof(RetrocederPagoDto.PagadaEl));

                GestorDeRemesasPag.RetrocederPago(Contexto, idRemesa);

                r.Consola = $"Remesa anulada correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede anular la remesa.");
            }
            return new JsonResult(r);
        }

        private void ImprimirRemesaPag(List<int> idsDeRem)
        {
            foreach (var id in idsDeRem)
            {
                var rem = Contexto.SeleccionarPorId<RemesaPagDtm>(id);

                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{rem.Referencia}.pdf".NormalizarFichero());
                //var remesaPagRpt = new GeneradorDeRemesaPagRpt(Contexto, rem).Generar(plantilla: null);
                //new ReporteDeRemesaPag(remesaPagRpt).GeneratePdf(rutaConFichero);
                var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.RemesaPag, enumNegocio.Archivos, rem.Id, idArchivo);
            }
        }

        protected override void ErrorAlTransitar(ContextoSe contexto, Exception excepcion, Resultado resultado, ElementoDeUnProcesoDto remesaDto)
        {
            base.ErrorAlTransitar(contexto, excepcion, resultado, remesaDto);
            var remesa = (RemesaPagDto)remesaDto;
            if (remesa != null)
            {
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{remesa.Referencia}.pdf".NormalizarFichero());
                ServidorDocumental.EliminarFichero(Contexto, rutaConFichero);
            }
        }

    }
}
