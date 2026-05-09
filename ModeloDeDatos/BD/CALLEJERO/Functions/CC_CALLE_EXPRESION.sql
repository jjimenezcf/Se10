 

CREATE FUNCTION [CALLEJERO].[CC_CALLE_EXPRESION] (@NOMBRE VarChar(250), @ID_MUNICIPIO int, @ID_TIPO_DE_VIA int)
RETURNS VarChar(2000)
AS
begin
  declare @municipio varchar(250)
  declare @tipoVia varchar(250)
  declare @resultado VARCHAR(250)
  
  select @municipio = NOMBRE from callejero.MUNICIPIO where id = @ID_MUNICIPIO
  select @tipoVia = NOMBRE from callejero.TIPO_VIA id where id = @ID_TIPO_DE_VIA
  
  select @resultado = @tipoVia + ' ' + @NOMBRE + ' (' + @municipio + ')'
  
  return @resultado
END
