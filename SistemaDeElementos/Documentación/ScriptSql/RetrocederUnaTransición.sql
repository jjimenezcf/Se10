-------------------------------------------------------------------------------------------------------------------
-- Retroceder transición de factura emitida
-------------------------------------------------------------------------------------------------------------------
begin transaction
    declare @id int = 5
    
    delete 
    from venta.FACTURA_EMT_HISTORIA 
    where id = (select max(id) from venta.FACTURA_EMT_HISTORIA where ID_ELEMENTO = 5)
    
    update venta.FACTURA_EMT_HISTORIA
    set ID_TRANSICION = null,
        TIEMPO = null 
    where id =  (select max(id) from venta.FACTURA_EMT_HISTORIA where ID_ELEMENTO = 5)
    
    update venta.FACTURA_EMT 
    set ID_ESTADO =  (select id_estado 
                      from venta.FACTURA_EMT_HISTORIA 
                      where id = (select max(id) from venta.FACTURA_EMT_HISTORIA where ID_ELEMENTO = 5))
    where id = 5
rollback

-------------------------------------------------------------------------------------------------------------------
-- Retroceder transición de factura recibida
-------------------------------------------------------------------------------------------------------------------
begin
   declare @idelemento int = 6
   declare @idestado int
   set @idestado = (select top(1) id_estado
                    from (
                    select top(2) * from gasto.FACTURA_REC_HISTORIA where ID_ELEMENTO = @idelemento order by id desc
                    ) t1 order by  id asc)
   update GASTO.FACTURA_REC set ID_ESTADO = @idestado where id = @idelemento
   delete gasto.FACTURA_REC_HISTORIA where ID_ELEMENTO = @idelemento and ID_TRANSICION is null
   update GASTO.FACTURA_REC_HISTORIA set ID_TRANSICION = null, tiempo = null 
   where id = (select max(id) from GASTO.FACTURA_REC_HISTORIA where ID_ELEMENTO = @idelemento and ID_ESTADO = @idestado)
end

-------------------------------------------------------------------------------------------------------------------
-- Retroceder transición de pago emitido
-------------------------------------------------------------------------------------------------------------------
begin
  declare @idelemento int = 177
  declare @idestado int
  set @idestado = (select top(1) id_estado
                   from (
                   select top(2) * from gasto.PAGO_HISTORIA where ID_ELEMENTO = @idelemento order by id desc
                   ) t1 order by  id asc)
  update GASTO.PAGO set ID_ESTADO = @idestado where id = @idelemento
  delete gasto.PAGO_HISTORIA where ID_ELEMENTO = @idelemento and ID_TRANSICION is null
  update GASTO.PAGO_HISTORIA set ID_TRANSICION = null, tiempo = null 
  where id = (select max(id) from GASTO.PAGO_HISTORIA where ID_ELEMENTO = @idelemento and ID_ESTADO = @idestado)
end

-------------------------------------------------------------------------------------------------------------------
-- consultas varias -----------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------
select ID_ESTADO, * from venta.FACTURA_EMT

select ID_ESTADO, * from venta.FACTURA_EMT_HISTORIA order by id desc

select * from venta.REMESA_FAE_FACTURA_EMT

update  venta.REMESA_FAE_FACTURA_EMT
 set DEVUELTA_EL = '2023-09-27 00:00:00.0000000'
 where id = 2

 
update  venta.REMESA_FAE_FACTURA_EMT
 set CARGADA_EL = '2023-05-27 00:00:00.0000000'
 where id = 2

 select * from GASTO.REMESA_PAG
 
select * from GASTO.FACTURA_REC_HISTORIA where ID_ELEMENTO = 6 order by id desc

update  GASTO.REMESA_PAG_PAGO set PAGADO_EL = null, PAGAR_EL = '2023-08-01' where ID_ELEMENTO = 3

select * from gasto.PAGO where id in (select id_pago from GASTO.REMESA_PAG_PAGO where ID_ELEMENTO = 3)