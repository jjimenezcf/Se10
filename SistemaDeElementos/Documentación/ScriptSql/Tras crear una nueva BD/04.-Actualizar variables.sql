
declare @sitioweb varchar(20) = '......';
declare @servidorDeCorreo varchar(30) = @sitioweb + '.gmx';
declare @logo varchar(30) = @sitioweb + '-logo.png';

update ENTORNO.VARIABLE set valor='C:\Temp\Trazas\'+ @sitioweb where NOMBRE like 'CFG_Ruta_Ficheros_De_Debug'
update ENTORNO.VARIABLE set valor ='c:\AlmacenDocumental\' + @sitioweb where NOMBRE like 'CFG_Servidor_Archivos'
update ENTORNO.VARIABLE set valor ='https://' + @sitioweb + '.femdek.com/' where NOMBRE like 'CFG_UrlBase'
update ENTORNO.VARIABLE set valor = @servidorDeCorreo where NOMBRE like 'CFG_Servidor_De_Correo'

update ENTORNO.VARIABLE set valor = @logo where NOMBRE like 'LYT_Imagen_De_Fondo'
update ENTORNO.VARIABLE set valor = '0.01' where NOMBRE like 'LYT_Opacidad'


select * from ENTORNO.VARIABLE

-- crear los directorios de trazas y de almacen documental ---
-- ejecutar y verificar que envía el correo de doble factor ---
-- crear el sitio web en IIS: ver Despliegues\CrearSitioWeb.ps1 y el manual Despliegues\Manual - Crear un sitio web nuevo.md
