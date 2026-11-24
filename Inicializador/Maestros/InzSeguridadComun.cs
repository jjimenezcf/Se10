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
using System.Collections.Generic;
using Utilidades;

namespace SistemaDeElementos.Inicializador
{
    public static class InzSeguridadComun
    {
        public const string n_rol_AFB_Entorno = "AFB: Entorno";
        public const string n_rol_archivador_basico = "Archivador: Básico";

        // roles AFB por menú (InzRolesFuncionales) que sustituyen a los antiguos AFB de callejero, terceros básicos, sistema documental,
        // registro de E/S, tareas y expedientes; el de sistema documental se completa con el rol de datos de archivadores básico
        public static readonly string[] n_roles_AFB_Callejero = [InzRolesFuncionales.NombreDelRol(InzMenus.Maestros, InzMenus.Callejero)];
        public static readonly string[] n_roles_AFB_TercerosBasico = [InzRolesFuncionales.NombreDelRol(InzMenus.Maestros), InzRolesFuncionales.NombreDelRol(InzMenus.Maestros, InzMenus.Terceros)];
        public static readonly string[] n_roles_AFB_Sistema_Documental = [InzRolesFuncionales.NombreDelRol(InzMenus.SistemaDocumental), InzRolesFuncionales.NombreDelRol(InzMenus.SistemaDocumental, InzMenus.Configuracion), n_rol_archivador_basico];
        public static readonly string[] n_roles_AFB_RegistroEs = [InzRolesFuncionales.NombreDelRol(InzMenus.GestionAdministrativa), InzRolesFuncionales.NombreDelRol(InzMenus.GestionAdministrativa, InzMenus.Configuracion, enumNegocio.Registro.ToString())];
        public static readonly string[] n_roles_AFB_Tareas = [InzRolesFuncionales.NombreDelRol(InzMenus.GestionAdministrativa), InzRolesFuncionales.NombreDelRol(InzMenus.GestionAdministrativa, InzMenus.Configuracion, enumNegocio.Tarea.ToString())];
        public static readonly string[] n_roles_AFB_Expedientes = [InzRolesFuncionales.NombreDelRol(InzMenus.GestionAdministrativa), InzRolesFuncionales.NombreDelRol(InzMenus.GestionAdministrativa, InzMenus.Configuracion, enumNegocio.Expediente.ToString())];


        public const string n_rol_callejero_gestor = "Callejero: Gestor";
        public const string n_rol_callejero_consultor = "Callejero: Consultor";
        public const string n_rol_terceros_gestor = "Terceros: Gestor";
        public const string n_rol_terceros_consultor = "Terceros: Consultor";
        public const string n_rol_seguridad_gestor = "Accesos: Gestor";
        public const string n_rol_seguridad_consultor = "Accesos: Consultor";


        public static void SeguridadFuncional(ContextoSe contexto)
        {
            InzRolesFuncionales.DefinirRoles(contexto);
            contexto.IniciarTraza("Definir seguridad funcional");
            var t = contexto.IniciarTransaccion();
            try
            {
                CrearRol_AFB_Entorno(contexto);
                CrearRol_Archivador_Basico(contexto);
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

            // vistas secundarias que no son de ningún módulo: auditoría (desde cualquier crud), errores y trazas (desde trabajos de usuario) y el correo del panel personal
            foreach (var vista in new[] { enumVistas.Configuracion.AuditoríaDeElementos, enumVistas.Entorno.ErroresDeUnTrabajo, enumVistas.Entorno.TrazasDeUnTrabajo,
                                          enumVistas.Entorno.MiCorreo, enumVistas.Entorno.MiCorreoImap, enumVistas.Entorno.MiCorreoApiKey })
            {
                idPermiso = contexto.Set<VistaMvcDtm>().Where(x => x.Nombre == vista).First().IdPermiso;
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, idPermiso, false);
            }
        }

        // rol de datos que conserva los permisos de tipo de archivador que daba el antiguo "AFB: Sistema documental"
        internal static void CrearRol_Archivador_Basico(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_basico;
            rol.Descripcion = "Permisos de datos básicos sobre archivadores: gestor del tipo básico y consultor del de exportación";
            rol = rol.PersistirPorNombre(contexto);

            var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
            var idTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_De_Exportacion(contexto);
            var tipoExportacion = contexto.SeleccionarPorId<TipoDeArchivadorDtm>(idTipo);
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoExportacion.IdPermisoDeConsultor, false);

            idTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_Basico(contexto);
            var tipoBasico = contexto.SeleccionarPorId<TipoDeArchivadorDtm>(idTipo);
            gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoBasico.IdPermisoDeGestor, false);
        }

        public static void AsignarRolesAlPuesto(ContextoSe contexto, int idPuesto, IEnumerable<string> roles)
        {
            var gestor = GestorDeRolesDeUnPuesto.Gestor(contexto, contexto.Mapeador);
            foreach (var nombre in roles.Distinct())
            {
                var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), nombre);
                gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), idPuesto, rol.Id, errorSiYaExiste: false);
            }
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
