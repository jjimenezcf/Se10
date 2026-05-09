using Gestor.Errores;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class MenusController : EntidadController<ContextoSe, MenuDtm, MenuDto>
    {
        public GestorDeMenus GestorDeMenus { get; set; }

        public MenusController(GestorDeMenus gestorDeMenus, GestorDeErrores gestorDeErrores)
        : base
        (
          gestorDeMenus,
          gestorDeErrores          
        )
        {
            GestorDeMenus = gestorDeMenus;
        }

        public IActionResult CrudMenu()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeMenu(Contexto, ModoDescriptor.Mantenimiento));
        }


        protected override dynamic CargarLista(string claseElemento, enumNegocio negocio, List<ClausulaDeFiltrado> filtro)
        {
            if (claseElemento == nameof(MenuDto))
                return ((GestorDeMenus)_GestorDeElementos).LeerPadres();

            return base.CargarLista(claseElemento, negocio, filtro);
        }


        public JsonResult epSeleccionarIa()
        {
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var body = ApiController.LeerBody(HttpContext);
                string iaSeleccionada = body.parametros.LeerValor(ltrParametrosEp.IaSeleccionada, ""); 
                enumIa ia = ApiDeEnsamblados.ToEnumerado<enumIa>(iaSeleccionada);
                GestorDeMenus.SeleccionarIa(ia);
                r.Consola = $"Ia seleccionada correctamente.";
                r.Datos = null;
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al seleccionar la Ia.");
            }
            return new JsonResult(r);
        }

    }
}