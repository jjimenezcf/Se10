-- Sustituye en los puestos de trabajo los roles AFB antiguos (InzSeguridadComun) por los roles AFB por menú (InzRolesFuncionales)
-- y elimina los antiguos.
--   AFB: Terceros básicos     -> AFB: Maestros + AFB: Maestros.Terceros
--   AFB: Seguridad de acceso  -> AFB: Configuración.Accesos
--   AFB: Callejero            -> AFB: Maestros.Callejero
--   AFB: Registro de E/S      -> AFB: Gestión Administrativa + AFB: Gestión Administrativa.Configuración.Registro
--   AFB: Tareas               -> AFB: Gestión Administrativa + AFB: Gestión Administrativa.Configuración.Tarea
--   AFB: Expedientes          -> AFB: Gestión Administrativa + AFB: Gestión Administrativa.Configuración.Expediente
--   AFB: Sistema documental   -> se queda en sus puestos renombrado a "Archivador: Básico" sin sus vistas (conserva los permisos
--                                de tipo de archivador) y esos puestos reciben AFB: Sistema Documental + AFB: Sistema Documental.Configuración
-- Además añade a "AFB: Entorno" las vistas de auditoría, errores y trazas de un trabajo y mi correo.
--
-- ORDEN DE EJECUCIÓN en cada BD (dos pasadas):
--   1. ejecutar este script: renombra "AFB: Sistema documental" a "Archivador: Básico". Hace falta antes de inicializar el entorno porque
--      los nombres de rol no distinguen mayúsculas y "AFB: Sistema Documental" (nuevo) chocaría con "AFB: Sistema documental" (antiguo)
--   2. ejecutar "Inicializar entorno": crea los roles AFB por menú
--   3. volver a ejecutar este script: sustituye los roles en los puestos y elimina los antiguos
-- Si en una BD falta alguno de los roles nuevos, el paso de sustitución no se hace en esa BD y se informa.
-- Las vistas se comprueban en vivo contra ENTORNO.USUARIO_PERMISO: no hace falta recalcular la seguridad,
-- pero sí reciclar los Application Pools (los permisos de vista se cachean en memoria).
-- Cada paso va en su propia transacción: si algo falla se deshace ese paso en esa BD y se informa.
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO
USE master
GO

DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)

CREATE TABLE #Databases (DatabaseName NVARCHAR(255))

INSERT INTO #Databases
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

DECLARE db_cursor CURSOR FOR
SELECT DatabaseName FROM #Databases

OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @DatabaseName

