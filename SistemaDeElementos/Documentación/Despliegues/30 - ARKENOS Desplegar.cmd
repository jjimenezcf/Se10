@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)


echo Ejecutando procedimientos almacenados...

set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E -h -1 -W"

for %%D in (SE_ARKENOS) do (
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Bloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)

echo Todos los procedimientos almacenados se ejecutaron correctamente.


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

powershell -command "Stop-WebSite arkenos" 

echo Parando grupo de aplicaciones...
powershell -command "Stop-WebAppPool -Name arkenos"

echo Listo para la copia...

pause
exit /b 0

:error
echo Se produjo un error al ejecutar el procedimiento almacenado: Bloquear_Cola
pause
exit /b 1

