using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Terceros;
using Inicializador.Procesos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Utilidades;


namespace SistemaDeElementos.Inicializador.AytoBeniel
{
    public static class InzSeguridadBeniel
    {

        public const string n_rol_Registro_gestor_ree = "Registro: (Gestor) Ree";
        public const string n_rol_Registro_consultor_ree = "Registro: (Consultor) Ree";
        public const string n_rol_Tarea_gestor_trr = "Tarea: (Gestor) Trr";
        public const string n_rol_Tarea_consultor_trr = "Tarea: (Consultor) Trr";


        public const string n_pt_PuestoDeVentanilla = "Ventanilla de registro de E/S";
        public const string n_pt_ResponsableDeVentanilla = "Responsable del registro de E/S";
        public const string n_pt_JefeDeServicio = "Jefe de servicio";
        public const string n_pt_Tecnico = "Técnico";
        public const string n_pt_Administrativo = "Administrativo";


        public static void CrearRolesDeDatos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Crear roles datos de Beniel");
            var t = contexto.IniciarTransaccion();
            try
            {
                CrearRolGestorDeRegistrosRee(contexto);
                CrearRolConsultorDeRegistrosRee(contexto);
                CrearRolConsultorDeTareasTrr(contexto);
                CrearRolGestorDeTareasTrr(contexto);
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
        public static void CrearPuestosDeTrabajo(ContextoSe contexto)
        {
            contexto.IniciarTraza("Crear puestos de trabajo de Beniel");
            CrearPT_Responsable_De_Ventanilla(contexto);
            CrearPT_Puesto_De_Ventanilla(contexto);
            CrearPT_JefeDeServicio_De_Urb(contexto);
            CrearPT_JefeDeServicio_De_Atc(contexto);
            CrearPT_Tecnico_De_Urb(contexto);
            CrearPT_Administrativo_De_Atc(contexto);
        }


        private static void CrearPT_Administrativo_De_Atc(ContextoSe contexto)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, InzMaestrosBeniel.n_nif_beniel, InzMaestrosBeniel.n_cg_beniel_nombre_atc, n_pt_Administrativo, "Define los roles y pemisos para el administrativo de atención al ciudadano");

            AsociarAccesoFuncionalDelRegistroEs(contexto, pt);
            AsociarAccesoFuncionalDeTareas(contexto, pt);

            AsociarRolesParaLaGestionDeTareasTrr(contexto, pt);

            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo, enumNegocio.Registro, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_atc, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            var devolverAPdt = enumNegocio.Registro.Transicion(contexto,InzRegistroEs.n_estado_ree_asignado_atc, InzRegistroEs.n_estado_ree_pdt_asignar);
            var permisosDeUnPt = new PermisosDirectosDtm();
            permisosDeUnPt.IdPermiso = devolverAPdt.IdPermiso;
            permisosDeUnPt.IdPuesto = pt.Id;
            permisosDeUnPt.CrearRelacion(contexto, errorSiExiste: false);
        }

