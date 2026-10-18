<#
.SYNOPSIS
  Crea un sitio web nuevo de Se en IIS: usuario local del pool, pool, sitio https, copia de la prepublicacion,
  permisos sobre sus carpetas y sobre LibreOffice.

.DESCRIPTION
  Sustituye al script que estaba como comentario en "ScriptSql\Tras crear una nueva BD\04.-Actualizar variables.sql".
  Cambios respecto a aquel:
    - La contrasena del usuario del pool ya no esta escrita en el script: se pide por pantalla (o se pasa como SecureString).
    - Todo es parametrizable (sitio, sitio del que copiar la configuracion, ruta, puerto, rutas de prepublicacion y LibreOffice).
    - No borra un sitio o pool que ya exista salvo que se indique -Recrear.
    - Copia la prepublicacion al directorio del sitio (si esta vacio o no existe), como hace 02-Desplegar y arrancar servicios.cmd.
    - Da permiso de lectura y ejecucion sobre C:\LibreOffice al usuario del pool, y lo anade al grupo IIS_IUSRS.
    - Copia tambien del pool origen el tiempo de inactividad, su accion (Suspend) y el modo de arranque (AlwaysRunning).
    - No sobrescribe el certificado de un puerto https que ya tenga otro certificado.
    - -SoloComprobar revisa los requisitos y muestra el plan sin cambiar nada.

  Despues de ejecutarlo quedan por hacer a mano (el script lo recuerda al final):
    - appsettings.json del sitio (la prepublicacion no lo incluye) y crear su base de datos.
    - Ejecutar el 04.-Actualizar variables.sql de la nueva base de datos.
    - Anadir el sitio, su pool, su base de datos y su carpeta a 01-Descomprimir y parar servicios.cmd,
      02-Desplegar y arrancar servicios.cmd y 20 - Preproductivo....cmd.
    - DNS del nombre del sitio.

.PARAMETER Sitio
  Nombre del sitio (y del pool). El usuario del pool sera www-<Sitio> (Windows limita el nombre a 20 caracteres).

.PARAMETER Origen
  Sitio existente del que se copia el certificado https y la configuracion del pool. Por defecto acromur.

.PARAMETER Ruta
  Directorio del sitio. Por defecto C:\inetpub\<Sitio>.

.PARAMETER PuertoHttps
  Puerto https del sitio. Por defecto 100, como en el script original.

.PARAMETER Contrasena
  Contrasena del usuario del pool (SecureString). Si no se indica, se pide por pantalla.

.PARAMETER RutaPrepublicacion
  Carpeta con la publicacion descomprimida. Por defecto C:\Prepublicacion\Se.

.PARAMETER NoCopiarPrepublicacion
  No copia la prepublicacion (el directorio del sitio ya tiene los ficheros).

.PARAMETER RutaLibreOffice
  Carpeta de LibreOffice a la que se da lectura y ejecucion. Por defecto C:\LibreOffice.

.PARAMETER Recrear
  Si el sitio o el pool ya existen, los borra y los vuelve a crear. SIN ESTE PARAMETRO SE ABORTA.

.PARAMETER SoloComprobar
  Revisa los requisitos y muestra lo que haria, sin cambiar nada.

.EXAMPLE
  .\CrearSitioWeb.ps1 -Sitio jhm -SoloComprobar

.EXAMPLE
  .\CrearSitioWeb.ps1 -Sitio jhm
#>
#Requires -Version 5.1
#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]*$')][string]$Sitio,
    [string]$Origen = 'acromur',
    [string]$Ruta,
    [int]$PuertoHttps = 100,
    [System.Security.SecureString]$Contrasena,
    [string]$RutaPrepublicacion = 'C:\Prepublicacion\Se',
    [switch]$NoCopiarPrepublicacion,
    [string]$RutaLibreOffice = 'C:\LibreOffice',
    [switch]$Recrear,
    [switch]$SoloComprobar
)

$ErrorActionPreference = 'Stop'

if (-not $Ruta) { $Ruta = "C:\inetpub\$Sitio" }
$usuarioPool = "www-$Sitio"
$cuenta = "$env:COMPUTERNAME\$usuarioPool"
$poolOrigen = "IIS:\AppPools\$Origen"
$poolNuevo = "IIS:\AppPools\$Sitio"

$errores = New-Object System.Collections.Generic.List[string]
$avisos = New-Object System.Collections.Generic.List[string]

