using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Utilidades;

namespace GestoresDeNegocio.Entorno
{

    public class GestorDeUsuarios : GestorDeElementos<ContextoSe, UsuarioDtm, UsuarioDto>
    {

        public class MapearUsuario : Profile
        {
            public MapearUsuario()
            {
                CreateMap<UsuarioDtm, UsuarioDto>()
                .ForMember(dto => dto.NombreCompleto, dtm => dtm.MapFrom(x => UsuarioDtm.NombreCompleto(x)));
                CreateMap<UsuarioDto, UsuarioDtm>()
                .ForMember(dtm => dtm.Archivo, dto => dto.Ignore());
            }

        }

        public override enumNegocio Negocio => enumNegocio.Usuario;

        public GestorDeUsuarios(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static UsuarioDtm Crear(ContextoSe contexto, string login, string nombre, string apellido)
        {
            var usuario = contexto.SeleccionarPorPropiedad<UsuarioDtm>(nameof(UsuarioDtm.Login), login, false);

            if (usuario == null)
            {
                usuario = new UsuarioDtm();
                usuario.Login = login;
                usuario.Nombre = nombre;
                usuario.Apellido = apellido;
                usuario.eMail = "jjimenecf@gmail.com";
                usuario = usuario.Insertar(contexto);
            }
            return usuario;
        }

        internal static UsuarioDtm LeerUsuario(ContextoSe contexto, int idUsuario)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.LeerRegistroPorId(idUsuario, true, false, false, aplicarJoin: false);
        }

        internal static UsuarioDtm LeerUsuario(ContextoSe contexto, string login)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.LeerRegistro(nameof(UsuarioDtm.Login), login, true, true, false, aplicarJoin: false);
        }

        public static CertificadoDtm SubirMiCertificado(ContextoSe contexto, int idUsuario, int idArchivo, string password)
        {
            if (contexto.DatosDeConexion.IdUsuario != idUsuario)
                GestorDeErrores.Emitir("No puede subir un certificado que no sea el suyo");

            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                var usuario = contexto.SeleccionarPorId<UsuarioDtm>(idUsuario);
                var certificado = GestorDeCertificados.PersisitirMiCertificado(contexto, usuario, idArchivo, password);
                if (certificado.creado)
                {
                    usuario.IdCertificado = certificado.certificado.Id;
                    usuario.Modificar(contexto);
                }
                return certificado.certificado;
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }

        public static void CambiarPassword(ContextoSe contexto, int idUsuario, string actual, string nueva, string repetida)
        {
            if (contexto.DatosDeConexion.IdUsuario != idUsuario)
                GestorDeErrores.Emitir("No puede cambiar una password que no es suya");

            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                var usuario = contexto.SeleccionarPorId<UsuarioDtm>(idUsuario);
                GestorDePassword.CambiarPassword(contexto, usuario, actual, nueva, repetida);
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }


        public static void NuevaContrasena(ContextoSe contexto, UsuarioDtm usuario, string nueva)
        {
            usuario.Guid = null;
            usuario.SolicitadaEl =null;

            usuario.Modificar(contexto, accionEjecutada: ltrDeUnUsuario.Accion_SolicitudDeNuevaContrasena);

            GestorDePassword.RegistraPassworDeConexion(contexto, usuario.Id, nueva);
        }

