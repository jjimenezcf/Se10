@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requires privilegios de administrador.
  pause
  exit /b 1
)

echo Parando servicios...

powershell -command "Stop-WebSite femdek"  

echo Parando grupo de aplicaciones...

powershell -command "Stop-WebAppPool -Name femdek"



echo servicios y grupos de aplicaciones parados

set ZIPFILE=C:\Prepublicacion\Publicacion.ZIP
set DESTINATION=C:\Prepublicacion\Se
set ORIGEN=C:\Prepublicacion\Se
set DIR2=C:\inetpub\femdek

rmdir /s /q "%ORIGEN%"


rem Unzip the file
powershell -command "Expand-Archive -Path '%ZIPFILE%' -DestinationPath '%DESTINATION%'"
if %errorlevel% neq 0 (
  echo Unzipping failed. Exiting.
  exit /b 1
)



echo empezando la copia...

rem Copy files to directories
xcopy /s /e /y "%ORIGEN%" "%DIR2%" > C:\Prepublicacion\99-femdek.txt

																						  
echo analizando copia de femdek                                                               
findstr /i /c:"0 File(s) copied" /c:"File not found" /c:" cannot " /c:" error " C:\Prepublicacion\99-femdek.txt
	
echo Iniciando servicios..
powershell -command "Start-WebSite femdek"


echo Arrancando grupo de aplicaciones...
powershell -command "Start-WebAppPool -Name femdek"



pause

