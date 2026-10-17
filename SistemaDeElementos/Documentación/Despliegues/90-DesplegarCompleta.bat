@echo off
setlocal enabledelayedexpansion

:: Despliega la publicación completa (la de Z_EmpaquetarPublicacionCompleta) en un único sitio web
::   Uso:     90-DesplegarCompleta <base de datos> <sitio web>
::   Ejemplo: 90-DesplegarCompleta SE_TEST test.se
::   Opcional: un tercer parametro /auto, que usa 91-DespliegueEnTodosLosSitiosWeb: no pide confirmacion, no hace pausas
::            y reutiliza el zip que ya esta descomprimido en la carpeta intermedia.
:: Antes de tocar el sitio descomprime el zip en una carpeta intermedia y pide confirmación mostrando qué ficheros
:: desaparecerán. Después bloquea la cola, para el sitio, hace una copia de seguridad, borra las dll y json de la
:: raíz (salvo appsettings*.json), copia la publicación, arranca el sitio y desbloquea la cola.
:: Las carpetas de datos (Archivos, Agendas y Plantillas) no se copian a la copia de seguridad ni se sobrescriben.
:: Si algo va mal tras modificar el sitio, se vuelve atrás con 90-RestaurarSitio.bat y la copia de seguridad.

set "APPCMD=%systemroot%\system32\inetsrv\appcmd.exe"
set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"
set "BASE_PATH=C:\Prepublicacion"
set "ZIPFILE=%BASE_PATH%\Publicacion.zip"
set "INTERMEDIA=%BASE_PATH%\SeCompleta"
set "COPIAS=C:\copia_sitioweb"

set "BD=%~1"
set "SITIO=%~2"
if "%BD%"=="" goto :uso
if "%SITIO%"=="" goto :uso
set "AUTO="
if /i "%~3"=="/auto" set "AUTO=1"

:: Verificar privilegios de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

:: --- 1. VALIDACIONES: todavía no se toca nada ---
if not exist "%ZIPFILE%" (
    echo ERROR: no existe %ZIPFILE%
    goto :abortar
)

set "RUTA="
for /f "delims=" %%P in ('call "%APPCMD%" list vdir "%SITIO%/" /text:physicalPath 2^>nul') do set "RUTA=%%P"
if "%RUTA%"=="" (
    echo ERROR: no existe el sitio web '%SITIO%' en IIS
    goto :abortar
)
:: La ruta física puede venir con variables, por ejemplo %SystemDrive%\inetpub\test
call set "RUTA=%RUTA%"
:: Sin barra final: robocopy interpreta "C:\ruta\" como una comilla escapada
if "%RUTA:~-1%"=="\" set "RUTA=%RUTA:~0,-1%"
if not exist "%RUTA%\" (
    echo ERROR: no existe la carpeta del sitio %RUTA%
    goto :abortar
)

set "POOL="
for /f "delims=" %%P in ('call "%APPCMD%" list app "%SITIO%/" /text:applicationPool 2^>nul') do set "POOL=%%P"
if "%POOL%"=="" (
    echo ERROR: no se ha podido obtener el grupo de aplicaciones del sitio '%SITIO%'
    goto :abortar
)

echo Sitio web:              %SITIO%
echo Carpeta:                %RUTA%
echo Grupo de aplicaciones:  %POOL%
echo Base de datos:          %BD%
echo.

