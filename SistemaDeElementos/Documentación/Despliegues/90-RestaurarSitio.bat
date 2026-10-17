@echo off
setlocal enabledelayedexpansion

:: Restaura un sitio web con la copia de seguridad que hizo 90-DesplegarCompleta.bat
::   Uso:     90-RestaurarSitio <sitio web> <carpeta de copia>
::   Ejemplo: 90-RestaurarSitio test.se C:\copia_sitioweb\test.se_20261017_093015
:: Deja la carpeta del sitio igual que la copia (robocopy /MIR): vuelven los ficheros anteriores y desaparecen
:: los que añadió el despliegue. No toca las carpetas de datos (Archivos, Agendas y Plantillas), que no están en
:: la copia, ni la carpeta logs, para conservar los registros que expliquen el fallo.

set "APPCMD=%systemroot%\system32\inetsrv\appcmd.exe"
set "BASE_PATH=C:\Prepublicacion"

set "SITIO=%~1"
set "COPIA=%~2"
if "%SITIO%"=="" goto :uso
if "%COPIA%"=="" goto :uso
if "%COPIA:~-1%"=="\" set "COPIA=%COPIA:~0,-1%"

:: Verificar privilegios de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

:: --- COMPROBACIONES: /MIR borra en el destino lo que no está en la copia, así que las dos rutas han de ser las correctas ---
if not exist "%COPIA%\_copia_completa.txt" (
    echo ERROR: %COPIA% no es una copia terminada de 90-DesplegarCompleta, le falta _copia_completa.txt
    goto :abortar
)
if not exist "%COPIA%\SistemaDeElementos.dll" (
    echo ERROR: %COPIA% no es una copia de seguridad del sitio, no contiene SistemaDeElementos.dll
    goto :abortar
)
if not exist "%COPIA%\web.config" (
    echo ERROR: %COPIA% no es una copia de seguridad del sitio, no contiene web.config
    goto :abortar
)

set "RUTA="
for /f "delims=" %%P in ('call "%APPCMD%" list vdir "%SITIO%/" /text:physicalPath 2^>nul') do set "RUTA=%%P"
if "%RUTA%"=="" (
    echo ERROR: no existe el sitio web '%SITIO%' en IIS
    goto :abortar
)
call set "RUTA=%RUTA%"
:: Sin barra final: robocopy interpreta "C:\ruta\" como una comilla escapada
if "%RUTA:~-1%"=="\" set "RUTA=%RUTA:~0,-1%"
if not exist "%RUTA%\web.config" (
    echo ERROR: la carpeta del sitio %RUTA% no contiene web.config
    goto :abortar
)
if /i "%RUTA%"=="%COPIA%" (
    echo ERROR: la copia y el sitio son la misma carpeta
    goto :abortar
)

set "POOL="
for /f "delims=" %%P in ('call "%APPCMD%" list app "%SITIO%/" /text:applicationPool 2^>nul') do set "POOL=%%P"
if "%POOL%"=="" (
    echo ERROR: no se ha podido obtener el grupo de aplicaciones del sitio '%SITIO%'
    goto :abortar
)

echo Sitio web:              %SITIO%
echo Grupo de aplicaciones:  %POOL%
echo Copia de seguridad:     %COPIA%
echo Carpeta a restaurar:    %RUTA%
echo.
echo La carpeta del sitio quedara igual que la copia, salvo Archivos, Agendas, Plantillas y logs.
set "RESPUESTA="
set /p "RESPUESTA=Restaurar %SITIO% con esta copia? (S/N): "
if /i not "%RESPUESTA%"=="S" (
    echo Cancelado, no se ha modificado nada.
    goto :abortar
)

:: --- PARAR el sitio y esperar a que el grupo de aplicaciones suelte las dll ---
echo Parando el sitio %SITIO% y el grupo de aplicaciones %POOL%...
call "%APPCMD%" stop site /site.name:"%SITIO%" >nul 2>&1
call "%APPCMD%" stop apppool /apppool.name:"%POOL%" >nul 2>&1
call :esperarParada
if errorlevel 1 (
    echo ERROR: el grupo de aplicaciones %POOL% no se ha parado, no se ha modificado nada.
    call :arrancar
    goto :abortar
)

:: --- RESTAURAR ---
echo Restaurando %RUTA% desde %COPIA%...
robocopy "%COPIA%" "%RUTA%" /MIR /R:2 /W:2 /NP /XD Archivos Agendas Plantillas logs /XF _copia_completa.txt /LOG:"%BASE_PATH%\99-restaurar-%SITIO%.txt" >nul
if errorlevel 8 (
    echo ERROR: fallo la restauracion, revisa %BASE_PATH%\99-restaurar-%SITIO%.txt
    echo El sitio %SITIO% queda PARADO.
    goto :abortar
)

call :arrancar
echo.
call :mostrarEstado
echo.
echo Restauracion terminada.
pause
exit /b 0


:: ================= Subrutinas =================

:: Espera hasta 60 segundos a que el grupo de aplicaciones esté parado
:esperarParada
set /a INTENTOS=0
:esperarParadaBucle
set "ESTADO="
for /f "delims=" %%E in ('call "%APPCMD%" list apppool "%POOL%" /text:state 2^>nul') do set "ESTADO=%%E"
if /i "%ESTADO%"=="Stopped" exit /b 0
set /a INTENTOS+=1
if %INTENTOS% geq 30 exit /b 1
ping -n 3 127.0.0.1 >nul
goto :esperarParadaBucle

:arrancar
echo Arrancando el grupo de aplicaciones %POOL% y el sitio %SITIO%...
call "%APPCMD%" start apppool /apppool.name:"%POOL%" >nul 2>&1
call "%APPCMD%" start site /site.name:"%SITIO%" >nul 2>&1
exit /b 0

:mostrarEstado
set "ESTADO_SITIO="
set "ESTADO_POOL="
for /f "delims=" %%E in ('call "%APPCMD%" list site "%SITIO%" /text:state 2^>nul') do set "ESTADO_SITIO=%%E"
for /f "delims=" %%E in ('call "%APPCMD%" list apppool "%POOL%" /text:state 2^>nul') do set "ESTADO_POOL=%%E"
echo Estado del sitio %SITIO%: %ESTADO_SITIO%
echo Estado del grupo %POOL%: %ESTADO_POOL%
exit /b 0

:abortar
pause
exit /b 1

:uso
echo Uso:     90-RestaurarSitio ^<sitio web^> ^<carpeta de copia^>
echo Ejemplo: 90-RestaurarSitio test.se C:\copia_sitioweb\test.se_20261017_093015
pause
exit /b 1
