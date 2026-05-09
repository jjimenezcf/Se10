
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Juridico;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Presupuesto;
using Inicializador.Expedientes;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Seguridad;
using SistemaDeElementos.Inicializador;
using Utilidades;

namespace Inicializador.Procesos
{
    public class InzSolicitudDeContrato
    {
        public static readonly string n_scv = "SCV";
        public static readonly string n_scc = "SCC";

        public static void ModeloDeSolicitudesDeContrato(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Acciones(contexto);
                Etapas(contexto);
                Tipos(contexto);
                RolesDeSolicitudesDeContrato(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }


        public static readonly string n_estado_scv_abierto = $"{n_scv}: Abierto";
        public static readonly string n_estado_scv_en_elaboracion = $"{n_scv}: En elaboración";
        public static readonly string n_estado_scv_con_contrato = $"{n_scv}: Con contrato";
        public static readonly string n_estado_scv_sin_contrato = $"{n_scv}: No contratado";
        public static readonly string n_estado_scv_cancelado = $"{n_scv}: Cancelado";

        public static readonly string n_estado_scc_abierto = $"{n_scc}: Abierto";
        public static readonly string n_estado_scc_en_elaboracion = $"{n_scc}: En elaboración";
        public static readonly string n_estado_scc_con_contrato = $"{n_scc}: Con contrato";
        public static readonly string n_estado_scc_sin_contrato = $"{n_scc}: No contratado";
        public static readonly string n_estado_scc_cancelado = $"{n_scc}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scv_abierto, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scv_en_elaboracion, inicial: false, terminado: false, cancelado: false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scv_con_contrato, inicial: false, terminado: true, cancelado: false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scv_sin_contrato, inicial: false, terminado: true, cancelado: false, 45);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scv_cancelado, inicial: false, terminado: false, cancelado: true, 50);

                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scc_abierto, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scc_en_elaboracion, inicial: false, terminado: false, cancelado: false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scc_con_contrato, inicial: false, terminado: true, cancelado: false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scc_sin_contrato, inicial: false, terminado: true, cancelado: false, 45);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_scc_cancelado, inicial: false, terminado: false, cancelado: true, 50);

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
            foreach (EstadoDtm estado in estados)
                if (estado.Inicial) iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";

            foreach (EstadoDtm estado in estados)
                if (!estado.Inicial && !estado.Terminado && !estado.Cancelado)
                    enCurso = $"{(enCurso.IsNullOrEmpty() ? estado.Id.ToString() : $"{enCurso},{estado.Id}")}";