# ---------------------------------------------------------------------------------------------------------------
# 1. Comprobaciones previas (no cambian nada)
# ---------------------------------------------------------------------------------------------------------------
if ($usuarioPool.Length -gt 20) {
    $errores.Add("El usuario '$usuarioPool' tiene mas de 20 caracteres, que es el maximo de un usuario local de Windows: use un nombre de sitio de 16 o menos.")
}

if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
    throw "No esta disponible el modulo WebAdministration: este equipo no tiene IIS o su consola de administracion instalada."
}
Import-Module WebAdministration

if (-not (Test-Path $poolOrigen)) { $errores.Add("No existe el pool origen '$Origen' del que copiar la configuracion.") }

$thumbprint = $null
$almacen = $null
if (-not (Get-Website -Name $Origen -ErrorAction SilentlyContinue)) {
    $errores.Add("No existe el sitio origen '$Origen'.")
}
else {
    $bindingOrigen = Get-WebBinding -Name $Origen -Protocol 'https' | Select-Object -First 1
    if (-not $bindingOrigen) {
        $errores.Add("El sitio origen '$Origen' no tiene un enlace https del que copiar el certificado.")
    }
    else {
        $thumbprint = $bindingOrigen.certificateHash
        $almacen = $bindingOrigen.certificateStoreName
        $certificado = Get-ChildItem "Cert:\LocalMachine\$almacen" | Where-Object { $_.Thumbprint -eq $thumbprint }
        if (-not $certificado) { $errores.Add("No se encuentra el certificado $thumbprint en Cert:\LocalMachine\$almacen.") }
    }
}

$existeSitio = [bool](Get-Website -Name $Sitio -ErrorAction SilentlyContinue)
$existePool = Test-Path $poolNuevo
if (($existeSitio -or $existePool) -and -not $Recrear) {
    $errores.Add("Ya existe el sitio o el pool '$Sitio'. No se toca: use -Recrear si de verdad quiere borrarlos y crearlos de nuevo.")
}

# Un puerto https con otro certificado: no se sobrescribe (cambiaria el certificado de los sitios que ya lo usan)
$enlaceSsl = "IIS:\SslBindings\0.0.0.0!$PuertoHttps"
if ((Test-Path $enlaceSsl) -and $thumbprint) {
    $otro = (Get-Item $enlaceSsl).Thumbprint
    if ($otro -ne $thumbprint) {
        $errores.Add("El puerto $PuertoHttps ya tiene un certificado distinto al de '$Origen' ($otro): elija otro puerto o resuelva primero el conflicto.")
    }
    else {
        $avisos.Add("El puerto $PuertoHttps ya tiene el mismo certificado: se reutiliza ese enlace.")
    }
}

$copiarPrepublicacion = -not $NoCopiarPrepublicacion
if ($copiarPrepublicacion) {
    if (-not (Test-Path $RutaPrepublicacion) -or -not (Get-ChildItem $RutaPrepublicacion -Force -ErrorAction SilentlyContinue)) {
        $errores.Add("No hay prepublicacion en '$RutaPrepublicacion' (use -NoCopiarPrepublicacion si el sitio ya tiene los ficheros).")
    }
    elseif ((Test-Path $Ruta) -and (Get-ChildItem $Ruta -Force -ErrorAction SilentlyContinue)) {
        if ($Recrear) {
            $avisos.Add("'$Ruta' ya tiene contenido: se copia la prepublicacion encima (los ficheros de igual nombre se sobrescriben).")
        }
        else {
            $copiarPrepublicacion = $false
            $avisos.Add("'$Ruta' ya tiene contenido: NO se copia la prepublicacion para no sobrescribir su configuracion.")
        }
    }
}

if (-not (Test-Path "$RutaLibreOffice\program\soffice.exe")) {
    $avisos.Add("No se encuentra '$RutaLibreOffice\program\soffice.exe': se omite el permiso sobre LibreOffice (la conversion usara ConvertApi si hay clave).")
    $RutaLibreOffice = $null
}

$carpetasDeDatos = @(
    "C:\AlmacenDocumental\$Sitio",
    "C:\Temp\Certificados\$Sitio",
    "C:\Temp\Excepciones\$Sitio",
    "C:\Temp\Firmar\$Sitio",
    "C:\Temp\Trazas\$Sitio",
    "C:\Temp\Zip\$Sitio"
)
$rutasConPermiso = @("$Ruta\wwwroot\Agendas", "$Ruta\wwwroot\Archivos", "$Ruta\wwwroot\Plantillas", "$Ruta\wwwroot\token.json", "$Ruta\wwwroot\js")

