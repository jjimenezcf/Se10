begin

declare @idTipoObra int
set @idTipoObra = (select id from expediente.expediente_tipo where nombre like 'OBR: Obra')

update presupuesto.presupuesto set id_Expediente = null
delete from Expediente.expediente_historia where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.expediente_observacion where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_AUDITORIA where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_DIRECCION where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_PERMISO where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_PLEITO where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_TRAZA where id_elemento in (select id from Expediente.expediente where id_tipo = @idTipoObra)


delete from Expediente.EXPEDIENTE_AGENDA_EVENTO where ID_ELEMENTO1 in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_ARCHIVADOR where ID_ELEMENTO1 in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_ARCHIVO where ID_ELEMENTO1 in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_INTERLOCUTOR where ID_ELEMENTO1 in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_REGISTRO where ID_ELEMENTO1 in (select id from Expediente.expediente where id_tipo = @idTipoObra)
delete from Expediente.EXPEDIENTE_TAREA where ID_ELEMENTO1 in (select id from Expediente.expediente where id_tipo = @idTipoObra)


delete from Expediente.expediente where id_tipo = @idTipoObra

delete from EXPEDIENTE.EXPEDIENTE_ACCION where ID_TRANSICION in  (select id from EXPEDIENTE.EXPEDIENTE_transicion where nombre like 'OBR: %')
delete from EXPEDIENTE.EXPEDIENTE_transicion where nombre like 'OBR: %'
delete from EXPEDIENTE.EXPEDIENTE_TIPO where nombre like 'OBR: %'
delete from EXPEDIENTE.EXPEDIENTE_Estado where nombre like 'OBR: %'

end