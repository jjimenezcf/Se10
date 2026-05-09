CREATE FUNCTION [ENTORNO].[CC_USUARIO_EXPRESION] (@id_creador int)
            RETURNS VarChar(250)
            AS
            begin
              declare @resultado VARCHAR(250)
              
              select @resultado = '('+ LOGIN +') ' + APELLIDO +' ' + NOMBRE from ENTORNO.USUARIO where id = @id_creador
              return @resultado
            END