:: --- 2. DESCOMPRIMIR en una carpeta intermedia, sin tocar el sitio ---
if defined AUTO if exist "%INTERMEDIA%\SistemaDeElementos.dll" goto :yaDescomprimido
echo Descomprimiendo %ZIPFILE% en %INTERMEDIA%...
if exist "%INTERMEDIA%" rmdir /s /q "%INTERMEDIA%"
if exist "%INTERMEDIA%" (
    echo ERROR: no se ha podido vaciar %INTERMEDIA%
    goto :abortar
)
powershell -NoProfile -Command "$ProgressPreference='SilentlyContinue'; try { Expand-Archive -LiteralPath '%ZIPFILE%' -DestinationPath '%INTERMEDIA%' -Force -ErrorAction Stop } catch { Write-Host $_.Exception.Message; exit 1 }"
if errorlevel 1 (
    echo ERROR: fallo en la descompresion
    goto :abortar
)
if not exist "%INTERMEDIA%\SistemaDeElementos.dll" (
    echo ERROR: el zip no contiene SistemaDeElementos.dll, no parece la publicacion completa
    goto :abortar
)
:yaDescomprimido

:: --- 3. MOSTRAR lo que desaparecerá y pedir confirmación ---
set /a EN_PUBLICACION=0
for %%F in ("%INTERMEDIA%\*") do set /a EN_PUBLICACION+=1

echo.
echo Ficheros de la raiz del sitio que desapareceran porque no vienen en la publicacion:
set /a DESAPARECEN=0
for %%F in ("%RUTA%\*.dll" "%RUTA%\*.json") do (
    echo %%~nxF| findstr /i /b "appsettings" >nul
    if errorlevel 1 (
        if not exist "%INTERMEDIA%\%%~nxF" (
            echo   %%~nxF
            set /a DESAPARECEN+=1
        )
    )
)
if %DESAPARECEN%==0 echo   ninguno
echo.
echo Se copiaran %EN_PUBLICACION% ficheros en la raiz, ademas de las carpetas de la publicacion.
echo Se conservan appsettings*.json y las carpetas Archivos, Agendas y Plantillas.
echo.
set "RESPUESTA=S"
if not defined AUTO (
    set "RESPUESTA="
    set /p "RESPUESTA=Desplegar la publicacion completa en %SITIO%? (S/N): "
)
if /i not "%RESPUESTA%"=="S" (
    echo Cancelado, no se ha modificado nada.
    goto :abortar
)

:: --- 4. BLOQUEAR la cola ---
call :cola Bloquear_Cola
if errorlevel 1 (
    echo ERROR: no se ha podido bloquear la cola de %BD%, no se ha modificado nada.
    goto :abortar
)

:: --- 5. PARAR el sitio y esperar a que el grupo de aplicaciones suelte las dll ---
echo Parando el sitio %SITIO% y el grupo de aplicaciones %POOL%...
call "%APPCMD%" stop site /site.name:"%SITIO%" >nul 2>&1
call "%APPCMD%" stop apppool /apppool.name:"%POOL%" >nul 2>&1
call :esperarParada
if errorlevel 1 (
    echo ERROR: el grupo de aplicaciones %POOL% no se ha parado, no se ha modificado nada.
    goto :abortarArrancando
)

:: --- 6. COPIA DE SEGURIDAD, con el sitio parado y antes de cualquier cambio ---
for /f %%I in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set "FECHA_HORA=%%I"
set "COPIA=%COPIAS%\%SITIO%_%FECHA_HORA%"
echo Copiando el sitio en %COPIA%...
robocopy "%RUTA%" "%COPIA%" /E /R:1 /W:1 /NP /NFL /NDL /XD Archivos Agendas Plantillas /LOG:"%BASE_PATH%\99-copia-%SITIO%.txt" >nul
if errorlevel 8 (
    echo ERROR: fallo la copia de seguridad, revisa %BASE_PATH%\99-copia-%SITIO%.txt. No se ha modificado nada.
    rmdir /s /q "%COPIA%" 2>nul
    goto :abortarArrancando
)
if not exist "%COPIA%\web.config" (
    echo ERROR: la copia de seguridad esta incompleta, no se ha modificado nada.
    rmdir /s /q "%COPIA%" 2>nul
    goto :abortarArrancando
)
:: Marca de copia terminada: 90-RestaurarSitio no acepta una copia sin ella
echo Copia de %SITIO% (%RUTA%) terminada el %date% a las %time%> "%COPIA%\_copia_completa.txt"

