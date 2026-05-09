begin
declare @idfae int
declare @idEsta int
set @idfae = (select id from VENTA.FACTURA_EMT where REFERENCIA like '2023-ACR-FAE-1')
delete VENTA.FACTURA_EMT_HISTORIA 
where ID_ELEMENTO = @idfae
and id <> (select top(1) id from VENTA.FACTURA_EMT_HISTORIA where ID_ELEMENTO = @idfae order by ID)

set @idEsta = (select top(1) ID_ESTADO from VENTA.FACTURA_EMT_HISTORIA where ID_ELEMENTO = @idfae )
update VENTA.FACTURA_EMT_HISTORIA set TIEMPO = null, ID_TRANSICION = null, ID_OBSERVACION = null where ID_ELEMENTO = @idfae
update VENTA.FACTURA_EMT set ID_ESTADO = @idEsta, NUMERO = null, FACTURADA_EL = null where id = @idfae
delete venta.FACTURA_EMT_ARCHIVO where ID_ELEMENTO1 = @idfae
end
