
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Terceros;
using Inicializador.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace SistemaDeElementos.Inicializador.Acromur
{
    public static class InzAcromur
    {
        public const string n_nif_acromur = "B73037863";
        public const string n_sociedad_acromur_nombre = "Acromur";
        public const string n_sociedad_acromur_cf = "ACR";
        public const int n_sociedad_acromur_cc = 4;

        public const string n_cg_acm_codigo = n_sociedad_acromur_cf;
        public static string n_cg_acm_codigo_fiscal = $"{n_cg_acm_codigo}.10";
        public static string n_cg_acm_codigo_laboral = $"{n_cg_acm_codigo}.20";
        public static string n_cg_acm_codigo_contable = $"{n_cg_acm_codigo}.30";
        public static string n_cg_acm_nombre = "Acromur";
        public const string n_cg_acm_nombre_fiscal = "Departamento fiscal";
        public const string n_cg_acm_nombre_laboral = "Departamento laboral";
        public const string n_cg_acm_nombre_contable = "Departamento contable";

        public const string n_pt_secretaria = "Secretaria";
        public const string n_pt_gerente = "Gerente";
        public const string n_pt_responsable = "Responsable";

        public const string n_usuario_contable = "contable.acromur";
        public const string n_usuario_fiscal = "fiscal.acromur";
        public const string n_usuario_gerente = "gerente.acromur";
        public const string n_usuario_laboral = "laboral.acromur";
        public const string n_usuario_secretaria = "secretaria.acromur";

        public static SociedadDtm Sociedad(ContextoSe contexto)
        {
            var t = contexto.IniciarTransaccion();
            try
            {
                contexto.IniciarTraza("Definir sociedad Acromur");
                var sociedad = ExtensorDeSociedades.CrearSiNoExiste(contexto, n_nif_acromur, n_sociedad_acromur_nombre, n_sociedad_acromur_nombre, n_sociedad_acromur_cf, "info@ingesmo.com", "968.20.44.83");
                DefinirDepartamentosDeAcromur(contexto, sociedad, n_cg_acm_nombre);
                return sociedad;
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

        public static void Usuarios(ContextoSe contexto)
        {
            var t = contexto.IniciarTransaccion();
            try
            {
                contexto.IniciarTraza("Definir usuarios de Acromur"); 
                InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_gerente, "Vicente", "Fernández", n_pt_gerente, n_cg_acm_nombre);
                InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_fiscal, "Manolo", "López", n_pt_responsable, n_cg_acm_nombre_fiscal);
                InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_laboral, "Luis", "Cano", n_pt_responsable, n_cg_acm_nombre_laboral);
                InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_contable, "Pepe", "Porriño", n_pt_responsable, n_cg_acm_nombre_contable);
                InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_secretaria, "Laura", "Jamón", n_pt_secretaria, n_cg_acm_nombre);
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

        public static void PuestosDeTrabajo(ContextoSe contexto)
        {
            var t = contexto.IniciarTransaccion();
            try
            {
                contexto.IniciarTraza("Definir puestos de trabajo de Acromur");
                CrearPT_Gerencia(contexto, contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzArchivadoresEcoFin.n_rol_archivador_consultor_eco));
                CrearPT_ResponsableFiscal(contexto, contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzArchivadoresEcoFin.n_rol_archivador_gestor_fiscal));
                CrearPT_ResponsableLaboral(contexto, contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzArchivadoresEcoFin.n_rol_archivador_gestor_laboral));
                CrearPT_ResponsableContable(contexto, contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzArchivadoresEcoFin.n_rol_archivador_gestor_contable));
                CrearPT_Secretaria(contexto, contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzArchivadoresEcoFin.n_rol_archivador_gestor_eco));
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

        private static void DefinirDepartamentosDeAcromur(ContextoSe contexto, SociedadDtm acromur, string nombreCgPadre)
        {
            var cgPadre = acromur.CrearCg(contexto, nombreCgPadre);
            cgPadre.CrearHijo(contexto, n_cg_acm_nombre_contable, n_cg_acm_codigo_contable);
            cgPadre.CrearHijo(contexto, n_cg_acm_nombre_laboral, n_cg_acm_codigo_laboral);
            cgPadre.CrearHijo(contexto, n_cg_acm_nombre_fiscal, n_cg_acm_codigo_fiscal);
        }


        private static PuestoDtm CrearPT_Gerencia(ContextoSe contexto, RolDtm rolConsultorEco)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_acromur, n_pt_gerente, "Define los roles y pemisos para el puesto de Gerencia");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);


            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rolConsultorEco.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_acromur_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_contable, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_fiscal, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_laboral, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);

            return pt;
        }

        private static PuestoDtm CrearPT_Secretaria(ContextoSe contexto, RolDtm rolGestorEco)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_acromur, n_pt_secretaria, "Define los roles y pemisos para secretaría");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);


            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rolGestorEco.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_acromur_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_contable, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_fiscal, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_laboral, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);

            return pt;
        }


        private static PuestoDtm CrearPT_ResponsableFiscal(ContextoSe contexto, RolDtm rolGestorFiscal)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_acromur, n_cg_acm_nombre_fiscal, n_pt_responsable, "Define los roles y pemisos para el puesto de responsable fiscal");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);


            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rolGestorFiscal.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_acromur_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_fiscal, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            return pt;
        }


        private static PuestoDtm CrearPT_ResponsableLaboral(ContextoSe contexto, RolDtm rolGestorLaboral)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_acromur, n_cg_acm_nombre_laboral, n_pt_responsable, "Define los roles y pemisos para el puesto de responsable laboral");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);


            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rolGestorLaboral.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_acromur_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_laboral, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            return pt;
        }
        private static PuestoDtm CrearPT_ResponsableContable(ContextoSe contexto, RolDtm rolGestorContable)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_acromur, n_cg_acm_nombre_contable, n_pt_responsable, "Define los roles y pemisos para el puesto de responsable contable");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Consultor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);


            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rolGestorContable.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_acromur_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_cg_acm_codigo_contable, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            return pt;
        }

        private static void AsignarAccesoFuncional(ContextoSe contexto, PuestoDtm pt)
        {
            var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Entorno);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Callejero);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_TercerosBasico);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Sistema_Documental);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_RegistroEs);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Tareas);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Expedientes);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);
        }


    }
}
