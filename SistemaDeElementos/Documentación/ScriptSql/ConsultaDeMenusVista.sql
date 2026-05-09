use SE_3
select t1.id
, t1.nombre as menu
, t2.NOMBRE as vista
, t1.ICONO
, t2.ELEMENTO_DTO
, * 
from ENTORNO.MENU t1
inner join ENTORNO.VISTA_MVC t2 on t2.id = t1.IDVISTA_MVC
where t1.nombre like '%plan%'
order by t1.id desc