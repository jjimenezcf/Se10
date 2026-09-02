-- Ejecutar contra cada una de las BD de negocio afectadas.
--
-- CALLEJERO.RedirigirReferencias: helper genérico y reutilizable. Dado un objeto referenciado
-- (@tablaReferenciada, 'ESQUEMA.TABLA') y dos IDs, descubre dinámicamente (vía sys.foreign_keys) TODAS
-- las tablas/columnas que tengan una FK hacia ese objeto y les hace UPDATE de @idAntiguo a @idNuevo.
-- No hay que mantener a mano la lista de tablas: si mañana aparece una tabla nueva con FK hacia el
-- objeto referenciado, se recoge sola.
CREATE OR ALTER PROCEDURE CALLEJERO.RedirigirReferencias
    @tablaReferenciada VARCHAR(261), -- 'ESQUEMA.TABLA', p.ej. 'CALLEJERO.MUNICIPIO' o 'CALLEJERO.CALLE'
    @idAntiguo INT,
    @idNuevo INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @esquema SYSNAME, @tabla SYSNAME, @columna SYSNAME, @sql NVARCHAR(MAX);

    DECLARE referencias CURSOR LOCAL FAST_FORWARD FOR
        SELECT SCHEMA_NAME(t.schema_id), t.name, c.name
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        INNER JOIN sys.tables t ON t.object_id = fkc.parent_object_id
        INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fk.referenced_object_id = OBJECT_ID(@tablaReferenciada);

    OPEN referencias;
    FETCH NEXT FROM referencias INTO @esquema, @tabla, @columna;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'UPDATE ' + QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabla)
                 + N' SET ' + QUOTENAME(@columna) + N' = @idNuevo'
                 + N' WHERE ' + QUOTENAME(@columna) + N' = @idAntiguo';
        EXEC sp_executesql @sql, N'@idNuevo INT, @idAntiguo INT', @idNuevo, @idAntiguo;

        FETCH NEXT FROM referencias INTO @esquema, @tabla, @columna;
    END
    CLOSE referencias;
    DEALLOCATE referencias;
END
GO

-- Requiere que exista CALLEJERO.FusionarCalle (ver "Unificar tipos de via y calles duplicadas.sql").
--
-- Corrige municipios mal importados (p.ej. por desalineación de columnas al leer el Excel del INE):
-- localiza el municipio incorrecto (b_*), localiza su equivalente correcto (e_*).
--
-- 1) Fusiona primero las CALLES: si una calle del municipio a borrar coincide (mismo tipo de vía +
--    nombre) con una que ya existe en el municipio equivalente, no se puede simplemente reasignar su
--    ID_MUNICIPIO (violaría el índice único I_CALLE_ID_MUNICIPIO_ID_TIPO_DE_VIA_NOMBRE); se fusiona con
--    CALLEJERO.FusionarCalle (que ya sabe mover CALLE_CP/CALLE_BARRIO/CALLE_ZONA solo cuando el destino
--    no los tiene ya, y repunta las 19 tablas de direcciones) y se borra la calle duplicada.
-- 2) Redirige el resto de referencias (las calles restantes -sin duplicado-, juzgados, códigos
--    postales del municipio, etc.) hacia el municipio equivalente, descubriendo dinámicamente (vía
--    CALLEJERO.RedirigirReferencias) todas las tablas con FK a CALLEJERO.MUNICIPIO.
-- 3) Borra el municipio incorrecto.
--
-- Si aparece OTRA violación de índice único en una tabla no cubierta por FusionarCalle ni por el
-- descubrimiento dinámico de FKs, hay que tratarla con el mismo patrón: localizar el duplicado y
-- fusionarlo (borrar uno, redirigir sus referencias al otro) antes de borrar el municipio.
CREATE OR ALTER PROCEDURE CALLEJERO.BorrarMunicipio
    @b_cpro     VARCHAR(2),
    @b_cmuni_dc VARCHAR(5),
    @b_nombre   VARCHAR(250),
    @e_cpro     VARCHAR(2),
    @e_cmuni_dc VARCHAR(5)
