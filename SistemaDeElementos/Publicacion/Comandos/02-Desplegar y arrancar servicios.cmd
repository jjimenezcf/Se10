@echo off

set ORIGEN=C:\Prepublicacion\Se
set DIR1=C:\inetpub\Acromur
set DIR2=C:\inetpub\secify
set DIR3=C:\inetpub\test
set DIR4=C:\inetpub\arkenos
set DIR5=C:\inetpub\Caifantasia
set DIR6=C:\inetpub\interiorismo
set DIR7=C:\inetpub\femdek
set DIR8=C:\inetpub\futurines


echo empezando la copia...

rem Copy files to directories
xcopy /s /e /y "%ORIGEN%" "%DIR1%" > C:\Prepublicacion\99-Acromur.txt
xcopy /s /e /y "%ORIGEN%" "%DIR2%" > C:\Prepublicacion\99-secify.txt
xcopy /s /e /y "%ORIGEN%" "%DIR3%" > C:\Prepublicacion\99-test.txt
xcopy /s /e /y "%ORIGEN%" "%DIR4%" > C:\Prepublicacion\99-arkenos.txt
xcopy /s /e /y "%ORIGEN%" "%DIR5%" > C:\Prepublicacion\99-caifantasia.txt
xcopy /s /e /y "%ORIGEN%" "%DIR6%" > C:\Prepublicacion\99-interiorismo.txt
xcopy /s /e /y "%ORIGEN%" "%DIR7%" > C:\Prepublicacion\99-femdek.txt
xcopy /s /e /y "%ORIGEN%" "%DIR8%" > C:\Prepublicacion\99-futurines.txt


echo analizando copias
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-Acromur.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-secify.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-test.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-arkenos.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-caifantasia.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-interiorismo.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-femdek.txt
findstr /i /c:"0 File(s) copied" /c:"Infracción al compartir" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-futurines.txt

echo Iniciando servicios..
rem Start services
powershell -command "Start-WebSite Acromur"
powershell -command "Start-WebSite secify"
powershell -command "Start-WebSite test.se"
powershell -command "Start-WebSite arkenos"
powershell -command "Start-WebSite caifantasia"
powershell -command "Start-WebSite interiorismo"
powershell -command "Start-WebSite femdek"
powershell -command "Start-WebSite futurines"


echo Arrancando grupo de aplicaciones...
powershell -command "Start-WebAppPool -Name acromur"
powershell -command "Start-WebAppPool -Name secify"
powershell -command "Start-WebAppPool -Name test.se"
powershell -command "Start-WebAppPool -Name arkenos"
powershell -command "Start-WebAppPool -Name caifantasia"
powershell -command "Start-WebAppPool -Name interiorismo"
powershell -command "Start-WebAppPool -Name femdek"
powershell -command "Start-WebAppPool -Name futurines"

echo liberar colas


set "SQLCMD=sqlcmd -S .\SQLEXPRESS -E"


for %%D in (SE_ACROMUR SE_SECIFY SE_TEST SE_ARKENOS SE_CAIFANTASIA SE_INTERIORISMO SE_FEMDEK) do (
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
