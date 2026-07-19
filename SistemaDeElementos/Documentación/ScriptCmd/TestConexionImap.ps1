# =============================================================================
# TestConexionImap.ps1
# Diagnóstico de conexión IMAP usando MailKit
#
# Uso:
#   .\TestConexionImap.ps1 -Usuario "cuenta@gmail.com" -Clave "xxxx xxxx xxxx xxxx"
#   .\TestConexionImap.ps1 -Usuario "cuenta@outlook.es" -Clave "abcdabcdabcdabcd" -Host "outlook.office365.com"
#
# El script crea un proyecto .NET temporal, lo compila y lo ejecuta.
# Requiere .NET 10 instalado.
# =============================================================================

param(
    [Parameter(Mandatory=$true)]  [string] $Usuario,
    [Parameter(Mandatory=$true)]  [string] $Clave,
    [string] $Host1 = "",   # Si se deja vacío se infiere del dominio del usuario
    [int]    $Puerto = 993
)

# Inferir host si no se indica
if ([string]::IsNullOrEmpty($Host1)) {
    if ($Usuario -match "gmail\.com")                              { $Host1 = "imap.gmail.com" }
    elseif ($Usuario -match "gmx\.")                               { $Host1 = "imap.gmx.com" }
    elseif ($Usuario -match "outlook\.|hotmail\.|live\.|msn\.")    { $Host1 = "outlook.office365.com" }
    else                                                           { $Host1 = "outlook.office365.com" }
}

$tmpDir = "C:\Users\jjimenez\AppData\Local\Temp\TestImap"
New-Item -ItemType Directory -Force $tmpDir | Out-Null

@"
using MailKit.Net.Imap;
using MailKit.Security;
using System;

class Test {
    static void Main() {
        var usuario = "$Usuario";
        var clave   = "$Clave";
        var hosts   = new[] {
            ("$Host1", $Puerto, SecureSocketOptions.SslOnConnect),
            ("$Host1", $Puerto, SecureSocketOptions.Auto),
        };
        foreach (var (host, port, ssl) in hosts) {
            Console.WriteLine($"\n--- {host}:{port} ({ssl}) ---");
            try {
                using var c = new ImapClient();
                c.Connect(host, port, ssl);
                Console.WriteLine("  CONECTADO OK");
                c.AuthenticationMechanisms.Remove("XOAUTH2");
                c.AuthenticationMechanisms.Remove("NTLM");
                Console.WriteLine($"  Mecanismos disponibles: {string.Join(", ", c.AuthenticationMechanisms)}");
                try {
                    c.Authenticate(usuario, clave);
                    Console.WriteLine("  AUTENTICADO OK -- la configuracion es correcta");
                    var carpetas = c.GetFolders(c.PersonalNamespaces[0]);
                    Console.WriteLine($"  Buzones encontrados: {string.Join(", ", carpetas)}");
                } catch(Exception ex) {
                    Console.WriteLine($"  AUTH FALLO: {ex.Message}");
                }
                c.Disconnect(true);
            } catch(Exception ex) {
                Console.WriteLine($"  CONEXION FALLO: {ex.Message}");
            }
        }
    }
}
"@ | Out-File "$tmpDir\Program.cs" -Encoding utf8

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MailKit" Version="4.16.0" />
  </ItemGroup>
</Project>
"@ | Out-File "$tmpDir\TestImap.csproj" -Encoding utf8

Write-Host ""
Write-Host "Probando conexion IMAP para: $Usuario en $Host1`:$Puerto"
Write-Host ""

Push-Location $tmpDir
dotnet run 2>&1
Pop-Location
