
insert into CALLEJERO.codigo_postal(Cp)
select cp
from [SE_INTERIORISMO].CALLEJERO.codigo_postal
go

insert into CALLEJERO.TIPO_VIA(
SIGLA
,NOMBRE)
select sigla, nombre
from [SE_INTERIORISMO].CALLEJERO.TIPO_VIA
go


insert into CALLEJERO.pais (
NOMBRE_INGLES
,CODIGO
,ISO2
,PREFIJO
,NOMBRE
,FECCRE
,ID_CREADOR
,FECMOD
,ID_MODIFICADOR
,ES_UE)
select 
NOMBRE_INGLES
,CODIGO
,ISO2
,PREFIJO
,NOMBRE
,GETDATE()
,1
,null
,null
,ES_UE
from [SE_INTERIORISMO].CALLEJERO.PAIS
go

insert into CALLEJERO.proVINCIA(
CODIGO
,SIGLA
,PREFIJO
,ID_PAIS
,NOMBRE
,FECCRE
,ID_CREADOR
,FECMOD
,ID_MODIFICADOR)
select 
CODIGO
,SIGLA
,PREFIJO
,1
,NOMBRE
,getdate()
,1
,null
,null
from [SE_INTERIORISMO].CALLEJERO.PROVINCIA
go


-- copiar cps de una provincia
insert into CALLEJERO.PROVINCIA_CP(
ID_PROVINCIA
,ID_CP)
select
(select id from CALLEJERO.PROVINCIA where nombre like (select NOMBRE from [SE_INTERIORISMO].CALLEJERO.PROVINCIA where id = ID_PROVINCIA)) as id_provincia, 
(select id from CALLEJERO.CODIGO_POSTAL where CP like (select CP from [SE_INTERIORISMO].CALLEJERO.CODIGO_POSTAL where id = ID_CP)) as id_cp
from [SE_INTERIORISMO].CALLEJERO.PROVINCIA_CP
go

--copiar municipios
insert into
callejero.MUNICIPIO (
DC
,ID_PROVINCIA
,NOMBRE
,FECCRE
,ID_CREADOR
,FECMOD
,ID_MODIFICADOR
)
select
DC
,(select id from CALLEJERO.PROVINCIA where nombre like (select NOMBRE from [SE_INTERIORISMO].CALLEJERO.PROVINCIA where id = ID_PROVINCIA)) as id_provincia
,NOMBRE
,GETDATE()
,1
,null
,null
from SE_INTERIORISMO.CALLEJERO.MUNICIPIO
go

--copiar cps de un municipio
insert into CALLEJERO.MUNICIPIO_CP(
ID_MUNICIPIO
,ID_CP)
select
(select top(1) id from CALLEJERO.MUNICIPIO where nombre like (select NOMBRE from [SE_INTERIORISMO].CALLEJERO.MUNICIPIO where id = ID_MUNICIPIO)) as ID_MUNICIPIO, 
(select top(1) id from CALLEJERO.CODIGO_POSTAL where CP like (select CP from [SE_INTERIORISMO].CALLEJERO.CODIGO_POSTAL where id = ID_CP)) as id_cp
from [SE_INTERIORISMO].CALLEJERO.MUNICIPIO_CP

