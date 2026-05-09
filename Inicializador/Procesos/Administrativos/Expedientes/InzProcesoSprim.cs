using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Inicializador.Procesos;
using ServicioDeDatos.Presupuesto;
using Inicializador.Presupuestos;

namespace Inicializador.Expedientes
{
    public static class InzProcesoSprim
    {

        #region definición de textos del flujo
        public static readonly string n_spr = "SPR";

        public static readonly string n_estado_spr_abierto = $"{n_spr}: Abierto";
        public static readonly string n_estado_spr_ejecuntandose = $"{n_spr}: Ejecutándose";
        public static readonly string n_estado_spr_cerrado = $"{n_spr}: Cerrado";
        public static readonly string n_estado_spr_cancelado = $"{n_spr}: Cancelado";


        public static readonly string n_exp_tipo_expediente_spr = $"{n_spr}: Sprint de Emuasa";

        public static readonly string n_tran_spr_cancelar = $"{n_spr}: Cancelar";

        public static readonly string n_tran_spr_iniciar = $"{n_spr}: Iniciar";
        public static readonly string n_tran_spr_valorar = $"{n_spr}: Valorar";
        
        public static readonly string n_tran_spr_reabrir = $"{n_spr}: Reabrir";
        public static readonly string n_tran_spr_cerrar = $"{n_spr}: Cerrar";
        #endregion


        public static readonly string n_rol_spr_gestor = $"Expediente: Gestor de {n_spr}";
        public static readonly string n_rol_spr_consultor = $"Expediente: Consultor de {n_spr}";

        public static void ProcesoDeSprin(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Etapas(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                Acciones(contexto);
                CrearRol(contexto, enumModoDeAccesoDeDatos.Gestor);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }


        private static void Etapas(ContextoSe contexto)
        {
            ExtensorDeExpedientes.DefinirEtapasBasicas(contexto);
            var etapaTarea =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_spr_abierto).Id.ToString();

            var etapaPpts =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_spr_abierto).Id + "," +
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_spr_ejecuntandose).Id + ",";

