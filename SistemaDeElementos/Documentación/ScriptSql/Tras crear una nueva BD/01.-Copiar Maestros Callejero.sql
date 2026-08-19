CREATE OR ALTER PROCEDURE dbo.CopiarMaestrosDeCallejero
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

        -- CODIGO_POSTAL
        SET @sql = N'
            INSERT INTO CALLEJERO.CODIGO_POSTAL (CP)
            SELECT o.CP
            FROM ' + @origen + N'.CALLEJERO.CODIGO_POSTAL o
            WHERE NOT EXISTS (SELECT 1 FROM CALLEJERO.CODIGO_POSTAL d WHERE d.CP = o.CP);';
        EXEC sp_executesql @sql;

        -- TIPO_VIA
        SET @sql = N'
            INSERT INTO CALLEJERO.TIPO_VIA (SIGLA, NOMBRE)
            SELECT o.SIGLA, ENTORNO.CapitalizarFrase(o.NOMBRE)
            FROM ' + @origen + N'.CALLEJERO.TIPO_VIA o
            WHERE NOT EXISTS (SELECT 1 FROM CALLEJERO.TIPO_VIA d WHERE d.SIGLA = o.SIGLA);';
        EXEC sp_executesql @sql;

        -- PAIS (CODIGO e ISO2 son ambos únicos en la tabla)
        SET @sql = N'
            INSERT INTO CALLEJERO.PAIS (NOMBRE_INGLES, CODIGO, ISO2, PREFIJO, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR, ES_UE)
            SELECT ENTORNO.CapitalizarFrase(o.NOMBRE_INGLES), o.CODIGO, o.ISO2, o.PREFIJO, ENTORNO.CapitalizarFrase(o.NOMBRE), GETDATE(), 1, NULL, NULL, o.ES_UE
            FROM ' + @origen + N'.CALLEJERO.PAIS o
            WHERE NOT EXISTS (SELECT 1 FROM CALLEJERO.PAIS d WHERE d.CODIGO = o.CODIGO)
              AND NOT EXISTS (SELECT 1 FROM CALLEJERO.PAIS d WHERE d.ISO2 = o.ISO2);';
        EXEC sp_executesql @sql;

        -- PROVINCIA (CODIGO y SIGLA son ambos únicos en la tabla; resuelve ID_PAIS por CODIGO en vez de asumir que España es siempre el ID 1)
        SET @sql = N'
            INSERT INTO CALLEJERO.PROVINCIA (CODIGO, SIGLA, PREFIJO, ID_PAIS, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR)
            SELECT o.CODIGO, o.SIGLA, o.PREFIJO, dpais.ID, ENTORNO.CapitalizarFrase(o.NOMBRE), GETDATE(), 1, NULL, NULL
            FROM ' + @origen + N'.CALLEJERO.PROVINCIA o
            INNER JOIN ' + @origen + N'.CALLEJERO.PAIS opais ON opais.ID = o.ID_PAIS
            INNER JOIN CALLEJERO.PAIS dpais ON dpais.CODIGO = opais.CODIGO
            WHERE NOT EXISTS (SELECT 1 FROM CALLEJERO.PROVINCIA d WHERE d.CODIGO = o.CODIGO)
              AND NOT EXISTS (SELECT 1 FROM CALLEJERO.PROVINCIA d WHERE d.SIGLA = o.SIGLA);';
        EXEC sp_executesql @sql;

        -- PROVINCIA_CP
        SET @sql = N'
            INSERT INTO CALLEJERO.PROVINCIA_CP (ID_PROVINCIA, ID_CP)
            SELECT dprov.ID, dcp.ID
            FROM ' + @origen + N'.CALLEJERO.PROVINCIA_CP o
            INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = o.ID_PROVINCIA
            INNER JOIN ' + @origen + N'.CALLEJERO.CODIGO_POSTAL ocp ON ocp.ID = o.ID_CP
            INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
            INNER JOIN CALLEJERO.CODIGO_POSTAL dcp ON dcp.CP = ocp.CP
            WHERE NOT EXISTS (
                SELECT 1 FROM CALLEJERO.PROVINCIA_CP d
                WHERE d.ID_PROVINCIA = dprov.ID AND d.ID_CP = dcp.ID
            );';
        EXEC sp_executesql @sql;

        -- MUNICIPIO
        SET @sql = N'
            INSERT INTO CALLEJERO.MUNICIPIO (DC, ID_PROVINCIA, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR)
            SELECT o.DC, dprov.ID, ENTORNO.CapitalizarFrase(o.NOMBRE), GETDATE(), 1, NULL, NULL
            FROM ' + @origen + N'.CALLEJERO.MUNICIPIO o
            INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = o.ID_PROVINCIA
            INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
            WHERE NOT EXISTS (
                SELECT 1 FROM CALLEJERO.MUNICIPIO d
                WHERE d.NOMBRE = o.NOMBRE AND d.ID_PROVINCIA = dprov.ID
            );';
        EXEC sp_executesql @sql;

        -- MUNICIPIO_CP
        SET @sql = N'
            INSERT INTO CALLEJERO.MUNICIPIO_CP (ID_MUNICIPIO, ID_CP)
            SELECT dmun.ID, dcp.ID
            FROM ' + @origen + N'.CALLEJERO.MUNICIPIO_CP o
            INNER JOIN ' + @origen + N'.CALLEJERO.MUNICIPIO om ON om.ID = o.ID_MUNICIPIO
            INNER JOIN ' + @origen + N'.CALLEJERO.PROVINCIA op ON op.ID = om.ID_PROVINCIA
            INNER JOIN ' + @origen + N'.CALLEJERO.CODIGO_POSTAL ocp ON ocp.ID = o.ID_CP
            INNER JOIN CALLEJERO.PROVINCIA dprov ON dprov.NOMBRE = op.NOMBRE
            INNER JOIN CALLEJERO.MUNICIPIO dmun ON dmun.NOMBRE = om.NOMBRE AND dmun.ID_PROVINCIA = dprov.ID
            INNER JOIN CALLEJERO.CODIGO_POSTAL dcp ON dcp.CP = ocp.CP
            WHERE NOT EXISTS (
                SELECT 1 FROM CALLEJERO.MUNICIPIO_CP d
                WHERE d.ID_MUNICIPIO = dmun.ID AND d.ID_CP = dcp.ID
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
EXEC dbo.CopiarMaestrosDeCallejero @BdOrigen = 'SE_INTERIORISMO';
EXEC dbo.CopiarMaestrosDeCallejero @BdOrigen = 'SE_ARKENOS';
EXEC dbo.CopiarMaestrosDeCallejero @BdOrigen = 'SE_SECIFY';
