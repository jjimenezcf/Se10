CREATE OR ALTER PROCEDURE CALLEJERO.PA_TIPO_VIA_SIGLAS_UNICAS
    @Simular       BIT = 1,   -- 1: muestra el resultado y deshace; 0: aplica
    @ResolverResto BIT = 0    -- 1: resuelve automáticamente duplicados y conflictos no previstos
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ------------------------------------------------------------------
    -- Cambios previstos: se mantiene la sigla de Catastro en el tipo
    -- más habitual y se asigna una de 3 letras al otro de la pareja
    ------------------------------------------------------------------
    DECLARE @Cambios TABLE (
        SiglaActual VARCHAR(10)  NOT NULL,
        Nombre      VARCHAR(250) NOT NULL,
        SiglaNueva  VARCHAR(10)  NOT NULL
    );

    INSERT INTO @Cambios (SiglaActual, Nombre, SiglaNueva) VALUES
        ('AL', 'Aldea',        'ALD'),   -- Alameda sigue AL
        ('AR', 'Area',         'ARE'),   -- Arrabal sigue AR
        ('CA', 'Campo',        'CPO'),   -- Cañada sigue CA
        ('CJ', 'Calleja',      'CLJ'),   -- Callejón sigue CJ
        ('CM', 'Carmen',       'CMN'),   -- Camino sigue CM
        ('CR', 'Carrera',      'CRA'),   -- Carretera sigue CR
        ('CT', 'Costanilla',   'CTN'),   -- Cuesta sigue CT
        ('HT', 'Huerto',       'HTO'),   -- Huerta sigue HT
        ('LD', 'Ladera',       'LDR'),   -- Lado sigue LD
        ('PJ', 'Pasadizo',     'PSD'),   -- Pasaje sigue PJ
        ('PQ', 'Parroquia',    'PRQ'),   -- Parque sigue PQ
        ('PR', 'Continuacion', 'CNT'),   -- Prolongación sigue PR
        ('RC', 'Rincona',      'RCN');   -- Rincón sigue RC

    DECLARE @Aplicados  TABLE (ID INT, NOMBRE VARCHAR(250), SiglaAnterior VARCHAR(10), SiglaFinal VARCHAR(10), Origen VARCHAR(20));
    DECLARE @Conflictos TABLE (ID INT, NOMBRE VARCHAR(250), SIGLA VARCHAR(10), NombreEsperado VARCHAR(250));
    DECLARE @Duplicados TABLE (ID INT, NOMBRE VARCHAR(250), SIGLA VARCHAR(10));

    BEGIN TRANSACTION;

    ------------------------------------------------------------------
    -- 1. Limpiar siglas: tabuladores, espacios no separables,
    --    saltos de línea, espacios y mayúsculas
    ------------------------------------------------------------------
    UPDATE CALLEJERO.TIPO_VIA
       SET SIGLA = UPPER(LTRIM(RTRIM(
                     REPLACE(REPLACE(REPLACE(REPLACE(SIGLA,
                        CHAR(9), ''), CHAR(160), ''), CHAR(13), ''), CHAR(10), ''))));

    ------------------------------------------------------------------
    -- 2. Comprobar siglas nuevas ya en uso
    ------------------------------------------------------------------

    -- 2a. Ya aplicado: el registro correcto ya tiene la sigla nueva -> se quita de la lista
    DELETE c
      FROM @Cambios c
     WHERE EXISTS (SELECT 1
                     FROM CALLEJERO.TIPO_VIA t
                    WHERE t.SIGLA = c.SiglaNueva
                      AND LTRIM(RTRIM(t.NOMBRE)) COLLATE Latin1_General_CI_AI = c.Nombre COLLATE Latin1_General_CI_AI);

    -- 2b. Conflicto real: la sigla nueva la tiene otro tipo de vía
    INSERT INTO @Conflictos (ID, NOMBRE, SIGLA, NombreEsperado)
    SELECT t.ID, t.NOMBRE, t.SIGLA, c.Nombre
      FROM @Cambios c
      JOIN CALLEJERO.TIPO_VIA t ON t.SIGLA = c.SiglaNueva;

    IF EXISTS (SELECT 1 FROM @Conflictos)
    BEGIN
        IF @ResolverResto = 1
            -- Se descarta el cambio previsto; el paso 4 le buscará otra sigla libre
            DELETE c FROM @Cambios c WHERE c.SiglaNueva IN (SELECT SIGLA FROM @Conflictos);
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 'Conflicto' AS Estado, * FROM @Conflictos ORDER BY SIGLA;
            THROW 50001, 'Alguna sigla nueva la usa otro tipo de vía. Revisa o ejecuta con @ResolverResto = 1.', 1;
        END;
    END;

    ------------------------------------------------------------------
    -- 3. Aplicar cambios previstos (por sigla + nombre, sin distinguir
    --    mayúsculas ni acentos)
    ------------------------------------------------------------------
    UPDATE t
       SET t.SIGLA = c.SiglaNueva
    OUTPUT inserted.ID, inserted.NOMBRE, deleted.SIGLA, inserted.SIGLA, 'Lista'
      INTO @Aplicados (ID, NOMBRE, SiglaAnterior, SiglaFinal, Origen)
      FROM CALLEJERO.TIPO_VIA t
      JOIN @Cambios c
        ON t.SIGLA = c.SiglaActual
       AND LTRIM(RTRIM(t.NOMBRE)) COLLATE Latin1_General_CI_AI = c.Nombre COLLATE Latin1_General_CI_AI;

    ------------------------------------------------------------------
    -- 4. Resolver automáticamente duplicados no previstos:
    --    se queda la sigla el ID más bajo; el resto recibe
    --    2 letras de la sigla + una letra libre del nombre (o un número)
    ------------------------------------------------------------------
    IF @ResolverResto = 1
    BEGIN
        DECLARE @Id INT, @Nombre VARCHAR(250), @Sigla VARCHAR(10),
                @Base VARCHAR(10), @Nueva VARCHAR(10), @Pos INT, @Letra CHAR(1), @N INT;

        DECLARE cur CURSOR LOCAL STATIC FOR
            SELECT ID, LTRIM(RTRIM(NOMBRE)), SIGLA
              FROM (SELECT ID, NOMBRE, SIGLA,
                           ROW_NUMBER() OVER (PARTITION BY SIGLA ORDER BY ID) AS RN
                      FROM CALLEJERO.TIPO_VIA) x
             WHERE RN > 1;

        OPEN cur;
        FETCH NEXT FROM cur INTO @Id, @Nombre, @Sigla;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @Base  = LEFT(@Sigla, 2);
            SET @Nueva = NULL;
            SET @Pos   = 2;

            -- Sigla + una letra del nombre que no esté en uso
            WHILE @Nueva IS NULL AND @Pos <= LEN(@Nombre)
            BEGIN
                SET @Letra = UPPER(SUBSTRING(@Nombre, @Pos, 1));
                IF @Letra COLLATE Latin1_General_BIN LIKE '[A-Z]'
                   AND NOT EXISTS (SELECT 1 FROM CALLEJERO.TIPO_VIA WHERE SIGLA = @Base + @Letra)
                    SET @Nueva = @Base + @Letra;
                SET @Pos += 1;
            END;

            -- Si no hay letra libre: sigla + número
            SET @N = 1;
            WHILE @Nueva IS NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM CALLEJERO.TIPO_VIA WHERE SIGLA = @Base + CAST(@N AS VARCHAR(3)))
                    SET @Nueva = @Base + CAST(@N AS VARCHAR(3));
                SET @N += 1;
            END;

            UPDATE CALLEJERO.TIPO_VIA SET SIGLA = @Nueva WHERE ID = @Id;

            INSERT INTO @Aplicados (ID, NOMBRE, SiglaAnterior, SiglaFinal, Origen)
            VALUES (@Id, @Nombre, @Sigla, @Nueva, 'Automático');

            FETCH NEXT FROM cur INTO @Id, @Nombre, @Sigla;
        END;

        CLOSE cur;
        DEALLOCATE cur;
    END;

    ------------------------------------------------------------------
    -- 5. Verificar que no quedan duplicados (con detalle)
    ------------------------------------------------------------------
    INSERT INTO @Duplicados (ID, NOMBRE, SIGLA)
    SELECT ID, NOMBRE, SIGLA
      FROM CALLEJERO.TIPO_VIA
     WHERE SIGLA IN (SELECT SIGLA FROM CALLEJERO.TIPO_VIA GROUP BY SIGLA HAVING COUNT(*) > 1);

    -- Resultado: cambios realizados
    SELECT * FROM @Aplicados ORDER BY Origen, SiglaAnterior, ID;

    -- Informativo: conflictos resueltos automáticamente
    IF EXISTS (SELECT 1 FROM @Conflictos)
        SELECT 'Conflicto resuelto automáticamente' AS Estado, * FROM @Conflictos ORDER BY SIGLA;

    IF EXISTS (SELECT 1 FROM @Duplicados)
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'Duplicado' AS Estado, * FROM @Duplicados ORDER BY SIGLA, ID;
        THROW 50002, 'Siguen existiendo siglas duplicadas; no se ha aplicado ningún cambio. Revisa los registros o ejecuta con @ResolverResto = 1.', 1;
    END;

    ------------------------------------------------------------------
    -- 6. Confirmar o deshacer
    ------------------------------------------------------------------
    IF @Simular = 1
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT 'Simulación correcta: cambios deshechos. Ejecuta con @Simular = 0 para aplicarlos.';
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        PRINT 'Cambios aplicados. Ya puedes crear el índice único sobre SIGLA.';
    END;
END;

-- 1. Ver qué registros chocan
EXEC CALLEJERO.PA_TIPO_VIA_SIGLAS_UNICAS;

-- 2. Probar la resolución automática sin aplicar
EXEC CALLEJERO.PA_TIPO_VIA_SIGLAS_UNICAS @ResolverResto = 1;

-- 3. Aplicar
EXEC CALLEJERO.PA_TIPO_VIA_SIGLAS_UNICAS @ResolverResto = 1, @Simular = 0;