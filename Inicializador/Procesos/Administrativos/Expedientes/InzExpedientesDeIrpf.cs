using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Inicializador.SistemaDocumental;
using ServicioDeDatos.Seguridad;

namespace Inicializador.Expedientes
{
    public static class InzExpedientesDeIrpf
    {
        
        #region definición de textos del flujo
        public static readonly string n_irpf = "IRPF";

        public static readonly string n_estado_irpf_abierto = $"{n_irpf}: Abierto";
        public static readonly string n_estado_irpf_en_elaboracion = $"{n_irpf}: En elaboración";
        public static readonly string n_estado_irpf_presentado = $"{n_irpf}: Presentado";
        public static readonly string n_estado_irpf_pdt_documentacion = $"{n_irpf}: Pdt de documentación";
        public static readonly string n_estado_irpf_cancelado = $"{n_irpf}: Cancelado";


        public static readonly string n_exp_tipo_expediente_irpf = $"{n_irpf}: Renta";

        public static readonly string n_tran_irpf_cancelar = $"{n_irpf}: Cancelar";

        public static readonly string n_tran_irpf_comenzar = $"{n_irpf}: Comenzar";
        public static readonly string n_tran_irpf_devolver_abierto = $"{n_irpf}: Devolver";
        public static readonly string n_tran_irpf_parar = $"{n_irpf}: Esperar documentación";
        public static readonly string n_tran_irpf_presentar = $"{n_irpf}: Presentar";

        public static readonly string n_tran_irpf_continuar = $"{n_irpf}: Continuar";
        public static readonly string n_tran_irpf_corregir = $"{n_irpf}: Corregir";
        #endregion

        private static readonly string parametroDeTransicion = @"[{""parametro"": ""p_2"",""valor"": @p_2 }]"
        .Replace("p_2", AccionesDeIrpf.enumParametros.IdTipoArchivador.ToString());
       
        private static readonly string parametroDeNegocio =
        @"[{""parametro"": ""p_1"",""valor"": @p_1 },{""parametro"": ""p_2"",""valor"": @p_2 }]"
        .Replace("p_1", AccionesDeIrpf.enumParametros.IdTipoExpediente.ToString())
        .Replace("p_2", AccionesDeIrpf.enumParametros.IdTipoArchivador.ToString());


        public static readonly string n_rol_irpf_gestor = $"Expediente: Gestor de {n_irpf}";
        public static readonly string n_rol_irpf_consultor = $"Expediente: Consultor de {n_irpf}";

        public static void ProcesoIrpf(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                Etapas(contexto);
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

        internal static RolDtm CrearRol(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            var rol = new RolDtm();
            rol.Nombre = ModoDeAcceso.SoyGestor(modo) ? n_rol_irpf_gestor : n_rol_irpf_consultor;
            rol.Descripcion = $"{(ModoDeAcceso.SoyGestor(modo)? "Gestor": "Consultor")} del proceso de IRPF";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosDeIRPFDe(contexto, rol, modo);
            return rol;
        }

        public static void AsignarPermisosDeIRPFDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_expediente_irpf);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);

            if (ModoDeAcceso.SoyGestor(modo))
            {
                var estado = enumNegocio.Expediente.Estado(contexto, n_estado_irpf_abierto);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
                estado = enumNegocio.Expediente.Estado(contexto, n_estado_irpf_en_elaboracion);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);
                estado = enumNegocio.Expediente.Estado(contexto, n_estado_irpf_pdt_documentacion);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

