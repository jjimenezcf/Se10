using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

enum Paso { Publicar, Empaquetar, Copiar, Descomprimir, Desplegar }

// Lo único que distingue a un despliegue de otro: los scripts del servidor que ejecuta
record Destino(string Nombre, string Alcance, string ScriptDescomprimir, string ScriptDesplegar);

class FalloDeDespliegue : Exception
{
    public FalloDeDespliegue(string mensaje) : base(mensaje) { }
}

// Compartido por Z_DespliegueTotal y Z_DesplieguePreproductivo
static class Desplegador
{
    const string Servidor = "flow0.secify.support";
    const string DirectorioDelRepositorio = @"C:\Users\jjimenez\source\repos\Se10";
    const string DirectorioDePublicacion = @"C:\Prepublicacion";
    const string DirectorioRemoto = @"C:\Prepublicacion";
    const string NombreDelZip = "publicacion.zip";
    const string PerfilDePublicacion = "Secify";
    const string Vswhere = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";
    const string MarcaDeSalida = "##SALIDA##";
    const int GigasLibresMinimos = 5;

    static readonly string Proyecto = $@"{DirectorioDelRepositorio}\SistemaDeElementos\SistemaDeElementos.csproj";
    static readonly string ProyectoDelEmpaquetador = $@"{DirectorioDelRepositorio}\Z_EmpaquetarPublicacion\Z_EmpaquetarSitioWeb.csproj";
    static readonly string Empaquetador = $@"{DirectorioDelRepositorio}\Z_EmpaquetarPublicacion\bin\Release\net10.0\win-x86\Z_EmpaquetarSitioWeb.exe";
    static readonly string ZipLocal = $@"{DirectorioDePublicacion}\{NombreDelZip}";
    static readonly string DirectorioCompartido = $@"\\{Servidor}\c$\{DirectorioRemoto.Substring(3)}";

    static readonly object Cerrojo = new();
    static StreamWriter _log;
    static Destino _destino;

    public static int Ejecutar(string[] args, Destino destino)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _destino = destino;

        int codigo = Desplegar(args);

