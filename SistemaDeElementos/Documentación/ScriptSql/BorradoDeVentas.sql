
begin
declare @idnegocio int

-------------------------------------------------------------------------------------------
set @idnegocio =  (select id from NEGOCIO.NEGOCIO where ENUMERADO like 'PlanificacionDeVenta')
delete from venta.PLANIFICACION_VENTA_AGENDA_EVENTO
delete from venta.PLANIFICACION_VENTA_historia
delete from venta.PLANIFICACION_VENTA_linea
delete from venta.PLANIFICACION_VENTA_TRAZA
delete from venta.PLANIFICACION_VENTA_OBSERVACION
delete from VENTA.planificacion_venta
delete from ENTORNO.AGENDA_EVENTO where ID_NEGOCIO = @idnegocio
delete from [NEGOCIO].[LIBRO] where ID_NEGOCIO = @idnegocio
update JURIDICO.PLANIFICADOR_VENTA set GENERADO = 0
update JURIDICO.CONTRATO_AVANCE set PLANIFICADO = 0, REALIZADO = 0, FACTURADO = 0

-------------------------------------------------------------------------------------------
set @idnegocio =  (select id from NEGOCIO.NEGOCIO where ENUMERADO like 'ParteDeTrabajo')
delete from venta.PARTE_TR_AGENDA_EVENTO
delete from venta.PARTE_TR_historia
delete from venta.PARTE_TR_linea
delete from venta.PARTE_TR_TRAZA
delete from venta.PARTE_TR_ASIGNACION
delete from venta.PARTE_TR_ARCHIVO
delete from venta.PARTE_TR_TAREA 
delete from venta.PARTE_TR_OBSERVACION
update venta.FACTURA_EMT set id_parte_tr = null
update venta.FACTURA_EMT_LINEA set id_parte_tr = null

delete from VENTA.PARTE_TR
delete from ENTORNO.AGENDA_EVENTO where ID_NEGOCIO = @idnegocio
delete from [NEGOCIO].[LIBRO] where ID_NEGOCIO = @idnegocio

-------------------------------------------------------------------------------------------
set @idnegocio =  (select id from NEGOCIO.NEGOCIO where ENUMERADO like 'FacturaEmitida')
delete from venta.FACTURA_EMT_AGENDA_EVENTO
delete from venta.FACTURA_EMT_historia
delete from venta.FACTURA_EMT_linea
delete from venta.FACTURA_EMT_TRAZA
delete from venta.FACTURA_EMT_ARCHIVO
delete from venta.FACTURA_EMT_OBSERVACION
delete from VENTA.FACTURA_EMT
delete from ENTORNO.AGENDA_EVENTO where ID_NEGOCIO = @idnegocio
delete from [NEGOCIO].[LIBRO] where ID_NEGOCIO = @idnegocio

end