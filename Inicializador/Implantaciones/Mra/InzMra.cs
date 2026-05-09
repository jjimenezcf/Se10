using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Terceros;
using Inicializador.Expedientes;
using Inicializador.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace SistemaDeElementos.Inicializador.Mra
{
    public static class InzMra
    {

        public static string n_cg_mra_nombre = "Despacho";

        public const string n_nif_mra = "B73061152"; public const string n_sociedad_mra_nombre = "MANUEL RAMOS ABOGADOS SLP";
        public const string n_sociedad_mra_cf = "MRA";
        public const int n_sociedad_mra_cc = 2;
        public const string n_pt_mra_secretaria = "Secretaria de MR";
        public const string n_pt_mra_gerente = "Gerente";

        public const string n_usuario_gerente = "gerente.mra";
        public const string n_usuario_secretaria = "secretaria.mra";

        public static void Sociedad(ContextoSe contexto)
        {
            contexto.IniciarTraza("Definir sociedad mra");
            var t = contexto.IniciarTransaccion();
            try
            {
                var sociedadMra = ExtensorDeSociedades.CrearSiNoExiste(contexto, n_nif_mra, n_sociedad_mra_nombre, n_sociedad_mra_nombre, n_sociedad_mra_cf, "manuelramosabogados@gmail.com", "968215449");
                var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), n_nif_mra, false);
                DefinirDepartamentosDeMra(contexto, sociedad, n_cg_mra_nombre);
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

        public static void Usuarios(ContextoSe contexto)
        {
            var t = contexto.IniciarTransaccion();
            try
            {
                contexto.IniciarTraza("Definir usuarios de mra");

                var gestor = GestorDePuestosDeUnUsuario.Gestor(contexto, contexto.Mapeador);

                var cg = GestorDeCentrosGestores.LeerCgPorNombre(contexto, n_nif_mra, n_cg_mra_nombre);
                var filtrosPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), n_pt_mra_gerente } };
                var puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk, aplicarJoin: false);
                puesto.IncluirUsuario(contexto, n_usuario_gerente, "Manolo", "Ramos");

                filtrosPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), n_pt_mra_secretaria } };
                puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk, aplicarJoin: false);
                puesto.IncluirUsuario(contexto, n_usuario_secretaria, "MªÁngeles", "Simón");
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
                contexto.IniciarTraza("Definir puestos de trabajo de mra");

                var roles = new List<RolDtm>();
                roles.Add(contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzArchivosJuridicos.n_rol_archivador_gestor_juridico));
                roles.Add(contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzProcesosJuridicos.n_rol_expediente_apertura_juridico));
                CrearPT_Secretaria(contexto, roles);

                roles.Add(contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzProcesosJuridicos.n_rol_expediente_gestor_juridico));
                CrearPT_Gerencia(contexto, roles);
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



        private static void DefinirDepartamentosDeMra(ContextoSe contexto, SociedadDtm mra, string nombreCgPadre)
        {
            var cgPadre = mra.CrearCg(contexto, nombreCgPadre);
        }


        private static PuestoDtm CrearPT_Gerencia(ContextoSe contexto, List<RolDtm> roles)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_mra, n_pt_mra_gerente, "Define los roles y pemisos para el puesto de Gerencia");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Gestor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Gestor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            foreach(RolDtm r in roles)
               contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, r.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_mra_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_mra_cf, enumNegocio.Expediente, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            return pt;
        }

        private static PuestoDtm CrearPT_Secretaria(ContextoSe contexto, List<RolDtm> roles)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, n_nif_mra, n_pt_mra_secretaria, "Define los roles y pemisos para secretaría");

            AsignarAccesoFuncional(contexto, pt);

            var rol = InzSeguridadComun.CrearRolDeCallejeroDe(contexto, enumModoDeAccesoDeDatos.Gestor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            rol = InzSeguridadComun.CrearRolDeTercerosDe(contexto, enumModoDeAccesoDeDatos.Gestor);
            contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id);

            foreach (RolDtm r in roles)
                contexto.CrearRelacion<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, r.Id);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_mra_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, n_sociedad_mra_cf, enumNegocio.Expediente, pt.Id, enumModoDeAccesoDeDatos.Gestor);

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
