-- Verificar si el procedimiento ya existe y eliminarlo si es el caso
IF OBJECT_ID('ENTORNO.MoverMenu', 'P') IS NOT NULL
    DROP PROCEDURE ENTORNO.MoverMenu;
GO

-- Creación del procedimiento almacenado
CREATE PROCEDURE ENTORNO.MoverMenu
    @MenuNombre NVARCHAR(255),
    @PadreAnteriorNombre NVARCHAR(255),
    @PadreNuevoNombre NVARCHAR(255)
AS
BEGIN
    -- Declarar variables
    DECLARE @idmenu INT
    DECLARE @idPadreAnterior INT
    DECLARE @idPadreNuevo INT

    -- 1. Obtener los IDs de los menús
    SET @idmenu = (SELECT id FROM ENTORNO.MENU WHERE nombre LIKE @MenuNombre and idpadre = (select id from ENTORNO.MENU WHERE nombre LIKE @PadreAnteriorNombre))
    SET @idPadreAnterior = (SELECT id FROM ENTORNO.MENU WHERE nombre LIKE @PadreAnteriorNombre)
    SET @idPadreNuevo = (SELECT id FROM ENTORNO.MENU WHERE nombre LIKE @PadreNuevoNombre)

    ---
    -- 2. Manejo de excepciones (lanza errores si algún menú no existe)
    ---
    IF @idmenu IS NULL
        BEGIN
            -- Usamos RAISERROR con SEVERITY 16 y STATE 1 para lanzar una excepción controlada
            RAISERROR('El menú con nombre "%s" no existe.', 16, 1, @MenuNombre)
            RETURN -- Detiene la ejecución del procedimiento
        END
    
    IF @idPadreAnterior IS NULL
        BEGIN
            RAISERROR('El menú padre anterior con nombre "%s" no existe.', 16, 1, @PadreAnteriorNombre)
            RETURN
        END
    
    IF @idPadreNuevo IS NULL
        BEGIN
            RAISERROR('El menú padre nuevo con nombre "%s" no existe.', 16, 1, @PadreNuevoNombre)
            RETURN
        END

    ---
    -- 3. Ejecutar la actualización
    ---
    UPDATE ENTORNO.MENU 
    SET IDPADRE = @idPadreNuevo 
    WHERE id = @idmenu 
      AND IDPADRE = @idPadreAnterior

    -- 4. Mostrar información de depuración
    PRINT 'ID Menú a mover: ' + CAST(@idmenu AS NVARCHAR(10))
    PRINT 'ID Padre Anterior (esperado): ' + CAST(@idPadreAnterior AS NVARCHAR(10))
    PRINT 'ID Nuevo Padre: ' + CAST(@idPadreNuevo AS NVARCHAR(10))
    PRINT 'Filas actualizadas: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) -- Muestra cuántas filas fueron afectadas
END
GO

DECLARE	@return_value int

EXEC	@return_value = [ENTORNO].[MoverMenu]
		@MenuNombre = N'trabajadores',
		@PadreAnteriorNombre = N'maestros',
		@PadreNuevoNombre = N'Módulo RRHH'

SELECT	'Return Value' = @return_value