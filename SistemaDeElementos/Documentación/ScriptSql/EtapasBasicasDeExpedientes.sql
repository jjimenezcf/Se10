declare @idNegocio int
declare @iniciales varchar(max)
declare @terminados varchar(max)
declare @cancelados varchar(max)

set @idNegocio = (select id from negocio.NEGOCIO where nombre like '%expediente%')

set @cancelados = (SELECT  STRING_AGG(ID, ',') AS IDs FROM expediente.EXPEDIENTE_ESTADO WHERE CANCELADO = 1)
set @iniciales = (SELECT  STRING_AGG(ID, ',') AS IDs FROM expediente.EXPEDIENTE_ESTADO WHERE INICIAL = 1)
set @terminados = (SELECT  STRING_AGG(ID, ',') AS IDs FROM expediente.EXPEDIENTE_ESTADO WHERE TERMINADO = 1)


delete negocio.PARAMETRO where nombre like 'EXP_Etapa_Termninada'
delete negocio.PARAMETRO where nombre like 'EXP_Etapa_Cancelada'
delete negocio.PARAMETRO where nombre like 'EXP_Etapa_Inicial'

if @terminados is not null 
insert into NEGOCIO.PARAMETRO (VALOR, ID_NEGOCIO, NOMBRE) values (@terminados, @idNegocio, 'EXP_Etapa_Termninada')

if @cancelados is not null 
insert into NEGOCIO.PARAMETRO (VALOR, ID_NEGOCIO, NOMBRE) values (@cancelados, @idNegocio, 'EXP_Etapa_Cancelada')

if @iniciales is not null 
insert into NEGOCIO.PARAMETRO (VALOR, ID_NEGOCIO, NOMBRE) values (@iniciales, @idNegocio, 'EXP_Etapa_Inicial')

select * from negocio.PARAMETRO where ID_NEGOCIO = @idNegocio and nombre like 'EXP_Etapa_Termninada'
select * from negocio.PARAMETRO where ID_NEGOCIO = @idNegocio and nombre like 'EXP_Etapa_Cancelada'
select * from negocio.PARAMETRO where ID_NEGOCIO = @idNegocio and nombre like 'EXP_Etapa_Inicial'