# ---------------------------------------------------------------------------------------------------------------
# 2. Plan
# ---------------------------------------------------------------------------------------------------------------
Write-Host ""
Write-Host "Sitio nuevo:         $Sitio  (ruta $Ruta, https en el puerto $PuertoHttps)"
Write-Host "Usuario del pool:    $cuenta"
Write-Host "Se copia de:         sitio/pool '$Origen' (certificado $thumbprint y configuracion del pool)"
Write-Host ("Prepublicacion:      " + $(if ($copiarPrepublicacion) { "se copia $RutaPrepublicacion -> $Ruta" } else { "no se copia" }))
Write-Host ("LibreOffice:         " + $(if ($RutaLibreOffice) { "lectura y ejecucion para $usuarioPool en $RutaLibreOffice" } else { "sin permisos (no encontrado)" }))
Write-Host "Carpetas de datos:   $($carpetasDeDatos -join ', ')"
foreach ($a in $avisos) { Write-Warning $a }
if ($errores.Count -gt 0) {
    Write-Host ""
    foreach ($e in $errores) { Write-Host "ERROR: $e" -ForegroundColor Red }
    throw "No se puede continuar: $($errores.Count) problema(s). No se ha cambiado nada."
}
if ($SoloComprobar) {
    Write-Host ""
    Write-Host "Comprobacion correcta: no se ha cambiado nada." -ForegroundColor Green
    return
}

# ---------------------------------------------------------------------------------------------------------------
# 3. Contrasena (por pantalla, nunca escrita en el script)
# ---------------------------------------------------------------------------------------------------------------
if (-not $Contrasena) {
    $c1 = Read-Host -Prompt "Contrasena para el usuario '$usuarioPool'" -AsSecureString
    $c2 = Read-Host -Prompt "Repita la contrasena" -AsSecureString
    $t1 = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($c1)
    $t2 = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($c2)
    try {
        if ([Runtime.InteropServices.Marshal]::PtrToStringBSTR($t1) -ne [Runtime.InteropServices.Marshal]::PtrToStringBSTR($t2)) { throw "Las contrasenas no coinciden." }
        if ($c1.Length -lt 12) { throw "La contrasena debe tener al menos 12 caracteres." }
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($t1)
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($t2)
    }
    $Contrasena = $c1
}

# ---------------------------------------------------------------------------------------------------------------
# 4. Creacion
# ---------------------------------------------------------------------------------------------------------------
if ($Recrear) {
    if ($existeSitio) { Remove-Website -Name $Sitio; Write-Host "Sitio '$Sitio' borrado." }
    if ($existePool) { Remove-WebAppPool -Name $Sitio; Write-Host "Pool '$Sitio' borrado." }
}

# Usuario local dedicado
if (-not (Get-LocalUser -Name $usuarioPool -ErrorAction SilentlyContinue)) {
    New-LocalUser -Name $usuarioPool -Password $Contrasena -PasswordNeverExpires -AccountNeverExpires -UserMayNotChangePassword -Description "Identidad del pool IIS $Sitio" | Out-Null
    Write-Host "Usuario local '$usuarioPool' creado."
}
else {
    Set-LocalUser -Name $usuarioPool -Password $Contrasena
    Write-Host "Usuario local '$usuarioPool' ya existia: se reutiliza y se actualiza su contrasena."
}
# Con una identidad propia el usuario ha de estar en IIS_IUSRS (acceso a los temporales y compresion de IIS)
if (-not (Get-LocalGroupMember -Group 'IIS_IUSRS' -Member $usuarioPool -ErrorAction SilentlyContinue)) {
    Add-LocalGroupMember -Group 'IIS_IUSRS' -Member $usuarioPool
    Write-Host "Usuario '$usuarioPool' anadido al grupo IIS_IUSRS."
}

