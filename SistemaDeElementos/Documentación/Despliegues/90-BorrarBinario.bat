@echo off
setlocal enabledelayedexpansion

:: Borra de los sitios desplegados, y de la carpeta de origen del despliegue, los ficheros indicados como parámetros
::   Uso:     90-BorrarBinario fichero1 [fichero2 ...]
::   Ejemplo: 90-BorrarBinario SixLabors.ImageSharp.dll MailKit.dll
:: Solo borra en la raíz de cada directorio y no admite rutas ni comodines, para no borrar de más por error.
:: Ejecútalo con los sitios parados (después del 01-Descomprimir y antes del 02-Desplegar): así IIS no tiene
:: los ficheros bloqueados y el 02 copia el deps.json nuevo, que ya no los referencia, antes de arrancar.

set ORIGEN=C:\Prepublicacion\Se
set DIR1=C:\inetpub\Acromur
set DIR2=C:\inetpub\secify
set DIR3=C:\inetpub\test
set DIR4=C:\inetpub\arkenos
set DIR5=C:\inetpub\Caifantasia
set DIR6=C:\inetpub\interiorismo
set DIR7=C:\inetpub\femdek
set DIR8=C:\inetpub\futurines
set DIR9=C:\inetpub\albertomartinez
set DIR10=C:\inetpub\ceom
set DIR11=C:\inetpub\jhm

set DIRECTORIOS="%ORIGEN%" "%DIR1%" "%DIR2%" "%DIR3%" "%DIR4%" "%DIR5%" "%DIR6%" "%DIR7%" "%DIR8%" "%DIR9%" "%DIR10%" "%DIR11%"

if "%~1"=="" goto :uso

:: Verificar privilegios de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

:: Validar los parámetros: solo nombres de fichero, sin rutas ni comodines
set "FICHEROS="
:leerParametros
if "%~1"=="" goto :buscar
echo(%~1| findstr /r /c:"[*?\\/:]" /c:"\.\." >nul
if not errorlevel 1 (
    echo ERROR: '%~1' no es valido, indica solo el nombre del fichero, sin rutas ni comodines.
    pause
    exit /b 1
)
set FICHEROS=!FICHEROS! "%~1"
shift
goto :leerParametros

:buscar
for %%D in (%DIRECTORIOS%) do (
    if not exist "%%~D\" echo AVISO: no existe el directorio %%~D
)

echo.
echo Ficheros encontrados:
set /a ENCONTRADOS=0
for %%F in (%FICHEROS%) do (
    set /a DE_ESTE=0
    for %%D in (%DIRECTORIOS%) do (
        if exist "%%~D\%%~F" (
            for %%A in ("%%~D\%%~F") do set "ATRIBUTOS=%%~aA"
            if /i "!ATRIBUTOS:~0,1!"=="d" (
                echo   AVISO: %%~D\%%~F es un directorio, no se borra
            ) else (
                echo   %%~D\%%~F
                set /a DE_ESTE+=1
                set /a ENCONTRADOS+=1
            )
        )
    )
    if !DE_ESTE!==0 echo   AVISO: %%~F no esta en ningun directorio
)

if %ENCONTRADOS%==0 (
    echo.
    echo No hay nada que borrar.
    pause
    exit /b 0
)

echo.
set "RESPUESTA="
set /p "RESPUESTA=Se van a borrar %ENCONTRADOS% ficheros. Continuar? (S/N): "
if /i not "%RESPUESTA%"=="S" (
    echo Cancelado, no se ha borrado nada.
    pause
    exit /b 0
)

echo.
set /a BORRADOS=0
set /a FALLIDOS=0
for %%F in (%FICHEROS%) do (
    for %%D in (%DIRECTORIOS%) do (
        if exist "%%~D\%%~F" (
            for %%A in ("%%~D\%%~F") do set "ATRIBUTOS=%%~aA"
            if /i not "!ATRIBUTOS:~0,1!"=="d" (
                del /f /q "%%~D\%%~F" >nul 2>&1
                if exist "%%~D\%%~F" (
                    echo   NO SE PUDO BORRAR: %%~D\%%~F - bloqueado o sin permisos
                    set /a FALLIDOS+=1
                ) else (
                    echo   Borrado: %%~D\%%~F
                    set /a BORRADOS+=1
                )
            )
        )
    )
)

echo.
echo Borrados: %BORRADOS%   Fallidos: %FALLIDOS%
pause
if %FALLIDOS% gtr 0 exit /b 1
exit /b 0

:uso
echo Uso:     90-BorrarBinario fichero1 [fichero2 ...]
echo Ejemplo: 90-BorrarBinario SixLabors.ImageSharp.dll MailKit.dll
pause
exit /b 1
