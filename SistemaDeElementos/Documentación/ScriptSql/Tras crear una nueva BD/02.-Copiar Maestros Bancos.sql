insert into TERCEROS.BANCO(
   ID_PAIS
   ,BIC_SWIFT
   ,CODIGO
   ,NOMBRE
   ,FECCRE
   ,ID_CREADOR
   ,FECMOD
   ,ID_MODIFICADOR)
select 
   (select id from CALLEJERO.PAIS where nombre like 'ESPAÑA') as ID_PAIS
   ,BIC_SWIFT
   ,CODIGO
   ,entorno.CapitalizarFrase(NOMBRE)
   ,GETDATE()
   ,1
   ,null
   ,null
from SE_INTERIORISMO.TERCEROS.BANCO
where
CODIGO not in (select CODIGO from TERCEROS.BANCO)


select * from TERCEROS.BANCO

---------- cuentas contables

insert into CONTABILIDAD.CUENTA(
    CODIGO
   ,NOMBRE
   )
select 
   CODIGO
   , entorno.CapitalizarFrase(NOMBRE)
from SE_INTERIORISMO.CONTABILIDAD.CUENTA
where
CODIGO not in (select CODIGO from CONTABILIDAD.CUENTA) and
NOMBRE not in (select NOMBRE from CONTABILIDAD.CUENTA) 



------------ naturalezas

insert into MT.MT_NATURALEZA( SIGLA
   , ID_CUENTA_GASTO
   , ID_CUENTA_INGRESO
   ,NOMBRE
   )
select 
     sigla
   ,  (select id from CONTABILIDAD.CUENTA where codigo = (select codigo from SE_INTERIORISMO.CONTABILIDAD.CUENTA where id = t1.ID_CUENTA_GASTO)) as ID_CUENTA_GASTO
   ,  (select id from CONTABILIDAD.CUENTA where codigo = (select codigo from SE_INTERIORISMO.CONTABILIDAD.CUENTA where id = t1.ID_CUENTA_INGRESO)) as ID_CUENTA_INGRESO
   ,NOMBRE
from SE_INTERIORISMO.MT.MT_NATURALEZA t1
where
sigla not in (select sigla from MT.MT_NATURALEZA)


------------ Clase de permiso

insert into SEGURIDAD.CLASE_PERMISO(NOMBRE)
select 
     NOMBRE
from SE_INTERIORISMO.SEGURIDAD.CLASE_PERMISO t1
where
NOMBRE not in (select NOMBRE from SEGURIDAD.CLASE_PERMISO)

