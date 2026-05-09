CREATE FUNCTION [TERCEROS].[CC_PERSONA_NOMBRE] (@ID int)
                RETURNS VarChar(600)
                AS
                begin
                  declare @resultado VARCHAR(600)

                  select @resultado = '('+ NIF +') ' + APELLIDOS +', ' + NOMBRE from TERCEROS.PERSONA where id = @ID
                  return @resultado
                END
