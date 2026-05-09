
declare @id int
select @id=id from trabajo.trabajo where nombre like 'Notificar que se ha sobrepasado el porcentaje de aviso'

delete from trabajo.ERROR where ID_TRABAJO_USUARIO in (SELECT id FROM TRABAJO.usuario where ID_TRABAJO=@id )
delete from trabajo.TRAZA where ID_TRABAJO_USUARIO in (SELECT id FROM TRABAJO.usuario where ID_TRABAJO=@id )
delete FROM TRABAJO.usuario where ID_TRABAJO=@id
delete FROM TRABAJO.TRABAJO where ID=@id