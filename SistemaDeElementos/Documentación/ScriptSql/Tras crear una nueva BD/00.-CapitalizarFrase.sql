Create FUNCTION Entorno.CapitalizarFrase(@Frase VARCHAR(MAX))
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @FraseCapitalizada VARCHAR(MAX) = ''
    DECLARE @Palabras TABLE (Palabra VARCHAR(100))

    DECLARE @Posicion INT = 1
    DECLARE @Longitud INT = LEN(@Frase) + 1
    DECLARE @PalabraActual VARCHAR(100)

    WHILE @Posicion < @Longitud
    BEGIN
        SET @PalabraActual = ''

        WHILE @Posicion < @Longitud AND SUBSTRING(@Frase, @Posicion, 1) <> ' '
        BEGIN
            SET @PalabraActual += SUBSTRING(@Frase, @Posicion, 1)
            SET @Posicion += 1
        END

        IF @PalabraActual <> ''
        BEGIN
            IF @FraseCapitalizada <> ''
                SET @FraseCapitalizada += ' '

            IF @Posicion = 1 OR ' a de del en y ' LIKE '% ' + LOWER(@PalabraActual) + ' %'
                SET @FraseCapitalizada += LOWER(@PalabraActual)
            ELSE
                SET @FraseCapitalizada += UPPER(LEFT(@PalabraActual, 1)) + LOWER(SUBSTRING(@PalabraActual, 2, LEN(@PalabraActual) - 1))
        END

        SET @Posicion += 1
    END

    RETURN @FraseCapitalizada
END