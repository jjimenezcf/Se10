using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;

namespace Inicializador.SistemaDocumental
{
    public static class InzArchivosJuridicos
    {

        public static readonly string n_jrd = "JUR";
        public static readonly string n_tipo_Juridico = $"{n_jrd}: Jurídico";
        public static readonly string n_tipo_Demanda = $"{n_jrd}: Demanda";
        public static readonly string n_tipo_Contrato = $"{n_jrd}: Contratos";
        public static readonly string n_tipo_Convenios = $"{n_jrd}: Convenios";


        public const string n_rol_archivador_gestor_juridico = "Archivador: (Gestor) Jurídico";
        public const string n_rol_archivador_gestor_Contratos = "Archivador: (Gestor) Contratos";
        public const string n_rol_archivador_gestor_Demanda = "Archivador: (Gestor) Demanda";
        public const string n_rol_archivador_gestor_Convenios = "Archivador: (Gestor) Convenios";
        public const string n_rol_archivador_consultor_jurídico = "Archivador: (Consultor) Jurídico";
        public const string n_rol_archivador_consultor_Contratos = "Archivador: (Consultor) Contratos";
        public const string n_rol_archivador_consultor_Demanda = "Archivador: (Consultor) Demanda";
        public const string n_rol_archivador_consultor_Convenios = "Archivador: (Consultor) Convenios";


        public static void ArchivadoresJuridicos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Definir tipos de archivadores de ámbito jurídico");
            var tran = contexto.IniciarTransaccion();
            try
            {
                TiposDeArchivadores(contexto);
                RolesDeArchivadoresJuridicos(contexto);
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
            }

        }

        private static void TiposDeArchivadores(ContextoSe contexto)
        {
            var tipoJuridico = GestorDeTiposDeArchivadores.PersistirTipo(contexto, n_tipo_Juridico, enumClaseDeLibro.POR_CG_TIPO, "JRD", visible: false, delSistema: false, nombreModificable: true, 0);
            GestorDeTiposDeArchivadores.PersistirTipo(contexto, n_tipo_Contrato, enumClaseDeLibro.POR_CG_TIPO, "CTR", visible: false, delSistema: false, nombreModificable: true, tipoJuridico.Id);
            GestorDeTiposDeArchivadores.PersistirTipo(contexto, n_tipo_Demanda, enumClaseDeLibro.POR_CG_TIPO, "DMD", visible: false, delSistema: false, nombreModificable: true, tipoJuridico.Id);
            GestorDeTiposDeArchivadores.PersistirTipo(contexto, n_tipo_Convenios, enumClaseDeLibro.POR_CG_TIPO, "CNV", visible: false, delSistema: false, nombreModificable: true, tipoJuridico.Id);
        }

        private static void RolesDeArchivadoresJuridicos(ContextoSe contexto)
        {
            try
            {
                contexto.IniciarTraza("Definir roles de archivadores jurídicos");
                CrearRolesDeGestorJuridico(contexto);
                CrearRolesDeGestorDeContratos(contexto);
                CrearRolesDeGestorConvenios(contexto);
                CrearRolesDeGestorDeDemandas(contexto);
                CrearRolesDeConsultorJuridico(contexto);
                CrearRolesDeConsultorContratos(contexto);
                CrearRolesDeConsultorDeConvenios(contexto);
                CrearRolesDeConsultorDeDemandas(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static RolDtm CrearRolesDeConsultorDeDemandas(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_Contratos;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador de demandas";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Demanda, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }

        private static RolDtm CrearRolesDeConsultorContratos(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_Convenios;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador de contratos";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Contrato, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }

        private static RolDtm CrearRolesDeConsultorDeConvenios(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_Demanda;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador de convenios";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Convenios, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }
        private static RolDtm CrearRolesDeGestorDeDemandas(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_Contratos;
            rol.Descripcion = "Define los permisos de gestor al archivador de demandas";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Demanda, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static RolDtm CrearRolesDeGestorJuridico(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_juridico;
            rol.Descripcion = "Define los permisos de gestor al archivador jurídico";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Gestor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Demanda, enumModoDeAccesoDeDatos.Gestor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Convenios, enumModoDeAccesoDeDatos.Gestor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Contrato, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static RolDtm CrearRolesDeConsultorJuridico(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_jurídico;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador jurídico";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Contrato, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Demanda, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Convenios, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }

        private static RolDtm CrearRolesDeGestorDeContratos(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_Convenios;
            rol.Descripcion = "Define los permisos de gestor al archivador de contratos";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Contrato, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static RolDtm CrearRolesDeGestorConvenios(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_Demanda;
            rol.Descripcion = "Define los permisos de gestor al archivador de convenios";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Juridico, enumModoDeAccesoDeDatos.Consultor);
            rol.AsignarPermisosDeUnTipo<TipoDeArchivadorDtm>(contexto, n_tipo_Convenios, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static void AsignarPermisosAlTipo(ContextoSe contexto, RolDtm rol, string nombreTipo, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(INombre.Nombre), nombreTipo, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);
            if (ModoDeAcceso.SoyGestor(modo))
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeConsultor);

        }


    }
}
