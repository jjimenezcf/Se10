using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using System;
using Utilidades;
using System.Collections.Generic;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using Inicializador.Ventas;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using GestoresDeNegocio.Entorno;
using System.IO;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class RemesasFaeController : EntidadController<ContextoSe, RemesaFaeDtm, RemesaFaeDto>
    {
        public IPdfServerClient PdfServerClient { get; }

        public RemesasFaeController(GestorDeRemesasFae gestorDeRemesasFae, GestorDeErrores gestorDeErrores, IPdfServerClient pdfServerClient)
         : base
         (
           gestorDeRemesasFae,
           gestorDeErrores
         )
        {
            PdfServerClient = pdfServerClient;
        }

        public IActionResult CrudRemesasFae()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeRemesasFae(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            var ids = parametros.LeerValor(ltrParametrosEp.ids,(List<int>) null);
            switch (opcion)
            {
                case eventosDeMf.Rem_Fae_ModalDeImprimir:
                    ImprimirRemesaFae(ids);
                    return new ServicioDePlantillas(Contexto, enumNegocio.RemesaFae).Plantillas();
                case eventosDeMf.Rem_Fae_AnularCargo:
                    if (ids.Count != 1) GestorDeErrores.Emitir("Con esta opción, sólo se puede anular los cargos de una remesa");
                    var datosDeAnular = GestorDeRemesasFae.InformacionDeAnularCargo(Contexto, ids[0]);
                    return new Resultado { Datos = datosDeAnular, ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render()};
                case eventosDeMf.Rem_Fae_Cargar:
                    if (ids.Count != 1) GestorDeErrores.Emitir("Con esta opción, sólo se puede anular los cargos de una remesa");
                    var datosDeCarga = GestorDeRemesasFae.InformacionDeCargar(Contexto, ids[0]);
                    return new Resultado { Datos = datosDeCarga, ModoDeAcceso = enumModoDeAccesoDeDatos.Gestor.Render() };
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public IActionResult MaestrosDeRemesasFae()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzRemesasFae.ModeloDeRemesasFae(Contexto);
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

        public JsonResult epCargarRemesa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(CargarRemesaDto.IdElemento))) GestorDeErrores.Emitir("Debe indicar la remesa");
                if (!parametros.ContieneClave(nameof(CargarRemesaDto.CargadaEl))) GestorDeErrores.Emitir("Debe indicar la fecha en la que se ha producido el cargo");

                var idRemesa = (int)parametros.LeerValor<long>(nameof(CargarRemesaDto.IdElemento));
                var cargadoEl = parametros.LeerValor<DateTime>(nameof(CargarRemesaDto.CargadaEl));

                GestorDeRemesasFae.Cargar(Contexto, idRemesa, cargadoEl);

                r.Consola = $"Remesa cargada correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede cargar la remesa.");
            }
            return new JsonResult(r);
        }

        public JsonResult epAnularCargoRemesa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(AnularCargoRemesaDto.IdElemento))) throw new Exception("Debe indicar la remesa");
                if (!parametros.ContieneClave(nameof(AnularCargoRemesaDto.CargadaEl))) throw new Exception("Debe indicar la fecha del nuevo cargo");

                var idRemesa = (int)parametros.LeerValor<long>(nameof(AnularCargoRemesaDto.IdElemento));
                var cargadoEl = parametros.LeerValor<DateTime>(nameof(AnularCargoRemesaDto.CargadaEl));

                GestorDeRemesasFae.AnularCargo(Contexto, idRemesa);

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

        private void ImprimirRemesaFae(List<int> idsDeRem)
        {
            foreach (var id in idsDeRem)
            {
                var rem = Contexto.SeleccionarPorId<RemesaFaeDtm>(id);

                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{rem.Referencia}.pdf".NormalizarFichero());
                //var remesaFaeRpt = new GeneradorDeRemesaFaeRpt(Contexto, rem).Generar(plantilla: null);
                //new ReporteDeRemesaFae(remesaFaeRpt).GeneratePdf(rutaConFichero);
                var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.RemesaFae, enumNegocio.Archivos, rem.Id, idArchivo);
            }
        }

        protected override void ErrorAlTransitar(ContextoSe contexto, Exception excepcion, Resultado resultado, ElementoDeUnProcesoDto remesaDto)
        {
            base.ErrorAlTransitar(contexto, excepcion, resultado, remesaDto);
            var remesa = (RemesaFaeDto)remesaDto;
            if (remesa != null)
            {
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{remesa.Referencia}.pdf".NormalizarFichero());
                ServidorDocumental.EliminarFichero(Contexto, rutaConFichero);
            }
        }


    }
}
