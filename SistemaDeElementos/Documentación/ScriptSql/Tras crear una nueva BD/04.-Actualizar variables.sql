
declare @sitioweb varchar(20) = '......';
declare @servidorDeCorreo varchar(30) = @sitioweb + '.gmx';
declare @logo varchar(30) = @sitioweb + '-logo.png';

update ENTORNO.VARIABLE set valor='C:\Temp\Trazas\'+ @sitioweb where NOMBRE like 'CFG_Ruta_Ficheros_De_Debug'
update ENTORNO.VARIABLE set valor ='c:\AlmacenDocumental\' + @sitioweb where NOMBRE like 'CFG_Servidor_Archivos'
update ENTORNO.VARIABLE set valor ='https://' + @sitioweb + '.femdek.com/' where NOMBRE like 'CFG_UrlBase'
update ENTORNO.VARIABLE set valor = @servidorDeCorreo where NOMBRE like 'CFG_Servidor_De_Correo'

update ENTORNO.VARIABLE set valor = @logo where NOMBRE like 'LYT_Imagen_De_Fondo'
update ENTORNO.VARIABLE set valor = '0.01' where NOMBRE like 'LYT_Opacidad'


select * from ENTORNO.VARIABLE

-- crear los directorios de trazas y de almacen documental ---
-- ejecutar y verificar que envía el correo de doble factor ---
/* crear sitio web en iis

Import-Module WebAdministration

# --- Parámetros ---
$origen       = "acromur"
$nuevo        = "jhm"
$ruta         = "C:\inetpub\jhm"
$sitio        = "jhm"
$usuarioPool  = "www-$nuevo"
$contraseña   = "kZBmanoYg655"

$securePwd = ConvertTo-SecureString $contraseña -AsPlainText -Force
$cuentaFQ  = "$env:COMPUTERNAME\$usuarioPool"

# --- Limpieza de restos de intentos anteriores ---
if (Test-Path "IIS:\Sites\$sitio")    { Remove-Website -Name $sitio }
if (Test-Path "IIS:\AppPools\$nuevo") { Remove-WebAppPool -Name $nuevo }

# --- Crear identidad local dedicada si no existe ---
if (-not (Get-LocalUser -Name $usuarioPool -ErrorAction SilentlyContinue)) {
    New-LocalUser -Name $usuarioPool -Password $securePwd -PasswordNeverExpires -AccountNeverExpires -UserMayNotChangePassword
    Write-Host "Usuario local '$usuarioPool' creado."
} else {
    Write-Host "Usuario local '$usuarioPool' ya existía, se reutiliza (se actualiza su contraseña)."
    Set-LocalUser -Name $usuarioPool -Password $securePwd
}

# --- 1. Crear el Application Pool ---
New-WebAppPool -Name $nuevo

$o = "IIS:\AppPools\$origen"
$n = "IIS:\AppPools\$nuevo"

Set-ItemProperty $n -Name "managedRuntimeVersion" -Value (Get-ItemProperty $o -Name "managedRuntimeVersion").Value
Set-ItemProperty $n -Name "enable32BitAppOnWin64" -Value (Get-ItemProperty $o -Name "enable32BitAppOnWin64").Value

$pipelineMode = Get-ItemProperty $o -Name "managedPipelineMode"
Set-ItemProperty $n -Name "managedPipelineMode" -Value $pipelineMode

Set-ItemProperty $n -Name "recycling.periodicRestart.time" -Value (Get-ItemProperty $o -Name "recycling.periodicRestart.time").Value

# Identidad propia del pool (en vez de copiar la del origen)
Set-ItemProperty $n -Name "processModel.identityType" -Value "SpecificUser"
Set-ItemProperty $n -Name "processModel.userName" -Value $usuarioPool
Set-ItemProperty $n -Name "processModel.password" -Value $contraseña


# --- 2. Crear el sitio web con enlace HTTPS ---
$puertoHttps = 100   # ajusta si quieres un puerto https distinto al que tenías

# Reutilizamos el certificado que ya usa el sitio origen en su enlace https
$bindingOrigenHttps = Get-WebBinding -Name $origen -Protocol "https" | Select-Object -First 1
if (-not $bindingOrigenHttps) {
    throw "El sitio origen '$origen' no tiene un enlace https del que copiar el certificado. Indícame qué certificado usar."
}

$thumbprint = $bindingOrigenHttps.certificateHash
$store      = $bindingOrigenHttps.certificateStoreName

New-Website -Name $sitio -PhysicalPath $ruta -ApplicationPool $nuevo -Port $puertoHttps -Ssl

$cert = Get-ChildItem "Cert:\LocalMachine\$store" | Where-Object { $_.Thumbprint -eq $thumbprint }
if (-not $cert) {
    throw "No se encontró el certificado con thumbprint $thumbprint en Cert:\LocalMachine\$store"
}

New-Item -Path "IIS:\SslBindings\0.0.0.0!$puertoHttps" -Value $cert -Force | Out-Null

# --- 3. Permisos de modificación en carpetas/ficheros ya existentes (copiados de acromur) ---
$rutasExistentes = @(
    "C:\inetpub\jhm\wwwroot\Agendas",
    "C:\inetpub\jhm\wwwroot\Archivos",
    "C:\inetpub\jhm\wwwroot\Plantillas",
    "C:\inetpub\jhm\wwwroot\token.json",
    "C:\inetpub\jhm\wwwroot\js"
)

foreach ($p in $rutasExistentes) {
    if (Test-Path $p) {
        if ((Get-Item $p) -is [System.IO.DirectoryInfo]) {
            icacls $p /grant "${cuentaFQ}:(OI)(CI)M" /T | Out-Null
        } else {
            icacls $p /grant "${cuentaFQ}:M" | Out-Null
        }
        Write-Host "Permisos concedidos en: $p"
    } else {
        Write-Warning "No existe (revisar copia de acromur): $p"
    }
}

# --- 4. Carpetas de datos: crear si no existen y dar permisos ---
$rutasNuevas = @(
    "C:\AlmacenDocumental\jhm",
    "C:\Temp\Certificados\jhm",
    "C:\Temp\Excepciones\jhm",
    "C:\Temp\Firmar\jhm",
    "C:\Temp\Trazas\jhm",
    "C:\Temp\Zip\jhm"
)

foreach ($p in $rutasNuevas) {
    if (-not (Test-Path $p)) {
        New-Item -ItemType Directory -Path $p -Force | Out-Null
        Write-Host "Creado: $p"
    }
    icacls $p /grant "${cuentaFQ}:(OI)(CI)M" /T | Out-Null
    Write-Host "Permisos concedidos en: $p"
}

# --- 5. Verificación ---
Get-ItemProperty $n -Name managedPipelineMode
Get-ItemProperty $n -Name "processModel.identityType"
Get-ItemProperty $n -Name "processModel.userName"
Get-Website -Name $sitio

*/