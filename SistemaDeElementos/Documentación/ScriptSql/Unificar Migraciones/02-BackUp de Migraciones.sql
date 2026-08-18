-- Declarar variables para el bucle
DECLARE @NombreBaseDatos NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);


DECLARE db_cursor CURSOR FOR
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND  (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Construir y ejecutar la consulta dinámica
    SET @SQL = N'
    USE ' + QUOTENAME(@NombreBaseDatos) + ';

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
               WHERE TABLE_SCHEMA = ''ENTORNO'' AND TABLE_NAME = ''__Migraciones'')
    BEGIN
        -- Verificar si existe la tabla de backup y borrarla si es así
        IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                   WHERE TABLE_SCHEMA = ''ENTORNO'' AND TABLE_NAME = ''__Migraciones_Backup'')
        BEGIN
            DROP TABLE ENTORNO.__Migraciones_Backup;
        END

        -- Crear la tabla de backup con la información actual
        SELECT * INTO ENTORNO.__Migraciones_Backup
        FROM ENTORNO.__Migraciones;

        PRINT ''Backup de migraciones creado para la base de datos: ' + @NombreBaseDatos + ''';
    END
    ELSE
    BEGIN
        PRINT ''La tabla ENTORNO.__Migraciones no existe en la base de datos: ' + @NombreBaseDatos + ''';
    END';

    EXEC sp_executesql @SQL;

    FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;
END

CLOSE db_cursor;
DEALLOCATE db_cursor;