:: --- 7. LIMPIAR las dll y json de la raíz, salvo appsettings*.json ---
echo Borrando las dll y json de la raiz de %RUTA%...
for %%F in ("%RUTA%\*.dll" "%RUTA%\*.json") do (
    echo %%~nxF| findstr /i /b "appsettings" >nul
    if errorlevel 1 del /f /q "%%~fF" >nul 2>&1
)
set /a QUEDAN=0
for %%F in ("%RUTA%\*.dll") do (
    echo   NO SE PUDO BORRAR: %%~nxF
    set /a QUEDAN+=1
)
if %QUEDAN% gtr 0 (
    echo ERROR: hay ficheros bloqueados.
    goto :abortarSitioModificado
)

:: --- 8. COPIAR la publicación al sitio ---
echo Copiando la publicacion en %RUTA%...
robocopy "%INTERMEDIA%" "%RUTA%" /E /R:2 /W:2 /NP /XD Archivos Agendas Plantillas /LOG:"%BASE_PATH%\99-completa-%SITIO%.txt" >nul
if errorlevel 8 (
    echo ERROR: fallo la copia de la publicacion, revisa %BASE_PATH%\99-completa-%SITIO%.txt
    goto :abortarSitioModificado
)
if not exist "%RUTA%\SistemaDeElementos.dll" (
    echo ERROR: tras la copia no esta SistemaDeElementos.dll en %RUTA%
    goto :abortarSitioModificado
)

:: --- 9. ARRANCAR el sitio y desbloquear la cola ---
call :arrancar
call :cola Desbloquear_Cola
if errorlevel 1 echo AVISO: no se ha podido desbloquear la cola de %BD%, ejecuta Desbloquear_Cola a mano.

:: --- 10. RESUMEN ---
echo.
call :mostrarEstado
echo.
echo Despliegue terminado.
echo Copia de seguridad: %COPIA%
echo Para volver atras:  90-RestaurarSitio %SITIO% "%COPIA%"
if not defined AUTO pause
exit /b 0


:: ================= Subrutinas =================

:: Ejecuta en la base de datos el procedimiento que se le pasa (Bloquear_Cola o Desbloquear_Cola)
:cola
set "LOG_COLA=%BASE_PATH%\99-%~1-%BD%.txt"
echo Ejecutando %~1 en %BD%...
%SQLCMD% -b -d %BD% -Q "USE %BD%; EXEC [dbo].[%~1]" > "%LOG_COLA%" 2>&1
set "RESULTADO=%errorlevel%"
findstr /v /r "^$ ^-*$" "%LOG_COLA%" | findstr /v "Result"
if not "%RESULTADO%"=="0" exit /b 1
findstr /i "ERROR" "%LOG_COLA%" >nul && exit /b 1
exit /b 0

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


:: ================= Salidas con error =================

:: El sitio no se ha modificado y sigue como estaba
:abortar
if not defined AUTO pause
exit /b 1

:: El sitio está parado y la cola bloqueada, pero sus ficheros no se han modificado: se deja como estaba
:abortarArrancando
call :arrancar
call :cola Desbloquear_Cola
call :mostrarEstado
if not defined AUTO pause
exit /b 1

:: Los ficheros del sitio ya se han modificado: se deja parado para restaurarlo con la copia de seguridad.
:: La cola se desbloquea porque, con el sitio parado, nadie la procesa y así queda bien al restaurar.
:abortarSitioModificado
call :cola Desbloquear_Cola
echo.
echo El sitio %SITIO% queda PARADO con los ficheros a medias.
echo Para volver atras: 90-RestaurarSitio %SITIO% "%COPIA%"
if not defined AUTO pause
exit /b 1

:uso
echo Uso:     90-DesplegarCompleta ^<base de datos^> ^<sitio web^>
echo Ejemplo: 90-DesplegarCompleta SE_TEST test.se
if not defined AUTO pause
exit /b 1
