CREATE OR ALTER PROCEDURE dbo.CopiarCallesDeCallejero
    @BdOrigen NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    IF @BdOrigen = DB_NAME()
    BEGIN
        PRINT N'La base de datos de origen es la misma en la que se ejecuta el procedimiento, no se hace nada.';
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = @BdOrigen AND state_desc = 'ONLINE')
    BEGIN
        RAISERROR(N'La base de datos de origen "%s" no existe o no está online.', 16, 1, @BdOrigen);
        RETURN;
    END

    DECLARE @sql NVARCHAR(MAX);
    DECLARE @origen SYSNAME = QUOTENAME(@BdOrigen);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Antes de copiar las calles nos aseguramos de que PAIS, PROVINCIA, MUNICIPIO y TIPO_VIA
        -- del origen ya existen en destino (creándolos en cascada si faltan)
        EXEC dbo.CopiarMaestrosDeCallejero @BdOrigen = @BdOrigen;

        -- BARRIO (el índice es ID_MUNICIPIO + NOMBRE; el municipio se resuelve por provincia+nombre.
        -- Se dedupe por si varias filas de origen resuelven a la misma clave de destino, p.ej. por
        -- nombres que solo difieren en mayúsculas/espacios y que la collation de destino equipara)
        SET @sql = N'
            ;WITH OrigenBase AS (
                SELECT dmun.ID AS ID_MUNICIPIO, ENTORNO.CapitalizarFrase(o.NOMBRE) AS NOMBRE
                FROM ' + @origen + N'.CALLEJERO.BARRIO o
                INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = o.ID_MUNICIPIO
                INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
                INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
                INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
            ),
            Origen AS (
                SELECT ID_MUNICIPIO, NOMBRE,
                       ROW_NUMBER() OVER (PARTITION BY ID_MUNICIPIO, NOMBRE ORDER BY (SELECT NULL)) AS RN
                FROM OrigenBase
            )
            INSERT INTO CALLEJERO.BARRIO (ID_MUNICIPIO, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR)
            SELECT ID_MUNICIPIO, NOMBRE, GETDATE(), 1, NULL, NULL
            FROM Origen o
            WHERE RN = 1
              AND NOT EXISTS (
                SELECT 1 FROM CALLEJERO.BARRIO d
                WHERE d.ID_MUNICIPIO = o.ID_MUNICIPIO AND d.NOMBRE = o.NOMBRE
            );';
        EXEC sp_executesql @sql;

        -- ZONA (el índice es ID_MUNICIPIO + NOMBRE; el municipio se resuelve por provincia+nombre.
        -- Se dedupe igual que BARRIO por si varias filas de origen colapsan en la misma clave destino)
        SET @sql = N'
            ;WITH OrigenBase AS (
                SELECT dmun.ID AS ID_MUNICIPIO, ENTORNO.CapitalizarFrase(o.NOMBRE) AS NOMBRE
                FROM ' + @origen + N'.CALLEJERO.ZONA o
                INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = o.ID_MUNICIPIO
                INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
                INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
                INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
            ),
            Origen AS (
                SELECT ID_MUNICIPIO, NOMBRE,
                       ROW_NUMBER() OVER (PARTITION BY ID_MUNICIPIO, NOMBRE ORDER BY (SELECT NULL)) AS RN
                FROM OrigenBase
            )
            INSERT INTO CALLEJERO.ZONA (ID_MUNICIPIO, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR)
            SELECT ID_MUNICIPIO, NOMBRE, GETDATE(), 1, NULL, NULL
            FROM Origen o
            WHERE RN = 1
              AND NOT EXISTS (
                SELECT 1 FROM CALLEJERO.ZONA d
                WHERE d.ID_MUNICIPIO = o.ID_MUNICIPIO AND d.NOMBRE = o.NOMBRE
            );';
        EXEC sp_executesql @sql;

        -- CALLE (el índice único real es ID_MUNICIPIO + ID_TIPO_DE_VIA + NOMBRE, ver migración EF Core
        -- 20260818160748_InitialCreate; el ModeloDeDatos/BD/.../CALLE.sql está desactualizado y solo
        -- documentaba ID_MUNICIPIO + NOMBRE. El municipio se resuelve por provincia+nombre y el tipo
        -- de vía por su SIGLA. Se dedupe por la misma razón que BARRIO/ZONA)
        SET @sql = N'
            ;WITH OrigenBase AS (
                SELECT dmun.ID AS ID_MUNICIPIO, dtv.ID AS ID_TIPO_DE_VIA, ENTORNO.CapitalizarFrase(o.NOMBRE) AS NOMBRE
                FROM ' + @origen + N'.CALLEJERO.CALLE o
                INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = o.ID_MUNICIPIO
                INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
                INNER JOIN ' + @origen + N'.CALLEJERO.TIPO_VIA otv ON otv.ID = o.ID_TIPO_DE_VIA
                INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
                INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
                INNER JOIN CALLEJERO.TIPO_VIA dtv ON dtv.SIGLA = otv.SIGLA
            ),
            Origen AS (
                SELECT ID_MUNICIPIO, ID_TIPO_DE_VIA, NOMBRE,
                       ROW_NUMBER() OVER (PARTITION BY ID_MUNICIPIO, ID_TIPO_DE_VIA, NOMBRE ORDER BY (SELECT NULL)) AS RN
                FROM OrigenBase
            )
            INSERT INTO CALLEJERO.CALLE (ID_MUNICIPIO, ID_TIPO_DE_VIA, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR)
            SELECT ID_MUNICIPIO, ID_TIPO_DE_VIA, NOMBRE, GETDATE(), 1, NULL, NULL
            FROM Origen o
            WHERE RN = 1
              AND NOT EXISTS (
                SELECT 1 FROM CALLEJERO.CALLE d
                WHERE d.ID_MUNICIPIO = o.ID_MUNICIPIO AND d.ID_TIPO_DE_VIA = o.ID_TIPO_DE_VIA AND d.NOMBRE = o.NOMBRE
            );';
        EXEC sp_executesql @sql;

        -- CALLE_CP (el índice es ID_CP + ID_CALLE; la calle se resuelve por municipio+tipo de vía+nombre
        -- (la clave única real de CALLE, ver comentario en el bloque CALLE) y el CP por CP.
        -- Se dedupe por si varias filas de origen (p.ej. tramos DESDE/HASTA distintos) resuelven a la
        -- misma pareja calle/CP de destino, para no violar la clave única con un solo INSERT)
        SET @sql = N'
            ;WITH Origen AS (
                SELECT dcalle.ID AS ID_CALLE, dcp.ID AS ID_CP, o.DESDE, o.HASTA, o.MANO,
                       ROW_NUMBER() OVER (PARTITION BY dcalle.ID, dcp.ID ORDER BY (SELECT NULL)) AS RN
                FROM ' + @origen + N'.CALLEJERO.CALLE_CP o
                INNER JOIN ' + @origen + N'.CALLEJERO.CALLE oc ON oc.ID = o.ID_CALLE
                INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = oc.ID_MUNICIPIO
                INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
                INNER JOIN ' + @origen + N'.CALLEJERO.TIPO_VIA otv ON otv.ID = oc.ID_TIPO_DE_VIA
                INNER JOIN ' + @origen + N'.CALLEJERO.CODIGO_POSTAL ocp ON ocp.ID = o.ID_CP
                INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
                INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
                INNER JOIN CALLEJERO.TIPO_VIA dtv ON dtv.SIGLA = otv.SIGLA
                INNER JOIN CALLEJERO.CALLE dcalle ON dcalle.NOMBRE = oc.NOMBRE AND dcalle.ID_MUNICIPIO = dmun.ID AND dcalle.ID_TIPO_DE_VIA = dtv.ID
                INNER JOIN CALLEJERO.CODIGO_POSTAL dcp ON dcp.CP = ocp.CP
            )
            INSERT INTO CALLEJERO.CALLE_CP (ID_CALLE, ID_CP, DESDE, HASTA, MANO)
            SELECT ID_CALLE, ID_CP, DESDE, HASTA, MANO
            FROM Origen o
            WHERE RN = 1
              AND NOT EXISTS (
                SELECT 1 FROM CALLEJERO.CALLE_CP d
                WHERE d.ID_CALLE = o.ID_CALLE AND d.ID_CP = o.ID_CP
            );';
        EXEC sp_executesql @sql;

        -- CALLE_BARRIO (el índice es ID_BARRIO + ID_CALLE; calle se resuelve por municipio+tipo de vía+nombre
        -- (la clave única real de CALLE) y barrio por municipio+nombre. Se dedupe por la misma razón que CALLE_CP)
        SET @sql = N'
            ;WITH Origen AS (
                SELECT dcalle.ID AS ID_CALLE, dbarrio.ID AS ID_BARRIO, o.DESDE, o.HASTA, o.MANO,
                       ROW_NUMBER() OVER (PARTITION BY dcalle.ID, dbarrio.ID ORDER BY (SELECT NULL)) AS RN
                FROM ' + @origen + N'.CALLEJERO.CALLE_BARRIO o
                INNER JOIN ' + @origen + N'.CALLEJERO.CALLE oc ON oc.ID = o.ID_CALLE
                INNER JOIN ' + @origen + N'.CALLEJERO.BARRIO ob ON ob.ID = o.ID_BARRIO
                INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = oc.ID_MUNICIPIO
                INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
                INNER JOIN ' + @origen + N'.CALLEJERO.TIPO_VIA otv ON otv.ID = oc.ID_TIPO_DE_VIA
                INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
                INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
                INNER JOIN CALLEJERO.TIPO_VIA dtv ON dtv.SIGLA = otv.SIGLA
                INNER JOIN CALLEJERO.CALLE dcalle ON dcalle.NOMBRE = oc.NOMBRE AND dcalle.ID_MUNICIPIO = dmun.ID AND dcalle.ID_TIPO_DE_VIA = dtv.ID
                INNER JOIN CALLEJERO.BARRIO dbarrio ON dbarrio.NOMBRE = ob.NOMBRE AND dbarrio.ID_MUNICIPIO = dmun.ID
            )
            INSERT INTO CALLEJERO.CALLE_BARRIO (ID_CALLE, ID_BARRIO, DESDE, HASTA, MANO)
            SELECT ID_CALLE, ID_BARRIO, DESDE, HASTA, MANO
            FROM Origen o
            WHERE RN = 1
              AND NOT EXISTS (
                SELECT 1 FROM CALLEJERO.CALLE_BARRIO d
                WHERE d.ID_CALLE = o.ID_CALLE AND d.ID_BARRIO = o.ID_BARRIO
            );';
        EXEC sp_executesql @sql;

        -- CALLE_ZONA (el índice es ID_ZONA + ID_CALLE; calle se resuelve por municipio+tipo de vía+nombre
        -- (la clave única real de CALLE) y zona por municipio+nombre. Se dedupe por la misma razón que CALLE_CP)
        SET @sql = N'
            ;WITH Origen AS (
                SELECT dcalle.ID AS ID_CALLE, dzona.ID AS ID_ZONA,
                       ROW_NUMBER() OVER (PARTITION BY dcalle.ID, dzona.ID ORDER BY (SELECT NULL)) AS RN
                FROM ' + @origen + N'.CALLEJERO.CALLE_ZONA o
                INNER JOIN ' + @origen + N'.CALLEJERO.CALLE oc ON oc.ID = o.ID_CALLE
                INNER JOIN ' + @origen + N'.CALLEJERO.ZONA oz ON oz.ID = o.ID_ZONA
                INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = oc.ID_MUNICIPIO
                INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
                INNER JOIN ' + @origen + N'.CALLEJERO.TIPO_VIA otv ON otv.ID = oc.ID_TIPO_DE_VIA
                INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
                INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
                INNER JOIN CALLEJERO.TIPO_VIA dtv ON dtv.SIGLA = otv.SIGLA
                INNER JOIN CALLEJERO.CALLE dcalle ON dcalle.NOMBRE = oc.NOMBRE AND dcalle.ID_MUNICIPIO = dmun.ID AND dcalle.ID_TIPO_DE_VIA = dtv.ID
                INNER JOIN CALLEJERO.ZONA dzona ON dzona.NOMBRE = oz.NOMBRE AND dzona.ID_MUNICIPIO = dmun.ID
            )
            INSERT INTO CALLEJERO.CALLE_ZONA (ID_CALLE, ID_ZONA)
            SELECT ID_CALLE, ID_ZONA
            FROM Origen o
            WHERE RN = 1
              AND NOT EXISTS (
                SELECT 1 FROM CALLEJERO.CALLE_ZONA d
                WHERE d.ID_CALLE = o.ID_CALLE AND d.ID_ZONA = o.ID_ZONA
            );';
        EXEC sp_executesql @sql;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Ejemplo de uso:
select Count(*) from CALLEJERO.CALLE

EXEC dbo.CopiarCallesDeCallejero @BdOrigen = 'SE_INTERIORISMO';
EXEC dbo.CopiarCallesDeCallejero @BdOrigen = 'SE_ARKENOS';
EXEC dbo.CopiarCallesDeCallejero @BdOrigen = 'SE_FUTURINES';
EXEC dbo.CopiarCallesDeCallejero @BdOrigen = 'SE_SECIFY';
EXEC dbo.CopiarCallesDeCallejero @BdOrigen = 'SE_TEST';

select Count(*) from CALLEJERO.CALLE