        public static GestorDeUsuarios Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUsuarios(contexto, mapeador);
        }

        protected override IQueryable<UsuarioDtm> AplicarFiltros(IQueryable<UsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            if (!HayFiltroPorId(filtros))
            {
                if (!parametros.Parametros.LeerValor(ltrParametrosNeg.EstoyLeyendoParaAnalizarElModoDeAcceso, false))
                {
                    if (parametros.Peticion == enumPeticion.epLeerElementos || parametros.Peticion == enumPeticion.epLeerDatosParaElGrid)
                        consulta = consulta.ExcluirUsuarioDeCliente(Contexto, filtros);
                }

                var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrParametrosNeg.SoloEnAlta.ToLower() && f.Valor.EsTrue());
                if (filtro != null)
                {
                    filtro.Aplicado = true;
                    consulta = parametros.CargarListaDinamica 
                    ? consulta.Where(x => x.Activo == true || (x.Activo == false && x.BloqueadoEl != null && x.IntentosFallidos > 0 )) 
                    : consulta.Where(x => x.Activo == true);
                }
            }

            consulta = consulta.FiltrarPorUsuarioDeCliente(Contexto, filtros);
            consulta = consulta.FiltrarPorNombreCompleto(filtros);
            consulta = consulta.FiltrarPorPermisos(filtros);
            consulta = consulta.FiltrarPorRol(filtros);
            consulta = consulta.FiltrarPorPuesto(filtros);
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(UsuarioDto usuarioDto, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(usuarioDto, opciones);
            usuarioDto.Alta = DateTime.Now;
            ValidarDatos(usuarioDto);
        }

        protected override void AntesDeMapearElRegistroParaModificar(UsuarioDto usuarioDto, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(usuarioDto, opciones);
            ValidarDatos(usuarioDto);
        }

        protected override void AntesDePersistir(UsuarioDtm usuario, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(usuario, parametros);

            if (usuario.IdArchivo == 0 || usuario.IdArchivo == null)
            {
                usuario.IdArchivo = null;
                usuario.Archivo = null;
            }

            if (parametros.Insertando)
            {
                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Solo el Administrador puede dar de alta usuarios");

                usuario.Alta = DateTime.Now;
                usuario.password = "";
                usuario.Activo = true;
                var agenda = new AgendaDtm() { Nombre = usuario.MiAgenda }.Insertar(Contexto);
                usuario.IdAgenda = agenda.Id;
            }
            else
            if (parametros.Modificando)
            {
                usuario.IdAgenda = ((UsuarioDtm)parametros.registroEnBd).IdAgenda;
                usuario.password = ((UsuarioDtm)parametros.registroEnBd).password;
                if (usuario.IdCertificado == null) usuario.IdCertificado = ((UsuarioDtm)parametros.registroEnBd).IdCertificado;

                if (parametros.AccionQueSeEjecuta != ltrDeUnUsuario.Accion_SolicitudDeNuevaContrasena)
                {
                    usuario.Guid = ((UsuarioDtm)parametros.registroEnBd).Guid;
                    usuario.SolicitadaEl = ((UsuarioDtm)parametros.registroEnBd).SolicitadaEl;
                }

                if (parametros.AccionQueSeEjecuta != ltrDeUnUsuario.Accion_ContraseñaErronea)
                {
                    usuario.IntentosFallidos = parametros.AccionQueSeEjecuta != ltrDeUnUsuario.Accion_ResetearIntentosFallidos ? ((UsuarioDtm)parametros.registroEnBd).IntentosFallidos: 0;
                    usuario.BloqueadoEl = ((UsuarioDtm)parametros.registroEnBd).BloqueadoEl;
                }

                if (usuario.SeHaModificadoElCampo<bool>(x => x.Name == nameof(UsuarioDtm.Activo), parametros))
                {
                    if (!Contexto.SePuedeParametrizar()) GestorDeErrores.Emitir("Sólo un parametrizador, puede activar o desactivar usuarios");

                    if (usuario.Activo)
                    {
                        usuario.Guid = null;
                        usuario.SolicitadaEl = null;
                        usuario.IntentosFallidos = 0;
                        usuario.BloqueadoEl = null;
                    }
                }
            }
        }

        protected override void DespuesDePersistir(UsuarioDtm usuario, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(usuario, parametros);

            if (parametros.Insertando)
            {
                GestorDePassword.RegistraPassworDeConexion(Contexto, usuario.Id, VariableDeUsuario.PasswordPorDefecto());

                string urlBase = CacheDeVariable.Cfg_UrlBase;

                string cuerpoHtml = $@"Estimado {usuario.Nombre},<br><br>
                        Se le ha dado de alta en el 
                        <a href='{urlBase}' style='text-decoration: underline; color: blue;'>
                        sistema de elementos
                        </a> 
                        <br><br>Conétese con el usuario '{usuario.Login}' y la password que le indique su administrador";

                Contexto.CrearCorreo(new List<string> {usuario.eMail}, "Alta de usuario en el sistema de elementos",
                    cuerpo: cuerpoHtml,
                    elementos: null,
                    archivos: null);
            }

            if (parametros.Modificando)
            {
                if (usuario.Login != ((UsuarioDtm)parametros.registroEnBd).Login)
                {
                    CambiarNombreDeMiAgenda(usuario);
                    CambiarNombreDeMiCertificado(usuario);

                    if (((UsuarioDtm)parametros.registroEnBd).Login == Contexto.DatosDeConexion.Login)
                        Contexto.AsignarUsuario(usuario);
                }
            }
        }

        protected override void EliminarCaches(UsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            if (!parametros.Insertando)
                ServicioDeCaches.EliminarCaches(typeof(UsuarioDtm).FullName);
            ServicioDeCaches.EliminarCaches(CacheDe.ArbolDeMenu);
            ServicioDeCaches.EliminarCaches(CacheDe.Ent_UsuariosPorLogin);
        }

        protected override void DespuesDeMapearElElemento(UsuarioDtm usuarioDtm, UsuarioDto usuarioDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(usuarioDtm, usuarioDto, parametros);

            if (usuarioDtm.Id == Contexto.DatosDeConexion.IdUsuario && (bool)parametros.Parametros.LeerValor(ltrParametrosNeg.ObtenerCertificado, false) && usuarioDtm.IdCertificado.Entero() > 0)
            {
                var certificado = Contexto.SeleccionarPorId<CertificadoDtm>((int)usuarioDtm.IdCertificado);
                usuarioDto.DatosCertificado = certificado.Datos;
            }

            var cliente = usuarioDtm.Cliente(Contexto, false);
            if (cliente != null)
            {
                usuarioDto.Cliente = cliente.Expresion;
                usuarioDto.EsClienteWeb = true;
            }

        }

        private void ValidarDatos(UsuarioDto usuario)
        {
            if (usuario.Login.IsNullOrEmpty())
                GestorDeErrores.Emitir("Es necesario indicar el login del usuario");
            if (usuario.Apellido.IsNullOrEmpty())
                GestorDeErrores.Emitir("Es necesario indicar el apellido del usuario");
            if (usuario.Nombre.IsNullOrEmpty())
                GestorDeErrores.Emitir("Es necesario indicar el nombre del usuario");
            if (usuario.eMail.IsNullOrEmpty())
                GestorDeErrores.Emitir("Es necesario indicar la dirección de correo del usuario");
            else
            {
                try
                {
                    var addr = new MailAddress(usuario.eMail);
                }
                catch (FormatException)
                {
                    GestorDeErrores.Emitir($"El eMail '{usuario.eMail}' no tiene formato correcto");
                }
            }
        }

        public UsuarioDto ValidarUsuario(string login, string password)
        {
            UsuarioDtm usuariodtm = null;
            var p = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);
            p.Parametros = new Dictionary<string, object> { { ltrParametrosNeg.ObtenerCertificado, false } };

            usuariodtm = Contexto.SeleccionarPorPropiedad<UsuarioDtm>(nameof(UsuarioDtm.Login), login, usarLaCache: false, parametros: p.Parametros, errorSiNoHay: false);
            if (usuariodtm == null)
                GestorDeErrores.Emitir($"Usuario '{login}' no válido");

            var puedeDesbloquear = false;
            if (usuariodtm.Activo == false && usuariodtm.BloqueadoEl != null)
            {
                var tiempoDeBloqueo = ParametroDeNegocioSql.Parametro(enumNegocio.Usuario, enumParametrosDeUsuarios.USU_TiempoDeBloqeo, emitirError: false, crearParametro: true, 5).Valor.Entero();

                var fechaDesbloqueo = usuariodtm.BloqueadoEl.Value.AddMinutes(tiempoDeBloqueo);
                var tiempoRestante = fechaDesbloqueo - DateTime.Now;

                if (tiempoRestante.TotalSeconds > 0)
                {
                    // Usamos Ceiling para que si faltan 8:20 aparezca "9 minutos" 
                    // o simplemente los minutos enteros con TotalMinutes
                    int minutosFaltantes = (int)Math.Ceiling(tiempoRestante.TotalMinutes);

                    string mensaje = $"Superado 3 intentos de conexión. " +
                                     $"En '{minutosFaltantes} {(minutosFaltantes == 1 ? "minuto" : "minutos")}' " +
                                     $"podrá intentarlo, espere o contacte con el administrador.";

                    GestorDeErrores.Emitir(mensaje);
                }
                puedeDesbloquear = true;
            }
            if (GestorDePassword.LeerPasswordDeConexion(login) != password)
            {
                usuariodtm.IntentosFallidos = puedeDesbloquear && usuariodtm.IntentosFallidos == 3 ? 1: usuariodtm.IntentosFallidos + 1;
                if (usuariodtm.IntentosFallidos > 3)
                {
                    usuariodtm.BloqueadoEl = DateTime.Now;
                    usuariodtm.Activo = false;
                    usuariodtm.IntentosFallidos = 3;
                }
                else
                {
                    usuariodtm.Activo = true;
                }
                usuariodtm.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnUsuario.Accion_ContraseñaErronea);
                GestorDeErrores.Emitir("Login/password incorrecto");
            }

            if (puedeDesbloquear)
            {
                usuariodtm.Activo = true;
                usuariodtm.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnUsuario.Accion_ContraseñaErronea);
            }

            if (usuariodtm.IntentosFallidos > 0 && usuariodtm.Activo)
            {
                usuariodtm.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnUsuario.Accion_ResetearIntentosFallidos);
            }

            return MapearElemento(usuariodtm, p);
        }

        public bool TienePermisoDeDatos(enumModoDeAccesoDeDatos permisosNecesarios, object elemento)
        {
            var gestorDeNegocio = GestorDeNegocios.Gestor(Contexto, Mapeador);
            return gestorDeNegocio.TienePermisos(permisosNecesarios, (enumNegocio)elemento);
        }

        public bool TienePermisoFuncional(UsuarioDtm usuarioConectado, object elemento)
        {
            var gestorDeVista = GestorDeVistaMvc.Gestor(Contexto, Mapeador);
            return gestorDeVista.TienePermisos(usuarioConectado, (string)elemento);
        }

        private void CambiarNombreDeMiCertificado(UsuarioDtm usuario)
        {
            if (usuario.IdCertificado.Entero() > 0)
            {
                var certificado = Contexto.SeleccionarPorId<CertificadoDtm>(usuario.IdCertificado.Entero());
                certificado.Nombre = usuario.MiCertificado;
                certificado.ModificarComoAdministrador(Contexto);
            }
        }

        private void CambiarNombreDeMiAgenda(UsuarioDtm usuario)
        {
            var agenda = Contexto.SeleccionarPorId<AgendaDtm>(usuario.IdAgenda);
            agenda.Nombre = usuario.MiAgenda;
            agenda.ModificarComoAdministrador(Contexto);
        }
    }

    public class GestorDePassword
    {

        internal static void CambiarPassword(ContextoSe contexto, UsuarioDtm usuario, string actual, string nueva, string repetida)
        {
            var passwordDeBd = LeerPasswordDeConexion(usuario.Login);

            if (actual.IsNullOrEmpty() || nueva.IsNullOrEmpty() || repetida.IsNullOrEmpty())
                GestorDeErrores.Emitir("Debe introducir los tres valores");

            if (!passwordDeBd.Equals(actual))
                GestorDeErrores.Emitir("La password indicada no es correcta");

            if (passwordDeBd.Equals(nueva))
                GestorDeErrores.Emitir("La nueva password debe diferir de la almacenada");

            if (!nueva.Equals(repetida))
                GestorDeErrores.Emitir("La nueva password no coincide con la repetida");

            RegistraPassworDeConexion(contexto, usuario.Id, nueva);
        }

        internal static string LeerPasswordDeConexion(string login)
        {
            var consulta = new ConsultaSql<CredencialesDeConexion>(
                $@"SELECT 
                    {ICampos.ID} as {nameof(CredencialesDeConexion.Id)}
                  , {ICampos.LOGIN} as {nameof(CredencialesDeConexion.Login)}
                  , CONVERT(VARCHAR , DECRYPTBYPASSPHRASE('sistemaSe',  {ICampos.PASSWORD})) as {nameof(CredencialesDeConexion.Password)}
                  FROM {Esquemas.ENTORNO}.{Tablas.USUARIO}
                  where {ICampos.LOGIN} like '{login}'");

            var credenciales = consulta.LanzarConsulta();
            if (credenciales.Count == 0)
                throw new Exception($"Credenciales del usuario {login} no localizadas");

            return credenciales[0].Password;
        }

        public static string Generar(string login)
        {
            var consulta = new ConsultaSql<CredencialesDeConexion>($@"SELECT '{login}' as Login,  CONVERT(VARCHAR , ENCRYPTBYPASSPHRASE('sistemaSe', '{VariableDeUsuario.PasswordPorDefecto()}')) as Password");
            var credenciales = consulta.LanzarConsulta();
            return credenciales[0].Password;
        }

        internal static void RegistraPassworDeConexion(ContextoSe contexto, int id, string password)
        {
            ValidarCriteriosDePassword(password);
            var consulta = new ConsultaSql<CredencialesDeConexion>(contexto, $"update {Esquemas.ENTORNO}.{Tablas.USUARIO} set {ICampos.PASSWORD} = ENCRYPTBYPASSPHRASE('sistemaSe','{password}') where {ICampos.ID} = {id}");
            consulta.EjecutarSentencia();
        }

        internal static void RegistrarPasswordDeCertificado(ContextoSe contexto, int id, string password)
        {
            var consulta = new ConsultaSql<CredencialesDeCertificado>(contexto, $"update {Esquemas.ENTORNO}.{Tablas.CERTIFICADO} set {ICampos.PASSWORD} = ENCRYPTBYPASSPHRASE('sistemaSe','{password}') where {ICampos.ID} = {id}");
            consulta.EjecutarSentencia();
        }

        internal static void ValidarCriteriosDePassword(string password)
        {
            // 2. Validar
            if (!CumpleCriteriosLaPassword(password))
            {
                GestorDeErrores.Emitir("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.");
            }
        }

        public static bool CumpleCriteriosLaPassword(string password)
        {
            // 1. Definir el patrón de validación:
            // ^                : Inicio de la cadena
            // (?=.*[a-z])      : Al menos una letra minúscula
            // (?=.*[A-Z])      : Al menos una letra mayúscula
            // (?=.*\d)         : Al menos un número
            // .{8,}            : Al menos 8 caracteres de longitud
            // $                : Fin de la cadena
            string patron = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

            // 2. Validar
            if (string.IsNullOrEmpty(password) || !Regex.IsMatch(password, patron))
            {
                return false;
            }
            return true;
        }

    }

}
