using Microsoft.AspNetCore.Mvc;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore.Authorization;
using ServicioDeDatos.Entorno;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using ModeloDeDto.Entorno;

namespace MVCSistemaDeElementos.Controllers
{
    [Authorize]
    public class FormularioController<TContexto> : BaseController<UsuarioDto>
    where TContexto : ContextoSe
    {

        public FormularioController(TContexto contexto, IMapper mapeador,  GestorDeErrores gestorErrores)
        : base(gestorErrores, contexto, mapeador)
        {
        }

        public IActionResult Index()
        {
            return View();
        }


        public ViewResult ViewFormulario(DescriptorDeFormulario formulario)
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            formulario.GestorDeUsuario = GestorDeUsuarios.Gestor(Contexto, Mapeador);
            formulario.UsuarioConectado = formulario.GestorDeUsuario.LeerRegistroCacheado(nameof(UsuarioDtm.Login), DatosDeConexion.Login, errorSiNoHay: true, errorSiHayMasDeUno: true, aplicarJoin: false);
            ViewBag.DatosDeConexion = DatosDeConexion;

            var destino = $"{$"../{formulario.RutaVista}/"}{formulario.Vista}";
            if (!this.ExisteLaVista(destino))
                return RenderMensaje($"La vista {destino} no está definida");

            if (!formulario.UsuarioConectado.EsAdministrador)
            {
                string nombreDeLaVista = ControllerContext.RouteData.Values["action"].ToString();
                string nombreDelControlador = ControllerContext.RouteData.Values["controller"].ToString();
                var hayPermisos = formulario.GestorDeUsuario.TienePermisoFuncional(formulario.UsuarioConectado, $"{nombreDelControlador}.{nombreDeLaVista}");
                if (!hayPermisos)
                    return RenderMensaje($"Solicite permisos de acceso a {destino}");
            }

            return base.View(destino, formulario);
        }

        ///// <summary>
        ///// END-POIN: desde el ApiDeArchivos. Sube un fichero al gestor documental o a la ruta indicada
        ///// </summary>
        ///// <param name="fichero">fichero a subir</param>
        ///// <param name="rutaDestino">si no se sube al gestor documenta, nombre de la ruta donde se almacenará</param>
        ///// <param name="extensionesValidas">extensiones que ha de tener el archivo a subir</param>
        ///// <returns>0 si no ha subido al gestor documental, o id del archivo subido al gestor documental</returns>
        //[HttpPost]
        //public JsonResult epSubirArchivo(IFormFile fichero, string rutaDestino, string extensionesValidas)
        //{
        //    return SubirArchivo(Contexto, Mapeador, HttpContext, fichero, rutaDestino, extensionesValidas);
        //}

    }
}