AS
BEGIN
    SET NOCOUNT ON;

    SET @b_cpro = RIGHT('0' + LTRIM(RTRIM(@b_cpro)), 2);
    SET @e_cpro = RIGHT('0' + LTRIM(RTRIM(@e_cpro)), 2);
    SET @b_cmuni_dc = LTRIM(RTRIM(@b_cmuni_dc));
    SET @e_cmuni_dc = LTRIM(RTRIM(@e_cmuni_dc));
    SET @b_nombre = LTRIM(RTRIM(@b_nombre));

    DECLARE @idBorrar INT, @idEquivalente INT;

    SELECT @idBorrar = m.ID
    FROM CALLEJERO.MUNICIPIO m
    INNER JOIN CALLEJERO.PROVINCIA p ON p.ID = m.ID_PROVINCIA
    WHERE p.CODIGO = @b_cpro
      AND m.DC = @b_cmuni_dc
      AND UPPER(m.NOMBRE) = UPPER(@b_nombre);

    IF @idBorrar IS NULL
    BEGIN
        PRINT 'No se ha encontrado el municipio a borrar: provincia ' + @b_cpro + ', dc ' + @b_cmuni_dc + ', nombre ''' + @b_nombre + '''';
        RETURN;
    END

    SELECT @idEquivalente = m.ID
    FROM CALLEJERO.MUNICIPIO m
    INNER JOIN CALLEJERO.PROVINCIA p ON p.ID = m.ID_PROVINCIA
    WHERE p.CODIGO = @e_cpro
      AND m.DC = @e_cmuni_dc;

    IF @idEquivalente IS NULL
    BEGIN
        PRINT 'No se ha encontrado el municipio equivalente: provincia ' + @e_cpro + ', dc ' + @e_cmuni_dc;
        RETURN;
    END

    IF @idEquivalente = @idBorrar
    BEGIN
        PRINT 'El municipio a borrar y el equivalente son el mismo registro (ID ' + CAST(@idBorrar AS VARCHAR(10)) + '); no hay nada que hacer';
        RETURN;
    END

    DECLARE @callesCursorAbierto BIT = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1) Fusionar calles duplicadas (misma vía+nombre ya existente en el municipio equivalente)
        DECLARE @idCalleMala INT, @idCalleBuena INT;
        DECLARE callesDuplicadas CURSOR LOCAL FAST_FORWARD FOR
            SELECT cMala.ID, cBuena.ID
            FROM CALLEJERO.CALLE cMala
            INNER JOIN CALLEJERO.CALLE cBuena
                    ON cBuena.ID_MUNICIPIO = @idEquivalente
                   AND cBuena.ID_TIPO_DE_VIA = cMala.ID_TIPO_DE_VIA
                   AND UPPER(cBuena.NOMBRE) = UPPER(cMala.NOMBRE)
            WHERE cMala.ID_MUNICIPIO = @idBorrar;

        OPEN callesDuplicadas;
        SET @callesCursorAbierto = 1;
        FETCH NEXT FROM callesDuplicadas INTO @idCalleMala, @idCalleBuena;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC CALLEJERO.FusionarCalle @IdOrigen = @idCalleMala, @IdDestino = @idCalleBuena;

            FETCH NEXT FROM callesDuplicadas INTO @idCalleMala, @idCalleBuena;
        END
        CLOSE callesDuplicadas;
        DEALLOCATE callesDuplicadas;
        SET @callesCursorAbierto = 0;

        -- 2) Redirigir el resto de referencias (incluidas las calles restantes, ya sin duplicado) al municipio equivalente
        EXEC CALLEJERO.RedirigirReferencias 'CALLEJERO.MUNICIPIO', @idBorrar, @idEquivalente;

        -- 3) Borrar el municipio incorrecto
        DELETE FROM CALLEJERO.MUNICIPIO WHERE ID = @idBorrar;

        COMMIT TRANSACTION;

        PRINT 'Municipio ''' + @b_nombre + ''' (ID ' + CAST(@idBorrar AS VARCHAR(10)) + ') borrado; sus referencias se han redirigido al municipio equivalente (ID ' + CAST(@idEquivalente AS VARCHAR(10)) + ')';
    END TRY
    BEGIN CATCH
        IF @callesCursorAbierto = 1
        BEGIN
            CLOSE callesDuplicadas;
            DEALLOCATE callesDuplicadas;
        END
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ================== EJEMPLO DE EJECUCIÓN ==================
-- Sustituir por los datos reales de cada fila incorrecta que se quiera fusionar/borrar.
-- Referencia (comprobada contra 26codmun.xlsx): Molina de Segura = CPRO 30, CMUN 027, DC 5 -> DC almacenado '0275'
--                                                Mazarrón         = CPRO 30, CMUN 026, DC 9 -> DC almacenado '0269'
-- EXEC CALLEJERO.BorrarMunicipio
--      @b_cpro = '30', @b_cmuni_dc = '30860', @b_nombre = 'Puerto de Mazarrón',
--      @e_cpro = '30', @e_cmuni_dc = '0269';   -- <-- el municipio equivalente real al que se deban redirigir las referencias

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30860', @b_nombre = 'Puerto de Mazarrón',
     @e_cpro = '30', @e_cmuni_dc = '0269';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30565', @b_nombre = 'LAS TORRES DE COTILLAS',
     @e_cpro = '30', @e_cmuni_dc = '0389';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30710', @b_nombre = 'LOS ALCAZARES',
     @e_cpro = '30', @e_cmuni_dc = '9027';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30150', @b_nombre = 'Alberca de las Torres',
     @e_cpro = '30', @e_cmuni_dc = '0308';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30430', @b_nombre = 'Cehegin',
     @e_cpro = '30', @e_cmuni_dc = '0177';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30508', @b_nombre = 'Ribera de Molina',
     @e_cpro = '30', @e_cmuni_dc = '0275';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '5', @b_nombre = 'Molina',
     @e_cpro = '30', @e_cmuni_dc = '0275';

	 
EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '162', @b_nombre = 'HUERTA DE SANTA CRUZ',
     @e_cpro = '30', @e_cmuni_dc = '0308';
      
	 
EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '01', @b_nombre = 'La Cueva - Monteagudo',
     @e_cpro = '30', @e_cmuni_dc = '0308';

	 	 
EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '01', @b_nombre = 'Roldan',
     @e_cpro = '30', @e_cmuni_dc = '0373';


	 EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '169', @b_nombre = 'SAN GINES',
     @e_cpro = '30', @e_cmuni_dc = '0308';

