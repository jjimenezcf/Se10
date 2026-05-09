@echo off


set webconfig=C:\Prepublicacion\Web.config
if exist %webconfig% (
	del %webconfig%
	echo "%webconfig% borrado"
) else (
	echo "%webconfig% no existe"
)
set appsettings=C:\Prepublicacion\appsettings.json
if exist %appsettings% (
	del %appsettings%
	echo "%appsettings% borrado"
) else (
	echo "%appsettings% no existe"
)

set directorios=C:\Prepublicacion\zh-Hant
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\zh-Hans
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\tr
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\ru
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\pt-BR
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\pl
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\ko
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\ja
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\it
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\fr
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\es
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\de
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\nl
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\cs
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)
set directorios=C:\Prepublicacion\wwwroot\Archivos
if exist %directorios% (
	RMDIR /S /Q %directorios% 
)

del C:\Prepublicacion\*.pdb
del C:\Prepublicacion\appsettings.Development.json
del C:\Prepublicacion\createdump.exe


set ficheros=C:\Prepublicacion\*.dll

for %%a in (%ficheros%) do Call :borrado %%a %%~ta

echo Fin de borrado %date% - %time%



set rutaPsTool=c:\PSTool\
set ruta=C:\Prepublicacion\
set nombreZip=Publicacion.zip

set execZip=%rutaPsTool%zip.exe


echo Inicio zip 
del %ruta%%nombreZip%
echo borrar %ruta%%nombreZip%

echo ruta de zip %execZip% 

%execZip% -q -r %ruta%%nombreZip% %ruta% -i "*.*"
echo comprimido %ruta%%nombreZip%


echo Fin zip


goto:eof
 
:borrado
Set Orig=%1
if Not %Orig% == C:\Prepublicacion\SistemaDeElementos.dll (
if Not %Orig% == C:\Prepublicacion\GestoresDeNegocio.dll  (
if Not %Orig% == C:\Prepublicacion\GestorDeElementos.dll  (
if Not %Orig% == C:\Prepublicacion\Utilidades.dll 		  (
if Not %Orig% == C:\Prepublicacion\Inicializador.dll 	  (
if Not %Orig% == C:\Prepublicacion\ModeloDeDto.dll 		  (
if Not %Orig% == C:\Prepublicacion\ServicioDeDatos.dll 	  (
if Not %Orig% == C:\Prepublicacion\ServicioDeReportes.dll (
if Not %Orig% == C:\Prepublicacion\ServicioDeCorreos.dll (
if Not %Orig% == C:\Prepublicacion\GestorDeErrores.dll (
if Not %Orig% == C:\Prepublicacion\Ical.Net.dll (
if Not %Orig% == C:\Prepublicacion\NodaTime.dll (
if Not %Orig% == C:\Prepublicacion\EPPlus.dll (
if Not %Orig% == C:\Prepublicacion\EPPlus.Interfaces.dll (
if Not %Orig% == C:\Prepublicacion\EPPlus.System.Drawing.dll (
if Not %Orig% == C:\Prepublicacion\Microsoft.IO.RecyclableMemoryStream.dll (
if Not %Orig% == C:\Prepublicacion\QuestPDF.dll (
if Not %Orig% == C:\Prepublicacion\SkiaSharp.dll (
if Not %Orig% == C:\Prepublicacion\ServicioSepa.dll (
if Not %Orig% == C:\Prepublicacion\Migraciones.dll (
if Not %Orig% == C:\Prepublicacion\libSkiaSharp.dll (
if Not %Orig% == C:\Prepublicacion\libHarfBuzzSharp.dll (
	Set Fech=%2
IF Not Fech == %Date% (
	del %orig% ) 
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)
)


