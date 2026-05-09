
select * from VENTA.FACTURA_EMT where id = 266

select *  from SISDOC.ARCHIVO order by id desc

begin transaction
declare @id1 int 
declare @id2 int 

set @id1 = 4840
set @id2 = @id1 - 2

delete  from VENTA.FACTURA_EMT_ARCHIVO where ID_ELEMENTO2 in (@id1, @id2)
delete  from SISDOC.ARCHIVO_AUDITORIA where ID_ARCHIVO in (@id1, @id2)
delete  from SISDOC.Firmado where id_firmado in (@id1, @id2)
delete  from SISDOC.ARCHIVO where id in (@id1, @id2)

commit