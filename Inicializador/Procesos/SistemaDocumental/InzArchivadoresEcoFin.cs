using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;

namespace Inicializador.SistemaDocumental
{
    public static class InzArchivadoresEcoFin
    {
        
        private static readonly string n_soc = "SOC";
        private static readonly string n_cli = "CLI";

        public static void ArchivadoresEconomicoFinanciero(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                TiposArchivadores(contexto);
                RolesSocietarios(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_tipo_EcoFin = $"{n_soc}: Económico Financiero";
        public static readonly string n_tipo_contable = $"{n_soc}: Contable";
        public static readonly string n_tipo_fiscal = $"{n_soc}: Fiscal";
        public static readonly string n_tipo_laboral = $"{n_soc}: Laboral";
        public static readonly string n_tipo_cliente = $"{n_cli}: Documentación";

        private static void TiposArchivadores(ContextoSe contexto)
        {
            try
            {
                contexto.IniciarTraza("Definir tipos de archivadores de ámbito contable");
                var tipoFinancieros = CrearTipo(contexto, n_tipo_EcoFin, "ECO", 0, visible: false, gestionadoPorElSistema: false);
                var tipocontable = CrearTipo(contexto, n_tipo_contable, "CTB", tipoFinancieros.Id, visible: false, gestionadoPorElSistema: false);
                var tipofiscal = CrearTipo(contexto, n_tipo_fiscal, "FSC", tipoFinancieros.Id, visible: false, gestionadoPorElSistema: false);
                var tipolaboral = CrearTipo(contexto, n_tipo_laboral, "LBR", tipoFinancieros.Id, visible: false, gestionadoPorElSistema: false);


                CrearTipo(contexto, n_tipo_cliente, "CLI", 0, visible: false, gestionadoPorElSistema: false);

                contexto.CerrarTraza();

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static void RolesSocietarios(ContextoSe contexto)
        {
            try
            {
                contexto.IniciarTraza("Definir roles de acromur");
                CrearRolesDeGestorAEco(contexto);
                CrearRolesDeGestorAFiscal(contexto);
                CrearRolesDeGestorAContable(contexto);
                CrearRolesDeGestorALaboral(contexto);
                CrearRolesDeConsultorAEco(contexto);
                CrearRolesDeConsultorAFiscal(contexto);
                CrearRolesDeConsultorAContable(contexto);
                CrearRolesDeConsultorALaboral(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }



        public const string n_rol_archivador_gestor_eco = "Archivador: (Gestor) Economico financiero";
        public const string n_rol_archivador_gestor_fiscal = "Archivador: (Gestor) Fiscal";
        public const string n_rol_archivador_gestor_laboral = "Archivador: (Gestor) Laboral";
        public const string n_rol_archivador_gestor_contable = "Archivador: (Gestor) Contable";
        public const string n_rol_archivador_consultor_eco = "Archivador: (Consultor) Economico financiero";
        public const string n_rol_archivador_consultor_fiscal = "Archivador: (Consultor) Fiscal";
        public const string n_rol_archivador_consultor_laboral = "Archivador: (Consultor) Laboral";
        public const string n_rol_archivador_consultor_contable = "Archivador: (Consultor) Contable";
        private static RolDtm CrearRolesDeGestorAEco(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_eco;
            rol.Descripcion = "Define los permisos de gestor al archivador economico financiero";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Gestor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_fiscal, enumModoDeAccesoDeDatos.Gestor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_laboral, enumModoDeAccesoDeDatos.Gestor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_contable, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static RolDtm CrearRolesDeConsultorAEco(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_eco;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador economico financiero";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, n_tipo_contable, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, n_tipo_fiscal, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, n_tipo_laboral, enumModoDeAccesoDeDatos.Consultor);

            return rol;
        }

        private static RolDtm CrearRolesDeConsultorAContable(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_contable;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador contable";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_contable, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }

        private static RolDtm CrearRolesDeConsultorALaboral(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_laboral;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador laboral";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_laboral, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }

        private static RolDtm CrearRolesDeConsultorAFiscal(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_consultor_fiscal;
            rol.Descripcion = "Define los permisos de consultor al tipo de archivador fiscal";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_fiscal, enumModoDeAccesoDeDatos.Consultor);
            return rol;
        }

        private static RolDtm CrearRolesDeGestorAFiscal(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_fiscal;
            rol.Descripcion = "Define los permisos de gestor al archivador fiscal";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_fiscal, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static RolDtm CrearRolesDeGestorAContable(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_contable;
            rol.Descripcion = "Define los permisos de gestor al archivador contable";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_contable, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }

        private static RolDtm CrearRolesDeGestorALaboral(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_archivador_gestor_laboral;
            rol.Descripcion = "Define los permisos de gestor al archivador laboral";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_EcoFin, enumModoDeAccesoDeDatos.Consultor);
            AsignarPermisosAlTipo(contexto, rol, InzArchivadoresEcoFin.n_tipo_laboral, enumModoDeAccesoDeDatos.Gestor);
            return rol;
        }


        private static void AsignarPermisosAlTipo(ContextoSe contexto, RolDtm rol, string nombreTipo, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(INombre.Nombre), nombreTipo, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);
            if (ModoDeAcceso.SoyGestor(modo))
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeConsultor);

        }

        public static TipoDeArchivadorDtm CrearTipo(ContextoSe contexto, string nombre, string sigla, int idPadre, bool visible, bool gestionadoPorElSistema)
        {
            return GestorDeTiposDeArchivadores.PersistirTipo(contexto, nombre, enumClaseDeLibro.POR_CG_TIPO, sigla, visible, gestionadoPorElSistema, nombreModificable: true, idPadre);
        }
    }
}
