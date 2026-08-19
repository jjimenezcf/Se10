CREATE OR ALTER PROCEDURE dbo.CopiarMaestrosBancarios
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

        -- TERCEROS.BANCO ((ID_PAIS, CODIGO), BIC_SWIFT y NOMBRE son únicos en la tabla)
        SET @sql = N'
            INSERT INTO TERCEROS.BANCO (ID_PAIS, BIC_SWIFT, CODIGO, NOMBRE, FECCRE, ID_CREADOR, FECMOD, ID_MODIFICADOR)
            SELECT dpais.ID, o.BIC_SWIFT, o.CODIGO, ENTORNO.CapitalizarFrase(o.NOMBRE), GETDATE(), 1, NULL, NULL
            FROM ' + @origen + N'.TERCEROS.BANCO o
            CROSS JOIN (SELECT ID FROM CALLEJERO.PAIS WHERE NOMBRE = ''ESPAÑA'') dpais
            WHERE NOT EXISTS (SELECT 1 FROM TERCEROS.BANCO d WHERE d.ID_PAIS = dpais.ID AND d.CODIGO = o.CODIGO)
              AND NOT EXISTS (SELECT 1 FROM TERCEROS.BANCO d WHERE d.BIC_SWIFT = o.BIC_SWIFT)
              AND NOT EXISTS (SELECT 1 FROM TERCEROS.BANCO d WHERE d.NOMBRE = ENTORNO.CapitalizarFrase(o.NOMBRE));';
        EXEC sp_executesql @sql;

        -- CONTABILIDAD.CUENTA (CODIGO y NOMBRE son ambos únicos en la tabla)
        SET @sql = N'
            INSERT INTO CONTABILIDAD.CUENTA (CODIGO, NOMBRE)
            SELECT o.CODIGO, ENTORNO.CapitalizarFrase(o.NOMBRE)
            FROM ' + @origen + N'.CONTABILIDAD.CUENTA o
            WHERE NOT EXISTS (SELECT 1 FROM CONTABILIDAD.CUENTA d WHERE d.CODIGO = o.CODIGO)
              AND NOT EXISTS (SELECT 1 FROM CONTABILIDAD.CUENTA d WHERE d.NOMBRE = ENTORNO.CapitalizarFrase(o.NOMBRE));';
        EXEC sp_executesql @sql;

        -- MT.MT_NATURALEZA (NOMBRE es único en la tabla; resuelve las cuentas de gasto/ingreso por CODIGO, no por ID)
        SET @sql = N'
            INSERT INTO MT.MT_NATURALEZA (SIGLA, ID_CUENTA_GASTO, ID_CUENTA_INGRESO, NOMBRE)
            SELECT o.SIGLA, dcg.ID, dci.ID, ENTORNO.CapitalizarFrase(o.NOMBRE)
            FROM ' + @origen + N'.MT.MT_NATURALEZA o
            LEFT JOIN ' + @origen + N'.CONTABILIDAD.CUENTA ocg ON ocg.ID = o.ID_CUENTA_GASTO
            LEFT JOIN CONTABILIDAD.CUENTA dcg ON dcg.CODIGO = ocg.CODIGO
            LEFT JOIN ' + @origen + N'.CONTABILIDAD.CUENTA oci ON oci.ID = o.ID_CUENTA_INGRESO
            LEFT JOIN CONTABILIDAD.CUENTA dci ON dci.CODIGO = oci.CODIGO
            WHERE NOT EXISTS (SELECT 1 FROM MT.MT_NATURALEZA d WHERE d.NOMBRE = o.NOMBRE);';
        EXEC sp_executesql @sql;

        -- SEGURIDAD.CLASE_PERMISO
        SET @sql = N'
            INSERT INTO SEGURIDAD.CLASE_PERMISO (NOMBRE)
            SELECT ENTORNO.CapitalizarFrase(o.NOMBRE)
            FROM ' + @origen + N'.SEGURIDAD.CLASE_PERMISO o
            WHERE NOT EXISTS (SELECT 1 FROM SEGURIDAD.CLASE_PERMISO d WHERE d.NOMBRE = o.NOMBRE);';
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
-- EXEC dbo.CopiarMaestrosBancarios @BdOrigen = 'SE_INTERIORISMO';
