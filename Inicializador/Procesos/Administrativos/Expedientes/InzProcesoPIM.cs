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
using SistemaDeElementos.Inicializador;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.Terceros;
using GestoresDeNegocio.Terceros;

namespace Inicializador.Expedientes
{
    public static class InzProcesoPIM
    {
        
        #region definición de textos del flujo
        public static readonly string n_pim = "PIM";

        public static readonly string n_estado_pim_abierto = $"{n_pim}: Abierto";
        public static readonly string n_estado_pim_cerrado = $"{n_pim}: Cerrado";
        public static readonly string n_estado_pim_cancelado = $"{n_pim}: Cancelado";


        public static readonly string n_exp_tipo_pim = $"{n_pim}: Impuestos";

        public static readonly string n_tran_pim_cancelar = $"{n_pim}: Cancelar";

        public static readonly string n_tran_pim_cerrar = $"{n_pim}: Cerrar";
        public static readonly string n_tran_pim_reabrir = $"{n_pim}: Reabrir";
        #endregion

      
        private static readonly string parametroDeNegocioPim =
        @"[{""parametro"": ""p_1"",""valor"": @p_1 },{""parametro"": ""p_2"",""valor"": @p_2 }]"
        .Replace("p_1", AccionesDePIM.enumParametros.IdTipoExpediente.ToString())
        .Replace("p_2", AccionesDePIM.enumParametros.IdTipoArchivador.ToString());


        public static readonly string n_rol_pim_gestor = $"Expediente: Gestor de {n_pim}";
        public static readonly string n_rol_pim_consultor = $"Expediente: Consultor de {n_pim}";

        public static void ProcesoPIM(ContextoSe contexto)
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
            rol.Nombre = ModoDeAcceso.SoyGestor(modo) ? n_rol_pim_gestor : n_rol_pim_consultor;
            rol.Descripcion = $"{(ModoDeAcceso.SoyGestor(modo)? "Gestor": "Consultor")} del proceso {n_pim}";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosDePIMDe(contexto, rol, modo);
            return rol;
        }

        public static void AsignarPermisosDePIMDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_pim);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);

            if (ModoDeAcceso.SoyGestor(modo))
            {
                var estado = enumNegocio.Expediente.Estado(contexto,n_estado_pim_abierto);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

                var transicion = enumNegocio.Expediente.Transicion(contexto,n_tran_pim_cerrar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_pim_cancelar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_pim_reabrir);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
            }
        }


        public static void Acciones(ContextoSe contexto)
        {
            contexto.IniciarTraza($"Acciones del proceso {n_pim}");
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
                var pim_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_pim_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.DeCliente, n_exp_tipo_pim, pim_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, 
                    sigla: n_pim, usaPpts: false, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
        
        private static void Transiciones(ContextoSe contexto)
        {
            //abierto --> cerrado, Cancelado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_pim_cerrar, n_estado_pim_abierto, n_estado_pim_cerrado, asunto: "Motivo de cierre");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_pim_cancelar, n_estado_pim_abierto, n_estado_pim_cancelado, asunto: "Motivo de cancelación");

            //Cerrado -->  Abierto
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_pim_reabrir, n_estado_pim_cerrado, n_estado_pim_abierto, asunto: "Motivo de reapertura");
        }
        
        private static void Estados(ContextoSe contexto)
        {
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_pim_abierto, inicial: true, orden: 10);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_pim_cerrado, terminado: true, orden: 15);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_pim_cancelado, cancelado: true, orden: 80);
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

            var etapaEjecucion = iniciales + "," + enCurso;

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, etapaEjecucion);
        }
        private static void CrearAcciones(ContextoSe contexto)
        {
            InzAcciones.CrearAccionesDeArchivadores(GestorDeAcciones.Gestor(contexto, contexto.Mapeador));

            var a = new AccionDtm();
            #region Abrir archivadores de la documentación del PIM
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDePIM)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();

            a.Nombre = AccionesDePIM.N_AbrirArchivadoresDePIM;
            a.Descripcion = $@"Crea los archivadores de impuestos del año en curso y el anterior.{Environment.NewLine}Parámetros: {parametroDeNegocioPim}";
            a.Metodo = nameof(AccionesDePIM.AbrirArchivadoresDePIM);
            a.PersistirAccion(contexto);
            #endregion

            #region Abrir archivadores de la documentación del cliente

            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Terceros)}.{nameof(AccionesDeClientes)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeClientes.N_AbrirArchivadorDeCliente;
            a.Descripcion = $@"Crea un archivador para el cliente.{Environment.NewLine}Usa los valores de los parámetros del negocio de clientes '{enumParametrosDeCliente.CLI_CG_De_Cliente}' y '{enumParametrosDeCliente.CLI_TipoArchivador}'";
            a.Metodo = nameof(AccionesDeClientes.AbrirArchivadorDeCliente);
            a.PersistirAccion(contexto);
            #endregion

        }

        private static void AccionesDeNegocio(ContextoSe contexto)
        {
            var tipoExpediente = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_pim);
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_fiscal);
            GestorDeAccionesDeNegocio.CrearAccionDeNegocio(contexto, 
                negocio:enumNegocio.Expediente,
                nombre: AccionesDePIM.N_AbrirArchivadoresDePIM,
                momento: enumMomentoDeAccion.DC,
                parametro: parametroDeNegocioPim
                           .Replace($"@{AccionesDePIM.enumParametros.IdTipoExpediente}", $"{tipoExpediente.Id}")
                           .Replace($"@{AccionesDePIM.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Crea los archivadores de impuestos del año en curso y el anterior");

            var cg = contexto.Set<CentroGestorDtm>().FirstOrDefault(x => x.Nombre == ltrDeSociedad.CentroGestorDeClientesWeb);

            if (cg == null)
            {
                var sociedades = contexto.Set<SociedadDtm>().Where(x => true);
                foreach(var sociedad in sociedades)
                {
                    if (sociedad.EsGestionada(contexto))
                    {
                        cg = new CentroGestorDtm();
                        cg.Nombre = ltrDeSociedad.CentroGestorDeClientesWeb;
                        cg.Codigo = sociedad.CodigoFiscal + "." + ltrDeSociedad.CodigoCgClienteWeb;
                        cg.eMail = sociedad.eMail;
                        cg.Sigla = ltrDeSociedad.SiglasDelCGDeClienteWeb;
                        cg.IdSociedad = sociedad.Id;
                        cg.IdResponsable = sociedad.IdUsuaCrea;
                        cg.Insertar(contexto);
                        break;
                    }
                }
            }

            tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_cliente, errorSiNoHay: false);
            if (tipoArchivador == null)
            {

                tipoArchivador = InzArchivadoresEcoFin.CrearTipo(contexto, InzArchivadoresEcoFin.n_tipo_cliente, "CLI", 0, visible: false, gestionadoPorElSistema: false);
            }

            GestorDeAccionesDeNegocio.CrearAccionDeNegocio(contexto,
                negocio: enumNegocio.Cliente,
                nombre: AccionesDeClientes.N_AbrirArchivadorDeCliente,
                momento: enumMomentoDeAccion.DC,
                parametro:null,
                orden: 10,
                descripcion: $"Crea un archivador en el CG del cliente para poder meter sus datos");
        }

        private static void AccionAlTransitar(ContextoSe contexto)
        {
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_pim_cancelar),
                nombreAccion: AccionesDeArchivadores.N_DarDeBajaArchivadores,
                momento: enumMomentoDeEjecucion.A,
                parametro: null,
                orden: 10,
                descripcion: $"Cierro los archivadores del proceso");
        }



    }
}
