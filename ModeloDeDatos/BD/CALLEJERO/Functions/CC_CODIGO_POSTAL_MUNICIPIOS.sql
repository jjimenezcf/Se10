CREATE FUNCTION [CALLEJERO].[CC_CODIGO_POSTAL_MUNICIPIOS] (@ID int)
RETURNS VarChar(250)
AS
begin
  declare @municipio VARCHAR(250)
  declare @resultado VARCHAR(250)

   DECLARE c CURSOR FOR SELECT t1.NOMBRE
   from CALLEJERO.MUNICIPIO t1
   inner join CALLEJERO.MUNICIPIO_CP t2 on t2.ID_MUNICIPIO = t1.ID
	where t2.ID_CP = @ID
   
   set @resultado = ''
   OPEN c
   FETCH NEXT FROM c INTO @municipio
   WHILE @@fetch_status = 0
   BEGIN
      if @resultado = ''  
         set  @resultado = @municipio 
   	  else 
         set  @resultado = @resultado + char(13) + @municipio 
      FETCH NEXT FROM c INTO @municipio
   END
   CLOSE c
   DEALLOCATE c
   return @resultado
END