        // Si se lanza con doble clic, que la ventana no se cierre sin ver el resultado
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("Presiona una tecla para salir...");
            Console.ReadKey();
        }
        return codigo;
    }

    static int Desplegar(string[] args)
    {
        bool soloComprobar = false, soloVerificar = false, sinConfirmar = false;
        Paso desde = Paso.Publicar, hasta = Paso.Desplegar;

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--comprobar": soloComprobar = true; break;
                    case "--verificar": soloVerificar = true; break;
                    case "--si": sinConfirmar = true; break;
                    case "--desde": desde = LeerPaso(args, ++i); break;
                    case "--hasta": hasta = LeerPaso(args, ++i); break;
                    default: throw new ArgumentException($"Opción desconocida: {args[i]}");
                }
            }
            if (desde > hasta)
                throw new ArgumentException($"--desde {desde} es posterior a --hasta {hasta}");
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            MostrarAyuda();
            return 2;
        }

        string directorioDeLogs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Z_Despliegue");
        Directory.CreateDirectory(directorioDeLogs);
        string rutaDelLog = Path.Combine(directorioDeLogs, $"{_destino.Nombre}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
        _log = new StreamWriter(rutaDelLog) { AutoFlush = true };

        Paso? pasoEnCurso = null;
        bool desplegado = false;
        try
        {
            if (soloVerificar)
            {
                desplegado = true;
                Verificar();
                Escribir("Verificación correcta.");
                return 0;
            }

            Comprobar(desde, hasta);
            if (soloComprobar)
            {
                Escribir("Comprobaciones correctas. No se ha modificado nada.");
                return 0;
            }

            if (hasta >= Paso.Descomprimir && !sinConfirmar && !Confirmar())
            {
                Escribir("Cancelado. No se ha modificado nada.");
                return 3;
            }

            Stopwatch total = Stopwatch.StartNew();
            for (Paso paso = desde; paso <= hasta; paso++)
            {
                pasoEnCurso = paso;
                Titulo($"{(int)paso + 1}/5 {paso}");
                Stopwatch cronometro = Stopwatch.StartNew();
                EjecutarPaso(paso);
                Escribir($"{paso} correcto en {cronometro.Elapsed:mm\\:ss}");
            }
            pasoEnCurso = null;

            if (hasta == Paso.Desplegar)
            {
                desplegado = true;
                Titulo("Verificación");
                Verificar();
            }

            Titulo($"{_destino.Nombre} CORRECTO en {total.Elapsed:hh\\:mm\\:ss}");
            Escribir($"Log: {rutaDelLog}");
            return 0;
        }
        catch (Exception e) when (e is FalloDeDespliegue or IOException or UnauthorizedAccessException)
        {
            Titulo($"{_destino.Nombre} FALLIDO");
            Escribir(e.Message);
            Escribir(ConsejoTrasFallo(pasoEnCurso, desplegado));
            Escribir($"Log: {rutaDelLog}");
            return 1;
        }
    }

    static Paso LeerPaso(string[] args, int indice)
    {
        if (indice >= args.Length || !Enum.TryParse(args[indice], ignoreCase: true, out Paso paso) || !Enum.IsDefined(paso))
            throw new ArgumentException($"Paso no válido. Valores: {string.Join(", ", Enum.GetNames<Paso>())}");
        return paso;
    }

    static void MostrarAyuda()
    {
        Console.WriteLine($"""

            {_destino.Nombre}: publica SistemaDeElementos en {Servidor} ({_destino.Alcance}).

              Sin opciones      Ejecuta todos los pasos; pide confirmación antes de tocar el servidor
              --comprobar       Solo comprueba los requisitos, sin modificar nada
              --verificar       Solo verifica lo desplegado en el servidor, sin modificar nada
              --desde <paso>    Empieza en ese paso (para reanudar tras un fallo)
              --hasta <paso>    Termina en ese paso
              --si              No pide confirmación

            Pasos: {string.Join(", ", Enum.GetNames<Paso>())}
            """);
    }

    static string ConsejoTrasFallo(Paso? paso, bool desplegado)
    {
        if (desplegado)
            return "ATENCIÓN: lo desplegado en el servidor no es correcto. Revisa los sitios indicados.";

        switch (paso)
        {
            case Paso.Descomprimir:
                return "ATENCIÓN: las colas pueden haber quedado bloqueadas y los sitios parados. Revisa la salida antes de reintentar.";
            case Paso.Desplegar:
                return "ATENCIÓN: los sitios pueden seguir parados. Corrige la causa y reanuda con: --desde Desplegar";
            default:
                return "Los sitios del servidor no se han tocado: siguen funcionando con la versión anterior.";
        }
    }

    static bool Confirmar()
    {
        if (Console.IsInputRedirected)
            throw new FalloDeDespliegue("No hay consola para confirmar. Usa --si para desplegar sin confirmación.");

        Console.WriteLine();
        Console.WriteLine($"Esto parará y actualizará {_destino.Alcance} en {Servidor}.");
        Console.Write("Escribe PUBLICAR para continuar: ");
        return Console.ReadLine()?.Trim() == "PUBLICAR";
    }

    static void EjecutarPaso(Paso paso)
    {
        switch (paso)
        {
            case Paso.Publicar: Publicar(); break;
            case Paso.Empaquetar: Empaquetar(); break;
            case Paso.Copiar: Copiar(); break;
            case Paso.Descomprimir: Descomprimir(); break;
            case Paso.Desplegar: EjecutarScriptRemoto(_destino.ScriptDesplegar); break;
        }
    }

    static void Comprobar(Paso desde, Paso hasta)
    {
        Titulo($"Comprobaciones de {_destino.Nombre}");

        var (rama, cambios) = EstadoDelRepositorio();
        Escribir($"Repositorio: {rama}, {cambios} fichero(s) sin confirmar{(cambios > 0 ? " (SE PUBLICAN TAL COMO ESTÁN)" : "")}");

        if (desde <= Paso.Publicar && hasta >= Paso.Publicar)
            Escribir($"MSBuild: {BuscarMsBuild()}");
        if (desde == Paso.Copiar && !File.Exists(ZipLocal))
            throw new FalloDeDespliegue($"No existe el zip: {ZipLocal}");

        if (hasta >= Paso.Copiar)
        {
            if (!Directory.Exists(DirectorioCompartido))
                throw new FalloDeDespliegue($"No se puede acceder a {DirectorioCompartido}");
            Escribir($"Acceso a {DirectorioCompartido}: correcto");

            // Todos los despliegues comparten el zip y la carpeta Se, así que cualquiera en curso impide lanzar otro
            int fallos = EjecutarRemoto($$"""
                $fallos = 0
                $principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
                if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { 'FALLO: el usuario no es administrador del servidor'; $fallos++ }
                foreach ($script in '{{_destino.ScriptDescomprimir}}', '{{_destino.ScriptDesplegar}}') {
                    if (-not (Test-Path -LiteralPath "{{DirectorioRemoto}}\$script")) { "FALLO: no existe $script"; $fallos++ }
                }
                $enCurso = Get-CimInstance Win32_Process -Filter "Name='cmd.exe'" | Where-Object { $_.CommandLine -match '0[12]-(Descomprimir|Desplegar)|20 - Preproductivo' }
                if ($enCurso) { 'FALLO: hay un despliegue en curso o una ventana de sus scripts abierta en el servidor'; $fallos++ }
                $libres = (Get-PSDrive C).Free / 1GB
                'Disco libre en el servidor: {0:N1} GB' -f $libres
                if ($libres -lt {{GigasLibresMinimos}}) { 'FALLO: poco espacio en disco'; $fallos++ }
                "{{MarcaDeSalida}}$fallos"
                """);
            if (fallos != 0)
                throw new FalloDeDespliegue("El servidor no cumple las comprobaciones.");
        }
    }

    static (string rama, int cambios) EstadoDelRepositorio()
    {
        var rama = new List<string>();
        var cambios = new List<string>();
        Lanzar("git", ["-C", DirectorioDelRepositorio, "rev-parse", "--abbrev-ref", "HEAD"], DirectorioDelRepositorio, rama.Add);
        Lanzar("git", ["-C", DirectorioDelRepositorio, "status", "--porcelain"], DirectorioDelRepositorio, cambios.Add);
        return (rama.FirstOrDefault() ?? "?", cambios.Count);
    }

    static string BuscarMsBuild()
    {
        // No vale dotnet publish: la compilación de TypeScript necesitaría node.exe, que Visual Studio aporta por su cuenta
        var rutas = new List<string>();
        if (File.Exists(Vswhere))
            Lanzar(Vswhere, ["-latest", "-requires", "Microsoft.Component.MSBuild", "-find", @"MSBuild\**\Bin\MSBuild.exe"], DirectorioDelRepositorio, rutas.Add);
        return rutas.FirstOrDefault(File.Exists) ?? throw new FalloDeDespliegue("No se encuentra el MSBuild de Visual Studio.");
    }

    static void Publicar()
    {
        int codigo = Lanzar(BuscarMsBuild(),
            [Proyecto, "/restore", "/p:Configuration=Release", "/p:DeployOnBuild=true", $"/p:PublishProfile={PerfilDePublicacion}", "/v:minimal", "/nologo"],
            DirectorioDelRepositorio, Escribir);
        if (codigo != 0)
            throw new FalloDeDespliegue($"La publicación ha fallado (código {codigo}).");
        if (!File.Exists($@"{DirectorioDePublicacion}\SistemaDeElementos.dll"))
            throw new FalloDeDespliegue($"La publicación no ha dejado SistemaDeElementos.dll en {DirectorioDePublicacion}.");
    }

    static void Empaquetar()
    {
        // Se compila en vez de usar el exe de SistemaDeElementos\Publicacion, que es para el paso a paso manual y espera una tecla
        int codigo = Lanzar("dotnet", ["build", ProyectoDelEmpaquetador, "-c", "Release", "-r", "win-x86", "--no-self-contained", "-v:quiet", "-nologo"],
            DirectorioDelRepositorio, Escribir);
        if (codigo != 0 || !File.Exists(Empaquetador))
            throw new FalloDeDespliegue($"No se ha podido compilar el empaquetador (código {codigo}).");

        DateTime inicio = DateTime.Now;
        int errores = 0;
        codigo = Lanzar(Empaquetador, [], Path.GetDirectoryName(Empaquetador), linea =>
        {
            Escribir(linea);
            if (linea.StartsWith("ERROR"))
                errores++;
        });

        if (codigo != 0 || errores > 0)
            throw new FalloDeDespliegue($"El empaquetado ha fallado (código {codigo}, {errores} error(es)).");
        if (!File.Exists(ZipLocal) || File.GetLastWriteTime(ZipLocal) < inicio)
            throw new FalloDeDespliegue($"El empaquetador no ha generado {ZipLocal}.");

        using ZipArchive zip = ZipFile.OpenRead(ZipLocal);
        if (zip.GetEntry("SistemaDeElementos.dll") == null)
            throw new FalloDeDespliegue("El zip no contiene SistemaDeElementos.dll.");
        Escribir($"Zip: {zip.Entries.Count} ficheros, {new FileInfo(ZipLocal).Length / 1024 / 1024} MB");
    }

    static void Copiar()
    {
        string destino = $@"{DirectorioCompartido}\{NombreDelZip}";
        string temporal = destino + ".subiendo";

        Escribir($"Copiando a {destino}...");
        File.Copy(ZipLocal, temporal, overwrite: true);

        string hashLocal;
        using (FileStream zip = File.OpenRead(ZipLocal))
            hashLocal = Convert.ToHexString(SHA256.HashData(zip));

        var salida = new List<string>();
        EjecutarRemoto($$"""
            (Get-FileHash -LiteralPath '{{DirectorioRemoto}}\{{NombreDelZip}}.subiendo' -Algorithm SHA256).Hash
            "{{MarcaDeSalida}}0"
            """, salida.Add);

        if (!string.Equals(salida.FirstOrDefault()?.Trim(), hashLocal, StringComparison.OrdinalIgnoreCase))
            throw new FalloDeDespliegue("El zip copiado no coincide con el local (SHA256 distinto).");

        File.Move(temporal, destino, overwrite: true);
        Escribir($"Copia verificada, SHA256 {hashLocal}");
    }

    static void Descomprimir()
    {
        EjecutarScriptRemoto(_destino.ScriptDescomprimir);
        if (!File.Exists($@"{DirectorioCompartido}\Se\SistemaDeElementos.dll"))
            throw new FalloDeDespliegue(@"Tras descomprimir no existe Se\SistemaDeElementos.dll en el servidor.");
    }

    static void EjecutarScriptRemoto(string script)
    {
        // Con la entrada en NUL los pause del script no se quedan esperando
        int codigo = EjecutarRemoto($$"""
            Set-Location '{{DirectorioRemoto}}'
            cmd.exe /d /c 'call "{{DirectorioRemoto}}\{{script}}" < NUL' 2>&1 | ForEach-Object { "$_" }
            "{{MarcaDeSalida}}$LASTEXITCODE"
            """);
        if (codigo != 0)
            throw new FalloDeDespliegue($"'{script}' ha terminado con código {codigo}.");
    }

    static void Verificar()
    {
        // Los sitios y directorios se leen del script de despliegue para no mantener aquí otra lista de clientes
        int fallos = EjecutarRemoto($$"""
            $fallos = 0
            $origen = '{{DirectorioRemoto}}\Se'
            $script = [IO.File]::ReadAllText('{{DirectorioRemoto}}\{{_destino.ScriptDesplegar}}')
            $directorios = [regex]::Matches($script, '(?im)^set DIR\d+=(.+?)\s*$') | ForEach-Object { $_.Groups[1].Value }
            $sitios = [regex]::Matches($script, '(?i)Start-WebSite\s+([^\s"]+)') | ForEach-Object { $_.Groups[1].Value }
            $grupos = [regex]::Matches($script, '(?i)Start-WebAppPool\s+-Name\s+([^\s"]+)') | ForEach-Object { $_.Groups[1].Value }
            if (-not $directorios -or -not $sitios -or -not $grupos) { 'FALLO: no se han podido leer los sitios del script de despliegue'; $fallos++ }

            $ficheros = @(Get-ChildItem -LiteralPath $origen -Recurse -File)
            if ($ficheros.Count -eq 0) { "FALLO: $origen está vacío"; $fallos++ }
            foreach ($directorio in $directorios) {
                $distintos = 0; $ejemplo = $null
                foreach ($fichero in $ficheros) {
                    $copia = [IO.FileInfo](Join-Path $directorio $fichero.FullName.Substring($origen.Length + 1))
                    if (-not $copia.Exists -or $copia.Length -ne $fichero.Length -or $copia.LastWriteTimeUtc -ne $fichero.LastWriteTimeUtc) {
                        $distintos++
                        if (-not $ejemplo) { $ejemplo = $copia.FullName }
                    }
                }
                if ($distintos -gt 0) { "FALLO: $directorio tiene $distintos fichero(s) sin actualizar, por ejemplo $ejemplo"; $fallos++ }
                else { "OK $directorio ($($ficheros.Count) ficheros)" }
            }

            Import-Module WebAdministration
            foreach ($sitio in $sitios) {
                $estado = (Get-WebsiteState -Name $sitio).Value
                if ($estado -ne 'Started') { "FALLO: el sitio $sitio está $estado"; $fallos++ }
            }
            foreach ($grupo in $grupos) {
                $estado = (Get-WebAppPoolState -Name $grupo).Value
                if ($estado -ne 'Started') { "FALLO: el grupo de aplicaciones $grupo está $estado"; $fallos++ }
            }
            "OK estados revisados: $(@($sitios).Count) sitios y $(@($grupos).Count) grupos de aplicaciones"
            "{{MarcaDeSalida}}$fallos"
            """);
        if (fallos != 0)
            throw new FalloDeDespliegue($"La verificación ha detectado {fallos} problema(s).");
    }

    // El bloque debe terminar escribiendo MarcaDeSalida seguida de un número: si no llega, la conexión o el bloque han fallado
    static int EjecutarRemoto(string bloque, Action<string> alEscribir = null)
    {
        string comando = $$"""
            $ErrorActionPreference = 'Stop'
            $ProgressPreference = 'SilentlyContinue'
            [Console]::OutputEncoding = [Text.Encoding]::UTF8
            Invoke-Command -ComputerName {{Servidor}} -ScriptBlock {
            {{bloque}}
            }
            """;

        int? resultado = null;
        // Con -EncodedCommand PowerShell escribe los errores en CLIXML; así se evitan el problema de las comillas y ese formato
        string base64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(comando));
        Lanzar("powershell.exe",
            ["-NoProfile", "-NonInteractive", "-Command", $"& ([scriptblock]::Create([Text.Encoding]::Unicode.GetString([Convert]::FromBase64String('{base64}'))))"],
            DirectorioDelRepositorio,
            linea =>
            {
                if (linea.StartsWith(MarcaDeSalida))
                    resultado = int.TryParse(linea.AsSpan(MarcaDeSalida.Length), out int valor) ? valor : -1;
                else
                    (alEscribir ?? Escribir)(linea);
            },
            Encoding.UTF8);

        return resultado ?? throw new FalloDeDespliegue($"Se ha perdido la ejecución remota en {Servidor} antes de terminar.");
    }

    static int Lanzar(string programa, string[] argumentos, string directorio, Action<string> alEscribir, Encoding codificacion = null)
    {
        codificacion ??= Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        var inicio = new ProcessStartInfo(programa)
        {
            WorkingDirectory = directorio,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = codificacion,
            StandardErrorEncoding = codificacion,
        };
        foreach (string argumento in argumentos)
            inicio.ArgumentList.Add(argumento);

        using Process proceso = Process.Start(inicio);
        proceso.StandardInput.Close();

        DataReceivedEventHandler alRecibir = (_, e) =>
        {
            if (e.Data == null)
                return;
            lock (Cerrojo)
                alEscribir(e.Data);
        };
        proceso.OutputDataReceived += alRecibir;
        proceso.ErrorDataReceived += alRecibir;
        proceso.BeginOutputReadLine();
        proceso.BeginErrorReadLine();
        proceso.WaitForExit();
        return proceso.ExitCode;
    }

    static void Titulo(string texto)
    {
        Escribir("");
        Escribir($"=== {texto} ===");
    }

    static void Escribir(string linea)
    {
        Console.WriteLine(linea);
        _log.WriteLine($"{DateTime.Now:HH:mm:ss} {linea}");
    }
}
