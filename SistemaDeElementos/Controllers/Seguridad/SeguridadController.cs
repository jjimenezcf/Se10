using System;
using System.Collections.Generic;
using AutoMapper;
using Gestor.Errores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Seguridad;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public static class ltrSeguridad
    {
        public const string IdUsuario = "idUsuario";
        public const string Clave = "clave";
        public const string Tipo = "tipo";
        public const string Objeto = "objeto";
        public const string Id = "id";
        public const string IdNegocio = "idNegocio";
        public const string Clase = "clase";
    }

    public class SeguridadController : FormularioController<ContextoSe>
    {
        private static string VistaDeSeguridad => $"{nameof(SeguridadController).Replace(ltrEndPoint.Controller, "")}.{nameof(SeguridadDeUsuarios)}";

        public SeguridadController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(contexto, mapeador, gestorDeErrores)
        {
        }

        public IActionResult SeguridadDeUsuarios()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                ViewBag.DatosDeConexion = DatosDeConexion;
                ValidarAcceso();
                return View(nameof(SeguridadDeUsuarios), new DescriptorDeSeguridad(Contexto));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        // END-POINT: desde Seguridad.ts, al seleccionar un usuario devuelve el nodo del usuario con sus nodos de primer nivel
        public JsonResult epMostrarSeguridad(string parametrosJson)
        {
            return Responder(parametrosJson, "No se ha podido leer la seguridad del usuario", parametros =>
                ArbolDeSeguridad.LeerUsuario(Contexto, Entero(parametros, ltrSeguridad.IdUsuario)));
        }

        // END-POINT: desde Seguridad.ts, al desplegar un nodo por primera vez devuelve sus hijos
        public JsonResult epLeerNodosHijos(string parametrosJson)
        {
            return Responder(parametrosJson, "No se han podido leer los nodos hijos", parametros =>
                ArbolDeSeguridad.LeerHijos(Contexto, Mapeador
                    , clavePadre: Cadena(parametros, ltrSeguridad.Clave)
                    , tipo: ApiDeEnsamblados.ToEnumerado<enumNodoDeSeguridad>(Cadena(parametros, ltrSeguridad.Tipo))
                    , idUsuario: Entero(parametros, ltrSeguridad.IdUsuario)
                    , id: Entero(parametros, ltrSeguridad.Id)
                    , idNegocio: Entero(parametros, ltrSeguridad.IdNegocio)
                    , clase: Cadena(parametros, ltrSeguridad.Clase)));
        }

        // END-POINT: desde Seguridad.ts, al seleccionar un puesto, rol o permiso devuelve sus datos para mostrarlos en consulta
        public JsonResult epLeerDatosDelObjeto(string parametrosJson)
        {
            return Responder(parametrosJson, "No se han podido leer los datos del nodo seleccionado", parametros =>
            {
                var elemento = ArbolDeSeguridad.LeerObjeto(Contexto, Mapeador
                    , ApiDeEnsamblados.ToEnumerado<enumObjetoDeSeguridad>(Cadena(parametros, ltrSeguridad.Objeto))
                    , Entero(parametros, ltrSeguridad.Id));
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                return elemento;
            });
        }

        private JsonResult Responder(string parametrosJson, string mensajeDeError, Func<Dictionary<string, object>, object> leer)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                ValidarAcceso();
                r.Datos = leer(parametrosJson.ToDiccionarioDeParametros());
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, mensajeDeError);
            }
            return new JsonResult(r);
        }

        // la seguridad de los usuarios solo la pueden consultar los administradores o quien tenga permiso sobre la vista
        private void ValidarAcceso()
        {
            if (DatosDeConexion.EsAdministrador)
                return;

            var gestor = GestorDeUsuarios.Gestor(Contexto, Mapeador);
            var usuario = gestor.LeerRegistroCacheado(nameof(UsuarioDtm.Login), DatosDeConexion.Login, errorSiNoHay: true, errorSiHayMasDeUno: true, aplicarJoin: false);
            if (!gestor.TienePermisoFuncional(usuario, VistaDeSeguridad))
                GestorDeErrores.Emitir($"Solicite permisos de acceso a {VistaDeSeguridad}");
        }

        private static int Entero(Dictionary<string, object> parametros, string clave)
        => parametros.ContainsKey(clave) && parametros[clave] != null ? parametros[clave].ToString().Entero() : 0;

        private static string Cadena(Dictionary<string, object> parametros, string clave)
        => parametros.ContainsKey(clave) && parametros[clave] != null ? parametros[clave].ToString() : "";
    }
}
