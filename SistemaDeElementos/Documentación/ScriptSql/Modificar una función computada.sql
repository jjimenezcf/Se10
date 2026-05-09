
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[TERCEROS].[PERSONA_OBSERVACION]') AND name = 'ELEMENTO')
    BEGIN
        ALTER TABLE [TERCEROS].[PERSONA_OBSERVACION] DROP COLUMN [ELEMENTO];
    END;
go

IF OBJECT_ID('[TERCEROS].[CC_PERSONA_NOMBRE]') IS NOT NULL
BEGIN
    DROP FUNCTION [TERCEROS].[CC_PERSONA_NOMBRE];
END
go

create FUNCTION [TERCEROS].[CC_PERSONA_NOMBRE] (@ID int)
                RETURNS VarChar(600)
                AS
                begin
                  declare @resultado VARCHAR(600)
                  select @resultado = '('+ NIF +') ' + APELLIDO +', ' + NOMBRE from TERCEROS.PERSONA where id = @ID
                  return @resultado
                END
GO

ALTER TABLE [TERCEROS].[PERSONA_OBSERVACION]
ADD [ELEMENTO] AS ([TERCEROS].[CC_PERSONA_NOMBRE]([ID_ELEMENTO]));
go

