-- Ejecutar contra cada una de las 8 BD de negocio (empezando por SE_JHM).
-- Orden: 1) crea los 3 PA de este fichero, 2) lanza los EXEC del final.
-- Es seguro relanzarlo: cada PA es idempotente (si no encuentra duplicados/pares, no hace nada).

CREATE OR ALTER PROCEDURE CALLEJERO.UnificarTiposDeVia
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Un "ganador" por cada NOMBRE de tipo de vía duplicado: el que ya tiene más calles.
        -- Colegio y Carril llevan sigla forzada; el resto se capitaliza (1ª mayúscula, resto minúsculas).
        DECLARE @Ganadores TABLE (
            NOMBRE VARCHAR(250) PRIMARY KEY,
            ID_GANADOR INT,
            SIGLA_FINAL VARCHAR(10)
        );

        ;WITH Conteo AS (
            SELECT t.ID, t.NOMBRE, t.SIGLA,
                   COUNT(c.ID) AS NumCalles,
                   ROW_NUMBER() OVER (PARTITION BY t.NOMBRE ORDER BY COUNT(c.ID) DESC, t.ID ASC) AS Orden
            FROM CALLEJERO.TIPO_VIA t
            LEFT JOIN CALLEJERO.CALLE c ON c.ID_TIPO_DE_VIA = t.ID
            WHERE t.NOMBRE IN (SELECT NOMBRE FROM CALLEJERO.TIPO_VIA GROUP BY NOMBRE HAVING COUNT(*) > 1)
            GROUP BY t.ID, t.NOMBRE, t.SIGLA
        )
        INSERT INTO @Ganadores (NOMBRE, ID_GANADOR, SIGLA_FINAL)
        SELECT NOMBRE, ID,
               CASE NOMBRE
                   WHEN 'Colegio' THEN 'Clg'
                   WHEN 'Carril'  THEN 'Crr'
                   ELSE UPPER(LEFT(SIGLA,1)) + LOWER(SUBSTRING(SIGLA,2,LEN(SIGLA)))
               END
        FROM Conteo
        WHERE Orden = 1;

        -- 1) Repuntar las calles del perdedor al ganador, solo si no colisiona con una ya existente
        UPDATE c
        SET c.ID_TIPO_DE_VIA = g.ID_GANADOR
        FROM CALLEJERO.CALLE c
        INNER JOIN CALLEJERO.TIPO_VIA t ON t.ID = c.ID_TIPO_DE_VIA
        INNER JOIN @Ganadores g ON g.NOMBRE = t.NOMBRE
        WHERE t.ID <> g.ID_GANADOR
          AND NOT EXISTS (
              SELECT 1 FROM CALLEJERO.CALLE d
              WHERE d.ID_TIPO_DE_VIA = g.ID_GANADOR
                AND d.ID_MUNICIPIO = c.ID_MUNICIPIO
                AND d.NOMBRE = c.NOMBRE
          );

        -- 2) Borrar los tipos de vía perdedores que ya se han quedado sin calles
        DELETE t
        FROM CALLEJERO.TIPO_VIA t
        INNER JOIN @Ganadores g ON g.NOMBRE = t.NOMBRE
        WHERE t.ID <> g.ID_GANADOR
          AND NOT EXISTS (SELECT 1 FROM CALLEJERO.CALLE c WHERE c.ID_TIPO_DE_VIA = t.ID);

        -- 3) Renombrar la sigla del ganador
        UPDATE t
        SET t.SIGLA = g.SIGLA_FINAL
        FROM CALLEJERO.TIPO_VIA t
        INNER JOIN @Ganadores g ON g.ID_GANADOR = t.ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE CALLEJERO.FusionarCalle
    @IdOrigen INT,
    @IdDestino INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @IdOrigen = @IdDestino RETURN;

    -- Relaciones con rango (CP / Barrio / Zona): mover solo si el destino no cubre ya ese CP/Barrio/Zona
    UPDATE o SET o.ID_CALLE = @IdDestino
    FROM CALLEJERO.CALLE_CP o
    WHERE o.ID_CALLE = @IdOrigen
      AND NOT EXISTS (SELECT 1 FROM CALLEJERO.CALLE_CP d WHERE d.ID_CALLE = @IdDestino AND d.ID_CP = o.ID_CP);
    DELETE FROM CALLEJERO.CALLE_CP WHERE ID_CALLE = @IdOrigen;

    UPDATE o SET o.ID_CALLE = @IdDestino
    FROM CALLEJERO.CALLE_BARRIO o
    WHERE o.ID_CALLE = @IdOrigen
      AND NOT EXISTS (SELECT 1 FROM CALLEJERO.CALLE_BARRIO d WHERE d.ID_CALLE = @IdDestino AND d.ID_BARRIO = o.ID_BARRIO);
    DELETE FROM CALLEJERO.CALLE_BARRIO WHERE ID_CALLE = @IdOrigen;

    UPDATE o SET o.ID_CALLE = @IdDestino
    FROM CALLEJERO.CALLE_ZONA o
    WHERE o.ID_CALLE = @IdOrigen
      AND NOT EXISTS (SELECT 1 FROM CALLEJERO.CALLE_ZONA d WHERE d.ID_CALLE = @IdDestino AND d.ID_ZONA = o.ID_ZONA);
    DELETE FROM CALLEJERO.CALLE_ZONA WHERE ID_CALLE = @IdOrigen;

    -- Las 19 tablas de direcciones (sin restricciones de unicidad sobre ID_CALLE, repunte directo)
    UPDATE TERCEROS.ABOGADO_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.INTERLOCUTOR_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.PERSONA_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.SOCIEDAD_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.CLIENTE_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE EXPEDIENTE.EXPEDIENTE_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.PROCURADOR_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE REGISTRO.REGISTRO_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.TRABAJADOR_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TERCEROS.PROVEEDOR_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE JURIDICO.CONTRATO_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE PRESUPUESTO.PRESUPUESTO_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE JURIDICO.PLEITO_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE GASTO.FACTURA_REC_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE LOGISTICA.PEDIDO_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE VENTA.FACTURA_EMT_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE VENTA.PARTE_TR_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE TAREA.TAREA_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;
    UPDATE VENTA.PLANIFICACION_VENTA_DIRECCION SET ID_CALLE = @IdDestino WHERE ID_CALLE = @IdOrigen;

    DELETE FROM CALLEJERO.CALLE WHERE ID = @IdOrigen;
