using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using System;
using ServicioDeDatos.Elemento;
using AutoMapper;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class EstadosController : EntidadController<ContextoSe, EstadoDtm, EstadoDto>
    {
        public EstadosController(GestorDeEstados gestorDeEstados, GestorDeErrores gestorDeErrores, IMapper mapper)
        : base(gestorDeEstados, gestorDeErrores)
        {
            Contexto.Mapeador = mapper;
        }

        public IActionResult CrudDeEstados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            try
            {
                enumNegocio negocio = ApiController.AsignarNegocioDeLaRequest(_GestorDeElementos, HttpContext);
                return ViewCrud(new DescriptorDeEstados(Contexto, ModoDescriptor.Mantenimiento, negocio));
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
                case eventosDeMf.Transiciones:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

    }

}
