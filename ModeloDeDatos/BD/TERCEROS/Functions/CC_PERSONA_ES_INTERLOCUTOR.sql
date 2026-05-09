CREATE FUNCTION [TERCEROS].[CC_PERSONA_ES_INTERLOCUTOR] (@ID int)
                RETURNS BIT
                AS
                begin
                  declare @resultado BIT

                  set @resultado = (select count(*)
                  from TERCEROS.INTERLOCUTOR 
                  where ID_PERSONA = @ID)

                  return @resultado
                END
