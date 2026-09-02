-- Ejecutar contra cada una de las BD de negocio afectadas.
--
-- CALLEJERO.TieneReferenciaExterna: dado un objeto referenciado (@tablaReferenciada, 'ESQUEMA.TABLA'),
-- un @id y un @esquemaPropio, comprueba dinámicamente (vía sys.foreign_keys) si algún registro de una
-- tabla de OTRO esquema referencia ese @id. Devuelve @encontrada=1 y @detalle='ESQUEMA.TABLA.COLUMNA'
-- con la primera referencia que encuentra (no sigue buscando más).
CREATE OR ALTER PROCEDURE CALLEJERO.TieneReferenciaExterna
    @tablaReferenciada VARCHAR(261),
    @id INT,
    @esquemaPropio SYSNAME,
    @encontrada BIT OUTPUT,
    @detalle VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @encontrada = 0;
    SET @detalle = NULL;

    DECLARE @esquema SYSNAME, @tabla SYSNAME, @columna SYSNAME, @sql NVARCHAR(MAX), @existe BIT;

    DECLARE referencias CURSOR LOCAL FAST_FORWARD FOR
        SELECT SCHEMA_NAME(t.schema_id), t.name, c.name
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        INNER JOIN sys.tables t ON t.object_id = fkc.parent_object_id
        INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fk.referenced_object_id = OBJECT_ID(@tablaReferenciada)
          AND SCHEMA_NAME(t.schema_id) <> @esquemaPropio;

    OPEN referencias;
    FETCH NEXT FROM referencias INTO @esquema, @tabla, @columna;
    WHILE @@FETCH_STATUS = 0 AND @encontrada = 0
    BEGIN
        SET @existe = 0;
        SET @sql = N'SELECT @existe = CASE WHEN EXISTS (SELECT 1 FROM ' + QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabla)
                 + N' WHERE ' + QUOTENAME(@columna) + N' = @id) THEN 1 ELSE 0 END';
        EXEC sp_executesql @sql, N'@id INT, @existe BIT OUTPUT', @id, @existe OUTPUT;
        IF @existe = 1
        BEGIN
            SET @encontrada = 1;
            SET @detalle = @esquema + '.' + @tabla + '.' + @columna;
        END
        FETCH NEXT FROM referencias INTO @esquema, @tabla, @columna;
    END
    CLOSE referencias;
    DEALLOCATE referencias;
END
GO

-- CALLEJERO.BorrarReferenciasDelEsquema: dado un objeto referenciado (@tablaReferenciada) y un @id,
-- borra (dinámicamente, vía sys.foreign_keys) todas las filas de tablas del esquema @esquema que
-- referencien ese @id.
CREATE OR ALTER PROCEDURE CALLEJERO.BorrarReferenciasDelEsquema
    @tablaReferenciada VARCHAR(261),
    @id INT,
    @esquema SYSNAME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @esq SYSNAME, @tabla SYSNAME, @columna SYSNAME, @sql NVARCHAR(MAX);

    DECLARE referencias CURSOR LOCAL FAST_FORWARD FOR
        SELECT SCHEMA_NAME(t.schema_id), t.name, c.name
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        INNER JOIN sys.tables t ON t.object_id = fkc.parent_object_id
        INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fk.referenced_object_id = OBJECT_ID(@tablaReferenciada)
          AND SCHEMA_NAME(t.schema_id) = @esquema;

    OPEN referencias;
    FETCH NEXT FROM referencias INTO @esq, @tabla, @columna;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'DELETE FROM ' + QUOTENAME(@esq) + N'.' + QUOTENAME(@tabla) + N' WHERE ' + QUOTENAME(@columna) + N' = @id';
        EXEC sp_executesql @sql, N'@id INT', @id;
        FETCH NEXT FROM referencias INTO @esq, @tabla, @columna;
    END
    CLOSE referencias;
    DEALLOCATE referencias;
END
GO

