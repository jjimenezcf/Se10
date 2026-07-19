@echo off
if "%~1"=="" (
    echo.
    echo ERROR: Debes pasar la clave como parametro.
    echo Uso: SecifyImap.bat ^<clave^>
    echo Ejemplo: SecifyImap.bat xxxxxx
    echo.
    pause
    exit /b 1
)
powershell -ExecutionPolicy Bypass -File "%~dp0TestConexionImap.ps1" -Usuario "Secify@gmx.es" -Clave "%~1" -Host1 "imap.gmx.com"
pause
