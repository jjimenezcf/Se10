using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using System;
using ServicioDeDatos.Elemento;
using GestorDeElementos;
using AutoMapper;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class TransicionesController : EntidadController<ContextoSe, TransicionDtm, TransicionDto>
    {
        public TransicionesController(GestorDeTransiciones gestorDeTransiciones, GestorDeErrores gestorDeErrores, IMapper mapper)
        : base(gestorDeTransiciones, gestorDeErrores)
        {
            Contexto.Mapeador = mapper;
        }

        public IActionResult CrudDeTransiciones()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            try
            {
                enumNegocio negocio = ApiController.AsignarNegocioDeLaRequest(_GestorDeElementos, HttpContext);
                return ViewCrud(new DescriptorDeTransiciones(Contexto, ModoDescriptor.Mantenimiento, negocio));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }


        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
           ApiController.PersistirElemento(new GestorDeTransiciones(Contexto, NegociosDeSe.ToEnumerado(idNegocio)), elementoJson, HttpContext, AntesDeEjecutar_CrearElemento);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
            ApiController.BorrarPorId(new GestorDeTransiciones(Contexto), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<TransicionDto> DespuesDeEjecutar_Leer(IEnumerable<TransicionDto> leidos, List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametros)
        {
            var idNegocio = ApiController.BuscarNegocio(filtros, parametros);
            var negocio = NegociosDeSe.ToEnumerado(idNegocio);
            if (negocio == enumNegocio.FacturaEmitida)
            {
               return ExtensorDeFacturasEmt.ExcluirTransiciones(leidos, filtros);
            }
            return base.DespuesDeEjecutar_Leer(leidos, filtros, parametros);
        }

    }

}
