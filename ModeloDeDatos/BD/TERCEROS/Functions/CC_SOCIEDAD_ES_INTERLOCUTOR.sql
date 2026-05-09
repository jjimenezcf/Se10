CREATE FUNCTION [TERCEROS].[CC_SOCIEDAD_ES_INTERLOCUTOR] (@ID int)
                RETURNS BIT
                AS
                begin
                  declare @resultado BIT

                  set @resultado = (select count(*)
                  from TERCEROS.INTERLOCUTOR 
                  where ID_SOCIEDAD = @ID and ID_CONTACTO IS NULL)

                  return @resultado
                END
