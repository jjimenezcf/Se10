use SE_3
begin

-- Estados de una factura y un parte
select  * from VENTA.FACTURA_EMT_ESTADO
select  * from VENTA.PARTE_TR_ESTADO 
/*
4	GEN: Pendiente
5	GEN: Realizado
6	GEN: Cancelado
7	GEN: Prefacturado
8	GEN: Facturado
*/

-- Mostrar prefacturas
select t1.ID_PARTE_TR, t1.ID_ESTADO, t2.nombre, * 
from VENTA.FACTURA_EMT t1
inner join VENTA.FACTURA_EMT_ESTADO t2 on t2.ID = t1.ID_ESTADO
where t1.ID_ESTADO = 6

-- Partes incluidos en prefacturas
select ID_FACTURA_EMT, * 
from VENTA.PARTE_TR 
where id in (select t1.ID_PARTE_TR
from VENTA.FACTURA_EMT t1
inner join VENTA.FACTURA_EMT_ESTADO t2 on t2.ID = t1.ID_ESTADO
where t1.ID_ESTADO = 6
)


select ID_PARTE_TR, * 
from VENTA.FACTURA_EMT_LINEA t1
where ID_ELEMENTO = 11


end
