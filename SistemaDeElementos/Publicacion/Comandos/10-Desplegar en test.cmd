@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requires privilegios de administrador.
  pause
  exit /b 1
)

echo Parando servicios...

powershell -command "Stop-WebSite test.se"  

echo Parando grupo de aplicaciones...

powershell -command "Stop-WebAppPool -Name test.se"



echo servicios y grupos de aplicaciones parados

set ZIPFILE=C:\Prepublicacion\Publicacion.ZIP
set DESTINATION=C:\Prepublicacion\Se
set ORIGEN=C:\Prepublicacion\Se
set DIR2=C:\inetpub\test

rmdir /s /q "%ORIGEN%"


rem Unzip the file
powershell -command "Expand-Archive -Path '%ZIPFILE%' -DestinationPath '%DESTINATION%'"
if %errorlevel% neq 0 (
  echo Unzipping failed. Exiting.
  exit /b 1
)



echo empezando la copia...

rem Copy files to directories
xcopy /s /e /y "%ORIGEN%" "%DIR2%" > C:\Prepublicacion\99-test.txt

																						  
echo analizando copia de test                                                               
findstr /i /c:"0 File(s) copied" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-test.txt
	
echo Iniciando servicios..
powershell -command "Start-WebSite test.se"


echo Arrancando grupo de aplicaciones...
powershell -command "Start-WebAppPool -Name test.se"


echo Arrancando colas...
echo schtasks /change /ENABLE /TN "Cola test

echo Finalizado

pause

