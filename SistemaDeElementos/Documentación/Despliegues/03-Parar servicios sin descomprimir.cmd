@echo off
setlocal enabledelayedexpansion

:: Igual que 01-Descomprimir y parar servicios, pero SIN copia de versiones anteriores ni descompresion:
:: C:\Prepublicacion\Se ya esta descomprimido (lo dejo el despliegue de preproductivo y es lo que se ha probado).

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

if not exist "C:\Prepublicacion\Se\SistemaDeElementos.dll" (
  echo No existe C:\Prepublicacion\Se\SistemaDeElementos.dll: el zip no esta descomprimido. Abortando.
  pause
  exit /b 1
)

echo Ejecutando procedimientos almacenados...

set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"

for %%D in (SE_ACROMUR SE_TEST SE_ARKENOS SE_CAIFANTASIA SE_FEMDEK SE_SECIFY SE_FUTURINES AB_ALBERTO_MARTINEZ SE_CEOM SE_INTERIORISMO SE_JHM) do (
    echo Procesando base de datos: %%D
    rem Si un despliegue anterior fallo y dejo la cola bloqueada, se libera antes de volver a bloquearla
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Desbloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Bloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)

echo Todos los procedimientos almacenados se ejecutaron correctamente.

echo Parando servicios y grupos de aplicaciones...

set "SITIOS=Acromur secify arkenos caifantasia  femdek futurines albertomartinez ceom test.se interiorismo jhm"
for %%S in (%SITIOS%) do (
    powershell -command "Stop-WebSite -Name '%%S'"
)

set "APPOOLS=acromur arkenos caifantasia femdek secify futurines albertomartinez ceom test.se interiorismo jhm"

for %%A in (%APPOOLS%) do (
    echo Configurando y parando grupo de aplicaciones: %%A
    powershell -command "Stop-WebAppPool -Name '%%A'"
    %systemroot%\system32\inetsrv\AppCmd.exe set apppool /apppool.name:"%%A" /processModel.idleTimeout:00:00:00
    %systemroot%\system32\inetsrv\AppCmd.exe set apppool /apppool.name:"%%A" /processModel.idleTimeoutAction:Suspend
    %systemroot%\system32\inetsrv\AppCmd.exe set apppool /apppool.name:"%%A" /startMode:AlwaysRunning
)

echo Listo para la copia...

pause
exit /b 0

:error
echo Se produjo un error al ejecutar el procedimiento almacenado.
echo Abortando el despliegue.
pause
exit /b 1
