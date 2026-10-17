@echo off

set ORIGEN=C:\Prepublicacion\Se
set DIR9=C:\inetpub\ceom


echo empezando la copia...

rem Copy files to directories
xcopy /s /e /y "%ORIGEN%" "%DIR9%" > C:\Prepublicacion\99-ceom.txt


echo analizando copias
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-ceom.txt

echo Iniciando servicios..
rem Start services
powershell -command "Start-WebSite ceom"


echo Arrancando grupo de aplicaciones...
powershell -command "Start-WebAppPool -Name ceom"

echo liberar colas


set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"


for %%D in (SE_CEOM) do (
    %SQLCMD% -d %%D -Q "USE %%D; EXEC [dbo].[Desbloquear_Cola]" | findstr /v "^$" | findstr /v /r "^-*$" | findstr /v "Result" > output_%%D.txt
    type output_%%D.txt
    findstr /i "ERROR" output_%%D.txt > nul && goto :error
)


pause
exit /b 0

:error
echo Se produjo un error al ejecutar el procedimiento almacenado: Desbloquear_Cola
pause
exit /b 1