        private static void CrearPT_Tecnico_De_Urb(ContextoSe contexto)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, InzMaestrosBeniel.n_nif_beniel, InzMaestrosBeniel.n_cg_beniel_nombre_urb, n_pt_Tecnico, "Define los roles y pemisos para el técnico de urbanismo");

            AsociarAccesoFuncionalDelRegistroEs(contexto, pt);
            AsociarAccesoFuncionalDeTareas(contexto, pt);

            AsociarRolesParaLaGestionDeTareasTrr(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo, enumNegocio.Registro, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_urb, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            var devolverAPdt = enumNegocio.Registro.Transicion(contexto, InzRegistroEs.n_estado_ree_asignado_urb, InzRegistroEs.n_estado_ree_pdt_asignar);
            var permisosDeUnPt = new PermisosDirectosDtm();
            permisosDeUnPt.IdPermiso = devolverAPdt.IdPermiso;
            permisosDeUnPt.IdPuesto = pt.Id;
            permisosDeUnPt.CrearRelacion(contexto, errorSiExiste: false);
        }

        private static void CrearPT_JefeDeServicio_De_Atc(ContextoSe contexto)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, InzMaestrosBeniel.n_nif_beniel, InzMaestrosBeniel.n_cg_beniel_nombre_atc, n_pt_JefeDeServicio, "Define los roles y pemisos para el Jefe de Servicio de atención al cliente");

            AsociarAccesoFuncionalDelRegistroEs(contexto, pt);
            AsociarAccesoFuncionalDeTareas(contexto, pt);

            AsociarRolesParaLaConsultaDelRegistroRee(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, enumNegocio.Registro, pt.Id, enumModoDeAccesoDeDatos.Consultor);

            AsociarRolesParaLaGestionDeTareasTrr(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_atc, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_atc, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);
        }

        private static void CrearPT_JefeDeServicio_De_Urb(ContextoSe contexto)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, InzMaestrosBeniel.n_nif_beniel, InzMaestrosBeniel.n_cg_beniel_nombre_urb, n_pt_JefeDeServicio, "Define los roles y pemisos para el Jefe de Servicio de Urbanismo");

            AsociarAccesoFuncionalDelRegistroEs(contexto, pt);
            AsociarAccesoFuncionalDeTareas(contexto, pt);

            AsociarRolesParaLaConsultaDelRegistroRee(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, enumNegocio.Registro, pt.Id, enumModoDeAccesoDeDatos.Consultor);

            AsociarRolesParaLaGestionDeTareasTrr(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_urb, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_urb, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);
        }

        private static void CrearPT_Responsable_De_Ventanilla(ContextoSe contexto)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, InzMaestrosBeniel.n_nif_beniel, n_pt_ResponsableDeVentanilla, "Define los roles y pemisos del responsable del registro de entrada y salida del ayuntamiento");

            AsociarAccesoFuncionalDelRegistroEs(contexto, pt);
            AsociarAccesoFuncionalDeTareas(contexto, pt);

            AsociarRolesParaLaGestionDelRegistroRee(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, enumNegocio.Registro, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            var cerrarRegistro = enumNegocio.Registro.Transicion(contexto, InzRegistroEs.n_tran_ree_cerrar_registro);
            var permisosDeUnPt = new PermisosDirectosDtm();
            permisosDeUnPt.IdPermiso = cerrarRegistro.IdPermiso;
            permisosDeUnPt.IdPuesto = pt.Id;
            permisosDeUnPt.CrearRelacion(contexto, errorSiExiste: false);

            AsociarRolesParaLaGestionDeTareasTrr(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_atc, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_urb, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Consultor);

        }

        private static void CrearPT_Puesto_De_Ventanilla(ContextoSe contexto)
        {
            var pt = GestorDePuestosDeTrabajo.PersistirPt(contexto, InzMaestrosBeniel.n_nif_beniel, n_pt_PuestoDeVentanilla, "Define los roles y pemisos para un puesto en una ventanilla del registro del ayuntamiento");

            AsociarAccesoFuncionalDelRegistroEs(contexto, pt);
            AsociarAccesoFuncionalDeTareas(contexto, pt);

            AsociarRolesParaLaGestionDelRegistroRee(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, enumNegocio.Registro, pt.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Gestor);

            AsociarRolesParaLaGestionDeTareasTrr(contexto, pt);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_atc, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_urb, enumNegocio.Tarea, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_atc, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
            GestorDeNegociosDeUnCg.AsignarPermisos(contexto, InzMaestrosBeniel.n_cg_beniel_codigo_urb, enumNegocio.Archivador, pt.Id, enumModoDeAccesoDeDatos.Consultor);
        }

        private static void AsociarAccesoFuncionalDelRegistroEs(ContextoSe contexto, PuestoDtm pt)
        {
            var gestor = GestorDeRolesDeUnPuesto.Gestor(contexto, contexto.Mapeador);
            var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Entorno);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);
            
            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Callejero);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_TercerosBasico);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Sistema_Documental);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_RegistroEs);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Tareas);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

        }

        private static void AsociarAccesoFuncionalDeTareas(ContextoSe contexto, PuestoDtm pt)
        {
            var gestor = GestorDeRolesDeUnPuesto.Gestor(contexto, contexto.Mapeador);
            var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Entorno);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_TercerosBasico);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Sistema_Documental);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

            rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), InzSeguridadComun.n_rol_AFB_Tareas);
            gestor.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);

        }

        private static void AsociarRolesParaLaConsultaDelRegistroRee(ContextoSe contexto, PuestoDtm pt)
        {
            var gestorRol = GestorDeRolesDeUnPuesto.Gestor(contexto, contexto.Mapeador);
            var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), n_rol_Registro_consultor_ree);
            gestorRol.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);
        }

        private static void AsociarRolesParaLaGestionDelRegistroRee(ContextoSe contexto, PuestoDtm pt)
        {
            var gestorRol = GestorDeRolesDeUnPuesto.Gestor(contexto, contexto.Mapeador);
            var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), n_rol_Registro_gestor_ree);
            gestorRol.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);
        }

        private static void AsociarRolesParaLaGestionDeTareasTrr(ContextoSe contexto, PuestoDtm pt)
        {
            var gestorR = GestorDeRolesDeUnPuesto.Gestor(contexto, contexto.Mapeador);
            var rol = contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), n_rol_Tarea_gestor_trr);
            gestorR.CrearRelacion(nameof(RolesDeUnPuestoDtm.IdPuesto), pt.Id, rol.Id, errorSiYaExiste: false);
        }


        private static void CrearRolGestorDeRegistrosRee(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_Registro_gestor_ree;
            rol.Descripcion = "Define los permisos de gestor a los datos del registro de entrada y salida";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            AsignarPermisosDeTipoDeReeDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Registro.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), InzRegistroEs.n_estado_ree_pdt_asignar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Registro.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), InzRegistroEs.n_estado_ree_pdt_respuesta);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            var transicion = enumNegocio.Registro.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzRegistroEs.n_tran_ree_asignar_atc);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Registro.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzRegistroEs.n_tran_ree_asignar_urb);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Registro.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzRegistroEs.n_tran_ree_cancelar_registro);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Registro.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzRegistroEs.n_tran_ree_registro_resuelto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
        }

        private static void CrearRolConsultorDeRegistrosRee(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_Registro_consultor_ree;
            rol.Descripcion = "Define los permisos de acceso a la consulta de datos del registro de entrada y salida";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            AsignarPermisosDeTipoDeReeDe(contexto, rol, enumModoDeAccesoDeDatos.Consultor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Consultor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Consultor);
        }

        private static void CrearRolConsultorDeTareasTrr(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_Tarea_consultor_trr;
            rol.Descripcion = "Define los permisos de acceso a la consulta de datos de las tareas de resolución de registro";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            AsignarPermisosDeTipoDeTrrDe(contexto, rol, enumModoDeAccesoDeDatos.Consultor);
        }

        private static void CrearRolGestorDeTareasTrr(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_Tarea_gestor_trr;
            rol.Descripcion = "Define los permisos de acceso a la gestión y resolución de datos de las tareas de resolución de registro";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            AsignarPermisosDeTipoDeTrrDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Tarea.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), InzTareasRre.n_estado_trr_en_resolucion);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            var transicion = enumNegocio.Tarea.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzTareasRre.n_tran_trr_cancelar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Tarea.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzTareasRre.n_tran_trr_iniciar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Tarea.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), InzTareasRre.n_tran_trr_resolver);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

        }

        private static void AsignarPermisosDeTipoDeTrrDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorPropiedad<TipoDeTareaDtm>(nameof(INombre.Nombre), InzTareasRre.n_tipo_trr, aplicarJoin: true);

            if (modo.SoyGestor())
            {
                var estado = enumNegocio.Tarea.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), InzTareasRre.n_estado_trr_pendiente);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeGestor);
                if (tipo.TipoArchivador != null) contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.TipoArchivador.IdPermisoDeGestor);
            }
            else
            if(modo.SoyConsultor())
            {
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeConsultor);
                if (tipo.TipoArchivador != null) contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.TipoArchivador.IdPermisoDeConsultor);
            }
        }

        private static void AsignarPermisosDeTipoDeReeDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorPropiedad<TipoDeRegistroEsDtm>(nameof(INombre.Nombre), InzRegistroEs.n_tipo_ree, aplicarJoin: true);

            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.TipoArchivadorDeEntrada.IdPermisoDeGestor : tipo.TipoArchivadorDeEntrada.IdPermisoDeConsultor);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.TipoArchivadorDeSalida.IdPermisoDeGestor : tipo.TipoArchivadorDeEntrada.IdPermisoDeConsultor);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.TipoArchivadorInterno.IdPermisoDeConsultor);
        }

    }
}
