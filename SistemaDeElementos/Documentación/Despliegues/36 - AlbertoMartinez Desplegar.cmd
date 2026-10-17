@echo off


net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)

echo Ejecutando procedimientos almacenados...

set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"

for %%D in (AB_ALBERTO_MARTINEZ) do (
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Bloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)

echo Todos los procedimientos almacenados se ejecutaron correctamente.


echo descomprimiendo Zip.

set ZIPFILE=C:\Prepublicacion\Publicacion.ZIP
set DESTINATION=C:\Prepublicacion\Se
set ORIGEN=C:\Prepublicacion\Se

rmdir /s /q "%ORIGEN%"


rem Unzip the file
powershell -NoProfile -Command "$ProgressPreference='SilentlyContinue'; try { Expand-Archive -LiteralPath '%ZIPFILE%' -DestinationPath '%DESTINATION%' -Force -ErrorAction Stop } catch { Write-Host $_.Exception.Message; exit 1 }"
if %errorlevel% neq 0 (
  echo Unzipping failed. Exiting.
  exit /b 1
)

echo Parando servicios...
powershell -command "Stop-WebSite albertomartinez" 


echo Parando grupo de aplicaciones...
powershell -command "Stop-WebAppPool -Name albertomartinez"


REM Lista de nombres de grupos de aplicaciones
set "APPOOLS=albertomartinez"

for %%A in (%APPOOLS%) do (
    echo Configurando grupo de aplicaciones: %%A
    %systemroot%\system32\inetsrv\AppCmd.exe set apppool /apppool.name:"%%A" /processModel.idleTimeout:00:00:00
    %systemroot%\system32\inetsrv\AppCmd.exe set apppool /apppool.name:"%%A" /processModel.idleTimeoutAction:Suspend
    %systemroot%\system32\inetsrv\AppCmd.exe set apppool /apppool.name:"%%A" /startMode:AlwaysRunning
)

echo listo para la copia...


pause
exit /b 0

:error
echo Se produjo un error al ejecutar el procedimiento almacenado. Abortando el despliegue.
pause
exit /b 1
