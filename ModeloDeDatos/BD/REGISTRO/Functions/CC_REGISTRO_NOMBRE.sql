CREATE FUNCTION [REGISTRO].[CC_REGISTRO_NOMBRE] (@ID_ELEMENTO int)
            RETURNS VarChar(250)
            AS
            begin
              declare @resultado VARCHAR(250)
              
              select @resultado = '('+ REFERENCIA +') ' + NOMBRE from REGISTRO.REGISTRO where id = @ID_ELEMENTO
              return @resultado
            END
