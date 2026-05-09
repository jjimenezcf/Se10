using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using SistemaDeElementos.Inicializador;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace SistemaDeElementos.Controllers.Entorno
{
    public class ArbolDeMenuController : EntidadController<ContextoSe, ArbolDeMenuDtm, ArbolDeMenuDto>
    {

        public ArbolDeMenuController(GestorDeArbolDeMenu gestor, GestorDeErrores gestorDeErrores)
        : base(gestor, gestorDeErrores)
        {
        }

        //END-POINT: Desde ArbolDeMenu.ts
        public JsonResult epSolicitarMenuHtml(string parametrosJson)
        {
            var r = new ResultadoHtml();
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);

            try
            {
                var cache = ServicioDeCaches.Obtener(CacheDe.ArbolDeMenu);
                if (!cache.ContainsKey(DatosDeConexion.Login))
                {
                    var procesadas = new List<int>();
                    List<ArbolDeMenuDto> menu = ((GestorDeArbolDeMenu)_GestorDeElementos).LeerArbolDeMenu(DatosDeConexion.Login);
                    var menuHtml =
                    @$"
                    <ul id='id_menuraiz' class=¨menu-contenido¨>
                       {RenderOpcionesMenu(menu, procesadas, idMenuPadre: 0, estructuraPlana: Contexto.DatosDeConexion.EsClienteWeb)}
                    </ul>
                    ";
                    cache[DatosDeConexion.Login] = menuHtml.Replace("¨", "\"");
                }
                r.Html = (string)cache[DatosDeConexion.Login];
                r.Estado = enumEstadoPeticion.Ok;
                r.Datos = DatosDeConexion.Login;
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                r.Mensaje = "No se ha podido leer el menú";
            }
            return new JsonResult(r);
        }

        private static string RenderOpcionesMenu(List<ArbolDeMenuDto> opcionesMenu, List<int> procesadas, int idMenuPadre, bool estructuraPlana)
        {
            var menuHtml = "";
            foreach (ArbolDeMenuDto fDto in opcionesMenu)
            {
                if (procesadas.Contains(fDto.Id))
                    continue;

                menuHtml = menuHtml + RenderMenu(funcion: fDto, procesadas, idMenuPadre, estructuraPlana);
                procesadas.Add(fDto.Id);
            }
            return menuHtml;
        }

        private static string RenderMenu(ArbolDeMenuDto funcion, List<int> procesadas, int idMenuPadre, bool estructuraPlana)
        {
            if (funcion.IdVistaMvc != null)
            {
                var opcionHtml = RenderAccionMenu(funcion);
                return opcionHtml;
            }
            if (estructuraPlana) return "";

            var subMenuHtml = funcion.Submenus != null ? RenderOpcionesMenu(funcion.Submenus, procesadas, funcion.Id, estructuraPlana) : "";

            var idMenuHtml = $"id_menu_{funcion.Id}";
            var idMenuPadreHtml = $"id_menu_{idMenuPadre}";
            var liHtml =
            $@"
            <li>
                <a style=¨display: flex; padding-top: 2px;padding-bottom: 2px;¨>
                  <img src=¨/images/menu/{funcion.Icono}¨ style=¨margin-top: 6px;margin-right: 0px; width: 20px; height: 20px;¨ alt=¨{funcion.Nombre}¨ />
                  <input id='{funcion.Id}' type='button' class='menu-opcion' value='{funcion.Nombre}' style=¨padding-left: 3px;¨ onclick =¨ArbolDeMenu.MenuPulsado('{idMenuHtml}')¨ />
                </a>
                <ul id=¨{idMenuHtml}¨ name=¨menu¨ menu-padre=¨{idMenuPadreHtml}¨ menu-plegado=¨true¨>
                  subMenuHtml
                </ul>
            </li>
            ";

            return liHtml.Replace("subMenuHtml", subMenuHtml);
        }

        private static string RenderAccionMenu(ArbolDeMenuDto funcion)
        {
            var idHtml = $"{funcion.VistaMvc.Id}";
            var opcionHtml =
            $@"<li  title=¨{funcion.Descripcion}¨>{Environment.NewLine}" +
            $@"  <a style=¨display: flex; padding-top: 2px;padding-bottom: 2px;¨>{Environment.NewLine}" +
            $@"    <img src=¨/images/menu/{funcion.Icono}¨ style=¨margin-top: 6px;margin-right: 0px; width: 20px; height: 20px;¨  alt=¨{funcion.Nombre}¨/>" +
            $@"    <input id='{idHtml}' type='button' class='menu-opcion' value='{funcion.Nombre}' style=¨padding-left: 3px;¨ onclick =¨ArbolDeMenu.OpcionSeleccionada('{idHtml}','{funcion.VistaMvc.Controlador}','{funcion.VistaMvc.Accion}','{funcion.Parametros}', event)¨ />{Environment.NewLine}" +
            $@"  </a>" +
            $@"</li>{Environment.NewLine}";

            return opcionHtml.Replace("¨", "\""); ;
        }

        private static string RenderAccesoRegistro(enumNegocio negocio, int id, string referencia, string nombre, string icono, string url)
        {
            //window.location.href='{url}'
            var idHtml = $"{negocio}-{id}";
            var opcionHtml =
            $@"<li  title=¨{negocio.Singular()}: {nombre}¨>{Environment.NewLine}" +
            $@"  <a style=¨display: flex; padding-top: 2px;padding-bottom: 2px;¨>{Environment.NewLine}" +
            $@"    <img src=¨/images/menu/{icono}¨ style=¨margin-top: 6px;margin-right: 0px; width: 20px; height: 20px;¨  alt=¨{referencia}¨/>" +
            $@"    <input id='{idHtml}' type='button' class='menu-opcion' value='{referencia}' style=¨padding-left: 3px;¨ onclick =¨ArbolDeMenu.UrlSeleccionada('{url}', event)¨ />{Environment.NewLine}" +
            $@"  </a>" +
            $@"</li>{Environment.NewLine}";

            return opcionHtml.Replace("¨", "\""); ;
        }

        [Authorize]
        public IActionResult InicializarEntorno()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                ServicioDeCaches.EliminarTodas();
                InicializarEntornoInternal(Contexto);
                ViewBag.Mensaje = "Maestros inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                Contexto.RegistrarConEnvio(asunto: $"Error al {nameof(InicializarEntorno)}", error: GestorDeErrores.Detalle(e));
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
        }

        internal static void InicializarEntornoInternal(ContextoSe contexto)
        {
            InzNegocios.DefinirNegocios(contexto);
            InzVistas.DefinirVistas(contexto);
            InzMenus.DefinirMenus(contexto);
            InzTrabajos.SometerTrabajos(contexto);
        }

        [HttpPost]
        public JsonResult epGuardarMenuAccedido(int idUsuario)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGuardarMenuAccedido));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var body = ApiController.LeerBody(HttpContext);
                var usuario = Contexto.SeleccionarPorId<UsuarioDtm>(idUsuario);
                var idVista = body.parametros.LeerValor(ltrParametrosEp.IdVista, 0);
                var parametros = body.parametros.LeerValor(ltrParametrosEp.Parametros, "");
                List<ArbolDeMenuDto> menu = ((GestorDeArbolDeMenu)_GestorDeElementos).LeerArbolDeMenu(DatosDeConexion.Login);
                var funcion = parametros.IsNullOrEmpty()
                    ? menu.FirstOrDefault(opciones => opciones.IdVistaMvc == idVista && opciones.Parametros is null)
                    : menu.FirstOrDefault(opciones => opciones.IdVistaMvc == idVista && opciones.Parametros == parametros.ToString());
                if (funcion is not null)
                {
                    var opcionHtml = RenderAccionMenu(funcion);
                    var urlAccedida = body.parametros.LeerValor(ltrParametrosEp.urlAccedida, "");
                    GestorDeAccesosRecientes.GuardarMenuAccedido(Contexto, idMenu: funcion.Id, idVista, parametros, opcionHtml, urlAccedida);
                    r.Datos = null;
                }
                else
                {
                    r.Datos = null;
                }
                r.Consola = $"Opción de menú guardada correctamente";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"error al guardar la opción de menu accedida");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [HttpPost]
        public JsonResult epGuardarRegistroAccedido(int idUsuario)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGuardarRegistroAccedido));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var body = ApiController.LeerBody(HttpContext);
                var usuario = Contexto.SeleccionarPorId<UsuarioDtm>(idUsuario);
                var idVista = body.parametros.LeerValor(ltrParametrosEp.IdVista, 0);
                var urlAccedida = body.parametros.LeerValor(ltrParametrosEp.urlAccedida, "");
                var idElemento = body.parametros.LeerValor(ltrParametrosEp.idElemento, 0);
                var negocio = body.parametros.LeerValor(ltrParametrosEp.enumNegocio, enumNegocio.No_Definido);
                var esGenerica = urlAccedida.Contains(enumVistasNegocio.CrudDeEstados, StringComparison.CurrentCultureIgnoreCase) ||
                                 urlAccedida.Contains(enumVistasNegocio.CrudDeTransiciones, StringComparison.CurrentCultureIgnoreCase) ||
                                 urlAccedida.Contains(enumVistasNegocio.CrudDeAccionesDeTransicion, StringComparison.CurrentCultureIgnoreCase);
                if (negocio != enumNegocio.No_Definido && !esGenerica)
                {
                    var registro = negocio.LeerRegistro(Contexto, idElemento);
                    var referencia = ((IElementoDtm)registro).Referencia(Contexto);
                    var opcionHtml = RenderAccesoRegistro(negocio, idElemento, referencia, ((INombre)registro).Nombre, negocio.Icono(), urlAccedida);
                    GestorDeAccesosRecientes.GuardarRegistroAccedido(Contexto, $"{negocio}: {referencia}", idVista, registro.Id.ToString(), opcionHtml, urlAccedida);
                }
                r.Datos = null;
                r.Consola = $"Acceso a registro guardado correctamente";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"error al guardar el acceso al registro");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }
    }
}
