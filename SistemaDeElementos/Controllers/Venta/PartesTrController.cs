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
using GestorDeElementos.Extensores;
using GestorDeElementos;
using ModeloDeDto;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeReportes.Ventas;
using System.IO;
using QuestPDF.Fluent;
using ServicioDeDatos.SistemaDocumental;

namespace MVCSistemaDeElementos.Controllers
{
    public class PartesTrController : EntidadController<ContextoSe, ParteTrDtm, ParteTrDto>
    {
        public PartesTrController(GestorDePartesTr gestorDePartesTr, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePartesTr,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPartesDeTrabajo()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDePartesTr(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Ptr_IrAFacturasEmt:
                    enumNegocio.ParteDeTrabajo.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(ParteTrDtm.IdFacturaEmt));
                    return null;
                case eventosDeMf.Ptr_IrAPpts:
                    enumNegocio.ParteDeTrabajo.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(ParteTrDtm.IdPresupuesto));
                    return null;
                case eventosDeMf.Ptr_IrAPlvDeVenta:
                    foreach (var id in (List<int>)parametros[ltrParametrosEp.ids])
                    {
                        var parte = Contexto.SeleccionarPorId<ParteTrDtm>(id);
                        parte.ValidarQueReferenciaA<PlanificacionDeVentaDtm>(Contexto, nameof(PlanificacionDeVentaDtm.IdParteTr));
                    }
                    return null;
                case eventosDeMf.Ptr_IrAContrato:
                    enumNegocio.ParteDeTrabajo.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(ParteTrDtm.IdContrato));
                    return null;
                case eventosDeMf.Ptr_ModalDeImprimir:
                    //ImprimirParte((List<int>)parametros[ltrParametrosEp.ids]);
                    return new ServicioDePlantillas(Contexto, enumNegocio.ParteDeTrabajo).Plantillas();
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }


        public IActionResult MaestrosDePartesTr()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzPartesTr.ModeloDePartesTr(Contexto);
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

        protected override int Vincular(int idNegocio, int idVinculado, int idElemento1, SelectorDto elemento2, Dictionary<string, object> parametros)
        {
            if (NegociosDeSe.ToEnumerado(idVinculado) == enumNegocio.ParteDeTrabajo)
            {
                return ExtensorDeFacturasEmt.IncluirParteTr(Contexto, idElemento1, elemento2, parametros);
            }
            return base.Vincular(idNegocio, idVinculado, idElemento1, elemento2, parametros);
        }

        protected override bool Imprimir(int idNegocio, Dictionary<string, object> parametros)
        {
            if (base.Imprimir(idNegocio, parametros))
                return true;

            List<int> idsDePartes = new List<int> { (int)parametros.LeerValor<long>(ltrParametrosEp.idElemento) };
            var plantilla = parametros.LeerValor<string>(ltrParametrosEp.Plantilla);

            if (!ExtensorDeEnum.Existe<enumPltPartesTrRpt>(plantilla))
                return false;

            foreach (var id in idsDePartes)
            {
                var ptr = Contexto.SeleccionarPorId<ParteTrDtm>(id, aplicarJoin: true);
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{plantilla}-{ptr.Referencia}.{enumExtensiones.pdf}".NormalizarFichero());
                var parteRpt = new GeneradorDeParteTrRpt(Contexto, ptr).ObtenerInformacionDeRpt(plantilla);
                var reporte = new ReporteDePartesTr(parteRpt, plantilla);
                reporte.GeneratePdf(rutaConFichero);
                var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.ParteDeTrabajo, enumNegocio.Archivos, ptr.Id, idArchivo);
            }

            return true;
        }
    }
}
