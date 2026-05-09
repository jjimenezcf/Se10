using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Utilidades;
using static Utilidades.Ampliaciones;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeUsuarios
    {
        public static IQueryable<RolDtm> LeerRoles(this UsuarioDtm usuario, ContextoSe contexto)
        {
            var rolesDeUnUsuario =
                  contexto.Set<RolDtm>().
                  Join(contexto.Set<RolesDeUnPuestoDtm>(), rol => rol.Id, puestosDeUnRol => puestosDeUnRol.IdRol, (t1, t2) => new { t1, t2 }).
                  Join(contexto.Set<PuestosDeUnUsuarioDtm>(), t => t.t2.IdPuesto, t3 => t3.IdPuesto, (t, t3) => new { t.t1, t.t2, t3 }).
                  Where(t => t.t3.IdUsuario == usuario.Id).
                  Select(t => t.t1);

            return rolesDeUnUsuario;
        }

        public static IQueryable<RolDtm> LeerRoles(this ContextoSe contexto)
        {
            var rolesDeUnUsuario =
                  contexto.Set<RolDtm>().
                  Join(contexto.Set<RolesDeUnPuestoDtm>(), rol => rol.Id, puestosDeUnRol => puestosDeUnRol.IdRol, (t1, t2) => new { t1, t2 }).
                  Join(contexto.Set<PuestosDeUnUsuarioDtm>(), t => t.t2.IdPuesto, t3 => t3.IdPuesto, (t, t3) => new { t.t1, t.t2, t3 }).
                  Where(t => t.t3.IdUsuario == contexto.DatosDeConexion.IdUsuario).
                  Select(t => t.t1);

            return rolesDeUnUsuario;
        }

        public static void AsignarAccesoAlCgDe(this RolDtm rol, ContextoSe contexto, IElementoDeProcesoDtm registro, enumModoDeAccesoDeDatos modo)
        {
            var negocioDeUnCg = contexto.SeleccionarTodos<NegociosDeUnCgDtm>(new Dictionary<string, object> {
                { nameof(NegociosDeUnCgDtm.IdCg), registro.IdCg },
                { nameof(NegociosDeUnCgDtm.IdNegocio), registro.GetType().NegocioDeUnDtm().IdNegocio() }
            });
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, modo == enumModoDeAccesoDeDatos.Gestor ? negocioDeUnCg[0].IdGestor : negocioDeUnCg[0].IdConsultor);
        }

        public static void ValidarQueSePuedeIncluirAlUsuarioEnPuesto(this PuestosDeUnUsuarioDtm puestoDeUnUsuario, ContextoSe contexto)
        {
            var usuario = contexto.SeleccionarPorId<UsuarioDtm>(puestoDeUnUsuario.IdUsuario);
            var puesto = contexto.SeleccionarPorId<PuestoDtm>(puestoDeUnUsuario.IdPuesto);

            if (usuario.EsDeCliente(contexto, usarLaCache: false))
            {
                if (!puesto.EsDelCliente(contexto, usuario.Cliente(contexto)))
                {
                    if (puesto.Usuarios(contexto).Where(x => !x.EsDeCliente(contexto, usarLaCache: false)).Count() > 0)
                        GestorDeErrores.Emitir($"Por ser el usuario '{usuario.Login}' del cliente '{usuario.Cliente(contexto).Referencia(contexto)}' el puesto de trabajo '{puesto.Expresion}' ha de ser del mismo cliente, o todos sus usuarios han de ser de clientes");
                }

                var clientePt = puesto.Cliente(contexto, errorSiNoHay: false);
                if (clientePt is not null)
                {
                    var clienteUsuario = usuario.Cliente(contexto);
                    if (clientePt.Id != clienteUsuario.Id)
                        GestorDeErrores.Emitir($"Por ser el puesto '{puesto.Expresion}' del cliente '{clientePt.Referencia(contexto)}' el usuario '{usuario.Login}' ha de ser del mismo cliente, y es del '{clienteUsuario.Referencia(contexto)}'");
                }
            }
            else
            {
                if (puesto.EsDeCliente(contexto))
                    GestorDeErrores.Emitir($"Ha un usuario que no es del cliente '{puesto.Cliente(contexto).Referencia(contexto)}' no se le puede asociar el puesto de cliente '{puesto.Expresion}' ");

            }

        }

        public static List<PuestoDtm> Puestos(this UsuarioDtm usuario, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_PuestosDeUnUsuario);
            if (!cache.ContainsKey(usuario.Id.ToString()))
            {
                cache[usuario.Id.ToString()] = contexto.SeleccionarTodos<PuestoDtm>(new Dictionary<string, object> { { nameof(PuestosDeUnUsuarioDtm.IdUsuario), usuario.Id } });
            }
            return (List<PuestoDtm>)cache[usuario.Id.ToString()];
        }

        public static List<UsuarioDtm> Usuarios(this PuestoDtm puesto, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_UsuariosDeUnPuesto);
            if (!cache.ContainsKey(puesto.Id.ToString()))
            {
                cache[puesto.Id.ToString()] = contexto.SeleccionarTodos<UsuarioDtm>(new Dictionary<string, object> { { nameof(PuestosDeUnUsuarioDtm.IdPuesto), puesto.Id } });
            }
            return (List<UsuarioDtm>)cache[puesto.Id.ToString()];
        }

        public static List<UsuarioDtm> UsuariosDeUnPuesto(ContextoSe contexto, int idPuesto)
        =>
        contexto.SeleccionarPorId<PuestoDtm>(idPuesto).Usuarios(contexto);

        public static ClienteDtm Cliente(this UsuarioDtm usuario, ContextoSe contexto, bool errorSiNoHay = true, bool usarLaCache = true)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_UsuarioDeCliente);
            if (!usarLaCache || !cache.ContainsKey(usuario.Id.ToString()))
            {
                var usuarioDeCliente = contexto.SeleccionarPorFk<UsuarioDeClienteDtm>(nameof(UsuarioDeClienteDtm.IdUsuario), usuario.Id, errorSiNoHay);
                cache[usuario.Id.ToString()] = usuarioDeCliente is null ? null : contexto.SeleccionarPorId<ClienteDtm>(usuarioDeCliente.IdElemento);
            }

            var cliente = (ClienteDtm)cache[usuario.Id.ToString()];
            if (cliente is null && errorSiNoHay)
                GestorDeErrores.Emitir($"El usuario '{usuario.Login}' no es de cliente");

            return cliente;
        }

        public static UsuarioDtm Administrador(this ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Administrador)))
            {
                cache[nameof(Administrador)] = contexto.SeleccionarPorPropiedad<UsuarioDtm>(nameof(UsuarioDtm.Login), ContextoSe.Login_Admin, errorSiNoHay: false, errorSiMasDeuno: false);
            }
            if (((UsuarioDtm)cache[nameof(Administrador)]) is null || !((UsuarioDtm)cache[nameof(Administrador)]).EsAdministrador)
                GestorDeErrores.Emitir($"Debe haber existir el usuario '{ContextoSe.Login_Admin}' en el SE como administrador");
            return ((UsuarioDtm)cache[nameof(Administrador)]);
        }

        private static bool EsDeCliente(this UsuarioDtm usuario, ContextoSe contexto, bool usarLaCache)
        =>
        usuario.Cliente(contexto, errorSiNoHay: false, usarLaCache) is null ? false : true;

        public static bool EsDelCliente(this PuestoDtm puesto, ContextoSe contexto, ClienteDtm cliente)
        =>
        contexto.SeleccionarPorAk<PuestoDeClienteDtm>(new Dictionary<string, object>
        {
            {nameof(PuestoDeClienteDtm.IdPuesto), puesto.Id },
            {nameof(PuestoDeClienteDtm.IdElemento), cliente.Id },
        }, errorSiNoHay: false) != null;

        private static bool EsDeCliente(this PuestoDtm puesto, ContextoSe contexto)
        =>
        puesto.Cliente(contexto, errorSiNoHay: false) is null ? false : true;

        public static bool SePuedeParametrizar(this ContextoSe contexto)
        =>
        contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario).EsParametrizador(contexto);

        public static bool SePuedeArchivarDocumentacionHistorica(this ContextoSe contexto)
        =>
        contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario).PuedenArchivarDocumentacionHistorica(contexto);

        private static bool EsParametrizador(this UsuarioDtm usuario, ContextoSe contexto)
        =>
        contexto.DatosDeConexion.EsAdministrador &&
        ParametroDeNegocioSql.Parametro(enumNegocio.Usuario, enumParametrosDeUsuarios.USU_Parametrizadores, emitirError: false, crearParametro: true, contexto.Administrador().Id)
        .Valor.ToLista<int>().Contains(usuario.Id);

        private static bool PuedenArchivarDocumentacionHistorica(this UsuarioDtm usuario, ContextoSe contexto)
        =>
        contexto.DatosDeConexion.EsAdministrador &&
        ParametroDeNegocioSql.Parametro(enumNegocio.Usuario, enumParametrosDeUsuarios.USU_PuedenArchivarDocumentacionHistorica, emitirError: false, crearParametro: true, contexto.Administrador().Id)
        .Valor.ToLista<int>().Contains(usuario.Id);

        private static ClienteDtm Cliente(this PuestoDtm puesto, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_PuestoDeCliente);
            if (!cache.ContainsKey(puesto.Id.ToString()))
            {
                var puestoDeCliente = contexto.SeleccionarPorFk<PuestoDeClienteDtm>(nameof(PuestoDeClienteDtm.IdPuesto), puesto.Id, errorSiNoHay);
                cache[puesto.Id.ToString()] = puestoDeCliente is null ? null : contexto.SeleccionarPorId<ClienteDtm>(puestoDeCliente.IdElemento);
            }

            var cliente = (ClienteDtm)cache[puesto.Id.ToString()];
            if (cliente is null && errorSiNoHay)
                GestorDeErrores.Emitir($"El puesto '{puesto.Expresion}' no es de cliente");

            return cliente;
        }

        private static bool EsClienteWeb(this UsuarioDtm usuario, ContextoSe contexto, ClienteDtm cliente)
        =>
        contexto.SeleccionarPorAk<UsuarioDeClienteDtm>(new Dictionary<string, object>
        {
            {nameof(UsuarioDeClienteDtm.IdUsuario), usuario.Id },
            {nameof(UsuarioDeClienteDtm.IdElemento), cliente.Id },
        }, errorSiNoHay: false) != null;

        public static bool OtorgarAdministrador(this UsuarioDtm usuario, ContextoSe contexto)
        {
            try
            {
                if (!contexto.AsignarRolDeAdministrador())
                    return false;
                usuario.EsAdministrador = true;
                usuario.Modificar(contexto);
                return true;
            }
            catch
            {
                contexto.QuitarRolDeAdministrador();
                usuario.EsAdministrador = false;
                throw;
            }
        }

        public static void SolicitarNuevaContrasena(this UsuarioDtm usuario, ContextoSe contexto)
        {
            var enlaceFinal = UrlNuevaContrasena(contexto, usuario, motivo: ltrDeUnUsuario.Motivo_OlvidoDeContraseña);

            contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { usuario.eMail }
                , $"Solicitud de nueva contraseña"
                , $"pulse en enlace adjunto para cambiar la contraseña <a href='{enlaceFinal}'>Haga clic aquí</a>", esHtlm: true);
        }

        public static string UrlNuevaContrasena(ContextoSe contexto, UsuarioDtm usuario, string motivo)
        {
            usuario.Guid = Guid.NewGuid();
            usuario.SolicitadaEl = DateTime.Now;

            usuario.Modificar(contexto, accionEjecutada: ltrDeUnUsuario.Accion_SolicitudDeNuevaContrasena);

            var enlace = new UriBuilder(CacheDeVariable.Cfg_UrlBase) { 
                Path = $"/{enumControladoresSeguridad.Acceso}/{enumVistasSeguridad.NuevaContrasena}.html" 
            }.ToString();

            var enlaceFinal = CompletarUrl(contexto, usuario, enlace, motivo).ToString();
            return enlaceFinal;


        }

        private static StringBuilder CompletarUrl(ContextoSe contexto, UsuarioDtm usuario, string baseUrl, string motivo)
        {
            if (!baseUrl.EndsWith("?")) baseUrl = baseUrl + "?";

            // Crear un diccionario para los parámetros
            var parametros = new Dictionary<string, string>
            {
                { "guid", usuario.Guid.ToString()},
                { "motivo", motivo}
            };

            // Construir la URL con URL encoding
            var urlBuilder = new StringBuilder(baseUrl);
            foreach (var param in parametros)
            {
                urlBuilder.Append($"{WebUtility.UrlEncode(param.Key)}={WebUtility.UrlEncode(param.Value)}&");
            }

            // Eliminar el último '&'
            urlBuilder.Length--;

            return urlBuilder;
        }

        public static void AnularAdministrador(this UsuarioDtm usuario, ContextoSe contexto, bool quitar)
        {
            if (!quitar) return;
            try
            {
                usuario.EsAdministrador = false;
                usuario.Modificar(contexto);
            }
            catch
            {
                contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, CacheDeVariable.Cfg_CorreoDeSoporte.Split(';').ToList()
                    , $"Excepción emitida al ejecutar {nameof(AnularAdministrador)}"
                    , $"No se ha podido anular los permisos de administrador al usuario {usuario.Expresion}");
            }
            finally
            {
                contexto.QuitarRolDeAdministrador();
            }
        }

        public static PermisosPorElementoDtm OtorgarPermisoDe(this UsuarioDtm Usuario, ContextoSe contexto, enumNegocio negocio, int idElemento, PermisosDelElementoDtm permiso, enumModoDeAccesoDeDatos modo)
        =>
        OtorgarPermisoDe(contexto, Usuario.Id, negocio, idElemento, permiso, modo);

        public static PermisosPorElementoDtm OtorgarPermisoDe(ContextoSe contexto, int idUsuario, enumNegocio negocio, int idElemento, PermisosDelElementoDtm permiso, enumModoDeAccesoDeDatos modo)
        {
            return new PermisosPorElementoDtm
            {
                IdNegocio = negocio.IdNegocio(),
                IdElemento = idElemento,
                IdUsuario = idUsuario,
                IdPermiso = enumModoDeAccesoDeDatos.Administrador == modo || enumModoDeAccesoDeDatos.Interventor == modo
                ? permiso.IdAdministrador
                : enumModoDeAccesoDeDatos.Gestor == modo
                ? permiso.IdGestor
                : permiso.IdConsultor,
                Calculado = false,
            }.
            InsertarComoAdministradorSiNoExiste(contexto,
               propiedades: new List<string>
               {
                 nameof(PermisosPorElementoDtm.IdElemento),
                 nameof(PermisosPorElementoDtm.IdNegocio),
                 nameof(PermisosPorElementoDtm.IdUsuario),
                 nameof(PermisosPorElementoDtm.IdPermiso),
                 nameof(PermisosPorElementoDtm.Calculado)
               },
               parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
        }

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, enumNegocio negocio, int idElemento, PermisosDelElementoDtm permiso)
        {
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                QuitarElPermiso(contexto, idUsuario, negocio, idElemento, permiso.IdAdministrador);
                QuitarElPermiso(contexto, idUsuario, negocio, idElemento, permiso.IdGestor);
                QuitarElPermiso(contexto, idUsuario, negocio, idElemento, permiso.IdConsultor);
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }

        private static void QuitarElPermiso(ContextoSe contexto, int idUsuario, enumNegocio negocio, int idElemento, int idPermiso)
        {
            var permisoPorElemento = contexto.SeleccionarPorAk<PermisosPorElementoDtm>(new Dictionary<string, object>
            {
                 {nameof(PermisosPorElementoDtm.IdElemento), idElemento},
                 {nameof(PermisosPorElementoDtm.IdNegocio),negocio.IdNegocio()},
                 {nameof(PermisosPorElementoDtm.IdUsuario),idUsuario},
                 {nameof(PermisosPorElementoDtm.IdPermiso),idPermiso},
                 { nameof(PermisosPorElementoDtm.Calculado), false}
            }, errorSiNoHay: false);

            if (permisoPorElemento != null) permisoPorElemento.Eliminar(contexto, esUnaAccion: true);
        }

        //public static bool TienePermisosSobreBuzones(ContextoSe contexto) 
        //=> 
        //contexto.Set<UsuariosDeUnPermisoDtm>().Any(permisos => permisos.IdUsuario == contexto.DatosDeConexion.IdUsuario &&
        //contexto.Set<BuzonDeMiSociedadDtm>().Any(buzon => buzon.IdPermiso == permisos.IdPermiso));

        public static TrabajadorDtm Trabajador(this ContextoSe contexto, bool errorSiNoHay = false)
        =>
        contexto.SeleccionarPorFk<TrabajadorDtm>(nameof(TrabajadorDtm.IdUsuario), contexto.Usuario.Id, errorSiNoHay);

        public static IaDeEntorno IaUsada(ContextoSe contexto)
        {
            var iaDisponibles = VariableDeMenu.Ias(errorSiNoHay: true);
            var ia = (string)enumNegocio.Usuario.LeerParametroDeUsuario<string>(contexto, enumParametrosDeUsuario.USU_Ia_Usada);
            var enumerado = !ia.IsNullOrEmpty() ? ApiDeEnsamblados.ToEnumerado<enumIa>(ia, errorSiNoEsValido: false) : null;

            if (ia.IsNullOrEmpty() || enumerado is null)
            {
                var iaUsada = iaDisponibles.First();
                GuardarIaUsada(contexto, iaUsada.Enumerado);
                return iaUsada;
            }

            var iaDelUsuario = iaDisponibles.FirstOrDefault(i => i.Enumerado == enumerado);
            if (iaDelUsuario is null)
            {
                var iaUsada = iaDisponibles.First();
                GuardarIaUsada(contexto, iaUsada.Enumerado);
                return iaUsada;
            }

            return iaDelUsuario;
        }

        public static void GuardarIaUsada(this ContextoSe contexto, enumIa ia)
        {
            enumNegocio.Usuario.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Ia_Usada, ia.ToString());
        }
    }
}
