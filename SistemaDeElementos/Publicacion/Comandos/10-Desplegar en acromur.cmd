@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requires privilegios de administrador.
  pause
  exit /b 1
)

echo Parando servicios...

powershell -command "Stop-WebSite acromur"  

echo Parando grupo de aplicaciones...

powershell -command "Stop-WebAppPool -Name acromur"



echo servicios y grupos de aplicaciones parados

echo Parando colas...

echo schtasks /end /TN "Cola acromur"
echo schtasks /change /DISABLE /TN "Cola acromur

echo colas paradas





set ZIPFILE=C:\Prepublicacion\Publicacion.ZIP
set DESTINATION=C:\Prepublicacion\Se
set ORIGEN=C:\Prepublicacion\Se
set DIR2=C:\inetpub\acromur

rmdir /s /q "%ORIGEN%"


rem Unzip the file
powershell -command "Expand-Archive -Path '%ZIPFILE%' -DestinationPath '%DESTINATION%'"
if %errorlevel% neq 0 (
  echo Unzipping failed. Exiting.
  exit /b 1
)



echo empezando la copia...

rem Copy files to directories
xcopy /s /e /y "%ORIGEN%" "%DIR2%" > C:\Prepublicacion\99-acromur.txt

																						  
echo analizando copia de acromur                                                               
findstr /i /c:"0 File(s) copied" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-acromur.txt
	
echo Iniciando servicios..
powershell -command "Start-WebSite acromur"


echo Arrancando grupo de aplicaciones...
powershell -command "Start-WebAppPool -Name acromur"


echo Arrancando colas...
echo schtasks /change /ENABLE /TN "Cola acromur

echo Finalizado

pause