END
GO

CREATE OR ALTER PROCEDURE CALLEJERO.UnificarCarrilEnCarretera
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Pares TABLE (IdOrigen INT PRIMARY KEY, IdDestino INT);

        ;WITH Emparejados AS (
            SELECT ccl.ID AS IdOrigen, cct.ID AS IdDestino,
                   ROW_NUMBER() OVER (PARTITION BY ccl.ID ORDER BY cct.ID) AS rn
            FROM CALLEJERO.CALLE ccl
            INNER JOIN CALLEJERO.TIPO_VIA tcl ON tcl.ID = ccl.ID_TIPO_DE_VIA AND tcl.NOMBRE = 'Carril'
            INNER JOIN CALLEJERO.CALLE cct ON cct.ID_MUNICIPIO = ccl.ID_MUNICIPIO AND cct.NOMBRE = ccl.NOMBRE
            INNER JOIN CALLEJERO.TIPO_VIA tct ON tct.ID = cct.ID_TIPO_DE_VIA AND tct.NOMBRE = 'Carretera'
        )
        INSERT INTO @Pares (IdOrigen, IdDestino)
        SELECT IdOrigen, IdDestino FROM Emparejados WHERE rn = 1;

        DECLARE @IdOrigen INT, @IdDestino INT;
        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT IdOrigen, IdDestino FROM @Pares;
        OPEN cur;
        FETCH NEXT FROM cur INTO @IdOrigen, @IdDestino;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC CALLEJERO.FusionarCalle @IdOrigen = @IdOrigen, @IdDestino = @IdDestino;
            FETCH NEXT FROM cur INTO @IdOrigen, @IdDestino;
        END
        CLOSE cur; DEALLOCATE cur;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE CALLEJERO.UnificarCallesDuplicadas
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Ganador por cada (ID_MUNICIPIO, NOMBRE) duplicado: el ID más bajo (el más antiguo)
        DECLARE @Pares TABLE (IdOrigen INT PRIMARY KEY, IdDestino INT);

        ;WITH Grupos AS (
            SELECT ID,
                   MIN(ID) OVER (PARTITION BY ID_MUNICIPIO, NOMBRE) AS IdGanador
            FROM CALLEJERO.CALLE
        )
        INSERT INTO @Pares (IdOrigen, IdDestino)
        SELECT ID, IdGanador
        FROM Grupos
        WHERE ID <> IdGanador;

        DECLARE @IdOrigen INT, @IdDestino INT;
        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT IdOrigen, IdDestino FROM @Pares;
        OPEN cur;
        FETCH NEXT FROM cur INTO @IdOrigen, @IdDestino;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC CALLEJERO.FusionarCalle @IdOrigen = @IdOrigen, @IdDestino = @IdDestino;
            FETCH NEXT FROM cur INTO @IdOrigen, @IdDestino;
        END
        CLOSE cur; DEALLOCATE cur;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ================== EJECUCIÓN (contra SE_JHM) ==================
EXEC CALLEJERO.UnificarTiposDeVia;
EXEC CALLEJERO.UnificarCarrilEnCarretera;
EXEC CALLEJERO.UnificarCallesDuplicadas;

SELECT Repeticiones, COUNT(*) AS NumGrupos
FROM (
    SELECT ID_MUNICIPIO, NOMBRE, COUNT(*) AS Repeticiones
    FROM CALLEJERO.CALLE
    GROUP BY ID_MUNICIPIO, NOMBRE
    HAVING COUNT(*) > 1
) x
GROUP BY Repeticiones
ORDER BY Repeticiones;
