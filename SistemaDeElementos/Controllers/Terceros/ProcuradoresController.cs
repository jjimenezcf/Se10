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
    public class ProcuradoresController : EntidadController<ContextoSe, ProcuradorDtm, ProcuradorDto>
    {
        public ProcuradoresController(GestorDeProcuradores gestorDeProcurador, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeProcurador,
           gestorDeErrores
         )
        {
            if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto))
                return;

            if (!ExtensorDePleitos.ModuloActivo(Contexto))
                Emitir(ltrDePleitos.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudProcuradores()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeProcuradores(Contexto, ModoDescriptor.Mantenimiento));
        }

        public IActionResult CrudTerceros()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                return RenderMensaje("No se ha indicado el Procurador del que se quiere obtener su detalle");
            try
            {
                var id = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                var procurador = GestorDeProcuradores.LeerRegistroPorId(Contexto, id, aplicarJoin: true);
                var url = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                if (procurador.Interlocutor.IdPersona != null)
                    url = @$"{url}/Personas/CrudPersonas?Id={procurador.Interlocutor.IdPersona}";
                if (procurador.Interlocutor.IdSociedad != null)
                    url = @$"{url}/Sociedades/CrudSociedades?Id={procurador.Interlocutor.IdSociedad}";

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

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(ProcuradorDto elemento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(elemento);
            var procurador = GestorDeProcuradores.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(elemento.Id);
            procurador.Telefono = elemento.Telefono;
            procurador.eMail = elemento.eMail;
            procurador.CopiarEn(elemento);
            return parametrosDeNegocio;
        }
    }
}
