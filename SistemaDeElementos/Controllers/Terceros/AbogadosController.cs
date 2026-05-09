using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using System;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class AbogadosController : EntidadController<ContextoSe, AbogadoDtm, AbogadoDto>
    {
        public AbogadosController(GestorDeAbogados gestorDeAbogado, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAbogado,
           gestorDeErrores
         )
        {
            if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto))
                return;
            if (!ExtensorDePleitos.ModuloActivo(Contexto))
                Emitir(ltrDePleitos.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudAbogados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeAbogados(Contexto, ModoDescriptor.Mantenimiento));
        }

        public IActionResult CrudTerceros()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                return RenderMensaje("No se ha indicado el Abogado del que se quiere obtener su detalle");
            try
            {
                var id = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                var abogado = GestorDeAbogados.LeerRegistroPorId(Contexto, id, aplicarJoin: true);
                var url = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                if (abogado.Interlocutor.IdPersona != null)
                    url = @$"{url}/Personas/CrudPersonas?Id={abogado.Interlocutor.IdPersona}";
                if (abogado.Interlocutor.IdSociedad != null)
                    url = @$"{url}/Sociedades/CrudSociedades?Id={abogado.Interlocutor.IdSociedad}";

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

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(AbogadoDto elemento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(elemento);
            var abogado = GestorDeAbogados.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(elemento.Id);
            abogado.Telefono = elemento.Telefono;
            abogado.eMail = elemento.eMail;
            abogado.CopiarEn(elemento);
            return parametrosDeNegocio;
        }
    }
}
