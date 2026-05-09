
begin

delete from venta.FACTURA_EMT_ACCION
delete from venta.FACTURA_EMT_TRANSICION
update JURIDICO.CONTRATO_TIPO set ID_TIPO_FACTURA = null
update PRESUPUESTO.PRESUPUESTO_TIPO set ID_TIPO_FACTURA = null
update VENTA.PARTE_TR_TIPO set ID_TIPO_FACTURA = null
delete from venta.FACTURA_EMT_TIPO
delete from venta.FACTURA_EMT_ESTADO

end