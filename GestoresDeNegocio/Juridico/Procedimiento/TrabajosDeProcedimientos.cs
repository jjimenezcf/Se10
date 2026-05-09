using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Utilidades;

namespace GestoresDeNegocio.Juridico.Procedimiento
{
    public enum enumTrabajosDeProcedimientos
    {
        [Description("Envía los archivos al procedimiento en curso")]
        LexnetEnviarArchivos,
        [Description("Actualiza catálogos judiciales")]
        LexnetActualizarCatalogos
    }

    public class TrabajosDeProcedimientos
    {
        public static TrabajoDeUsuarioDtm SometerLexnetEnviarArchivos(ContextoSe contexto, int idArchivador)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeProcedimientos).FullName;
            var parametrosEntrada = new Dictionary<string, object> { { nameof(ArchivadorDtm.Id), idArchivador } };
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeProcedimientos.LexnetEnviarArchivos.Descripcion(), dll, clase, nameof(enumTrabajosDeProcedimientos.LexnetEnviarArchivos), comunicarFin: true);
            var datosDeCreacion = new Dictionary<string, object>
            {
               { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerLexnetActualizarCatalogos(ContextoSe contexto, int idSociedad)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeProcedimientos).FullName;
            var parametrosEntrada = new Dictionary<string, object> { { nameof(SociedadDtm.Id), idSociedad } };
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeProcedimientos.LexnetActualizarCatalogos.Descripcion(), dll, clase, nameof(enumTrabajosDeProcedimientos.LexnetActualizarCatalogos), comunicarFin: true);
            var datosDeCreacion = new Dictionary<string, object>
            {
               { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void LexnetEnviarArchivos(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(LexnetEnviarArchivos));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
                Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
                var idExpediente = (int)parametros.LeerValor<long>(nameof(ExpedienteDtm.Id));
                var idArchivador = (int)parametros.LeerValor<long>(nameof(ArchivadorDtm.Id));
                var expediente = contexto.SeleccionarPorId<ExpedienteDtm>(idExpediente);
                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
                var tran = contexto.IniciarTransaccion();
                try
                {
                    var login = new Lexnet.LexNetLogin(contexto, expediente.Sociedad(contexto));
                    try
                    {
                        Task.Run(() => login.Conectar()).Wait();
                        var archivos = archivador.LeerAnexados(contexto);
                        // Aquí iría la lógica para enviar los archivos a Lexnet
                        entorno.CrearTraza($"archivos del procedimiento con NIG '...' enviados");
                        contexto.Commit(tran);
                    }
                    finally
                    {
                        login.Desconectar();
                    }
                    entorno.CrearTraza($"Fin del proceso realizado");
                }
                catch (Exception ex)
                {
                    entorno.AnotarError($"No se ha podido enviar el/los archivos del procedimiento con NIG '...'", ex);
                    contexto.Rollback(tran);
                    throw;
                }
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static void LexnetActualizarCatalogos(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(LexnetActualizarCatalogos));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
                Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
                var idSociedad = (int)parametros.LeerValor<long>(nameof(SociedadDtm.Id));
                var tran = contexto.IniciarTransaccion();
                try
                {
                    var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
                    var login = new Lexnet.LexNetLogin(contexto, sociedad);
                    try
                    {
                        Task.Run(() => login.Conectar()).Wait();
                        //var catalogos = new Lexnet.LexNetCatalogos(login.IdSesion);
                        //GuardarCatalogos(catalogos);
                    }
                    finally
                    {
                        login.Desconectar();
                    }
                    entorno.CrearTraza($"archivos del procedimiento con NIG '...' enviados");
                    entorno.CrearTraza($"Fin del proceso realizado");
                    contexto.Commit(tran);
                }
                catch (Exception ex)
                {
                    entorno.AnotarError($"No se han podido actualizar los catálogos de LexNET", ex);
                    contexto.Rollback(tran);
                    throw;
                }
            }
            finally
            {
                contexto.CerrarTraza();
            }

        }

        /*
         * public static void EnvioParaLexnet(EntornoDeTrabajo entorno)
{
    var contexto = entorno.contextoDelProceso;
    contexto.IniciarTraza(nameof(EnvioParaLexnet));
    try
    {
        entorno.CrearTraza("Inicio del proceso de envío a LexNET");
        Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
        var idArchivador = (int)parametros.LeerValor<long>(nameof(ArchivadorDtm.Id));
        
        // 1. Obtener el archivador y sus documentos
        var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
        var archivos = archivador.LeerAnexados(contexto);

        foreach (var archivo in archivos)
        {
            // 2. Obtener la ruta física del archivo en tu sistema documental
            string rutaFisica = archivo.ObtenerRutaFisica(); // Ajusta según tu lógica de almacenamiento

            // 3. Validaciones obligatorias de LexNET
            entorno.CrearTraza($"Validando archivo: {archivo.Nombre}");
            
            if (!EsPdfA(rutaFisica))
            {
                throw new Exception($"El archivo {archivo.Nombre} no cumple el estándar PDF/A requerido por LexNET.");
            }

            if (!HasOCR(rutaFisica))
            {
                throw new Exception($"El archivo {archivo.Nombre} no tiene capa de texto (OCR) detectable.");
            }
        }

        var tran = contexto.IniciarTransaccion();
        try
        {
            // 4. Lógica de Comunicación con LexNET
            // Aquí llamarías a tu cliente de Web Service de LexNET
            // Ejemplo hipotético de empaquetado:
            var clienteLexnet = new ClienteLexnetWS(); // Clase que deberás implementar
            
            entorno.CrearTraza("Generando paquete firmado y enviando a la plataforma...");
            
            // Suponiendo que el archivador tiene los datos del procedimiento (NIG, etc.)
            bool exito = clienteLexnet.EnviarDocumentacion(
                nig: archivador.NIG, 
                documentos: archivos, 
                certificado: entorno.CertificadoDigital // El que use el despacho
            );

            if (exito)
            {
                entorno.CrearTraza($"Archivos del procedimiento con NIG '{archivador.NIG}' enviados correctamente.");
                contexto.Commit(tran);
            }
            else
            {
                throw new Exception("LexNET rechazó el paquete. Revise los logs de la plataforma.");
            }
        }
        catch (Exception ex)
        {
            entorno.AnotarError($"Fallo en la transmisión a LexNET del NIG '{archivador.NIG}'", ex);
            contexto.Rollback(tran);
        }
    }
    finally
    {
        entorno.CrearTraza($"Fin del proceso realizado");
        contexto.CerrarTraza();
    }
}
        */
    }
}
