using Microsoft.AspNetCore.Mvc;
using System;
using Gestor.Errores;
using ServicioDeDatos;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using ModeloDeDto.Negocio;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Controllers
{
    [Authorize]
    public class JerarquiaController<TContexto> : FormularioController<TContexto>
    where TContexto : ContextoSe
    {

        public JerarquiaController(TContexto contexto, IMapper mapeador, GestorDeErrores gestorErrores)
        : base(contexto, mapeador, gestorErrores)
        {
        }

        public virtual JsonResult epLeerJerarquia(string negocio, int idPadre, string filtrosJson)
        {
            throw new Exception($"Debe definir en el controlador el método {nameof(epLeerJerarquia)}");
        }

        protected JsonResult PersistirElemento(string negocio, string operacion, Func<ElementoDto> persistir)
        {
            var r = new Resultado();

            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var elementoDto = persistir();
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"Elemento del {negocio} {operacion}";
                r.Datos = elementoDto;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, $"No se ha podido {operacion}.");
            }
            return new JsonResult(r);
        }

        protected JsonResult LeerJerarquia(string negocio, int idPadre, Func<JerarquiaDto> leer)
        {
            var r = new Resultado();

            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var jerarquiaDto = leer();
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"Jerarquía leida desde el padre {idPadre}";
                r.Datos = jerarquiaDto;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, $"No se ha podido leer la jerarquía del negocio {negocio}.");
            }

            return new JsonResult(r);
        }
        public JsonResult LeerNodoSeleccionado(string negocio, int id, Func<ElementoDto> leer)
        {
            var r = new Resultado();

            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var jerarquiaDto = leer();
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = $"El nodo se ha leido";
                r.Datos = jerarquiaDto;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, $"No se ha podido leer el nodo con id {id} del negocio {negocio}.");
            }

            return new JsonResult(r);
        }
    }
}
