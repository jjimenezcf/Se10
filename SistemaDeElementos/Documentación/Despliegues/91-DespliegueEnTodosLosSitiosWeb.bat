@echo off
setlocal enabledelayedexpansion

:: Despliega la publicación completa en todos los sitios web, uno detrás de otro, salvo en test.se
::   Uso: 91-DespliegueEnTodosLosSitiosWeb
:: Primero se prueba en test con 90-DesplegarCompleta SE_TEST test.se. Este script descomprime el zip una sola vez,
:: pide una única confirmación y llama a 90-DesplegarCompleta con /auto para cada sitio (sin preguntas ni pausas).
:: Se detiene en el primer sitio que falle, para no extender el problema a los demás. Los sitios ya desplegados
:: quedan desplegados y cada uno tiene su copia de seguridad en C:\copia_sitioweb, para volver atrás con
:: 90-RestaurarSitio. Tras corregir el fallo se puede lanzar de nuevo: vuelve a desplegar también los ya hechos.
:: La lista (base de datos:sitio) sigue las del 01-Descomprimir y parar servicios, sin SE_TEST. Si se añade un sitio
:: allí hay que añadirlo aquí.

set "SITIOS=SE_ACROMUR:Acromur SE_ARKENOS:arkenos SE_CAIFANTASIA:caifantasia SE_FEMDEK:femdek SE_SECIFY:secify SE_FUTURINES:futurines AB_ALBERTO_MARTINEZ:albertomartinez SE_CEOM:ceom SE_INTERIORISMO:interiorismo SE_JHM:jhm"

set "DIR_SCRIPTS=%~dp0"
set "DESPLEGAR=%DIR_SCRIPTS%90-DesplegarCompleta.bat"
set "BASE_PATH=C:\Prepublicacion"
set "ZIPFILE=%BASE_PATH%\Publicacion.zip"
set "INTERMEDIA=%BASE_PATH%\SeCompleta"

:: Verificar privilegios de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

if not exist "%DESPLEGAR%" (
    echo ERROR: no existe %DESPLEGAR%
    pause
    exit /b 1
)
if not exist "%ZIPFILE%" (
    echo ERROR: no existe %ZIPFILE%
    pause
    exit /b 1
)

set /a TOTAL=0
echo Se desplegara la publicacion completa en estos sitios, por este orden:
for %%P in (%SITIOS%) do (
    for /f "tokens=1,2 delims=:" %%A in ("%%P") do (
        set /a TOTAL+=1
        echo   !TOTAL!. %%B  ^(base de datos %%A^)
    )
)
echo.
echo No se incluye test.se. Se detiene en el primer sitio que falle.
echo.
set "RESPUESTA="
set /p "RESPUESTA=Desplegar en los %TOTAL% sitios? (S/N): "
if /i not "%RESPUESTA%"=="S" (
    echo Cancelado, no se ha modificado nada.
    pause
    exit /b 0
)

:: --- Descomprimir una sola vez ---
echo.
echo Descomprimiendo %ZIPFILE% en %INTERMEDIA%...
if exist "%INTERMEDIA%" rmdir /s /q "%INTERMEDIA%"
if exist "%INTERMEDIA%" (
    echo ERROR: no se ha podido vaciar %INTERMEDIA%
    pause
    exit /b 1
)
powershell -NoProfile -Command "$ProgressPreference='SilentlyContinue'; try { Expand-Archive -LiteralPath '%ZIPFILE%' -DestinationPath '%INTERMEDIA%' -Force -ErrorAction Stop } catch { Write-Host $_.Exception.Message; exit 1 }"
if errorlevel 1 (
    echo ERROR: fallo en la descompresion
    pause
    exit /b 1
)
if not exist "%INTERMEDIA%\SistemaDeElementos.dll" (
    echo ERROR: el zip no contiene SistemaDeElementos.dll, no parece la publicacion completa
    pause
    exit /b 1
)

:: --- Un sitio detrás de otro ---
set /a N=0
set "HECHOS="
for %%P in (%SITIOS%) do (
    for /f "tokens=1,2 delims=:" %%A in ("%%P") do (
        set /a N+=1
        echo.
        echo ==================== !N! de %TOTAL%: %%B ====================
        call "%DESPLEGAR%" %%A %%B /auto
        if errorlevel 1 (
            set "FALLIDO=%%B"
            goto :fallo
        )
        set "HECHOS=!HECHOS! %%B"
    )
)

echo.
echo ====================================================
echo Despliegue terminado en los %TOTAL% sitios:%HECHOS%
echo Las copias de seguridad estan en C:\copia_sitioweb
echo ====================================================
pause
exit /b 0

:: El 90 ya ha explicado qué pasó y cómo volver atrás en el sitio que falló
:fallo
set "PENDIENTES="
set "PASADO="
for %%P in (%SITIOS%) do (
    for /f "tokens=1,2 delims=:" %%A in ("%%P") do (
        if defined PASADO set "PENDIENTES=!PENDIENTES! %%B"
        if /i "%%B"=="!FALLIDO!" set "PASADO=1"
    )
)
echo.
echo ====================================================
echo SE HA DETENIDO el despliegue en el sitio: %FALLIDO%
if defined HECHOS (echo Desplegados antes:%HECHOS%) else (echo No se habia desplegado ningun sitio.)
if defined PENDIENTES (echo Sin tocar:%PENDIENTES%) else (echo No quedaban sitios por desplegar.)
echo Revisa el mensaje del sitio %FALLIDO% mas arriba.
echo ====================================================
pause
exit /b 1
