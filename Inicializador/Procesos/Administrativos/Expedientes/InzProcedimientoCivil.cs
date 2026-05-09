using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Seguridad;
using SistemaDeElementos.Inicializador;
using Utilidades;

namespace Inicializador.Procedimientos
{
    public static class InzProcedimientoCivil
    {
        public static readonly string n_civ = "CIV";

        public static void ModeloCivil(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Etapas(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                RolesDeExpedientes(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_civ_previo = $"{n_civ}: Previo";
        public static readonly string n_estado_civ_preparar_demanda = $"{n_civ}: Preparando demanda";
        public static readonly string n_estado_civ_pdt_de_admision = $"{n_civ}: Pdt de admisión";
        public static readonly string n_estado_civ_contestado = $"{n_civ}: Contestado";
        public static readonly string n_estado_civ_en_audiencia_previa = $"{n_civ}: En audiencia";
        public static readonly string n_estado_civ_en_jucio = $"{n_civ}: En jucio";
        public static readonly string n_estado_civ_sentenciado = $"{n_civ}: Sentenciado";
        public static readonly string n_estado_civ_ejecutado = $"{n_civ}: Ejecutado";
        public static readonly string n_estado_civ_no_admitida = $"{n_civ}: No admitida";
        public static readonly string n_estado_civ_con_acuerdo = $"{n_civ}: Con acuerdo";
        public static readonly string n_estado_civ_cancelado = $"{n_civ}: Cancelado";



        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del procedimiento judicial");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_previo, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_preparar_demanda, inicial: false, terminado: false, cancelado: false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_pdt_de_admision, inicial: false, terminado: false, cancelado: false, 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_contestado, inicial: false, terminado: false, cancelado: false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_en_audiencia_previa, inicial: false, terminado: false, cancelado: false, 50);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_en_jucio, inicial: false, terminado: false, cancelado: false, 60);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_sentenciado, inicial: false, terminado: false, cancelado: false, 70);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_ejecutado, inicial: false, terminado: true, cancelado: false, 80);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_no_admitida, inicial: false, terminado: true, cancelado: false, 81);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_con_acuerdo, inicial: false, terminado: true, cancelado: false, 82);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_civ_cancelado, inicial: false, terminado: false, cancelado: true, 99);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Etapas(ContextoSe contexto)
        {
            ExtensorDeExpedientes.DefinirEtapasBasicas(contexto);
            
            var estados = enumNegocio.Expediente.Estados(contexto);
            var iniciales = "";
            var enCurso = "";
            var cancelados = "";
            var terminados = "";

            foreach (EstadoDtm estado in estados)
                if (estado.Inicial) 
                    iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales}, {estado.Id}")}";

            foreach (EstadoDtm estado in estados)
                if (!estado.Inicial && !estado.Terminado && !estado.Cancelado)
                    enCurso = $"{(enCurso.IsNullOrEmpty() ? estado.Id.ToString() : $"{enCurso}, {estado.Id}")}";

            foreach (EstadoDtm estado in estados.Where(estado => estado.Cancelado))
                    cancelados = $"{(cancelados.IsNullOrEmpty() ? estado.Id.ToString() : $"{cancelados}, {estado.Id}")}";

            foreach (EstadoDtm estado in estados.Where(estado => estado.Terminado))
                terminados = $"{(terminados.IsNullOrEmpty() ? estado.Id.ToString() : $"{terminados}, {estado.Id}")}";

            var ppts = contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_previo).Id.ToString();
            var enJuzgado = contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_pdt_de_admision).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_contestado).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_en_audiencia_previa).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_en_jucio).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_sentenciado).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_ejecutado).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_no_admitida).Id + ", " +
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_civ_con_acuerdo).Id ;

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + ", " + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, ppts);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enCurso);
            enumNegocio.Expediente.ResetearParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_En_Juzgado, enJuzgado);
        }

        public static readonly string n_tran_civ_demandar = $"{n_civ}: Demandar";

        public static readonly string n_tran_civ_presentar = $"{n_civ}: Presentada";
        public static readonly string n_tran_civ_contestada = $"{n_civ}: Contestada";
        public static readonly string n_tran_civ_rechazada = $"{n_civ}: No Admitida";

        public static readonly string n_tran_civ_intentar_acuerdo = $"{n_civ}: Intentar Acuerdo";
        public static readonly string n_tran_civ_a_jucio = $"{n_civ}: Ir a jucio";
        public static readonly string n_tran_civ_cerrada = $"{n_civ}: Acuerdo de las partes";

        public static readonly string n_tran_civ_sentenciada = $"{n_civ}: Sentenciada";
        public static readonly string n_tran_civ_Ejecutada = $"{n_civ}: Ejecutada";

        public static readonly string n_tran_civ_cancelar = $"{n_civ}: Cancelar";
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de solicitudes de procedimientos judiciales");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_demandar, n_estado_civ_previo, n_estado_civ_preparar_demanda, activo: true, conObservacion: false, delSistema: false, porDefecto: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_cancelar, n_estado_civ_previo, n_estado_civ_cancelado, activo: true, conObservacion: true, delSistema: false, asunto: "Motivo de cancelación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_presentar, n_estado_civ_preparar_demanda, n_estado_civ_pdt_de_admision, activo: true, conObservacion: false, delSistema: false);

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_rechazada, n_estado_civ_pdt_de_admision, n_estado_civ_no_admitida, activo: true, conObservacion: true, delSistema: false, asunto: "Motivo de no admisión");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_contestada, n_estado_civ_pdt_de_admision, n_estado_civ_contestado, activo: true, conObservacion: false, delSistema: false);

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_intentar_acuerdo, n_estado_civ_contestado, n_estado_civ_en_audiencia_previa, activo: true, conObservacion: false, delSistema: false);

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_cerrada, n_estado_civ_en_audiencia_previa, n_estado_civ_con_acuerdo, activo: true, conObservacion: false, delSistema: false,porDefecto: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_a_jucio, n_estado_civ_en_audiencia_previa, n_estado_civ_en_jucio, activo: true, conObservacion: true, delSistema: false, asunto: "Motivo del no acuerdo", porDefecto: true);
                                
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_a_jucio, n_estado_civ_en_jucio, n_estado_civ_sentenciado, activo: true, conObservacion: false, delSistema: false, porDefecto: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_civ_Ejecutada, n_estado_civ_sentenciado, n_estado_civ_ejecutado, activo: true, conObservacion: false, delSistema: false, porDefecto: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_exp_tipo_civil = $"{n_civ}: Procedimiento Civil";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de procedimiento");
            try
            {
                var jur_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_civ_previo);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.juridico, n_exp_tipo_civil, jur_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, n_civ, usaPpts: true, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_rol_gestor_civil = "Procedimiento: (Gestor) Civil";
        public static readonly string n_rol_expediente_apertura_juridico = "Procedimiento: (Apertura) Civil";

        private static void RolesDeExpedientes(ContextoSe contexto)
        {
            contexto.IniciarTraza("Crear roles datos de procedimientos");
            try
            {
                CrearRolGestorDeCivil(contexto);
                CrearRolAperturaDeCivil(contexto);
                CrearRolConsultorCivil(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void CrearRolConsultorCivil(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_expediente_apertura_juridico;
            rol.Descripcion = "Define los permisos para poder consultar procesos civiles";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipo = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_civil, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeConsultor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Consultor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Consultor);
        }

        private static void CrearRolGestorDeCivil(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_gestor_civil;
            rol.Descripcion = "Define los permisos de gestor a los datos del procedimiento civil";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipo = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_civil, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeGestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_previo);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_preparar_demanda);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_contestado);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);


            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_en_audiencia_previa);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_en_jucio);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_sentenciado);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            var transiciones = ExtensorDeTransiciones.Transiciones(enumNegocio.Expediente, contexto, estadoOrigen: null, estadoDestino: null);
            foreach (var transicion in transiciones)
            {
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
            }
        }


        private static void CrearRolAperturaDeCivil(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_expediente_apertura_juridico;
            rol.Descripcion = "Define los permisos para poder abrir expedientes de jurídica";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipo = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_civil, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeGestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_civ_previo);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
        }



    }
}
