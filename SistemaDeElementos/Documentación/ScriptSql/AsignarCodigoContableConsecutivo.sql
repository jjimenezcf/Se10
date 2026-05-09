CREATE OR ALTER PROCEDURE AsignarCodigoContableConsecutivo
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UltimoNumero INT;

    -- Obtener el último número asignado
    SELECT @UltimoNumero = ISNULL(MAX(CAST(CODIGO_CONTABLE AS INT)), 0)
    FROM TERCEROS.SOCIEDAD
    WHERE ISNUMERIC(CODIGO_CONTABLE) = 1;

    -- Asignar números consecutivos
    WITH CTE AS (
        SELECT id, 
               ROW_NUMBER() OVER (ORDER BY id) + @UltimoNumero AS NuevoNumero
        FROM TERCEROS.SOCIEDAD
        WHERE id IN (
            SELECT ID_SOCIEDAD 
            FROM TERCEROS.INTERLOCUTOR
            WHERE id IN (
                SELECT ID_INTERLOCUTOR 
                FROM TERCEROS.PROVEEDOR
            )
        )
        AND CODIGO_CONTABLE IS NULL
    )
    UPDATE s
    SET s.CODIGO_CONTABLE = CAST(c.NuevoNumero AS VARCHAR(4))
    FROM TERCEROS.SOCIEDAD s
    INNER JOIN CTE c ON s.id = c.id;
	
    -- Mostrar el número de filas actualizadas
    SELECT @@ROWCOUNT AS 'Proveedores actualizados';

    -- Asignar números consecutivos
    WITH CTE AS (
        SELECT id, 
               ROW_NUMBER() OVER (ORDER BY id) + @UltimoNumero AS NuevoNumero
        FROM TERCEROS.SOCIEDAD
        WHERE id IN (
            SELECT ID_SOCIEDAD 
            FROM TERCEROS.INTERLOCUTOR
            WHERE id IN (
                SELECT ID_INTERLOCUTOR 
                FROM TERCEROS.CLIENTE
            )
        )
        AND CODIGO_CONTABLE IS NULL
    )
    UPDATE s
    SET s.CODIGO_CONTABLE = CAST(c.NuevoNumero AS VARCHAR(4))
    FROM TERCEROS.SOCIEDAD s
    INNER JOIN CTE c ON s.id = c.id;

    -- Mostrar el número de filas actualizadas
    SELECT @@ROWCOUNT AS 'Clientes actualizados';
END
