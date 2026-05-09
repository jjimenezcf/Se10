CREATE FUNCTION [TERCEROS].[CC_SOCIEDAD_NOMBRE] (@id_elemento int)
RETURNS VarChar(250)
AS
begin
  declare @resultado VARCHAR(250)
  
  select @resultado = NOMBRE from TERCEROS.SOCIEDAD where id = @id_elemento
  return @resultado
END
