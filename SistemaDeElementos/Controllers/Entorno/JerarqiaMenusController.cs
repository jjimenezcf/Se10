using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using GestorDeElementos;
using System.Collections.Generic;
using ModeloDeDto.Entorno;
using ModeloDeDto;
using GestoresDeNegocio.Entorno;

namespace MVCSistemaDeElementos.Controllers
{
    public class JerarqiaMenusController : JerarquiaController<ContextoSe>
    {
        private GestorDeMenus gestorDeMenus;

        public JerarqiaMenusController(GestorDeMenus gestor, GestorDeErrores gestorDeErrores)
         : base(gestor.Contexto
               ,gestor.Mapeador
               , gestorDeErrores)
        {
            gestorDeMenus = gestor;
        }


        public IActionResult CrudJerarqiaMenus()
        {
            try
            {
                return ViewFormulario(new DescriptorDeJerarqiaMenus(gestorDeMenus, "menu", "Menus del SE", nameof(JerarqiaMenusController), nameof(CrudJerarqiaMenus)));
            }
            catch (Exception e)
            {
                return RenderMensaje(GestorDeErrores.Mensaje(e));
            }
        }

        public override JsonResult epLeerJerarquia(string negocio, int idPadre, string filtrosJson)
        {
            return LeerJerarquia(negocio, idPadre, () => GestorDeMenus.LeerJerarquia(Contexto, idPadre, filtrosJson));
        }

        public JsonResult epCrearNodo(string negocio, string json)
        {
           return PersistirElemento(negocio, enumTipoOperacion.Insertar.ToString(), () => GestorDeMenus.PersistirMenuJson(Contexto, json, new ParametrosDeNegocio(enumTipoOperacion.Insertar)));
        }

        public JsonResult epPersistirNodo(string negocio, string json, string operacion)
        {
            return PersistirElemento(negocio, operacion.ToTipoOperacion().ToString(), () => GestorDeMenus.PersistirMenuJson(Contexto, json, new ParametrosDeNegocio(operacion.ToTipoOperacion())));
        }

        protected override IEnumerable<MenuDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            return ApiController.LeerElementos(gestorDeMenus, posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }
        public JsonResult epLeerNodoSeleccionado(string negocio, int id, string filtrosJson)
        {
            var parametros = new Dictionary<string, object>();
            parametros[ltrParametrosDto.DescargarGestionDocumental] = true;
            return LeerNodoSeleccionado(negocio, id, () => gestorDeMenus.LeerElementoPorId(id, parametros));
        }
    }
}
