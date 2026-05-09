CREATE FUNCTION [TERCEROS].[CC_INTERLOCUTOR_NOMBRE] (@ID int)
                RETURNS VarChar(600)
                AS
                begin
                  declare @resultado VARCHAR(600)

                  select @resultado = '('+ T1.NIF +') ' + T1.APELLIDOS +', ' + T1.NOMBRE 
                  from TERCEROS.INTERLOCUTOR T0
                  INNER JOIN TERCEROS.PERSONA T1 ON T1.ID = T0.ID_PERSONA where t0.ID = @ID

                  return @resultado
                END