WHILE @@FETCH_STATUS = 0
BEGIN
    -- CAST a NVARCHAR(MAX): sin él la concatenación con @DatabaseName se trunca a 4000 caracteres
    SET @SQL = CAST(N'' AS NVARCHAR(MAX)) + N'
    USE [' + @DatabaseName + ']
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ''SEGURIDAD'' AND TABLE_NAME = ''ROL'')
    BEGIN
        -- Paso A: "AFB: Sistema documental" pasa a ser el rol de datos "Archivador: Básico" (sin vistas, en los mismos puestos)
        -- se compara con COLLATE ... CS para no confundirlo con el nuevo "AFB: Sistema Documental"
        BEGIN TRY
            BEGIN TRANSACTION
            DECLARE @IdSisDoc INT = (SELECT ID FROM SEGURIDAD.ROL WHERE NOMBRE COLLATE Modern_Spanish_CS_AS = N''AFB: Sistema documental'')
            DECLARE @IdArcBasico INT = (SELECT ID FROM SEGURIDAD.ROL WHERE NOMBRE = N''Archivador: Básico'')
            IF @IdSisDoc IS NOT NULL
            BEGIN
                DELETE rp FROM SEGURIDAD.ROL_PERMISO rp
                JOIN SEGURIDAD.PERMISO p ON p.ID = rp.IDPERMISO
                JOIN SEGURIDAD.CLASE_PERMISO c ON c.ID = p.IDCLASE
                WHERE rp.IDROL = @IdSisDoc AND c.NOMBRE = N''Vista''

                IF @IdArcBasico IS NULL
                BEGIN
                    UPDATE SEGURIDAD.ROL
                    SET NOMBRE = N''Archivador: Básico'',
                        DESCRIPCION = N''Permisos de datos básicos sobre archivadores: gestor del tipo básico y consultor del de exportación''
                    WHERE ID = @IdSisDoc
                    PRINT ''' + @DatabaseName + ': AFB: Sistema documental renombrado a Archivador: Básico''
                END
                ELSE
                BEGIN
                    -- ya existe: se le pasan los permisos y puestos del antiguo y el antiguo se borra
                    INSERT INTO SEGURIDAD.ROL_PERMISO (IDROL, IDPERMISO)
                    SELECT @IdArcBasico, rp.IDPERMISO FROM SEGURIDAD.ROL_PERMISO rp
                    WHERE rp.IDROL = @IdSisDoc AND NOT EXISTS (SELECT 1 FROM SEGURIDAD.ROL_PERMISO x WHERE x.IDROL = @IdArcBasico AND x.IDPERMISO = rp.IDPERMISO)
                    INSERT INTO SEGURIDAD.ROL_PUESTO (IDROL, IDPUESTO)
                    SELECT @IdArcBasico, rp.IDPUESTO FROM SEGURIDAD.ROL_PUESTO rp
                    WHERE rp.IDROL = @IdSisDoc AND NOT EXISTS (SELECT 1 FROM SEGURIDAD.ROL_PUESTO x WHERE x.IDROL = @IdArcBasico AND x.IDPUESTO = rp.IDPUESTO)
                    DELETE FROM SEGURIDAD.ROL_PUESTO WHERE IDROL = @IdSisDoc
                    DELETE FROM SEGURIDAD.ROL_PERMISO WHERE IDROL = @IdSisDoc
                    DELETE FROM SEGURIDAD.ROL WHERE ID = @IdSisDoc
                    PRINT ''' + @DatabaseName + ': AFB: Sistema documental unificado en Archivador: Básico''
                END
            END
            COMMIT TRANSACTION
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
            PRINT ''ERROR en ' + @DatabaseName + ' renombrando AFB: Sistema documental (se deshace): '' + ERROR_MESSAGE()
            RETURN
        END CATCH

        -- Paso B: sustitución de los roles antiguos en los puestos (ANTIGUO = rol de origen, NUEVO = rol que recibe el puesto)
        DECLARE @Mapa TABLE (ANTIGUO NVARCHAR(250), NUEVO NVARCHAR(250))
        INSERT INTO @Mapa VALUES
            (N''AFB: Terceros básicos'', N''AFB: Maestros''),
            (N''AFB: Terceros básicos'', N''AFB: Maestros.Terceros''),
            (N''AFB: Seguridad de acceso'', N''AFB: Configuración.Accesos''),
            (N''AFB: Callejero'', N''AFB: Maestros.Callejero''),
            (N''AFB: Registro de E/S'', N''AFB: Gestión Administrativa''),
            (N''AFB: Registro de E/S'', N''AFB: Gestión Administrativa.Configuración.Registro''),
            (N''AFB: Tareas'', N''AFB: Gestión Administrativa''),
            (N''AFB: Tareas'', N''AFB: Gestión Administrativa.Configuración.Tarea''),
            (N''AFB: Expedientes'', N''AFB: Gestión Administrativa''),
            (N''AFB: Expedientes'', N''AFB: Gestión Administrativa.Configuración.Expediente''),
            (N''Archivador: Básico'', N''AFB: Sistema Documental''),
            (N''Archivador: Básico'', N''AFB: Sistema Documental.Configuración'')

        -- roles que se eliminan al terminar ("Archivador: Básico" se conserva)
        DECLARE @Borrar TABLE (NOMBRE NVARCHAR(250))
        INSERT INTO @Borrar VALUES (N''AFB: Terceros básicos''), (N''AFB: Seguridad de acceso''), (N''AFB: Callejero''),
                                   (N''AFB: Registro de E/S''), (N''AFB: Tareas''), (N''AFB: Expedientes'')

        DECLARE @Faltan NVARCHAR(MAX) = (SELECT STRING_AGG(m.NUEVO, N'', '') FROM (SELECT DISTINCT NUEVO FROM @Mapa) m
                                         WHERE NOT EXISTS (SELECT 1 FROM SEGURIDAD.ROL r WHERE r.NOMBRE COLLATE Modern_Spanish_CS_AS = m.NUEVO))
        IF @Faltan IS NOT NULL
        BEGIN
            PRINT ''' + @DatabaseName + ': sustitución pendiente, faltan los roles nuevos (ejecutar Inicializar entorno y volver a lanzar el script): '' + @Faltan
            RETURN
        END

        BEGIN TRY
            BEGIN TRANSACTION

            -- los puestos con un rol antiguo reciben sus roles nuevos
            INSERT INTO SEGURIDAD.ROL_PUESTO (IDROL, IDPUESTO)
            SELECT DISTINCT nuevo.ID, rp.IDPUESTO
            FROM SEGURIDAD.ROL_PUESTO rp
            JOIN SEGURIDAD.ROL antiguo ON antiguo.ID = rp.IDROL
            JOIN @Mapa m ON m.ANTIGUO = antiguo.NOMBRE COLLATE Modern_Spanish_CS_AS
            JOIN SEGURIDAD.ROL nuevo ON nuevo.NOMBRE COLLATE Modern_Spanish_CS_AS = m.NUEVO
            WHERE NOT EXISTS (SELECT 1 FROM SEGURIDAD.ROL_PUESTO x WHERE x.IDROL = nuevo.ID AND x.IDPUESTO = rp.IDPUESTO)
            PRINT ''' + @DatabaseName + ': asignaciones de roles nuevos a puestos: '' + CAST(@@ROWCOUNT AS NVARCHAR(10))

            -- AFB: Entorno recibe las vistas secundarias que no son de ningún módulo
            DECLARE @IdEntorno INT = (SELECT ID FROM SEGURIDAD.ROL WHERE NOMBRE = N''AFB: Entorno'')
            IF @IdEntorno IS NOT NULL
            BEGIN
                INSERT INTO SEGURIDAD.ROL_PERMISO (IDROL, IDPERMISO)
                SELECT DISTINCT @IdEntorno, v.IDPERMISO FROM ENTORNO.VISTA_MVC v
                WHERE v.NOMBRE IN (N''Auditoría de elementos'', N''Errores de un trabajo'', N''Traza de un trabajo'',
                                   N''Visor de la bandeja de entrada'', N''Mi correo (Imap)'', N''Mi correo (ApiKey)'')
                  AND NOT EXISTS (SELECT 1 FROM SEGURIDAD.ROL_PERMISO x WHERE x.IDROL = @IdEntorno AND x.IDPERMISO = v.IDPERMISO)
                PRINT ''' + @DatabaseName + ': vistas añadidas a AFB: Entorno: '' + CAST(@@ROWCOUNT AS NVARCHAR(10))
            END

            -- se eliminan los roles antiguos
            DECLARE @Antiguos TABLE (ID INT)
            INSERT INTO @Antiguos SELECT r.ID FROM SEGURIDAD.ROL r WHERE r.NOMBRE COLLATE Modern_Spanish_CS_AS IN (SELECT NOMBRE FROM @Borrar)
            DELETE FROM SEGURIDAD.ROL_PUESTO WHERE IDROL IN (SELECT ID FROM @Antiguos)
            DELETE FROM SEGURIDAD.ROL_PERMISO WHERE IDROL IN (SELECT ID FROM @Antiguos)
            DELETE FROM SEGURIDAD.ROL WHERE ID IN (SELECT ID FROM @Antiguos)
            PRINT ''' + @DatabaseName + ': roles antiguos eliminados: '' + CAST(@@ROWCOUNT AS NVARCHAR(10))

            COMMIT TRANSACTION
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
            PRINT ''ERROR en ' + @DatabaseName + ' sustituyendo roles (se deshace): '' + ERROR_MESSAGE()
        END CATCH
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

DROP TABLE #Databases
