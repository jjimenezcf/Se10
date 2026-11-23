-- Cambia el modelo de Gemini en el parámetro Menu_Ias_Disponibles (negocio Menu) de todas las BD
-- Sólo modifica la propiedad "Modelo" de las entradas con "Enumerado": "IaGeminis"; ApiKey y el resto de IAs no se tocan
-- Requisitos:
--   * SQL Server 2017 o superior (JSON_MODIFY con ruta en variable)
--   * desplegada la versión que admite el modelo en enumModelosGeminis (IaGeminis.cs), si no dará "modelo no válido"
--   * tras ejecutarlo reciclar el Application Pool de cada sitio: el parámetro está en la caché Negocio_ParametrosDeNegocio
USE master
GO

DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)
DECLARE @Modelo NVARCHAR(100) = N'gemini-3.5-flash'

-- Crear una tabla temporal para almacenar los nombres de las bases de datos
CREATE TABLE #Databases (DatabaseName NVARCHAR(255))

-- Insertar los nombres de las bases de datos en la tabla temporal, excluyendo las del sistema
INSERT INTO #Databases
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

-- Cursor para recorrer cada base de datos
DECLARE db_cursor CURSOR FOR
SELECT DatabaseName FROM #Databases

OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @DatabaseName

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = N'
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
            -- cada vuelta cambia una entrada de Gemini con otro modelo, hasta que no quede ninguna
            WHILE 1 = 1
            BEGIN
                SET @Indice = NULL
                SELECT TOP 1 @Indice = [key]
                FROM OPENJSON(@Json)
                WHERE JSON_VALUE(value, ''$.Enumerado'') = ''IaGeminis''
                AND ISNULL(JSON_VALUE(value, ''$.Modelo''), '''') <> @Modelo

                IF @Indice IS NULL BREAK

                SET @Json = JSON_MODIFY(@Json, ''$['' + @Indice + ''].Modelo'', @Modelo)
                SET @Cambios = @Cambios + 1
            END

            IF @Cambios > 0
            BEGIN
                UPDATE NEGOCIO.PARAMETRO SET VALOR = @Json WHERE ID = @IdParametro
                PRINT ''Actualizado en ' + @DatabaseName + '''
            END
            ELSE
                PRINT ''Sin cambios en ' + @DatabaseName + '''
        END
    END
    ELSE
    BEGIN
        PRINT ''Tables not found in ' + @DatabaseName + '''
    END'

    EXEC sp_executesql @SQL, N'@Modelo NVARCHAR(100)', @Modelo = @Modelo

    FETCH NEXT FROM db_cursor INTO @DatabaseName
END

CLOSE db_cursor
DEALLOCATE db_cursor

-- Eliminar la tabla temporal
DROP TABLE #Databases
