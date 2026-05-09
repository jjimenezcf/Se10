USE master
GO

DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)
DECLARE @parametro NVARCHAR(255) = 'FAR_IncrementarOrdenEn'
DECLARE @negocio NVARCHAR(255) = 'Facturas recibidas'
DECLARE @valor NVARCHAR(255) = '10'

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
        INSERT INTO negocio.PARAMETRO (valor, id_negocio, NOMBRE)
        SELECT @valor, ID, @parametro
        FROM negocio.negocio 
        WHERE negocio.NOMBRE LIKE @negocio
        AND NOT EXISTS ( 
            SELECT 1 
            FROM negocio.PARAMETRO t1
            JOIN negocio.negocio t2 ON t2.id = t1.id_negocio 
            WHERE t1.NOMBRE LIKE @parametro
            AND t2.NOMBRE LIKE @negocio  
        )
        
        IF @@ROWCOUNT > 0
            PRINT ''Inserted parameter in database: ' + @DatabaseName + '''
        ELSE
            PRINT ''Parameter already exists or no matching business found in database: ' + @DatabaseName + '''
    END
    ELSE
    BEGIN
        PRINT ''Required tables not found in database: ' + @DatabaseName + '''
    END'
	
    EXEC sp_executesql @SQL, 
        N'@parametro NVARCHAR(255), @negocio NVARCHAR(255), @valor NVARCHAR(255)', 
        @parametro, @negocio, @valor

    FETCH NEXT FROM db_cursor INTO @DatabaseName
END

CLOSE db_cursor
DEALLOCATE db_cursor

-- Eliminar la tabla temporal
DROP TABLE #Databases