-- CALLEJERO.DepurarMunicipiosAntiguos: recorre los municipios "antiguos"/mal importados (DC de longitud
-- distinta de 4, es decir CMUN+DC descuadrado) y, para cada uno:
--   1) Si el propio municipio, o alguna de sus calles, está referenciado desde una tabla de OTRO esquema
--      (direcciones de terceros, expedientes, facturas...), NO lo borra: hace PRINT indicando en qué
--      tabla está esa referencia (si es una calle, indicando cuál) y pasa al siguiente municipio.
--   2) Si no hay ninguna referencia fuera de CALLEJERO, borra en cascada dentro de CALLEJERO (por cada
--      calle del municipio: sus CALLE_CP/CALLE_BARRIO/CALLE_ZONA y la calle; luego, del propio
--      municipio: BARRIO, MUNICIPIO_CP y cualquier otra tabla de CALLEJERO con FK al municipio) y borra
--      el municipio. Cada municipio se procesa en su propia transacción: si algo falla inesperadamente
--      se deshace solo ese municipio y se continúa con el resto.
--
-- Es seguro relanzarlo: solo actúa sobre municipios con DC mal formado, y dentro de una transacción por
-- municipio, así que los que ya se hayan borrado (o bloqueado) en una ejecución anterior no se repiten
-- (los borrados ya no existen; los bloqueados se vuelven a evaluar, y volverán a bloquear si la
-- referencia externa sigue ahí).
CREATE OR ALTER PROCEDURE CALLEJERO.DepurarMunicipiosAntiguos
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idMunicipio INT, @nombreMunicipio VARCHAR(250), @codigoProvincia VARCHAR(2), @dc VARCHAR(5);
    DECLARE municipios CURSOR LOCAL FAST_FORWARD FOR
        SELECT m.ID, m.NOMBRE, p.CODIGO, m.DC
        FROM CALLEJERO.MUNICIPIO m
        INNER JOIN CALLEJERO.PROVINCIA p ON p.ID = m.ID_PROVINCIA
        WHERE LEN(m.DC) <> 4;

    OPEN municipios;
    FETCH NEXT FROM municipios INTO @idMunicipio, @nombreMunicipio, @codigoProvincia, @dc;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @bloqueado BIT = 0, @encontrada BIT, @detalle VARCHAR(500);

        -- ¿El municipio está referenciado directamente fuera de CALLEJERO?
        EXEC CALLEJERO.TieneReferenciaExterna 'CALLEJERO.MUNICIPIO', @idMunicipio, 'CALLEJERO', @encontrada OUTPUT, @detalle OUTPUT;
        IF @encontrada = 1
        BEGIN
            PRINT 'Municipio ''' + @nombreMunicipio + ''' (ID ' + CAST(@idMunicipio AS VARCHAR(10)) + ', provincia ' + @codigoProvincia + ', dc ''' + @dc + ''') NO se borra: referenciado en ' + @detalle;
            SET @bloqueado = 1;
        END

        -- ¿Alguna de sus calles está referenciada fuera de CALLEJERO?
        IF @bloqueado = 0
        BEGIN
            DECLARE @idCalle INT, @nombreCalle VARCHAR(250);
            DECLARE calles CURSOR LOCAL FAST_FORWARD FOR
                SELECT ID, NOMBRE FROM CALLEJERO.CALLE WHERE ID_MUNICIPIO = @idMunicipio;
            OPEN calles;
            FETCH NEXT FROM calles INTO @idCalle, @nombreCalle;
            WHILE @@FETCH_STATUS = 0 AND @bloqueado = 0
            BEGIN
                EXEC CALLEJERO.TieneReferenciaExterna 'CALLEJERO.CALLE', @idCalle, 'CALLEJERO', @encontrada OUTPUT, @detalle OUTPUT;
                IF @encontrada = 1
                BEGIN
                    PRINT 'Municipio ''' + @nombreMunicipio + ''' (ID ' + CAST(@idMunicipio AS VARCHAR(10)) + ') NO se borra: la calle ''' + @nombreCalle + ''' (ID ' + CAST(@idCalle AS VARCHAR(10)) + ') está referenciada en ' + @detalle;
                    SET @bloqueado = 1;
                END
                FETCH NEXT FROM calles INTO @idCalle, @nombreCalle;
            END
            CLOSE calles;
            DEALLOCATE calles;
        END

        -- Sin referencias externas: borrar en cascada dentro de CALLEJERO
        IF @bloqueado = 0
        BEGIN
            BEGIN TRY
                BEGIN TRANSACTION;

                DECLARE @idCalleABorrar INT;
                DECLARE callesABorrar CURSOR LOCAL FAST_FORWARD FOR
                    SELECT ID FROM CALLEJERO.CALLE WHERE ID_MUNICIPIO = @idMunicipio;
                OPEN callesABorrar;
                FETCH NEXT FROM callesABorrar INTO @idCalleABorrar;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    EXEC CALLEJERO.BorrarReferenciasDelEsquema 'CALLEJERO.CALLE', @idCalleABorrar, 'CALLEJERO';
                    DELETE FROM CALLEJERO.CALLE WHERE ID = @idCalleABorrar;

                    FETCH NEXT FROM callesABorrar INTO @idCalleABorrar;
                END
                CLOSE callesABorrar;
                DEALLOCATE callesABorrar;

                EXEC CALLEJERO.BorrarReferenciasDelEsquema 'CALLEJERO.MUNICIPIO', @idMunicipio, 'CALLEJERO';
                DELETE FROM CALLEJERO.MUNICIPIO WHERE ID = @idMunicipio;

                COMMIT TRANSACTION;

                PRINT 'Municipio ''' + @nombreMunicipio + ''' (ID ' + CAST(@idMunicipio AS VARCHAR(10)) + ', provincia ' + @codigoProvincia + ', dc ''' + @dc + ''') borrado junto con sus calles y referencias de CALLEJERO';
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                PRINT 'Municipio ''' + @nombreMunicipio + ''' (ID ' + CAST(@idMunicipio AS VARCHAR(10)) + ') NO se ha podido borrar: ' + ERROR_MESSAGE();
            END CATCH
        END

        FETCH NEXT FROM municipios INTO @idMunicipio, @nombreMunicipio, @codigoProvincia, @dc;
    END
    CLOSE municipios;
    DEALLOCATE municipios;
END
GO

-- ================== EJECUCIÓN ==================
-- EXEC CALLEJERO.DepurarMunicipiosAntiguos;
