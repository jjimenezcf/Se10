DECLARE @NombreBaseDatos NVARCHAR(128);
DECLARE @SQL NVARCHAR(MAX);
DECLARE @NombreBuscado NVARCHAR(150) = 'Consulta de facturas en la AEAT'; 
DECLARE @NuevoNombre NVARCHAR(150) = 'Consultar en AEAT'; 

-- Cursor para recorrer las BD filtradas
DECLARE db_cursor CURSOR FOR 
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' OR name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb');

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Construimos la consulta dinámica. 
    -- Nota: Declaramos los IDs dentro del bloque para que sean locales a la ejecución de la BD actual.
    SET @SQL = N'
    USE ' + QUOTENAME(@NombreBaseDatos) + ';
    
    DECLARE @idMenu INT;
    DECLARE @idVista INT;

    -- Buscamos los IDs en la base de datos actual
    SELECT @idVista = id FROM ENTORNO.VISTA_MVC WHERE nombre LIKE @pNombreBuscado;
    SELECT @idMenu = id FROM ENTORNO.MENU WHERE nombre LIKE @pNombreBuscado;

    -- Actualizamos Menú si existe
    IF @idMenu IS NOT NULL 
    BEGIN
        UPDATE ENTORNO.MENU SET nombre = @pNuevoNombre WHERE id = @idMenu;
    END

    -- Actualizamos Vista si existe (Corregido: Antes apuntabas a MENU con el id de Vista)
    IF @idVista IS NOT NULL 
    BEGIN
        UPDATE ENTORNO.VISTA_MVC SET nombre = @pNuevoNombre WHERE id = @idVista;
    END';

    -- Ejecutamos pasando los parámetros correctamente
    EXEC sp_executesql @SQL, 
        N'@pNombreBuscado NVARCHAR(150), @pNuevoNombre NVARCHAR(150)', 
        @pNombreBuscado = @NombreBuscado, 
        @pNuevoNombre = @NuevoNombre;

    FETCH NEXT FROM db_cursor INTO @NombreBaseDatos;
END

CLOSE db_cursor;
DEALLOCATE db_cursor;