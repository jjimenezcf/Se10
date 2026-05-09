CREATE FUNCTION [TAREA].[CC_TAREA_NOMBRE] (@ID_ELEMENTO int)
            RETURNS VarChar(250)
            AS
            begin
              declare @resultado VARCHAR(250)
              
              select @resultado = '('+ REFERENCIA +') ' + NOMBRE from TAREA.TAREA where id = @ID_ELEMENTO
              return @resultado
            END