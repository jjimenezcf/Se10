using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using GestoresDeNegocio.TrabajosSometidos;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using System.Collections.Generic;
using System;
using GestoresDeNegocio.SistemaDocumental;
using Utilidades;
using static MVCSistemaDeElementos.Descriptores.DescriptorDeTrabajosDeUsuario;

namespace MVCSistemaDeElementos.Controllers
{
    public class TrabajosDeUsuarioController : EntidadController<ContextoSe, TrabajoDeUsuarioDtm, TrabajoDeUsuarioDto>
    {

        public TrabajosDeUsuarioController(GestorDeTrabajosDeUsuario gestorDeTu, GestorDeErrores gestorDeErrores)
        : base
        (
          gestorDeTu,
          gestorDeErrores
        )
        {

        }

        public IActionResult CrudDeTrabajosDeUsuario()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeTrabajosDeUsuario(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.MensajeCompleto());
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            var listaIds = (List<int>)parametros[ltrParametrosEp.ids];
            if (listaIds.Count > 1)
                GestorDeErrores.Emitir($"la opción de '{opcion}' sólo es válida para un trabajo");
            int idTrabajoUsuario = listaIds[0];
            switch (opcion)
            {
                case eventosDeMfDeTrabajos.MfEjecutar:
                    GestorDeTrabajosDeUsuario.Iniciar(_GestorDeElementos.Contexto, idTrabajoUsuario, false);
                    return null;
                case eventosDeMfDeTrabajos.MfBloquear:
                    GestorDeTrabajosDeUsuario.Bloquear(_GestorDeElementos.Contexto, idTrabajoUsuario);
                    return null;
                case eventosDeMfDeTrabajos.MfDesbloquear:
                    GestorDeTrabajosDeUsuario.Desbloquear(_GestorDeElementos.Contexto, idTrabajoUsuario);
                    return null;
                case eventosDeMfDeTrabajos.MfResometer:
                    GestorDeTrabajosDeUsuario.Resometer(_GestorDeElementos.Contexto, idTrabajoUsuario, false);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }



        public JsonResult epIniciarTrabajoDeUsuario(int idTrabajoUsuario)
        {
            var r = new Resultado();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                GestorDeTrabajosDeUsuario.Iniciar(_GestorDeElementos.Contexto, idTrabajoUsuario, false);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Trabajo iniciado";
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                    r.Mensaje = e.Message;
                else
                    r.Mensaje = $"Error al iniciar el trabajo. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
            }

            return new JsonResult(r);
        }
        public JsonResult epBloquearTrabajoDeUsuario(int idTrabajoUsuario)
        {
            var r = new Resultado();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                GestorDeTrabajosDeUsuario.Bloquear(_GestorDeElementos.Contexto, idTrabajoUsuario);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Trabajo bloqueado";
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                    r.Mensaje = e.Message;
                else
                    r.Mensaje = $"Error al bloquear el trabajo. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
            }

            return new JsonResult(r);
        }
        public JsonResult epDesbloquearTrabajoDeUsuario(int idTrabajoUsuario)
        {
            var r = new Resultado();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                GestorDeTrabajosDeUsuario.Desbloquear(_GestorDeElementos.Contexto, idTrabajoUsuario);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Trabajo desbloqueado";
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                    r.Mensaje = e.Message;
                else
                    r.Mensaje = $"Error al desbloquear el trabajo. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
            }

            return new JsonResult(r);
        }
        public JsonResult epResometerTrabajoDeUsuario(int idTrabajoUsuario)
        {
            var r = new Resultado();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                GestorDeTrabajosDeUsuario.Resometer(_GestorDeElementos.Contexto, idTrabajoUsuario, false);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Trabajo resometido";
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                    r.Mensaje = e.Message;
                else
                    r.Mensaje = $"Error al resometer el trabajo. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
            }

            return new JsonResult(r);
        }
        public JsonResult epSometerTrabajoDeUsuario(string trabajo, string parametros)
        {
            var r = new Resultado();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                switch (trabajo)
                {
                    case enumTrabajosSometidos.SincronizarArchivador:
                        GestorDeArchivadores.SometerSincronizarArchivador(Contexto, parametros);
                        break;
                    default:
                        throw new Exception("Implemenar esta función");
                }
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Trabajo resometido";
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                    r.Mensaje = e.Message;
                else
                    r.Mensaje = $"Error al someter el trabajo. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
            }

            return new JsonResult(r);
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_CrearElemento(TrabajoDeUsuarioDto elemento)
        {
            var p = base.AntesDeEjecutar_CrearElemento(elemento);
            p.Parametros[GestorDeTrabajosDeUsuario.PeticionDelControlador] = true;

            if (elemento.Planificado == null)
                elemento.Planificado = DateTime.Now;

            if (!elemento.IdEjecutor.HasValue || elemento.IdEjecutor == 0)
                elemento.IdEjecutor = elemento.IdSometedor;

            return p;
        }

        protected override void AntesDeEjecutar_Leer(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> ordenes, Dictionary<string, object> parametros)
        {
            base.AntesDeEjecutar_Leer(posicion, cantidad, filtros, ordenes, parametros);
            if (!DatosDeConexion.EsAdministrador)
            {
                var f = new ClausulaDeFiltrado { Clausula = nameof(TrabajoDeUsuarioDtm.IdEjecutor), Criterio = enumCriteriosDeFiltrado.igual, Valor = DatosDeConexion.IdUsuario.ToString() };
                filtros.Add(f);
            }
            if (ordenes.Count == 0)
                ordenes.Add(new ClausulaDeOrdenacion { Modo = ModoDeOrdenancion.descendente, OrdenarPor = nameof(TrabajoDeUsuarioDtm.Planificado) });
        }

    }

}
