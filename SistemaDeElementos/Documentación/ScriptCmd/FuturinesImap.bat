@echo off
if "%~1"=="" (
    echo.
    echo ERROR: Debes pasar la clave como parametro.
    echo Uso: FuturinesImap.bat ^<clave^>
    echo Ejemplo: FuturinesImap.bat xxxxxx
    echo.
    pause
    exit /b 1
)
powershell -ExecutionPolicy Bypass -File "%~dp0TestConexionImap.ps1" -Usuario "futurines@gmx.es" -Clave "%~1" -Host1 "imap.gmx.com"
pause
