using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace SistemaDeElementos.Inicializador
{
    public static class InzSeguridadComun
    {
        public const string n_rol_AFB_Entorno = "AFB: Entorno";
        public const string n_rol_AFB_Callejero = "AFB: Callejero";
        public const string n_rol_AFB_TercerosBasico = "AFB: Terceros básicos";
        public const string n_rol_AFB_Sistema_Documental = "AFB: Sistema documental";
        public const string n_rol_AFB_RegistroEs = "AFB: Registro de E/S";
        public const string n_rol_AFB_Tareas = "AFB: Tareas";
        public const string n_rol_AFB_Expedientes = "AFB: Expedientes";
        public const string n_rol_AFB_Seguridad_Acceso = "AFB: Seguridad de acceso";


        public const string n_rol_callejero_gestor = "Callejero: Gestor";
        public const string n_rol_callejero_consultor = "Callejero: Consultor";
        public const string n_rol_terceros_gestor = "Terceros: Gestor";
        public const string n_rol_terceros_consultor = "Terceros: Consultor";
        public const string n_rol_seguridad_gestor = "Accesos: Gestor";
        public const string n_rol_seguridad_consultor = "Accesos: Consultor";


        public static void SeguridadFuncional(ContextoSe contexto)
        {
            contexto.IniciarTraza("Definir seguridad funcional");
            var t = contexto.IniciarTransaccion();
            try
            {
                CrearRol_AFB_Entorno(contexto);
                CrearRol_AFB_Seguridad_Acceso(contexto);
                CrearRol_AFB_Callejero(contexto);
                CrearRol_AFB_Sistema_Documental(contexto);
                CrearRol_AFB_Terceros_Basicos(contexto);
                CrearRol_AFB_RegistroEs(contexto);
                CrearRol_AFB_Tareas(contexto);
                CrearRol_AFB_Expedientes(contexto);
                contexto.Commit(t);
            }
            catch
            {
                contexto.Rollback(t);
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static void SeguridadDatosComunes(ContextoSe contexto)
        {
            contexto.DatosDeConexion.CreandoModelo = true;
            contexto.IniciarTraza(nameof(SeguridadDatosComunes));
            var tran = contexto.IniciarTransaccion();
            try
            {
                CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Gestor);
                CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Consultor);
                CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Gestor);
                CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Consultor);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
                contexto.DatosDeConexion.CreandoModelo = false;
            }
        }

        private static void CrearRol_AFB_Tareas(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_Tareas;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca al acceso de las tareas";

            if (contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), rol.Nombre, false) == null)
            {
                rol = rol.Insertar(contexto);

                var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
                var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Administracion.Tareas).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

                idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Administracion.TipoDeTareas).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);
            }
        }
        private static void CrearRol_AFB_Expedientes(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_Expedientes;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca al acceso de los expedientes";

            if (contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), rol.Nombre, false) == null)
            {
                rol = rol.Insertar(contexto);

                var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
                var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Administracion.Expedientes).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

                idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Administracion.TipoDeExpedientes).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);
            }
        }

        internal static void CrearRol_AFB_Sistema_Documental(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_Sistema_Documental;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca al acceso del sistema documental";

            rol = rol.PersistirPorNombre(contexto);

            var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
            var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.SistemaDocumental.Archivadores).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.SistemaDocumental.Carpetas).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.SistemaDocumental.TipoDeArchivadores).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            var idTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_De_Exportacion(contexto);
            var tipoExportacion = contexto.SeleccionarPorId<TipoDeArchivadorDtm>(idTipo);
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoExportacion.IdPermisoDeConsultor, false);

            idTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_Basico(contexto);
            var tipoBasicon = contexto.SeleccionarPorId<TipoDeArchivadorDtm>(idTipo);
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoBasicon.IdPermisoDeGestor, false);
        }

        private static void CrearRol_AFB_RegistroEs(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_RegistroEs;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca al acceso del registro de Entrada y Salida";

            if (contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), rol.Nombre, false) == null)
            {
                rol = rol.Insertar(contexto);

                var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
                var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Administracion.RegistrosEs).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

                idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Administracion.TipoDeRegistros).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);
            }
        }

        internal static void CrearRol_AFB_Terceros_Basicos(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_TercerosBasico;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca personas, sociedaes e interlocutores y centrods gestores";
            rol = rol.PersistirPorNombre(contexto);

            var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
            var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Terceros.Personas).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Terceros.Sociedades).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Terceros.Interlocutores).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Terceros.CentrosGestores).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Terceros.Bancos ).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

        }

        internal static void CrearRol_AFB_Callejero(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_Callejero;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad del callejero";
            rol = rol.PersistirPorNombre(contexto);

            var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.TipoDeVia).First().IdPermiso;
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso);

            var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.Paises).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.Provincias).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.Municipios).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.Calles).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.Zonas).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.Barrios).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.CpsDeProv).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.CpsDeMuni).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.CallesDeUnBarrio).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Callejero.CallesDeUnaZona).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);
        }

        internal static void CrearRol_AFB_Entorno(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_Entorno;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca del sistema de elementos";

            rol = rol.PersistirPorNombre(contexto);

            var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
            var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.Usuarios).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.TrabajosDeUsuario).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.Correos).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.VisorDeAgenda).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.Agendas).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.MiCalendario).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

        }

        internal static void CrearRol_AFB_Seguridad_Acceso(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_AFB_Seguridad_Acceso;
            rol.Descripcion = "Define los permisos de acceso a la funcionalidad básisca de la seguridad de acceso al sistema de elementos";

            rol = rol.PersistirPorNombre(contexto);

            var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
            var idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.PuestosDeTrabajo).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.Roles).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);

            idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == enumVistas.Entorno.Permisos).First().IdPermiso;
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);
        }

        internal static RolDtm CrearRolDeCallejeroDe(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            var rol = new RolDtm();
            rol.Nombre = ModoDeAcceso.SoyGestor(modo) ? n_rol_callejero_gestor : n_rol_callejero_consultor;
            rol.Descripcion = $"Define los permisos en modo {modo} a los datos del callejero";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlCallejeroDe(contexto, rol, modo);
            return rol;
        }
        internal static RolDtm CrearRolDeTercerosDe(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            var rol = new RolDtm();
            rol.Nombre = ModoDeAcceso.SoyGestor(modo) ? n_rol_terceros_gestor : n_rol_terceros_consultor;
            rol.Descripcion = $"Define los permisos en modo {modo} a los datos de terceros";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosDeTercerosDe(contexto, rol, modo);
            return rol;
        }

        public static void AsignarPermisosDeTercerosDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Persona.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Sociedad.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Interlocutor.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Banco.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Cliente.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Proveedor.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);
        }

        public static void AsignarPermisosAlCallejeroDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            NegocioDtm negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Pais.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Provincia.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Municipio.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Calle.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Barrio.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.Zona.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);

            negocio = contexto.SeleccionarPorPropiedad<NegocioDtm>(nameof(NegocioDtm.Enumerado), enumNegocio.TipoDeVia.ToString());
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? negocio.IdGestor : negocio.IdConsultor);
        }


        public static void CrearUsuarioEnPuesto(ContextoSe contexto, string login, string nombre, string apellido, string puestoTr)
        {
            var puesto = contexto.SeleccionarPorPropiedad<PuestoDtm>(nameof(PuestoDtm.Nombre), puestoTr);
            puesto.IncluirUsuario(contexto, login, nombre, apellido);
        }

        internal static void CrearUsuarioEnPuestoPorNombreDeCg(ContextoSe contexto, string login, string nombre, string apellido, string puestoTr, string nombreCg)
        {
            var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Nombre), nombreCg);
            var filtroPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), puestoTr } };
            var puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtroPorAk, false);
            puesto.IncluirUsuario(contexto, login, nombre, apellido);
        }

    }
}
