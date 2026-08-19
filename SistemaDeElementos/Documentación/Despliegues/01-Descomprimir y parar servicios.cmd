@echo off
setlocal enabledelayedexpansion

:: Verificar privilegios de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

echo Ejecutando procedimientos almacenados...

:: Configuración de SQLCMD
set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"

:: Ejecución de procedimientos en bases de datos 
for %%D in (SE_ACROMUR SE_TEST SE_ARKENOS SE_CAIFANTASIA SE_FEMDEK SE_SECIFY SE_FUTURINES AB_ALBERTO_MARTINEZ SE_CEOM SE_INTERIORISMO SE_JHM) do (
    echo Procesando base de datos: %%D
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Bloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)

echo Todos los procedimientos almacenados se ejecutaron correctamente.

:: --- SECCIÓN DE BACKUP CORREGIDA ---
echo Preparando copia de seguridad historica...

set "BASE_PATH=C:\Prepublicacion"
set "ORIGEN=%BASE_PATH%\Se"
set "VERSIONES_RAIZ=%BASE_PATH%\versiones_anteriores"

:: Obtener fecha y hora de forma segura para nombres de carpetas
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set "dt=%%I"
set "FECHA_HORA=%dt:~0,4%%dt:~4,2%%dt:~6,2%_%dt:~8,2%%dt:~10,2%"
set "BACKUP_DEST=%VERSIONES_RAIZ%\version_%FECHA_HORA%"

:: Crear directorio raíz si no existe
if not exist "%VERSIONES_RAIZ%" mkdir "%VERSIONES_RAIZ%"

:: Realizar backup antes de borrar 
if exist "%ORIGEN%" (
    echo Creando backup en: %BACKUP_DEST%
    mkdir "%BACKUP_DEST%"
    xcopy "%ORIGEN%\*" "%BACKUP_DEST%\" /S /E /Y /H /I >nul
)

:: --- DESCOMPRESIÓN ---
echo Descomprimiendo Zip...

set "ZIPFILE=%BASE_PATH%\Publicacion.ZIP"
set "DESTINATION=%BASE_PATH%\Se"

:: Borrar directorio actual tras el backup 
rmdir /s /q "%ORIGEN%"

:: Unzip del archivo 
powershell -command "Expand-Archive -Path '%ZIPFILE%' -DestinationPath '%DESTINATION%'"
if %errorlevel% neq 0 (
  echo Fallo en la descompresion. Abortando.
  exit /b 1
)

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