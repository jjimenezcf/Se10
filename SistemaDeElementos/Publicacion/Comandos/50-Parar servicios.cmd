@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requires privilegios de administrador.
  pause
  exit /b 1
)

echo Parando servicios...

powershell -command "Stop-WebSite Acromur" 
powershell -command "Stop-WebSite femdek" 
powershell -command "Stop-WebSite test.se" 
powershell -command "Stop-WebSite arkenos" 

echo Parando grupo de aplicaciones...

powershell -command "Stop-WebAppPool -Name acromur"
powershell -command "Stop-WebAppPool -Name femdek"
powershell -command "Stop-WebAppPool -Name test.se"
powershell -command "Stop-WebAppPool -Name arkenos"



echo servicios y grupos de aplicaciones parados

pause

