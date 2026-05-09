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
using System.Collections.Generic;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;

namespace MVCSistemaDeElementos.Controllers
{
    public class InterlocutoresController : EntidadController<ContextoSe, InterlocutorDtm, InterlocutorDto>
    {
        public InterlocutoresController(GestorDeInterlocutores gestorDeInterlocutor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeInterlocutor,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudInterlocutores()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeInterlocutores(Contexto, ModoDescriptor.Mantenimiento));
        }

        public IActionResult CrudTerceros()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                return RenderMensaje("No se ha indicado el interlocutor del que se quiere obtener su detalle");
            try
            {
                var id = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                var inter = GestorDeInterlocutores.LeerRegistroPorId(Contexto, id);
                var url = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                if (inter.IdPersona != null)
                    url = @$"{url}/Personas/CrudPersonas?Id={inter.IdPersona}";
                if (inter.IdSociedad != null)
                    url = @$"{url}/Sociedades/CrudSociedades?Id={inter.IdSociedad}";

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

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(InterlocutorDto elemento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(elemento);
            var inter = GestorDeInterlocutores.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(elemento.Id);
            inter.Telefono = elemento.Telefono;
            inter.eMail = elemento.eMail;
            inter.CopiarEn(elemento);
            return parametrosDeNegocio;
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Procuradores:
                    return GestorDeProcuradores.CrearProcuradores(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                case eventosDeMf.Abogados:
                    return GestorDeAbogados.CrearAbogados(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                case eventosDeMf.Proveedor:
                    return GestorDeProveedores.CrearProveedores(Contexto, (List<int>)parametros[ltrParametrosEp.ids],
                        Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Proveedores).Id);
                case eventosDeMf.Cliente:
                    return GestorDeClientes.CrearClientes(Contexto, (List<int>)parametros[ltrParametrosEp.ids],
                        Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes).Id);
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epLeerCuentaDeIngreso(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza(nameof(epLeerCuentaDeIngreso));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idInterlocutor = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.id));
                r.Datos = GestorDeInterlocutores.LeerCuentaDeIngreso(Contexto,idInterlocutor);
                r.Consola = $"Cuenta de ingreso del interlocutor leida correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer los datos de la cuenta de ingreso del interlocutor.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }

}

