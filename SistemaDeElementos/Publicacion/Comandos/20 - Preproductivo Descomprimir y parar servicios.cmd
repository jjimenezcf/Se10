@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requiere privilegios de administrador.
  pause
  exit /b 1
)


echo Ejecutando procedimientos almacenados...

set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E -h -1 -W"

for %%D in (SE_TEST) do (
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Bloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)

echo Todos los procedimientos almacenados se ejecutaron correctamente.


echo Parando servicios...

powershell -command "Stop-WebSite test.se" 

echo Parando grupo de aplicaciones...
powershell -command "Stop-WebAppPool -Name test.se"

echo Listo para la copia...

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
