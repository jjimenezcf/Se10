CREATE FUNCTION [TERCEROS].[CC_SOCIEDAD_CONTACTO_ES_INTERLOCUTOR] (@ID int)
                RETURNS BIT
                AS
                begin
                  declare @resultado BIT

                  set @resultado = (select count(*)
                  from TERCEROS.INTERLOCUTOR 
                  where ID_CONTACTO = @ID)

                  return @resultado
                END
