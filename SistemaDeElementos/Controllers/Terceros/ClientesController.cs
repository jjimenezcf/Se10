using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class ClientesController : EntidadController<ContextoSe, ClienteDtm, ClienteDto>
    {
        public ClientesController(GestorDeClientes gestorDeCliente, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCliente,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudClientes()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeClientes).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Terceros}/{nameof(CrudClientes)}";
                    return base.View(destino, new DescriptorDeClientes(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<ClienteDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeClientes(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudTerceros()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                return RenderMensaje("No se ha indicado el Cliente del que se quiere obtener su detalle");
            try
            {
                var id = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                var Cliente = GestorDeClientes.LeerRegistroPorId(Contexto, id, aplicarJoin: true);
                var url = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                if (Cliente.Interlocutor.IdPersona != null)
                    url = @$"{url}/Personas/CrudPersonas?Id={Cliente.Interlocutor.IdPersona}";
                if (Cliente.Interlocutor.IdSociedad != null)
                    url = @$"{url}/Sociedades/CrudSociedades?Id={Cliente.Interlocutor.IdSociedad}";

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

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(ClienteDto elemento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(elemento);
            var Cliente = GestorDeClientes.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(elemento.Id);
            Cliente.Telefono = elemento.Telefono;
            Cliente.eMail = elemento.eMail;
            Cliente.CopiarEn(elemento);
            return parametrosDeNegocio;
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            var idCliente = (List<int>)parametros[ltrParametrosEp.ids];
            if (idCliente.Count != 1) GestorDeErrores.Emitir("Debe indicar un cliente");
            var cliente = Contexto.SeleccionarPorId<ClienteDtm>(idCliente[0]);
            switch (opcion)
            {
                case eventosDeMf.Cli_PuestoDeTrabajo:
                    if (!cliente.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir("La definición de 'puestos de trabajo' sólo se puede hacerse con permisos de administración");
                    return null;
                case eventosDeMf.Cli_NuevoClienteWeb:
                    if (!cliente.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir("La definición de 'clientes web' sólo se puede hacerse con permisos de administración");
                    return null;
                case eventosDeMf.Cli_AsociarClienteWeb:
                    if (!cliente.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir("La definición de 'clientes web' sólo se puede hacerse con permisos de administración");
                    return null;
                case eventosDeMf.Cli_CentroAdministrativo:
                    if (!cliente.EsInterventor(Contexto))
                        GestorDeErrores.Emitir("La definición de 'clientes web' sólo se puede hacerse con permisos de administración");
                    return null;
                case eventosDeMf.Cli_ValidarNif:
                    var ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de una sociedad");
                    ((GestorDeClientes)_GestorDeElementos).ValidarClienteEnAeat(ids[0]);
                    return new Resultado { Mensaje = "Cliente validado en la AEAT" , Estado = enumEstadoPeticion.Ok } ;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }
    }
}
