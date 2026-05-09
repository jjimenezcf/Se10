using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using Utilidades;
using AutoMapper;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using System;
using GestorDeElementos;
using System.Linq;
using Newtonsoft.Json;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Entorno;
using ModeloDeDto.Entorno;

namespace MVCSistemaDeElementos.Controllers
{
    public class AuditoriaController : BaseController<AuditoriaDto>
    {
        public AuditoriaController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }


        public IActionResult CrudDeAuditoria(string negocio)
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var negocioEnum = NegociosDeSe.ToEnumerado(negocio);
            var descriptor = new DescriptorDeAuditoria(Contexto, ModoDescriptor.Mantenimiento, negocioEnum);
            descriptor.GestorDeUsuario = GestorDeUsuarios.Gestor(Contexto, Mapeador);
            descriptor.UsuarioConectado = descriptor.GestorDeUsuario.LeerRegistroCacheado(nameof(UsuarioDtm.Login), DatosDeConexion.Login, errorSiNoHay: true, errorSiHayMasDeUno: true, aplicarJoin: false);

            var destino = $"../{descriptor.RutaBase}/{descriptor.Vista}";
            //if (!this.ExisteLaVista(destino))
            //    return RenderMensaje($"La vista {destino} no está definida");

            string nombreDeLaVista = ControllerContext.RouteData.Values["action"].ToString();
            string nombreDelControlador = ControllerContext.RouteData.Values["controller"].ToString();

            if (!descriptor.UsuarioConectado.EsAdministrador)
            {
                try
                {
                    var hayPermisos = descriptor.GestorDeUsuario.TienePermisoFuncional(descriptor.UsuarioConectado, $"{nombreDelControlador}.{nombreDeLaVista}");
                    if (!hayPermisos)
                        GestorDeErrores.Emitir($"Solicite permisos de acceso a {destino}");

                    hayPermisos = descriptor.GestorDeUsuario.TienePermisoDeDatos(enumModoDeAccesoDeDatos.Consultor, negocioEnum);
                    if (!hayPermisos)
                        GestorDeErrores.Emitir($"Solicite al menos permisos de consulta sobre los elementos de negocio {descriptor.Negocio.ToNombre()}");
                }
                catch(Exception e)
                {
                    return RenderMensaje(e.Message);
                }
            }
            ViewBag.DatosDeConexion = DatosDeConexion;
            return base.View(destino, descriptor);
        }


        [HttpPost]
        public JsonResult epLeerDatosPost(string modo, string accion, string posicion, string cantidad)
        {
            var body = ApiController.LeerBody(HttpContext);
            return LeerDatosParaElGrid(modo, accion, posicion, cantidad, body.parametros.LeerValor<string>(ltrParametrosEp.Filtro), body.parametros.LeerValor<string>(ltrParametrosEp.Orden));
        }

        private JsonResult LeerDatosParaElGrid(string modo, string accion, string posicion, string cantidad, string filtro, string orden)
        {
            var r = new Resultado();
            int pos = posicion.Entero();
            int can = cantidad.Entero();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                //if (!Contexto.DatosDeConexion.EsAdministrador)
                //    GestorDeErrores.Emitir("No tiene acceso a los datos de auditoría");

                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
                var restrictor = ObtenerRestrictores(filtros);
                var negocioDtm = GestorDeNegocios.LeerNegocio(Contexto, restrictor.idNegocio);
                var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, NegociosDeSe.ToEnumerado(negocioDtm.Nombre), restrictor.idElemento);
                if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso a los datos de auditoría del negocio {negocioDtm.Nombre}");

                var datos = ApiController.LeerDatosParaElGrid(
                    () => AuditoriaDeNegocio.LeerElementos(Contexto, NegociosDeSe.ToEnumerado(negocioDtm.Nombre), restrictor.idElemento, restrictor.usuarios, pos, can)
                  , () => AuditoriaDeNegocio.ContarElementos(Contexto, NegociosDeSe.ToEnumerado(negocioDtm.Nombre), restrictor.idElemento, restrictor.usuarios));

                var infoObtenida = new ResultadoDeLectura<AuditoriaDto>();
                infoObtenida.registros = datos.elementos.ToList(); 
                infoObtenida.total = datos.total;
                infoObtenida.posicion = pos;
                infoObtenida.cantidad = can;
                r.Datos = infoObtenida;
                r.Estado = enumEstadoPeticion.Ok;
                if (pos > 0 && datos.elementos.Count() == 0)
                    r.Mensaje = "No hay más elementos";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se ha podido recuperar datos para el grid.");
            }

            var a = new JsonResult(r);
            return a;
        }

        protected override AuditoriaDto LeerPorId(int id, Dictionary<string,object> parametros)
        {
            if (!parametros.ContieneClave(NegocioPor.idNegocio))
                GestorDeErrores.Emitir("Debe definir el negocio del que ha de leerse la auditoría");
            if (!parametros.ContieneClave(nameof(AuditoriaDto.IdElemento)))
                GestorDeErrores.Emitir("Debe indicar el elemento del que se quiere visualizar la auditoría");

            var id32 = (int) parametros.LeerValor<long>(NegocioPor.idNegocio);
            var idElemento = (int) parametros.LeerValor<long>(nameof(AuditoriaDto.IdElemento));

            var negocioDtm = GestorDeNegocios.LeerNegocio(Contexto, id32);
            var negocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocioDtm.Enumerado);
            
            if (!Contexto.DatosDeConexion.EsAdministrador) {
                var ma = ApiDePermisos.LeerModoDeAcceso(Contexto, negocio, idElemento);
                if (ma != enumModoDeAccesoDeDatos.Administrador)
                    GestorDeErrores.Emitir("No tiene acceso a los datos de auditoría");
            }

            return AuditoriaDeNegocio.LeerElemento(Contexto,negocio, id);
        }

        private static (int idNegocio, int idElemento, List<int> usuarios) ObtenerRestrictores(List<ClausulaDeFiltrado> filtros)
        {
            var restrictores = ApiController.ObtenerNegocioYelemento(filtros);
            var usuarios = new List<int>();
            foreach (var f in filtros.Where(f => f.Clausula == UsuariosPor.AlgunUsuario))
            {
                usuarios.Incluir(f.Valor);
            }

            return (restrictores.idNegocio, restrictores.idElemento, usuarios);
        }

    }

}
