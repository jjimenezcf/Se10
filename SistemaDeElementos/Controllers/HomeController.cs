using AutoMapper;
using Dapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using SistemaDeElementos.Controllers.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilidades;
using static ServicioDeDatos.Literal;
using static SistemaDeElementos.Inicializador.enumVistas;

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
                var conexion = Contexto.Database.GetDbConnection();
                var transaccion = Contexto.Database.CurrentTransaction?.GetDbTransaction();

                var resultado = new List<object>();

                foreach (var negocio in Enum.GetValues<enumNegocio>())
                {
                    if (negocio == enumNegocio.No_Definido) continue;
                    if (!negocio.UsaTipo() || !negocio.UsaEstado()) continue;

                    var metadatos = negocio.ObtenerMetadatos(emitirError: false);
                    if (metadatos?.TipoDtm == null || metadatos?.EstadoDtm == null) continue;

                    try
                    {
                        var tablaElementos = ApiDeRegistroDtm.EsquemaTabla(negocio.TipoDtm());
                        var tablaEstados = ApiDeEstado.TablaDeEstados(negocio.TipoDtm());
                        var tablaTipos = ApiDeRegistroDtm.EsquemaTabla(metadatos.TipoDtm);

                        var tipos = conexion.Query<ItemNombreId>(
                            $"SELECT ID as Id, NOMBRE as Nombre FROM {tablaTipos} ORDER BY NOMBRE",
                            null, transaccion).ToList();

                        var estados = conexion.Query<ItemNombreId>(
                            $"SELECT ID as Id, NOMBRE as Nombre FROM {tablaEstados} ORDER BY ORDEN",
                            null, transaccion).ToList();

                        var celdas = conexion.Query<CeldaDeMatriz>(
                            $"SELECT ID_TIPO as IdTipo, ID_ESTADO as IdEstado, COUNT(*) AS Cantidad " +
                            $"FROM {tablaElementos} " +
                            $"WHERE ID_TIPO IS NOT NULL AND ID_ESTADO IS NOT NULL " +
                            $"GROUP BY ID_TIPO, ID_ESTADO",
                            null, transaccion).ToList();

                        resultado.Add(new
                        {
                            nombre = negocio.Singular(),
                            enumerado = negocio.ToString(),
                            numTipos = tipos.Count,
                            numEstados = estados.Count,
                            tipos,
                            estados,
                            celdas
                        });
                    }
                    catch
                    {
                        // ignorar negocios con errores de consulta individuales
                    }
                }

                r.Datos = resultado;
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

        private class ItemNombreId
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        private class CeldaDeMatriz
        {
            public int IdTipo { get; set; }
            public int IdEstado { get; set; }
            public int Cantidad { get; set; }
        }
    }
}
