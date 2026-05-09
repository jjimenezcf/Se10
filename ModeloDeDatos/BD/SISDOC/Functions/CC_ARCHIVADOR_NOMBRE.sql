
CREATE FUNCTION [SISDOC].[CC_ARCHIVADOR_NOMBRE] (@id_elemento int)
RETURNS VarChar(250)
AS
begin
  declare @resultado VARCHAR(250)
  
  select @resultado = NOMBRE from SISDOC.ARCHIVADOR where id = @id_elemento
  return @resultado
END
