using Microsoft.AspNetCore.Mvc;
using Gestor.Errores;
using System;
using ServicioDeDatos;
using Microsoft.AspNetCore.Authorization;
using ModeloDeDto.Entorno;
using System.Security.Claims;
using AutoMapper;
using MVCSistemaDeElementos.Descriptores;
using SistemaDeElementos.Controllers.Seguridad;
using System.Threading.Tasks;

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