                var transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_comenzar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_parar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_continuar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_presentar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
            }
        }


        public static void Acciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Acciones de las transiciones de tareas de TRR");
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
                var irpf_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_irpf_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.DeCliente, n_exp_tipo_expediente_irpf, irpf_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, 
                    sigla: n_irpf, usaPpts: false, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
        
        private static void Transiciones(ContextoSe contexto)
        {
            //abierto --> En elaboración, Cancelado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_comenzar, n_estado_irpf_abierto, n_estado_irpf_en_elaboracion);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_cancelar, n_estado_irpf_abierto, n_estado_irpf_cancelado, asunto: "Motivo de cancelación");

            //En elaboración -->  Presentado, Abierto, Parado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_presentar, n_estado_irpf_en_elaboracion, n_estado_irpf_presentado);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_devolver_abierto, n_estado_irpf_en_elaboracion, n_estado_irpf_abierto, asunto: "Motivo de devolución");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_parar, n_estado_irpf_en_elaboracion, n_estado_irpf_pdt_documentacion, asunto: "Motivo de espera");

            //Parado --> En elaboración y Presentado --> En elaboración
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_continuar, n_estado_irpf_pdt_documentacion, n_estado_irpf_en_elaboracion);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_irpf_corregir, n_estado_irpf_presentado, n_estado_irpf_en_elaboracion, asunto: "Motivo de corrección");

        }
        
        private static void Estados(ContextoSe contexto)
        {
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_irpf_abierto, inicial: true, orden: 10);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_irpf_en_elaboracion, orden: 15);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_irpf_pdt_documentacion, orden: 25);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_irpf_presentado, terminado: true, orden: 75);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_irpf_cancelado, cancelado: true, orden: 80);
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

            var etapaEjecucion =
                contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_irpf_en_elaboracion).Id.ToString();

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.ResetearParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, etapaEjecucion);
        }

        private static void CrearAcciones(ContextoSe contexto)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDeIrpf)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();

            #region Cerrar archivador de la documentacion del irpf
            a.Nombre = AccionesDeIrpf.N_CerrarArchivadorDeIrpf;
            a.Descripcion = @$"Cierra el archivador de la documentación del irpf.{Environment.NewLine}Parámetros: {parametroDeTransicion}";
            a.Metodo = nameof(AccionesDeIrpf.CerrarArchivadorDeIrpf);
            a.PersistirAccion(contexto);
            #endregion


            #region Abrir archivador de la documentación del irpf
            a.Nombre = AccionesDeIrpf.N_AbrirArchivadorDeIrpf;
            a.Descripcion = $@"Crea o activa un archivador para poder incluir la documentación del irpf.{Environment.NewLine}Parámetros: {parametroDeTransicion}";
            a.Metodo = nameof(AccionesDeIrpf.AbrirArchivadorDeIrpf);
            a.PersistirAccion(contexto);
            #endregion

        }

        private static void AccionesDeNegocio(ContextoSe contexto)
        {
            var tipoExpediente = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_expediente_irpf);
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_fiscal);
            GestorDeAccionesDeNegocio.CrearAccionDeNegocio(contexto, 
                negocio:enumNegocio.Expediente,
                nombre: AccionesDeIrpf.N_AbrirArchivadorDeIrpf,
                momento: enumMomentoDeAccion.DC,
                parametro: parametroDeNegocio.Replace($"@{AccionesDeIrpf.enumParametros.IdTipoExpediente}", $"{tipoExpediente.Id}")
                           .Replace($"@{AccionesDeIrpf.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Crear archivador para el irpf del año en curso");
        }

        private static void AccionAlTransitar(ContextoSe contexto)
        {
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_fiscal);
            //Al cancelar expediente cierro el documento
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_cancelar),
                nombreAccion: AccionesDeIrpf.N_CerrarArchivadorDeIrpf,
                momento: enumMomentoDeEjecucion.A,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeIrpf.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Cierro el archivador del irpf");

            //Al presentar expediente cierro el documento
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_presentar),
                nombreAccion: AccionesDeIrpf.N_CerrarArchivadorDeIrpf,
                momento: enumMomentoDeEjecucion.A,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeIrpf.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Cierro el archivador del irpf");


            //Al corregir expediente reabro el documento
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_irpf_corregir),
                nombreAccion: AccionesDeIrpf.N_AbrirArchivadorDeIrpf,
                momento: enumMomentoDeEjecucion.A,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeIrpf.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Reabro el archivador del irpf");

        }


    }
}
