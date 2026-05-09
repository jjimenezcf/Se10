CREATE OR ALTER PROCEDURE CorregirTiemposNegativos
AS
BEGIN
    SET NOCOUNT ON;

    -- Tabla temporal para almacenar los registros a corregir
    CREATE TABLE #RegistrosACorregir (
        ID INT,
        ID_ELEMENTO INT,
        FECHA DATETIME2(7),
        TIEMPO BIGINT
    );

    -- Insertar los registros con tiempo negativo en la tabla temporal
    INSERT INTO #RegistrosACorregir (ID, ID_ELEMENTO, FECHA, TIEMPO)
    SELECT ID, ID_ELEMENTO, FECHA, TIEMPO
    FROM SISDOC.CIRCUITO_DOC_HISTORIA
    WHERE TIEMPO < 0;

    -- Declarar un cursor para iterar sobre los registros a corregir
    DECLARE @ID INT, @ID_ELEMENTO INT, @FECHA DATETIME2(7);
    DECLARE @NextID INT, @NextFecha DATETIME2(7);
    DECLARE @NuevoTiempo BIGINT;

    DECLARE RegistrosCursor CURSOR FOR
    SELECT ID, ID_ELEMENTO, FECHA FROM #RegistrosACorregir;

    OPEN RegistrosCursor;
    FETCH NEXT FROM RegistrosCursor INTO @ID, @ID_ELEMENTO, @FECHA;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Encontrar el siguiente registro para el mismo ID_ELEMENTO
        SELECT TOP 1 @NextID = ID, @NextFecha = FECHA
        FROM SISDOC.CIRCUITO_DOC_HISTORIA
        WHERE ID_ELEMENTO = @ID_ELEMENTO AND ID > @ID
        ORDER BY ID;

        -- Calcular la diferencia de tiempo correcta
SET @NuevoTiempo = DATEDIFF_BIG(NANOSECOND, @FECHA, @NextFecha) / 100;


        -- Actualizar el registro con el nuevo tiempo calculado
        UPDATE SISDOC.CIRCUITO_DOC_HISTORIA
        SET TIEMPO = @NuevoTiempo
        WHERE ID = @ID;

        FETCH NEXT FROM RegistrosCursor INTO @ID, @ID_ELEMENTO, @FECHA;
    END

    CLOSE RegistrosCursor;
    DEALLOCATE RegistrosCursor;

    -- Limpiar la tabla temporal
    DROP TABLE #RegistrosACorregir;
END

EXEC CorregirTiemposNegativos;


SELECT * FROM SISDOC.CIRCUITO_DOC_HISTORIA