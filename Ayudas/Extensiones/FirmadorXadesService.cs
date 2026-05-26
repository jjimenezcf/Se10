using System;
using System.Diagnostics;
using System.IO;

public class FirmadorXadesService
{
    /// <summary>
    /// Comprueba preventivamente si el entorno cumple los requisitos mínimos para poder firmar.
    /// Si falta algo, lanza una excepción inmediata que burbujea para ser capturada en el flujo principal.
    /// </summary>
    public static void ValidarQueSePuedeFirmarConXade()
    {
        ValidarDirectorios();

        try
        {
            ValidarMaquinaVirtualDeJava();
        }
        catch (Exception ex)
        {
            throw new Exception("Error preventivo: No se pudo verificar la instalación de Java en el servidor.", ex);
        }
    }

    /// <summary>
    /// Comprueba de forma silenciosa (sin lanzar excepciones) si el entorno cumple los requisitos para usar el JAR.
    /// </summary>
    public static bool PuedeFirmarConJar()
    {
        try
        {
            ValidarDirectorios();
            ValidarMaquinaVirtualDeJava();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ejecuta la firma XAdES-BES en formato Enveloped utilizando el componente unificado de Java (DSS).
    /// </summary>
    public static bool Firmar(string xmlOrigen, string xmlDestino, string rutaCertificado, string passwordCertificado)
    {
        string rutaBaseJar = ObtenerRutaBaseJar();
        string rutaJar = Path.Combine(rutaBaseJar, "FirmaDss.jar");

        if (!Directory.Exists(rutaBaseJar))
        {
            throw new Exception($"Error de configuración: No se encontró el directorio del firmador en: {rutaBaseJar}");
        }

        if (!File.Exists(rutaJar))
        {
            throw new Exception($"Error de configuración: No se encontró el archivo 'FirmaDss.jar' en: {rutaJar}");
        }

        // Configuración de argumentos usando el Classpath para cargar el JAR y la carpeta externa 'lib/*'
        string classPath = "\"FirmaDss.jar;lib/*\"";
        string argumentos = $"-cp {classPath} Main \"{xmlOrigen}\" \"{xmlDestino}\" \"{rutaCertificado}\" \"{passwordCertificado}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ObtenerRutaJava(rutaBaseJar),
            Arguments = argumentos,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true, // Redirigimos para capturar fallos de inicialización de la JVM
            WorkingDirectory = rutaBaseJar
        };

        try
        {
            using (Process proceso = Process.Start(startInfo))
            {
                if (proceso == null)
                {
                    throw new Exception("No se pudo iniciar el proceso del binario 'java'.");
                }

                // Leemos el canal de error en frío por si el subproceso falla antes de procesar el código Java
                string errorConsolaJava = proceso.StandardError.ReadToEnd();

                proceso.WaitForExit();

                if (proceso.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    // Intentamos recuperar el log de la ejecución criptográfica de DSS
                    string rutaLog = Path.ChangeExtension(xmlOrigen, ".log");
                    string detallesError = string.Empty;

                    if (File.Exists(rutaLog))
                    {
                        detallesError = $"Detalles del archivo log:\n{File.ReadAllText(rutaLog)}";
                    }
                    else
                    {
                        // Si el log no se generó, exponemos la salida nativa de error de la consola de Java
                        detallesError = $"Java no pudo iniciar el código. Salida de la consola de errores de Java:\n{errorConsolaJava}";
                    }

                    throw new Exception($"El motor de firma Java devolvió el código de error {proceso.ExitCode}.\n{detallesError}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error crítico inesperado lanzando el motor de firma Java: {ex.Message}", ex);
        }
    }
    
    // NUEVO método — localiza java.exe (JRE compartido o fallback al PATH)
    private static string ObtenerRutaJava(string rutaBaseJar)
    {
        // Desde jar\xades\ subimos dos niveles hasta lib\ y buscamos el JRE compartido
        string rutaLib = Path.GetFullPath(Path.Combine(rutaBaseJar, "..", ".."));
        string javaLocal = Path.Combine(rutaLib, "jre", "bin", "java.exe");
        if (File.Exists(javaLocal))
            return javaLocal;

        // Fallback: java del PATH del sistema
        return "java";
    }

    /// <summary>
    /// Centraliza el cálculo de la ubicación de la carpeta del .jar dependiendo del entorno (Desarrollo vs Producción)
    /// </summary>
    private static string ObtenerRutaBaseJar()
    {
        string rutaEjecucion = AppContext.BaseDirectory;
        string rutaProduccion = Path.Combine(rutaEjecucion, "wwwroot", "lib", "jar", "xades");

        if (Directory.Exists(rutaProduccion))
        {
            return rutaProduccion;
        }
        else
        {
            DirectoryInfo directorioActual = new DirectoryInfo(rutaEjecucion);
            // Subimos 4 niveles para saltar desde la carpeta bin del proyecto de utilidades a la raíz común
            string raizSolucion = directorioActual.Parent?.Parent?.Parent?.Parent?.FullName ?? rutaEjecucion;
            return Path.Combine(raizSolucion, "SistemaDeElementos", "wwwroot", "lib", "jar", "xades");
        }
    }

    private static void ValidarMaquinaVirtualDeJava()
    {
        string rutaJava = ObtenerRutaJava(ObtenerRutaBaseJar());

        ProcessStartInfo checkJava = new ProcessStartInfo
        {
            FileName = rutaJava,
            Arguments = "-version",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };

        using (Process proc = Process.Start(checkJava))
        {
            proc?.WaitForExit(3000);
            if (proc == null || proc.ExitCode != 0)
            {
                throw new Exception($"La máquina virtual de Java no responde o no está instalada. Ruta probada: {rutaJava}");
            }
        }
    }

    private static void ValidarDirectorios()
    {
        string rutaBaseJar = ObtenerRutaBaseJar();
        if (!Directory.Exists(rutaBaseJar))
        {
            throw new Exception($"No se encontró el directorio del firmador en: {rutaBaseJar}");
        }

        string rutaJar = Path.Combine(rutaBaseJar, "FirmaDss.jar");
        if (!File.Exists(rutaJar))
        {
            throw new Exception($"No se encontró el archivo 'FirmaDss.jar' en: {rutaJar}");
        }
    }
}