# Pool: configuracion copiada del origen e identidad propia
New-WebAppPool -Name $Sitio | Out-Null
foreach ($p in 'managedRuntimeVersion', 'enable32BitAppOnWin64', 'managedPipelineMode', 'recycling.periodicRestart.time',
               'processModel.idleTimeout', 'processModel.idleTimeoutAction', 'startMode') {
    try {
        $v = Get-ItemProperty $poolOrigen -Name $p
        if ($null -ne $v -and $v.PSObject.Properties['Value']) { $v = $v.Value }
        Set-ItemProperty $poolNuevo -Name $p -Value $v
    }
    catch {
        Write-Warning "No se pudo copiar '$p' del pool '$Origen': $($_.Exception.Message)"
    }
}
$textoContrasena = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Contrasena))
Set-ItemProperty $poolNuevo -Name 'processModel.identityType' -Value 'SpecificUser'
Set-ItemProperty $poolNuevo -Name 'processModel.userName' -Value $usuarioPool
Set-ItemProperty $poolNuevo -Name 'processModel.password' -Value $textoContrasena
$textoContrasena = $null

# Prepublicacion
if (-not (Test-Path $Ruta)) { New-Item -ItemType Directory -Path $Ruta -Force | Out-Null }
if ($copiarPrepublicacion) {
    Write-Host "Copiando la prepublicacion a $Ruta ..."
    robocopy $RutaPrepublicacion $Ruta /E /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy fallo al copiar la prepublicacion (codigo $LASTEXITCODE)." }
    $global:LASTEXITCODE = 0
}

# Sitio https con el certificado del origen
New-Website -Name $Sitio -PhysicalPath $Ruta -ApplicationPool $Sitio -Port $PuertoHttps -Ssl | Out-Null
if (-not (Test-Path $enlaceSsl)) {
    New-Item -Path $enlaceSsl -Value $certificado | Out-Null
}

# Permisos de modificacion sobre lo que escribe la aplicacion
foreach ($p in $rutasConPermiso) {
    if (Test-Path $p) {
        if ((Get-Item $p) -is [System.IO.DirectoryInfo]) { icacls $p /grant "${cuenta}:(OI)(CI)M" /T /C /Q | Out-Null }
        else { icacls $p /grant "${cuenta}:M" /C /Q | Out-Null }
        Write-Host "Permisos de modificacion en: $p"
    }
    else {
        Write-Warning "No existe: $p"
    }
}

# Carpetas de datos
foreach ($p in $carpetasDeDatos) {
    if (-not (Test-Path $p)) { New-Item -ItemType Directory -Path $p -Force | Out-Null; Write-Host "Creada: $p" }
    icacls $p /grant "${cuenta}:(OI)(CI)M" /T /C /Q | Out-Null
    Write-Host "Permisos de modificacion en: $p"
}

# LibreOffice: solo lectura y ejecucion (no escribe en su carpeta; el perfil va a la carpeta temporal)
if ($RutaLibreOffice) {
    icacls $RutaLibreOffice /grant "${cuenta}:(OI)(CI)RX" /T /C /Q | Out-Null
    Write-Host "Lectura y ejecucion en: $RutaLibreOffice"
}

# ---------------------------------------------------------------------------------------------------------------
# 5. Verificacion y pendiente
# ---------------------------------------------------------------------------------------------------------------
Write-Host ""
Write-Host "--- Verificacion ---"
Get-ItemProperty $poolNuevo -Name managedPipelineMode
Get-ItemProperty $poolNuevo -Name 'processModel.identityType'
Get-ItemProperty $poolNuevo -Name 'processModel.userName'
Get-ItemProperty $poolNuevo -Name startMode
Get-Website -Name $Sitio
if (-not (Test-Path "$Ruta\web.config")) { Write-Warning "No hay web.config en '$Ruta': el sitio no arrancara hasta que se copie la publicacion." }
if (-not (Test-Path "$Ruta\appsettings.json")) { Write-Warning "No hay appsettings.json en '$Ruta': la prepublicacion no lo incluye, hay que crearlo con la cadena de conexion de este sitio." }

Write-Host ""
Write-Host "--- Pendiente a mano ---" -ForegroundColor Yellow
Write-Host " 1. Crear la base de datos del sitio y su appsettings.json en $Ruta."
Write-Host " 2. Ejecutar 04.-Actualizar variables.sql en la base de datos nueva."
Write-Host " 3. Anadir '$Sitio' (sitio, pool, base de datos y carpeta) a los scripts 01-Descomprimir, 02-Desplegar y 20 - Preproductivo."
Write-Host " 4. Dar de alta el nombre del sitio en el DNS."
