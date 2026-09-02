-- Ejecutar contra cada una de las BD de negocio.
-- Requiere que exista ENTORNO.CapitalizarFrase (ver "Tras crear una nueva BD\00.-CapitalizarFrase.sql").
-- Es seguro relanzarlo: si un NOMBRE ya está capitalizado, no lo toca.

CREATE OR ALTER PROCEDURE CALLEJERO.CapitalizarMunicipiosYProvincias
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE p
        SET p.NOMBRE = ENTORNO.CapitalizarFrase(p.NOMBRE),
            p.FECMOD = GETDATE(),
            p.ID_MODIFICADOR = 1
        FROM CALLEJERO.PROVINCIA p
        WHERE p.NOMBRE COLLATE Latin1_General_BIN <> ENTORNO.CapitalizarFrase(p.NOMBRE) COLLATE Latin1_General_BIN;

        UPDATE m
        SET m.NOMBRE = ENTORNO.CapitalizarFrase(m.NOMBRE),
            m.FECMOD = GETDATE(),
            m.ID_MODIFICADOR = 1
        FROM CALLEJERO.MUNICIPIO m
        WHERE m.NOMBRE COLLATE Latin1_General_BIN <> ENTORNO.CapitalizarFrase(m.NOMBRE) COLLATE Latin1_General_BIN;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ================== EJECUCIÓN ==================
EXEC CALLEJERO.CapitalizarMunicipiosYProvincias;

SELECT ID, NOMBRE FROM CALLEJERO.PROVINCIA WHERE NOMBRE COLLATE Latin1_General_BIN <> ENTORNO.CapitalizarFrase(NOMBRE) COLLATE Latin1_General_BIN;
SELECT ID, NOMBRE FROM CALLEJERO.MUNICIPIO WHERE NOMBRE COLLATE Latin1_General_BIN <> ENTORNO.CapitalizarFrase(NOMBRE) COLLATE Latin1_General_BIN;
