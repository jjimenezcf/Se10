@echo off


net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requires privilegios de administrador.
  pause
  exit /b 1
)

echo Ejecutando procedimientos almacenados...

set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"

for %%D in (SE_ACROMUR SE_SECIFY SE_TEST SE_ARKENOS SE_CAIFANTASIA SE_INTERIORISMO SE_FEMDEK) do (
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Bloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)

echo Todos los procedimientos almacenados se ejecutaron correctamente.

echo Parando servicios...

powershell -command "Stop-WebSite Acromur" 
powershell -command "Stop-WebSite secify" 
powershell -command "Stop-WebSite test.se" 
powershell -command "Stop-WebSite arkenos" 
powershell -command "Stop-WebSite caifantasia" 
powershell -command "Stop-WebSite interiorismo" 
powershell -command "Stop-WebSite femdek" 
powershell -command "Stop-WebSite futurines" 


echo Parando grupo de aplicaciones...
powershell -command "Stop-WebAppPool -Name acromur"
powershell -command "Stop-WebAppPool -Name secify"
powershell -command "Stop-WebAppPool -Name test.se"
powershell -command "Stop-WebAppPool -Name arkenos"
powershell -command "Stop-WebAppPool -Name caifantasia"
powershell -command "Stop-WebAppPool -Name interiorismo"
powershell -command "Stop-WebAppPool -Name femdek"
powershell -command "Stop-WebAppPool -Name futurines"

echo listo para la copia...

set ZIPFILE=C:\Prepublicacion\Publicacion.ZIP
set DESTINATION=C:\Prepublicacion\Se
set ORIGEN=C:\Prepublicacion\Se

rmdir /s /q "%ORIGEN%"


rem Unzip the file
powershell -command "Expand-Archive -Path '%ZIPFILE%' -DestinationPath '%DESTINATION%'"
if %errorlevel% neq 0 (
  echo Unzipping failed. Exiting.
  exit /b 1
)

pause
exit /b 0

:error
echo Se produjo un error al ejecutar el procedimiento almacenado. Abortando el despliegue.
pause
exit /b 1
