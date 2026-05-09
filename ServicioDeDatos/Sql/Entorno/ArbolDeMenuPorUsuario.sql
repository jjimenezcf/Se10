
drop FUNCTION [ENTORNO].[ARBOL_MENU_POR_USUARIO] 
go

CREATE FUNCTION [ENTORNO].[ARBOL_MENU_POR_USUARIO] 
(
  @IDUSUARIO INT	
)
RETURNS 
@MenusAccedidos TABLE 
  (ID			int 
   ,NOMBRE		varchar(250)
   ,DESCRIPCION	varchar(2000)
   ,ICONO		varchar(250)
   ,ACTIVO		bit
   ,IDPADRE		int
   ,IDVISTA_MVC int
   ,ORDEN		int)
AS
BEGIN

    declare @idpadre int

    insert @MenusAccedidos
    SELECT T1.ID, T1.NOMBRE, T1.DESCRIPCION, T1.ICONO, T1.ACTIVO, T1.IDPADRE,  T1.IDVISTA_MVC, T1.ORDEN  
    from ENTORNO.MENU t1
    inner join ENTORNO.VISTA_MVC t2 on t2.id = t1.IDVISTA_MVC
    where t1.IDVISTA_MVC is not null
	  and t1.ACTIVO = 1
      and exists (select 1 from ENTORNO.USU_PERMISO where IDUSUA = @IDUSUARIO and IDPERMISO = t2.IDPERMISO)
    

    declare menus  CURSOR for select IDPADRE from @MenusAccedidos 
       
    open menus
    	FETCH menus INTO @idpadre
    	WHILE (@@fetch_status = 0) 
    	BEGIN
    	 insert into @MenusAccedidos  
    	 select T1.ID, T1.NOMBRE, T1.DESCRIPCION, T1.ICONO, T1.ACTIVO, T1.IDPADRE,  T1.IDVISTA_MVC, T1.ORDEN
    	 from ENTORNO.MENU t1
    	 where id = @idpadre
    	   and id not in (select id from @MenusAccedidos)
    	   
    	 FETCH menus INTO @idpadre
    	end
    
    CLOSE menus
    DEALLOCATE menus    
    
	RETURN 
END
GO

select * from ENTORNO.ARBOL_MENU_POR_USUARIO(10)
go