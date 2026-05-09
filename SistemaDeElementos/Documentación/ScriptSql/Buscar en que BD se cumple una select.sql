USE master;
GO

-- Crear una tabla temporal para almacenar los resultados
CREATE TABLE #Results (DatabaseName NVARCHAR(128), HasRecord BIT);

-- Declarar variables
DECLARE @DatabaseName NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);

-- Cursor para recorrer todas las bases de datos
DECLARE db_cursor CURSOR FOR
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

-- Abrir el cursor
OPEN db_cursor;

-- Obtener la primera base de datos
FETCH NEXT FROM db_cursor INTO @DatabaseName;

-- Recorrer todas las bases de datos
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Construir la consulta dinámica
    SET @SQL = N'
    USE [' + @DatabaseName + '];
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ''NEGOCIO'' AND TABLE_NAME = ''PLANTILLA_EXPORTACION'')
    BEGIN
        IF EXISTS (SELECT 1 FROM NEGOCIO.PLANTILLA_EXPORTACION WHERE nombre LIKE ''Facturas a contabilizar'')
            INSERT INTO #Results (DatabaseName, HasRecord) VALUES (''' + @DatabaseName + ''', 1)
        ELSE
            INSERT INTO #Results (DatabaseName, HasRecord) VALUES (''' + @DatabaseName + ''', 0)
    END
    ELSE
        INSERT INTO #Results (DatabaseName, HasRecord) VALUES (''' + @DatabaseName + ''', 0)';

    -- Ejecutar la consulta dinámica
    EXEC sp_executesql @SQL;

    -- Obtener la siguiente base de datos
    FETCH NEXT FROM db_cursor INTO @DatabaseName;
END

-- Cerrar y liberar el cursor
CLOSE db_cursor;
DEALLOCATE db_cursor;

-- Mostrar los resultados
SELECT DatabaseName, 
       CASE WHEN HasRecord = 1 THEN 'Registro encontrado' ELSE 'Registro no encontrado' END AS Status
FROM #Results
WHERE HasRecord = 1
ORDER BY DatabaseName;

-- Limpiar la tabla temporal
DROP TABLE #Results;