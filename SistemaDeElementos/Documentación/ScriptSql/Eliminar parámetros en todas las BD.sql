USE master
GO

DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)

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
        DELETE FROM NEGOCIO.PARAMETRO 
        WHERE id IN (
            SELECT t1.ID 
            FROM NEGOCIO.PARAMETRO t1
            INNER JOIN NEGOCIO.NEGOCIO t2 ON t2.id = t1.ID_NEGOCIO
            WHERE (t2.NOMBRE LIKE ''Presupuestos'' AND t1.nombre LIKE ''FAR_%'') or
			(t1.NOMBRE like ''FAR_ToleranciaEnImportes'' and t2.NOMBRE like ''Facturas emitida'' ) or
            (t1.NOMBRE like ''FAR_Naturaleza'' and t2.NOMBRE like ''Interlocutor'' )
        )
        PRINT ''Deleted records from ' + @DatabaseName + '''
    END
    ELSE
    BEGIN
        PRINT ''Tables not found in ' + @DatabaseName + '''
    END'

    EXEC sp_executesql @SQL

    FETCH NEXT FROM db_cursor INTO @DatabaseName
END

CLOSE db_cursor
DEALLOCATE db_cursor

-- Eliminar la tabla temporal
DROP TABLE #Databases