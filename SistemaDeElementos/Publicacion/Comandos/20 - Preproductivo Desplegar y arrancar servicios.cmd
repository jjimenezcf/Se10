@echo off

set ORIGEN=C:\Prepublicacion\Se
set DIR3=C:\inetpub\test


echo empezando la copia...

rem Copy files to directories
xcopy /s /e /y "%ORIGEN%" "%DIR3%" > C:\Prepublicacion\99-test.txt


echo analizando copias
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-test.txt

echo Iniciando servicios..
rem Start services
powershell -command "Start-WebSite test.se"


echo Arrancando grupo de aplicaciones...
powershell -command "Start-WebAppPool -Name test.se"

echo Finalizado


set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"


for %%D in (SE_TEST) do (
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
