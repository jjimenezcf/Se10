-- Declarar una tabla temporal para almacenar los resultados
CREATE TABLE #ResultadosMigraciones (
    NombreBaseDatos NVARCHAR(128),
    MigrationId NVARCHAR(MAX),
    ProductVersion NVARCHAR(32)
);

-- Declarar variables para el bucle
DECLARE @NombreBaseDatos NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);

-- Cursor para recorrer todas las bases de datos
DECLARE db_cursor CURSOR FOR 
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Construir y ejecutar la consulta dinámica
    SET @SQL = N'
    IF EXISTS (SELECT 1 FROM ' + QUOTENAME(@NombreBaseDatos) + '.INFORMATION_SCHEMA.TABLES 
               WHERE TABLE_SCHEMA = ''ENTORNO'' AND TABLE_NAME = ''__Migraciones'')
    BEGIN
        INSERT INTO #ResultadosMigraciones (NombreBaseDatos, MigrationId, ProductVersion)
        SELECT TOP 1 
            ''' + @NombreBaseDatos + ''' AS NombreBaseDatos,
            MigrationId,
            ProductVersion
        FROM ' + QUOTENAME(@NombreBaseDatos) + '.ENTORNO.__Migraciones
        ORDER BY MigrationId DESC;
    END';

    EXEC sp_executesql @SQL;

    FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;
END

CLOSE db_cursor;
DEALLOCATE db_cursor;

-- Mostrar los resultados
SELECT 
    NombreBaseDatos,
    MigrationId,
    ProductVersion
FROM #ResultadosMigraciones
ORDER BY MigrationId DESC, NombreBaseDatos;

-- Limpiar la tabla temporal
DROP TABLE #ResultadosMigraciones;
