select * from seguridad.permiso

begin transaction

declare @idvista int
declare @idtipo int
declare @idclase int
declare @idpermiso int
declare @vista varchar(250)
declare @permiso varchar(250)

select id, nombre from ENTORNO.VISTA_MVC where IDPERMISO is null

declare vistas CURSOR for select id, nombre from ENTORNO.VISTA_MVC where IDPERMISO is null

set @idtipo = (select id from SEGURIDAD.TIPO_PERMISO tp  where tp.NOMBRE  like 'Acceso')
set @idclase = (select id from SEGURIDAD.CLASE_PERMISO cp where cp.NOMBRE like 'Vista')
open vistas
	FETCH vistas INTO @idvista, @vista
	WHILE (@@fetch_status = 0) 
	BEGIN
	   set @permiso = 'VISTA: ' + @vista
	   

	   insert into SEGURIDAD.PERMISO (NOMBRE, IDTIPO, IDCLASE) values(@permiso,@idtipo, @idclase)
	   set @idpermiso = @@identity
	   UPDATE ENTORNO.VISTA_MVC set IDPERMISO = @idpermiso where id = @idvista
	   
	 FETCH vistas INTO @idvista, @vista
	end

CLOSE vistas
DEALLOCATE vistas

select * from ENTORNO.VISTA_MVC 

commit