            var etapaPpts =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_scv_en_elaboracion).Id.ToString();

            var etapaScv =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_scv_en_elaboracion).Id + "," +
                           contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_scv_con_contrato).Id;

            var etapaScc =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_scc_en_elaboracion).Id + "," +
                           contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_scc_con_contrato).Id;

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, etapaPpts);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra, etapaScc);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta, etapaScv);
        }

        public static readonly string n_tran_scv_iniciar = $"{n_scv}: Elaborar";
        public static readonly string n_tran_scv_contratar = $"{n_scv}: Contratado";
        public static readonly string n_tran_scv_no_contratada = $"{n_scv}: No contratar";
        public static readonly string n_tran_scv_cancelar = $"{n_scv}: Cancelar";

        public static readonly string n_tran_scv_devolver = $"{n_scv}: Devolver a abierto";
        public static readonly string n_tran_scv_reabrir = $"{n_scv}: Reabrir";
        public static readonly string n_tran_scv_reactivar= $"{n_scv}: Reactivar";
        public static readonly string n_tran_scv_desasociar = $"{n_scv}: Desasociar";

        public static readonly string n_tran_scc_iniciar = $"{n_scc}: Elaborar";
        public static readonly string n_tran_scc_contratar = $"{n_scc}: Contratado";
        public static readonly string n_tran_scc_cerrar = $"{n_scc}: No contratar";
        public static readonly string n_tran_scc_cancelar = $"{n_scc}: Cancelar";

        public static readonly string n_tran_scc_devolver = $"{n_scc}: Devolver a abierto";
        public static readonly string n_tran_scc_reabrir = $"{n_scc}: Reabrir";
        public static readonly string n_tran_scc_reactivar = $"{n_scc}: Reactivar";
        public static readonly string n_tran_scc_desasociar = $"{n_scc}: Desasociar";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de expedientes");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_iniciar, n_estado_scv_abierto, n_estado_scv_en_elaboracion, activo: true, conObservacion: false, delSistema: false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_cancelar, n_estado_scv_abierto, n_estado_scv_cancelado, true, true, false, "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_contratar, n_estado_scv_en_elaboracion, n_estado_scv_con_contrato, true, false, true, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_no_contratada, n_estado_scv_en_elaboracion, n_estado_scv_sin_contrato, true, false, false, "Motivo de no contratación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_devolver, n_estado_scv_en_elaboracion, n_estado_scv_abierto, asunto: "Motivo de devolución");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_reabrir, n_estado_scv_cancelado, n_estado_scv_abierto, asunto: "Motivo de reapertura");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_reactivar, n_estado_scv_sin_contrato, n_estado_scv_en_elaboracion, asunto: "Motivo de activación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scv_desasociar, n_estado_scv_con_contrato, n_estado_scv_en_elaboracion, delSistema: true); 

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_iniciar, n_estado_scc_abierto, n_estado_scc_en_elaboracion, activo: true, conObservacion: false, delSistema: false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_cancelar, n_estado_scc_abierto, n_estado_scc_cancelado, true, true, false, "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_contratar, n_estado_scc_en_elaboracion, n_estado_scc_con_contrato, true, false, true, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_cerrar, n_estado_scc_en_elaboracion, n_estado_scc_sin_contrato, true, false, false, "Motivo de no contratación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_devolver, n_estado_scc_en_elaboracion, n_estado_scc_abierto, asunto: "Motivo de devolución");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_reabrir, n_estado_scc_cancelado, n_estado_scc_abierto, asunto: "Motivo de reapertura");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_reactivar, n_estado_scc_sin_contrato, n_estado_scc_en_elaboracion, asunto: "Motivo de activación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_scc_desasociar, n_estado_scc_con_contrato, n_estado_scc_en_elaboracion, delSistema: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Acciones(ContextoSe contexto)
        {
            var jsonVacio = "[]";

            var transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_scv_desasociar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Expediente, transicion, AccionesDeSolicitudes.N_DesasociarContratosDeLaSolicitud, enumMomentoDeEjecucion.A, jsonVacio, 10, "Desasociar contratos");

            transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_scv_devolver);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Expediente, transicion, AccionesDeSolicitudes.N_ValidarQueNoHayaContratos, enumMomentoDeEjecucion.A, jsonVacio, 10, "Valida que no hay contratos");

            transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_scv_no_contratada);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Expediente, transicion, AccionesDeSolicitudes.N_ValidarQueSiHayContratosEstanCancelados, enumMomentoDeEjecucion.A, jsonVacio, 10, "Valida que no hay contratos y si los hay, están cancelados");

            transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_scv_contratar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Expediente, transicion, AccionesDeSolicitudes.N_ValidarQueHayAlMenosUnContratoAsociadoNoCancelado, enumMomentoDeEjecucion.A, jsonVacio, 10, "Valida que al menos haya un contrato");
        }

        public static readonly string n_exp_tipo_expediente_scv = $"{n_scv}: Venta";
        public static readonly string n_exp_tipo_expediente_scc = $"{n_scc}: Compra";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de expedientes");
            try
            {
                var scv_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_scv_abierto);
                var scc_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_scc_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.solicitudContrato, n_exp_tipo_expediente_scv, scv_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, n_scv, usaPpts: true, scDeVenta: true, scDeCompra: false, usaDatosJuridicos: false);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.solicitudContrato, n_exp_tipo_expediente_scc, scc_estadoInicial.Id,
                    enumClaseDeLibro.POR_CG_TIPO, n_scc, usaPpts: false, scDeVenta: false, scDeCompra: true, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        public static readonly string n_rol_expediente_gestor_sc = "Expediente: (Gestor) Solicitud de Contratos";
        public static readonly string n_rol_expediente_apertura_sc = "Expediente: (Apertura) Solicitud de Contratos";

        private static void RolesDeSolicitudesDeContrato(ContextoSe contexto)
        {
            contexto.IniciarTraza("Crear roles datos de SC");
            try
            {
                CrearRolGestorDeExpedientesSc(contexto);
                CrearRolAperturaDeExpedientesSc(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void CrearRolGestorDeExpedientesSc(ContextoSe contexto)
        {

            var rol = new RolDtm();
            rol.Nombre = n_rol_expediente_gestor_sc;
            rol.Descripcion = "Define los permisos de gestor a los datos de las solicitudes de contrato";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipoScv = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_expediente_scv, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoScv.IdPermisoDeGestor);
            var tipoScc = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_expediente_scc, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoScc.IdPermisoDeGestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            permisosSobreElFlujoDeScv(contexto, rol);
            permisosSobreElFlujoDeScc(contexto, rol);
        }

        private static void permisosSobreElFlujoDeScv(ContextoSe contexto, RolDtm rol)
        {
            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_scv_abierto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_scv_en_elaboracion);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            var transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scv_cancelar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scv_no_contratada);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scv_contratar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scv_iniciar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
        }
        private static void permisosSobreElFlujoDeScc(ContextoSe contexto, RolDtm rol)
        {
            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_scc_abierto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_scc_en_elaboracion);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            var transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scc_cancelar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scc_cerrar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scc_contratar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_scc_iniciar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
        }

        private static void CrearRolAperturaDeExpedientesSc(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_expediente_apertura_sc;
            rol.Descripcion = "Define los permisos para poder abrir una solicitud de contrato";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipoScv = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_expediente_scv, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoScv.IdPermisoDeGestor);
            var tipoScc = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_expediente_scc, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipoScc.IdPermisoDeGestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_scv_abierto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_scc_abierto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
        }
    }
}
