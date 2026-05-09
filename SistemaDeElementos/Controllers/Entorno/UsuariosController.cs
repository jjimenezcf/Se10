using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using System;
using GestorDeElementos;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class UsuariosController : EntidadController<ContextoSe, UsuarioDtm, UsuarioDto>
    {
        class UsuarioDeConexion
        {
            public int id { get; set; }
            public string login { get; set; }
            public string mail { get; set; }
            public string administrador { get; set; }
            public string parametrizador { get; set; }            
            public string clienteWeb { get; set; }
            public int idagenda { get; set; }
            public string nombreagenda { get; set; }
        }

        public UsuariosController(GestorDeUsuarios gestorDeUsuarios, GestorDeErrores gestorDeErrores)
        : base
        (
          gestorDeUsuarios,
          gestorDeErrores
        )
        {

        }


        public IActionResult CrudUsuario()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeUsuario(Contexto, ModoDescriptor.Mantenimiento));
        }

        public JsonResult epLeerUsuarioDeConexion()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                var usuario = _GestorDeElementos.LeerRegistroPorId(_GestorDeElementos.Contexto.DatosDeConexion.IdUsuario, true, false, false, aplicarJoin: false);
                r.Consola = $"registro de usuario de conexión leido correctamente";
                r.Estado = enumEstadoPeticion.Ok;
                r.Datos = new UsuarioDeConexion()
                {
                    login = usuario.Login,
                    id = usuario.Id,
                    mail = usuario.eMail,
                    administrador = DatosDeConexion.EsAdministrador ? "S" : "N",
                    parametrizador = Contexto.SePuedeParametrizar() ? "S" : "N",
                    clienteWeb = DatosDeConexion.EsClienteWeb ? "S" : "N",
                    idagenda = usuario.IdAgenda,
                    nombreagenda = Contexto.SeleccionarPorId<AgendaDtm>(usuario.IdAgenda).Nombre
                };
            }
            catch (Exception e)
            {
                r.Estado = enumEstadoPeticion.Error;
                r.Consola = GestorDeErrores.Detalle(e);
                r.Mensaje = $"Error al leer el usuario de conexión. {(e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true ? e.Message : "")}";
            }

            return new JsonResult(r);
        }

        public JsonResult epSubirMiCertificado(int idUsuario, int idArchivo, string password)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(nameof(epSubirMiCertificado));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                GestorDeUsuarios.SubirMiCertificado(_GestorDeElementos.Contexto, idUsuario, idArchivo, password);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Certificado subido";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al subir el certificado.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }

            return new JsonResult(r);
        }

        public JsonResult epCambiarPassword(int idUsuario, string actual, string nueva, string repetida)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(nameof(epCambiarPassword));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                GestorDeUsuarios.CambiarPassword(_GestorDeElementos.Contexto, idUsuario, actual, nueva, repetida);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "password cambiada";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al cambiar la password.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }

            return new JsonResult(r);
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.MiCertificado:
                    if (Contexto.DatosDeConexion.IdUsuario != (int)parametros.LeerValor(nameof(ltrParametrosEp.idElemento), 0))
                        GestorDeErrores.Emitir($"No puede añadir el certificado personal del usuario seleccionado ({Contexto.DatosDeConexion.IdUsuario},{(int)parametros.LeerValor(nameof(ltrParametrosEp.idElemento), 0)})");
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        protected override void AntesDeEjecutar_Leer(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> ordenes, Dictionary<string, object> parametros)
        {
            base.AntesDeEjecutar_Leer(posicion, cantidad, filtros, ordenes, parametros);
            if (Contexto.DatosDeConexion.EsClienteWeb)
                filtros.Add(new ClausulaDeFiltrado(ltrUsuariosDeUnCliente.ExcluirUsuariosDeCliente, enumCriteriosDeFiltrado.igual, true));
        }

    }

}
