using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Terceros;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using Utilidades;
using System;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Controllers
{
    public class TrabajadoresController : EntidadController<ContextoSe, TrabajadorDtm, TrabajadorDto>
    {
        public TrabajadoresController(GestorDeTrabajadores gestorDeTrabajador, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeTrabajador,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudTrabajadores()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeTrabajadores(Contexto, ModoDescriptor.Mantenimiento));
        }

        public IActionResult CrudTerceros()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                return RenderMensaje("No se ha indicado el Trabajador del que se quiere obtener su detalle");
            try
            {
                var id = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                var Trabajador = GestorDeTrabajadores.LeerRegistroPorId(Contexto, id, aplicarJoin: true);
                var url = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                if (Trabajador.Interlocutor.IdPersona != null)
                    url = @$"{url}/Personas/CrudPersonas?Id={Trabajador.Interlocutor.IdPersona}";
                if (Trabajador.Interlocutor.IdSociedad != null)
                    url = @$"{url}/Sociedades/CrudSociedades?Id={Trabajador.Interlocutor.IdSociedad}";

                return Redirect(url);
            }
            catch (Exception e)
            {
                var m = e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true
                    ? e.Message
                    : $"Error al acceder a los datos del tercero";
                return RenderMensaje(m);
            }
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(TrabajadorDto elemento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(elemento);
            var Trabajador = GestorDeTrabajadores.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(elemento.Id);
            Trabajador.Telefono = elemento.Telefono;
            Trabajador.eMail = elemento.eMail;
            Trabajador.CopiarEn(elemento);
            return parametrosDeNegocio;
        }
    }
}
