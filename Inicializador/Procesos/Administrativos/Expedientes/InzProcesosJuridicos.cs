using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using Inicializador.Procesos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Seguridad;
using SistemaDeElementos.Inicializador;
using Utilidades;

namespace Inicializador.Expedientes
{
    public static class InzProcesosJuridicos
    {
        public static readonly string n_Jur = "JUR";

        public static void ModeloDeJuridica(ContextoSe contexto)
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

        public static readonly string n_estado_exp_jur_abierto = $"{n_Jur}: Abierto";
        public static readonly string n_estado_exp_jur_iniciado = $"{n_Jur}: Iniciado";
        public static readonly string n_estado_exp_jur_pendiente = $"{n_Jur}: Pdt de respuesta";
        public static readonly string n_estado_exp_jur_cerrado = $"{n_Jur}: Cerrado";
        public static readonly string n_estado_exp_jur_cancelado = $"{n_Jur}: Cancelado";



        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_jur_abierto, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_jur_iniciado, false, false, false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_jur_pendiente, false, false, false, 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_jur_cerrado, false, true, false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_jur_cancelado, false, false, true, 50);
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
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_exp_jur_iniciado).Id + "," +
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_exp_jur_pendiente).Id ;



            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, etapaPpts);
        }

        public static readonly string n_tran_exp_jur_iniciar = $"{n_Jur}: Iniciar";
        public static readonly string n_tran_exp_jur_solicitado = $"{n_Jur}: Solicitar";
        public static readonly string n_tran_exp_jur_respondido = $"{n_Jur}: Contestado";
        public static readonly string n_tran_exp_jur_cerrar = $"{n_Jur}: Cerrar";
        public static readonly string n_tran_exp_jur_cancelar = $"{n_Jur}: Cancelar";
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de solicitudes de contrato");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_jur_iniciar, n_estado_exp_jur_abierto, n_estado_exp_jur_iniciado, activo: true, conObservacion: false, delSistema: false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_jur_cancelar, n_estado_exp_jur_iniciado, n_estado_exp_jur_cancelado, true, true, false, "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_jur_solicitado, n_estado_exp_jur_iniciado, n_estado_exp_jur_pendiente, true, true, false, "A la espera de");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_jur_cerrar, n_estado_exp_jur_iniciado, n_estado_exp_jur_cerrado, true, true, false, "Resultado");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_jur_respondido, n_estado_exp_jur_pendiente, n_estado_exp_jur_iniciado, true, true, true, "Qué me han contestado");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_exp_tipo_expediente_juridico = $"{n_Jur}: Jurídico";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de expedientes");
            try
            {
                var jur_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_exp_jur_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.juridico, n_exp_tipo_expediente_juridico, jur_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, n_Jur, usaPpts: false, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_rol_expediente_gestor_juridico = "Expediente: (Gestor) Jurídico";
        public static readonly string n_rol_expediente_apertura_juridico = "Expediente: (Apertura) Jurídico";

        private static void RolesDeExpedientes(ContextoSe contexto)
        {
            contexto.IniciarTraza("Crear roles datos de Beniel");
            try
            {
                CrearRolGestorDeExpedientesJuridicos(contexto);
                CrearRolAperturaDeExpedientesJuridicos(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static void CrearRolGestorDeExpedientesJuridicos(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_expediente_gestor_juridico;
            rol.Descripcion = "Define los permisos de gestor a los datos del expediente jurídico";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipo = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_expediente_juridico, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeGestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_exp_jur_abierto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_exp_jur_iniciado);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

            var transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_exp_jur_iniciar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_exp_jur_cancelar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_exp_jur_solicitado);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_exp_jur_respondido);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);

            transicion = enumNegocio.Expediente.SeleccionarPorPropiedad<TransicionDtm>(contexto, nameof(TransicionDtm.Nombre), n_tran_exp_jur_cerrar);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
        }


        private static void CrearRolAperturaDeExpedientesJuridicos(ContextoSe contexto)
        {
            var rol = new RolDtm();
            rol.Nombre = n_rol_expediente_apertura_juridico;
            rol.Descripcion = "Define los permisos para poder abrir expedientes de jurídica";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);

            var tipo = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(INombre.Nombre), n_exp_tipo_expediente_juridico, aplicarJoin: true);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeGestor);
            InzSeguridadComun.AsignarPermisosDeTercerosDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);
            InzSeguridadComun.AsignarPermisosAlCallejeroDe(contexto, rol, enumModoDeAccesoDeDatos.Gestor);

            var estado = enumNegocio.Expediente.SeleccionarPorPropiedad<EstadoDtm>(contexto, nameof(EstadoDtm.Nombre), n_estado_exp_jur_abierto);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
        }



    }
}
