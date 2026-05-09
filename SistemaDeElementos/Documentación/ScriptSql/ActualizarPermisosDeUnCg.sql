DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)

DECLARE db_cursor CURSOR FOR
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @DatabaseName

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = '
    USE [' + @DatabaseName + '];

    DECLARE @IDClase INT, @IDTipo INT;

    -- Validar y crear registro en CLASE_PERMISO si no existe
    IF NOT EXISTS (SELECT 1 FROM SEGURIDAD.CLASE_PERMISO WHERE nombre LIKE ''Elemento'')
    BEGIN
        INSERT INTO SEGURIDAD.CLASE_PERMISO (NOMBRE)
        VALUES (''Elemento'');
    END

    -- Obtener ID de clase y tipo
    SELECT @IDClase = ID FROM SEGURIDAD.CLASE_PERMISO WHERE nombre LIKE ''Elemento'';
    SELECT @IDTipo = ID FROM SEGURIDAD.TIPO_PERMISO WHERE nombre LIKE ''Gestor'';

    DECLARE @IDGestor INT, @IDConsultor INT, @PermisoNombre NVARCHAR(255);
    DECLARE @Codigo NVARCHAR(255), @Nombre NVARCHAR(255);

    -- Procesar registros de TERCEROS.CENTRO_GESTOR
    DECLARE cg_cursor CURSOR FOR
    SELECT CODIGO, NOMBRE, ID_GESTOR, ID_CONSULTOR
    FROM TERCEROS.CENTRO_GESTOR

    OPEN cg_cursor
    FETCH NEXT FROM cg_cursor INTO @Codigo, @Nombre, @IDGestor, @IDConsultor

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Procesar ID_GESTOR
        IF @IDGestor IS NULL
        BEGIN
            SET @PermisoNombre = ''CG: (Gestor) ('' + @Codigo + '') '' + @Nombre;
            SELECT @IDGestor = ID FROM SEGURIDAD.PERMISO WHERE NOMBRE = @PermisoNombre;

            IF @IDGestor IS NULL
            BEGIN
                INSERT INTO SEGURIDAD.PERMISO (NOMBRE, IDCLASE, IDTIPO)
                VALUES (@PermisoNombre, @IDClase, @IDTipo);

                SET @IDGestor = SCOPE_IDENTITY();
            END

            UPDATE TERCEROS.CENTRO_GESTOR
            SET ID_GESTOR = @IDGestor
            WHERE CODIGO = @Codigo AND NOMBRE = @Nombre;
        END

        -- Procesar ID_CONSULTOR
        IF @IDConsultor IS NULL
        BEGIN
            SET @PermisoNombre = ''CG: (Consultor) ('' + @Codigo + '') '' + @Nombre;
            SELECT @IDConsultor = ID FROM SEGURIDAD.PERMISO WHERE NOMBRE = @PermisoNombre;

            IF @IDConsultor IS NULL
            BEGIN
                INSERT INTO SEGURIDAD.PERMISO (NOMBRE, IDCLASE, IDTIPO)
                VALUES (@PermisoNombre, @IDClase, @IDTipo);

                SET @IDConsultor = SCOPE_IDENTITY();
            END

            UPDATE TERCEROS.CENTRO_GESTOR
            SET ID_CONSULTOR = @IDConsultor
            WHERE CODIGO = @Codigo AND NOMBRE = @Nombre;
        END

        FETCH NEXT FROM cg_cursor INTO @Codigo, @Nombre, @IDGestor, @IDConsultor
    END

    CLOSE cg_cursor
    DEALLOCATE cg_cursor
    '

    EXEC sp_executesql @SQL

    FETCH NEXT FROM db_cursor INTO @DatabaseName
END

CLOSE db_cursor
DEALLOCATE db_cursor