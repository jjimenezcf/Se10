@echo off

net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Este script requires privilegios de administrador.
  pause
  exit /b 1
)

echo Arrancando servicios...

powershell -command "Start-WebSite Acromur" 
powershell -command "Start-WebSite femdek" 
powershell -command "Start-WebSite test.se" 
powershell -command "Start-WebSite arkenos" 

echo Arrancando grupo de aplicaciones...

powershell -command "Start-WebAppPool -Name acromur"
powershell -command "Start-WebAppPool -Name femdek"
powershell -command "Start-WebAppPool -Name test.se"
powershell -command "Start-WebAppPool -Name arkenos"



echo servicios y grupos de aplicaciones arrancados

pause