            var etapaEjecucion =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_spr_ejecuntandose).Id.ToString();

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, etapaTarea);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, etapaPpts);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, etapaEjecucion);
        }

        internal static RolDtm CrearRol(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            var rol = new RolDtm();
            rol.Nombre = ModoDeAcceso.SoyGestor(modo) ? n_rol_spr_gestor : n_rol_spr_consultor;
            rol.Descripcion = $"{(ModoDeAcceso.SoyGestor(modo) ? "Gestor" : "Consultor")} del proceso {n_spr}";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosDeSPRDe(contexto, rol, modo);
            return rol;
        }

        public static void AsignarPermisosDeSPRDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_expediente_spr);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);

            if (ModoDeAcceso.SoyGestor(modo))
            {
                var estado = enumNegocio.Expediente.Estado(contexto, n_estado_spr_abierto);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

                var transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_spr_iniciar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_spr_cancelar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_spr_cerrar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_spr_reabrir);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
            }
        }


        public static void Acciones(ContextoSe contexto)
        {
            contexto.IniciarTraza($"Acciones del proceso {n_spr}");
            try
            {
                CrearAcciones(contexto);
                AccionesDeNegocio(contexto);
                AccionAlTransitar(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de expedientes");
            try
            {
                var spr_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_spr_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.DeCliente, n_exp_tipo_expediente_spr, spr_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, sigla: n_spr, 
                    usaPpts: true, 
                    scDeVenta: false, 
                    scDeCompra: false,
                    usaDatosJuridicos: false
                    );
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Transiciones(ContextoSe contexto)
        {
            //abierto --> ejecutándose, Cancelado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_spr_iniciar, n_estado_spr_abierto, n_estado_spr_ejecuntandose);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_spr_valorar, n_estado_spr_abierto, n_estado_spr_ejecuntandose);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_spr_cancelar, n_estado_spr_abierto, n_estado_spr_cancelado, asunto: "Motivo de cancelación");

            //ejecutándose -->  cerrado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_spr_cerrar, n_estado_spr_ejecuntandose, n_estado_spr_cerrado);

            //Cerrado -->  ejecutándose
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_spr_reabrir, n_estado_spr_cerrado, n_estado_spr_abierto, asunto: "Motivo de reapertura");
        }

        private static void Estados(ContextoSe contexto)
        {
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_spr_abierto, inicial: true, orden: 10);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_spr_ejecuntandose, orden: 15);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_spr_cerrado, terminado: true, orden: 15);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_spr_cancelado, cancelado: true, orden: 80);
        }

        private static void CrearAcciones(ContextoSe contexto)
        {
            var accion = new AccionDtm();
            accion.Dll = $"{nameof(GestoresDeNegocio)}";
            accion.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDeExpedientes)}";
                        
            accion.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            accion.Nombre = AccionesDeExpedientes.N_ValidarQueHayPresupuesto;
            accion.Descripcion = $@"Valida que el expediente tiene al menos un presupuesto asociado y no está cancelado";
            accion.Metodo = nameof(AccionesDeExpedientes.ValidarQueHayPresupuesto);
            accion.PersistirAccion(contexto);

            accion.Id = 0;
            accion.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            accion.Nombre = AccionesDeExpedientes.N_ValidarQueSiHayPresupuestosNoEstanPrefacturados;
            accion.Descripcion = AccionesDeExpedientes.N_ValidarQueSiHayPresupuestosNoEstanPrefacturados;
            accion.Metodo = nameof(AccionesDeExpedientes.ValidarQueSiHayPresupuestosNoEstanPrefacturados);
            accion.PersistirAccion(contexto);
            
            accion.Id = 0;
            accion.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            accion.Nombre = AccionesDeExpedientes.N_ValidarQueHayTareasRelacionadas;
            accion.Descripcion = $@"Valida que el expediente tiene al menos una tarea de alguno de los tipos indicados (y que no esté cancelada){Environment.NewLine}Parametros:{Environment.NewLine}{AccionesDeExpedientes.parametrosParaValidarTareasRelacionadas}";
            accion.Metodo = nameof(AccionesDeExpedientes.ValidarQueHayTareasRelacionadas);
            accion.PersistirAccion(contexto);

            accion.Id = 0;
            accion.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            accion.Nombre = AccionesDeExpedientes.N_ValidarQueLasTareasRelacionadasEstanPlanificadas;
            accion.Descripcion = $@"Valida que las tareas del expediente, que son planificables y no están canceladas, están planificadas";
            accion.Metodo = nameof(AccionesDeExpedientes.ValidarQueLasTareasRelacionadasEstanPlanificadas);
            accion.PersistirAccion(contexto);

            accion.Id = 0;
            accion.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            accion.Nombre = AccionesDeExpedientes.N_PresupuestarTareasRelacionadas;
            accion.Descripcion = $@"Presupuesta las tareas relacionadas al expediente, que no estén ya presupuestadas{Environment.NewLine}{AccionesDeExpedientes.ParametrosParaPresupuestarTareasRelacionadas}";
            accion.Metodo = nameof(AccionesDeExpedientes.PresupuestarTareasRelacionadas);
            accion.PersistirAccion(contexto);
        }

        private static void AccionesDeNegocio(ContextoSe contexto)
        {

        }

        private static void AccionAlTransitar(ContextoSe contexto)
        {
            var transiciones = enumNegocio.Expediente.TransicionesPorNombre(contexto, n_tran_spr_iniciar);
            var tipoEvo = contexto.SeleccionarPorNombre<TipoDeTareaDtm>(InzTareasEvo.n_tipo_Evo);
            var tipoSpc = contexto.SeleccionarPorNombre<TipoDeTareaDtm>(InzTareasSpc.n_tipo_Spc);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
             negocio: enumNegocio.Expediente,
             transicion: transiciones[0],
             nombreAccion: AccionesDeExpedientes.N_ValidarQueHayTareasRelacionadas,
             momento: enumMomentoDeEjecucion.A,
             parametro: AccionesDeExpedientes.parametrosParaValidarTareasRelacionadas.Replace($"@{AccionesDeExpedientes.enumParametros.IdTiposDeTarea}", $"{tipoEvo.Id};{tipoSpc.Id}"),
             orden: 10,
             descripcion: $"Valida que el sprint tenga relacionadas al menos una tare de tipo '{tipoSpc.Nombre}' o '{tipoEvo.Nombre}' que este activa o terminada");

            transiciones = enumNegocio.Expediente.TransicionesPorNombre(contexto, n_tran_spr_valorar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
             negocio: enumNegocio.Expediente,
             transicion: transiciones[0],
             nombreAccion: AccionesDeExpedientes.N_ValidarQueHayTareasRelacionadas,
             momento: enumMomentoDeEjecucion.A,
             parametro: AccionesDeExpedientes.parametrosParaValidarTareasRelacionadas.Replace($"@{AccionesDeExpedientes.enumParametros.IdTiposDeTarea}", $"{tipoEvo.Id};{tipoSpc.Id}"),
             orden: 10,
             descripcion: $"Valida que el sprint tenga relacionadas al menos una tare de tipo '{tipoSpc.Nombre}' o '{tipoEvo.Nombre}' que este activa o terminada");


            var tipoPpt = contexto.SeleccionarPorNombre<TipoDePresupuestoDtm>(InzValoraciones.n_tipo_valoracion);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
             negocio: enumNegocio.Expediente,
             transicion: transiciones[0],
             nombreAccion: AccionesDeExpedientes.N_PresupuestarTareasRelacionadas,
             momento: enumMomentoDeEjecucion.A,
             parametro: AccionesDeExpedientes.ParametrosParaPresupuestarTareasRelacionadas.
                        Replace($"@{AccionesDeExpedientes.enumParametros.IdTipoPpt}", $"{tipoPpt.Id}").
                        Replace($"@{AccionesDeExpedientes.enumParametros.Naturaleza}", "CST").
                        Replace($"@{AccionesDeExpedientes.enumParametros.PrecioPorHora}", $"33,66"),
             orden: 20,
             descripcion: $"Crea una valoración en base a las tareas asociadas");

            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
             negocio: enumNegocio.Expediente,
             transicion: transiciones[0],
             nombreAccion: AccionesDeExpedientes.N_ValidarQueLasTareasRelacionadasEstanPlanificadas,
             momento: enumMomentoDeEjecucion.A,
             parametro: null,
             orden: 20,
             descripcion: $"Valida que el sprint tenga las tareas planificadas");

            transiciones = enumNegocio.Expediente.TransicionesPorNombre(contexto, n_tran_spr_reabrir);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
             negocio: enumNegocio.Expediente,
             transicion: transiciones[0],
             nombreAccion: AccionesDeExpedientes.N_ValidarQueSiHayPresupuestosNoEstanPrefacturados,
             momento: enumMomentoDeEjecucion.D,
             parametro: null,
             orden: 10,
             descripcion: $"Valida que si el sprint se ha facturado, no permita abrirlo");
        }


    }
}
