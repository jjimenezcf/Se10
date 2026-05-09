using Microsoft.AspNetCore.Mvc;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using ModeloDeDto.Seguridad;
using GestorDeElementos;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{

    public class PermisosController : EntidadController<ContextoSe, PermisoDtm, PermisoDto>
    {
        public PermisosController(GestorDePermisos gestorDePermisos, GestorDeErrores gestorDeErrores)
        : base
        (
         gestorDePermisos,
         gestorDeErrores
        )
        {
        }

        public IActionResult CrudPermiso()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePermiso(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_CrearElemento(PermisoDto elemento)
        {
            var a = base.AntesDeEjecutar_CrearElemento(elemento);
            GestorDeErrores.Emitir("Los permisos solo los puede crear el sistema");
            return a;
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_BorrarPorId(PermisoDto elemento)
        {
            var a = base.AntesDeEjecutar_BorrarPorId(elemento);
            GestorDeErrores.Emitir("Los permisos solo los puede borrar el sistema");
            return a;
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarPorId(PermisoDto elemento, Dictionary<string, object> parametros)
        {
            var a = base.AntesDeEjecutar_ModificarPorId(elemento, parametros);
            GestorDeErrores.Emitir("Los permisos solo los puede modificar el sistema");
            return a;
        }


        //protected override dynamic CargarLista(string claseElemento, enumNegocio negocio, List<ClausulaDeFiltrado> filtros)
        //{
        //    if (claseElemento == nameof(ClasePermisoDto))
        //        return ((GestorDePermisos)_GestorDeElementos).LeerClases();

        //    if (claseElemento == nameof(TipoPermisoDto))
        //        return ((GestorDePermisos)_GestorDeElementos).LeerTipos();

        //    return base.CargarLista(claseElemento, negocio, filtros);
        //}

    }
}
