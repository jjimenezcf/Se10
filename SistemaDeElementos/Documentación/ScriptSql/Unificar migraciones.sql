-- Declarar variables para el bucle
DECLARE @NombreBaseDatos NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);
-- Ajusta este valor según tu migración inicial
DECLARE @MigracionInicial NVARCHAR(150) = '20241010143448_InitialCreate'; 

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
    USE ' + QUOTENAME(@NombreBaseDatos) + ';

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
               WHERE TABLE_SCHEMA = ''ENTORNO'' AND TABLE_NAME = ''__Migraciones'')
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            -- Borrar todos los registros existentes
            DELETE FROM [ENTORNO].[__Migraciones];

            -- Insertar el registro de la migración inicial
            INSERT INTO [ENTORNO].[__Migraciones] ([MigrationId], [ProductVersion])
            VALUES (@MigracionInicial, ''8.0.0'');

            COMMIT TRANSACTION;
            PRINT ''Migración inicial insertada en la base de datos: ' + @NombreBaseDatos + ''';
        END TRY
        BEGIN CATCH
            ROLLBACK TRANSACTION;
            PRINT ''Error al procesar la base de datos: ' + @NombreBaseDatos + ' - '' + ERROR_MESSAGE();
        END CATCH
    END
    ELSE
    BEGIN
        PRINT ''La tabla ENTORNO.__Migraciones no existe en la base de datos: ' + @NombreBaseDatos + ''';
    END';

    EXEC sp_executesql @SQL, N'@MigracionInicial NVARCHAR(150)', @MigracionInicial;

    FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;
END

CLOSE db_cursor;
DEALLOCATE db_cursor;
