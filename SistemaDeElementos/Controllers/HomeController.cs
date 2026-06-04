using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using SistemaDeElementos.Controllers.Seguridad;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class HomeController : BaseController<UsuarioDto>
    {

        public HomeController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores):
        base(gestorDeErrores, contexto, mapeador)
        {
        }

        [Authorize]
        public IActionResult Index()
        {

            if (Contexto.AsignarLogin(ApiController.ObtenerUsuarioDeLaRequest(HttpContext), emitirError: false) != null)
            {
                ViewBag.DatosDeConexion = DatosDeConexion;
            }
            return PanelDeControl();
        }

        protected IActionResult PanelDeControl()
        {
            var claimsDeUsuario = HttpContext.User;
            var login = claimsDeUsuario.FindFirstValue(nameof(UsuarioDto.Login));
            try
            {
                Contexto.AsignarLogin(login, emitirError: false);

                if (DatosDeConexion.IdUsuario == 0)
                    return
                        Task.Run(() => new AccesoController(Contexto, new GestoresDeNegocio.Entorno.GestorDeUsuarios(Contexto, Contexto.Mapeador), GestorDeErrores).Logout()).Result;

                if (!Contexto.Usuario.Activo)
                    throw new Exception($"El usuario {login} no está activo");

                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return VistaDelPanelDeControl(Contexto);

            }
            catch(Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        [HttpPost]
        public JsonResult epGrabarGraficasDeNegocio()
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGrabarGraficasDeNegocio));
            var peticion = eventosDeMf.Comun_GuardarDisposicionDashBoard.ToString();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var body = ApiController.LeerBody(HttpContext);
                r.Datos = ProcesarPeticion(enumNegocio.Negocio, vista: null, peticion, body.parametros);
                r.Consola = $"Petición '{peticion}' procesada correctamente";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al procesar la petición '{peticion}'.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        [HttpGet]
        public JsonResult epGraficasDeNegocio()
        {
            var r = new Resultado();
            try
            {
                r.Datos = NegociosDeSe.LeerInformacioParaDashBoard(Contexto);
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Mensaje = e.Message;
            }
            return Json(r);
        }

        public IActionResult About()
        {
            try
            {
                int[] a = { 2, 4 };
                var b = 0;
                b = a[5];
                ViewData["Message"] = "Your application description page.";
            }
            catch(Exception e)
            {

                return Error(e);
            }

            return View();
        }

        public IActionResult Contact()
        {
            int[] a = { 2, 4 };
            var b = 0;
            b = a[5];

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public JsonResult epSolicitarModalHtml(string modal, string parametrosJson)
        {
            var r = new ResultadoHtml();
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);


            PanelDeControl layout = new PanelDeControl(Contexto, "layout-Se");
            try
            {
                string modalHtml = @$"";
                switch (modal)
                {
                    case "cambiar-password":
                        r.Html = layout.RenderModalCambiarPassword().Replace("¨", "\"");
                        break;
                    case "subir-certificado":
                        r.Html = layout.RenderModalSubirCertificado().Replace("¨", "\"");
                        break;
                    case "modal-ia":
                        r.Html = layout.RenderModalIa().Replace("¨", "\"");
                        break;
                }
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                r.Mensaje = "No se ha podido leer el menú";
            }
            return new JsonResult(r);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(Exception e)
        {
            return RenderMensaje($"Se ha producido un error.{Environment.NewLine}{e.Message}");
        }
    }
}
