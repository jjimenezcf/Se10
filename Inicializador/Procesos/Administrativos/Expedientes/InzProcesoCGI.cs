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
    public static class InzProcesoCGI
    {
        
        #region definición de textos del flujo
        public static readonly string n_cgi = "CGI";

        public static readonly string n_estado_cgi_abierto = $"{n_cgi}: Abierto";
        public static readonly string n_estado_cgi_cerrado = $"{n_cgi}: Cerrado";
        public static readonly string n_estado_cgi_cancelado = $"{n_cgi}: Cancelado";


        public static readonly string n_exp_tipo_expediente_cgi = $"{n_cgi}: Contabilización";

        public static readonly string n_tran_cgi_cancelar = $"{n_cgi}: Cancelar";

        public static readonly string n_tran_cgi_cerrar = $"{n_cgi}: Cerrar";
        public static readonly string n_tran_cgi_reabrir = $"{n_cgi}: Reabrir";
        #endregion

        private static readonly string parametroDeTransicion = @"[{""parametro"": ""p_2"",""valor"": @p_2 }]"
        .Replace("p_2", AccionesDeCGI.enumParametros.IdTipoArchivador.ToString());
       
        private static readonly string parametroDeNegocio =
        @"[{""parametro"": ""p_1"",""valor"": @p_1 },{""parametro"": ""p_2"",""valor"": @p_2 }]"
        .Replace("p_1", AccionesDeCGI.enumParametros.IdTipoExpediente.ToString())
        .Replace("p_2", AccionesDeCGI.enumParametros.IdTipoArchivador.ToString());


        public static readonly string n_rol_cgi_gestor = $"Expediente: Gestor de {n_cgi}";
        public static readonly string n_rol_cgi_consultor = $"Expediente: Consultor de {n_cgi}";

        public static void ProcesoCGI(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
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

        internal static RolDtm CrearRol(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            var rol = new RolDtm();
            rol.Nombre = ModoDeAcceso.SoyGestor(modo) ? n_rol_cgi_gestor : n_rol_cgi_consultor;
            rol.Descripcion = $"{(ModoDeAcceso.SoyGestor(modo)? "Gestor": "Consultor")} del proceso {n_cgi}";

            rol = rol.PersistirPorNombre(contexto, errorSiYaExiste: false);
            AsignarPermisosDeCGIDe(contexto, rol, modo);
            return rol;
        }

        public static void AsignarPermisosDeCGIDe(ContextoSe contexto, RolDtm rol, enumModoDeAccesoDeDatos modo)
        {
            var tipo = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_expediente_cgi);
            contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo) ? tipo.IdPermisoDeGestor : tipo.IdPermisoDeConsultor);

            if (ModoDeAcceso.SoyGestor(modo))
            {
                var estado = enumNegocio.Expediente.Estado(contexto,n_estado_cgi_abierto);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, estado.IdPermiso);

                var transicion = enumNegocio.Expediente.Transicion(contexto,n_tran_cgi_cerrar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_cgi_cancelar);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
                transicion = enumNegocio.Expediente.Transicion(contexto, n_tran_cgi_reabrir);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, transicion.IdPermiso);
            }
        }


        public static void Acciones(ContextoSe contexto)
        {
            contexto.IniciarTraza($"Acciones del proceso {n_cgi}");
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
                var cgi_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_cgi_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.DeCliente, n_exp_tipo_expediente_cgi, cgi_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, sigla: n_cgi, usaPpts: false, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
        
        private static void Transiciones(ContextoSe contexto)
        {
            //abierto --> cerrado, Cancelado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_cgi_cerrar, n_estado_cgi_abierto, n_estado_cgi_cerrado);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_cgi_cancelar, n_estado_cgi_abierto, n_estado_cgi_cancelado, asunto: "Motivo de cancelación");

            //Cerrado -->  Abierto
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_cgi_reabrir, n_estado_cgi_cerrado, n_estado_cgi_abierto, asunto: "Motivo de reapertura");
        }
        
        private static void Estados(ContextoSe contexto)
        {
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_cgi_abierto, inicial: true, orden: 10);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_cgi_cerrado, terminado: true, orden: 15);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_cgi_cancelado, cancelado: true, orden: 80);
        }

        private static void CrearAcciones(ContextoSe contexto)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDeCGI)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();

            #region Cerrar archivadores de la documentacion del CGI
            a.Nombre = AccionesDeCGI.N_CerrarArchivadoresDeCGI;
            a.Descripcion = @$"Cierra los archivadores de gastos, ingresos y contabilizados del CGI.{Environment.NewLine}Parámetros: {parametroDeTransicion}";
            a.Metodo = nameof(AccionesDeCGI.CerrarArchivadoresDeCGI);
            a.PersistirAccion(contexto);
            #endregion


            #region Abrir archivadores de la documentación del CGI
            a.Nombre = AccionesDeCGI.N_AbrirArchivadoresDeCGI;
            a.Descripcion = $@"Crea o activar los archivadores de gasto, ingresos y contabilizados.{Environment.NewLine}Parámetros: {parametroDeNegocio}";
            a.Metodo = nameof(AccionesDeCGI.AbrirArchivadoresDeCGI);
            a.PersistirAccion(contexto);
            #endregion

        }

        private static void AccionesDeNegocio(ContextoSe contexto)
        {
            var tipoExpediente = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_expediente_cgi);
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_contable);
            GestorDeAccionesDeNegocio.CrearAccionDeNegocio(contexto, 
                negocio:enumNegocio.Expediente,
                nombre: AccionesDeCGI.N_AbrirArchivadoresDeCGI,
                momento: enumMomentoDeAccion.DC,
                parametro: parametroDeNegocio
                           .Replace($"@{AccionesDeCGI.enumParametros.IdTipoExpediente}", $"{tipoExpediente.Id}")
                           .Replace($"@{AccionesDeCGI.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Crear archivadores para gastos, ingresos y contabilizados del año en curso");
        }

        private static void AccionAlTransitar(ContextoSe contexto)
        {
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_contable);
            //Al cancelar expediente cierro los archivos
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_cgi_cancelar),
                nombreAccion: AccionesDeCGI.N_CerrarArchivadoresDeCGI,
                momento: enumMomentoDeEjecucion.A,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeCGI.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Se dan de baja los archivadores de gasto, ingresos y contables");

            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_cgi_cerrar),
                nombreAccion: AccionesDeCGI.N_CerrarArchivadoresDeCGI,
                momento: enumMomentoDeEjecucion.A,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeCGI.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Bloqueo los archivadores de gasto, ingresos y contables");

            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, n_tran_cgi_reabrir),
                nombreAccion: AccionesDeCGI.N_AbrirArchivadoresDeCGI,
                momento: enumMomentoDeEjecucion.D,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeCGI.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString()),
                orden: 10,
                descripcion: $"Desbloqueo los archivadores de gasto, ingresos y contables");
        }


    }
}
