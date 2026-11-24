-- Cambia el modelo y, opcionalmente, la ApiKey de una IA en el parámetro Menu_Ias_Disponibles (negocio Menu) de todas las BD
-- Sólo modifica las entradas cuyo "Enumerado" sea $(Enumerado); el resto de IAs no se tocan.
-- La ApiKey no se guarda en el script: se pasa al ejecutarlo con variables de sqlcmd (vacía = no cambiarla), p.ej.
--   sqlcmd -S .\SQLEXPRESS -E -f 65001 -i IA_CambiarModeloYApiKey.SQL -v Enumerado="IaMistral" Modelo="mistral-small-latest" ApiKey="..." BD="%"
--   BD es el patrón de nombre de las BD a tratar ("%" = todas las SE_ y AB_, "SE_TEST" = sólo esa)
-- Requisitos:
--   * SQL Server 2017 o superior (JSON_MODIFY con ruta en variable)
--   * desplegada la versión que admite el modelo en el enumerado de modelos de la IA, si no dará "modelo no válido"
--   * tras ejecutarlo reciclar el Application Pool de cada sitio: el parámetro está en la caché Negocio_ParametrosDeNegocio
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO
USE master
GO

DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)
DECLARE @Enumerado NVARCHAR(100) = N'$(Enumerado)'
DECLARE @Modelo NVARCHAR(100) = N'$(Modelo)'
DECLARE @ApiKey NVARCHAR(500) = N'$(ApiKey)'
DECLARE @PatronBd NVARCHAR(255) = N'$(BD)'

CREATE TABLE #Databases (DatabaseName NVARCHAR(255))

INSERT INTO #Databases
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name LIKE @PatronBd
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

DECLARE db_cursor CURSOR FOR
SELECT DatabaseName FROM #Databases

OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @DatabaseName

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = CAST(N'' AS NVARCHAR(MAX)) + N'
    USE [' + @DatabaseName + ']
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ''NEGOCIO'' AND TABLE_NAME = ''PARAMETRO'')
    AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ''NEGOCIO'' AND TABLE_NAME = ''NEGOCIO'')
    BEGIN
        DECLARE @Json NVARCHAR(MAX), @IdParametro INT, @Indice NVARCHAR(10), @Cambios INT = 0

        SELECT @IdParametro = P.ID, @Json = P.VALOR
        FROM NEGOCIO.PARAMETRO P
        INNER JOIN NEGOCIO.NEGOCIO N ON N.ID = P.ID_NEGOCIO
        WHERE N.ENUMERADO = ''Menu'' AND P.NOMBRE = ''Menu_Ias_Disponibles''

        IF @IdParametro IS NULL
            PRINT ''Sin parámetro Menu_Ias_Disponibles en ' + @DatabaseName + '''
        ELSE IF ISJSON(@Json) = 0
            PRINT ''JSON no válido en Menu_Ias_Disponibles de ' + @DatabaseName + '''
        ELSE
        BEGIN
            -- cada vuelta corrige una entrada de la IA que aún no tiene el modelo o la ApiKey pedidos
            WHILE 1 = 1
            BEGIN
                SET @Indice = NULL
                SELECT TOP 1 @Indice = [key]
                FROM OPENJSON(@Json)
                WHERE JSON_VALUE(value, ''$.Enumerado'') = @Enumerado
                AND (ISNULL(JSON_VALUE(value, ''$.Modelo''), '''') <> @Modelo
                     OR (@ApiKey <> '''' AND ISNULL(JSON_VALUE(value, ''$.ApiKey''), '''') <> @ApiKey))

                IF @Indice IS NULL BREAK

                SET @Json = JSON_MODIFY(@Json, ''$['' + @Indice + ''].Modelo'', @Modelo)
                IF @ApiKey <> '''' SET @Json = JSON_MODIFY(@Json, ''$['' + @Indice + ''].ApiKey'', @ApiKey)
                SET @Cambios = @Cambios + 1
            END

            IF @Cambios > 0
            BEGIN
                UPDATE NEGOCIO.PARAMETRO SET VALOR = @Json WHERE ID = @IdParametro
                PRINT ''Actualizado en ' + @DatabaseName + '''
            END
            ELSE IF NOT EXISTS (SELECT 1 FROM OPENJSON(@Json) WHERE JSON_VALUE(value, ''$.Enumerado'') = @Enumerado)
                PRINT ''Sin entrada '' + @Enumerado + '' en ' + @DatabaseName + '''
            ELSE
                PRINT ''Sin cambios en ' + @DatabaseName + '''
        END
    END
    ELSE
    BEGIN
        PRINT ''Tables not found in ' + @DatabaseName + '''
    END'

    EXEC sp_executesql @SQL, N'@Enumerado NVARCHAR(100), @Modelo NVARCHAR(100), @ApiKey NVARCHAR(500)', @Enumerado = @Enumerado, @Modelo = @Modelo, @ApiKey = @ApiKey

    FETCH NEXT FROM db_cursor INTO @DatabaseName
END

CLOSE db_cursor
DEALLOCATE db_cursor

DROP TABLE #Databases
