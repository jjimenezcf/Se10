CREATE FUNCTION [NEGOCIO].[MODO_ACCESO_AL_NEGOCIO_POR_USUARIO] 
 (
  @IDNEGOCIO INT,
  @IDUSUARIO INT
 )
 RETURNS TABLE 
 AS
 RETURN 
 (
      select t1.ID, t1.ADMINISTRADOR, t1.GESTOR, t1.CONSULTOR, t2.IDUSUA, t2.IDPERMISO, t2.ORIGEN
      from (
             SELECT t1.ID, cast(1 as bit) as Administrador,  cast(0 as bit) as Gestor, cast(0 as bit) as Consultor
             FROM ENTORNO.USUARIO_PERMISO t1 with(noLock)
             inner join NEGOCIO.NEGOCIO t2 with(noLock) on t2.ID_ADM = t1.IDPERMISO 
             where t1.IDUSUA =  @IDUSUARIO
               and t2.ID = @IDNEGOCIO
             UNION 
             SELECT t1.ID, cast(0 as bit) as Administrador,  cast(1 as bit) as Gestor, cast(0 as bit) as Consultor
             FROM ENTORNO.USUARIO_PERMISO t1  with(noLock)
             inner join NEGOCIO.NEGOCIO t2 with(noLock) on t2.ID_GESTOR = t1.IDPERMISO 
             where t1.IDUSUA =  @IDUSUARIO
               and t2.ID = @IDNEGOCIO
             UNION 
             SELECT  t1.ID,  cast(0 as bit) as Administrador, cast(0 as bit) as Gestor,  cast(1 as bit) as Consultor
             FROM ENTORNO.USUARIO_PERMISO t1  with(noLock)
             inner join NEGOCIO.NEGOCIO t2  with(noLock) on t2.ID_CONSULTOR = t1.IDPERMISO 
             where t1.IDUSUA =  @IDUSUARIO
               and t2.ID = @IDNEGOCIO
        ) t1
      inner join ENTORNO.USUARIO_PERMISO t2 on t2.ID = t1.ID
 )
