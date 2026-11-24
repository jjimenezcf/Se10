-- Unifica los permisos de vista creados con un plural mal formado ('VISTA: Interlocutors', 'VISTA: Sociedads')
-- con los correctos ('VISTA: Interlocutores', 'VISTA: Sociedades').
-- En las BD donde conviven los dos, la vista apunta al mal nombrado y los roles tienen el correcto:
-- quien no es administrador no puede abrir la vista aunque su rol tenga el permiso correcto.
-- Por cada par, en cada BD:
--   * si sólo existe el mal nombrado, se renombra
--   * si existen los dos: la vista pasa a apuntar al correcto, los roles y puestos que tenían el mal nombrado
--     reciben el correcto (sin duplicar) y se borra el mal nombrado
-- Cada BD va en su propia transacción: si algo falla se deshace esa BD y se informa.
-- Tras ejecutarlo reciclar los Application Pools (los permisos de vista se cachean en memoria)
-- QUOTED_IDENTIFIER y ANSI_NULLS a ON: hay índices filtrados en estas tablas (sqlcmd los deja a OFF por defecto)
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
    SET @SQL = N'
    USE [' + @DatabaseName + ']
    SET QUOTED_IDENTIFIER ON
    SET ANSI_NULLS ON
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ''SEGURIDAD'' AND TABLE_NAME = ''PERMISO'')
    BEGIN
        DECLARE @Pares TABLE (MALO NVARCHAR(250), BUENO NVARCHAR(250))
        INSERT INTO @Pares VALUES (N''VISTA: Interlocutors'', N''VISTA: Interlocutores''), (N''VISTA: Sociedads'', N''VISTA: Sociedades'')

        DECLARE @Malo NVARCHAR(250), @Bueno NVARCHAR(250), @IdMalo INT, @IdBueno INT
        DECLARE pares CURSOR LOCAL FOR SELECT MALO, BUENO FROM @Pares

        BEGIN TRY
            BEGIN TRANSACTION
            OPEN pares
            FETCH NEXT FROM pares INTO @Malo, @Bueno
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @IdMalo = (SELECT ID FROM SEGURIDAD.PERMISO WHERE NOMBRE = @Malo)
                SET @IdBueno = (SELECT ID FROM SEGURIDAD.PERMISO WHERE NOMBRE = @Bueno)

                IF @IdMalo IS NOT NULL AND @IdBueno IS NULL
                BEGIN
                    UPDATE SEGURIDAD.PERMISO SET NOMBRE = @Bueno WHERE ID = @IdMalo
                    PRINT ''' + @DatabaseName + ': renombrado '' + @Malo + '' a '' + @Bueno
                END
                ELSE IF @IdMalo IS NOT NULL AND @IdBueno IS NOT NULL
                BEGIN
                    UPDATE ENTORNO.VISTA_MVC SET IDPERMISO = @IdBueno WHERE IDPERMISO = @IdMalo

                    INSERT INTO SEGURIDAD.ROL_PERMISO (IDROL, IDPERMISO)
                    SELECT DISTINCT r.IDROL, @IdBueno FROM SEGURIDAD.ROL_PERMISO r
                    WHERE r.IDPERMISO = @IdMalo
                      AND NOT EXISTS (SELECT 1 FROM SEGURIDAD.ROL_PERMISO b WHERE b.IDROL = r.IDROL AND b.IDPERMISO = @IdBueno)
                    DELETE FROM SEGURIDAD.ROL_PERMISO WHERE IDPERMISO = @IdMalo

                    INSERT INTO SEGURIDAD.PERMISO_DIRECTOS (IDPUESTO, IDPERMISO)
                    SELECT DISTINCT d.IDPUESTO, @IdBueno FROM SEGURIDAD.PERMISO_DIRECTOS d
                    WHERE d.IDPERMISO = @IdMalo
                      AND NOT EXISTS (SELECT 1 FROM SEGURIDAD.PERMISO_DIRECTOS b WHERE b.IDPUESTO = d.IDPUESTO AND b.IDPERMISO = @IdBueno)
                    DELETE FROM SEGURIDAD.PERMISO_DIRECTOS WHERE IDPERMISO = @IdMalo

                    DELETE FROM SEGURIDAD.PERMISO WHERE ID = @IdMalo
                    PRINT ''' + @DatabaseName + ': unificado '' + @Malo + '' en '' + @Bueno
                END

                FETCH NEXT FROM pares INTO @Malo, @Bueno
            END
            CLOSE pares
            DEALLOCATE pares
            COMMIT TRANSACTION
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
            IF CURSOR_STATUS(''local'', ''pares'') >= 0 CLOSE pares
            IF CURSOR_STATUS(''local'', ''pares'') >= -1 DEALLOCATE pares
            PRINT ''ERROR en ' + @DatabaseName + ' (se deshace): '' + ERROR_MESSAGE()
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
