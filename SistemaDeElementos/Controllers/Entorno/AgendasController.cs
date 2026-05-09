using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using Utilidades;
using System.Collections.Generic;
using System;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace MVCSistemaDeElementos.Controllers
{
    public class AgendasController : EntidadController<ContextoSe, AgendaDtm, AgendaDto>
    {

        public AgendasController(GestorDeAgendas gestorDeAgendas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAgendas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudAgendas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeAgendas(Contexto, ModoDescriptor.Mantenimiento));
        }

        //[AllowAnonymous]
        //public FileStreamResult Subcribirme(int id, Guid guid)
        //{
        //    Contexto.AsignarUsuario(Contexto.Administrador());
        //    Contexto.IniciarTraza("Petición de subcripción", debugar: true);
        //    var agenda = Contexto.SeleccionarPorId<AgendaDtm>(id);
        //    try
        //    {
        //        if (agenda.Uri.Contains($"&guid={guid}"))
        //        {
        //            string contentType = "text/calendar";
        //            agenda.GenerarAgendaIcs(Contexto);
        //            Contexto.AnotarTraza("Ruta", agenda.UrlDeAgenda().AbsolutePath.ToString());
        //            var fileStream = new FileStream(agenda.RutaIcs, FileMode.Open);
        //            return new FileStreamResult(fileStream, contentType)
        //            {
        //                FileDownloadName = $"{agenda.Nombre}.ics" 
        //            };
        //        }
        //        throw new Exception($"La agenda solicitada no está disponible");
        //    }
        //    finally
        //    {
        //        //agenda.EliminarAgendaIcs();
        //        Contexto.CerrarTraza();
        //    }
        //}

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Subcribirme(int id, Guid guid)
        {
            Contexto.AsignarUsuario(Contexto.Administrador());
            Contexto.IniciarTraza("Petición de subcripción", debugar: true);

            try
            {
                var agenda = Contexto.SeleccionarPorId<AgendaDtm>(id);
                if (agenda == null)
                {
                    Contexto.AnotarTraza("Error", $"Agenda con ID {id} no encontrada");
                    return NotFound("Agenda no encontrada");
                }

                if (!agenda.Uri.Contains($"&guid={guid}"))
                {
                    Contexto.AnotarTraza("Error", "GUID no coincide");
                    return BadRequest("La agenda solicitada no está disponible");
                }

                agenda.GenerarAgendaIcs(Contexto);
                Contexto.AnotarTraza("Ruta", agenda.UrlDeAgenda().AbsolutePath.ToString());

                if (!System.IO.File.Exists(agenda.RutaIcs))
                {
                    Contexto.AnotarTraza("Error", "Archivo ICS no generado");
                    return StatusCode(500, "Error al generar el calendario");
                }

                var fileStream = new FileStream(agenda.RutaIcs, FileMode.Open, FileAccess.Read);
                return File(fileStream, "text/calendar", $"{agenda.Nombre}.ics");
            }
            catch (Exception ex)
            {
                Contexto.AnotarTraza("Error", ex.ToString());
                return StatusCode(500, "Error interno del servidor");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.AbrirAgenda:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }



    }
}
